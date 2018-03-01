using System;
using System.Configuration;

namespace MethodTest
{
    public class MethodTest2
    {
        public void Write()
        {
            var testKey = ConfigurationManager.AppSettings["TestKey"];
            var str = $"{Guid.NewGuid().ToString("N")}, testkey={testKey}";
            Logger.Info("MethodTest2.txt", str);
        }
    }
}
