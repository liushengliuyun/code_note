using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using CatLib.EventDispatcher;
using Core.Controllers;
using Core.Extensions;
using Core.Manager;
using Core.Models;
using Core.Services.PersistService.API.Facade;
using Core.Services.UserInterfaceService.API.Facade;
using DataAccess.Controller;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using LitJson;
using UI;
using UI.UIWithDrawFlow;
using UniRx;
using UnityEngine;
using Utils;

namespace DataAccess.Model
{
    public class Root : Singleton<Root>
    {
        //token
        private string authorization;

        /*public bool PushAlarm
        {
            set => PersistSystem.That.SaveValue(GlobalEnum.PUSH_ALARM, value);
            get => !(bool)PersistSystem.That.GetValue<bool>(GlobalEnum.PUSH_ALARM);
        }*/


        /// <summary>
        /// 默认打开
        /// </summary>
        public bool VibrationON
        {
            set => PersistSystem.That.SaveValue(GlobalEnum.VIBRATION, value);
            get => !(bool)PersistSystem.That.GetValue<bool>(GlobalEnum.VIBRATION);
        }

        private string ip;
        
        /// <summary>
        /// 默认打开
        /// </summary>
        public string IP
        {
            set
            {
                if (value.IsNullOrEmpty())
                {
                    return;
                }

                if (!ip.IsNullOrEmpty() && ip != value)
                {
                    LastIP = ip;
                }

                ip = value;
                PersistSystem.That.SaveValue(GlobalEnum.IP, value);
            }
            get
            {
                return PersistSystem.That.GetValue<string>(GlobalEnum.IP) as string;
            }
        }

        public string LastIP
        {
            set
            {
                if (value.IsNullOrEmpty())
                {
                    return;
                }
                PersistSystem.That.SaveValue(GlobalEnum.LastIP, value);
            }
            get
            {
                return PersistSystem.That.GetValue<string>(GlobalEnum.LastIP) as string;
            }
        }


        // private string userAgent ="";
        // public string UserAgent
        // {
        //     set
        //     {
        //         if (value.IsNullOrEmpty())
        //         {
        //             return;
        //         }
        //
        //         userAgent = value;
        //         // PersistSystem.That.SaveValue(GlobalEnum.UserAgent, value);
        //     }
        //     get
        //     {
        //         return userAgent;
        //         // return PersistSystem.That.GetValue<string>(GlobalEnum.UserAgent) as string;
        //     }
        // }
        
        public bool ClickHowToPlay
        {
            set
            {
                PersistSystem.That.SaveValue(GlobalEnum.ClickHowToPlay, value, true);
                EventDispatcher.Root.Raise(GlobalEvent.Click_How_To_Play);
            }
            get => (bool)PersistSystem.That.GetValue<bool>(GlobalEnum.ClickHowToPlay, true);
        }

        public string AuthorizationDebug
        {
            get => authorization;
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                PersistSystem.That.SaveValue(GlobalEnum.AUTHORIZATION_DEBUG, value);
                authorization = value;
            }
        }

        public string AuthorizationRelease
        {
            get => authorization;
            set
            {
                if (string.IsNullOrEmpty(value)) return;
                PersistSystem.That.SaveValue(GlobalEnum.AUTHORIZATION_RELAESE, value);
                authorization = value;
            }
        }

        private Role role;

        public bool IsReachMaxVipLevel => role.VipLevel >= VipConfig.Count;

        private int wheelFreeTicket;

        private bool? isFirstLogin;

        /// <summary>
        /// 是否是今天的第一次登陆
        /// </summary>
        public bool IsFirstLogin
        {
            get
            {
                var lastLoginTime = (int)PersistSystem.That.GetValue<int>(GlobalEnum.LAST_LOGIN_TIME, true);
                if (isFirstLogin == null)
                {
                    isFirstLogin = lastLoginTime < TimeUtils.Instance.BeforeDawn;
                    PersistSystem.That.SaveValue(GlobalEnum.LAST_LOGIN_TIME, TimeUtils.Instance.LocalTimeNow, true);
                }

                return (bool)isFirstLogin;
            }
        }

        /// <summary>
        /// 免费转盘门票数
        /// </summary>
        public int WheelFreeTicket
        {
            set
            {
                wheelFreeTicket = value;
                if (fortuneWheelInfo != null)
                {
                    fortuneWheelInfo.wheel_free_ticket = value;
                }

                EventDispatcher.Root.Raise(GlobalEvent.Sync_WheelFreeTicket);
            }
            get => wheelFreeTicket;
        }

        private FortuneWheelInfo fortuneWheelInfo;

        public FortuneWheelInfo FortuneWheelInfo
        {
            get { return fortuneWheelInfo ??= new FortuneWheelInfo(); }

            set
            {
                fortuneWheelInfo = value;
                EventDispatcher.Root.Raise(GlobalEvent.SYNC_FORTUNE_WHEEL_INFO);
            }
        }

        public int UserId => Role.user_id;

        public bool IsMale => Role.wizard == 2;

        #region ---------------------------------------------- 配置-------------------------------------------------------------

        public Dictionary<int, float> VipConfig;

        /// <summary>
        /// 免费美金场转盘
        /// </summary>
        public List<YZReward> FreeWheelList;

        /// <summary>
        /// 充值转盘
        /// </summary>
        public List<YZReward> SmallPayWheelList;


        public List<YZReward> BigPayWheelList;

        /// <summary>
        /// 充值转盘相关的充值信息
        /// </summary>
        public Dictionary<int, WheelChargeInfo> WheelChargeInfos;

        public List<YZReward> SignAwardsList;

        public List<YZReward> SignHeapAwardsList;
                                                                                                        
        public Dictionary<int, List<TaskConfig>> TaskConfigs;

        public DailyMission DailyMissionConfigs;

        public List<FriendsDuelConfig> FriendsDualConfigs;

        public List<SpecialOfferConfig> SpecialOfferConfig;
        
        public List<AdditionalGiftConfig> AdditionalGiftConfigs;

        public ChargeConfig ChargeConfig;

        /// <summary>
        /// 在线奖励配置
        /// </summary>
        public YZReward[][] OnlineActiveConfig;


        private List<ShopInfo> shopConfig;

        /// <summary>
        /// 商店配置
        /// </summary>
        public List<ShopInfo> ShopConfig
        {
            get => shopConfig;

            set
            {
                shopConfig = value;
                EventDispatcher.Root.Raise(GlobalEvent.SHOP_REFRESH);
            }
        }

        public List<MagicBallData> MagicBallDatas;

        public List<MagicPageReward> MagicPageRewards;

        private MagicBallInfo magicBallInfo;

        public MagicBallInfo MagicBallInfo
        {
            get { return magicBallInfo; }
            set
            {
                var isReachMax = magicBallInfo?.IsReachMax ?? false;

                var oldPoint = magicBallInfo?.magic_essence;
                var newPoint = value?.magic_essence;

                magicBallInfo = value;
                EventDispatcher.Root.Raise(GlobalEvent.SYNC_MAGIC_BALL_INFO);


                if (!isReachMax && newPoint > oldPoint &&
                    MediatorActivity.Instance.IsActivityBegin(ActivityType.MagicBall) && !value.dont_sync_add)
                {
                    MediatorItem.Instance.AddExShowItem(
                        new Item("MagicBallPoint", (float)newPoint - (float)oldPoint));
                    EventDispatcher.Root.Raise(GlobalEvent.MAGIC_BALL_POINT_ADD);
                }

                if (magicBallInfo != null)
                {
                    magicBallInfo.dont_sync_add = false;
                }
                
                if (value?.IsReachMax ?? false)
                {
                    EventDispatcher.Root.Raise(GlobalEvent.MAGIC_BALL_REACH_MAX);
                }
            }
        }

        /// <summary>
        /// 每日礼包
        /// </summary>
        public List<DailyReward> DailyRewardList;

        public string GameGuideList
        {
            get => UserInfo?.game_guide_list;

            set
            {
                if (value.IsNullOrEmpty())
                {
                    return;
                }
                
                if (UserInfo != null)
                {
                    UserInfo.game_guide_list = value;
                }
            }
        }

        private int dailyRewardChance;

        /// <summary>
        /// 每日礼包领取机会
        /// </summary>
        public int DailyRewardChance
        {
            get => dailyRewardChance;
            set
            {
                dailyRewardChance = value;
                EventDispatcher.Root.Raise(GlobalEvent.DAILY_REWARD_CHANCE);
            }
        }

        /// <summary>
        /// 不同充值挡位对应的礼包信息
        /// </summary>
        public Dictionary<int, List<ChargeGoodInfo>> StarterPackConfig;

        public List<ChargeGoodInfo> FreeBonusConfig;

        public List<ChargeGoodInfo> LuckyGuyConfig;
        
        private List<MuseumItem> museumItems;
        
        public List<MuseumItem> MuseumItems
        {
            set
            { 
                museumItems = value;
                EventDispatcher.Root.Raise(GlobalEvent.SYNC_MUSEUM_INFO);
            }
            get => museumItems;
        }

        public PiggyBankConfig PiggyBankConfig;

        /// <summary>
        /// 幸运卡礼包配置，两个挡位
        /// </summary>
        public Dictionary<int, List<LuckyCardConfig>> LuckyCardConfigs;

        public bool CanLuckyCardClick = true;

        public DragonConfig DragonConfig;

        #endregion ---------------------END  配置  ------------------------------


        #region ---------------------------------数据------------------------------------------

        private LuckyGuyInfo luckyGuyInfo;

        public LuckyGuyInfo LuckyGuyInfo
        {
            get
            {
                return luckyGuyInfo;
            }
            set
            {
                if (value != null && luckyGuyInfo != null)
                {
                    value.FirstFailTime = luckyGuyInfo.FirstFailTime;
                }
                luckyGuyInfo = value;
                EventDispatcher.Root.Raise(GlobalEvent.SYNC_LUCKY_GUY_INFO);
            }
        }
        
        private MuseumInfo museumInfo;

        public MuseumInfo MuseumInfo
        {
            set
            {
                var isReachMax = museumInfo?.IsReachMax ?? false;
                var oldPoint = museumInfo?.museum_points;
                var newPoint = value?.museum_points;

                museumInfo = value;
                EventDispatcher.Root.Raise(GlobalEvent.SYNC_MUSEUM_INFO);

                if (!isReachMax && newPoint > oldPoint && !value.dont_sync_add)
                {
                    MediatorItem.Instance.AddExShowItem(
                        new Item("MuseumPoint", (float)newPoint - (float)oldPoint));
                    EventDispatcher.Root.Raise(GlobalEvent.MUSEUM_INFO_ADD);
                }

                if (museumInfo != null)
                {
                    museumInfo.dont_sync_add = false;
                }
               
                if (value?.IsReachMax ?? false)
                {
                    EventDispatcher.Root.Raise(GlobalEvent.MUSEUM_INFO_REACH_MAX);
                }
            }

            get => museumInfo;
        }
        
        
        private FreeBonusInfo freeBonusInfo;

        public FreeBonusInfo FreeBonusInfo
        {
            set
            {
                freeBonusInfo = value;
                EventDispatcher.Root.Raise(GlobalEvent.SYNC_FREE_BONUS_INFO);
            }

            get => freeBonusInfo ??= new FreeBonusInfo();
        }
        
        private StarterPackInfo starterPackInfo;

        public StarterPackInfo StarterPackInfo
        {
            get => starterPackInfo;
            set
            {
                starterPackInfo = value;
                EventDispatcher.Root.Raise(GlobalEvent.SYNC_STARTERPACKINFO);
            }
        }

        private RoomAdInfo roomAdInfo;

        public RoomAdInfo RoomAdInfo
        {
            get => roomAdInfo;
            set
            {
                roomAdInfo = value;
                EventDispatcher.Root.Raise(GlobalEvent.SYNC_ROOM_AD_INFO);
            }
        }

        private PiggyBankInfo piggyBankInfo;

        public PiggyBankInfo PiggyBankInfo
        {
            get { return piggyBankInfo; }
            set
            {
                piggyBankInfo = value;
                EventDispatcher.Root.Raise(GlobalEvent.SYNC_PIGGY_BANK);
            }
        }

        private ServerMaintain serverMaintainInfo;

        public ServerMaintain ServerMaintainInfo
        {
            get { return serverMaintainInfo; }
            set
            {
                serverMaintainInfo = value;
                EventDispatcher.Root.Raise(GlobalEvent.SYNC_SERVER_MAINTAIN);
            }
        }

        /// <summary>
        /// 对局校验开关
        /// </summary>
        public int tool_game_replay_close;

        /// <summary>
        /// 0 开启【默认关闭？】
        /// </summary>
        public bool IsReplayCheckOpen => tool_game_replay_close == 0;

        public SignInfo SignInfo
        {
            set
            {
                signInfo = value;
                EventDispatcher.Root.Raise(GlobalEvent.Sync_SignInfo);
            }
            get => signInfo;
        }

        /// <summary>
        /// 账号注册时间
        /// </summary>
        public int RegisterTime;

        private OnlineRewardInfo onlineRewardInfo;

        private SignInfo signInfo;

        /// <summary>
        /// 玩家每日最高分
        /// </summary>
        public int DailyMaxScore;

        /// <summary>
        /// 玩家历史最高分
        /// </summary>
        public int HistoryMaxScore;


        /// <summary>
        /// 获得最高分标识 0 -未获得最高分，1 - 获得每日最高分，2-获得历史最高分，3 - 同时获得每日和历史最高分
        /// </summary>
        public int HistoryMaxType;
        
        // 需要弹出通知申请权限
        public bool IsNeedRequestNotification = false;

        /// <summary>
        /// 是否有是每日最高分
        /// </summary>
        public bool IsDailyScoreHigher => (HistoryMaxType & 1) > 0;

        /// <summary>
        /// 是否是历史最高分
        /// </summary>
        public bool IsHistoryScoreHigher => (HistoryMaxType & 2) > 0;

        public OnlineRewardInfo OnlineRewardInfo
        {
            set
            {
                onlineRewardInfo = value;
                EventDispatcher.Root.Raise(GlobalEvent.Sync_OnlineRewardInfo);
            }
            get => onlineRewardInfo;
        }

        private TaskInfo curTaskInfo;

        /// <summary>
        /// 当前选择的任务， 在结束后可以选择其他难度的任务
        /// </summary>
        public TaskInfo CurTaskInfo
        {
            set
            {
                curTaskInfo = value;
                EventDispatcher.Root.Raise(GlobalEvent.Sync_TaskInfo);
            }
            get => curTaskInfo;
        }

        private RoomChargeInfo roomChargeInfo;

        /// <summary>
        /// 当前选择的任务， 在结束后可以选择其他难度的任务
        /// </summary>
        public RoomChargeInfo RoomChargeInfo
        {
            set
            {
                roomChargeInfo = value;
                EventDispatcher.Root.Raise(GlobalEvent.Sync_RoomChargeInfo);
            }
            get => roomChargeInfo;
        }

        /// <summary>
        /// 充值成功次数
        /// </summary>
        public int ChargeSuccessCount
        {
            get
            {
                if (ChargeInfo == null)
                {
                    return 0;
                }

                return ChargeInfo.success_count;
            }
        }

        public ChargeInfo ChargeInfo;


        /// <summary>
        /// 充值版本, 0 表示没有vip等级
        /// </summary>
        public int ChargeVersion;

        /// <summary>
        /// 充值系统是否开放 , 客户端锁定充值系统
        /// </summary>
        public bool ChargeOpen => ChargeVersion > 0;

        /// <summary>
        /// 是否已经选择了任务
        /// </summary>
        public bool IsTaskBegin => CurTaskInfo is { begin_time: > 0 };

        private int GuideStepValue => Role.match_first_game_guide;

        /// <summary>
        /// 玩家已登陆， 玩家已绑定邮箱
        /// </summary>
        public bool IsBindMail
        {
            get
            {
                if (UserInfo == null)
                {
                    return false;
                }

                return !UserInfo.email.IsNullOrEmpty();
            }
        }

        /// <summary>
        /// 邮箱是否验证， 暂时没有用
        /// </summary>
        public bool IsMailValid => IsBindMail && UserInfo.validata == 1;

        private UserInfo _userInfo;
        
        public UserInfo UserInfo
        {
            set => _userInfo = value;
            get
            {
                // userInfo ??= new UserInfo();
                return _userInfo ;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetYZEmialStatus()
        {
            if (string.IsNullOrEmpty(UserInfo.email))
            {
                return 0;
            }

            if (UserInfo.validata == 0)
            {
                return 1;
            }

            return 2;
        }

        public int GetHyperMailVerified()
        {
            return YZDataUtil.GetYZInt(YZConstUtil.YZWithdrawMailVerified, 0);
        }

        public int GetHyperNameAndAddressVerified()
        {
            return YZDataUtil.GetYZInt(YZConstUtil.YZWithdrawNameAndAddressVerified, 0);
        }

        /// <summary>
        /// 表现上是否是自然量用户, 可以看作是否显示美金
        /// 是否是自然量用户 1-自然量用户，0-非自然量用户 -1 无数据 视为自然量用户
        /// </summary>
        public bool IsNaturalFlow
        {
            get
            {
// #if UNITY_EDITOR
//                 return false;
// #endif

#if UNITY_IOS
                return false;
#endif
                if (UserInfo == null)
                {
                    return true;
                }

                if (UserInfo.is_organic == null)
                {
                    return true;
                }

                return UserInfo.is_organic != 0;
            }
        }

        /// <summary>
        /// 数据层面是否是自然量
        /// </summary>
        public bool IsNaturalFlow_InData
        {
            get
            {
                if (UserInfo == null)
                {
                    return true;
                }

                if (UserInfo.is_organic == null)
                {
                    return true;
                }

                return UserInfo.is_organic != 0;
            }
        }
        

        private List<Room> roomList;

        public List<Room> ShowRoomList => RoomList?.Where(room => room.room_show).ToList();

        public bool HaveAdRoom => ShowRoomList != null && ShowRoomList.Exists(room => room.IsADRoom);
        
        public List<Room> OriginRoomList => roomList;
        
        public List<Room> RoomList
        {
            get
            {
                if (IsNaturalFlow)
                {
                    return roomList.Where(room => !room.IsAboutMoney).ToList();
                }

                return roomList;
            }

            set
            {
                roomList = value;
                EventDispatcher.Root.Raise(GlobalEvent.Refresh_Room_List);
            }
        }

        public Dictionary<string, float> NewPlayerBonus;

        #endregion --------------------- 数据 -------------------------------------

        //private int signWheel
        
        private MonthCardInfo monthCardInfo;

        /// <summary>
        /// 周卡 月卡
        /// </summary>
        public MonthCardInfo MonthCardInfo
        {
            set
            {
                monthCardInfo = value;
                EventDispatcher.Root.Raise(GlobalEvent.SYNC_MONTH_CARD_INFO);
            }
            get => monthCardInfo;
        }

        public InfiniteWeekConfig WeekConfig;
        public InfiniteWeekInfo WeekInfo;

        /// <summary>
        /// 充值挡位
        /// </summary>
        public Dictionary<int, MonthCardBonusInfo> MonthCardBonusInfos;

        public List<ChargeGoodInfo> WeekCardChargeInfos;
        
        public List<ChargeGoodInfo> MonthCardChargeInfos;

        public bool IsChargeToday
        {
            get
            {
                if (ChargeInfo == null)
                {
                    return false;
                }
                return ChargeInfo.today_charge_index == 1;
            }

            set
            {
                if (ChargeInfo != null)
                {
                    ChargeInfo.today_charge_index = value ? 1 : 0;
                }
            }
        }

        public List<ChargeMethods> ChargeMethodsList;

        public Role Role
        {
            get => role ??= new Role();
            set => role = value;
        }


        public Room GetRoomById(int roomId)
        {
            return RoomList.Find(room => room.id == roomId);
        }
        
        /// <summary>
        /// 提现记录
        /// </summary>
        public List<WithdrawHistoryData> WithdrawHistoryData;

        public bool WithdrawInProgress
        {
            get
            {
                if (WithdrawHistoryData == null)
                {
                    return false;
                }

                if (UserInfo.InCancelCD)
                {
                    return false;
                }

                if (WithdrawHistoryData.Exists(data => data.status == WithDrawState.submitted_to_a_third_party))
                {
                    return false;
                }
                
                return WithdrawHistoryData.Exists(data => data.InProgress());
            }
        }

        private List<MatchHistory> matchHistory;

        //自身的历史记录
        public List<MatchHistory> MatchHistory
        {
            set
            {
                if (!MediatorGuide.Instance.IsTriggerGuidePass(TriggerGuideStep.Lucky_Guy_Played_Effect) && value is {Count: > 0})
                {
                    var uimain = UserInterfaceSystem.That.Get<UIMain>();
                    
                    if (!(uimain == null || !uimain.IsInitEnd))
                    {
                        var history = value.Find(history => history.IsLuckyRoom && history.CanClaim);
                        if (history != null)
                        {
                            EventDispatcher.Root.Raise(GlobalEvent.LUCKY_GUY_FAKE_NEWS, history.match_id);
                        }
                    }
                }
                matchHistory = value;
            }

            get
            {
                matchHistory ??= new List<MatchHistory>();
                if (IsNaturalFlow)
                {
                    return matchHistory.Where(history =>
                    {
                        return !history.IsAboutDollar;
                    }).ToList();
                }
                else
                {
                    return matchHistory;
                }
            }
        }

        /// <summary>
        /// 生涯匹配次数, 登陆时数据
        /// </summary>
        public int MatchCountAtLoginTime => UserInfo?.match_count ?? 0;

        /// <summary>
        /// 如果没有重启， 对局结束后+1； 如果重启了，  在局内也+1
        /// </summary>
        public int MatchHistoryCount => Math.Max(MatchHistory.Count, MatchCountAtLoginTime - 1) ; 
        
        public int FinishHistoryCount
        {
            get
            {
                var count = MatchHistory.Count(history => history.status >= (int)Status.Game_End);
                return Math.Max(count, MatchCountAtLoginTime - 1);
            }
        }

        public int RecordRewardCount
        {
            get
            {
                int result = 0;
                foreach (var history in MatchHistory)
                {
                    if (history.CanClaim)
                    {
                        result++;
                    }
                }

                return result;
            }
        }

        private Dictionary<string, List<Match>> matchMap;

        //key = table id, 这数据好像没啥用
        public Dictionary<string, List<Match>> MatchMap
        {
            get { return matchMap ??= new Dictionary<string, List<Match>>(); }
        }

        private Dictionary<string, List<MatchHistory>> matchHistoryMap;

        //key = match id
        public Dictionary<string, List<MatchHistory>> MatchHistoryMap
        {
            get { return matchHistoryMap ??= new Dictionary<string, List<MatchHistory>>(); }
        }

        public string LastInCompleteCursor { get; set; }
        public string LastCompleteCursor { get; set; }
        public bool InCompleteRecordsOver { get; set; }
        public bool CompleteRecordsOver { get; set; }

        public List<MatchHistory> GetMatchHistoryList(string matchId, bool sortby_game_score = false)
        {
            MatchHistoryMap.TryGetValue(matchId, out var histories);

            if (histories != null && sortby_game_score)
            {
                //todo 引入order
                histories.Sort((a, b) =>
                {
                    if (a.win_result < b.win_result)
                    {
                        return -1;
                    }
                    else if (a.win_result > b.win_result)
                    {
                        return 1;
                    }
                    else
                    {
                        return 0;
                    }
                });
            }

            return histories;
        }

        public double GetLessVipUpNeed()
        {
            if (role.VipLevel >= VipConfig.Count)
            {
                return 0;
            }

            var level = role.VipLevel;
            var current = role.TotalCharge;
            var All = VipConfig[level + 1];

            return All - current;
        }

        /// <summary>
        /// 没有通过某步新手引导
        /// </summary>
        /// <param name="newPlayerGuideStep"></param>
        /// <returns></returns>
        public bool NotPassTheNewPlayerGuide(NewPlayerGuideStep newPlayerGuideStep)
        {
            if (NewPlayerGuideFinish())
            {
                return false;
            }

            return GuideStepValue < (int)newPlayerGuideStep;
        }

        private bool NewPlayerGuideFinish()
        {
            // return GuideStepValue is 60 or 65;
            return false;
        }

        public Sprite LoadPlayerIconByIndex(int index)
        {
            return MediatorBingo.Instance.GetSpriteByUrl($"common/playericon_{index}");
        }

        /// <summary>
        /// true = 非自然量
        /// </summary>
        /// <returns></returns>
        public bool IsSetNoNatural()
        {
            return (bool)PersistSystem.That.GetValue<bool>(GlobalEnum.SET_NOT_NATURAL);
        }

        public void SetNaturalMark(bool value)
        {
#if !RELEASE
            PersistSystem.That.SaveValue(GlobalEnum.SET_NOT_NATURAL, value);
            YZSDKsController.Shared.AF_ORGANIC = !value;
#endif
        }

        /// <summary>
        /// 是否所有需要数据已经准备完成
        /// </summary>
        /// <returns></returns>
        public bool IsSetUp()
        {
            //检查房间列表
            if (roomList == null || !roomList.Any())
            {
                YZLog.LogColor("缺少 roomList");
                return false;
            }

            if (!IsNaturalFlow)
            {
                //检查每日奖励
                if (onlineRewardInfo == null)
                {
                    YZLog.LogColor("缺少 onlineRewardInfo");
                    return false;
                }

                //检查博物馆数据
                // if (museumInfo == null)
                // {
                //     return false;
                // }
                
                //转盘
                if (fortuneWheelInfo == null)
                {
                    YZLog.LogColor("缺少 fortuneWheelInfo");
                    return false;
                }

                if (MaybeInTaskTime && curTaskInfo == null)
                {
                    YZLog.LogColor("缺少 curTaskInfo");
                    return false;
                }

                if (signInfo == null)
                {
                    YZLog.LogColor("缺少 signInfo");
                    return false;
                }

                if (matchHistory == null)
                {
                    YZLog.LogColor("缺少 matchHistory");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 5 + 3
        /// </summary>
        public bool MaybeInTaskTime => !TimeUtils.Instance.IsDayPassRegisterTime(8);

        public bool WithdrawValid = false;
        public float WithdrawCashFlow = 0;
        public float WithdrawToday = 0;
        public string WithdrawAmount = "";

        public string SessionId = "";
        public string PaymentType = "";
        public int GameRounds = 0;

        public string LastCashFlowCursor { get; set; }
        public bool CashFlowRecordsOver { get; set; }

        // 玩家流水记录
        private List<CashFlow> cashFlow;

        public List<CashFlow> CashFlow
        {
            set { cashFlow = value; }

            get => cashFlow ?? new List<CashFlow>();
        }

        public int OpenChargeTimeStamp;
        public int OpenChargeChannelTimeStamp;
        public int SubmitChargeOrderTimeStamp;
        public int OrderStartTimeStamp;
        
        // 每日任务
        public DailyTaskInfo dailyTaskInfo;
        public bool DailyTaskInfoLock;
        public bool DailyTaskClaimLock;
        public bool DailyTaskGetRewardLock;
        
        // 美金诱导弹窗
        public bool IsInduceReady = false;

        /// <summary>
        /// 邀请对战相关
        /// </summary>
        public DuelInfo DuelInfo;
        public DuelData DuelData;
        public DuelCloseData DuelCloseData;
        public DuelStatusInfo DuelStatusInfo;
        public string DuelOfflineMatchId = "";

        // 充值打点新增数据
        public JsonData CardUserName;
        
        /// <summary>
        /// ip合法性
        /// </summary>
        public int ProvinceValid = -1;

        public bool IsQuerying = false;

        /// <summary>
        /// 非自然量用户不能转为自然量用户
        /// </summary>
        public bool illegalityPeople => IsNaturalFlow_InData && ProvinceValid == 3 
                                                             // #if !RELEASE
                                                             && Role.white == 0
// #endif
        ;

        public bool IPIllegal => ProvinceValid == 1;

    }
}