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
using Azure;

namespace TaskMaster.ViewModels
{
    public class MainPageViewModel : BaseViewModel
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
        private string _projectname;
        public string ProjectName
        {
            get => _projectname;
            set
            {
                _projectname = value;
                OnPropertyChanged();
                SaveNewProjectCommand.RaiseCanExecuteChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private ObservableCollection<Project> _projects; 
        public ObservableCollection<Project> Projects
        {
            get => _projects;
            set
            {
                _projects = value;
                OnPropertyChanged(nameof(Projects));
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

        private string _selectedColor;

        public string SelectedColor
        {
            get => _selectedColor;
            set
            {
                _selectedColor = value;
                OnPropertyChanged(); 
            }
        }

        //Join project
        private string _joinProjectCode;
        private string _joinProjectPassword;

        public string JoinProjectCode
        {
            get => _joinProjectCode;
            set
            {
                if (_joinProjectCode != value)
                {
                    _joinProjectCode = value;
                    OnPropertyChanged(nameof(JoinProjectCode));
                }
            }
        }

        public string JoinProjectPassword
        {
            get => _joinProjectPassword;
            set
            {
                if (_joinProjectPassword != value)
                {
                    _joinProjectPassword = value;
                    OnPropertyChanged(nameof(JoinProjectPassword));
                }
            }
        }
        private string _JoinStatusTextBox;
        private bool _isJoinError;

        public string JoinStatusTextBox
        {
            get => _JoinStatusTextBox;
            set { _JoinStatusTextBox = value; OnPropertyChanged(); }
        }

        public bool isJoinError
        {
            get => _isJoinError;
            set { _isJoinError = value; OnPropertyChanged(); }
        }

        private bool _isOpenJoinPopUp;
        public bool isOpenJoinPopUp
        {
            get => _isOpenJoinPopUp;
            set { _isOpenJoinPopUp = value; OnPropertyChanged(); }
        }

        private bool _isOpenCreateProject;
        public bool isOpenCreateProject
        {
            get => _isOpenCreateProject;
            set { _isOpenCreateProject = value; OnPropertyChanged(); }
        }
        public RelayCommand JoinProjectCommand { get; }
        public RelayCommand SaveProjectCommand { get; }
        public RelayCommand AccountButton_Click { get; }
        public RelayCommand ProfileOpenCommand { get; }
        public RelayCommand LogOutCommand { get; }
        public RelayCommand ShowTaskCommand { get; }
        public RelayCommand ShowProjectCommand { get; }
        public RelayCommand OpenProjectCreationPopUpCommand { get; }
        public RelayCommand SaveNewProjectCommand { get; }
        public RelayCommand CloseCreateProjectCommand { get; }
        public RelayCommand<String> ChooseColorCommand { get; }
        public RelayCommand<String> ViewProjectCommand { get; }
        public RelayCommand NavigateToMyTaskCommand {  get; }
        public RelayCommand CreateProjectButton_QuitClick { get; }
        public RelayCommand JoinProjectButton_QuitClick { get; }
        public MainPageViewModel()
        {
            isOpenCreateProject = false;
            isOpenJoinPopUp = false;
            Task task = LoadUserAvatar(UserSession.CurrentUserID);
            Task task2 = LoadUserProjects(UserSession.CurrentUserID);
            Projects = new ObservableCollection<Project>(); 
            AccountButton_Click = new RelayCommand(ToggleMenu);
            ProfileOpenCommand = new RelayCommand(ProfileOpen);
            LogOutCommand = new RelayCommand(LogOut_Click);
            OpenProjectCreationPopUpCommand = new RelayCommand(OpenProjectCreationPopUp);
            ChooseColorCommand = new RelayCommand<string>(OnChooseColor);
            SaveNewProjectCommand = new RelayCommand(SaveNewProject);
            ViewProjectCommand = new RelayCommand<String>(ViewProject);
            NavigateToMyTaskCommand = new RelayCommand(NavigateToMyTask);
            JoinProjectCommand = new RelayCommand(OpenJoinProjectPopup);
            SaveProjectCommand = new RelayCommand(SaveExistingProject);
            CreateProjectButton_QuitClick = new RelayCommand(QuitCreateProject);
            JoinProjectButton_QuitClick = new RelayCommand(QuitJoinProject);
        }

        private void QuitJoinProject()
        {
            isOpenJoinPopUp = false;
        }

        private void QuitCreateProject()
        {
            isOpenCreateProject = false;
        }

        private void OpenJoinProjectPopup()
        {
            isJoinError = false;
            JoinProjectCode = "";
            JoinProjectPassword = "";
            isOpenJoinPopUp = true;
            isOpenCreateProject = false;
        }
        private async void SaveExistingProject()
        {
            if (string.IsNullOrWhiteSpace(JoinProjectCode) || string.IsNullOrWhiteSpace(JoinProjectPassword))
            {
                JoinStatusTextBox = "Mã dự án và mật khẩu không được để trống!";
                isJoinError = true;
                return;
            }

            // Gọi AddMemberToProject và lấy về thông báo lỗi
            string errorMessage = AddMemberToProject(JoinProjectCode, JoinProjectPassword);

            if (string.IsNullOrEmpty(errorMessage))
            {
                JoinStatusTextBox = "Bạn đã tham gia dự án thành công!";
                isJoinError = true;
                isOpenJoinPopUp = false; // Đóng popup khi thành công
                await LoadUserProjects(UserSession.CurrentUserID); // Refresh danh sách dự án
                UpdateProjectList?.Invoke();
            }
            else
            {
                JoinStatusTextBox = errorMessage;
                isJoinError = true;
            }
        }

        private string AddMemberToProject(string projectCode, string password)
        {
            try
            {
                using (SqlConnection connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    string projectId;

                    // Kiểm tra project có tồn tại và mật khẩu có đúng không
                    using (SqlCommand checkCommand = new SqlCommand("SELECT Project_ID, Password FROM Project WHERE Project_ID = @Code", connection))
                    {
                        checkCommand.Parameters.AddWithValue("@Code", projectCode);
                        using (SqlDataReader reader = checkCommand.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                return "Mã dự án không tồn tại, vui lòng kiểm tra lại.";
                            }

                            projectId = reader["Project_ID"].ToString();
                            string storedPassword = reader["Password"].ToString();
                            if (storedPassword != password)
                            {
                                return "Mật khẩu không chính xác, vui lòng kiểm tra lại.";
                            }
                        }
                    }

                    // Kiểm tra xem user đã join project này chưa
                    string userId = UserSession.CurrentUserID;
                    using (SqlCommand memberCheckCommand = new SqlCommand(
                        "SELECT COUNT(*) FROM Member WHERE User_ID = @UserId AND Project_ID = @ProjectId",
                        connection))
                    {
                        memberCheckCommand.Parameters.AddWithValue("@UserId", userId);
                        memberCheckCommand.Parameters.AddWithValue("@ProjectId", projectId);
                        int count = (int)memberCheckCommand.ExecuteScalar();
                        if (count > 0)
                        {
                            return "Bạn đã tham gia dự án này rồi.";
                        }
                    }

                    // Thêm thành viên vào project
                    using (SqlCommand command = new SqlCommand(
                        "INSERT INTO Member (User_ID, Project_ID, Role, JoinDate) VALUES (@UserId, @ProjectId, @Role, @JoinDate)",
                        connection))
                    {
                        command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@ProjectId", projectId);
                        command.Parameters.AddWithValue("@Role", "Thành viên");
                        command.Parameters.AddWithValue("@JoinDate", DateTime.Now);
                        command.ExecuteNonQuery();
                    }

                    return string.Empty; // Không có lỗi
                }
            }
            catch (Exception ex)
            {
                return "Có lỗi xảy ra khi tham gia dự án: " + ex.Message;
            }
        }
        private void OnChooseColor(string tag)
        {
            if (!string.IsNullOrEmpty(tag))
            {
                SelectedColor = tag;
            }
            else
            {
                SelectedColor = "#FFFFFF"; 
            }
        }

        public Action NavigateMyTask { get; set; }
        private void NavigateToMyTask()
        {
            NavigateMyTask?.Invoke();
        }

        public Action NavigateToProject {  get; set; }
        private void ViewProject(string projectName)
        {
            if (string.IsNullOrWhiteSpace(projectName))
            {
                Console.WriteLine("Project name is null or empty.");
                return;
            }
            UserSession.CurrentProjectName = projectName;
            NavigateToProject?.Invoke();
        }

        public Action ChooseColorAnimation { get; set; }
        private void ChooseProjectColor()
        {
            ChooseColorAnimation?.Invoke();
        }
        public Action UpdateProjectList { get; set; }
        private async void SaveNewProject()
        {
            string trimmedProjectName = ProjectName?.Trim();

            if (string.IsNullOrWhiteSpace(trimmedProjectName))
            {
                if (string.IsNullOrWhiteSpace(ProjectName))
                {
                    StatusMessage = "Tên dự án không được để trống!";
                    IsErrorVisible = true;
                    return;
                }
            }
            using (SqlConnection connection = DatabaseConnection.GetConnection())
            {
                await connection.OpenAsync();
                using (SqlCommand userCommand = new SqlCommand(UserSession.CurrentUserID, connection))
                {
                    
                    using (SqlCommand checkCommand = new SqlCommand("SELECT COUNT(*) FROM Project WHERE Name = @Name AND Project_ID IN (SELECT Project_ID FROM Member WHERE User_ID = @User_ID);", connection))
                    {
                        checkCommand.Parameters.AddWithValue("@Name", trimmedProjectName);
                        checkCommand.Parameters.AddWithValue("@User_ID", UserSession.CurrentUserID);

                        int count = (int)checkCommand.ExecuteScalar();

                        if (count > 0)
                        {
                            StatusMessage = "Tên dự án đã tồn tại, vui lòng chọn tên khác.";
                            IsErrorVisible = true;
                            return;
                        }
                    }
                }
                if (string.IsNullOrEmpty(SelectedColor))
                {
                    SelectedColor = "#FFFFFF"; 
                }
                string projectId = GenerateProjectId(connection);
                string projectPassword = GenerateProjectPassword();
                using (SqlCommand command = new SqlCommand("INSERT INTO Project (Project_ID, Name, StartDate, Password, Color) VALUES (@ProjectId, @Name, @StartDate, @Password, @Color)", connection))
                {
                    command.Parameters.AddWithValue("@ProjectId", projectId);
                    command.Parameters.AddWithValue("@Name", trimmedProjectName);
                    command.Parameters.AddWithValue("@StartDate", DateTime.Now);
                    command.Parameters.AddWithValue("@Password", projectPassword);
                    command.Parameters.AddWithValue("@Color", SelectedColor);
                    command.ExecuteNonQuery();
                }
                using (SqlCommand memberCommand = new SqlCommand("INSERT INTO Member (User_ID, Project_ID, Role, JoinDate) VALUES (@UserId, @ProjectId, @Role, @JoinDate)", connection))
                {
                    memberCommand.Parameters.AddWithValue("@UserId", UserSession.CurrentUserID);
                    memberCommand.Parameters.AddWithValue("@ProjectId", projectId);
                    memberCommand.Parameters.AddWithValue("@Role", "Quản trị viên");
                    memberCommand.Parameters.AddWithValue("@JoinDate", DateTime.Now);
                    memberCommand.ExecuteNonQuery();
                }
                StatusMessage = "Lưu thành công";
                IsErrorVisible = true;
                UpdateProjectList?.Invoke();
            }
        }
        private string GenerateProjectPassword()
        {
            Random random = new Random();
            char[] password = new char[6];

            for (int i = 0; i < 6; i++)
            {
                int asciiRange = random.Next(3); 
                if (asciiRange == 0)
                    password[i] = (char)random.Next(48, 58); 
                else if (asciiRange == 1)
                    password[i] = (char)random.Next(65, 91); 
                else
                    password[i] = (char)random.Next(97, 123);
            }
            return new string(password);
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

        public Action ProjectCreationPopUp { get; set; }
        private void OpenProjectCreationPopUp()
        {
            IsErrorVisible = false;
            isOpenJoinPopUp = false;
            isOpenCreateProject = true;
        }

        // Phương thức tải avatar người dùng
        public async System.Threading.Tasks.Task LoadUserAvatar(string userID)
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
        public async System.Threading.Tasks.Task LoadUserProjects(string userID)
        {
            try
            {
                using (SqlConnection connection = DatabaseConnection.GetConnection())
                {
                    await connection.OpenAsync();
                    string query = "SELECT p.Name, p.Color FROM Project p JOIN Member m ON p.Project_ID = m.Project_ID JOIN [User] u ON m.User_ID = u.User_ID WHERE u.User_ID = @UserID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UserID", userID);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                string projectName = reader.GetString(0);
                                string projectColor = reader.GetString(1);
                                Projects.Add(new Project { Name = projectName,
                                Color = projectColor});
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading user projects: {ex.Message}");
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
