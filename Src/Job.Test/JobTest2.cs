using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
            var str = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")} {Guid.NewGuid().ToString("N")}\r\n";
            File.AppendAllText("d:\\JobTest2.txt", str);
        }
    }
}
