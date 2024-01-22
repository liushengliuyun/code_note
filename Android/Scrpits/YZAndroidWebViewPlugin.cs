using System;
using UnityEngine;
using Utils;

namespace AndroidCShape
{
#if UNITY_ANDROID || UNITY_EDITOR
    public class YZAndroidWebViewPlugin : AndroidJavaProxy
    {
        private readonly AndroidJavaClass brwebviewplugin;

        public Action<int, string> brback;

        public YZAndroidWebViewPlugin() : base("com.bingo.win.WebViewCallback")
        {
            brwebviewplugin = new AndroidJavaClass("com.bingo.win.WebViewPlugin");
        }

        /// <summary>
        /// 初始化网页
        /// </summary>
        /// <param name="m_back"> -1.网页加载错误，2000.网页标题改变 2001.网页进度改变 2002.网页加载完成 2003.网页开始加载 2004.开始加载网址 2005.关闭网页</param>
        public void InitWebView(string url, int left, int top, int right, int bottom, Action<int, string> m_back)
        {
            brback = m_back;
            brwebviewplugin.CallStatic("initWebView", url, left, top, right, bottom, this);
        }

        public void LoadUrl(string url)
        {
            brwebviewplugin.CallStatic("loadUrl", url);
        }

        public void Reload()
        {
            brwebviewplugin.CallStatic("reload");
        }

        public void EvaluateJavascript(string js)
        {
            brwebviewplugin.CallStatic("evaluateJavascript", js);
        }

        public void DestroyWebView()
        {
            brwebviewplugin.CallStatic("destroyWebView");
        }

        public void onError(int code, string error)
        {
            brback?.Invoke(-1, error);
        }

        public void onTitleChange(string title)
        {
            brback?.Invoke(2000, title);
        }

        public void onProgressChange(int progress)
        {
            brback?.Invoke(2001, YZString.Concat(progress));
        }

        public void onPageFinished(string url)
        {
            brback?.Invoke(2002, url);
        }

        public void onPageStarted(string url)
        {
            brback?.Invoke(2003, url);
        }

        public void onUrlLoading(string url)
        {
            brback?.Invoke(2004, url);
        }

        public void onClose()
        {
            brback?.Invoke(2005, "");
        }
    }
#endif
}