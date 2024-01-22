using Newtonsoft.Json;

namespace DataAccess.Model
{
    public class ChargeMethods
    {
        public string type;
        public string title;
        public string icon;
        public string url;
        public string sub_types;
    }
    
    public class ChargeInfo
    {
        /// <summary>
        /// 总充值成功次数
        /// </summary>
        public int success_count;
        /// <summary>
        /// 总充值金额
        /// </summary>
        public float success_total;
        
        public float recent_success_amount;
        /// <summary>
        /// 最近一次充值的时间
        /// </summary>
        public int recent_success_date;
        
        /// <summary>
        /// 1 今日已充值 , 0 今日无充值
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int today_charge_index;
    }
}