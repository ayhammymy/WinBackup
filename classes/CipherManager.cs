using Pastel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace WinBackup.classes
{
    public class CipherManager: ICipher
    {
        private ILogger Logger { get; set; }

        public CipherManager(ILogger logger)
        {
            Logger = logger;
        }
         
        public void Encrypt(string inputFile, string outputFile, string key)
        {
            const int BUFFER_SIZE = 8192;
            byte[] buffer = new byte[BUFFER_SIZE];
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(ivBytes);
            }

            using (var inputStream = File.Open(inputFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (var outputStream = File.Open(outputFile, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    outputStream.Write(ivBytes, 0, ivBytes.Length);
                    using (var cryptoAlgo = Aes.Create())
                    {
                        using (var encryptor = cryptoAlgo.CreateEncryptor(keyBytes, ivBytes))
                        {
                            using (var cryptoStream = new CryptoStream(outputStream, encryptor, CryptoStreamMode.Write))
                            {
                                int count;
                                while ((count = inputStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    cryptoStream.Write(buffer, 0, count);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void Decrypt(string inputFile, string outputFile, string key)
        {
            const int BUFFER_SIZE = 8192;
            byte[] buffer = new byte[BUFFER_SIZE];
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = new byte[16];
            using (var inputStream = File.Open(inputFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                inputStream.Read(ivBytes, 0, ivBytes.Length);
                using (var outputStream = File.Open(outputFile, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (var cryptoAlgo = Aes.Create())
                    {
                        using (var decryptor = cryptoAlgo.CreateDecryptor(keyBytes, ivBytes))
                        {
                            using (var cryptoStream = new CryptoStream(inputStream, decryptor, CryptoStreamMode.Read))
                            {
                                int count;
                                while ((count = cryptoStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    outputStream.Write(buffer, 0, count);
                                }
                            }
                        }
                    }
                }
            }
        }
         
    }
}
