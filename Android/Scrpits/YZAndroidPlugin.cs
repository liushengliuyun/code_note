using System;
using CatLib.EventDispatcher;
using Core.Controllers;
using Core.Extensions;
using Core.MyAttribute;
using Core.Services.PersistService.API.Facade;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Third.I18N;
using DataAccess.Model;
using DataAccess.Utils.Static;
using iOSCShape;
using UI;
using UnityEngine;
using Utils;

namespace AndroidCShape
{
#if (UNITY_ANDROID && !NO_SDK) || UNITY_EDITOR
    public class YZAndroidPlugin : YZBaseController<YZAndroidPlugin>
    {
        private Action<string> func;

        private Action<string> func_tongdun;

        private Action<string> func_kyc;

        public void AndroidKYCInit(string token, Action<string> func)
        {
            func_kyc = func;
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            WudiAttribution.CallStatic("InitKYC", YZDefineUtil.IsDebugger ? "1" : "0", token);
        }

        public string AndroidGetTimeZone()
        {
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            return WudiAttribution.CallStatic<string>("getTimeZone");
        }

        public string AndroidGetIDFA()
        {
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            return WudiAttribution.CallStatic<string>("GetIDFA");
        }

        public int AndroidGetVersionCode()
        {
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            return WudiAttribution.CallStatic<int>("GetVersionCode");
        }

        public string AndroidGetLanguage()
        {
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            return WudiAttribution.CallStatic<string>("getLanguage");
        }

        public string AndroidGetDeviceInfo()
        {
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            return WudiAttribution.CallStatic<string>("getDeviceInfo");
        }
        
        public string AndroidGetIPAdress()
        {
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            return WudiAttribution.CallStatic<string>("GetIPAdress");
        }

        public string AndroidGetSimInfo()
        {
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            return WudiAttribution.CallStatic<string>("GetSimInfo");
        }

        public int AndroidGetPayApp()
        {
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            return WudiAttribution.CallStatic<int>("GetPayApp");
        }

        public string AndroidGetLocalCountryCode()
        {
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            return WudiAttribution.CallStatic<string>("GetLocalCountryCode");
        }

        public bool AndroidIsNetworkAvailable()
        {
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            return WudiAttribution.CallStatic<bool>("isNetworkConnected");
        }

        public void AndroidOpenMaxTestTool()
        {
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            WudiAttribution.CallStatic("OpenMaxTestTool");
        }

        public void AndroidForterSetAccount()
        {
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            WudiAttribution.CallStatic("ForterSetAccount", YZDebug.GetAccountId());
        }

        public string AndroidGetForterId()
        {
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            return WudiAttribution.CallStatic<string>("GetForterId");
        }

        public bool AndroidIsEmulator()
        {
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            return WudiAttribution.CallStatic<bool>("IsEmulator");
        }
        
        public bool AndroidPingGoogle()
        {
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            return WudiAttribution.CallStatic<bool>("PingGoogle");
        }

        public void AndroidForterSendEvent(string eventName, string eventParams = "")
        {
            // Forter 和 riskified 的打点事件都写在这里
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            WudiAttribution.CallStatic("ForterSendEvent", eventName, eventParams);
        }

        public string AndroidGetIPV4V6()
        {
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            return WudiAttribution.CallStatic<string>("GetIPV4V6");
        }
        
        public string AndroidGetLastIP()
        {
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            return WudiAttribution.CallStatic<string>("GetIPSingle");
        }

        public string AndroidGetUserAgent()
        {
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            return WudiAttribution.CallStatic<string>("GetUserAgent");
        }

        public void AndroidContactUS(string body)
        {
            string json = I18N.Get("key_choose");
            string subject = I18N.Get("key_hello_email", YZNativeUtil.GetYZAppName());
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            WudiAttribution.CallStatic("ContactUS", YZDefineUtil.GetYZEmail(), subject, body, json);
        }

        public void AndroidOpenEmailApp()
        {
            string json = I18N.Get("key_choose");
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            if (!WudiAttribution.CallStatic<bool>("OpenEmail", json))
            {
                UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_fail_open_mail_app"));
            }
        }

        //震动强度 = 震动时长 0-99
        public void AndroidVibrate()
        {
            if (!Root.Instance.VibrationON)
            {
                return;
            }

            var value = (float)PersistSystem.That.GetValue<float>(GlobalEnum.VIBRATION_VOLUME) +
                        GlobalEnum.SliderValueOffset;

            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");

            //持续时间
            var duration = Math.Max(0, value) / 99 * 600;

            if (duration <= 0)
            {
                return;
            }

            long[] mpattern = new long[] { 100, 200 + (long)duration };

            //-1表示不重复执行这个模式
            WudiAttribution.CallStatic("Vibrate", mpattern, -1);
        }


        /// <summary>
        /// 轻微震动
        /// </summary>
        public void TinyAndroidVibrate()
        {
            if (!Root.Instance.VibrationON)
            {
                return;
            }

            var value = (float)PersistSystem.That.GetValue<float>(GlobalEnum.VIBRATION_VOLUME) +
                        GlobalEnum.SliderValueOffset;

            if (value <= 0)
            {
                return;
            }

            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");

            //持续时间
            var duration = Math.Max(0, value) / 99 * 40;

            YZLog.LogColor("震动持续时间  " + (long)duration);
            
            long[] mpattern = { 0, (long)duration };

            //-1表示不重复执行这个模式
            WudiAttribution.CallStatic("Vibrate", mpattern, -1);
        }

        public void AndroidStartPickPicture(string json, Action<string> func)
        {
            this.func = func;
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            WudiAttribution.CallStatic("StartPickPicture", json);
        }

        public void AndroidStopPickPicture()
        {
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            WudiAttribution.CallStatic("StopPickPicture");
        }

        public void AndroidExitApplication()
        {
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            WudiAttribution.CallStatic("ExitApplication");
        }

        public void AndroidTongdunBegin(Action<string> back)
        {
            func_tongdun = back;
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            WudiAttribution.CallStatic("BeginTongdun", YZDefineUtil.IsDebugger ? "1" : "0");
        }

        public void TongDunBlackboxCallback(string msg)
        {
            func_tongdun?.Invoke(msg);
        }

        public void KYCFinish(string msg)
        {
            func_kyc?.Invoke(msg);
        }
        
        [CallByAndroid]
        public void PickerImageFinish(string file)
        {
            if (!file.IsNullOrEmpty())
            {
                func?.Invoke(file);
                func = null;
            }
            else
            {
                YZLog.LogColor("照片路径为空", "red");
            }
            EventDispatcher.Root.Raise(GlobalEvent.Pick_Image_Finish);
        }

        public void PickPictureFailed(string type)
        {
            if (type == YZNativeErrorCode.picture_c)
            {
                UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_pitcure_c_error"));
            }
            else
            {
                UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_picture_l_error"));
            }
        }

        public int AndroidVersion
        {
            get
            {
                int iVersionNumber = 0;
                if (Application.platform == RuntimePlatform.Android)
                {
                    string androidVersion = SystemInfo.operatingSystem;
                    int sdkPos = androidVersion.IndexOf("API-");
                    iVersionNumber = int.Parse(androidVersion.Substring(sdkPos + 4, 2).ToString());
                }

                return iVersionNumber;
            }
        }
    }
#endif
}