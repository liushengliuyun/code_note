using System;
using System.Collections.Generic;
using System.Linq;
using AppSettings;
using Core.Third.I18N;
using DataAccess.Utils.JsonParse;
using DataAccess.Utils.Static;
using Newtonsoft.Json;
using UnityEngine;

namespace DataAccess.Model
{
    public class Room
    {
        public int id;
        public string name;
        public string type;
        public int order;

        public int seat;
        public string title;
        public string sub_title;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public float museum_point;
        
        [JsonConverter(typeof(StringToDic))] public Dictionary<int, float> in_items;

        [JsonConverter(typeof(StringToListDic))]
        // public Dictionary<int, Dictionary<int, float>> out_items;
        public Dictionary<int, Dictionary<int, float>> out_items;

        public int status;
        [JsonProperty(PropertyName = "lock")] public int islock;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string open_conditions;

        public int min_version;
        public int max_version;
        public DateTime created_at;
        public DateTime updated_at;

        public bool room_show;

        public bool IsMulti;
        
        /// <summary>
        /// 魔法球点数
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public float wizard_treasure_point;

        public RoomSetting config => RoomSettings.Get(id);

        public RoomStyle Style
        {
            get
            {
                if (IsFreeBonusRoom)
                {
                    return RoomStyle.FreeBonus;
                }
                
                // if (IsADRoom)
                // {
                //     return RoomStyle.VallyStyle;
                // }
                
                switch (name)
                {
                    case "Magic Castle":
                        return RoomStyle.CastleStyle;
                    case "Wizard Arena":
                        return RoomStyle.Battleground;
                    case "Free Bonus":
                        return RoomStyle.FreeBonus;
                    case "Magic Storm":
                        return RoomStyle.Windstorm;
                    case "Treasure Plain":
                        return RoomStyle.TreasurePlant;
                    case "Arcane Spring":
                        return RoomStyle.SpringWater;
                    case "Thorn Forest":
                    case "Elves Tavern":
                        return RoomStyle.VallyStyle;
                }
                
                return config != null ? (RoomStyle)config.Style : RoomStyle.CastleStyle;
            }
        }

        private int ShowIndex
        {
            get
            {
                var index = Root.Instance.ShowRoomList.FindIndex(room => room.id == id);
                return Math.Max(0, index);
            }
        }

        public string Icon
        {
            get
            {
                // 1, 2 是钻石
                var top_reward = GetRankReward(1);

                var icon_index = 1;
                if (top_reward is { Count: > 0 })
                {
                    var item = top_reward[0];
                    if (item.id == Const.Chips)
                    {
                        if (PrizePool <= 100)
                        {
                            icon_index = 1;
                        }
                        else
                        {
                            icon_index = 2;
                        }
                    }
                    else if (item.id is Const.Bonus or Const.Cash)
                    {
                        switch (PrizePool)
                        {
                            case <3:
                                icon_index = 3;
                                break;
                            case <=6:
                                icon_index = 4;
                                break;
                            case <=15:
                                icon_index = 5;
                                break;
                            case <=30:
                                icon_index = 6;
                                break;
                            case <=40:
                                icon_index = 7;
                                break;
                            case <=90:
                                icon_index = 8;
                                break;
                            case <=140:
                                icon_index = 9;
                                break;
                            case <=250:
                                icon_index = 10;
                                break;
                            default:
                                icon_index = 10;
                                break;
                        }
                    }
                }

                if (icon_index >= 10)
                {
                    return $"uimain/room_icon_{icon_index}";
                }
                
                return $"uimain/room_icon_0{icon_index}";
            }
        }

        public string Bg => $"uimain/lab{ShowIndex % 2}";
        
        /*public Color RewardDescColor
        {
            get
            {
                if (seat == 5)
                {
                    return new Color(26f / 255f, 110 / 255f, 182f / 255f);
                }
                return new Color(255 / 255f, 255 / 255f, 255 / 255f);
            }
        }*/

        public Color DescTextColor
        {
            get
            {
                if (seat < 5)
                {
                    return Color.white;
                }
                return new Color(167 / 255f, 250 / 255f, 255 / 255f);
            }
        }
        
        /// <summary>
        /// 房间总共的奖金
        /// </summary>
        public float PrizePool
        {
            get
            {
                float result = 0;
                var topReward = GetRankReward(1);

                if (topReward == null || !topReward.Any())
                {
                    return result;
                }

                //第一名奖励的类型
                var topType = topReward[0].id;
                
                foreach (var dic in out_items)
                {
                    //int float
                    foreach (var kv in dic.Value)
                    {
                        if (kv.Key == topType)
                        {
                            result += kv.Value;
                        }
                    }
                }

                return result;
            }
        }
        
        public string TitleBG => $"uimain/room_name_bg_{ShowIndex % 2}";

        public string RoomEntryDesc
        {
            get
            {
                if (IsADRoom)
                {
                    return I18N.Get("key_room_entry_8");
                }

                if (name == "Thorn Forest")
                {
                    return I18N.Get("key_room_entry_4");
                }

                switch (Style)
                {
                    case RoomStyle.CastleStyle:
                        return I18N.Get("key_room_entry_1");
                        break;
                    case RoomStyle.VallyStyle:
                        return I18N.Get("key_room_entry_3");
                        break;
                    case RoomStyle.TreasurePlant:
                        return I18N.Get("key_room_entry_2");
                    case RoomStyle.FreeBonus:
                        return I18N.Get("key_room_entry_8");
                        break;
                    case RoomStyle.SpringWater:
                        return I18N.Get("key_room_entry_5");
                        break;
                    case RoomStyle.Windstorm:
                        return I18N.Get("key_room_entry_7");
                        break;
                    case RoomStyle.Battleground:
                        return I18N.Get("key_room_entry_6");
                    case RoomStyle.ThornForest:
                        return I18N.Get("key_room_entry_4");
                }

                return "";
            }
        }

        public bool IsLuckyRoom => type == "1";
        
        public bool IsFreeBonusRoom => type == "0";

        /// <summary>
        /// 是否是广告房间
        /// </summary>
        public bool IsADRoom => type == "5";

        public bool IsNormal => !(IsADRoom || IsFreeBonusRoom);

        public bool IsFriendsDuelRoom => type == "15";

        public bool IsDiamondRoom => type == "8";

        public bool IsFree => in_items == null || !in_items.Any();

        //获取根据排名的奖励
        public List<Item> GetRankReward(int rank)
        {
            if (out_items == null)
            {
                return null;
            }

            List<Item> result = null;
            out_items.TryGetValue(rank, out var itemDic);
            if (itemDic != null)
            {
                result = new List<Item>();
                foreach (var kv in itemDic)
                {
                    result.Add(new Item(kv.Key, kv.Value));
                }
            }

            return result;
        }

        public Item GetInItem()
        {
            if (in_items == null)
            {
                return null;
            }

            Item result = null;

            foreach (var kv in in_items)
            {
                var row = ItemSettings.Get(kv.Key);

                if (row == null) continue;

                result = new Item(kv.Key, kv.Value);
                break;
            }

            return result;
        }

        /// <summary>
        /// 是否和美金局相关
        /// </summary>
        public bool IsAboutMoney => TicketIsDollar() || TopRewardIsDollar();

        public bool IsDollarRoom => TicketIsDollar() && TopRewardIsDollar();
        
        private bool TopRewardIsDollar()
        {
            var topRewards = GetRankReward(1);
            if (topRewards != null)
            {
                foreach (var item in topRewards)
                {
                    if (item.id is Const.Bonus or Const.Cash)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool TicketIsDollar()
        {
            if (in_items != null)
            {
                foreach (var kv in in_items)
                {
                    if (kv.Key is Const.Bonus or Const.Cash)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}