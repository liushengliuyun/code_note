namespace DataAccess.Model
{
    public class AdditionalGiftConfig
    {
        public int id;
        public string type;
        public string sub_id;
        public string name;
        public float amount;
        public string created_at;
        public string updated_at;
    }

    public class AdditionalGiftInfo
    {
        public int begin_time;
        public float charge_amount;
    }
}