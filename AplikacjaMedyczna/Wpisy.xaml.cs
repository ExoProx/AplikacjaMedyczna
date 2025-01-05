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
        public Wpis SelectedWpis { get; set; } // For binding to ContentDialog
        public ObservableCollection<Wpis> WpisyCollection { get; set; }
        public Wpisy()
        {
            this.InitializeComponent();
            NavigationHelper.SplitViewInstance = splitView;
            splitView.IsPaneOpen = true;
            LoadWpisy();
            this.DataContext = this; // Set the DataContext for the page.
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
        private async void FilteredListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Cast the clicked item to a Wpis object
            var clickedWpis = e.ClickedItem as Wpis;

            if (clickedWpis != null)
            {
                // Update SelectedWpis
                SelectedWpis = clickedWpis;

                // Bind the SelectedWpis to the ContentDialog DataContext
                WpisDetailDialog.DataContext = SelectedWpis;

                // Show the ContentDialog
                await WpisDetailDialog.ShowAsync();
            }
        }
        public static ObservableCollection<Wpis> GetWpisy()
        {
            var wpisy = new ObservableCollection<Wpis>();
            var cs = "host=localhost;username=postgres;Password=admin;Database=BazaMedyczna";
            string pesel = SharedData.pesel;
            decimal peselNumeric;

            // Convert PESEL to numeric (decimal)
            if (!decimal.TryParse(pesel, out peselNumeric))
            {
                // Invalid PESEL format
                return new ObservableCollection<Wpis>(); ;
            }
            using (var connection = new NpgsqlConnection(cs))
            {
                connection.Open();
                string query = @"
                    SELECT 
                    Wpisy.""id"" AS wpisy_id, 
                    Wpisy.""wpis"",
                    Wpisy.""dataWpisu"",  
                    personel.""imie"", 
                    personel.""nazwisko"" 
                FROM 
                    ""WpisyMedyczne"" as Wpisy
                JOIN 
                    ""PersonelMedyczny"" as personel
                ON 
                    Wpisy.""idPersonelu"" = personel.""id"" WHERE Wpisy.""peselPacjenta"" = @pesel ORDER BY Wpisy.""dataWpisu"" DESC;";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@pesel", peselNumeric);
                    using (var reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            string imie = reader.GetString(3);
                            string nazwisko = reader.GetString(4);
                            wpisy.Add(new Wpis
                            {
                                Id = reader.GetInt32(0),
                                wpis = reader.GetString(1),
                                peselPacjenta = peselNumeric,
                                dataWpisu = reader.GetDateTime(2),
                                danePersonelu = String.Concat(imie, " ", nazwisko)
                            });
                        }
                    }
                }
            }

            return wpisy;
        }
        
        private void LoadWpisy()
        {
            WpisyCollection = GetWpisy(); // Load the data into the ObservableCollection.
        }
        private async void WpisDetailDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            // Check if the dialog should close, or cancel the close if needed
            if (SomeConditionToPreventClose())
            {
                args.Cancel = true; // This will prevent the dialog from closing
                                    // Optionally, you can show a message or handle additional logic
                var dialog = new ContentDialog
                {
                    Title = "Cannot Close",
                    Content = "Please complete all necessary actions before closing.",
                    CloseButtonText = "OK"
                };
                await dialog.ShowAsync(); // Show a new dialog if needed
            }
            else
            {
                // Perform any necessary cleanup or actions before closing
                // For example, you could save data or log an action
                
            }
        }

        private bool SomeConditionToPreventClose()
        {
            // Your custom condition to prevent closing, for example:
            // return true if the dialog should not close
            return false; // In this case, always allow closing
        }

    }
}
