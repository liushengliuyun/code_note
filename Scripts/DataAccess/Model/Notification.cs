using System;
using System.Collections.Generic;
using CatLib.EventDispatcher;
using Core.Extensions;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UI;
using UI.Activity;
using Utils;

namespace DataAccess.Model
{
    public class Notification
    {
        public string name;

        public string value;

        public void Command()
        {
            try
            {
                switch (name)
                {
                    case "user_block":
                        var o1 = JsonConvert.DeserializeObject(value);
                        if (o1 is JObject jObject1)
                        {
                            var blockValue = YZJsonUtil.DeserializeJObject<int>("block", jObject1);
                            if (blockValue > 0)
                            {
                                Root.Instance.Role.block = blockValue;
                            }
                        }
                        Root.Instance.Role.block = JsonConvert.DeserializeObject<int>(value);
                        break;
                    case "wizard_point_update":
                        Root.Instance.MagicBallInfo = JsonConvert.DeserializeObject<MagicBallInfo>(value);
                        break;
                    //总共的充值数量
                    case "charge_total_update":
                        Root.Instance.Role.TotalCharge = Math.Round(JsonConvert.DeserializeObject<double>(value), 2);
                        break;
                    //转盘门票更新
                    case "free_wheel_ticket_update":
                        Root.Instance.WheelFreeTicket = JsonConvert.DeserializeObject<int>(value);
                        EventDispatcher.Root.Raise(GlobalEvent.SYNC_FORTUNE_WHEEL_INFO);
                        break;
                    case "pay_wheel_ticket_update":
                        Root.Instance.FortuneWheelInfo.wheel_pay_chance =
                            JsonConvert.DeserializeObject<Dictionary<int, int>>(value);
                        EventDispatcher.Root.Raise(GlobalEvent.SYNC_FORTUNE_WHEEL_INFO);
                        break;
                    //任务更新
                    case "castle_task_update":
                        Root.Instance.CurTaskInfo.task.progress = JsonConvert.DeserializeObject<int>(value);
                        EventDispatcher.Root.Raise(GlobalEvent.Sync_TaskInfo);
                        break;
                    case "email_verify":
                        YZLog.LogColor(name + " :" + value, "red");
                        break;
                    //商店礼包刷新
                    case "shop_charge_gear_update":
                        var o = JsonConvert.DeserializeObject(value);
                        if (o is JObject jObject)
                        {
                            var shopConfig = YZJsonUtil.DeserializeJObject<List<ShopInfo>>("shop", jObject);
                            if (shopConfig != null)
                            {
                                Root.Instance.ShopConfig = shopConfig;
                            }
                        }

                        break;
                    //房间刷新
                    case "room_list_update":
                        var updateValue = JsonConvert.DeserializeObject<int>(value);
                        if (updateValue == 1)
                        {
                            MediatorRequest.Instance.GetRoomList();   
                        }
                        break;
                    case "cash_enter_fee_update":
                        break;
                    case "starter_pack_chance_update":
                        Root.Instance.StarterPackInfo = JsonConvert.DeserializeObject<StarterPackInfo>(value);
                        YZLog.LogColor(name + " :" + value, "red");
                        break;
                    //存钱罐 已存bonus 变化  
                    case "piggy_bank_bonus_update":
                        var newValue = JsonConvert.DeserializeObject<int>(value);
                        Root.Instance.PiggyBankInfo.AddPiggyBonus = newValue - Root.Instance.PiggyBankInfo.piggy_bonus ;
                        Root.Instance.PiggyBankInfo.piggy_bonus = newValue;
                        if (Root.Instance.PiggyBankInfo.AddPiggyBonus > 0)
                        {
                            EventDispatcher.Root.Raise(GlobalEvent.ADD_PIGGY_BONUS);
                        }
                 
                        break;
                    //存钱罐已满提示
                    case "piggy_bank_bonus_full":
                        Root.Instance.PiggyBankInfo.piggy_bonus = JsonConvert.DeserializeObject<int>(value);
                        break;
                    //存钱罐是否展示消息：0-不展示，1-展示
                    case "piggy_chance_update":
                        Root.Instance.PiggyBankInfo.piggy_chance = JsonConvert.DeserializeObject<int>(value);
                        EventDispatcher.Root.Raise(GlobalEvent.SYNC_PIGGY_BANK);
                        break;
                    case "lucky_card_chance_update":
                        Root.Instance.Role.luckyCardInfo.lucky_card_chance = JsonConvert.DeserializeObject<int>(value);
                        EventDispatcher.Root.Raise(GlobalEvent.Sync_LuckyCard);
                        break;
                    case "one_stop_show":
                        Root.Instance.Role.dragonInfo = JsonConvert.DeserializeObject<DragonInfo>(value);
                        if (Root.Instance.Role.dragonInfo != null)
                            Root.Instance.Role.dragonInfo.end_timestamp =
                                Root.Instance.Role.dragonInfo.one_stop_begin_time + 24 * 3600;
                        EventDispatcher.Root.Raise(GlobalEvent.Sync_Dragon);
                        break;
                    case "special_today_card_chance_update":
                        Root.Instance.Role.specialGiftInfo.special_gift_today_chance =
                            JsonConvert.DeserializeObject<int>(value);
                        EventDispatcher.Root.Raise(GlobalEvent.Sync_Special_Gift);
                        break;
                    case "room_charge_chance_update":
                    case "room_charge_info_update":
                        YZLog.LogColor(name + " :" + value, "red");
                        var data = JsonConvert.DeserializeObject<RoomChargeInfo>(value);
                        if (data != null && Root.Instance.RoomChargeInfo != null)
                        {
                            Root.Instance.RoomChargeInfo.charge_B_chance = data.charge_B_chance;
                            Root.Instance.RoomChargeInfo.charge_chance = data.charge_chance;
                            Root.Instance.RoomChargeInfo.room_charge_A_begin_time = data.room_charge_A_begin_time;
                            Root.Instance.RoomChargeInfo.room_charge_B_begin_time = data.room_charge_B_begin_time;
                            EventDispatcher.Root.Raise(GlobalEvent.Sync_RoomChargeInfo);
                        }

                        break;
                    case "ad_room_chance_update":
                        Root.Instance.RoomAdInfo = JsonConvert.DeserializeObject<RoomAdInfo>(value);
                        break;
                    case "special_card_chance_update":
                        Root.Instance.Role.specialGiftInfo = JsonConvert.DeserializeObject<SpecialGiftInfo>(value);
                        if (Root.Instance.Role.specialGiftInfo != null)
                        {
                            var spLessTime = TimeUtils.Instance.EndDayTimeStamp -
                                             Root.Instance.Role.specialGiftInfo.special_gift_create_time;
                            if (spLessTime / 3600 <= 24 && spLessTime > 0)
                                Root.Instance.Role.specialGiftInfo.special_gift_end_time =
                                    TimeUtils.Instance.EndDayTimeStamp;
                        }

                        EventDispatcher.Root.Raise(GlobalEvent.Sync_Special_Gift);
                        break;
                    case "free_bonus_chance":
                    case "free_bonus_level_change":
                        YZLog.LogColor(name + " :" + value, "red");
                        Root.Instance.FreeBonusInfo = JsonConvert.DeserializeObject<FreeBonusInfo>(value);
                        break;
                    //提现失败
                    case "cash_result_failure":
                        MediatorRequest.Instance.SyncItem(value, false);
                        MediatorRequest.Instance.RefreshWithDraw();
                        break;
                    case "museum_point_update":
                        Root.Instance.MuseumInfo = JsonConvert.DeserializeObject<MuseumInfo>(value);
                        YZLog.LogColor(name + " :" + value, "red");
                        break;
                    case "infinite_grail_update":
                        Root.Instance.MonthCardInfo = JsonConvert.DeserializeObject<MonthCardInfo>(value);
                        YZLog.LogColor(name + " :" + value, "red");
                        break;
                    case "daily_task_update":
                        YZDataUtil.SetYZInt(YZConstUtil.YZDailyMissionHaveReward, 1);
                        break;
                    case "lucky_you_info_update":
                        Root.Instance.LuckyGuyInfo = JsonConvert.DeserializeObject<LuckyGuyInfo>(value);
                        break;
                    case "infinite_week_12_update":
                        Root.Instance.WeekInfo = JsonConvert.DeserializeObject<InfiniteWeekInfo>(value);
                        EventDispatcher.Root.Raise(GlobalEvent.SYNC_WEEK_CARD_INFO);
                        break;
                    case "special_offer_show_time_update":
                        Root.Instance.Role.SpecialOfferInfo = JsonConvert.DeserializeObject<SpecialOfferInfo>(value);
                        EventDispatcher.Root.Raise(GlobalEvent.Sync_Special_Offer);
                        break;
                    case "add_charge_amount_update":
                        Root.Instance.Role.AdditionalGiftInfo =
                            JsonConvert.DeserializeObject<AdditionalGiftInfo>(value);
                        if (Root.Instance.Role.AdditionalGiftInfo.charge_amount > 0)
                            Root.Instance.Role.AdditionalGiftNeedShow = true;
                        break;
                    case "daily_first_win_game":
                        YZDataUtil.SetYZString(YZConstUtil.YZDailyFirstWinMatchId, 
                            JsonConvert.DeserializeObject<string>(value));
                        break;
                    case "cash_back_money":
                        if (JsonConvert.DeserializeObject(value) is JObject cashJObject)
                        {
                            var dic = YZJsonUtil.DeserializeJObject<Dictionary<int, float>>("add", cashJObject);
                            dic.TryGetValue(1, out var cash);
                            UserInterfaceSystem.That.ShowQueue<UIConfirm>(new UIConfirmData()
                            {
                                Type = UIConfirmData.UIConfirmType.OneBtn,
                                HideCloseBtn = true,
                                desc = I18N.Get("key_cash_back", cash),
                                title = I18N.Get("key_refund"),
                                confirmTitle = I18N.Get("key_ok"),
                                confirmCall = () =>
                                {
                                    MediatorRequest.Instance.RefreshItem();
                                }
                            });
                        }
                        break;
                    default:
                        YZLog.LogColor(name + " :" + value, "red");
                        break;
                }
            }
            catch (Exception e)
            {
                YZLog.LogColor(name + " :" + value + " error : "+ e, "red");
            }
        }
    }
}