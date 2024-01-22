using DataAccess.Utils;

namespace DataAccess.Model
{
    public class RoomChargeInfo
    {
        public int charge_B_chance;
        public int charge_chance;


        private int a_beginTime;
        private int b_beginTime;
        
        public int room_charge_A_begin_time
        {
            get
            {
                return AChargeInfo?.begin_time ?? a_beginTime;
            }
            set
            {
                if (AChargeInfo != null)
                {
                    AChargeInfo.begin_time = value;
                }

                a_beginTime = value;
            }
        }

        public int room_charge_B_begin_time
        {
            get { return BChargeInfo?.begin_time ?? b_beginTime; }
            set
            {
                if (BChargeInfo != null)
                {
                    BChargeInfo.begin_time = value;
                }

                b_beginTime = value;
            }
        }

        public bool ShouldGenB => CanOpenB && room_charge_A_begin_time - room_charge_B_begin_time > 600;

        public ChargeGoodInfo AChargeInfo;

        public ChargeGoodInfo BChargeInfo;

        /// <summary>
        /// 是否能够开启活动 , 充值后才能开启
        /// </summary>
        public bool CanOpenA => charge_chance > 0;

        /// <summary>
        /// 活动A持续20分钟
        /// </summary>
        public bool ChargeAIsBegin => CanOpenA && ALessTime > 0;

        public bool CanOpenB => charge_B_chance > 0;

        /// <summary>
        /// 活动B持续10分钟
        /// </summary>
        public bool ChargeBIsBegin => CanOpenB && room_charge_B_begin_time + 10 * 60 >= TimeUtils.Instance.UtcTimeNow;
        

        public int ALessTime => room_charge_A_begin_time + 20 * 60 - TimeUtils.Instance.UtcTimeNow;

        public int BLessTime => room_charge_B_begin_time + 10 * 60 - TimeUtils.Instance.UtcTimeNow;
    }
}