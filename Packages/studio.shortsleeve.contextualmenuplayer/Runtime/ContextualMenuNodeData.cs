using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ContextualMenuPlayer
{
    public abstract class ContextualMenuNodeData
    {
        public readonly string NodeName;

        public abstract void AddChild(ContextualMenuNodeData node);
        public abstract void Execute();
        public abstract List<ContextualMenuNodeData> Children { get; }
        public abstract int ChildCount { get; }
        public abstract bool IsSubMenu { get; }
        public abstract bool IsSeparator { get; }
        public abstract bool IsDisabled { get; }
        public abstract bool IsChecked { get; }

        protected ContextualMenuNodeData(string name) => NodeName = name;

        public static ContextualMenuNodeData ConstructMenuTree(DropdownMenu dropdownMenu)
        {
            SubMenuNode root = new(string.Empty);
            root.ConstructFromMenu(dropdownMenu);
            return root;
        }

        public static ContextualMenuNodeData NewTree() => new SubMenuNode(string.Empty);

        private class SeparatorMenuNode : ContextualMenuNodeData
        {
            private static readonly NotImplementedException NoChildrenException = new(
                "Contexual menu separator cannot have children"
            );
            private static readonly NotImplementedException NoExecuteException = new(
                "Contexual menu separator cannot execute"
            );

            internal SeparatorMenuNode(string name)
                : base(name) { }

            public override void AddChild(ContextualMenuNodeData node) => throw NoChildrenException;

            public override void Execute() => throw NoExecuteException;

            public override List<ContextualMenuNodeData> Children => throw NoChildrenException;
            public override int ChildCount => throw NoChildrenException;
            public override bool IsSeparator => true;
            public override bool IsSubMenu => false;
            public override bool IsDisabled => false;
            public override bool IsChecked => false;
        }

        private class ActionMenuNode : ContextualMenuNodeData
        {
            private static readonly NotImplementedException NoChildrenException = new(
                "Contexual menu action cannot have children"
            );

            private readonly DropdownMenuAction m_MenuAction;

            internal ActionMenuNode(string name, DropdownMenuAction action)
                : base(name) => m_MenuAction = action;

            public override void AddChild(ContextualMenuNodeData node) => throw NoChildrenException;

            public override void Execute() => m_MenuAction.Execute();

            public override List<ContextualMenuNodeData> Children => throw NoChildrenException;
            public override int ChildCount => throw NoChildrenException;
            public override bool IsSeparator => false;
            public override bool IsSubMenu => false;
            public override bool IsDisabled => m_MenuAction.IsDisabled();
            public override bool IsChecked => m_MenuAction.IsChecked();
        }

        private class SubMenuNode : ContextualMenuNodeData
        {
            private static readonly NotImplementedException NoExecuteException = new(
                "Contexual submenu cannot execute"
            );

            private readonly List<ContextualMenuNodeData> m_Children;

            internal SubMenuNode(string name)
                : base(name) => m_Children = new();

            public override void AddChild(ContextualMenuNodeData node) => m_Children.Add(node);

            public override void Execute() => throw NoExecuteException;

            public override List<ContextualMenuNodeData> Children => m_Children;
            public override int ChildCount => m_Children.Count;
            public override bool IsChecked => false;
            public override bool IsDisabled => false;
            public override bool IsSeparator => false;
            public override bool IsSubMenu => true;

            internal void ConstructFromMenu(DropdownMenu dropdownMenu)
            {
                foreach (DropdownMenuItem item in dropdownMenu.MenuItems())
                {
                    switch (item)
                    {
                        case DropdownMenuAction action:
                            AddActionNode(action);
                            break;
                        case DropdownMenuSeparator separator:
                            AddSeparatorNode(separator);
                            break;
                    }
                }
            }

            private void AddActionNode(DropdownMenuAction action)
            {
                // Skip Hidden
                if (action.IsHidden())
                    return;

                // Break Path
                string[] pathElements = SplitPathAndClean(action.name);

                // Create New Node
                ActionMenuNode newNode = new(pathElements[^1], action);

                // Add Node
                AddMenuNode(pathElements, newNode);
            }

            private void AddSeparatorNode(DropdownMenuSeparator separator)
            {
                // Break Path
                string[] pathElements = SplitPathAndClean(separator.subMenuPath, true);

                // Create New Node
                SeparatorMenuNode newNode = new(pathElements[^1]);

                // Add Node
                AddMenuNode(pathElements, newNode);
            }

            private void AddMenuNode(string[] pathElements, ContextualMenuNodeData newNode)
            {
                int endIndex = pathElements.Length - 1;
                ContextualMenuNodeData currentNode = this;
                for (int i = 0; i < pathElements.Length; i++)
                {
                    // Current Path Element
                    string currentPathElement = pathElements[i];

                    // Is Last Index
                    bool isFinalElement = i == endIndex;

                    // Find or Create Path Element
                    bool found = false;
                    ContextualMenuNodeData nodeIterator = null;
                    if (currentPathElement == string.Empty)
                    {
                        // The only case where "" is acceptable is when we're adding as separator to
                        // the root menu.
                        if (!isFinalElement)
                        {
                            string errPath = string.Join('/', pathElements);
                            throw new($"Path cannot contain double forward slashes {errPath}");
                        }
                        found = true;
                        nodeIterator = currentNode;
                    }
                    else
                    {
                        int childCount = currentNode.ChildCount;
                        List<ContextualMenuNodeData> children = currentNode.Children;
                        for (int j = 0; j < childCount; j++)
                        {
                            nodeIterator = children[j];
                            if (nodeIterator.NodeName == currentPathElement)
                            {
                                found = true;
                                break;
                            }
                        }
                    }

                    // Not Found
                    if (!found)
                    {
                        ContextualMenuNodeData childToAdd;
                        // Add New Menu Node
                        if (isFinalElement)
                            childToAdd = newNode;
                        // Add new Sub Menu Node
                        else
                            childToAdd = new SubMenuNode(currentPathElement);
                        currentNode.AddChild(childToAdd);
                        currentNode = childToAdd;
                    }
                    else
                    {
                        if (isFinalElement)
                        {
                            // Duplicate Path Found
                            if (newNode is SeparatorMenuNode)
                            {
                                // Add Separator Node
                                nodeIterator.AddChild(newNode);
                            }
                            else
                                throw new(
                                    $"Duplicate menu entry found {string.Join('/', pathElements)}"
                                );
                        }
                        else
                        {
                            // Still Searching, Update Current Node
                            currentNode = nodeIterator;
                        }
                    }
                }
            }

            private string[] SplitPathAndClean(string path, bool isSeparator = false)
            {
                // Sanity
                if (path == null)
                    throw new($"Invalid path menu {path}");

                // Split
                string[] pathElements = path.Split('/');

                // Calculate empty element count
                int emptyCount = 0;
                for (int i = 0; i < pathElements.Length; i++)
                {
                    if (pathElements[i] == string.Empty)
                        emptyCount++;
                }

                // Remove empty elements, and if this is separator, add empty string to the end
                string[] cleanedPathElements = new string[
                    pathElements.Length - emptyCount + (isSeparator ? 1 : 0)
                ];
                int j = 0;
                for (int i = 0; i < pathElements.Length; i++)
                {
                    if (pathElements[i] != string.Empty)
                    {
                        cleanedPathElements[j++] = pathElements[i];
                    }
                }
                if (isSeparator)
                    cleanedPathElements[j] = string.Empty;

                // Sanity
                if (cleanedPathElements.Length == 0)
                {
                    throw new($"Invalid path menu {path}");
                }

                // Return eleaned path elements
                return cleanedPathElements;
            }
        }
    }
}
