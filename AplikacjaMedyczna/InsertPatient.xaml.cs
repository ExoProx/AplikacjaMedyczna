using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Npgsql;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Animation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AplikacjaMedyczna
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
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
            DataUrodzeniaDatePicker.MaxDate = DateTimeOffset.Now;
            DataUrodzeniaDatePicker.MinDate = DateTimeOffset.Now.AddYears(-120);
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

        private void PeselRodzicaTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                string text = textBox.Text;
                textBox.Text = new string(text.Where(char.IsDigit).ToArray());
                textBox.SelectionStart = textBox.Text.Length; // Set cursor to the end
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
            TypKrwiTextBox.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Gray);
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
            if(TypKrwiTextBox.Text.Length > 6)
            {
                TypKrwiTextBox.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Red);
                allFieldsFilled = false;
            }
            

            if (allFieldsFilled)
            {
                
                string imie = ImieTextBox.Text;
                string nazwisko = NazwiskoTextBox.Text;
                string typKrwi = TypKrwiTextBox.Text;
                string pesel = PeselTextBox.Text;
                string numerKontaktowy = NumerKontaktowyTextBox.Text;
                string adresZamieszkania = AdresZamieszkaniaTextBox.Text;
                string peselRodzica = PeselRodzicaTextBox.Text;

                // Check if patient with the given pesel already exists
                if (await PatientExistsAsync(pesel))
                {
                    await ShowWarningDialog("Pacjent z podanym Peselem już istnieje");
                    return;
                }

                // Check if peselRodzica exists (if set)
                if (!string.IsNullOrWhiteSpace(peselRodzica) && !await PatientExistsAsync(peselRodzica))
                {
                    await ShowWarningDialog("Nie ma osoby z podanym Peselem rodzica");
                    return;
                }

                // Show confirmation dialog
                var result = await ShowConfirmationDialog(imie, nazwisko, dataUrodzenia, typKrwi, pesel, numerKontaktowy, adresZamieszkania, peselRodzica, isUnderage);
                if (result == ContentDialogResult.Primary)
                {
                    // Insert data into the database
                    await InsertPatientAsync(imie, nazwisko, dataUrodzenia, typKrwi, pesel, numerKontaktowy, adresZamieszkania);

                    if (!string.IsNullOrWhiteSpace(PeselRodzicaTextBox.Text) && PeselRodzicaTextBox.Text.Length == 11)
                    {
                        // Insert Pesel Rodzica into SharedPesel table
                        await InsertSharedPeselAsync(pesel, peselRodzica);
                    }

                    // Show success dialog
                    await ShowSuccessDialog();
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

        private async Task ShowWarningDialog(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Ostrzeżenie",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync();
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
                Title = "Potwierdzenie",
                Content = content,
                PrimaryButtonText = "Tak",
                CloseButtonText = "Nie",
                XamlRoot = this.Content.XamlRoot
            };

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
                    command.Parameters.AddWithValue("@peselAllowed",Convert.ToDecimal(peselAllowed));

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        private async Task ShowSuccessDialog()
        {
            var dialog = new ContentDialog
            {
                Title = "Sukces",
                Content = "Nowy pacjent został dodany.",
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }

        private void NavbarToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            NavigationHelper.TogglePane();
        }
    }
}


