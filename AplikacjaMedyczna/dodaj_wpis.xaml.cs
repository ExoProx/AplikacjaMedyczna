using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using System.Data.SqlClient;
using AplikacjaMedyczna;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Npgsql;

using static System.Runtime.InteropServices.JavaScript.JSType;


namespace AplikacjaMedyczna
{
    /// <summary>
    /// A page to add an entry into the database.
    /// </summary>
    public sealed partial class dodaj_wpis : Page
    {
        public dodaj_wpis()
        {
            this.InitializeComponent();
            Window window = App.MainWindow;
            window.ExtendsContentIntoTitleBar = true;

        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainFrame.CanGoBack)
            {
                App.MainFrame.GoBack();
            }
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            int status = SaveEntry(WpisTextBox.Text);

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

        private void Input_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                SubmitButton_Click(sender, e);
            }
        }

        private int SaveEntry(string wpis)
        {
            if (string.IsNullOrWhiteSpace(wpis))
            {
                // Entry field is empty
                return 0;
            }

            var connectionString = "host=localhost;username=lekarz;Password=haslo;Database=BazaMedyczna";

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "INSERT INTO public.\"WpisyMedyczne\" (id, \"peselPacjenta\", \"idPersonelu\", wpis, \"dataWpisu\")  VALUES (default, @pesel, @id, @wpis, CURRENT_DATE);";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@wpis", wpis);
                        command.Parameters.AddWithValue("@pesel", long.Parse(SharedData.pesel));
                        command.Parameters.AddWithValue("@id", long.Parse(SharedData.id));
                        command.ExecuteNonQuery();
                    }
                }

                // Successfully saved the entry
                return 1;
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
