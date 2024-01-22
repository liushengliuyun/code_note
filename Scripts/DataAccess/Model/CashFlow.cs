using HT.InfiniteList;

namespace DataAccess.Model
{
    public class CashFlow : InfiniteListData
    {
        public int id;
        public int date;
        public int user_id;
        public int type;
        public string money;
        public string bonus;
        public int current_chips;
        public int current_coin;
        public int chips;
        public int coin;
        public string current_money;
        public string current_bonus;
        public int method;
        public int param1;
        public int param2;
        public string created_at;
        public string updated_at;
        
        public bool PullFlag;
        public bool IsOverLengthSign;
    }
}