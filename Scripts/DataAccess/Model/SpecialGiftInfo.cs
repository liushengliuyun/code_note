using System.Collections.Generic;
using DataAccess.Utils;
using DataAccess.Utils.JsonParse;
using Newtonsoft.Json;

namespace DataAccess.Model
{
    public class SpecialGiftInfo
    {
        public int special_gift_chance;             // 是否展示：0-不展示，1-展示
        public int special_gift_today_chance;       // 今天充值机会次数
        public charge_info charge_info;
        public string today_week;

        public int special_gift_create_time;
        public int special_gift_end_time;
        
        public int LessTime => special_gift_end_time - TimeUtils.Instance.UtcTimeNow;
    }

    public class charge_info
    {
        public int id;
        public string type;
        public int sub_id;
        public string name;
        public string amount;
        public float show_bonus;
        public string show_ites;
        [JsonConverter(typeof(StringToDicStringObj))]
        public Dictionary<string, object> out_items;
        public string extra;
        public string condition;
        public int status;
        public string create_at;
        public string updated_at;
    }
}