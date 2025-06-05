package com.liusheng.x;

import com.unity3d.player.UnityPlayer;

public class XUnityAgent {
    public static String MessageReceiver = "MyFramework";
//    public static final String Method_NetStateChange = "OnNetChanged";
    public static final String Method_GetGAID = "OnGetAdId";

    public static void ExecuteUnityMethod(String methodName, String content) {
        UnityPlayer.UnitySendMessage(MessageReceiver, methodName, content);
    }

    public static void SendEmail(String myEmail, String title, String mailContent, String choose) {
        XRuntimeActivity.Instance.SendEmail(myEmail, title, mailContent, choose);
    }

//    public static void Vibrate(long[] pattern, int[]amplitudes, int index)
//    {
//        SodaRuntimeActivity.Instance.Vibrate(pattern, amplitudes, index);
//    }

    public static void TryGetAdId() {
        XRuntimeActivity.Instance.TryGetGoogleAdId();
    }

    public static void HideSplash() {
        XRuntimeActivity.Instance.HideSplash();
    }

}
