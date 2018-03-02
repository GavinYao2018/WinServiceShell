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

            var msg = "启动时执行";
            Logger.Info("MethodTest1.txt", msg);
        }
        public void OnStop(string name, string i)
        {
            //int i = 0;
            //i = 5 / i;

            var msg = $"结束时执行. name={name}, i={i}";
            Logger.Info("MethodTest1.txt", msg);
        }
    }
}
