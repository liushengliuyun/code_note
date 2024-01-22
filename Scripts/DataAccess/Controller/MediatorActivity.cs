using System;
using System.Linq;
using CatLib.EventDispatcher;
using Core.Controllers;
using Core.Extensions;
using Core.Services.NetService.API.Facade;
using Core.Services.PersistService.API.Facade;
using Core.Services.UserInterfaceService.API.Facade;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using UI;
using UI.Activity;
using UnityTimer;
using Utils;

namespace DataAccess.Controller
{
    public enum ActivityType
    {
        /// <summary>
        /// 首充礼包
        /// </summary>
        StartPacker,

        /// <summary>
        /// 新手任务
        /// </summary>
        TaskSystem,

        /// <summary>
        /// 存钱罐
        /// </summary>
        PiggyBank,

        /// <summary>
        /// 幸运卡
        /// </summary>
        LuckyCard,

        /// <summary>
        /// 一条龙
        /// </summary>
        Dragon,

        
        SpecialGift,

        /// <summary>
        /// room charge a
        /// </summary>
        JustForYou,

        /// <summary>
        /// room charge b
        /// </summary>
        BestOffer,

        /// <summary>
        /// 转盘
        /// </summary>
        FortuneWheel,

        /// <summary>
        /// 周卡和月卡活动
        /// </summary>
        MonthCard,

        /// <summary>
        /// 每日任务
        /// </summary>
        DailyMission,

        MagicBall,

        LuckyGuy,
        
        SpecialOffer,

        /// <summary>
        /// 在线奖励
        /// </summary>
        OnlineReward,

        // 补充充值
        AddCharge,
        
        /// <summary>
        /// 占位
        /// </summary>
        None,
        
        
        /// <summary>
        /// 房间直充
        /// </summary>
        RoomCharge,
        
        
        /// <summary>
        /// 商店
        /// </summary>
        StoreCharge,
        
        
        FreeBonusRoom,
        
        /// <summary>
        /// 好友对战
        /// </summary>
        FriendDuel,
        
        
        /// <summary>
        /// 签到
        /// </summary>
        DailySign
    }

    public class MediatorActivity : global::Utils.Runtime.Singleton<MediatorActivity>
    {
        /// <summary>
        /// 登陆时, 已经对局结束后 , 活动是否开启
        /// </summary>
        /// <param name="activityType"></param>
        /// <param name="isAfterGame"></param>
        /// <returns></returns>
        public bool IsActivityOpen(ActivityType activityType, bool isAfterGame = false)
        {
            if (Root.Instance.IsNaturalFlow)
            {
                return false;
            }

            switch (activityType)
            {
                case ActivityType.FortuneWheel:
                case ActivityType.MonthCard:
                case ActivityType.DailyMission:
                    //daily  mission当日已领取/已积累点数用户不刷新，第二天整点刷新隐藏
                    if (Root.Instance.dailyTaskInfo == null)
                    {
                        return false;
                    }

                    return Root.Instance.dailyTaskInfo.complete > -1 || Root.Instance.dailyTaskInfo.progress > 0;
                case ActivityType.MagicBall:
                    if (Root.Instance.MagicBallInfo == null)
                    {
                        return false;
                    }

                    return MediatorUnlock.Instance.IsActivityBtnUnlock(ActivityType.MagicBall) &&
                           !Root.Instance.MagicBallInfo.IsGetAllReward;
                case ActivityType.TaskSystem:
                    return MediatorTask.Instance.IsTaskActivityOpen();
                case ActivityType.JustForYou:
                    return Root.Instance.RoomChargeInfo is { CanOpenA: true };
                case ActivityType.BestOffer:
                    return Root.Instance.RoomChargeInfo is { CanOpenB: true };
                case ActivityType.StartPacker:
                    if (Root.Instance.StarterPackInfo is not { CanBuy: true })
                    {
                        return false;
                    }

                    if (!isAfterGame)
                    {
                        return true;
                    }
                    else
                    {
                        //对局结束后 , 仅在7天内会弹
                        if (!TimeUtils.Instance.IsDayPassRegisterTime(7))
                        {
                            return true;
                        }
                    }

                    break;

                case ActivityType.PiggyBank:
                    return Root.Instance.PiggyBankInfo.piggy_chance > 0;
                    break;

                case ActivityType.LuckyCard:
                {
                    if (!isAfterGame)
                    {
                        return Root.Instance.Role.luckyCardInfo.lucky_card_chance == 1;
                    }
                }
                    break;

                case ActivityType.Dragon:
                {
                    return Root.Instance.Role.dragonInfo.one_stop_today_chance == 1;
                }

                case ActivityType.SpecialGift:
                {
                    return Root.Instance.Role.specialGiftInfo.special_gift_chance > 0 &&
                           Root.Instance.Role.specialGiftInfo.special_gift_today_chance > 0;
                }
                
                case ActivityType.LuckyGuy:
                    return Root.Instance.LuckyGuyInfo is { IsOpen: true };
                
                case ActivityType.SpecialOffer:
                    return Root.Instance.Role.SpecialOfferInfo != null &&
                           Root.Instance.Role.SpecialOfferInfo.show_time > 0 &&
                           Root.Instance.Role.SpecialOfferInfo.show_time + 3600 - TimeUtils.Instance.UtcTimeNow > 0 &&
                           Root.Instance.ChargeInfo.success_total <= 0;

                case ActivityType.AddCharge:
                    return Root.Instance.Role.AdditionalGiftInfo != null;
            }

            return false;
        }


        private ActivityType[] hotSellings = new[] { ActivityType.StartPacker, ActivityType.SpecialGift };

        /// <summary>
        /// 是否有活动
        /// </summary>
        /// <returns></returns>
        public bool HaveHotSellings()
        {
            foreach (var activityType in hotSellings)
            {
                var isBeginning = IsActivityBegin(activityType);
                if (isBeginning)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 活动是否已经开始
        /// </summary>
        /// <param name="activityType"></param>
        /// <param name="checkOpen"></param>
        /// <returns></returns>
        public bool IsActivityBegin(ActivityType activityType, bool checkOpen = true)
        {
            if (Root.Instance.IsNaturalFlow)
            {
                return false;
            }

            if (checkOpen)
            {
                if (!IsActivityOpen(activityType))
                {
                    return false;
                }
            }

            switch (activityType)
            {
                case ActivityType.StartPacker:
                case ActivityType.LuckyCard:
                case ActivityType.PiggyBank:
                case ActivityType.Dragon:
                case ActivityType.SpecialGift:
                case ActivityType.JustForYou:
                case ActivityType.BestOffer:
                case ActivityType.MagicBall:
                    return GetActivityLessTime(activityType) > 0;
                case ActivityType.FortuneWheel:
                case ActivityType.MonthCard:
                    return true;
                case ActivityType.LuckyGuy:
                    return Root.Instance.LuckyGuyInfo is { UntilOpenTime : <= 0 }; 
                case ActivityType.SpecialOffer:
                    return Root.Instance.Role.SpecialOfferInfo != null &&
                           Root.Instance.Role.SpecialOfferInfo.show_time > 0 &&
                           Root.Instance.Role.SpecialOfferInfo.show_time + 3600 - TimeUtils.Instance.UtcTimeNow > 0 &&
                           Root.Instance.ChargeInfo.success_total <= 0;
                case ActivityType.AddCharge:
                    return Root.Instance.Role.AdditionalGiftInfo != null;
                    break;
            }

            return false;
        }

        /// <summary>
        /// 登陆时弹窗顺序
        /// </summary>
        private ActivityType[] loginPopOrder =
        {
            ActivityType.StartPacker,
            ActivityType.SpecialGift,
            ActivityType.PiggyBank,
            ActivityType.LuckyCard,
            ActivityType.Dragon,
        };

        /// <summary>
        /// 游戏结束时弹窗顺序
        /// </summary>
        private ActivityType[] afterGameOrder =
        {
            ActivityType.StartPacker,
            ActivityType.Dragon,
            ActivityType.PiggyBank,
            // ActivityType.DailyMission,
        };


        /// <summary>
        /// 这些不受前10局对局影响
        /// </summary>
        private ActivityType[] afterGameOrderNew =
        {
            ActivityType.LuckyGuy,
        };

        private ActivityType[] RecordIntervalActivites =
        {
            ActivityType.StartPacker,
            ActivityType.SpecialGift,
            ActivityType.PiggyBank,
            ActivityType.LuckyCard,
            ActivityType.Dragon,
            ActivityType.LuckyGuy
        };

        /// <summary>
        /// 所有充值相关活动
        /// </summary>
        private ActivityType[] chargeActivity =
        {
            ActivityType.StartPacker,
            ActivityType.FortuneWheel,
            ActivityType.PiggyBank,
            ActivityType.LuckyCard,
            ActivityType.Dragon,
            ActivityType.SpecialGift,
            ActivityType.JustForYou,
            ActivityType.BestOffer
        };

        public void PopAllActivity(bool isAfterGame = false)
        {
            if (Root.Instance.IsNaturalFlow)
            {
                return;
            }

            if (isAfterGame && Root.Instance.MatchHistoryCount <= 10)
            {
                PopAfterGame();
            }
            else
            {
                foreach (var activityType in isAfterGame ? afterGameOrder : loginPopOrder)
                {
                    PopActivity(activityType, isAfterGame);
                }
            }

            if (isAfterGame)
            {
                foreach (var activityType in afterGameOrderNew)
                {
                    PopActivity(activityType, true);
                }
            }
        }

        /// <summary>
        /// 对局结束后, 生涯前十局 , 各有规则
        /// </summary>
        private void PopAfterGame()
        {
            switch (Root.Instance.MatchHistoryCount)
            {
                //生涯第一局
                case 1:
                    if (Root.Instance.SignInfo.sign_chance == 1)
                        UserInterfaceSystem.That.SingleTonQueue<UISign>();

                    MediatorUnlock.Instance.RecordShowUI(typeof(UIPiggyBank));
                    UserInterfaceSystem.That.SingleTonQueue<UIPiggyBank>(ActivityEnterType.FirstTenGame);

                    //自动接取任务
                    void Callback()
                    {
                        MediatorTask.Instance.PopTaskSystem(2);
                    }

                    NetSystem.That.SetFailCallBack(s => { Callback(); });
                    MediatorTask.Instance.Choose(1, Callback);
                    
                    // MediatorUnlock.Instance.RecordShowUI(typeof(UIHighScore));
                    // UserInterfaceSystem.That.SingleTonQueue<UIHighScore>();
                    break;
                case 2:
                    YZSDKsController.Shared.PromptForPush();
                    
                    Timer.Register(0.5f, () =>
                    {
                        MediatorUnlock.Instance.RecordShowUI(typeof(UIStartPack));
                        UserInterfaceSystem.That.SingleTonQueue<UIStartPack>(ActivityEnterType.FirstTenGame);
                    });
                   
                    break;
                case 3:
                    MediatorUnlock.Instance.RecordShowUI(typeof(UIWheel));
                    MediatorUnlock.Instance.RecordShowUI(typeof(UIMagicBall));
                    UserInterfaceSystem.That.SingleTonQueue<UIMagicBall>(
                        new GameData()
                        {
                            ["ActivityEnterType"] = ActivityEnterType.FirstTenGame
                        });
                    UserInterfaceSystem.That.SingleTonQueue<UIWheel>((int)WheelType.PaySmall);
                    EventDispatcher.Root.Raise(GlobalEvent.Refresh_Room_List);
                    break;
                case 4:
                    MediatorUnlock.Instance.RecordShowUI(typeof(UIDragon));
                    MediatorUnlock.Instance.RecordShowUI(typeof(UIPiggyBank));
                    UserInterfaceSystem.That.SingleTonQueue<UIDragon>(ActivityEnterType.FirstTenGame);
                    UserInterfaceSystem.That.SingleTonQueue<UIPiggyBank>(ActivityEnterType.FirstTenGame);
                    break;
                case 5:
                    MediatorUnlock.Instance.RecordShowUI(typeof(UIMonthCardNew));
                    MediatorUnlock.Instance.RecordShowUI(typeof(UIOnlineReward));
                    
                    //周卡月卡
                    if (Root.Instance.WeekInfo != null)
                        UserInterfaceSystem.That.SingleTonQueue<UIMonthCardNew>(new GameData()
                            {
                                ["ActivityEnterType"] = ActivityEnterType.FirstTenGame
                            }
                        );
                    
                    UserInterfaceSystem.That.SingleTonQueue<UIOnlineReward>(new GameData()
                    {
                        ["ActivityEnterType"] = ActivityEnterType.FirstTenGame,
                        ["IsFirstShow"] = true
                    });
                    break;
                case 6:
                    UserInterfaceSystem.That.SingleTonQueue<UIWheel>((int)WheelType.PayBig);
                    
                    if (Root.Instance.Role.SpecialOfferInfo != null && Root.Instance.Role.SpecialOfferInfo.show_time > 0 && 
                        Root.Instance.Role.SpecialOfferInfo.show_time + 3600 - TimeUtils.Instance.UtcTimeNow > 0 &&
                        Root.Instance.ChargeInfo.success_total <= 0)
                        UserInterfaceSystem.That.SingleTonQueue<UISpecialOffer>(new GameData()
                        {
                            ["ActivityEnterType"] = ActivityEnterType.FirstTenGame
                        });
                    break;
                case 7:
                    UserInterfaceSystem.That.SingleTonQueue<UIFriendsDuel>(new GameData()
                    {
                        ["isTriggerPop"] = true
                    }); 
                    break;
                case 8:
                    UserInterfaceSystem.That.SingleTonQueue<UIWheel>((int)WheelType.PaySmall);
                    break;
                case 9:
                    UserInterfaceSystem.That.SingleTonQueue<UIStartPack>(ActivityEnterType.FirstTenGame);
                    break;
                case 10:
                    UserInterfaceSystem.That.SingleTonQueue<UIWheel>((int)WheelType.PayBig);
                    break;
            }
        }

        /// <summary>
        /// 随机弹出一个 开启中的活动
        /// </summary>
        public bool PopActivityRandom()
        {
            //随机打乱数组
            bool result = false;
            chargeActivity.Shuffle();
            foreach (var activityType in chargeActivity)
            {
                if (PopActivity(activityType, ActivityEnterType.Random))
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        public void PopActivityRandomAtStore()
        {
            if (Root.Instance.IsChargeToday)
            {
                return;
            }

            if (TimeUtils.Instance.PassDay < 3)
            {
                return;
            }

            if (!GameUtils.IsProbabilityAbove(80f))
            {
                return;
            }

            int time = (int)PersistSystem.That.GetValue<int>(GlobalEnum.PopActivityStore, true);

            //客户端记录间隔10分钟弹一次， 前3天不弹  80%概率
            if (TimeUtils.Instance.UtcTimeNow - time > 600)
            {
                PersistSystem.That.SaveValue(GlobalEnum.PopActivityStore, TimeUtils.Instance.UtcTimeNow, true);
                PopActivityRandom();
            }
        }


        /// <summary>
        /// 弹脸?
        /// </summary>
        /// <param name="activityType"></param>
        /// <param name="isAfterGame"></param>
        /// <returns></returns>
        public bool PopActivity(ActivityType activityType, bool isAfterGame = false)
        {
            if (Root.Instance.IsNaturalFlow)
            {
                return false;
            }

// #if DAI_TEST
//             return false;
// #endif

            if (!IsActivityOpen(activityType, isAfterGame))
            {
                return false;
            }

            // GenerateActivity(activityType);

            if (!IsLessTimeEnough(activityType))
            {
                return false;
            }

            if (!IsPopCountEnough(activityType, isAfterGame))
            {
                return false;
            }

            if (!CheckSpecialRule(activityType, isAfterGame))
            {
                return false;
            }

            var entryType = isAfterGame ? ActivityEnterType.AfterGame : ActivityEnterType.Login;

            switch (activityType)
            {
                case ActivityType.StartPacker:
                    UserInterfaceSystem.That.SingleTonQueue<UIStartPack>(entryType);
                    break;

                case ActivityType.LuckyCard:
                    UserInterfaceSystem.That.SingleTonQueue<UILuckyCard>(entryType);
                    break;

                case ActivityType.PiggyBank:
                    UserInterfaceSystem.That.SingleTonQueue<UIPiggyBank>(entryType);
                    break;

                case ActivityType.Dragon:
                    UserInterfaceSystem.That.SingleTonQueue<UIDragon>(entryType);
                    break;
                case ActivityType.SpecialGift:
                    int week = Root.Instance.Role.specialGiftInfo.today_week.ToInt();
                    if (week == 1 || week == 2)
                    {
                        UserInterfaceSystem.That.SingleTonQueue<UISpecialGiftA>(entryType);
                    }
                    else if (week == 3 || week == 4 || week == 5)
                    {
                        UserInterfaceSystem.That.SingleTonQueue<UISpecialGiftB>(entryType);
                    }
                    else if (week == 6 || week == 0)
                    {
                        UserInterfaceSystem.That.SingleTonQueue<UISpecialGiftC>(entryType);
                    }
                    else
                    {
                        return false;
                    }

                    break;


                case ActivityType.MonthCard:
                    if (Root.Instance.MonthCardInfo.NotBuy)
                        UserInterfaceSystem.That.SingleTonQueue<UIMonthCardNew>(new GameData()
                        {
                            ["ActivityEnterType"] = entryType
                        });
                    else
                        UserInterfaceSystem.That.SingleTonQueue<UIMonthCard>(entryType);
                    break;

                case ActivityType.DailyMission:
                    if (YZDataUtil.GetYZInt(YZConstUtil.YZDailyMissionHaveReward, 0) == 1)
                        MediatorRequest.Instance.ClaimDailyTask(true);
                    break;

                case ActivityType.LuckyGuy:
                    UserInterfaceSystem.That.SingleTonQueue<UILuckyGuy>(new GameData()
                    {
                        ["enterType"] = entryType,
#if DAI_TEST
                        ["queueOrder"] = 10
#endif
                    });
                    break;
            }

            return true;
        }


        private bool PopActivity(ActivityType activityType, ActivityEnterType entryType)
        {
// #if DAI_TEST
//             return false;
// #endif
            if (Root.Instance.IsNaturalFlow)
            {
                return false;
            }

            if (!IsActivityOpen(activityType, false))
            {
                return false;
            }

            GenerateActivity(activityType);

            if (!IsLessTimeEnough(activityType))
            {
                return false;
            }

            switch (activityType)
            {
                case ActivityType.StartPacker:
                    UserInterfaceSystem.That.SingleTonQueue<UIStartPack>(entryType);
                    break;

                case ActivityType.LuckyCard:
                    UserInterfaceSystem.That.SingleTonQueue<UILuckyCard>(entryType);
                    break;

                case ActivityType.PiggyBank:
                    UserInterfaceSystem.That.SingleTonQueue<UIPiggyBank>(entryType);
                    break;

                case ActivityType.Dragon:
                    UserInterfaceSystem.That.SingleTonQueue<UIDragon>(entryType);
                    break;
                case ActivityType.SpecialGift:
                    int week = Root.Instance.Role.specialGiftInfo.today_week.ToInt();
                    if (week == 1 || week == 2)
                    {
                        UserInterfaceSystem.That.SingleTonQueue<UISpecialGiftA>(entryType);
                    }
                    else if (week == 3 || week == 4 || week == 5)
                    {
                        UserInterfaceSystem.That.SingleTonQueue<UISpecialGiftB>(entryType);
                    }
                    else if (week == 6 || week == 0)
                    {
                        UserInterfaceSystem.That.SingleTonQueue<UISpecialGiftC>(entryType);
                    }
                    else
                    {
                        return false;
                    }

                    break;

                case ActivityType.BestOffer:
                    UserInterfaceSystem.That.ShowUI<UIBestOffer>(new GameData()
                    {
                        ["ActivityEnterType"] = entryType
                    });
                    break;
                case ActivityType.JustForYou:
                    UserInterfaceSystem.That.ShowUI<UIJustForYou>(new GameData()
                    {
                        ["ActivityEnterType"] = entryType
                    });
                    break;
                case ActivityType.FortuneWheel:
                    UserInterfaceSystem.That.SingleTonQueue<UIWheel>((int)GetTargetWheelType());
                    break;
            }

            return true;
        }


        /// <summary>
        /// 获得目标跳转的转盘
        /// </summary>
        /// <returns></returns>
        public WheelType GetTargetWheelType()
        {
            if (Root.Instance.FortuneWheelInfo == null)
            {
                return WheelType.PaySmall;
            }

            if (Root.Instance.FortuneWheelInfo.IsLocateBig)
            {
                return WheelType.PayBig;
            }

            return WheelType.PaySmall;
        }

        /// <summary>
        /// 生成活动
        /// </summary>
        /// <param name="activityType"></param>
        private void GenerateActivity(ActivityType activityType)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="activityType"></param>
        /// <param name="isAfterGame"></param>
        /// <returns></returns>
        bool IsPopCountEnough(ActivityType activityType, bool isAfterGame = false)
        {
            bool result = true;
            //检查弹出次数, 已经弹出的次数
            var popCount = GetPopCount(activityType, isAfterGame);
            switch (activityType)
            {
                case ActivityType.StartPacker:
                    if (isAfterGame)
                    {
                        result = popCount < 7;
                    }
                    else
                    {
                        result = popCount < 5;
                    }

                    break;
                case ActivityType.Dragon:
                    result = popCount < 5;
                    break;
                case ActivityType.SpecialGift:
                case ActivityType.LuckyCard:
                    if (!isAfterGame)
                    {
                        result = popCount < 5;
                    }

                    break;
                case ActivityType.PiggyBank:
                    result = popCount < 5;
                    break;

                case ActivityType.LuckyGuy:
                 
#if !DAI_TEST || true

   var bothPopCount = GetBothPopCount(ActivityType.LuckyGuy);
result = bothPopCount < 3;
#endif
                    break;
            }

            return result;
        }

        /// <summary>
        /// 检查弹出间隔
        /// </summary>
        /// <param name="activityType"></param>
        /// <param name="isAfterGame"></param>
        /// <returns>true 可以弹出</returns>
        bool CheckSpecialRule(ActivityType activityType, bool isAfterGame = false)
        {
            bool result = true;
            var intervalGames = GetIntervalGameCount(activityType);

            switch (activityType)
            {
                case ActivityType.LuckyGuy:
                    if (isAfterGame)
                    {
                        result = intervalGames > 0 && intervalGames % 2 == 0;
                    }

                    break;
                case ActivityType.DailyMission:
                    if (isAfterGame)
                    {
                        result = YZDataUtil.GetYZInt(YZConstUtil.YZDailyMissionHaveReward, 0) == 1;
                    }

                    break;
                case ActivityType.MonthCard:
                    result = Root.Instance.MonthCardInfo.HaveRewardToClaim;
                    break;
                case ActivityType.StartPacker:
                    if (isAfterGame)
                    {
                        switch (TimeUtils.Instance.PassDay)
                        {
                            //生涯前24H: 每3局对战结束后弹出一次；生涯24H-72H: 每4局对局弹出一次;
                            case < 1 when intervalGames >= 3:
                            case >= 1 and < 3 when intervalGames >= 4:
                            //生涯72H-7D: 每5局弹出一次；生涯7D后，对局结束不再弹出
                            case >= 3 and < 7 when intervalGames >= 5:
                                result = true;
                                break;
                            default:
                                result = false;
                                break;
                        }
                    }

                    break;

                case ActivityType.PiggyBank:
                    if (isAfterGame)
                    {
                        //满之后的下一局弹一次, 之后每隔三局弹出一次
                        if (Root.Instance.PiggyBankInfo.IsFull)
                        {
                            //第5局弹， 8， 11
                            if ((intervalGames - 2) % 3 != 0)
                            {
                                result = false;
                            }
                        }
                        else
                        {
                            AlterGameInterval(activityType, 0);
                            result = false;
                        }
                    }
                    else
                    {
                        result = Root.Instance.PiggyBankInfo.IsFull;
                    }

                    break;

                case ActivityType.Dragon:
                    if (isAfterGame)
                    {
                        //  对局结束后每3局弹出一次（即，第3,6,9,12局结束后弹出）
                        if (intervalGames >= 3 && intervalGames % 3 == 0)
                        {
                            result = true;
                        }
                        else
                        {
                            result = false;
                        }
                    }

                    break;
            }

            return result;
        }

        private bool IsLessTimeEnough(ActivityType activityType)
        {
            bool result = true;
            switch (activityType)
            {
                case ActivityType.PiggyBank:
                case ActivityType.LuckyCard:
                case ActivityType.Dragon:
                case ActivityType.SpecialGift:
                case ActivityType.BestOffer:
                case ActivityType.JustForYou:
                case ActivityType.StartPacker:
                    result = GetActivityLessTime(activityType) > 0;
                    break;
            }

            return result;
        }

        /// <summary>
        /// 使用门票的时候, 就算进行了一局
        /// </summary>
        public void AddAllGameInterval()
        {
            foreach (var activityType in RecordIntervalActivites)
            {
                if (!IsActivityBegin(activityType))
                {
                    continue;
                }
                ChangeGameInterval(activityType, false);
            }
        }

        /// <summary>
        /// 已间隔了多少次没弹
        /// </summary>
        /// <param name="activityType"></param>
        /// <param name="isPop"></param>
        private void ChangeGameInterval(ActivityType activityType, bool isPop)
        {
            var key = GetIntervalGameKey(activityType);
            if (isPop)
            {
                PersistSystem.That.SaveValue(key, 0, true);
            }
            else
            {
                var intervalGameCount = GetIntervalGameCount(activityType);
                PersistSystem.That.SaveValue(key, ++intervalGameCount, true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="activityType"></param>
        /// <param name="value"></param>
        private void AlterGameInterval(ActivityType activityType, int value)
        {
            var key = GetIntervalGameKey(activityType);
            PersistSystem.That.SaveValue(key, value, true);
        }

        /// <summary>
        /// 计数
        /// </summary>
        /// <param name="activityType"></param>
        /// <param name="activityEnterType"></param>
        /// <returns></returns>
        public void AddPopCount(ActivityType activityType, ActivityEnterType activityEnterType)
        {
            var isAfterGame = activityEnterType is ActivityEnterType.AfterGame or ActivityEnterType.FirstTenGame;
            var popCount = GetPopCount(activityType, isAfterGame);
            var record = TimeUtils.Instance.Today + "," + ++popCount;
            PersistSystem.That.SaveValue(GetKey(activityType, isAfterGame), record, true);

            //前10局弹 ，不清空间隔  
            if (activityEnterType is ActivityEnterType.AfterGame)
            {
                switch (activityType)
                {
                    case ActivityType.PiggyBank:
                        break;
                    default:
                        ChangeGameInterval(activityType, true);
                        break;
                }
            }
        }

        /// <summary>
        /// 获取中间间隔的对局数
        /// </summary>
        /// <param name="activityType"></param>
        /// <returns></returns>
        private int GetIntervalGameCount(ActivityType activityType)
        {
            var key = GetIntervalGameKey(activityType);
            var cacheValue = (int)PersistSystem.That.GetValue<int>(key, true);

            return cacheValue;
        }

        private string GetIntervalGameKey(ActivityType activityType)
        {
            return activityType + "interval";
        }

        int GetBothPopCount(ActivityType activityType)
        {
            return GetPopCount(activityType, true) + GetPopCount(activityType, false);
        }

        int GetPopCount(ActivityType activityType, bool isAfterGame = false)
        {
            var key = GetKey(activityType, isAfterGame);
            var cacheValue = (string)PersistSystem.That.GetValue<string>(key, true);
            if (cacheValue.IsNullOrEmpty())
            {
                return 0;
            }

            var strings = cacheValue.Split(',');
            if (strings == null || strings.Length < 2)
            {
                return 0;
            }

            var day = strings[0].ToInt32();
            if (day != TimeUtils.Instance.Today)
            {
                return 0;
            }

            return strings[1].ToInt32();
        }

        private static string GetKey(ActivityType activityType, bool isAfterGame)
        {
            var key = activityType.ToString() + (isAfterGame ? 1 : 0);
            return key;
        }

        private void SaveActivityBeginTime(ActivityType activityType)
        {
            switch (activityType)
            {
                case ActivityType.StartPacker:
                    // PersistSystem.That.SaveValue(GlobalEnum.START_PACKER_BEGIN_TIME, TimeUtils.Instance.UtcTimeNow,
                    //     true);
                    break;
            }
        }

        /// <summary>
        /// 检查是否需要更新充值挡位
        /// </summary>
        public void CheckStartPacker()
        {
            if (!IsActivityOpen(ActivityType.StartPacker))
            {
                return;
            }

            if (Root.Instance.ChargeInfo.success_count > 0)
            {
                return;
            }

            //计算玩家应该处在哪个挡位
            var gear = GetGearOfStartPacker();

            if (gear != Root.Instance.StarterPackInfo.starter_pack_level)
            {
                //清空原来的信息
                Root.Instance.StarterPackInfo = null;
                //上报 新挡位
                MediatorRequest.Instance.PushNewStartPackerGear(gear);
            }
        }

        private int GetGearOfStartPacker()
        {
            var passTime = TimeUtils.Instance.UtcTimeNow - Root.Instance.StarterPackInfo.starter_pack_begin_time;

            if (passTime is < 24 * 3600)
            {
                return 1;
            }

            //24小时未进行任何付费
            if (passTime is >= 24 * 3600 and < 3 * 24 * 3600)
            {
                return 2;
            }

            if (passTime is >= 3 * 24 * 3600 and < 7 * 24 * 3600)
            {
                return 3;
            }

            return 4;
        }


        /// <summary>
        /// 获取活动剩余时间
        /// </summary>
        /// <param name="activityType"></param>
        /// <returns></returns>
        public int GetActivityLessTime(ActivityType activityType)
        {
            if (!IsActivityOpen(activityType))
            {
                return 0;
            }

            switch (activityType)
            {
                //-1 = 无限
                case ActivityType.FreeBonusRoom:
                    return -1;
                case ActivityType.JustForYou:
                    return Root.Instance.RoomChargeInfo.ALessTime;
                case ActivityType.BestOffer:
                    return Root.Instance.RoomChargeInfo.BLessTime;
                case ActivityType.StartPacker:
                    var beginTime = GetActivityOpenTime(activityType);
                    if (beginTime > 0)
                    {
                        //开始4小时结束
                        return beginTime + 3600 * 4 - TimeUtils.Instance.UtcTimeNow;
                    }

                    break;
                case ActivityType.TaskSystem:
                    //todo
                    break;
                case ActivityType.PiggyBank:
                    return Root.Instance.PiggyBankInfo.start_time + 86400 - TimeUtils.Instance.UtcTimeNow;
                    break;

                case ActivityType.LuckyCard:
                    var lessTime = Root.Instance.Role.luckyCardInfo.end_timestamp - TimeUtils.Instance.UtcTimeNow;
                    if (lessTime > 0)
                    {
                        return lessTime;
                    }

                    break;

                case ActivityType.Dragon:
                    var lessTimeDragon = Root.Instance.Role.dragonInfo.end_timestamp - TimeUtils.Instance.UtcTimeNow;
                    if (lessTimeDragon > 0)
                    {
                        return lessTimeDragon;
                    }

                    break;

                case ActivityType.SpecialGift:
                    var lessTimeSp = Root.Instance.Role.specialGiftInfo.LessTime;
                    if (lessTimeSp > 0)
                        return lessTimeSp;
                    break;
                case ActivityType.MagicBall:
                    return Root.Instance.MagicBallInfo.LessTime;
            }

            return 0;
        }

        /// <summary>
        /// 获取活动开始时间
        /// </summary>
        /// <param name="activityType"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public int GetActivityOpenTime(ActivityType activityType)
        {
            switch (activityType)
            {
                case ActivityType.StartPacker:
                    // return (int)PersistSystem.That.GetValue<int>(GlobalEnum.START_PACKER_BEGIN_TIME, true);
                    return Root.Instance.StarterPackInfo.starter_park_create_time;
                    break;
                case ActivityType.TaskSystem:
                    //todo 
                    break;

                case ActivityType.LuckyCard:
                    return Root.Instance.Role.luckyCardInfo.lucky_card_begin_time;
                    break;

                case ActivityType.Dragon:
                    return Root.Instance.Role.dragonInfo.one_stop_begin_time;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(activityType), activityType, null);
            }

            return 0;
        }

        /// <summary>
        /// 是否自动弹出
        /// </summary>
        /// <param name="activityType"></param>
        /// <returns></returns>
        public bool IsSuggest(ActivityType activityType)
        {
            return loginPopOrder.Contains(activityType)
                   || afterGameOrder.Contains(activityType)
                   || afterGameOrderNew.Contains(activityType);
        }

        /// <summary>
        /// 活动剩余次数
        /// </summary>
        /// <param name="activityType"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public int GetRemainTimes(ActivityType activityType)
        {
            int result = -1;

            switch (activityType)
            {
                //只能购买一次
                case ActivityType.StartPacker:
                case ActivityType.PiggyBank:
                case ActivityType.LuckyCard:
                case ActivityType.BestOffer:
                    return 0;
            }
            
            return result;
        }
    }
}