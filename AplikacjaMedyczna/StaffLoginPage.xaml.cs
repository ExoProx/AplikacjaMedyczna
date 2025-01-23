using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
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
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

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
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            int stan = logowanie(Pesel.Text, Password.Password);

            if (stan == 1)
            {
                SharedData.id = Pesel.Text;
                if(SharedData.rola == "Administrator")
                {
                    App.MainFrame.Navigate(typeof(Admin_Panel));
                }
                else
                {
                    App.MainFrame.Navigate(typeof(PanelGlowny));
                }

            }
            else if (stan == 2)
            {
                ErrorPESEL.Visibility = Visibility.Collapsed;
                ErrorPassword.Visibility = Visibility.Visible;
                ErrorDatabase.Visibility = Visibility.Collapsed;
            }
            else if (stan == 0)
            {
                ErrorPESEL.Visibility = Visibility.Visible;
                ErrorPassword.Visibility = Visibility.Collapsed;
                ErrorDatabase.Visibility = Visibility.Collapsed;
            }
            else if (stan == 3)
            {
                ErrorPESEL.Visibility = Visibility.Collapsed;
                ErrorPassword.Visibility = Visibility.Collapsed;
                ErrorDatabase.Visibility = Visibility.Visible;
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

            // Delates all non-numeric characters
            text = Regex.Replace(text, "[^0-9]", "");

            // Limiting the length of the PESEL number to 11 characters
            if (text.Length > 11)
            {
                text = text.Substring(0, 11);
            }

            // Update the TextBox text
            textBox.Text = text;

            // Set the cursor position to the end of the text
            textBox.SelectionStart = text.Length;
        }
        int logowanie(string pesel, string haslo)
        {
            decimal peselNumeric;

            // Convert PESEL to numeric (decimal)
            if (!decimal.TryParse(pesel, out peselNumeric))
            {
                // Invalid PESEL format
                return 0;
            }

            var cs = "host=bazamedyczna.cziamyieoagt.eu-north-1.rds.amazonaws.com;" +
                     "username=postgres;" +
                     "Password=adminadmin;" +
                     "Database=medical_database";

            using (var con = new NpgsqlConnection(cs))
            {
                con.Open();

                // SQL query with JOIN
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
                pm.""id"" = @pesel AND pm.""haslo"" = @haslo";

                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@pesel", peselNumeric);
                    cmd.Parameters.AddWithValue("@haslo", haslo);

                    using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            string hasloBaza = rdr.GetString(rdr.GetOrdinal("haslo"));
                            if (peselNumeric == rdr.GetDecimal(rdr.GetOrdinal("id")))
                            {
                                if (haslo == hasloBaza)
                                {
                                    // Save the value of "rola" column in SharedData.rola
                                    SharedData.rola = rdr.GetString(rdr.GetOrdinal("rola"));
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
                            return 3; // Invalid PESEL
                        }
                    }
                }
            }
        }
    }
}
