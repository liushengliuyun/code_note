using System.Text;

namespace Utils
{
    public class YZNumberUtil
    {
        // 美金格式化 concise 是否自动去掉小数后面的0
        public static string FormatYZMoney(string money, bool concise = true)
        {
            if (!concise)
            {
                if (decimal.TryParse(money, out decimal d))
                {
                    return string.Format("{0:N2}", d);
                }

                return "0";
            }
            else
            {
                if (decimal.TryParse(money, out decimal d))
                {
                    return d.ToString("0.##");
                }

                return "0";
            }
        }

        /// 筹码格式化 coin 是否逗号分割
        public static string FormatYZChips(int chips, bool coin = false)
        {
            if (coin && chips > 0)
            {
                return chips.ToString("###,###");
            }
            else
            {
                return chips.ToString();
            }
        }

        public static string SimplifyNumber(double num)
        {
            int B = 1000000000;
            int M = 1000000;
            int K = 1000;
            if (num >= B * 10)
            {
                return (num / B).ToString("f1") + "B";
            }
            else if (num >= M * 10)
            {
                return (num / M).ToString("f1") + "M";
            }
            else if (num >= K * 10)
            {
                return (num / K).ToString("f1") + "K";
            }

            return num.ToString();
        }
    }

    public static class YZNnumberExt
    {
        public static float ToFloat(this string str, float dft = 0)
        {
            if (float.TryParse(str, out float i))
                return i;
            return dft;
        }

        public static double ToDouble(this string src, double dft = 0)
        {
            if (double.TryParse(src, out double result))
                return result;
            return dft;
        }

        public static int ToInt(this string src, int dft = 0)
        {
            if (int.TryParse(src, out int result))
                return result;
            return dft;
        }
    }
}