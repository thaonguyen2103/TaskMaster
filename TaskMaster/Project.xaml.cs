using System;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI;
using System.Diagnostics;
using System.Collections.Generic;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TaskMaster
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Project : Page
    {
        public Project()
        {
            this.InitializeComponent();
            ProjectName.Text = UserSession.CurrentProjectName;
            loadListContainer();
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
        private void MyTaskButton_Click(Object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
            MainPage mainPage = (MainPage)Frame.Content;
            mainPage.showTaskContainer();
        }
        private void MyProjectButton_Click(Object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
            MainPage mainPage = (MainPage)Frame.Content;
            mainPage.showProjectContainer();
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
        private void MyComboBox3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MyComboBox3.SelectedItem is ComboBoxItem selectedItem)
            {
                StackPanel stackPanel = selectedItem.Content as StackPanel;
                SolidColorBrush backgroundBrush = stackPanel.Background as SolidColorBrush;
                SolidColorBrush foregroundBrush = (stackPanel.Children[0] as TextBlock).Foreground as SolidColorBrush;
                string selectedColorName = (stackPanel.Children[0] as TextBlock).Text;
                Button colorButton = new Button
                {
                    Content = new TextBlock
                    {
                        Text = selectedColorName,
                        FontSize = 10,
                        Foreground = foregroundBrush
                    },
                    Background = backgroundBrush,
                    Padding = new Thickness(6, 1, 6, 1),
                    CornerRadius = new CornerRadius(4),
                    Margin = new Thickness(3)
                };
                PopUp_LabelContainer.Children.Add(colorButton);
            }
        }
        //Tạo task mới
        private void addTask_Click(object sender, RoutedEventArgs e)
        {
            CreateTaskField.IsOpen = true;

            // Lấy Button từ sender
            Button addTaskButton = sender as Button;
            if (addTaskButton == null)
            {
                // Xử lý khi không thể cast sender
                return;
            }

            // Lấy StackPanel cha của Button // Hoặc sử dụng cách khác để lấy StackPanel nếu cần
            string listID = addTaskButton.Tag as string; // Lấy ListID từ Tag của StackPanel

            if (listID != null)
            {
                UserSession.CurrentListID = listID; // Gán ListID vào UserSession
            }
        }

        private async void addTask_Add(object sender, RoutedEventArgs e)
        {
            TextBlock errorTextBlock = new TextBlock
            {
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.Red),
                Visibility = Visibility.Collapsed // Ẩn theo mặc định
            };
            try
            {
                // Kiểm tra thông tin cần thiết trước khi mở kết nối
                if (string.IsNullOrEmpty(TaskName.Text))
                {
                    // Hiển thị thông báo hoặc xử lý khi tên tác vụ trống
                    var emptyTaskNameDialog = new ContentDialog
                    {
                        Title = "Input Error",
                        Content = "Please enter a task name.",
                        CloseButtonText = "Close"
                    };
                    await emptyTaskNameDialog.ShowAsync();
                    return;
                }

                using (SqlConnection connection = new SqlConnection("Server=LAPTOP-KNDSJBLQ\\SQLEXPRESS;Database=TaskMaster;Trusted_Connection=True;TrustServerCertificate=True;"))
                {
                    connection.Open();

                    string taskID = GenerateTaskId(connection); // Tạo ID cho tác vụ
                    string listID = UserSession.CurrentListID; // Lấy ID danh sách từ UserSession
                    string taskname = TaskName.Text; // Lấy tên tác vụ từ TextBox

                    // Ngày bắt đầu
                    DateTimeOffset? date1 = DueDate.Date;
                    DateTime startDate = date1?.DateTime ?? DateTime.Now;

                    // Ngày hết hạn
                    DateTimeOffset? date2 = DueDate.Date;
                    DateTime dueDate = date2?.DateTime ?? DateTime.Now;

                    // Lấy ưu tiên từ ComboBox
                    string selectedContent = (MyComboBox.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Low";
                    // Lấy trạng thái từ ComboBox
                    string selectedStatus = (MyComboBox2.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Not Started";

                    // Đóng popup tạo tác vụ
                    CreateTaskField.IsOpen = false;

                    // Thêm tác vụ vào cơ sở dữ liệu
                    using (SqlCommand command = new SqlCommand(@"INSERT INTO Task (Task_ID, List_ID, Title, StartDate, DueDate, Description, Priority, Status) 
                                                 VALUES (@TaskId, @ListId, @Title, @StartDate, @DueDate, @Description, @Priority, @Status)", connection))
                    {
                        // Thêm các tham số vào câu lệnh SQL
                        command.Parameters.AddWithValue("@TaskId", taskID);
                        command.Parameters.AddWithValue("@ListId", listID);
                        command.Parameters.AddWithValue("@Title", taskname);
                        command.Parameters.AddWithValue("@StartDate", startDate);
                        command.Parameters.AddWithValue("@DueDate", dueDate);
                        command.Parameters.AddWithValue("@Description", ""); // Bạn có thể thay đổi nếu cần
                        command.Parameters.AddWithValue("@Priority", selectedContent);
                        command.Parameters.AddWithValue("@Status", selectedStatus);

                        // Thực thi câu lệnh
                        command.ExecuteNonQuery();
                    }

                    // Tải lại danh sách các tác vụ để hiển thị
                    loadListContainer();
                }
            }
            catch (SqlException sqlEx)
            {
                // Cập nhật nội dung của TextBlock với thông báo lỗi SQL
                errorTextBlock.Text = $"An SQL error occurred: {sqlEx.Message}";
                errorTextBlock.Visibility = Visibility.Visible; // Hiện TextBlock
            }
            catch (Exception ex)
            {
                // Cập nhật nội dung của TextBlock với thông báo lỗi chung
                errorTextBlock.Text = $"An unexpected error occurred: {ex.Message}";
                errorTextBlock.Visibility = Visibility.Visible; // Hiện TextBlock
            }

            // Thêm TextBlock vào giao diện người dùng, ví dụ vào một Grid
            List_Container.Children.Add(errorTextBlock);
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
            // Xóa các thành phần ngoại trừ "addListBox"
            var elementsToKeep = new List<UIElement> { addListBox };
            for (int i = List_Container.Children.Count - 1; i >= 0; i--)
            {
                UIElement child = List_Container.Children[i];
                if (!elementsToKeep.Contains(child))
                {
                    List_Container.Children.RemoveAt(i);
                }
            }

            string currentProjectId = UserSession.CurrentProjectID;
            try
            {
                using (SqlConnection connection = new SqlConnection("Server=LAPTOP-KNDSJBLQ\\SQLEXPRESS;Database=TaskMaster;Trusted_Connection=True;TrustServerCertificate=True;"))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(@"SELECT List_ID, Name FROM List WHERE Project_ID = @ProjectId", connection))
                    {
                        command.Parameters.AddWithValue("@ProjectId", currentProjectId);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string listID = reader["List_ID"].ToString();
                                string listName = reader["Name"].ToString();

                                // Truyền cả listID và listName vào LoadListItem
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
        private void addList_Click(object sender, RoutedEventArgs e)
        {
            NewList_Btn.Visibility = Visibility.Collapsed;
            AddNewList_Btn.Visibility = Visibility.Visible;
            EnterNewNameList.Visibility = Visibility.Visible;
            Cancel_AddNewList_Btn.Visibility = Visibility.Visible;
        }
        private void NewList_Cancel(object sender, RoutedEventArgs e)
        {
            NewList_Btn.Visibility = Visibility.Visible;
            AddNewList_Btn.Visibility = Visibility.Collapsed;
            EnterNewNameList.Visibility = Visibility.Collapsed;
            Cancel_AddNewList_Btn.Visibility = Visibility.Collapsed;
        }
        //Hàm tạo nhãn dán nhỏ cho task
        private Button CreateLabelButton(string labelText)
        {
            // Tạo Button cho nhãn
            Button labelButton = new Button
            {
                Content = labelText,
                Style = (Style)Application.Current.Resources["CustomLabelStyle"] // Áp dụng style cho nhãn
            };
            return labelButton;
        }
        //Hàm tạo task (khung nhỏ)
        private Border CreateTaskItem(string taskName, DateTime? dueDate)
        {
            // Tạo Border cho nhiệm vụ
            Border taskBorder = new Border
            {
                Style = (Style)Application.Current.Resources["CustomTaskStyle"],
                Margin = new Thickness(0, 5, 0, 0)
            };

            // Tạo Grid bên trong Border
            Grid taskGrid = new Grid();

            // Định nghĩa các cột và hàng
            taskGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            taskGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            taskGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            taskGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            taskGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

            // Tạo TextBlock cho tên nhiệm vụ
            TextBlock taskNameTextBlock = new TextBlock
            {
                Text = taskName, // Sử dụng taskName từ tham số
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5, 0, 5)
            };
            Grid.SetColumn(taskNameTextBlock, 0); // Thiết lập cột cho TextBlock
            Grid.SetRow(taskNameTextBlock, 1);    // Thiết lập hàng cho TextBlock

            // Tạo nút "more" cho nhiệm vụ
            Button moreButton = new Button
            {
                Background = new SolidColorBrush(Colors.Transparent),
                Padding = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Right,
            };

            Image moreImage = new Image
            {
                Source = new BitmapImage(new Uri("ms-appx:///Assets/Image/moreTask.png")),
                Width = 12
            };
            moreButton.Content = moreImage;
            Grid.SetColumn(moreButton, 1); // Thiết lập cột cho nút "more"
            Grid.SetRow(moreButton, 0);     // Thiết lập hàng cho nút "more"

            // Tạo MenuFlyout cho nút "more"
            MenuFlyout moreMenuFlyout = new MenuFlyout
            {
                Placement = FlyoutPlacementMode.Bottom
            };
            moreMenuFlyout.Items.Add(new MenuFlyoutItem { Text = "Rename" });
            moreMenuFlyout.Items.Add(new MenuFlyoutItem { Text = "Delete task" });
            moreButton.Flyout = moreMenuFlyout;

            // Tạo VariableSizedWrapGrid cho nhãn
            VariableSizedWrapGrid labelGrid = new VariableSizedWrapGrid
            {
                Margin = new Thickness(0),
                Orientation = Orientation.Horizontal,
            };
            Grid.SetColumn(labelGrid, 0); // Thiết lập ColumnSpan cho VariableSizedWrapGrid
            Grid.SetRow(labelGrid, 0);         // Thiết lập hàng cho VariableSizedWrapGrid

            // Đọc dữ liệu label_task
            Button labelButton = CreateLabelButton("Label 1");
            labelGrid.Children.Add(labelButton);
            taskGrid.Children.Add(taskNameTextBlock);
            taskGrid.Children.Add(moreButton);
            taskGrid.Children.Add(labelGrid);

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

            // Đặt Grid vào Border
            taskBorder.Child = taskGrid;

            return taskBorder;
        }

        //Load 1 list cũ
        private StackPanel LoadListItem(string listName, string ListID)
        {
            StackPanel listItem = new StackPanel
            {
                Style = (Style)Application.Current.Resources["CustomListFrameStyle"],
                Tag = ListID  // Gán ListID cho Tag của listItem
            };

            StackPanel list_header = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Width = 300,
                Spacing = 240
            };

            // Tên danh sách
            TextBlock list_header_name = new TextBlock
            {
                Text = listName,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
            };

            // Thêm nút more ở danh sách
            Button list_header_more = new Button
            {
                Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
                Padding = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Image moreImage = new Image
            {
                Source = new BitmapImage(new Uri("ms-appx:///Assets/Image/moreList.png")),
                Width = 20
            };
            list_header_more.Content = moreImage;

            MenuFlyout menuFlyout = new MenuFlyout
            {
                Placement = FlyoutPlacementMode.Bottom
            };

            MenuFlyoutItem renameItem = new MenuFlyoutItem { Text = "Rename" };
            MenuFlyoutItem deleteItem = new MenuFlyoutItem { Text = "Delete list" };

            menuFlyout.Items.Add(renameItem);
            menuFlyout.Items.Add(deleteItem);
            list_header_more.Flyout = menuFlyout;

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
                HorizontalAlignment = HorizontalAlignment.Left,
                Tag = ListID  // Gán ListID cho Tag của addTaskButton
            };
            addTaskButton.Click += addTask_Click;
            stackPanel.Children.Add(addTaskButton);
            LoadTasksForList(ListID, stackPanel);
            scrollViewer.Content = stackPanel;

            list_header.Children.Add(list_header_name);
            list_header.Children.Add(list_header_more);
            listItem.Children.Add(list_header);
            listItem.Children.Add(scrollViewer);
            
            return listItem;
        }
        private void LoadTasksForList(string listID, StackPanel stackPanel)
        {
            using (SqlConnection connection = new SqlConnection("Server=LAPTOP-KNDSJBLQ\\SQLEXPRESS;Database=TaskMaster;Trusted_Connection=True;TrustServerCertificate=True;"))
            {
                connection.Open();

                string query = "SELECT Title, DueDate FROM Task WHERE List_ID = @ListId";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ListId", listID);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string taskTitle = reader["Title"].ToString();
                            DateTime? dueDate = reader["DueDate"] as DateTime?;

                            // Tạo Border cho từng nhiệm vụ
                            Border taskItem = CreateTaskItem(taskTitle, dueDate);

                            // Thêm Border vào StackPanel
                            stackPanel.Children.Add(taskItem);
                        }
                    }
                }
            }
        }

        private void NewList_Add(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection("Server=LAPTOP-KNDSJBLQ\\SQLEXPRESS;Database=TaskMaster;Trusted_Connection=True;TrustServerCertificate=True;"))
                {
                    connection.Open();
                    string newListId = GenerateListId(connection);
                    string Name = EnterNewNameList.Text;
                    string projectID = UserSession.CurrentProjectID;
                    Random random = new Random();
                    byte r = (byte)random.Next(256);
                    byte g = (byte)random.Next(256);
                    byte b = (byte)random.Next(256);
                    string ColorofList = $"#{r:X2}{g:X2}{b:X2}";
                    using (SqlCommand command = new SqlCommand("INSERT INTO List (List_ID, Project_ID, Name, Color) VALUES (@List_ID, @Project_ID, @Name, @Color)", connection))
                    {
                        command.Parameters.AddWithValue("@List_ID", newListId);
                        command.Parameters.AddWithValue("@Project_ID", projectID);
                        command.Parameters.AddWithValue("@Name", Name);
                        command.Parameters.AddWithValue("@Color", ColorofList);
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                    List_Container.Children.Add(LoadListItem(Name,newListId));
                }

            }
            catch (Exception ex)
            {
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
        public void openAccountPage_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MyAccountPage));
        }
    }
}
