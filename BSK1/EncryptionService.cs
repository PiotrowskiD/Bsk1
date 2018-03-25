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

        private long BUFFER_SIZE = 1024*1024; // 1MB

        public void EncryptFile(ProgressBar progressBar, String inputPath, String outputFolderPath, String outputFileName, String encryptionKey, CipherMode mode, String[] fileRecipents) {
            UnicodeEncoding encoding = new UnicodeEncoding();
            byte[] key = encoding.GetBytes(encryptionKey);

            FileStream outputStream = new FileStream(outputFolderPath + outputFileName, FileMode.Create);

            Aes aes = new AesCryptoServiceProvider();
            aes.BlockSize = 128;
            aes.FeedbackSize = 128;
            aes.Mode = mode;
            byte[] iv = SecureRand(aes.BlockSize / 8);

            outputStream.Write(BitConverter.GetBytes(aes.BlockSize), 0, 4);
            outputStream.Write(BitConverter.GetBytes(aes.FeedbackSize), 0, 4);
            outputStream.Write(BitConverter.GetBytes((int)mode), 0, 4);
            outputStream.Write(iv, 0, aes.BlockSize / 8);
            outputStream.Write(BitConverter.GetBytes(fileRecipents.Length), 0, 4);
            foreach(String fileRecipent in fileRecipents) {
                // zapisywanie hashy i kluczy adresatów
            }

            CryptoStream cryptoStream = new CryptoStream(outputStream, aes.CreateEncryptor(key, iv), CryptoStreamMode.Write);

            FileStream inputStream = new FileStream(inputPath, FileMode.Open);
            int data;
            long progress = 0;
            byte[] buffer = new byte[BUFFER_SIZE];
            while ((data = inputStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                progress += data;
                cryptoStream.Write(buffer, 0, data);
                Console.WriteLine(((progress * 100) / inputStream.Length));
                // update progress bar here
            }
            inputStream.Close();
            cryptoStream.Close();
            outputStream.Close();
        }

        public void DecryptFile(ProgressBar progressBar, String inputPath, String outputFolderPath, String outputFileName, String decryptionKey) {
            UnicodeEncoding encoding = new UnicodeEncoding();
            byte[] key = encoding.GetBytes(decryptionKey);

            FileStream inputStream = new FileStream(inputPath, FileMode.Open);
            byte[] metadata = new byte[4];
            inputStream.Read(metadata, 0, 4);
            int blockSize = BitConverter.ToInt32(metadata, 0);
            inputStream.Read(metadata, 0, 4);
            int feedbackSize = BitConverter.ToInt32(metadata, 0);
            inputStream.Read(metadata, 0, 4);
            CipherMode mode = (CipherMode)BitConverter.ToInt32(metadata, 0);
            byte[] iv = new byte[blockSize / 8];
            inputStream.Read(iv, 0, blockSize / 8);
            inputStream.Read(metadata, 0, 4);
            int fileRecipentsCount = BitConverter.ToInt32(metadata, 0);
            for (int i = 0; i < fileRecipentsCount; i++) {
                // check recipent
            }

            Aes aes = new AesCryptoServiceProvider();
            aes.BlockSize = blockSize;
            aes.FeedbackSize = feedbackSize;
            aes.Mode = mode;
            CryptoStream cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(key, iv), CryptoStreamMode.Read);

            FileStream outputStream = new FileStream(outputFolderPath + outputFileName, FileMode.Create);
            int progress = 0;
            int data;
            byte[] buffer = new byte[BUFFER_SIZE];
            while ((data = cryptoStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                progress += data;
                outputStream.Write(buffer, 0, data);
                Console.WriteLine(((progress * 100) / inputStream.Length));
                //progressBar.Dispatcher.Invoke(() => progressBar.Value = ((progress * 100) / inputStream.Length), DispatcherPriority.Background);
            }
            inputStream.Close();
            cryptoStream.Close();
            outputStream.Close();
        }

        private byte[] SecureRand(int arraySize) {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            var byteArray = new byte[arraySize];
            provider.GetBytes(byteArray);
            return byteArray;
        }

    }
}
