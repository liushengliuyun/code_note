using LitJson;

namespace Core.Models
{
    public class YZConfig
    {
    }

    public class ListItem
    {
        public int id;
        public string seat;
        public string title;
        public string sub_title;
        public JsonData in_items;
        public JsonData out_items;
        public JsonData open_conditions;
        public string rank_weight;
        //public Extra extra;
        public JsonData other_configs;
        public int is_open;

        // public string RuleDesc()
        // {
        //     int game_time = BRJsonUtil.GetBRInt(other_configs, "game_time", 0);
        //     if (game_time <= 0)
        //     {
        //         game_time = 90;
        //     }
        //
        //     int ball_time = BRJsonUtil.GetBRInt(other_configs, "ball_time", 0);
        //     if (ball_time <= 0)
        //     {
        //         ball_time = 3;
        //     }
        //
        //     return BRString.Format(BRLocal.GetLocal(BRLocalID.key_room_rule), game_time, ball_time);
        // }
        //
        // public string LockDesc()
        // {
        //     if (ABGroupManager.Shared.BRIsBCGroup(ABTagName.room_show_1201))
        //     {
        //         int chipsnumber = PlayerManager.Shared.Player.Other.count.total_chips_game_count;
        //         int chipslimit = BRJsonUtil.GetBRInt(open_conditions, "chips_game", -1);
        //         if (chipslimit > 0 && chipslimit > chipsnumber)
        //         {
        //             return BRString.Format(BRLocal.GetLocal(BRLocalID.key_unlock_room_gem), chipslimit - chipsnumber);
        //         }
        //
        //         int moneynumber = PlayerManager.Shared.Player.Other.count.total_money_game_count;
        //         int moneylimit = BRJsonUtil.GetBRInt(open_conditions, "money_game", -1);
        //         if (moneylimit > 0 && moneylimit > moneynumber)
        //         {
        //             return BRString.Format(BRLocal.GetLocal(BRLocalID.key_unlock_room_cash), moneylimit - moneynumber);
        //         }
        //     }
        //     else
        //     {
        //         return "Locked";
        //     }
        //
        //     return "";
        // }
        //
        // // 是否有锁
        // public bool IsLocked(int type = -1)
        // {
        //     if (is_open == 1)
        //     {
        //         if (ABGroupManager.Shared.BRIsBCGroup(ABTagName.room_show_1201))
        //         {
        //             if (type == RewardType.Chips)
        //             {
        //                 int chipsnumber = PlayerManager.Shared.Player.Other.count.total_chips_game_count;
        //                 int chipslimit = BRJsonUtil.GetBRInt(open_conditions, "chips_game", -1);
        //                 if (chipslimit > 0 && chipslimit > chipsnumber)
        //                 {
        //                     return true;
        //                 }
        //             }
        //             else if (type == RewardType.Money || type == RewardType.Bonus)
        //             {
        //                 int moneynumber = PlayerManager.Shared.Player.Other.count.total_money_game_count;
        //                 int moneylimit = BRJsonUtil.GetBRInt(open_conditions, "money_game", -1);
        //                 if (moneylimit > 0 && moneylimit > moneynumber)
        //                 {
        //                     return true;
        //                 }
        //             }
        //             else
        //             {
        //                 int moneynumber = PlayerManager.Shared.Player.Other.count.total_money_game_count;
        //                 int moneylimit = BRJsonUtil.GetBRInt(open_conditions, "money_game", -1);
        //                 if (moneylimit > 0 && moneylimit > moneynumber)
        //                 {
        //                     return true;
        //                 }
        //
        //                 int chipsnumber = PlayerManager.Shared.Player.Other.count.total_chips_game_count;
        //                 int chipslimit = BRJsonUtil.GetBRInt(open_conditions, "chips_game", -1);
        //                 if (chipslimit > 0 && chipslimit > chipsnumber)
        //                 {
        //                     return true;
        //                 }
        //             }
        //         }
        //         else
        //         {
        //             if (PlayerManager.Shared.Player.Other.deblock_cartoon != null &&
        //                 (sub_title == RoomType.diamonds || sub_title == RoomType.money))
        //             {
        //                 return !PlayerManager.Shared.Player.Other.deblock_cartoon.Contains(id);
        //             }
        //
        //             return false;
        //         }
        //     }
        //
        //     return false;
        // }
        //
        // // 是否是钻石场
        // public bool IsChips()
        // {
        //     if (BRJsonUtil.GetBRInt(other_configs, "only_organic") == 1)
        //     {
        //         return true;
        //     }
        //
        //     return sub_title == RoomType.diamonds;
        // }
        //
        // // 是否是活动场
        // public bool IsEvent()
        // {
        //     if (BRJsonUtil.GetBRInt(other_configs, "only_organic") == 1)
        //     {
        //         return false;
        //     }
        //
        //     return sub_title == RoomType.cost || sub_title == RoomType.active_charge || sub_title == RoomType.ads ||
        //            sub_title == RoomType.diamond_bonus;
        // }
        //
        // // 是否有下挂
        // public bool IsBottom()
        // {
        //     if (BRJsonUtil.GetBRInt(other_configs, "only_organic") == 1)
        //     {
        //         return false;
        //     }
        //
        //     return sub_title == RoomType.cost || sub_title == RoomType.active_charge ||
        //            sub_title == RoomType.diamond_bonus;
        // }
        //
        // // 是否有计时
        // public bool IsTimes()
        // {
        //     if (BRJsonUtil.GetBRInt(other_configs, "only_organic") == 1)
        //     {
        //         return RoomManager.Shared.GetBRRoomStatus(this).status == RoomStatusType.Limited;
        //     }
        //
        //     return sub_title == RoomType.cost || sub_title == RoomType.active_charge ||
        //            sub_title == RoomType.diamond_bonus ||
        //            RoomManager.Shared.GetBRRoomStatus(this).status == RoomStatusType.Limited;
        // }
    }

    public class Charge_configsItem
    {
        public int id;
        public float bonusValue = 0;
        public string amount;
        public JsonData extra_items;
        public int gift_begin_time;
        public int gift_end_time;
        public JsonData gift_items;
        public int first_charge_items;
        public string position;
        public string discount_id; // 折扣的充值id
        public string discount_amount; // 折扣的充值金额
    }

    public class CreditCardDetails
    {
        public string card_number;
        public string email;
        public string cvc;
        public string month;
        public string year;
        public string first_name;
        public string last_name;
    }
    
    public class ActivitiesItem
    {
        public int id;
        public string config_id;
        public string begin_preheat;
        public int begin_time;
        public int end_time;
        public int claim_end_time;
        public JsonData open_conditions;
        public JsonData in_items;
        public JsonData out_items;
        public string connect;
    }
}