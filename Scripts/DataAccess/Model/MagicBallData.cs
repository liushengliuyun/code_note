using System.Collections.Generic;
using DataAccess.Utils;
using DataAccess.Utils.Static;

namespace DataAccess.Model
{
    public class MagicBallData
    {
        public MagicBallInfo info => Root.Instance.MagicBallInfo;

        public int order;

        /// <summary>
        /// 奖励类型
        /// </summary>
        public int type;

        /// <summary>
        /// 奖励数量
        /// </summary>
        public float amount;

        /// <summary>
        /// 需要解锁的魔法能量数量
        /// </summary>
        public float weight;

        /// <summary>
        /// 页数 从0开始
        /// </summary>
        public int page => (order - 1) / GlobalEnum.MAGIC_BALL_STEP;


        public int pageIndex => (order - 1) % GlobalEnum.MAGIC_BALL_STEP;

        /// <summary>
        /// 是否已经解锁 , 第一页魔法球默认开放，其他页魔法球，只有在前面的所有魔法球被点亮后开启
        /// </summary>
        public bool IsUnLock => info != null && info.IsPageUnlock(page);


        /// <summary>
        /// 是否可领取
        /// </summary>
        public bool IsClaimed => info != null && info.IsClaimed(order);

        public Item PageReward
        {
            get
            {
                if (Root.Instance.MagicPageRewards == null)
                {
                    return null;
                }

                if (page < 0 || page >= Root.Instance.MagicPageRewards.Count)
                {
                    return null;
                }

                var reward = Root.Instance.MagicPageRewards[page];
                return new Item(reward.type, reward.amount);
            }
        }
        
        public bool WillGetAddedBonus
        {
            get
            {
                if (info == null)
                {
                    return false;
                }

                if (IsClaimed)
                {
                    return false;
                }
                
                for (int i = 0; i < 4; i++)
                {
                    var ball_data = info.GetMagicBallData(page, i);

                    if (ball_data == null)
                    {
                        continue;
                    }
                    if (ball_data == this)
                    {
                        continue;
                    }
                    if (!ball_data.IsClaimed)
                    {
                        return false;
                    }
                }

                //其余三个全部领取
                return true;
            }
        }
    }

    public class MagicPageReward
    {
        public int order;
        public int type;
        public float amount;
    }

    public class MagicBallInfo
    {
        public bool dont_sync_add;
        
        public int begin_time;

        /// <summary>
        /// 活动天数
        /// </summary>
        public int days;

        /// <summary>
        /// 当前累计分数
        /// </summary>
        public float magic_essence;

        public string wizard_claimed;

        /// <summary>
        /// 累计奖励领取位置
        /// </summary>
        public int all_claimed;
        
        public float CurrentPoint
        {
            get
            {
                if (config == null)
                {
                    return 0;
                }

                var result = magic_essence;
                foreach (var ballData in config)
                {
                    if (ballData.IsClaimed)
                    {
                        result -= ballData.weight;
                    }
                }

                return result;
            }
        }

        private List<MagicBallData> config => Root.Instance.MagicBallDatas;

        private List<MagicPageReward> pageRewards => Root.Instance.MagicPageRewards;

        public int LessTime => begin_time + days * 86400 - TimeUtils.Instance.UtcTimeNow;

        public bool EnoughClaimReward
        {
            get
            {
                if (config == null)
                {
                    return false;
                }

                // if (IsGetAllReward)
                // {
                //     return false;
                // }
                
                for (int i = 0; i < GlobalEnum.MAGIC_BALL_STEP; i++)
                {
                    var data = GetMagicBallData(CurrentPage, i);

                    if (data == null)
                    {
                        continue;
                    }
                    
                    if (data.IsClaimed)
                    {
                        continue;
                    }

                    if (CurrentPoint >= data.weight)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
        
        public bool IsReachMax
        {
            get
            {
                if (config == null)
                {
                    return false;
                }
                
                return magic_essence >= AllPoints;
            }
        }

        private float AllPoints
        {
            get
            {
                if (config == null)
                {
                    return 0;
                }

                float result = 0;

                foreach (var ballData in config)
                {
                    result += ballData.weight;
                }

                return result;
            }
        }
        
        public bool IsGetAllReward
        {
            get
            {
                if (config == null)
                {
                    return false;
                }

                foreach (var ballData in config)
                {
                    if (!IsClaimed(ballData.order))
                    {
                        return false;
                    }
                }
                
                return true;
            }
        }

        public MagicBallData GetMagicBallData(int page, int i)
        {
            if (config == null)
            {
                return null;
            }

            var index = page * GlobalEnum.MAGIC_BALL_STEP + i;
            if (index >= 0 && index < config.Count)
            {
                return config[index];
            }

            return null;
        }


        public bool CanClaim(int order)
        {
            if (order < 0 || order >= config.Count)
            {
                return false;
            }

            if (IsClaimed(order))
            {
                return false;
            }

            return magic_essence > UnlockNeedWeight(order);
        }

        /// <summary>
        /// 解锁需要的点数
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        private float UnlockNeedWeight(int order)
        {
            if (order < 0 || order >= config.Count)
            {
                return 0;
            }

            float result = 0;
            for (int i = 0; i < order; i++)
            {
                var data = config[i];
                result += data.weight;
            }

            return result;
        }

        /// <summary>
        /// 是否已经领取
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public bool IsClaimed(int order)
        {
            var state = wizard_claimed[order - 1].ToString().ToInt32();

            return state == 1;
        }

        public float PageTotalBonus(int page)
        {
            float result = 0;
            foreach (var ballData in config)
            {
                if (page != ballData.page)
                {
                    continue;
                }

                if (ballData.type is Const.Bonus or Const.Cash)
                {
                    result += ballData.amount;
                }
            }

            return result;
        }

        public float PageAddedBonus(int page)
        {
            if (page < 0 || page >= pageRewards.Count)
            {
                return 0;
            }

            var magicPageReward = pageRewards[page];
            if (magicPageReward.type is Const.Bonus or Const.Cash)
            {
                return magicPageReward.amount;
            }

            return 0;
        }

        /// <summary>
        /// page 从0开始
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public bool IsPageUnlock(int page)
        {
            return page <= CurrentPage;
        }

        public int CurrentPage => all_claimed;
    }
}