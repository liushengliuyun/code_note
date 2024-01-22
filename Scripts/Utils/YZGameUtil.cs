using AndroidCShape;
using Core.Extensions;
using Core.Services.PersistService.API.Facade;
using DataAccess.Utils.Static;
using iOSCShape;

namespace Utils
{
    using System;
    using System.Text;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using UnityEngine;
    using UnityEngine.UI;
    using UnityEngine.Events;
    using UnityEngine.EventSystems;
    using DG.Tweening;
    using Spine.Unity;
    using System.Net;
    using System.Linq;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Text.RegularExpressions;
    using DataAccess.Utils.Static;

    public class YZPlatform
    {
        public const string Editor = "Editor";
        public const string Android = "Android";
        public const string iOS = "iOS";
        public const string Other = "Other";
    }

    public class YZGameUtil
    {
        public static void OnVibrateSliderChanged(float value, Toggle toggle)
        {
            PersistSystem.That.SaveValue(GlobalEnum.VIBRATION_VOLUME, value);
            
            if (value > GlobalEnum.MinPreSliderValue)
            {
                toggle.isOn = false;
            }
        }
        
        public static void PrintBuglyLog(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                BuglyAgent.PrintLog(LogSeverity.LogError, "{0}", str);
            }
        }

        /// <summary>
        /// 至少win环境下拿不到公网ip
        /// </summary>
        /// <param name="addressType"></param>
        /// <returns></returns>
        public static string GetIpAddress(IPAddressType addressType)
        {
            if (addressType == IPAddressType.IPv6 && !Socket.OSSupportsIPv6)
            {
                return null;
            }

            string output = string.Empty;

            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                NetworkInterfaceType _type1 = NetworkInterfaceType.Wireless80211;
                NetworkInterfaceType _type2 = NetworkInterfaceType.Ethernet;
                if ((item.NetworkInterfaceType == _type1 || item.NetworkInterfaceType == _type2) && item.OperationalStatus == OperationalStatus.Up)
#endif
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        //IPv4
                        if (addressType == IPAddressType.IPv4)
                        {
                            var ipAddress = ip.Address.ToString();
                            var ipType = item.NetworkInterfaceType;
                            YZLog.LogColor($"ipAddress  = {ipAddress}  , ipType = {ipType} ");
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork && !ipAddress.StartsWith("192"))
                            {
                                output = ip.Address.ToString();
                            }
                        }

                        //IPv6
                        else if (addressType == IPAddressType.IPv6)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetworkV6)
                            {
                                output = ip.Address.ToString();
                            }
                        }
                    }
                }
            }

            return output;
        }

        public static string GetIpAddressByAmazon()
        {
            string IP = string.Empty;
            try
            {
                //通过访问amazon获取本机ip数据  
                System.Net.WebClient client = new System.Net.WebClient();
                client.Encoding = System.Text.Encoding.Default;
                IP = client.DownloadString("http://checkip.amazonaws.com/");
                client.Dispose();
                IP = Regex.Replace(IP, @"[\r\n]", "");
                YZLog.LogColor($"IP amazonaws = {IP}");
            }
            catch (Exception) { }

            return IP;
        }

        public static void Vibrate()
        {
#if UNITY_ANDROID
                        //次数
            YZAndroidPlugin.Shared.TinyAndroidVibrate();
#elif UNITY_IOS
            iOSCShapeTool.Shared.IOSYZStartVibration("Heavy");
#endif
        }
        //public static string GetIPAddress(bool ipv4)
        //{
        //    IPAddress local_ip = null;
        //    try
        //    {
        //        if (ipv4)
        //        {
        //            IPAddress[] ips;
        //            ips = Dns.GetHostAddresses(Dns.GetHostName());
        //            local_ip = ips.First(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
        //            if (local_ip != null)
        //            {
        //                return local_ip.ToString();
        //            }
        //            else
        //            {
        //                return string.Empty;
        //            }
        //        }
        //        else
        //        {
        //            IPAddress[] ips;
        //            ips = Dns.GetHostAddresses(Dns.GetHostName());
        //            local_ip = ips.First(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6);
        //            if (local_ip != null)
        //            {
        //                return local_ip.ToString();
        //            }
        //            else
        //            {
        //                return string.Empty;
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        YZDebug.Log("获取本机ip失败");
        //    }

        //    return string.Empty;
        //}

        public static AndroidJavaObject Current()
        {
            if (Application.platform == RuntimePlatform.Android)
                return new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>(
                    "currentActivity");
            else
                return null;
        }

        #region UGUI

        public static Color HexColor(string hex)
        {
            hex = hex.Replace("0x", "");
            hex = hex.Replace("#", "");
            byte a = byte.MaxValue;
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            }

            return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
        }

        public static string GetPlatform()
        {
#if UNITY_EDITOR || NO_SDK
            return YZPlatform.Editor;
#elif UNITY_ANDROID
            return YZPlatform.Android;
#elif UNITY_IOS
            return YZPlatform.iOS;
#else
            return YZPlatform.Other;
#endif
        }

        public static bool GetIsAndroid()
        {
#if UNITY_ANDROID
            return true;
#endif
            return false;
        }

        public static bool GetIsiOS()
        {
#if UNITY_IOS
            return true;
#endif
            return false;
        }

        public static void ResetYZShader(SkeletonGraphic go)
        {
#if UNITY_EDITOR
            Material m = go.material;
            m.shader = Shader.Find("Spine/SkeletonGraphic");
#endif
        }

        public static void SetYZTextWithEllipsis(Text text_component, string value)
        {
            var brgeneratora = new TextGenerator();
            var brtransform = text_component.GetComponent<RectTransform>();
            var brssettings = text_component.GetGenerationSettings(brtransform.rect.size);
            brgeneratora.Populate(value, brssettings);

            var brcharactervisible = brgeneratora.characterCountVisible;
            var brPpdateText = value;
            if (value.Length > brcharactervisible)
            {
                brPpdateText = value.Substring(0, brcharactervisible - 1);
                brPpdateText += "...";
            }

            text_component.text = brPpdateText;
        }

        public static void SetYZRectTransformSize(RectTransform brtrans, Vector2 brsizes)
        {
            Vector2 broldessize = brtrans.rect.size;
            Vector2 brdeltasize = brsizes - broldessize;
            brtrans.offsetMin -= new Vector2(brdeltasize.x * brtrans.pivot.x, brdeltasize.y * brtrans.pivot.y);
            brtrans.offsetMax += new Vector2(brdeltasize.x * (1f - brtrans.pivot.x),
                brdeltasize.y * (1f - brtrans.pivot.y));
        }

        public static void AddYZEventTrigger(EventTriggerType type, GameObject go, UnityAction<BaseEventData> func)
        {
            EventTriggerType brtype = type;
            EventTrigger brtrigger = go.AddComponent<EventTrigger>();
            UnityAction<BaseEventData> action = new UnityAction<BaseEventData>(func);
            if (brtrigger.triggers.Count != 0)
            {
                for (int i = 0; i < brtrigger.triggers.Count; i++)
                {
                    if (brtrigger.triggers[i].eventID == brtype)
                    {
                        brtrigger.triggers[i].callback.AddListener(func);
                        return;
                    }
                }
            }

            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = brtype;
            entry.callback.AddListener(action);
            brtrigger.triggers.Add(entry);
        }

        public static string FixYZStringCode(string brtext)
        {
            if (string.IsNullOrEmpty(brtext))
            {
                return "";
            }

            return brtext.Replace(" ", " ");
        }

        public static string GetYZRandomString(int min, int max)
        {
            int count = UnityEngine.Random.Range(min, max + 1);
            StringBuilder stringBuilder = YZString.GetShareStringBuilder();
            for (int i = 0; i < count; i++)
            {
                int charIndex = UnityEngine.Random.Range(0, 36);
                if (charIndex < 10)
                {
                    stringBuilder.Append((char)(charIndex + 48));
                }
                else
                {
                    stringBuilder.Append((char)(charIndex - 10 + 97));
                }
            }

            return stringBuilder.ToString();
        }

        public static float GetYZClipLength(Animator animator, string clip)
        {
            if (null == animator || string.IsNullOrEmpty(clip) || null == animator.runtimeAnimatorController)
                return 0;
            RuntimeAnimatorController atbmac = animator.runtimeAnimatorController;
            AnimationClip[] YZAnimationClips = atbmac.animationClips;
            if (null == YZAnimationClips || YZAnimationClips.Length <= 0) return 0;
            AnimationClip atbmAnimationClip;
            for (int i = 0, tLen = YZAnimationClips.Length; i < tLen; i++)
            {
                atbmAnimationClip = atbmac.animationClips[i];
                if (null != atbmAnimationClip && atbmAnimationClip.name == clip)
                    return atbmAnimationClip.length;
            }

            return 0F;
        }

#endregion

#region 加解密

        public static string EncryptYZString(string text)
        {
            byte[] brbytes = Encoding.UTF8.GetBytes(text);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] outPut = md5.ComputeHash(brbytes);

            StringBuilder hashAtbmString = YZString.GetShareStringBuilder();
            for (int i = 0; i < outPut.Length; i++)
            {
                hashAtbmString.Append(Convert.ToString(outPut[i], 16).PadLeft(2, '0'));
            }

            return hashAtbmString.ToString();
        }

        public static string EncryptYZSHA1(string text)
        {
            byte[] brres = Encoding.Default.GetBytes(text);
            HashAlgorithm iSHA = new SHA1CryptoServiceProvider();
            brres = iSHA.ComputeHash(brres);
            StringBuilder Text = new StringBuilder();
            foreach (byte iByte in brres)
            {
                Text.AppendFormat("{0:x2}", iByte);
            }

            return Text.ToString();
        }

        public static string Base64YZDecode(string text)
        {
            byte[] brbt = Convert.FromBase64String(text);
            return Encoding.Default.GetString(brbt);
        }

        public static string Base64YZEncode(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                return Convert.ToBase64String(Encoding.Default.GetBytes(text));
            }

            return "";
        }

        public static string ParamYZEncodeString(string src)
        {
            byte[] brbyte = Convert.FromBase64String(src);
            List<int> temp = new List<int>();
            for (int i = 0; i < brbyte.Length; ++i)
            {
                temp.Add(Convert.ToInt32(brbyte[i]));
            }

            return string.Join(",", temp);
        }

        public static string YZEncrypt(string s)
        {
            if (s == null) return null;
            string result = "";
            s = Convert.ToBase64String(Encoding.Default.GetBytes(s));
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '=')
                {
                    result += "=";
                    continue;
                }

                int x = 0;
                if (c >= 'A' && c <= 'Z')
                {
                    x = c - 'A' + 26;
                }
                else if (c >= 'a' && c <= 'z')
                {
                    x = c - 'a';
                }
                else if (c >= '0' && c <= '9')
                {
                    x = c - '0' + 52;
                }
                else if (c == '+')
                {
                    x = 62;
                }
                else if (c == '/')
                {
                    x = 63;
                }

                int y = (x * 41 + 29) % 64;
                char d = '0';
                if (y < 26)
                {
                    d = (char)('a' + (char)y);
                }
                else if (y < 52)
                {
                    d = (char)('A' + y - 26);
                }
                else if (y < 62)
                {
                    d = (char)('0' + y - 52);
                }
                else if (y == 62)
                {
                    d = '+';
                }
                else if (y == 63)
                {
                    d = '/';
                }

                result += d.ToString();
            }

            result = result.Replace('=', '.');
            return result;
        }

        public static string YZDecrypt(string s)
        {
            if (s == null) return null;
            string result = "";
            s = s.Replace('.', '=');
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '=')
                {
                    result += "=";
                    continue;
                }

                int x = 0;
                if (c >= 'A' && c <= 'Z')
                {
                    x = c - 'A' + 26;
                }
                else if (c >= 'a' && c <= 'z')
                {
                    x = c - 'a';
                }
                else if (c >= '0' && c <= '9')
                {
                    x = c - '0' + 52;
                }
                else if (c == '+')
                {
                    x = 62;
                }
                else if (c == '/')
                {
                    x = 63;
                }

                int y = (x * 25 + 43) % 64;
                char d = '0';
                if (y < 26)
                {
                    d = (char)('a' + (char)y);
                }
                else if (y < 52)
                {
                    d = (char)('A' + y - 26);
                }
                else if (y < 62)
                {
                    d = (char)('0' + y - 52);
                }
                else if (y == 62)
                {
                    d = '+';
                }
                else if (y == 63)
                {
                    d = '/';
                }

                result += d.ToString();
            }

            result = Encoding.Default.GetString(Convert.FromBase64String(result));
            return result;
        }

        /// 检查信用卡号是否满足mod10
        public static bool CheckCreditCardNumberModTen(string card)
        {
            if (string.IsNullOrEmpty(card))
                return false;

            // 1. 将字符串转为一个数字数组
            int len = card.Length;
            int[] arr_numbers = new int[len];
            char[] arr_cardStr = card.ToCharArray();
            for (int i = 0; i < arr_cardStr.Length; ++i)
            {
                // 1.1 转为数字时，颠倒顺序，之后的处理，就可以方便的从左往右操作了。
                arr_numbers[len - 1 - i] = int.Parse(arr_cardStr[i].ToString());
            }

            // 2. 从右到左，所有奇数位数字和
            // 2. 从右到左，所有偶数位数字翻倍（如果翻倍之后大于等于10，再将十位与个位求和，得到一位数）之后，在求和
            int obb_sum = 0;
            int even_sum = 0;
            for (int i = 0; i < arr_numbers.Length; ++i)
            {
                if (i % 2 == 0)
                {
                    // 2.1 因为下标是从零开始的，所以，奇偶互换
                    obb_sum += arr_numbers[i];
                }
                else
                {
                    // 2.2 先翻倍
                    int tem = arr_numbers[i] * 2;
                    if (tem >= 10)
                    {
                        tem = tem / 10 + tem % 10;
                    }

                    even_sum += tem;
                }
            }

            // 3. 将前面得到的结果求和，然后对10求余，余数为0，则合法；否则，非法
            YZDebug.LogConcat("信用卡校验: obb_sum: ", obb_sum, " even_sum:", even_sum);
            int remainder = (obb_sum + even_sum) % 10;

            return remainder == 0;
        }

#endregion

#region Dotween

        /// 按钮防重复点击
        public static void PreventYZMultipleClicks(Button button, float dealy = 1.5f)
        {
            button.interactable = false;
            Sequence brsequence = DOTween.Sequence();
            brsequence.SetUpdate(true);
            brsequence.AppendCallback(() =>
            {
                if (button != null)
                {
                    button.interactable = true;
                }
            });
            brsequence.SetDelay(dealy);
            brsequence.PlayForward();
        }

        public static void DelayYZAction(TweenCallback action, float delay, bool up = true)
        {
            if (delay <= 0)
            {
                action?.Invoke();
                return;
            }

            Sequence brsequence = DOTween.Sequence();
            brsequence.SetUpdate(up);
            brsequence.AppendCallback(action);
            brsequence.SetDelay(delay);
            brsequence.PlayForward();
        }

#endregion

#region Json

        public static T JsonYZToObject<T>(string json)
        {
            try
            {
                return LitJson.JsonMapper.ToObject<T>(json);
            }
            catch
            {
                // TODO
                //YZTopControl.YZShowDebugAutoHideTips(YZString.Concat("json数据解析失败: ", json));
                return default;
            }
        }

#endregion

        public static void PauseYZGame()
        {
            Time.timeScale = 0;
        }

        public static void StartYZGame()
        {
            Time.timeScale = 1;
        }
    }
}