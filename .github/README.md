# Unity Runtime Contextual Menu
![screenshot](example.png)

## Quick Start
```
// Import the contents of the "Plugins" folder into your project's "Plugins" folder.
// Write and add a MonoBehaviour to the same GameObject that has your UIDocument.
private void OnEnable()
{
    // Grab document
    UIDocument uiDocument = GetComponent<UIDocument>();

    // Add RUIContextualMenuManipulator to the top level element you want be able to right click
    uiDocument.rootVisualElement.AddManipulator(new RUIContextualMenuManipulator());

    // Grab the element that you want to right click 
    VisualElement elementWithMenu  = uiDocument
        .rootVisualElement
        .Q<VisualElement>(name: "element-with-menu");

    // Register a callback to populate the context menu
    elementWithMenu.RegisterCallback<ContextualMenuPopulateEvent>(PopulateMenuCallback);
}
...

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
    populateEvent.menu.AppendAction($"Option 4", NoOp, action => DropdownMenuAction.Status.Checked);
}

private void NoOp() {}
```

## Custom Styling 
All default styling is contained under: `Plugins/RUI/ContextualMenu/Resources/RUI/RUIContextualMenu.uss`.  However, if you want to use your own StyleSheet, here's what you do:
```
// See the Quick Start example and change/add the following
private void OnEnable()
{
    ...
    // Construct an RUIContextualMenuManager and feed it your StyleSheet
    RUIContextualMenuManager manager = new(<YOUR STYLE SHEET>);
    
    // Pass the manager in to your MenuManipulator 
    uiDocument.rootVisualElement.AddManipulator(new RUIContextualMenuManipulator(manager));
    ...
}
```