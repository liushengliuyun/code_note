using System.Runtime.InteropServices;
using Core.Controllers;
using Utils;

namespace iOSCShape
{
    public class iOSCShapeThinkTool : YZBaseController<iOSCShapeThinkTool>
    {
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void ObjcFlushDatasUnity();
    [DllImport("__Internal")] private static extern void ObjcCalibrateTimeUnity(string param);
    [DllImport("__Internal")] private static extern void ObjcInitThinkUnity(string param);
    [DllImport("__Internal")] private static extern void ObjcLoginThinkUnity(string param);
    [DllImport("__Internal")] private static extern void ObjcStartAutoEventUnity(string param);
    [DllImport("__Internal")] private static extern void ObjcSetGameRoundsUnity(string param);
    [DllImport("__Internal")] private static extern void ObjcThinkUserSetUnity(string param);
    [DllImport("__Internal")] private static extern void ObjcThinkUserSetOnceUnity(string param);
    [DllImport("__Internal")] private static extern void ObjcThinkTrackUnity(string param);
    [DllImport("__Internal")] private static extern string ObjcGetDistinctIDUnity();
#endif

        public string IOSGetDistinctID()
        {
#if UNITY_IOS && !UNITY_EDITOR
        return ObjcGetDistinctIDUnity();
#endif
            return "Unity editor";
        }

        public void IOSCalibrateTime(string name)
        {
#if UNITY_IOS && !UNITY_EDITOR
        ObjcCalibrateTimeUnity(name);
#endif
        }

        public void IOSInitThink()
        {
#if UNITY_IOS && !UNITY_EDITOR
        ObjcInitThinkUnity(YZServerUtil.GetYZThinkTypeiOS());
#endif
        }

        public void IOSLoginThink(string name)
        {
#if UNITY_IOS && !UNITY_EDITOR
        ObjcLoginThinkUnity(name);
#endif
        }

        public void IOSStartAutoEvent(string name)
        {
#if UNITY_IOS && !UNITY_EDITOR
        ObjcStartAutoEventUnity(name);
#endif
        }

        public void IOSSetGameRounds(string name)
        {
#if UNITY_IOS && !UNITY_EDITOR
        ObjcSetGameRoundsUnity(name);
#endif
        }

        public void IOSThinkUserSet(string name)
        {
#if UNITY_IOS && !UNITY_EDITOR
        ObjcThinkUserSetUnity(name);
#endif
        }

        public void IOSThinkUserSetOnce(string name)
        {
#if UNITY_IOS && !UNITY_EDITOR
        ObjcThinkUserSetOnceUnity(name);
#endif
        }

        public void IOSThinkTrack(string name)
        {
#if UNITY_IOS && !UNITY_EDITOR
        ObjcThinkTrackUnity(name);
#endif
        }

        public void IOSFlushDatas()
        {
#if UNITY_IOS && !UNITY_EDITOR
        ObjcFlushDatasUnity();
#endif
        }
    }
}
