using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Threading;


namespace RCloud
{

    public class FileTransfer
    {
        FileSchedule.ScheduleProperty m_property = null;

        IFileService m_fromFileService = null;
        IFileService m_toFileService = null;

        bool m_nowWorking = false;
        public FileTransfer(FileSchedule.ScheduleProperty property)
        {
            m_property = property;
            try
            {
                if (m_property.FromDiskType == DriveType.Disk)
                    m_fromFileService = new ServiceDisk(m_property.FromDrivePath);
                else
                    m_fromFileService = new ServiceFtp(m_property.FromFtpUser, m_property.FromFtpPw, 5000, m_property.FromFtpUri);

                if (m_property.ToDiskType == DriveType.Disk)
                    m_toFileService = new ServiceDisk(m_property.ToDrivePath);
                else
                    m_toFileService = new ServiceFtp(m_property.ToFtpUser, m_property.ToFtpPw, 5000, m_property.ToFtpUri);
            }
            catch(Exception ex)
            {
                if (m_property.Name != null && m_property.Name != string.Empty)
                    SystemLog.Output(SystemLog.MSG_TYPE.Err, m_property.Name, "Create Object : " + ex.Message);
                else
                    SystemLog.Output(SystemLog.MSG_TYPE.Err, "File Sync", "Create Object : " + ex.Message);
            }
            m_nowWorking = false;
        }
        public FileSchedule.ScheduleProperty Property
        {
            get { return m_property; }
        }
        public bool FileSync()
        {
            if (m_nowWorking == true)
                return true;
            m_nowWorking = true;
            try
            {
                int copyCount = 0;
                if (m_property.FromSyncType == FileSchedule.SyncType.Copy)
                    copyCount = CopyFile();
                else
                    copyCount = MoveFile();
                SystemLog.Output(SystemLog.MSG_TYPE.Nor, m_property.Name, "File Sync : {0} OK", copyCount);
                if (copyCount > 0)
                    EventBroker.AsyncSend(EventBroker.EventID.etFileSync, new EventBroker.EventParam(this, 1, m_property.Name + " : " + copyCount.ToString()));
            }
            catch (Exception ex)
            {
                EventBroker.AsyncSend(EventBroker.EventID.etFileSync, new EventBroker.EventParam(this, 0, m_property.Name));
                if (m_property.Name != null && m_property.Name != string.Empty)
                    SystemLog.Output(SystemLog.MSG_TYPE.Err, m_property.Name, "File Sync Error : " + ex.Message);
                else
                    SystemLog.Output(SystemLog.MSG_TYPE.Err, "File Sync", "File Sync Error: " + ex.Message);
                return false;
            }
            finally
            {
                m_nowWorking = false;
            }
            return true;
        }
        private int MoveFile()
        {
            int copyCount = 0;
            List<FileInfo> UpdateFileList = m_fromFileService.GetFileList();
            foreach(FileInfo info in UpdateFileList)
            {
                try
                {
                    MemoryStream stream = m_fromFileService.ReadFile(info.FullName);
                    m_toFileService.WriteFile(info.FullName, stream);
                    m_fromFileService.DeleteFile(info.FullName);
                    copyCount++;
                    Thread.Sleep(100);
                    if (m_property.Name != null && m_property.Name != string.Empty)
                        SystemLog.Output(SystemLog.MSG_TYPE.Nor, m_property.Name, "Move File : " + info.FullName);
                    else
                        SystemLog.Output(SystemLog.MSG_TYPE.Nor, "File Sync", "Move File : " + info.FullName);
                }
                catch(Exception ex)
                {
                    if (m_property.Name != null && m_property.Name != string.Empty)
                        SystemLog.Output(SystemLog.MSG_TYPE.Err, m_property.Name, "Move File Error: {0}, {1}", info.FullName, ex.Message);
                    else
                        SystemLog.Output(SystemLog.MSG_TYPE.Err, "File Sync", "Move File Error: {0}, {1}" + info.FullName, ex.Message);
                    throw ex;
                }
            }
            return copyCount;
        }

        private int CopyFile()
        {
            List<FileInfo> fromFileList = m_fromFileService.GetFileList();
            List<FileInfo> toFileList = m_toFileService.GetFileList();
            int copyCount = 0;
            foreach (FileInfo info in fromFileList)
            {
                bool needUpdate = true;
                foreach(FileInfo file in toFileList)
                {
                    if (info.FullName != file.FullName)
                        continue;

                    if (info.Size == file.Size)
                    {
                        if (m_fromFileService.ConnectionDrive == DriveType.Disk && m_toFileService.ConnectionDrive == DriveType.Disk)
                        {
                            if (info.LastWriteTime == file.LastWriteTime)
                                needUpdate = false;
                        }
                        else if (m_fromFileService.ConnectionDrive == DriveType.FTP && m_toFileService.ConnectionDrive == DriveType.Disk)
                        {
                            if (info.LastWriteTime.Year == 1)
                            {
                                if (info.LastWriteTime.Month == file.LastWriteTime.Month && info.LastWriteTime.Day == file.LastWriteTime.Day &&
                                    info.LastWriteTime.Hour == file.LastWriteTime.Hour && info.LastWriteTime.Minute == file.LastWriteTime.Minute)
                                    needUpdate = false;
                            }
                            else
                            {
                                if (info.LastWriteTime.Month == file.LastWriteTime.Month && info.LastWriteTime.Day == file.LastWriteTime.Day)
                                    needUpdate = false;
                            }
                        }
                        else
                        {
                            needUpdate = false;
                        }
                    }
                    break;
                }
                try
                {
                    if (needUpdate == true)
                    {
                        MemoryStream stream = m_fromFileService.ReadFile(info.FullName);
                        m_toFileService.WriteFile(info.FullName, stream);
                        if (m_toFileService.ConnectionDrive == DriveType.Disk)
                        {
                            DateTime dt = DateTime.Now;
                            if (info.LastWriteTime.Year == 1)
                                info.LastWriteTime = new DateTime(dt.Year, info.LastWriteTime.Month, info.LastWriteTime.Day, info.LastWriteTime.Hour, info.LastWriteTime.Minute,0);
                            else
                                info.LastWriteTime = new DateTime(info.LastWriteTime.Year, info.LastWriteTime.Month, info.LastWriteTime.Day, dt.Hour, dt.Minute, dt.Second);
                            m_toFileService.WriteFileDateTime(info.FullName, info.LastWriteTime);
                        }
                        copyCount++;
                        Thread.Sleep(100);
                        if (m_property.Name != null && m_property.Name != string.Empty)
                            SystemLog.Output(SystemLog.MSG_TYPE.Nor, m_property.Name, "Copy File : " + info.FullName);
                        else
                            SystemLog.Output(SystemLog.MSG_TYPE.Nor, "File Sync", "Copy File : " + info.FullName);
                    }
                }
                catch(Exception ex)
                {
                    if (m_property.Name != null && m_property.Name != string.Empty)
                        SystemLog.Output(SystemLog.MSG_TYPE.Err, m_property.Name, "Copy File Error: {0}, {1}", info.FullName, ex.Message);
                    else
                        SystemLog.Output(SystemLog.MSG_TYPE.Err, "File Sync", "Copy File Error: {0}, {1}" + info.FullName, ex.Message);
                    throw ex;
                }
            }
            return copyCount;
        }
    }
}
