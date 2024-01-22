using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using Carbon.Util;
using Core.Controllers;
using Core.Controls;
using Core.MyAttribute;
using Core.Third.I18N;
using DataAccess.Model;
using Utils;

namespace iOSCShape
{
    public class YZNativeErrorCode
    {
        public const string shared_sms = "s1"; // 打开短信失败
        public const string shared_msg = "s2"; // 打开messenger失败
        public const string shared_wtp = "s3"; // 打开whatsapp失败
        public const string shared_dft = "s4"; // 打开分享失败

        public const string picture_c = "p1"; // 打开相机失败
        public const string picture_l = "p2"; // 打开相册失败
    }

    public enum YZLocationAccuracy : int
    {
        kCLLocationAccuracyBestForNavigation = 1,
        kCLLocationAccuracyBest = 2,
        kCLLocationAccuracyNearestTenMeters = 3,
        kCLLocationAccuracyHundredMeters = 4,
        kCLLocationAccuracyKilometer = 5,
        kCLLocationAccuracyThreeKilometers = 6,
    }

    public enum YZNotifyStatus
    {
        None,
        Denied,
        Authorized
    }

    public enum YZAttStatus
    {
        None,
        Denied,
        Restricted,
        Authorized
    }

    public class iOSCShapeTool : YZBaseController<iOSCShapeTool>
    {

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern string ObjcGetUDIDUnity();
    [DllImport("__Internal")] private static extern string ObjcGetSimInfoUnity();
    [DllImport("__Internal")] private static extern string ObjcGetTimeZoneUnity();
    [DllImport("__Internal")] private static extern string ObjcGetLanguageUnity();
    [DllImport("__Internal")] private static extern string ObjcGetATTStatusUnity();
    [DllImport("__Internal")] private static extern string ObjcGetDeviceInfoUnity();
    [DllImport("__Internal")] private static extern string ObjcGetIPAddressUnity();
    [DllImport("__Internal")] private static extern string ObjcGetPasteBoardTextUnity();
    [DllImport("__Internal")] private static extern string ObjcGetLocalCountryCodeUnity();
    
    [DllImport("__Internal")] private static extern bool ObjcIsFullSecreenUnity();
    [DllImport("__Internal")] private static extern bool ObjcIsEnableWIFIUnity();
    [DllImport("__Internal")] private static extern bool ObjcSendEmailUnity(string param);

    [DllImport("__Internal")] private static extern void ObjcSetPasteBoardTextUnity(string param);
    [DllImport("__Internal")] private static extern void ObjcOpenEmailAppUnity();
    [DllImport("__Internal")] private static extern void ObjcRequestATTUnity();
    [DllImport("__Internal")] private static extern void ObjcStartVibrationUnity(string param);
    [DllImport("__Internal")] private static extern void ObjcGotoRateUnity(string param);
    [DllImport("__Internal")] private static extern void ObjcGotoSettingUnity();
    [DllImport("__Internal")] private static extern void ObjcGetNetworkAuthUnity();
    [DllImport("__Internal")] private static extern void ObjcGetTongDunBlackBoxUnity();

    [DllImport("__Internal")] private static extern void ObjcInitKycUnity(string param);
    [DllImport("__Internal")] private static extern void ObjcUpdateKycTokenUnity(string param);
#endif

        private Action<string> brtongdunfunc;
        private Action<string> brkycfunc;

        public void IOSInitKyc(YZKYCParam pay, Action<string> func)
        {
#if UNITY_IOS && !UNITY_EDITOR
        brkycfunc = func;
        string json = JsonUtility.ToJson(pay);
        ObjcInitKycUnity(json);
#endif
        }

        public void IOSUpdateKycToken(string param)
        {
#if UNITY_IOS && !UNITY_EDITOR
        ObjcUpdateKycTokenUnity(param);
#endif
        }

        public void IOSYZGotoSetting()
        {
#if UNITY_IOS && !UNITY_EDITOR
        ObjcGotoSettingUnity();
#endif
        }

        public void IOSYZGetPasteBoardText()
        {
#if UNITY_IOS && !UNITY_EDITOR
        ObjcGetPasteBoardTextUnity();
#endif
        }

        public void IOSYZGetNetworkAuth()
        {
#if UNITY_IOS && !UNITY_EDITOR
        ObjcGetNetworkAuthUnity();
#endif
        }

        public void IOSYZSetPasteBoardText(string text)
        {
#if UNITY_IOS && !UNITY_EDITOR
        ObjcSetPasteBoardTextUnity(text);
#endif
#if UNITY_EDITOR
            GUIUtility.systemCopyBuffer = text;
#endif
        }

        public void IOSYZGotoRate()
        {
#if UNITY_IOS && !UNITY_EDITOR
        ObjcGotoRateUnity(YZDefineUtil.AppleID);
#endif
        }

        public void IOSYZOpenEmailApp()
        {
#if UNITY_IOS && !UNITY_EDITOR
        ObjcOpenEmailAppUnity();
#endif
        }

        public void IOSYZRequestATT()
        {
#if UNITY_IOS && !UNITY_EDITOR
                try
                {
 ObjcRequestATTUnity();
                }
                catch (Exception e)
                {
                        CarbonLogger.LogError("IOSYZRequestATT " + e + "\n" + e.StackTrace);
                        throw;
                }
#endif
        }

        /// 获取当前用户的授权状态
        /// "0":用户还未决定授权
        /// "1":用户已同意
        /// "2":用户已拒绝
        /// "3":访问受限，ATT服务授权状态是受限制的。可能是由于活动限制ATT服务，用户不能改变。这个状态可能不是用户拒绝的ATT服务
        public string IOSYZGetATTStatus()
        {
#if UNITY_IOS && !UNITY_EDITOR
        return ObjcGetATTStatusUnity();
#else
            return "1";
#endif
        }

        public string IOSYZGetUDID()
        {
#if UNITY_IOS && !UNITY_EDITOR
        return ObjcGetUDIDUnity();
#else
            return SystemInfo.deviceUniqueIdentifier;
#endif
        }

        public string IOSYZGetTimeZone()
        {
#if UNITY_IOS && !UNITY_EDITOR
        return ObjcGetTimeZoneUnity();
#else
            return "";
#endif
        }

        public string IOSYZGetLanguage()
        {
#if UNITY_IOS && !UNITY_EDITOR
        return ObjcGetLanguageUnity();
#else
            return "";
#endif
        }

        public string IOSYZGetDeviceInfo()
        {
#if UNITY_IOS && !UNITY_EDITOR
        return ObjcGetDeviceInfoUnity();
#else
            return "";
#endif
        }

        public string IOSYZGetIPAddress()
        {
#if UNITY_IOS && !UNITY_EDITOR
        return ObjcGetIPAddressUnity();
#else
            return "";
#endif
        }

        public string IOSYZGetLocalCountryCode()
        {
#if UNITY_IOS && !UNITY_EDITOR
        return ObjcGetLocalCountryCodeUnity();
#else
            return "";
#endif
        }

        public string IOSYZGetSimInfo()
        {
#if UNITY_IOS && !UNITY_EDITOR
        return ObjcGetSimInfoUnity();
#else
            return "";
#endif
        }

        /// <summary>
        /// Light,Medium,Heavy,Success,Warning,Error,Changed
        /// </summary>
        /// <param name="type"></param>
        public void IOSYZStartVibration(string type)
        {
            if (!Root.Instance.VibrationON)
            {
                return;
            }
#if UNITY_IOS && !UNITY_EDITOR
        ObjcStartVibrationUnity(type);
#endif
        }

        public bool IOSYZIsFullSecreen()
        {
#if UNITY_IOS && !UNITY_EDITOR
        return ObjcIsFullSecreenUnity();
#else
            return false;
#endif
        }

        public bool IOSYZSendEmailUnity(string pos)
        {
#if UNITY_IOS && !UNITY_EDITOR
        YZiOSEmail e = new YZiOSEmail();
        e.title = I18N.Get("key_contact_us");
        e.subject = I18N.Get("key_hello_email", YZNativeUtil.GetYZAppName());
        e.email = YZDefineUtil.GetYZEmail();
        e.id = Root.Instance.UserId.ToString();
        e.version = YZNativeUtil.GetYZAppVersion();
        // e.code = PlayerManager.Shared.User.YZCountry;
        e.pos = pos;
        string json = JsonUtility.ToJson(e);
        return ObjcSendEmailUnity(json);
#else
            return false;
#endif
        }

        public bool IOSYZIsEnableWIFI()
        {
#if UNITY_IOS && !UNITY_EDITOR
        return ObjcIsEnableWIFIUnity();
#else
            return true;
#endif
        }

        public void IOSGetTongDunBlackBox(Action<string> back)
        {
#if UNITY_IOS && !UNITY_EDITOR
            brtongdunfunc = back;
            ObjcGetTongDunBlackBoxUnity();
#endif
        }

        // KYC更新token
        public void CShapeKYCUpdateToken(string msg)
        {
            // YZServerSupportCenter.Shared.RequestKycToken((token) => {
            //     IOSUpdateKycToken(token);
            // });
        }

        public void CShapeKYCFinish(string msg)
        {
            brkycfunc?.Invoke(msg);
        }

        // 同盾返回blackbox
        public void CShapeGetTongDunBlackBoxFinish(string msg)
        {
            brtongdunfunc?.Invoke(msg);
        }

        [CallByIOS]
        public void CShapeTrackingAuthorizationWithCompletion(string status)
        {
            YZDebug.LogConcat("ATT: ", status);
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add(FunnelEventID.brpermissionidfa, status == "1");
            YZFunnelUtil.UserYZSet(dict);
            YZNativeUtil.InitYZIDFA();
        }

        public void CShapeiOSLowMemory(string msg)
        {
            Debug.Log("ios low memory");
        }

        public void CShapeUserNetworkAuth(string msg)
        {
            Debug.Log(YZString.Concat("wifi status: ", msg));
        }

        public void CShapeSendEmailFinish(string msg)
        {
            if (msg == "1")
            {
                // 取消了发送
                //YZTopControl.YZShowAutoHideTips("");
            }
            else if (msg == "2")
            {
                // 保存了邮件
                //YZTopControl.YZShowAutoHideTips("");
            }
            else if (msg == "3")
            {
                // 发送成功
                // YZTopControl.YZShowAutoHideTips("Send Success");
            }
            else
            {
                // 发生失败
                // YZTopControl.YZShowAutoHideTips("Send Failed");
            }
        }
    }
}