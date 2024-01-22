using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Core.Controllers;
using Core.Extensions;
using Core.MyAttribute;
using Core.Server;
using Utils;

namespace iOSCShape
{
    public class iOSCShapeAppsflyerTool : YZBaseController<iOSCShapeAppsflyerTool>
    {
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void ObjcStartAppsFlyerUnity();
    [DllImport("__Internal")] private static extern void ObjcSendAppsFlyerEventUnity(string param);
    [DllImport("__Internal")] private static extern void ObjcSendAppsFlyerValueUnity(string param);
    [DllImport("__Internal")] private static extern string ObjcGetAppsFlyerIdUnity();
#endif

        public void IOSStartAppsFlyer()
        {
#if UNITY_IOS && !UNITY_EDITOR
        ObjcStartAppsFlyerUnity();
#endif
        }

        public string IOSGetAppsFlyerId()
        {
#if UNITY_IOS && !UNITY_EDITOR
        return ObjcGetAppsFlyerIdUnity();
#endif
            return "";
        }

        public void IOSSendAppsFlyerEvent(string json)
        {
#if UNITY_IOS && !UNITY_EDITOR
        ObjcSendAppsFlyerEventUnity(json);
#endif
        }

        public void IOSSendAppsFlyerValue(string json)
        {
#if UNITY_IOS && !UNITY_EDITOR
        ObjcSendAppsFlyerValueUnity(json);
#endif
        }

        [CallByIOS]
        public void CShapeAppsflyerSuccess(string json)
        {
            YZLog.LogColor("iOS解析Appsflyer成功");
            YZDebug.LogConcat("AF Succ: ", json);
            YZAppsflyer m = YZGameUtil.JsonYZToObject<YZAppsflyer>(json);
            YZServerApiOrganic.Shared.TryToOrganic(1, m.status, m.source, m.invite);
        }

        public void CShapeAppsflyerSuccessJson(string json)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add(FunnelEventID.brmediasource, json);
            YZFunnelUtil.UserYZSetOnce(dict);
        }

        public void CShapeAppsflyerFailure(string error)
        {
            YZDebug.LogConcat("AF Fail: ", error);
        }
    }
}
