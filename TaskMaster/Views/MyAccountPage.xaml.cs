using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using System;

using Microsoft.Data.SqlClient;
using System.Drawing.Text;
using WinRT.Interop;
using Windows.Storage.Pickers;
using TaskMaster.ViewModels;

namespace TaskMaster.Views
{
    public sealed partial class MyAccountPage : Page
    {

        private MyAccountViewModel viewModel;

        public MyAccountPage()
        {
            this.InitializeComponent();
            viewModel = new MyAccountViewModel();
            this.DataContext = viewModel;
            string CurrentUserID = UserSession.CurrentUserID;
            viewModel.LoadUserDataAsync(CurrentUserID);
            viewModel.OpenAvatarPicker += changeAvatar_Click;
            viewModel.BackToMainPage = backtoHome_Click;
            viewModel.UpdateSuccess += OnUpdateSuccess;
        }
       
        private void changeAvatar_Click(object sender, EventArgs e)
        {
            chooseAvatar.IsOpen = true;
        }

        private void AvatarImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is Image selectedImage && selectedImage.Tag is string imagePath)
            {
                Avatar.Source = new BitmapImage(new Uri(this.BaseUri, imagePath));
                chooseAvatar.IsOpen = false;
            }
        }
        private void backtoHome_Click()
        {
            Frame.Navigate(typeof(MainPage));
        }

        private async void OnUpdateSuccess(object sender, EventArgs e)
        {
            
            ContentDialog successDialog = new ContentDialog
            {
                Title = "Update Successful",
                Content = "Your information has been updated.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await successDialog.ShowAsync();
        }

    }
}
