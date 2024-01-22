using System;
using System.Runtime.InteropServices;
using Core.Controllers;
using UnityEngine;
using Utils;

namespace iOSCShape
{
    public class iOSCShapePushTool : YZBaseController<iOSCShapePushTool>
    {
        [HideInInspector]
        public string YZTags;

        [HideInInspector]
        public bool YZInited = false;

        [HideInInspector]
        public bool YZIsRequestPushAuth = false;

#if UNITY_IOS && !UNITY_EDITOR
     [DllImport("__Internal")] private static extern void ObjcInitPushUnity();
     [DllImport("__Internal")] private static extern void ObjcSetTagsUnity(string json);
     [DllImport("__Internal")] private static extern string ObjcGetPushAuthUnity();
#endif

//        public void IOSYZInitPush()
//        {
//            if (YZDefineUtil.IsSpecials)
//            {
//#if UNITY_IOS && !UNITY_EDITOR
//             ObjcInitPushUnity();
//#endif
//            }
//        }

        public void IOSYZSetTags(string json)
        {
            YZTags = json;
        }

//        public void IOSYZSetTags(string json)
//        {
//#if UNITY_IOS && !UNITY_EDITOR
//             ObjcSetTagsUnity(json);
//#endif
//        }

        // 0.未做决定 1.已拒绝 2.已同意 3.临时通知 4.每天8小时
        //        public string IOSYZGetPushAuth()
        //        {
        //#if UNITY_IOS && !UNITY_EDITOR
        //             return ObjcGetPushAuthUnity();
        //#endif
        //            return "2";
        //        }

        // 推送权限返回
        //public void CShapePushAuth(string msg)
        //{
        //    string status = YZString.Concat(PlayerPrefs.GetInt(YZConstUtil.YZPushStatusInt, 0));
        //    if (status != "0" && status != "1" && msg == "1")
        //    {
        //        YZDebug.Log("已经同意了推送，修改为了拒绝");
        //        YZFunnelUtil.SendPush(FunnelEventID.brpushclose);
        //    }
        //    PlayerPrefs.SetInt(YZConstUtil.YZPushStatusInt, int.Parse(msg));
        //    if (YZIsRequestPushAuth)
        //    {
        //        YZIsRequestPushAuth = false;
        //        YZFunnelUtil.SendPush(FunnelEventID.brpushsys, -1, msg == "1");
        //    }
        //}

        // 申请推送权限
        //public void IOSYZRequestPushAuth(int type, int id, Action back)
        //{
        //    int num = PlayerPrefs.GetInt(YZConstUtil.YZPushDayTimesInt, 0);
        //    if (num > 0)
        //    {
        //        YZDebug.Log("推送权限请求今日已弹，不再弹出了");
        //        back?.Invoke();
        //        return;
        //    }
        //    PlayerPrefs.SetInt(YZConstUtil.YZPushDayTimesInt, num + 1);

        //    string status = IOSYZGetPushAuth();
        //    if (status != "0" && status != "1")
        //    {
        //        YZDebug.Log("推送权限已同意");
        //        IOSYZInitOrSendTags();
        //        back?.Invoke();
        //        return;
        //    }

        //    if (status == "0")
        //    {
        //        // 未决定，确定后再初始化推送
        //        YZIsRequestPushAuth = true;
        //        YZFunnelUtil.SendPush(FunnelEventID.brpushpopup, id);
        //        string txt_title = YZLocal.GetLocal(YZLocalID.key_notive);
        //        string txt_tips = YZLocal.GetLocal(type == 1 ? YZLocalID.key_push_request_event_desc : YZLocalID.key_push_request_desc);
        //        string txt_btn = YZLocal.GetLocal(YZLocalID.key_allow);
        //        YZTopControl.YZShowTips(txt_title, txt_tips, txt_btn, () =>
        //        {
        //            YZDebug.Log("点击拒绝推送");
        //            YZFunnelUtil.SendPush(FunnelEventID.brpushgame, id, true);
        //            back?.Invoke();
        //        }, () =>
        //        {
        //            IOSYZInitOrSendTags();
        //            YZFunnelUtil.SendPush(FunnelEventID.brpushgame, id, false);
        //            back?.Invoke();
        //        });
        //    }
        //    else
        //    {
        //        // 已拒绝，直接初始化推送
        //        string txt_title = YZLocal.GetLocal(YZLocalID.key_notive);
        //        string txt_tips = YZLocal.GetLocal(YZLocalID.key_push_guide_desc);
        //        string txt_btn = YZLocal.GetLocal(YZLocalID.key_GO);
        //        YZTopControl.YZShowTips(txt_title, txt_tips, txt_btn, () =>
        //        {
        //            YZDebug.Log("点击拒绝去设置中心修改推送权限");
        //            YZFunnelUtil.SendPush(FunnelEventID.brpushopenguide, id, true);
        //            back?.Invoke();
        //        }, () =>
        //        {
        //            iOSCShapeTool.Shared.IOSYZGotoSetting();
        //            YZFunnelUtil.SendPush(FunnelEventID.brpushopenguide, id, false);
        //            back?.Invoke();
        //        });
        //        IOSYZInitOrSendTags();
        //    }
        //}

        // 初始化或者上传Tags，iOS只有专机才会初始化
        //public void IOSYZInitOrSendTags()
        //{
        //    if (YZDefineUtil.IsSpecials)
        //    {
        //        if (!YZInited)
        //        {
        //            YZInited = true;
        //            IOSYZInitPush();
        //        }
        //        if (YZTags != null)
        //        {
        //            IOSYZSetTags(YZTags);
        //            YZTags = null;
        //        }
        //    }
        //}


    }
}
