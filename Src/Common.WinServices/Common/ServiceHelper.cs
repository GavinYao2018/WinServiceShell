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
        public static void StartJob()
        {
            Logger.Info("定时任务读取开始");
            ISchedulerFactory sf = new Quartz.Impl.StdSchedulerFactory();
            ServiceHelper._sched = sf.GetScheduler();
            int jobCounter = 0;
            var currentService = ServiceHelper.CurrentWinService;
            if (currentService == null)
            {
                Logger.Error(null, $"找不到【{ServiceHelper.CurrentServiceName}】的配置");
                return;
            }
            foreach (var quartzJob in currentService.QuartzJob.JobItemList)
            {
                //配置检查
                if (string.IsNullOrWhiteSpace(quartzJob.JobKey) || string.IsNullOrWhiteSpace(quartzJob.Assembly) || string.IsNullOrWhiteSpace(quartzJob.QuartzCron))
                {
                    Logger.Error(null, $"QuartzJob.JobItem的JobKey，Assembly和QuartzCron不能为空. JobKey：{quartzJob.JobKey}, Assembly：{quartzJob.Assembly}, QuartzCron：{quartzJob.QuartzCron}");
                    continue;
                }

                //二选一
                if (string.IsNullOrWhiteSpace(quartzJob.ClassName) && string.IsNullOrWhiteSpace(quartzJob.MethodName))
                {
                    Logger.Error(null, $"QuartzJob.JobItem的JobKey={quartzJob.JobKey}的ClassName或MethodName不能为空. ClassName：{quartzJob.ClassName}, MethodName：{quartzJob.MethodName}");
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
                    Logger.Info($"读取成功. JobKey：{quartzJob.JobKey}");
                }
                catch (Exception ex)
                {
                    string msg = $"读取失败. JobKey：{quartzJob.JobKey}";
                    Logger.Error(ex, msg);
                    throw ex;
                }
            }
            Logger.Info("定时任务读取完成");
            if (jobCounter > 0)
            {
                ServiceHelper._sched.Start();
            }
        }

        /// <summary>
        /// 定时任务结束
        /// </summary>
        public static void StopJob()
        {
            if (ServiceHelper._sched != null)
            {
                ServiceHelper._sched.Shutdown(false);
                ServiceHelper._sched = null;
            }
            ServiceHelper.OnStop();
        }

        private static string _currentServiceName;

        /// <summary>
        /// 服务名
        /// </summary>
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

        private static CommonWinService _currentWinService;

        /// <summary>
        /// 获取当前配置的Service配置
        /// </summary>
        /// <returns></returns>
        public static CommonWinService CurrentWinService
        {
            get
            {
                if (_currentWinService != null)
                {
                    return _currentWinService;
                }

                string currentServiceName = ServiceHelper.CurrentServiceName;
                if (string.IsNullOrEmpty(currentServiceName))
                {
                    throw new Exception("ServiceName为空");
                }
                Logger.Info("ServiceName=" + currentServiceName);                
                _currentWinService = CommonWinService.WinServiceList.FirstOrDefault(p => string.Equals(p.ServiceName, currentServiceName, StringComparison.OrdinalIgnoreCase));

                #region 初始化Null值，以防调用出错
                if (_currentWinService != null)
                {
                    if (_currentWinService.ServiceStart == null)
                    {
                        _currentWinService.ServiceStart = new ServiceStart();
                    }
                    if (_currentWinService.ServiceStart.OnStartItemList == null)
                    {
                        _currentWinService.ServiceStart.OnStartItemList = new List<MethodItem>();
                    }
                    if (_currentWinService.ServiceStop == null)
                    {
                        _currentWinService.ServiceStop = new ServiceStop();
                    }
                    if (_currentWinService.ServiceStop.OnStopItemList == null)
                    {
                        _currentWinService.ServiceStop.OnStopItemList = new List<MethodItem>();
                    }
                    if (_currentWinService.QuartzJob == null)
                    {
                        _currentWinService.QuartzJob = new QuartzJob();
                    }
                    if (_currentWinService.QuartzJob.JobItemList == null)
                    {
                        _currentWinService.QuartzJob.JobItemList = new List<JobItem>();
                    }
                }
                #endregion

                return _currentWinService;
            }
        }

        /// <summary>
        /// 定时任务开始需要执行的方法
        /// </summary>
        public static void OnStartInit()
        {
            Logger.Info("服务初始化开始");
            var currentService = ServiceHelper.CurrentWinService;
            if (currentService != null)
            {
                InvokeMethod(currentService.ServiceStart.OnStartItemList);
            }
            else
            {
                Logger.Info($"找不到【{ServiceHelper.CurrentServiceName}】的配置");
            }
            Logger.Info("服务初始化结束");
        }

        /// <summary>
        /// 定时任务结束需要执行的方法
        /// </summary>
        private static void OnStop()
        {
            Logger.Info("服务开始停止");
            var currentService = ServiceHelper.CurrentWinService;
            if (currentService != null)
            {
                InvokeMethod(currentService.ServiceStop.OnStopItemList);
            }
            else
            {
                Logger.Info($"找不到【{ServiceHelper.CurrentServiceName}】的配置");
            }
            Logger.Info("服务停止");
        }

        /// <summary>
        /// 执行开始和结束的方法
        /// </summary>
        /// <param name="methodItems"></param>
        private static void InvokeMethod(List<MethodItem> methodItems)
        {
            foreach (var osi in methodItems)
            {
                if (string.IsNullOrWhiteSpace(osi.Assembly) || string.IsNullOrWhiteSpace(osi.MethodName))
                {
                    Logger.Info($"Assembly和MethodName不能为空. Assembly:{ osi.Assembly}, MethodName: { osi.MethodName}");
                    continue;
                }
                try
                {
                    var assembly = ServiceHelper.GetAssembly(osi.Assembly);

                    var tupple = GetClassAndMethodName(osi.MethodName);
                    var className = tupple.Item1;// 第一个为className
                    var methodName = tupple.Item2;// 第一个为methodName

                    var obj = assembly.CreateInstance(className);
                    Type type = obj.GetType();

                    var methodInfo = type.GetMethod(methodName);
                    object[] arrParam = null;
                    if (!string.IsNullOrEmpty(osi.Parameters))
                    {
                        var arr = osi.Parameters.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (arr.Length > 0)
                        {
                            arrParam = arr.ToArray();
                        }
                    }
                    methodInfo.Invoke(obj, arrParam);
                    Logger.Info($"执行完毕. { osi.Assembly}, { osi.MethodName}");
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, $"执行异常. { osi.Assembly}, { osi.MethodName}");
                }
            }
        }

        /// <summary>
        /// 反射获取程序集信息
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 根据FullName的方法名获取类名以及方法的Name
        /// </summary>
        /// <param name="methodName"></param>
        /// <returns></returns>
        private static Tuple<string, string> GetClassAndMethodName(string methodName)
        {
            //如果执行的方法名不为空
            var iIndex = methodName.LastIndexOf(".");
            var className = methodName.Substring(0, iIndex);//将方法名给到类名
            methodName = methodName.Substring(iIndex + 1); //留下方法名

            return Tuple.Create<string, string>(className, methodName);
        }
    }
}
