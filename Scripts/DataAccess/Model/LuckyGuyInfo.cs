using DataAccess.Utils;
using DataAccess.Utils.Static;

namespace DataAccess.Model
{
    public class LuckyGuyInfo
    {
        public int play_chance;

        /// <summary>
        /// 充值状态：0-初始化，1-两次对局已完成，2-待充值，3-充值完成
        /// </summary>
        public int pay_status;

        
        public int register_time;


        public bool IsChargeSuccess => pay_status == 3;
        
        public bool IsOpen
        {
            get
            {
                if (IsFinish()) return false;

                if (TimeUtils.Instance.UtcTimeNow - FirstFailTime <= GlobalEnum.Lucky_guy_show_interval)
                {
                    return false;
                }

                if (play_chance == 1)
                {
                    if (LuckyRoom != null)
                    {
                        var matchHistory = Root.Instance.MatchHistory.Find(history =>
                            history.room_id == LuckyRoom.id && history.IsWait);

                        //要等游戏出结果
                        if (matchHistory != null)
                        {
                            return false;
                        }

                        //预期结果 如果是 1，则没有机会， 否则还有一次机会
                        if (matchHistory is { want_rank: 1 })
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        public bool IsFinish(bool check_play_chance = true)
        {
            if (check_play_chance && play_chance <= 0)
            {
                return true;
            }

            if (pay_status != 0)
            {
                return true;
            }

            //有任何充值都不显示
            if (Root.Instance.ChargeSuccessCount > 0)
            {
                return true;
            }

            return false;
        }

        public int FirstFailTime;

        private int OpenTime => register_time + 86400;
        
        /// <summary>
        /// 距离活动开始的时间
        /// </summary>
        public int UntilOpenTime => OpenTime - TimeUtils.Instance.UtcTimeNow;

        /// <summary>
        /// 倒计时
        /// </summary>
        public int RefreshCountTime
        {
            get
            {
                var result = 86400 - (TimeUtils.Instance.UtcTimeNow - OpenTime) % 86400;

                if (result == 86400)
                {
                    result = 0;
                }
                
                return result;
            }
        }

        public Room LuckyRoom
        {
            get
            {
                if (Root.Instance.RoomList == null)
                {
                    return null;
                }

                return Root.Instance.RoomList.Find(room => room.IsLuckyRoom);
            }
        }

        public ChargeGoodInfo LuckGood
        {
            get
            {
                if (Root.Instance.LuckyGuyConfig is {Count: < 1})
                {
                    return null;
                }

                return Root.Instance.LuckyGuyConfig[0];
            }
        }
    }
}