using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Npgsql;

namespace AplikacjaMedyczna
{
    public sealed partial class PeselChoice : Page
    {
        public PeselChoice()
        {
            this.InitializeComponent();
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            SearchButton.IsEnabled = CountFilledFields() >= 2;
        }

        private int CountFilledFields()
        {
            int count = 0;
            if (!string.IsNullOrEmpty(NameTextBox.Text)) count++;
            if (!string.IsNullOrEmpty(SurnameTextBox.Text)) count++;
            if (!string.IsNullOrEmpty(AddressTextBox.Text)) count++;
            if (!string.IsNullOrEmpty(TelephoneTextBox.Text)) count++;
            return count;
        }

        private void OnSearchButtonClicked(object sender, RoutedEventArgs e)
        {

            var connectionString = "host = bazamedyczna.cziamyieoagt.eu-north-1.rds.amazonaws.com; " +
                        "username=postgres;" +
                        "Password=adminadmin;" +
                        "Database=medical_database";

            var patients = GetPatientsFromDatabase(connectionString, NameTextBox.Text, SurnameTextBox.Text, AddressTextBox.Text, TelephoneTextBox.Text, PeselTextBox.Text);
            PatientsListBox.ItemsSource = patients;
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

            SearchButton.IsEnabled = text.Length == 11;

            textBox.Text = text;
            textBox.SelectionStart = text.Length;
        }

        private List<Patient> GetPatientsFromDatabase(string connectionString, string name, string surname, string address, string telephone, string pesel)
        {
            var patients = new List<Patient>();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var query = "SELECT imie, nazwisko, \"adresZamieszkania\", \"numerKontaktowy\", pesel FROM public.\"Pacjenci\" WHERE " +
                            "(imie = @name OR @name IS NULL) AND " +
                            "(nazwisko = @surname OR @surname IS NULL) AND " +
                            "(\"adresZamieszkania\" = @address OR @address IS NULL) AND " +
                            "(\"numerKontaktowy\" = @telephone OR @telephone IS NULL) AND " +
                            "(pesel = @pesel OR @pesel IS NULL)";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("name", string.IsNullOrEmpty(name) ? (object)DBNull.Value : name);
                    command.Parameters.AddWithValue("surname", string.IsNullOrEmpty(surname) ? (object)DBNull.Value : surname);
                    command.Parameters.AddWithValue("address", string.IsNullOrEmpty(address) ? (object)DBNull.Value : address);
                    command.Parameters.AddWithValue("telephone", string.IsNullOrEmpty(telephone) ? (object)DBNull.Value : telephone);

                    if (string.IsNullOrEmpty(pesel))
                    {
                        command.Parameters.AddWithValue("pesel", DBNull.Value);
                    }
                    else if (long.TryParse(pesel, out long numericPesel))
                    {
                        command.Parameters.AddWithValue("pesel", numericPesel);
                    }
                    else
                    {
                        throw new ArgumentException("Invalid PESEL format");
                    }

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            patients.Add(new Patient
                            {
                                Name = reader.GetString(0),
                                Surname = reader.GetString(1),
                                Address = reader.GetString(2),
                                Telephone = reader.GetString(3),
                                Pesel = reader.GetInt64(4).ToString()
                            });
                        }
                    }
                }
            }

            return patients;
        }

        private void OnPatientSelected(object sender, SelectionChangedEventArgs e)
        {
            var selectedPatient = (Patient)PatientsListBox.SelectedItem;
            if (selectedPatient != null)
            {
                SharedData.pesel = selectedPatient.Pesel;
                if (SharedData.rola == "Specjalista")
                {
                    App.MainFrame.Navigate(typeof(Wyniki));
                }
                else
                {
                    Frame.Navigate(typeof(PanelGlowny));
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
        public class Patient
        {
            public string Name { get; set; }
            public string Surname { get; set; }
            public string Address { get; set; }
            public string Telephone { get; set; }
            public string Pesel { get; set; }
        }
    }
}