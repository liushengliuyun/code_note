using System;
using CatLib;
using CatLib.EventDispatcher;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils.Static;
using NCat;
using UnityEngine;
using UnityTimer;
using Utils;

namespace Core.Runtime.Game
{
    public class RedPointMonoDriver : MonoBehaviour
    {
        private void Start()
        {
            /*EventDispatcher.Root.AddListener(GlobalEvent.DAILY_REWARD_CHANCE,
                (sender, args) =>
                {
                    Timer.Register(0.1f, () =>
                    {
                        RedPointNotify.SetMark(ERedPointItem.DailyGift, Root.Instance.DailyRewardChance);
                    });
                });*/

            EventDispatcher.Root.AddListener(GlobalEvent.REFRESH_RED_POINT,
                (sender, args) =>
                {
                    if (MediatorUnlock.Instance.IsActivityBtnUnlock(ActivityType.OnlineReward))
                    {
                        RedPointNotify.SetMark(ERedPointItem.OnlineReward,
                            Root.Instance.OnlineRewardInfo.CanGetReward ? 1 : 0);
                    }
                });

            //任务奖励
            EventDispatcher.Root.AddListener(GlobalEvent.Sync_TaskInfo,
                (sender, args) =>
                {
                    RedPointNotify.SetMark(ERedPointItem.TaskReward, Root.Instance.CurTaskInfo.CanGetReward ? 1 : 0);
                });

            EventDispatcher.Root.AddListener(GlobalEvent.Sync_History,
                (sender, args) =>
                {
                    RedPointNotify.SetMark(ERedPointItem.RecordReward, Root.Instance.RecordRewardCount);
                });

            // 一条龙
            EventDispatcher.Root.AddListener(GlobalEvent.Sync_Dragon,
                (sender, args) =>
                {
                    // 计算一条龙第一条奖励是不是免费
                    try
                    {
                        var level = Root.Instance.Role.dragonInfo.one_stop_level;
                        var itemsDic = Root.Instance.DragonConfig.one_stop_level_list[level.ToString()];
                        var dragonFirstIndex = Root.Instance.Role.dragonInfo.one_stop_claimed;
                        var subId = YZNumberUtil.FormatYZMoney(itemsDic[(dragonFirstIndex + 1).ToString()][0].weight);
                        var chargeInfo = Root.Instance.DragonConfig.one_stop_charge_list.Find
                            (match: charge => charge.sub_id.Equals(subId));
                        bool isDragonRedPoint = chargeInfo == null;
                        RedPointNotify.SetMark(ERedPointItem.Dragon, isDragonRedPoint ? 1 : 0);
                    }
                    catch
                    {
                        RedPointNotify.SetMark(ERedPointItem.Dragon, 0);
                    }
                });

            EventDispatcher.Root.AddListener(GlobalEvent.Duel_Red_Point, (sender, args) =>
            {
                try
                {
                    int stage = 0;
                    int currentCount = Root.Instance.DuelInfo.match_count;
                    for (int i = 0; i < 5; ++i)
                    {
                        if (currentCount >= Root.Instance.FriendsDualConfigs[i].amount)
                        {
                            stage = i + 1;
                        }
                    }

                    var lastClaimed = Root.Instance.DuelInfo?.last_claimed;
                    if (stage > lastClaimed)
                        RedPointNotify.SetMark(ERedPointItem.FriendsDuel, 1);
                    else
                        RedPointNotify.SetMark(ERedPointItem.FriendsDuel, 0);
                }
                catch
                {
                    RedPointNotify.SetMark(ERedPointItem.FriendsDuel, 0);
                }
            });

            EventDispatcher.Root.AddListener(GlobalEvent.SYNC_MUSEUM_INFO, (sender, args) =>
            {
                var rewardCount = Root.Instance.MuseumInfo?.RewardCount ?? 0;
                RedPointNotify.SetMark(ERedPointItem.Museum, rewardCount);
            });

            EventDispatcher.Root.AddListener(GlobalEvent.SYNC_MONTH_CARD_INFO, (sender, args) =>
            {
                var rewardCount = Root.Instance.MonthCardInfo.HaveRewardToClaim ? 1 : 0;
                RedPointNotify.SetMark(ERedPointItem.MonthCard, rewardCount);
            });
            
            EventDispatcher.Root.AddListener(GlobalEvent.SYNC_WEEK_CARD_INFO, (sender, args) =>
            {
                var rewardCount = Root.Instance.WeekInfo.HaveRewardToClaim ? 1 : 0;
                RedPointNotify.SetMark(ERedPointItem.WeekCard, rewardCount);
            });
            
            EventDispatcher.Root.AddListener(GlobalEvent.Click_How_To_Play, (sender, args) =>
            {
                RedPointNotify.SetMark(ERedPointItem.Click_How_To_Play, Root.Instance.ClickHowToPlay ? 0 : 1);
            });
#if DAI_TEST       
            EventDispatcher.Root.AddListener(GlobalEvent.SYNC_MAGIC_BALL_INFO, (sender, args) =>
            {
                if (Root.Instance.MagicBallInfo != null)
                {
                    RedPointNotify.SetMark(ERedPointItem.MagicBall, Root.Instance.MagicBallInfo.EnoughClaimReward ? 1 : 0);
                }
            });
#endif
            
        }
    }
}