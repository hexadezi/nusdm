using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

// Thanks to:
// https://wiidatabase.de/wii-3ds-und-wii-u-title-key-generierung-geleaked/
// https://gbatemp.net/threads/3ds-wii-u-titlekey-generation-algorithm-leaked.566318/
// https://github.com/V10lator/NUSspli
// https://stackoverflow.com/a/53654681



namespace nusdm.Services
{
    public static class Keygen
    {
        private static readonly string commonKeyNoWhitespace = SettingsProvider.Settings.CommonKey;

        public static string GenerateKey(string tid)
        {
            byte[] secret = DeriveSecret(-3, 10);

            byte[] munged = MungeTid(tid.Replace(" ", ""));

            byte[] hashed = GetHash(secret.Concat(munged).ToArray());

            string pass = "mypass";

            byte[] key;

            using (var pbkdf2 = new Rfc2898DeriveBytes(pass, hashed, 20, HashAlgorithmName.SHA1))
            {
                key = pbkdf2.GetBytes(16);
            }

            byte[] commonKey = HexToByteArray(FormatHexString(commonKeyNoWhitespace));



            // Initialization vector IV: https://en.wikipedia.org/wiki/Initialization_vector
            byte[] iv = HexToByteArray(FormatHexString(tid));

            Array.Resize(ref iv, 16);

            var crypto = new AesCryptographyService();

            var encrypted = crypto.Encrypt(key, commonKey, iv);

            return BitConverter.ToString(encrypted).Replace("-", "").ToUpper();
        }
        public static byte[] DeriveSecret(int start, int len)
        {
            var key = "";

            var add = start + len;

            for (int i = 0; i < len; i++)
            {
                // Convert to hex 
                // Always double-digit output
                var tmp = start.ToString("X2");

                // Add last two characters to key string
                key += tmp.Substring(tmp.Length - 2);

                var next = start + add;

                add = start;

                start = next;
            }

            return HexToByteArray(FormatHexString(key));
        }
        private static byte[] MungeTid(string tid)
        {
            while (tid[0] == '0' && tid[1] == '0')
            {
                tid = tid.Substring(2);
            }

            return HexToByteArray(FormatHexString(tid));
        }
        public static string FormatHexString(string s)
        {
            for (int i = 2; i < s.Length; i += 3)
            {
                s = s.Insert(i, " ");
            }
            return s.ToUpper();
        }
        public static byte[] HexToByteArray(string hex, string separator = " ")
        {
            return hex.Split(separator).Select(o => Convert.ToByte(o, 16)).ToArray();
        }
        public static byte[] GetHash(byte[] arr)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            return md5.ComputeHash(arr);
        }
        public class AesCryptographyService
        {
            public byte[] Encrypt(byte[] data, byte[] key, byte[] iv)
            {
                using (var aes = Aes.Create())
                {
                    aes.KeySize = 128;
                    aes.BlockSize = 128;
                    aes.Padding = PaddingMode.Zeros;

                    aes.Key = key;
                    aes.IV = iv;

                    using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    {
                        return PerformCryptography(data, encryptor);
                    }
                }
            }

            private byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
            {
                using (var ms = new MemoryStream())
                using (var cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.FlushFinalBlock();

                    return ms.ToArray();
                }
            }
        }
    }

}
