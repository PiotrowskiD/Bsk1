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

        public void EncryptFile(String inputPath, String outputPath, String encryptionKey, CipherMode mode) {
            UnicodeEncoding encoding = new UnicodeEncoding();
            byte[] key = encoding.GetBytes(encryptionKey);

            FileStream outputStream = new FileStream(outputPath + ".encrypted", FileMode.Create);

            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.BlockSize = 128;
            aes.FeedbackSize = 128;
            aes.Mode = mode;
            CryptoStream cryptoStream = new CryptoStream(outputStream, aes.CreateEncryptor(key, key), CryptoStreamMode.Write);

            FileStream inputStream = new FileStream(inputPath, FileMode.Open);
            int data;
            while ((data = inputStream.ReadByte()) != -1) {
                cryptoStream.WriteByte((byte)data);
            }
            inputStream.Close();
            cryptoStream.Close();
            outputStream.Close();
        }

        public void DecryptFile(String inputPath, String outputPath, String decryptionKey, CipherMode mode) {
            UnicodeEncoding encoding = new UnicodeEncoding();
            byte[] key = encoding.GetBytes(decryptionKey);

            FileStream inputStream = new FileStream(inputPath, FileMode.Open);

            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.BlockSize = 128;
            aes.FeedbackSize = 128;
            aes.Mode = mode;
            CryptoStream cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(key, key), CryptoStreamMode.Read);

            FileStream outputStream = new FileStream(outputPath + @".decrypted", FileMode.Create);
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
