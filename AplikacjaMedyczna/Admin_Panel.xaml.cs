using System;
using System.Collections.Generic;
using System.Data;
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
    public sealed partial class Admin_Panel : Page
    {
        private List<Personnel> allPersonnelList = new List<Personnel>();
        private List<Personnel> filteredPersonnelList = new List<Personnel>();
        private string connectionString = "host=bazamedyczna.cziamyieoagt.eu-north-1.rds.amazonaws.com;" +
                                          "username=administrator;" +
                                          "Password=haslo;" +
                                          "Database=medical_database";

        public Admin_Panel()
        {
            this.InitializeComponent();
            NavigationHelper.SplitViewInstance = splitView;
            splitView.IsPaneOpen = true;
            LoadPersonnelData();
        }

        private void AddPersonnelButton_Click(object sender, RoutedEventArgs e)
        {
            ShowAddPersonnelDialog();
        }

        private async void ShowAddPersonnelDialog()
        {
            var dialog = new ContentDialog
            {
                PrimaryButtonText = "Dodaj",
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
                Text = "Dodaj Personel",
                TextAlignment = TextAlignment.Center,
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 20,
                FontWeight = FontWeights.Bold
            };

            headerGrid.Children.Add(headerTextBlock);
            stackPanel.Children.Add(headerGrid);

            var imieTextBox = new TextBox
            {
                PlaceholderText = "Imię",
                BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.DarkBlue),
                BorderThickness = new Thickness(2),
                Margin = new Thickness(0, 10, 0, 5),
                Width = 477,
                MaxLength = 32
            };
            var nazwiskoTextBox = new TextBox
            {
                PlaceholderText = "Nazwisko",
                BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.DarkBlue),
                BorderThickness = new Thickness(2),
                Margin = new Thickness(0, 10, 0, 5),
                Width = 477,
                MaxLength = 32
            };
            var rolaComboBox = new ComboBox
            {
                PlaceholderText = "Rola",
                BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.DarkBlue),
                BorderThickness = new Thickness(2),
                Margin = new Thickness(0, 10, 0, 5),
                Width = 477
            };

            stackPanel.Children.Add(imieTextBox);
            stackPanel.Children.Add(nazwiskoTextBox);
            stackPanel.Children.Add(rolaComboBox);

            dialog.Content = stackPanel;

            await LoadRolesAsync(rolaComboBox);

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var rola = rolaComboBox.SelectedItem as Role;
                if (string.IsNullOrWhiteSpace(imieTextBox.Text) || string.IsNullOrWhiteSpace(nazwiskoTextBox.Text) || rola == null)
                {
                    await ShowMessageDialog("Błąd", "Uzupełnij wszystkie pola.");
                    ShowAddPersonnelDialog();
                    return;
                }

                var imie = imieTextBox.Text;
                var nazwisko = nazwiskoTextBox.Text;

                await InsertPersonnelAsync(imie, nazwisko, rola.Id, "haslo");
                LoadPersonnelData();

            }
            else if (result == ContentDialogResult.None)
            {
                await ShowMessageDialog("Anulowano", "Dodawanie personelu zostało anulowane.");
            }
        }

        private async Task ShowEditPersonnelDialog(Personnel personnel)
        {
            var dialog = new ContentDialog
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
                Text = "Zmień Rolę",
                TextAlignment = TextAlignment.Center,
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 20,
                FontWeight = FontWeights.Bold
            };

            headerGrid.Children.Add(headerTextBlock);
            stackPanel.Children.Add(headerGrid);

            var rolaComboBox = new ComboBox
            {
                PlaceholderText = "Role",
                BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.DarkBlue),
                BorderThickness = new Thickness(2),
                Margin = new Thickness(0, 10, 0, 5),
                Width = 477
            };

            stackPanel.Children.Add(rolaComboBox);

            await LoadRolesAsync(rolaComboBox);

            dialog.Content = stackPanel;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var selectedRole = rolaComboBox.SelectedItem as Role;
                if (selectedRole == null)
                {
                    await ShowMessageDialog("Błąd", "Wybierz rolę.");
                    await ShowEditPersonnelDialog(personnel);
                    return;
                }
                await UpdatePersonnelRoleAsync(personnel.Id, selectedRole.Id);
                LoadPersonnelData();
            }
            else if (result == ContentDialogResult.None)
            {
                await ShowMessageDialog("Anulowano", "Edycja roli została anulowana.");
            }
        }

        private async Task LoadRolesAsync(ComboBox comboBox)
        {
            var roles = new List<Role>();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var query = "SELECT id, nazwa FROM \"RolePersonelu\"";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            roles.Add(new Role
                            {
                                Id = reader.GetInt32(0),
                                Nazwa = reader.GetString(1)
                            });
                        }
                    }
                }
            }

            comboBox.ItemsSource = roles;
            comboBox.DisplayMemberPath = "Nazwa";
            comboBox.SelectedValuePath = "Id";
        }

        private async Task InsertPersonnelAsync(string imie, string nazwisko, int idRoli, string haslo)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var query = @"INSERT INTO ""PersonelMedyczny"" (""imie"", ""nazwisko"", ""idRoli"", ""haslo"")
                              VALUES (@imie, @nazwisko, @idRoli, crypt(@haslo, gen_salt('bf')))";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@imie", imie);
                    command.Parameters.AddWithValue("@nazwisko", nazwisko);
                    command.Parameters.AddWithValue("@idRoli", idRoli);
                    command.Parameters.AddWithValue("@haslo", haslo);

                    await command.ExecuteNonQueryAsync();
                }
            }
            await ShowMessageDialog("Sukces", "Personel został dodany.");
        }

        private async void LoadPersonnelData()
        {
            allPersonnelList = await GetPersonnelAsync();
            filteredPersonnelList = new List<Personnel>(allPersonnelList);
            MedicalStaffListView.ItemsSource = filteredPersonnelList;
        }

        private async Task<List<Personnel>> GetPersonnelAsync()
        {
            var personnelList = new List<Personnel>();

            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = @"
                SELECT 
                    pm.id,
                    pm.imie,
                    pm.nazwisko,
                    nr.nazwa AS rola,
                    pm.aktywne
                FROM 
                    ""PersonelMedyczny"" pm
                JOIN 
                    ""RolePersonelu"" nr 
                ON 
                    pm.""idRoli"" = nr.""id""
                ORDER BY pm.id;";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    using (NpgsqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            personnelList.Add(new Personnel
                            {
                                Id = reader.GetInt32(0),
                                Imie = reader.GetString(1),
                                Nazwisko = reader.GetString(2),
                                Rola = reader.GetString(3),
                                Aktywne = reader.GetBoolean(4)
                            });
                        }
                    }
                }
            }

            return personnelList;
        }

        private void OnFilterChanged(object sender, TextChangedEventArgs args)
        {
            filteredPersonnelList = allPersonnelList
                .Where(personnel => Filter(personnel))
                .ToList();

            MedicalStaffListView.ItemsSource = filteredPersonnelList;
        }

        private bool Filter(Personnel personnel)
        {
            if (!string.IsNullOrWhiteSpace(FilterNumerId.Text) &&
                !personnel.Id.ToString().Contains(FilterNumerId.Text, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(FilterImie.Text) &&
                !personnel.Imie.Contains(FilterImie.Text, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(FilterNazwisko.Text) &&
                !personnel.Nazwisko.Contains(FilterNazwisko.Text, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(FilterProfesja.Text) &&
                !personnel.Rola.Contains(FilterProfesja.Text, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            return true;
        }

        private async void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Personnel personnel)
            {
                string message = personnel.Aktywne
                    ? $"Czy chcesz dezaktywować pracownika {personnel.Imie} {personnel.Nazwisko}?"
                    : $"Czy chcesz aktywować pracownika {personnel.Imie} {personnel.Nazwisko}?";

                string message2 = personnel.Aktywne
                    ? $"Dezaktywacja Pracownika"
                    : $"Aktywacja Pracownika";

                string message3 = personnel.Aktywne
                    ? $"Dezaktywacja pracownika została anulowana."
                    : $"Aktywacja pracownika została anulowana.";

                string message4 = personnel.Aktywne
                    ? $"Pracownik został dezaktywowany."
                    : $"Pracownik został aktywowany.";

                var dialog = new ContentDialog
                {
                    PrimaryButtonText = "Tak",
                    CloseButtonText = "Nie",
                    XamlRoot = this.Content.XamlRoot,
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
                    Text = message2,
                    TextAlignment = TextAlignment.Center,
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 20,
                    FontWeight = FontWeights.Bold
                };

                headerGrid.Children.Add(headerTextBlock);
                stackPanel.Children.Add(headerGrid);

                var messageTextBlock = new TextBlock
                {
                    Text = message,
                    Margin = new Thickness(0, 10, 0, 5),
                    FontSize = 16,
                    Foreground = new SolidColorBrush(Colors.Black),
                    TextWrapping = TextWrapping.Wrap,
                    Width = 477
                };

                stackPanel.Children.Add(messageTextBlock);

                dialog.Content = stackPanel;

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    await UpdatePersonnelStatusAsync(personnel.Id, !personnel.Aktywne);
                    personnel.Aktywne = !personnel.Aktywne;
                    await ShowMessageDialog("Sukces", message4);
                    LoadPersonnelData();
                }
                else
                {
                    checkBox.IsChecked = !checkBox.IsChecked;
                    await ShowMessageDialog("Anulowano", message3);
                }
            }
        }

        private async Task UpdatePersonnelStatusAsync(int id, bool isActive)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                string query = @"
                    UPDATE ""PersonelMedyczny""
                    SET aktywne = @isActive
                    WHERE id = @id;";

                using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@isActive", isActive);
                    command.Parameters.AddWithValue("@id", id);

                    await command.ExecuteNonQueryAsync();
                }
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

        private async void EditPersonnelButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int personnelId)
            {
                var personnel = allPersonnelList.FirstOrDefault(p => p.Id == personnelId);
                if (personnel != null)
                {
                    await ShowEditPersonnelDialog(personnel);
                }
            }
        }

        private async Task UpdatePersonnelRoleAsync(int personnelId, int newRoleId)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var query = @"
            UPDATE ""PersonelMedyczny""
            SET ""idRoli"" = @newRoleId
            WHERE ""id"" = @personnelId;";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@newRoleId", newRoleId);
                    command.Parameters.AddWithValue("@personnelId", personnelId);

                    await command.ExecuteNonQueryAsync();
                }
            }
            await ShowMessageDialog("Sukces", "Rola została zmieniona.");
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

        private class Personnel
        {
            public int Id { get; set; }
            public string Imie { get; set; }
            public string Nazwisko { get; set; }
            public string Rola { get; set; }
            public bool Aktywne { get; set; }
        }

        private class Role
        {
            public int Id { get; set; }
            public string Nazwa { get; set; }
        }
    }
}