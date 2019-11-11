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
    /// WindowConfigSchedule.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WindowConfigSchedule : Window
    {
        Dictionary<string, FileSchedule.ScheduleProperty> m_schedule = null;
        bool m_isChange = false;
        public WindowConfigSchedule(Dictionary<string, FileSchedule.ScheduleProperty> schedule)
        {
            InitializeComponent();
            m_schedule = new Dictionary<string, FileSchedule.ScheduleProperty>();
            foreach (var prop in schedule)
                m_schedule.Add(prop.Key, prop.Value.Clone());
        }

        private void OnContentRendered(object sender, EventArgs e)
        {
            dataGrid.ItemsSource = m_schedule;
            buttonEdit.IsEnabled = false;
            buttonDelete.IsEnabled = false;

            buttonSave.IsEnabled = false;

            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Major.ToString(); //AssemblyVersion을 가져온다.
            version += "." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString();
            version += "." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Build.ToString();
            Title = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + " " + version;
        }
        private void EnableSaveButton()
        {
            buttonSave.IsEnabled = true;
        }
        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGrid.SelectedItem != null)
            {
                buttonEdit.IsEnabled = true;
                buttonDelete.IsEnabled = true;
            }
            else
            {
                buttonEdit.IsEnabled = false;
                buttonDelete.IsEnabled = false;
            } 
        }
        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            WindowProperty winSetting = new WindowProperty(new FileSchedule.ScheduleProperty());
            winSetting.Owner = this;
            winSetting.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            winSetting.ShowDialog();

            if (winSetting.IsChanged == true)
            {
                FileSchedule.ScheduleProperty property = winSetting.Property;
                if (m_schedule.ContainsKey(property.Name) == true)
                    MessageBox.Show(this, "Same Name");
                else
                    m_schedule.Add(property.Name, property);
                dataGrid.Items.Refresh();
                EnableSaveButton();
            }
        }

        private void buttonEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItem == null)
                return;
            KeyValuePair<string, FileSchedule.ScheduleProperty> ekySchedule = (KeyValuePair < string, FileSchedule.ScheduleProperty > )dataGrid.SelectedItem;
            FileSchedule.ScheduleProperty schedule = ekySchedule.Value as FileSchedule.ScheduleProperty;
            if (schedule == null)
                return;
            WindowProperty winSetting = new WindowProperty(schedule, true);
            winSetting.Owner = this;
            winSetting.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            winSetting.ShowDialog();
            if (winSetting.IsChanged == true)
            {
                dataGrid.Items.Refresh();
                EnableSaveButton();
            }
        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItem == null)
                return;
            KeyValuePair<string, FileSchedule.ScheduleProperty> ekySchedule = (KeyValuePair<string, FileSchedule.ScheduleProperty>)dataGrid.SelectedItem;
            FileSchedule.ScheduleProperty schedule = ekySchedule.Value as FileSchedule.ScheduleProperty;
            if (schedule == null)
                return;
            MessageBoxResult msgResult = MessageBox.Show(this, "Do you want to delete(" + schedule.Name + ")?", "Delete", MessageBoxButton.YesNo);
            if (msgResult == MessageBoxResult.No)
                return;
            if (m_schedule.ContainsKey(schedule.Name) == true)
                m_schedule.Remove(schedule.Name);

            dataGrid.Items.Refresh();
            EnableSaveButton();
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {
            m_isChange = true;
            Close();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            m_isChange = false;
            Close();
        }
        public bool IsChanged
        {
            get{ return m_isChange; }
        }
        public Dictionary<string, FileSchedule.ScheduleProperty> ScheduleItem
        {
            get { return m_schedule; }
        }
    }
}
