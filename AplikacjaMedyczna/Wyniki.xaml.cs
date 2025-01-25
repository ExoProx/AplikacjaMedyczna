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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AplikacjaMedyczna
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Wyniki : Page
    {
        private string cs = "host=bazamedyczna.cziamyieoagt.eu-north-1.rds.amazonaws.com;" +
                    "username=pacjent;" +
                    "Password=pacjent;" +
                    "Database=medical_database";
        public Wyniki()
        {
            this.InitializeComponent();
            NavigationHelper.SplitViewInstance = splitView;
            splitView.IsPaneOpen = true;
            CheckUserId();
            LoadResults();
        }

        private async void LoadResults()
        {
            var results = await GetResultsAsync();
            FilteredListView.ItemsSource = results;
        }

        private async Task<List<Result>> GetResultsAsync()
        {
            var results = new List<Result>();

            using (var con = new NpgsqlConnection(cs))
            {
                await con.OpenAsync();

                var query = @"
        SELECT 
            Wyniki.id AS ""Wynik nr:"", 
            Wyniki.""dataWyniku"" AS ""Data Wykonania Wyniku:"",  
            personel.imie || ' ' || personel.nazwisko AS ""Personel Wykonujący Badanie:"",
            Wyniki.""wynikiBadania"" AS ""Wyniki Badania""
        FROM 
            ""WynikibadanDiagnostycznych"" AS Wyniki
        JOIN 
            ""PersonelMedyczny"" AS personel
        ON 
            Wyniki.""idPersonelu"" = personel.""id"" 
        WHERE 
            Wyniki.""peselPacjenta"" = @pesel
        ORDER BY 
            ""Data Wykonania Wyniku:"" DESC";

                using (var cmd = new NpgsqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@pesel", Convert.ToDecimal(SharedData.pesel)); // Replace with actual PESEL

                    using (var rdr = await cmd.ExecuteReaderAsync())
                    {
                        while (await rdr.ReadAsync())
                        {
                            results.Add(new Result
                            {
                                WynikNr = rdr.GetInt32(0),
                                DataWykonaniaWyniku = rdr.GetDateTime(1).ToString("yyyy-MM-dd"),
                                PersonelWykonujacyBadanie = rdr.GetString(2),
                                WynikiBadania = rdr.IsDBNull(3) ? string.Empty : rdr.GetString(3)
                            });
                        }
                    }
                }
            }

            return results;
        }

        private void FilteredListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var selectedResult = e.ClickedItem as Result;
            if (selectedResult != null)
            {
                ShowResultDetailDialog(selectedResult);
            }
        }

        private async void ShowResultDetailDialog(Result result)
        {
            var dialog = new ContentDialog
            {
                Title = "Szczegóły Wyniku",
                CloseButtonText = "Zamknij",
                PrimaryButtonText = "Edytuj",
                PrimaryButtonStyle = (Style)Application.Current.Resources["PrimaryButtonStyle"],
                CloseButtonStyle = (Style)Application.Current.Resources["CloseButtonStyle"],
                XamlRoot = this.XamlRoot // Set the XamlRoot property
            };
            dialog.PrimaryButtonClick += EditButton_Click;
            var stackPanel = new StackPanel
            {
                Padding = new Thickness(10)
            };

            stackPanel.Children.Add(new TextBlock { Text = "Wynik nr:", Style = (Style)Application.Current.Resources["HeaderTextBlockStyle"] });
            stackPanel.Children.Add(new TextBlock { Text = result.WynikNr.ToString(), Style = (Style)Application.Current.Resources["ContentTextBlockStyle"] });
            stackPanel.Children.Add(new TextBlock { Text = "Data Wykonania Wyniku:", Style = (Style)Application.Current.Resources["HeaderTextBlockStyle"] });
            stackPanel.Children.Add(new TextBlock { Text = result.DataWykonaniaWyniku, Style = (Style)Application.Current.Resources["ContentTextBlockStyle"] });
            stackPanel.Children.Add(new TextBlock { Text = "Personel Wykonujący Badanie:", Style = (Style)Application.Current.Resources["HeaderTextBlockStyle"] });
            stackPanel.Children.Add(new TextBlock { Text = result.PersonelWykonujacyBadanie, Style = (Style)Application.Current.Resources["ContentTextBlockStyle"] });
            stackPanel.Children.Add(new TextBlock { Text = "Wyniki Badania:", Style = (Style)Application.Current.Resources["HeaderTextBlockStyle"] });
            stackPanel.Children.Add(new TextBlock { Text = result.WynikiBadania, Style = (Style)Application.Current.Resources["ContentTextBlockStyle"] });

            dialog.Content = stackPanel;

            await dialog.ShowAsync();
        }

        private void EditButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Handle edit button click
        }

        private class Result
        {
            public int WynikNr { get; set; }
            public string DataWykonaniaWyniku { get; set; }
            public string PersonelWykonujacyBadanie { get; set; }
            public string WynikiBadania { get; set; }
        }
        private void AddResultButton_Click(object sender, RoutedEventArgs e)
        {
            App.MainFrame.Navigate(typeof(Insert_result_form));
        }
        private void CheckUserId()
        {
            if (!string.IsNullOrEmpty(SharedData.id))
            {
                PatientChoiceButton.Visibility = Visibility.Visible;
                PatientChoiceButton.IsEnabled = true;
                AddResultButton.Visibility = Visibility.Visible;
                AddResultButton.IsEnabled = true;
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
    }
}
