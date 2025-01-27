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
    public sealed partial class Skierowania : Page
    {
        public Skierowanie SelectedSkierowanie { get; set; }
        public ObservableCollection<Skierowanie> SkierowaniaCollection { get; set; }
        private ObservableCollection<Skierowanie> skierowaniaFiltered;

        public Skierowania()
        {
            this.InitializeComponent();
            NavigationHelper.SplitViewInstance = splitView;
            splitView.IsPaneOpen = true;
            LoadSkierowania();
            this.DataContext = this;
            CheckUserId();
        }

        private void CheckUserId()
        {
            if (!string.IsNullOrEmpty(SharedData.id))
            {
                PatientChoiceButton.Visibility = Visibility.Visible;
                PatientChoiceButton.IsEnabled = true;
            }
            else
            {
                SkierowanieDetailDialog.PrimaryButtonText = "";
                SkierowanieDetailDialog.IsPrimaryButtonEnabled = false;
            }
            if (SharedData.rola == "Lekarz")
            {
                AddRefferalButton.Visibility = Visibility.Visible;
                AddRefferalButton.IsEnabled = true;
            }
            if (SharedData.rola == "Specjalista" || string.IsNullOrEmpty(SharedData.id))
            {
                WynikiButton.Visibility = Visibility.Visible;
                WynikiButton.IsEnabled = true;
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

        private async void FilteredListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clickedSkierowanie = e.ClickedItem as Skierowanie;

            if (clickedSkierowanie != null)
            {
                SelectedSkierowanie = clickedSkierowanie;

                // Resetuj ContentDialog przed pokazaniem
                SkierowanieDetailDialog.Content = null;
                SkierowanieDetailDialog.DataContext = SelectedSkierowanie;

                // Utwórz nowy StackPanel i dodaj TextBlocki
                var stackPanel = new StackPanel();
                AddTextBlock(stackPanel, "Skierowanie:", SelectedSkierowanie.SkierowanieText);
                AddTextBlock(stackPanel, "Data skierowania:", SelectedSkierowanie.DataSkierowania);
                AddTextBlock(stackPanel, "PESEL Pacjenta:", SelectedSkierowanie.PeselPacjenta.ToString());
                AddTextBlock(stackPanel, "Dane personelu:", SelectedSkierowanie.DanePersonelu);

                SkierowanieDetailDialog.Content = stackPanel;

                // Check if the current doctor is the same as the doctor who made the skierowanie
                if (SelectedSkierowanie.IdPersonelu.ToString() == SharedData.id)
                {
                    SkierowanieDetailDialog.PrimaryButtonText = "Edytuj";
                    SkierowanieDetailDialog.IsPrimaryButtonEnabled = true;
                }
                else
                {
                    SkierowanieDetailDialog.PrimaryButtonText = "";
                    SkierowanieDetailDialog.IsPrimaryButtonEnabled = false;
                }

                await SkierowanieDetailDialog.ShowAsync();
            }
        }
        public static ObservableCollection<Skierowanie> GetSkierowania()
        {
            var skierowania = new ObservableCollection<Skierowanie>();
            var cs = "host=bazamedyczna.cziamyieoagt.eu-north-1.rds.amazonaws.com;" +
                     "username=pacjent;" +
                     "Password=pacjent;" +
                     "Database=medical_database";
            string pesel = SharedData.pesel;

            if (!decimal.TryParse(pesel, out decimal peselNumeric))
            {
                return new ObservableCollection<Skierowanie>();
            }

            using (var connection = new NpgsqlConnection(cs))
            {
                connection.Open();
                string query = @"
            SELECT 
                Skierowania.""id"",
                Skierowania.""skierowanie"",
                TO_CHAR(Skierowania.""dataSkierowania"", 'DD.MM.YYYY') AS data_skierowania_formatted,
                personel.""imie"",
                personel.""nazwisko"",
                personel.""id"" AS personel_id
            FROM 
                ""Skierowania"" as Skierowania
            JOIN 
                ""PersonelMedyczny"" as personel
            ON 
                Skierowania.""idPersonelu"" = personel.""id"" 
            WHERE 
                Skierowania.""peselPacjenta"" = @pesel 
            ORDER BY 
                Skierowania.""dataSkierowania"" DESC;";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@pesel", peselNumeric);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string imie = reader.GetString(3);
                            string nazwisko = reader.GetString(4);
                            int personelId = reader.GetInt32(5);
                            skierowania.Add(new Skierowanie
                            {
                                Id = reader.GetInt32(0),
                                SkierowanieText = reader.GetString(1),
                                PeselPacjenta = peselNumeric,
                                DataSkierowania = reader.GetString(2),
                                DanePersonelu = String.Concat(imie, " ", nazwisko),
                                IdPersonelu = personelId
                            });
                        }
                    }
                }
            }

            return skierowania;
        }

        private async void LoadSkierowania()
        {
            SkierowaniaCollection = GetSkierowania();
            skierowaniaFiltered = new ObservableCollection<Skierowanie>(SkierowaniaCollection);

            if (!SkierowaniaCollection.Any())
            {
                this.Loaded += async (s, e) =>
                {
                    await ShowMessageDialog("Brak dodanych skierowañ", "Nie znaleziono ¿adnych skierowañ w systemie.");
                };
            }

            FilteredListView.ItemsSource = skierowaniaFiltered;
        }

        private void OnFilterChanged(object sender, TextChangedEventArgs args)
        {
            var filtered = SkierowaniaCollection.Where(skierowanie => Filter(skierowanie));
            Remove_NonMatching(filtered);
            AddBack_Skierowania(filtered);
        }

        private bool Filter(Skierowanie skierowanie)
        {
            return skierowanie.DataSkierowania.Contains(FilterByDataSkierowania.Text, StringComparison.InvariantCultureIgnoreCase) &&
                   skierowanie.DanePersonelu.Contains(FilterByDanePersonelu.Text, StringComparison.InvariantCultureIgnoreCase);
        }

        private void Remove_NonMatching(IEnumerable<Skierowanie> filteredData)
        {
            for (int i = skierowaniaFiltered.Count - 1; i >= 0; i--)
            {
                var item = skierowaniaFiltered[i];
                if (!filteredData.Contains(item))
                {
                    skierowaniaFiltered.Remove(item);
                }
            }
        }

        private void AddBack_Skierowania(IEnumerable<Skierowanie> filteredData)
        {
            foreach (var item in filteredData)
            {
                if (!skierowaniaFiltered.Contains(item))
                {
                    skierowaniaFiltered.Add(item);
                }
            }
        }

        private async void SkierowanieDetailDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
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
            var selectedSkierowanie = SkierowanieDetailDialog.DataContext as Skierowanie;

            if (selectedSkierowanie != null)
            {
                SkierowanieDetailDialog.Hide();

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
                    Text = "Edytuj Skierowanie", // Mo¿esz zmieniæ tekst na dowolny
                    TextAlignment = TextAlignment.Center,
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 20,
                    FontWeight = FontWeights.Bold
                };

                headerGrid.Children.Add(headerTextBlock);
                stackPanel.Children.Add(headerGrid);

                // Dodaj TextBox dla skierowania
                var skierowanieTextBox = new TextBox
                {
                    Text = selectedSkierowanie.SkierowanieText,
                    Margin = new Thickness(0, 0, 0, 10),
                    TextWrapping = TextWrapping.Wrap,
                    AcceptsReturn = true,
                };

                // Dodaj TextBlock dla etykiety "Skierowanie:"
                var skierowanieLabel = new TextBlock
                {
                    Text = "Skierowanie:",
                    Margin = new Thickness(0, 0, 0, 2), // Minimalny margines dolny, aby odseparowaæ TextBlock od TextBox
                    FontSize = 14, // Opcjonalnie dostosuj rozmiar czcionki
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 74, 173))
                };
                stackPanel.Children.Add(skierowanieLabel);

                ScrollViewer.SetVerticalScrollBarVisibility(skierowanieTextBox, ScrollBarVisibility.Auto);
                stackPanel.Children.Add(new ScrollViewer
                {
                    Content = skierowanieTextBox,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Height = 200
                });

                // U¿yj metody pomocniczej do dodawania pozosta³ych TextBlock
                AddTextBlock(stackPanel, "Data Skierowania:", selectedSkierowanie.DataSkierowania.ToString());
                AddTextBlock(stackPanel, "PESEL Pacjenta:", selectedSkierowanie.PeselPacjenta.ToString());
                AddTextBlock(stackPanel, "Dane personelu:", selectedSkierowanie.DanePersonelu);

                editDialog.Content = stackPanel;

                var result = await editDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    if (string.IsNullOrWhiteSpace(skierowanieTextBox.Text))
                    {
                        await ShowMessageDialog("B³¹d", "Pole 'Skierowanie' nie mo¿e byæ puste.");
                        await EditButton_ClickAsync(sender, args);
                        return;
                    }

                    selectedSkierowanie.SkierowanieText = skierowanieTextBox.Text;

                    var cs = "host=bazamedyczna.cziamyieoagt.eu-north-1.rds.amazonaws.com;" +
                             "username=lekarz;" +
                             "Password=lekarz;" +
                             "Database=medical_database";
                    using (var connection = new NpgsqlConnection(cs))
                    {
                        connection.Open();
                        string query = @"
                            UPDATE ""Skierowania""
                            SET ""skierowanie"" = @skierowanie
                            WHERE ""id"" = @id";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@skierowanie", selectedSkierowanie.SkierowanieText);
                            command.Parameters.AddWithValue("@id", selectedSkierowanie.Id);
                            command.ExecuteNonQuery();
                        }
                    }

                    var skierowanieToUpdate = SkierowaniaCollection.FirstOrDefault(s => s.Id == selectedSkierowanie.Id);
                    if (skierowanieToUpdate != null)
                    {
                        skierowanieToUpdate.SkierowanieText = selectedSkierowanie.SkierowanieText;
                    }

                    await ShowMessageDialog("Sukces", "Skierowanie zosta³o pomyœlnie zaktualizowane.");
                }
                else
                {
                    await ShowMessageDialog("Anulowano", "Edycja skierowania zosta³a anulowana.");
                }

                await ShowSkierowanieDetailDialog(selectedSkierowanie);
            }
        }

        private async Task ShowSkierowanieDetailDialog(Skierowanie skierowanie)
        {
            SkierowanieDetailDialog.DataContext = skierowanie;

            var stackPanel = new StackPanel();

            // U¿yj metody pomocniczej do dodawania TextBlock
            AddTextBlock(stackPanel, "Skierowanie:", skierowanie.SkierowanieText);
            AddTextBlock(stackPanel, "Data Wpisu:", skierowanie.DataSkierowania);
            AddTextBlock(stackPanel, "PESEL Pacjenta:", skierowanie.PeselPacjenta.ToString());
            AddTextBlock(stackPanel, "Dane personelu:", skierowanie.DanePersonelu);

            SkierowanieDetailDialog.Content = stackPanel;
            SkierowanieDetailDialog.PrimaryButtonText = "Edytuj";
            SkierowanieDetailDialog.CloseButtonText = "Zamknij";
            SkierowanieDetailDialog.Style = (Style)Application.Current.Resources["ContentDialogStyle"];
            SkierowanieDetailDialog.PrimaryButtonStyle = (Style)Application.Current.Resources["PrimaryButtonStyle"]; // Przypisz styl PrimaryButton
            SkierowanieDetailDialog.CloseButtonStyle = (Style)Application.Current.Resources["CloseButtonStyle"];   // Przypisz styl CloseButton

            await SkierowanieDetailDialog.ShowAsync();
        }

        private async Task AddRefferalButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            // Utworzenie ContentDialog dla dodawania wpisu
            var addSkierowanieDialog = new ContentDialog
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
                Text = "Dodaj Skierowanie",
                TextAlignment = TextAlignment.Center,
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 20,
                FontWeight = FontWeights.Bold
            };

            headerGrid.Children.Add(headerTextBlock);
            stackPanel.Children.Add(headerGrid);

            // Pole tekstowe dla treœci wpisu
            var skierowanieTextBox = new TextBox
            {
                PlaceholderText = "Wpisz treœæ skierowania...",
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                Height = 200,
                Margin = new Thickness(0, 10, 0, 10),
                MaxLength = 256
            };
            ScrollViewer.SetVerticalScrollBarVisibility(skierowanieTextBox, ScrollBarVisibility.Auto);

            stackPanel.Children.Add(skierowanieTextBox);

            addSkierowanieDialog.Content = stackPanel;

            var result = await addSkierowanieDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // Sprawdzenie, czy pole tekstowe nie jest puste
                if (string.IsNullOrWhiteSpace(skierowanieTextBox.Text))
                {
                    await ShowMessageDialog("B³¹d", "Treœæ skierowania nie mo¿e byæ puste.");
                    await AddRefferalButton_ClickAsync(sender, e); // Ponowne otwarcie dialogu
                    return;
                }

                // Dodanie skierowania do bazy danych
                var skierowanie = skierowanieTextBox.Text;
                var connectionString = "host=bazamedyczna.cziamyieoagt.eu-north-1.rds.amazonaws.com;" +
                                       "username=lekarz;" +
                                       "Password=lekarz;" +
                                       "Database=medical_database";

                try
                {
                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();

                        string sql = "INSERT INTO public.\"Skierowania\" (id, \"skierowanie\", \"dataSkierowania\", \"peselPacjenta\", \"idPersonelu\") VALUES (default, @skierowanie, CURRENT_DATE, @pesel, @id);";
                        using (var command = new NpgsqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@skierowanie", skierowanie);
                            command.Parameters.AddWithValue("@pesel", long.Parse(SharedData.pesel));
                            command.Parameters.AddWithValue("@id", long.Parse(SharedData.id));
                            command.ExecuteNonQuery();
                        }
                    }
                    LoadSkierowania();
                    await ShowMessageDialog("Sukces", "Skierowanie zosta³o pomyœlnie dodane.");
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
                await ShowMessageDialog("Anulowano", "Dodawanie skierowania zosta³o anulowane.");
            }
        }

        private async void AddRefferalButton_Click(object sender, RoutedEventArgs e)
        {
            await AddRefferalButton_ClickAsync(sender, e);
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