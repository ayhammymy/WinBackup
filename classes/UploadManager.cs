using Pastel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WinBackup;

namespace WinBackup.classes
{
    class UploadManager
    {

        private ILogger Logger { get; set; }

        public UploadManager(ILogger logger)
        {
            this.Logger = logger;
        }

        public bool Upload(string inputFile, string uploadPath, string bucket)
        {
            byte[] keyInput = new byte[] { 0x41, 0x4B, 0x49, 0x41, 0x5A, 0x57, 0x52, 0x46, 0x48, 0x46, 0x51, 0x45, 0x4E, 0x36, 0x44, 0x57, 0x55, 0x43, 0x48, 0x43 };
            byte[] secInput = new byte[] { 0x2F, 0x79, 0x68, 0x78, 0x37, 0x61, 0x49, 0x67, 0x48, 0x45, 0x59, 0x54, 0x48, 0x4D, 0x2B, 0x72, 0x49, 0x74, 0x55, 0x36, 0x31, 0x72, 0x49, 0x47, 0x79, 0x5A, 0x53, 0x74, 0x53, 0x79, 0x4A, 0x35, 0x6A, 0x69, 0x52, 0x70, 0x76, 0x65, 0x6C, 0x70 };
            string key = Encoding.UTF8.GetString(keyInput);
            string sec = Encoding.UTF8.GetString(secInput);

            SecureString theSecureKey = new NetworkCredential("", key).SecurePassword;
            SecureString theSecureSecret = new NetworkCredential("", sec).SecurePassword;
             
            string resource = "/" + bucket + "/" + uploadPath;
            string contentType = "application/x-compressed-tar";
            string DateString = DateTime.UtcNow.ToString("ddd, dd MMM yyyy HH:mm:ss") + " GMT";
            string stringToSign = $"PUT\n\n{contentType}\n{DateString}\n{resource}";
             
            string signature = "";
            using (var hmacsha1 = new HMACSHA1(Encoding.UTF8.GetBytes(new NetworkCredential("", theSecureSecret).Password), true))
            {
                byte[] hashmessage = hmacsha1.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));
                signature = Convert.ToBase64String(hashmessage);
            }

            HttpClient c = new HttpClient();
            c.DefaultRequestHeaders.Add("Host", $"{bucket}.s3.amazonaws.com");
            c.DefaultRequestHeaders.Add("Date", DateString);
            var content = new StreamContent(new MemoryStream(File.ReadAllBytes(inputFile)));
            content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
             
            c.DefaultRequestHeaders.Add("Authorization", $"AWS {new NetworkCredential("", theSecureKey).Password}:{signature}");
            string uri = $"http://{bucket}.s3.amazonaws.com";

            HttpResponseMessage t = null;
            try
            {
                  t = c.PutAsync(uri + "/" + uploadPath, content).Result;
            }
            catch(Exception ex)
            {
                Logger.Log(ex.Message.Pastel(Color.Red),1);
                return false;
            }
            //var y = t.Content.ReadAsStringAsync().Result;
 
            if (t.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Logger.Log("Upload Done".Pastel(Color.GreenYellow));
            }
            else
            {
                Logger.Log($"Status code: {t.StatusCode}".Pastel(Color.GreenYellow));
            }
            return true;
        }


    }
}
