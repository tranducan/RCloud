using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace RCloud.Config
{
    public class AutoRunRegister
    {
        public bool RegistrationStartUp()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            string appFileName = AppDomain.CurrentDomain.FriendlyName;
            if (path == null || path == string.Empty || appFileName == null || appFileName == string.Empty)
                return false;
            path += appFileName;
            int index = appFileName.LastIndexOf('.');
            string appName = index > 0 ? appFileName.Substring(0, index) : appFileName;
            if (appName == null || appName == string.Empty)
                return false;

            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            //레지스트리 등록 할때
            if (registryKey.GetValue(appName) == null)
                registryKey.SetValue(appName, path);
            return true;
        }
        public bool DeleteStartUp()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            string appFileName = AppDomain.CurrentDomain.FriendlyName;
            if (path == null || path == string.Empty || appFileName == null || appFileName == string.Empty)
                return false;
            path += appFileName;
            int index = appFileName.LastIndexOf('.');
            string appName = index > 0 ? appFileName.Substring(0, index) : appFileName;
            if (appName == null || appName == string.Empty)
                return false;

            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            //레지스트리 등록 할때
            if (registryKey.GetValue(appName) != null)
                registryKey.DeleteValue(appName, false);
            return true;
        }
        public bool IsStartUp()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            string appFileName = AppDomain.CurrentDomain.FriendlyName;
            if (path == null || path == string.Empty || appFileName == null || appFileName == string.Empty)
                return false;
            path += appFileName;
            int index = appFileName.LastIndexOf('.');
            string appName = index > 0 ? appFileName.Substring(0, index) : appFileName;
            if (appName == null || appName == string.Empty)
                return false;

            RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
            //레지스트리 등록 할때
            if (registryKey.GetValue(appName) == null)
                return false;
            return true;
        }
        public string AppPath
        {
            get { return AppDomain.CurrentDomain.BaseDirectory; }
        }
        public string AppName
        {
            get
            {
                string appFileName = AppDomain.CurrentDomain.FriendlyName;
                if (appFileName == null || appFileName == string.Empty)
                    return "";
                int index = appFileName.LastIndexOf('.');
                string appName = index > 0 ? appFileName.Substring(0, index) : appFileName;
                return appName;
            }
        }
    }
}
