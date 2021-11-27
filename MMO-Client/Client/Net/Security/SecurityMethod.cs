using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace MMO_Client.Client.Net.Security
{
    internal class SecurityMethod : IDisposable
    {
        public string SecurityRequestKey { get; init; }

        private readonly RijndaelManaged rijndael;
        private readonly char[] hexChars = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

        public SecurityMethod(string securityRequestKey)
        {
            SecurityRequestKey = securityRequestKey;

            rijndael = new RijndaelManaged
            {
                BlockSize = 128,
                KeySize = 128,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.Zeros
            };
        }

        public string[] CreateValidationDigest(string key)
        {
            StringBuilder validationSB = new(28);
            validationSB.Append(Prepare(MainWindow.Username));
            validationSB.Append(DateTimeOffset.Now.ToUnixTimeMilliseconds());

            while (validationSB.Length % 16 != 0)
                validationSB.Append('0');

            string validationStr = validationSB.ToString();
            string hash = EncryptRijndael(validationStr, key);
            string num = validationStr[MainWindow.Username.Length..];

            return new string[] { num, hash };
        }

        private string Prepare(string s) =>
            s.Replace('Ñ', 'N');

        private string EncryptRijndael(string value, string encryptionKey)
        {
            byte[] key = Encoding.UTF8.GetBytes(encryptionKey);
            rijndael.Key = key;
            rijndael.IV = key;

            ICryptoTransform transform = rijndael.CreateEncryptor();
            using MemoryStream ms = new();
            using CryptoStream cs = new(ms, transform, CryptoStreamMode.Write);
            byte[] buffer = Encoding.UTF8.GetBytes(value);

            cs.Write(buffer, 0, buffer.Length);
            cs.FlushFinalBlock();
            cs.Close();

            ms.Close();
            return CharsToHex(ms.ToArray());
        }

        /*private string DecryptRijndael(string value, string encryptionKey)
        {
            byte[] key = Encoding.UTF8.GetBytes(encryptionKey);
            byte[] buffer = Encoding.UTF8.GetBytes(value);
            rijndael.IV = key;
            rijndael.Key = key;
            ICryptoTransform transform = rijndael.CreateDecryptor();
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, transform, CryptoStreamMode.Write))
                {
                    cs.Write(buffer, 0, buffer.Length);
                    cs.FlushFinalBlock();
                    cs.Close();
                }
                ms.Close();
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }*/

        private string CharsToHex(byte[] bytes)
        {
            StringBuilder sb = new(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(hexChars[bytes[i] >> 4]);
                sb.Append(hexChars[bytes[i] & 15]);
            }

            return sb.ToString();
        }

        public void Dispose() =>
            rijndael.Dispose();
    }
}