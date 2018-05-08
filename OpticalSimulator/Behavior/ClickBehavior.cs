using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OpticalSimulator.Behavior
{
    public enum EventType
    {
        LeftMouseClick,
        RightMouseClick,
        DoubleClick,
    }

    public class ClickBehavior : DependencyObject
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.RegisterAttached("Command", typeof(ICommand), typeof(ClickBehavior), new PropertyMetadata(OnChangedCommand));

        public static ICommand GetCommand(Control target)
        {
            return (ICommand)target.GetValue(CommandProperty);
        }

        public static void SetCommand(Control target, ICommand value)
        {
            target.SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.RegisterAttached("CommandParameter", typeof(object), typeof(ClickBehavior), new PropertyMetadata(default(object)));

        public static object GetCommandParameter(Control target)
        {
            return target.GetValue(CommandParameterProperty);
        }

        public static void SetCommandParameter(Control target, object value)
        {
            target.SetValue(CommandParameterProperty, value);
        }

        public static readonly DependencyProperty EventTypeProperty = DependencyProperty.RegisterAttached("EventType", typeof(EventType), typeof(ClickBehavior), new PropertyMetadata(EventType.LeftMouseClick));

        public static EventType GetEventType(Control target)
        {
            return (EventType)target.GetValue(EventTypeProperty);
        }

        public static void SetEventType(Control target, EventType value)
        {
            target.SetValue(EventTypeProperty, value);
        }

        private static void OnChangedCommand(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Control control = d as Control;
            EventType eventType = GetEventType(control);

            switch (eventType)
            {
                case EventType.LeftMouseClick:
                    control.PreviewMouseLeftButtonDown += Control_Click;
                    break;
                case EventType.RightMouseClick:
                    control.PreviewMouseRightButtonDown += Control_Click;
                    break;
                case EventType.DoubleClick:
                    control.PreviewMouseDoubleClick += Control_Click;
                    break;
            }
        }

        private static void Control_Click(object sender, MouseButtonEventArgs e)
        {
            Control control = sender as Control;
            ICommand command = GetCommand(control);
            object parameter = GetCommandParameter(control);

            if (command.CanExecute(parameter))
            {
                command.Execute(parameter);
                e.Handled = true;
            }
        }
    }
}
