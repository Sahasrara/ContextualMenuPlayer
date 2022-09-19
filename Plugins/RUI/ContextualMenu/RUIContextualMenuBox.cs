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

            // Reset Layout 
            menuBox.style.top = -1;
            menuBox.style.left = -1;
            menuBox.style.translate = StyleKeyword.None;

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

            // Position 
            Rect parentRect = m_Parent.AbsoluteRect();
            Vector2 normalOrigin = new(parentRect.x + parentRect.width, parentRect.y);
            Vector2 translatedOrigin = parentRect.position;
            float distancePastBottomOfViewport
                = Mathf.Max(0, parentRect.y + resolvedStyle.height - viewportHeight);

            // Translation
            float normalExtent = normalOrigin.x + this.resolvedStyle.width;
            float translatedExtent = translatedOrigin.x - this.resolvedStyle.width;
            bool canTranslate = translatedExtent >= 0;
            bool mustTranslate = normalExtent > viewportWidth;

            // Apply Position and Translation
            if (mustTranslate || (m_Parent.IsTranslated() && canTranslate))
            {
                style.translate = new Translate(new Length(-100, LengthUnit.Percent), 0, 0);
                style.left = new Length(parentRect.x, LengthUnit.Pixel);
                style.top = new Length(
                    parentRect.y - distancePastBottomOfViewport, LengthUnit.Pixel);
            }
            else
            {
                style.translate = new Translate(new Length(0, LengthUnit.Percent), 0, 0);
                style.left = new Length(parentRect.x + parentRect.width, LengthUnit.Pixel);
                style.top = new Length(
                    parentRect.y - distancePastBottomOfViewport, LengthUnit.Pixel);
            }
        }

        internal struct MenuBoxCreationContext
        {
            public RUIContextualMenuNodeData menuData;
            public RUIContextualMenu rootMenu;
            public IMenuBoxParent parentElement;
        }
    }

    internal interface IMenuBoxParent
    {
        bool IsTranslated();
        Rect AbsoluteRect();
    }
}
