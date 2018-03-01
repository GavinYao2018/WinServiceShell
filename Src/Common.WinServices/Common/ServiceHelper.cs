using Quartz;
using Quartz.Impl.Matchers;
using Quartz.Impl.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Script.Serialization;

namespace Common.WinServices.Common
{
    public class ServiceHelper
    {
        /// <summary>
        /// 定时任务壳的输出日志名称
        /// </summary>
        public static string LogName = "基础定时服务";

        /// <summary>
        /// 程序集缓存
        /// </summary>
        static Dictionary<string, Assembly> AssemblyCollection = new Dictionary<string, Assembly>();

        /// <summary>
        /// TriggerGrouopName
        /// </summary>
        static string TriggerGrouopName = "TimingTaskFramework";

        /// <summary>
        /// matcherList
        /// </summary>
        static List<IMatcher<JobKey>> matcherList = new List<IMatcher<JobKey>>();

        /// <summary>
        /// Scheduler
        /// </summary>
        static IScheduler _sched = null;

        /// <summary>
        /// 定时任务开始
        /// </summary>
        /// <param name="logBuilder"></param>
        public static void StartJob(StringBuilder logBuilder)
        {
            //初始化
            ServiceHelper.OnStartInit(logBuilder);

            logBuilder.AppendLine("【定时任务读取开始】");
            ISchedulerFactory sf = new Quartz.Impl.StdSchedulerFactory();
            ServiceHelper._sched = sf.GetScheduler();
            int jobCounter = 0;
            var currentService = ServiceHelper.GetCurrentFrontpayWinService();
            if (currentService == null)
            {
                logBuilder.AppendFormat("找不到【{0}】的配置", ServiceHelper.CurrentServiceName).AppendLine();
                return;
            }
            foreach (var quartzJob in currentService.QuartzJob.JobItemList)
            {
                //配置检查
                if (string.IsNullOrWhiteSpace(quartzJob.JobKey) || string.IsNullOrWhiteSpace(quartzJob.Assembly)
                    || string.IsNullOrWhiteSpace(quartzJob.QuartzCron))
                {
                    logBuilder.AppendFormat("JobKey：{0}", quartzJob.JobKey).AppendLine();
                    logBuilder.AppendFormat("Assembly：{0}", quartzJob.Assembly).AppendLine();
                    logBuilder.AppendFormat("QuartzCron：{0}", quartzJob.QuartzCron).AppendLine();
                    logBuilder.AppendLine("QuartzJob.JobItem配置中，JobKey，Assembly和QuartzCron不能为空");
                    continue;
                }

                //二选一
                if (string.IsNullOrWhiteSpace(quartzJob.ClassName) && string.IsNullOrWhiteSpace(quartzJob.MethodName))
                {
                    logBuilder.AppendFormat("ClassName：{0}", quartzJob.ClassName).AppendLine();
                    logBuilder.AppendFormat("MethodName：{0}", quartzJob.MethodName).AppendLine();
                    logBuilder.AppendLine("QuartzJob.JobItem配置中，ClassName或MethodName不能为空");
                    continue;
                }

                try
                {
                    //如果执行方法不为空，则认为是没有继承Quartz.IJob
                    if (!string.IsNullOrEmpty(quartzJob.MethodName))
                    {
                        //如果执行的方法名不为空
                        var iIndex = quartzJob.MethodName.LastIndexOf(".");
                        quartzJob.ClassName = quartzJob.MethodName.Substring(0, iIndex);//将方法名给到类名
                        quartzJob.MethodName = quartzJob.MethodName.Substring(iIndex + 1); //留下方法名
                    }

                    Assembly assembly = ServiceHelper.GetAssembly(quartzJob.Assembly);
                    var job = assembly.CreateInstance(quartzJob.ClassName, false);

                    if (currentService.QuartzJob.JobItemList.Where(p => p.JobKey == quartzJob.JobKey).Count() > 1)
                    {
                        throw new Exception($"【{quartzJob.JobKey}】出现重复的JobKey");
                    }

                    #region 指定执行方法的处理与验证

                    if (!string.IsNullOrEmpty(quartzJob.MethodName))
                    {
                        //如果MethodName不为空，则启动本系统定时任务执行MethodName
                        if (job == null)
                        {
                            throw new Exception($"未能实例化对象：{quartzJob.ClassName}");
                        }
                        Type type = job.GetType();
                        var methodInfo = type.GetMethod(quartzJob.MethodName);
                        if (methodInfo == null)
                        {
                            throw new Exception($"不存在方法名：{quartzJob.ClassName}.{quartzJob.MethodName}");
                        }
                        object[] arrParam = null;
                        if (!string.IsNullOrEmpty(quartzJob.Parameters))
                        {
                            var arr = quartzJob.Parameters.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                            if (arr.Length > 0)
                            {
                                arrParam = arr.ToArray();
                            }
                        }
                        CommonJob.AddJob(quartzJob.JobKey, job, methodInfo, arrParam);

                        //完成后，实例化成CommonJob
                        job = new CommonJob();
                    }
                    else
                    {
                        //否则，校验ClassName是否继承了IJob
                        if (!(job is IJob))
                        {
                            throw new Exception(string.Format("{0}，没有继承Quartz.IJob", quartzJob.ClassName));
                        }
                    }

                    #endregion

                    //加载Job
                    var jobKey = new JobKey(quartzJob.JobKey, ServiceHelper.TriggerGrouopName);
                    var jobDetail = JobBuilder.Create(job.GetType()).UsingJobData("Parameters", quartzJob.Parameters).WithIdentity(jobKey).Build();
                    var jobTrig = new CronTriggerImpl(quartzJob.JobKey + "Trigger", ServiceHelper.TriggerGrouopName, quartzJob.QuartzCron);
                    ServiceHelper._sched.ScheduleJob(jobDetail, jobTrig);
                    jobCounter++;

                    //添加适配器（可以利用适配器添加监听器）
                    var matcher = KeyMatcher<JobKey>.KeyEquals(jobDetail.Key);
                    matcherList.Add(matcher);

                    logBuilder.Append(quartzJob.JobKey).AppendLine("，读取成功");

                }
                catch (Exception ex)
                {
                    logBuilder.Append(quartzJob.JobKey).AppendLine("，读取失败，");
                    throw ex;
                }
                logBuilder.AppendLine("【定时任务读取完成】");
            }
            if (jobCounter > 0)
            {
                ServiceHelper._sched.Start();
            }
        }

        public static void StopJob(StringBuilder logBuilder)
        {
            if (ServiceHelper._sched != null)
            {
                ServiceHelper._sched.Shutdown(false);
                ServiceHelper._sched = null;
            }
            ServiceHelper.OnStopCollect(logBuilder);
        }

        private static string _currentServiceName;

        public static string CurrentServiceName
        {
            get
            {
                if (string.IsNullOrEmpty(ServiceHelper._currentServiceName))
                {
                    SettingHelper sh = new SettingHelper();
                    ServiceHelper._currentServiceName = sh.ServiceName;
                }
                return ServiceHelper._currentServiceName;
            }
        }

        /// <summary>
        /// 获取当前配置的Service
        /// </summary>
        /// <returns></returns>
        public static CommonWinService GetCurrentFrontpayWinService()
        {
            CommonWinService result = null;
            string currentServiceName = ServiceHelper.CurrentServiceName;
            if (string.IsNullOrEmpty(currentServiceName))
            {
                throw new Exception("ServiceName为空");
            }
            Logger.Log(ServiceHelper.LogName, "ServiceName=" + currentServiceName);
            var listString = new JavaScriptSerializer().Serialize(CommonWinService.WinServiceList.Select(n => n.ServiceName));
            Logger.Log(ServiceHelper.LogName, "CommonWinService.WinServiceList=" + listString);
            result = CommonWinService.WinServiceList.FirstOrDefault(p => string.Equals(p.ServiceName, currentServiceName, StringComparison.OrdinalIgnoreCase));

            #region 初始化Null值，以防调用出错
            if (result != null)
            {
                if (result.ServiceStart == null)
                {
                    result.ServiceStart = new ServiceStart();
                }
                if (result.ServiceStart.OnStartItemList == null)
                {
                    result.ServiceStart.OnStartItemList = new List<OnStartItem>();
                }
                if (result.ServiceStop == null)
                {
                    result.ServiceStop = new ServiceStop();
                }
                if (result.ServiceStop.OnStopItemList == null)
                {
                    result.ServiceStop.OnStopItemList = new List<OnStopItem>();
                }
                if (result.QuartzJob == null)
                {
                    result.QuartzJob = new QuartzJob();
                }
                if (result.QuartzJob.JobItemList == null)
                {
                    result.QuartzJob.JobItemList = new List<JobItem>();
                }
            }
            #endregion

            return result;
        }

        private static void OnStartInit(StringBuilder logBuilder)
        {
            logBuilder.Append("【初始化开始】：").AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffff"));
            var currentService = ServiceHelper.GetCurrentFrontpayWinService();
            if (currentService != null)
            {
                foreach (var osi in currentService.ServiceStart.OnStartItemList)
                {
                    if (!string.IsNullOrWhiteSpace(osi.Assembly) && !string.IsNullOrWhiteSpace(osi.ClassName) && !string.IsNullOrWhiteSpace(osi.MethodName))
                    {
                        var assembly = ServiceHelper.GetAssembly(osi.Assembly);
                        var obj = assembly.CreateInstance(osi.ClassName);
                        Type type = obj.GetType();

                        string[] methodNameArr = osi.MethodName.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var mn in methodNameArr)
                        {
                            string method = mn.Trim();
                            logBuilder.AppendFormat("【执行{0}.{1}】：{2}", osi.ClassName, method, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffff")).AppendLine();
                            var methodInfo = type.GetMethod(osi.MethodName);
                            methodInfo.Invoke(obj, null);
                        }
                    }
                    else
                    {
                        logBuilder.AppendFormat("Assembly：{0}", osi.Assembly).AppendLine();
                        logBuilder.AppendFormat("ClassName：{0}", osi.ClassName).AppendLine();
                        logBuilder.AppendFormat("MethodName：{0}", osi.MethodName).AppendLine();
                        logBuilder.AppendLine("ServiceStart.OnStartItem配置中，Assembly，ClassName和MethodName不能为空");
                    }
                }
            }
            else
            {
                logBuilder.AppendFormat("找不到【{0}】的配置", ServiceHelper.CurrentServiceName).AppendLine();
            }
            logBuilder.Append("【初始化结束】：").AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffff"));
        }

        private static void OnStopCollect(StringBuilder logBuilder)
        {
            logBuilder.Append("【任务回收开始】：").AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffff"));
            var currentService = ServiceHelper.GetCurrentFrontpayWinService();
            if (currentService != null)
            {
                foreach (var osi in currentService.ServiceStop.OnStopItemList)
                {
                    if (!string.IsNullOrWhiteSpace(osi.Assembly) && !string.IsNullOrWhiteSpace(osi.ClassName) && !string.IsNullOrWhiteSpace(osi.MethodName))
                    {
                        var assembly = ServiceHelper.GetAssembly(osi.Assembly);
                        var obj = assembly.CreateInstance(osi.ClassName);
                        Type type = obj.GetType();

                        string[] methodNameArr = osi.MethodName.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var mn in methodNameArr)
                        {
                            string method = mn.Trim();
                            logBuilder.AppendFormat("【执行{0}.{1}】：{2}", osi.ClassName, method, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffff")).AppendLine();
                            var methodInfo = type.GetMethod(osi.MethodName);
                            methodInfo.Invoke(obj, null);
                        }
                    }
                    else
                    {
                        logBuilder.AppendFormat("Assembly：{0}", osi.Assembly).AppendLine();
                        logBuilder.AppendFormat("ClassName：{0}", osi.ClassName).AppendLine();
                        logBuilder.AppendFormat("MethodName：{0}", osi.MethodName).AppendLine();
                        logBuilder.AppendLine("ServiceStop.OnStopItem配置中，Assembly，ClassName和MethodName不能为空");
                    }
                }
            }
            else
            {
                logBuilder.AppendFormat("找不到【{0}】的配置", ServiceHelper.CurrentServiceName).AppendLine();
            }
            logBuilder.Append("【任务回收结束】：").AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffff"));
        }

        private static Assembly GetAssembly(string assemblyName)
        {
            Assembly assembly = null;
            if (ServiceHelper.AssemblyCollection.ContainsKey(assemblyName))
            {
                assembly = ServiceHelper.AssemblyCollection[assemblyName];
            }
            else
            {
                assembly = System.Reflection.Assembly.Load(assemblyName);
                ServiceHelper.AssemblyCollection[assemblyName] = assembly;
            }
            return assembly;
        }
    }
}
