using BSK1.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSK1
{
    public class SessionService {

        public const String PASSWORD_HASH_FILENAME = @"PH";
        public const String RSA_PUBLIC_KEY_FILENAME = @"PU";
        public const String RSA_PRIVATE_KEY_FILENAME = @"PR";

        private EncryptionService encryptionService;
        private String dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SE9000");

        // current session
        private String loggedUserUsername;
        private String loggedUserDataDirectory;
        private byte[] sessionKey;

        public SessionService(EncryptionService encryptionService) {
            this.encryptionService = encryptionService;
            Directory.CreateDirectory(dataDirectory); // if data folder does not exist - create it
        }

        public void Authenticate(String login, String password) {
            byte[] loginHash = encryptionService.GetMD5Hash(login);
            byte[] passwordHash = encryptionService.GetSHA256Hash(password);
            byte[] registeredUserPasswordHash = GetRegisteredUserPasswordHash(Convert.ToBase64String(loginHash));
            if (!passwordHash.SequenceEqual(registeredUserPasswordHash)) {
                throw new AuthenticationException();
            }
            loggedUserUsername = login;
        }

        public void Register(String login, String password) {
            String userDirectory = GetUserDirectoryPath(login);
            if (Directory.Exists(userDirectory)) {
                throw new UserAlreadyExistsException();
            }
            Directory.CreateDirectory(userDirectory);
            byte[] passwordHash = encryptionService.GetSHA256Hash(password);
            File.WriteAllBytes(Path.Combine(userDirectory, PASSWORD_HASH_FILENAME), passwordHash);
            String rsaPrivateKey;
            String rsaPublicKey;
            encryptionService.GenerateRSAKeyPair(out rsaPrivateKey, out rsaPublicKey);
            byte[] encryptionKey = GetRegisteredUserPasswordHash(Convert.ToBase64String(encryptionService.GetMD5Hash(loggedUserUsername)));
            encryptionService.EncryptRSAKey(rsaPublicKey, encryptionKey, Path.Combine(userDirectory, RSA_PUBLIC_KEY_FILENAME));
            encryptionService.EncryptRSAKey(rsaPrivateKey, encryptionKey, Path.Combine(userDirectory, RSA_PRIVATE_KEY_FILENAME));
        }

        public byte[] GetRegisteredUserPasswordHash(String loginHash) {
            String userDirectory = Path.Combine(dataDirectory, loginHash);
            if (Directory.Exists(userDirectory)) {
                return File.ReadAllBytes(Path.Combine(userDirectory, PASSWORD_HASH_FILENAME));
            }
            return null;
        }

        public String GetUserDirectoryPath(String login) {
            String loginHashB64 = Convert.ToBase64String(encryptionService.GetMD5Hash(login));
            return Path.Combine(dataDirectory, loginHashB64);
        }

    }
}
