using System;
using System.Security.Cryptography;
using System.Text;

namespace Whiskey.Utility.Secutiry
{
    public class AesHelper
    {
        /// <summary>  
        /// AES加密,key长度最多16位
        /// </summary>  
        /// <param name="toEncrypt">明文</param>  
        /// <param name="key">密钥</param>  
        /// <returns></returns>
        public static string Encrypt(string toEncrypt, string key)
        {
            try
            {
                if (key.Length < 16)
                    key = key.PadLeft(16, '0');
                byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key);
                byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

                RijndaelManaged provider = new RijndaelManaged();
                provider.Key = keyArray;
                provider.IV = keyArray;
                provider.Mode = CipherMode.CBC;
                provider.Padding = PaddingMode.Zeros;

                ICryptoTransform cTransform = provider.CreateEncryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>  
        /// AES解密,key长度最多16位
        /// </summary>  
        /// <param name="toDecrypt">密文</param>  
        /// <param name="key">密钥</param>  
        /// <returns></returns>
        public static string Decrypt(string toDecrypt, string key)
        {
            try
            {
                if (key.Length < 16)
                    key = key.PadLeft(16, '0');
                byte[] keyArray = UTF8Encoding.UTF8.GetBytes(key);
                byte[] toEncryptArray = Convert.FromBase64String(toDecrypt);

                RijndaelManaged provider = new RijndaelManaged();
                provider.Key = keyArray;
                provider.IV = keyArray;
                provider.Mode = CipherMode.CBC;
                provider.Padding = PaddingMode.Zeros;

                ICryptoTransform cTransform = provider.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                return UTF8Encoding.UTF8.GetString(resultArray);
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
