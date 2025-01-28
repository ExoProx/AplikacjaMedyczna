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

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.MainFrame.GoBack();
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedDate = TerminReceptyCalendarView.Date;
            if (selectedDate == null)
            {
                SuccessMessage.Visibility = Visibility.Collapsed;
                EmptyFieldMessage.Visibility = Visibility.Visible;
                ErrorDatabaseMessage.Visibility = Visibility.Collapsed;
                return;
            }

            string fileName = PickAPhotoOutputTextBlock.Text.Replace("Wybrany wynik: ", "").Trim();
            int status = SaveEntry(ResultTextBox.Text, selectedDate.Value.DateTime);

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
                    filename = file.Name;
                    form.Add(streamContent, "plik", file.Name);

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
                PickAPhotoOutputTextBlock.Text = "Wybrany wynik: " + file.Name;

                bool uploadSuccess = await UploadFileToServer(file);
                if (uploadSuccess)
                {
                    PickAPhotoOutputTextBlock.Text = "Plik pomyślnie przesłany: " + file.Name;
                }
                else
                {
                    PickAPhotoOutputTextBlock.Text = "Wystąpił błąd podczas przesyłania pliku.";
                }
            }
            else
            {
                PickAPhotoOutputTextBlock.Text = "Operacja anulowana.";
            }

            senderButton.IsEnabled = true;
        }


        private int SaveEntry(string wpis, DateTime examinationDate)
        {
            if (string.IsNullOrWhiteSpace(wpis) || string.IsNullOrWhiteSpace(filename))
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
                    string sql = "INSERT INTO \"WynikibadanDiagnostycznych\" (\"peselPacjenta\", \"idPersonelu\", \"wynikiBadania\", \"dataWyniku\", \"sciezkaDoPliku\") VALUES (@1, @2, @3, @4, @5);";

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@1", long.Parse(SharedData.pesel));
                        command.Parameters.AddWithValue("@2", long.Parse(SharedData.id));
                        command.Parameters.AddWithValue("@3", wpis);
                        command.Parameters.AddWithValue("@4", examinationDate);
                        command.Parameters.AddWithValue("@5", filename);

                        command.ExecuteNonQuery();
                    }
                }

                return 1;
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
    }
}
