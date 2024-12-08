using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.UI.Xaml.Controls;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TaskMaster.Models;
using Windows.ApplicationModel.DataTransfer;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
namespace TaskMaster.ViewModels 
{
    public class ProjectViewModel : BaseViewModel
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
        private Project _project;

        public Project Project
        {
            get { return _project; }
            set
            {
                if (_project != value)
                {
                    _project = value;
                    OnPropertyChanged(nameof(Project));
                }
            }
        }
        private DateTime? _startDate;
        public DateTimeOffset? StartDateForDatePicker
        {
            get => _startDate.HasValue ? (DateTimeOffset?)_startDate.Value : null;
            set
            {
                if (_startDate != value?.DateTime)
                {
                    _startDate = value?.DateTime;
                    if (Task != null)
                    {
                        Task.StartDate = _startDate;
                    }
                    OnPropertyChanged(nameof(StartDateForDatePicker));
                }
            }
        }
        private DateTime? _dueDate;
        public DateTimeOffset? DueDateForDatePicker
        {
            get => _dueDate.HasValue ? (DateTimeOffset?)_dueDate.Value : null;
            set
            {
                if (_dueDate != value?.DateTime)
                {
                    _dueDate = value?.DateTime;
                    if (Task != null)
                    {
                        Task.DueDate = _dueDate;
                    }
                    OnPropertyChanged(nameof(DueDateForDatePicker));
                }
            }
        }
        private _Task _task;
        public _Task Task
        {
            get { return _task; }
            set
            {
                if (_task != value)
                {
                    _task = value;
                    _startDate = _task?.StartDate;
                    _dueDate = _task?.DueDate;
                    OnPropertyChanged(nameof(Task));
                    OnPropertyChanged(nameof(StartDateForDatePicker));
                    OnPropertyChanged(nameof(DueDateForDatePicker));
                }
            }
        }
        private string _taskName;
        public string TaskName
        {
            get => _taskName;
            set
            {
                if (_taskName != value)
                {
                    _taskName = value;
                    if (Task != null)
                    {
                        Task.Content = _taskName;
                    }
                    OnPropertyChanged(nameof(TaskName));
                }
            }
        }
        private string _copyStatus;
        public string CopyStatus
        {
            get => _copyStatus;
            set
            {
                if (_copyStatus != value)
                {
                    _copyStatus = value;
                    if (Task != null)
                    {
                        Task.Content = _copyStatus;
                    }
                    OnPropertyChanged(nameof(CopyStatus));
                }
            }
        }
        // Thuộc tính PriorityIndex để binding với ComboBox của Priority
        private int _priorityIndex;
        public int PriorityIndex
        {
            get => _priorityIndex;
            set
            {
                if (_priorityIndex != value)
                {
                    _priorityIndex = value;
                    OnPropertyChanged(nameof(PriorityIndex));
                }
            }
        }

        // Thuộc tính StatusIndex để binding với ComboBox của Status
        private int _statusIndex;
        public int StatusIndex
        {
            get => _statusIndex;
            set
            {
                if (_statusIndex != value)
                {
                    _statusIndex = value;
                    OnPropertyChanged(nameof(StatusIndex));
                }
            }
        }


        // Description property for two-way binding
        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    if (Task != null)
                    {
                        Task.Description = _description;
                    }
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        private ObservableCollection<User> _users;
        public ObservableCollection<User> Users
        {
            get => _users;
            set
            {
                _users = value;
                OnPropertyChanged(nameof(Users));
            }
        }
        //Danh sách các nhãn
        private ObservableCollection<Label> _labels;
        public ObservableCollection<Label> Labels
        {
            get => _labels;
            set
            {
                _labels = value;
                OnPropertyChanged(nameof(Labels));
            }
        }
        private string _projectName;
        public string ProjectName
        {
            get => _projectName;
            set { _projectName = value; OnPropertyChanged(); }
        }

        private string _listName;
        public string ListName
        {
            get => _listName;
            set { _listName = value; 
                OnPropertyChanged();
                SaveNewListCommand.RaiseCanExecuteChanged();
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

        //Thuộc tính IsOpen của PopUp filter
        private bool _isFilterOpen;
        public bool IsFilterOpen
        {
            get => _isFilterOpen;
            set { _isFilterOpen = value; OnPropertyChanged(); }
        }
        //Thuộc tính IsOpen của PopUp share
        private bool _isShareOpen;
        public bool IsShareOpen
        {
            get => _isShareOpen;
            set { _isShareOpen = value; OnPropertyChanged(); }
        }

        //Thuộc tính Text của thanh tìm kiếm
        private string _searchKeyWord;
        public string SearchKeyWord
        {
            get => _searchKeyWord;
            set { _searchKeyWord = value; OnPropertyChanged(); }
        }

        private ObservableCollection<Label> _selectedLabels;
        public ObservableCollection<Label> SelectedLabels
        {
            get { return _selectedLabels; }
            set
            {
                _selectedLabels = value;
                OnPropertyChanged(nameof(SelectedLabels));
            }
        }
        private ObservableCollection<User> _selectedAssignments;
        public ObservableCollection<User> SelectedAssignments
        {
            get { return _selectedAssignments; }
            set
            {
                _selectedAssignments = value;
                OnPropertyChanged(nameof(SelectedAssignments));
            }
        }

        public RelayCommand<User> RemoveAssignCommand { get; set; }
        public RelayCommand<Label> RemoveLabelCommand { get; }
        public RelayCommand OpenFilterPopUpCommand { get; }
        public RelayCommand OpenSharePopUpCommand { get; }
        public RelayCommand AccountButton_Click { get; }
        public RelayCommand ProfileOpenCommand { get; }
        public RelayCommand LogOutCommand { get; }
        public RelayCommand AddListCommand { get; }
        public RelayCommand SaveNewListCommand {  get; }
        public RelayCommand SearchCommand { get; }
        public RelayCommand ApplyFilterCommand { get; }
        public RelayCommand NavigateToMainPageCommand { get; }
        public RelayCommand NavigateToMyTaskCommand { get; }
        public RelayCommand CopyProjectCommand { get; }
        public RelayCommand CancelAddListCommand { get; }
        public ProjectViewModel()
        {
            Users = new ObservableCollection<User>();
            Labels = new ObservableCollection<Label>();
            SelectedLabels = new ObservableCollection<Label>();
            SelectedAssignments = new ObservableCollection<User>();
            AccountButton_Click = new RelayCommand(ToggleMenu);
            ProfileOpenCommand = new RelayCommand(ProfileOpen);
            LogOutCommand = new RelayCommand(LogOut_Click);
            AddListCommand = new RelayCommand(AddList);
            SaveNewListCommand = new RelayCommand(SaveNewList);
            OpenFilterPopUpCommand = new RelayCommand(OpenFilterPopUp);
            OpenSharePopUpCommand = new RelayCommand(OpenSharePopUp);
            SearchCommand = new RelayCommand(Search);
            ApplyFilterCommand = new RelayCommand(ApplyFilter);
            RemoveLabelCommand = new RelayCommand<Label>(RemoveLabel);
            RemoveAssignCommand = new RelayCommand<User>(RemoveAssign);
            NavigateToMainPageCommand = new RelayCommand(NavigateToMainPage);
            NavigateToMyTaskCommand = new RelayCommand(NavigateToMyTask);
            CopyProjectCommand = new RelayCommand(OnSaveProjectToClipBoard);
            CancelAddListCommand = new RelayCommand(CancelAddList);
        }
        public Action ClosePopUpCreateList {  get; set; }
        private void CancelAddList()
        {
            IsErrorVisible = false;
            ClosePopUpCreateList?.Invoke();
            ListName = "";
        }

        //Sao chép nội dung project (projectID, project password vào clipboard)
        private void OnSaveProjectToClipBoard()
        {
            CopyStatus = "Coppied";
            string textToCopy = "Welcome to TaskMaster, your project information: \nProject ID: " + Project.Project_ID + "\nProject password: " + Project.Password;
            var dataPackage = new DataPackage();
            dataPackage.SetText(textToCopy);
            Clipboard.SetContent(dataPackage);
        }

        public Action NavigateMyTask { get; set; }
        private void NavigateToMyTask()
        {
            NavigateMyTask?.Invoke();
        }
        public Action NavigateMainPage { get; set; }
        private void NavigateToMainPage()
        {
            NavigateMainPage?.Invoke();
        }
        private void RemoveLabel(Label label)
        {
            if (SelectedLabels.Contains(label))
            {
                SelectedLabels.Remove(label);
            }
        }
        private void RemoveAssign(User assignment)
        {
            if (SelectedAssignments.Contains(assignment))
            {
                SelectedAssignments.Remove(assignment);
            }
        }

        private void OpenSharePopUp()
        {
            CopyStatus = "Copy";
            IsShareOpen = !IsShareOpen;
        }

        
        private void ApplyFilter()
        {
            // Logic lấy dữ liệu từ database và cập nhật FilteredTasks.
        }

        //Tìm kiếm theo task
        private void Search()
        {
            
        }
        //Mở PopUp Filter
        private void OpenFilterPopUp()
        {
            LoadLabels();
            IsFilterOpen = !IsFilterOpen;
        }

        public async void LoadLabels()
        {
            try
            {
                using (SqlConnection connection = DatabaseConnection.GetConnection())
                {
                    await connection.OpenAsync();
                    string query = "SELECT LabelColor, Name FROM Label"; 

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            Labels.Clear(); // Xóa danh sách cũ

                            while (reader.Read())
                            {
                                var label = new Label
                                {
                                    LabelColor = reader.GetString(reader.GetOrdinal("LabelColor")),
                                    Name = reader.GetString(reader.GetOrdinal("Name"))
                                };

                                Labels.Add(label); // Thêm label vào ObservableCollection
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading labels: {ex.Message}");
            }
        }
        public void LoadTaskDetail()
        {
            using (SqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();

                string query = "SELECT Title, StartDate, DueDate, Priority, Status, Description FROM Task WHERE Task_ID = @TaskId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TaskId", UserSession.CurrentTaskID);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Task = new _Task
                            {
                                Content = reader["Title"].ToString(),
                                Description = reader["Description"].ToString(),
                                Priority = reader["Priority"].ToString(),
                                Status = reader["Status"].ToString(),
                                StartDate = reader["StartDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["StartDate"],
                                DueDate = reader["DueDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["DueDate"],
                            };
                            TaskName = Task.Content;
                            PriorityIndex = Task.Priority switch
                            {
                                "Urgent" => 0,
                                "Important" => 1,
                                "Medium" => 2,
                                "Low" => 3,
                                _ => -1
                            };

                            // Xử lý chỉ số trạng thái
                            StatusIndex = Task.Status switch
                            {
                                "Not Started" => 0,
                                "In Progress" => 1,
                                "Completed" => 2,
                                _ => -1
                            };
                            Description = Task.Description;
                        }
                    }
                }
                string assignQuery = @"SELECT u.Name, u.Avatar FROM Assignment a JOIN [User] u ON a.User_ID = u.User_ID WHERE a.Task_ID = @TaskId";

                using (SqlCommand labelCommand = new SqlCommand(assignQuery, connection))
                {
                    labelCommand.Parameters.AddWithValue("@TaskId", UserSession.CurrentTaskID);

                    using (SqlDataReader reader = labelCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string name = reader["Name"].ToString();
                            string avatar = reader["Avatar"].ToString();
                            var assignment = new User
                            {
                                Name = name,
                                Avatar = avatar
                            };
                            SelectedAssignments.Add(assignment);
                        }
                    }
                }

                string labelQuery = @"SELECT label.LabelColor, label.Name
                      FROM Label
                      JOIN Task_Label ON label.LabelColor = Task_Label.LabelColor
                      WHERE Task_Label.Task_ID = @TaskId";

                using (SqlCommand labelCommand = new SqlCommand(labelQuery, connection))
                {
                    labelCommand.Parameters.AddWithValue("@TaskId", UserSession.CurrentTaskID);

                    using (SqlDataReader reader = labelCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string labelColor = reader["LabelColor"].ToString();
                            string labelName = reader["Name"].ToString();
                            var label = new Label
                            {
                                LabelColor = labelColor,
                                Name = labelName
                            };
                            SelectedLabels.Add(label);
                        }
                    }
                }
                LoadSelectedLabelForTask(UserSession.CurrentTaskID);
            }
        }
        
        public void LoadSelectedLabelForTask(string taskID)
        {
            SelectedLabels.Clear();
            using (SqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string labelQuery = @"SELECT label.LabelColor, label.Name
                      FROM Label
                      JOIN Task_Label ON label.LabelColor = Task_Label.LabelColor
                      WHERE Task_Label.Task_ID = @TaskId";

                using (SqlCommand labelCommand = new SqlCommand(labelQuery, connection))
                {
                    labelCommand.Parameters.AddWithValue("@TaskId", taskID);

                    using (SqlDataReader reader = labelCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string labelColor = reader["LabelColor"].ToString();
                            string labelName = reader["Name"].ToString();
                            var label = new Label
                            {
                                LabelColor = labelColor,
                                Name = labelName
                            };
                            SelectedLabels.Add(label);
                        }
                    }
                }
            }
        }
        public void LoadSelectedAssignmentForTask(string taskID)
        {
            SelectedAssignments.Clear();
            using (SqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string labelQuery = @"SELECT u.Name, u.Avatar
                                        FROM [user] u
                                        JOIN Assignment a ON u.User_ID = a.User_ID
                                        Where a.Task_ID = @TaskID";

                using (SqlCommand labelCommand = new SqlCommand(labelQuery, connection))
                {
                    labelCommand.Parameters.AddWithValue("@TaskId", taskID);

                    using (SqlDataReader reader = labelCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string name = reader["Name"].ToString();
                            string avatar = reader["Avatar"].ToString();
                            var assignment = new User
                            {
                                Name = name,
                                Avatar = avatar
                            };
                            SelectedAssignments.Add(assignment);
                        }
                    }
                }
            }
        }
        public async Task LoadUsersForAssignment()
        {
            try
            {
                using (SqlConnection connection = DatabaseConnection.GetConnection())
                {
                    await connection.OpenAsync();
                    string query = @"
                    SELECT u.User_ID, u.Name, u.Avatar
                    FROM [User] u
                    JOIN Member m ON u.User_ID = m.User_ID
                    WHERE m.Project_ID = @ProjectID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProjectID", UserSession.CurrentProjectID);

                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            Users.Clear();  
                            while (reader.Read())
                            {
                                string userID = reader.GetString(0);
                                string userName = reader.GetString(1);
                                string userAvatar = reader.GetString(2);
                                var user = new User
                                {
                                    User_ID = userID,
                                    Name = userName,
                                    Avatar = userAvatar
                                };
                                if (user.User_ID == UserSession.CurrentUserID)
                                {
                                    user.Name = $"{user.Name} (tôi)";
                                }
                                
                                Users.Add(user); 
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading users: {ex.Message}");
            }
        }

        public Action updateList { get; set; }
        private async void SaveNewList()
        {
                string trimmedListName = ListName?.Trim();
                if (string.IsNullOrWhiteSpace(trimmedListName))
                {
                    if (string.IsNullOrWhiteSpace(ListName))
                    {
                        StatusMessage = "Tên danh sách không được để trống!";
                        IsErrorVisible = true;
                        return;
                    }
                }
                try
                {
                    using (SqlConnection connection = DatabaseConnection.GetConnection())
                    {
                        await connection.OpenAsync();

                        // Check if list name already exists
                        using (SqlCommand checkCommand = new SqlCommand("SELECT COUNT(*) FROM List WHERE Name = @Name AND Project_ID = @Project_ID", connection))
                        {
                            checkCommand.Parameters.AddWithValue("@Name", trimmedListName);
                            checkCommand.Parameters.AddWithValue("@Project_ID", UserSession.CurrentProjectID);
                            int count = (int)await checkCommand.ExecuteScalarAsync();

                            if (count > 0)
                            {
                                StatusMessage = "Tên đã tồn tại";
                                IsErrorVisible = true;
                                return;
                            }
                        }

                        // Generate List ID
                        string newListId = GenerateListId(connection);
                        Random random = new Random();
                        byte r = (byte)random.Next(256);
                        byte g = (byte)random.Next(256);
                        byte b = (byte)random.Next(256);
                        string ColorofList = $"#{r:X2}{g:X2}{b:X2}";

                        // Insert the new list into the database
                        using (SqlCommand command = new SqlCommand("INSERT INTO List (List_ID, Project_ID, Name, Color) VALUES (@List_ID, @Project_ID, @Name, @Color)", connection))
                        {
                            command.Parameters.AddWithValue("@List_ID", newListId);
                            command.Parameters.AddWithValue("@Project_ID", UserSession.CurrentProjectID);
                            command.Parameters.AddWithValue("@Name", trimmedListName);
                            command.Parameters.AddWithValue("@Color", ColorofList);

                            await command.ExecuteNonQueryAsync();
                        }
                        updateList?.Invoke();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error inserting new list: {ex.Message}");
                }
            
        }


        private string GenerateListId(SqlConnection connection)
        {
            string newListId;
            using (SqlCommand command = new SqlCommand("SELECT TOP 1 List_ID FROM List ORDER BY List_ID DESC", connection))
            {
                object result = command.ExecuteScalar();
                int nextId = 1;

                if (result != null)
                {
                    string lastId = result.ToString();
                    if (lastId.StartsWith("Ls") && lastId.Length > 2)
                    {
                        string numberPart = lastId.Substring(2);
                        if (int.TryParse(numberPart, out int currentId))
                        {
                            nextId = currentId + 1;
                        }
                    }
                }
                newListId = "Ls" + nextId.ToString("D8");
            }
            return newListId;
        }
        public Action AddList_Click {  get; set; }
        private void AddList()
        {
            AddList_Click?.Invoke();
        }

        //Tải avatar người dùng 
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
        //Phương thức tải tên dự án lên header
        public async System.Threading.Tasks.Task LoadProjectData(string projectID)
        {
            try
            {
                using (SqlConnection connection = DatabaseConnection.GetConnection())
                {
                    await connection.OpenAsync();
                    string query = "SELECT Project_ID, Password, Name FROM [Project] WHERE Project_ID = @ProjectID";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@ProjectID", projectID);

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                            {
                                if (Project == null)
                                {
                                    Project = new Project
                                    {
                                        Project_ID = reader["Project_ID"].ToString(),
                                        Password = reader["Password"].ToString(),
                                        Name = reader["Name"].ToString()
                                    };
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading project data: {ex.Message}");
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
