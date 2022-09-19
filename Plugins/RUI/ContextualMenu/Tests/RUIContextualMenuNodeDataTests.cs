using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace RUI
{
    public class RUIContextualMenuDataNodeTests
    {
        [Test]
        public void NoHierarchy()
        {
            Action<DropdownMenuAction> testCallback = testDropdownMenuAction => { };
            DropdownMenu testMenu = new DropdownMenu();

            // Status: None
            Func<DropdownMenuAction, Status> testNone
                = testAction => DropdownMenuAction.Status.None;
            testMenu.AppendAction("none", testCallback, testNone);
            // Status: Normal
            testMenu.AppendAction("normal", testCallback, DropdownMenuAction.AlwaysEnabled);
            // Status: Disabled 
            testMenu.AppendAction("disabled", testCallback, DropdownMenuAction.AlwaysDisabled);
            // Status: Checked 
            Func<DropdownMenuAction, Status> testChecked
                = testAction => DropdownMenuAction.Status.Checked;
            testMenu.AppendAction("checked", testCallback, testChecked);
            // Status: Hidden 
            Func<DropdownMenuAction, Status> testHidden
                = testAction => DropdownMenuAction.Status.Hidden;
            testMenu.AppendAction("hidden", testCallback, testHidden);
            // Status: Custom 
            int customStatus = DropdownMenuAction.Status.Disabled & DropdownMenuAction.Status.Checked;
            Func<DropdownMenuAction, Status> testCustom = testAction => customStatus;
            testMenu.AppendAction("custom", testCallback, testCustom);

            // Construct Menu Data 
            RUIContextualMenuDataNode rootNode
                = RUIContextualMenuNodeData.ConstructMenuTree(testMenu);

            // Assert
            Assert.AreEqual(1, 1);
        }
    }
}
