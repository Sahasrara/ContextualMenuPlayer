using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace RUI
{
    //
    // Summary:
    //     Manipulator that displays a contextual menu when the user clicks the right mouse
    //     button or presses the menu key on the keyboard.
    public class RUIContextualMenuManipulator : MouseManipulator
    {
        // Cached Reflection Data
        private static readonly object[] s_ArgumentTrue = new object[] { true };
        private static readonly object[] s_ArgumentFalse = new object[] { false };
        private readonly RUIContextualMenuManager m_ContextualMenuManager;
        private PropertyInfo m_PropertyDisplayMenuHandledOSX;
        private MethodInfo m_PropertyDisplayMenuHandledOSXGetter;
        private MethodInfo m_PropertyDisplayMenuHandledOSXSetter;
        private bool DisplayMenuHandledOSX
        {
            get
            {
                if (m_PropertyDisplayMenuHandledOSX == null) CacheReflectionData();
                return (bool)m_PropertyDisplayMenuHandledOSXGetter.Invoke(
                    m_ContextualMenuManager, null);
            }
            set
            {
                if (m_PropertyDisplayMenuHandledOSX == null) CacheReflectionData();
                m_PropertyDisplayMenuHandledOSXSetter.Invoke(
                    m_ContextualMenuManager, value ? s_ArgumentTrue : s_ArgumentFalse);
            }
        }

        public RUIContextualMenuManipulator() : this(new RUIContextualMenuManager()) { }
        public RUIContextualMenuManipulator(RUIContextualMenuManager contextualMenuManager)
        {
            activators.Add(new ManipulatorActivationFilter
            {
                button = MouseButton.RightMouse
            });
            if (Application.platform == RuntimePlatform.OSXEditor
                || Application.platform == RuntimePlatform.OSXPlayer)
            {
                activators.Add(new ManipulatorActivationFilter
                {
                    button = MouseButton.LeftMouse,
                    modifiers = EventModifiers.Control
                });
            }
            m_ContextualMenuManager = contextualMenuManager;
        }

        //
        // Summary:
        //     Register the event callbacks on the manipulator target.
        protected override void RegisterCallbacksOnTarget()
        {
            if (Application.platform == RuntimePlatform.OSXEditor
                || Application.platform == RuntimePlatform.OSXPlayer)
            {
                base.target.RegisterCallback<MouseDownEvent>(OnMouseDownEventOSX);
                base.target.RegisterCallback<MouseUpEvent>(OnMouseUpEventOSX);
            }
            else
            {
                base.target.RegisterCallback<MouseUpEvent>(OnMouseUpDownEvent);
            }
        }

        //
        // Summary:
        //     Unregister the event callbacks from the manipulator target.
        protected override void UnregisterCallbacksFromTarget()
        {
            if (Application.platform == RuntimePlatform.OSXEditor
                || Application.platform == RuntimePlatform.OSXPlayer)
            {
                base.target.UnregisterCallback<MouseDownEvent>(OnMouseDownEventOSX);
                base.target.UnregisterCallback<MouseUpEvent>(OnMouseUpEventOSX);
            }
            else
            {
                base.target.UnregisterCallback<MouseUpEvent>(OnMouseUpDownEvent);
            }
        }

        private void OnMouseUpDownEvent(IMouseEvent evt)
        {
            if (CanStartManipulation(evt))
            {
                DoDisplayMenu(evt as EventBase);
            }
        }

        private void OnMouseDownEventOSX(MouseDownEvent evt)
        {
            DisplayMenuHandledOSX = false;
            if (!evt.isDefaultPrevented)
            {
                OnMouseUpDownEvent(evt);
            }
        }

        private void OnMouseUpEventOSX(MouseUpEvent evt)
        {
            if (!DisplayMenuHandledOSX)
            {
                OnMouseUpDownEvent(evt);
            }
        }

        private void DoDisplayMenu(EventBase evt)
        {
            m_ContextualMenuManager.DisplayMenu(evt, evt.target);
            evt.StopPropagation();
            evt.PreventDefault();
        }

        private void CacheReflectionData()
        {
            m_PropertyDisplayMenuHandledOSX = typeof(ContextualMenuManager).GetProperty(
                "displayMenuHandledOSX", BindingFlags.NonPublic | BindingFlags.Instance);
            m_PropertyDisplayMenuHandledOSXGetter = m_PropertyDisplayMenuHandledOSX
                .GetGetMethod(nonPublic: true);
            m_PropertyDisplayMenuHandledOSXSetter = m_PropertyDisplayMenuHandledOSX
                .GetSetMethod(nonPublic: true);
        }
    }
}
