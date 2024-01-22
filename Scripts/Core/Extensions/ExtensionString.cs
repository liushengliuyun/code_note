using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Core.Extensions
{
    public static class ExtensionString
    {
        private static Regex addSpacesRegex = new Regex("(?<=[a-z])(?=[A-Z])");

        public static string NameFormat(
            this string name,
            string prefix,
            string suffix,
            bool addSpaces)
        {
            if (name.IsNullOrEmpty())
                return "";
            int startIndex = string.IsNullOrEmpty(prefix) || !name.StartsWith(prefix) ? 0 : prefix.Length;
            int length = string.IsNullOrEmpty(suffix) || !name.EndsWith(suffix)
                ? name.Length
                : name.Length - suffix.Length;
            name = name.Substring(startIndex, length);
            if (addSpaces)
                name = name.NameFormat();
            return ExtensionString.FirstCharToUppercase(name);
        }

        public static string NameFormat(this string name) => name.IsNullOrEmpty()
            ? ""
            : ExtensionString.FirstCharToUppercase(ExtensionString.addSpacesRegex.Replace(name, " "));

        private static string FirstCharToUppercase(string name) =>
            name.IsNullOrEmpty() ? name : name[0].ToString().ToUpper() + name.Substring(1);

        public static bool IsNullOrEmpty(this string text) => string.IsNullOrEmpty(text);

        public static string Join(this IEnumerable<object> values, string separator = null)
        {
            StringBuilder stringBuilder = new StringBuilder();
            int num = 0;
            foreach (object obj in values)
            {
                if (separator != null && num++ > 0)
                    stringBuilder.Append(separator);
                stringBuilder.Append(obj);
            }

            return stringBuilder.ToString();
        }

        public static string FormatText(this string text, params object[] values) => string.Format(text, values);

        public static string FormatReplace(this string text, string key, object replacement)
        {
            if (text.IsNullOrEmpty())
                return "";
            if (key.IsNullOrEmpty())
                return text;
            if (replacement == null)
                replacement = (object)"";
            return text.Replace("{" + key + "}", replacement.ToString());
        }

        public static long CheckSum(this string text)
        {
            long num1 = 12345;
            foreach (char ch in text)
            {
                long num2 = (long)Convert.ToInt32(ch);
                if (num2 == 0L)
                    num2 = -1L;
                num1 = (1103515245L * num1 * num2 / 65536L + 12345L) % 32768L;
            }

            return num1;
        }
        
        public static string RemoveExtension(this string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            int index = str.LastIndexOf(".");
            if (index == -1)
                return str;
            else
                return str.Remove(index); //"assets/config/test.unity3d" --> "assets/config/test"
        }
    }
}