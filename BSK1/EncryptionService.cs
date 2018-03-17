using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace BSK1
{
    class EncryptionService
    {

        private long BUFFER_SIZE = 1048576; // 1MB

        public void EncryptFile(ProgressBar progressBar, String inputPath, String outputFolderPath, String outputFileName, String encryptionKey, CipherMode mode) {
            UnicodeEncoding encoding = new UnicodeEncoding();
            byte[] key = encoding.GetBytes(encryptionKey);

            FileStream outputStream = new FileStream(outputFolderPath + outputFileName, FileMode.Create);

            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.BlockSize = 128;
            aes.FeedbackSize = 128;
            aes.Mode = mode;
            CryptoStream cryptoStream = new CryptoStream(outputStream, aes.CreateEncryptor(key, key), CryptoStreamMode.Write);

            FileStream inputStream = new FileStream(inputPath, FileMode.Open);
            int data;
            int progress = 0;
            byte[] buffer = new byte[BUFFER_SIZE];
            while ((data = inputStream.Read(buffer, 0, buffer.Length)) != -1)
            {
                progress++;
                cryptoStream.WriteByte((byte)data);
                progressBar.Dispatcher.Invoke(() => progressBar.Value = ((progress*100)/inputStream.Length), DispatcherPriority.Background);
            }
            inputStream.Close();
            cryptoStream.Close();
            outputStream.Close();
        }

        public void DecryptFile(ProgressBar progressBar, String inputPath, String outputFolderPath, String outputFileName, String decryptionKey, CipherMode mode) {
            UnicodeEncoding encoding = new UnicodeEncoding();
            byte[] key = encoding.GetBytes(decryptionKey);

            FileStream inputStream = new FileStream(inputPath, FileMode.Open);

            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.BlockSize = 128;
            aes.FeedbackSize = 128;
            aes.Mode = mode;
            CryptoStream cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(key, key), CryptoStreamMode.Read);

            FileStream outputStream = new FileStream(outputFolderPath + outputFileName, FileMode.Create);
            int progress = 0;
            int data;
            byte[] buffer = new byte[BUFFER_SIZE];
            while ((data = cryptoStream.Read(buffer, 0, buffer.Length)) != -1)
            {
                progress++;
                outputStream.Write(buffer, 0, data);
                progressBar.Dispatcher.Invoke(() => progressBar.Value = ((progress * 100) / inputStream.Length), DispatcherPriority.Background);
            }
            inputStream.Close();
            cryptoStream.Close();
            outputStream.Close();
        }

    }
}
