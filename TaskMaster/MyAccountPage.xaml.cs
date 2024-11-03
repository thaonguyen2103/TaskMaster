using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using Windows.UI.Xaml; // Đảm bảo thêm namespace này

using Microsoft.Data.SqlClient;
using System.Drawing.Text;
using WinRT.Interop;
using Windows.Storage.Pickers;

namespace TaskMaster
{
    public sealed partial class MyAccountPage : Page
    {
   
        bool editStatus = false;

        public MyAccountPage()
        {
            string currentUsername = UserSession.CurrentUsername;
            LoadUserDataAsync(currentUsername);
            this.InitializeComponent();
        }

        private async void LoadUserDataAsync(string username)
        {
            try
            {
                using (SqlConnection connection = DatabaseConnection.GetConnection())
                {
                    await connection.OpenAsync();
                    string query = "SELECT Name, Email, Phone, Avatar, DOB FROM [User] WHERE Username = @Username";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                            {
                                // Hiển thị các trường thông tin người dùng
                                FullNameField.Text = reader["Name"].ToString();
                                EmailField.Text = reader["Email"].ToString();
                                PhoneNumberField.Text = reader["Phone"].ToString();
                                string avatarPath = reader["Avatar"].ToString();
                                if (!string.IsNullOrEmpty(avatarPath))
                                {
                                    Avatar.Source = new BitmapImage(new Uri(this.BaseUri, avatarPath));
                                }
                                BirthdayField.Date = DateTime.Parse(reader["DOB"].ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private async void UpdateUserDataAsync()
        {
            try
            {
                using (SqlConnection connection = DatabaseConnection.GetConnection())
                {
                    await connection.OpenAsync();
                    string query = "UPDATE [User] SET Name = @Name, Email = @Email, Phone = @Phone, Avatar = @Avatar, DOB = @DOB WHERE Username = @Username";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Name", FullNameField.Text);
                        cmd.Parameters.AddWithValue("@Email", EmailField.Text);
                        cmd.Parameters.AddWithValue("@Phone", PhoneNumberField.Text);

                        BitmapImage avatarImage = Avatar.Source as BitmapImage;
                        string avatarPath = avatarImage?.UriSource?.ToString() ?? "";
                        cmd.Parameters.AddWithValue("@Avatar", avatarPath);

                        // Kiểm tra nếu BirthdayField.Date có giá trị, nếu không thì gán DBNull.Value
                        if (BirthdayField.Date.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@DOB", BirthdayField.Date.Value.ToString("yyyy-MM-dd"));
                        }
                        else
                        {
                            cmd.Parameters.AddWithValue("@DOB", DBNull.Value);
                        }
                        // Thêm tham số @Username
                        string currentUsername = UserSession.CurrentUsername;
                        cmd.Parameters.AddWithValue("@Username", currentUsername);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                SaveEdit.Visibility = Visibility.Collapsed;
                editStatus = false;
                ContentDialog successDialog = new ContentDialog
                {
                    Title = "Update Successful",
                    Content = "Your information has been updated.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await successDialog.ShowAsync();
            }
            catch (Exception ex)
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Lỗi",
                    Content = $"Không thể cập nhật thông tin: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }



        private void SaveEdit_Click(object sender, RoutedEventArgs e)
        {
            UpdateUserDataAsync();
        }

        private void isEditing()
        {
            if (FullNameField.IsEnabled || BirthdayField.IsEnabled || EmailField.IsEnabled || editStatus || PhoneNumberField.IsEnabled)
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
