//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Security.Cryptography;
//using System.Text;
//using System.Threading.Tasks;


using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class Extensions
{
    // convert a secure string into a normal plain text string
    public static String ToPlainString(this System.Security.SecureString secureStr)
    {
        String plainStr = new System.Net.NetworkCredential(string.Empty,
                          secureStr).Password;
        return plainStr;
    }

    // convert a plain text string into a secure string
    public static System.Security.SecureString ToSecureString(this String plainStr)
    {
        var secStr = new System.Security.SecureString(); secStr.Clear();
        foreach (char c in plainStr.ToCharArray())
        {
            secStr.AppendChar(c);
        }
        return secStr;
    }
}


class Security
{



    public static void Encrypt(string inputFile, string outputFile, string key)
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

    public static void Decrypt(string inputFile, string outputFile, string key)
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







    //class Security
    //{

    //    internal class Protected
    //    {
    //        private Byte[] salt = Guid.NewGuid().ToByteArray();

    //        protected byte[] Protect(byte[] data)
    //        {
    //            try
    //            {
    //                return ProtectedData.Protect(data, salt, DataProtectionScope.CurrentUser);
    //            }
    //            catch (CryptographicException)
    //            {
    //                throw;
    //            }
    //        }

    //        protected byte[] Unprotect(byte[] data)
    //        {
    //            try
    //            {
    //                return ProtectedData.Unprotect(data, salt, DataProtectionScope.CurrentUser);
    //            }
    //            catch (CryptographicException)
    //            {

    //                throw;
    //            }
    //        }
    //    }
    //}
}



//namespace ApacheLogger
//{
//    class Security
//    {
//        public void Encrypt(byte[] toEncrypt)
//        {
//            // Create the original data to be encrypted
//            toEncrypt = UnicodeEncoding.ASCII.GetBytes("This is some data of any length.");

//            // Create a file.
//            FileStream fStream = new FileStream("Data.dat", FileMode.OpenOrCreate);

//            // Create some random entropy.
//            byte[] entropy = CreateRandomEntropy();

//            Console.WriteLine();
//            Console.WriteLine($"Original data: {UnicodeEncoding.ASCII.GetString(toEncrypt)}");
//            Console.WriteLine("Encrypting and writing to disk...");

//            // Encrypt a copy of the data to the stream.
//            int bytesWritten = EncryptDataToStream(toEncrypt, entropy, DataProtectionScope.CurrentUser, fStream);

//            fStream.Close();

//        }

//        public void Decrypt(string fileName)
//        {
//            Console.WriteLine("Reading data from disk and decrypting...");

//            // Open the file.
//            FileStream fStream = new FileStream(fileName, FileMode.Open);

//            // Read from the stream and decrypt the data.
//            byte[] decryptData = DecryptDataFromStream(entropy, DataProtectionScope.CurrentUser, fStream, bytesWritten);

//            fStream.Close();

//            Console.WriteLine($"Decrypted data: {UnicodeEncoding.ASCII.GetString(decryptData)}");
//        }

//        public static byte[] CreateRandomEntropy()
//        {
//            // Create a byte array to hold the random value.
//            byte[] entropy = new byte[16];

//            // Create a new instance of the RNGCryptoServiceProvider.
//            // Fill the array with a random value.
//            new RNGCryptoServiceProvider().GetBytes(entropy);

//            // Return the array.
//            return entropy;
//        }

//        public static int EncryptDataToStream(byte[] Buffer, byte[] Entropy, DataProtectionScope Scope, Stream S)
//        {
//            if (Buffer == null)
//                throw new ArgumentNullException(nameof(Buffer));
//            if (Buffer.Length <= 0)
//                throw new ArgumentException("The buffer length was 0.", nameof(Buffer));
//            if (Entropy == null)
//                throw new ArgumentNullException(nameof(Entropy));
//            if (Entropy.Length <= 0)
//                throw new ArgumentException("The entropy length was 0.", nameof(Entropy));
//            if (S == null)
//                throw new ArgumentNullException(nameof(S));

//            int length = 0;

//            // Encrypt the data and store the result in a new byte array. The original data remains unchanged.
//            byte[] encryptedData = ProtectedData.Protect(Buffer, Entropy, Scope);

//            // Write the encrypted data to a stream.
//            if (S.CanWrite && encryptedData != null)
//            {
//                S.Write(encryptedData, 0, encryptedData.Length);

//                length = encryptedData.Length;
//            }

//            // Return the length that was written to the stream.
//            return length;
//        }

//        public static byte[] DecryptDataFromStream(byte[] Entropy, DataProtectionScope Scope, Stream S, int Length)
//        {
//            if (S == null)
//                throw new ArgumentNullException(nameof(S));
//            if (Length <= 0)
//                throw new ArgumentException("The given length was 0.", nameof(Length));
//            if (Entropy == null)
//                throw new ArgumentNullException(nameof(Entropy));
//            if (Entropy.Length <= 0)
//                throw new ArgumentException("The entropy length was 0.", nameof(Entropy));

//            byte[] inBuffer = new byte[Length];
//            byte[] outBuffer;

//            // Read the encrypted data from a stream.
//            if (S.CanRead)
//            {
//                S.Read(inBuffer, 0, Length);

//                outBuffer = ProtectedData.Unprotect(inBuffer, Entropy, Scope);
//            }
//            else
//            {
//                throw new IOException("Could not read the stream.");
//            }

//            // Return the decrypted data
//            return outBuffer;
//        }
//    }
//}
