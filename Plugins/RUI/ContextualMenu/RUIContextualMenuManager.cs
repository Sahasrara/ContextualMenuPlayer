using UnityEngine.UIElements;

namespace RUI
{
    public class RUIContextualMenuManager : ContextualMenuManager
    {
        private readonly StyleSheet m_StyleSheet;

        public RUIContextualMenuManager() : this(null) { }
        public RUIContextualMenuManager(StyleSheet styleSheet)
        {
            m_StyleSheet = styleSheet;
        }

        // Do actual display here so it can be used independently of manipulator events
        public void DisplayMenu(RUIContextualMenu.MenuCreationContext creationContext)
        {
            RUIContextualMenu.OpenMenu(creationContext);
        }

        // NOTE: I've only seen this used in the context of GraphView. Leaving for now.
        public override void DisplayMenuIfEventMatches(
            EventBase evt, IEventHandler eventHandler)
        { }

        protected override void DoDisplayMenu(DropdownMenu menu, EventBase triggerEvent)
        {
            if (menu.MenuItems().Count == 0) return;
            DisplayMenu(new()
            {
                menu = menu,
                position = ((IMouseEvent)triggerEvent).mousePosition,
                styleSheetOverride = m_StyleSheet,
                root = GetElementRoot((VisualElement)triggerEvent.target),
            });
        }

        private VisualElement GetElementRoot(VisualElement visualElement)
        {
            VisualElement result = null;
            for (; visualElement != null; visualElement = visualElement.hierarchy.parent)
            {
                if (visualElement is TemplateContainer)
                {
                    result = visualElement;
                    break;
                }
            }
            return result;
        }
    }
}
