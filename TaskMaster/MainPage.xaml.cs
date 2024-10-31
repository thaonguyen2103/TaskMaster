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
        }
        // Xử lý khi người dùng nhấn vào My Project
        private void MyProjectButton_Click(object sender, RoutedEventArgs e)
        {
            showProjectContainer();
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
        private void MyAccountButton_Click(Object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MyAccountPage));
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
        //Tạo dự án mới
        private void SaveNewProject_Click(object sender, RoutedEventArgs e)
        {
            CreateProjectField.IsOpen = false;
            Button newProject = new Button
            {
                Content = CreateProject_Name.Text,
                Style = (Style)Application.Current.Resources["CustomProjectIconStyle"],

            };
            newProject.Click += ProjectView_Click;
            ProjectList.Children.Add(newProject);

        }
        private void ProjectView_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Project));
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
        //Tham gia một dự án có sẵn
        private void SaveExistingProject_Click(object sender, RoutedEventArgs e)
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
