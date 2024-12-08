using Microsoft.Data.SqlClient;
using System;
using Microsoft.UI.Dispatching;

namespace TaskMaster.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _username;
        private string _password;
        private string _statusMessage;
        private bool _isErrorVisible;
        // Property to display error or status messages
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }
        // Indicates whether an error message is visible
        public bool IsErrorVisible
        {
            get => _isErrorVisible;
            set { _isErrorVisible = value; OnPropertyChanged(); }
        }
        // Property to hold the username input
        public string Username
        {
            get => _username;
            set { _username = value; 
                OnPropertyChanged(); 
                LoginCommand.RaiseCanExecuteChanged(); }
        }
        // Property to hold the password input
        public string Password
        {
            get => _password;
            set { 
                _password = value; 
                OnPropertyChanged(); 
                LoginCommand.RaiseCanExecuteChanged(); }
        }
        // Command to execute the login operation
        public RelayCommand LoginCommand { get; }
        public Action LoginSuccess { get; set; }
        

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin, CanLogin);
            RegisterCommand = new RelayCommand(ExecuteRegister);
        }

        private void ExecuteLogin()
        {
            try
            {
                if (CheckUserCredentials(Username, Password))
                {
                    StatusMessage = string.Empty; 
                    IsErrorVisible = false;
                    DispatcherQueue.GetForCurrentThread().TryEnqueue(() => LoginSuccess?.Invoke());
                }
                else
                {
                    StatusMessage = "Invalid username or password!";
                    IsErrorVisible = true;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                IsErrorVisible = true;
            }
        }
        // Validates if the login can proceed based on input fields
        private bool CanLogin()
        {
            return !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);
        }
        // Executes the login logic and checks credentials
        private bool CheckUserCredentials(string username, string password)
        {
            using (var connection = DatabaseConnection.GetConnection())
            {
                string query = "SELECT User_ID FROM [User] WHERE Username = @Username AND Password = @Password";
                using var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@Password", password);
                try
                {
                    connection.Open();
                    var result = command.ExecuteScalar();
                    if (result != null)
                    {
                        UserSession.CurrentUserID = result.ToString();
                        return true;
                    }
                    else
                    {
                        return false; 
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return false; 
                }
            }
        }
        // Command to navigate to the registration page
        public Action RegisterPageNavigation { get; set; }
        public RelayCommand RegisterCommand { get; }
        private void ExecuteRegister()
        {
            RegisterPageNavigation?.Invoke();
        }
    }
}
