using System;
using System.Collections.Generic;
using System.Linq;
using AppSettings;
using DataAccess.Utils;
using DataAccess.Utils.JsonParse;
using DataAccess.Utils.Static;
using Newtonsoft.Json;

namespace DataAccess.Model
{
    public class Match
    {
        public int end_time;

        public int match_time;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int game_score;

        /// <summary>
        /// game_expire 没有在规定时间内完成游戏， match_expire 匹配超时。
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string sub_status;

        public string class_name;

        public int user_id;

        public string match_id;

        public int status;

        public long table_id;

        public int room_id;

        public string nickname;

        public int head_index;

        public string head_url;
    }

    public class MatchHistory
    {
        /// <summary>
        /// 作为属性， 说不定需要保存在本地
        /// </summary>
        public int id { get; set; }

        public int user_id { get; set; }
        public int room_id { get; set; }

        public string room_name { get; set; }
        public string match_id { get; set; }

        public long table_id { get; set; }
        public int status { get; set; }

        /// <summary>
        /// game_expire 没有在规定时间内完成游戏， match_expire 匹配超时。
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string sub_status;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int game_score { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int win_result { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string nickname { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int head_index { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string head_url { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int wizard { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int want_rank { get; set; }

        /// <summary>
        /// 和room 的type 保持类型一致
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string room_type { get; set; }

        public bool IsLuckyRoom => room_type == "1";

        public bool IsFromFreeBonus => room_name == "Free Bonus";

        public bool IsAboutDollar
        {
            get
            {
                if (room is { IsAboutMoney: true })
                {
                    return true;
                }

                return RewardIsDollar;
            }
        }

        
        private bool RewardIsDollar => RewardsList != null && RewardsList.Exists(item => item.Count > 0 &&
            item.id is Const.Bonus or Const.Cash);
        
        private int beginTime;

        private Room room => Root.Instance.OriginRoomList.Find(room1 => room1.id == room_id);

        /// <summary>
        /// 应该是局内游戏开始的时间
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int begin_time
        {
            get
            {
                if (beginTime > 0)
                {
                    return beginTime;
                }
                else
                {
                    return end_time - 10;
                }
            }
            set => beginTime = value;
        }

        [JsonConverter(typeof(StringToDic))] [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<int, float> rewards;

        public List<Item> RewardsList
        {
            get
            {
                if (rewards == null || !rewards.Any())
                {
                    return null;
                }

                List<Item> result = new List<Item>();
                foreach (var kv in rewards)
                {
                    if (ItemSettings.Get(kv.Key) != null)
                    {
                        result.Add(new Item(kv.Key, kv.Value));
                    }
                }

                return result;
            }
        }


        public int end_time { get; set; }

        public DateTime created_at { get; set; }

        public DateTime updated_at { get; set; }

        public bool PullFlag;

        /// <summary>
        /// 根据 win_result
        /// </summary>
        public bool IsTopOne
        {
            get
            {
                if (rewards == null)
                {
                    return false;
                }

                if (IsWait)
                {
                    return false;
                }

                if (IsTableMatchExpire)
                {
                    return true;
                }

                return win_result == 1;
            }
        }

        public bool IsTableMatchExpire => sub_status == "table_match_expire";

        public bool HavaReward => rewards != null;

        public bool IsClaimed => status == (int)Status.Claimed;

        /// <summary>
        /// 未领取奖励 视为未完成 失败, 视为已完成
        /// </summary>
        public bool NotFinish => !IsClaimed && (HavaReward || IsWait);

        /// <summary>
        /// 等待对手出分或超时  
        /// </summary>
        public bool IsWait => status <= (int)Status.Game_End;

        public bool IsGameEnd => status >= (int)Status.Game_End;

        public bool CanClaim => HavaReward && status == (int)Status.CanClime;

        public bool CanClaimWhenWithDraw
        {
            get
            {
                //没有充值不能领
                if (IsLuckyRoom)
                {
                    if (Root.Instance.LuckyGuyInfo == null || !Root.Instance.LuckyGuyInfo.IsChargeSuccess)
                    {
                        return false;
                    }
                }
                return HavaReward && status == (int)Status.CanClime;
            }
        }
    }
    
    public class MatchTable
    {
        public int id;
        public long table_id;
        public string type;
        public int seat;
        
        public int status;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string sub_status;

        public bool IsFinish => status >= 3;
    }
    
}