using System;
using DataAccess.Utils;
using DataAccess.Utils.Static;

namespace DataAccess.Model
{
    public class OnlineRewardInfo
    {
        public int vip_level = 0;
        public int cool_down_time = 0;
        public int claimed;
        public int last_claimed_time = 0;

        public bool CanGetReward => LessTime <= 0 && RewardCount > claimed;
        
        public int RewardCount
        {
            get
            {
                if (Root.Instance.OnlineActiveConfig == null)
                {
                    return 0;
                }
                var vipLevel = Root.Instance.Role.VipLevel;
                return Root.Instance.OnlineActiveConfig[vipLevel].Length;
            }
        }

        /// <summary>
        /// 所有的美金奖励
        /// </summary>
        public float AllBonus
        {
            get
            {
                if (Root.Instance.OnlineActiveConfig == null)
                {
                    return 0;
                }

                float result = 0;
                var vipLevel = Root.Instance.Role.VipLevel;
                foreach (var reward in Root.Instance.OnlineActiveConfig[vipLevel])
                {
                    if (reward.type is Const.Bonus or Const.Cash)
                    {
                        result += reward.amount;
                    }
                }
                return result;
            }
        }
        
        public bool GetAllReward => claimed >= RewardCount;
        
        public int CurrentIndex => Math.Min(claimed, RewardCount - 1);
        
        public int LessTime
        {
            get
            {
                var lastTime = last_claimed_time;
                var coolDownTime = cool_down_time;
                var lessTime = coolDownTime - (TimeUtils.Instance.UtcTimeNow - lastTime);
                return lessTime;
            }
        }
    }
}