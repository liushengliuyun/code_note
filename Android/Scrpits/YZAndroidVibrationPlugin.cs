using Core.Controllers;
using UnityEngine;

namespace AndroidCShape
{
#if (UNITY_ANDROID && !NO_SDK) || UNITY_EDITOR
    public class YZAndroidVibrationPlugin : YZBaseController<YZAndroidVibrationPlugin>
    {
        public static AndroidJavaClass unityPlayer;
        public static AndroidJavaObject currentActivity;
        public static AndroidJavaObject vibrator;
        public static AndroidJavaClass vibrationEffect;

        public override void InitController()
        {
            base.InitController();

            if (Application.isMobilePlatform)
            {
                unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");

                if (YZAndroidPlugin.Shared.AndroidVersion >= 26)
                {
                    vibrationEffect = new AndroidJavaClass("android.os.VibrationEffect");
                }
            }
        }

        public void YZVibrate(long milliseconds)
        {
            if (Application.isMobilePlatform)
            {
                if (YZAndroidPlugin.Shared.AndroidVersion >= 26)
                {
                    AndroidJavaObject createOneShot =
                        vibrationEffect.CallStatic<AndroidJavaObject>("createOneShot", milliseconds, -1);
                    vibrator.Call("vibrate", createOneShot);
                }
                else
                {
                    vibrator.Call("vibrate", milliseconds);
                }
            }
        }
    }
#endif
}