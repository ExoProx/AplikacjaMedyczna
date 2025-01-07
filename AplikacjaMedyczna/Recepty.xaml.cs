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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AplikacjaMedyczna
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Recepty : Page
    {
        public Recepta SelectedRecepta { get; set; } // For binding to ContentDialog
        public ObservableCollection<Recepta> ReceptyCollection { get; set; }
        public Recepty()
        {
            this.InitializeComponent();
            NavigationHelper.SplitViewInstance = splitView;
            splitView.IsPaneOpen = true;
            LoadRecepty();
            this.DataContext = this; // Set the DataContext for the page.
            CheckUserId();
        }
        private void CheckUserId()
        {
            if (!string.IsNullOrEmpty(SharedData.id))
            {
                PatientChoiceButton.Visibility = Visibility.Visible;
                PatientChoiceButton.IsEnabled = true;
                AddRecipeButton.Visibility = Visibility.Visible;
                AddRecipeButton.IsEnabled = true;

                ReceptaDetailDialog.PrimaryButtonText = "Edytuj";
                ReceptaDetailDialog.IsPrimaryButtonEnabled = true;
            }
            else
            {
                ReceptaDetailDialog.PrimaryButtonText = "";
                ReceptaDetailDialog.IsPrimaryButtonEnabled = false;
            }
        }
        private void AddRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            App.MainFrame.Navigate(typeof(Insert_Recipe_Form));
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
            // Cast the clicked item to a Recepta object
            var clickedRecepta = e.ClickedItem as Recepta;

            if (clickedRecepta != null)
            {
                // Update SelectedRecepta
                SelectedRecepta = clickedRecepta;

                // Bind the SelectedRecepta to the ContentDialog DataContext
                ReceptaDetailDialog.DataContext = SelectedRecepta;

                // Show the ContentDialog
                await ReceptaDetailDialog.ShowAsync();
            }
        }
        public static ObservableCollection<Recepta> GetRecepty()
        {
            var recepty = new ObservableCollection<Recepta>();
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
                return new ObservableCollection<Recepta>(); ;
            }
            using (var connection = new NpgsqlConnection(cs))
            {
                connection.Open();
                string query = @"
           SELECT 
            Recepty.""id"" AS Recepty_id, 
            Recepty.""dataWystawienia"" as Recepty_dataWystawienia, 
            Recepty.""dataWaznosci"" as Recepty_dataWaznosci, 
            Recepty.""przypisaneLeki"",
            personel.""imie"" AS personel_imie, 
            personel.""nazwisko"" AS personel_nazwisko
        FROM 
            ""Recepty"" as Recepty
        JOIN 
            ""PersonelMedyczny"" as personel
        ON 
            Recepty.""idPersonelu"" = personel.""id"" WHERE Recepty.""peselPacjenta"" = @pesel ORDER BY Recepty_dataWystawienia DESC;";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@pesel", peselNumeric);
                    using (var reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            string imie = reader.GetString(4);
                            string nazwisko = reader.GetString(5);
                            recepty.Add(new Recepta
                            {
                                Id = reader.GetInt32(0),
                                DataWystawieniaRecepty = reader.GetDateTime(1),
                                PeselPacjenta = peselNumeric,
                                DataWaznosciRecepty = reader.GetDateTime(2),
                                DanePersonelu = String.Concat(imie, " ", nazwisko),
                                Leki = reader.GetString(3)
                            });
                        }
                    }
                }
            }

            return recepty;
        }
        private void LoadRecepty()
        {
            ReceptyCollection = GetRecepty(); // Load the data into the ObservableCollection.
        }
        private async void ReceptaDetailDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
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
            var selectedRecepta = ReceptaDetailDialog.DataContext as Recepta;

            if (selectedRecepta != null)
            {
                ReceptaDetailDialog.Hide();

                var editDialog = new ContentDialog
                {
                    Title = "Edytuj Receptê",
                    PrimaryButtonText = "Zapisz",
                    CloseButtonText = "Anuluj",
                    XamlRoot = this.XamlRoot
                };

                var stackPanel = new StackPanel();
                var lekiTextBox = new TextBox { Text = selectedRecepta.Leki, Margin = new Thickness(0, 0, 0, 10) };

                stackPanel.Children.Add(new TextBlock { Text = "Leki:", FontWeight = FontWeights.Bold });
                stackPanel.Children.Add(lekiTextBox);
                stackPanel.Children.Add(new TextBlock { Text = "Data Wystawienia:", FontWeight = FontWeights.Bold });
                stackPanel.Children.Add(new TextBlock { Text = selectedRecepta.DataWystawieniaRecepty.ToString(), Margin = new Thickness(0, 0, 0, 10) });
                stackPanel.Children.Add(new TextBlock { Text = "Data Wa¿noœci:", FontWeight = FontWeights.Bold });
                stackPanel.Children.Add(new TextBlock { Text = selectedRecepta.DataWaznosciRecepty.ToString(), Margin = new Thickness(0, 0, 0, 10) });
                stackPanel.Children.Add(new TextBlock { Text = "PESEL Pacjenta:", FontWeight = FontWeights.Bold });
                stackPanel.Children.Add(new TextBlock { Text = selectedRecepta.PeselPacjenta.ToString(), Margin = new Thickness(0, 0, 0, 10) });
                stackPanel.Children.Add(new TextBlock { Text = "Dane personelu:", FontWeight = FontWeights.Bold });
                stackPanel.Children.Add(new TextBlock { Text = selectedRecepta.DanePersonelu });

                editDialog.Content = stackPanel;

                var result = await editDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    if (string.IsNullOrWhiteSpace(lekiTextBox.Text))
                    {
                        await ShowMessageDialog("B³¹d", "Pole 'Leki' nie mo¿e byæ puste.");
                        await EditButton_ClickAsync(sender, args);
                        return;
                    }

                    selectedRecepta.Leki = lekiTextBox.Text;

                    var cs = "host=bazamedyczna.cziamyieoagt.eu-north-1.rds.amazonaws.com;" +
                                "username=postgres;" +
                                "Password=adminadmin;" +
                                "Database=medical_database";
                    using (var connection = new NpgsqlConnection(cs))
                    {
                        connection.Open();
                        string query = @"
                UPDATE ""Recepty""
                SET ""przypisaneLeki"" = @leki
                WHERE ""id"" = @id";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@leki", selectedRecepta.Leki);
                            command.Parameters.AddWithValue("@id", selectedRecepta.Id);
                            command.ExecuteNonQuery();
                        }
                    }

                    var receptaToUpdate = ReceptyCollection.FirstOrDefault(r => r.Id == selectedRecepta.Id);
                    if (receptaToUpdate != null)
                    {
                        receptaToUpdate.Leki = selectedRecepta.Leki;
                    }

                    await ShowMessageDialog("Sukces", "Recepta zosta³a pomyœlnie zaktualizowana.");
                }
                else
                {
                    await ShowMessageDialog("Anulowano", "Edycja recepty zosta³a anulowana.");
                }

                await ShowReceptaDetailDialog(selectedRecepta);
            }
        }

        private async Task ShowReceptaDetailDialog(Recepta recepta)
        {
            ReceptaDetailDialog.DataContext = recepta;

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(new TextBlock { Text = "Leki:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(new TextBlock { Text = recepta.Leki, Margin = new Thickness(0, 0, 0, 10) });
            stackPanel.Children.Add(new TextBlock { Text = "Data Wystawienia:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(new TextBlock { Text = recepta.DataWystawieniaRecepty.ToString(), Margin = new Thickness(0, 0, 0, 10) });
            stackPanel.Children.Add(new TextBlock { Text = "Data Wa¿noœci:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(new TextBlock { Text = recepta.DataWaznosciRecepty.ToString(), Margin = new Thickness(0, 0, 0, 10) });
            stackPanel.Children.Add(new TextBlock { Text = "PESEL Pacjenta:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(new TextBlock { Text = recepta.PeselPacjenta.ToString(), Margin = new Thickness(0, 0, 0, 10) });
            stackPanel.Children.Add(new TextBlock { Text = "Dane personelu:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(new TextBlock { Text = recepta.DanePersonelu });

            ReceptaDetailDialog.Content = stackPanel;
            ReceptaDetailDialog.PrimaryButtonText = "Edytuj";
            ReceptaDetailDialog.CloseButtonText = "Close";

            await ReceptaDetailDialog.ShowAsync();
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