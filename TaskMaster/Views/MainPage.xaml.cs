
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using TaskMaster.ViewModels;
using System.Threading.Tasks;

namespace TaskMaster.Views
{
    public sealed partial class MainPage : Page
    {

        private MainPageViewModel viewModel;
        public MainPage()
        {
            viewModel = new MainPageViewModel();
            this.InitializeComponent();
            viewModel.Toggle = () => { AccountPopup.IsOpen = !AccountPopup.IsOpen; };
            viewModel.NavigateMyAccount = () => { Frame.Navigate(typeof(MyAccountPage)); };
            viewModel.LogOut = () => { Frame.Navigate(typeof(LoginPage)); };
            viewModel.UpdateProjectList = () => { Frame.Navigate(typeof(MainPage)); };
            viewModel.NavigateMyTask = () => { Frame.Navigate(typeof(MyTask)); };
            //viewModel.ChooseColorAnimation += chooseProjectColor_Click;
            viewModel.NavigateToProject = () => { Frame.Navigate(typeof(Project)); };
            this.DataContext = viewModel;
        }

        private void Button_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var button = sender as Button;
            var scaleTransform = new ScaleTransform
            {
                ScaleX = 1.5,
                ScaleY = 1.5
            };
            button.RenderTransform = scaleTransform;
            button.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
        }
        private void Button_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var button = sender as Button;
            button.RenderTransform = null;
        }
        //private void chooseProjectColor_Click(object sender)
        //{
        //    var buttons = new List<Button>
        //    {
        //        ColorGradientDefault,
        //        ColorGradientBluePink,
        //        ColorGradientYellowPurple,
        //        ColorGradientRed,
        //        ColorGradientPink,
        //        ColorGradientBlue,
        //        ColorGradientGreen,
        //        ColorGradientYellow
        //    };
        //    foreach (var btn in buttons)
        //    {
        //        btn.Width = 25;
        //        btn.Height = 25;
        //        if (btn.Name != "ColorGradientDefault")
        //        {
        //            btn.BorderBrush = null;
        //            btn.BorderThickness = new Thickness(0);
        //        }
        //        else
        //        {
        //            btn.BorderThickness = new Thickness(1);
        //        }

        //    }
        //    if (sender is Button button)
        //    {
        //        button.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Black);
        //        button.BorderThickness = new Thickness(2);
        //        button.Width = 33;
        //        button.Height = 33;
        //    }
        //}

        private void NotificationButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ProjectView_Click(object sender, RoutedEventArgs e)
        {
            string projectName = (sender as Button).Content.ToString();
            UserSession.CurrentProjectName = projectName;
            using (SqlConnection connection = DatabaseConnection.GetConnection())
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
            Frame.Navigate(typeof(Project));
        }

    }
}
