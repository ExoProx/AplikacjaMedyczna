using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace apkaStart
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PanelGlowny : Page
    {
        public PanelGlowny()
        {
            this.InitializeComponent();
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainFrame.CanGoBack)
            {
                App.MainFrame.GoBack();
            }
        }
        private void Wpisy_Click(object sender, RoutedEventArgs e)
        {
            App.MainFrame.Navigate(typeof(Wpisy));

        }
        private void Recepty_Click(object sender, RoutedEventArgs e)
        {
            App.MainFrame.Navigate(typeof(Recepty));

        }
        private void Skierowania_Click(object sender, RoutedEventArgs e)
        {
            App.MainFrame.Navigate(typeof(Skierowania));

        }
        private void Wyniki_Click(object sender, RoutedEventArgs e)
        {
            App.MainFrame.Navigate(typeof(Wyniki));

        }
    }
}
