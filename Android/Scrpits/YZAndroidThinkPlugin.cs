using System.Collections;
using System.Collections.Generic;
using Core.Controllers;
using ThinkingAnalytics;
using UnityEngine;

namespace AndroidCShape
{
#if UNITY_ANDROID || UNITY_EDITOR
    public class YZAndroidThinkPlugin : YZBaseController<YZAndroidThinkPlugin>
    {
        public string AndroidGetDistinctID()
        {
            return "Unity editor";
        }

        public void AndroidCalibrateTime(string name)
        {
        }

        public void AndroidInitThink(string name)
        {
        }

        public void AndroidLoginThink(string name)
        {
        }

        public void AndroidStartAutoEvent(string name)
        {
        }

        public void AndroidSetGameRounds(int count)
        {
        }

        public void AndroidThinkUserSet(Dictionary<string, object> properties)
        {
        }

        public void AndroidThinkUserSetOnce(Dictionary<string, object> properties)
        {
        }

        public void AndroidThinkTrack(string name, Dictionary<string, object> value = null)
        {
            ThinkingAnalyticsAPI.Track(name, value);
        }

        public void AndroidFlushDatas()
        {
        }
    }
#endif
}