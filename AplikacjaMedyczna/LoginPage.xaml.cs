using System.Text.RegularExpressions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Npgsql;
using Windows.System;

namespace AplikacjaMedyczna
{
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
        }

        private void MoveToStaffLogin(object sender, RoutedEventArgs e)
        {
            App.MainFrame.Navigate(typeof(StaffLoginPage));
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            int stan = logowanie(Pesel.Text, Password.Password);

            if (stan == 1)
            {
                SharedData.pesel = Pesel.Text;
                SharedData.PrimaryPesel = Pesel.Text;
                App.MainFrame.Navigate(typeof(PanelGlowny));
            }
            else if (stan == 2 || stan == 0)
            {
                ErrorMessage.Text = "Nieprawid�owy PESEL lub has�o";
                ErrorMessage.Visibility = Visibility.Visible;
            }
            else if (stan == 3)
            {
                ErrorMessage.Text = "B��d po��czenia z baz� danych";
                ErrorMessage.Visibility = Visibility.Visible;
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
            if (pesel.Length != 11)
            {
                return 0;
            }

            if (!decimal.TryParse(pesel, out decimal peselNumeric))
            {
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
                    string sql = "SELECT pesel, haslo FROM public.\"Pacjenci\" WHERE pesel = @pesel";

                    using (var cmd = new NpgsqlCommand(sql, con))
                    {
                        cmd.Parameters.AddWithValue("@pesel", peselNumeric);

                        using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                        {
                            if (rdr.Read())
                            {
                                string hasloBaza = rdr.GetString(1);
                                if (peselNumeric == rdr.GetDecimal(0))
                                {
                                    if (haslo == hasloBaza)
                                    {
                                        return 1;
                                    }
                                    else return 2;
                                }
                                else
                                {
                                    return 0;
                                }
                            }
                            else
                            {
                                return 3;
                            }
                        }
                    }
                }
            }
            catch (NpgsqlException)
            {
                return 3;
            }
        }
    }
}