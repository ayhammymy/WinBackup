using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBackup;

namespace WinBackup.classes
{
    public class ConsoleLogger : ILogger
    {
        public bool AddTimeStamp { get; set; }

        public void Log(string text, int Level = 0)
        {
            if (AddTimeStamp == true)
            {
                Console.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] " + text);
            }
            else
            {
                Console.WriteLine(text);
            }
        }
    }

    public class FileLogger : ILogger
    {
        private string FilePath { get; set; }

        public FileLogger(string filePath)
        {
            this.FilePath = filePath;
        }

        public void Log(string text, int Level)
        {
            File.AppendAllText(this.FilePath, text);
        }
    }
}
