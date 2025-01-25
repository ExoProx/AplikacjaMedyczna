using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace AplikacjaMedyczna
{
    public sealed partial class PanelGlowny : Page
    {
        public PanelGlowny()
        {
            this.InitializeComponent();
            NavigationHelper.SplitViewInstance = splitView;
            CheckUserId();
        }

        private void CheckUserId()
        {
            if (!string.IsNullOrEmpty(SharedData.id))
            {
                PatientChoiceButton.Visibility = Visibility.Visible;
                PatientChoiceButton.IsEnabled = true;
            }
        }

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                NavigationHelper.Navigate(button.Name);
            }
        }

        private void NavbarToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            NavigationHelper.TogglePane();
        }

        private void NavbarToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            NavigationHelper.TogglePane();
        }
    }
}