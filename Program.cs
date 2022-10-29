using Pastel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using Newtonsoft.Json;
using WinBackup.classes;

namespace ConsoleApp3
{


    class Program
    {
        public static bool weAreExitting = false;
        private static object _privateObject = new object();

        public enum EnumLoggerType { Console, File };

        static EnumLoggerType LoggerType = EnumLoggerType.Console;

        public static string logFilePath = "";

        static void Log(string log)
        {
            if (LoggerType == EnumLoggerType.Console)
            {
                Console.WriteLine(log);
            }
            else if (LoggerType == EnumLoggerType.File)
            {
                File.AppendAllText(logFilePath, log);
            }
        }

        static int delay()
        {
            Thread.Sleep(2000);
            return 1;
        }

        public static string path = @".\sounds\";
        static void checkStatus()
        {


            // var site = item.Value;

            //try
            //{

            //    var contents = wc.DownloadString(site.url);
            //    if (contents.Length == 10 && contents != site.oldStatus)
            //    {
            //        site.oldStatus = contents;
            //        site.isUp = true;
            //        site.error = "";
            //    }
            //}
            //catch (Exception ex)
            //{
            //    site.isUp = false;
            //    site.error = ex.Message;
            //}


            Console.WriteLine(DateTime.Now.ToString("hh:mm tt"));
            System.Media.SoundPlayer player = new System.Media.SoundPlayer();
            //  item = "";
            {
                var error = "";

                // error = item.Value.error.Pastel(Color.White);
                player.SoundLocation = path + "synthetic-intruder.wav";
                player.Load();
                player.Play();


                // var str = item.Key + " : " + (item.Value.isUp == true ? "Up".Pastel(Color.GreenYellow) : "Down".Pastel(Color.Red)) + " " + error;
                //Console.WriteLine( str.PadRight(Console.WindowWidth));

            }


        }









        class AppSetting
        {
            public string bucket { get; set; }
            public string connStr { get; set; }
            public string storageDir { get; set; }
            public List<string> dbName { get; set; }
        }

        static int Main(string[] args)
        {
            var l = new ConsoleLogger();
            l.AddTimeStamp = true;

            var argsList = args.ToList();
            bool deleteBkFile = false;
            bool encryptFile = false;
            if (args.Length < 1)
            {
                l.Log("args: ConfFile".Pastel(Color.Red));
                Console.ReadKey();
                return -1;
            }

            string settingFile = args[0];
            if (File.Exists(settingFile) == false)
            {
                l.Log($"{settingFile} is not found".Pastel(Color.Red));
                Console.ReadKey();
                return -1;
            }

            if (argsList.IndexOf("-delete") >= 0)
            {
                l.Log("* Will Delete the file after upload".Pastel(Color.Orange));
                deleteBkFile = true;
            }

            if (argsList.IndexOf("-encrypt") >= 0)
            {
                l.Log("* Will Encrypt the file".Pastel(Color.Orange));
                encryptFile = true;
            }

            var result = new AppSetting();
            try
            {
                string data = File.ReadAllText(settingFile);
                result = JsonConvert.DeserializeObject<AppSetting>(data);
            }
            catch (Exception ex)
            {
                l.Log(ex.Message.Pastel(Color.Red));
                return -1;
            }

            string conString = result.connStr;
            string bucket = result.bucket;
            var ci = new CipherManager(l);
            var bm = new DatabaseBackupManager(l, ci);
            string dbStorageFile = "";

            foreach (var dbName in result.dbName)
            {
                dbStorageFile = result.storageDir + $"/point_{dbName}_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm") + ".difenc";
                if (encryptFile == true)
                {
                    var key = new SecureString();
                    key.AppendChar('1');
                    key.AppendChar('2');
                    key.AppendChar('3');
                    key.AppendChar('4');
                    key.AppendChar('5');
                    key.AppendChar('6');
                    key.AppendChar('7');
                    key.AppendChar('8');
                    key.AppendChar('!');
                    key.AppendChar('@');
                    key.AppendChar('#');
                    key.AppendChar('$');
                    key.AppendChar('%');
                    key.AppendChar('^');
                    key.AppendChar('&');
                    key.AppendChar('*');
                    bm.BackupToEncryptedFile(conString, dbName, dbStorageFile, key.ToPlainString());
                }
                else
                {
                    dbStorageFile = result.storageDir + $"/point_{dbName}_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm") + ".dif";
                    bm.BackupToFile(conString, dbName, dbStorageFile);
                }

                string uploadPath = "myStorageRzf/" + Path.GetFileName(dbStorageFile);

                l.Log("Start doing upload".Pastel(Color.Orange));
                UploadManager um = new UploadManager(l);

                var uResult = um.Upload(dbStorageFile, uploadPath, bucket);
                if (uResult == false)
                {
                    l.Log("Upload Failed".Pastel(Color.Red));
                    return -1;
                }

                if (deleteBkFile == true)
                {
                    l.Log("Will Delete local backup file".Pastel(Color.DarkRed));
                    File.Delete(dbStorageFile);
                }
            }

            l.Log("Finished".Pastel(Color.Yellow));
            return 0;
            //Console.ReadKey();
            //return;
            //var allTasks = new List<Task>();

            ////var t = new Task(() => logAccessFile(item));
            ////t.Start();
            ////allTasks.Add(t);

            //Task.WaitAll(allTasks.ToArray());

        }


        //public static void logAccessFile(string sp)
        //{
        //    var wh = new AutoResetEvent(false);
        //    var fsw = new FileSystemWatcher(Path.GetDirectoryName(sp.logFile));
        //    fsw.Filter = "file-to-read";
        //    fsw.EnableRaisingEvents = true;
        //    fsw.Changed += (s, e) => wh.Set();
        //x:
        //    wh.Reset();
        //    var fs = new FileStream(sp.logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        //    using (var sr = new StreamReader(fs))
        //    {
        //        var s = "";
        //        sr.BaseStream.Seek(0, SeekOrigin.End);
        //        try
        //        {
        //            while (true)
        //            {
        //                s = sr.ReadLine();
        //                if (s != null && sp.Excludes.Any(x => s.Contains(x)))
        //                {
        //                    continue;
        //                }
        //                if (sr.BaseStream.Length < sr.BaseStream.Position)
        //                {
        //                    sr.BaseStream.Seek(0, SeekOrigin.Begin);
        //                    Console.WriteLine("file is truncated");
        //                }
        //                if (s != null)
        //                    process(sp, s);
        //                else
        //                    wh.WaitOne(1000);

        //                if (weAreExitting == true)
        //                    break;



        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
        //            Console.WriteLine(ex.Message);
        //            sr.Close();
        //            wh.Close();
        //            fs.Close();
        //            goto x;
        //        }
        //    }
        //    wh.Close();
        //    fs.Close();
        //}


        //public static void process( string sp, string dataLine)
        //{
        //    lock (_privateObject)
        //    {
        //        if (string.IsNullOrEmpty(dataLine))
        //            return;
        //        var lines = dataLine.Split(new char[] { ' ' });
        //        if (lines.Length < 9)
        //            return;

        //        var siteNameStyled = sp.siteName.Pastel(sp.siteNameStyle);
        //        var statusCode = lines[8];
        //        var statusCodeStyled = styleStatusCode(statusCode);
        //        var verb = lines[5].Replace("\"", "");
        //        var verbStyled = styleVerb(verb);
        //        var ip = lines[0];
        //        var ipStyled = ip.PadRight(15).Pastel(Color.White);
        //        var httpProtocol = lines[7];
        //        var requestPath = "";
        //        var parsedTime = DateTime.ParseExact((lines[3] + " " + lines[4]).Replace("[", "").Replace("]", ""), "dd/MMM/yyyy:HH:mm:ss K", CultureInfo.CreateSpecificCulture("en-US"));


        //        if (!httpProtocol.Contains("HTTP/1.1"))
        //        {
        //            requestPath = (lines[6].Replace("\"", "") + " " + httpProtocol);
        //        }
        //        else
        //        {
        //            requestPath = (lines[6].Replace("\"", ""));
        //        }

        //        var requestPathStyled = requestPath.Pastel(Color.Wheat);
        //        var executeTime = "";
        //        var timeFormated = parsedTime.ToString("d-M-yyyy hh:mm:ss:tt").Pastel(Color.Gold);

        //        Console.WriteLine($"{siteNameStyled} {ipStyled} {verbStyled}:{statusCodeStyled} {requestPathStyled} {timeFormated}");
        //        sound(statusCode);
        //    }
        //}


        public static void sound(string statusCode)
        {

            System.Media.SoundPlayer player = new System.Media.SoundPlayer();

            switch (statusCode)
            {
                case "200":
                    player.SoundLocation = path + "ambient-glass.wav";
                    break;
                case "304":
                    player.SoundLocation = path + "ambient-glass.wav";
                    break;
                case "400":
                    player.SoundLocation = path + "percussive-synthkick-lo.wav";
                    break;
                case "401":
                case "403":
                    player.SoundLocation = path + "resonant-organ.wav";
                    break;
                case "404":
                    player.SoundLocation = path + "ambient-swoosh.wav";
                    break;
                case "500":
                    player.SoundLocation = path + "percussive-bongo-hi.wav";
                    break;
                default:
                    player.SoundLocation = path + "ambient-glass.wav";
                    break;
            }
            player.Load();
            player.Play();
        }

        public static string styleStatusCode(string statusCode)
        {
            statusCode = statusCode.Trim();
            switch (statusCode)
            {
                case "200":
                    return statusCode.Pastel(Color.LimeGreen);
                    break;
                case "400":
                    return statusCode.Pastel(Color.Red);
                    break;
                case "401": // unAuthorized
                    return statusCode.Pastel(Color.OrangeRed).PastelBg(Color.Yellow); ;
                    break;
                case "403": // Forbid
                    return statusCode.Pastel(Color.Red).PastelBg(Color.Yellow);
                    break;
                case "404": // Not found
                    return statusCode.Pastel(Color.Orange);
                    break;
                case "500": // Server error
                    return statusCode.Pastel(Color.Fuchsia);
                    break;
                default:
                    return statusCode.Pastel(Color.GreenYellow);
                    break;
            }
        }

        public static string styleVerb(string verb)
        {
            switch (verb.ToLower())
            {
                case "get":
                    return verb.PadLeft(7).Pastel(Color.LimeGreen);

                case "post":
                    return verb.PadLeft(7).Pastel(Color.Yellow);

                case "options":
                    return verb.PadLeft(7).Pastel(Color.DarkGray);

                case "update":
                    return verb.PadLeft(7).Pastel(Color.Red).PastelBg(Color.Yellow);


                default:
                    return verb.PadLeft(7).Pastel(Color.GreenYellow);

            }
        }
    }
}
