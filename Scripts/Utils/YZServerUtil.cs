using AndroidCShape;
using UnityEngine;
using DataAccess.Model;
using DataAccess.Utils;

namespace Utils
{
    public class YZServerUtil
    {
        /// 当前服务器
        private static int GetServerType()
        {
            if (YZDefineUtil.IsDebugger)
            {
                return PlayerPrefs.GetInt(YZConstUtil.YZServerType, 0);
            }
            else
            {
                return ServerType.Release;
            }
        }

        /// 是否需要上传价值
        public static bool GetNeedSendValue()
        {
            return GetServerType() == ServerType.Release;
        }

        /// 获取打点前缀
        public static string GetYZThinkPreName()
        {
            return GetServerType() == ServerType.Release ? "" : YZDefineUtil.PreName;
        }

        /// 服务器 url
        public static string GetYZServerURL()
        {
            if (YZGameUtil.GetIsiOS())
            {
                if (GetServerType() == ServerType.Backup)
                {
                    // iOS备份服
                    return "http://192.168.10.24:2001/api/";
                }
                else if (GetServerType() == ServerType.Release)
                {
                    // iOS正式服
                    return "https://app.fishbear.club/api/";
                }
                else
                {
                    // iOS测试服
                    return YZGameUtil.Base64YZDecode("aHR0cDovL2JpbmdvLXJhaWRlci5sdWNrZnVuLnZpcC9hcGkv");
                }
            }
            else
            {
                if (GetServerType() == ServerType.Backup)
                {
                    // 安卓备份服
                    return "";
                }
                else if (GetServerType() == ServerType.Release)
                {
                    // 安卓正式服
                    return "https://app-bingo-gp.yatzybrawl.com/api/";
                }
                else
                {
                    // 安卓测试服
                    return "https://app-bingo-gp.yatzybrawl.com/api/";
                }
            }
        }

        /// 支持中心 url
        public static string GetYZSupportCenterURL()
        {
            if (YZGameUtil.GetIsiOS())
            {
                if (GetServerType() == ServerType.Backup)
                {
                    // iOS备份服
                    return "https://supportcenter-test.winnerstudio.vip/";
                }
                else if (GetServerType() == ServerType.Release)
                {
                    // iOS正式服
                    return "https://supportcenter.winnerstudio.vip/";
                }
                else
                {
                    // iOS测试服
                    return "https://supportcenter-test.winnerstudio.vip/";
                }
            }
            else
            {
                if (GetServerType() == ServerType.Backup)
                {
                    // 安卓备份服
                    return "https://supportcenter-test.winnerstudio.vip/";
                }
                else if (GetServerType() == ServerType.Release)
                {
                    // 安卓正式服
                    return "https://supportcenter-test.winnerstudio.vip/";
                }
                else
                {
                    // 安卓测试服
                    return "https://supportcenter-test.winnerstudio.vip/";
                }
            }
        }

        /// 支付中心 app_key
        public static string GetYZChargeCenterAppKey()
        {
            string appKey = YZGameUtil.Base64YZDecode(Root.Instance.ChargeConfig.app_key);
            return appKey;
            //if (YZGameUtil.GetIsiOS())
            //{
            //    if (GetServerType() == ServerType.Test)
            //    {
            //        // iOS测试服
            //        return "EXiuq6IvBiqo3GjX";
            //    }
            //    else if (GetServerType() == ServerType.Release)
            //    {
            //        // iOS正式服
            //        return "FOkwU63ZIriyzRZE";
            //    }
            //    else
            //    {
            //        // iOS备份服
            //        return "z0LoI5DxmxeXYx20";
            //    }
            //}
            //else
            //{
            //    if (GetServerType() == ServerType.Test)
            //    {
            //        // 安卓测试服
            //        return "KJYItIjzvwVCC1RX";
            //    }
            //    else if (GetServerType() == ServerType.Release)
            //    {
            //        // 安卓正式服
            //        return "SCr2ic9amAcJ4Iuw";
            //    }
            //    else
            //    {
            //        // 安卓备份服
            //        return "KJYItIjzvwVCC1RX";
            //    }
            //}
        }

        /// 支付中心 app_secret
        public static string GetYZChargeCenterAppSecret()
        {
            string secretKey = YZGameUtil.Base64YZDecode(Root.Instance.ChargeConfig.secret_key);
            return secretKey;
            //if (YZGameUtil.GetIsiOS())
            //{
            //    if (GetServerType() == ServerType.Test)
            //    {
            //        // iOS测试服
            //        return "joorltE5aaVMb3UoOjUnKbEvONG27s9y";
            //    }
            //    else if (GetServerType() == ServerType.Release)
            //    {
            //        // iOS正式服
            //        return "D2YkmPUY0mp2A5BuIgbQrs2Qg5qQiMdt";
            //    }
            //    else
            //    {
            //        // iOS备份服
            //        return "ngpcgLXLADPGi94nUCeIo8MQ9M74zfYv";
            //    }
            //}
            //else
            //{
            //    if (GetServerType() == ServerType.Test)
            //    {
            //        // 安卓测试服
            //        return "UEHjicEfQ96bYLjNmzrTNZdRFxjMlOCY";
            //    }
            //    else if (GetServerType() == ServerType.Release)
            //    {
            //        // 安卓正式服
            //        return "C5MneWl5HnoGNfhimj9iTiB6QoOghwer";
            //    }
            //    else
            //    {
            //        // 安卓备份服
            //        return "UEHjicEfQ96bYLjNmzrTNZdRFxjMlOCY";
            //    }
            //}
        }

        /// 支持中心 app_key
        public static string GetYZSupportCenterAppKey()
        {
            if (YZGameUtil.GetIsiOS())
            {
                if (GetServerType() == ServerType.Test)
                {
                    // iOS测试服
                    return "p6vmUFRjkaOn082J";
                }
                else if (GetServerType() == ServerType.Release)
                {
                    // iOS正式服
                    return "KqpT62CzqqbySc72";
                }
                else
                {
                    // iOS备份服
                    return "p6vmUFRjkaOn082J";
                }
            }
            else
            {
                if (GetServerType() == ServerType.Test)
                {
                    // 安卓测试服
                    return "p6vmUFRjkaOn082J";
                }
                else if (GetServerType() == ServerType.Release)
                {
                    // 安卓正式服
                    return "SCr2ic9amAcJ4Iuw";
                }
                else
                {
                    // 安卓备份服
                    return "p6vmUFRjkaOn082J";
                }
            }
        }

        /// 支持中心 app_secret
        public static string GetYZSupportCenterAppSecret()
        {
            if (YZGameUtil.GetIsiOS())
            {
                if (GetServerType() == ServerType.Test)
                {
                    // iOS测试服
                    return "C4koTTEVoofAyviFJ8QHdoWrxcYTBlrK";
                }
                else if (GetServerType() == ServerType.Release)
                {
                    // iOS正式服
                    return "BPTqSvjGRQqRvZ4RBFSfgCYkknbfDwuf";
                }
                else
                {
                    // iOS备份服
                    return "fStE47oBMXRpAaTomlJ444hLX0mWjmPa";
                }
            }
            else
            {
                if (GetServerType() == ServerType.Test)
                {
                    // 安卓测试服
                    return "cOUVC3uDZGPUH38m06HBrbAWdPdeUYPy";
                }
                else if (GetServerType() == ServerType.Release)
                {
                    // 安卓正式服
                    return "KopanRTztnnVvJ2FWJ2B5TdqpF5Nx88C";
                }
                else
                {
                    // 安卓备份服
                    return "fStE47oBMXRpAaTomlJ444hLX0mWjmPa";
                }
            }
        }

        /// 获取数数id
        public static string GetYZThinkAppid()
        {
            if (GetServerType() == ServerType.Release)
            {
                // 正式服
                return "7f98ac7894c543bda75b49abd933b561";
            }
            else if (GetServerType() == ServerType.Test)
            {
                // 测试服
                return "60b68a02530f49a682bc21c46d88eeac";
            }
            else
            {
                // 备份服
                return "60b68a02530f49a682bc21c46d88eeac";
            }
        }

        /// 获取数数初始化类型(iOS专用)
        public static string GetYZThinkTypeiOS()
        {
            return GetServerType() == ServerType.Release ? "2" : "1";
        }

        public static string GetYZUserAgent()
        {
#if UNITY_ANDROID
           return YZAndroidPlugin.Shared.AndroidGetUserAgent(); 
#endif
            return DeviceInfoUtils.Instance.GetUserAgent();
        }

        public static string GetYZOperatingSystem()
        {
            return DeviceInfoUtils.Instance.GetOperatingSystem();
        }

        #region Debug 专用

        /// 是否完成了选中服务器
        public static bool GetDidSelectedServer()
        {
            return GetServerType() > 0;
        }

        /// 获取服务器名称
        public static string GetYZServerName()
        {
            return GetServerType() == ServerType.Test ? "测试服" : GetServerType() == ServerType.Backup ? "备份服" : "正式服";
        }

        #endregion
    }
}