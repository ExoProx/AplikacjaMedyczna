using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Contacts;
using Windows.UI;

namespace AplikacjaMedyczna
{
    public sealed partial class Wpisy : Page
    {
        public Wpis SelectedWpis { get; set; }
        public ObservableCollection<Wpis> WpisyCollection { get; set; }
        private ObservableCollection<Wpis> wpisyFiltered;

        public Wpisy()
        {
            this.InitializeComponent();
            NavigationHelper.SplitViewInstance = splitView;
            splitView.IsPaneOpen = true;
            LoadWpisy();
            this.DataContext = this;
            CheckUserId();
        }

        private void CheckUserId()
        {
            if (!string.IsNullOrEmpty(SharedData.id))
            {
                PatientChoiceButton.Visibility = Visibility.Visible;
                PatientChoiceButton.IsEnabled = true;
                AddDescriptionButton.Visibility = Visibility.Visible;
                AddDescriptionButton.IsEnabled = true;

                WpisDetailDialog.PrimaryButtonText = "Edytuj";
                WpisDetailDialog.IsPrimaryButtonEnabled = true;
            }
            else
            {
                WpisDetailDialog.PrimaryButtonText = "";
                WpisDetailDialog.IsPrimaryButtonEnabled = false;
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

        public async void FilteredListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clickedWpis = e.ClickedItem as Wpis;

            if (clickedWpis != null)
            {
                SelectedWpis = clickedWpis;

                // Resetuj ContentDialog przed pokazaniem
                WpisDetailDialog.Content = null;
                WpisDetailDialog.DataContext = SelectedWpis;

                // Utwórz nowy StackPanel i dodaj TextBlocki
                var stackPanel = new StackPanel();
                AddTextBlock(stackPanel, "Wpis:", SelectedWpis.WpisText);
                AddTextBlock(stackPanel, "Data Wpisu:", SelectedWpis.DataWpisu);
                AddTextBlock(stackPanel, "PESEL Pacjenta:", SelectedWpis.PeselPacjenta.ToString());
                AddTextBlock(stackPanel, "Dane personelu:", SelectedWpis.DanePersonelu);

                WpisDetailDialog.Content = stackPanel;

                await WpisDetailDialog.ShowAsync();
            }
        }
        public static ObservableCollection<Wpis> GetWpisy()
        {
            var wpisy = new ObservableCollection<Wpis>();
            var cs = "host=bazamedyczna.cziamyieoagt.eu-north-1.rds.amazonaws.com;" +
                     "username=pacjent;" +
                     "Password=pacjent;" +
                     "Database=medical_database";
            string pesel = SharedData.pesel;

            if (!decimal.TryParse(pesel, out decimal peselNumeric))
            {
                return new ObservableCollection<Wpis>();
            }

            using (var connection = new NpgsqlConnection(cs))
            {
                connection.Open();
                string query = @"
                    SELECT 
                        Wpisy.""id"" AS wpisy_id, 
                        Wpisy.""wpis"",
                        TO_CHAR(Wpisy.""dataWpisu"", 'DD.MM.YYYY') AS data_wpisu_formatted,  
                        personel.""imie"", 
                        personel.""nazwisko"" 
                    FROM 
                        ""WpisyMedyczne"" as Wpisy
                    JOIN 
                        ""PersonelMedyczny"" as personel
                    ON 
                        Wpisy.""idPersonelu"" = personel.""id"" 
                    WHERE 
                        Wpisy.""peselPacjenta"" = @pesel 
                    ORDER BY 
                        Wpisy.""dataWpisu"" DESC;";

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
                                WpisText = reader.GetString(1),
                                PeselPacjenta = peselNumeric,
                                DataWpisu = reader.GetString(2),
                                DanePersonelu = String.Concat(imie, " ", nazwisko)
                            });
                        }
                    }
                }
            }

            return wpisy;
        }

        private void LoadWpisy()
        {
            WpisyCollection = GetWpisy();
            wpisyFiltered = new ObservableCollection<Wpis>(WpisyCollection);
            FilteredListView.ItemsSource = wpisyFiltered;
        }

        private void OnFilterChanged(object sender, TextChangedEventArgs args)
        {
            var filtered = WpisyCollection.Where(wpis => Filter(wpis));
            Remove_NonMatching(filtered);
            AddBack_Wpisy(filtered);
        }

        private bool Filter(Wpis wpis)
        {
            return wpis.DataWpisu.Contains(FilterByDataWpisu.Text, StringComparison.InvariantCultureIgnoreCase) &&
                   wpis.DanePersonelu.Contains(FilterByDanePersonelu.Text, StringComparison.InvariantCultureIgnoreCase);
        }

        private void Remove_NonMatching(IEnumerable<Wpis> filteredData)
        {
            for (int i = wpisyFiltered.Count - 1; i >= 0; i--)
            {
                var item = wpisyFiltered[i];
                if (!filteredData.Contains(item))
                {
                    wpisyFiltered.Remove(item);
                }
            }
        }

        private void AddBack_Wpisy(IEnumerable<Wpis> filteredData)
        {
            foreach (var item in filteredData)
            {
                if (!wpisyFiltered.Contains(item))
                {
                    wpisyFiltered.Add(item);
                }
            }
        }

        private async void WpisDetailDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (SomeConditionToPreventClose())
            {
                args.Cancel = true;
                var dialog = new ContentDialog
                {
                    Title = "Cannot Close",
                    Content = "Please complete all necessary actions before closing.",
                    CloseButtonText = "OK"
                };
                await dialog.ShowAsync();
            }
        }

        private bool SomeConditionToPreventClose()
        {
            return false;
        }

        private async void EditButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            await EditButton_ClickAsync(sender, args);
        }

        private void AddTextBlock(StackPanel stackPanel, string headerText, string contentText)
        {
            // Dodaj TextBlock dla nag³ówka
            stackPanel.Children.Add(new TextBlock
            {
                Text = headerText,
                Style = (Style)Application.Current.Resources["HeaderTextBlockStyle"] // Styl dla nag³ówka
            });

            // Dodaj TextBlock dla treœci
            stackPanel.Children.Add(new TextBlock
            {
                Text = contentText,
                Style = (Style)Application.Current.Resources["ContentTextBlockStyle"] // Styl dla treœci
            });
        }
        private async Task EditButton_ClickAsync(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var selectedWpis = WpisDetailDialog.DataContext as Wpis;

            if (selectedWpis != null)
            {
                WpisDetailDialog.Hide();

                var editDialog = new ContentDialog
                {
                    PrimaryButtonText = "Zapisz",
                    CloseButtonText = "Anuluj",
                    XamlRoot = this.XamlRoot,
                    Style = (Style)Application.Current.Resources["ContentDialogStyle"],
                    PrimaryButtonStyle = (Style)Application.Current.Resources["PrimaryButtonStyle"], // Przypisz styl PrimaryButton
                    CloseButtonStyle = (Style)Application.Current.Resources["CloseButtonStyle"]   // Przypisz styl CloseButton
                };

                var stackPanel = new StackPanel();

                // Dodaj niestandardowy nag³ówek
                var headerGrid = new Grid
                {
                    Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 74, 173)), // Kolor t³a #004AAD
                    Padding = new Thickness(10),
                    Width = 477
                };

                var headerTextBlock = new TextBlock
                {
                    Text = "Edytuj Wpis", // Mo¿esz zmieniæ tekst na dowolny
                    TextAlignment = TextAlignment.Center,
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 20,
                    FontWeight = FontWeights.Bold
                };

                headerGrid.Children.Add(headerTextBlock);
                stackPanel.Children.Add(headerGrid);

                // Dodaj TextBox dla wpisu
                var wpisTextBox = new TextBox
                {
                    Text = selectedWpis.WpisText,
                    Margin = new Thickness(0, 0, 0, 10),
                    TextWrapping = TextWrapping.Wrap,
                    AcceptsReturn = true,
                };

                // Dodaj TextBlock dla etykiety "Wpis:"
                var wpisLabel = new TextBlock
                {
                    Text = "Wpis:",
                    Margin = new Thickness(0, 0, 0, 2), // Minimalny margines dolny, aby odseparowaæ TextBlock od TextBox
                    FontSize = 14, // Opcjonalnie dostosuj rozmiar czcionki
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 74, 173))
                };
                stackPanel.Children.Add(wpisLabel);

                ScrollViewer.SetVerticalScrollBarVisibility(wpisTextBox, ScrollBarVisibility.Auto);
                stackPanel.Children.Add(new ScrollViewer
                {
                    Content = wpisTextBox,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Height = 200
                });

                // U¿yj metody pomocniczej do dodawania pozosta³ych TextBlock
                AddTextBlock(stackPanel, "Data Wpisu:", selectedWpis.DataWpisu.ToString());
                AddTextBlock(stackPanel, "PESEL Pacjenta:", selectedWpis.PeselPacjenta.ToString());
                AddTextBlock(stackPanel, "Dane personelu:", selectedWpis.DanePersonelu);

                editDialog.Content = stackPanel;

                var result = await editDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    if (string.IsNullOrWhiteSpace(wpisTextBox.Text))
                    {
                        await ShowMessageDialog("B³¹d", "Pole 'Wpis' nie mo¿e byæ puste.");
                        await EditButton_ClickAsync(sender, args);
                        return;
                    }

                    selectedWpis.WpisText = wpisTextBox.Text;

                    var cs = "host=bazamedyczna.cziamyieoagt.eu-north-1.rds.amazonaws.com;" +
                             "username=lekarz;" +
                             "Password=lekarz;" +
                             "Database=medical_database";
                    using (var connection = new NpgsqlConnection(cs))
                    {
                        connection.Open();
                        string query = @"
                            UPDATE ""WpisyMedyczne""
                            SET ""wpis"" = @wpis
                            WHERE ""id"" = @id";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@wpis", selectedWpis.WpisText);
                            command.Parameters.AddWithValue("@id", selectedWpis.Id);
                            command.ExecuteNonQuery();
                        }
                    }

                    var wpisToUpdate = WpisyCollection.FirstOrDefault(w => w.Id == selectedWpis.Id);
                    if (wpisToUpdate != null)
                    {
                        wpisToUpdate.WpisText = selectedWpis.WpisText;
                    }

                    await ShowMessageDialog("Sukces", "Wpis zosta³ pomyœlnie zaktualizowany.");
                }
                else
                {
                    await ShowMessageDialog("Anulowano", "Edycja wpisu zosta³a anulowana.");
                }

                await ShowWpisDetailDialog(selectedWpis);
            }
        }

        private async Task ShowWpisDetailDialog(Wpis wpis)
        {
            WpisDetailDialog.DataContext = wpis;

            var stackPanel = new StackPanel();

            // U¿yj metody pomocniczej do dodawania TextBlock
            AddTextBlock(stackPanel, "Wpis:", wpis.WpisText);
            AddTextBlock(stackPanel, "Data Wpisu:", wpis.DataWpisu);
            AddTextBlock(stackPanel, "PESEL Pacjenta:", wpis.PeselPacjenta.ToString());
            AddTextBlock(stackPanel, "Dane personelu:", wpis.DanePersonelu);

            WpisDetailDialog.Content = stackPanel;
            WpisDetailDialog.PrimaryButtonText = "Edytuj";
            WpisDetailDialog.CloseButtonText = "Zamknij";
            WpisDetailDialog.Style = (Style)Application.Current.Resources["ContentDialogStyle"];
            WpisDetailDialog.PrimaryButtonStyle = (Style)Application.Current.Resources["PrimaryButtonStyle"]; // Przypisz styl PrimaryButton
            WpisDetailDialog.CloseButtonStyle = (Style)Application.Current.Resources["CloseButtonStyle"];   // Przypisz styl CloseButton

            await WpisDetailDialog.ShowAsync();
        }

        private async Task AddDescriptionButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            // Utworzenie ContentDialog dla dodawania wpisu
            var addWpisDialog = new ContentDialog
            {
                PrimaryButtonText = "Zapisz",
                CloseButtonText = "Anuluj",
                XamlRoot = this.XamlRoot,
                Style = (Style)Application.Current.Resources["ContentDialogStyle"],
                PrimaryButtonStyle = (Style)Application.Current.Resources["PrimaryButtonStyle"],
                CloseButtonStyle = (Style)Application.Current.Resources["CloseButtonStyle"]
            };

            var stackPanel = new StackPanel();

            // Nag³ówek dialogu
            var headerGrid = new Grid
            {
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 74, 173)),
                Padding = new Thickness(10),
                Width = 477
            };

            var headerTextBlock = new TextBlock
            {
                Text = "Dodaj Wpis",
                TextAlignment = TextAlignment.Center,
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 20,
                FontWeight = FontWeights.Bold
            };

            headerGrid.Children.Add(headerTextBlock);
            stackPanel.Children.Add(headerGrid);

            // Pole tekstowe dla treœci wpisu
            var wpisTextBox = new TextBox
            {
                PlaceholderText = "Wpisz treœæ wpisu...",
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                Height = 200,
                Margin = new Thickness(0, 10, 0, 10)
            };
            ScrollViewer.SetVerticalScrollBarVisibility(wpisTextBox, ScrollBarVisibility.Auto);

            stackPanel.Children.Add(wpisTextBox);

            addWpisDialog.Content = stackPanel;

            var result = await addWpisDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Sprawdzenie, czy pole tekstowe nie jest puste
                if (string.IsNullOrWhiteSpace(wpisTextBox.Text))
                {
                    await ShowMessageDialog("B³¹d", "Treœæ wpisu nie mo¿e byæ puste.");
                    await AddDescriptionButton_ClickAsync(sender, e); // Ponowne otwarcie dialogu
                    return;
                }

                // Dodanie wpisu do bazy danych
                var wpis = wpisTextBox.Text;
                var connectionString = "host=bazamedyczna.cziamyieoagt.eu-north-1.rds.amazonaws.com;" +
                                       "username=lekarz;" +
                                       "Password=lekarz;" +
                                       "Database=medical_database";

                try
                {
                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();

                        string sql = "INSERT INTO public.\"WpisyMedyczne\" (id, \"peselPacjenta\", \"idPersonelu\", wpis, \"dataWpisu\") VALUES (default, @pesel, @id, @wpis, CURRENT_DATE);";
                        using (var command = new NpgsqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@wpis", wpis);
                            command.Parameters.AddWithValue("@pesel", long.Parse(SharedData.pesel));
                            command.Parameters.AddWithValue("@id", long.Parse(SharedData.id));
                            command.ExecuteNonQuery();
                        }
                    }
                    LoadWpisy();
                    await ShowMessageDialog("Sukces", "Wpis zosta³ pomyœlnie dodany.");
                }
                catch (Exception ex)
                {
                    // Obs³uga b³êdu po³¹czenia z baz¹ danych
                    await ShowMessageDialog("B³¹d", "B³¹d po³¹czenia z baz¹ danych.");
                }
            }
            else
            {
                // Anulowanie dodawania wpisu
                await ShowMessageDialog("Anulowano", "Dodawanie wpisu zosta³o anulowane.");
            }
        }
        private async void AddDescriptionButton_Click(object sender, RoutedEventArgs e)
        {
            await AddDescriptionButton_ClickAsync(sender, e);
        }


        private async Task ShowMessageDialog(string title, string content)
        {
            var dialog = new ContentDialog
            {
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot,
                CloseButtonStyle = (Style)Application.Current.Resources["PrimaryButtonStyle"],
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 240, 248, 255)), // Kolor t³a dialogu
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 74, 173))     // Kolor tekstu w dialogu
            };

            // Tworzenie kontenera dla tytu³u
            var titleContainer = new Border
            {
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 74, 173)), // Kolor t³a (#004AAD)
                Padding = new Thickness(10), // Odstêpy wewnêtrzne
                Child = new TextBlock
                {
                    Text = title, // Ustawienie tekstu
                    TextAlignment = TextAlignment.Center,
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    Width = 250
                }
            };

            // Ustawienie dostosowanego elementu jako tytu³u dialogu
            dialog.Title = titleContainer;

            await dialog.ShowAsync();
        }
    }
}