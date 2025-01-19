using CommunityToolkit.Mvvm.Input;
using MVPlot.Managers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MVPlot.Utilities
{
    public class DialogUtility
    {
        static Window GetStringInputDialogWindow(string captain, string title, string defaultValue) => new()
        {
            Width = 300,
            Height = 150,
            ResizeMode = ResizeMode.NoResize,
            Title = captain,
            Icon = new BitmapImage(new Uri("pack://application:,,,/Resources/MVPlot.ico")),
            Content = new StackPanel()
            {
                Children =
                {
                    new TextBlock()
                    {
                        FontSize = 20,
                        Margin = new(8),
                        Text = title
                    },
                    new TextBox()
                    {
                        Margin = new(8,0,8,0),
                        Text = defaultValue
                    },
                    new Button()
                    {
                        Content = LanguageManager.Instance["Confirm"],
                        Margin = new(0,8,0,8)
                    }
                }
            }
        };

        static Window? currentWindow;

        static ICommand? InputCommand;

        public static string ShowInputString(string captain, string title, string defaultValue = "")
        {
            Window DialogWindow = GetStringInputDialogWindow(captain, title, defaultValue);
            currentWindow = DialogWindow;
            InputCommand = new RelayCommand(Input);
            DialogWindow.InputBindings.Add(new KeyBinding(InputCommand, Key.Enter, ModifierKeys.None));
            ((TextBox)((StackPanel)DialogWindow.Content).Children[1]).Focus();
            ((Button)((StackPanel)DialogWindow.Content).Children[2]).Click += (object? _, RoutedEventArgs _) =>
            {
                DialogWindow.Close();
            };
            DialogWindow.ShowDialog();
            return ((TextBox)((StackPanel)DialogWindow.Content).Children[1]).Text;
        }

        private static void Input()
        {
            currentWindow!.Close();
            currentWindow = null;
        }
    }
}
