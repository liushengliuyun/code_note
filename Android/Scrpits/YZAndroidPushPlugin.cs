using System;
using System.Collections;
using System.Collections.Generic;
using Core.Controllers;
using Core.Server;
using UnityEngine;

namespace AndroidCShape
{
#if UNITY_ANDROID || UNITY_EDITOR
    public class YZAndroidPushPlugin : YZBaseController<YZAndroidPushPlugin>
    {
        private bool brisinited = false;

        public string YZTags;

        /// <summary>
        /// 好像是废代码
        /// </summary>
        [Obsolete]
        public void AndroidYZInitPush()
        {
            if (!brisinited && YZServerApiOrganic.Shared.IsYZShowMoney())
            {
                brisinited = true;
            }
        }

        public void AndroidYZSetTags(string json)
        {
            if (!string.IsNullOrEmpty(json))
            {
                YZTags = json;
            }

            AndroidYZInitPush();
            // if (brisinited)
            // {
            //     YZTags = null;
            // }
        }
    }
#endif
}