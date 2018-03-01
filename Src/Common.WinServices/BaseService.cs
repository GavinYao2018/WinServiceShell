using Common.WinServices.Common;
using System;
using System.ServiceProcess;
using System.Text;

namespace Common.WinServices
{
    public partial class BaseService : ServiceBase
    {
        public BaseService()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 调试用
        /// </summary>
        public void Start()
        {
            this.OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            string logName = ServiceHelper.LogName;
            StringBuilder logBuilder = new StringBuilder();
            try
            {
                ServiceHelper.StartJob(logBuilder);
            }
            catch(Exception ex)
            {
                logName += "_异常";
                logBuilder.AppendLine("发生异常：").AppendLine(ex.ToString());
            }
            finally
            {
                Logger.Log(logName, logBuilder.ToString());
            }
        }

        protected override void OnStop()
        {
            string logName = ServiceHelper.LogName;
            StringBuilder logBuilder = new StringBuilder();
            try
            {
                ServiceHelper.StopJob(logBuilder);
                logBuilder.AppendLine("【定时任务结束】");
            }
            catch (Exception ex)
            {
                logName += "_异常";
                logBuilder.AppendLine("异常：").AppendLine(ex.ToString());
            }
            finally
            {
                Logger.Log(logName, logBuilder.ToString());
            }
        }
    }
}
