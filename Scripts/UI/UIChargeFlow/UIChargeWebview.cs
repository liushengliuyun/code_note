using Core.Extensions;
using Core.Server;
using Core.Services.UserInterfaceService.Internal;
using UnityEngine;
using Utils;
using UnityEngine.UI;

namespace UI.UIChargeFlow
{
    public class UIChargeWebview : UIBase<UIChargeWebview>
    {
        
        #region 需要控制的子控件
        [SerializeField] private Button brbtnback;
        [SerializeField] private Text brtxttitle;
        [SerializeField] public YZWebviewPanel panel_webview;
        [SerializeField] private Image brimg_fillbar;
        #endregion

        #region 私有变量
        private string current_url;
        #endregion
        
        public WebViewCallBack webview_closed_callback;
        public WebViewCallBack webview_changed_callback;
        private static UIChargeWebview Inst;
        public static UIChargeWebview Shared()
        {
            return Inst;
        }
        public override void InitEvents()
        {
        }

        public void Awake()
        {
            base.Awake();
            Inst = this;
        }

        public override void OnStart()
        {
            current_url = args.Length > 0 ? (string)args[0] : string.Empty;

            if (current_url.IsNullOrEmpty())
            {
                Close();
                return;
            }
            
            brbtnback.onClick.AddListener(()=>
            {
                // 用户手动关闭
                YZServerApiCharge.Shared.SendWebViewEnd();
                Close();
            });
            
            brimg_fillbar.fillAmount = 0;
            
            if (panel_webview == null)
                return;

#if UNITY_ANDROID
            panel_webview.webview_closed_callback = webview_closed_callback;
            panel_webview.webview_changed_callback = webview_changed_callback;
            panel_webview.webview_process_callback = UpdateProcessFillBar;
            panel_webview.InitWebview(current_url);
#endif
            
        }

        private void UpdateProcessFillBar(string process)
        {
            var percent = int.Parse(process);
            brimg_fillbar.fillAmount = percent / 100f;
        }

        public override void InitVm()
        {
        }

        public override void InitBinds()
        {
        }

        public override void Close()
        {
#if UNITY_ANDROID
            panel_webview.CloseWebview();
#endif
            base.Close();
        }
    }
}