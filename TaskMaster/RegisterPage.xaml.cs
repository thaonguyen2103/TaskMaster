using Microsoft.Data.SqlClient;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Threading.Tasks;

namespace TaskMaster
{
    public sealed partial class RegisterPage : Page
    {
        private const string connectionString = "Server=MSI\\SQLEXPRESS;Database=TaskMaster;Trusted_Connection=True;TrustServerCertificate=True;";

        public RegisterPage()
        {
            this.InitializeComponent();
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
                StatusTextBlock.Text = "Vui lòng điền đầy đủ thông tin.";
                StatusTextBlock.Visibility = Visibility.Visible;
                return;
            }
            if (password == confirmPassword)
            {
                if (IsUsernameExists(username))
                {
                    StatusTextBlock.Text = "Tài khoản đã tồn tại, vui lòng nhập lại.";
                    StatusTextBlock.Visibility = Visibility.Visible;
                }
                else
                {
                    SaveUserToDatabase(name, email, phone, username, password);
                }
            }
            else
            {
                StatusTextBlock.Text = "Mật khẩu không trùng khớp, vui lòng nhập lại.";
                StatusTextBlock.Visibility = Visibility.Visible;
            }
        }

        private bool IsUsernameExists(string username)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM [User] WHERE Username = @Username";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", username);

                connection.Open();
                int userCount = (int)command.ExecuteScalar();
                return userCount > 0;
            }
        }

        private void SaveUserToDatabase(string name, string email, string phone, string username, string password)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string userId = GetNextUserId();
                string query = "INSERT INTO [User] (User_ID, Name, Email, Phone, Username, Password) VALUES (@User_ID, @Name, @Email, @Phone, @Username, @Password)";

                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@User_ID", userId);
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Email", email);
                command.Parameters.AddWithValue("@Phone", phone);
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@Password", password);

                try
                {
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery(); 

                    if (rowsAffected > 0)
                    {
                        StatusTextBlock.Text = $"Đăng kí thành công";
                    }
                    else
                    {
                        StatusTextBlock.Text = "Đăng kí thất bại";
                    }
                }
                catch (SqlException ex)
                {
                    StatusTextBlock.Text = $"SQL Error: {ex.Message}\nError Code: {ex.Number}\nState: {ex.State}";
                }
                catch (Exception ex)
                {
                    StatusTextBlock.Text = $"General Error: {ex.Message}";
                }
                finally
                {
                    StatusTextBlock.Visibility = Visibility.Visible;
                }
            }
        }

        private string GetNextUserId()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
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
