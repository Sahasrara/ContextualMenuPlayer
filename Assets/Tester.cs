using UnityEngine;
using UnityEngine.UIElements;

public class Tester : MonoBehaviour
{
    private void OnEnable()
    {
        // Grab document
        UIDocument uiDocument = GetComponent<UIDocument>();

        // Add ContextualMenuManipulator to the top level element you want be able to right click
        uiDocument.rootVisualElement.AddManipulator(
            new ContextualMenuPlayer.ContextualMenuManipulator()
        );

        // Grab the element that you want to right click
        VisualElement elementWithMenu = uiDocument.rootVisualElement.Q<VisualElement>(
            name: "element-with-menu"
        );

        // Register a callback to populate the context menu
        elementWithMenu.RegisterCallback<ContextualMenuPopulateEvent>(PopulateMenuCallback);
    }

    private void PopulateMenuCallback(ContextualMenuPopulateEvent populateEvent)
    {
        // This will be a single top-level option
        populateEvent.menu.AppendAction("Option 1", NoOp, DropdownMenuAction.AlwaysEnabled);
        // This will add a separator
        populateEvent.menu.AppendSeparator();
        // This will be nested
        populateEvent.menu.AppendAction($"Option 2/nested", NoOp, DropdownMenuAction.AlwaysEnabled);
        // This will add a separator, again
        populateEvent.menu.AppendSeparator();
        // This will be disabled
        populateEvent.menu.AppendAction($"Option 3", NoOp, DropdownMenuAction.AlwaysDisabled);
        // This will add a separator, again
        populateEvent.menu.AppendSeparator();
        // This will be checked
        populateEvent.menu.AppendAction(
            $"Option 4",
            NoOp,
            action => DropdownMenuAction.Status.Checked
        );
    }

    private void NoOp(DropdownMenuAction action) { }
}
