using Appsettings;
using System.Collections.Generic;

namespace Common.WinServices.Common
{

    public class CommonWinService
    {
        public string ServiceName { get; set; }

        public ServiceStart ServiceStart { get; set; }

        public QuartzJob QuartzJob { get; set; }

        public ServiceStop ServiceStop { get; set; }

        public static List<CommonWinService> WinServiceList = null;

        static CommonWinService()
        {
            WinServiceList = AppSettingsManager.GetEntityList<CommonWinService>();
        }
    }

    public class ServiceStart
    {
        /// <summary>
        /// OnStart列表
        /// </summary>
        public List<OnStartItem> OnStartItemList { get; set; }
    }

    public class OnStartItem
    {
        /// <summary>
        /// 程序集
        /// </summary>
        public string Assembly { get; set; }

        /// <summary>
        /// 方法名
        /// </summary>
        public string MethodName { get; set; }        
    }

    public class QuartzJob
    {
        /// <summary>
        /// JobItem列表
        /// </summary>
        public List<JobItem> JobItemList { get; set; }
    }

    public class JobItem
    {
        /// <summary>
        /// JobKey
        /// </summary>
        public string JobKey { get; set; }

        /// <summary>
        /// 程序集
        /// </summary>
        public string Assembly { get; set; }

        /// <summary>
        /// 继承了Quartz.IJob的类的名称
        /// </summary>
        public string ClassName { get; set; }

        /// <summary>
        /// 需要定时执行的（完成命名空间的）方法名
        /// </summary>
        public string MethodName { get; set; }

        /// <summary>
        /// Quartz时间间隔配置
        /// </summary>
        public string QuartzCron { get; set; }

        /// <summary>
        /// 自定义参数，使用,隔开
        /// </summary>
        public string Parameters { get; set; }
    }

    public class ServiceStop
    {
        /// <summary>
        /// OnStart列表
        /// </summary>
        public List<OnStopItem> OnStopItemList { get; set; }
    }

    public class OnStopItem
    {
        /// <summary>
        /// 程序集
        /// </summary>
        public string Assembly { get; set; }
        
        /// <summary>
        /// 方法名
        /// </summary>
        public string MethodName { get; set; }
    }
}
