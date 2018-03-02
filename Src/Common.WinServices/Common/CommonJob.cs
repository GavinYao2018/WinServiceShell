using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Common.WinServices.Common
{
    [DisallowConcurrentExecution]
    public class CommonJob : IJob
    {
        /// <summary>
        /// 使用CommonJob的定时任务列表
        /// </summary>
        private static List<CommonJobItem> JobList;
        static CommonJob()
        {
            JobList = new List<CommonJobItem>();
        }

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                if (JobList.Count == 0) return;
                if (context == null) return;

                var key = context.JobDetail.Key.Name;
                var item = JobList.FirstOrDefault(n => n.JobKey.Equals(key));
                if (item == null) return;

                item.MmethodInfo.Invoke(item.Instance, item.Parameters);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "CommonJob执行出错");
            }
        }

        /// <summary>
        /// 注册CommonJob定时任务
        /// </summary>
        /// <param name="jobKey"></param>
        /// <param name="instance"></param>
        /// <param name="methodInfo"></param>
        /// <param name="parameters"></param>
        internal static void AddJob(string jobKey, object instance, MethodInfo methodInfo, object[] parameters = null)
        {
            JobList.Add(new CommonJobItem() { JobKey = jobKey, Instance = instance, MmethodInfo = methodInfo, Parameters = parameters });
        }
    }

    /// <summary>
    /// 使用CommonJob的定时任务执行信息
    /// </summary>
    internal class CommonJobItem
    {
        public string JobKey { get; set; }

        public object Instance { get; set; }

        public MethodInfo MmethodInfo { get; set; }

        public object[] Parameters { get; set; }
    }
}
