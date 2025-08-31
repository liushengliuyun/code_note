// using System;
// using System.Globalization;
// using DG.Tweening;
// using JerryMouse.Extensions;
// using JerryMouse.Model;
//
// #if use_unirx
// using UniRx;
// #endif
//
// using UnityEngine;
//
//
// namespace JerryMouse.Utils
// {
//     public class TimeUtils : global::SlotX.Utils.Singleton.XSingleton<TimeUtils>
//     {
//         private int _offSet;
//
//         private bool _setServerTime;
//
//         private int _utcOffSet;
//
//         // public bool SetServerTime => setServerTime;
//
//         /// <summary>
//         /// UTC时间
//         /// </summary>  
//         public int UtcTimeNow
//         {
//             get
//             {
//                 if (_offSet == 0)
//                 {
//                     return GetUTCTimeStamp();
//                 }
//
//                 return GetClockTime() + _offSet;
//             }
//         }
//         
//         
//         private IDisposable end_day_timer;
//
//         /// <summary>
//         /// 跨天登陆的时间戳
//         /// </summary>
//         public int EndDayTimeStamp { set; get; }
//
//         private int _loainTimeStampWisdom;
//         
//         /// <summary>
//         /// 玩家登陆成功的时间
//         /// </summary>
//         public int Lover_LoginTimeStamp_wisdom
//         {
//             get
//             {
//                 return _loainTimeStampWisdom;
//             }
//             set
//             {
//                 _loainTimeStampWisdom = value;
//                 // var nextDayStamp = value + 86400;
//                 // EndDayTimeStamp = ConvertToLocalTime(nextDayStamp - nextDayStamp % 86400) ;
//             }
//         }
//
//         /// <summary>
//         /// 西8区时间
//         /// </summary>
//         public int LocalTimeNow => ConvertToLocalTime_Twain(UtcTimeNow);
//
//         public int ConvertToLocalTime_Twain(int time)
//         {
//             return time - 8 * 3600;
//         }
//
//         // public DateTime LocalNowDate
//         // {
//         //     get => DateTimeOffset.FromUnixTimeSeconds(LocalTimeNow).DateTime;
//         // }
//
//         // public int LocalYearMonthDay
//         // {
//         //     get
//         //     {
//         //         var date = LocalNowDate;
//         //         return DataTimeToInt(date);
//         //     }
//         // }
//
//         public bool ShouldPassDay => EndDayTimeStamp < UtcTimeNow;
//         
//         private int UTCYearMonthDay
//         {
//             get
//             {
//                 DateTime date = DateTimeOffset.FromUnixTimeSeconds(UtcTimeNow).DateTime;
//                 return DataTimeToInt(date);
//             }
//         }
//         
//         private static int DataTimeToInt(DateTime date)
//         {
//             var str = $"{date.Year:0000}{date.Month:00}{date.Day:00}";
//             int.TryParse(str, out var result);
//             return result;
//         }
//
//         public string GetTimeDisplay(int utcTime)
//         {
//             var localUtc = ConvertToLocalTime_Twain(utcTime);
//             
//             DateTime date = DateTimeOffset.FromUnixTimeSeconds(localUtc).DateTime;
//
//             return $"{ConvertTo12HourFormat(date.Hour)} on {GetMonthLang(date.Month)} {date.Day}";
//         }
//
//         private string GetMonthLang(int month)
//         {
//             if (month >= 12)
//             {
//                 return null;
//             }
//             
//             string[] months = {
//                 "January", "February", "March", "April", "May", "June",
//                 "July", "August", "September", "October", "November", "December"
//             };
//             return months[month - 1];
//         }
//         
//         private string ConvertTo12HourFormat(int hour)
//         {
//             string period = hour >= 12 ? "PM" : "AM";
//
//             // 处理 0 点和 12 点的特殊情况
//             if (hour == 0)
//             {
//                 hour = 12;
//             }
//             else if (hour > 12)
//             {
//                 hour -= 12;
//             }
//
//             return $"{hour}:00 {period}";
//         }
//         
//         
//         /// <summary>
//         /// LocalTimeNow 除以 86400的商
//         /// </summary>
//         public int Today => LocalTimeNow / 86400;
//
//         /// <summary>
//         /// 距离注册时间过了几天 从0开始的
//         /// </summary>
//         public int PassDay => PassTime / 86400;
//
//         /// <summary>
//         /// 距离玩家注册后过了多久
//         /// </summary>
//         public int PassTime => UtcTimeNow - (GetDawn(MyApp.Instance.PlayerInfo.register_time) - 16 * 3600);
//
//         private int _lastSetClockTime;
//
//         private void SetOffSet(int serverTime, bool force)
//         {
//             var newClockTime = GetClockTime();
//             if (force || newClockTime - _lastSetClockTime >= 5)
//             {
//                 _setServerTime = true;
//                 _lastSetClockTime = newClockTime;
//                 _offSet = serverTime - newClockTime;
//                 XLog.LogColor($"offSet 修正 = {_offSet}" );
//             }
//         }
//
//         public static long GetUTCByData(string dateString)
//         {
//             // 将日期字符串转换为 DateTime 对象
//             DateTime dateTime = DateTime.ParseExact(dateString, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
//
//             // 将 DateTime 对象转换为 UTC 时间戳
//             long unixTimestamp = ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
//
//             return unixTimestamp + 8 * 3600;
//         }
//         
//         public void SetTimeOffset(int serverTime, bool force = false)
//         {
//             SetOffSet(serverTime, force);
//         }
//
//         private int GetClockTime()
//         {
//             return (int)Math.Round(Time.realtimeSinceStartup);
//         }
//         
//         public long GetLongUTCTimeStamp()
//         {
//             TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
//             long ret = Convert.ToInt64(ts.TotalSeconds);
//             return ret;
//         }
//         
//         /// <summary>
//         /// 当天的凌晨时间
//         /// </summary>
//         public int BeforeDawn => GetDawn(UtcTimeNow);
//
//         /// <summary>
//         /// 因为这里倒退了一天 , 相当于减一天再加8小时 
//         /// </summary>
//         public int BeforeDawnLocal => GetDawnLocal(UtcTimeNow);
//         
//         /// <summary>
//         /// 只能输入标准utc时间
//         /// </summary>
//         /// <param name="utcTime"></param>
//         /// <returns></returns>
//         public int GetDawn(int utcTime)
//         {
//            return utcTime - utcTime % 86400;
//         }
//
//         public int GetDawnLocal(int utcTime)
//         {
//             return GetDawn(utcTime) - 16 * 3600;
//         }
//         
//         /// <summary>
//         /// 距离上一次登陆过去的时间
//         /// </summary>
//         public int DeltaTime { get; set; }
//         
//         // public int GetLocalTimeStamp()
//         // {
//         //     var ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
//         //     return (int)ts.TotalSeconds;
//         // }
//
//         public int GetUTCTimeStamp()
//         {
//             var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
//             return (int)ts.TotalSeconds;
//         }
//
//         /// <summary>
//         ///  如果时间超过了一天, 是有问题的
//         /// </summary>
//         /// <param name="timeSpan"></param>
//         /// <returns></returns>
//         public string ToHourMinuteSecond(int timeSpan)
//         {
//             TimeSpan time = TimeSpan.FromSeconds(timeSpan);
//             //:00好像是指定两位数字的意思
//             if (time.Hours > 0 || time.Days > 0)
//             {
//                 return $"{time.Days * 24 + time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}";
//             }
//         
//             return ToMinuteSecond(timeSpan);
//         }
//
//         // /// <summary>
//         // /// 距离多久之前
//         // /// </summary>
//         // /// <param name="timePoint">时间点</param>
//         // /// <returns></returns>
//         // public string PassTimeFormat(int timePoint)
//         // {
//         //     TimeSpan time = TimeSpan.FromSeconds(UtcTimeNow - timePoint);
//         //
//         //     if (time.Days >= 1)
//         //     {
//         //         return MyLang.TimeDay.CsFormat(time.Days);
//         //     }
//         //
//         //     if (time.Hours >= 1)
//         //     {
//         //         return MyLang.TimeHour.CsFormat(time.Hours);
//         //     }
//         //
//         //     if (time.Minutes >= 1)
//         //     {
//         //         return MyLang.TimeMinute.CsFormat(time.Minutes);
//         //     }
//         //
//         //     return MyLang.KeyJustNow;
//         // }
//
//         public string ToMinuteSecond(int timeSpan)
//         {
//             if (timeSpan < 0)
//             {
//                 timeSpan = 0;
//             }
//             TimeSpan time = TimeSpan.FromSeconds(timeSpan);
//             // return time.ToString("mm':'ss");
//             return $"{((int)time.TotalDays * 24 + time.Hours) * 60 + time.Minutes:00}:{time.Seconds:00}";
//         }
//
//         public int TimeToTomorrow()
//         {
//             return Math.Max(0, EndDayTimeStamp - UtcTimeNow);
//         }
//
//         public string FormatTimeToTomorrow()
//         {
//             return ToHourMinuteSecond(TimeToTomorrow());
//         }
//
//         // /// <summary>
//         // /// 距离玩家登陆时间, 是否超过了day 天
//         // /// </summary>
//         // /// <param name="day"></param>
//         // /// <returns></returns>
//         // public bool IsDayPassRegisterTime(int day)
//         // {
//         //     return PassDay >= day;
//         // }
//
//         // public TimeSpan ToNextSundayMidnight()
//         // {
//         //     // 获取当前时间
//         //     var dto = DateTimeOffset.FromUnixTimeSeconds(UtcTimeNow - 8 * 3600);
//         //     DateTime now = dto.DateTime;
//         //     // 获取当前星期几（0表示周日，1表示周一，依次类推）
//         //     int currentDayOfWeek = (int)now.DayOfWeek;
//         //
//         //     // 计算距离下一个周天的天数差
//         //     int daysUntilNextSunday = currentDayOfWeek == 0 ? 1 : 8 - currentDayOfWeek;
//         //
//         //     // 获取下一个周天凌晨的时间
//         //     DateTime nextSundayMidnight = now.Date.AddDays(daysUntilNextSunday);
//         //
//         //     // 计算距离下一个周天凌晨的时间差
//         //     TimeSpan timeUntilNextSundayMidnight = nextSundayMidnight - now;
//         //
//         //     return timeUntilNextSundayMidnight;
//         // }
//         //
//         /// <summary>
//         ///  转换成日、小时、分钟、秒
//         /// </summary>
//         /// <param name="timeSpan"></param>
//         /// <returns></returns>
//         public string ToDayHourMinuteSecond(int timeSpan)
//         {
//             TimeSpan time = TimeSpan.FromSeconds(timeSpan);
//             //:00好像是指定两位数字的意思
//             if (time.Days > 0)
//             {
//                 return $"{time.Days}:{time.Hours:00}:{time.Minutes:00}:{time.Seconds:00}";
//             }
//         
//             if (time.Hours > 0)
//             {
//                 return ToHourMinuteSecond(timeSpan);
//             }
//         
//             return ToMinuteSecond(timeSpan);
//         }
//     }
//
//     public static class Timer
//     {
//         public static void Register(float time, Action callBack)
//         {
//             DOTween.Sequence().AppendInterval(time).AppendCallback(() =>
//             {
//                 callBack?.Invoke();
//             });
//         }
//     }
// }