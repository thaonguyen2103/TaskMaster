using Microsoft.Data.SqlClient;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Threading.Tasks;
using TaskMaster.ViewModels;

namespace TaskMaster.Views
{
    public sealed partial class RegisterPage : Page
    {
        public RegisterViewModel viewModel { get; }
        public RegisterPage()
        {
            this.InitializeComponent();
            ShowRegisterPanelAfterDelay();
            viewModel = new RegisterViewModel();
            DataContext = viewModel;
            viewModel.LogInNavigation = () => { Frame.Navigate(typeof(LoginPage)); };
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            FadeInWelcomeStoryboard.Begin();
        }

        private void ShowRegisterPanelAfterDelay()
        {
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(3)
            };
            timer.Tick += (sender, e) =>
            {
                timer.Stop();
                WelcomeTextBlock.Visibility = Visibility.Collapsed; // Hide welcome message
                RegisterStackPanel.Visibility = Visibility.Visible;
                FadeInRegisterStoryboard.Begin(); // Show registration panel
            };
            timer.Start(); // Start the timer
        }
    }
}
