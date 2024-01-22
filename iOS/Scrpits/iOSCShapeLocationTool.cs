using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using Core.Controllers;
using Core.Controls;
using Core.Extensions;
using Core.Manager;
using Core.MyAttribute;
using Core.Server;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Third.I18N;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using Newtonsoft.Json.Linq;
using Utils;

namespace iOSCShape
{
    public class iOSCShapeLocationTool : YZBaseController<iOSCShapeLocationTool>
    {

        [NonSerialized]
        public bool iOSLocateHaveResult = false;
        
        
        [NonSerialized]
        public bool iOSRejectLocate = false;
        
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")] private static extern void ObjcSetupLocationUnity(string param);
    [DllImport("__Internal")] private static extern void ObjcGetLocationUnity();
    [DllImport("__Internal")] private static extern void ObjcRequestLocationAuthorizationUnity();
    [DllImport("__Internal")] private static extern string ObjcGetLastLocationUnity();
    [DllImport("__Internal")] private static extern string ObjcGetLocationAuthorizationUnity();
#endif
        private Action<int> func_auth;

        private Action<string> func_address;

        public override void InitController()
        {
            base.InitController();
            IOSYZSetupLocation(new YZLocationParams(YZLocationAccuracy.kCLLocationAccuracyThreeKilometers, 3000));
        }

        /// 初始化定位相关参数
        public void IOSYZSetupLocation(YZLocationParams param)
        {
#if UNITY_IOS && !UNITY_EDITOR
        ObjcSetupLocationUnity(JsonUtility.ToJson(param));
#endif
        }

        /// 获取定位权限
        public void IOSYZRequestLocationAuthorization()
        {
#if UNITY_IOS && !UNITY_EDITOR
        ObjcRequestLocationAuthorizationUnity();
#endif
        }

        /// 开启一次单次定位
        public void IOSYZGetLocation(Action<string> func)
        {
            func_address = func;
#if UNITY_IOS && !UNITY_EDITOR
        ObjcGetLocationUnity();
#endif
        }

        /// 获取上次定位的经纬度
        public string IOSYZGetLastLocation()
        {
#if UNITY_IOS && !UNITY_EDITOR
        return ObjcGetLastLocationUnity();
#endif
            return "";
        }

        /// <summary>
        /// 用户还未决定授权
        /// </summary>
        public const string notDiside = "1"; 
        
        /// <summary>
        /// 访问受限(用户关闭了定位总开关，iOS系统没法去请求权限)
        /// </summary>
        public const string limiteByiOS = "2"; 
        
        /// <summary>
        /// 用户拒绝
        /// </summary>
        public const string  rejectByUser= "3"; 
        
        /// <summary>
        /// 获得可以一直定位权限
        /// </summary>
        public const string  alwaysAllow= "4"; 
        
        /// <summary>
        /// 获得使用时可以定位权限
        /// </summary>
        public const string  inUsingAllow= "5"; 
        
        /// 获取当前用户的授权状态
        /// "1":用户还未决定授权
        /// "2":访问受限(用户关闭了定位总开关，iOS系统没法去请求权限)
        /// "3":用户拒绝
        /// "4":获得可以一直定位权限
        /// "5":获得使用时可以定位权限
        public string IOSYZGetLocationAuthorization()
        {
#if UNITY_IOS && !UNITY_EDITOR
        return ObjcGetLocationAuthorizationUnity();
#endif
#if UNITY_IOS && !UNITY_EDITOR
            YZLog.LogColor("IOSYZGetLocationAuthorization =" + ObjcGetLocationAuthorizationUnity()); 
#endif
            return "5";
        }

        /// 用户授权
        [CallByIOS]
        public void CShapeLocationDidChangeAuthorization(string status)
        {
            YZDebug.LogConcat("[定位] 定位权限: ", status);
            Dictionary<string, object> dict = new Dictionary<string, object>();
            if (status == alwaysAllow || status == inUsingAllow)
            {
                dict.Add(FunnelEventID.brpermissiongps, true);
                func_auth?.Invoke(Const.LocateAllow);
            }
            else
            {
                dict.Add(FunnelEventID.brpermissiongps, false);
                func_auth?.Invoke(Const.LocateReject);
            }
            YZFunnelUtil.UserYZSet(dict);
        }
        
        /// 定位成功
        [CallByIOS]
        public void CShapeLocationDidSuccess(string json)
        {
            YZDebug.LogConcat("[定位] 定位成功: ", json);
            LocationManager.Shared.SaveLocateData(json);
            func_address?.Invoke(json);
            iOSLocateHaveResult = true;
            func_address = null;
        }

        
        /// 定位失败
        [CallByIOS]
        public void CShapeLocationDidFailWithError(string error)
        {
            YZDebug.LogConcat("[定位] 定位失败: ", error);
            YZFunnelUtil.SendYZEvent(FunnelEventID.brgpslocationerror);
            func_address?.Invoke("");
            iOSLocateHaveResult = true;
            func_address = null;
            
        }

     
        /// 定位超时
        [CallByIOS]
        public void CShapeLocationTimeOut()
        {
            YZDebug.LogConcat("[定位] 定位超时");
            func_address?.Invoke("");
            iOSLocateHaveResult = true;
            func_address = null;
        }

        /// 虚拟定位开启
        [CallByIOS]
        public void CShapeLocationMock()
        {
            YZDebug.LogConcat("[定位] 虚拟定位开启");
            DeviceInfoUtils.Instance.SelfGPSExtra.gps_camouflage = true;
        }
        
        /// 经纬度解析失败
        [CallByIOS]
        public void CShapeLocationReverseGeocodeError(string error)
        {
            string location = IOSYZGetLastLocation();
            YZDebug.LogConcat("[定位] 解析经纬度失败: ", error);
            YZDebug.LogConcat("[定位] 开始使用服务器Api解析经纬度");
            YZDebug.LogConcat("[定位] 当前经纬度: ", location);
            
            bool sendEvent = true;
            if (string.IsNullOrEmpty(location))
            {
                func_address?.Invoke("");
                func_address = null;
                iOSLocateHaveResult = true;
            }
            else
            {
                if (func_address != null)
                {
                    //这里不写定位成功iOSLocateHaveResult = true, 在网络回调里写了
                    DeviceInfoUtils.Instance.SetGPSExtra(location, out var sameLocation, true, () =>
                    {
                        func_address?.Invoke(Const.FakeGetGPSFlag);
                        func_address = null;
                    });
                    
                    //如果是相同定位, 就不发送事件
                    sendEvent = !sameLocation;
                }
                
                // RequestYZServerLocation(location);
            }

            if (sendEvent)
            {
                YZFunnelUtil.SendYZEvent(FunnelEventID.brgpsreverseerror);
            }
        }

        /// 经纬度解析成功
        [CallByIOS]
        public void CShapeLocationReverseGeocodeSuccess(string json)
        {
            string location = IOSYZGetLastLocation();
            DeviceInfoUtils.Instance.SetGPSExtra(location, out _);
            YZDebug.LogConcat("[定位] 获取到的定位信息：", json);
            LocationManager.Shared.SaveLocateData(json);
            
            func_address?.Invoke(json);
            iOSLocateHaveResult = true;
            func_address = null;
        }

        /// 获取定位权限 0: 已经拒绝 1: 已经同意
        public void RequestYZLocationAuthorization(Action<int> func)
        {
            string status = IOSYZGetLocationAuthorization();
        
            if (status == alwaysAllow || status == inUsingAllow)
            {
                func?.Invoke(Const.LocateAllow);
                func_auth = null;
            }
            else if (status == notDiside)
            {
                // 没决定
                string txt_title =  I18N.Get("key_allow_permissiom");
                string txt_tips =  I18N.Get("key_allow_yatzy", YZNativeUtil.GetYZAppName());
                string txt_btn = I18N.Get("key_allow");
                // YZTopControl.YZShowTips(txt_title, txt_tips, txt_btn, () =>
                // {
                //     //拒绝开启定位
                //     func?.Invoke(0);
                //     func_auth = null;
                // }, () => {
                //     func_auth = func;
                //     IOSYZRequestLocationAuthorization();
                // });

                UserInterfaceSystem.That.ShowUI<UIConfirm>(new UIConfirmData()
                {
                    Type = UIConfirmData.UIConfirmType.TwoBtn,
                    title = txt_title,
                    desc = txt_tips,
                    confirmTitle = txt_btn,
                    confirmCall = () =>
                    {
                        func_auth = func;
                        IOSYZRequestLocationAuthorization();
                    },
                    cancleCall = () =>
                    {
                        //拒绝开启定位
                        func?.Invoke(Const.LocateReject);
                        func_auth = null;
                    }
                });
            }
            else if (status == limiteByiOS)
            {
                // 访问受限：定位服务授权状态是受限制的。可能是由于活动限制定位服务，用户不能改变。这个状态可能不是用户拒绝的定位服务
                func?.Invoke(Const.LocateReject);
                func_auth = func;
            }
            else
            {
                // 已拒绝
                string txt_title = I18N.Get("key_ERROR");
                string txt_tips = I18N.Get("key_the_location", YZNativeUtil.GetYZAppName());
                string txt_btn = I18N.Get("key_go_to");
                
                UserInterfaceSystem.That.ShowUI<UIConfirm>(new UIConfirmData()
                {
                    Type = UIConfirmData.UIConfirmType.TwoBtn,
                    title = txt_title,
                    desc = txt_tips,
                    confirmTitle = txt_btn,
                    confirmCall = () =>
                    {
                        func_auth = func;
                        iOSCShapeTool.Shared.IOSYZGotoSetting();
                    },
                    cancleCall = () =>
                    {
                        func?.Invoke(Const.LocateReject);
                        func_auth = null;
                    }
                });
                
                // YZTopControl.YZShowTips(txt_title, txt_tips, txt_btn, () =>
                // {
                //     //cancel btn
                //     func?.Invoke(0);
                //     func_auth = null;
                // }, () => {
                //     func_auth = func;
                //     iOSCShapeTool.Shared.IOSYZGotoSetting();
                // });
            }
        }
        
        /// 向服务器请求， 获取经纬度信息
        private void RequestYZServerLocation(string location)
        {
            // YZLocation cl = YZGameUtil.JsonYZToObject<YZLocation>(location);
            // //向服务器请求， 获取经纬度信息
            // YZServerApi.Shared.GetYZAddressLocation(cl.latitude.ToString(), cl.longitude.ToString()).AddYZSuccessHandler((json) =>
            // {
            //     LitJson.JsonData data = YZGameUtil.JsonYZToObject<LitJson.JsonData>(json);
            //     SYZPlacemark spk = YZGameUtil.JsonYZToObject<SYZPlacemark>(data["data"].ToJson());
            //     if (spk != null && spk.address != null && !string.IsNullOrEmpty(spk.address))
            //     {
            //         SYZAddress sds = YZGameUtil.JsonYZToObject<SYZAddress>(spk.address);
            //         YZPlacemark pk = new YZPlacemark
            //         {
            //             country = sds.country,
            //             ISOcountryCode = sds.country_code.ToUpper(),
            //             administrativeArea = sds.state,
            //             subAdministrativeArea = sds.county
            //         };
            //         func_address?.Invoke(JsonUtility.ToJson(pk));
            //     }
            //     else
            //     {
            //         func_address?.Invoke("");
            //     }
            // }).AddYZFailureHandler((code, err) =>
            // {
            //     func_address?.Invoke("");
            // });
        }
    }
}