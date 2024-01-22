using System.Collections.Generic;
using Core.Models;
using Core.Server;
using DataAccess.Model;
using UnityEngine;
using Utils;

namespace Core.Manager
{
    public class ActivityManager
    {
        public static YZActivity Shared
        {
            get => PlayerManager.Shared.Activity;
        }
    }

    public class ActivityTime
    {
        public const int EndedTime = 1;
        public const int ClaimTime = 2;
    }

    /// 活动管理
    public class YZActivity
    {
        /// 获取所有可以弹脸的活动
        public List<ActivitiesItem> GetYZAllFaceActivities()
        {
            if (!YZServerApiOrganic.Shared.IsYZShowMoney())
            {
                return new List<ActivitiesItem>();
            }

            // 找出有入口的活动
            List<ActivitiesItem> temp = new List<ActivitiesItem>();
            foreach (ActivitiesItem item in GetYZConfigActivities())
            {
                if (item.id == 3 && GetYZIsShowLuckyCard())
                {
                    // 幸运卡
                    temp.Add(item);
                }

                // if (item.id == 2 && GetYZIsShowSevenDay())
                // {
                //     // 7日任务
                //     temp.Add(item);
                // }
                //
                // if (item.id == 8 && GetYZIsShowBingoParty())
                // {
                //     // Bingo Party
                //     temp.Add(item);
                // }
                // else if (item.id == 40 && GetYZIsShowBingoSlots())
                // {
                //     // Bingo Slots
                //     temp.Add(item);
                // }
                // else if (item.id == 22 && GetYZIsShowBingoCup())
                // {
                //     // Bingo Cup
                //     temp.Add(item);
                // }
                // else if (item.id == 47 && GetYZIsShowFish())
                // {
                //     // 钓鱼
                //     temp.Add(item);
                // }
                // else if (item.id == 50 && GetYZIsShowTask())
                // {
                //     // 任务
                //     temp.Add(item);
                // }
                // else if (item.id == 32 && GetYZIsShowChristmas())
                // {
                //     // 圣诞节充值
                //     temp.Add(item);
                // }
                // else if (GetYZIsShowDiffterCharge(item.id))
                // {
                //     // 差价充值
                //     temp.Add(item);
                // }
                // else if (ChargeManager.Shared.YZGetChargeIsOpen(
                //              ChargeManager.Shared.YZGetChargePositionByActivityId(item.id)))
                // {
                //     // 充值活动(排除圣诞节充值，前面已经处理了，而且这里的判断不准确)
                //     if (item.id != 32)
                //     {
                //         temp.Add(item);
                //     }
                // }
            }

            // // 钓鱼充特殊处理(默认入口常开，不再判断入口数据，通过登录数据判断是否开启)
            // if (GetYZIsShowFishCharge())
            // {
            //     ActivitiesItem cashback = new ActivitiesItem();
            //     cashback.id = 48;
            //     temp.Add(cashback);
            // }
            //
            // // 一条龙特殊处理(默认入口常开，不再判断入口数据，通过登录数据判断是否开启)
            // if (GetYZIsShowDragon())
            // {
            //     ActivitiesItem cashback = new ActivitiesItem();
            //     cashback.id = 33;
            //     temp.Add(cashback);
            // }
            //
            // // 存钱罐特殊处理(默认入口常开，不再判断入口数据，通过登录数据判断是否开启) 
            // if (GetYZIsShowCashback())
            // {
            //     ActivitiesItem cashback = new ActivitiesItem();
            //     cashback.id = 28;
            //     temp.Add(cashback);
            // }

            // 按开始时间排序
            temp.Sort((a, b) =>
            {
                if (a.begin_time == b.begin_time)
                {
                    return a.id - b.id;
                }
                else
                {
                    return a.begin_time - b.begin_time;
                }
            });
            // 按顺序加入
            List<ActivitiesItem> newtemp = new List<ActivitiesItem>();
            // // 1.加入首冲
            // List<int> ids1 = new List<int>() { 3, 4, 9, 10, 23, 30, 31, 34, 39 };
            // for (int i = 0; i < temp.Count; i++)
            // {
            //     if (ids1.Contains(temp[i].id))
            //     {
            //         newtemp.Add(temp[i]);
            //     }
            // }
            //
            // // 2.破冰充值
            // List<int> ids2 = new List<int>() { 44, 45, 46, 47, 48 };
            // for (int i = 0; i < temp.Count; i++)
            // {
            //     if (ids2.Contains(temp[i].id))
            //     {
            //         newtemp.Add(temp[i]);
            //     }
            // }
            //
            // // 3.限时充值
            // List<int> ids3 = new List<int>() { 26, 27, 29, 32, 33, 43 };
            // for (int i = 0; i < temp.Count; i++)
            // {
            //     if (ids3.Contains(temp[i].id))
            //     {
            //         newtemp.Add(temp[i]);
            //     }
            // }
            //
            // // 4.任务活动
            // List<int> ids4 = new List<int>() { 2, 50 };
            // for (int i = 0; i < temp.Count; i++)
            // {
            //     if (ids4.Contains(temp[i].id))
            //     {
            //         newtemp.Add(temp[i]);
            //     }
            // }
            //
            // // 5.普通充值
            // for (int i = 0; i < temp.Count; i++)
            // {
            //     if (!ids1.Contains(temp[i].id) && !ids2.Contains(temp[i].id) && !ids3.Contains(temp[i].id) &&
            //         !ids4.Contains(temp[i].id))
            //     {
            //         newtemp.Add(temp[i]);
            //     }
            // }

            // // 6.高分引导
            // if (GetYZIsShowHighScoreGuide())
            // {
            //     ActivitiesItem cashback = new ActivitiesItem();
            //     cashback.id = 1000;
            //     newtemp.Add(cashback);
            // }

            return newtemp;
        }

        /// 通过id获取活动
        public ActivitiesItem GetYZActivity(int id)
        {
            foreach (ActivitiesItem item in GetYZConfigActivities())
            {
                if (item.id == id)
                {
                    return item;
                }
            }

            return default;
        }

        /// 判断活动是否处于领奖状态
        public bool GetYZIsClaimActivity(int id)
        {
            foreach (ActivitiesItem item in GetYZConfigActivities())
            {
                if (item.id == id)
                {
                    int serve = YZServerApi.Shared.GetYZServerTime();
                    return serve > item.end_time && serve < item.claim_end_time;
                }
            }

            return false;
        }

        /// 是否展示活动入口(通过入口数据时间判断)
        public bool GetYZIsShowActivity(int id, int ty)
        {
            foreach (ActivitiesItem item in GetYZConfigActivities())
            {
                if (item.id == id)
                {
                    int begin = item.begin_time;
                    int ended = item.end_time;
                    int claim = item.claim_end_time;
                    int serve = YZServerApi.Shared.GetYZServerTime();
                    return ty == ActivityTime.EndedTime ? serve > begin && serve < ended : serve > begin && serve < claim;
                }
            }

            return false;
        }

        /// 是否开启幸运卡 
        public bool GetYZIsShowLuckyCard()
        {
            return true;
        }

        // /// 是否有高分教学
        // public bool GetYZIsShowHighScoreGuide()
        // {
        //     int timer = PlayerPrefs.GetInt(YZConstUtil.YZHighScoreGuideTimerInt, 0);
        //     int newtm = YZServerApi.Shared.GetYZServerTime();
        //     return timer > newtm;
        // }
        //
        // /// 是否开启7天乐
        // public bool GetYZIsShowSevenDay()
        // {
        //     foreach (ActivitiesItem item in GetYZConfigActivities())
        //     {
        //         if (item.id == 2)
        //         {
        //             if (PlayerManager.Shared.Player.data != null && PlayerManager.Shared.Player.Other != null &&
        //                 PlayerManager.Shared.Player.Other.sevendays != null)
        //             {
        //                 return PlayerManager.Shared.Player.Other.sevendays.end_timestamp >=
        //                        YZServerApi.Shared.GetYZServerTime();
        //             }
        //         }
        //     }
        //
        //     return false;
        // }
        //
        // /// 是否开启Bingo Party
        // public bool GetYZIsShowBingoParty()
        // {
        //     if (GetYZIsShowActivity(8, ActivityTime.EndedTime) &&
        //         PlayerManager.Shared.Player.Other.user_collect_bingo_info != null)
        //     {
        //         return true;
        //     }
        //
        //     return false;
        // }
        //
        // /// 是否开启Bingo Slots
        // public bool GetYZIsShowBingoSlots()
        // {
        //     if (GetYZIsShowActivity(40, ActivityTime.EndedTime) &&
        //         PlayerManager.Shared.Player.Other.user_slots_bingo_info != null)
        //     {
        //         return true;
        //     }
        //
        //     return false;
        // }
        //
        // /// 是否开启Bingo Cup
        // private bool GetYZIsShowBingoCup()
        // {
        //     return BingocupManager.Shared.YZGetIsShowBingocup();
        // }
        //
        // /// 是否开启圣诞节
        // public bool GetYZIsShowChristmas()
        // {
        //     if (PlayerManager.Shared.Player.data.other.count.game_end_count < 3)
        //     {
        //         return false;
        //     }
        //
        //     return ChargeManager.Shared.YZGetChargeIsOpen(ChargeManager.Shared.YZGetChargePositionByActivityId(32));
        // }
        //
        // /// 是否开启差价充值
        // public bool GetYZIsShowDiffterCharge(int id)
        // {
        //     if (id == 20 || id == 21)
        //     {
        //         return ChargeManager.Shared.YZGetDiffterChargeLocalIsOpen(id);
        //     }
        //
        //     return false;
        // }
        //
        // /// 是否开启一条龙
        // public bool GetYZIsShowDragon()
        // {
        //     return YZChargeActivity2023UICtrler.GetChargePresentEndTimeOffset() > 0;
        // }
        //
        // /// 是否开启钓鱼
        // public bool GetYZIsShowFish()
        // {
        //     return PlayerManager.Shared.GetFishActivityTimstampOffset() > 0;
        // }
        //
        // /// 是否开启任务
        // public bool GetYZIsShowTask()
        // {
        //     return false;
        // }
        //
        // /// 是否开启钓鱼充值
        // public bool GetYZIsShowFishCharge()
        // {
        //     return YZChargeActivityFishUICtrler.IsOpenFishCharge();
        // }
        //
        // /// 是否开启存钱罐
        // public bool GetYZIsShowCashback()
        // {
        //     if (PlayerManager.Shared.Player.data != null && PlayerManager.Shared.Player.Other != null &&
        //         PlayerManager.Shared.Player.Other.moneybox_info != null)
        //     {
        //         return PlayerManager.Shared.Player.Other.moneybox_info.end_timestamp >=
        //                YZServerApi.Shared.GetYZServerTime();
        //     }
        //
        //     return false;
        // }
        //
        // /// 获取存钱罐数值
        // public float YZGetCashbackValue()
        // {
        //     if (PlayerManager.Shared.Player.data != null && PlayerManager.Shared.Player.Other != null &&
        //         PlayerManager.Shared.Player.Other.moneybox_info != null)
        //     {
        //         return PlayerManager.Shared.Player.Other.moneybox_info.reward_round.ToFloat();
        //     }
        //
        //     return 0.0f;
        // }

        public int GetYZActivityTime(ActivitiesItem activitydata)
        {
            bool ispre = int.TryParse(activitydata.begin_preheat, out int preat);
            int server = YZServerApi.Shared.GetYZServerTime();
            if (activitydata.id == 3)
            {
                if (GetYZIsShowLuckyCard())
                {
                    return Root.Instance.Role.luckyCardInfo.end_timestamp - server;
                }
            }
            else
            {
                if (ispre && preat > server)
                {
                    return preat - server;
                }
                else if (activitydata.begin_time > server)
                {
                    return activitydata.begin_time - server;
                }
                else if (activitydata.end_time > server)
                {
                    return activitydata.end_time - server;
                }
                else if (activitydata.claim_end_time > server)
                {
                    return activitydata.claim_end_time - server;
                }
            }

            return -1;
        }

        private List<ActivitiesItem> GetYZConfigActivities()
        {
            if (!YZServerApiOrganic.Shared.IsYZShowMoney())
            {
                return new List<ActivitiesItem>();
            }

            List<ActivitiesItem> temp = new List<ActivitiesItem>();
            if (Root.Instance.Role != null && Root.Instance.Role.activities != null)
            {
                return Root.Instance.Role.activities;
            }

            return temp;
        }
    }
}