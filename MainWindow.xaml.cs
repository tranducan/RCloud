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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using Microsoft.Win32;
using System.IO;

namespace RCloud
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        EventBroker.EventObserver m_observerLog = null;
        EventBroker.EventObserver m_observerFileSync = null;
        EventBroker.EventObserver m_observerUpdateMe = null;
        EventBroker.EventParam m_timerEvent = null;

        FlowDocument m_flowDoc = null;
        Config.WindowConfigSystem m_windowConfigSystem = null;
        Config.WindowConfigSchedule m_windowConfigSchedule = null;
        Config.SchedulePropertiesSetting m_propertySetting = null;
        ScheduleProcess m_schedule = new ScheduleProcess();

        System.Windows.Forms.NotifyIcon m_notify = null;
        bool m_programShowDown = false;
        bool m_disableProgramUpdate = false;
        public MainWindow()
        {
            System.Diagnostics.Process thisproc = System.Diagnostics.Process.GetCurrentProcess();
            System.Diagnostics.Process[] procs = System.Diagnostics.Process.GetProcessesByName(thisproc.ProcessName);
            if (procs.Count() > 1)
            {
                MessageBox.Show("Already Run!", thisproc.ProcessName);
                Close();
                return;
            }

            InitializeComponent();

            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Major.ToString(); //AssemblyVersion을 가져온다.
            version += "." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Minor.ToString();
            version += "." + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Build.ToString();
            Title = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + " " + version;
            LabelHeader.Content = Title;
            LabelVersion.Content += ": " + version;
            m_flowDoc = richTextBox.Document;
            m_flowDoc.LineHeight = 2;
            richTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            this.ShowInTaskbar = false;

            m_observerLog = new EventBroker.EventObserver(OnReceiveLog);
            EventBroker.AddObserver(EventBroker.EventID.etLog, m_observerLog);

            m_observerFileSync = new EventBroker.EventObserver(OnReceiveFileSync);
            EventBroker.AddObserver(EventBroker.EventID.etFileSync, m_observerFileSync);

            m_observerUpdateMe = new EventBroker.EventObserver(OnReceiveUpdateMe);
            EventBroker.AddObserver(EventBroker.EventID.etUpdateMe, m_observerUpdateMe);

            SystemLog.Output(SystemLog.MSG_TYPE.War, "Fs Transfer", "Start " + Title);

            m_propertySetting = new Config.SchedulePropertiesSetting();
            m_schedule.ScheduleList = m_propertySetting.Property;
            m_schedule.ServiceStart();

            SystemEvents.SessionEnding += SystemEvents_SessionEnding;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ContextMenu menu = new System.Windows.Forms.ContextMenu();
            System.Windows.Forms.MenuItem itemConfig = new System.Windows.Forms.MenuItem();
            itemConfig.Index = 0;
            itemConfig.Text = "Config";
            itemConfig.Click += ItemConfig_Click;
            menu.MenuItems.Add(itemConfig);

            System.Windows.Forms.MenuItem itemSchedule = new System.Windows.Forms.MenuItem();
            itemSchedule.Index = 1;
            itemSchedule.Text = "Schedule";
            itemSchedule.Click += ItemSchedule_Click;
            menu.MenuItems.Add(itemSchedule);

            System.Windows.Forms.MenuItem itemLog = new System.Windows.Forms.MenuItem();
            itemLog.Index = 2;
            itemLog.Text = "LogView";
            itemLog.Click += (object send, EventArgs args) => { this.Show(); this.WindowState = WindowState.Normal; this.ShowInTaskbar = true; };
            menu.MenuItems.Add(itemLog);

            System.Windows.Forms.MenuItem itemExit = new System.Windows.Forms.MenuItem();
            itemExit.Index = 4;
            itemExit.Text = "Cloud End";
            itemExit.Click += ItemExit_Click1;
            menu.MenuItems.Add(itemExit);

            m_notify = new System.Windows.Forms.NotifyIcon();
            m_notify.Icon = Properties.Resources.Butterfree_icon;
            m_notify.Visible = true;
            m_notify.DoubleClick += (object send, EventArgs args) => { this.Show(); this.WindowState = WindowState.Normal; this.ShowInTaskbar = true; };
            m_notify.ContextMenu = menu;

            Version ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            string str = string.Format("Start Cloud v{0}.{1}.{2}", ver.Major, ver.Minor, ver.Build);
            m_notify.Text = "Fs Transfer";
            notiftyBalloonTip(str, 1000);
            this.WindowState = WindowState.Minimized;

            if (m_timerEvent == null)
            {
                m_timerEvent = new EventBroker.EventParam(this, 0);
                EventBroker.AddTimeEvent(EventBroker.EventID.etUpdateMe, m_timerEvent, 3960000, true);//66분에 한번씩
                //EventBroker.AddTimeEvent(EventBroker.EventID.etUpdateMe, m_timerEvent, 20000, true);//66분에 한번씩
            }
        }

        private void notiftyBalloonTip(string message, int showTime, string title = null)
        {
            if (m_notify == null)
                return;
            if(title == null)
                m_notify.BalloonTipTitle = "Fs Transfer";
            else
                m_notify.BalloonTipTitle = title;
            m_notify.BalloonTipText = message;
            m_notify.ShowBalloonTip(showTime);
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState.Minimized.Equals(WindowState))
                this.Hide();
            base.OnStateChanged(e);
        }

        private void OnReceiveLog(EventBroker.EventID id, EventBroker.EventParam param)
        {
            if (param == null)
                return;
            SystemLog.MSG_TYPE type = (SystemLog.MSG_TYPE)param.ParamInt;
            if (type == SystemLog.MSG_TYPE.Err)
                Output(param.ParamString, Brushes.Yellow);
            else if (type == SystemLog.MSG_TYPE.War)
                Output(param.ParamString, Brushes.YellowGreen);
            else
                Output(param.ParamString, Brushes.LightGray);
        }

        private void Output(string msg, Brush brush = null, bool isBold = false)
        {
            if (richTextBox.Dispatcher.Thread == Thread.CurrentThread)
                addMessage(msg, brush, false);
            else
                richTextBox.Dispatcher.BeginInvoke(new Action(delegate { addMessage(msg, brush, false); }));
        }

        private void addMessage(string msg, Brush brush, bool isBold)
        {
            Paragraph newExternalParagraph = new Paragraph();
            newExternalParagraph.Inlines.Add(new Bold(new Run(DateTime.Now.ToString("HH:mm:ss.fff "))));

            if (isBold)
                newExternalParagraph.Inlines.Add(new Bold(new Run(msg/* + Environment.NewLine*/)));
            else
                newExternalParagraph.Inlines.Add(new Run(msg/* + Environment.NewLine*/));

            if (brush == null)
                newExternalParagraph.Foreground = Brushes.White;
            else
                newExternalParagraph.Foreground = brush;
            m_flowDoc.Blocks.Add(newExternalParagraph);
            while (m_flowDoc.Blocks.Count >= 1000)
            {
                m_flowDoc.Blocks.Remove(m_flowDoc.Blocks.FirstBlock);
            }
            if (!richTextBox.IsSelectionActive)
                richTextBox.ScrollToEnd();
        }
        
        private void Window_Closed(object sender, EventArgs e)
        {
            if (m_timerEvent != null)
                EventBroker.RemoveTimeEvent(EventBroker.EventID.etUpdateMe, m_timerEvent);
            EventBroker.RemoveObserver(EventBroker.EventID.etUpdateMe, m_observerUpdateMe);
            EventBroker.RemoveObserver(EventBroker.EventID.etFileSync, m_observerFileSync);
            EventBroker.RemoveObserver(EventBroker.EventID.etLog, m_observerLog);
            EventBroker.Relase();
            m_notify.Visible = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(m_programShowDown == false)
            {
                e.Cancel = true;
                this.WindowState = WindowState.Minimized;
            }
        }

        private void ItemConfig_Click(object sender, EventArgs e)
        {
            if (m_windowConfigSystem == null)
            {
                m_disableProgramUpdate = true;
                m_windowConfigSystem = new Config.WindowConfigSystem();
                m_windowConfigSystem.Owner = this;
                m_windowConfigSystem.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                m_windowConfigSystem.Closed += (object _sender, EventArgs _e) => { m_windowConfigSystem = null; m_disableProgramUpdate = false; };
                m_windowConfigSystem.Show();
            }
        }
        private void ItemSchedule_Click(object sender, EventArgs e)
        {
            if (m_windowConfigSchedule == null)
            {
                m_disableProgramUpdate = true;
                m_windowConfigSchedule = new Config.WindowConfigSchedule(m_schedule.ScheduleList);
                m_windowConfigSchedule.Owner = this;
                m_windowConfigSchedule.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                m_windowConfigSchedule.Closed += (object _sender, EventArgs _e) =>
                {
                    if (m_windowConfigSchedule.IsChanged == true)
                    {
                        m_schedule.ServiceStop();
                        m_schedule.ScheduleList = m_windowConfigSchedule.ScheduleItem;
                        m_propertySetting.Property = m_schedule.ScheduleList;
                        m_propertySetting.Save();
                        m_schedule.ServiceStart();
                    }
                    m_windowConfigSchedule = null;
                    m_disableProgramUpdate = false;
                };
                m_windowConfigSchedule.Show();
            }
        }
        private void ItemExit_Click1(object sender, EventArgs e)
        {
            m_disableProgramUpdate = true;
            MessageBoxResult msgResult = MessageBox.Show(this, "RCloud exit ?", "Question ", MessageBoxButton.YesNo);
            if (msgResult == MessageBoxResult.No)
            {
                m_disableProgramUpdate = false;
                return;
            }
                
            m_programShowDown = true;
            Close();
        }
        private void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            m_disableProgramUpdate = true;
            m_schedule.OnSystemShutDown();
            m_programShowDown = true;
        }

        private void OnReceiveFileSync(EventBroker.EventID id, EventBroker.EventParam param)
        {
            if (param == null)
                return;
            if(param.ParamString != null)
            {
                if(param.ParamInt == 0)
                    notiftyBalloonTip(param.ParamString + " Error", 1000, "File Sync");
                else if (param.ParamInt == 1)
                    notiftyBalloonTip(param.ParamString + " OK", 2000, "File Sync");
                else if (param.ParamInt == 2)
                    notiftyBalloonTip(param.ParamString, 1000, "Service");
                else
                    notiftyBalloonTip(param.ParamString, 1000, "Renaissance Cloud");
            }
        }
        private void OnReceiveUpdateMe(EventBroker.EventID id, EventBroker.EventParam param)
        {
            if (param == null || m_disableProgramUpdate)
                return;
            try
            {
                PropertiesSystem prop = PropertiesSetting.DefaultSetting.Property;
                if(prop == null)
                {
                    SystemLog.Output(SystemLog.MSG_TYPE.Err, "RCloud Auto update", "ProperiesSystem null");
                    return;
                }
                if (prop.UpdateEnable)
                {
                    IFileService server = null;
                    try
                    {
                        if (prop.UpdateDrvieType == DriveType.FTP)
                            server = new ServiceFtp(prop.UpdateFtpUser, prop.UpdateFtpPw, 5000, prop.UpdateFtpUri);
                        else
                            server = new ServiceDisk(prop.UpdateDiskUri);
                    }
                    catch(Exception ex)
                    {
                        SystemLog.Output(SystemLog.MSG_TYPE.Err, "RCloud Auto update", "Connection Server fail, " + ex.Message);
                        return;
                    }
                    List<FileInfo> info = null;
                    try
                    {
                        info = server.GetFileList(null, "zip");
                        if (info == null)
                            return;
                    }
                    catch(Exception ex)
                    {
                        SystemLog.Output(SystemLog.MSG_TYPE.Err, "RCloud Auto update", "Get File List fail, " + ex.Message);
                        return;
                    }
                    try
                    {
                        foreach (FileInfo fo in info)
                        {
                            string str = fo.Name.ToUpper();
                            if (fo.Name == null)
                                continue;
                            if (str.IndexOf("RCLOUD ") == 0)
                            {
                                string[] strArray = str.Split(' ');
                                if (strArray.Length == 2)
                                {
                                    string[] verArray = strArray[1].Split('.');
                                    if (verArray.Length == 3)
                                    {
                                        try
                                        {
                                            int major = Convert.ToInt32(verArray[0]);
                                            int minor = Convert.ToInt32(verArray[1]);
                                            int build = Convert.ToInt32(verArray[2]);
                                            Version ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                                            if (major != ver.Major || minor != ver.Minor || build != ver.Build)
                                            {
                                                string currentPath = AppDomain.CurrentDomain.BaseDirectory;
                                                SystemLog.Output(SystemLog.MSG_TYPE.Nor, "RCloud Auto update", "Update Start");
                                                int indexRemove = currentPath.LastIndexOf(@"\");
                                                if (indexRemove > 1 && (indexRemove + 1) >= currentPath.Length)
                                                    currentPath = currentPath.Substring(0, indexRemove);

                                                string tempPath = currentPath + "\\UpdateTemp";
                                                if (UpdateMeDownload(server, fo, tempPath) == true)
                                                    UpdateMeFileChange(currentPath, tempPath);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            SystemLog.Output(SystemLog.MSG_TYPE.Err, "RCloud Auto update", "file update check fail, " + ex.Message);
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                    }catch(Exception ex)
                    {
                        SystemLog.Output(SystemLog.MSG_TYPE.Err, "RCloud Auto update", "iterator, " + ex.Message);
                    }
                }
            }catch(Exception ex)
            {
                SystemLog.Output(SystemLog.MSG_TYPE.Err, "RCloud Auto update", "Error, " + ex.Message);
            }
            
        }
        /// <summary>
        /// Update File을 Download 후 압축해제함
        /// </summary>
        /// <param name="server"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        private bool UpdateMeDownload(IFileService server, FileInfo info, string tempPath)
        {
            SystemLog.Output(SystemLog.MSG_TYPE.Nor, "RCloud Auto update", "Download, " + info.FullName);
            MemoryStream memStream = null;
            FileStream writer = null;
            bool result = true;
            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(tempPath);
                if (dirInfo.Exists == false)
                {
                    dirInfo.Create();
                    SystemLog.Output(SystemLog.MSG_TYPE.Nor, "RCloud Auto update", "Create Temporarily folder");
                }
                else
                {
                    dirInfo.Delete(true);
                    Thread.Sleep(100);
                    dirInfo.Create();
                    SystemLog.Output(SystemLog.MSG_TYPE.Nor, "RCloud Auto update", "Delete and Create Temporarily folder");
                }

                memStream = server.ReadFile(info.FullName);
                SystemLog.Output(SystemLog.MSG_TYPE.Nor, "RCloud Auto update", "Download OK");
                string strFullPath = tempPath + "\\" + info.FullName;
                SystemLog.Output(SystemLog.MSG_TYPE.Nor, "RCloud Auto update", "Save Start File, " + strFullPath);
                writer = new FileStream(strFullPath, FileMode.Create);
                memStream.WriteTo(writer);
                writer.Close();
                memStream.Close();
                SystemLog.Output(SystemLog.MSG_TYPE.Nor, "RCloud Auto update", "Save OK");
                //압축하기 System.IO.Compression.ZipFile.CreateFromDirectory(startPath, zipPath);
                //압축 풀기
                try
                {
                    System.IO.Compression.ZipFile.ExtractToDirectory(strFullPath, tempPath);
                }
                catch (Exception ex)
                {
                    SystemLog.Output(SystemLog.MSG_TYPE.Err, "RCloud Auto update", "Unzip error," + ex.Message);
                }
                Thread.Sleep(200);
                try
                {
                    System.IO.FileInfo sfi = new System.IO.FileInfo(strFullPath);
                    sfi.Delete();
                }
                catch (Exception ex)
                {
                    SystemLog.Output(SystemLog.MSG_TYPE.Err, "RCloud Auto update", "delete zip error," + ex.Message);
                }
            }
            catch (Exception ex)
            {
                if(writer != null)
                    writer.Close();
                if (memStream != null)
                    memStream.Close();
                result = false;
                Thread.Sleep(5000);
                SystemLog.Output(SystemLog.MSG_TYPE.Err, "RCloud Auto update", "Update File download fail," + ex.Message);
            }
            return result;
        }
        /// <summary>
        /// Bat file을 생성 후 프로그램 종료함
        /// </summary>
        private void UpdateMeFileChange(string currentPath, string tempPath)
        {
            try
            {
                string batFilePath = tempPath + @"\update.bat";
                string runFile = currentPath + @"\RCloud.exe";
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(batFilePath))
                {
                    file.WriteLine(@"C:\Windows\System32\timeout /t 5 /nobreak");
                    file.WriteLine(@":Repeat");

                    file.WriteLine(@"C:\Windows\System32\timeout /t 2 /nobreak");
                    string str = string.Format("del /q {0}", runFile);
                    file.WriteLine(str);

                    str = string.Format(@"if exist ""{0}"" goto Repeat", runFile);
                    file.WriteLine(str);

                    str = string.Format(@"C:\Windows\System32\xcopy ""{0}"" ""{1}"" /E /C /R /Y", tempPath, currentPath);
                    file.WriteLine(str);

                    file.WriteLine(@"C:\Windows\System32\timeout /t 3 /nobreak");
                    file.WriteLine("start /b " + runFile);

                    str = string.Format("del /q {0}", tempPath);
                    file.WriteLine(str);

                    str = string.Format("del /q {0}", currentPath + @"\update.bat");
                    file.WriteLine(str);

                    file.WriteLine(@"C:\Windows\System32\timeout /t 3");
                    file.WriteLine("exit");
                    file.Close();
                }
                SystemLog.Output(SystemLog.MSG_TYPE.Nor, "RCloud Auto update", "Run Batch file," + tempPath);
                // batch파일을 실행합니다.
                System.Diagnostics.ProcessStartInfo start = new System.Diagnostics.ProcessStartInfo();
                start.FileName = @"cmd.exe";
                start.Arguments = "/c " + batFilePath;
                start.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;   // 윈도우 속성을  windows hidden  으로 지정  
                start.CreateNoWindow = true;  // hidden 을 시키기 위해서 이 속성도 true 로 체크해야 함  
                System.Diagnostics.Process.Start(start);
                //System.Diagnostics.Process.Start("cmd.exe", "/c " + batFilePath);

                m_programShowDown = true;
                this.Dispatcher.Invoke(new Action(() => { Close(); }));
                
            }
            catch (Exception ex)
            {
                SystemLog.Output(SystemLog.MSG_TYPE.Err, "RCloud Auto update", "Error, Updater Bat file make and run," + ex.Message);
            }
        }
    }
}
