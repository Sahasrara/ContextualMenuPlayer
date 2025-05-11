# Unity Runtime Contextual Menu
![screenshot](example.png)

## Note
This project isn't actively maintained. I created in a few years back as I was starting to work on a dialogue system. I ended up using Electron to build the dialogue system so this little tool fell by the wayside.

Incidentally, if you require a dialogue system, [GameScript](https://github.com/ShortSleeveStudio/GameScript) is free and open source and awesome.

## Quick Start
You can install this package with the Package Manager with this URL. Feel free to substitute the latest release:
`https://github.com/ShortSleeveStudio/FMODHelpers.git?path=/Packages/studio.shortsleeve.contextualmenuplayer#v0.0.0`

Please open the test project to see a working example. You can copy the code from Tester.cs to form the basis of your integration.

## Custom Styling 
All default styling is contained under: `Runtime/Resources/ContextualMenu.uss`.  However, if you want to use your own StyleSheet, here's what you can:
```
private void OnEnable()
{
    ...
    // Construct an ContextualMenuManager and feed it your StyleSheet
    ContextualMenuManager manager = new(<YOUR STYLE SHEET>);
    
    // Pass the manager in to your MenuManipulator 
    uiDocument.rootVisualElement.AddManipulator(new ContextualMenuManipulator(manager));
    ...
}
```

## Submenu Behavior
When you hover your cursor over a menu item that has submenus, the submenus will open instantly.  If you then move your cursor outside of the menu item that opened the submenu, an invisible triangle will be created between your cursor and the new submenu.  This triangle represents a safe-zone for your cursor.  Even if it's hovering over a different menu item, the current submenu won't close.

But that safe zone only lasts for 0.3 seconds by default.  That delay is set in RUIContextualMenuBox as a constant called `SafeZoneTimeoutSecs`.  I'm hoping to make that customizable, but if you need to change the behavior immediately, that's where it is.
