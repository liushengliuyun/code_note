using System;
using System;
using AndroidCShape;
using Core.Extensions;
using Core.Manager;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using iOSCShape;
using UI;
using UI.UIChargeFlow;
using UnityEngine;
using UnityEngine.UI;

namespace Utils
{
    public class YZNativeUtil
    {
        private static string IDFA = "";

        public static void InitYZIDFA()
        {
            Application.RequestAdvertisingIdentifierAsync((string advertisingId, bool trackingEnabled, string error) =>
            {
                YZDebug.LogConcat("IDFA: ", advertisingId);
                if (!string.IsNullOrEmpty(advertisingId))
                {
                    if (IDFA.IsNullOrEmpty())
                    {
                        IDFA = advertisingId;
                        if (Root.Instance.UserId > 0)
                        {
                            MediatorRequest.Instance.SendAFID();
                        }
                    }
                }
            });
        }

        public static void GetTongDunBlackBox(Action<string> back)
        {
            string platform = YZGameUtil.GetPlatform();
            if (platform == YZPlatform.iOS)
            {
                // iOS
                iOSCShapeTool.Shared.IOSGetTongDunBlackBox(back);
            }
            else if (platform == YZPlatform.Android)
            {
                // Andriod
#if (UNITY_ANDROID && !NO_SDK)
                YZAndroidPlugin.Shared.AndroidTongdunBegin(back);
#endif
            }
            else
            {
                back?.Invoke("pass");
            }
        }

        //        public static void InitKyc(YZKYCParam param, Action<string> back)
        //        {
        //            string platform = YZGameUtil.GetPlatform();
        //            if (platform == YZPlatform.iOS)
        //            {
        //                // iOS
        //                iOSCShapeTool.Shared.IOSInitKyc(param, back);
        //            }
        //            else if (platform == YZPlatform.Android)
        //            {
        //                // Andriod
        //#if UNITY_ANDROID
        //                YZAndroidPlugin.Shared.AndroidKYCInit(param.token, back);
        //#endif
        //            }
        //            else
        //            {
        //                back?.Invoke("-1");
        //            }
        //        }

        #region 头像选择
        [Obsolete]
        public static void StartYZPickerImage(string cancel, string camare, string photos, Action<Sprite, string> func)
        {
            YZPickerImageParam param = new YZPickerImageParam
            {
                cancel = cancel,
                choosecamare = camare,
                choosephotos = photos
            };

            string platform = YZGameUtil.GetPlatform();
            if (platform == YZPlatform.iOS)
            {
                // iOS
                iOSCShapePhotoTool.Shared.IOSYZStartPickerImage(param, func);
            }
            else if (platform == YZPlatform.Android)
            {
                // Andriod
#if UNITY_ANDROID
                // YZAndroidPlugin.Shared.AndroidSatrtPickPicture(JsonUtility.ToJson(param), func);
#endif
            }
        }
        #endregion

        //#region 新闻中心
        //public static void ShowYZNewsViewController(string json)
        //{
        //    string platform = YZGameUtil.GetPlatform();
        //    if (platform == YZPlatform.Editor)
        //    {
        //        // Android
        //        YZNewsUICtrler.Shared().YZOnPushUI();
        //    }
        //    else if (platform == YZPlatform.iOS)
        //    {
        //        // iOS
        //        iOSCShapeWebTool.Shared.IOSYZShowNewsViewController(json);
        //    }
        //    else if (platform == YZPlatform.Android)
        //    {
        //        // Android
        //        YZNewsUICtrler.Shared().YZOnPushUI();
        //    }
        //    else
        //    {

        //    }
        //}
        //#endregion

        #region 定位

        public static void RequestYZLocationAuthorization(Action<int> func)
        {
            string platform = YZGameUtil.GetPlatform();
            if (platform == YZPlatform.iOS)
            {
                // iOS
                iOSCShapeLocationTool.Shared.RequestYZLocationAuthorization(func);
            }
            else if (platform == YZPlatform.Android)
            {
                // Android
#if (UNITY_ANDROID && !NO_SDK)
                YZAndroidLocationPlugin.AndroidLocationParams param = new YZAndroidLocationPlugin.AndroidLocationParams
                {
                    desc = YZString.Format(I18N.Get("key_allow_yatzy"), GetYZAppName()),
                    denied_desc = YZString.Format(I18N.Get("key_allow_yatzy"), GetYZAppName()),
                    denied_text = I18N.Get("key_allow_permissiom"),
                    denied_ok = I18N.Get("key_allow"),
                    denied_no = I18N.Get("key_cancel")
                };
                YZDebug.LogConcat("[定位] 弹出权限请求窗口", JsonUtility.ToJson(param));
                YZAndroidLocationPlugin.Shared.RequestLocationPermissions(JsonUtility.ToJson(param), func);
#endif
            }
            else
            {
                func?.Invoke(1);
            }
            UserInterfaceSystem.That.RemoveUIByName(nameof(UIWaitingCtrler));
        }

        public static void GetYZLocation(bool real, Action<string> func)
        {
            // if (YZDefineUtil.IsDebugger && !real)
            // {
            //     string location = PlayerPrefs.GetString(YZConstUtil.YZGpsDebugInfo);
            //     if (!string.IsNullOrEmpty(location))
            //     {
            //         func?.Invoke(location);
            //         return;
            //     }
            // }

            string platform = YZGameUtil.GetPlatform();
            if (platform == YZPlatform.iOS)
            {
                // iOS
                iOSCShapeLocationTool.Shared.IOSYZGetLocation(func); 
            }
            else if (platform == YZPlatform.Android)
            {
                // Android
#if (UNITY_ANDROID && !NO_SDK)
                YZAndroidLocationPlugin.Shared.GetGPSLocation(func);
#endif
            }
            else
            {
                func?.Invoke("Limit");
            }
        }
        #endregion

        #region 下载图片、隐私协议
        public static void SetYZUrlSprite(Image image, string url)
        {
            //YZImageController.Shared.SetYZUrlSprite(image, url);
        }

        public static void OpenYZPrivacyPolicyUrl()
        {
            Application.OpenURL(YZDefineUtil.GetYZPrivacyPolicyUrl());
        }

        public static void OpenYZPrivacyServiceUrl()
        {
            Application.OpenURL(YZDefineUtil.GetYZPrivacyServiceUrl());
        }
        #endregion


        #region 基础信息

        public static string GetYZTimeZone()
        {
            string platform = YZGameUtil.GetPlatform();
             if (platform == YZPlatform.iOS)
             {
                 return iOSCShapeTool.Shared.IOSYZGetTimeZone();
             }
            if (platform == YZPlatform.Android)
            {
#if (UNITY_ANDROID && !NO_SDK)
                return YZAndroidPlugin.Shared.AndroidGetTimeZone();
#endif
            }
            return "timezone";
        }

        public static int GetVersionCode()
        {
            string platform = YZGameUtil.GetPlatform();
            if (platform == YZPlatform.Android)
            {
#if (UNITY_ANDROID && !NO_SDK)
                return YZAndroidPlugin.Shared.AndroidGetVersionCode();
#endif
            }
            //            if (platform == YZPlatform.iOS)
            //            {
            //                return iOSCShapeTool.Shared.IOSYZGetTimeZone();
            //            }
            //            else if (platform == YZPlatform.Android)
            //            {
            //#if (UNITY_ANDROID && !NO_SDK)
            //                return YZAndroidPlugin.Shared.AndroidGetVersionCode();
            //#endif
            //            }

            //return int.Parse(Application.version.Split(".")[2]) + 1;
            return 2;
        }

        public static string GetYZLanguage()
        {
            string platform = YZGameUtil.GetPlatform();
            if (platform == YZPlatform.iOS)
            {
                return iOSCShapeTool.Shared.IOSYZGetLanguage();
            }
            else if (platform == YZPlatform.Android)
            {
#if UNITY_EDITOR || UNITY_EDITOR
                return YZAndroidPlugin.Shared.AndroidGetLanguage();
#endif
            }
            return "language";
        }

        public static string GetYZSimInfo()
        {
            string platform = YZGameUtil.GetPlatform();
            if (platform == YZPlatform.iOS)
            {
                //
                return iOSCShapeTool.Shared.IOSYZGetSimInfo();
            }
            else if (platform == YZPlatform.Android)
            {
    #if (UNITY_ANDROID && !NO_SDK)
                return YZAndroidPlugin.Shared.AndroidGetSimInfo();
    #endif
            }
            return "siminfo";
        }

        public static int GetYZPayApp()
        {
    #if UNITY_ANDROID
             return YZAndroidPlugin.Shared.AndroidGetPayApp();
    #endif
            return 0;
        }

//        public static string GetYZLocalCountryCode()
//        {
//            string platform = YZGameUtil.GetPlatform();
//            if (platform == YZPlatform.iOS)
//            {
//                return iOSCShapeTool.Shared.IOSYZGetLocalCountryCode();
//            }
//            else if (platform == YZPlatform.Android)
//            {
//#if UNITY_ANDROID
//                return YZAndroidPlugin.Shared.AndroidGetLocalCountryCode();
//#endif
//            }
//            return "countrycode";
//        }

        public static string GetYZDeviceInfo()
        {
            string platform = YZGameUtil.GetPlatform();
            if (platform == YZPlatform.iOS)
            {
                return iOSCShapeTool.Shared.IOSYZGetDeviceInfo();
            }
            else if (platform == YZPlatform.Android)
            {
#if (UNITY_ANDROID && !NO_SDK)
                return YZAndroidPlugin.Shared.AndroidGetDeviceInfo();
#endif
            }
            return "deviceinfo";
        }

        public static string GetIPAdress()
        {
            string platform = YZGameUtil.GetPlatform();
            if (platform == YZPlatform.iOS)
            {
                // string ipAddr = YZGameUtil.GetIpAddress(IPAddressType.IPv4);
                string ipAddr = Root.Instance.IP;
                return ipAddr;
                //return iOSCShapeTool.Shared.IOSYZGetIPAddress();
            }
            else if (platform == YZPlatform.Android)
            {
#if UNITY_ANDROID
                return YZAndroidPlugin.Shared.AndroidGetIPAdress();
#endif
            }
            return "";
        }

        public static string GetYZUDID()
        {
            if (!string.IsNullOrEmpty(PlayerManager.Shared.User.YZUDID))
            {
                return PlayerManager.Shared.User.YZUDID;
            }

            string platform = YZGameUtil.GetPlatform();
            if (platform == YZPlatform.iOS)
            {
                //PlayerManager.Shared.User.YZUDID = YZString.Concat(YZDefineUtil.PreName, iOSCShapeTool.Shared.IOSYZGetUDID());
            }
            else if (platform == YZPlatform.Android)
            {
                PlayerManager.Shared.User.YZUDID =
                    YZString.Concat(YZDefineUtil.PreName, SystemInfo.deviceUniqueIdentifier);
            }
            else
            {
                PlayerManager.Shared.User.YZUDID =
                    YZString.Concat(YZDefineUtil.PreName, SystemInfo.deviceUniqueIdentifier);
            }
            return PlayerManager.Shared.User.YZUDID;
        }

//     public static bool HasYZNotchInScreen()
//     {
//         string platform = YZGameUtil.GetPlatform();
//         if (platform == YZPlatform.iOS)
//         {
//             return iOSCShapeTool.Shared.IOSYZIsFullSecreen();
//         }
//         else if (platform == YZPlatform.Android)
//         {
//             return false;
//         }
//         return false;
//     }
//
//     /// 这里暂时都使用100个像数，不用太精确了
//     public static float GetYZTopSafeHight()
//     {
//         if (HasYZNotchInScreen())
//         {
//             return 100;
//         }
//         return 0;
//     }

        public static string GetYZIDFA()
        {
            if (string.IsNullOrEmpty(IDFA))
            {
#if (UNITY_ANDROID && !NO_SDK)
                IDFA = YZAndroidPlugin.Shared.AndroidGetIDFA();
                YZDebug.Log("IDFA = " + IDFA);
                // 依然没拿到（可能是模拟器)
                if (string.IsNullOrEmpty(IDFA))
                    IDFA = "IDFA";
                return IDFA;
#else
            // todo modify
            return "IDFA";
#endif
            }

            return IDFA;
        }

        public static string GetYZAppVersion()
        {
            return Application.version;
        }

        public static string GetYZPackageName()
        {
#if UNITY_ANDROID || UNITY_IOS
            return YZDebug.GetBundleId();
#else
            return "com.yzstudio.test";
#endif
        }

        public static string GetYZAppName()
        {
#if UNITY_IOS
            return "Bingo Bliss";
#else
            return "Bingo Brawl";
#endif
        }
        #endregion
/// <summary>
/// 发送邮件
/// </summary>
/// <param name="pos"></param>
        public static void ContactYZUS(string pos)
        {
            if (pos is EmailPos.Loading or EmailPos.Code9997 or EmailPos.Code9998 or EmailPos.Setting or EmailPos.Charge)
            {
                var strBuilder = YZString.GetShareStringBuilder();
                strBuilder.Append("------------------------------------->");
                strBuilder.Append("\r\n").Append(GetYZDeviceInfo());
                strBuilder.Append("\r\nAppVersion:").Append(GetYZAppVersion());
                strBuilder.Append("\r\nID:").Append(Root.Instance.UserId);
                strBuilder.Append("\r\nPos:").Append(pos);
                strBuilder.Append("\r\nCountry:").Append(Root.Instance.Role.country);
                strBuilder.Append("\r\n<-------------------------------------");
                strBuilder.Append("\r\n").Append("Please tell us how can we help you:");
                string platform = YZGameUtil.GetPlatform();
                if (platform == YZPlatform.Editor)
                {
                    //Editor
                    string subject = I18N.Get("key_hello_email", GetYZAppName());
                    Uri uri = new Uri(string.Format("mailto:{0}?subject={1}&body={2}", YZDefineUtil.GetYZEmail(),
                        subject, strBuilder.ToString()));
                    Application.OpenURL(uri.AbsoluteUri);
                }
                else if (platform == YZPlatform.iOS)
                {
                    // iOS
                    string subject = I18N.Get("key_hello_email", GetYZAppName());
                    Uri uri = new Uri(string.Format("mailto:{0}?subject={1}&body={2}", YZDefineUtil.GetYZEmail(),
                        subject, strBuilder.ToString()));
                    Application.OpenURL(uri.AbsoluteUri);
                }
                else if (platform == YZPlatform.Android)
                {
                    // Android
#if (UNITY_ANDROID && !NO_SDK)
                    YZLog.LogColor("发送邮件 ： " + strBuilder.ToString());
                    YZAndroidPlugin.Shared.AndroidContactUS(strBuilder.ToString());
#endif
                }
            }
            else
            {
                // LocationManager.Shared.IsLocationValid(YZSafeType.AiHelp, null, -1, () => { }, (location) => {
                //     AIHelpManager.Shared.YZShowFAQSections();
                //     AIHelpManager.Shared.YZUpdateFAQData(pos, location);
                // });
            }
        }

                public static void OpenYZEmailApp()
                {
                    string platform = YZGameUtil.GetPlatform();
                    if (platform == YZPlatform.iOS)
                    {
                        // iOS
                        iOSCShapeTool.Shared.IOSYZOpenEmailApp();
                    }
                    else if (platform == YZPlatform.Android)
                    {
                        // Android
        #if UNITY_ANDROID
                        YZAndroidPlugin.Shared.AndroidOpenEmailApp();
        #endif
                    }
                }

//        #region 振动
//        public static void StartYZVibration()
//        {
//            string platform = YZGameUtil.GetPlatform();
//            if (platform == YZPlatform.iOS)
//            {
//                // iOS
//                iOSCShapeTool.Shared.IOSYZStartVibration(YZVibrationType.Heavy);
//            }
//            else if (platform == YZPlatform.Android)
//            {
//                // Android
//#if UNITY_ANDROID
//                YZAndroidVibrationPlugin.Shared.YZVibrate(100);
//#endif
//            }
//        }
//        #endregion

        //     #region 跳转app store
        //     public static void GoYZtoRate()
        //     {
        //         string platform = YZGameUtil.GetPlatform();
        //         if (platform == YZPlatform.iOS)
        //         {
        //             // iOS
        //             iOSCShapeTool.Shared.IOSYZGotoRate();
        //         }
        //         else if (platform == YZPlatform.Android)
        //         {
        //             // Android
        //             Application.OpenURL(YZDefineUtil.GoYZAppStore());
        //         }
        //     }
        //     #endregion
        //
        //     #region WebView

        public static void ShowYZWebView(string webviewparams, WebViewCallBack closedCallback = null, WebViewCallBack changedCallback = null)
        {
            YZInAppWebViewParams webview = YZGameUtil.JsonYZToObject<YZInAppWebViewParams>(webviewparams);
            iOSCShapeWebTool.Shared.IOSYZShowWebViewInApp(webview);
            iOSCShapeWebTool.Shared.webview_closed_callback = closedCallback;
            iOSCShapeWebTool.Shared.webview_changed_callback = changedCallback;
        }

        public static void ShowYZWebView(YZInAppWebViewParams webview, WebViewCallBack closedCallback = null, WebViewCallBack changedCallback = null)
        {
            iOSCShapeWebTool.Shared.IOSYZShowWebViewInApp(webview);
            iOSCShapeWebTool.Shared.webview_closed_callback = closedCallback;
            iOSCShapeWebTool.Shared.webview_changed_callback = changedCallback;
        }

        public static void CloseYZWebView()
        {
            iOSCShapeWebTool.Shared.IOSYZCloseWebView();
        }

        public static void EvaluateYZJavaScript(string js)
        {
            YZDebug.Log("JavaScript string:" + js);
            // 1. 拆分字符串，得到IV
            string[] subStrs = js.Split('#');
            if (subStrs.Length < 2)
                return;
            YZAESUtil aesTool = new YZAESUtil(YZServerUtil.GetYZChargeCenterAppSecret(), subStrs[0]);
            string aes_output_str = aesTool.DecipherAESYZ(subStrs[1]);

            // 调用最终接口，执行js
            byte[] arr_byte = Convert.FromBase64String(aes_output_str);
            string targetJs = System.Text.Encoding.ASCII.GetString(arr_byte);

            YZDebug.Log("JavaScript string:" + targetJs);
            //iOSCShapeWebTool.Shared.IOSYZEvaluateJavaScript(targetJs);

#if UNITY_ANDROID
              UIChargeWebview.Shared().panel_webview.EvaluateJavaScript(targetJs);
#elif UNITY_IOS
            iOSCShapeWebTool.Shared.IOSYZEvaluateJavaScript(targetJs);
#endif
        }

        public static void OpenYZSafariURL(string url)
        {
            iOSCShapeWebTool.Shared.IOSYZShowWebViewSafari(url);
        }

        #region Appsflyer
        public static string GetYZAFID()
        {
            string platform = YZGameUtil.GetPlatform();
            if (platform == YZPlatform.iOS)
            {
                // iOS
                var result = iOSCShapeAppsflyerTool.Shared.IOSGetAppsFlyerId();
                YZLog.LogColor("IOSGetAppsFlyerId = " + result);
                return result;
                //return "iOS AFID";
            }
            else if (platform == YZPlatform.Android)
            {
                // Android
#if UNITY_ANDROID 
                return YZAndroidAppsflyerPlugin.Shared.AndroidGetAppsFlyerId();
#endif
            }
            return "Unity editor";
        }
        #endregion

         #region 推送
         //public static void RequestPushAuth(int type, int id, Action back)
         //{
         //    string platform = YZGameUtil.GetPlatform();
         //    if (platform == YZPlatform.iOS)
         //    {
         //       iOSCShapePushTool.Shared.IOSYZRequestPushAuth(type, id, back);
         //    }
         //    else
         //    {
         //        back?.Invoke();
         //    }
         //}

        public static void SetPushTags(string json)
        {
            string platform = YZGameUtil.GetPlatform();
            if (platform == YZPlatform.iOS)
            {
                // iOS
                if (iOSCShapePushTool.Shared.YZInited)
                {
                    iOSCShapePushTool.Shared.IOSYZSetTags(json);
                }
                else
                {
                    iOSCShapePushTool.Shared.YZTags = json;
                }
            }
            else
            {
                // Android
#if UNITY_ANDROID || UNITY_EDITOR
                YZAndroidPushPlugin.Shared.AndroidYZSetTags(json);
#endif
            }
        }
        #endregion

    }
}