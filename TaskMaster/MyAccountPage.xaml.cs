using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
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
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MyAccountPage : Page
    {
        bool editStatus = false;
        public MyAccountPage()
        {
            this.InitializeComponent();
        }
        private void isEditing()
        {
            if (FullNameField.IsEnabled || BirthdayField.IsEnabled || EmailField.IsEnabled || SocialLinkField.IsEnabled || editStatus || PhoneNumberField.IsEnabled)
            {
                SaveEdit.Visibility = Visibility.Visible;
            }
            else
            {
                SaveEdit.Visibility = Visibility.Collapsed;
            }

        }
        private void editFullName(object sender, RoutedEventArgs e)
        {
            FullNameField.IsEnabled = true;
            isEditing();

        }
        private void editDOB(object sender, RoutedEventArgs e)
        {
            BirthdayField.IsEnabled = !(BirthdayField.IsEnabled);
            isEditing();
        }
        private void editPhoneNumber(object sender, RoutedEventArgs e)
        {
            PhoneNumberField.IsEnabled = true;
            isEditing();
        }
        private void editEmail(object sender, RoutedEventArgs e)
        {
            EmailField.IsEnabled = true;
            isEditing();
        }
        private void editSocialLink(object sender, RoutedEventArgs e)
        {
            SocialLinkField.IsEnabled = true;
            isEditing();
        }
        private void changeAvatar_Click(object sender, RoutedEventArgs e)
        {
            chooseAvatar.IsOpen = true;
        }
        private void AvatarImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is Image selectedImage && selectedImage.Tag is string imagePath)
            {
                Avatar.Source = new BitmapImage(new Uri(this.BaseUri, imagePath));
                chooseAvatar.IsOpen = false;
                editStatus = true;
                isEditing();
            }
        }
        private void backtoHome_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }
    }
}
