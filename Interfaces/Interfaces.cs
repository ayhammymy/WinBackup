using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinBackup
{
    public interface ILogger
    {
        //  bool AddTimeStamp { get; set; }
        void Log(string text, int Level = 0);
    }

    public interface ICipher
    {
        void Encrypt(string inputFile, string outputFile, string key);
        void Decrypt(string inputFile, string outputFile, string key);
    }
}
