using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Npgsql;

namespace AplikacjaMedyczna
{
    public sealed partial class Recepty : Page
    {
        public Recepta SelectedRecepta { get; set; }
        public ObservableCollection<Recepta> ReceptyCollection { get; set; }
        private ObservableCollection<Recepta> receptyFiltered;

        public Recepty()
        {
            this.InitializeComponent();
            NavigationHelper.SplitViewInstance = splitView;
            splitView.IsPaneOpen = true;
            LoadRecepty();
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
                ReceptaDetailDialog.PrimaryButtonText = "";
                ReceptaDetailDialog.IsPrimaryButtonEnabled = false;
            }
            if (SharedData.rola == "Lekarz")
            {
                WynikiButton.Visibility = Visibility.Visible;
                WynikiButton.IsEnabled = true;
                AddRecipeButton.Visibility = Visibility.Visible;
                AddRecipeButton.IsEnabled = true;
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
            var clickedRecepta = e.ClickedItem as Recepta;

            if (clickedRecepta != null)
            {
                SelectedRecepta = clickedRecepta;

                ReceptaDetailDialog.Content = null;
                ReceptaDetailDialog.DataContext = SelectedRecepta;

                var stackPanel = new StackPanel();
                AddTextBlock(stackPanel, "Przepisane leki:", SelectedRecepta.Leki);
                AddTextBlock(stackPanel, "Data wystawienia recepty:", SelectedRecepta.DataWystawieniaRecepty);
                AddTextBlock(stackPanel, "Data ważności recepty:", SelectedRecepta.DataWaznosciRecepty);
                AddTextBlock(stackPanel, "PESEL Pacjenta:", SelectedRecepta.PeselPacjenta.ToString());
                AddTextBlock(stackPanel, "Dane personelu:", SelectedRecepta.DanePersonelu);

                ReceptaDetailDialog.Content = stackPanel;
                ScrollViewer.SetVerticalScrollBarVisibility(ReceptaDetailDialog, ScrollBarVisibility.Auto);

                if (SelectedRecepta.IdPersonelu.ToString() == SharedData.id)
                {
                    ReceptaDetailDialog.PrimaryButtonText = "Edytuj";
                    ReceptaDetailDialog.IsPrimaryButtonEnabled = true;
                }
                else
                {
                    ReceptaDetailDialog.PrimaryButtonText = "";
                    ReceptaDetailDialog.IsPrimaryButtonEnabled = false;
                }

                await ReceptaDetailDialog.ShowAsync();
            }
        }

        public static ObservableCollection<Recepta> GetRecepty()
        {
            var recepty = new ObservableCollection<Recepta>();
            var cs = "host=bazamedyczna.cziamyieoagt.eu-north-1.rds.amazonaws.com;" +
                     "username=pacjent;" +
                     "Password=pacjent;" +
                     "Database=medical_database";
            string pesel = SharedData.pesel;

            if (!decimal.TryParse(pesel, out decimal peselNumeric))
            {
                return new ObservableCollection<Recepta>();
            }

            using (var connection = new NpgsqlConnection(cs))
            {
                connection.Open();
                string query = @"
            SELECT 
                Recepty.""id"" AS Recepty_id, 
                TO_CHAR(Recepty.""dataWystawienia"", 'DD.MM.YYYY') as Recepty_dataWystawienia, 
                TO_CHAR(Recepty.""dataWaznosci"", 'DD.MM.YYYY') as Recepty_dataWaznosci, 
                Recepty.""przypisaneLeki"",
                personel.""imie"" AS personel_imie, 
                personel.""nazwisko"" AS personel_nazwisko,
                personel.""id"" AS personel_id
            FROM 
                ""Recepty"" as Recepty
            JOIN 
                ""PersonelMedyczny"" as personel
            ON 
                Recepty.""idPersonelu"" = personel.""id"" 
            WHERE 
                Recepty.""peselPacjenta"" = @pesel 
            ORDER BY 
                Recepty.""dataWystawienia"" DESC;";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@pesel", peselNumeric);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string imie = reader.GetString(4);
                            string nazwisko = reader.GetString(5);
                            int personelId = reader.GetInt32(6);
                            recepty.Add(new Recepta
                            {
                                Id = reader.GetInt32(0),
                                DataWystawieniaRecepty = reader.GetString(1),
                                PeselPacjenta = peselNumeric,
                                DataWaznosciRecepty = reader.GetString(2),
                                DanePersonelu = String.Concat(imie, " ", nazwisko),
                                Leki = reader.GetString(3),
                                IdPersonelu = personelId
                            });
                        }
                    }
                }
            }

            return recepty;
        }

        private async void LoadRecepty()
        {
            ReceptyCollection = GetRecepty();
            receptyFiltered = new ObservableCollection<Recepta>(ReceptyCollection);

            if (!ReceptyCollection.Any())
            {
                this.Loaded += async (s, e) =>
                {
                    await ShowMessageDialog("Brak dodanych recept", "Nie znaleziono żadnych recept w systemie.");
                };
            }

            FilteredListView.ItemsSource = receptyFiltered;
        }

        private void OnFilterChanged(object sender, TextChangedEventArgs args)
        {
            var filtered = ReceptyCollection.Where(recepta => Filter(recepta));
            Remove_NonMatching(filtered);
            AddBack_Recepty(filtered);
        }

        private bool Filter(Recepta recepta)
        {
            return recepta.DataWystawieniaRecepty.Contains(FilterByDataWystawieniaRecepty.Text, StringComparison.InvariantCultureIgnoreCase) &&
                   recepta.DanePersonelu.Contains(FilterByDanePersonelu.Text, StringComparison.InvariantCultureIgnoreCase);
        }

        private void Remove_NonMatching(IEnumerable<Recepta> filteredData)
        {
            for (int i = receptyFiltered.Count - 1; i >= 0; i--)
            {
                var item = receptyFiltered[i];
                if (!filteredData.Contains(item))
                {
                    receptyFiltered.Remove(item);
                }
            }
        }

        private void AddBack_Recepty(IEnumerable<Recepta> filteredData)
        {
            foreach (var item in filteredData)
            {
                if (!receptyFiltered.Contains(item))
                {
                    receptyFiltered.Add(item);
                }
            }
        }

        private async void ReceptaDetailDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
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
            stackPanel.Children.Add(new TextBlock
            {
                Text = headerText,
                Style = (Style)Application.Current.Resources["HeaderTextBlockStyle"]
            });

            stackPanel.Children.Add(new TextBlock
            {
                Text = contentText,
                Style = (Style)Application.Current.Resources["ContentTextBlockStyle"],
                MaxWidth = 477
            });
        }
        private async Task EditButton_ClickAsync(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var selectedRecepta = ReceptaDetailDialog.DataContext as Recepta;

            if (selectedRecepta != null)
            {
                ReceptaDetailDialog.Hide();

                var editDialog = new ContentDialog
                {
                    PrimaryButtonText = "Zapisz",
                    CloseButtonText = "Anuluj",
                    XamlRoot = this.XamlRoot,
                    Style = (Style)Application.Current.Resources["ContentDialogStyle"],
                    PrimaryButtonStyle = (Style)Application.Current.Resources["PrimaryButtonStyle"],
                    CloseButtonStyle = (Style)Application.Current.Resources["CloseButtonStyle"]
                };

                var stackPanel = new StackPanel();

                var headerGrid = new Grid
                {
                    Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 74, 173)),
                    Padding = new Thickness(10),
                    Width = 477
                };

                var headerTextBlock = new TextBlock
                {
                    Text = "Edytuj Receptę",
                    TextAlignment = TextAlignment.Center,
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 20,
                    FontWeight = FontWeights.Bold
                };

                headerGrid.Children.Add(headerTextBlock);
                stackPanel.Children.Add(headerGrid);

                var przypisaneLekiTextBox = new TextBox
                {
                    Text = selectedRecepta.Leki,
                    Margin = new Thickness(0, 0, 0, 10),
                    AcceptsReturn = true,
                    TextWrapping = TextWrapping.Wrap,
                    Width = 477,
                    MaxLength = 256,
                    Height = 170
                };

                var przypisaneLekiLabel = new TextBlock
                {
                    Text = "Przypisane leki:",
                    Margin = new Thickness(0, 0, 0, 2),
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 74, 173))
                };
                stackPanel.Children.Add(przypisaneLekiLabel);

                ScrollViewer.SetVerticalScrollBarVisibility(przypisaneLekiTextBox, ScrollBarVisibility.Auto);
                stackPanel.Children.Add(new ScrollViewer
                {
                    Content = przypisaneLekiTextBox,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                });

                AddTextBlock(stackPanel, "Data wystawienia recepty:", selectedRecepta.DataWystawieniaRecepty.ToString());
                AddTextBlock(stackPanel, "Data ważności recepty:", selectedRecepta.DataWaznosciRecepty.ToString());
                AddTextBlock(stackPanel, "PESEL Pacjenta:", selectedRecepta.PeselPacjenta.ToString());
                AddTextBlock(stackPanel, "Dane personelu:", selectedRecepta.DanePersonelu);

                editDialog.Content = stackPanel;

                var result = await editDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    if (string.IsNullOrWhiteSpace(przypisaneLekiTextBox.Text))
                    {
                        await ShowMessageDialog("Błąd", "Pole 'Przypisane leki' nie może być puste.");
                        await EditButton_ClickAsync(sender, args);
                        return;
                    }

                    selectedRecepta.Leki = przypisaneLekiTextBox.Text;

                    var cs = "host=bazamedyczna.cziamyieoagt.eu-north-1.rds.amazonaws.com;" +
                             "username=lekarz;" +
                             "Password=lekarz;" +
                             "Database=medical_database";
                    using (var connection = new NpgsqlConnection(cs))
                    {
                        connection.Open();
                        string query = @"
                            UPDATE ""Recepty""
                            SET ""przypisaneLeki"" = @leki
                            WHERE ""id"" = @id";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@leki", selectedRecepta.Leki);
                            command.Parameters.AddWithValue("@id", selectedRecepta.Id);
                            command.ExecuteNonQuery();
                        }
                    }

                    var receptaToUpdate = ReceptyCollection.FirstOrDefault(r => r.Id == selectedRecepta.Id);
                    if (receptaToUpdate != null)
                    {
                        receptaToUpdate.Leki = selectedRecepta.Leki;
                    }

                    await ShowMessageDialog("Sukces", "Recepta została pomyślnie zaktualizowana.");
                }
                else
                {
                    await ShowMessageDialog("Anulowano", "Edycja recepty została anulowana.");
                }

                await ShowReceptaDetailDialog(selectedRecepta);
            }
        }

        private async Task ShowReceptaDetailDialog(Recepta recepta)
        {
            ReceptaDetailDialog.DataContext = recepta;

            var stackPanel = new StackPanel();

            AddTextBlock(stackPanel, "Przypisane leki:", recepta.Leki);
            AddTextBlock(stackPanel, "Data wystawienia recepty:", recepta.DataWystawieniaRecepty);
            AddTextBlock(stackPanel, "Data ważności recepty:", recepta.DataWaznosciRecepty);
            AddTextBlock(stackPanel, "PESEL Pacjenta:", recepta.PeselPacjenta.ToString());
            AddTextBlock(stackPanel, "Dane personelu:", recepta.DanePersonelu);



            ReceptaDetailDialog.Content = stackPanel;
            ReceptaDetailDialog.PrimaryButtonText = "Edytuj";
            ReceptaDetailDialog.CloseButtonText = "Close";
            ReceptaDetailDialog.Style = (Style)Application.Current.Resources["ContentDialogStyle"];
            ReceptaDetailDialog.PrimaryButtonStyle = (Style)Application.Current.Resources["PrimaryButtonStyle"];
            ReceptaDetailDialog.CloseButtonStyle = (Style)Application.Current.Resources["CloseButtonStyle"];

            await ReceptaDetailDialog.ShowAsync();
        }

        private async Task AddRecipeButton_ClickAsync(object sender, RoutedEventArgs e)
        {
            var addReceptaDialog = new ContentDialog
            {
                PrimaryButtonText = "Zapisz",
                CloseButtonText = "Anuluj",
                XamlRoot = this.XamlRoot,
                Style = (Style)Application.Current.Resources["ContentDialogStyle"],
                PrimaryButtonStyle = (Style)Application.Current.Resources["PrimaryButtonStyle"],
                CloseButtonStyle = (Style)Application.Current.Resources["CloseButtonStyle"]
            };

            var stackPanel = new StackPanel();

            var headerGrid = new Grid
            {
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 74, 173)),
                Padding = new Thickness(10),
                Width = 477
            };

            var headerTextBlock = new TextBlock
            {
                Text = "Dodaj Receptę",
                TextAlignment = TextAlignment.Center,
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 20,
                FontWeight = FontWeights.Bold
            };

            headerGrid.Children.Add(headerTextBlock);
            stackPanel.Children.Add(headerGrid);

            var przypisaneLekiTextBox = new TextBox
            {
                PlaceholderText = "Wpisz leki...",
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                Height = 200,
                Width = 477,
                Margin = new Thickness(0, 10, 0, 10),
                MaxLength = 256
            };
            ScrollViewer.SetVerticalScrollBarVisibility(przypisaneLekiTextBox, ScrollBarVisibility.Auto);

            stackPanel.Children.Add(przypisaneLekiTextBox);

            var dataWaznosciDatePicker = new CalendarDatePicker
            {
                MinDate = DateTime.Now.AddDays(1),
                Margin = new Thickness(0, 5, 0, 5)
            };

            var dataWaznosciLabel = new TextBlock
            {
                Text = "Data ważności recepty:",
                Margin = new Thickness(0, 5, 0, 5),
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 74, 173)),
                FontWeight = FontWeights.Bold,
                FontSize = 16
            };

            stackPanel.Children.Add(dataWaznosciLabel);
            stackPanel.Children.Add(dataWaznosciDatePicker);

            var jednorazowaCheckBox = new CheckBox
            {
                Content = "Jednorazowa",
                Margin = new Thickness(0, 10, 0, 10),
            };


            stackPanel.Children.Add(jednorazowaCheckBox);

            addReceptaDialog.Content = stackPanel;

            var result = await addReceptaDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                if (string.IsNullOrWhiteSpace(przypisaneLekiTextBox.Text))
                {
                    await ShowMessageDialog("Błąd", "Pole 'Przypisane leki' nie może być puste.");
                    await AddRecipeButton_ClickAsync(sender, e);
                    return;
                }

                if (dataWaznosciDatePicker.Date == null)
                {
                    await ShowMessageDialog("Błąd", "Musisz wybrać date ważności recepty.");
                    await AddRecipeButton_ClickAsync(sender, e);
                    return;
                }

                var przypisaneLeki = przypisaneLekiTextBox.Text.Trim();
                var dataWaznosci = dataWaznosciDatePicker.Date.Value.Date;

                var cs = "Host=bazamedyczna.cziamyieoagt.eu-north-1.rds.amazonaws.com;" +
                         "Username=lekarz;" +
                         "Password=lekarz;" +
                         "Database=medical_database";

                var jednorazowa = jednorazowaCheckBox.IsChecked;

                try
                {
                    using (var connection = new NpgsqlConnection(cs))
                    {
                        await connection.OpenAsync();
                        string query = jednorazowa == true
                            ? "INSERT INTO \"Recepty\" (id, \"przypisaneLeki\", \"dataWystawienia\", \"dataWaznosci\", \"peselPacjenta\", \"idPersonelu\", \"odebranieRecepty\") " +
                              "VALUES(default, @przypisaneLeki, CURRENT_DATE, @dataWaznosci, @pesel, @id, false);"
                            : "INSERT INTO \"Recepty\" (id, \"przypisaneLeki\", \"dataWystawienia\", \"dataWaznosci\", \"peselPacjenta\", \"idPersonelu\", \"odebranieRecepty\") " +
                              "VALUES(default, @przypisaneLeki, CURRENT_DATE, @dataWaznosci, @pesel, @id, null);";
                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@pesel", long.Parse(SharedData.pesel));
                            command.Parameters.AddWithValue("@id", long.Parse(SharedData.id));
                            command.Parameters.AddWithValue("@przypisaneLeki", przypisaneLeki);
                            command.Parameters.AddWithValue("@dataWaznosci", dataWaznosci);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                    LoadRecepty();
                    await ShowMessageDialog("Sukces", "Recepta została pomyślnie dodana.");
                }
                catch (Exception ex)
                {
                    await ShowMessageDialog("Błąd", $"Błąd połączenia z bazą danych: {ex.Message}");
                }
            }
            else
            {
                await ShowMessageDialog("Anulowano", "Dodawanie recepty zostało anulowane.");
            }
        }



        private async void AddRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            await AddRecipeButton_ClickAsync(sender, e);
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