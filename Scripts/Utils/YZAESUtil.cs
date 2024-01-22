using System;
using System.Text;
using System.Security.Cryptography;

namespace Utils
{
    public class YZAESUtil
    {
        private readonly string YZKey;
        private readonly string YZIV;
        private readonly CipherMode YZModel;
        private readonly PaddingMode YZPadding;

        //构造方法
        public YZAESUtil(string Key, string IV, CipherMode Model = CipherMode.CBC,
            PaddingMode Padding = PaddingMode.PKCS7)
        {
            this.YZKey = Key;
            this.YZIV = IV;
            this.YZModel = Model;
            this.YZPadding = Padding;
        }

        //获取加密/解密器的实例
        private ICryptoTransform GetYZCrypto(bool isEncrypt)
        {
            using (RijndaelManaged YZRm = new RijndaelManaged())
            {
                YZRm.Key = Encoding.UTF8.GetBytes(this.YZKey);
                YZRm.IV = Encoding.UTF8.GetBytes(this.YZIV);
                YZRm.Mode = this.YZModel;
                YZRm.Padding = this.YZPadding;
                if (isEncrypt)
                {
                    return YZRm.CreateEncryptor();
                }
                else
                {
                    return YZRm.CreateDecryptor();
                }
            }
        }

        /// AES加密
        public string EncryptAESYZ(string content)
        {
            using (ICryptoTransform ict = GetYZCrypto(true))
            {
                byte[] contentBytes = Encoding.UTF8.GetBytes(content);
                byte[] resultBytes = ict.TransformFinalBlock(contentBytes, 0, contentBytes.Length);
                return Convert.ToBase64String(resultBytes, 0, resultBytes.Length);
            }
        }

        /// AES解密
        public string DecipherAESYZ(string base64Str)
        {
            using (ICryptoTransform ict = GetYZCrypto(false))
            {
                byte[] contentBytes = Convert.FromBase64String(base64Str);
                byte[] resultBytes = ict.TransformFinalBlock(contentBytes, 0, contentBytes.Length);
                return Encoding.UTF8.GetString(resultBytes);
            }
        }
    }
}