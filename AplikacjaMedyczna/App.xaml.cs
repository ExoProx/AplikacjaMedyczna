using System.Threading;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AplikacjaMedyczna
{
    public partial class App : Application
    {
        private static Mutex mutex = null;
        public static Window MainWindow { get; private set; }
        public static Frame MainFrame { get; private set; }

        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            const string appName = "AplikacjaMedyczna";
            bool createdNew;

            mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                Application.Current.Exit();
                return;
            }

            MainWindow = new Window();
            MainFrame = new Frame();
            MainWindow.Content = MainFrame;
            MainFrame.Navigate(typeof(LoginPage));
            MainWindow.Activate();
        }
    }
}