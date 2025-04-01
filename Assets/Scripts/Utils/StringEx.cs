using System;
using System.Text.RegularExpressions;

namespace Utils
{
    public static class StringExt
    {
        public static string Decrypt(this string value)
        {
            return EncryptUtils.Decrypt(value);
        }

        public static string Encrypt(this string value)
        {
            return EncryptUtils.Encrypt(value);
        }
        
        public static string AddTransferSymbol(this string input)
        {
            return input.Replace("\"", "\\\"");
        }
        
        /// <summary>
        /// 小数点的位置
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static int NumOfDecimal(this string s)
        {
            int decimalIndex = s.IndexOf(".", StringComparison.Ordinal);

            if (decimalIndex >= 0)
            {
                return s.Length - decimalIndex - 1;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// 字符串是否是纯数字
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsNumeric(this string s)
        {
            Regex regex = new Regex("^[0-9]+$");
            return regex.IsMatch(s);
        }
        
        /// <summary>
        /// 是否是纯数字或字母
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsNumericOrLetter(this string s)
        {
            Regex regex = new Regex("^[a-zA-Z0-9]+$");
            return regex.IsMatch(s);
        }
        
        public static bool IsNullOrEmpty(this string text) => string.IsNullOrEmpty(text);
        
        /// <summary>
        /// 是否是邮箱地址
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static bool IsEmailAddress(this string s)
        {
            if (s.IsNullOrEmpty())
            {
                return false;
            }
            //@后面不能出现连续的.. ， 但是unity已经在输入时做了限制
            // bool isValid = Regex.IsMatch(s, @"^[a-zA-Z0-9]+([._-][a-zA-Z0-9]+)*@[\w\.-]+(?:\w[.])*\w+\.\w+$");
        
            bool isValid = Regex.IsMatch(s, @"^[a-zA-Z0-9]+([._-][a-zA-Z0-9]+)*@[\w\.-]+\.\w+$");
            return isValid;
        }
        
        public static string CsFormat(this string str, params object[] args)
        {
            return String.Format(str, args);
        }
    }
}