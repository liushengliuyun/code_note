using System.Text;

namespace Utils
{
    public static class StringBuilderUtils
    {
        private static StringBuilder _stringBuilder = new StringBuilder();
        private static StringBuilder _shareStringBuilder = new StringBuilder();

        public static StringBuilder GetBuilder()
        {
            _shareStringBuilder.Remove(0, _shareStringBuilder.Length);
            return _shareStringBuilder;
        }

        public static string BuilderFormat(string src, params object[] args)
        {
            _stringBuilder.Remove(0, _stringBuilder.Length);
            _stringBuilder.AppendFormat(src, args);
            return _stringBuilder.ToString();
        }

        public static string Join(string s1, string s2)
        {
            _stringBuilder.Remove(0, _stringBuilder.Length);
            _stringBuilder.Append(s1);
            _stringBuilder.Append(s2);
            return _stringBuilder.ToString();
        }

        public static string Join(string s1, string s2, string s3)
        {
            _stringBuilder.Remove(0, _stringBuilder.Length);
            _stringBuilder.Append(s1);
            _stringBuilder.Append(s2);
            _stringBuilder.Append(s3);
            return _stringBuilder.ToString();
        }

        public static string Join(params object[] args)
        {
            _stringBuilder.Remove(0, _stringBuilder.Length);
            foreach (var itor in args)
            {
                _stringBuilder.Append(itor);
            }

            return _stringBuilder.ToString();
        }
    }
}