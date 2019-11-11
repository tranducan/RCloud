using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace RCloud.Config
{
    public class SchedulePropertiesSetting
    {

        Dictionary<string, FileSchedule.ScheduleProperty> m_schedule = new Dictionary<string, FileSchedule.ScheduleProperty>();

        public SchedulePropertiesSetting()
        {
            string dirPath = AppDomain.CurrentDomain.BaseDirectory + "Config";
            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(dirPath);
            if (dir.Exists == false)
                dir.Create();
            string fileName = dirPath + "\\RCoudSchedule.xml";
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(fileName);
            if(fileInfo.Exists)
                m_schedule = XmlToScheduleProperty(fileName);
        }

        public void Load()
        {
            string dirPath = AppDomain.CurrentDomain.BaseDirectory + "Config";
            string fileName = dirPath + "\\RCoudSchedule.xml";
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(fileName);
            if (fileInfo.Exists)
                m_schedule = XmlToScheduleProperty(fileName);
        }
        public void Save()
        {
            string dirPath = AppDomain.CurrentDomain.BaseDirectory + "Config";
            string fileName = dirPath + "\\RCoudSchedule.xml";
            SchedulePropertyToXml(fileName, m_schedule);
        }
        public Dictionary<string, FileSchedule.ScheduleProperty> Property
        {
            get { return m_schedule; }
            set { m_schedule = value; }
        }
        private Dictionary<string, FileSchedule.ScheduleProperty> XmlToScheduleProperty(string filePath)
        {
            Dictionary<string, FileSchedule.ScheduleProperty> maptree = new Dictionary<string, FileSchedule.ScheduleProperty>();
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
                        if (node.Name != "FileTransmit")
                            continue;
                        FileSchedule.ScheduleProperty property = new FileSchedule.ScheduleProperty();
                        property.Name = node["Name"].InnerText;
                        property.RepeateTime = false;
                        property.PCTurnOn = false;
                        property.PCTurnOff = false;
                        property.PCDateChanges = false;
                        XmlNode repeateNode = node["RepeatType"];
                        if(repeateNode!=null)
                        {// 구버젼 호환용
                            if (repeateNode.InnerText == "Time")
                            {
                                property.RepeateTime = true;
                            }
                            else if (repeateNode.InnerText == "WinEnd")
                            {
                                property.PCTurnOff = true;
                            }
                            else if (repeateNode.InnerText == "WinEndDate")
                            {
                                property.PCTurnOff = true;
                                property.PCDateChanges = true;
                            }
                            else if (repeateNode.InnerText == "WinStart")
                            {
                                property.PCTurnOn = true;
                            }
                            else if (repeateNode.InnerText == "WinStartDate")
                            {
                                property.PCTurnOn = true;
                                property.PCDateChanges = true;
                            }
                            else
                                continue;
                        }
                        else
                        {
                            repeateNode = node["WorkRepeateTime"];
                            if (repeateNode != null)
                              property.RepeateTime = repeateNode.InnerText == "true";

                            repeateNode = node["WorkPCTurnOn"];
                            if (repeateNode != null)
                                property.PCTurnOn = repeateNode.InnerText == "true";

                            repeateNode = node["WorkPCTurnOff"];
                            if (repeateNode != null)
                                property.PCTurnOff = repeateNode.InnerText == "true";

                            repeateNode = node["WorkPCDateChanges"];
                            if (repeateNode != null)
                                property.PCDateChanges = repeateNode.InnerText == "true";
                        }

                        property.RepeatTimeMinute = Convert.ToInt32( node["RepeatTimeMinute"].InnerText);
                        property.DelayTimeMinute = Convert.ToInt32(node["DelayTimeMinute"].InnerText);
                        if (node["FromDiskType"].InnerText == DriveType.FTP.ToString())
                            property.FromDiskType = DriveType.FTP;
                        else if (node["FromDiskType"].InnerText == DriveType.Disk.ToString())
                            property.FromDiskType = DriveType.Disk;
                        else
                            continue;

                        property.FromFtpUri = node["FromFtpUri"].InnerText;
                        property.FromFtpUser = node["FromFtpUser"].InnerText;
                        property.FromFtpPw = node["FromFtpPw"].InnerText;
                        property.FromDrivePath = node["FromDrivePath"].InnerText;

                        if (node["FromSyncType"].InnerText == FileSchedule.SyncType.Copy.ToString())
                            property.FromSyncType = FileSchedule.SyncType.Copy;
                        else if (node["FromSyncType"].InnerText == FileSchedule.SyncType.Move.ToString())
                            property.FromSyncType = FileSchedule.SyncType.Move;
                        else
                            continue;

                        if (node["ToDiskType"].InnerText == DriveType.FTP.ToString())
                            property.ToDiskType = DriveType.FTP;
                        else if (node["ToDiskType"].InnerText == DriveType.Disk.ToString())
                            property.ToDiskType = DriveType.Disk;
                        else
                            continue;

                        property.ToFtpUri = node["ToFtpUri"].InnerText;
                        property.ToFtpUser = node["ToFtpUser"].InnerText;
                        property.ToFtpPw = node["ToFtpPw"].InnerText;
                        property.ToDrivePath = node["ToDrivePath"].InnerText;
                        maptree.Add(property.Name, property);
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
            return maptree;
        }

        private void SchedulePropertyToXml(string filePath, Dictionary<string, FileSchedule.ScheduleProperty> property)
        {
            try
            {
                XElement scheduleXml = new XElement("Schedules");
                foreach (var prop in property)
                {
                    try
                    { 
                        XElement fileTransmit = new XElement("FileTransmit");
                        fileTransmit.Add(new XElement("Name", prop.Value.Name));

                        fileTransmit.Add(new XElement("WorkRepeateTime", prop.Value.RepeateTime.ToString()));
                        fileTransmit.Add(new XElement("WorkPCTurnOn", prop.Value.PCTurnOn.ToString()));
                        fileTransmit.Add(new XElement("WorkPCTurnOff", prop.Value.PCTurnOff.ToString()));
                        fileTransmit.Add(new XElement("WorkPCDateChanges", prop.Value.PCDateChanges.ToString()));
                        fileTransmit.Add(new XElement("RepeatTimeMinute", prop.Value.RepeatTimeMinute.ToString()));
                        fileTransmit.Add(new XElement("DelayTimeMinute", prop.Value.DelayTimeMinute.ToString()));
                        fileTransmit.Add(new XElement("FromDiskType", prop.Value.FromDiskType.ToString()));
                        fileTransmit.Add(new XElement("FromFtpUri", prop.Value.FromFtpUri));
                        fileTransmit.Add(new XElement("FromFtpUser", prop.Value.FromFtpUser));
                        fileTransmit.Add(new XElement("FromFtpPw", prop.Value.FromFtpPw));
                        fileTransmit.Add(new XElement("FromDrivePath", prop.Value.FromDrivePath));
                        fileTransmit.Add(new XElement("FromSyncType", prop.Value.FromSyncType.ToString()));
                        fileTransmit.Add(new XElement("ToDiskType", prop.Value.ToDiskType.ToString()));
                        fileTransmit.Add(new XElement("ToFtpUri", prop.Value.ToFtpUri));
                        fileTransmit.Add(new XElement("ToFtpUser", prop.Value.ToFtpUser));
                        fileTransmit.Add(new XElement("ToFtpPw", prop.Value.ToFtpPw));
                        fileTransmit.Add(new XElement("ToDrivePath", prop.Value.ToDrivePath));

                        scheduleXml.Add(fileTransmit);
                    }
                    catch (Exception ex)
                    {
                        SystemLog.Output(SystemLog.MSG_TYPE.Err, "Property Setting", "Save: {0}", ex.Message);
                    }
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
}
