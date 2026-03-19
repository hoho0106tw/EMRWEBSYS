using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace MRLOGQUERY.Controllers.FunCode
{
    public class Token
    {
        public string UserId { get; set; }       
        public string SubmissionNum { get; set; }
        public string REASION { get; set; }
    }



    public static class TokenCrypt
    {
        private static readonly string Key = "bgF8XA9uEBP7AQ5bpvXd6nBF4dK82DxS"; // 必須 32 bytes
        private static readonly string IV = "bhuxtVeDg9vGZmxA"; // 必須 16 bytes

        public static string Encrypt(Token data)
        {
            string json = JsonConvert.SerializeObject(data);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(Key);
                aes.IV = Encoding.UTF8.GetBytes(IV);

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                MemoryStream msEncrypt = new MemoryStream();
                CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                StreamWriter swEncrypt = new StreamWriter(csEncrypt);
                swEncrypt.Write(json);
                swEncrypt.Flush();
                csEncrypt.FlushFinalBlock();

                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }

        public static Token Decrypt(string token)
        {
            byte[] cipherText = Convert.FromBase64String(token);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(Key);
                aes.IV = Encoding.UTF8.GetBytes(IV);

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                 MemoryStream msDecrypt = new MemoryStream(cipherText);
                 CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                 StreamReader srDecrypt = new StreamReader(csDecrypt);

                string json = srDecrypt.ReadToEnd();
                return JsonConvert.DeserializeObject<Token>(json);
            }
        }
    }




}
