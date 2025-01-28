using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Text;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Npgsql;

namespace AplikacjaMedyczna
{
    public sealed partial class Wyniki : Page
    {
        private string cs = "host=bazamedyczna.cziamyieoagt.eu-north-1.rds.amazonaws.com;" +
                    "username=pacjent;" +
                    "Password=pacjent;" +
                    "Database=medical_database";

        public ObservableCollection<Result> AllResults { get; set; }
        private ObservableCollection<Result> filteredResults;

        public Wyniki()
        {
            this.InitializeComponent();
            NavigationHelper.SplitViewInstance = splitView;
            splitView.IsPaneOpen = true;
            CheckUserId();
            LoadResults();
            AllResults = new ObservableCollection<Result>();
            filteredResults = new ObservableCollection<Result>();
        }

        private async void LoadResults()
        {
            AllResults = new ObservableCollection<Result>(await GetResultsAsync());
            filteredResults = new ObservableCollection<Result>(AllResults);

            if (!AllResults.Any())
            {
                this.Loaded += async (s, e) =>
                {
                    await ShowMessageDialog("Brak dodanych wyników", "Nie znaleziono żadnych wyników w systemie.");
                };
            }

            FilteredListView.ItemsSource = filteredResults;
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

        private void OnFilterChanged(object sender, TextChangedEventArgs args)
        {
            var filtered = AllResults.Where(result => Filter(result));
            Remove_NonMatching(filtered);
            AddBack_Recepty(filtered);
        }

        private bool Filter(Result result)
        {
            return result.WynikNr.ToString().Contains(FilterWynikNr.Text, StringComparison.InvariantCultureIgnoreCase) &&
                   result.DataWykonaniaWyniku.Contains(FilterDataWykonania.Text, StringComparison.InvariantCultureIgnoreCase) &&
                   result.PersonelWykonujacyBadanie.Contains(FilterPersonel.Text, StringComparison.InvariantCultureIgnoreCase);
        }

        private void Remove_NonMatching(IEnumerable<Result> filteredData)
        {
            for (int i = filteredResults.Count - 1; i >= 0; i--)
            {
                var item = filteredResults[i];
                if (!filteredData.Contains(item))
                {
                    filteredResults.Remove(item);
                }
            }
        }

        private void AddBack_Recepty(IEnumerable<Result> filteredData)
        {
            foreach (var item in filteredData)
            {
                if (!filteredResults.Contains(item))
                {
                    filteredResults.Add(item);
                }
            }
        }

        private async void FilteredListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clickedResult = e.ClickedItem as Result;

            if (clickedResult != null)
            {
                ShowResultDetailDialog(clickedResult);
            }
        }

        private async void ShowResultDetailDialog(Result result)
        {
            var stackPanel = new StackPanel();

            var dialog = new ContentDialog
            {
                CloseButtonText = "Zamknij",
                CloseButtonStyle = (Style)Application.Current.Resources["CloseButtonStyle"],
                XamlRoot = this.XamlRoot
            };

            var headerGrid = new Grid
            {
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 74, 173)),
                Padding = new Thickness(10),
                Width = 477
            };

            var headerTextBlock = new TextBlock
            {
                Text = "Szczegóły Wyniku",
                TextAlignment = TextAlignment.Center,
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 20,
                FontWeight = FontWeights.Bold
            };

            headerGrid.Children.Add(headerTextBlock);
            stackPanel.Children.Add(headerGrid);

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
                    Content = "Otwórz plik z wynikiem",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 10, 0, 0),
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

                stackPanel.Children.Add(openFileButton);
            }

            dialog.Content = stackPanel;


            await dialog.ShowAsync();
        }

        public class Result
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

        private async Task ShowMessageDialog(string title, string content)
        {
            var dialog = new ContentDialog
            {
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot,
                CloseButtonStyle = (Style)Application.Current.Resources["PrimaryButtonStyle"],
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 240, 248, 255)),
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 74, 173))
            };

            var titleContainer = new Border
            {
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 74, 173)),
                Padding = new Thickness(10),
                Child = new TextBlock
                {
                    Text = title,
                    TextAlignment = TextAlignment.Center,
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    Width = 250
                }
            };

            dialog.Title = titleContainer;

            await dialog.ShowAsync();
        }
    }
}
