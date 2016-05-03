using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Configuration;
using System.IO;
using System.Threading;

namespace Test.WindowService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            this.WriteToFile("Service Started: {0}");
            this.ScheduleService();
        }

        protected override void OnStop()
        {
        }

        private Timer Schedular;
        public void ScheduleService()
        {
            try
            {
                Schedular = new Timer(new TimerCallback(SchedularCallback));
                string mode = ConfigurationManager.AppSettings["Mode"];
                this.WriteToFile("Service Mode:" + mode + " {0}");

                DateTime scheduledTime = DateTime.MinValue;
                if (mode == "Daily")
                {
                    scheduledTime = DateTime.Parse(ConfigurationManager.AppSettings["ScheduledTime"]);
                    if (DateTime.Now > scheduledTime)
                    {
                        // if scheduled time is passed set schedule for the next interval
                        scheduledTime = scheduledTime.AddDays(1);
                    }
                }
                if (mode == "Interval")
                {
                    int intervalMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["IntervalMinutes"]);
                    scheduledTime = DateTime.Now.AddMinutes(intervalMinutes);
                    if (DateTime.Now > scheduledTime)
                    {
                        scheduledTime = scheduledTime.AddMinutes(intervalMinutes);
                    }
                }

                TimeSpan timeSpan = scheduledTime.Subtract(DateTime.Now);
                string schedule = string.Format("{0} day(s) {1} hour(s) {2} minuite(s) {3} second(s)", timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
                this.WriteToFile("Simple Service scheduled to run after: " + schedule + " {0}");
                int dueTime = Convert.ToInt32(timeSpan.TotalMilliseconds);


                //change the timer due time
                Schedular.Change(dueTime, Timeout.Infinite);
            }
            catch (Exception ex)
            {
                WriteToFile("Simple Service Error on: {0}" + ex.Message + ex.StackTrace);
                //Stop the Windows Service
                using (System.ServiceProcess.ServiceController serviceController = new ServiceController("TAMS.WindowService"))
                {
                    serviceController.Stop();
                }
            }
        }

        private void SchedularCallback(object e)
        {
            this.WriteToFile("service log: {0}");
            this.ScheduleService();
        }

        private void WriteToFile(string text)
        {
            string path = ConfigurationManager.AppSettings["SimpleLog.FilePath"];
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(string.Format(text, DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")));
                writer.Close();
            };
        }
    }
}
