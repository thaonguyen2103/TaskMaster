using Microsoft.Data.SqlClient;
using System;

namespace TaskMaster.ViewModels
{
    public class RegisterViewModel : BaseViewModel
    {
        private string _name;
        private string _email;
        private string _phone;
        private string _username;
        private string _password;
        private string _confirmPassword;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
            }
        }

        public string Phone
        {
            get => _phone;
            set
            {
                _phone = value;
                OnPropertyChanged();
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                OnPropertyChanged();
            }
        }

        private string _statusMessage;
        private bool _isErrorVisible;

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public bool IsErrorVisible
        {
            get => _isErrorVisible;
            set { _isErrorVisible = value; OnPropertyChanged(); }
        }

        public RelayCommand BackLogInCommand { get; set; }
        public RelayCommand RegisterCommand { get; }
        public Action LogInNavigation { get; set; }

        public RegisterViewModel()
        {
            RegisterCommand = new RelayCommand(ExecuteRegister, CanRegister);
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Name) ||
                    e.PropertyName == nameof(Email) ||
                    e.PropertyName == nameof(Phone) ||
                    e.PropertyName == nameof(Username) ||
                    e.PropertyName == nameof(Password) ||
                    e.PropertyName == nameof(ConfirmPassword))
                {
                    (RegisterCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            };
                BackLogInCommand = new RelayCommand(BackLogIn);
        }

        private void BackLogIn()
        {
            LogInNavigation?.Invoke();
        }

        private void ExecuteRegister()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Email) ||
                    string.IsNullOrWhiteSpace(Phone) || string.IsNullOrWhiteSpace(Username) ||
                    string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
                {
                    StatusMessage = "Please fill in all required fields.";
                    IsErrorVisible = true;
                    return;
                }

                // Kiểm tra mật khẩu có trùng khớp không
                if (Password != ConfirmPassword)
                {
                    StatusMessage = "Passwords do not match!";
                    IsErrorVisible = true;
                    return;
                }

                // Kiểm tra tên đăng nhập đã tồn tại chưa
                if (IsUsernameExists(Username))
                {
                    StatusMessage = "Username already exists.";
                    IsErrorVisible = true;
                    return;
                }

                // Lưu thông tin người dùng vào cơ sở dữ liệu
                SaveUserToDatabase();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                IsErrorVisible = true;
            }
        }

        private bool CanRegister()
        {
            return !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Email) &&
                   !string.IsNullOrEmpty(Phone) && !string.IsNullOrEmpty(Username) &&
                   !string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(ConfirmPassword);
        }

        private bool IsUsernameExists(string username)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                string query = "SELECT COUNT(*) FROM [User] WHERE Username = @Username";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", username);
                try
                {
                    connection.Open();
                    int userCount = (int)command.ExecuteScalar();
                    return userCount > 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error checking username: {ex.Message}");
                    return false;
                }
            }
        }

        private void SaveUserToDatabase()
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                string userId = GetNextUserId();
                string defaultAvatar = "ms-appx:///Assets/avatar/Picture0.png";
                string query = "INSERT INTO [User] (User_ID, Name, Email, Phone, Username, Password, Avatar) " +
                               "VALUES (@User_ID, @Name, @Email, @Phone, @Username, @Password, @Avatar)";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@User_ID", userId);
                command.Parameters.AddWithValue("@Name", Name);
                command.Parameters.AddWithValue("@Email", Email);
                command.Parameters.AddWithValue("@Phone", Phone);
                command.Parameters.AddWithValue("@Username", Username);
                command.Parameters.AddWithValue("@Password", Password);
                command.Parameters.AddWithValue("@Avatar", defaultAvatar);  
                try
                {
                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        StatusMessage = "Registration successful!";
                        IsErrorVisible = false;
                        // Điều hướng đến trang đăng nhập
                        LogInNavigation?.Invoke();
                    }
                    else
                    {
                        StatusMessage = "Registration failed. Please try again.";
                        IsErrorVisible = true;
                    }
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Error: {ex.Message}";
                    IsErrorVisible = true;
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
    }
}
