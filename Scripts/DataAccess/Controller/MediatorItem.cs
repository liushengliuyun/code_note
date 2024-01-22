using System.Collections.Generic;
using AppSettings;
using CatLib.EventDispatcher;
using Core.Extensions;
using Core.Services.NetService.API.Facade;
using Core.Services.ResourceService.API.Facade;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Third.I18N;
using DataAccess.Model;
using DataAccess.Utils.Static;
using UI;
using UI.Activity;
using UI.Effect;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace DataAccess.Controller
{
    public class MediatorItem : global::Utils.Runtime.Singleton<MediatorItem>
    {
        private static Font CashFont;
        private static Font GemFont;
        private static Font CoinFont;
        
        public void ResNotEnoughGoTo(int itemType, Room room = null, bool isFromDuel = false)
        {
            EventDispatcher.Root.Raise(GlobalEvent.Sync_Item);
            switch (itemType)
            {
                case Const.Bonus:
                case Const.Cash:
                    if (Root.Instance.IsNaturalFlow)
                    {
                        return;
                    }
                    // 1.若当前有Starter Pack三选一活动，则弹出该活动
                    // 2.若当前有新手存钱罐活动，则弹出该活动
                    // 3.若满足房间直充活动触发条件，则弹出该活动
                    // 4.帮玩家跳转商店

                    if (MediatorActivity.Instance.IsActivityBegin(ActivityType.StartPacker))
                    {
                        UserInterfaceSystem.That.TopInQueue<UIStartPack>();
                    }
                    else if (MediatorActivity.Instance.IsActivityBegin(ActivityType.PiggyBank))
                    {
                        UserInterfaceSystem.That.TopInQueue<UIPiggyBank>();
                    }
                    else if (MediatorActivity.Instance.IsActivityBegin(ActivityType.BestOffer))
                    {
                        UserInterfaceSystem.That.TopInQueue<UIBestOffer>(new GameData()
                        {
                            ["ActivityEnterType"] = ActivityEnterType.Trigger
                        });
                    }
                    else if (MediatorActivity.Instance.IsActivityBegin(ActivityType.JustForYou))
                    {
                        UserInterfaceSystem.That.TopInQueue<UIJustForYou>(new GameData()
                        {
                            ["ActivityEnterType"] = ActivityEnterType.Trigger
                        });
                    }
                    else if (MediatorActivity.Instance.IsActivityOpen(ActivityType.JustForYou) && room != null)
                    {
                        NetSystem.That.SetFailCallBack(s => { MediatorRequest.Instance.GetRoomChargeInfo(room.id); });
                        if (!isFromDuel)
                            MediatorRequest.Instance.MatchBegin(room);
                        else
                            MediatorRequest.Instance.DuelMatchBegin(room.id, "ERROR");
                    }
                    else
                    {
                        EventDispatcher.Root.Raise(GlobalEvent.GO_TO_MAIN_STORE);
                    }

                    break;
                case Const.Chips:
                    UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_http_code_1103"));
                    break;
                case Const.Coin:
                    UserInterfaceSystem.That.ShowUI<UITip>( I18N.Get("RES_NOT_ENOUGH"));
                    break;
            }
        }

        public bool CheckResEnough(int id, int count)
        {
            if (id is Const.Bonus or Const.Cash)
            {
                return Root.Instance.Role.GetDollars() >= count;
            }
            else
            {
                return GetItemCount(id) >= count;
            }
        }

        public float GetItemCount(int id)
        {
            var item = GetItem(id);
            return item?.Count ?? 0;
        }

        public Item GetItem(int id)
        {
            if (Root.Instance.Role.Items == null)
            {
                return null;
            }

            return Root.Instance.Role.Items.Find(item => item.id == id);
        }

        public string GetItemName(int id)
        {
            return I18N.Get($"item_name_{id}");
        }

        public Sprite GetItemSprite(int id)
        {
            var item = ItemSettings.Get(id);
            if (item == null || item.Icon.IsNullOrEmpty())
            {
                return null;
            }

            return ResourceSystem.That.LoadAssetSync<Sprite>(ItemSettings.Get(id).Icon);
        }

        public void SetItemText(string key, Text text)
        {
            text.TryGetComponent<Gradient2>(out var gradient);
            text.TryGetComponent<Outline8>(out var outline8);
            text.TryGetComponent<Shadow>(out var shadow);
            GradientColorKey[] colorKeys = null;
            if (gradient != null)
            {
                gradient.enabled = true;
                colorKeys = gradient.EffectGradient.colorKeys;
            }

            if (outline8 != null)
            {
                outline8.enabled = true;
            }

            if (shadow != null)
            {
                shadow.enabled = true;
            }

            switch (key)
            {
                case "bigCardMask":
                    if (colorKeys != null)
                    {
                        colorKeys[0].color = new Color(126 / 255.0f, 196 / 255.0f, 255 / 255.0f);
                        colorKeys[1].color = new Color(201 / 255.0f, 198 / 255.0f, 199 / 255.0f);
                    }

                    if (shadow != null)
                    {
                        shadow.effectColor = new Color(28 / 255.0f, 45 / 255.0f, 89 / 255.0f);
                    }

                    text.color = Color.white;
                    break;
                case "smallCard":

                    if (shadow != null)
                    {
                        shadow.enabled = false;
                    }

                    if (gradient != null)
                    {
                        gradient.enabled = false;
                    }

                    text.color = new Color(145 / 255.0f, 76 / 255.0f, 18 / 255.0f);
                    break;
                case "bigCardShow":
                    if (colorKeys != null)
                    {
                        colorKeys[0].color = new Color(241 / 255.0f, 233 / 255.0f, 178 / 255.0f);
                        colorKeys[1].color = new Color(251 / 255.0f, 254 / 255.0f, 223 / 255.0f);
                    }

                    if (shadow != null)
                    {
                        shadow.effectColor = new Color(103 / 255.0f, 22 / 255.0f, 40 / 255.0f);
                    }

                    text.color = Color.white;
                    break;
            }

            if (colorKeys != null)
            {
                gradient.EffectGradient.SetKeys(colorKeys, gradient.EffectGradient.alphaKeys);
            }
        }

        public void SetItemText(int id, Text text)
        {
            text.TryGetComponent<Gradient2>(out var gradient);
            text.TryGetComponent<Outline8>(out var outline8);
            GradientColorKey[] colorKeys = null;
            if (gradient != null)
            {
                colorKeys = gradient.EffectGradient.colorKeys;
            }

            text.color = Color.white;
            switch (id)
            {
                case Const.Bonus:
                case Const.Cash:
                    if (colorKeys != null)
                    {
                        colorKeys[0].color = new Color(72 / 255.0f, 255 / 255.0f, 7 / 255.0f);
                        colorKeys[1].color = new Color(192 / 255.0f, 255 / 255.0f, 63 / 255.0f);
                    }

                    if (outline8 != null)
                    {
                        outline8.effectColor = new Color(35 / 255.0f, 114 / 255.0f, 0 / 255.0f);
                    }
                    CashFont ??= Resources.Load<Font>("Fonts/bmfont_cash01");
                    text.font = CashFont;
                    
                    break;
                case Const.Chips:
                    if (colorKeys != null)
                    {
                        colorKeys[0].color = new Color(252 / 255.0f, 109 / 255.0f, 251 / 255.0f);
                        colorKeys[1].color = new Color(255 / 255.0f, 211 / 255.0f, 254 / 255.0f);
                    }

                    if (outline8 != null)
                    {
                        outline8.effectColor = new Color(197 / 255.0f, 47 / 255.0f, 173 / 255.0f);
                    }

                    if (GemFont == null)
                    {
                        GemFont = Resources.Load<Font>("Fonts/bmfont_gem01");
                    }

                    text.font = GemFont;
                    break;
                case Const.Coin:
                    if (colorKeys != null)
                    {
                        colorKeys[0].color = new Color(255 / 255.0f, 167 / 255.0f, 26 / 255.0f);
                        colorKeys[1].color = new Color(255 / 255.0f, 255 / 255.0f, 61 / 255.0f);
                    }

                    if (outline8 != null)
                    {
                        outline8.effectColor = new Color(168 / 255.0f, 65 / 255.0f, 0 / 255.0f);
                    }
                    CoinFont ??= Resources.Load<Font>("Fonts/bmfont_coin01");
                    text.font = CoinFont;
                    break;
            }

            if (colorKeys != null)
            {
                gradient.EffectGradient.SetKeys(colorKeys, gradient.EffectGradient.alphaKeys);
            }
        }
        
        
        /// <summary>
        /// 下一次打开 uigetreward 将要额外显示的
        /// </summary>
        private List<Item> ExShowItems; 

        public void AddExShowItem(Item item)
        {
            ExShowItems ??= new List<Item>();
            if (item != null)
            {
                ExShowItems.Add(item);
            }
        }
        
        public List<Item> GetExShowItem()
        {
            var result = ExShowItems;
            return result;
        }

        public bool HaveExShowItem()
        {
            return ExShowItems is { Count: > 0 };
        }
        
        public void ClearExShowItem()
        {
            ExShowItems?.Clear();
        }
    }
}