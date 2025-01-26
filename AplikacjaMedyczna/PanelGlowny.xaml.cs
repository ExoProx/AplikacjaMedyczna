using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Npgsql;

namespace AplikacjaMedyczna
{
    public sealed partial class PanelGlowny : Page
    {
        private string connectionString = "host=bazamedyczna.cziamyieoagt.eu-north-1.rds.amazonaws.com;" +
                "username=pacjent;" +
                "Password=pacjent;" +
                "Database=medical_database";
        public PanelGlowny()
        {
            this.InitializeComponent();
            NavigationHelper.SplitViewInstance = splitView;
            CheckUserId();
            _ = InitializeAsync();
        }
        private async Task InitializeAsync()
        {
            await LoadPatientInfoAsync(SharedData.pesel);
            await LoadAllergiesAsync(SharedData.pesel);
            await LoadMostRecentWpisAsync(SharedData.pesel);
            await LoadMostRecentSkierowaniaAsync(SharedData.pesel);
            await LoadMostRecentReceptyAsync(SharedData.pesel);
            await LoadMostRecentWynikiAsync(SharedData.pesel);
            await LoadObecnyPacjentComboBoxAsync(SharedData.PrimaryPesel);
        }

        private async Task LoadObecnyPacjentComboBoxAsync(string primaryPesel)
        {
            var peselList = new List<string> { primaryPesel };

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    System.Diagnostics.Debug.WriteLine("Database connection opened.");

                    var query = @"
                        SELECT 
                            ""peselOwner""
                        FROM 
                            public.""SharedPesel""
                        WHERE 
                            ""peselAllowed"" = @primaryPesel;";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@primaryPesel", Convert.ToDecimal(primaryPesel));
                        System.Diagnostics.Debug.WriteLine("Query prepared.");

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            System.Diagnostics.Debug.WriteLine("Executing query.");
                            while (await reader.ReadAsync())
                            {
                                peselList.Add(reader["peselOwner"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Npgsql.PostgresException ex)
            {
                System.Diagnostics.Debug.WriteLine($"PostgresException: {ex.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
            }

            ObecnyPacjentComboBox.SelectionChanged -= ObecnyPacjentComboBox_SelectionChanged;
            ObecnyPacjentComboBox.ItemsSource = peselList;
            ObecnyPacjentComboBox.SelectedItem = SharedData.pesel;
            ObecnyPacjentComboBox.SelectionChanged += ObecnyPacjentComboBox_SelectionChanged;
        }

        private void ObecnyPacjentComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ObecnyPacjentComboBox.SelectedItem != null)
            {
                SharedData.pesel = ObecnyPacjentComboBox.SelectedItem.ToString();
                _ = InitializeAsync();
            }
        }

        private void CheckUserId()
        {
            if (string.IsNullOrEmpty(SharedData.id))
            {
                ObecnyPacjentPanel.Visibility = Visibility.Visible;
            }
            else
            if (!string.IsNullOrEmpty(SharedData.id))
            {
                PatientChoiceButton.Visibility = Visibility.Visible;
                PatientChoiceButton.IsEnabled = true;
            }
        }

        private async Task LoadPatientInfoAsync(string pesel)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var query = @"
                    SELECT 
                    ""pesel"",
                    ""imie"",
                    ""nazwisko"",
                    ""adresZamieszkania"",
                    ""numerKontaktowy"",
                    ""dataUrodzenia"",
	                ""typkrwi""
                FROM 
                    public.""Pacjenci""
                WHERE 
                    ""pesel"" = @pesel;";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@pesel", Convert.ToDecimal(pesel));

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            PeselTextBox.Text = reader["pesel"].ToString();
                            ImieTextBox.Text = reader["imie"].ToString();
                            NazwiskoTextBox.Text = reader["nazwisko"].ToString();
                            AdresZamieszkaniaTextBox.Text = reader["adresZamieszkania"].ToString();
                            NumerKontaktowyTextBox.Text = reader["numerKontaktowy"].ToString();
                            DataUrodzeniaTextBox.Text = Convert.ToDateTime(reader["dataUrodzenia"]).ToString("dd.MM.yyyy");
                            TypKrwiTextBox.Text = reader["typKrwi"].ToString();
                        }
                    }
                }
            }
        }
        private async Task LoadAllergiesAsync(string pesel)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    System.Diagnostics.Debug.WriteLine("Database connection opened.");

                    var query = @"
                SELECT 
                    a.""nazwa"" AS ""Alergia""
                FROM 
                    public.""SpisAlergii"" sa
                JOIN 
                    public.""Alergeny"" a
                ON 
                    sa.""idAlergenu"" = a.""id""
                WHERE 
                    sa.""peselPacjenta"" = @pesel;";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@pesel", Convert.ToDecimal(pesel));
                        System.Diagnostics.Debug.WriteLine("Query prepared.");

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            System.Diagnostics.Debug.WriteLine("Executing query.");
                            var allergies = new List<Allergy>();

                            while (await reader.ReadAsync())
                            {
                                allergies.Add(new Allergy
                                {
                                    Alergia = reader["Alergia"].ToString()
                                });
                            }

                            if (allergies.Count == 0)
                            {
                                allergies.Add(new Allergy
                                {
                                    Alergia = "Brak znanych Alergi"
                                });
                            }

                            System.Diagnostics.Debug.WriteLine($"Fetched {allergies.Count} allergies.");
                            AllergiesListView.ItemsSource = allergies;
                        }
                    }
                }
            }
            catch (Npgsql.PostgresException ex)
            {
                System.Diagnostics.Debug.WriteLine($"PostgresException: {ex.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
            }
        }


        private async Task LoadMostRecentWpisAsync(string pesel)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    System.Diagnostics.Debug.WriteLine("Database connection opened.");

                    var query = @"
                SELECT 
                    Wpisy.""id"" AS wpisy_id, 
                    Wpisy.""dataWpisu"" AS wpisy_data,  
                    personel.""imie"" AS personel_imie, 
                    personel.""nazwisko"" AS personel_nazwisko,
                    Wpisy.""wpis"" AS wpisy_tresc
                FROM 
                    public.""WpisyMedyczne"" AS Wpisy
                JOIN 
                    public.""PersonelMedyczny"" AS personel
                ON 
                    Wpisy.""idPersonelu"" = personel.""id"" 
                WHERE 
                    Wpisy.""peselPacjenta"" = @pesel
                ORDER BY 
                    Wpisy.""dataWpisu"" DESC 
                LIMIT 1;";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@pesel", Convert.ToDecimal(pesel));
                        System.Diagnostics.Debug.WriteLine("Query prepared.");

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            System.Diagnostics.Debug.WriteLine("Executing query.");
                            if (await reader.ReadAsync())
                            {
                                var wpisyData = Convert.ToDateTime(reader["wpisy_data"]).ToString("dd.MM.yyyy");
                                var personelImie = reader["personel_imie"].ToString();
                                var personelNazwisko = reader["personel_nazwisko"].ToString();
                                var wpisyTresc = reader["wpisy_tresc"].ToString();

                                WpisyTextBlock.Text = $"Data Wpisu: {wpisyData}\nLekarz: {personelImie} {personelNazwisko}\nWpis:\n{wpisyTresc}";
                            }
                            else
                            {
                                WpisyTextBlock.Text = "Brak wpisów";
                            }
                        }
                    }
                }
            }
            catch (Npgsql.PostgresException ex)
            {
                System.Diagnostics.Debug.WriteLine($"PostgresException: {ex.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
            }
        }

        private async Task LoadMostRecentSkierowaniaAsync(string pesel)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    System.Diagnostics.Debug.WriteLine("Database connection opened.");

                    var query = @"
                SELECT 
                    ""id"", 
                    ""dataSkierowania"", 
                    ""skierowanie""
                FROM 
                    public.""Skierowania""
                WHERE 
                    ""peselPacjenta"" = @pesel
                ORDER BY 
                    ""dataSkierowania"" DESC
                LIMIT 1;";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@pesel", Convert.ToDecimal(pesel));
                        System.Diagnostics.Debug.WriteLine("Query prepared.");

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            System.Diagnostics.Debug.WriteLine("Executing query.");
                            if (await reader.ReadAsync())
                            {
                                var skierowanieData = Convert.ToDateTime(reader["dataSkierowania"]).ToString("dd.MM.yyyy");
                                var skierowanieText = reader["skierowanie"].ToString();

                                SkierowaniaTextBlock.Text = $"Data Skierowania: {skierowanieData}\nSkierowanie: {skierowanieText}";
                            }
                            else
                            {
                                SkierowaniaTextBlock.Text = "Brak skierowań";
                            }
                        }
                    }
                }
            }
            catch (Npgsql.PostgresException ex)
            {
                System.Diagnostics.Debug.WriteLine($"PostgresException: {ex.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
            }
        }

        private async Task LoadMostRecentReceptyAsync(string pesel)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    System.Diagnostics.Debug.WriteLine("Database connection opened.");

                    var query = @"
                SELECT 
                    ""Recepty"".""id"" AS recepty_id, 
                    ""Recepty"".""dataWystawienia"" AS recepty_data_wystawienia,  
                    ""Recepty"".""dataWaznosci"" AS recepty_data_waznosci,  
                    ""Recepty"".""przypisaneLeki"" AS recepty_tresc,  
                    ""PersonelMedyczny"".""imie"" AS personel_imie, 
                    ""PersonelMedyczny"".""nazwisko"" AS personel_nazwisko
                FROM 
                    public.""Recepty""
                JOIN 
                    public.""PersonelMedyczny""
                ON 
                    ""Recepty"".""idPersonelu"" = ""PersonelMedyczny"".""id""
                WHERE 
                    ""Recepty"".""peselPacjenta"" = @pesel
                ORDER BY 
                    ""dataWystawienia"" DESC 
                LIMIT 1;";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@pesel", Convert.ToDecimal(pesel));
                        System.Diagnostics.Debug.WriteLine("Query prepared.");

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            System.Diagnostics.Debug.WriteLine("Executing query.");
                            if (await reader.ReadAsync())
                            {
                                var receptyDataWystawienia = Convert.ToDateTime(reader["recepty_data_wystawienia"]).ToString("dd.MM.yyyy");
                                var receptyDataWaznosci = Convert.ToDateTime(reader["recepty_data_waznosci"]).ToString("dd.MM.yyyy");
                                var receptyTresc = reader["recepty_tresc"].ToString();
                                var personelImie = reader["personel_imie"].ToString();
                                var personelNazwisko = reader["personel_nazwisko"].ToString();

                                ReceptyTextBlock.Text = $"Data Wystawienia: {receptyDataWystawienia}\nData Ważności: {receptyDataWaznosci}\nRecepta:\n{receptyTresc}\nLekarz: {personelImie} {personelNazwisko}";
                            }
                            else
                            {
                                ReceptyTextBlock.Text = "Brak recept";
                            }
                        }
                    }
                }
            }
            catch (Npgsql.PostgresException ex)
            {
                System.Diagnostics.Debug.WriteLine($"PostgresException: {ex.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
            }
        }

        private async Task LoadMostRecentWynikiAsync(string pesel)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    System.Diagnostics.Debug.WriteLine("Database connection opened.");

                    var query = @"
                SELECT 
                    ""id"", 
                    ""dataWyniku"", 
                    ""wynikiBadania""
                FROM 
                    public.""WynikibadanDiagnostycznych""
                WHERE 
                    ""peselPacjenta"" = @pesel
                ORDER BY 
                    ""dataWyniku"" DESC
                LIMIT 1;";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@pesel", Convert.ToDecimal(pesel));
                        System.Diagnostics.Debug.WriteLine("Query prepared.");

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            System.Diagnostics.Debug.WriteLine("Executing query.");
                            if (await reader.ReadAsync())
                            {
                                var dataWyniku = Convert.ToDateTime(reader["dataWyniku"]).ToString("dd.MM.yyyy");
                                var wynikiBadania = reader["wynikiBadania"].ToString();

                                WynikiTextBlock.Text = $"Data Wyniku: {dataWyniku}\nWyniki:\n{wynikiBadania}";
                            }
                            else
                            {
                                WynikiTextBlock.Text = "Brak wyników";
                            }
                        }
                    }
                }
            }
            catch (Npgsql.PostgresException ex)
            {
                System.Diagnostics.Debug.WriteLine($"PostgresException: {ex.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
            }
        }

        private class Skierowanie
        {
            public string Skierowania { get; set; }
        }

        private class Wpis
        {
            public string Wpisy { get; set; }
        }

        private class Allergy
        {
            public string Alergia { get; set; }
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

        private void NavbarToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            NavigationHelper.TogglePane();
        }
    }
}