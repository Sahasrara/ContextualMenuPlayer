using System;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace ContextualMenuPlayer
{
    public class ContextualMenuDataNodeTests
    {
        [Test]
        public void TrailingSlash()
        {
            ContextualMenuNodeData rootNode = GenerateSingleActionNoopTree("test/");
            Assert.AreEqual(1, CountChildren(rootNode));
            Assert.AreEqual("test", rootNode.Children[0].NodeName);
        }

        [Test]
        public void LeadingSlash()
        {
            ContextualMenuNodeData rootNode = GenerateSingleActionNoopTree("/test");
            Assert.AreEqual(1, CountChildren(rootNode));
            Assert.AreEqual("test", rootNode.Children[0].NodeName);
        }

        [Test]
        public void LeadingAndTrailingSlash()
        {
            ContextualMenuNodeData rootNode = GenerateSingleActionNoopTree("/test/");
            Assert.AreEqual(1, CountChildren(rootNode));
            Assert.AreEqual("test", rootNode.Children[0].NodeName);
        }

        [Test]
        public void NestedSeparatorOnly()
        {
            ContextualMenuNodeData rootNode = GenerateSingleSeparatorTree("test");
            Assert.AreEqual(3, CountChildren(rootNode));
            Assert.IsTrue(rootNode.Children[1].Children[0].IsSeparator);
        }

        [Test]
        public void NestedDoubleSlash()
        {
            ContextualMenuNodeData rootNode = GenerateSingleActionNoopTree(
                "Double Slash Test//Nested");
            Assert.AreEqual(1, CountChildren(rootNode));
            Assert.AreEqual("Nested", rootNode.Children[0].Children[0].NodeName);
        }

        [Test]
        public void NestedTrailingSlash()
        {
            ContextualMenuNodeData rootNode = GenerateSingleActionNoopTree(
                "Trailing Slash Test/Test/");
            Assert.AreEqual(1, CountChildren(rootNode));
            Assert.AreEqual("Test", rootNode.Children[0].Children[0].NodeName);
        }

        [Test]
        public void NestedTrailingDoubleSlash()
        {
            ContextualMenuNodeData rootNode = GenerateSingleActionNoopTree(
                "Double Slash Test//");
            Assert.AreEqual(1, CountChildren(rootNode));
            Assert.AreEqual("Double Slash Test", rootNode.Children[0].NodeName);
        }

        [Test]
        public void SingleSlashOnly()
        {
            Assert.That(() => GenerateSingleActionNoopTree("/"), Throws.TypeOf<Exception>());
        }


        [Test]
        public void DoubleSlashOnly()
        {
            Assert.That(() => GenerateSingleActionNoopTree("//"), Throws.TypeOf<Exception>());
        }

        [Test]
        public void ManySlashOnly()
        {
            Assert.That(() => GenerateSingleActionNoopTree("/////"), Throws.TypeOf<Exception>());
        }

        [Test]
        public void AllStatuses()
        {
            DropdownMenu testMenu = new();

            // Status: None
            testMenu.AppendAction("none", NoOpMenuAction, ReturnNone);
            // Status: Normal
            testMenu.AppendAction("normal", NoOpMenuAction, DropdownMenuAction.AlwaysEnabled);
            // Status: Disabled 
            testMenu.AppendAction("disabled", NoOpMenuAction, DropdownMenuAction.AlwaysDisabled);
            // Status: Checked 
            testMenu.AppendAction("checked", NoOpMenuAction, ReturnChecked);
            // Status: Hidden 
            testMenu.AppendAction("hidden", NoOpMenuAction, ReturnHidden);
            // Prepare
            testMenu.PrepareForDisplay(null);

            // Construct Menu Data 
            ContextualMenuNodeData rootNode
                = ContextualMenuNodeData.ConstructMenuTree(testMenu);

            // Assert
            Assert.AreEqual(3, CountChildren(rootNode));
            Assert.AreEqual("normal", rootNode.Children[0].NodeName);
            Assert.AreEqual("disabled", rootNode.Children[1].NodeName);
            Assert.IsTrue(rootNode.Children[1].IsDisabled);
            Assert.AreEqual("checked", rootNode.Children[2].NodeName);
            Assert.IsTrue(rootNode.Children[2].IsChecked);
        }

        [Test]
        public void ALittleOfEverything()
        {
            DropdownMenu testMenu = new();
            testMenu.AppendAction($"Paste", NoOpMenuAction, DropdownMenuAction.AlwaysEnabled);
            testMenu.AppendAction($"Copy", NoOpMenuAction, DropdownMenuAction.AlwaysEnabled);
            testMenu.AppendSeparator();
            testMenu.AppendAction($"Nested/Option 1", NoOpMenuAction,
                DropdownMenuAction.AlwaysEnabled);
            testMenu.AppendAction($"Nested/Option 2", NoOpMenuAction,
                DropdownMenuAction.AlwaysEnabled);
            testMenu.AppendAction($"Nested/Option 3", NoOpMenuAction,
                DropdownMenuAction.AlwaysEnabled);
            testMenu.AppendAction($"Nested/Option 4", NoOpMenuAction,
                DropdownMenuAction.AlwaysEnabled);
            testMenu.AppendAction($"Nested/Option 5", NoOpMenuAction,
                DropdownMenuAction.AlwaysEnabled);
            testMenu.AppendSeparator();
            testMenu.AppendAction($"Nested Deep 0/Nested Deep 1/Nested Deep 2/Nested Deep 3",
                NoOpMenuAction, DropdownMenuAction.AlwaysEnabled);
            testMenu.AppendAction($"Nested Deep 0/Option 1", NoOpMenuAction,
                DropdownMenuAction.AlwaysEnabled);
            testMenu.AppendAction($"Nested Deep 0/Nested Disabled", NoOpMenuAction,
                 DropdownMenuAction.AlwaysDisabled);
            testMenu.AppendAction($"Nested Deep 0/Nested Checked", NoOpMenuAction, ReturnChecked);
            testMenu.AppendAction($"Disabled", NoOpMenuAction, DropdownMenuAction.AlwaysDisabled);
            testMenu.AppendAction($"Checked", NoOpMenuAction, ReturnChecked);
            testMenu.AppendAction($"More Separators", NoOpMenuAction, ReturnChecked);
            testMenu.AppendSeparator();
            testMenu.AppendAction($"Last Item", NoOpMenuAction, ReturnChecked);

            testMenu.PrepareForDisplay(null);

            // Construct Menu Data 
            ContextualMenuNodeData rootNode
                = ContextualMenuNodeData.ConstructMenuTree(testMenu);

            // Assert
            Assert.AreEqual(18, CountChildren(rootNode));
        }

        private void NoOpMenuAction(DropdownMenuAction action) { }

        private DropdownMenuAction.Status ReturnChecked(DropdownMenuAction action)
            => DropdownMenuAction.Status.Checked;
        private DropdownMenuAction.Status ReturnHidden(DropdownMenuAction action)
            => DropdownMenuAction.Status.Hidden;
        private DropdownMenuAction.Status ReturnNone(DropdownMenuAction action)
            => DropdownMenuAction.Status.None;

        private int CountChildren(ContextualMenuNodeData node)
        {
            if (!node.IsSubMenu) return 1;
            int count = 0;
            for (int i = 0; i < node.ChildCount; i++) count += CountChildren(node.Children[i]);
            return count;
        }

        private ContextualMenuNodeData GenerateSingleActionNoopTree(string path)
        {
            DropdownMenu testMenu = new();
            testMenu.AppendAction(path, NoOpMenuAction, DropdownMenuAction.AlwaysEnabled);
            testMenu.PrepareForDisplay(null);
            return ContextualMenuNodeData.ConstructMenuTree(testMenu);
        }

        private ContextualMenuNodeData GenerateSingleSeparatorTree(string path)
        {
            DropdownMenu testMenu = new();
            // Can't add separator to empty list for some reason
            testMenu.AppendAction("abcdefghijkl", NoOpMenuAction, DropdownMenuAction.AlwaysEnabled);
            testMenu.AppendSeparator(path);
            testMenu.AppendAction(
                "mnopqrstuvwxyz", NoOpMenuAction, DropdownMenuAction.AlwaysEnabled);
            testMenu.PrepareForDisplay(null);
            return ContextualMenuNodeData.ConstructMenuTree(testMenu);
        }
    }
}
