using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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
        private Popup list_more;
        private void List_more_open(object sender, TappedRoutedEventArgs e)
        {
            if (list_more == null)
            {
                list_more = new Popup
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    IsOpen = false
                };
                StackPanel stackPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Padding = new Thickness(5),
                    Background = new SolidColorBrush(Microsoft.UI.Colors.White),
                    BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Black),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8)
                };

                Button deleteButton = new Button
                {
                    Content = "Delete",
                    Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
                    FontSize = 13
                };
                stackPanel.Children.Add(deleteButton);

                Button renameButton = new Button
                {
                    Content = "Rename",
                    Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
                    FontSize = 13
                };
                stackPanel.Children.Add(renameButton);

                list_more.Child = stackPanel;
            }
            var image = sender as Image;
            if (image != null)
            {
                list_more.XamlRoot = this.XamlRoot;
                if (list_more.IsOpen)
                {
                    list_more.IsOpen = false;
                }
                else
                {
                    var position = image.TransformToVisual(this).TransformPoint(new Point(0, 0));
                    list_more.HorizontalOffset = position.X + image.ActualWidth;
                    list_more.VerticalOffset = position.Y;
                    list_more.IsOpen = true;
                }
            }
        }
        private void addTask_Click(object sender, RoutedEventArgs e)
        {
            CreateTaskField.IsOpen = true;
        }
        private Popup task_more;
        private void Task_more_open(object sender, TappedRoutedEventArgs e)
        {
            if (task_more == null)
            {
                task_more = new Popup
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    IsOpen = false
                };
                StackPanel stackPanel = new StackPanel
                {
                    Orientation = Orientation.Vertical,
                    Padding = new Thickness(5),
                    Background = new SolidColorBrush(Microsoft.UI.Colors.White),
                    BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Black),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8)
                };
                Button deleteButton = new Button
                {
                    Content = "Delete",
                    Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
                    FontSize = 13
                };
                stackPanel.Children.Add(deleteButton);
                Button renameButton = new Button
                {
                    Content = "Rename",
                    Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
                    FontSize = 13
                };
                stackPanel.Children.Add(renameButton);

                task_more.Child = stackPanel;
            }
            var image = sender as Image;
            if (image != null)
            {
                task_more.XamlRoot = this.XamlRoot;

                if (task_more.IsOpen)
                {
                    task_more.IsOpen = false;
                }
                else
                {
                    var position = image.TransformToVisual(this).TransformPoint(new Point(0, 0));
                    task_more.HorizontalOffset = position.X + image.ActualWidth;
                    task_more.VerticalOffset = position.Y;
                    task_more.IsOpen = true;
                }
            }
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
        private void NewList_Add(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection("Server=MSI\\SQLEXPRESS;Database=TaskMaster;Trusted_Connection=True;TrustServerCertificate=True;"))
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
                }
            }
            catch (Exception ex)
            {
                StatusTextBox.Text = "Error: " + ex.Message;
            }
        }

        private void LoadUserLists()
        {

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
        private void list_more_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
