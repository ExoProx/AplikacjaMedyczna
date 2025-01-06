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
    public sealed partial class Insert_referral_Form : Page
    {
        public Insert_referral_Form()
        {
            this.InitializeComponent();
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.MainFrame.GoBack();
        }
        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            int status = SaveEntry(RefferalTextBox.Text);
            if (status == 1)
            {
                // Success: Entry saved
                SuccessMessage.Visibility = Visibility.Visible;
                EmptyFieldMessage.Visibility = Visibility.Collapsed;
                ErrorDatabaseMessage.Visibility = Visibility.Collapsed;
                App.MainFrame.GoBack();
            }
            else if (status == 0)
            {
                // Error: Field is empty
                SuccessMessage.Visibility = Visibility.Collapsed;
                EmptyFieldMessage.Visibility = Visibility.Visible;
                ErrorDatabaseMessage.Visibility = Visibility.Collapsed;
            }
            else if (status == 3)
            {
                // Error: Database issue
                SuccessMessage.Visibility = Visibility.Collapsed;
                EmptyFieldMessage.Visibility = Visibility.Collapsed;
                ErrorDatabaseMessage.Visibility = Visibility.Visible;
            }
        }

        private int SaveEntry(string wpis)
        {
            if (string.IsNullOrWhiteSpace(wpis))
            {
                // Entry field or date field is empty
                return 0;
            }

            var connectionString = "host=localhost;username=lekarz;Password=haslo;Database=BazaMedyczna";

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "INSERT INTO \"Skierowania\" (\"skierowanie\", \"dataSkierowania\", \"peselPacjenta\","+ 
                                "\"idPersonelu\")  VALUES (@1, CURRENT_DATE, @2, @3)"; ;
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@1", wpis);
                        command.Parameters.AddWithValue("@2", long.Parse(SharedData.pesel));
                        command.Parameters.AddWithValue("@3", long.Parse(SharedData.id));
                        command.ExecuteNonQuery();
                    }
                }

                // Successfully saved the entry
                return 1;
            }
            catch (FormatException)
            {
                // Handle the case where wpis or date is not a valid long
                System.Diagnostics.Debug.WriteLine("Invalid pesel or id format.");
                return 0;
            }
            catch (Exception ex)
            {
                // Log exception if needed
                System.Diagnostics.Debug.WriteLine($"Database error: {ex.Message}");
                return 3; // Database error
            }
        }
    }
}
