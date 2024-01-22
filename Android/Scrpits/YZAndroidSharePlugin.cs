using Core.Controllers;
using UnityEngine;

namespace AndroidCShape
{
#if UNITY_ANDROID || UNITY_EDITOR
    public class YZAndroidSharePlugin : YZBaseController<YZAndroidSharePlugin>
    {
        public static void AndroidSetPasteBoardText(string text)
        {
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            WudiAttribution.CallStatic("SetPasteBoardText", text);
        }

        public static void AndroidShareToWhatsApp(string param)
        {
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            WudiAttribution.CallStatic("SendToWhatsapp", param);
        }

        public static void AndroidShareToMessenger(string param)
        {
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            WudiAttribution.CallStatic("SendToMessenger", param);
        }

        public static void AndroidShareToMessage(string param)
        {
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            WudiAttribution.CallStatic("SendToSms", param);
        }

        public static void AndroidShareToDefault(string param, string share)
        {
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            WudiAttribution.CallStatic("SendToDefault", param, share);
        }
    }
#endif
}