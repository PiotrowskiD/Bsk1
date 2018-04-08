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
    public class EncryptionService
    {

        private const long BUFFER_SIZE = 1024*1024; // 1MB
        private const int AES_BLOCK_SIZE = 256;
        private const int AES_FEEDBACK_SIZE = 256;
        private const int RSA_KEY_SIZE = 2048;

        SessionService sessionService;

        public void SetSessionService(SessionService service) {
            sessionService = service;
        }

        public void EncryptFile(ProgressBar progressBar, String inputPath, String outputFolderPath, String outputFileName, String encryptionKey, CipherMode mode, String[] fileRecipents) {
            UnicodeEncoding encoding = new UnicodeEncoding();
            byte[] key = encoding.GetBytes(encryptionKey);

            FileStream outputStream = new FileStream(outputFolderPath + outputFileName, FileMode.Create);

            Aes aes = new AesCryptoServiceProvider();
            aes.BlockSize = AES_BLOCK_SIZE;
            aes.FeedbackSize = AES_FEEDBACK_SIZE;
            aes.Mode = mode;
            byte[] iv = SecureRand(aes.BlockSize / 8);

            outputStream.Write(BitConverter.GetBytes(aes.BlockSize), 0, 4);
            outputStream.Write(BitConverter.GetBytes(aes.FeedbackSize), 0, 4);
            outputStream.Write(BitConverter.GetBytes((int)mode), 0, 4);
            outputStream.Write(iv, 0, aes.BlockSize / 8);
            outputStream.Write(BitConverter.GetBytes(fileRecipents.Length), 0, 4);
            foreach(String fileRecipent in fileRecipents) {
                String recipentDirectory = Path.Combine(sessionService.GetUserDirectoryPath(fileRecipent),SessionService.RSA_PUBLIC_KEY_FILENAME);
                // TODO :szyfrowanie klucza sesji
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

        public void EncryptRSAKey(String rsaKey, byte[] encryptionKey, String outputPath) {
            Aes aes = new AesCryptoServiceProvider();
            aes.BlockSize = 256;
            aes.FeedbackSize = 256;
            aes.Mode = CipherMode.CBC;

            UnicodeEncoding encoding = new UnicodeEncoding();
            byte[] rsaKeyBytes = encoding.GetBytes(rsaKey);

            MemoryStream memoryStream = new MemoryStream(rsaKeyBytes);
            FileStream outputStream = new FileStream(outputPath, FileMode.Create);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(key, key), CryptoStreamMode.Write);
            int data;
            byte[] buffer = new byte[BUFFER_SIZE];
            while ((data = memoryStream.Read(buffer, 0, buffer.Length)) != 0) {
                cryptoStream.Write(buffer, 0, data);
            }
            memoryStream.Close();
            cryptoStream.Close();
            outputStream.Close();
        }

        public String DecryptRSAKey(byte[] decryptionKey, String keyFilePath) {
            Aes aes = new AesCryptoServiceProvider();
            aes.BlockSize = 256;
            aes.FeedbackSize = 256;
            aes.Mode = CipherMode.CBC;

            FileStream inputStream = new FileStream(keyFilePath, FileMode.Open);
            CryptoStream cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(key, key), CryptoStreamMode.Read);
            StreamReader streamReader = new StreamReader(cryptoStream);
            String decryptedRsaKey = streamReader.ReadToEnd();

            inputStream.Close();
            cryptoStream.Close();
            return decryptedRsaKey;
        }

        public byte[] GetMD5Hash(String input) {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            return md5.ComputeHash(inputBytes);
        }

        public byte[] GetSHA256Hash(String input) {
            SHA256 sha256 = SHA256Managed.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            return sha256.ComputeHash(inputBytes);
        }

        public void GenerateRSAKeyPair(out String privateKey, out String publicKey) {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.PersistKeyInCsp = false;
            privateKey = ExtractKeyString(rsa.ExportParameters(true));
            publicKey = ExtractKeyString(rsa.ExportParameters(false));
        }

        private String ExtractKeyString(RSAParameters key) {
            var stringWriter = new System.IO.StringWriter();
            var xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            xmlSerializer.Serialize(stringWriter, key);
            return stringWriter.ToString();
        }

        private byte[] SecureRand(int arraySize) {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            var byteArray = new byte[arraySize];
            provider.GetBytes(byteArray);
            return byteArray;
        }

    }
}
