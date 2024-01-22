using System.Collections.Generic;
using DataAccess.Utils;

namespace DataAccess.Model
{
    public class InfiniteWeekConfig
    {
        public Dictionary<int, InfiniteWeekBonusInfo> bonus_info;
        public InfiniteWeekChargeInfo charge_info;
    }

    public class InfiniteWeekBonusInfo
    {
        public int type;
        public float week;
        public float day;
    }

    public class InfiniteWeekChargeInfo
    {
        public List<ChargeGoodInfo> week;
    }
    
    public class InfiniteWeekInfo
    {
        public struct TimeList
        {
            /// <summary>
            /// 周卡剩余天数
            /// </summary>
            public int week;
        }
        
        
        public int level;           // 当前玩家所处于的一个挡位
        public int buy_level;       // 当前玩家 购买了哪个挡位 
        public TimeList time_list;
        public int week_last_claim_time;
        
        public bool IsWeeklyPassLock => time_list.week <= 0;

        public bool HaveRewardToClaim => CanWeeklyPassClaim;

        /// <summary>
        /// 没有购买
        /// </summary>
        public bool NotBuy => IsWeeklyPassLock;
        
        /// <summary>
        /// 充值完后当天也可以领取
        /// </summary>
        public bool CanWeeklyPassClaim =>
            !IsWeeklyPassLock && week_last_claim_time < TimeUtils.Instance.LocalYearMonthDay;
    }
}