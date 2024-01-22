using Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AndroidCShape;
using iOSCShape;
using UnityEngine;
using AppsFlyerSDK;
using Core.Server;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using Carbon.Util;
using Core.Extensions;
using LitJson;
using Newtonsoft.Json;
using OneSignalSDK.Debug.Models;

#if !NO_SDK
using OneSignalSDK;
using Facebook.Unity;
#endif

using ThinkingAnalytics;
using ThinkingSDK.PC.Main;
using UI;
using AUTO_TRACK_EVENTS = ThinkingAnalytics.AUTO_TRACK_EVENTS;

namespace Core.Controllers
{
    public class YZSDKsController : YZBaseController<YZSDKsController>, IAppsFlyerConversionData
    {
#if RELEASE || RTEST
        public bool AF_ORGANIC = true;
#else
        /// <summary>
        /// true 自然量
        /// </summary>
        public bool AF_ORGANIC = true;
#endif
        public string MediaSource = "";
        
        private bool _isInit = false;
        
        private bool _isInitAD = false;
        
        public bool ClientBlock = false;
        
#if RELEASE
        public bool IsBlockValid = true;
#else
        public bool IsBlockValid = false;
#endif
        
        public string CurrentUrl = "";
        public int Pay_App = 0;

        private bool isNotifyRequestLock = false;

        public bool LocateHaveResult
        {
            get
            {
#if UNITY_ANDROID
                  return YZAndroidLocationPlugin.Shared.AndroidLocateHaveResult;    
#endif
                
#if UNITY_IOS
                return iOSCShapeLocationTool.Shared.iOSLocateHaveResult;    
#endif
                
                return true;
            }
        }

        /// <summary>
        /// 加载SDK
        /// </summary>
        public void YZInitSDKAndConfig()
        {
#if !NO_SDK
            if (_isInit)
            return;
              
            AppsFlyer.setCustomerUserId(Root.Instance.UserId.ToString());
            
            YZDebug.enable = YZDefineUtil.IsDebugger;
            YZNativeUtil.InitYZIDFA();

            // if (YZDefineUtil.IsDebugger)
            // {
            //     YZServerApiOrganic.Shared.SetOrganic(YZOrganic.YZNONORGANIC);
            // }
            
            YZServerApiOrganic.Shared.SetOrganic(YZOrganic.YZNONORGANIC);

            Root.Instance.SessionId = Root.Instance.Role.user_id + "_" + TimeUtils.Instance.LoginTimeStamp;
            
            SetupThink();
            //SetupYZLitjson();
            SetupYZBugly();
#if UNITY_ANDROID
            YZDebug.Log(YZAndroidPlugin.Shared.AndroidPingGoogle() ? "已翻墙": "未翻墙");
#endif

            // SetupAds();
            SetupFirebase();
            SetupOneSignal();

            SetupForter();
            SetupRiskified();

            Debug.Log("wjs facebook init");
            Debug.Log("wjs facebook appid = " + YZDefineUtil.GetFBAPPId() + " clienttoken = " + YZDefineUtil.GetFBClientToken());
            FB.Init(YZDefineUtil.GetFBAPPId(), YZDefineUtil.GetFBClientToken(), true, true,
               true, false, true, "auth", "en_US", null, () =>
               {
                   if (FB.IsInitialized)
                   {
                       Debug.Log("wjs facebook initialized");
                       FacebookLogger.Warn("FB.IsInitialized");
                       FacebookLogger.Warn("FB appId = " + FB.AppId);
                       FacebookLogger.Warn("FB client token = " + FB.ClientToken);
                       Dictionary<string, object> fbDic = new Dictionary<string, object>();
                       fbDic["user_id"] = Root.Instance.Role.user_id.ToString();
                       Debug.Log("wjs facebook app_start param = " + fbDic["user_id"]);
                       FB.ActivateApp();
                       FB.LogAppEvent("app_start", 1, fbDic);
                   }
                   else
                   {
                       Debug.Log("wjs facebook initialize failed");
                       FacebookLogger.Warn("FB. not  IsInitialized");
                   }
               });

            // 登录通用后台
            //if (!YZDefineUtil.IsDebugger)
#if RELEASE || LOG
            YZLog.LogColor("登录通用后台");
            SetupGeneralSever();
#endif
            // 请求国家信息
            MediatorRequest.Instance.GetCountryInfo();

            _isInit = true;
#endif
        }

        private void SetupGeneralSever()
        {
            YZServerCommon.Shared.SendYZLogin();
        }

        private void SetupFirebase()
        {
            YZFirebaseController.Shared.Init((isSuccess)=>{
                if (isSuccess)
                {
                    YZFirebaseController.Shared.FireBaseSetUserId(Root.Instance.UserId.ToString());
                    YZFirebaseController.Shared.FireBaseAuth();
                    if (YZSDKsController.Shared.CurrentUrl.Contains("guest"))
                    {
                        YZFirebaseController.Shared.FireBaseTrackRegisterEvent();
                    }
                    else
                    {
                        YZFirebaseController.Shared.FireBaseTrackLoginEvent();
                    }
                }
            });
        }

        public void SetupOneSignal()
        {
            // 自然量不弹通知
            //if (Root.Instance.IsNaturalFlow)
            //    return;
#if !NO_SDK && RELEASE
            Debug.Log("wjs setup onesignal");
            // var result = await OneSignal.Default.Debug.ToString();
            // PromptForPush();
            OneSignal.Initialize(YZDefineUtil.GetYZOneSignalAppId());
            OneSignal.Debug.LogLevel = OneSignalSDK.Debug.Models.LogLevel.Verbose;
            OneSignalLogin();
#endif
        }

        public void SetupForter()
        {
#if UNITY_IOS
            iOSCShapeForterTool.Shared.IOSForterSetupWithSiteId(YZDebug.GetAccountId());
            iOSCShapeForterTool.Shared.IOSForterSetDeviceUniqueIdentifier(iOSCShapeTool.Shared.IOSYZGetUDID());
#elif UNITY_ANDROID
#endif
        }

        public void SetupRiskified()
        {
#if UNITY_IOS
#if RELEASE
            iOSCShapeRiskifiedTool.Shared.IOSRiskifiedStartBeacon(@"www.winnerstudio.com", iOSCShapeTool.Shared.IOSYZGetUDID(), false);
#else
            iOSCShapeRiskifiedTool.Shared.IOSRiskifiedStartBeacon(@"www.winnerstudio.com", iOSCShapeTool.Shared.IOSYZGetUDID(), true);
#endif
#elif UNITY_ANDROID
#endif
        }

        public async void PromptForPush()
        {
            if (isNotifyRequestLock)
                return;

            isNotifyRequestLock = true;

            YZDebug.Log("Calling PromptForPushNotificationsWithUserResponse and awaiting result...");
#if !NO_SDK && RELEASE
            //if (OneSignal.Notifications.CanRequestPermission)
            {
                //YZDataUtil.SetYZInt(YZConstUtil.YZNotifyWindowPopDay, timeToday);
                // 当天只弹出一次请求权限

                var result = await OneSignal.Notifications.RequestPermissionAsync(false);
                OneSignal.Notifications.PermissionChanged += (sender, e) =>
                {
                    Dictionary<string, object> dict = new Dictionary<string, object>();
                    
                    if (e.Permission)
                    {
                        // User just accepted notifications or turned them back on!
                        if (result)
                        {
                            YZDebug.Log("OneSignal Authorized");
                            dict.Add(FunnelEventID.brpermissionpush, true);
                        }
                        else
                        {
                            YZDataUtil.SetYZInt(YZConstUtil.YZDeniedNotifyPermission, 1);
                            YZDebug.Log("OneSignal Not Authorized");
                            dict.Add(FunnelEventID.brpermissionpush, false);
                        }
                    }
                    else
                    {
                        YZDataUtil.SetYZInt(YZConstUtil.YZDeniedNotifyPermission, 1);
                        YZDebug.Log("OneSignal Not Authorized");
                        dict.Add(FunnelEventID.brpermissionpush, false);
                    }
                    YZFunnelUtil.UserYZSet(dict);
                    OneSignalLogin();
                };
            
                YZDebug.Log($"Prompt completed with <b>{result}</b>");
                if (!result)
                {
                    // 未授予权限也需要登录
                    OneSignalLogin();
                }

            }
#endif
        }

        private void OneSignalLogin()
        {
#if UNITY_ANDROID
            isNotifyRequestLock = false;
            YZDebug.Log("OneSignal Login Start");
            try
            {
                string tagStr = YZAndroidPushPlugin.Shared.YZTags;
                Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(tagStr);
                if (dic != null && dic.ContainsKey("user_id"))
                {
                    OneSignal.Login(dic["user_id"]);
                    OneSignal.User.AddTags(dic);
                    YZDebug.Log("OneSignal Login with" + dic["user_id"]);
                }
            }
            catch (Exception e)
            {
                CarbonLogger.LogError("OneSignal登录异常!" + e + "\n" + e.StackTrace);
                throw;
            }
#elif UNITY_IOS
            isNotifyRequestLock = false;
            YZDebug.Log("OneSignal Login Start");
            try
            {
                string tagStr = iOSCShapePushTool.Shared.YZTags;
                Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(tagStr);
                if (dic != null && dic.ContainsKey("user_id"))
                {
                    YZDebug.Log("OneSignal Login with" + dic["user_id"]);
                    OneSignal.Login(dic["user_id"]);
                    OneSignal.User.AddTags(dic);
                }
            }
            catch (Exception e)
            {
                CarbonLogger.LogError("OneSignal登录异常!" + e + "\n" + e.StackTrace);
                throw;
            }
#endif
        }

        void AppsFlyerOnRequestResponse(object sender, EventArgs e)
        {
            // 200	null
            // 10	"Event timeout. Check 'minTimeBetweenSessions' param"
            // 11	"Skipping event because 'isStopTracking' enabled"
            // 40	Network error: Error description comes from Android
            // 41	"No dev key"
            // 50	"Status code failure" + actual response code from the server
            var args = e as AppsFlyerRequestEventArgs;
            AppsFlyer.AFLog("AppsFlyerOnRequestResponse", " status code " + args.statusCode);
        }
        
        public void SetupAppsFlyer()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Application.targetFrameRate = YZDefineUtil.GameFrameRate;

            iOSCShapeAppsflyerTool.Shared.IOSStartAppsFlyer();
            AppsFlyer.initSDK(YZDefineUtil.GetAppsFlyerDevKey(), YZDefineUtil.GetAppsFlyerAppId());
            
#if UNITY_IOS && !UNITY_EDITOR
            AppsFlyer.waitForATTUserAuthorizationWithTimeoutInterval(60);
            AppsFlyer.OnRequestResponse += AppsFlyerOnRequestResponse;
#endif
            
            AppsFlyer.startSDK();
            
            YZFunnelUtil.SendYZEvent("AppsFlyer_app_start");

            AppsFlyer.getConversionData("YZController");
        }

        // private void Start()
        // {
        //     // TODO iOS
        //     // if (YZGameUtil.GetIsiOS())
        //     // {
        //     //     string status = iOSCShapePushTool.Shared.IOSYZGetPushAuth();
        //     //     if (status != "0")
        //     //     {
        //     //         YZDebug.Log("Init Onesignal");
        //     //         iOSCShapePushTool.Shared.IOSYZInitOrSendTags();
        //     //     }
        //     // }
        //
        //     YZInitSDKAndConfig();
        // }

        public void SetupAds()
        {
            if (_isInitAD || !Root.Instance.HaveAdRoom)
            {
                 return;   
            }

            _isInitAD = true;
            YZAdsController.Shared.YZSetupAdsSDK(null, null, null);
        }

        /// <summary>
        /// 初始化数数
        /// </summary>
        public void SetupThink()
        {
#if UNITY_IOS
            iOSCShapeThinkTool.Shared.IOSInitThink();
#endif
            // TODO login user ID
            ThinkingAnalyticsAPI.Login(Root.Instance.Role.user_id.ToString());

            var dic = new Dictionary<string, object>();
            dic["session_id"] = Root.Instance.SessionId;
            
            ThinkingAnalyticsAPI.EnableAutoTrack(AUTO_TRACK_EVENTS.APP_INSTALL, dic);
            
            ThinkingAnalyticsAPI.EnableAutoTrack(AUTO_TRACK_EVENTS.APP_START, dic);
            
            // 给业务服发送 session_id
            MediatorRequest.Instance.SendThinkingSessionId();
            
            // TODO 设置公共事件属性
            // //设置公共事件属性以后，每个事件都会带有公共事件属性
            // Dictionary<string, object> superProperties = new Dictionary<string, object>();
            // superProperties["channel"] = "ta";//字符串
            // superProperties["age"] = 1;//数字
            // superProperties["isSuccess"] = true;//布尔
            // superProperties["birthday"] = DateTime.Now;//时间
            // superProperties["object"] = new Dictionary<string, object>(){{ "key", "value"}};//对象
            // superProperties["object_arr"] = new List<object>() {new Dictionary<string, object>(){{ "key", "value" }}};//对象组
            // superProperties["arr"] = new List<object>() { "value" };//数组
            // ThinkingAnalyticsAPI.SetSuperProperties(superProperties);//设置公共事件属性
            
            // 设置用户属性
            ThinkingAnalyticsAPI.UserSet(new Dictionary<string, object>(){{"user_name", Root.Instance.Role.user_id.ToString()}});
            
            
            ThinkingAnalyticsAPI.Track("app_start");
            
            // //发送事件
            // Dictionary<string, object> properties = new Dictionary<string, object>(){{"product_name", "商品名")}};
            // ThinkingAnalyticsAPI.Track("product_buy", properties);
        }

        private void SetupYZLitjson()
        {
            // JsonMapper.RegisterImporter((int input) => { return input.ToString(); });
            // JsonMapper.RegisterImporter((string input) => { return int.Parse(input); });
            //
            // JsonMapper.RegisterImporter((long input) => { return input.ToString(); });
            // JsonMapper.RegisterImporter((string input) => { return long.Parse(input); });
            //
            // JsonMapper.RegisterImporter((Int32 input) => { return input.ToString(); });
            // JsonMapper.RegisterImporter((string input) => { return Int32.Parse(input); });
            //
            // JsonMapper.RegisterImporter((Int64 input) => { return input.ToString(); });
            // JsonMapper.RegisterImporter((string input) => { return Int64.Parse(input); });
            //
            // JsonMapper.RegisterImporter((double input) => { return input.ToString(); });
            // JsonMapper.RegisterImporter((string input) => { return input.ToDouble(); });
            //
            // JsonMapper.RegisterImporter((string input) => { return input.ToFloat(); });
            // JsonMapper.RegisterImporter((float input) => { return input.ToString(); });
            //
            // JsonMapper.RegisterImporter((double input) => { return (float)input; });
            // JsonMapper.RegisterImporter((float input) => { return (double)input; });
        }

        /// <summary>
        /// 初始化bugly
        /// </summary>
        private void SetupYZBugly()
        {
#if UNITY_ANDROID
            string devicesInfo = DeviceInfoUtils.Instance.GetDeviceInfo().ToLower();
            if (devicesInfo.Contains("de2118") || devicesInfo.Contains("oneplus"))
                return;

            // bugly 忽略自然量
            if (Root.Instance.IsNaturalFlow)
                return;
#endif
            
            BuglyAgent.InitWithAppId(YZDefineUtil.GetYZBuglyAppId());
            
#if UNITY_ANDROID || UNITY_IOS
            BuglyAgent.ConfigCrashReporter(0, 1);
#endif
            BuglyAgent.ConfigDebugMode(false);
            BuglyAgent.EnableExceptionHandler();
#if RELEASE
            BuglyAgent.SetUserId(Root.Instance.Role.user_id.ToString());
#else
            BuglyAgent.SetUserId(Root.Instance.Role.user_id.ToString() + ".test");
#endif
            BuglyAgent.RegisterLogCallback(LogMessageReceived);
        }

        private void LogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (YZDefineUtil.IsDebugger)
            {
                if (type == LogType.Exception || type == LogType.Error)
                {
                    // if (UserInterfaceSystem.That != null)
                    // UserInterfaceSystem.That.ShowUI<UITip>(
                    //     string.Format("{0}\n{1}", condition, stackTrace), 10);
                }
            }

            YZGameUtil.PrintBuglyLog(YZString.Concat("CShape Exception", condition));
        }

        public void OnApplicationPause(bool pauseStatus)
        {
            YZDebug.LogConcat("pause  state = ", pauseStatus.ToString());
            if (pauseStatus)
            {
                // 离开游戏
                var dic = new Dictionary<string, object>();
                dic["session_id"] = Root.Instance.SessionId;
                dic["game_rounds"] = Root.Instance.GameRounds.ToString();
                ThinkingAnalyticsAPI.EnableAutoTrack(AUTO_TRACK_EVENTS.APP_END, dic);

                YZDebug.LogConcat("game_rounds = ", Root.Instance.GameRounds.ToString());

            }
            else
            {
                //YZServerApiMessage.Shared.YZAppEnter();
            }
        }

        /// <summary>
        /// 归因成功的回调 mark ios会从哪调用？
        /// </summary>
        /// <param name="conversionData"></param>
        public void onConversionDataSuccess(string conversionData)
        {
            // 不是第一次登录
            if (Root.Instance.Role.user_id != 0)
                return;
            
            AppsFlyer.AFLog("didReceiveConversionData", conversionData);
       
            // add deferred deeplink logic here
#if RELEASE || RTEST
     Dictionary<string, object> conversionDataDictionary = AppsFlyer.CallbackStringToDictionary(conversionData);
            if (conversionDataDictionary.TryGetValue("af_status", out var value))
            { 
                string isOrganic = value.ToString().ToLower();
                if (isOrganic.Equals("organic"))
                {
                    Debug.Log(" AppsFlyer isOrganic" + isOrganic);
                    AF_ORGANIC = true;
                }
                else
                {
                    AF_ORGANIC = false;
                    Debug.Log("AppsFlyer is not  Organic");
                }
            }
            else
            {
                Debug.LogError("AppsFlyer isOrganic Key not exist");
            }


            if (conversionDataDictionary.TryGetValue("media_source", out var valueSource))
            {
                MediaSource = valueSource.ToString();
            }

            conversionDataDictionary.TryGetValue("media_source", out string channel);
            conversionDataDictionary.TryGetValue("campaign", out string campaign);
            // 打点归因
            Dictionary<string, object> properties = new Dictionary<string, object>()
            {
                { "af_id", YZNativeUtil.GetYZAFID() },
                { "channel", channel },
                { "campaign", campaign },
                { "media_info", conversionData },
            };
            YZFunnelUtil.SendYZEvent("media_source", properties);
#if UNITY_ANDROID
            StartCoroutine(LoginCor());
#else
            StartCoroutine(SetOrganicCor());
#endif
            
#endif
        }
        
        IEnumerator SetOrganicCor()
        {
            yield return new WaitUntil(() => Root.Instance.UserId > 0);

            MediatorRequest.Instance.SetOrganic();
        }

        IEnumerator LoginCor()
        {
#if UNITY_IOS && RELEASE
            // yield return new WaitUntil(() => LocateHaveResult && !Root.Instance.IP.IsNullOrEmpty());
            yield return new WaitForSeconds(0.5f);
#else
            yield return new WaitUntil(() => LocateHaveResult);
            // yield return new WaitForSeconds(0.5f);
#endif
            
            if (Root.Instance.Role.user_id <= 0)
            {
                MediatorRequest.Instance.PlayerLogin();
            }
        }
        public void onConversionDataFail(string error)
        {
            AppsFlyer.AFLog("didReceiveConversionDataWithError", error);
        }

        public void onAppOpenAttribution(string attributionData)
        {
            AppsFlyer.AFLog("onAppOpenAttribution", attributionData);
            Dictionary<string, object> attributionDataDictionary = AppsFlyer.CallbackStringToDictionary(attributionData);
            // add direct deeplink logic here
        }

        public void onAppOpenAttributionFailure(string error)
        {
            AppsFlyer.AFLog("onAppOpenAttributionFailure", error);
        }

        public void Reset()
        {
#if RELEASE || RTEST
            AF_ORGANIC = true;
#else
            AF_ORGANIC = false;
#endif
            _isInit = false;
            _isInitAD = false;
            AppsFlyer.OnRequestResponse -= AppsFlyerOnRequestResponse;
            YZAdsController.Shared.Reset();
        }
    }
}