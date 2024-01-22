using System;
using Core.Controllers;
using Core.Manager;
using Core.MyAttribute;
using Core.Services.UserInterfaceService.API.Facade;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using iOSCShape;
using MyBox;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace AndroidCShape
{
#if (UNITY_ANDROID && !NO_SDK) || UNITY_EDITOR

    public class YZAndroidLocationPlugin : YZBaseController<YZAndroidLocationPlugin>
    {
        private Action<int> brauthfunc;

        private Action<string> braddressfunc;

        private string brlocation;

        private const float locationTimeMax = 8.0f;
        private float locationTimer = 0;

        private bool isLocating = false;

        [NonSerialized]
        public bool AndroidLocateHaveResult = false;
        
        [NonSerialized]
        public bool RejectLocate = false;
        
        private void Update()
        {
            if (isLocating)
            {
                locationTimer += Time.deltaTime;

                if (locationTimer > locationTimeMax)
                {
                    // 超过定位时间限制，关闭等待弹窗，认为定位成功
                    // 关闭等待界面
                    UserInterfaceSystem.That.RemoveUIByName("UIWaitingCtrler");
                    // string msg =
                    //     "{\"name\":\"9120\",\"thoroughfare\":\"Fraser Road\",\"subThoroughfare\":\"9120\",\"locality\":\"Holland Patent\",\"administrativeArea\":\"New York\",\"subAdministrativeArea\":\"Oneida County\",\"postalCode\":\"13354\",\"ISOcountryCode\":\"US\",\"country\":\"United States\"}";
                    // 给定位一个假数据
                    GeocoderLocationFinish("");

                    locationTimer = 0;
                }
            }
        }

        public bool CheckLocationPermission()
        {
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            return WudiAttribution.CallStatic<bool>("CheckLocationPermission");
        }

        /// 获取定位权限 0: 已经拒绝 1: 已经同意
        public void RequestLocationPermissions(string json, Action<int> func)
        {
            brauthfunc = func;
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            WudiAttribution.CallStatic("RequestLocationPermissions", json);
        }

        public void  GetGPSLocation(Action<string> func)
        {
            braddressfunc = func;
            AndroidJavaClass WudiAttribution = new AndroidJavaClass("com.bingo.win.AndroidTool");
            WudiAttribution.CallStatic("GetGPSLocation");
        }

        [CallByAndroid]
        public void LocationManagerDidAuthorization(string status)
        {
            YZDebug.Log(status == "2" ? ">>>>>有定位权限" : ">>>>>没有定位权限");
            if (status == "2")
            {
                brauthfunc?.Invoke(Const.LocateAllow);
                RejectLocate = false;
            }
            else
            {
                brauthfunc?.Invoke(Const.LocateReject);
                AndroidLocateHaveResult = true;
                RejectLocate = true;
            }
        }

        [CallByAndroid]
        public void LocationDidSuccess(string msg)
        {
            YZDebug.LogConcat("定位成功: ", msg);
            brlocation = msg;
            DeviceInfoUtils.Instance.SetGPSExtra(msg, out _);
            isLocating = true;
        }

        public void LocationDidFailure(string msg)
        {
            YZDebug.LogConcat("定位失败: ", msg);
            braddressfunc?.Invoke("");
        }


        [CallByAndroid]
        public void GeocoderLocationFinish(string msg)
        {
            isLocating = false;

            YZDebug.LogConcat("定位完成: ", msg);
            if (msg.Contains("ISOcountryCode") && !RejectLocate)
            {
                var localJson = JObject.Parse(msg);
                string countryCode = localJson.SelectToken("ISOcountryCode")?.ToString();
                string area = localJson.SelectToken("administrativeArea")?.ToString();
                string city = localJson.SelectToken("locality")?.ToString();
                LocationManager.Shared.SaveLocateData(countryCode, area, city);
            }
            else
            {
                // 清空定位
                YZDebug.Log("用户拒绝定位权限，清空定位");
                LocationManager.Shared.ClearLocateData();
            }

            if (string.IsNullOrEmpty(msg) && string.IsNullOrEmpty(brlocation))
            {
                RequestYZServerLocation(brlocation);
            }
            else
            {
                braddressfunc?.Invoke(msg);
            }

            AndroidLocateHaveResult = true;
        }

        private void RequestYZServerLocation(string location)
        {
            // TODO 定位相关
            //     YZLocation cl = YZGameUtil.JsonYZToObject<YZLocation>(location);
            //     YZServerApi.Shared.GetYZAddressLocation(cl.latitude.ToString(), cl.longitude.ToString()).AddYZSuccessHandler((json) =>
            //     {
            //         LitJson.JsonData data = YZGameUtil.JsonYZToObject<LitJson.JsonData>(json);
            //         SYZPlacemark spk = YZGameUtil.JsonYZToObject<SYZPlacemark>(data["data"].ToJson());
            //         if (spk != null && spk.address != null && !string.IsNullOrEmpty(spk.address))
            //         {
            //             SYZAddress sds = YZGameUtil.JsonYZToObject<SYZAddress>(spk.address);
            //             YZPlacemark pk = new YZPlacemark
            //             {
            //                 country = sds.country,
            //                 ISOcountryCode = sds.country_code.ToUpper(),
            //                 administrativeArea = sds.state,
            //                 subAdministrativeArea = sds.county
            //             };
            //             braddressfunc?.Invoke(JsonUtility.ToJson(pk));
            //         }
            //         else
            //         {
            //             braddressfunc?.Invoke("");
            //         }
            //     }).AddYZFailureHandler((code, err) =>
            //     {
            //         braddressfunc?.Invoke("");
            //     });
            // }
        }

        [Serializable]
        public class AndroidLocationParams
        {
            public string desc;
            public string denied_text;
            public string denied_desc;
            public string denied_ok;
            public string denied_no;
        }
    }

#endif
}