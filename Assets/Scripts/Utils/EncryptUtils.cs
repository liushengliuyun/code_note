using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Utils
{
    public static class EncryptUtils
    {
        private const string PrivateKey = "xxxxxMxxxxxxxxxxxSxxxxLxxxTxxxxx";


        /// <summary>
        /// Rijndael加密算法
        /// </summary>
        /// <param name="pString">待加密的明文</param>
        /// <param name="pKey">密钥,长度可以为:64位(byte[8]),128位(byte[16]),192位(byte[24]),256位(byte[32])</param>
        /// <param name="iv">iv向量,长度为128（byte[16])</param>
        /// <returns></returns>
        public static string Encrypt(string pString, string pKey = PrivateKey)
        {
            try
            {
                //密钥
                var keyArray = Encoding.UTF8.GetBytes(pKey);
                //待加密明文数组
                var toEncryptArray = Encoding.UTF8.GetBytes(pString);

                var rDel = new RijndaelManaged { Key = keyArray, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 };
                var cTransform = rDel.CreateEncryptor();

                //返回加密后的密文
                var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
                return string.Empty;
            }
        }

        /// <summary>
        /// ijndael解密算法
        /// </summary>
        /// <param name="pString">待解密的密文</param>
        /// <param name="pKey">密钥,长度可以为:64位(byte[8]),128位(byte[16]),192位(byte[24]),256位(byte[32])</param>
        /// <param name="iv">iv向量,长度为128（byte[16])</param>
        /// <returns></returns>
        public static string Decrypt(string pString, string pKey = PrivateKey)
        {
            try
            {
                //解密密钥
                var keyArray = Encoding.UTF8.GetBytes(pKey);
                //待解密密文数组
                var toEncryptArray = Convert.FromBase64String(pString);

                //Rijndael解密算法
                var rDel = new RijndaelManaged { Key = keyArray, Mode = CipherMode.ECB, Padding = PaddingMode.PKCS7 };
                var cTransform = rDel.CreateDecryptor();

                //返回解密后的明文
                var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                return Encoding.UTF8.GetString(resultArray);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString() + $"content is {pString}");
                return "";
            }
        }
    }
}