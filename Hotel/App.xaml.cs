using System.Configuration;
using System.Data;
using System.Windows;

namespace Hotel
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            EventManager.RegisterClassHandler(
                typeof(Window),
                Window.LoadedEvent,
                new RoutedEventHandler(WindowLoaded));
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is Window window)
            {
                window.WindowState = WindowState.Maximized;
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }

    }

}
