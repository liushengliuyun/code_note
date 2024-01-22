
using DataAccess.Utils.Static;

namespace UI.UIWithDrawFlow
{
    public class WithdrawHistoryData
    {
        public int id ;
        public string user_id ;
        public string cash_id ;
        public string freeze_type ;
        public float freeze_amount ;
        public string address ;
        public int status ;
        public string receive;
        public string cost;
        public float bonus;
        public string finish_at ;
        public string created_at ;
        public string update_at ;


        public bool InProgress()
        {
            return WithDrawState.IsInProgress(status);
        }
    }
}