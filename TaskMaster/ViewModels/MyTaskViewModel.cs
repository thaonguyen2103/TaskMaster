using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMaster.Models;
using TaskMaster.Helpers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Data.SqlClient;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using System.Windows.Input;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Windows;

namespace TaskMaster.ViewModels
{
    public class MyTaskViewModel : BaseViewModel
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
                }
            }
        }
        private ObservableCollection<UserTask> _userTasks;
        public ObservableCollection<UserTask> UserTasks
        {
            get => _userTasks;
            set
            {
                _userTasks = value;
                OnPropertyChanged(nameof(UserTasks));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public RelayCommand AccountButton_Click { get; }
        public RelayCommand ProfileOpenCommand { get; }
        public RelayCommand LogOutCommand { get; }
        public RelayCommand NavigateToMainPageCommand { get; }
        public RelayCommand AllUserTaskCommand { get; }
        public RelayCommand PrivateUserTaskCommand { get; }
        public RelayCommand AssignedUserTaskCommand { get; }
        public RelayCommand<String> NavigateFromMyTaskToProjectCommand { get; }
        public MyTaskViewModel()
        {
            UserTasks = new ObservableCollection<UserTask>();
            Task task = LoadUserAvatar(UserSession.CurrentUserID);
            Task task2 = LoadUserTask();
            AccountButton_Click = new RelayCommand(ToggleMenu);
            ProfileOpenCommand = new RelayCommand(ProfileOpen);
            LogOutCommand = new RelayCommand(LogOut_Click);
            NavigateToMainPageCommand = new RelayCommand(NavigateToMainPage);
            AllUserTaskCommand = new RelayCommand(AllUserTask);
            PrivateUserTaskCommand = new RelayCommand(PrivateUserTask);
            AssignedUserTaskCommand = new RelayCommand(AssignedUserTask);
            NavigateFromMyTaskToProjectCommand = new RelayCommand<String>(NavigateFromMyTaskToProject);
        }
        public Action NavigateToProject {  get; set; }
        private void NavigateFromMyTaskToProject(String projectName)
        {
            if (string.IsNullOrWhiteSpace(projectName) || projectName == "Me")
            {
                return;
            }
            UserSession.CurrentProjectName = projectName;
            NavigateToProject?.Invoke();
        }

        private void AllUserTask()
        {
            Task task = LoadUserTask();
        }
        private async void PrivateUserTask()
        {
            try
            {
                using (SqlConnection connection = DatabaseConnection.GetConnection())
                {
                    await connection.OpenAsync();
                    string query = "SELECT a.title, a.source, t.startdate, t.duedate " +
                                   "FROM Task t " +
                                   "RIGHT JOIN Assignment a ON a.Task_ID = t.Task_ID " +
                                   "WHERE a.User_ID = @UserID AND a.Source = 'Me'";

                    using (SqlCommand cmd1 = new SqlCommand(query, connection))
                    {
                        cmd1.Parameters.AddWithValue("@UserID", UserSession.CurrentUserID);
                        using (SqlDataReader reader = await cmd1.ExecuteReaderAsync())
                        {
                            UserTasks.Clear();
                            while (await reader.ReadAsync())
                            {
                                var startDate = reader["startdate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["startdate"]).Date;
                                var dueDate = reader["duedate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["duedate"]).Date;

                                UserTasks.Add(new UserTask
                                {
                                    Title = reader["title"].ToString(),
                                    Source = reader["source"].ToString(),
                                    StartDate = startDate,
                                    DueDate = dueDate
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // You can log or handle the exception here
                Console.WriteLine("Error loading user tasks: " + ex.Message);
            }
        }
        private async void AssignedUserTask()
        {
            try
            {
                using (SqlConnection connection = DatabaseConnection.GetConnection())
                {
                    await connection.OpenAsync();
                    string query = "SELECT a.title, a.source, t.startdate, t.duedate " +
                                   "FROM Task t " +
                                   "RIGHT JOIN Assignment a ON a.Task_ID = t.Task_ID " +
                                   "WHERE a.User_ID = @UserID AND a.Source != 'Me'";

                    using (SqlCommand cmd1 = new SqlCommand(query, connection))
                    {
                        cmd1.Parameters.AddWithValue("@UserID", UserSession.CurrentUserID);
                        using (SqlDataReader reader = await cmd1.ExecuteReaderAsync())
                        {
                            UserTasks.Clear();
                            while (await reader.ReadAsync())
                            {
                                var startDate = reader["startdate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["startdate"]).Date;
                                var dueDate = reader["duedate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["duedate"]).Date;

                                UserTasks.Add(new UserTask
                                {
                                    Title = reader["title"].ToString(),
                                    Source = reader["source"].ToString(),
                                    StartDate = startDate,
                                    DueDate = dueDate
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // You can log or handle the exception here
                Console.WriteLine("Error loading user tasks: " + ex.Message);
            }
        }

        public Action NavigateMainPage { get; set; }
        private void NavigateToMainPage()
        {
            NavigateMainPage?.Invoke();
        }

        public async Task LoadUserTask()
        {
            try
            {
                using (SqlConnection connection = DatabaseConnection.GetConnection())
                {
                    await connection.OpenAsync();
                    string query = "SELECT a.title, a.source, t.startdate, t.duedate " +
                                   "FROM Task t " +
                                   "RIGHT JOIN Assignment a ON a.Task_ID = t.Task_ID " +
                                   "WHERE a.User_ID = @UserID";

                    using (SqlCommand cmd1 = new SqlCommand(query, connection))
                    {
                        cmd1.Parameters.AddWithValue("@UserID", UserSession.CurrentUserID);
                        using (SqlDataReader reader = await cmd1.ExecuteReaderAsync())
                        {
                            UserTasks.Clear();
                            while (await reader.ReadAsync())
                            {
                                var startDate = reader["startdate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["startdate"]).Date;
                                var dueDate = reader["duedate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["duedate"]).Date;

                                UserTasks.Add(new UserTask
                                {
                                    Title = reader["title"].ToString(),
                                    Source = reader["source"].ToString(),
                                    StartDate = startDate,
                                    DueDate = dueDate
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // You can log or handle the exception here
                Console.WriteLine("Error loading user tasks: " + ex.Message);
            }
        }


        // Phương thức tải avatar người dùng
        public async Task LoadUserAvatar(string userID)
        {
            try
            {
                using (SqlConnection connection = DatabaseConnection.GetConnection())
                {
                    await connection.OpenAsync();
                    string query = "SELECT Avatar FROM [User] WHERE User_ID = @UserID";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userID);

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                            {
                                if (User == null)
                                {
                                    User = new User();
                                }

                                User.Avatar = reader["Avatar"].ToString();
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
        
        // Phương thức mở menu
        public Action Toggle { get; set; }
        private void ToggleMenu()
        {
            Toggle?.Invoke();
        }

        // Phương thức đăng xuất
        public Action LogOut { get; set; }
        private void LogOut_Click()
        {
            LogOut?.Invoke();
        }

        // Phương thức mở tài khoản người dùng
        public Action NavigateMyAccount { get; set; }
        private void ProfileOpen()
        {
            NavigateMyAccount?.Invoke();
        }
    }
}
