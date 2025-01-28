using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Npgsql;
using System.IO;
using WinRT.Interop;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;

namespace AplikacjaMedyczna
{
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
                Wyniki.""wynikiBadania"" AS ""Wyniki Badania"",
                personel.""id"" AS personel_id,
                Wyniki.""sciezkaDoPliku"" AS ""SciezkaDoPliku""  -- Dodajemy ścieżkę do pliku
            FROM 
                ""WynikibadanDiagnostycznych"" AS Wyniki
            JOIN 
                ""PersonelMedyczny"" AS personel
            ON 
                Wyniki.""idPersonelu"" = personel.""id"" 
            WHERE 
                Wyniki.""peselPacjenta"" = @pesel
            ORDER BY 
                ""Wynik nr:"" DESC;";

                using (var cmd = new NpgsqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@pesel", Convert.ToDecimal(SharedData.pesel));

                    using (var rdr = await cmd.ExecuteReaderAsync())
                    {
                        while (await rdr.ReadAsync())
                        {
                            results.Add(new Result
                            {
                                WynikNr = rdr.GetInt32(0),
                                DataWykonaniaWyniku = rdr.GetDateTime(1).ToString("dd.MM.yyyy"),
                                PersonelWykonujacyBadanie = rdr.GetString(2),
                                WynikiBadania = rdr.IsDBNull(3) ? string.Empty : rdr.GetString(3),
                                IdPersonelu = rdr.GetInt32(4),
                                FilePath = rdr.IsDBNull(5) ? string.Empty : rdr.GetString(5)
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
                CloseButtonStyle = (Style)Application.Current.Resources["CloseButtonStyle"],
                XamlRoot = this.XamlRoot
            };

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

            if (!string.IsNullOrEmpty(result.FilePath))
            {
                var openFileButton = new Button
                {
                    Content = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                {
                    new FontIcon { Glyph = "\uE8A5", Margin = new Thickness(0, 0, 5, 0) }, // Icon for opening file
                    new TextBlock { Text = "Otwórz plik z wynikiem" }
                }
                    },
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 10, 0, 0),
                    Background = new SolidColorBrush(ColorHelper.FromArgb(255, 0, 75, 172)), // Use #004bac color
                    Foreground = new SolidColorBrush(Microsoft.UI.Colors.White),
                    CornerRadius = new CornerRadius(5) // Add corner radius
                };

                openFileButton.Click += (sender, e) =>
                {
                    var newStackPanel = new StackPanel
                    {
                        Padding = new Thickness(10)
                    };

                    string url = $"https://studencki-portal-medyczny.pl/getfile.php?file={result.FilePath}";

                    if (result.FilePath.EndsWith(".pdf"))
                    {
                        var webView = new WebView2
                        {
                            Source = new Uri(url),
                            VerticalAlignment = VerticalAlignment.Stretch,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            Height = 600,
                            Width = 500
                        };
                        newStackPanel.Children.Add(webView);
                    }
                    else
                    {
                        BitmapImage bitmapImage = new BitmapImage(new Uri(url));
                        newStackPanel.Children.Add(new Image
                        {
                            Source = bitmapImage,
                            MaxHeight = 600,
                            MaxWidth = 600
                        });
                    }

                    dialog.Content = newStackPanel;
                };

                var downloadButton = new Button
                {
                    Content = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Children =
                {
                    new FontIcon { Glyph = "\uE896", Margin = new Thickness(0, 0, 5, 0) }, // Icon for downloading file
                    new TextBlock { Text = "Pobierz plik" }
                }
                    },
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 10, 0, 0),
                    Background = new SolidColorBrush(ColorHelper.FromArgb(255, 0, 75, 172)), // Use #004bac color
                    Foreground = new SolidColorBrush(Microsoft.UI.Colors.White),
                    CornerRadius = new CornerRadius(5) // Add corner radius
                };

                downloadButton.Click += async (sender, e) =>
                {
                    var url = $"https://studencki-portal-medyczny.pl/getfile.php?file={result.FilePath}";
                    var client = new HttpClient();
                    var response = await client.GetAsync(url);
                    var fileBytes = await response.Content.ReadAsByteArrayAsync();

                    var savePicker = new Windows.Storage.Pickers.FileSavePicker
                    {
                        SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads
                    };
                    savePicker.FileTypeChoices.Add("Plik", new List<string> { Path.GetExtension(result.FilePath) });
                    savePicker.SuggestedFileName = Path.GetFileName(result.FilePath);

                    // Initialize with window handle
                    var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
                    InitializeWithWindow.Initialize(savePicker, hwnd);

                    var file = await savePicker.PickSaveFileAsync();
                    if (file != null)
                    {
                        await Windows.Storage.FileIO.WriteBytesAsync(file, fileBytes);
                    }
                };

                stackPanel.Children.Add(openFileButton);
                stackPanel.Children.Add(downloadButton);
            }

            dialog.Content = stackPanel;

            await dialog.ShowAsync();
        }




        private class Result
        {
            public int WynikNr { get; set; }
            public string DataWykonaniaWyniku { get; set; }
            public string PersonelWykonujacyBadanie { get; set; }
            public string WynikiBadania { get; set; }
            public int IdPersonelu { get; set; }
            public string FilePath { get; internal set; }
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
            }
            if (SharedData.rola == "Lekarz")
            {
                WynikiButton.Visibility = Visibility.Visible;
                WynikiButton.IsEnabled = true;
            }
            if (SharedData.rola == "Specjalista")
            {
                AddResultButton.Visibility = Visibility.Visible;
                AddResultButton.IsEnabled = true;
                WynikiButton.Visibility = Visibility.Collapsed;
                WynikiButton.IsEnabled = false;
                PanelButton.Visibility = Visibility.Collapsed;
                PanelButton.IsEnabled = false;
                WpisyButton.Visibility = Visibility.Collapsed;
                WpisyButton.IsEnabled = false;
                SkierowaniaButton.Visibility = Visibility.Collapsed;
                SkierowaniaButton.IsEnabled = false;
                ReceptyButton.Visibility = Visibility.Collapsed;
                ReceptyButton.IsEnabled = false;
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
