using Quartz;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
            var str = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")} : name = {name}  i = {i} {Guid.NewGuid().ToString("N")}\r\n";
            File.AppendAllText("d:\\JobTest1.txt", str);
        }
    }
}
