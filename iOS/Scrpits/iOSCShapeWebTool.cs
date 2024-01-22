using UnityEngine;
using System.Runtime.InteropServices;
using Core.Controllers;
using Utils;

namespace iOSCShape
{
    public class iOSCShapeWebTool : YZBaseController<iOSCShapeWebTool>
    {

#if UNITY_IOS && !UNITY_EDITOR
     [DllImport("__Internal")] private static extern void ObjcShowWebViewInAppSafariUnity(string param);
     [DllImport("__Internal")] private static extern void ObjcShowWebViewSafariUnity(string param);
     [DllImport("__Internal")] private static extern void ObjcEvaluateJavaScriptUnity(string param);
     [DllImport("__Internal")] private static extern void ObjcShowWebViewInAppUnity(string param);
     [DllImport("__Internal")] private static extern void ObjcCloseWebViewUnity();
     [DllImport("__Internal")] private static extern void ObjcShowNewsVCUnity(string param);
#endif

        public WebViewCallBack webview_closed_callback;

        public WebViewCallBack webview_changed_callback;

        // 内嵌safari打开
        public void IOSYZShowWebViewInAppSafari(YZInAppSafariParams param)
        {
#if UNITY_IOS && !UNITY_EDITOR
         ObjcShowWebViewInAppSafariUnity(JsonUtility.ToJson(param));
#endif
        }

        // 内嵌webview打开
        public void IOSYZShowWebViewInApp(YZInAppWebViewParams param)
        {
#if UNITY_IOS && !UNITY_EDITOR
         ObjcShowWebViewInAppUnity(JsonUtility.ToJson(param));
#endif
        }

        // 内嵌webview关闭
        public void IOSYZCloseWebView()
        {
#if UNITY_IOS && !UNITY_EDITOR
         ObjcCloseWebViewUnity();
#endif
        }

        // 执行js代码，只有在展示内嵌webview才使用
        public void IOSYZEvaluateJavaScript(string js)
        {
#if UNITY_IOS && !UNITY_EDITOR
         ObjcEvaluateJavaScriptUnity(js);
#endif
        }

        // 打开news网页
        public void IOSYZShowNewsViewController(string json)
        {
#if UNITY_IOS && !UNITY_EDITOR
         ObjcShowNewsVCUnity(json);
#endif
        }

        // 外部safari浏览器打开
        public void IOSYZShowWebViewSafari(string url)
        {
#if UNITY_IOS && !UNITY_EDITOR
         ObjcShowWebViewSafariUnity(url);
#endif
        }

        // 内嵌safari关闭
        public void CShapeInAppSafariClosed(string msg)
        {
            YZDebug.Log("[Web]应用内Safari关闭了");
        }

        // 【回调】内嵌news发来消息
        public void CShapeNewsSendEvents(string msg)
        {
            YZDebug.LogConcat("[Web]收到News网页发来消息: ", msg);
        }

        // 【回调】内嵌webview关闭了
        public void CShapeWKUrlDidClosed(string msg)
        {
            YZDebug.Log("[Web]用户点击了原生页面的关闭按钮");
            webview_closed_callback?.Invoke(msg);
        }

        // 【回调】内嵌webview网址改变了
        public void CShapeWKUrlDidChanged(string msg)
        {
            YZDebug.LogConcat("[Web]Url发生了变化: ", msg);
        }

        // 【回调】内嵌webview加载状态改变了
        public void CShapeWKLoadDidChanged(string json)
        {
            YZLoadingStatus status = YZGameUtil.JsonYZToObject<YZLoadingStatus>(json);
            YZDebug.LogConcat("[Web]加载状态改变了, url: ", status.url, " loading: ", status.loading);
            webview_changed_callback?.Invoke(json);
        }

        // 【回调】内容webview打开app失败
        public void CShapeWKOpenAppFailed(string url)
        {
            YZDebug.LogConcat("[Web]打开app失败: ", url);
        }
    }
}