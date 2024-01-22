using System.Collections.Generic;
using DataAccess.Utils.JsonParse;
using Newtonsoft.Json;

namespace DataAccess.Model
{
    public class SpecialOfferConfig
    {
        public int id;
        public string type;
        public string sub_id;
        public string name;
        public float amount;
        public float show_bonus;
        [JsonConverter(typeof(StringToDicStringObj))]
        public Dictionary<string, object> out_items;
        public string created_at;
        public string updated_at;
    }

    public class SpecialOfferInfo
    {
        public int show_time;
    }
}