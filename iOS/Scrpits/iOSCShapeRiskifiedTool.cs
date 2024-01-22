using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Core.Controllers;

namespace iOSCShape
{
    public class iOSCShapeRiskifiedTool : YZBaseController<iOSCShapeRiskifiedTool>
    {
        private bool isStarted = false;
#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void ObjcRiskifiedStartBeaconUnity(string account, string token, bool debug);
        [DllImport("__Internal")] private static extern void ObjcRiskifiedUpdateSessionTokenUnity(string token);
        [DllImport("__Internal")] private static extern void ObjcRiskifiedLogRequestUnity(string eventName);
#endif

        public void IOSRiskifiedStartBeacon(string account, string token, bool debug)
        {
            if (isStarted)
            {
                return;
            }
            isStarted = true;
#if UNITY_IOS && !UNITY_EDITOR
            Debug.Log("wjs riskified start beacon : " + account);
            ObjcRiskifiedStartBeaconUnity(account, token, debug);
#endif
        }

        public void IOSRiskifiedUpdateSessionToken(string token)
        {
#if UNITY_IOS && !UNITY_EDITOR
            Debug.Log("wjs riskified update session token : " + token);
            ObjcRiskifiedUpdateSessionTokenUnity(token);
#endif
        }

        public void IOSRiskifiedLogRequest(string eventName)
        {
#if UNITY_IOS && !UNITY_EDITOR
            string url = string.Format("/{0}", eventName);
            Debug.Log("wjs riskified log request : " + url);
            ObjcRiskifiedLogRequestUnity(url);
#endif
        }








    }










}




