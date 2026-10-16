using System;
using System.Security.Cryptography;

namespace DoAn_web.Security
{
    public static class PasswordHasher
    {
        private const string Prefix = "PBKDF2";
        private const int Iterations = 100000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public static string Hash(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Mật khẩu không được để trống.", "password");
            }

            var salt = new byte[SaltSize];
            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(salt);
            }

            byte[] hash;
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                hash = deriveBytes.GetBytes(HashSize);
            }

            return string.Join("$", Prefix, Iterations, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
        }

        public static bool Verify(string password, string storedValue)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedValue))
            {
                return false;
            }

            if (!IsHashed(storedValue))
            {
                return FixedTimeEquals(password, storedValue);
            }

            var parts = storedValue.Split('$');
            int iterations;
            byte[] salt;
            byte[] expectedHash;

            if (parts.Length != 4 || !int.TryParse(parts[1], out iterations) || iterations <= 0)
            {
                return false;
            }

            try
            {
                salt = Convert.FromBase64String(parts[2]);
                expectedHash = Convert.FromBase64String(parts[3]);
            }
            catch (FormatException)
            {
                return false;
            }

            byte[] actualHash;
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                actualHash = deriveBytes.GetBytes(expectedHash.Length);
            }

            return FixedTimeEquals(actualHash, expectedHash);
        }

        public static bool IsHashed(string value)
        {
            return !string.IsNullOrEmpty(value) && value.StartsWith(Prefix + "$", StringComparison.Ordinal);
        }

        private static bool FixedTimeEquals(string left, string right)
        {
            return FixedTimeEquals(System.Text.Encoding.UTF8.GetBytes(left), System.Text.Encoding.UTF8.GetBytes(right));
        }

        private static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            var difference = left.Length ^ right.Length;
            var length = Math.Min(left.Length, right.Length);

            for (var index = 0; index < length; index++)
            {
                difference |= left[index] ^ right[index];
            }

            return difference == 0;
        }
    }
}
