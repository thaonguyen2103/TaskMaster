using System;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Windows.Foundation;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI;
using System.Diagnostics;
using System.Collections.Generic;
using TaskMaster.ViewModels;
using TaskMaster.Models;
using Windows.Gaming.Input.ForceFeedback;
using System.Transactions;
using System.Threading.Tasks;
using System.Linq;

namespace TaskMaster.Views
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Project : Page
    {
        private ProjectViewModel viewModel;

        public bool isApplyFilter = false;
        public bool isSearch = false;
        public Project()
        {
            this.InitializeComponent();
            viewModel = new ProjectViewModel();
            viewModel.Toggle = () => { AccountPopup.IsOpen = !AccountPopup.IsOpen; };
            viewModel.NavigateMyAccount = () => { Frame.Navigate(typeof(MyAccountPage)); };
            viewModel.LogOut = () => { Frame.Navigate(typeof(LoginPage)); };
            viewModel.NavigateMainPage = () => { Frame.Navigate(typeof(MainPage)); };
            viewModel.NavigateMyTask = () => { Frame.Navigate(typeof(MyTask)); };
            viewModel.AddList_Click += addList_Click;
            viewModel.updateList += addListSuccess;
            viewModel.ClosePopUpCreateList += NewList_Cancel;
            this.DataContext = viewModel;
            loadListContainer();
        }

        private async void addListSuccess()
        {
            ContentDialog failDialog = new ContentDialog
            {
                Title = "Thông báo",
                Content = "Thêm thành công",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await failDialog.ShowAsync();
       
            loadListContainer();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                viewModel.LoadUsersForAssignment();
                await viewModel.LoadUserAvatar(UserSession.CurrentUserID);
                await viewModel.LoadProjectData(UserSession.CurrentProjectID);
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                Debug.WriteLine($"Error loading user avatar: {ex.Message}");
            }
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
        
        bool isPinned = false;
        private void Pin_Click(object sender, TappedRoutedEventArgs e)
        {
            if (!isPinned)
            {
                PinBtn.Source = new BitmapImage(new Uri("ms-appx:///Assets/Image/pinned.png"));
                isPinned = true;
            }
            else
            {
                PinBtn.Source = new BitmapImage(new Uri("ms-appx:///Assets/Image/star.png"));
                isPinned = false;
            }
        }
        //Xử lý giao diện chọn label trong cài đặt task
        private void LabelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedLabel = LabelComboBox.SelectedItem as Label;
            if (selectedLabel != null)
            {
                if (!viewModel.SelectedLabels.Any(label => label.Name == selectedLabel.Name))
                {
                    viewModel.SelectedLabels.Add(selectedLabel);
                }
            }
        }
        private void AssignComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedAssign = AssignmentComboBox.SelectedItem as User;
            if (selectedAssign != null)
            {
                // Kiểm tra nếu assignment chưa được chọn
                if (!viewModel.SelectedAssignments.Any(assignment => assignment.User_ID == selectedAssign.User_ID))
                {
                    viewModel.SelectedAssignments.Add(selectedAssign);
                }
            }
           
        }
        //Tạo task mới
        private void addTask_Click(object sender, RoutedEventArgs e)
        {
            emptyPopUp();
            viewModel.LoadUsersForAssignment();
            viewModel.LoadLabels();
            taskPopUp_Save.Visibility = Visibility.Collapsed;
            taskPopUp_Add.Visibility = Visibility.Visible;
            CreateTaskField.IsOpen = true;

            Button addTaskButton = sender as Button;
            if (addTaskButton == null)
            {
                return;
            }
            string listID = addTaskButton.Tag as string; 

            if (listID != null)
            {
                UserSession.CurrentListID = listID; // Gán ListID vào UserSession
            }
        }

        private async void addTask_Add(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(TaskName.Text))
                {
                    ContentDialog failNameDialog = new ContentDialog
                    {
                        Title = "Thông báo",
                        Content = "Vui lòng nhập tên task!",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await failNameDialog.ShowAsync();
                    return;
                }

                using (SqlConnection connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    string taskName = TaskName.Text.Trim();
                    string listID = UserSession.CurrentListID;

                    using (SqlCommand checkCommand = new SqlCommand("SELECT COUNT(*) FROM Task WHERE Title = @Title AND List_ID = @ListId", connection))
                    {
                        checkCommand.Parameters.AddWithValue("@Title", taskName);
                        checkCommand.Parameters.AddWithValue("@ListId", listID);

                        int count = (int)checkCommand.ExecuteScalar();

                        if (count > 0)
                        {
                            ContentDialog failDialog = new ContentDialog
                            {
                                Title = "Thông báo",
                                Content = "Tên đã tồn tại trong list",
                                CloseButtonText = "OK",
                                XamlRoot = this.XamlRoot
                            };
                            await failDialog.ShowAsync();
                            return;
                        }
                    }
                    string description = Description.Text.Trim();
                    // Tạo ID cho tác vụ
                    string taskID = GenerateTaskId(connection);

                    // Ngày bắt đầu
                    DateTimeOffset? date1 = StartDate.Date;
                    DateTime startDate = date1?.DateTime ?? DateTime.Now;

                    // Ngày hết hạn
                    DateTimeOffset? date2 = DueDate.Date;
                    DateTime dueDate = date2?.DateTime ?? DateTime.Now;

                    // Lấy ưu tiên từ ComboBox
                    string selectedContent = (Priority.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Low";
                    // Lấy trạng thái từ ComboBox
                    string selectedStatus = (Status.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Not Started";
                    // Đóng popup tạo tác vụ
                    CreateTaskField.IsOpen = false;
                    // Thêm tác vụ vào cơ sở dữ liệu
                    using (SqlCommand command = new SqlCommand(@"INSERT INTO Task (Task_ID, List_ID, Title, StartDate, DueDate, Description, Priority, Status) 
                                         VALUES (@TaskId, @ListId, @Title, @StartDate, @DueDate, @Description, @Priority, @Status)", connection))
                    {
                        // Thêm các tham số vào câu lệnh SQL
                        command.Parameters.AddWithValue("@TaskId", taskID);
                        command.Parameters.AddWithValue("@ListId", listID);
                        command.Parameters.AddWithValue("@Title", taskName);
                        command.Parameters.AddWithValue("@StartDate", startDate);
                        command.Parameters.AddWithValue("@DueDate", dueDate);
                        command.Parameters.AddWithValue("@Description", description);
                        command.Parameters.AddWithValue("@Priority", selectedContent);
                        command.Parameters.AddWithValue("@Status", selectedStatus);

                        command.ExecuteNonQuery();
                    }
                    foreach (var user in viewModel.SelectedAssignments)
                    {
                        string selectedAssign = user.User_ID; 
                        if (!string.IsNullOrEmpty(selectedAssign))
                        {
                            using (SqlCommand cmd = new SqlCommand(@"INSERT INTO Assignment (Task_ID, User_ID, Dedication, JoinDate, Source, Title) 
                                                 VALUES (@TaskId, @UserId, @Dedication, @JoinDate, @Source, @Title)", connection))
                            {
                                cmd.Parameters.AddWithValue("@TaskId", taskID);  
                                cmd.Parameters.AddWithValue("@UserId", selectedAssign); 
                                int percent = 0;  
                                cmd.Parameters.AddWithValue("@Dedication", percent);
                                cmd.Parameters.AddWithValue("@JoinDate", DateTime.Now.Date);  
                                cmd.Parameters.AddWithValue("@Source", UserSession.CurrentProjectName);  
                                cmd.Parameters.AddWithValue("@Title", taskName); 
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    foreach (var label in viewModel.SelectedLabels)
                    {
                        if (label.LabelColor != null)
                        {
                            var labelColorHex = label.LabelColor.ToString();

                            using (SqlCommand cmd2 = new SqlCommand(@"INSERT INTO Task_Label (Task_ID, LabelColor) 
                      VALUES (@TaskId, @LabelColor)", connection))
                            {
                                cmd2.Parameters.AddWithValue("@TaskId", taskID);
                                cmd2.Parameters.AddWithValue("@LabelColor", labelColorHex);
                                cmd2.ExecuteNonQuery();
                            }
                        }
                    }
                    loadListContainer();
                }
            }
            catch (SqlException sqlEx)
            {
            }
            catch (Exception ex)
            {
            }
        }


        private string GenerateTaskId(SqlConnection connection)
        {
            string newTaskId;
            using (SqlCommand command = new SqlCommand("SELECT TOP 1 Task_ID FROM Task ORDER BY Task_ID DESC", connection))
            {
                object result = command.ExecuteScalar();
                int nextId = 1;

                if (result != null)
                {
                    string lastId = result.ToString();
                    if (lastId.StartsWith("Ta") && lastId.Length > 2)
                    {
                        string numberPart = lastId.Substring(2);
                        if (int.TryParse(numberPart, out int currentId))
                        {
                            nextId = currentId + 1;
                        }
                    }
                }
                newTaskId = "Ta" + nextId.ToString("D8");
            }
            return newTaskId;
        }
        private void loadListContainer()
        {
            var elementsToKeep = new List<UIElement> { addListBox };
            for (int i = List_Container.Children.Count - 1; i >= 0; i--)
            {
                UIElement child = List_Container.Children[i];
                if (!elementsToKeep.Contains(child))
                {
                    List_Container.Children.RemoveAt(i);
                }
            }
            try
            {
                using (SqlConnection connection = DatabaseConnection.GetConnection())
                {
                    connection.Open();
                    // First query to get the project ID
                    using (SqlCommand command = new SqlCommand(@"SELECT p.Project_ID FROM Project p 
                                                        JOIN Member m ON m.Project_ID = p.Project_ID 
                                                        WHERE p.Name = @ProjectName 
                                                        AND m.User_ID = @UserID", connection))
                    {
                        command.Parameters.AddWithValue("@ProjectName", UserSession.CurrentProjectName);
                        command.Parameters.AddWithValue("@UserID", UserSession.CurrentUserID);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                UserSession.CurrentProjectID = reader["Project_ID"].ToString();
                            }
                            else
                            {
                                Console.WriteLine("No project found for the current user.");
                                return;
                            }
                        }
                    }

                    // Second query to get the lists based on the project ID
                    using (SqlCommand command = new SqlCommand(@"SELECT List_ID, Name FROM List WHERE Project_ID = @ProjectId", connection))
                    {
                        command.Parameters.AddWithValue("@ProjectId", UserSession.CurrentProjectID);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string listID = reader["List_ID"].ToString();
                                string listName = reader["Name"].ToString();

                                StackPanel listPanel = LoadListItem(listName, listID);

                                List_Container.Children.Add(listPanel);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu có
                Console.WriteLine("Có lỗi xảy ra khi tải danh sách: " + ex.Message);
            }
        }
        private void addTask_Cancel(object sender, RoutedEventArgs e)
        {
            CreateTaskField.IsOpen = false;
        }
        private void addList_Click()
        {
            EnterNewNameList.Text = "";
            NewList_Btn.Visibility = Visibility.Collapsed;
            AddNewList_Btn.Visibility = Visibility.Visible;
            EnterNewNameList.Visibility = Visibility.Visible;
            Cancel_AddNewList_Btn.Visibility = Visibility.Visible;
        }
        private void NewList_Cancel()
        {
            NewList_Btn.Visibility = Visibility.Visible;
            AddNewList_Btn.Visibility = Visibility.Collapsed;
            EnterNewNameList.Visibility = Visibility.Collapsed;
            Cancel_AddNewList_Btn.Visibility = Visibility.Collapsed;
        }
        //Hàm tạo nhãn dán nhỏ cho task
        private Button CreateLabelButton(string labelText, string labelColor)
        {
            // Tạo Button cho nhãn
            Button labelButton = new Button
            {
                Content = labelText,
                Background = new SolidColorBrush((Windows.UI.Color)Microsoft.UI.Xaml.Markup.XamlBindingHelper.ConvertValue(typeof(Windows.UI.Color), labelColor)),
                Style = (Style)Application.Current.Resources["CustomLabelStyle"] // Áp dụng style cho nhãn
            };
            return labelButton;
        }
        
        //Hàm tạo task (khung nhỏ)
        private Border CreateTaskItem(string taskId, string taskName, DateTime? dueDate)
        {
            // Tạo Border cho nhiệm vụ
            Border taskBorder = new Border
            {
                Style = (Style)Application.Current.Resources["CustomTaskStyle"],
                Margin = new Thickness(0, 2, 0, 0)
            };

            // Tạo Grid bên trong Border
            Grid taskGrid = new Grid();

            // Định nghĩa các cột và hàng
            taskGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
            taskGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            taskGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            taskGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            taskGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            taskGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

            // Tạo TextBlock cho tên nhiệm vụ
            TextBlock taskNameTextBlock = new TextBlock
            {
                
                Text = taskName, // Sử dụng taskName từ tham số
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5, 0, 5),
                FontSize=12,
                FontWeight=FontWeights.Bold
            };
          

            // Tạo nút "more" cho nhiệm vụ
            Button moreButton = new Button
            {
                Background = new SolidColorBrush(Colors.Transparent),
                Padding = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Right,
                Tag = taskId
            };

            Image moreImage = new Image
            {
                Source = new BitmapImage(new Uri("ms-appx:///Assets/Image/moreTask.png")),
                Width = 12
            };
            moreButton.Content = moreImage;
            moreButton.Click += settingforTask;
            Grid.SetColumn(moreButton, 1); // Thiết lập cột cho nút "more"
            Grid.SetRow(moreButton, 0);     // Thiết lập hàng cho nút "more"

            //Label
            viewModel.LoadSelectedLabelForTask(taskId);
            if(viewModel.SelectedLabels != null)
            {
                Grid.SetColumn(taskNameTextBlock, 0); 
                Grid.SetRow(taskNameTextBlock, 0);
                VariableSizedWrapGrid labelGrid = new VariableSizedWrapGrid
                {
                    Margin = new Thickness(0),
                    Orientation = Orientation.Horizontal,
                };
                Grid.SetColumnSpan(labelGrid, 2); 
                Grid.SetRow(labelGrid, 1);         

                // Đọc dữ liệu label_task
               
                foreach (var label in viewModel.SelectedLabels)
                {
                    Button labelButton = CreateLabelButton(label.Name, label.LabelColor);
                    labelGrid.Children.Add(labelButton);
                }
                taskGrid.Children.Add(labelGrid); 
            }
            
            taskGrid.Children.Add(taskNameTextBlock);
            taskGrid.Children.Add(moreButton);



            // Tạo StackPanel cho thời gian
            if (dueDate.HasValue)
            {
                DateTime currentDate = DateTime.Now;
                DateTime selectedDate = dueDate.Value;

                StackPanel timeStackPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Spacing = 4,
                };
                Grid.SetRow(timeStackPanel, 2);

                Image timeImage = new Image
                {
                    Width = 15
                };

                if ((selectedDate - currentDate).TotalDays <= 3)
                {
                    timeImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/Image/time2.png"));
                }
                else
                {
                    timeImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/Image/time.png"));
                }

                TextBlock dateTextBlock = new TextBlock
                {
                    Text = selectedDate.ToString("d"),
                    FontSize = 10,
                    VerticalAlignment = VerticalAlignment.Center
                };

                timeStackPanel.Children.Add(timeImage);
                timeStackPanel.Children.Add(dateTextBlock);
                taskGrid.Children.Add(timeStackPanel);
            }
            //Assignment
            viewModel.LoadSelectedAssignmentForTask(taskId);
            if (viewModel.SelectedAssignments != null)
            {
                VariableSizedWrapGrid assignmentGrid = new VariableSizedWrapGrid
                {
                    Margin = new Thickness(0),
                    Orientation = Orientation.Horizontal,
                };
                Grid.SetColumnSpan(assignmentGrid, 2);
                Grid.SetRow(assignmentGrid, 3);

                // Đọc dữ liệu label_task

                foreach (var assignment in viewModel.SelectedAssignments)
                {
                    Image assignmentPerson = new Image
                    {
                        Source = new BitmapImage(new Uri(assignment.Avatar, UriKind.Absolute)),
                        Height = 20,
                        Margin = new Thickness(2)
                    };
                    assignmentGrid.Children.Add(assignmentPerson);
                }
                taskGrid.Children.Add(assignmentGrid);
            }
            // Đặt Grid vào Border
            taskBorder.Child = taskGrid;

            return taskBorder;
        }
        private void settingforTask(object sender, RoutedEventArgs e)
        {
            viewModel.LoadLabels();
            var settingList_btn = sender as Button;
            var taskId = settingList_btn.Tag as string;
            if (taskId != null)
            {
                UserSession.CurrentTaskID = taskId; // Gán TaskID vào UserSession
            }

            MenuFlyout menuFlyout = new MenuFlyout
            {
                Placement = FlyoutPlacementMode.Bottom,
            };

            MenuFlyoutItem settingItem = new MenuFlyoutItem
            {
                Text = "Setting",
                Icon = new SymbolIcon(Symbol.Setting),
                Style = (Style)Application.Current.Resources["CustomMenuFlyoutItemStyle"]
            };
            settingItem.Click += SettingTask_Click;

            MenuFlyoutItem deleteItem = new MenuFlyoutItem
            {
                Text = "Delete",
                Icon = new SymbolIcon(Symbol.Delete),
                Style = (Style)Application.Current.Resources["CustomMenuFlyoutItemStyle"]
            };
            deleteItem.Click += DeleteTask_Click;
            menuFlyout.Items.Add(settingItem);
            menuFlyout.Items.Add(deleteItem);
            settingList_btn.Flyout = menuFlyout;
        }
        private async void DeleteTask_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog deleteConfirmationDialog = new ContentDialog
            {
                Title = "Xác nhận xóa",
                Content = "Bạn có chắc chắn muốn xóa task này không?",
                PrimaryButtonText = "Yes",
                CloseButtonText = "No",
                XamlRoot = this.XamlRoot
            };

            ContentDialogResult result = await deleteConfirmationDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                DeleteTaskFromDatabase();
            }
            else
            {
            }
        }
        private void DeleteTaskFromDatabase()
        {
            using (SqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();

                // Xóa Assignment
                string deleteAssignmentsQuery = "DELETE FROM Assignment WHERE Task_ID = @TaskId";
                using (SqlCommand deleteAssignmentsCommand = new SqlCommand(deleteAssignmentsQuery, connection))
                {
                    deleteAssignmentsCommand.Parameters.AddWithValue("@TaskId", UserSession.CurrentTaskID);
                    deleteAssignmentsCommand.ExecuteNonQuery();
                }

                // Xóa Task_Label
                string deleteTaskLabelsQuery = "DELETE FROM Task_Label WHERE Task_ID = @TaskId";
                using (SqlCommand deleteTaskLabelsCommand = new SqlCommand(deleteTaskLabelsQuery, connection))
                {
                    deleteTaskLabelsCommand.Parameters.AddWithValue("@TaskId", UserSession.CurrentTaskID);
                    deleteTaskLabelsCommand.ExecuteNonQuery();
                }

                // Xóa Task
                string deleteTaskQuery = "DELETE FROM Task WHERE Task_ID = @TaskId";
                using (SqlCommand deleteTaskCommand = new SqlCommand(deleteTaskQuery, connection))
                {
                    deleteTaskCommand.Parameters.AddWithValue("@TaskId", UserSession.CurrentTaskID);
                    deleteTaskCommand.ExecuteNonQuery();
                }

                // Cập nhật giao diện
                loadListContainer();
            }
        }


        private void SettingTask_Click(object sender, RoutedEventArgs e)
        {
            emptyPopUp();
            viewModel.LoadTaskDetail();
            taskPopUp_Save.Visibility = Visibility.Visible;
            taskPopUp_Add.Visibility = Visibility.Collapsed;
            CreateTaskField.IsOpen = true;
        }
        private void emptyPopUp()
        {
            StartDate.Date = null;
            DueDate.Date = null;
            Priority.SelectedIndex = -1;
            Status.SelectedIndex = -1;
            viewModel.SelectedLabels.Clear();
            viewModel.SelectedAssignments.Clear();
        }
        private void settingTask_Save(object sender, RoutedEventArgs e)
        {
            string title = TaskName.Text;
            string description = Description.Text;
            DateTimeOffset? date1 = StartDate.Date;
            DateTime startDate = date1?.DateTime ?? DateTime.Now;
            DateTimeOffset? date2 = DueDate.Date;
            DateTime dueDate = date2?.DateTime ?? DateTime.Now;
            string priority = (Priority.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Low";
            string status = (Status.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Not Started";

            using (SqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();

                string query = @"
            UPDATE Task 
            SET Title = @Title, 
                StartDate = @StartDate, 
                DueDate = @DueDate, 
                Priority = @Priority, 
                Status = @Status,
                Description = @Description
            WHERE Task_ID = @TaskId";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Title", title);
                    command.Parameters.AddWithValue("@StartDate", startDate);
                    command.Parameters.AddWithValue("@DueDate", dueDate);
                    command.Parameters.AddWithValue("@Priority", priority);
                    command.Parameters.AddWithValue("@Status", status);
                    command.Parameters.AddWithValue("@Description", description);
                    command.Parameters.AddWithValue("@TaskId", UserSession.CurrentTaskID);
                    command.ExecuteNonQuery();
                }

                //Load danh sách thành viên nhận nhiệm vụ
                string deleteAssignQuery = @"DELETE FROM Assignment WHERE Task_ID = @TaskId";
                using (SqlCommand deleteCommand = new SqlCommand(deleteAssignQuery, connection))
                {
                    deleteCommand.Parameters.AddWithValue("@TaskId", UserSession.CurrentTaskID);
                    deleteCommand.ExecuteNonQuery();
                }
                foreach (var user in viewModel.SelectedAssignments)
                {
                    string selectedAssign2 = user.User_ID;
                    if (!string.IsNullOrEmpty(selectedAssign2))
                    {
                        using (SqlCommand cmd = new SqlCommand(@"INSERT INTO Assignment (Task_ID, User_ID, Dedication, JoinDate, Source, Title) 
                                                 VALUES (@TaskId, @UserId, @Dedication, @JoinDate, @Source, @Title)", connection))
                        {
                            cmd.Parameters.AddWithValue("@TaskId", UserSession.CurrentTaskID);
                            cmd.Parameters.AddWithValue("@UserId", selectedAssign2);
                            int percent = 0;
                            cmd.Parameters.AddWithValue("@Dedication", percent);
                            cmd.Parameters.AddWithValue("@JoinDate", DateTime.Now.Date);
                            cmd.Parameters.AddWithValue("@Source", UserSession.CurrentProjectName);
                            cmd.Parameters.AddWithValue("@Title", title);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                //Load danh sách label đã được gán
                string deleteQuery = @"DELETE FROM Task_Label WHERE Task_ID = @TaskId";
                using (SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection))
                {
                    deleteCommand.Parameters.AddWithValue("@TaskId", UserSession.CurrentTaskID);
                    deleteCommand.ExecuteNonQuery();
                }
                if (viewModel.SelectedLabels != null && viewModel.SelectedLabels.Any())
                {
                    // Duyệt qua các nhãn trong SelectedLabels và insert vào Task_Label
                    foreach (var label in viewModel.SelectedLabels)
                    {
                        if (!string.IsNullOrEmpty(label.LabelColor))
                        {
                            string insertQuery = @"INSERT INTO Task_Label (Task_ID, LabelColor) 
                                       VALUES (@TaskId, @LabelColor)";

                            using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                            {
                                insertCommand.Parameters.AddWithValue("@TaskId", UserSession.CurrentTaskID);
                                insertCommand.Parameters.AddWithValue("@LabelColor", label.LabelColor);

                                insertCommand.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            loadListContainer();
            CreateTaskField.IsOpen = false;
        }

        //Load 1 list 
        private StackPanel LoadListItem(string listName, string ListID)
        {
            StackPanel stackpanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 230,
                
                
            };
            StackPanel listItem = new StackPanel
            {
                Style = (Style)Application.Current.Resources["CustomListFrameStyle"],
                Tag = ListID  // Gán ListID cho Tag của listItem
            };

            StackPanel list_header = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Width = 300,
                Spacing = 5
            };

            // Tên danh sách
            TextBlock list_header_name = new TextBlock
            {
                Text = listName,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                MaxWidth=140
            };

            // Thêm nút more ở danh sách
            Button list_header_more = new Button
            {
                Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
                Padding = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Right,
                Tag = ListID 
            };
            list_header_more.Click += settingforList;
            Image moreImage = new Image
            {
                Source = new BitmapImage(new Uri("ms-appx:///Assets/Image/moreList.png")),
                Width = 20
            };
            list_header_more.Content = moreImage;

            

            // Thêm nút add new task
            ScrollViewer scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalAlignment = VerticalAlignment.Top,
                MaxHeight = 350
            };

            StackPanel stackPanel = new StackPanel
            {
                Width = 250,
                Orientation = Orientation.Vertical,
                Padding = new Thickness(0, 10, 0, 0),
                Spacing = 10
            };

            Button addTaskButton = new Button
            {
                Content = "+ Add new task",
                Padding = new Thickness(10, 2, 10, 2),
                Style = (Style)Application.Current.Resources["CommonButtonStyle"],
                Width = 200,
                BorderBrush =new SolidColorBrush(Microsoft.UI.Colors.Gray),
                HorizontalAlignment = HorizontalAlignment.Left,
                Tag = ListID  // Gán ListID cho Tag của addTaskButton
            };
            addTaskButton.Click += addTask_Click;
            stackPanel.Children.Add(addTaskButton);
            TextBlock completedTask = new TextBlock
            {
                Text = "Completed",
                FontSize = 13,
                Margin = new Thickness(0, 10, 0, 0)
            };
            ScrollViewer scrollViewer_completed = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalAlignment = VerticalAlignment.Top,
                MaxHeight = 85,
                Opacity = 0.5
            };

            StackPanel stackPanel_completed = new StackPanel
            {
                Width = 250,
                Orientation = Orientation.Vertical,
               
                Padding = new Thickness(0, 10, 0, 0),
                Spacing = 10,
                
            };

            if (isApplyFilter == false && isSearch == false)
            {
                LoadTasksForList(ListID, stackPanel,stackPanel_completed);
            }
            else if (isApplyFilter == true && isSearch == false)
            {
                ApplyFilter(ListID, stackPanel,stackPanel_completed);
            }
            else if (isApplyFilter == false && isSearch == true)
            {
                SearchTasks(ListID, stackPanel,stackPanel_completed);
            }
            

            
            scrollViewer.Content = stackPanel;
            list_header.Children.Add(list_header_more);
            list_header.Children.Add(list_header_name);
            listItem.Children.Add(list_header);
            listItem.Children.Add(scrollViewer);
            stackpanel.Children.Add(listItem);
            scrollViewer_completed.Content = stackPanel_completed;
            stackpanel.Children.Add(completedTask);
            stackpanel.Children.Add(scrollViewer_completed);
            
            return stackpanel;
        }
        //Cài đặt cho danh sách
        private void settingforList(object sender, RoutedEventArgs e)
        {
            var settingList_btn = sender as Button;
            var listId = settingList_btn.Tag as string;
            if (listId != null)
            {
                UserSession.CurrentListID = listId; // Gán ListID vào UserSession
            }

            MenuFlyout menuFlyout = new MenuFlyout
            {
                Placement = FlyoutPlacementMode.Bottom,
            };

            MenuFlyoutItem renameItem = new MenuFlyoutItem
            {
                Text = "ReName",
                Icon = new SymbolIcon(Symbol.Refresh),
                Style = (Style)Application.Current.Resources["CustomMenuFlyoutItemStyle"]
            };
            renameItem.Click += RenameList_Click;

            MenuFlyoutItem deleteItem = new MenuFlyoutItem
            {
                Text = "Delete",
                Icon = new SymbolIcon(Symbol.Delete),
                Style = (Style)Application.Current.Resources["CustomMenuFlyoutItemStyle"]
            };
            deleteItem.Click += (s, args) => DeleteItem_Click(listId);
            menuFlyout.Items.Add(renameItem);
            menuFlyout.Items.Add(deleteItem);
            settingList_btn.Flyout = menuFlyout;
        }
        //Đổi tên danh sách
        private void RenameList_Click(object sender, RoutedEventArgs e)
        {

        }
        //Xóa danh sách
        private void DeleteItem_Click(string listId)
        {
            using (SqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();
                string getTasksQuery = "SELECT Task_ID FROM Task WHERE List_ID = @ListId";
                List<string> taskIds = new List<string>();
                using (SqlCommand getTasksCommand = new SqlCommand(getTasksQuery, connection))
                {
                    getTasksCommand.Parameters.AddWithValue("@ListId", listId);
                    using (SqlDataReader reader = getTasksCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            taskIds.Add(reader["Task_ID"].ToString());
                        }
                    }
                }
                foreach (var taskId in taskIds)
                {
                    UserSession.CurrentTaskID = taskId;
                    DeleteTaskFromDatabase();
                }
                string deleteListQuery = "DELETE FROM List WHERE List_ID = @ListId";
                using (SqlCommand deleteListCommand = new SqlCommand(deleteListQuery, connection))
                {
                    deleteListCommand.Parameters.AddWithValue("@ListId", listId);
                    int rowsAffected = deleteListCommand.ExecuteNonQuery();
                    Debug.WriteLine($"Danh sách (List) bị xóa: {rowsAffected} dòng.");
                }
            }
            loadListContainer();
        }
        private void LoadTasksForList(string listID, StackPanel stackPanel, StackPanel stackPanel_completed)
        {
            using (SqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();

                // Modify the SQL query to order by priority
                string query = @"
            SELECT Task_ID, Title, DueDate, Priority, Status 
            FROM Task 
            WHERE List_ID = @ListId
            ORDER BY 
                CASE 
                    WHEN Priority = 'Urgent' THEN 0
                    WHEN Priority = 'Important' THEN 1
                    WHEN Priority = 'Medium' THEN 2
                    WHEN Priority = 'Low' THEN 3
                    ELSE 5 -- for any other priority values (if needed)
                END";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ListId", listID);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string taskId = reader["Task_ID"].ToString();
                            string taskTitle = reader["Title"].ToString();
                            DateTime? dueDate = reader["DueDate"] as DateTime?;
                            string status = reader["Status"].ToString();
                            // Tạo Border cho từng nhiệm vụ
                            Border taskItem = CreateTaskItem(taskId, taskTitle, dueDate);

                            // Thêm Border vào StackPanel
                            if(status == "Completed")
                            {
                                stackPanel_completed.Children.Add(taskItem);
                            }
                            else
                            {
                                stackPanel.Children.Add(taskItem);
                            }
                        }
                    }
                }
            }
        }

        public void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            isApplyFilter = false;
            PriorityFilter.SelectedIndex = -1;
            StatusFilter.SelectedIndex = -1;
            LabelFilter.SelectedIndex = -1;
            loadListContainer();
        }
        public void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            isApplyFilter = true;
            loadListContainer();
        }
        public void ApplyFilter(string listID, StackPanel stackPanel, StackPanel stackPanel_completed)
        {
            stackPanel.Children.Clear();
            string priority = PriorityFilter.SelectedItem != null ? (PriorityFilter.SelectedItem as ComboBoxItem).Content.ToString() : null;
            string status = StatusFilter.SelectedItem != null ? (StatusFilter.SelectedItem as ComboBoxItem).Content.ToString() : null;
            string label = null;

            if (LabelFilter.SelectedItem != null)
            {
                var selectedLabel = (Label)LabelFilter.SelectedItem;
                label = selectedLabel.LabelColor; 
            }
            using (SqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();

                // Khởi tạo câu truy vấn cơ bản
                string query = @"
            SELECT t.Task_ID, t.Title, t.DueDate, t.Status
            FROM Task t
            LEFT JOIN Task_Label tl ON t.Task_ID = tl.Task_ID
            WHERE t.List_ID = @ListId";

                // Thêm các điều kiện vào câu truy vấn nếu có giá trị filter
                if (!string.IsNullOrEmpty(priority))
                {
                    query += " AND t.Priority = @Priority";
                }

                if (!string.IsNullOrEmpty(status))
                {
                    query += " AND t.Status = @Status";
                }

                if (!string.IsNullOrEmpty(label))
                {
                    query += " AND tl.LabelColor = @Label"; // Lọc theo Label từ bảng Task_Label
                }

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Thêm tham số vào câu truy vấn
                    command.Parameters.AddWithValue("@ListId", listID);

                    if (!string.IsNullOrEmpty(priority))
                        command.Parameters.AddWithValue("@Priority", priority);

                    if (!string.IsNullOrEmpty(status))
                        command.Parameters.AddWithValue("@Status", status);

                    if (!string.IsNullOrEmpty(label))
                        command.Parameters.AddWithValue("@Label", label);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string taskId = reader["Task_ID"].ToString();
                            string taskTitle = reader["Title"].ToString();
                            DateTime? dueDate = reader["DueDate"] as DateTime?;
                            string status1 = reader["Status"].ToString();
                            // Create Task Item
                            Border taskItem = CreateTaskItem(taskId, taskTitle, dueDate);
                            if (status1 == "Completed")
                            {
                                stackPanel_completed.Children.Add(taskItem);
                            }
                            else
                            {
                                stackPanel.Children.Add(taskItem);
                            }
                        }
                    }
                }
            }
        }
        public void SearchTasks(string listID, StackPanel stackPanel, StackPanel stackPanel_completed)
        {
            stackPanel.Children.Clear();
            string searchKeyword = SearchTextBox.Text;

            using (SqlConnection connection = DatabaseConnection.GetConnection())
            {
                connection.Open();

                string query = @"
            SELECT t.Task_ID, t.Title, t.DueDate, t.Status
            FROM Task t
            WHERE t.List_ID = @ListId";

                // Nếu có từ khóa tìm kiếm, thêm điều kiện WHERE cho cột Title
                if (!string.IsNullOrEmpty(searchKeyword))
                {
                    query += " AND t.Title LIKE @SearchKeyword";  // Tìm kiếm theo tên task
                }

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Thêm tham số vào câu truy vấn
                    command.Parameters.AddWithValue("@ListId", listID);

                    if (!string.IsNullOrEmpty(searchKeyword))
                        command.Parameters.AddWithValue("@SearchKeyword", "%" + searchKeyword + "%"); // Dùng LIKE với dấu %

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string taskId = reader["Task_ID"].ToString();
                            string taskTitle = reader["Title"].ToString();
                            DateTime? dueDate = reader["DueDate"] as DateTime?;
                            string status = reader["Status"].ToString();
                            // Tạo task item
                            Border taskItem = CreateTaskItem(taskId, taskTitle, dueDate);
                            if (status == "Completed")
                            {
                                stackPanel_completed.Children.Add(taskItem);
                            }
                            else
                            {
                                stackPanel.Children.Add(taskItem);
                            }
                        }
                    }
                }
            }
        }
        private DispatcherTimer _searchTimer;

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            isSearch = !string.IsNullOrWhiteSpace(SearchTextBox.Text);
            loadListContainer();
        }
        public void openAccountPage_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MyAccountPage));
        }
    }
}
