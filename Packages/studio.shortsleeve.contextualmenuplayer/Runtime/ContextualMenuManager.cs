using UnityEngine.UIElements;

namespace ContextualMenuPlayer
{
    public class ContextualMenuManager : UnityEngine.UIElements.ContextualMenuManager
    {
        private readonly StyleSheet m_StyleSheet;

        public ContextualMenuManager()
            : this(null) { }

        public ContextualMenuManager(StyleSheet styleSheet)
        {
            m_StyleSheet = styleSheet;
        }

        // Do actual display here so it can be used independently of manipulator events
        public void DisplayMenu(ContextualMenu.MenuCreationContext creationContext)
        {
            ContextualMenu.OpenMenu(creationContext);
        }

        // NOTE: I've only seen this used in the context of GraphView. Leaving for now.
        public override void DisplayMenuIfEventMatches(
            EventBase evt,
            IEventHandler eventHandler
        ) { }

        protected override void DoDisplayMenu(DropdownMenu menu, EventBase triggerEvent)
        {
            if (menu.MenuItems().Count == 0)
                return;
            DisplayMenu(
                new()
                {
                    menu = menu,
                    position = ((IMouseEvent)triggerEvent).mousePosition,
                    styleSheetOverride = m_StyleSheet,
                    root = GetElementRoot((VisualElement)triggerEvent.target),
                    direction = RUIContextualMenuGrowDirection.SE,
                }
            );
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
