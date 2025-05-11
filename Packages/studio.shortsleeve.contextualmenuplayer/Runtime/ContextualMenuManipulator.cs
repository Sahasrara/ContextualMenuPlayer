using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace ContextualMenuPlayer
{
    //
    // Summary:
    //     Manipulator that displays a contextual menu when the user clicks the right mouse
    //     button or presses the menu key on the keyboard.
    public class ContextualMenuManipulator : MouseManipulator
    {
        // Cached Reflection Data
        private static readonly object[] s_ArgumentTrue = { true };
        private static readonly object[] s_ArgumentFalse = { false };
        private readonly ContextualMenuManager m_ContextualMenuManager;
        private PropertyInfo m_PropertyDisplayMenuHandledOSX;
        private MethodInfo m_PropertyDisplayMenuHandledOSXGetter;
        private MethodInfo m_PropertyDisplayMenuHandledOSXSetter;
        private bool DisplayMenuHandledOSX
        {
            get
            {
                if (m_PropertyDisplayMenuHandledOSX == null)
                    CacheReflectionData();
                return (bool)
                    m_PropertyDisplayMenuHandledOSXGetter.Invoke(m_ContextualMenuManager, null);
            }
            set
            {
                if (m_PropertyDisplayMenuHandledOSX == null)
                    CacheReflectionData();
                m_PropertyDisplayMenuHandledOSXSetter.Invoke(
                    m_ContextualMenuManager,
                    value ? s_ArgumentTrue : s_ArgumentFalse
                );
            }
        }

        public ContextualMenuManipulator()
            : this(new()) { }

        public ContextualMenuManipulator(ContextualMenuManager contextualMenuManager)
        {
            activators.Add(new() { button = MouseButton.RightMouse });
            if (
                Application.platform == RuntimePlatform.OSXEditor
                || Application.platform == RuntimePlatform.OSXPlayer
            )
            {
                activators.Add(
                    new() { button = MouseButton.LeftMouse, modifiers = EventModifiers.Control }
                );
            }
            m_ContextualMenuManager = contextualMenuManager;
        }

        //
        // Summary:
        //     Register the event callbacks on the manipulator target.
        protected override void RegisterCallbacksOnTarget()
        {
            if (
                Application.platform == RuntimePlatform.OSXEditor
                || Application.platform == RuntimePlatform.OSXPlayer
            )
            {
                target.RegisterCallback<MouseDownEvent>(OnMouseDownEventOSX);
                target.RegisterCallback<MouseUpEvent>(OnMouseUpEventOSX);
            }
            else
            {
                target.RegisterCallback<MouseUpEvent>(OnMouseUpDownEvent);
            }
        }

        //
        // Summary:
        //     Unregister the event callbacks from the manipulator target.
        protected override void UnregisterCallbacksFromTarget()
        {
            if (
                Application.platform == RuntimePlatform.OSXEditor
                || Application.platform == RuntimePlatform.OSXPlayer
            )
            {
                target.UnregisterCallback<MouseDownEvent>(OnMouseDownEventOSX);
                target.UnregisterCallback<MouseUpEvent>(OnMouseUpEventOSX);
            }
            else
            {
                target.UnregisterCallback<MouseUpEvent>(OnMouseUpDownEvent);
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
            if (!evt.isPropagationStopped)
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
            (evt.target as VisualElement)?.focusController?.IgnoreEvent(evt);
        }

        private void CacheReflectionData()
        {
            m_PropertyDisplayMenuHandledOSX =
                typeof(UnityEngine.UIElements.ContextualMenuManager).GetProperty(
                    "displayMenuHandledOSX",
                    BindingFlags.NonPublic | BindingFlags.Instance
                );
            m_PropertyDisplayMenuHandledOSXGetter = m_PropertyDisplayMenuHandledOSX.GetGetMethod(
                nonPublic: true
            );
            m_PropertyDisplayMenuHandledOSXSetter = m_PropertyDisplayMenuHandledOSX.GetSetMethod(
                nonPublic: true
            );
        }
    }
}
