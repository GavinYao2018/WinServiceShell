using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

namespace Common.WinServices.Common
{
    public static class Logger
    { 
        public static void Log(string logName, string message)
        {
            message = string.Format("{0}{2}{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffff"), message, Environment.NewLine);
            string filepath = Path.Combine(ConfigurationManager.AppSettings["TxtLogPath"], DateTime.Now.ToString("yyyy-MM-dd"), logName);
            if (!Directory.Exists(filepath))
            {
                Directory.CreateDirectory(filepath);
            }
            string filename = string.Format("{0}{1}{2}.txt", filepath, Path.DirectorySeparatorChar, DateTime.Now.ToString("HH"));
            File.AppendAllText(filename, message + Environment.NewLine, System.Text.Encoding.UTF8);
        }
    }
}
