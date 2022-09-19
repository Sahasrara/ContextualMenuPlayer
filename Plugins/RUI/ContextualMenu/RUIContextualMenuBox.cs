using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace RUI
{
    internal class RUIContextualMenuBox : VisualElement
    {
        private const float SafeZoneTimeoutSecs = 0.3f;
        private const long SafeZoneTimeoutMSecs = (long)(SafeZoneTimeoutSecs * 1000);
        private static readonly ObjectPool<RUIContextualMenuBox> s_MenuBoxPool =
            new(Create, null, TearDown);
        private readonly IVisualElementScheduledItem m_RetrySubMenuOpen;
        private float m_SubMenuCreateTime;
        private Triangle m_SubMenuTemporarySafeZone;
        private RUIContextualMenu m_Root;
        private IMenuBoxParent m_Parent;
        private RUIContextualMenuItem m_LastSubMenuAttempt;
        private RUIContextualMenuGrowDirection m_Direction;

        public RUIContextualMenuGrowDirection Direction { get => m_Direction; }
        public RUIContextualMenu RootMenu { get => m_Root; }

        private RUIContextualMenuBox()
        {
            AddToClassList("contextual-menu-box");

            // Temporary Safe Zone
            m_SubMenuCreateTime = 2 * (-SafeZoneTimeoutSecs);
            m_RetrySubMenuOpen = schedule.Execute(RetryTryOpenSubMenu);
            m_RetrySubMenuOpen.Pause();
        }

        internal static RUIContextualMenuBox GetFromPool(MenuBoxCreationContext ctx)
        {
            // Grab from Pool
            RUIContextualMenuBox menuBox = s_MenuBoxPool.Get();

            // Set Root
            menuBox.m_Root = ctx.rootMenu;

            // Set Parent
            menuBox.m_Parent = ctx.parentElement;

            // Set Direction
            menuBox.m_Direction = ctx.direction;

            // Reset Layout 
            menuBox.style.top = -1;
            menuBox.style.left = -1;
            // menuBox.style.translate = StyleKeyword.None;

            // Add Items
            int childCount = ctx.menuData.ChildCount;
            List<RUIContextualMenuNodeData> itemsData = ctx.menuData.Children;
            for (int i = 0; i < childCount; i++)
            {
                // Grab Menu Data
                RUIContextualMenuNodeData itemData = itemsData[i];

                // Create Item
                RUIContextualMenuItem itemElement = RUIContextualMenuItem.GetFromPool(new()
                {
                    itemData = itemData,
                    parentMenu = menuBox,
                });

                // Add to Menu
                menuBox.Add(itemElement);
            }

            // Add to Root
            ctx.rootMenu.Add(menuBox);

            // Add Event Handlers
            menuBox.RegisterCallback<GeometryChangedEvent>(menuBox.OnGeometryChanged);

            // Return
            return menuBox;
        }
        internal static void ReleaseToPool(RUIContextualMenuBox menuBox)
            => s_MenuBoxPool.Release(menuBox);

        private static RUIContextualMenuBox Create() => new();
        private static void TearDown(RUIContextualMenuBox toTearDown)
        {
            toTearDown.UnregisterCallback<GeometryChangedEvent>(toTearDown.OnGeometryChanged);
            toTearDown.RemoveFromHierarchy();
            for (int i = toTearDown.childCount - 1; i >= 0; i--)
            {
                RUIContextualMenuItem.ReleaseToPool(toTearDown[i] as RUIContextualMenuItem);
            }
            toTearDown.m_Root = null;
            toTearDown.m_Parent = null;
            toTearDown.m_Direction = RUIContextualMenuGrowDirection.SE;
            toTearDown.m_LastSubMenuAttempt = null;
        }

        internal void SetTemporarySafeZone(Triangle safeZone)
        {
            // Set Time
            m_SubMenuCreateTime = Time.time;
            // Set Zone
            m_SubMenuTemporarySafeZone = safeZone;
        }

        internal bool IsInSafeZone(Vector2 point)
        {
            bool requiresSafeCheck = (Time.time - m_SubMenuCreateTime) < SafeZoneTimeoutSecs;
            bool isSafe = requiresSafeCheck && m_SubMenuTemporarySafeZone.ContainsPoint(point);
            return isSafe;
        }

        internal void TryOpenSubMenu(RUIContextualMenuItem item, Vector2 mousePosition)
        {
            if (!IsInSafeZone(mousePosition))
            {
                item.OpenSubMenu();
                return;
            }

            // Try again after timeout
            if (m_LastSubMenuAttempt != item)
            {
                m_LastSubMenuAttempt = item;
                m_RetrySubMenuOpen.ExecuteLater(SafeZoneTimeoutMSecs);
            }
        }

        internal void Close() => ReleaseToPool(this);
        internal void CloseRoot() => m_Root.Close();
        internal void CloseSubMenus(RUIContextualMenuItem excluding = null)
        {
            for (int i = 0; i < this.childCount; i++)
            {
                RUIContextualMenuItem childItem = this[i] as RUIContextualMenuItem;
                if (childItem != excluding) childItem.CloseSubMenu();
            }
        }

        private void RetryTryOpenSubMenu()
        {
            // Check if mouse is still hovering over last menu that tried to open
            if (m_LastSubMenuAttempt != null && m_LastSubMenuAttempt.MouseIsHovering)
            {
                m_LastSubMenuAttempt.OpenSubMenu();
                m_LastSubMenuAttempt = null;
            }
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            // Viewport 
            float viewportWidth = m_Root.resolvedStyle.width;
            float viewportHeight = m_Root.resolvedStyle.height;

            // Parent Rect
            Rect parentRect = m_Parent.AbsoluteRect();

            // X-Axis Position 
            float normalOriginX = parentRect.x + parentRect.width;
            float normalExtentX = normalOriginX + resolvedStyle.width;
            float translatedOriginX = parentRect.x - resolvedStyle.width;
            bool mustTranslateX = normalExtentX > viewportWidth;
            bool willTranslateX;
            if (!mustTranslateX)
            {
                bool canTranslateX = translatedOriginX >= 0;
                bool parentTranslatedX = m_Parent.Direction() switch
                {
                    RUIContextualMenuGrowDirection.NW or RUIContextualMenuGrowDirection.SW => true,
                    _ => false,
                };
                willTranslateX = parentTranslatedX && canTranslateX;
            }
            else
            {
                willTranslateX = true;
            }
            float finalX = willTranslateX
                ? translatedOriginX
                : normalOriginX
                ;

            // Y-Axis Position
            float originY;
            float extentY;
            float distancePastViewport;
            bool willTranslateY = m_Parent.Direction() switch
            {
                RUIContextualMenuGrowDirection.NE or RUIContextualMenuGrowDirection.NW => true,
                _ => false,
            };
            if (willTranslateY)
            {
                extentY = parentRect.y + parentRect.height;
                originY = extentY - resolvedStyle.height;
                distancePastViewport = Mathf.Min(0, originY);
            }
            else
            {
                originY = parentRect.y;
                extentY = originY + resolvedStyle.height;
                distancePastViewport = Mathf.Max(0, extentY - viewportHeight);
            }
            float finalY = originY - distancePastViewport;

            // Apply Position
            style.left = new Length(finalX, LengthUnit.Pixel);
            style.top = new Length(finalY, LengthUnit.Pixel);

            // Store Direction
            if (willTranslateX)
            {
                m_Direction = willTranslateY
                    ? RUIContextualMenuGrowDirection.NW
                    : RUIContextualMenuGrowDirection.SW;
            }
            else
            {
                m_Direction = willTranslateY
                    ? RUIContextualMenuGrowDirection.NE
                    : RUIContextualMenuGrowDirection.SE;
            }
        }

        internal struct MenuBoxCreationContext
        {
            public RUIContextualMenuNodeData menuData;
            public RUIContextualMenu rootMenu;
            public IMenuBoxParent parentElement;
            public RUIContextualMenuGrowDirection direction;
        }
    }

    internal interface IMenuBoxParent
    {
        RUIContextualMenuGrowDirection Direction();
        Rect AbsoluteRect();
    }
}
