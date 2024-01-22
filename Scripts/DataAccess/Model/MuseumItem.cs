using System;
using Core.Extensions;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using UnityEngine;

namespace DataAccess.Model
{
    /// <summary>
    /// 博物馆数据
    /// </summary>
    public class MuseumItem
    {
        /// <summary>
        /// 从1 开始 ,代表顺序
        /// </summary>
        public int order;

        /// <summary>
        /// 奖励道具
        /// </summary>
        public int type;

        /// <summary>
        /// 奖励的数量
        /// </summary>
        public float amount;

        /// <summary>
        /// 需要的点数
        /// </summary>
        public float weight;

        public bool IsRowUnlock
        {
            get
            {
                if (RowOrder == 1)
                {
                    return true;
                }
                
                //当前行第一个 是否已经解锁
                if (Root.Instance.MuseumItems == null)
                {
                    return false;
                }

                return Root.Instance.MuseumItems[FristRowIndex - 1].currentPoint > 0;
            }
        }

        /// <summary>
        /// 当前行第一个
        /// </summary>
        private int FristRowIndex => (RowOrder - 1) * 5 + 1;

        public float currentPoint
        {
            get
            {
                if (Root.Instance.MuseumItems == null || Root.Instance.MuseumInfo == null)
                {
                    return 0;
                }

                var currentPoint = Root.Instance.MuseumInfo.museum_points;

                foreach (var item in Root.Instance.MuseumItems)
                {
                    if (item.order == order)
                    {
                        break;
                    }

                    currentPoint -= item.weight;
                }

                return Math.Clamp(currentPoint, 0, weight);
            }
        }

        /// <summary>
        /// -1 已领取, 0 未解锁 , 1 解锁 未领取 
        /// </summary>
        /// <returns></returns>
        public int State => GetState();

        /// <summary>
        /// -1 已领取, 0 未解锁 , 1 解锁 未领取 
        /// </summary>
        /// <returns></returns>
        private int GetState()
        {
            if (Root.Instance.MuseumItems == null || Root.Instance.MuseumInfo == null)
            {
                return 0;
            }

            var index = order - 1;
            if (index < 0 || index >= Root.Instance.MuseumInfo.museum_claimed.Length)
            {
                return 0;
            }

            var claim_state = Root.Instance.MuseumInfo.museum_claimed[index].ToString().ToInt32();
            if (claim_state == 1)
            {
                return -1;
            }

            if (currentPoint < weight)
            {
                return 0;
            }

            if (claim_state == 0)
            {
                //解锁 未领取
                return 1;
            }

            YZLog.LogColor("博物馆物品状态出问题 order = " + order);
            return 0;
        }

        /// <summary>
        /// 第几排 1-5
        /// </summary>
        public int RowOrder => (order - 1) / 5 + 1;

        /// <summary>
        /// 第几排 1-5
        /// </summary>
        public int ColOrder
        {
            get
            {
                if (order % 5 == 0)
                {
                    return 5;
                }

                return order % 5;
            }
        }

        public Sprite DisplayBig => State == 0 ? BigMask : BigIcon;

        public Sprite DisplaySmall => State == 0 ? SmallMask : SmallIcon;

        /// <summary>
        /// 展示的背景
        /// </summary>
        public Sprite DisplayBG
        {
            get
            {
                if (!IsBigCard)
                {
                    return MediatorBingo.Instance.GetSpriteByUrl("museum/museum_bg_normal");
                }

                if (State == 0)
                {
                    return MediatorBingo.Instance.GetSpriteByUrl("museum/museum_bg_mask");
                }

                if (State == 1)
                {
                    return MediatorBingo.Instance.GetSpriteByUrl("museum/museum_bg_box");
                }

                return MediatorBingo.Instance.GetSpriteByUrl("museum/museum_bg_show");
            }
        }

        private Sprite BigIcon => MediatorBingo.Instance.GetSpriteByUrl("cards/" + order + "b");

        private Sprite SmallIcon => BigIcon;

        private Sprite SmallMask => BigMask;

        private Sprite BigMask => MediatorBingo.Instance.GetSpriteByUrl("cards/" + order + "bm");

        public bool IsBigCard => order % 5 == 0;

        public string name => I18N.Get("key_museum_name_" + order);
    }

    public class MuseumInfo
    {
        /// <summary>
        /// 累积分数
        /// </summary>
        public float museum_points;

        /// <summary>
        /// 1 已领取, 0 未领取
        /// </summary>
        public string museum_claimed;

        
        /// <summary>
        /// 客户端使用 ， 传递数据
        /// </summary>
        public bool dont_sync_add;

        public int begin_time;

        public int refresh_days;
        
        /// <summary>
        /// 当前道具的点数
        /// </summary>
        public float cur_points
        {
            get
            {
                if (Root.Instance.MuseumItems == null)
                {
                    return 0;
                }

                var currentPoint = museum_points;

                foreach (var item in Root.Instance.MuseumItems)
                {
                    if (currentPoint - item.weight <= 0)
                        break;
                    currentPoint -= item.weight;
                }

                return currentPoint;
            }
        }

        public int LessTime => begin_time + refresh_days * 86400 - TimeUtils.Instance.UtcTimeNow;
        

        private int currnetShowCount = 20;

        public int RewardCount
        {
            get
            {
                if (Root.Instance.MuseumItems == null)
                {
                    return 0;
                }

                int result = 0;
                foreach (var item in Root.Instance.MuseumItems)
                {
                    if (item.State == 1)
                    {
                        result++;
                    }
                }
                
                return result;
            }
        }

        public bool HaveBonusReward
        {
            get
            {
                if (Root.Instance.MuseumItems == null)
                {
                    return false;
                }

                
                foreach (var item in Root.Instance.MuseumItems)
                {
                    if (item.State == 1 && item.type is Const.Bonus or Const.Cash)
                    {
                        return true;
                    }
                }
                
                return false;
            }
        }

        private float AllPoints
        {
            get
            {
                if (Root.Instance.MuseumItems == null)
                {
                    return 0;
                }

                int length = Math.Min(Root.Instance.MuseumItems.Count, currnetShowCount);

                float result = 0;

                for (int i = 0; i < length; i++)
                {
                    result += Root.Instance.MuseumItems[i].weight;
                }
                
                return result;
            }
        }

        /// <summary>
        /// 是否已经 得到了所有分数
        /// </summary>
        public bool IsReachMax => museum_points >= AllPoints;

        
        public float GetRowAllDollars(int rowIndex)
        {
            if (Root.Instance.MuseumItems == null)
            {
                return 0;
            }

            float result = 0;
            foreach (var museumItem in Root.Instance.MuseumItems)
            {
                if (museumItem.RowOrder != rowIndex)
                {
                    continue;
                }

                if (museumItem.type is  Const.Bonus or Const.Cash)
                {
                    result += museumItem.amount;
                }
            }

            return result;
        }
        
    }
}