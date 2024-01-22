using System.Collections.Generic;

namespace DataAccess.Model
{
    public class DragonConfig
    {
        public Dictionary<string, Dictionary<string, List<DragonAwardItem>>> one_stop_level_list;
        public List<DragonPurchaseItem> one_stop_charge_list;
    }

    public class DragonAwardItem
    {
        public string order;
        public string type;
        public string amount;
        public string weight;
    }

    public class DragonPurchaseItem
    {
        public int id;
        public string type;
        public string sub_id;
        public string name;
        public string amount;
        public string show_bonus;
        public string out_items;
        public string status;
        public string create_at;
        public string update_at;
    }
}