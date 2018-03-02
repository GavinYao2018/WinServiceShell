using System;
using System.Configuration;

namespace MethodTest
{
    public class MethodTest2
    {
        static string logName = "MethodTest2.txt";
        public void Write()
        {
            var testKey = ConfigurationManager.AppSettings["TestKey"];
            var str = $"{Guid.NewGuid().ToString("N")}, testkey={testKey}";
            Logger.Info(logName, str);
        }
    }
}
