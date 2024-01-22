//using AndroidCShape;
using LitJson;
using System.Text;
using AndroidCShape;
using UnityEngine;

namespace Utils
{
    public class YZWebviewPanel : MonoBehaviour
    {
#if UNITY_ANDROID
        private RectTransform tran_webview;
        private YZAndroidWebViewPlugin plugin_webview;

        private int top, left, bottom, right;
        private string url_loading;
        private bool bWillShow;


        public WebViewCallBack webview_closed_callback;
        public WebViewCallBack webview_changed_callback;
        public WebViewCallBack webview_process_callback;


        private void Awake()
        {
            tran_webview = (RectTransform)transform;
            plugin_webview = new YZAndroidWebViewPlugin();
        }


        public void InitWebview(string url)
        {
            Location(tran_webview);
            if (!string.IsNullOrEmpty(url))
            {
                bWillShow = true;
                plugin_webview.InitWebView(url, left, top, right, bottom, OnWebViewCallBack);
            }
        }


        public void LoadFromURL(string url)
        {
            plugin_webview.LoadUrl(url);
        }

        public void CloseWebview()
        {
            bWillShow = false;
            plugin_webview.brback = null;
            plugin_webview.DestroyWebView();
        }

        public void EvaluateJavaScript(string script)
        {
            plugin_webview.EvaluateJavascript(script);
        }

        private void Location(RectTransform rectTran)
        {
            if (rectTran == null)
            {
                this.top = bottom = left = right = 0;
            }

            RectTransform parent_rect = (RectTransform)tran_webview.parent;

            float factorX;
            if (Mathf.Approximately(parent_rect.sizeDelta.x, 0))
            {
                factorX = Mathf.Approximately(parent_rect.rect.width, 0) ? 1 : Screen.width / parent_rect.rect.width;
            }
            else
            {
                factorX = Screen.width / parent_rect.sizeDelta.x;
            }

            float factorY;
            if (Mathf.Approximately(parent_rect.sizeDelta.y, 0))
            {
                factorY = Mathf.Approximately(parent_rect.rect.height, 0) ? 1 : Screen.height / parent_rect.rect.height;
            }
            else
            {
                factorY = Screen.height / parent_rect.sizeDelta.y;
            }

            // 
            top = Mathf.Abs(Mathf.FloorToInt(tran_webview.offsetMax.y * factorY));

            int StatusBarHeight = (int)Screen.safeArea.y;
            if (StatusBarHeight > 0 && top > 0)
            {
                top += StatusBarHeight;
            }

            bottom = Mathf.FloorToInt(tran_webview.offsetMin.y * factorY);
            left = Mathf.FloorToInt(tran_webview.offsetMin.x * factorX);
            right = Mathf.Abs(Mathf.FloorToInt(tran_webview.offsetMax.x * factorX));

            if (YZDefineUtil.IsDebugger)
            {
                StringBuilder builder = YZString.GetShareStringBuilder();
                builder.Append("WebView size info:\n");
                builder.Append("WebView top:").Append(top).Append("\n");
                builder.Append("WebView bottom:").Append(bottom).Append("\n");
                builder.Append("WebView left:").Append(left).Append("\n");
                builder.Append("WebView right:").Append(right).Append("\n");
                builder.Append("UniWebViewHelper.screenWidth:").Append(Screen.width).Append("\n");
                builder.Append("UniWebViewHelper.screenHeight:").Append(Screen.height).Append("\n");
                builder.Append("Screen.width:").Append(Screen.width).Append("\n");
                builder.Append("Screen.height:").Append(Screen.height).Append("\n");
                builder.Append("Screen.currentResolution.width:").Append(Screen.currentResolution.width).Append("\n");
                builder.Append("Screen.currentResolution.height:").Append(Screen.currentResolution.height).Append("\n");
                builder.Append("Screen.fullScreen:").Append(Screen.fullScreen).Append("\n");
                builder.Append("Screen.dpi:").Append(Screen.dpi).Append("\n");
                builder.Append("parent_rect.sizeDelta.x:").Append(parent_rect.sizeDelta.x).Append("\n");
                builder.Append("parent_rect.sizeDelta.y:").Append(parent_rect.sizeDelta.y).Append("\n");
                builder.Append("parent_rect.rect.width:").Append(parent_rect.rect.width).Append("\n");
                builder.Append("parent_rect.rect.height:").Append(parent_rect.rect.height).Append("\n");

                YZDebug.Log(builder.ToString());
            }
        }


        /// <summary>
        /// webview的回调
        /// </summary>
        /// <param name="code"></param>
        /// <param name="loadingUrl"></param>
        void OnWebViewCallBack(int code, string loadingUrl)
        {
            switch (code)
            {
                case -1: //error
                    break;
                case 2001: // load process
                {
                    webview_process_callback?.Invoke(loadingUrl);
                }
                    break;
                case 2003: // start loading
                {
                    JsonData json = new JsonData();
                    json["url"] = loadingUrl;
                    json["loading"] = 1;
                    //
                    webview_changed_callback?.Invoke(json.ToJson());
                }
                    break;
                case 2002: // finished loading
                {
                    JsonData json = new JsonData();
                    json["url"] = loadingUrl;
                    json["loading"] = 0;
                    //
                    webview_changed_callback?.Invoke(json.ToJson());
                }
                    break;
                case 2005: // close page
                    webview_closed_callback?.Invoke(loadingUrl);
                    break;
            }
        }
#endif
    }
}