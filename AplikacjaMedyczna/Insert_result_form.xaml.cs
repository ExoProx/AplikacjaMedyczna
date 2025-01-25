using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Npgsql;

namespace AplikacjaMedyczna
{
    public sealed partial class Insert_result_form : Page
    {
        public Insert_result_form()
        {
            this.InitializeComponent();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.MainFrame.GoBack();
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            int status = SaveEntry(ResultTextBox.Text, TerminReceptyDatePicker.Date.DateTime);

            if (status == 1)
            {
                SuccessMessage.Visibility = Visibility.Visible;
                EmptyFieldMessage.Visibility = Visibility.Collapsed;
                ErrorDatabaseMessage.Visibility = Visibility.Collapsed;
                App.MainFrame.GoBack();
            }
            else if (status == 0)
            {
                SuccessMessage.Visibility = Visibility.Collapsed;
                EmptyFieldMessage.Visibility = Visibility.Visible;
                ErrorDatabaseMessage.Visibility = Visibility.Collapsed;
            }
            else if (status == 3)
            {
                SuccessMessage.Visibility = Visibility.Collapsed;
                EmptyFieldMessage.Visibility = Visibility.Collapsed;
                ErrorDatabaseMessage.Visibility = Visibility.Visible;
            }
        }

        private int SaveEntry(string wpis, DateTime examinationDate)
        {
            if (string.IsNullOrWhiteSpace(wpis))
            {
                return 0;
            }

            var connectionString = "host=bazamedyczna.cziamyieoagt.eu-north-1.rds.amazonaws.com;" +
                                   "username=lekarz;" +
                                   "Password=lekarz;" +
                                   "Database=medical_database";

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "INSERT INTO \"WynikibadanDiagnostycznych\" (\"peselPacjenta\", \"idPersonelu\", \"wynikiBadania\", \"dataWyniku\") VALUES (@1, @2, @3, @4);";

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@1", long.Parse(SharedData.pesel));
                        command.Parameters.AddWithValue("@2", long.Parse(SharedData.id));
                        command.Parameters.AddWithValue("@3", wpis);
                        command.Parameters.AddWithValue("@4", examinationDate);
                        command.ExecuteNonQuery();
                    }
                }

                return 1;
            }
            catch (FormatException)
            {
                System.Diagnostics.Debug.WriteLine("Invalid pesel or id format.");
                return 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database error: {ex.Message}");
                return 3;
            }
        }
    }
}