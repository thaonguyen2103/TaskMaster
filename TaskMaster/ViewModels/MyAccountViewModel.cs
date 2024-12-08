using Microsoft.Data.SqlClient;
using System;
using TaskMaster.Models;

namespace TaskMaster.ViewModels
{
    public class MyAccountViewModel : BaseViewModel
    {
        private User _user;

        public User User
        {
            get { return _user; }
            set
            {
                if (_user != value)
                {
                    _user = value;
                    OnPropertyChanged(nameof(User));
                    OnPropertyChanged(nameof(DOBForDatePicker));
                }
            }
        }
        private string _InformationVisibility;
        public string InformationVisibility
        {
            get => _InformationVisibility;
            set { _InformationVisibility = value; OnPropertyChanged(); }
        }
        private string _AboutUsVisibility;
        public string AboutUsVisibility
        {
            get => _AboutUsVisibility;
            set { _AboutUsVisibility = value; OnPropertyChanged(); }
        }
        // Command to update account details
        public RelayCommand UpdateAccountCommand { get; }

        // Command to change avatar
        public RelayCommand ChangeAvatarCommand { get; }
        
        //Command to open Information
        public RelayCommand openInformationCommand { get; }
        //Command to open About Us
        public RelayCommand openAboutUsCommand { get; }

        // Event for when the user updates successfully
        public event EventHandler UpdateSuccess;

        // Event to open avatar picker
        public event EventHandler OpenAvatarPicker;


        public Action BackToMainPage { get; set; }
        public RelayCommand MainPageNavigation { get; }

        public MyAccountViewModel()
        {
            AboutUsVisibility = "Collapsed";
            InformationVisibility = "Visible";
            ChangeAvatarCommand = new RelayCommand(ChangeAvatar);
            MainPageNavigation = new RelayCommand(ExecuteMainPageNavigation);
            UpdateAccountCommand = new RelayCommand(UpdateUserDataAsync);
            openInformationCommand = new RelayCommand(openInformation);
            openAboutUsCommand = new RelayCommand(openAboutUs);
        }

        private void openAboutUs()
        {
            InformationVisibility = "Collapsed";
            AboutUsVisibility = "Visible";
        }

        private void openInformation()
        {
            InformationVisibility = "Visible";
            AboutUsVisibility = "Collapsed";
        }

        private void ChangeAvatar()
        {
            OpenAvatarPicker?.Invoke(this, EventArgs.Empty);
        }

        private void ExecuteMainPageNavigation()
        {
            BackToMainPage?.Invoke();
        }

        public DateTimeOffset? DOBForDatePicker
        {
            get => User?.DOB.HasValue == true ? (DateTimeOffset?)User.DOB.Value : null;
            set
            {
                if (User != null)
                {
                    User.DOB = value?.DateTime;
                    OnPropertyChanged(nameof(User));
                    OnPropertyChanged(nameof(DOBForDatePicker));
                }
            }
        }

        public async void LoadUserDataAsync(string userID)
        {
            try
            {
                using (SqlConnection connection = DatabaseConnection.GetConnection())
                {
                    await connection.OpenAsync();
                    string query = "SELECT Name, Email, Phone, Avatar, DOB FROM [User] WHERE User_ID = @UserID";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userID);

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                            {
                                
                                User = new User
                                {
                                    Name = reader["Name"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    Phone = reader["Phone"].ToString(),
                                    Avatar = reader["Avatar"].ToString(),
                                    DOB = reader["DOB"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["DOB"]
                                };
                                OnPropertyChanged(nameof(DOBForDatePicker));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading user data: {ex.Message}");
            }
        }

        private async void UpdateUserDataAsync()
        {
            try
            {
                using (SqlConnection connection = DatabaseConnection.GetConnection())
                {
                    await connection.OpenAsync();
                    string query = "UPDATE [User] SET Name = @Name, Email = @Email, Phone = @Phone, Avatar = @Avatar, DOB = @DOB WHERE User_ID = @UserID";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        // Use User properties for updating
                        cmd.Parameters.AddWithValue("@Name", User.Name);
                        cmd.Parameters.AddWithValue("@Email", User.Email);
                        cmd.Parameters.AddWithValue("@Phone", User.Phone);
                        cmd.Parameters.AddWithValue("@Avatar", User.Avatar);
                        cmd.Parameters.AddWithValue("@DOB", User.DOB ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@UserID", UserSession.CurrentUserID);

                        int rowsAffected = await cmd.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            // Notify the UI that the update was successful
                            UpdateSuccess?.Invoke(this, EventArgs.Empty);
                        }
                        else
                        {
                            // Optionally, handle the case where no rows were affected (e.g., no matching user found)
                            Console.WriteLine("No rows updated.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions appropriately
                Console.WriteLine($"Error updating user data: {ex.Message}");
            }
        }
    }
}
