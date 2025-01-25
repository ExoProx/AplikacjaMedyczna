using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Npgsql;

namespace AplikacjaMedyczna
{
    public sealed partial class ChangePasswordDialog : ContentDialog
    {
        private string userId;
        public bool IsPasswordChanged { get; private set; }
        public ChangePasswordDialog(string userId)
        {
            this.InitializeComponent();
            this.userId = userId;
        }

        private void ChangePasswordDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            string newPassword = NewPasswordBox.Password;
            string repeatPassword = RepeatPasswordBox.Password;

            if (newPassword.Length < 11)
            {
                ErrorMessage.Text = "Hasło musi mieć co najmniej 11 znaków";
                ErrorMessage.Visibility = Visibility.Visible;
                args.Cancel = true;
                return;
            }

            if (!newPassword.Any(char.IsDigit))
            {
                ErrorMessage.Text = "Hasło musi zawierać co najmniej jedną cyfrę";
                ErrorMessage.Visibility = Visibility.Visible;
                args.Cancel = true;
                return;
            }

            if (newPassword != repeatPassword)
            {
                ErrorMessage.Text = "Hasła nie są takie same";
                ErrorMessage.Visibility = Visibility.Visible;
                args.Cancel = true;
                return;
            }

            ChangePassword(userId, newPassword);
        }

        private void ChangePasswordDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            IsPasswordChanged = false;
        }

        private void ChangePassword(string userId, string newPassword)
        {
            var cs = "host=bazamedyczna.cziamyieoagt.eu-north-1.rds.amazonaws.com;" +
                     "username=pierwszyLogin;" +
                     "Password=haslo;" +
                     "Database=medical_database";

            using (var con = new NpgsqlConnection(cs))
            {
                con.Open();

                string sql = @"
                UPDATE public.""PersonelMedyczny""
                SET ""haslo"" = @newPassword, ""pierwszehaslo"" = false
                WHERE ""id"" = @userId";

                using (var cmd = new NpgsqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@newPassword", newPassword);
                    cmd.Parameters.AddWithValue("@userId", Convert.ToDecimal(userId));

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        IsPasswordChanged = true;
                    }
                    else
                    {
                        ErrorMessage.Text = "Błąd zmiany hasła";
                        ErrorMessage.Visibility = Visibility.Visible;
                    }
                }
            }
        }
    }
}