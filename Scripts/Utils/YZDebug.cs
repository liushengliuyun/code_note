using System;
using System.Collections.Generic;
using AndroidCShape;
using DataAccess.Model;
using DataAccess.Utils;
using UnityEngine;

namespace Utils
{
    public class YZDebug
    {
        public static bool enable = true;

        private static List<string> deviceAFIDList;

        public static void Log(object obj)
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
            enable = true;
#endif
            if (enable)
            {
                Debug.Log(obj);
            }
        }

        public static void LogWarning(object obj)
        {
            if (enable)
            {
                Debug.LogWarning(obj);
            }
        }

        public static void LogFormat(string format, params object[] args)
        {
            if (enable)
            {
                Debug.LogFormat(format, args);
            }
        }

        public static void LogWarningFormat(string format, params object[] args)
        {
            if (enable)
            {
                Debug.LogWarning(YZString.Format(format, args));
            }
        }

        public static void LogConcat(params object[] args)
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
            enable = true;
#endif
            if (enable)
            {
                Debug.Log(YZString.Concat(args));
            }
        }

        public static void LogError(object obj)
        {
            Debug.LogError(obj);
        }

        public static void LogErrorFormat(string format, params object[] args)
        {
            Debug.LogError(YZString.Format(format, args));
        }

        public static bool IsWhiteListTestDevice()
        {
            if (deviceAFIDList == null)
            {
                deviceAFIDList = new List<string>();
                // deviceAFIDList.Add("b8e51307-48ce-4cf9-bd2e-8eaa96b852ec");
                // deviceAFIDList.Add("c20c5dcb-c7e5-43b0-9f0c-dd0a6181553a");
                // deviceAFIDList.Add("a8b5dd32-5ca9-4d22-9101-6dbaf27a373e");
            }

            string idfa = YZNativeUtil.GetYZIDFA();
            bool isTestDevice =  deviceAFIDList.Find(match: s => s.Equals(idfa)) != null;

            if (isTestDevice)
            {
                Log("这台是测试机！");
            }

            return isTestDevice;
        }

        public static string GetBundleId()
        {
#if RELEASE 
                return Application.identifier;
#else
                return Application.identifier + ".test";
#endif
        }

        /// <summary>
        /// Forter【风控】使用
        /// </summary>
        /// <returns></returns>
        public static string GetAccountId()
        {
            return GetGameNameLower() + "_" + Root.Instance.Role.user_id;
        }

        public static string GetGameNameLower()
        {
#if UNITY_IOS
            return "bingobliss";
#endif
            return "bingobrawl";
        }

        public static string GetLastIP()
        {
#if UNITY_ANDROID
                 return YZAndroidPlugin.Shared.AndroidGetLastIP();
#endif
            return Root.Instance.LastIP ?? "";
        }

        public static string GetIPV4V6()
        {
#if UNITY_ANDROID
            return YZAndroidPlugin.Shared.AndroidGetIPV4V6();
#endif
            return Root.Instance.IP ?? "";
        }

        /// <summary>
        /// 风控id , 应该就是设备Id
        /// </summary>
        /// <returns></returns>
        public static string GetForterId()
        {
#if UNITY_ANDROID
            return YZAndroidPlugin.Shared.AndroidGetForterId();
#endif
            return DeviceInfoUtils.Instance.GetEquipmentId();
        }

        public static string GetForterRelatedUUID()
        {
            return GetGameNameLower() + "_" + SystemInfo.deviceUniqueIdentifier;
        }
    }
}