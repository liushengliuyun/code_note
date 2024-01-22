using Core.Extensions;
using DataAccess.Utils;
using Newtonsoft.Json;

namespace DataAccess.Model
{
    public class UserInfo
    {
        public int id;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string email;

        /// <summary>
        /// 是否已验证
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int validata;

        /// <summary>
        /// 是否是自然量玩家
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? is_organic;


        public string game_guide_list;

        /// <summary>
        /// 匹配了就算
        /// </summary>
        public int match_count;


        public string v_phone;

        public string v_email;

        /// <summary>
        /// 上一次真机场打的对象
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int last_cash_room;

        public bool IsBindedVipInfo => !v_phone.IsNullOrEmpty() || !v_email.IsNullOrEmpty();


        public string save_email_group;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int high_score_teaching_begin_time;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DiscountInfo discount_info;

        /// <summary>
        /// 最后一次取消提现时间
        /// </summary>
        public int last_cancel_cash_time;

        /// <summary>
        /// 剩余刷新时间
        /// </summary>
        public int lastCancelCashTime => last_cancel_cash_time + 3 * 86400 - TimeUtils.Instance.UtcTimeNow;
        
        public bool InCancelCD
        {
            get
            {
                return lastCancelCashTime > 0;
            }
        }
    }

    public class DiscountInfo
    {
        public string paypal_discount;
        public string glocash_discount;
    }

}