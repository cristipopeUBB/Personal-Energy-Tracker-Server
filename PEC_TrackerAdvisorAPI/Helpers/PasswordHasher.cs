using System.Security.Cryptography;

namespace PEC_TrackerAdvisorAPI.Helpers
{
    public class PasswordHasher
    {
        private static readonly int SaltSize = 16;
        private static readonly int HashSize = 20;
        private static readonly int Iterations = 10000;

        public static string Hash(string password)
        {
            byte[] salt;
            using (var randomNumberGenerator = RandomNumberGenerator.Create())
            {
                salt = new byte[SaltSize];
                randomNumberGenerator.GetBytes(salt);
            }

            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] hash = deriveBytes.GetBytes(HashSize);

                byte[] hashBytes = new byte[SaltSize + HashSize];
                Buffer.BlockCopy(salt, 0, hashBytes, 0, SaltSize);
                Buffer.BlockCopy(hash, 0, hashBytes, SaltSize, HashSize);

                return Convert.ToBase64String(hashBytes);
            }
        }

        public static bool Verify(string password, string hashedPassword)
        {
            byte[] hashBytes = Convert.FromBase64String(hashedPassword);
            byte[] salt = new byte[SaltSize];
            Buffer.BlockCopy(hashBytes, 0, salt, 0, SaltSize);

            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] hash = deriveBytes.GetBytes(HashSize);
                return hashBytes.Skip(SaltSize).SequenceEqual(hash);
            }
        }
    }
}
