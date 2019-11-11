using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RCloud.Config
{
    /// <summary>
    /// WindowConfigSystem.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WindowConfigSystem : Window
    {
        AutoRunRegister autoRun = new AutoRunRegister();
        bool IsUpdate = true;
        bool needUpdate = false;
        bool IsDefaultAutoRun = false;
        PropertiesSystem properties = null;
        public WindowConfigSystem()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IsDefaultAutoRun = autoRun.IsStartUp();
            properties = PropertiesSetting.DefaultSetting.Property;
            PropertyToUi();

            textBoxFtpUri.TextChanged += textBox_TextChanged;
            textBoxFtpUser.TextChanged += textBox_TextChanged;
            passwordBoxFtpPw.PasswordChanged += passwordBoxFtpPw_PasswordChanged;
            textBoxFolder.TextChanged += textBox_TextChanged;

            IsUpdate = false;

            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Major.ToString(); //AssemblyVersion을 가져온다.
            version += "." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString();
            version += "." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Build.ToString();
            Title = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + " " + version;
        }

        private void PropertyToUi()
        {
            checkBoxStartUpAutoRun.IsChecked = IsDefaultAutoRun;

            radioButtonFTP.IsChecked = (properties.UpdateDrvieType == DriveType.FTP);
            radioButtonDrive.IsChecked = (properties.UpdateDrvieType == DriveType.Disk);
            
            if (properties.UpdateEnable)
            {
                checkBoxAutoUpdate.IsChecked = true;
                radioButtonFTP.IsEnabled = true;
                radioButtonDrive.IsEnabled = true;
            }
            else
            {
                checkBoxAutoUpdate.IsChecked = false;
                radioButtonFTP.IsEnabled = false;
                radioButtonDrive.IsEnabled = false;
                textBoxFtpUri.IsEnabled = false;
                textBoxFtpUser.IsEnabled = false;
                passwordBoxFtpPw.IsEnabled = false;
                buttonFtpCheck.IsEnabled = false;
                textBoxFolder.IsEnabled = false;
                buttonFolder.IsEnabled = false;
            }

            textBoxFtpUri.Text = properties.UpdateFtpUri;
            textBoxFtpUser.Text = properties.UpdateFtpUser;
            passwordBoxFtpPw.Password = properties.UpdateFtpPw;
            textBoxFolder.Text = properties.UpdateDiskUri;
        }

        private void UiToProperty()
        {
            IsDefaultAutoRun = checkBoxStartUpAutoRun.IsChecked == true;

            properties.UpdateEnable = (checkBoxAutoUpdate.IsChecked == true);
            properties.UpdateDrvieType = (radioButtonFTP.IsChecked == true) ? DriveType.FTP : DriveType.Disk;
            properties.UpdateFtpUri = textBoxFtpUri.Text;
            properties.UpdateFtpUser = textBoxFtpUser.Text;
            properties.UpdateFtpPw = passwordBoxFtpPw.Password;
            properties.UpdateDiskUri = textBoxFolder.Text;
        }
        private void SaveButtonEnable()
        {
            if (IsUpdate)
                return;
            if (needUpdate == false)
            {
                needUpdate = true;
                buttonSave.IsEnabled = true;
            }
        }
        private void checkBoxStartUpAutoRun_Click(object sender, RoutedEventArgs e)
        {
            SaveButtonEnable();
        }
        private void checkBoxAutoUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (checkBoxAutoUpdate.IsChecked == false)
            {
                radioButtonFTP.IsEnabled = false;
                radioButtonDrive.IsEnabled = false;
                textBoxFtpUri.IsEnabled = false;
                textBoxFtpUser.IsEnabled = false;
                passwordBoxFtpPw.IsEnabled = false;
                buttonFtpCheck.IsEnabled = false;
                textBoxFolder.IsEnabled = false;
                buttonFolder.IsEnabled = false;
            }
            else
            {
                radioButtonFTP.IsEnabled = true;
                radioButtonDrive.IsEnabled = true;
                textBoxFtpUri.IsEnabled = (radioButtonFTP.IsChecked == true);
                textBoxFtpUser.IsEnabled = (radioButtonFTP.IsChecked == true);
                passwordBoxFtpPw.IsEnabled = (radioButtonFTP.IsChecked == true);
                buttonFtpCheck.IsEnabled = (radioButtonFTP.IsChecked == true);
                textBoxFolder.IsEnabled = (radioButtonFTP.IsChecked != true);
                buttonFolder.IsEnabled = (radioButtonFTP.IsChecked != true);
            }
            SaveButtonEnable();
        }

        private void radioButtonFTP_Checked(object sender, RoutedEventArgs e)
        {
            textBoxFtpUri.IsEnabled = (radioButtonFTP.IsChecked == true);
            textBoxFtpUser.IsEnabled = (radioButtonFTP.IsChecked == true);
            passwordBoxFtpPw.IsEnabled = (radioButtonFTP.IsChecked == true);
            buttonFtpCheck.IsEnabled = (radioButtonFTP.IsChecked == true);
            textBoxFolder.IsEnabled = (radioButtonFTP.IsChecked != true);
            buttonFolder.IsEnabled = (radioButtonFTP.IsChecked != true);
            SaveButtonEnable();
        }

        private void radioButtonDrive_Checked(object sender, RoutedEventArgs e)
        {
            textBoxFtpUri.IsEnabled = (radioButtonFTP.IsChecked == true);
            textBoxFtpUser.IsEnabled = (radioButtonFTP.IsChecked == true);
            passwordBoxFtpPw.IsEnabled = (radioButtonFTP.IsChecked == true);
            buttonFtpCheck.IsEnabled = (radioButtonFTP.IsChecked == true);
            textBoxFolder.IsEnabled = (radioButtonFTP.IsChecked != true);
            buttonFolder.IsEnabled = (radioButtonFTP.IsChecked != true);
            SaveButtonEnable();
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            UiToProperty();

            if (IsDefaultAutoRun == true)
                autoRun.RegistrationStartUp();
            else
                autoRun.DeleteStartUp();

            PropertiesSetting.DefaultSetting.Property = properties;
            PropertiesSetting.DefaultSetting.Save();
            Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveButtonEnable();
        }

        private void passwordBoxFtpPw_PasswordChanged(object sender, RoutedEventArgs e)
        {
            SaveButtonEnable();
        }

        private void buttonFtpCheck_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Input.Cursor oldCursor = Mouse.OverrideCursor;
            try
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

                ServiceFtp ftp = new ServiceFtp(textBoxFtpUser.Text, passwordBoxFtpPw.Password, 5000, textBoxFtpUri.Text);
                if (ftp.UriPathExist() == true)
                {
                    Mouse.OverrideCursor = oldCursor;
                    MessageBox.Show(this, "FTP Check OK");
                }
                else
                {
                    Mouse.OverrideCursor = oldCursor;
                    MessageBox.Show(this, "FTP Check Fail!");
                }
            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = oldCursor;
                MessageBox.Show(this, "FTP Check Fail!, " + ex.Message);
            }
        }

        private void buttonFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "Select disk folder";
            if (textBoxFolder.Text != "")
                dialog.SelectedPath = textBoxFolder.Text;
            try
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                    textBoxFolder.Text = dialog.SelectedPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }
    }
}
