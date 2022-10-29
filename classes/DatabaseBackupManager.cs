using Pastel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinBackup;

namespace WinBackup.classes
{
    public class DatabaseBackupManager
    {
        private ILogger Logger { get; set; }
        private ICipher Cipher { get; set; }

        public DatabaseBackupManager(ILogger Logger, ICipher Cipher)
        {
            this.Logger = Logger;
            this.Cipher = Cipher;
        }

        public void BackupToFile(string connString, string DatabaseName, string BackupFilePath)
        {
            Logger.Log("Start Database buckup".Pastel(Color.GreenYellow));
            var sqlCon = new SqlConnection(connString);
            sqlCon.InfoMessage += new SqlInfoMessageEventHandler(myConnection_InfoMessage);
            sqlCon.Open();

            if (sqlCon.State == ConnectionState.Open)
            {
                Console.WriteLine("Connection is open".Pastel(Color.GreenYellow));
            }

            string str = $"USE {DatabaseName};";

            string str1 = $"BACKUP DATABASE {DatabaseName} TO DISK = '{BackupFilePath}' WITH NOFORMAT, NOINIT, NAME = '{DatabaseName}', SKIP, NOREWIND, NOUNLOAD,  STATS = 10;";
            SqlCommand cmd1 = new SqlCommand(str, sqlCon);
            SqlCommand cmd2 = new SqlCommand(str1, sqlCon);
            cmd1.ExecuteNonQuery();
            cmd2.ExecuteNonQuery();
            sqlCon.Close();
            Logger.Log("Connection is closed".Pastel(Color.GreenYellow));

            Logger.Log("Backup is done".Pastel(Color.GreenYellow));
        }

        public void BackupToEncryptedFile(string connString, string DatabaseName, string BackupFilePath, string key)
        {
            Logger.Log("Start Database buckup with encryption".Pastel(Color.GreenYellow));
            if (Cipher is null)
            {
                throw new Exception("Cipher is not set");
            }

            Logger.Log("".Pastel(Color.Green));

            BackupToFile(connString, DatabaseName, BackupFilePath);

            var newEncryptedName = BackupFilePath + "_enc";
            Cipher.Encrypt(BackupFilePath, BackupFilePath + "_enc", key);
            File.Delete(BackupFilePath);
            File.Move(newEncryptedName, BackupFilePath);

            Logger.Log("Encryption is done".Pastel(Color.Green));
        }

        private void myConnection_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            Logger.Log(e.Message);
        }
    }

}
