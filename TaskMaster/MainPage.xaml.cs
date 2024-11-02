using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Data.SqlClient;
using System.Drawing;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TaskMaster
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        // Xử lý khi người dùng nhấn vào My Project
        public void showTaskContainer()
        {
            Mytask_Container.Visibility = Visibility.Visible;
            Myproject_Container.Visibility = Visibility.Collapsed;
        }
        public void showProjectContainer()
        {
            Myproject_Container.Visibility = Visibility.Visible;
            Mytask_Container.Visibility = Visibility.Collapsed;
            LoadUserProjects();
        }
        // Xử lý khi người dùng nhấn vào My Project
        private void MyProjectButton_Click(object sender, RoutedEventArgs e)
        {
            showProjectContainer();
        }

        private void LoadUserProjects()
        {
            ProjectList.Children.Clear();
            string currentUsername = UserSession.CurrentUsername;
            try
            {
                using (SqlConnection connection = new SqlConnection("Server=MSI\\SQLEXPRESS;Database=TaskMaster;Trusted_Connection=True;TrustServerCertificate=True;"))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(@"SELECT p.Name 
                                                                        FROM Project p
                                                                        JOIN Member m ON p.Project_ID = m.Project_ID
                                                                        JOIN [User] u ON m.User_ID = u.User_ID
                                                                        WHERE u.Username = @Username", connection))
                    {
                        command.Parameters.AddWithValue("@Username", currentUsername);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string projectName = reader.GetString(0);
                                Button projectButton = new Button
                                {
                                    Content = projectName,
                                    Style = (Style)Application.Current.Resources["CustomProjectIconStyle"],
                                };
                                projectButton.Click += ProjectView_Click;
                                ProjectList.Children.Add(projectButton);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                StatusTextBox.Text = "Có lỗi xảy ra khi tải dự án: " + ex.Message;
                StatusTextBox.Visibility = Visibility.Visible;
            }
        }

        private void MyTaskButton_Click(object sender, RoutedEventArgs e)
        {
            showTaskContainer();
        }
        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            AccountPopup.IsOpen = !AccountPopup.IsOpen;

        }
        private void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            AccountPopup.IsOpen = false;
            Frame.Navigate(typeof(LoginPage));
        }
        private void NotificationButton_Click(object sender, RoutedEventArgs e)
        {

        }
        private void CreateProjectButton_Click(object sender, RoutedEventArgs e)
        {
            CreateProjectField.IsOpen = true;
            JoinProjectField.IsOpen = false;

        }
        private void CreateProjectButton_QuitClick(object sender, RoutedEventArgs e)
        {
            CreateProjectField.IsOpen = false;

        }
        private void SaveNewProject_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CreateProject_Name.Text))
            {
                StatusTextBox.Text = "Tên dự án không được để trống!";
                StatusTextBox.Visibility = Visibility.Visible;
                return;
            }
            string errorMessage = SaveNewProject(CreateProject_Name.Text);

            if (string.IsNullOrEmpty(errorMessage))
            {
                CreateProjectField.IsOpen = false;
                Button newProject = new Button
                {
                    Content = CreateProject_Name.Text,
                    Style = (Style)Application.Current.Resources["CustomProjectIconStyle"],
                };
                newProject.Click += ProjectView_Click;
                ProjectList.Children.Add(newProject);
                StatusTextBox.Text = "Dự án đã được tạo thành công!";
                StatusTextBox.Visibility = Visibility.Visible;
            }
            else
            {
                StatusTextBox.Text = errorMessage;
                StatusTextBox.Visibility = Visibility.Visible;
            }
        }

        private string SaveNewProject(string projectName)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection("Server=MSI\\SQLEXPRESS;Database=TaskMaster;Trusted_Connection=True;TrustServerCertificate=True;"))
                {
                    connection.Open();

                    using (SqlCommand checkCommand = new SqlCommand("SELECT COUNT(*) FROM Project WHERE Name = @Name", connection))
                    {
                        checkCommand.Parameters.AddWithValue("@Name", projectName);
                        int count = (int)checkCommand.ExecuteScalar();

                        if (count > 0)
                        {
                            return "Tên dự án đã tồn tại, vui lòng chọn tên khác.";
                        }
                    }
                    string projectId = GenerateProjectId(connection);

                    using (SqlCommand command = new SqlCommand("INSERT INTO Project (Project_ID, Name, StartDate) VALUES (@ProjectId, @Name, @StartDate)", connection))
                    {
                        command.Parameters.AddWithValue("@ProjectId", projectId);
                        command.Parameters.AddWithValue("@Name", projectName);
                        command.Parameters.AddWithValue("@StartDate", DateTime.Now);
                        command.ExecuteNonQuery();
                    }
                    string currentUsername = UserSession.CurrentUsername;
                    string userId;
                    using (SqlCommand userCommand = new SqlCommand("SELECT User_ID FROM [User] WHERE Username = @Username", connection))
                    {
                        userCommand.Parameters.AddWithValue("@Username", currentUsername);
                        object userResult = userCommand.ExecuteScalar();
                        if (userResult == null)
                        {
                            return "Người dùng không tồn tại.";
                        }
                        userId = userResult as string;
                    }

                    using (SqlCommand memberCommand = new SqlCommand("INSERT INTO Member (User_ID, Project_ID, Role, JoinDate) VALUES (@UserId, @ProjectId, @Role, @JoinDate)", connection))
                    {
                        memberCommand.Parameters.AddWithValue("@UserId", userId);
                        memberCommand.Parameters.AddWithValue("@ProjectId", projectId);
                        memberCommand.Parameters.AddWithValue("@Role", "Trưởng nhóm");
                        memberCommand.Parameters.AddWithValue("@JoinDate", DateTime.Now);
                        memberCommand.ExecuteNonQuery();
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return "Có lỗi xảy ra khi lưu dự án: " + ex.Message;
            }
        }

        private string GenerateProjectId(SqlConnection connection)
        {
            string newProjectId;
            using (SqlCommand command = new SqlCommand("SELECT TOP 1 Project_ID FROM Project ORDER BY Project_ID DESC", connection))
            {
                object result = command.ExecuteScalar();
                int nextId = 1;

                if (result != null)
                {
                    string lastId = result.ToString();
                    if (lastId.StartsWith("Pr") && lastId.Length > 2)
                    {
                        string numberPart = lastId.Substring(2);
                        if (int.TryParse(numberPart, out int currentId))
                        {
                            nextId = currentId + 1;
                        }
                    }
                }
                newProjectId = "Pr" + nextId.ToString("D8");
            }
            return newProjectId;
        }

        private void ProjectView_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Project));
            string projectName = (sender as Button).Content.ToString();
            UserSession.CurrentProjectName = projectName;
            using (SqlConnection connection = new SqlConnection("Server=MSI\\SQLEXPRESS;Database=TaskMaster;Trusted_Connection=True;TrustServerCertificate=True;"))
            {
                connection.Open();
                
                using (SqlCommand command = new SqlCommand("SELECT Project_ID FROM Project WHERE Name = @Name", connection))
                {
                    command.Parameters.AddWithValue("@Name", projectName);
                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        string projectID = result.ToString();
                        UserSession.CurrentProjectID = projectID;
                    }
                }
            }
        }

        private void JoinProjectButton_Click(Object sender, RoutedEventArgs e)
        {
            JoinProjectField.IsOpen = true;
            CreateProjectField.IsOpen = false;
        }
        private void JoinProjectButton_QuitClick(object sender, RoutedEventArgs e)
        {
            JoinProjectField.IsOpen = false;
        }
        private void SaveExistingProject_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(JoinProject_Code.Text))
            {
                StatusTextBox2.Text = "Mã dự án không được để trống!";
                StatusTextBox2.Visibility = Visibility.Visible;
                return;
            }
            string errorMessage = AddMemberToProject(JoinProject_Code.Text);

            if (string.IsNullOrEmpty(errorMessage))
            {
                JoinProjectField.IsOpen = false;
                Button existingProject = new Button
                {
                    Content = JoinProject_Code.Text,
                    Style = (Style)Application.Current.Resources["CustomProjectIconStyle"],
                    Background = new SolidColorBrush(Microsoft.UI.Colors.Purple)
                };
                existingProject.Click += ProjectView_Click;
                ProjectList.Children.Add(existingProject);
                StatusTextBox2.Text = "Bạn đã tham gia dự án thành công!";
                StatusTextBox2.Visibility = Visibility.Visible;
            }
            else
            {
                StatusTextBox2.Text = errorMessage;
                StatusTextBox2.Visibility = Visibility.Visible;
            }
        }

        private string AddMemberToProject(string projectCode)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection("Server=MSI\\SQLEXPRESS;Database=TaskMaster;Trusted_Connection=True;TrustServerCertificate=True;"))
                {
                    connection.Open();
                    string projectId;
                    using (SqlCommand checkCommand = new SqlCommand("SELECT Project_ID FROM Project WHERE Name = @Code", connection))
                    {
                        checkCommand.Parameters.AddWithValue("@Code", projectCode);
                        object result = checkCommand.ExecuteScalar();

                        if (result == null)
                        {
                            return "Mã dự án không tồn tại, vui lòng kiểm tra lại.";
                        }
                        projectId = result as string;
                    }
                    string userId;
                    string currentUsername = UserSession.CurrentUsername;
                    using (SqlCommand userCommand = new SqlCommand("SELECT User_ID FROM [User] WHERE Username = @Username", connection))
                    {
                        userCommand.Parameters.AddWithValue("@Username", currentUsername);
                        object userResult = userCommand.ExecuteScalar();

                        if (userResult == null)
                        {
                            return "Người dùng không tồn tại.";
                        }
                        userId = userResult as string;
                    }
                    using (SqlCommand memberCheckCommand = new SqlCommand("SELECT COUNT(*) FROM Member WHERE User_ID = @UserId AND Project_ID = @ProjectId", connection))
                    {
                        memberCheckCommand.Parameters.AddWithValue("@UserId", userId);
                        memberCheckCommand.Parameters.AddWithValue("@ProjectId", projectId);
                        int count = (int)memberCheckCommand.ExecuteScalar();
                        if (count > 0)
                        {
                            return "Bạn đã tham gia dự án này rồi.";
                        }
                    }
                    using (SqlCommand command = new SqlCommand("INSERT INTO Member (User_ID, Project_ID, Role, JoinDate) VALUES (@UserId, @ProjectId, @Role, @JoinDate)", connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@ProjectId", projectId);
                        command.Parameters.AddWithValue("@Role", "Thành viên");
                        command.Parameters.AddWithValue("@JoinDate", DateTime.Now);
                        command.ExecuteNonQuery();
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return "Có lỗi xảy ra khi tham gia dự án: " + ex.Message;
            }
        }


        private void Addnewtask_Click(object sender, RoutedEventArgs e)
        {
            Grid taskGrid = new Grid
            {
                Style = (Style)Application.Current.Resources["CustomTaskGridStyle"]
            };
            taskGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            taskGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            taskGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            taskGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            CheckBox checkbox = new CheckBox
            {
                Content = "Do something"
            };
            Grid.SetColumn(checkbox, 0);
            taskGrid.Children.Add(checkbox);

            TextBlock sourceTask = new TextBlock
            {
                Text = "Project B"
            };
            Grid.SetColumn(sourceTask, 1);
            taskGrid.Children.Add(sourceTask);

            TextBlock start = new TextBlock
            {
                Text = "02/02/2024"
            };
            Grid.SetColumn(start, 2);
            taskGrid.Children.Add(start);

            TextBlock due = new TextBlock
            {
                Text = "20/02/2024"
            };
            Grid.SetColumn(due, 3);
            taskGrid.Children.Add(due);

            TaskList.Children.Add(taskGrid);
        }
    }
}
