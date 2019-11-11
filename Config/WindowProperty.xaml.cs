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
    /// WindowDriveSetting.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WindowProperty : Window
    {
        FileSchedule.ScheduleProperty m_property = null;
        bool m_isChageProperty = false;
        bool m_isEditWindow = false;
        public WindowProperty(FileSchedule.ScheduleProperty property, bool isEdit = false)
        {
            InitializeComponent();
            m_property = property;
            m_isEditWindow = isEdit;
            m_isChageProperty = false;
            buttonSave.IsEnabled = false;

            buttonSave.IsEnabled = false;

            checkBoxTimer.Click += checkBoxWork_Checked;
            checkBoxPCTurnON.Click += checkBoxWork_Checked;
            checkBoxPCTurnOff.Click += checkBoxWork_Checked;
            checkBoxDataChanges.Click += checkBoxWork_Checked;

            textBoxRepeatTime.TextChanged += TextBox_TextChanged;
            textBoxDelayTime.TextChanged += TextBox_TextChanged;

            textBoxScheduleName.TextChanged += TextBox_TextChanged;

            radioButtonFromFTP.Checked += RadioButtonFromConnect_Checked;
            radioButtonFromDrive.Checked += RadioButtonFromConnect_Checked;

            textBoxFromFtpUri.TextChanged += TextBox_TextChanged;
            textBoxFromFtpUser.TextChanged += TextBox_TextChanged;
            passwordBoxFromFtpPw.PasswordChanged += PasswordBoxFtpPw_PasswordChanged;
            textBoxFromFolder.TextChanged += TextBox_TextChanged;

            radioButtonFromCopy.Checked += RadioButtonFromCopy_Checked;
            radioButtonFromMove.Checked += RadioButtonFromCopy_Checked;

            radioButtonToFTP.Checked += RadioButtonToConnect_Checked;
            radioButtonToDrive.Checked += RadioButtonToConnect_Checked;

            textBoxToFtpUri.TextChanged += TextBox_TextChanged;
            textBoxToFtpUser.TextChanged += TextBox_TextChanged;
            passwordBoxToFtpPw.PasswordChanged += PasswordBoxFtpPw_PasswordChanged;
            textBoxToFolder.TextChanged += TextBox_TextChanged;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            PropertyToUi(m_property);
            m_isChageProperty = false;
            buttonSave.IsEnabled = false;

            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Major.ToString(); //AssemblyVersion을 가져온다.
            version += "." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString();
            version += "." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Build.ToString();
            Title = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + " " + version;
        }
        private void PropertyToUi(FileSchedule.ScheduleProperty property)
        {
            checkBoxTimer.IsChecked = property.RepeateTime;
            checkBoxPCTurnON.IsChecked = property.PCTurnOn;
            checkBoxPCTurnOff.IsChecked = property.PCTurnOff;
            checkBoxDataChanges.IsChecked = property.PCDateChanges;

            textBoxRepeatTime.Text = property.RepeatTimeMinute.ToString();
            textBoxDelayTime.Text = property.DelayTimeMinute.ToString();

            textBoxScheduleName.Text = property.Name;

            if (property.FromDiskType == DriveType.FTP)
                radioButtonFromFTP.IsChecked = true;
            else if (property.FromDiskType == DriveType.Disk)
                radioButtonFromDrive.IsChecked = true;

            textBoxFromFtpUri.Text = property.FromFtpUri;
            textBoxFromFtpUser.Text = property.FromFtpUser;
            passwordBoxFromFtpPw.Password = property.FromFtpPw;

            textBoxFromFolder.Text = property.FromDrivePath;
            if (property.FromSyncType == FileSchedule.SyncType.Copy)
                radioButtonFromCopy.IsChecked = true;
            else if (property.FromSyncType == FileSchedule.SyncType.Move)
                radioButtonFromMove.IsChecked = true;

            if (property.ToDiskType == DriveType.FTP)
                radioButtonToFTP.IsChecked = true;
            else if (property.ToDiskType == DriveType.Disk)
                radioButtonToDrive.IsChecked = true;

            textBoxToFtpUri.Text = property.ToFtpUri;
            textBoxToFtpUser.Text = property.ToFtpUser;
            passwordBoxToFtpPw.Password = property.ToFtpPw;
            textBoxToFolder.Text = property.ToDrivePath;

            textBoxScheduleName.IsEnabled = m_isEditWindow == false;
        }

        private bool UiToProperty(FileSchedule.ScheduleProperty property)
        {
            if(textBoxScheduleName.Text == string.Empty)
            {
                Keyboard.Focus(textBoxScheduleName);
                return false;
            }

            property.RepeateTime = checkBoxTimer.IsChecked == true ;
            property.PCTurnOn = checkBoxPCTurnON.IsChecked == true;
            property.PCTurnOff = checkBoxPCTurnOff.IsChecked == true;
            property.PCDateChanges = checkBoxDataChanges.IsChecked == true;

            property.RepeatTimeMinute = Convert.ToInt32( textBoxRepeatTime.Text );
            property.DelayTimeMinute = Convert.ToInt32( textBoxDelayTime.Text );

            property.Name = textBoxScheduleName.Text;
            if (radioButtonFromFTP.IsChecked == true)
                property.FromDiskType = DriveType.FTP;
            else //radioButtonFromDrive.IsChecked == true
                property.FromDiskType = DriveType.Disk;

            property.FromFtpUri = textBoxFromFtpUri.Text;
            property.FromFtpUser = textBoxFromFtpUser.Text;
            property.FromFtpPw = passwordBoxFromFtpPw.Password;

            property.FromDrivePath = textBoxFromFolder.Text;
            if (radioButtonFromCopy.IsChecked == true)
                property.FromSyncType = FileSchedule.SyncType.Copy;
            else if (radioButtonFromMove.IsChecked == true)
                property.FromSyncType = FileSchedule.SyncType.Move;

            if (radioButtonToFTP.IsChecked == true)
                property.ToDiskType = DriveType.FTP;
            else if (radioButtonToDrive.IsChecked == true)
                property.ToDiskType = DriveType.Disk;

            property.ToFtpUri = textBoxToFtpUri.Text;
            property.ToFtpUser = textBoxToFtpUser.Text;
            property.ToFtpPw = passwordBoxToFtpPw.Password;
            property.ToDrivePath = textBoxToFolder.Text;
            return true;
        }

        private void EnableSaveButton()
        {
            if(m_isChageProperty == false)
            {
                buttonSave.IsEnabled = true;
                m_isChageProperty = true;
            }
        }

        private void checkBoxWork_Checked(object sender, RoutedEventArgs e)
        {
            EnableSaveButton();
            textBoxRepeatTime.IsEnabled = checkBoxTimer.IsChecked == true;
            textBoxDelayTime.IsEnabled = checkBoxPCTurnON.IsChecked == true;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            EnableSaveButton();
        }

        private void RadioButtonFromConnect_Checked(object sender, RoutedEventArgs e)
        {
            EnableSaveButton();

            textBoxFromFtpUri.IsEnabled = radioButtonFromFTP.IsChecked == true;
            textBoxFromFtpUser.IsEnabled = radioButtonFromFTP.IsChecked == true;
            passwordBoxFromFtpPw.IsEnabled = radioButtonFromFTP.IsChecked == true;
            buttonFromFtpCheck.IsEnabled = radioButtonFromFTP.IsChecked == true;

            textBoxFromFolder.IsEnabled = radioButtonFromDrive.IsChecked == true;
            buttonFromFolder.IsEnabled = radioButtonFromDrive.IsChecked == true;
        }


        private void RadioButtonFromCopy_Checked(object sender, RoutedEventArgs e)
        {
            EnableSaveButton();
        }
        private void PasswordBoxFtpPw_PasswordChanged(object sender, RoutedEventArgs e)
        {
            EnableSaveButton();
        }

        private void RadioButtonToConnect_Checked(object sender, RoutedEventArgs e)
        {
            EnableSaveButton();

            textBoxToFtpUri.IsEnabled = radioButtonToFTP.IsChecked == true;
            textBoxToFtpUser.IsEnabled = radioButtonToFTP.IsChecked == true;
            passwordBoxToFtpPw.IsEnabled = radioButtonToFTP.IsChecked == true;
            buttonToFtpCheck.IsEnabled = radioButtonToFTP.IsChecked == true;

            textBoxFromFolder.IsEnabled = radioButtonToDrive.IsChecked == true;
            buttonToFolder.IsEnabled = radioButtonToDrive.IsChecked == true;
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            m_isChageProperty = false;
            Close();
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            if(m_isChageProperty)
            {
                if(UiToProperty(m_property) == true)
                    Close();
            }
        }
        public bool IsChanged
        {
            get { return m_isChageProperty; }
        }
        public FileSchedule.ScheduleProperty Property
        {
            set
            {
                m_property = value;
                PropertyToUi(m_property);
            }
            get { return m_property; }
        }

        private void buttonFromFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "Select disk folder";
            if (textBoxFromFolder.Text != "")
                dialog.SelectedPath = textBoxFromFolder.Text;
            try
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                    textBoxFromFolder.Text = dialog.SelectedPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        

        private void buttonToFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "Select disk folder";
            if (textBoxToFolder.Text != "")
                dialog.SelectedPath = textBoxToFolder.Text;
            try
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                    textBoxToFolder.Text = dialog.SelectedPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }
        private void buttonFromFtpCheck_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Input.Cursor oldCursor = Mouse.OverrideCursor;
            try
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

                ServiceFtp ftp = new ServiceFtp(textBoxFromFtpUser.Text, passwordBoxFromFtpPw.Password, 5000, textBoxFromFtpUri.Text);
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
        private void buttonToFtpCheck_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Input.Cursor oldCursor = Mouse.OverrideCursor;
            try
            {
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

                ServiceFtp ftp = new ServiceFtp(textBoxToFtpUser.Text, passwordBoxToFtpPw.Password, 5000, textBoxToFtpUri.Text);
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
    }
}
