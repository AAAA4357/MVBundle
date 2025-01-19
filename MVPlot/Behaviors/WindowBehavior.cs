using Microsoft.Xaml.Behaviors;
using MVPlot.Managers;
using System.Windows;

namespace MVPlot.Behaviors
{
    public class WindowBehavior : Behavior<Window>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            WindowManager.Register(AssociatedObject.GetType().Name, AssociatedObject);
        }
    }
}
