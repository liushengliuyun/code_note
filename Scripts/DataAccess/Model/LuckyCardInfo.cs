namespace DataAccess.Model
{
    public class LuckyCardInfo
    {
        public int lucky_card_chance;
        public int lucky_card_begin_time; // 开启lucky card时间
        public int[] lucky_card_choose_list;
        public int lucky_card_level; // 当前挡位
        public int end_timestamp; // 关闭时间
    }
}