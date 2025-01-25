using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Npgsql;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AplikacjaMedyczna
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Admin_Panel : Page
    {
        private List<Personnel> allPersonnelList = new List<Personnel>();
        private string connectionString = "host=bazamedyczna.cziamyieoagt.eu-north-1.rds.amazonaws.com;" +
                        "username=administrator;" +
                        "Password=haslo;" +
                        "Database=medical_database"; 
        public Admin_Panel()
        {
            this.InitializeComponent();
            NavigationHelper.SplitViewInstance = splitView;
            LoadPersonnelData();
        }

        private void PeselTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                string text = textBox.Text;
                textBox.Text = new string(text.Where(char.IsDigit).ToArray());
                textBox.SelectionStart = textBox.Text.Length; // Set cursor to the end
            }
        }

        private void NumerKontaktowyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                string text = textBox.Text;
                textBox.Text = new string(text.Where(char.IsDigit).ToArray());
                textBox.SelectionStart = textBox.Text.Length; // Set cursor to the end
            }
        }
        private void AddPersonnelButton_Click(object sender, RoutedEventArgs e)
        {
            ShowAddPersonnelDialog();
        }
        private async void ShowAddPersonnelDialog()
        {
            var dialog = new ContentDialog
            {
                Title = "Dodaj Personel",
                PrimaryButtonText = "Dodaj",
                CloseButtonText = "Anuluj",
                XamlRoot = this.XamlRoot, // Set the XamlRoot property
              
            };

            var titleBorder = new Border
            {
                Background = new SolidColorBrush(Microsoft.UI.Colors.DarkBlue),
                Child = new TextBlock
                {
                    Text = "Dodaj Personel",
                    Foreground = new SolidColorBrush(Microsoft.UI.Colors.White),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 20,
                    TextAlignment = TextAlignment.Center,
                    FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                    Width = 500, // Set static width
                    Height = 30 // Set static height
                }
            };

            dialog.Title = titleBorder;

            var stackPanel = new StackPanel();

            var imieTextBox = new TextBox
            {
                PlaceholderText = "Imię",
                BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.DarkBlue),
                BorderThickness = new Thickness(2),
                Margin = new Thickness(0, 10, 0, 5),
                Width = 500 // Set static width
            };
            var nazwiskoTextBox = new TextBox
            {
                PlaceholderText = "Nazwisko",
                BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.DarkBlue),
                BorderThickness = new Thickness(2),
                Margin = new Thickness(0, 10, 0, 5),
                Width = 500 // Set static width
            };
            var rolaComboBox = new ComboBox
            {
                PlaceholderText = "Rola",
                BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.DarkBlue),
                BorderThickness = new Thickness(2),
                Margin = new Thickness(0, 10, 0, 5),
                Width = 500 // Set static width
            };

            stackPanel.Children.Add(imieTextBox);
            stackPanel.Children.Add(nazwiskoTextBox);
            stackPanel.Children.Add(rolaComboBox);

            dialog.Content = stackPanel;

            await LoadRolesAsync(rolaComboBox);

            var primaryButtonStyle = new Style(typeof(Button));
            primaryButtonStyle.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Microsoft.UI.Colors.DarkBlue)));
            primaryButtonStyle.Setters.Add(new Setter(Button.ForegroundProperty, new SolidColorBrush(Microsoft.UI.Colors.White)));
            primaryButtonStyle.Setters.Add(new Setter(Button.CornerRadiusProperty, new CornerRadius(10)));

            var closeButtonStyle = new Style(typeof(Button));
            closeButtonStyle.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Microsoft.UI.Colors.Red)));
            closeButtonStyle.Setters.Add(new Setter(Button.ForegroundProperty, new SolidColorBrush(Microsoft.UI.Colors.White)));
            closeButtonStyle.Setters.Add(new Setter(Button.CornerRadiusProperty, new CornerRadius(10)));

            dialog.PrimaryButtonStyle = primaryButtonStyle;
            dialog.CloseButtonStyle = closeButtonStyle;

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var imie = imieTextBox.Text;
                var nazwisko = nazwiskoTextBox.Text;
                var rola = rolaComboBox.SelectedItem as Role;

                if (rola != null)
                {
                    await InsertPersonnelAsync(imie, nazwisko, rola.Id, "haslo");
                    LoadPersonnelData(); // Refresh the data
                }
            }
        }
        private async Task ShowEditPersonnelDialog(Personnel personnel)
        {
            var dialog = new ContentDialog
            {
                
                Title = "Zmień Rolę",
                PrimaryButtonText = "Zapisz",
                CloseButtonText = "Anuluj",
                XamlRoot = this.XamlRoot,
            };

            var stackPanel = new StackPanel();

            var rolaComboBox = new ComboBox
            {

                PlaceholderText = "Role",
                BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.DarkBlue),
                BorderThickness = new Thickness(2),
                Margin = new Thickness(0, 10, 0, 5),
                Width = 500
            };

            stackPanel.Children.Add(rolaComboBox);

            dialog.Content = stackPanel;

            await LoadRolesAsync(rolaComboBox);


            var primaryButtonStyle = new Style(typeof(Button));
            primaryButtonStyle.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Microsoft.UI.Colors.DarkBlue)));
            primaryButtonStyle.Setters.Add(new Setter(Button.ForegroundProperty, new SolidColorBrush(Microsoft.UI.Colors.White)));
            primaryButtonStyle.Setters.Add(new Setter(Button.CornerRadiusProperty, new CornerRadius(10)));

            var CloseButtonStyle = new Style(typeof(Button));
            CloseButtonStyle.Setters.Add(new Setter(Button.BackgroundProperty, new SolidColorBrush(Microsoft.UI.Colors.Red)));
            CloseButtonStyle.Setters.Add(new Setter(Button.ForegroundProperty, new SolidColorBrush(Microsoft.UI.Colors.White)));
            CloseButtonStyle.Setters.Add(new Setter(Button.CornerRadiusProperty, new CornerRadius(10)));

            dialog.PrimaryButtonStyle = primaryButtonStyle;
            dialog.CloseButtonStyle = CloseButtonStyle;

            var result = await dialog.ShowAsync();


            if (result == ContentDialogResult.Primary)
            {
                var selectedRole = rolaComboBox.SelectedItem as Role;
                if (selectedRole != null)
                {
                    await UpdatePersonnelRoleAsync(personnel.Id, selectedRole.Id);
                    LoadPersonnelData(); // Refresh the data
                }
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

                var query = @"
            INSERT INTO ""PersonelMedyczny"" (""imie"", ""nazwisko"", ""idRoli"", ""haslo"")
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
        }

        private async void LoadPersonnelData()
        {
            allPersonnelList = await GetPersonnelAsync();
            MedicalStaffListView.ItemsSource = allPersonnelList;
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

        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            string filterId = FilterNumerId.Text.ToLower();
            string filterImie = FilterImie.Text.ToLower();
            string filterNazwisko = FilterNazwisko.Text.ToLower();
            string filterProfesja = FilterProfesja.Text.ToLower();

            var filteredList = allPersonnelList.Where(p =>
                (string.IsNullOrEmpty(filterId) || p.Id.ToString().Contains(filterId)) &&
                (string.IsNullOrEmpty(filterImie) || p.Imie.ToLower().Contains(filterImie)) &&
                (string.IsNullOrEmpty(filterNazwisko) || p.Nazwisko.ToLower().Contains(filterNazwisko)) &&
                (string.IsNullOrEmpty(filterProfesja) || p.Rola.ToLower().Contains(filterProfesja))
            ).ToList();

            MedicalStaffListView.ItemsSource = filteredList;
        }

        private async void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Personnel personnel)
            {
                string message = personnel.Aktywne
                    ? $"Czy chcesz dezaktywować pracownika {personnel.Imie} {personnel.Nazwisko}?"
                    : $"Czy chcesz reaktywować pracownika {personnel.Imie} {personnel.Nazwisko}?";

                ContentDialog dialog = new ContentDialog
                {
                    Title = "Potwierdzenie",
                    Content = message,
                    PrimaryButtonText = "Tak",
                    CloseButtonText = "Nie",
                    XamlRoot = this.Content.XamlRoot // Set the XamlRoot property
                };

                ContentDialogResult result = await dialog.ShowAsync();
                  
                if (result == ContentDialogResult.Primary)
                {
                    await UpdatePersonnelStatusAsync(personnel.Id, !personnel.Aktywne);
                    personnel.Aktywne = !personnel.Aktywne;
                    LoadPersonnelData(); // Refresh the data
                }
                else
                {
                    checkBox.IsChecked = !checkBox.IsChecked; // Revert the checkbox state
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
