using System;
using System.Collections.Generic;

namespace DataAccess.Model
{
    public class PiggyBankRow
    {
        /// <summary>
        /// 充值额度
        /// </summary>
        public float cash;

        public Dictionary<string, float> add;

        /// <summary>
        /// 初始bonus
        /// </summary>
        public float base_bonus;


        public float MaxBonus
        {
            get
            {
                var getMaxSuccess = add.TryGetValue("max", out var max);
                if (getMaxSuccess)
                {
                    return max;
                }

                return 0;
            }
        }
        
        
        //amount的值不会变
        /*public bool IsFull
        {
            get
            {
                var getAmountSuccess = add.TryGetValue("amount", out var amount);
                var getMaxSuccess = add.TryGetValue("max", out var max);
                if (getAmountSuccess && getMaxSuccess)
                {
                    return amount >= max;
                }

                return false;
            }
        }*/

        public float AllBonus
        {
            get
            {
                var getAmountSuccess = add.TryGetValue("amount", out var amount);
                var getMaxSuccess = add.TryGetValue("max", out var max);
                if (getAmountSuccess && getMaxSuccess)
                {
                    return Math.Min(max, amount) + base_bonus;
                }

                return base_bonus;
            }
        }
    }

    public class PiggyBankConfig
    {
        public Dictionary<int, PiggyBankRow> piggy_bank;

        public List<ChargeGoodInfo> charge_info;
    }


    public class PiggyBankInfo
    {
        /// <summary>
        /// 存钱罐活动是否展示，0-不展示，1-展示
        /// </summary>
        public int piggy_chance;

        public int piggy_level;

        public float AddPiggyBonus;

        /// <summary>
        /// 已存bonus
        /// </summary>
        public float piggy_bonus;

        public int start_time;

        public PiggyBankRow Row
        {
            get
            {
                if (Root.Instance.PiggyBankConfig == null)
                {
                    return null;
                }

                Root.Instance.PiggyBankConfig.piggy_bank.TryGetValue(piggy_level, out var row);
                return row;
            }
        }

        /// <summary>
        /// 存钱罐是否满了
        /// </summary>
        public bool IsFull
        {
            get
            {
                if (Row == null)
                {
                    return false;
                }
                
                return AllBonus >= Row.MaxBonus;
            }
        }

        public float Cash => Row?.cash ?? 0;

        /// <summary>
        /// 全部会获得的bonus
        /// </summary>
        public float AllBonus => piggy_bonus;

        public ChargeGoodInfo ChargeInfo => Root.Instance.PiggyBankConfig.charge_info[piggy_level - 1];
    }
}