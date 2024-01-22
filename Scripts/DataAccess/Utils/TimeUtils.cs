using System;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using Newtonsoft.Json.Linq;
using UniRx;
using UnityEngine;

namespace DataAccess.Utils
{
    public class TimeUtils : global::Utils.Runtime.Singleton<TimeUtils>
    {
        private int offSet;

        private bool setServerTime;

        private int utcOffSet;

        public bool SetServerTime => setServerTime;

        /// <summary>
        /// UTC时间
        /// </summary>
        public int UtcTimeNow
        {
            get
            {
                if (offSet == 0)
                {
                    return GetUTCTimeStamp();
                }

                return GetClockTime() + offSet;
            }
        }

        private int endDayTimeStamp;


        private IDisposable end_day_timer;

        /// <summary>
        /// 跨天登陆的时间戳
        /// </summary>
        public int EndDayTimeStamp
        {
            set
            {
                end_day_timer?.Dispose();

                end_day_timer = Observable.Timer(TimeSpan.FromSeconds(
                        value - LoginTimeStamp
                    ))
                    .Subscribe(_ => { MediatorRequest.Instance.PassDayLogin(); });
                endDayTimeStamp = value;
            }

            get => endDayTimeStamp;
        }

        /// <summary>
        /// 玩家登陆成功的时间
        /// </summary>
        public int LoginTimeStamp;

        /// <summary>
        /// 西8区时间
        /// </summary>
        public int LocalTimeNow => ConvertToLocalTime(UtcTimeNow);

        public int ConvertToLocalTime(int time)
        {
            return time - 8 * 3600;
        }

        public DateTime LocalNowDate
        {
            get => DateTimeOffset.FromUnixTimeSeconds(LocalTimeNow).DateTime;
        }

        public int LocalYearMonthDay
        {
            get
            {
                var date = LocalNowDate;
                return DataTimeToInt(date);
            }
        }

        public int UTCYearMonthDay
        {
            get
            {
                var date = DateTimeOffset.FromUnixTimeSeconds(UtcTimeNow).DateTime;
                return DataTimeToInt(date);
            }
        }
        
        private static int DataTimeToInt(DateTime date)
        {
            var str = $"{date.Year:0000}{date.Month:00}{date.Day:00}";
            int.TryParse(str, out var result);
            return result;
        }

        /// <summary>
        /// LocalTimeNow 除以 86400的商
        /// </summary>
        public int Today => LocalTimeNow / 86400;

        /// <summary>
        /// 距离注册时间过了几天
        /// </summary>
        public int PassDay => PassTime / 86400;

        public int PassTime => UtcTimeNow - Root.Instance.RegisterTime;

        private int lastSetClockTime;

        private void SetOffSet(int serverTime)
        {
            var newClockTime = GetClockTime();
            if (newClockTime - lastSetClockTime >= 5)
            {
                setServerTime = true;
                lastSetClockTime = newClockTime;
                offSet = serverTime - newClockTime;
            }
        }

        public void SetTimeOffset(JObject jObject)
        {
            var time = jObject.SelectToken("time")?.Value<int>();
            if (time != null)
            {
                SetOffSet((int)time);
            }
        }

        private int GetClockTime()
        {
            return (int)Time.realtimeSinceStartup;
        }

        /// <summary>
        /// 当天的凌晨时间
        /// </summary>
        public int BeforeDawn => LocalTimeNow - LocalTimeNow % 86400;

        public int GetLocalTimeStamp()
        {
            var ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return (int)ts.TotalSeconds;
        }

        public int GetUTCTimeStamp()
        {
            var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return (int)ts.TotalSeconds;
        }

        /// <summary>
        ///  如果时间超过了一天, 是有问题的
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public string ToHourMinuteSecond(int timeSpan)
        {
            TimeSpan time = TimeSpan.FromSeconds(timeSpan);
            //:00好像是指定两位数字的意思
            if (time.Hours > 0 || time.Days > 0)
            {
                return $"{time.Days * 24 + time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}";
            }

            return ToMinuteSecond(timeSpan);
        }

        /// <summary>
        /// 距离多久之前
        /// </summary>
        /// <param name="timePoint">时间点</param>
        /// <returns></returns>
        public string PassTimeFormat(int timePoint)
        {
            TimeSpan time = TimeSpan.FromSeconds(UtcTimeNow - timePoint);

            if (time.Days >= 1)
            {
                return I18N.Get("TIME_DAY", time.Days);
            }

            if (time.Hours >= 1)
            {
                return I18N.Get("TIME_HOUR", time.Hours);
            }

            if (time.Minutes >= 1)
            {
                return I18N.Get("TIME_MINUTE", time.Minutes);
            }

            return I18N.Get("key_just_now");
        }

        public string ToMinuteSecond(int timeSpan)
        {
            TimeSpan time = TimeSpan.FromSeconds(timeSpan);
            // return time.ToString("mm':'ss");
            return $"{((int)time.TotalDays * 24 + time.Hours) * 60 + time.Minutes:00}:{time.Seconds:00}";
        }

        public int TimeToTomorrow()
        {
            if (EndDayTimeStamp < UtcTimeNow)
            {
                MediatorRequest.Instance.PassDayLogin();
            }

            return Math.Max(0, EndDayTimeStamp - UtcTimeNow);
        }

        public string FormatTimeToTomorrow()
        {
            return ToHourMinuteSecond(TimeToTomorrow());
        }

        /// <summary>
        /// 距离玩家登陆时间, 是否超过了day 天
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        public bool IsDayPassRegisterTime(int day)
        {
            return PassDay >= day;
        }

        public TimeSpan ToNextSundayMidnight()
        {
            // 获取当前时间
            var dto = DateTimeOffset.FromUnixTimeSeconds(UtcTimeNow - 8 * 3600);
            DateTime now = dto.DateTime;
            // 获取当前星期几（0表示周日，1表示周一，依次类推）
            int currentDayOfWeek = (int)now.DayOfWeek;

            // 计算距离下一个周天的天数差
            int daysUntilNextSunday = currentDayOfWeek == 0 ? 1 : 8 - currentDayOfWeek;

            // 获取下一个周天凌晨的时间
            DateTime nextSundayMidnight = now.Date.AddDays(daysUntilNextSunday);

            // 计算距离下一个周天凌晨的时间差
            TimeSpan timeUntilNextSundayMidnight = nextSundayMidnight - now;

            return timeUntilNextSundayMidnight;
        }

        /// <summary>
        ///  转换成日、小时、分钟、秒
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public string ToDayHourMinuteSecond(int timeSpan)
        {
            TimeSpan time = TimeSpan.FromSeconds(timeSpan);
            //:00好像是指定两位数字的意思
            if (time.Days > 0)
            {
                return $"{time.Days}:{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}";
            }

            if (time.Hours > 0)
            {
                return ToHourMinuteSecond(timeSpan);
            }

            return ToMinuteSecond(timeSpan);
        }
    }
}