using System;

namespace MethodTest
{
    public class MethodTest1
    {
        public void Write(string name, string i)
        {
            var str = $"name = {name}, i = {i}, {Guid.NewGuid().ToString("N")}";
            Logger.Info("MethodTest1.txt", str);
        }


        public void OnStart()
        {
            //int i = 0;
            //i = 5 / i;
            Logger.Info("MethodTest1.txt", "启动时执行");
        }
        public void OnStop()
        {
            //int i = 0;
            //i = 5 / i;
            Logger.Info("MethodTest1.txt", "结束时执行");
        }        
    }
}
