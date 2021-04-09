using System;

namespace jahndigital.studentbank.utils
{
    /// <summary>
    /// Simple utility class to generate a random invite code of a given length.
    /// </summary>
    public static class InviteCode
    {
        /// <summary>
        /// List of valid characters for invite codes.
        /// </summary>
        private const string VALID_CHARS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        /// <summary>
        /// Create a new invite code of the specified length and return it.
        /// </summary>
        /// <param name="length"></param>
        public static string NewCode(int length = 6)
        {
            var chars = new char[length];
            var rng = new Random();

            for (int i = 0; i < length; i++) {
                chars[i] = VALID_CHARS[rng.Next(0, VALID_CHARS.Length)];
            }

            return new String(chars);
        }
    }
}
