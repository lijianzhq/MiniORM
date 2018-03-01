using System;
using System.Web.Security;

namespace MiniORM
{
    public static class EncryptHelper
    {
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static String EncryptMD5(String text)
        {
            //if (String.IsNullOrEmpty(text)) return "";
            //String outputStr = String.Empty;
            //Byte[] dataToHash = (new System.Text.ASCIIEncoding()).GetBytes(text);
            //Byte[] hashvalue = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(dataToHash);
            //return Encoding.UTF8.GetString(hashvalue);
            //Byte[] data = System.Text.Encoding.Unicode.GetBytes(text.ToCharArray());
            ////创建一个Md5对象
            //System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            ////加密Byte[]数组
            //Byte[] result = md5.ComputeHash(data);
            ////将加密后的数组转化为字段
            return FormsAuthentication.HashPasswordForStoringInConfigFile(text, "MD5");
        }
    }
}
