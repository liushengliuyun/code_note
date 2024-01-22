using System;
using Core.Third.I18N;

namespace Utils
{
    public class YZTimeUtil
    {
        public static long GetYZTimestamp()
        {
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            long ret = Convert.ToInt64(ts.TotalSeconds);
            return ret;
        }

        public static int GetYZTimestampInt()
        {
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            int ret = Convert.ToInt32(ts.TotalSeconds);
            return ret;
        }

        public static long GetYZTimestampUTC()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            long ret = Convert.ToInt64(ts.TotalSeconds);
            return ret;
        }

        public static string GetLocalTime(string time, string format = "yyyy-MM-dd HH:mm:ss")
        {
            if (time == null)
            {
                return "";
            }

            DateTime start = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            start = start.AddSeconds(long.Parse(time));
            if (format != null)
            {
                return start.ToString(format);
            }
            else
            {
                return start.ToShortDateString();
            }
        }

        public static long GetYZDayStartStamp(long time)
        {
            DateTime original = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            DateTime date = original.AddSeconds(time);
            DateTime need = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
            return Convert.ToInt64((need - original).TotalSeconds);
        }

        public static int ParseYZTimeString(string time)
        {
            DateTime original = DateTime.Parse(time);
            TimeSpan ts = original - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return (int)ts.TotalSeconds;
        }

        public static DateTime StampYZToDateTime(int stamp)
        {
            DateTime original = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return original.AddSeconds(stamp);
        }

        // 刚刚、几分钟前，几小时前，几天前
        public static string FormatYZTime(int time)
        {
            TimeSpan t = TimeSpan.FromSeconds(time);
            if (time < 60)
            {
                return I18N.Get("key_time_now");
            }

            if (time < 60 * 60)
            {
                
                return YZString.Format(I18N.Get("key_minute_ago"), t.Minutes);
            }

            if (time < 60 * 60 * 24)
            {
                return YZString.Format(I18N.Get("key_hour_ago"), t.Hours);
            }

            return YZString.Format(I18N.Get("key_days_ago"), t.Days);
        }

        // 大于1天，使用format  如: {0}d {1:D2}h {2:D2}m
        // 小于1天，忽略format  如: 12:00:00
        public static string FormatYZTimeEvent(string format, int time)
        {
            TimeSpan t = TimeSpan.FromSeconds(time);
            if (t.Days >= 1)
            {
                return YZString.Format(format, t.Days, t.Hours, t.Minutes);
            }

            return YZString.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds);
        }

        // 记录排行榜时间格式化
        public static string TimeYZFormatRecoredRank(int stamp)
        {
            DateTime date = StampYZToDateTime(stamp);
            return YZString.Format("{0:D2}/{1:D2}/{2} {3:D2}:{4:D2}", date.Day, date.Month, date.Year, date.Hour,
                date.Minute);
        }

        // 返回如 Start in 00:00:00
        public static string FormatYZTimeHour(string format, int time)
        {
            TimeSpan t = TimeSpan.FromSeconds(time);
            string d;
            if (t.TotalHours >= 48)
            {
                d = YZString.Format("{0}d {1:D2}h {2:D2}m", t.Days, t.Hours, t.Minutes);
            }
            else
            {
                d = YZString.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours + (t.Days * 24), t.Minutes, t.Seconds);
            }

            if (string.IsNullOrEmpty(format))
            {
                return d;
            }

            return YZString.Format(format, d);
        }

        // 返回 00:00
        public static string FormatYZBattleTime(int time)
        {
            TimeSpan t = TimeSpan.FromSeconds(Math.Max(time, 0));
            return YZString.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
        }
    }
}