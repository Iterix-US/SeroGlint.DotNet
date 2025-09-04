using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace SeroGlint.DotNet.Security
{
    /// <summary>
    /// Provides secure hashing and verification utilities using PBKDF2 with HMAC-SHA256.
    /// This implementation is compatible with .NET Standard 2.0 by manually performing the key derivation steps.
    /// </summary>
    public static class PasswordUtility
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100_000;
        // Defined in case of different algorithm selections in the future
        private const string Algorithm = "PBKDF2-SHA256";
        private const char SegmentDelimiter = ':';

        /// <summary>
        /// Hashes a plaintext password using PBKDF2 with HMAC-SHA256.
        /// </summary>
        /// <param name="password">The plaintext password to hash. This input is normalized using Unicode Form KC before processing.</param>
        /// <returns>
        /// A string containing the algorithm identifier, iteration count, base64-encoded salt, and base64-encoded hash,
        /// delimited by colons. Example format: <c>PBKDF2-SHA256:100000:&lt;base64-salt&gt;:&lt;base64-hash&gt;</c>
        /// </returns>
        public static string HashPassword(string password)
        {
            var normalizedPassword = password.Normalize(NormalizationForm.FormKC);
            var salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var hash = Pbkdf2HmacSha256(normalizedPassword, salt, Iterations, KeySize);

            return string.Join(SegmentDelimiter.ToString(),
                Algorithm,
                Iterations.ToString(),
                Convert.ToBase64String(salt),
                Convert.ToBase64String(hash)
            );
        }

        /// <summary>
        /// Verifies whether the provided password matches the stored hash.
        /// </summary>
        /// <param name="password">The plaintext password to verify. This input is normalized using Unicode Form KC before hashing.</param>
        /// <param name="storedHash">
        /// A previously generated password hash string in the format:
        /// <c>PBKDF2-SHA256:iterations:base64-salt:base64-hash</c>.
        /// </param>
        /// <param name="logger"></param>
        /// <returns><c>true</c> if the password matches the stored hash; otherwise, <c>false</c>.</returns>
        public static bool VerifyPassword(string password, string storedHash, ILogger logger = null)
        {
            try
            {
                var parts = storedHash.Split(SegmentDelimiter);
                if (parts.Length != 4)
                {
                    return false;
                }

                var algorithm = parts[0];
                var iterations = int.Parse(parts[1]);
                var salt = Convert.FromBase64String(parts[2]);
                var expectedHash = Convert.FromBase64String(parts[3]);

                if (algorithm != Algorithm)
                {
                    logger?.LogError("Invalid algorithm");
                    return false;
                }

                var actualHash = Pbkdf2HmacSha256(password, salt, iterations, expectedHash.Length);
                return FixedTimeEquals(actualHash, expectedHash);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error verifying password");
                return false;
            }
        }

        /// <summary>
        /// Performs a constant-time comparison of two byte arrays to mitigate timing attacks.
        /// </summary>
        /// <param name="input">The first byte array.</param>
        /// <param name="hash">The second byte array to compare against.</param>
        /// <param name="logger"></param>
        /// <returns><c>true</c> if both arrays are equal in content and length; otherwise, <c>false</c>.</returns>
        public static bool FixedTimeEquals(byte[] input, byte[] hash, ILogger logger = null)
        {
            if (input.Length != hash.Length)
            {
                logger?.LogError("Invalid hash length");
                return false;
            }

            var result = 0;
            for (var i = 0; i < input.Length; i++)
            {
                result |= input[i] ^ hash[i];
            }

            return result == 0;
        }

        /// <summary>
        /// Derives a cryptographic key using PBKDF2 with HMAC-SHA256.
        /// </summary>
        /// <param name="password">The normalized password to derive the key from.</param>
        /// <param name="salt">The cryptographic salt.</param>
        /// <param name="iterations">The number of PBKDF2 iterations to perform.</param>
        /// <param name="outputBytes">The desired length of the derived key in bytes.</param>
        /// <returns>A byte array containing the derived key.</returns>
        public static byte[] Pbkdf2HmacSha256(string password, byte[] salt, int iterations, int outputBytes)
        {
            using (var hmac = new HMACSHA256())
            {
                var hashLength = hmac.HashSize / 8;
                var numBlocks = (int)Math.Ceiling((double)outputBytes / hashLength);
                var derived = new byte[outputBytes];
                var buffer = new byte[hashLength];
                var block = new byte[salt.Length + 4];

                Buffer.BlockCopy(salt, 0, block, 0, salt.Length);

                var passwordBytes = Encoding.UTF8.GetBytes(password);

                for (var i = 1; i <= numBlocks; i++)
                {
                    block[salt.Length] = (byte)(i >> 24);
                    block[salt.Length + 1] = (byte)(i >> 16);
                    block[salt.Length + 2] = (byte)(i >> 8);
                    block[salt.Length + 3] = (byte)(i);

                    byte[] source;
                    using (var hmacInner = new HMACSHA256(passwordBytes))
                    {
                        source = hmacInner.ComputeHash(block);
                    }

                    Buffer.BlockCopy(source, 0, buffer, 0, hashLength);
                    for (var j = 1; j < iterations; j++)
                    {
                        using (var hmacIter = new HMACSHA256(passwordBytes))
                        {
                            source = hmacIter.ComputeHash(source);
                        }
                        for (var k = 0; k < hashLength; k++)
                            buffer[k] ^= source[k];
                    }

                    var offset = (i - 1) * hashLength;
                    var toCopy = Math.Min(hashLength, outputBytes - offset);
                    Buffer.BlockCopy(buffer, 0, derived, offset, toCopy);
                }

                return derived;
            }
        }
    }
}
