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
    public sealed partial class Skierowania : Page
    {
        public Skierowanie SelectedSkierowanie { get; set; } // For binding to ContentDialog
        public ObservableCollection<Skierowanie> SkierowaniaCollection { get; set; }
        public Skierowania()
        {
            this.InitializeComponent();
            NavigationHelper.SplitViewInstance = splitView;
            splitView.IsPaneOpen = true;
            LoadSkierowania();
            this.DataContext = this; // Set the DataContext for the page.
            CheckUserId();
        }
        private void AddRefferalButton_Click(object sender, RoutedEventArgs e)
        {
            App.MainFrame.Navigate(typeof(Insert_referral_Form));
        }
        private void CheckUserId()
        {
            if (!string.IsNullOrEmpty(SharedData.id))
            {
                PatientChoiceButton.Visibility = Visibility.Visible;
                PatientChoiceButton.IsEnabled = true;
                AddRefferalButton.Visibility = Visibility.Visible;
                AddRefferalButton.IsEnabled = true;

                SkierowanieDetailDialog.PrimaryButtonText = "Edytuj";
                SkierowanieDetailDialog.IsPrimaryButtonEnabled = true;
            }
            else
            {
                SkierowanieDetailDialog.PrimaryButtonText = "";
                SkierowanieDetailDialog.IsPrimaryButtonEnabled = false;
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

        private async void FilteredListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Cast the clicked item to a Skierowanie object
            var clickedSkierowanie = e.ClickedItem as Skierowanie;

            if (clickedSkierowanie != null)
            {
                // Update SelectedSkierowanie
                SelectedSkierowanie = clickedSkierowanie;

                // Bind the SelectedSkierowanie to the ContentDialog DataContext
                SkierowanieDetailDialog.DataContext = SelectedSkierowanie;

                // Show the ContentDialog
                await SkierowanieDetailDialog.ShowAsync();
            }
        }

        public static ObservableCollection<Skierowanie> GetSkierowania()
        {
            var skierowania = new ObservableCollection<Skierowanie>();

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
                return new ObservableCollection<Skierowanie>(); ;
            }
            using (var connection = new NpgsqlConnection(cs))
            {
                connection.Open();
                string query = @"
            SELECT 
            Skierowania.""id"",
            Skierowania.""skierowanie"" ,
            Skierowania.""dataSkierowania"",  
            personel.""imie"", 
            personel.""nazwisko""
        FROM 
            ""Skierowania"" as Skierowania
        JOIN 
            ""PersonelMedyczny"" as personel
        ON 
            Skierowania.""idPersonelu"" = personel.""id"" WHERE Skierowania.""peselPacjenta"" = @pesel ORDER BY Skierowania.""dataSkierowania"" DESC;";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@pesel", peselNumeric);
                    using (var reader = command.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            string imie = reader.GetString(3);
                            string nazwisko = reader.GetString(4);
                            skierowania.Add(new Skierowanie
                            {
                                Id = reader.GetInt32(0),
                                SkierowanieText = reader.GetString(1),
                                PeselPacjenta = peselNumeric,
                                DataSkierowania = reader.GetDateTime(2),
                                DanePersonelu = String.Concat(imie, " ", nazwisko)
                            });
                        }
                    }
                }
            }

            return skierowania;
        }

        private void LoadSkierowania()
        {
            SkierowaniaCollection = GetSkierowania(); // Load the data into the ObservableCollection.
        }

        private async void SkierowanieDetailDialog_Closing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (SomeConditionToPreventClose())
            {
                args.Cancel = true;
                var dialog = new ContentDialog
                {
                    Title = "Cannot Close",
                    Content = "Please complete all necessary actions before closing.",
                    CloseButtonText = "OK"
                };
                await dialog.ShowAsync();
            }
        }

        private bool SomeConditionToPreventClose()
        {
            return false;
        }

        private async void EditButton_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            await EditButton_ClickAsync(sender, args);
        }

        private async Task EditButton_ClickAsync(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var selectedSkierowanie = SkierowanieDetailDialog.DataContext as Skierowanie;

            if (selectedSkierowanie != null)
            {
                SkierowanieDetailDialog.Hide();

                var editDialog = new ContentDialog
                {
                    Title = "Edytuj Skierowanie",
                    PrimaryButtonText = "Zapisz",
                    CloseButtonText = "Anuluj",
                    XamlRoot = this.XamlRoot
                };

                var stackPanel = new StackPanel();
                var skierowanieTextBox = new TextBox { Text = selectedSkierowanie.SkierowanieText, Margin = new Thickness(0, 0, 0, 10) };

                stackPanel.Children.Add(new TextBlock { Text = "Skierowanie:", FontWeight = FontWeights.Bold });
                stackPanel.Children.Add(skierowanieTextBox);
                stackPanel.Children.Add(new TextBlock { Text = "Data Skierowania:", FontWeight = FontWeights.Bold });
                stackPanel.Children.Add(new TextBlock { Text = selectedSkierowanie.DataSkierowania.ToString(), Margin = new Thickness(0, 0, 0, 10) });
                stackPanel.Children.Add(new TextBlock { Text = "PESEL Pacjenta:", FontWeight = FontWeights.Bold });
                stackPanel.Children.Add(new TextBlock { Text = selectedSkierowanie.PeselPacjenta.ToString(), Margin = new Thickness(0, 0, 0, 10) });
                stackPanel.Children.Add(new TextBlock { Text = "Dane personelu:", FontWeight = FontWeights.Bold });
                stackPanel.Children.Add(new TextBlock { Text = selectedSkierowanie.DanePersonelu });

                editDialog.Content = stackPanel;

                var result = await editDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    if (string.IsNullOrWhiteSpace(skierowanieTextBox.Text))
                    {
                        await ShowMessageDialog("B³¹d", "Pole 'Skierowanie' nie mo¿e byæ puste.");
                        await EditButton_ClickAsync(sender, args);
                        return;
                    }

                    selectedSkierowanie.SkierowanieText = skierowanieTextBox.Text;

                    var cs = "host=bazamedyczna.cziamyieoagt.eu-north-1.rds.amazonaws.com;" +
                                "username=postgres;" +
                                "Password=adminadmin;" +
                                "Database=medical_database";
                    using (var connection = new NpgsqlConnection(cs))
                    {
                        connection.Open();
                        string query = @"
                UPDATE ""Skierowania""
                SET ""skierowanie"" = @skierowanie
                WHERE ""id"" = @id";

                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@skierowanie", selectedSkierowanie.SkierowanieText);
                            command.Parameters.AddWithValue("@id", selectedSkierowanie.Id);
                            command.ExecuteNonQuery();
                        }
                    }

                    var skierowanieToUpdate = SkierowaniaCollection.FirstOrDefault(s => s.Id == selectedSkierowanie.Id);
                    if (skierowanieToUpdate != null)
                    {
                        skierowanieToUpdate.SkierowanieText = selectedSkierowanie.SkierowanieText;
                    }

                    await ShowMessageDialog("Sukces", "Skierowanie zosta³o pomyœlnie zaktualizowane.");
                }
                else
                {
                    await ShowMessageDialog("Anulowano", "Edycja skierowania zosta³a anulowana.");
                }

                await ShowSkierowanieDetailDialog(selectedSkierowanie);
            }
        }

        private async Task ShowSkierowanieDetailDialog(Skierowanie skierowanie)
        {
            SkierowanieDetailDialog.DataContext = skierowanie;

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(new TextBlock { Text = "Skierowanie:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(new TextBlock { Text = skierowanie.SkierowanieText, Margin = new Thickness(0, 0, 0, 10) });
            stackPanel.Children.Add(new TextBlock { Text = "Data Skierowania:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(new TextBlock { Text = skierowanie.DataSkierowania.ToString(), Margin = new Thickness(0, 0, 0, 10) });
            stackPanel.Children.Add(new TextBlock { Text = "PESEL Pacjenta:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(new TextBlock { Text = skierowanie.PeselPacjenta.ToString(), Margin = new Thickness(0, 0, 0, 10) });
            stackPanel.Children.Add(new TextBlock { Text = "Dane personelu:", FontWeight = FontWeights.Bold });
            stackPanel.Children.Add(new TextBlock { Text = skierowanie.DanePersonelu });

            SkierowanieDetailDialog.Content = stackPanel;
            SkierowanieDetailDialog.PrimaryButtonText = "Edytuj";
            SkierowanieDetailDialog.CloseButtonText = "Close";

            await SkierowanieDetailDialog.ShowAsync();
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

