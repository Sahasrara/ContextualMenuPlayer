using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace RUI
{
    internal abstract class RUIContextualMenuNodeData
    {
        public readonly string NodeName;

        internal abstract void AddChild(RUIContextualMenuNodeData node);
        internal abstract void Execute();
        internal abstract List<RUIContextualMenuNodeData> Children { get; }
        internal abstract int ChildCount { get; }
        internal abstract bool IsSubMenu { get; }
        internal abstract bool IsSeparator { get; }
        internal abstract bool IsDisabled { get; }
        internal abstract bool IsChecked { get; }

        internal RUIContextualMenuNodeData(string name) => NodeName = name;

        internal static RUIContextualMenuNodeData ConstructMenuTree(DropdownMenu dropdownMenu)
        {
            SubMenuNode root = new(string.Empty);
            root.ConstructFromMenu(dropdownMenu);
            return root;
        }

        internal static RUIContextualMenuNodeData NewTree() => new SubMenuNode(string.Empty);

        private class SeparatorMenuNode : RUIContextualMenuNodeData
        {
            private static readonly NotImplementedException NoChildrenException
                = new("Contexual menu separator cannot have children");
            private static readonly NotImplementedException NoExecuteException
                = new("Contexual menu separator cannot execute");

            internal SeparatorMenuNode(string name) : base(name) { }

            internal override void AddChild(RUIContextualMenuNodeData node)
                => throw NoChildrenException;
            internal override void Execute() => throw NoExecuteException;
            internal override List<RUIContextualMenuNodeData> Children => throw NoChildrenException;
            internal override int ChildCount => throw NoChildrenException;
            internal override bool IsSeparator => true;
            internal override bool IsSubMenu => false;
            internal override bool IsDisabled => false;
            internal override bool IsChecked => false;
        }

        private class ActionMenuNode : RUIContextualMenuNodeData
        {
            private static readonly NotImplementedException NoChildrenException
                = new("Contexual menu action cannot have children");

            private readonly DropdownMenuAction m_MenuAction;

            internal ActionMenuNode(string name, DropdownMenuAction action) : base(name)
                => m_MenuAction = action;

            internal override void AddChild(RUIContextualMenuNodeData node)
                => throw NoChildrenException;
            internal override void Execute() => m_MenuAction.Execute();
            internal override List<RUIContextualMenuNodeData> Children => throw NoChildrenException;
            internal override int ChildCount => throw NoChildrenException;
            internal override bool IsSeparator => false;
            internal override bool IsSubMenu => false;
            internal override bool IsDisabled => m_MenuAction.IsDisabled();
            internal override bool IsChecked => m_MenuAction.IsChecked();
        }

        private class SubMenuNode : RUIContextualMenuNodeData
        {
            private static readonly NotImplementedException NoExecuteException
                = new("Contexual submenu cannot execute");

            private readonly List<RUIContextualMenuNodeData> m_Children;

            internal SubMenuNode(string name) : base(name) => m_Children = new();

            internal override void AddChild(RUIContextualMenuNodeData node) => m_Children.Add(node);
            internal override void Execute() => throw NoExecuteException;
            internal override List<RUIContextualMenuNodeData> Children => m_Children;
            internal override int ChildCount => m_Children.Count;
            internal override bool IsChecked => false;
            internal override bool IsDisabled => false;
            internal override bool IsSeparator => false;
            internal override bool IsSubMenu => true;

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
                if (action.IsHidden()) return;

                // Break Path
                string[] pathElements = SplitPath(action.name);

                // Create New Node
                ActionMenuNode newNode = new(pathElements[^1], action);

                // Add Node
                AddMenuNode(pathElements, newNode);
            }

            private void AddSeparatorNode(DropdownMenuSeparator separator)
            {
                // Break Path
                string[] pathElements = SplitPath(separator.subMenuPath);

                // Create New Node
                SeparatorMenuNode newNode = new(pathElements[^1]);

                // Add Node
                AddMenuNode(pathElements, newNode);
            }

            private void AddMenuNode(string[] pathElements, RUIContextualMenuNodeData newNode)
            {
                int endIndex = pathElements.Length - 1;
                RUIContextualMenuNodeData currentNode = this;
                for (int i = 0; i < pathElements.Length; i++)
                {
                    // Current Path Element
                    string currentPathElement = pathElements[i];

                    // Find or Create Path Element
                    RUIContextualMenuNodeData nodeIterator = null;
                    bool found = false;
                    int childCount = currentNode.ChildCount;
                    List<RUIContextualMenuNodeData> children = currentNode.Children;
                    for (int j = 0; j < childCount; j++)
                    {
                        nodeIterator = children[j];
                        if (nodeIterator.NodeName == currentPathElement)
                        {
                            found = true;
                            break;
                        }
                    }

                    // Not Found
                    if (!found)
                    {
                        RUIContextualMenuNodeData childToAdd;
                        // Add New Menu Node
                        if (i == endIndex) childToAdd = newNode;
                        // Add new Sub Menu Node
                        else childToAdd = new SubMenuNode(currentPathElement);
                        currentNode.AddChild(childToAdd);
                        currentNode = childToAdd;
                    }
                    else
                    {
                        if (i == endIndex)
                        {
                            // Duplicate Path Found
                            if (newNode is SeparatorMenuNode)
                            {
                                // Add Separator Node
                                nodeIterator.AddChild(newNode);
                            }
                            else throw new Exception(
                                $"Duplicate menu entry found {string.Join('/', pathElements)}");
                        }
                        else
                        {
                            // Still Searching, Update Current Node 
                            currentNode = nodeIterator;
                        }
                    }
                }
            }

            private string[] SplitPath(string path)
            {
                string[] pathElements = path.Split('/');
                if (pathElements.Length == 0) throw new Exception($"Invalid path menu {path}");
                return pathElements;
            }
        }
    }
}
