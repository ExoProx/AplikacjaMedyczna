using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Npgsql;

namespace AplikacjaMedyczna
{
    public sealed partial class InsertPatient : Page
    {
        private string connectionString = "host=bazamedyczna.cziamyieoagt.eu-north-1.rds.amazonaws.com;" +
                                          "username=administrator;" +
                                          "Password=haslo;" +
                                          "Database=medical_database";

        public InsertPatient()
        {
            this.InitializeComponent();
            NavigationHelper.SplitViewInstance = splitView;
            splitView.IsPaneOpen = true;
            DataUrodzeniaDatePicker.MaxDate = DateTimeOffset.Now;
            DataUrodzeniaDatePicker.MinDate = DateTimeOffset.Now.AddYears(-120);

            List<string> typyKrwi = new List<string>
            {
                "Wybierz typ krwi",
                "0 Rh+",
                "0 Rh-",
                "A Rh+",
                "A Rh-",
                "B Rh+",
                "B Rh-",
                "AB Rh+",
                "AB Rh-",
            };
            TypKrwiComboBox.ItemsSource = typyKrwi;
            TypKrwiComboBox.SelectedIndex = 0;
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

        private void PeselTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                string text = textBox.Text;
                textBox.Text = new string(text.Where(char.IsDigit).ToArray());
                textBox.SelectionStart = textBox.Text.Length;
            }
        }

        private void NumerKontaktowyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                string text = textBox.Text;
                textBox.Text = new string(text.Where(char.IsDigit).ToArray());
                textBox.SelectionStart = textBox.Text.Length;
            }
        }

        private void PeselRodzicaTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                string text = textBox.Text;
                textBox.Text = new string(text.Where(char.IsDigit).ToArray());
                textBox.SelectionStart = textBox.Text.Length;
            }
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            bool allFieldsFilled = true;
            DateTime dataUrodzenia = DateTime.Today;
            bool isUnderage = false;
            DataUrodzeniaDatePicker.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Gray);
            ImieTextBox.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Gray);
            NazwiskoTextBox.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Gray);
            PeselRodzicaTextBox.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Gray);
            PeselTextBox.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Gray);
            AdresZamieszkaniaTextBox.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Gray);
            TypKrwiComboBox.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Gray);
            NumerKontaktowyTextBox.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Gray);
            if (string.IsNullOrWhiteSpace(ImieTextBox.Text))
            {
                ImieTextBox.PlaceholderText = "Pole musi być wypełnione";
                ImieTextBox.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Red);
                allFieldsFilled = false;
            }

            if (string.IsNullOrWhiteSpace(NazwiskoTextBox.Text))
            {
                NazwiskoTextBox.PlaceholderText = "Pole musi być wypełnione";
                NazwiskoTextBox.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Red);
                allFieldsFilled = false;
            }

            if (!DataUrodzeniaDatePicker.Date.HasValue)
            {
                DataUrodzeniaDatePicker.PlaceholderText = "Pole musi być wypełnione";
                DataUrodzeniaDatePicker.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Red);
                allFieldsFilled = false;
            }
            else
            {
                dataUrodzenia = DataUrodzeniaDatePicker.Date.Value.DateTime;
                isUnderage = (DateTime.Now - dataUrodzenia).TotalDays < 18 * 365;

                if (isUnderage && (string.IsNullOrWhiteSpace(PeselRodzicaTextBox.Text) || PeselRodzicaTextBox.Text.Length != 11))
                {
                    PeselRodzicaTextBox.Text = string.Empty;
                    PeselRodzicaTextBox.PlaceholderText = "Pole musi być wypełnione (11 cyfr)";
                    PeselRodzicaTextBox.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Red);
                    return;
                }
            }
            if (string.IsNullOrWhiteSpace(PeselTextBox.Text) || PeselTextBox.Text.Length != 11)
            {
                PeselTextBox.Text = string.Empty;
                PeselTextBox.PlaceholderText = "Pole musi być wypełnione (11 cyfr)";
                PeselTextBox.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Red);
                allFieldsFilled = false;
            }

            if (string.IsNullOrWhiteSpace(NumerKontaktowyTextBox.Text) || NumerKontaktowyTextBox.Text.Length != 9)
            {
                NumerKontaktowyTextBox.Text = string.Empty;
                NumerKontaktowyTextBox.PlaceholderText = "Pole musi być wypełnione (9 cyfr)";
                NumerKontaktowyTextBox.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Red);
                allFieldsFilled = false;
            }

            if (string.IsNullOrWhiteSpace(AdresZamieszkaniaTextBox.Text))
            {
                AdresZamieszkaniaTextBox.PlaceholderText = "Pole musi być wypełnione";
                AdresZamieszkaniaTextBox.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Red);
                allFieldsFilled = false;
            }
            if (TypKrwiComboBox.SelectedIndex == 0)
            {
                TypKrwiComboBox.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Red);
                allFieldsFilled = false;
            }


            if (allFieldsFilled)
            {

                string imie = ImieTextBox.Text;
                string nazwisko = NazwiskoTextBox.Text;
                string typKrwi = TypKrwiComboBox.SelectedItem.ToString();
                string pesel = PeselTextBox.Text;
                string numerKontaktowy = NumerKontaktowyTextBox.Text;
                string adresZamieszkania = AdresZamieszkaniaTextBox.Text;
                string peselRodzica = PeselRodzicaTextBox.Text;

                if (await PatientExistsAsync(pesel))
                {
                    await ShowMessageDialog("Ostrzeżenie", "Pacjent z podanym Peselem już istnieje");
                    return;
                }

                if (!string.IsNullOrWhiteSpace(peselRodzica) && !await PatientExistsAsync(peselRodzica))
                {
                    await ShowMessageDialog("Ostrzeżenie", "Nie ma osoby z podanym Peselem rodzica");
                    return;
                }

                var result = await ShowConfirmationDialog(imie, nazwisko, dataUrodzenia, typKrwi, pesel, numerKontaktowy, adresZamieszkania, peselRodzica, isUnderage);
                if (result == ContentDialogResult.Primary)
                {
                    await InsertPatientAsync(imie, nazwisko, dataUrodzenia, typKrwi, pesel, numerKontaktowy, adresZamieszkania);

                    if (!string.IsNullOrWhiteSpace(PeselRodzicaTextBox.Text) && PeselRodzicaTextBox.Text.Length == 11)
                    {
                        await InsertSharedPeselAsync(pesel, peselRodzica);
                    }

                    await ShowMessageDialog("Sukces", "Nowy pacjent został dodany.");
                    App.MainFrame.Navigate(typeof(Admin_Panel), null, new DrillInNavigationTransitionInfo());
                }
            }
        }

        private async Task<bool> PatientExistsAsync(string pesel)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var query = @"SELECT COUNT(*) FROM public.""Pacjenci"" WHERE ""pesel"" = @pesel;";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@pesel", Convert.ToDecimal(pesel));
                    var count = (long)await command.ExecuteScalarAsync();
                    return count > 0;
                }
            }
        }

        private async Task<ContentDialogResult> ShowConfirmationDialog(string imie, string nazwisko, DateTime dataUrodzenia, string typKrwi, string pesel, string numerKontaktowy, string adresZamieszkania, string peselRodzica, bool isUnderage)
        {
            string content = $"Czy na pewno chcesz dodać nowego pacjenta?\n\n" +
                             $"Imię: {imie}\n" +
                             $"Nazwisko: {nazwisko}\n" +
                             $"Data Urodzenia: {dataUrodzenia.ToShortDateString()}\n" +
                             $"Typ Krwi: {typKrwi}\n" +
                             $"Pesel: {pesel}\n" +
                             $"Numer Kontaktowy: {numerKontaktowy}\n" +
                             $"Adres Zamieszkania: {adresZamieszkania}";

            if (isUnderage)
            {
                content += $"\nPesel Rodzica: {peselRodzica}";
            }

            var dialog = new ContentDialog
            {
                Content = content,
                PrimaryButtonText = "Tak",
                CloseButtonText = "Nie",
                XamlRoot = this.Content.XamlRoot,
                PrimaryButtonStyle = (Style)Application.Current.Resources["PrimaryButtonStyle"],
                CloseButtonStyle = (Style)Application.Current.Resources["CloseButtonStyle"],
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 240, 248, 255)),
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 74, 173))
            };

            var titleContainer = new Border
            {
                Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 74, 173)),
                Padding = new Thickness(10),
                Child = new TextBlock
                {
                    Text = "Potwierdzenie",
                    TextAlignment = TextAlignment.Center,
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    Width = 300
                }
            };

            dialog.Title = titleContainer;
            return await dialog.ShowAsync();
        }

        private async Task InsertPatientAsync(string imie, string nazwisko, DateTime dataUrodzenia, string typKrwi, string pesel, string numerKontaktowy, string adresZamieszkania)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var query = @"
                    INSERT INTO public.""Pacjenci"" 
                    (""pesel"", ""imie"", ""nazwisko"", ""haslo"", ""adresZamieszkania"", ""numerKontaktowy"", ""dataUrodzenia"", ""typkrwi"") 
                    VALUES 
                    (@pesel, @imie, @nazwisko, 'haslo', @adresZamieszkania, @numerKontaktowy, @dataUrodzenia, @typKrwi);";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@pesel", Convert.ToDecimal(pesel));
                    command.Parameters.AddWithValue("@imie", imie);
                    command.Parameters.AddWithValue("@nazwisko", nazwisko);
                    command.Parameters.AddWithValue("@adresZamieszkania", adresZamieszkania);
                    command.Parameters.AddWithValue("@numerKontaktowy", numerKontaktowy);
                    command.Parameters.AddWithValue("@dataUrodzenia", dataUrodzenia);
                    command.Parameters.AddWithValue("@typKrwi", typKrwi);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task InsertSharedPeselAsync(string peselOwner, string peselAllowed)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var query = @"
                    INSERT INTO public.""SharedPesel"" 
                    (""peselOwner"", ""peselAllowed"") 
                    VALUES 
                    (@peselOwner, @peselAllowed);";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@peselOwner", Convert.ToDecimal(peselOwner));
                    command.Parameters.AddWithValue("@peselAllowed", Convert.ToDecimal(peselAllowed));

                    await command.ExecuteNonQueryAsync();
                }
            }
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


