using Core.Controllers;
using Core.Services.PersistService.API.Facade;
using DataAccess.Utils.Static;
using Utils;

namespace Core.Manager
{
    using System.Collections.Generic;
    using AndroidCShape;
    using iOSCShape;
    using LitJson;
    using UnityEngine;

    public class PlayerManager : MonoBehaviour
    {
        private static PlayerManager YZInstance;

        public GameObject Viewer;

        public YZPlayerDataUtil User = new YZPlayerDataUtil();
        //
        // public YZAI AI = new YZAI();
        //
        public YZADS Ads = new YZADS();
        //
        // public YZKYC Kyc = new YZKYC();
        //
        // public YZRoom Room = new YZRoom();
        //
        // public YZSFace Face = new YZSFace();
        //
        public YZActivity Activity = new YZActivity();
        //
        // public YZPlayer Player = new YZPlayer();
        //
        // public YZConfig Config = new YZConfig();
        //
        // public YZCharge Charge = new YZCharge();
        //
        // public YZBingocups Cup = new YZBingocups();
        //
        // public YZRefresh Refresh = new YZRefresh();
        //
        // public YZABGroup ABGroup = new YZABGroup();
        //
        // public YZCollect Collect = new YZCollect();
        //
        // public YZGlobalVar Global = new YZGlobalVar();
        //
        public YZLocations Location = new YZLocations();

        public static PlayerManager Shared
        {
            get { return YZInstance; }
        }

        // public YZMoneyBox Moneybox
        // {
        //     get
        //     {
        //         if (Player.Other.moneybox_info != null)
        //         {
        //             return Player.Other.moneybox_info;
        //         }
        //
        //         return new YZMoneyBox();
        //     }
        // }

        private void Awake()
        {
            YZInstance = this;
            DontDestroyOnLoad(this);
            //Viewer.SetActive(YZDefineUtil.IsDebugger);
            //YZSDKsController.Shared.YZInitSDKAndConfig();
        }

//         public void SetYZPlayer(YZPlayer player, bool pass)
//         {
//             if (player != null)
//             {
//                 Player = player;
//                 // 1.token
//                 User.YZPlayerAuth = Player.data.user.authorization;
//                 User.YZCountry = player.data.other.country;
//                 User.YZUserID = player.data.user.user_id;
//                 // 2.登录奖励
//                 SFaceManager.Shared.YZSetVipReward(player.data.user.vip.reward);
//                 // 3.跨天检查
//                 YZUpdateLoginDays();
//                 // 4.缓存清理
//                 YZClearCaches();
//                 if (!pass)
//                 {
//                     // 登录战斗
//                     SFaceManager.Shared.YZSetLastLoginPlayGame(User.YZLoginGameCount > 0);
//                     // 登录局数
//                     User.YZLoginGameCount = 0;
//                     // 归因判断
//                     YZUpdateOrganic();
//                     // 数数打点
//                     YZFunnelThinking();
//                 }
//
//                 // 5.登录通用后台
//                 YZServerCommon.Shared.SendYZLogin();
//                 // 6.排行榜历史记录
//                 BingocupManager.Shared.YZUpdateRecordRankList();
//                 // 7.上传设备信息
//                 YZServerApi.Shared.YZSetUserDeviceInfo(
//                     null,
//                     YZNativeUtil.GetYZTimeZone(),
//                     YZNativeUtil.GetYZLocalCountryCode(),
//                     YZNativeUtil.GetYZSimInfo(),
//                     YZNativeUtil.GetYZLanguage()
//                 );
// #if UNITY_ANDROID || UNITY_EDITOR
//                 // 8.安卓推送初始化
//                 YZAndroidPushPlugin.Shared.AndroidYZInitPush();
// #endif
//                 // 10.日志上报
//                 BuglyAgent.SetUserId(player.data.user.user_id);
//             }
//             else
//             {
//                 YZTopControl.YZShowDebugAutoHideTips("解析登录数据失败，直接弹断网重联");
//                 YZTopControl.YZLostConnect();
//             }
//         }
//
//         public void SetYZConfig(YZConfig config, bool pass)
//         {
//             if (config != null)
//             {
//                 Config = config;
//
//                 // 刷新通知
//                 this.PostNotification(new YZNotifyObj(YZNotifyName.YZRefreshBalance));
//                 this.PostNotification(new YZNotifyObj(YZNotifyName.YZRefreshTicket));
//                 this.PostNotification(new YZNotifyObj(YZNotifyName.YZRefreshLevel));
//
//                 // 红点刷新
//                 YZRedPointController.Shared.YZRefreshMiniGameRedPoint();
//                 YZMallUICtrler.Shared().YZRefreshTimeRewards();
//
//                 // 激活破冰充值
//                 SFaceManager.Shared.YZTryActiveIceBreaker();
//
//                 if (!pass)
//                 {
//                     // 初始化AI
//                     AIHelpManager.Shared.AIHelpBeginInit();
//                 }
//             }
//             else
//             {
//                 YZTopControl.YZShowDebugAutoHideTips("解析配置数据失败，直接弹断网重联");
//                 YZTopControl.YZLostConnect();
//             }
//         }
//
//         private void YZUpdateLoginDays()
//         {
//             if (User.YZServerLoginDays > 0 && int.Parse(Player.User.login_days) != User.YZServerLoginDays)
//             {
//                 User.YZDailyGameCount = 0;
//                 GlobalVarManager.Shared.YZIsBingoPartyClaim = false;
//                 GlobalVarManager.Shared.YZIsBingoSlotsClaim = false;
//                 PlayerPrefs.SetInt(YZConstUtil.YZDailyBestValueCloseInt, 0);
//                 PlayerPrefs.SetInt(YZConstUtil.YZRateDayTimesInt, 0);
//                 PlayerPrefs.SetInt(YZConstUtil.YZPushDayTimesInt, 0);
//                 PlayerPrefs.SetInt(YZConstUtil.YZUpgradeDialogTimesInt, 0);
//                 PlayerPrefs.SetInt(YZConstUtil.YZDailyHighCount, 0);
//                 PlayerPrefs.SetString(YZConstUtil.YZDailyChargeShowTimes, "");
//                 ChargeManager.Shared.ChangeChagre2001SkinType();
//             }
//
//             User.YZServerLoginDays = int.Parse(Player.User.login_days);
//         }
//
//         private void YZUpdateOrganic()
//         {
//             if (Player.User.is_organic == 0)
//             {
//                 YZServerApiOrganic.Shared.TryToOrganic(2, YZOrganic.YZNONORGANIC, "", "");
//             }
//             else if (Player.User.is_organic == 1)
//             {
//                 YZServerApiOrganic.Shared.TryToOrganic(2, YZOrganic.YZORGANIC, "", "");
//             }
//             else if (Player.User.is_organic == -1)
//             {
//                 YZServerApiOrganic.Shared.TryToOrganic(2, YZOrganic.YZNONE, "", "");
//             }
//         }
//
//         private void YZFunnelThinking()
//         {
//             YZFunnelUtil.ThinkingYZCalibrateTime();
//             YZFunnelUtil.ThinkingYZLogin(YZUserId);
//         }
//
//         private void YZClearCaches()
//         {
//             PlayerPrefs.SetString(YZConstUtil.YZGameBeginMatchIdStr, "");
//             User.YZBingoGameCache = "";
//         }
//
//         public Balance GetYZUserBalance()
//         {
//             if (Player == null || Player.data == null || Player.data.user == null || Player.data.user.balance == null)
//             {
//                 Balance b = new Balance();
//                 b.bonus = 0;
//                 b.chips = 0;
//                 b.money = 0;
//                 return b;
//             }
//
//             return Player.data.user.balance;
//         }
//
//         public bool GetYZNeedTongdun(YZSafeType type)
//         {
//             if (Config.data.other.system_switch != null && Config.data.other.system_switch.tongdun == 1)
//             {
//                 if (YZServerApiOrganic.Shared.IsYZOrganic() && YZGameUtil.GetIsiOS() && type != YZSafeType.Withdraw)
//                 {
//                     return Player.Other.safety_data != null && Player.Other.safety_data.tongdun != null &&
//                            Player.Other.safety_data.tongdun.GetEnded() < YZServerApi.Shared.GetYZServerTime();
//                 }
//             }
//
//             return false;
//         }
//
//         public bool GetYZNeedKYCWithdraw(double amount)
//         {
//             if (Config.data.other.system_switch != null && Config.data.other.system_switch.kyc == 1)
//             {
//                 if (YZServerApiOrganic.Shared.IsYZOrganic() && YZGameUtil.GetIsiOS())
//                 {
//                     if (Player.Other.safety_data != null && Player.Other.safety_data.kyc.cash_sum + amount >=
//                         Player.Other.safety_data.kyc.cash_limit)
//                     {
//                         return true;
//                     }
//                 }
//             }
//
//             return false;
//         }
//
//         public bool GetYZNeedKYCChagreMoney(YZPlacemark pl)
//         {
//             if (Config.data.other.system_switch != null && Config.data.other.system_switch.kyc == 1)
//             {
//                 if (YZServerApiOrganic.Shared.IsYZOrganic() && YZGameUtil.GetIsiOS())
//                 {
//                     if (Config.data.other.kyc_not_available != null && pl != null)
//                     {
//                         foreach (string item in Config.data.other.kyc_not_available)
//                         {
//                             if (pl.administrativeArea == item)
//                             {
//                                 return true;
//                             }
//                         }
//                     }
//                 }
//             }
//
//             return false;
//         }
//
//         public int GetYZSlotsTicketNumber(int room_id)
//         {
//             if (Config == null || Config.SlotsConfg == null || Config.SlotsConfg.room_get == null)
//                 return 0;
//             SlotsBingo config = Config.SlotsConfg;
//             if (config.room_get.ContainsKey(YZString.Concat(room_id)))
//             {
//                 return config.room_get[YZString.Concat(room_id)];
//             }
//
//             return 0;
//         }
//
//         public int GetYZTicketAmount(int type)
//         {
//             if (type == TicketType.card_ticket)
//             {
//                 return int.Parse(Player.Other.ticket.card_ticket);
//             }
//             else if (type == TicketType.wheel_ticket_junior)
//             {
//                 return int.Parse(Player.Other.ticket.wheel_ticket_junior);
//             }
//             else if (type == TicketType.wheel_ticket_senior)
//             {
//                 return int.Parse(Player.Other.ticket.wheel_ticket_senior);
//             }
//             else if (type == TicketType.active_charge_game)
//             {
//                 return int.Parse(Player.Other.ticket.active_charge_game);
//             }
//             else if (type == TicketType.active_charge_t1)
//             {
//                 return int.Parse(Player.Other.ticket.active_charge_t1);
//             }
//             else if (type == TicketType.active_charge_t2)
//             {
//                 return int.Parse(Player.Other.ticket.active_charge_t2);
//             }
//             else if (type == TicketType.active_charge_t3)
//             {
//                 return int.Parse(Player.Other.ticket.active_charge_t3);
//             }
//             else if (type == TicketType.active_charge_t4)
//             {
//                 return int.Parse(Player.Other.ticket.active_charge_t4);
//             }
//             else if (type == TicketType.active_charge_t5)
//             {
//                 return int.Parse(Player.Other.ticket.active_charge_t5);
//             }
//             else if (type == TicketType.active_charge_t6)
//             {
//                 return int.Parse(Player.Other.ticket.active_charge_t6);
//             }
//
//             return 0;
//         }
//
//         public bool GetYZIsPlayer()
//         {
//             if (Player == null || Player.data == null)
//             {
//                 return false;
//             }
//
//             return true;
//         }
//
//         public bool GetYZIsConfig()
//         {
//             if (Config == null || Config.data == null)
//             {
//                 return false;
//             }
//
//             return true;
//         }
//
//         /// <summary>
//         /// 获取vip奖励，每日奖励除外
//         /// </summary>
//         /// <param name="lv"></param>
//         /// <param name="type">1.转盘 2.高级转盘 3.充值</param>
//         /// <returns></returns>
//         public string GetYZVipBenefitsOther(int lv, int type)
//         {
//             List<double> list;
//             if (type == 1)
//             {
//                 list = Config.VipBenefits.wheel_junior_energy;
//             }
//             else if (type == 2)
//             {
//                 list = Config.VipBenefits.wheel_senior_energy;
//             }
//             else
//             {
//                 list = Config.VipBenefits.deposit_bonus;
//             }
//
//             return list[lv - 1].ToString();
//         }
//
//         /// <summary>
//         /// 获取vip每日奖励
//         /// </summary>
//         /// <param name="lv"></param>
//         /// <returns></returns>
//         public JsonData GetYZVipBenefitsDaily(int lv)
//         {
//             return Config.VipBenefits.daily_gift[lv - 1];
//         }
//
//         public int GetYZEmialStatus()
//         {
//             if (Player.User.account == null)
//             {
//                 return 0;
//             }
//
//             if (Player.User.account.validate == 0)
//             {
//                 return 1;
//             }
//
//             return 2;
//         }
//
//         public int GetYZHyperWalletStatus()
//         {
//             if (Player.Other.hyper_wallet == null || string.IsNullOrEmpty(Player.Other.hyper_wallet.email))
//             {
//                 return 0;
//             }
//
//             if (Player.Other.hyper_wallet.validate == 0)
//             {
//                 return 1;
//             }
//
//             if (Player.Other.hyper_wallet.account_ready == 0)
//             {
//                 return 2;
//             }
//
//             return 3;
//         }
//
//         public string GetYZHyperProvince(string key)
//         {
//             if (Config.HyperData == null || Config.HyperData.hyper_province == null)
//             {
//                 return default;
//             }
//
//             if (YZJsonUtil.ContainsYZKey(Config.HyperData.hyper_province, key))
//             {
//                 return Config.HyperData.hyper_province[key].ToString();
//             }
//             else
//             {
//                 return key;
//             }
//         }
//
//         public float GetYZWithdrawFee(float amount)
//         {
//             float result = 0;
//             float amountCloser = 0;
//             foreach (Cash_feeItem item in Config.CashFee)
//             {
//                 if (item.cash_amount <= amount && item.cash_amount >= amountCloser)
//                 {
//                     amountCloser = item.cash_amount;
//                     result = (float)item.fee;
//                 }
//             }
//
//             return result;
//         }
//
//         public bool YZIsNeedShowUnlock(ListItem room)
//         {
//             if (ABGroupManager.Shared.YZIsBCGroup(ABTagName.room_show_1201) && room.IsLocked())
//             {
//                 // B组，且没有解锁
//                 return false;
//             }
//
//             if (room != null && Player.Other.deblock_cartoon != null &&
//                 (room.sub_title == RoomType.diamonds || room.sub_title == RoomType.money))
//             {
//                 return !Player.Other.deblock_cartoon.Contains(room.id);
//             }
//
//             return false;
//         }
//
//         #region Player数据获取接口
//
//         public string YZUserName
//         {
//             get => Player.User.user_info.name;
//         }
//
         public string YZUserId
         {
             //get => PersistSystem.That.GetValue<string>(GlobalEnum.UID) as string;
             get => "123";
         }
//
//         public double YZTotalMoney
//         {
//             get => Player.User.balance.Money;
//         }
//
//         public int YZAverageScore
//         {
//             get => Player.Other.count.average_score;
//         }
//
//         public int YZBestScoreToday(int current)
//         {
//             int result = Player.Other.count.daily_max_score;
//             if (current > result)
//                 result = current;
//             return result;
//         }
//
//         public int YZBestScoreAllTime(int current)
//         {
//             int result = Player.Other.count.total_max_score;
//             if (current > result)
//                 result = current;
//             return result;
//         }
//
//         public void YZGetCurrentTimeReward(ref List<YZReward> listReward)
//         {
//             var rewardData = Player.Other.time_reward.next_rewards;
//             foreach (var key in rewardData.Keys)
//             {
//                 YZReward item = new YZReward(int.Parse(key), YZJsonUtil.GetYZFloat(rewardData, key));
//                 listReward.Add(item);
//             }
//         }
//
//         /// <summary>
//         /// 获取转盘奖励配置
//         /// </summary>
//         /// <param name="type">类型</param>
//         /// <param name="index">为数组下标，从0到7</param>
//         /// <returns></returns>
//         public YZReward YZGetWheelRewardData(int type, int index)
//         {
//             if (type == TicketType.wheel_ticket_junior)
//             {
//                 if (index < 0 || index >= Config.WheelRewards.wheel_junior_rewards.Count)
//                     return null;
//                 var data = Config.WheelRewards.wheel_junior_rewards[index];
//                 foreach (var key in data.Keys)
//                 {
//                     return new YZReward(int.Parse(key), YZJsonUtil.GetYZFloat(data, key));
//                 }
//             }
//             else if (type == TicketType.wheel_ticket_senior)
//             {
//                 if (index < 0 || index >= Config.WheelRewards.wheel_senior_random.Count)
//                     return null;
//                 var data = Config.WheelRewards.wheel_senior_random[index];
//                 foreach (var key in data.Keys)
//                 {
//                     return new YZReward(int.Parse(key), YZJsonUtil.GetYZFloat(data, key));
//                 }
//             }
//
//             return null;
//         }
//
//         /// <summary>
//         /// 根据类型获取转盘奖励id:从1到8
//         /// </summary>
//         /// <param name="type"></param>
//         /// <returns></returns>
//         public int YZGetWheelRewardID(int type)
//         {
//             if (type == TicketType.wheel_ticket_junior)
//             {
//                 return Player.Other.wheel.next_junior_reward;
//             }
//             else if (type == TicketType.wheel_ticket_senior)
//             {
//                 return Player.Other.wheel.next_senior_reward;
//             }
//
//             return 1;
//         }
//
//         /// <summary>
//         /// 是否完成所有新手引导
//         /// </summary>
//         /// <returns></returns>
//         public bool YZIsFinishAllGuide()
//         {
//             if (YZIsShowCashGuide())
//             {
//                 return YZGetLocalGuideStep() >= 11;
//             }
//             else
//             {
//                 return YZGetLocalGuideStep() >= 6;
//             }
//         }
//
//         /// 是否展示美金引导
//         public bool YZIsShowCashGuide()
//         {
//             return YZServerApiOrganic.Shared.IsYZShowMoney() && ABGroupManager.Shared.YZIsBCGroup(ABTagName.guide_1213);
//         }
//
//         /// <summary>
//         /// 新手引导步骤(钻石) 0.进入大厅 1.弹框强引导 2.20钻强引导 3.20钻房间弱引导 4.20钻房间名次高亮强引导 5.20钻房间play按钮点击 6.所有引导完成
//         /// 新手引导步骤(美金) 0.进入大厅 1.强引导高亮美金房间 2.开始匹配美金局 3.进入美金局 4.提交分数 5.高亮玩家名次+提示窗 6.领取奖励 7.确认奖励 8.弱引导高亮1美金场+小手指 9.首充弹窗 10.小手指引导1美金场-弱引 11.所有引导完成
//         /// </summary>
//         /// <returns></returns>
//         public int YZGetLocalGuideStep()
//         {
//             if (Player.Other.count.game_end_count >= 2)
//             {
//                 return YZIsShowCashGuide() ? 11 : 6;
//             }
//
//             return PlayerPrefs.GetInt(YZConstUtil.YZBGroupGuideStepInt, 0);
//         }
//
//         /// <summary>
//         /// 进入引导游戏
//         /// </summary>
//         public void YZEnterGuideGame()
//         {
//             if (!YZIsShowCashGuide())
//             {
//                 // 开启匹配首句钻石局
//                 YZAdaptionUICtrler.Shared().YZOnOpenUI(RoomManager.Shared.GetYZTutorialRoom());
//             }
//             else
//             {
//                 // 直接进入新手引导，不需要匹配对局
//                 YZAdaptionUICtrler.Shared().YZStartCashGuide();
//             }
//         }
//
//         #endregion
//
//         #region 打点分析数据（为了数据持久化需求，不用静态函数）
//
//         private JsonData cache_save_data;
//
//         /// <summary>
//         /// 获取客户端暂存的打点分析数据
//         /// </summary>
//         /// <param name="name"></param>
//         /// <returns></returns>
//         public int GetSavedCount(string name)
//         {
//             if (cache_save_data == null)
//             {
//                 if (string.IsNullOrEmpty(YZPlayerDataUtil.Shared.YZAnalytics))
//                 {
//                     cache_save_data = new JsonData();
//                 }
//                 else
//                 {
//                     cache_save_data = JsonMapper.ToObject(YZPlayerDataUtil.Shared.YZAnalytics);
//                 }
//             }
//
//             if (YZJsonUtil.ContainsYZKey(cache_save_data, name))
//             {
//                 return YZJsonUtil.GetYZInt(cache_save_data, name);
//             }
//
//             return 0;
//         }
//
//         /// <summary>
//         /// 增加打点分析数据的计数
//         /// </summary>
//         /// <param name="name"></param>
//         public void AddSavedCount(string name)
//         {
//             if (cache_save_data == null)
//             {
//                 if (string.IsNullOrEmpty(YZPlayerDataUtil.Shared.YZAnalytics))
//                 {
//                     cache_save_data = new JsonData();
//                 }
//                 else
//                 {
//                     cache_save_data = JsonMapper.ToObject(YZPlayerDataUtil.Shared.YZAnalytics);
//                 }
//             }
//
//             if (!YZJsonUtil.ContainsYZKey(cache_save_data, name))
//             {
//                 cache_save_data[name] = 0;
//             }
//
//             //--1.获取当前值
//             int dataValue = YZJsonUtil.GetYZInt(cache_save_data, name);
//             //-- 2.增加当前值
//             dataValue += 1;
//             cache_save_data[name] = dataValue;
//             //-- 3.存档
//             YZPlayerDataUtil.Shared.YZAnalytics = cache_save_data.ToJson();
//         }
//
//         #endregion
//
//         public bool GetYZIsPlayingGame()
//         {
//             if (Player.data != null && Player.Other.playing_game_info != null &&
//                 YZJsonUtil.ContainsYZKey(Player.Other.playing_game_info, "room_id"))
//             {
//                 int status = int.Parse(Player.Other.playing_game_info["status"].ToString());
//                 return status == 1 || status == 2;
//             }
//
//             return false;
//         }
//
//         public bool GetYZMissionHaveFinsh(int day)
//         {
//             if (Player.Other.sevendays == null)
//             {
//                 return false;
//             }
//
//             if (day == 1)
//             {
//                 foreach (Days_Item reward in Player.Other.sevendays.days_1)
//                 {
//                     if (reward.status == 1)
//                     {
//                         return true;
//                     }
//                 }
//             }
//             else if (day == 2)
//             {
//                 foreach (Days_Item reward in Player.Other.sevendays.days_2)
//                 {
//                     if (reward.status == 1)
//                     {
//                         return true;
//                     }
//                 }
//             }
//             else if (day == 3)
//             {
//                 foreach (Days_Item reward in Player.Other.sevendays.days_3)
//                 {
//                     if (reward.status == 1)
//                     {
//                         return true;
//                     }
//                 }
//             }
//             else if (day == 4)
//             {
//                 foreach (Days_Item reward in Player.Other.sevendays.days_4)
//                 {
//                     if (reward.status == 1)
//                     {
//                         return true;
//                     }
//                 }
//             }
//             else if (day == 5)
//             {
//                 foreach (Days_Item reward in Player.Other.sevendays.days_5)
//                 {
//                     if (reward.status == 1)
//                     {
//                         return true;
//                     }
//                 }
//             }
//             else if (day == 6)
//             {
//                 foreach (Days_Item reward in Player.Other.sevendays.days_6)
//                 {
//                     if (reward.status == 1)
//                     {
//                         return true;
//                     }
//                 }
//             }
//             else if (day == 7)
//             {
//                 foreach (Days_Item reward in Player.Other.sevendays.days_7)
//                 {
//                     if (reward.status == 1)
//                     {
//                         return true;
//                     }
//                 }
//             }
//
//             return false;
//         }
//
//         public List<Days_Item> YZGetSevenDayData(int day)
//         {
//             if (Player.Other.sevendays == null)
//             {
//                 return null;
//             }
//
//             if (day == 1)
//             {
//                 return Player.Other.sevendays.days_1;
//             }
//             else if (day == 2)
//             {
//                 return Player.Other.sevendays.days_2;
//             }
//             else if (day == 3)
//             {
//                 return Player.Other.sevendays.days_3;
//             }
//             else if (day == 4)
//             {
//                 return Player.Other.sevendays.days_4;
//             }
//             else if (day == 5)
//             {
//                 return Player.Other.sevendays.days_5;
//             }
//             else if (day == 6)
//             {
//                 return Player.Other.sevendays.days_6;
//             }
//             else if (day == 7)
//             {
//                 return Player.Other.sevendays.days_7;
//             }
//
//             return null;
//         }
//
//         public int YZGetAllSevenDaysNumber()
//         {
//             if (Player.Other.sevendays == null)
//             {
//                 return 0;
//             }
//
//             return Player.Other.sevendays.days_1.Count +
//                    Player.Other.sevendays.days_2.Count +
//                    Player.Other.sevendays.days_3.Count +
//                    Player.Other.sevendays.days_4.Count +
//                    Player.Other.sevendays.days_5.Count +
//                    Player.Other.sevendays.days_6.Count +
//                    Player.Other.sevendays.days_7.Count;
//         }
//
//         public int YZGetReward(JsonData jsonData, out List<YZReward> rewards)
//         {
//             rewards = new List<YZReward>();
//             if (jsonData == null)
//             {
//                 return 0;
//             }
//
//             if (jsonData.IsObject)
//             {
//                 rewards = new List<YZReward>();
//                 foreach (string key in jsonData.Keys)
//                 {
//                     if (double.TryParse(YZString.Concat(jsonData[key]), out double d))
//                     {
//                         rewards.Add(new YZReward(int.Parse(key), d));
//                     }
//                 }
//
//                 return rewards.Count;
//             }
//
//             return 0;
//         }
//
//         public bool YZIsNeedShowRate()
//         {
//             int count = Player.Other.count.game_end_count;
//             if (count < 3)
//             {
//                 return false;
//             }
//
//             if (YZServerApiOrganic.Shared.IsYZOrganic())
//             {
//                 return false;
//             }
//
//             int rates = PlayerPrefs.GetInt(YZConstUtil.YZRateScoreInt, 0);
//             int times = PlayerPrefs.GetInt(YZConstUtil.YZRateDayTimesInt, 0);
//             if (rates >= 4)
//             {
//                 return false;
//             }
//
//             if (times >= 1)
//             {
//                 return false;
//             }
//
//             return true;
//         }
//
//         public bool Is3DayLogin()
//         {
//             if (Player.Other.safety_data == null || Player.Other.safety_data.kyc == null)
//                 return false;
//
//             return Player.Other.safety_data.kyc.login_force == 1;
//         }
//
//         public bool IsHighRiskChips()
//         {
//             if (YZDataUtil.GetYZInt(YZConstUtil.YZHighRiskChargeSucc, 0) == 1)
//             {
//                 return YZDataUtil.GetYZInt(YZConstUtil.YZHighRiskChagreChipsGames, 0) >= 1;
//             }
//
//             return false;
//         }
//
//         public bool IsHighRiskPlayer()
//         {
//             if (Player.Other.safety_data == null || Player.Other.safety_data.kyc == null)
//                 return false;
//
//             return Player.Other.safety_data.kyc.high_risk == 1;
//         }
//
//         #region Fish
//
//         /// <summary>
//         /// 获取钓鱼活动的剩余时间
//         /// </summary>
//         /// <returns></returns>
//         public int GetFishEventTimstampOffset()
//         {
//             var fishInfo = Player.Other.fish_info;
//             if (fishInfo == null)
//             {
//                 return 0;
//             }
//
//             var serverTime = YZServerApi.Shared.GetYZServerTime();
//             return fishInfo.end_timestamp - serverTime;
//         }
//
//         public int GetFishEventChargeTimes()
//         {
//             var fishInfo = Player.Other.fish_info;
//             if (fishInfo == null)
//             {
//                 return 0;
//             }
//
//             return Player.Other.fish_info.charge_times;
//         }
//
//         public bool GetFishHasLoginReward()
//         {
//             var fishInfo = Player.Other.fish_info;
//             if (fishInfo == null)
//             {
//                 return false;
//             }
//
//             return Player.Other.fish_info.login_get == 1;
//         }

        //#endregion
    }
}