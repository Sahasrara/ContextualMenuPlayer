using System;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace RUI
{
    public class RUIContextualMenuItem : VisualElement, IMenuBoxParent
    {
        private readonly static ObjectPool<RUIContextualMenuItem> s_Pool
            = new(Create, null, TearDown);
        private readonly Label m_Label;
        private readonly VisualElement m_Icon;
        private Action m_OnClick;
        private RUIContextualMenuNodeData m_ItemData;
        private RUIContextualMenuBox m_ParentMenu;
        private RUIContextualMenuBox m_SubMenu;
        private bool m_MouseIsHovering;

        public string MenuText { get => m_Label.text; set => m_Label.text = value; }
        public bool MouseIsHovering { get => m_MouseIsHovering; }

        internal static RUIContextualMenuItem GetFromPool(MenuBoxItemCreationContext ctx)
        {
            // Get Item from Pool
            RUIContextualMenuItem menuItem = s_Pool.Get();

            // Set Parent Menu
            menuItem.m_ParentMenu = ctx.parentMenu;

            // Set Menu Data
            menuItem.m_ItemData = ctx.itemData;

            // Style 
            if (ctx.itemData.IsSeparator) // Separator
            {
                menuItem.m_Label.style.display = DisplayStyle.None;
                menuItem.m_Icon.AddToClassList("contextual-menu-item-icon-separator");
            }
            else
            {
                // Set Name
                menuItem.MenuText = ctx.itemData.NodeName;

                // Sub Menu
                if (ctx.itemData.IsSubMenu)
                {
                    menuItem.m_Icon.AddToClassList("contextual-menu-item-icon-submenu");
                }
                // Action
                else
                {
                    if (ctx.itemData.IsChecked)
                    {
                        menuItem.m_Icon.AddToClassList("contextual-menu-item-icon-checked");
                    }
                    if (ctx.itemData.IsDisabled)
                    {
                        menuItem.SetEnabled(false);
                    }
                    else
                    {
                        menuItem.m_OnClick = ctx.itemData.Execute;
                    }
                }
            }

            // Register Event Handlers
            menuItem.RegisterCallback<MouseUpEvent>(menuItem.OnMouseUp);
            menuItem.RegisterCallback<MouseEnterEvent>(menuItem.OnMouseEnter);
            menuItem.RegisterCallback<MouseLeaveEvent>(menuItem.OnMouseLeave);

            // Return Item
            return menuItem;
        }

        internal static void ReleaseToPool(RUIContextualMenuItem toRelease)
            => s_Pool.Release(toRelease);

        private static RUIContextualMenuItem Create() => new();
        private static void TearDown(RUIContextualMenuItem toTearDown)
        {
            toTearDown.CloseSubMenu();
            toTearDown.RemoveFromHierarchy();
            toTearDown.UnregisterCallback<MouseUpEvent>(toTearDown.OnMouseUp);
            toTearDown.UnregisterCallback<MouseEnterEvent>(toTearDown.OnMouseEnter);
            toTearDown.UnregisterCallback<MouseLeaveEvent>(toTearDown.OnMouseLeave);
            toTearDown.MenuText = string.Empty;
            toTearDown.m_ItemData = null;
            toTearDown.m_OnClick = null;
            toTearDown.SetEnabled(true);
            toTearDown.m_Label.style.display = DisplayStyle.Flex;
            toTearDown.m_MouseIsHovering = false;
            // TODO
            toTearDown.m_Icon.RemoveFromClassList("contextual-menu-item-icon-checked");
            toTearDown.m_Icon.RemoveFromClassList("contextual-menu-item-icon-submenu");
            toTearDown.m_Icon.RemoveFromClassList("contextual-menu-item-icon-separator");
        }

        public RUIContextualMenuItem()
        {
            // Structure
            m_Label = new Label();
            m_Icon = new VisualElement();
            Add(m_Label);
            Add(m_Icon);

            // Style
            AddToClassList("contextual-menu-item-container");
            m_Label.AddToClassList("contextual-menu-item-label");
            m_Icon.AddToClassList("contextual-menu-item-icon");
        }

        public override bool ContainsPoint(Vector2 localPoint)
        {
            if (m_ItemData.IsSubMenu && m_SubMenu == null && m_MouseIsHovering)
            {
                m_ParentMenu.TryOpenSubMenu(this, this.LocalToWorld(localPoint));
            }
            return base.ContainsPoint(localPoint);
        }

        internal void OpenSubMenu()
        {
            if (m_ItemData.IsSubMenu)
            {
                // Close Siblings
                m_ParentMenu.CloseSubMenus(this);

                if (m_SubMenu == null)
                {
                    // Create Sub Menu
                    m_SubMenu = RUIContextualMenuBox.GetFromPool(new()
                    {
                        menuData = m_ItemData,
                        rootMenu = m_ParentMenu.RootMenu,
                        parentElement = this,
                    });
                }
            }
        }

        internal void CloseSubMenu()
        {
            if (m_SubMenu != null)
            {
                RUIContextualMenuBox.ReleaseToPool(m_SubMenu);
                m_SubMenu = null;
            }
        }

        bool IMenuBoxParent.IsTranslated() => m_ParentMenu.resolvedStyle.translate.x < 0;
        Rect IMenuBoxParent.AbsoluteRect()
        {
            Rect menuBoxRect = this.m_ParentMenu.layout;
            Rect itemRect = this.layout;
            Vector2 menuBoxPosition
                = m_ParentMenu.resolvedStyle.translate + (Vector3)menuBoxRect.min;
            return new(
                new(menuBoxPosition.x + itemRect.x, menuBoxPosition.y + itemRect.y),
                new(itemRect.width, itemRect.height));
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            switch (evt.button)
            {
                case (int)MouseButton.LeftMouse:
                case (int)MouseButton.RightMouse:
                    if (m_OnClick != null)
                    {
                        m_OnClick();
                        m_ParentMenu.CloseRoot();
                    }
                    m_ParentMenu.CloseSubMenus(this);
                    break;
            }
        }

        private void OnMouseEnter(MouseEnterEvent evt)
        {
            // Record Hover Begin
            m_MouseIsHovering = true;
            // Not a Sub Menu
            if (!m_ItemData.IsSubMenu) return;
            // Sub Menu Already Open
            if (m_SubMenu != null)
            {
                // Close Child Sub Menus
                m_SubMenu.CloseSubMenus();
                return;
            }
            m_ParentMenu.TryOpenSubMenu(this, evt.mousePosition);
        }

        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            // If Sub Menu is Open, Create Temporary Safe Zone
            if (m_SubMenu != null)
            {
                Rect subMenuRect = m_SubMenu.layout;
                float subMenuX = m_SubMenu.layout.position.x;
                Triangle safeZone = new()
                {
                    p0 = evt.mousePosition,
                    p1 = new(subMenuX, subMenuRect.y + subMenuRect.height),
                    p2 = new(subMenuX, subMenuRect.y),
                };
                m_ParentMenu.SetTemporarySafeZone(safeZone);
            }
            // Record Hover End
            m_MouseIsHovering = false;
        }

        internal struct MenuBoxItemCreationContext
        {
            public RUIContextualMenuNodeData itemData;
            public RUIContextualMenuBox parentMenu;
        }
    }
}