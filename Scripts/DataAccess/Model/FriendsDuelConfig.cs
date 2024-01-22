namespace DataAccess.Model
{
    public class FriendsDuelConfig
    {
        public int order;
        public int type;
        public float amount;
        public float weight;
    }

    public class DuelInfo
    {
        public int match_count;
        public int last_claimed;
        public string wait_match;
        public int room_id;
    }
    
    // status 当前对战房间状态：0-等待匹配中，1-匹配成功，2-对局中，3-取消匹配，4-超时等待，5-对局结束

    public class DuelData
    {
        public int id;
        public int room_id;
        public string room_no;
        public int status;
        public int create_time;
    }

    public class DuelCloseData
    {
        public int id;
        public int room_id;
        public int status;
    }

    public class DuelStatusInfo
    {
        public int room_id;
        public string room_no;
        public int status;
        public Competitor competitor;
    }

    public class Competitor
    {
        public int user_id;
        public string nickname;
        public int head_index;
        public string head_url;
    }
}