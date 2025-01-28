using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Npgsql;
using Windows.Storage.Pickers;

namespace AplikacjaMedyczna
{
    public sealed partial class Insert_result_form : Page
    {
        public string filename = "";
        private Windows.Storage.StorageFile selectedFile = null;

        public Insert_result_form()
        {
            this.InitializeComponent();
            SetCalendarRestrictions();
        }

        private void SetCalendarRestrictions()
        {
            var today = DateTime.Today;
            TerminReceptyCalendarView.MinDate = today.AddYears(-50);
            TerminReceptyCalendarView.MaxDate = today;
        }

        private async void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog cancelDialog = new ContentDialog
            {
                Title = "Potwierdzenie anulowania",
                Content = "Czy na pewno chcesz anulować dodawanie wpisu?",
                PrimaryButtonText = "Tak",
                CloseButtonText = "Nie",
                XamlRoot = this.XamlRoot
            };

            var result = await cancelDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                App.MainFrame.GoBack();
            }
        }

        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedDate = TerminReceptyCalendarView.Date;
            if (selectedDate == null || selectedFile == null)
            {
                SuccessMessage.Visibility = Visibility.Collapsed;
                EmptyFieldMessage.Visibility = Visibility.Visible;
                ErrorDatabaseMessage.Visibility = Visibility.Collapsed;
                return;
            }

            // Save entry to get the result ID
            int resultId = SaveEntry(ResultTextBox.Text, selectedDate.Value.DateTime);
            if (resultId <= 0)
            {
                SuccessMessage.Visibility = Visibility.Collapsed;
                EmptyFieldMessage.Visibility = Visibility.Visible;
                ErrorDatabaseMessage.Visibility = Visibility.Collapsed;
                return;
            }

            // Add result ID to the filename
            filename = $"{resultId}_{selectedFile.Name}";

            bool uploadSuccess = await UploadFileToServer(selectedFile);
            if (!uploadSuccess)
            {
                SuccessMessage.Visibility = Visibility.Collapsed;
                EmptyFieldMessage.Visibility = Visibility.Collapsed;
                ErrorDatabaseMessage.Visibility = Visibility.Visible;
                return;
            }

            // Update the database with the new filename
            bool updateSuccess = UpdateEntryWithFilename(resultId, filename);
            if (!updateSuccess)
            {
                SuccessMessage.Visibility = Visibility.Collapsed;
                EmptyFieldMessage.Visibility = Visibility.Collapsed;
                ErrorDatabaseMessage.Visibility = Visibility.Visible;
                return;
            }

            SuccessMessage.Visibility = Visibility.Visible;
            EmptyFieldMessage.Visibility = Visibility.Collapsed;
            ErrorDatabaseMessage.Visibility = Visibility.Collapsed;
            App.MainFrame.GoBack();
        }

        private async Task<bool> UploadFileToServer(Windows.Storage.StorageFile file)
        {
            try
            {
                string serverUrl = "https://studencki-portal-medyczny.pl/endpoint.php";

                using (var client = new HttpClient())
                using (var form = new MultipartFormDataContent())
                {
                    var fileStream = await file.OpenStreamForReadAsync();
                    var streamContent = new StreamContent(fileStream);
                    streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                    form.Add(streamContent, "plik", filename);

                    var response = await client.PostAsync(serverUrl, form);
                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Błąd podczas przesyłania pliku: {ex.Message}");
                return false;
            }
        }

        private async void PickAPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            var senderButton = sender as Button;
            senderButton.IsEnabled = false;

            PickAPhotoOutputTextBlock.Text = "";

            var openPicker = new FileOpenPicker();

            var window = App.MainWindow;

            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".pdf");

            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                selectedFile = file;
                PickAPhotoOutputTextBlock.Text = "Wybrany wynik: " + file.Name;
            }
            else
            {
                PickAPhotoOutputTextBlock.Text = "Operacja anulowana.";
            }

            senderButton.IsEnabled = true;
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
                    string sql = "INSERT INTO \"WynikibadanDiagnostycznych\" (\"peselPacjenta\", \"idPersonelu\", \"wynikiBadania\", \"dataWyniku\", \"sciezkaDoPliku\") VALUES (@1, @2, @3, @4, @5) RETURNING id;";

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@1", long.Parse(SharedData.pesel));
                        command.Parameters.AddWithValue("@2", long.Parse(SharedData.id));
                        command.Parameters.AddWithValue("@3", wpis);
                        command.Parameters.AddWithValue("@4", examinationDate);
                        command.Parameters.AddWithValue("@5", filename);

                        return (int)command.ExecuteScalar();
                    }
                }
            }
            catch (FormatException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Invalid pesel or id format: {ex.Message}");
                return 0;
            }
            catch (NpgsqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database connection error: {ex.Message}");
                return 3;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unexpected error: {ex.Message}");
                return 3;
            }
        }

        private bool UpdateEntryWithFilename(int resultId, string newFilename)
        {
            var connectionString = "host=bazamedyczna.cziamyieoagt.eu-north-1.rds.amazonaws.com;" +
                                   "username=lekarz;" +
                                   "Password=lekarz;" +
                                   "Database=medical_database";

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "UPDATE \"WynikibadanDiagnostycznych\" SET \"sciezkaDoPliku\" = @1 WHERE \"id\" = @2;";

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@1", newFilename);
                        command.Parameters.AddWithValue("@2", resultId);

                        command.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (NpgsqlException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database connection error: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unexpected error: {ex.Message}");
                return false;
            }
        }
    }
}
