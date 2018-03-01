using Quartz;
using System;

namespace Job.Test
{
    public class JobTest1 : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            string name = "name"; //默认值
            string i = "i";

            //读取参数
            var p = Convert.ToString(context.JobDetail.JobDataMap.Get("Parameters"));

            //自行解析
            if (!string.IsNullOrEmpty(p))
            {
                var arr = p.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (arr.Length > 1)
                {
                    name = arr[0];
                    i = arr[1];
                }
            }

            Write(name, i);
        }

        public void Write(string name, string i)
        {
            var str = $"name = {name}, i = {i}, {Guid.NewGuid().ToString("N")}";
            Logger.Info("JobTest1.txt", str);
        }


        public void OnStart()
        {
            Logger.Info("JobTest1.txt", "启动时执行");
        }
        public void OnStop()
        {
            Logger.Info("JobTest1.txt", "结束时执行");
        }
    }
}
