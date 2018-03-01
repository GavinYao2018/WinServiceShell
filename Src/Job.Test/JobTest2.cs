using Quartz;
using System;

namespace Job.Test
{
    public class JobTest2 : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Write();
        }

        public void Write()
        {
            var str = Guid.NewGuid().ToString("N");
            Logger.Info("JobTest2.txt", str);
        }
    }
}
