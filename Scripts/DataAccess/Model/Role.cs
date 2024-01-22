using System.Collections.Generic;
using Core.Extensions;
using Core.Models;
using DataAccess.Utils.JsonParse;
using DataAccess.Utils.Static;
using Newtonsoft.Json;
using UnityEngine.UI;

namespace DataAccess.Model
{
    public class Role
    {
        public int date;

        public string country;

        public int country_code;

        public int user_id;

        /// <summary>
        /// white = 1表示在白名单内， 可以绕过禁登
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int white;

        /// <summary>
        /// 玩家名称
        /// </summary>
        public string nickname;

        public int head_index;

        public string head_url;

        /// <summary>
        /// 新手引导的进度
        /// </summary>
        public int match_first_game_guide;
        
        /// <summary>
        /// 选择的巫师
        /// </summary>
        public int wizard;

        /// <summary>
        /// 是否被冻结（block == 2) ， 0 正常 ， 1 封停
        /// </summary>
        public int block;

        public bool IsFreeze => block == 2 && white == 0;
        
        public int VipLevel => Root.Instance.ChargeOpen ? Root.Instance.OnlineRewardInfo.vip_level : 0;

        /// <summary>
        /// 玩家总共充值的金额
        /// </summary>
        public double TotalCharge;

        public int ImageId => GlobalEnum.DefaultImageId;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string first_ip;

        /// <summary>
        /// 玩家的道具
        /// </summary>
        [JsonConverter(typeof(NameToItem))] [JsonProperty(PropertyName = "balance")]
        public List<Item> Items;

        private List<Match> matchList;

        public List<Match> MatchList
        {
            get { return matchList ??= new List<Match>(); }
            set { matchList = value; }
        }

        public Item FindItem(int id)
        {
            return Items.Find(item => item.id == id);
        }

        public float GetItemCount(int id)
        {
            var item = FindItem(id);
            return item?.Count ?? 0;
        }

        /// <summary>
        /// 玩家总共的”美金“ Cash + Bonus
        /// </summary>
        /// <returns></returns>
        public float GetDollars()
        {
            float result = 0;
            foreach (var item in Items)
            {
                if (item.id is Const.Bonus or Const.Cash)
                {
                    result += item.Count;
                }
            }

            return result;
        }

        public float GetBonus()
        {
            float result = 0;
            foreach (var item in Items)
            {
                if (item.id is Const.Bonus)
                {
                    result += item.Count;
                }
            }

            return result;
        }

        /// <summary>
        /// 还没有绑定登录邮箱
        /// </summary>
        public bool NotBindLoginEmail => Root.Instance.GetYZEmialStatus() == 0 && !HaveSavedMail;
        
        public float GetCash()
        {
            float result = 0;
            foreach (var item in Items)
            {
                if (item.id is Const.Cash)
                {
                    result += item.Count;
                }
            }

            return result;
        }

        // 单例暂存的值，已经绑定邮箱
        public bool HaveSavedMail = false;

        public void LoadIcon(Image image)
        {
            if (image == null)
            {
                return;
            }
            
            if (!string.IsNullOrEmpty(head_url))
            {
                image.ServerUrl(head_url, user_id == Root.Instance.UserId);
            }
            else
            {
                var sprite = Root.Instance.LoadPlayerIconByIndex(head_index);
                if (sprite != null)
                {
                    image.sprite = sprite;
                }
            }
        }

        public WithdrawInfo withdrawInfo;
        
        // 活动
        public List<ActivitiesItem> activities;
        // 幸运卡
        public LuckyCardInfo luckyCardInfo;
        // 一条龙
        public DragonInfo dragonInfo;
        // 特殊礼包
        public SpecialGiftInfo specialGiftInfo;

        public SpecialOfferInfo SpecialOfferInfo;
        
        public AdditionalGiftInfo AdditionalGiftInfo;

        public bool AdditionalGiftNeedShow = false;
    }
}