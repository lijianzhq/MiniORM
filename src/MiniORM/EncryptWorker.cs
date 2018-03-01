using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace MiniORM
{
    public class EncryptWorker : IEncryptWorker
    {
        protected String _key;

        public EncryptWorker(String key)
        {
            if (String.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");
            _key = EncryptHelper.EncryptMD5(key);
        }

        public String Encrypt(String text)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            Byte[] inputByteArray = Encoding.Default.GetBytes(text);
            //Key和IV的Byte数组长度为8即可
            Byte[] inputKey = ASCIIEncoding.ASCII.GetBytes(_key.Substring(0, 8));
            des.Key = inputKey;
            des.IV = inputKey;
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            return Convert.ToBase64String(ms.ToArray());
        }

        public String Decrypt(String text)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            Byte[] inputByteArray = Convert.FromBase64String(text);
            //Key和IV的Byte数组长度为8即可
            Byte[] inputKey = ASCIIEncoding.ASCII.GetBytes(_key.Substring(0, 8));
            des.Key = inputKey;
            des.IV = inputKey;
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            return Encoding.Default.GetString(ms.ToArray());
        }
    }
}
