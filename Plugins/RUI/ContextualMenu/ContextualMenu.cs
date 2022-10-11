using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace ContextualMenuPlayer
{
    public class ContextualMenu : VisualElement, IMenuBoxParent
    {
        private static readonly ObjectPool<ContextualMenu> s_ContextualMenuPool
            = new(Create, null, TearDown);
        private static int s_PoolCount;
        private static StyleSheet s_DefaultStyle;
        private static StyleSheet DefaultStyle
        {
            get
            {
                if (s_DefaultStyle == null)
                {
                    s_DefaultStyle = Resources.Load<StyleSheet>("ContextualMenu/ContextualMenu");
                }
                return s_DefaultStyle;
            }
        }

        private readonly VisualElement m_Closer;
        private ContextualMenuBox m_RootMenuBox;
        private ContextualMenuNodeData m_MenuTreeData;
        private Vector2 m_OriginalClickPoint;
        private float m_OpenTime;
        private bool m_OpenClickComplete;
        private RUIContextualMenuGrowDirection m_Direction;

        private ContextualMenu()
        {
            name = "contextual-menu-" + s_PoolCount++;
            AddToClassList("contextual-menu-viewport");
            m_Closer = new();
            m_Closer.AddToClassList("contextual-menu-viewport");
            m_Closer.name = "menu-closer";
            Add(m_Closer);
        }

        public static ContextualMenu GetFromPool(MenuCreationContext ctx)
        {
            // Create Menu Container
            ContextualMenu menu = s_ContextualMenuPool.Get();

            // Set Open Time
            menu.m_OpenTime = Time.time;
            menu.m_OpenClickComplete = false;

            // Set Click Point
            menu.m_OriginalClickPoint = ctx.position;

            // Set Direction
            menu.m_Direction = ctx.direction;

            // Style
            menu.styleSheets.Add(ctx.styleSheetOverride ?? DefaultStyle);

            // Parse Tree Data
            menu.m_MenuTreeData = ContextualMenuNodeData.ConstructMenuTree(ctx.menu);

            // Register Event Handlers 
            menu.m_Closer.RegisterCallback<MouseUpEvent>(menu.OnMouseUp);

            // Open Menu
            menu.m_RootMenuBox = ContextualMenuBox.GetFromPool(new()
            {
                menuData = menu.m_MenuTreeData,
                rootMenu = menu,
                parentElement = menu,
                direction = ctx.direction,
            });

            // Add to Root 
            ctx.root.Add(menu);

            // Return Menu
            return menu;
        }

        public static void ReleaseToPool(ContextualMenu toRelease)
            => s_ContextualMenuPool.Release(toRelease);

        internal static void OpenMenu(MenuCreationContext creationContext)
        {
            // Don't store reference
            GetFromPool(creationContext);
        }

        private static ContextualMenu Create() => new();
        private static void TearDown(ContextualMenu toTearDown)
        {
            ContextualMenuBox.ReleaseToPool(toTearDown.m_RootMenuBox);
            toTearDown.RemoveFromHierarchy();
            toTearDown.m_Closer.UnregisterCallback<MouseUpEvent>(toTearDown.OnMouseUp);
            toTearDown.m_OpenTime = 0f;
            toTearDown.m_OpenClickComplete = false;
            toTearDown.m_Direction = RUIContextualMenuGrowDirection.SE;
            toTearDown.styleSheets.Clear();
            toTearDown.m_MenuTreeData = null;
            toTearDown.m_RootMenuBox = null;
        }

        internal void Close() => ReleaseToPool(this);

        RUIContextualMenuGrowDirection IMenuBoxParent.Direction() => m_Direction;
        Rect IMenuBoxParent.AbsoluteRect() => new(m_OriginalClickPoint, Vector2.zero);

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (!m_OpenClickComplete)
            {
                // Ignore the first click if it wasn't a hold 
                m_OpenClickComplete = true;
                float timeSinceOpenMS = Time.time - m_OpenTime;
                if (timeSinceOpenMS < 0.3f) return;
            }
            Close();
        }

        public struct MenuCreationContext
        {
            public Vector2 position;
            public DropdownMenu menu;
            public StyleSheet styleSheetOverride;
            public VisualElement root;
            public RUIContextualMenuGrowDirection direction;
        }
    }

    public enum RUIContextualMenuGrowDirection
    {
        SE,
        NE,
        SW,
        NW,
    }
}
