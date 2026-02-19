using System.Security.Cryptography;

namespace MinesweeperWeb.Security
{
    /// <summary>
    /// Provides secure password hashing and verification using PBKDF2.
    /// 
    /// PBKDF2:
    /// - It is a standard, recommended approach for password storage.
    /// - It uses a per-user random salt to prevent rainbow table attacks.
    /// - It uses many iterations to slow down brute-force attempts.
    /// </summary>
    public static class PasswordHelper
    {
        // Recommended salt size for PBKDF2 (128-bit / 16 bytes or higher is common).
        private const int SaltSizeBytes = 16;

        // Recommended key size for PBKDF2 derived hash (256-bit / 32 bytes).
        private const int HashSizeBytes = 32;

        private const int Iterations = 100_000;

        /// <summary>
        /// Generates a cryptographically secure random salt.
        /// </summary>
        /// <returns>Base64-encoded salt string.</returns>
        public static string GenerateSalt()
        {
            byte[] saltBytes = RandomNumberGenerator.GetBytes(SaltSizeBytes);
            return Convert.ToBase64String(saltBytes);
        }

        /// <summary>
        /// Hashes a plain-text password using PBKDF2 with the provided salt.
        /// </summary>
        /// <param name="password">Plain-text password entered by the user.</param>
        /// <param name="base64Salt">Base64-encoded salt stored for the user.</param>
        /// <returns>Base64-encoded password hash.</returns>
        public static string HashPassword(string password, string base64Salt)
        {
            byte[] saltBytes = Convert.FromBase64String(base64Salt);

            // PBKDF2 key derivation using SHA256.
            byte[] hashBytes = Rfc2898DeriveBytes.Pbkdf2(
                password,
                saltBytes,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSizeBytes);

            return Convert.ToBase64String(hashBytes);
        }

        /// <summary>
        /// Verifies a password attempt against the stored hash and salt.
        /// </summary>
        /// <param name="passwordAttempt">Password attempt from login form.</param>
        /// <param name="storedHash">Stored Base64-encoded hash from database.</param>
        /// <param name="storedSalt">Stored Base64-encoded salt from database.</param>
        /// <returns>True if the password matches; otherwise false.</returns>
        public static bool VerifyPassword(string passwordAttempt, string storedHash, string storedSalt)
        {
            
            string attemptHash = HashPassword(passwordAttempt, storedSalt);

            byte[] attemptBytes = Convert.FromBase64String(attemptHash);
            byte[] storedBytes = Convert.FromBase64String(storedHash);

            return CryptographicOperations.FixedTimeEquals(attemptBytes, storedBytes);
        }
    }
}
