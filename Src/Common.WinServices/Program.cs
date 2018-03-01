using Common.WinServices.Common;
using System;
using System.ServiceProcess;
using System.Text;

namespace Common.WinServices
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new BaseService()
            };
            ServiceBase.Run(ServicesToRun);
            //new BaseService().Start();//测试
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            StringBuilder logBuilder = new StringBuilder();
            logBuilder.Append("异常来源：").AppendLine(sender != null ? sender.ToString() : "");
            logBuilder.AppendLine("异常信息：");
            logBuilder.AppendLine(e.ExceptionObject.ToString());
            Logger.Log("定时任务_未处理异常", logBuilder.ToString());
        }
    }
}
