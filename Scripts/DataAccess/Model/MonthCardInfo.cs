using DataAccess.Utils;

namespace DataAccess.Model
{
    public class MonthCardInfo
    {
        public struct TimeList
        {
            /// <summary>
            /// 周卡剩余天数
            /// </summary>
            public int week;

            /// <summary>
            /// 月卡剩余天数
            /// </summary>
            public int month;
        }

        /// <summary>
        /// 挡位
        /// </summary>
        public int level;

        /// <summary>
        /// 周卡最后一次领取时间
        /// </summary>
        public int week_last_claim_time;

        /// <summary>
        /// 周卡最后一次领取时间
        /// </summary>
        public int month_last_claim_time;

        public TimeList time_list;


        public bool IsWeeklyPassLock => time_list.week <= 0;

        /// <summary>
        /// 月卡还没有解锁 或者已经领完了
        /// </summary>
        public bool IsMonthlyPassLock => time_list.month <= 0;

        /// <summary>
        /// 充值完后当天也可以领取
        /// </summary>
        public bool CanWeeklyPassClaim =>
            !IsWeeklyPassLock && week_last_claim_time < TimeUtils.Instance.LocalYearMonthDay;

        public bool CanMonthlyPassClaim =>
            !IsMonthlyPassLock && month_last_claim_time < TimeUtils.Instance.LocalYearMonthDay;

        public bool HaveRewardToClaim => CanWeeklyPassClaim || CanMonthlyPassClaim;

        public bool NotBuy => IsWeeklyPassLock && IsMonthlyPassLock;

        public float WeekBuyBonus
        {
            get
            {
                Root.Instance.MonthCardBonusInfos.TryGetValue(level, out var bonusInfo);
                if (bonusInfo == null)
                {
                    return 0;
                }

                return bonusInfo.week;
            }
        }


        public float MonthBuyBonus
        {
            get
            {
                Root.Instance.MonthCardBonusInfos.TryGetValue(level, out var bonusInfo);
                if (bonusInfo == null)
                {
                    return 0;
                }

                return bonusInfo.month;
            }
        }

        public ChargeGoodInfo WeeklyChargeGoodInfo
        {
            get
            {
                if (Root.Instance.WeekCardChargeInfos == null)
                {
                    return null;
                }

                if (level < 1 && level > Root.Instance.WeekCardChargeInfos.Count)
                {
                    return null;
                }

                return Root.Instance.WeekCardChargeInfos[level - 1];
            }
        }

        public ChargeGoodInfo MonthlyChargeGoodInfo
        {
            get
            {
                if (Root.Instance.MonthCardChargeInfos == null)
                {
                    return null;
                }

                if (level < 1 && level > Root.Instance.MonthCardChargeInfos.Count)
                {
                    return null;
                }

                return Root.Instance.MonthCardChargeInfos[level - 1];
            }
        }
    }

    public class MonthCardBonusInfo
    {
        /// <summary>
        /// 道具类型
        /// </summary>
        public int type;

        public float week;
        public float month;
    }
}