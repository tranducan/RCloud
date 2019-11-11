using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace RCloud
{
    public class PropertiesSetting
    {
        static PropertiesSetting g_instance = null;
        static public PropertiesSetting DefaultSetting
        {
            get
            {
                if (g_instance == null)
                {
                    g_instance = new PropertiesSetting();
                    g_instance.Load();
                }
                return g_instance;
            }
        }

        PropertiesSystem m_properties = new PropertiesSystem();

        public PropertiesSetting()
        {
            string dirPath = AppDomain.CurrentDomain.BaseDirectory + "Config";
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(dirPath);
            if (dir.Exists == false)
                dir.Create();
            string fileName = dirPath + "\\RCloudProperty.xml";
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(fileName);
            if (fileInfo.Exists)
                m_properties = XmlToScheduleProperty(fileName);
        }

        public void Load()
        {
            string dirPath = AppDomain.CurrentDomain.BaseDirectory + "Config";
            string fileName = dirPath + "\\RCloudProperty.xml";
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(fileName);
            if (fileInfo.Exists)
                m_properties = XmlToScheduleProperty(fileName);
        }
        public void Save()
        {
            string dirPath = AppDomain.CurrentDomain.BaseDirectory + "Config";
            string fileName = dirPath + "\\RCloudProperty.xml";
            SchedulePropertyToXml(fileName, m_properties);
        }
        public PropertiesSystem Property
        {
            get { return m_properties; }
            set { m_properties = value; }
        }
        private PropertiesSystem XmlToScheduleProperty(string filePath)
        {
            PropertiesSystem property = new PropertiesSystem();
            try
            {
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.Load(filePath);
                XmlElement element = xmldoc.DocumentElement;
                XmlNodeList nodes = element.ChildNodes;
                foreach (XmlNode node in nodes)
                {
                    try
                    {
                        if (node.Name != "System")
                            continue;

                        if (node.SelectSingleNode("UpdateEnable") != null)
                            property.UpdateEnable = node["UpdateEnable"].InnerText == "1" ? true : false;

                        if (node.SelectSingleNode("UpdateDrvieType") != null)
                        {
                            if(node["UpdateDrvieType"].InnerText == DriveType.FTP.ToString())
                                property.UpdateDrvieType = DriveType.FTP;
                            else// if (str == DriveType.Disk.ToString())
                                property.UpdateDrvieType = DriveType.Disk;
                        }
                        if (node.SelectSingleNode("UpdateFtpUri") != null)
                            property.UpdateFtpUri = node["UpdateFtpUri"].InnerText;
                        if (node.SelectSingleNode("UpdateFtpUser") != null)
                            property.UpdateFtpUser = node["UpdateFtpUser"].InnerText;
                        if (node.SelectSingleNode("UpdateFtpPw") != null)
                            property.UpdateFtpPw = node["UpdateFtpPw"].InnerText;
                        if (node.SelectSingleNode("UpdateDiskUri") != null)
                            property.UpdateDiskUri = node["UpdateDiskUri"].InnerText;
                    }
                    catch (Exception ex)
                    {
                        SystemLog.Output(SystemLog.MSG_TYPE.Err, "Property Setting", "Load: {0}", ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                SystemLog.Output(SystemLog.MSG_TYPE.Err, "Property Setting", "Load: {0}", ex.Message);
            }
            return property;
        }

        private void SchedulePropertyToXml(string filePath, PropertiesSystem prop)
        {
            try
            {
                XElement scheduleXml = new XElement("Properties");
                try
                {
                    XElement sysXml = new XElement("System");
                    sysXml.Add(new XElement("UpdateEnable", prop.UpdateEnable ? "1" : "0"));
                    sysXml.Add(new XElement("UpdateDrvieType", prop.UpdateDrvieType.ToString()));
                    sysXml.Add(new XElement("UpdateFtpUri", prop.UpdateFtpUri));
                    sysXml.Add(new XElement("UpdateFtpUser", prop.UpdateFtpUser));
                    sysXml.Add(new XElement("UpdateFtpPw", prop.UpdateFtpPw));
                    sysXml.Add(new XElement("UpdateDiskUri", prop.UpdateDiskUri));

                    scheduleXml.Add(sysXml);
                }
                catch (Exception ex)
                {
                    SystemLog.Output(SystemLog.MSG_TYPE.Err, "Property Setting", "Save: {0}", ex.Message);
                }

                XDocument doc = new XDocument(new XDeclaration("1.0", "UTF-8", null), scheduleXml);
                doc.Save(filePath);
            }
            catch (Exception ex)
            {
                SystemLog.Output(SystemLog.MSG_TYPE.Err, "Property Setting", "Save: {0}", ex.Message);
            }
        }
    }
    public class PropertiesSystem
    {
        public bool UpdateEnable = false;
        public DriveType UpdateDrvieType = DriveType.FTP;
        public string UpdateFtpUri = "ftp://127.0.0.1/RCloudUpdate";
        public string UpdateFtpUser = "Rnsc";
        public string UpdateFtpPw = string.Empty;
        public string UpdateDiskUri = @"C:\";
    }
}
