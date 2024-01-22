using System.Collections.Generic;
using Core.Manager;
using Core.Services.PersistService.API.Facade;
using DataAccess.Controller;
using DataAccess.Utils.Static;
using UnityEngine;

namespace Utils
{
    public class YZPlayerDataUtil
    {
        public static YZPlayerDataUtil Shared
        {
            get => PlayerManager.Shared.User;
        }

        // #region int
        //
        // public int YZServerLoginDays
        // {
        //     get { return YZDataUtil.GetYZInt(YZConstUtil.YZServerLoginDays, 0); }
        //     set { YZDataUtil.SetYZInt(YZConstUtil.YZServerLoginDays, value); }
        // }
        //
        // public int YZDailyGameCount
        // {
        //     get { return YZDataUtil.GetYZInt(YZConstUtil.YZDailyGameCount, 0); }
        //     set { YZDataUtil.SetYZInt(YZConstUtil.YZDailyGameCount, value); }
        // }
        //
        // public int YZLoginGameCount
        // {
        //     get { return YZDataUtil.GetYZInt(YZConstUtil.YZLoginGameCountInt, 0); }
        //     set { YZDataUtil.SetYZInt(YZConstUtil.YZLoginGameCountInt, value); }
        // }
        //
        // #endregion
        //
        // #region bool
        //
        // public bool YZAdviseUpgrade
        // {
        //     get { return YZDataUtil.GetYZInt(YZConstUtil.YZAdviseUpgrade, 0) == 1; }
        //     set { YZDataUtil.SetYZInt(YZConstUtil.YZAdviseUpgrade, value ? 1 : 0); }
        // }
        //
        // public bool YZMusic
        // {
        //     get { return YZDataUtil.GetYZInt(YZConstUtil.YZMusic, 1) == 1; }
        //     set
        //     {
        //         YZDataUtil.SetYZInt(YZConstUtil.YZMusic, value ? 1 : 0);
        //         YZSoundController.Shared.RefreshYZAudioPlay();
        //     }
        // }
        //
        // public bool YZSFX
        // {
        //     get { return YZDataUtil.GetYZInt(YZConstUtil.YZSfx, 1) == 1; }
        //     set { YZDataUtil.SetYZInt(YZConstUtil.YZSfx, value ? 1 : 0); }
        // }
        //
        // public bool YZVibration
        // {
        //     get { return PlayerPrefs.GetInt(YZConstUtil.YZVibration, 1) == 1; }
        //     set { PlayerPrefs.SetInt(YZConstUtil.YZVibration, value ? 1 : 0); }
        // }
        //
        // public bool YZBingoAwaysHit
        // {
        //     get { return YZDataUtil.GetYZInt(YZConstUtil.YZBingoAwaysHit, 0) == 1; }
        //     set { YZDataUtil.SetYZInt(YZConstUtil.YZBingoAwaysHit, value ? 1 : 0); }
        // }
        //
        // #endregion
        //
        // public string YZCountry
        // {
        //     get
        //     {
        //         string country = YZDataUtil.GetLocaling(YZConstUtil.YZCountry);
        //         if (string.IsNullOrEmpty(country))
        //         {
        //             return "US";
        //         }
        //
        //         return country;
        //     }
        //     set { YZDataUtil.SetYZString(YZConstUtil.YZCountry, value); }
        // }

        public string YZPlayerAuth
        {
            // get { return YZDataUtil.GetLocaling(YZConstUtil.YZAuth); }
            // set { YZDataUtil.SetYZString(YZConstUtil.YZAuth, value); }
            get { return MediatorRequest.Instance.GetAuthorization(); }
            //set { PersistSystem.That.SaveValue(GlobalEnum.UID, value);}
        }

        public string YZOrganic
        {
            get { return YZDataUtil.GetLocaling(YZConstUtil.YZOrganic); }
            set { YZDataUtil.SetYZString(YZConstUtil.YZOrganic, value); }
        }
        
        public string YZMediaSource
        {
            get { return YZDataUtil.GetLocaling(YZConstUtil.YZMediaSource); }
            set { YZDataUtil.SetYZString(YZConstUtil.YZMediaSource, value); }
        }
        
        // public string YZAnalytics
        // {
        //     get { return YZDataUtil.GetLocaling(YZConstUtil.YZAnalytics); }
        //     set { YZDataUtil.SetYZString(YZConstUtil.YZAnalytics, value); }
        // }

        public string YZUDID
        {
            get { return PersistSystem.That.GetValue<string>(GlobalEnum.ClientUID) as string; }
            set { PersistSystem.That.SaveValue(GlobalEnum.ClientUID, value); }
        }

        public static void SetDeposit18YearOldInt(bool isOn)
        {
            YZDataUtil.SetYZInt(YZConstUtil.YZDeposit18YearOldInt, isOn ? 1 : 0);
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict.Add(FunnelEventID.brpermissionage, isOn ? 1 : 0);
            YZFunnelUtil.UserYZSet(dict);
        }
        
        // public string YZUserID
        // {
        //     get { return YZDataUtil.GetLocaling(YZConstUtil.YZUserid); }
        //     set { YZDataUtil.SetYZString(YZConstUtil.YZUserid, value); }
        // }
        //
        // #region bingo 游戏的缓存数据
        //
        // /// 游戏数据缓存：分数，操作等
        // public string YZBingoGameCache
        // {
        //     get { return YZDataUtil.GetLocaling(YZConstUtil.YZBingoGameCache); }
        //     set { YZDataUtil.SetYZString(YZConstUtil.YZBingoGameCache, value); }
        // }
        //
        // #endregion
        //
        // public void SaveRoomFee(int room_id)
        // {
        //     ListItem room = RoomManager.Shared.GetYZRoomById(room_id);
        //     YZReward numb = RoomManager.Shared.GetYZRoomTicket(room, 1);
        //     if (numb == null || numb.amount <= 0)
        //         return;
        //
        //     string feestr = PlayerPrefs.GetString(YZConstUtil.YZLastFiveFeeListStr, "");
        //     if (string.IsNullOrEmpty(feestr))
        //     {
        //         YZFivePlayer five = new YZFivePlayer();
        //         five.rooms = new List<float>() { (float)numb.amount };
        //         PlayerPrefs.SetString(YZConstUtil.YZLastFiveFeeListStr, JsonUtility.ToJson(five));
        //     }
        //     else
        //     {
        //         YZFivePlayer five = YZGameUtil.JsonYZToObject<YZFivePlayer>(feestr);
        //         if (five.rooms == null)
        //         {
        //             five.rooms = new List<float>() { (float)numb.amount };
        //         }
        //         else
        //         {
        //             five.rooms.Add((float)numb.amount);
        //         }
        //
        //         if (five.rooms.Count > 5)
        //         {
        //             five.rooms.RemoveAt(0);
        //         }
        //
        //         PlayerPrefs.SetString(YZConstUtil.YZLastFiveFeeListStr, JsonUtility.ToJson(five));
        //     }
        // }
        //
        // public float GetRoomFeeAverage()
        // {
        //     float f = 0;
        //     string feestr = PlayerPrefs.GetString(YZConstUtil.YZLastFiveFeeListStr, "");
        //     YZDebug.LogConcat("最近5场对局: ", feestr);
        //     if (string.IsNullOrEmpty(feestr))
        //     {
        //         return f;
        //     }
        //
        //     YZFivePlayer five = YZGameUtil.JsonYZToObject<YZFivePlayer>(feestr);
        //     if (five == null)
        //     {
        //         return f;
        //     }
        //
        //     if (five.rooms.Count >= 2)
        //     {
        //         five.rooms.Sort((a, b) =>
        //         {
        //             if (Mathf.Approximately(a, b))
        //                 return 0;
        //             else
        //                 return a - b > 0 ? -1 : 1;
        //         });
        //     }
        //
        //     if (five.rooms.Count == 1)
        //     {
        //         return five.rooms[0];
        //     }
        //
        //     if (five.rooms.Count >= 2)
        //     {
        //         return (five.rooms[0] + five.rooms[1]) / 2;
        //     }
        //
        //     return f;
        // }
    }
}