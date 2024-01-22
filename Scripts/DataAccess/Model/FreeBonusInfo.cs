namespace DataAccess.Model
{
    public class FreeBonusInfo
    {
        public int free_bonus_level;
        
        /// <summary>
        /// 当日剩余次数 这是充值次数
        /// </summary>
        public float free_bonus_daily_chance;
        
        /// <summary>
        /// 可以进行游戏的次数
        /// </summary>
        public float free_bonus_game_chance;
        
        /// <summary>
        /// 这里服务器视为余额 , 要大于进场要扣除的 , 也就时Amount , 这里是防御写法, 正常应该每次进入都扣完的
        /// </summary>
        public bool CanPlay => free_bonus_game_chance >= Amount;
        
        public bool Lock => !CanPlay && free_bonus_daily_chance <= 0;
        // public bool Lock => true;
        
        public ChargeGoodInfo ChargeInfo
        {
            get
            {
                if (Root.Instance.FreeBonusConfig == null)
                {
                    return null;
                }

                return Root.Instance.FreeBonusConfig.Find(info => info.sub_id == free_bonus_level);
            }
        }

        public float Amount
        {
            get
            {
                if (ChargeInfo == null)
                {
                    return 0;
                    
                }
                return ChargeInfo.amount;
            }
        }
    }
}