namespace Utils
{
    using System.Text;

    public class YZString
    {
        private static StringBuilder stringBuilder = new StringBuilder();
        private static StringBuilder shareStringBuilder = new StringBuilder();

        public static StringBuilder GetShareStringBuilder()
        {
            shareStringBuilder.Remove(0, shareStringBuilder.Length);
            return shareStringBuilder;
        }

        public static string Format(string src, params object[] args)
        {
            stringBuilder.Remove(0, stringBuilder.Length);
            stringBuilder.AppendFormat(src, args);
            return stringBuilder.ToString();
        }

        public static string Concat(string s1, string s2)
        {
            stringBuilder.Remove(0, stringBuilder.Length);
            stringBuilder.Append(s1);
            stringBuilder.Append(s2);
            return stringBuilder.ToString();
        }

        public static string Concat(string s1, string s2, string s3)
        {
            stringBuilder.Remove(0, stringBuilder.Length);
            stringBuilder.Append(s1);
            stringBuilder.Append(s2);
            stringBuilder.Append(s3);
            return stringBuilder.ToString();
        }

        public static string Concat(params object[] args)
        {
            stringBuilder.Remove(0, stringBuilder.Length);
            foreach (var itor in args)
            {
                stringBuilder.Append(itor);
            }

            return stringBuilder.ToString();
        }
    }
}