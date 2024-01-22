using System.Collections.Generic;
using DataAccess.Utils.Static;
using Newtonsoft.Json;
using UI;

namespace DataAccess.Model
{
    public class FortuneWheelInfo
    {
        public int wheel_free_ticket;

        /// <summary>
        /// charge_id为key，次数为value
        /// </summary>
        public Dictionary<int, int> wheel_pay_chance;

        public float offer_price;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public float pay_level_threshold;

        public bool IsLocateBig => offer_price >= pay_level_threshold;

        /// <summary>
        /// 是否有
        /// </summary>
        public bool HaveChance => wheel_free_ticket >= Const.FREE_WHEEL_COST || PaySmallCount > 0 || PayBigCount > 0;

        public int PaySmallCount
        {
            get
            {
                if (wheel_pay_chance == null)
                {
                    return 0;
                }

                wheel_pay_chance.TryGetValue(GetPayChargeId(WheelType.PaySmall), out var result);
                return result;
            }
        }

        public int PayBigCount
        {
            get
            {
                if (wheel_pay_chance == null)
                {
                    return 0;
                }

                wheel_pay_chance.TryGetValue(GetPayChargeId(WheelType.PayBig), out var result);
                return result;
            }
        }

        /// <summary>
        /// 获取充值转盘的充值id
        /// </summary>
        /// <returns></returns>
        public static int GetPayChargeId(WheelType wheelType)
        {
            var key = GetKey(wheelType);

            Root.Instance.WheelChargeInfos.TryGetValue(key, out var data);
            if (data != null)
            {
                return data.id;
            }

            return -1;
        }

        public static int GetKey(WheelType wheelType)
        {
            switch (wheelType)
            {
                case WheelType.PaySmall:
                    return 1;
                case WheelType.PayBig:
                    return 2;
            }

            return -1;
        }
    }
}