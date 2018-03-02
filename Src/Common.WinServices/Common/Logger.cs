using System;
using System.Configuration;
using System.IO;
using System.Text;

namespace Common.WinServices.Common
{
    public static class Logger
    {
        public static void Info(string message)
        {
            string logPath = ConfigurationManager.AppSettings["LogPath"];
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
            string fileName = Path.Combine(logPath, "WinServiceShell.log");

            message = string.Format("{0} {1}{2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffff"), message, Environment.NewLine);
            WireLog(fileName, message);
        }

        public static void Error(Exception ex, string message = null)
        {
            string logPath = ConfigurationManager.AppSettings["LogPath"];
            if (!Directory.Exists(logPath))
            {
                Directory.CreateDirectory(logPath);
            }
            string fileName = Path.Combine(logPath, "WinServiceShell_Error.log");

            var exMsg = ex == null ? "" : ex.ToString();

            message = string.Format("{0} {1} {2}{3}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffff"), message, exMsg, Environment.NewLine);
            WireLog(fileName, message);
        }

        private static void WireLog(string fileName, string message)
        {
            try
            {
                File.AppendAllText(fileName, message, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                message = $"{message}, {ex.ToString()}";
                try
                {
                    string logPath = ConfigurationManager.AppSettings["LogPath"];
                    if (!Directory.Exists(logPath))
                    {
                        Directory.CreateDirectory(logPath);
                    }
                    fileName = Path.Combine(logPath, "WireLog_Error.log");
                    File.AppendAllText(fileName, message, Encoding.UTF8);
                }
                finally
                { }
            }
        }
    }
}