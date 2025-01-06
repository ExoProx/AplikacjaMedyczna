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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AplikacjaMedyczna
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Skierowania : Page
    {
        public Skierowanie SelectedSkierowanie { get; set; } // For binding to ContentDialog
        public ObservableCollection<Skierowanie> SkierowaniaCollection { get; set; }
        public Skierowania()
        {
            this.InitializeComponent();
            NavigationHelper.SplitViewInstance = splitView;
            splitView.IsPaneOpen = true;
            LoadSkierowania();
            this.DataContext = this; // Set the DataContext for the page.
            CheckUserId();
        }
        private void AddRefferalButton_Click(object sender, RoutedEventArgs e)
        {
            App.MainFrame.Navigate(typeof(Insert_referral_Form));
        }
        private void CheckUserId()
        {
            if (!string.IsNullOrEmpty(SharedData.id))
            {
                PatientChoiceButton.Visibility = Visibility.Visible;
                PatientChoiceButton.IsEnabled = true;
                AddRefferalButton.Visibility = Visibility.Visible;
                AddRefferalButton.IsEnabled = true;
            }
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
        
        private async void FilteredListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Cast the clicked item to a Wpis object
            var clickedSkierowanie = e.ClickedItem as Skierowanie;

            if (clickedSkierowanie != null)
            {
                // Update SelectedWpis
                SelectedSkierowanie = clickedSkierowanie;

                // Bind the SelectedWpis to the ContentDialog DataContext
                SkierowanieDetailDialog.DataContext = SelectedSkierowanie;

                // Show the ContentDialog
                await SkierowanieDetailDialog.ShowAsync();
            }
        }
        public static ObservableCollection<Skierowanie> GetSkierowania()
        {
            var skierowania = new ObservableCollection<Skierowanie>();
            var cs = "host=localhost;username=pacjent;Password=haslo;Database=BazaMedyczna";
            string pesel = SharedData.pesel;
            decimal peselNumeric;

            // Convert PESEL to numeric (decimal)
            if (!decimal.TryParse(pesel, out peselNumeric))
            {
                // Invalid PESEL format
                return new ObservableCollection<Skierowanie>(); ;
            }
            using (var connection = new NpgsqlConnection(cs))
            {
                connection.Open();
                string query = @"
                    SELECT 
                    Skierowania.""id"",
                    Skierowania.""skierowanie"" ,
                    Skierowania.""dataSkierowania"",  
                    personel.""imie"", 
                    personel.""nazwisko""
                FROM 
                    ""Skierowania"" as Skierowania
                JOIN 
                    ""PersonelMedyczny"" as personel
                ON 
                    Skierowania.""idPersonelu"" = personel.""id"" WHERE Skierowania.""peselPacjenta"" = @pesel ORDER BY Skierowania.""dataSkierowania"" DESC;";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@pesel", peselNumeric);
                    using (var reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            string imie = reader.GetString(3);
                            string nazwisko = reader.GetString(4);
                            skierowania.Add(new Skierowanie
                            {
                                Id = reader.GetInt32(0),
                                skierowanie = reader.GetString(1),
                                peselPacjenta = peselNumeric,
                                dataSkierowania = reader.GetDateTime(2),
                                danePersonelu = String.Concat(imie, " ", nazwisko)

                            });
                        }
                    }
                }
            }

            return skierowania;
        }

        private void LoadSkierowania()
        {
            SkierowaniaCollection = GetSkierowania(); // Load the data into the ObservableCollection.
        }
    }
}
