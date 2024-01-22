using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AppsFlyerSDK;
using Core.Controllers;
using UnityEngine;
using Utils;

namespace AndroidCShape
{
#if UNITY_ANDROID
    public class YZAndroidAppsflyerPlugin : YZBaseController<YZAndroidAppsflyerPlugin>
    {
        public string AndroidGetAppsFlyerId()
        {
            YZDebug.Log("AppsFlyerId = " + AppsFlyer.getAppsFlyerId());
            return AppsFlyer.getAppsFlyerId();
        }

        public void AndroidSendAppsFlyerEvent(string name, Dictionary<string, object> value = null)
        {
            if (value != null)
            {
                Dictionary<string, string> sd = new Dictionary<string, string>(); 
                foreach (KeyValuePair<string, object> keyValuePair in value)
                {
                    sd.Add(keyValuePair.Key, keyValuePair.Value == null ? "" : keyValuePair.Value.ToString());
                }
                AppsFlyer.sendEvent(name, sd);
            }
            else
            {
                AppsFlyer.sendEvent(name, null);
            }
        }

        public void AndroidSendAppsFlyerValue(string name, string value)
        {
            AppsFlyer.sendEvent(name, new Dictionary<string, string>()
            {
                {AFInAppEvents.REVENUE, value},
                {AFInAppEvents.CURRENCY, "USD"}
            });
        }

        public void CShapeAppsflyerSuccess(string json)
        {
        }

        public void CShapeAppsflyerFailure(string error)
        {
            YZDebug.LogConcat("AF Fail: ", error);
        }
    }
#endif
}