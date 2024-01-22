using Newtonsoft.Json;

namespace DataAccess.Model
{
    public class ShopInfo
    {
        public int id;
        public int sub_id;
        public float money;
        public float bonus;
    }

    public class DailyReward
    {
        public int order;
        public int type;
        public float amount;
    }

    public class WheelChargeInfo
    {
        public int id;
        public string type;
        public int sub_id;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string name;

        public float amount;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public float show_bonus;

        public int status;
    }
}