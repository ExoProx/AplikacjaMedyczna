using AplikacjaMedyczna;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AplikacjaMedyczna
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 

    public sealed partial class Wpisy : Page
    {
        public Wpis SelectedWpis { get; set; } // For binding to ContentDialog
        public ObservableCollection<Wpis> WpisyCollection { get; set; }
        public Wpisy()
        {
            this.InitializeComponent();
            NavigationHelper.SplitViewInstance = splitView;
            splitView.IsPaneOpen = true;
            LoadWpisy();
            this.DataContext = this; // Set the DataContext for the page.
            CheckUserId();
        }
        private void AddDescriptionButton_Click(object sender, RoutedEventArgs e)
        {
            App.MainFrame.Navigate(typeof(dodaj_wpis));
        }
        private void CheckUserId()
        {
            if (!string.IsNullOrEmpty(SharedData.id))
            {
                PatientChoiceButton.Visibility = Visibility.Visible;
                PatientChoiceButton.IsEnabled = true;
                AddDescriptionButton.Visibility = Visibility.Visible;
                AddDescriptionButton.IsEnabled = true;

                WpisDetailDialog.PrimaryButtonText = "Edytuj";
                WpisDetailDialog.IsPrimaryButtonEnabled = true;
            }
            else
            {
                WpisDetailDialog.PrimaryButtonText = "";
                WpisDetailDialog.IsPrimaryButtonEnabled = false;
            }
        }
        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                // Pass the button name or content to a helper method
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
        private void DodajButton_Click(object sender, RoutedEventArgs e)
        {
            App.MainFrame.Navigate(typeof(dodaj_wpis));

        }
        private async void FilteredListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Cast the clicked item to a Wpis object
            var clickedWpis = e.ClickedItem as Wpis;

            if (clickedWpis != null)
            {
                // Update SelectedWpis
                SelectedWpis = clickedWpis;

                // Bind the SelectedWpis to the ContentDialog DataContext
                WpisDetailDialog.DataContext = SelectedWpis;

                // Show the ContentDialog
                await WpisDetailDialog.ShowAsync();
            }
        }
        public static ObservableCollection<Wpis> GetWpisy()
        {
            var wpisy = new ObservableCollection<Wpis>();

            var cs = "host=bazamedyczna.cziamyieoagt.eu-north-1.rds.amazonaws.com;" +
                        "username=postgres;" +
                        "Password=adminadmin;" +
                        "Database=medical_database";
            string pesel = SharedData.pesel;
            decimal peselNumeric;

            // Convert PESEL to numeric (decimal)
            if (!decimal.TryParse(pesel, out peselNumeric))
            {
                // Invalid PESEL format
                return new ObservableCollection<Wpis>(); ;
            }
            using (var connection = new NpgsqlConnection(cs))
            {
                connection.Open();
                string query = @"
            SELECT 
            Wpisy.""id"" AS wpisy_id, 
            Wpisy.""wpis"",
            Wpisy.""dataWpisu"",  
            personel.""imie"", 
            personel.""nazwisko"" 
        FROM 
            ""WpisyMedyczne"" as Wpisy
        JOIN 
            ""PersonelMedyczny"" as personel
        ON 
            Wpisy.""idPersonelu"" = personel.""id"" WHERE Wpisy.""peselPacjenta"" = @pesel ORDER BY Wpisy.""dataWpisu"" DESC;";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@pesel", peselNumeric);
                    using (var reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            string imie = reader.GetString(3);
                            string nazwisko = reader.GetString(4);
                            wpisy.Add(new Wpis
                            {
                                Id = reader.GetInt32(0),
                                WpisText = reader.GetString(1),
                                PeselPacjenta = peselNumeric,
                                DataWpisu = reader.GetDateTime(2),
                                DanePersonelu = String.Concat(imie, " ", nazwisko)
                            });
                        }
                    }
                }
            }

            return wpisy;
        }

        private void LoadWpisy()
        {
            WpisyCollection = GetWpisy(); // Load the data into the ObservableCollection.
        }
        private async void WpisDetailDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            // Check if the dialog should close, or cancel the close if needed
            if (SomeConditionToPreventClose())
            {
                args.Cancel = true; // This will prevent the dialog from closing
                                    // Optionally, you can show a message or handle additional logic
                var dialog = new ContentDialog
                {
                    Title = "Cannot Close",
                    Content = "Please complete all necessary actions before closing.",
                    CloseButtonText = "OK"
                };
                await dialog.ShowAsync(); // Show a new dialog if needed
            }
            else
            {
                // Perform any necessary cleanup or actions before closing
                // For example, you could save data or log an action

            }
        }

        private bool SomeConditionToPreventClose()
        {
            // Your custom condition to prevent closing, for example:
            // return true if the dialog should not close
            return false; // In this case, always allow closing
        }

        private async void EditButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            await EditButton_ClickAsync(sender, args);
        }

        private async Task EditButton_ClickAsync(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var selectedWpis = WpisDetailDialog.DataContext as Wpis;

            if (selectedWpis != null)
            {
                WpisDetailDialog.Hide();

                var editDialog = new ContentDialog
                {
                    Title = "Edytuj Wpis",
                    PrimaryButtonText = "Zapisz",
                    CloseButtonText = "Anuluj",
                    XamlRoot = this.XamlRoot
                };

                var stackPanel = new StackPanel();
                var wpisTextBox = new TextBox { Text = selectedWpis.WpisText, Margin = new Thickness(0, 0, 0, 10) };

                stackPanel.Children.Add(new TextBlock { Text = "Wpis:", FontWeight = FontWeights.Bold });
                stackPanel.Children.Add(wpisTextBox);
                stackPanel.Children.Add(new TextBlock { Text = "Data Wpisu:", FontWeight = FontWeights.Bold });
                stackPanel.Children.Add(new TextBlock { Text = selectedWpis.DataWpisu.ToString(), Margin = new Thickness(0, 0, 0, 10) });
                stackPanel.Children.Add(new TextBlock { Text = "PESEL Pacjenta:", FontWeight = FontWeights.Bold });
                stackPanel.Children.Add(new TextBlock { Text = selectedWpis.PeselPacjenta.ToString(), Margin = new Thickness(0, 0, 0, 10) });
                stackPanel.Children.Add(new TextBlock { Text = "Dane personelu:", FontWeight = FontWeights.Bold });
                stackPanel.Children.Add(new TextBlock { Text = selectedWpis.DanePersonelu });

                editDialog.Content = stackPanel;

                var result = await editDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    if (string.IsNullOrWhiteSpace(wpisTextBox.Text))
                    {
                        await ShowMessageDialog("B³¹d", "Pole 'Wpis' nie mo¿e byæ puste.");
                        await EditButton_ClickAsync(sender, args); // Ponownie uruchom dialog edycji
                        return;
                    }

                    selectedWpis.WpisText = wpisTextBox.Text;

                    var cs = "host=bazamedyczna.cziamyieoagt.eu-north-1.rds.amazonaws.com;" +
                                "username=postgres;" +
                                "Password=adminadmin;" +
                                "Database=medical_database";
                    using (var connection = new NpgsqlConnection(cs))
                    {
                        connection.Open();
                        string query = @"
                UPDATE ""WpisyMedyczne""
                SET ""wpis"" = @wpis
                WHERE ""id"" = @id";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@wpis", selectedWpis.WpisText);
                            command.Parameters.AddWithValue("@id", selectedWpis.Id);
                            command.ExecuteNonQuery();
                        }
                    }

                    var wpisToUpdate = WpisyCollection.FirstOrDefault(w => w.Id == selectedWpis.Id);
                    if (wpisToUpdate != null)
                    {
                        wpisToUpdate.WpisText = selectedWpis.WpisText;
                    }

                    await ShowMessageDialog("Sukces", "Wpis zosta³ pomyœlnie zaktualizowany.");
                }
                else
                {
                    await ShowMessageDialog("Anulowano", "Edycja wpisu zosta³a anulowana.");
                }

                await ShowWpisDetailDialog(selectedWpis);
            }
        }

        private async Task ShowWpisDetailDialog(Wpis wpis)
        {
            WpisDetailDialog.DataContext = wpis;

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(new TextBlock { Text = "Wpis:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(new TextBlock { Text = wpis.WpisText, Margin = new Thickness(0, 0, 0, 10) });
            stackPanel.Children.Add(new TextBlock { Text = "Data Wpisu:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(new TextBlock { Text = wpis.DataWpisu.ToString(), Margin = new Thickness(0, 0, 0, 10) });
            stackPanel.Children.Add(new TextBlock { Text = "PESEL Pacjenta:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(new TextBlock { Text = wpis.PeselPacjenta.ToString(), Margin = new Thickness(0, 0, 0, 10) });
            stackPanel.Children.Add(new TextBlock { Text = "Dane personelu:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(new TextBlock { Text = wpis.DanePersonelu });

            WpisDetailDialog.Content = stackPanel;
            WpisDetailDialog.PrimaryButtonText = "Edytuj";
            WpisDetailDialog.CloseButtonText = "Close";

            await WpisDetailDialog.ShowAsync();
        }

        private async Task ShowMessageDialog(string title, string content)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}
