using Microsoft.Data.SqlClient;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using TaskMaster.ViewModels;
using Windows.Networking.NetworkOperators;

namespace TaskMaster.Views
{
    public sealed partial class LoginPage : Page
    {
        public LoginViewModel viewModel { get; }
        public LoginPage()
        {
            InitializeComponent();
            viewModel = new LoginViewModel();
            viewModel.LoginSuccess = NavigateToMainPage;
            DataContext = viewModel;
            viewModel.RegisterPageNavigation = OnRegisterPageNavigation;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox && DataContext is LoginViewModel viewModel)
            {
                viewModel.Password = passwordBox.Password;
            }
        }

        private void NavigateToMainPage()
        {
            Frame.Navigate(typeof(MainPage));
        }
        private void OnRegisterPageNavigation()
        {
            Frame.Navigate(typeof(RegisterPage));
        }
    }
}
