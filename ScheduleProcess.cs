using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCloud
{
    class ScheduleProcess
    {
        Dictionary<string, FileSchedule.ScheduleProperty> m_schedule = new Dictionary<string, FileSchedule.ScheduleProperty>();
        Dictionary<string, EventBroker.EventParam> m_fileTransfer = new Dictionary<string,  EventBroker.EventParam>();
        EventBroker.EventObserver m_observerTimer = null;
        EventBroker.EventObserver m_observerStart = null;
        EventBroker.EventObserver m_observerCheckDate = null;
        
        bool m_isRun = false;
        static bool m_isFirstStart = true;
        DateTime m_currentDate = DateTime.MinValue;
        EventBroker.EventParam m_checkDateParam = null;
        public ScheduleProcess()
        {
            m_observerTimer = new EventBroker.EventObserver(OnTimer);
            EventBroker.AddObserver(EventBroker.EventID.etFileTimer, m_observerTimer);

            m_observerStart = new EventBroker.EventObserver(OnStartProgram);
            EventBroker.AddObserver(EventBroker.EventID.etStartProgram, m_observerStart);

            m_observerCheckDate = new EventBroker.EventObserver(OnCheckDate);
            EventBroker.AddObserver(EventBroker.EventID.etCheckDate, m_observerCheckDate);

            m_checkDateParam = new EventBroker.EventParam(this, 0);

            m_currentDate = DateTime.Now ;
        }
        public Dictionary<string, FileSchedule.ScheduleProperty>  ScheduleList
        {
            get { return m_schedule; }
            set { m_schedule = value; }
        }

       
        public void ServiceStart()
        {
            if (m_isRun)
                ServiceStop();

            SystemLog.Output(SystemLog.MSG_TYPE.Nor, "Schedule Process", "Service Start");
            foreach(var sch in m_schedule)
            {
                try
                {
                    SystemLog.Output(SystemLog.MSG_TYPE.Nor, "Schedule Process", "Start : {0}", sch.Key);
                    FileTransfer ft = new FileTransfer(sch.Value);
                    EventBroker.EventParam eParam = new EventBroker.EventParam(this, 0, sch.Key, ft);
                    m_fileTransfer.Add(sch.Key, eParam);
                    
                    if (sch.Value.PCTurnOn == true && m_isFirstStart == true)
                        EventBroker.AddTimeEvent(EventBroker.EventID.etStartProgram, eParam, sch.Value.DelayTimeMinute* 60000, false);

                    if (sch.Value.RepeateTime == true)
                        EventBroker.AddTimeEvent(EventBroker.EventID.etFileTimer, eParam, sch.Value.RepeatTimeMinute * 60000, true);
                }
                catch (Exception ex)
                {
                    SystemLog.Output(SystemLog.MSG_TYPE.Err, "Schedule Process", "Service Start : " + ex.Message);
                }
            }
            EventBroker.AddTimeEvent(EventBroker.EventID.etCheckDate, m_checkDateParam, 60000, true);
            EventBroker.AsyncSend(EventBroker.EventID.etFileSync, new EventBroker.EventParam(this, 2, "Start"));
            m_isFirstStart = true;
            m_isRun = true;
        }
        public void ServiceStop()
        {
            SystemLog.Output(SystemLog.MSG_TYPE.Nor, "Schedule Process", "Service Stop");
            EventBroker.AsyncSend(EventBroker.EventID.etFileSync, new EventBroker.EventParam(this, 2, "Stop"));
            EventBroker.RemoveTimeEvent(EventBroker.EventID.etCheckDate, m_checkDateParam);
            foreach (var fte in m_fileTransfer)
            {
                try
                { 
                    SystemLog.Output(SystemLog.MSG_TYPE.Nor, "Schedule Process", "Stop : {0}", fte.Key);
                    FileTransfer ft = fte.Value.ParamObj as FileTransfer;
                    FileSchedule.ScheduleProperty sp = ft.Property;

                    if (sp.PCTurnOn == true)
                        EventBroker.RemoveTimeEvent(EventBroker.EventID.etStartProgram, fte.Value);
                    if (sp.RepeateTime == true)
                        EventBroker.RemoveTimeEvent(EventBroker.EventID.etFileTimer, fte.Value);
                }
                catch (Exception ex)
                {
                    SystemLog.Output(SystemLog.MSG_TYPE.Err, "Schedule Process", "Service Start : " + ex.Message);
                }
            }
            m_fileTransfer.Clear();
            m_isRun = false;
        }
        public void OnSystemShutDown()
        {
            SystemLog.Output(SystemLog.MSG_TYPE.Nor, "Schedule Process", "System ShutDown");
            foreach (var fte in m_fileTransfer)
            {
                FileTransfer ft = fte.Value.ParamObj as FileTransfer;
                FileSchedule.ScheduleProperty sp = ft.Property;

                if (sp.PCTurnOff == true)
                    ft.FileSync();
            }
        }
        private void OnTimer(EventBroker.EventID id, EventBroker.EventParam param)
        {
            if (param == null)
                return;
            FileTransfer ft = param.ParamObj as FileTransfer;
            ft.FileSync();
        }

        private void OnStartProgram(EventBroker.EventID id, EventBroker.EventParam param)
        {
            SystemLog.Output(SystemLog.MSG_TYPE.Nor, "Schedule Process", "System Start");
            foreach (var fte in m_fileTransfer)
            {
                FileTransfer ft = fte.Value.ParamObj as FileTransfer;
                FileSchedule.ScheduleProperty sp = ft.Property;

                if (sp.PCTurnOn == true)
                    ft.FileSync();
            }
        }

        private void OnCheckDate(EventBroker.EventID id, EventBroker.EventParam param)
        {
            if(m_currentDate.Day != DateTime.Now.Day)
            {
                m_currentDate = DateTime.Now;
                SystemLog.Output(SystemLog.MSG_TYPE.Nor, "Schedule Process", "System Date Change");
                foreach (var fte in m_fileTransfer)
                {
                    FileTransfer ft = fte.Value.ParamObj as FileTransfer;
                    FileSchedule.ScheduleProperty sp = ft.Property;

                    if (sp.PCDateChanges == true)
                        ft.FileSync();
                }
            }
        }
    }
}
