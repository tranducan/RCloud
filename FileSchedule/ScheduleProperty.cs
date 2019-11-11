using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCloud.FileSchedule
{
    public enum SyncType
    {
        Copy,
        Move
    }
    public class ScheduleProperty
    {
        public ScheduleProperty()
        {
            Name = string.Empty;
            RepeateTime = true;
            PCTurnOn = false;
            PCTurnOff = false;
            PCDateChanges = false;

            RepeatTimeMinute = 10;
            DelayTimeMinute = 5;

            FromDiskType = DriveType.FTP;
            FromFtpUri = string.Empty;
            FromFtpUser = string.Empty;
            FromFtpPw = string.Empty;
            FromDrivePath = string.Empty;
            FromSyncType = SyncType.Copy;

            ToDiskType = DriveType.FTP;
            ToFtpUri = string.Empty;
            ToFtpUser = string.Empty;
            ToFtpPw = string.Empty;
            ToDrivePath = string.Empty;
        }
        public ScheduleProperty Clone()
        {
            ScheduleProperty schedule = new ScheduleProperty();
            schedule.Name = this.Name;

            schedule.RepeateTime = this.RepeateTime;
            schedule.PCTurnOn = this.PCTurnOn;
            schedule.PCTurnOff = this.PCTurnOff;
            schedule.PCDateChanges = this.PCDateChanges;

            schedule.RepeatTimeMinute = this.RepeatTimeMinute;
            schedule.DelayTimeMinute = this.DelayTimeMinute;

            schedule.FromDiskType = this.FromDiskType;
            schedule.FromFtpUri = this.FromFtpUri;
            schedule.FromFtpUser = this.FromFtpUser;
            schedule.FromFtpPw = this.FromFtpPw;
            schedule.FromDrivePath = this.FromDrivePath;
            schedule.FromSyncType = this.FromSyncType;

            schedule.ToDiskType = this.ToDiskType;
            schedule.ToFtpUri = this.ToFtpUri;
            schedule.ToFtpUser = this.ToFtpUser;
            schedule.ToFtpPw = this.ToFtpPw;
            schedule.ToDrivePath = this.ToDrivePath;
            return schedule;
        }

        public string Name { get; set; }
        public bool RepeateTime { get; set; }
        public int RepeatTimeMinute { get; set; }
        public bool PCTurnOn { get; set; }
        public int DelayTimeMinute { get; set; }
        public bool PCTurnOff { get; set; }
        public bool PCDateChanges { get; set; }
        public DriveType FromDiskType { get; set; }
        public string FromFtpUri { get; set; }
        public string FromFtpUser { get; set; }
        public string FromFtpPw { get; set; }
        public string FromDrivePath { get; set; }
        public SyncType FromSyncType { get; set; }

        public DriveType ToDiskType { get; set; }
        public string ToFtpUri { get; set; }
        public string ToFtpUser { get; set; }
        public string ToFtpPw { get; set; }
        public string ToDrivePath { get; set; }
    }
}
