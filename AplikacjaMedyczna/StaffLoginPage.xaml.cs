using System;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Npgsql;
using Windows.System;

namespace AplikacjaMedyczna
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StaffLoginPage : Page
    {
        public StaffLoginPage()
        {
            this.InitializeComponent();
        }

        private void MoveToLogin(object sender, RoutedEventArgs e)
        {
            App.MainFrame.Navigate(typeof(LoginPage));
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            int stan = logowanie(Pesel.Text, Password.Password);

            if (stan == 1)
            {
                SharedData.id = Pesel.Text;
                NavigateBasedOnRole();
            }
            else if (stan == 3 || stan == 0)
            {
                ErrorMessage.Text = "Nieprawidłowy ID Pracownika lub hasło";
                ErrorMessage.Visibility = Visibility.Visible;
            }
            else if (stan == 2)
            {
                ErrorMessage.Text = "Błąd połączenia z bazą o odanych";
                ErrorMessage.Visibility = Visibility.Visible;
            }
            else if (stan == 4)
            {
                ErrorMessage.Text = "Konto Nieaktywne";
                ErrorMessage.Visibility = Visibility.Visible;
            }
            else if (stan == 5)
            {
                var dialog = new ChangePasswordDialog(Pesel.Text);
                dialog.XamlRoot = this.XamlRoot;
                await dialog.ShowAsync();
                if (dialog.IsPasswordChanged)
                {
                    SharedData.id = Pesel.Text;
                    NavigateBasedOnRole();
                }
            }
        }
        private void NavigateBasedOnRole()
        {
            if (SharedData.rola == "Administrator")
            {
                App.MainFrame.Navigate(typeof(Admin_Panel));
            }
            else
            {
                App.MainFrame.Navigate(typeof(PeselChoice));
            }
        }
        private void Input_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                LoginButton_Click(sender, e);
            }
        }

        private void Pesel_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            string text = textBox.Text;

            text = Regex.Replace(text, "[^0-9]", "");

            if (text.Length > 11)
            {
                text = text.Substring(0, 11);
            }

            textBox.Text = text;
            textBox.SelectionStart = text.Length;
        }

        private int logowanie(string pesel, string haslo)
        {
            decimal peselNumeric;

            // Convert PESEL to numeric (decimal)
            if (!decimal.TryParse(pesel, out peselNumeric))
            {
                // Invalid PESEL format
                return 0;
            }

            var cs = "host=bazamedyczna.cziamyieoagt.eu-north-1.rds.amazonaws.com;" +
                     "username=pacjent;" +
                     "Password=pacjent;" +
                     "Database=medical_database";

            try
            {
                using (var con = new NpgsqlConnection(cs))
                {
                    con.Open();

                    // SQL query with JOIN and password decryption
                    string sql = @"
                    SELECT 
                        pm.*, rp.nazwa AS rola
                    FROM 
                        public.""PersonelMedyczny"" AS pm
                    JOIN 
                        public.""RolePersonelu"" AS rp 
                    ON 
                        pm.""idRoli"" = rp.id
                    WHERE 
                        pm.""id"" = @pesel AND pm.""haslo"" = crypt(@haslo, pm.""haslo"")";

                    using (var cmd = new NpgsqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@pesel", peselNumeric);
                        cmd.Parameters.AddWithValue("@haslo", haslo);

                        using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                        {
                            if (rdr.Read())
                            {
                                if (rdr.GetBoolean(rdr.GetOrdinal("aktywne")) == false)
                                {
                                    return 4; // Account is inactive
                                }
                                SharedData.rola = rdr.GetString(rdr.GetOrdinal("rola"));
                                if (rdr.GetBoolean(rdr.GetOrdinal("pierwszehaslo")))
                                {
                                    return 5; // First login detected
                                }
                                return 1;
                            }
                            else
                            {
                                return 3; // Invalid PESEL or password
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                return 2; // Database connection error
            }
        }
    
    }
 }
    

