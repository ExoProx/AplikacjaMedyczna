using AplikacjaMedyczna;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AplikacjaMedyczna
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 

    public sealed partial class Wpisy : Page
    {
        public Wpisy()
        {
            this.InitializeComponent();
            NavigationHelper.SplitViewInstance = splitView;
            splitView.IsPaneOpen = true;
            ShowWpisy(GetWpisy());
        }
        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                // Pass the button name or content to a helper method
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
        private void DodajButton_Click(object sender, RoutedEventArgs e)
        {
            App.MainFrame.Navigate(typeof(dodaj_wpis));

        }
        public static ObservableCollection<Wpis> GetWpisy()
        {
            var wpisy = new ObservableCollection<Wpis>();
            var cs = "host=localhost;username=postgres;Password=admin;Database=BazaMedyczna";

            using (var connection = new NpgsqlConnection(cs))
            {
                connection.Open();
                string query = "SELECT id, name, age FROM WpisyMedyczne";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            wpisy.Add(new Wpis
                            {
                                Id = reader.GetInt32(0),
                                wpis = reader.GetString(1),
                                peselPacjenta = reader.GetDecimal(2),
                                idPersonelu = reader.GetInt32(3),
                            });
                        }
                    }
                }
            }

            return wpisy;
        }
        public void ShowWpisy(ObservableCollection<Wpis> wpisy)
        {
            
        }
    }
}
