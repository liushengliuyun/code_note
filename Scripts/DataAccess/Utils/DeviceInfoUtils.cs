using System;
using System.Collections.Generic;
using System.Globalization;
using Core.Extensions;
using Core.Services.NetService.API.Facade;
using DataAccess.Model;
using DataAccess.Utils.Static;
using LitJson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Utils;
using Application = UnityEngine.Device.Application;
using SystemInfo = UnityEngine.Device.SystemInfo;

namespace DataAccess.Utils
{
    public class DeviceInfoUtils : global::Utils.Runtime.Singleton<DeviceInfoUtils>
    {
        public string TestDeviceId;
        
        public CountryInfo CountryInfo;
        public GPSInfo GPSInfo;
        public GPSExtra SelfGPSExtra;

        /// <summary>
        /// 获取设备标识 
        /// </summary>
        /// <returns></returns>
        public string GetEquipmentId()
        {
            var deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier;
            if (deviceUniqueIdentifier == SystemInfo.unsupportedIdentifier)
            {
                //不支持获取设备唯一ID todo   
            }

            // return "908d86d41d9ff7beb40d3689fb22d518";
            return TestDeviceId.IsNullOrEmpty() ? deviceUniqueIdentifier : TestDeviceId;
        }

        public void Init()
        {
            if (CountryInfo == null)
            {
                CountryInfo = new CountryInfo();
                GPSInfo = new GPSInfo();
                SelfGPSExtra = new GPSExtra();
            }
        }

        public string GetDeviceInfo()
        {
            return YZNativeUtil.GetYZDeviceInfo();
        }

        public string GetLanguage()
        {
            string result;

            switch (Application.systemLanguage)
            {
                case SystemLanguage.Afrikaans:
                    result = "af";
                    break;
                case SystemLanguage.Arabic:
                    result = "ar";
                    break;
                case SystemLanguage.Basque:
                    result = "eu";
                    break;
                case SystemLanguage.Belarusian:
                    result = "be";
                    break;
                case SystemLanguage.Bulgarian:
                    result = "bg";
                    break;
                case SystemLanguage.Catalan:
                    result = "ca";
                    break;
                case SystemLanguage.Chinese:
                    result = "zh";
                    break;
                case SystemLanguage.Czech:
                    result = "cs";
                    break;
                case SystemLanguage.Danish:
                    result = "da";
                    break;
                case SystemLanguage.Dutch:
                    result = "nl";
                    break;
                case SystemLanguage.English:
                    result = GlobalEnum.ENGLISH;
                    break;
                case SystemLanguage.Estonian:
                    result = "et";
                    break;
                case SystemLanguage.Faroese:
                    result = "fo";
                    break;
                case SystemLanguage.Finnish:
                    result = "fu";
                    break;
                case SystemLanguage.French:
                    result = "fr";
                    break;
                case SystemLanguage.German:
                    result = "de";
                    break;
                case SystemLanguage.Greek:
                    result = "el";
                    break;
                case SystemLanguage.Hebrew:
                    result = "en-IL";
                    break;
                case SystemLanguage.Icelandic:
                    result = "is";
                    break;
                case SystemLanguage.Indonesian:
                    result = "id";
                    break;
                case SystemLanguage.Italian:
                    result = "it";
                    break;
                case SystemLanguage.Japanese:
                    result = "ja";
                    break;
                case SystemLanguage.Korean:
                    result = "ko";
                    break;
                case SystemLanguage.Latvian:
                    result = "lv";
                    break;
                case SystemLanguage.Lithuanian:
                    result = "lt";
                    break;
                case SystemLanguage.Norwegian:
                    result = "nn";
                    break; // TODO: Check
                case SystemLanguage.Polish:
                    result = "pl";
                    break;
                case SystemLanguage.Portuguese:
                    result = "pt";
                    break;
                case SystemLanguage.Romanian:
                    result = "ro";
                    break;
                case SystemLanguage.Russian:
                    result = "ru";
                    break;
                case SystemLanguage.SerboCroatian:
                    result = "sr";
                    break; // TODO: Check
                case SystemLanguage.Slovak:
                    result = "sk";
                    break;
                case SystemLanguage.Slovenian:
                    result = "sl";
                    break;
                case SystemLanguage.Spanish:
                    result = "es";
                    break;
                case SystemLanguage.Swedish:
                    result = "sv";
                    break;
                case SystemLanguage.Thai:
                    result = "th";
                    break;
                case SystemLanguage.Turkish:
                    result = "tr";
                    break;
                case SystemLanguage.Ukrainian:
                    result = "uk";
                    break;
                case SystemLanguage.Vietnamese:
                    result = "vi";
                    break;
                case SystemLanguage.ChineseSimplified:
                    result = "CN";
                    break;
                case SystemLanguage.ChineseTraditional:
                    result = "HANS";
                    break;
                case SystemLanguage.Hungarian:
                    result = "hu";
                    break;
                case SystemLanguage.Unknown:
                default:
                    result = "un_know";
                    break;
            }

            return result;
        }

        public string GetCountry()
        {
            YZLog.LogColor("当前国家地区 " + RegionInfo.CurrentRegion.Name);
            return RegionInfo.CurrentRegion.Name;
        }

        public string GetLangAndCountry()
        {
            return $"{GetLanguage()}_{GetCountry()}";
        }
        
        public string GetGPSJson()
        {
            var gps_info = new JsonData
            {
                ["country"] = YZDataUtil.GetLocaling(YZConstUtil.YZCountryCode,""),
                ["province"] = YZDataUtil.GetLocaling(YZConstUtil.YZAreaCode,""),
                ["city"] = YZDataUtil.GetLocaling(YZConstUtil.YZCity,""),
            };
            var gpsData = SelfGPSExtra ?? new GPSExtra();
            var gps_extra = new JsonData
            {
                ["gps_camouflage"] = gpsData.gps_camouflage,
                ["gps_reject"] = gpsData.gps_reject,
                ["gps"] = gpsData.gps,
                ["gps_info"] = gps_info.ToJson()
            };
            return gps_extra.ToJson();
        }

        public void SetGPSExtra(string msg, out bool sameLocation, bool requestGeoNames = false, Action callback = null)
        {
            var gpsJson = JsonConvert.DeserializeObject(msg) as JObject;
            sameLocation = true;
            if (gpsJson != null)
            {
                string latitude = gpsJson.SelectToken("latitude")?.ToString();
                string longitude = gpsJson.SelectToken("longitude")?.ToString();
                var gpsExtraGps = latitude + "," + longitude;
                if (SelfGPSExtra.gps.IsNullOrEmpty() || !SelfGPSExtra.gps.Equals(gpsExtraGps))
                {
                    SelfGPSExtra.gps = gpsExtraGps;
                    sameLocation = false;
                    if (requestGeoNames)
                    {
                        NetSystem.That.GetCountryFromCoordinates(Convert.ToSingle(latitude), Convert.ToSingle(longitude));
                    }
                }
            }

            if (sameLocation)
            {
                callback?.Invoke();
            }
        }
        
        public Dictionary<string,string> GetGPSInfoData()
        {
            var dic = new Dictionary<string, string>()
            {
                ["country"] = YZDataUtil.GetLocaling(YZConstUtil.YZCountryCode, ""),
                ["province"] = YZDataUtil.GetLocaling(YZConstUtil.YZAreaCode, ""),
                ["city"] = YZDataUtil.GetLocaling(YZConstUtil.YZCity, ""),
            };
            
            return dic;
        }
        
        public Dictionary<string,string> GetIpInfoData()
        {
            if (GPSInfo == null)
            {
                return new Dictionary<string, string>();
            }
            
            var dic = new Dictionary<string, string>()
            {
                ["country"] = GPSInfo.country ?? "",
                ["province"] = GPSInfo.province ?? "",
                ["city"] = GPSInfo.city ?? ""
            };
            
            return dic;
        }
        
        public string GetUserAgent()
        {
            // 使用设备型号和操作系统信息构造一个简单的 User-Agent
            string userAgent = "Mozilla/5.0 " + SystemInfo.deviceModel + " " + SystemInfo.operatingSystem;
            Debug.Log("User Agent: " + userAgent);
            return userAgent;
        }

        public string GetOperatingSystem()
        {
            return SystemInfo.operatingSystem;
        }

        /// <summary>
        /// 获取充值时候的session id
        /// </summary>
        /// <returns></returns>
        public string GetChargeSessionId()
        {
            return $"{Root.Instance.UserId}-{TimeUtils.Instance.UTCYearMonthDay}-{TimeUtils.Instance.UtcTimeNow}";
        }
    }
}