using System;
using System.IO;

namespace MethodTest
{
    public class MethodTest1
    {
        public void Write(string name, string i)
        {
            var str = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")} name = {name} : i = {i} {Guid.NewGuid().ToString("N")}\r\n";
            File.AppendAllText("d:\\MethodTest1.txt", str);
        }
    }
}
