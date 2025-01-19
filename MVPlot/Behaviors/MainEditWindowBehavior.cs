using Microsoft.Xaml.Behaviors;
using MVPlot.Managers;
using System.Windows;

namespace MVPlot.Behaviors
{
    public class MainEditWindowBehavior : Behavior<Window>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Closed += OnWindowClosed;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Closed -= OnWindowClosed;
        }

        private void OnWindowClosed(object? sender, EventArgs e)
        {
            PlotProjectManager.ClosePlotProject();
        }
    }
}
