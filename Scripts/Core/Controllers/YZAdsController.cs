using System;
using Core.Server;
using DataAccess.Model;
using AndroidCShape;
using Carbon.Util;
using UnityEngine;
using UnityTimer;
using Utils;

namespace Core.Controllers
{
    public enum AdsStatus
    {
        NONE, // 没有IRS
        NOTIMES, // 没有IRS次数了
        NOCOMPLETED, // IRS没有播放完成
        ERROR, // IRS播放失败
        REWARD, // 关闭IRS，并收到奖励
    }

    public class YZAdsController : YZBaseController<YZAdsController>
    {
        [HideInInspector] public string brrewardadpos;

        [HideInInspector] public MaxSdkBase.AdInfo brcurrentadsinfo;

        private bool brrewarded = false;


        private Action<AdsStatus> brrewardaction;
        
#if UNITY_IOS
        string adUnitId = "e7d714284533281f";
#else // UNITY_ANDROID
        string adUnitId = "b76d2969f1b8ecd7";
#endif
        int retryAttempt;
        
        string netWork = "empty_network";

        public override void InitController()
        {
            base.InitController();

            // IronSourceEvents.onRewardedVideoAdOpenedEvent += YZRewardedVideoAdOpenedEvent;
            // IronSourceEvents.onRewardedVideoAdClickedEvent += YZRewardedVideoAdClickedEvent;
            // IronSourceEvents.onRewardedVideoAdClosedEvent += YZRewardedVideoAdClosedEvent;
            // IronSourceEvents.onRewardedVideoAvailabilityChangedEvent += YZRewardedVideoAvailabilityChangedEvent;
            // IronSourceEvents.onRewardedVideoAdStartedEvent += YZRewardedVideoAdStartedEvent;
            // IronSourceEvents.onRewardedVideoAdEndedEvent += YZRewardedVideoAdEndedEvent;
            // IronSourceEvents.onRewardedVideoAdRewardedEvent += YZRewardedVideoAdRewardedEvent;
            // IronSourceEvents.onRewardedVideoAdShowFailedEvent += YZRewardedVideoAdShowFailedEvent;
            //
            // IronSourceEvents.onImpressionDataReadyEvent += YZImpressionSuccessEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += OnRewardedAdReceivedRewardEvent;

        }
        
        private void LoadRewardedAd()
        {
            MaxSdk.LoadRewardedAd(adUnitId);
        }

        private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad is ready for you to show. MaxSdk.IsRewardedAdReady(adUnitId) now returns 'true'.

            netWork = adInfo.NetworkName ?? "empty_network";
            
            YZDebug.Log("【MAX】激励 加载成功");
            // Reset retry attempt
            retryAttempt = 0;
        }

        private void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            // Rewarded ad failed to load 
            // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds).

            YZDebug.Log("【MAX】激励 加载失败");
            retryAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, retryAttempt));
    
            Invoke("LoadRewardedAd", (float) retryDelay);
        }

        private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            YZDebug.Log("【MAX】激励 开始播放");
        }

        private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad failed to display. AppLovin recommends that you load the next ad.
            LoadRewardedAd();
            
            CarbonLogger.LogError(YZString.Concat("【IRS】激励 播放失败: ", errorInfo.Code, " error: ", errorInfo.Message , " adUnitId : ", adUnitId));
            brrewardaction?.Invoke(AdsStatus.ERROR);
        }

        private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            
        }

        private void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad is hidden. Pre-load the next ad
            LoadRewardedAd();
        }

        private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            // The rewarded ad displayed and the user should receive the reward.
            
            YZDebug.Log(YZString.Concat("【MAX】激励 收到奖励: "));
            brrewarded = true;
            brrewardaction?.Invoke(brrewarded ? AdsStatus.REWARD : AdsStatus.NOCOMPLETED);
            brrewardaction = null;

            if (adInfo != null)
            {
                //decimal revenue = brcurrentadsinfo.revenue.HasValue ? (decimal)brcurrentadsinfo.revenue : 0;
                
                YZDebug.Log("AD Revenue = " + adInfo.Revenue.ToString());
#if RELEASE || LOG
                if (!YZDebug.IsWhiteListTestDevice())
                    YZServerCommon.Shared.SendYZWorth( this.brrewardadpos, adInfo.Revenue.ToString(), "max");
#endif
            }
        }

        private void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Ad revenue paid. Use this callback to track user revenue.
        }

        // private void YZImpressionSuccessEvent(IronSourceImpressionData impressionData)
        // {
        //     brcurrentadsinfo = impressionData;
        // }


        private void InitializedEvent(MaxSdkBase.SdkConfiguration sdkConfiguration)
        {
            // AppLovin SDK is initialized, start loading ads
            YZDebug.Log("【MAX】MAX 初始化完成");
            LoadRewardedAd();
        }
        
        public void YZSetupAdsSDK(string user_id, string reward_id, string inters_id)
        {
            // IronSource.Agent.init(YZDefineUtil.GetYZADSKey(), IronSourceAdUnits.REWARDED_VIDEO);
            // IronSource.Agent.validateIntegration();
            // YZDebug.Log("【IRS】Irs 初始化完成");

#if !UNITY_EDITOR
#if UNITY_ANDROID
            MaxSdkCallbacks.OnSdkInitializedEvent += InitializedEvent;
            MaxSdk.SetSdkKey("3VQ4TIs0L-iEa-cZIosrUoRFAouBezHiTinx_dyLbeaBcBlw-blTOBkZ-blcT0J1i5zikv2JTdUAqE4prqGD0-");
            MaxSdk.SetUserId(YZNativeUtil.GetYZAFID());
            YZDebug.Log("Root.Instance.HaveAdRoom " + Root.Instance.HaveAdRoom);
            if (YZAndroidPlugin.Shared.AndroidPingGoogle())
            {
                YZDebug.Log("【MAX】MAX 初始化开始");
                MaxSdk.InitializeSdk();
            }
#elif UNITY_IOS
            MaxSdkCallbacks.OnSdkInitializedEvent += InitializedEvent;
            MaxSdk.SetSdkKey("3VQ4TIs0L-iEa-cZIosrUoRFAouBezHiTinx_dyLbeaBcBlw-blTOBkZ-blcT0J1i5zikv2JTdUAqE4prqGD0-");
            MaxSdk.SetUserId(YZNativeUtil.GetYZAFID());
            YZDebug.Log("Root.Instance.HaveAdRoom " + Root.Instance.HaveAdRoom);
            YZDebug.Log("【MAX】MAX 初始化开始");
            MaxSdk.InitializeSdk();
#endif
#endif



//#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
//            // string[] adId = new []{""};
//            // MaxSdkAndroid.InitializeSdk(adId);
//            // MaxSdkAndroid.SetUserId(YZNativeUtil.GetYZAFID());

//            MaxSdkCallbacks.OnSdkInitializedEvent += InitializedEvent;
//            MaxSdk.SetSdkKey("3VQ4TIs0L-iEa-cZIosrUoRFAouBezHiTinx_dyLbeaBcBlw-blTOBkZ-blcT0J1i5zikv2JTdUAqE4prqGD0-");
//            MaxSdk.SetUserId(YZNativeUtil.GetYZAFID());
//            YZDebug.Log("Root.Instance.HaveAdRoom " + Root.Instance.HaveAdRoom);
//#if UNITY_ANDROID
//            if (YZAndroidPlugin.Shared.AndroidPingGoogle())
//            {
//                YZDebug.Log("【MAX】MAX 初始化开始");
//                MaxSdk.InitializeSdk();
//            }
//#elif UNITY_IOS
             
//                    YZDebug.Log("【MAX】MAX 初始化开始");
//                    MaxSdk.InitializeSdk();
              
//#endif
//#endif

        }

        private void OnApplicationPause(bool pause)
        {
            //IronSource.Agent.onApplicationPause(pause);
        }

#region reward

        public void ShowRewardAd(string adpos, Action<AdsStatus> action)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            brrewardadpos = adpos;
            action?.Invoke(AdsStatus.REWARD);
        YZDebug.Log(YZString.Concat("【MAX】激励 准备展示: ", adpos));
#else
        brrewarded = false;
        brrewardadpos = adpos;
        brrewardaction = action;
        // if (IronSource.Agent.isRewardedVideoAvailable())
        // {
        //     brcurrentadsinfo = null;
        //     IronSource.Agent.showRewardedVideo();
        //     IronSource.Agent.SetPauseGame(true);
        //     YZGameUtil.PrintBuglyLog("Play Ads");
        // }
        // else
        // {
        //     action?.Invoke(AdsStatus.NONE);
        // }

        string placement = adpos + "_" + netWork;
        YZDebug.Log("AD Placement = " + placement);
        //string adId = "b76d2969f1b8ecd7";
        string adId = adUnitId;
        if (MaxSdk.IsRewardedAdReady(adId))
        {
            brcurrentadsinfo = null;
            MaxSdk.ShowRewardedAd(adId, placement);
            //MaxSdk.
            //IronSource.Agent.SetPauseGame(true);
            YZGameUtil.PrintBuglyLog("Play Ads");

        }else
        {
            action?.Invoke(AdsStatus.NONE);

        }    

#endif
        }

        void YZRewardedVideoAdOpenedEvent()
        {
            YZDebug.Log("【IRS】激励 已经打开");
        }

        // void YZRewardedVideoAdClickedEvent(IronSourcePlacement placement)
        // {
        //     YZDebug.Log(YZString.Concat("【IRS】激励 被点击了: ", placement.ToString()));
        // }

        void YZRewardedVideoAdClosedEvent()
        {
            YZDebug.Log("【IRS】激励 已经关闭");
            //特殊处理 , 激励奖励可能有延时
            Timer.Register(0.5f, () => brrewardaction?.Invoke(brrewarded ? AdsStatus.REWARD : AdsStatus.NOCOMPLETED));
            //IronSource.Agent.SetPauseGame(false);
            YZGameUtil.PrintBuglyLog("Ads Closed");
        }

        void YZRewardedVideoAvailabilityChangedEvent(bool available)
        {
            YZDebug.Log(YZString.Concat("【IRS】激励 状态变更: ", available));
        }

        void YZRewardedVideoAdStartedEvent()
        {
            YZDebug.Log("【IRS】激励 开始播放");
        }

        void YZRewardedVideoAdEndedEvent()
        {
            YZDebug.Log("【IRS】激励 播放完成");
        }

        public void Reset()
        {
            MaxSdkCallbacks.ClearInitEvent();
        }

        //         void YZRewardedVideoAdRewardedEvent(IronSourcePlacement placement)
//         {
//             YZDebug.Log(YZString.Concat("【IRS】激励 收到奖励: ", placement.ToString()));
//             brrewarded = true;
//             brrewardaction?.Invoke(brrewarded ? AdsStatus.REWARD : AdsStatus.NOCOMPLETED);
//             brrewardaction = null;
//
//             if (brcurrentadsinfo != null)
//             {
//                 decimal revenue = brcurrentadsinfo.revenue.HasValue ? (decimal)brcurrentadsinfo.revenue : 0;
// #if RELEASE
//                 if (!YZDebug.IsWhiteListTestDevice())
//                     YZServerCommon.Shared.SendYZWorth(brcurrentadsinfo.allData, this.brrewardadpos, revenue, "ironsource");
// #endif
//             }
//         }

        // void YZRewardedVideoAdShowFailedEvent(IronSourceError error)
        // {
        //     YZDebug.Log(YZString.Concat("【IRS】激励 播放失败: ", error.getCode(), " error: ", error.getDescription()));
        //     brrewardaction?.Invoke(AdsStatus.ERROR);
        //
        //     //无意义
        //     IronSource.Agent.SetPauseGame(false);
        // }

#endregion
    }
}