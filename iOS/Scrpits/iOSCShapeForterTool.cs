using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Core.Controllers;

namespace iOSCShape
{
    public class iOSCShapeForterTool : YZBaseController<iOSCShapeForterTool>
    {
        private bool isStarted = false;
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void ObjcForterSetupWithSiteIdUnity(string param);
        [DllImport("__Internal")] private static extern void ObjcForterSetDeviceUniqueIdentifierUnity();
        [DllImport("__Internal")] private static extern void ObjcForterTrackActionUnity(string param1, string param2);
#endif

        public void IOSForterSetupWithSiteId(string param)
        {
            if (isStarted)
            {
                return;
            }
            isStarted = true;
#if UNITY_IOS && !UNITY_EDITOR
            Debug.Log("wjs forter setup with siteid : " + param);
            ObjcForterSetupWithSiteIdUnity(param);
#endif
        }

        public void IOSForterSetDeviceUniqueIdentifier(string param)
        {
            if (isStarted)
            {
                return;
            }
#if UNITY_IOS && !UNITY_EDITOR
            Debug.Log("wjs forter set device unique identifier : " + param);
            ObjcForterSetDeviceUniqueIdentifierUnity();
#endif
        }

        public void IOSForterTrackAction(string eventName, string eventParams)
        {
#if UNITY_IOS && !UNITY_EDITOR
            Debug.Log("wjs forter track action : " + eventName);
            ObjcForterTrackActionUnity(eventName, eventParams);
#endif
        }








    }



    






}



