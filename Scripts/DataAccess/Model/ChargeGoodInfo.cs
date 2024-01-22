using System;
using System.Collections.Generic;
using DataAccess.Utils.JsonParse;
using Newtonsoft.Json;

namespace DataAccess.Model
{
    /// <summary>
    /// 充值项信息
    /// </summary>
    public class ChargeGoodInfo
    {
        public int id;

        public string type;

        public string name;

        public int sub_id;

        public int status;

        public string position;

        /// <summary>
        /// 价格, 充值的美金
        /// </summary>
        public float amount;
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        private float show_bonus;
        
        [JsonConverter(typeof(StringToDicStringObj))]
        public Dictionary<string, object> out_items;

        public int begin_time;

        public DateTime created_at { get; set; }

        public DateTime updated_at { get; set; }

        public int MoreValue => (int)Math.Floor(ShowBonus / amount * 100);

        public float ShowBonus
        {
            get
            {
                return show_bonus;
                
                if (out_items == null)
                {
                    return 0;
                }

                return Convert.ToSingle(out_items["bonus"]);
            }
        }

        /// <summary>
        /// 钻石
        /// </summary>
        public float ShowGems
        {
            get
            {
                if (out_items == null)
                {
                    return 0;
                }

                return Convert.ToSingle(out_items["chips"]);
            }
        }
    }

    public class StarterPackInfo
    {
        /// <summary>
        /// 充值挡位
        /// </summary>
        public int starter_pack_level;

        /// <summary>
        /// 
        /// </summary>
        public int starter_pack_chance;


        /// <summary>
        /// 跟挡位相关的时间
        /// </summary>
        public int starter_pack_begin_time;
        
        
        /// <summary>
        /// 活动开始时间
        /// </summary>
        public int starter_park_create_time;
        
        public bool CanBuy => starter_pack_chance > 0;
    }
}