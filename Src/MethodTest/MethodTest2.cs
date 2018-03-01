using System;
using System.IO;

namespace MethodTest
{
    public class MethodTest2
    {
        public void Write()
        {
            var str = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")} {Guid.NewGuid().ToString("N")}\r\n";
            File.AppendAllText("d:\\MethodTest2.txt", str);
        }
    }
}
