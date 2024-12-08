
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Diagnostics;
using TaskMaster.ViewModels;
namespace TaskMaster.Views
{
    public sealed partial class MyTask : Page
    {
        private MyTaskViewModel viewModel;
        public MyTask()
        {
            this.InitializeComponent();
            viewModel = new MyTaskViewModel();
            viewModel.Toggle = () => { AccountPopup.IsOpen = !AccountPopup.IsOpen; };
            viewModel.NavigateMyAccount = () => { Frame.Navigate(typeof(MyAccountPage)); };
            viewModel.LogOut = () => { Frame.Navigate(typeof(LoginPage)); };

            //viewModel.ChooseColorAnimation += chooseProjectColor_Click;
            viewModel.NavigateMainPage = () => { Frame.Navigate(typeof(MainPage)); };
            viewModel.NavigateToProject = () => { Frame.Navigate(typeof(Project)); };
            this.DataContext = viewModel;
        }
        private void GridTask_Loaded(object sender, RoutedEventArgs e)
        {
            Button gridTaskButton = sender as Button;
            if (gridTaskButton != null)
            {
                // Sử dụng FindName trực tiếp từ Button thay vì từ Template
                var taskElementGrid = gridTaskButton.FindName("TaskElement") as Grid;
                if (taskElementGrid != null)
                {
                    taskElementGrid.Width = gridTaskButton.ActualWidth;
                    taskElementGrid.Height = gridTaskButton.ActualHeight;
                }
            }
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            try
            {
                // Tải avatar người dùng
                await viewModel.LoadUserAvatar(UserSession.CurrentUserID);
                
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                Debug.WriteLine($"Error loading user avatar: {ex.Message}");
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
