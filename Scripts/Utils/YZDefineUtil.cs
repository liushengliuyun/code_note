using AndroidCShape;
using UnityEngine;

namespace Utils
{
    public class YZDefineUtil
    {
        public const string AppleID = "1635306934"; // 苹果ID

        public const bool EidtorIsOgranic = false; // 编辑器模式下，归因结果(该属性只在编辑器模式下生效)

        /// 推荐帧率
        public static int GameFrameRate
        {
            get
            {
                return 90;
            }
        }

        /// 是否测试
        public static bool IsDebugger
        {
#if DEBUG
            get { return true; }
#else
            get { return false; }
#endif
        }

        /// 是否打点
        public static bool GetIsFunnel
        {
            get { return true; }
        }

        /// 是否专机
        public static bool IsSpecials
        {
            get { return Application.identifier == YZNativeUtil.GetYZPackageName(); }
        }

        /// 打点前缀
        public static string PreName
        {
            get
            {
                if (YZGameUtil.GetIsiOS())
                {
                    return "";
                }
                else
                {
                    return "";
                }
            }
        }

        /// 后台版本
        public static string GetYZVersionCode()
        {
#if (UNITY_ANDROID && !NO_SDK)
                var versionCode = YZAndroidPlugin.Shared.AndroidGetVersionCode();
            YZDebug.Log("version code = " + versionCode);
            return versionCode.ToString();
#endif
            return "";
        }

        /// Apple Pay 商业id
        public static string GetYZApplePayId()
        {
            if (IsSpecials)
            {
                return "merchant.game.bingocash";
            }
            else
            {
                return "merchant.com.applepay.checkouttest";
            }
        }

        /// 服务器 key
        public static string GetYZServerKey()
        {
            if (YZGameUtil.GetIsiOS())
            {
                return "ofkRdSOxWUWrhVimhDSKuj73LLOuISMC";
            }
            else
            {
                return "VjRwCG4PVE18BM3AyCteBaSJGoJLF2kY";
            }
        }

        /// 通用后台 url
        public static string GetYZGenaralURL()
        {
            if (YZGameUtil.GetIsiOS())
            {
                return "https://bingobliss-ios.apexdynamics.club/";
            }
            else
            {
                return "https://bingobrawl-gp.yatzybrawl.com/";
            }
        }

        /// 通用后台 key
        public static string GetYZGenaralAppKey()
        {
            if (YZGameUtil.GetIsiOS())
            {
                return "C9lcMn0O48TlfXhz";
            }
            else
            {
                return "C6qQAbJAGyfdJwB7";
            }
        }

        /// 通用后台 Secret
        public static string GetYZGenaralSecretKey()
        {
            if (YZGameUtil.GetIsiOS())
            {
                return "joorltE5aaVMb3UoOjUnKbEvONG27s9y";
            }
            else
            {
                return "lAHAZZs48zgJPKkYZQ9VoKESt0HEj3o0";
            }
        }

        /// Policy
        public static string GetYZPrivacyPolicyUrl()
        {
            if (YZGameUtil.GetIsiOS())
            {
                return "https://apexdynamics.club/pp.html";
            }
            else
            {
                return "https://superdrawl.club/pp.html";
            }
        }

        /// Service
        public static string GetYZPrivacyServiceUrl()
        {
            if (YZGameUtil.GetIsiOS())
            {
                return "https://apexdynamics.club/tos.html";
            }
            else
            {
                return "https://superdrawl.club/tos.html";
            }
        }

        /// 邮箱
        public static string GetYZEmail()
        {
            if (YZGameUtil.GetIsiOS())
            {
                return "BingoBliss96@outlook.com";
            }
            else
            {
                return "BingoBrawl@outlook.com";
            }
        }

        /// 广告 key
        public static string GetYZADSKey()
        {
            if (YZGameUtil.GetIsiOS())
            {
                return "1959888cd";
            }
            else
            {
                return "19e539fbd";
            }
        }

        /// bugly
        public static string GetYZBuglyAppId()
        {
            if (YZGameUtil.GetIsiOS())
            {
                return "25faeb6d05";
            }
            else
            {
                return "cbf5f173f3";
            }
        }

        // onesignal
        public static string GetYZOneSignalAppId()
        {
            if (YZGameUtil.GetIsiOS())
            {
                return "762c80f0-a628-4d1e-890c-0a06a3080327";
            }
            else
            {
                return "e70985e5-d40d-4acb-8599-d4d628307a7f";
            }
        }
        
        // appsflyer
        public static string GetAppsFlyerDevKey()
        {
            if (YZGameUtil.GetIsiOS())
            {
                return "fbFzQ4SPxJXoYBmHhaZbAa";
            }
            else
            {
                return "oK79pgYkL6hHsjcaoeqeVZ";
            }
        }
        
        public static string GetAppsFlyerAppId()
        {
            if (YZGameUtil.GetIsiOS())
            {
                return "";
            }
            else
            {
                return "";
            }
        }

        public static string GetFBAPPId()
        {
            if (YZGameUtil.GetIsiOS())
            {
                return "376118308109668";
            }
            else if (YZGameUtil.GetIsAndroid())
            {
                return "661176902664482";
            }
            else
            {
                return "";
            }
        }

        public static string GetFBClientToken()
        {
            if (YZGameUtil.GetIsiOS())
            {
                return "237e9ca21962ee7ce08b4acc320db21b";
            }
            else if (YZGameUtil.GetIsAndroid())
            {
                return "5574f917aa08a8b53efc436fadc39d08";
            }
            else
            {
                return "";
            }
        }

        /// 去 App Store
        public static string GoYZAppStore(bool isRate = true)
        {
            if (YZGameUtil.GetIsiOS())
            {
                if (isRate)
                {
                    return YZString.Format("https://itunes.apple.com/us/app/id{0}?action=write-review", AppleID);
                }
                else
                {
                    return YZString.Format("https://apps.apple.com/us/app/id{0}", AppleID);
                }
            }
            else
            {
                return YZString.Format("https://play.google.com/store/apps/details?id={0}", Application.identifier);
            }
        }

        /// 网页提现的KEY
        public static string YZCouponKey()
        {
            if (YZGameUtil.GetIsiOS())
            {
                return "WQaYKQD39ESBYInH";
            }
            else
            {
                return "WQaYKQD39ESBYInH";
            }
        }

        /// 判断iOS设备
        public static bool YZiOSModelVersion(int v)
        {
            string device = SystemInfo.deviceModel;
            if (string.IsNullOrEmpty(device))
            {
                return true;
            }
            else
            {
                string[] ds = device.Split(',');
                if (ds != null && ds.Length > 0)
                {
                    string model = ds[0].Replace("iPhone", "").Replace("iPad", "");
                    int im = model.ToInt();
                    YZDebug.LogConcat("device: ", device, " im: ", im);
                    if (im > 0)
                    {
                        return im >= v;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
        }
    }
}