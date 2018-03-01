using System.IO;

namespace System
{
    public static class Logger
    {
        static string logPath = "d:\\";
        public static void Info(string fileName, string content)
        {
            string logFullName = Path.Combine(logPath, fileName);
            content = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff")} {content}\r\n";
            File.AppendAllText(logFullName, content);
        }
    }
}
