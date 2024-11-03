using Microsoft.Data.SqlClient;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Threading.Tasks;

namespace TaskMaster
{
    public sealed partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            this.InitializeComponent();
            ShowRegisterPanelAfterDelay();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            FadeInWelcomeStoryboard.Begin();
        }

        private void ShowRegisterPanelAfterDelay()
        {
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            timer.Tick += (sender, e) =>
            {
                timer.Stop();
                WelcomeTextBlock.Visibility = Visibility.Collapsed; // Hide welcome message
                RegisterStackPanel.Visibility = Visibility.Visible;
                FadeInRegisterStoryboard.Begin(); // Show registration panel
            };
            timer.Start(); // Start the timer
        }
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text;
            string email = EmailTextBox.Text;
            string phone = PhoneTextBox.Text;
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;
            string confirmPassword = PasswordBox2.Password;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                StatusTextBlock.Text = "Please fill in all required information.";
                StatusTextBlock.Visibility = Visibility.Visible;
                return;
            }
            if (password == confirmPassword)
            {
                if (IsUsernameExists(username))
                {
                    StatusTextBlock.Text = "User name already exists, please try again.";
                    StatusTextBlock.Visibility = Visibility.Visible;
                }
                else
                {
                    SaveUserToDatabase(name, email, phone, username, password);
                }
            }
            else
            {
                StatusTextBlock.Text = "Please confirm the correct password.";
                StatusTextBlock.Visibility = Visibility.Visible;
            }
        }

        private bool IsUsernameExists(string username)
        {
            using (SqlConnection connection = DatabaseConnection.GetConnection())
            {
                string query = "SELECT COUNT(*) FROM [User] WHERE Username = @Username";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", username);

                connection.Open();
                int userCount = (int)command.ExecuteScalar();
                return userCount > 0;
            }
        }

        private async void SaveUserToDatabase(string name, string email, string phone, string username, string password)
        {
            using (SqlConnection connection = DatabaseConnection.GetConnection())
            {
                string userId = GetNextUserId();
                string query = "INSERT INTO [User] (User_ID, Name, Email, Phone, Username, Password, Avatar) VALUES (@User_ID, @Name, @Email, @Phone, @Username, @Password, @Avatar)";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@User_ID", userId);
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@Phone", phone);
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@Password", password);
                string avatar = "ms-appx:///Assets/avatar/Picture0.png";
                command.Parameters.AddWithValue("@Avatar", avatar);
                try
                {
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery(); 

                    if (rowsAffected > 0)
                    {
                        ContentDialog failDialog = new ContentDialog
                        {
                            Title = "Notification",
                            Content = "Registration successful",
                            CloseButtonText = "OK",
                            XamlRoot = this.XamlRoot
                        };
                        await failDialog.ShowAsync();
                        Frame.Navigate(typeof(LoginPage));
                    }
                    else
                    {
                     
                    }
                }
                catch (SqlException ex)
                {
                    
                }
                catch (Exception ex)
                {
                    
                }
                finally
                {
                    StatusTextBlock.Visibility = Visibility.Visible;
                }
            }
        }

        private string GetNextUserId()
        {
            using (SqlConnection connection = DatabaseConnection.GetConnection())
            {
                string query = "SELECT ISNULL(MAX(CAST(SUBSTRING(User_ID, 3, 8) AS INT)), 0) + 1 FROM [User]";
                SqlCommand command = new SqlCommand(query, connection);

                connection.Open();
                int nextId = (int)command.ExecuteScalar();
                return $"US{nextId:D8}";
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(LoginPage));
        }

    }
}
