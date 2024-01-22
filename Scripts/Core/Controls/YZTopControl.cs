using System;
using UnityEngine;
using Utils;

namespace Core.Controls
{
    public class YZTopControl : MonoBehaviour
    {
        // public UITopKYCController YZKYCController;
        //
        // public UITopAutoController YZHUDController;
        //
        // public UITopLoadController YZLoadController;
        //
        // public UITopTipsController YZTipsController;
        //
        // public UITopServerController YZServerController;
        //
        // public UITopNetworkErrorController YZNetworkErrorCtrler;

        //public UITopShowRewardUtils YZShowRewardCtrler;

        private static YZTopControl YZInstance;

        public static YZTopControl Shared
        {
            get { return YZInstance; }
        }

        private void Awake()
        {
            YZInstance = this;
        }

        // #region 提示框
        //
        // // 展示自动隐藏的提示 
        // public static void YZShowAutoHideTips(string tips, float time = 2)
        // {
        //     if (Shared != null)
        //     {
        //         Shared.YZHUDController.ShowUI(tips, time);
        //     }
        // }
        //
        // // 展示自动隐藏的提示 
        // public static void YZShowDebugAutoHideTips(string tips)
        // {
        //     if (YZDefineUtil.IsDebugger && Shared != null)
        //     {
        //         Shared.YZHUDController.ShowUI(tips, 6);
        //         YZDebug.Log(tips);
        //     }
        // }
        //
        // // 标题、详情、按钮
        // // is_hide 点击确认是否关闭提示框
        // // is_middle 文字是否居中，默认居中
        // // close_click 传空，则表示隐藏关闭按钮
        // public static void YZShowTips(string txt_title, string txt_detail, string txt_btn, Action close_click = null,
        //     Action btn_click = null, bool is_middle = true, bool is_hide = true)
        // {
        //     Shared.YZTipsController.ShowTips(txt_title, txt_detail, txt_btn, close_click, btn_click, is_middle,
        //         is_hide);
        // }
        //
        // #endregion

        // #region 登录相关
        //
        // public static void YZSetLoginProgress(float progress)
        // {
        //     Shared.YZLoadController.SetLoginProgress(progress);
        // }
        //
        // public static void YZStartRelogin()
        // {
        //     // 0.清除比赛记录
        //     YZRecordUICtrler.Shared().YZClearRecordList();
        //
        //     // 1.重新初始化变量
        //     GlobalVarManager.Shared.YZInitVars();
        //
        //     // 2.关闭所有界面
        //     YZMainControl.YZOnCloseAllController();
        //
        //     // 3.清除所有请求
        //     YZServerApiLogin.Shared.LogoutYZ();
        //
        //     // 4.打开加载界面
        //     Shared.YZLoadController.ShowUI();
        //
        //     // 5.开始重连
        //     YZServerApiLogin.Shared.YZUpgradeCheck();
        // }
        //
        // // 断网重连
        // public static void YZLostConnect()
        // {
        //     // 1.如果在游戏中，不弹断网
        //     if (YZServerApiMatchGame.Shared.YZCurrentMainStatus == MatchStatus.Gameing)
        //         return;
        //
        //     // 2.重新初始化变量
        //     GlobalVarManager.Shared.YZInitVars();
        //
        //     // 3.清除所有请求
        //     YZServerApiLogin.Shared.LogoutYZ();
        //
        //     // 4.打开断网弹框
        //     string txt_title = YZLocal.GetLocal(YZLocalID.key_lost_connection);
        //     string txt_tips = YZLocal.GetLocal(YZLocalID.key_lost_connection_desc);
        //     string txt_btn = YZLocal.GetLocal(YZLocalID.key_reconnect);
        //     YZShowTips(txt_title, txt_tips, txt_btn, null, () =>
        //     {
        //         // 关闭所有界面
        //         YZMainControl.YZOnCloseAllController();
        //         // 打开登录界面
        //         Shared.YZLoadController.ShowUI();
        //         // 开始检查更新
        //         YZServerApiLogin.Shared.YZUpgradeCheck();
        //     });
        // }
        //
        // // 维护退出
        // public static void YZMaintain()
        // {
        //     // 1.如果在游戏中，不弹维护
        //     if (YZServerApiMatchGame.Shared.YZCurrentMainStatus == MatchStatus.Gameing)
        //         return;
        //
        //     // 2.重新初始化变量
        //     GlobalVarManager.Shared.YZInitVars();
        //
        //     // 3.清除所有请求
        //     YZServerApiLogin.Shared.LogoutYZ();
        //
        //     // 4.关闭所有界面
        //     YZMainControl.YZOnCloseAllController();
        //
        //     // 5.打开登录界面
        //     Shared.YZLoadController.ShowUI();
        //
        //     // 6.开始检查更新
        //     YZServerApiLogin.Shared.YZUpgradeCheck();
        // }
        //
        // #endregion
        //
        // #region Debug弹框
        //
        // // 弹出选择服务器弹框
        // public static void YZShowSelectedServerTips()
        // {
        //     Shared.YZServerController.ShowUI();
        // }
        //
        // #endregion
        //
        // #region 网络错误重试弹窗
        //
        // // 断网重试
        // public static void YZRetryConnect(Action back)
        // {
        //     // 1.如果在游戏中，不弹断网
        //     if (YZServerApiMatchGame.Shared.YZCurrentMainStatus == MatchStatus.Gameing)
        //         return;
        //
        //     // 2.弹出重连
        //     if (YZServerApi.Shared.YZStatus >= LoginStatus.config)
        //     {
        //         Shared.YZNetworkErrorCtrler.OnRetryCallBack(back);
        //         Shared.YZNetworkErrorCtrler.ShowUI();
        //     }
        // }
        //
        // public static void YZReloginConnect()
        // {
        //     if (YZServerApi.Shared.YZStatus >= LoginStatus.config)
        //     {
        //         Shared.YZNetworkErrorCtrler.ShowUIRelogin();
        //     }
        // }
        //
        // #endregion
    }
}