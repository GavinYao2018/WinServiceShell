using Quartz;
using System;

namespace Job.Test
{
    public class JobTest2 : IJob
    {
        static string logName = "JobTest2.txt";
        public void Execute(IJobExecutionContext context)
        {
            Write();
        }

        public void Write()
        {
            var str = Guid.NewGuid().ToString("N");
            Logger.Info(logName, str);
        }
    }
}
