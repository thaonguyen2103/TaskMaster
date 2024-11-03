using Microsoft.Data.SqlClient;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.Networking.NetworkOperators;

namespace TaskMaster
{
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            this.InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (CheckUserCredentials(username, password))
            {
                UserSession.CurrentUsername = username;
                Frame.Navigate(typeof(MainPage));
            }
            else
            {
                StatusTextBlock.Text = "Username or password is incorrect.";
                StatusTextBlock.Visibility = Visibility.Visible;
            }
        }

        private bool CheckUserCredentials(string username, string password)
        {
            using (SqlConnection connection = DatabaseConnection.GetConnection())
            {
                string query = "SELECT COUNT(*) FROM [User] WHERE Username = @Username AND Password = @Password";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@Password", password);

                try
                {
                    connection.Open();
                    int userCount = (int)command.ExecuteScalar();

                    return userCount > 0;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RegisterPage));
        }
    }
}
