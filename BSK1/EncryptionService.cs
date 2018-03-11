using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BSK1
{
    class EncryptionService
    {

        public void Hello() {
            Console.WriteLine("EncryptionService");
        }

        public void EncryptFile(String path, String output) {
            String password = @"test1234"; // musi być 16 bajtów
            UnicodeEncoding encoding = new UnicodeEncoding();
            byte[] key = encoding.GetBytes(password);

            FileStream outputStream = new FileStream(output + ".encrypted", FileMode.Create);

            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.BlockSize = 128;
            aes.FeedbackSize = 128;
            aes.Mode = CipherMode.CBC;
            CryptoStream cryptoStream = new CryptoStream(outputStream, aes.CreateEncryptor(key, key), CryptoStreamMode.Write);

            FileStream inputStream = new FileStream(path, FileMode.Open);
            int data;
            while ((data = inputStream.ReadByte()) != -1) {
                cryptoStream.WriteByte((byte)data);
            }
            inputStream.Close();
            cryptoStream.Close();
            outputStream.Close();
        }

        public void DecryptFile(String path, String output) {
            String password = @"test1234";
            UnicodeEncoding encoding = new UnicodeEncoding();
            byte[] key = encoding.GetBytes(password);

            FileStream inputStream = new FileStream(path, FileMode.Open);

            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.BlockSize = 128;
            aes.FeedbackSize = 128;
            aes.Mode = CipherMode.CBC;
            CryptoStream cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(key, key), CryptoStreamMode.Read);

            FileStream outputStream = new FileStream(output, FileMode.Create);
            int data;
            while ((data = cryptoStream.ReadByte()) != -1)
            {
                outputStream.WriteByte((byte)data);
            }
            inputStream.Close();
            cryptoStream.Close();
            outputStream.Close();
        }

    }
}
