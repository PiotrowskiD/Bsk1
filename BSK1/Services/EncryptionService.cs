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

        private const long BUFFER_SIZE = 1024 * 1024; // 1MB
        public const int AES_BLOCK_SIZE = 128;
        public const int AES_FEEDBACK_SIZE = 128;
        public const int RSA_KEY_SIZE = 2048;

        SessionService sessionService;

        public void SetSessionService(SessionService service)
        {
            sessionService = service;
        }

        public void EncryptFile(ProgressBar progressBar, String inputPath, String outputFolderPath, String outputFileName, byte[] key, CipherMode mode, String[] fileRecipents)
        {
            FileStream outputStream = new FileStream(outputFolderPath + outputFileName, FileMode.Create);

            Aes aes = new AesCryptoServiceProvider();
            aes.BlockSize = AES_BLOCK_SIZE;
            aes.FeedbackSize = AES_FEEDBACK_SIZE;
            aes.Mode = mode;
            aes.Padding = PaddingMode.Zeros;
            byte[] iv = SecureRand(aes.BlockSize / 8);

            // metadata header
            outputStream.Write(BitConverter.GetBytes(aes.BlockSize), 0, 4);
            outputStream.Write(BitConverter.GetBytes(aes.FeedbackSize), 0, 4);
            outputStream.Write(BitConverter.GetBytes((int)mode), 0, 4);
            outputStream.Write(iv, 0, aes.BlockSize / 8);
            outputStream.Write(BitConverter.GetBytes(fileRecipents.Length), 0, 4);
            foreach (String fileRecipent in fileRecipents)
            {
                String recipentDirectory = sessionService.GetUserDirectoryPath(fileRecipent);
                if (recipentDirectory != null)
                {
                    String recipentPublicKey = DecryptRSAKey(GetMD5Hash(fileRecipent), Path.Combine(recipentDirectory, SessionService.RSA_PUBLIC_KEY_FILENAME));
                    byte[] encryptedSessionKey = EncryptSessionKey(recipentPublicKey);
                    byte[] recipentLoginHash = GetMD5Hash(fileRecipent);
                    outputStream.Write(recipentLoginHash, 0, 16); // MD5 hash is always 16 bytes
                    outputStream.Write(BitConverter.GetBytes(encryptedSessionKey.Length), 0, 4);
                    outputStream.Write(encryptedSessionKey, 0, encryptedSessionKey.Length);
                }
                else { // write random data if user not found
                    outputStream.Write(SecureRand(16), 0, 16); // MD5 hash is always 16 bytes
                    outputStream.Write(BitConverter.GetBytes(0), 0, 4);
                }
            }
            outputStream.Flush();

            // file contents
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
            cryptoStream.Close();
            inputStream.Close();
            outputStream.Close();
        }

        public void DecryptFile(ProgressBar progressBar, String inputPath, String outputFolderPath, String outputFileName)
        {
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
            byte[] decryptedSessionKey = SecureRand(AES_FEEDBACK_SIZE / 8);
            for (int i = 0; i < fileRecipentsCount; i++)
            {
                byte[] recipentLoginHash = new byte[16];
                inputStream.Read(recipentLoginHash, 0, 16);
                inputStream.Read(metadata, 0, 4);
                int encryptedSessionKeyLength = BitConverter.ToInt32(metadata, 0);
                byte[] encryptedSessionKey = new byte[encryptedSessionKeyLength];
                inputStream.Read(encryptedSessionKey, 0, encryptedSessionKeyLength);
                byte[] loggedUserLoginHash = GetMD5Hash(sessionService.GetUsername());
                if (recipentLoginHash.SequenceEqual(loggedUserLoginHash))
                {
                    String loggedUserDirectory = sessionService.GetUserDirectoryPath(sessionService.GetUsername());
                    String privateKey = DecryptRSAKey(loggedUserLoginHash, Path.Combine(loggedUserDirectory, SessionService.RSA_PRIVATE_KEY_FILENAME));
                    decryptedSessionKey = DecryptSessionKey(privateKey, encryptedSessionKey);
                }
            }

            Aes aes = new AesCryptoServiceProvider();
            aes.BlockSize = blockSize;
            aes.FeedbackSize = feedbackSize;
            aes.Mode = mode;
            aes.Padding = PaddingMode.Zeros;

            ICryptoTransform decryptor = aes.CreateDecryptor(decryptedSessionKey, iv);
            CryptoStream cryptoStream = new CryptoStream(inputStream, decryptor, CryptoStreamMode.Read);
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

        public void EncryptRSAKey(String rsaKey, byte[] encryptionKey, String outputPath)
        {
            Aes aes = new AesCryptoServiceProvider();
            aes.BlockSize = AES_BLOCK_SIZE;
            aes.FeedbackSize = AES_FEEDBACK_SIZE;
            aes.Mode = CipherMode.CBC;

            UnicodeEncoding encoding = new UnicodeEncoding();
            byte[] rsaKeyBytes = encoding.GetBytes(rsaKey);

            MemoryStream memoryStream = new MemoryStream(rsaKeyBytes);
            FileStream outputStream = new FileStream(outputPath, FileMode.Create);
            CryptoStream cryptoStream = new CryptoStream(outputStream, aes.CreateEncryptor(encryptionKey, encryptionKey), CryptoStreamMode.Write);
            int data;
            byte[] buffer = new byte[BUFFER_SIZE];
            while ((data = memoryStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                cryptoStream.Write(buffer, 0, data);
            }
            memoryStream.Close();
            cryptoStream.Close();
            outputStream.Close();
        }

        public String DecryptRSAKey(byte[] decryptionKey, String keyFilePath)
        {
            Aes aes = new AesCryptoServiceProvider();
            aes.BlockSize = 128;
            aes.FeedbackSize = 128;
            aes.Mode = CipherMode.CBC;

            FileStream inputStream = new FileStream(keyFilePath, FileMode.Open);
            CryptoStream cryptoStream = new CryptoStream(inputStream, aes.CreateDecryptor(decryptionKey, decryptionKey), CryptoStreamMode.Read);
            StreamReader streamReader = new StreamReader(cryptoStream);
            String decryptedRsaKey = streamReader.ReadToEnd();

            inputStream.Close();
            cryptoStream.Close();
            return decryptedRsaKey.Replace("\0", ""); ;
        }

        public byte[] GetMD5Hash(String input)
        {
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            return md5.ComputeHash(inputBytes);
        }

        public byte[] GetSHA256Hash(String input)
        {
            SHA256 sha256 = SHA256Managed.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            return sha256.ComputeHash(inputBytes);
        }

        public void GenerateRSAKeyPair(out String privateKey, out String publicKey)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(RSA_KEY_SIZE);
            rsa.PersistKeyInCsp = false;
            privateKey = ExtractKeyString(rsa.ExportParameters(true));
            publicKey = ExtractKeyString(rsa.ExportParameters(false));
        }

        public byte[] SecureRand(int arraySize)
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            var byteArray = new byte[arraySize];
            provider.GetBytes(byteArray);
            return byteArray;
        }

        private String ExtractKeyString(RSAParameters key)
        {
            var stringWriter = new System.IO.StringWriter();
            var xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            xmlSerializer.Serialize(stringWriter, key);
            return stringWriter.ToString();
        }

        private byte[] EncryptSessionKey(String recipentPublicKey)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(RSA_KEY_SIZE);
            rsa.PersistKeyInCsp = false;
            rsa.FromXmlString(recipentPublicKey);
            return rsa.Encrypt(sessionService.GetSessionKey(), true);
        }

        private byte[] DecryptSessionKey(String loggedUserPrivateKey, byte[] encryptedSessionKey)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(RSA_KEY_SIZE);
            rsa.PersistKeyInCsp = false;
            rsa.FromXmlString(loggedUserPrivateKey);
            return rsa.Decrypt(encryptedSessionKey, true);
        }

    }
}
