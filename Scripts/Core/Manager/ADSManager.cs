using System;
using System.Collections.Generic;
using Core.Controllers;
using Core.Controls;
using Core.Manager;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Third.I18N;
using DataAccess.Model;
using UI;
using UnityEngine;
using Utils;

public class ADSManager
{
    public static YZADS Shared
    {
        get => PlayerManager.Shared.Ads;
    }
}

public class YZADSPos
{
    public const string YZRewardAdsPosTest = "200"; // 激励 测试入口
    public const string YZRewardAdsPosRoom = "201"; // 激励 房间入口
    public const string YZRewardAdsPosMall = "202"; // 激励 商城入口-bonus
    public const string YZRewardAdsPosMallChips = "203"; // 激励 商城入口-chips
    public const string YZRewardAdsPosRoomChips = "204"; // 激励 商城入口-chips
}

/// <summary>
/// 广告管理
/// </summary>
public class YZADS
{
    public void YZShowReward(string pos, Action<AdsStatus> back)
    {
        pos = (pos == "ADRoom" ? "1" : "2");
        Dictionary<string, object> properties = new Dictionary<string, object>()
        {
            {"ad_id", pos},
        };
        YZFunnelUtil.SendYZEvent("ad_start", properties);
        
        YZAdsController.Shared.ShowRewardAd(pos, (status) => {
            if (status == AdsStatus.NONE)
            {
                UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("ads_none"));
                YZFunnelUtil.SendYZEvent("ad_not_ready", properties);
            }
            else if (status == AdsStatus.NOCOMPLETED)
            {
                UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("ads_no_reward"));
                YZFunnelUtil.SendYZEvent("ad_failed", properties);
            }
            else if (status == AdsStatus.ERROR)
            {
                UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("ads_fail"));
                YZFunnelUtil.SendYZEvent("ad_failed", properties);
            }
            else if (status == AdsStatus.REWARD)
            {
#if UNITY_ANDROID || UNITY_IOS
                double worth = YZAdsController.Shared.brcurrentadsinfo?.Revenue ?? 0;
                properties.Add("ad_worth", worth);
                YZDebug.Log("数数打点 ad_worth = " + worth);
                YZFunnelUtil.SendYZEvent("ad_success", properties);

                Dictionary<string, object> propertiesDone = new Dictionary<string, object>()
                {
                    { "bankroll", Root.Instance.Role.GetDollars() },
                    { "bankroll_coin", Root.Instance.Role.GetItemCount(4)},
                    { "revenue", worth},
                    { "af_id", YZNativeUtil.GetYZAFID()},
                    { "ad_id", 1}
                };
                YZFunnelUtil.SendYZEvent("ad_done", propertiesDone);
#endif
            }

            if (status != AdsStatus.NONE && status != AdsStatus.ERROR)
            {
                YZFunnelUtil.SendYZEvent("ad_pop", properties);
            }

            back(status);
        });
    }

    public string YZGetAdsInfo()
    {
        if (YZAdsController.Shared.brcurrentadsinfo != null)
        {
            YZADSInfo dict = new YZADSInfo();
            // dict.ad_pos = YZAdsController.Shared.brrewardadpos;
            // dict.network_id = YZAdsController.Shared.brcurrentadsinfo.adUnit;
            // dict.network_name = YZAdsController.Shared.brcurrentadsinfo.adNetwork;
            // dict.network_worth = (YZAdsController.Shared.brcurrentadsinfo.revenue ?? 0).ToString();
            return JsonUtility.ToJson(dict);
        }
        else
        {
#if UNITY_EDITOR
            YZADSInfo dict = new YZADSInfo();
            dict.ad_pos = YZAdsController.Shared.brrewardadpos;
            dict.network_id = "test_network_id";
            dict.network_name = "test_network_name";
            dict.network_worth = "0.1";
            string ss = JsonUtility.ToJson(dict);
            return JsonUtility.ToJson(dict);
#else
            return "";
#endif
        }
    }

    // public void YZSetRoomAdsCD(int room_id)
    // {
    //     ListItem roomData = RoomManager.Shared.GetYZRoomById(room_id);
    //     if (roomData != null && roomData.sub_title == RoomType.ads)
    //     {
    //         if (roomData.IsEvent())
    //         {
    //             PlayerPrefs.SetInt(YZString.Concat(YZConstUtil.YZRoomAdsPlayTimeInt, room_id), YZServerApi.Shared.GetYZServerTime() + 60 * 10);
    //         }
    //         else
    //         {
    //             PlayerPrefs.SetInt(YZString.Concat(YZConstUtil.YZRoomAdsPlayTimeInt, room_id), YZServerApi.Shared.GetYZServerTime() + 30);
    //         }
    //     }
    // }
    //
    // public int YZGetRoomAdsCD(int room_id)
    // {
    //     int time = PlayerPrefs.GetInt(YZString.Concat(YZConstUtil.YZRoomAdsPlayTimeInt, room_id), 0);
    //     return Math.Max(0, time - YZServerApi.Shared.GetYZServerTime());
    // }
}

[Serializable]
public class YZADSInfo
{
    public string ad_pos;
    public string network_id;
    public string network_name;
    public string network_worth;
}