using System;
using System.Collections.Generic;
using System.Linq;
using AndroidCShape;
using Core.Manager;
using Core.Models;
using Core.Server;
using Core.Services.PersistService.API.Facade;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using iOSCShape;
using LitJson;
using UIWidgets;
using UnityEngine;
using ThinkingAnalytics;
using AppsFlyerSDK;
using DataAccess.Controller;
using Newtonsoft.Json;

namespace Utils
{
    public static class FunnelEventID
    {
        public const string bruserinit = "user_init"; // 游戏初始化完成

        public const string brpermissiongps = "permission_gps";
        public const string brpermissionidfa = "permission_idfa";

        public const string brgpslocationerror = "gps_location_error";
        public const string brgpsreverseerror = "gps_reverse_error";

        public const string brmediasource = "af_data";
        public const string brspecialtipstrigger = "special_tips_trigger";

        public const string brblacklist = "black_list";

        public const string brwarning = "warning";
        
        /// <summary>
        /// 活动弹框
        /// </summary>
        public const string pop_up = "pop_up";
        
        
        public const string brfirstpay = "first_pay";

        //-------- 属性---------
        public const string brgpsstate = "gps_state";
        /// <summary>
        /// 推送权限
        /// </summary>
        public const string brpermissionpush = "permission_push";

        public const string brpermissionage = "permission_age"; //-- 充值选择是否18岁 未完成
        //-------- 属性---------

        //-------- 引导---------
        public const string brguide = "guide";
        //-------- 引导---------

        //-------- 推送---------
        public const string brpushpopup = "push_popup";
        public const string brpushgame = "push_game";
        public const string brpushsys = "push_sys";
        public const string brpushclose = "push_close";

        public const string brpushopenguide = "push_open_guide";
        //-------- 推送---------

        //-------- 按钮---------
        public const string brbuttonclick = "button_click";
        
        /// <summary>
        /// 活动按钮点击
        /// </summary>
        public const string button_click = "button_click";
        
        //-------- 按钮---------

        //-------- 用户---------
        public const string brusernewinfoenter = "user_new_info_enter";
        public const string brusernewsignin = "user_new_sign_in";
        public const string brusersave = "user_save";

        public const string bruserenterhall = "user_enter_hall";
        //-------- 用户---------

        //-------- 房间---------
        public const string brroomplayclick = "room_play_click";
        //-------- 房间---------

        //-------- 匹配---------
        public const string brmatchfailedbegin = "match_failed_begin";
        public const string brmatchfailedend = "match_failed_end";

        public const string brmatchfailedgameend = "match_failed_game_end";
        //-------- 匹配---------

        //-------- 充值---------
        public const string brordergps = "order_gps";
        public const string brordergpsreject = "order_gps_reject";
        public const string brordergpssuccess = "order_gps_success";
        public const string brorderstart = "order_start"; //-- 开始充值 未完成
        public const string brorder18 = "order_18"; //-- 是否大于18 未完成
        public const string brorderuiopen = "order_ui_open"; //-- 打开充值UI 未完成
        public const string brorderuichannel = "order_ui_channel"; //-- 确认订单弹窗 未完成
        public const string brorderclose = "order_close"; //-- 关闭订单窗口 未完成
        public const string brorderurl = "order_url"; //-- 网页跳转

        public const string brorderfail = "order_fail"; //-- 充值失败
        //-------- 充值---------

        //-------- 活动---------
        public const string brchargeactivityshow = "charge_activity_show";
        //-------- 活动---------

        //-------- 安全---------
        public const string brtriggerlimit = "trigger_limit";

        public const string brtriggerreturn = "trigger_return";
        //-------- 安全---------
    }

    public static class FunnelEventParam
    {
        public const string brroomid = "room_id";
        public const string bristure = "is_ture";

        public const string brstep = "step";
        public const string brtype = "type";
        public const string brisorganic = "is_organic";

        public const string brblacklistcountry = "country"; // 当前定位国家
        public const string brblacklistarea = "area"; // 当前定位州
        public const string brwhitelistnumber = "white_list_number"; // 当前白名单国家数量，用于判断是否服务器没有下发白名单

        public const string brlimittype = "limit_type";
        public const string brreasons = "reasons";
        public const string brresult = "result";

        public const string brmode = "mode";

        //-------- 按钮---------
        public const string brpos = "pos"; //-- 普通按钮点击
        public const string brdeposit = "deposit"; //-- 充值按钮点击

        public const string brpractice = "practice";
        public const string brrealcash = "realcash";
        public const string brrecord = "record";
        public const string brmenu = "menu";
        public const string brbingoparty = "bingoparty";
        public const string brcashback = "cashback";
        public const string brbingocup = "bingocup";
        public const string brsevendays = "sevendays";
        public const string brscratch = "scratch";
        public const string brwheel = "wheel";
        public const string bronlinereward = "onlinereward";
        public const string brminigame = "minigame";
        public const string brplayer = "player";
        public const string brshop = "shop";
        public const string brfreebonus = "freebonus";
        public const string brfreechips = "freechips";
        public const string brdailyreward = "dailyreward";
        public const string brchargepresent = "chargepresent";
        public const string brhighsocreguide = "highsocreguide";
        public const string brbingoslots = "bingoslots";
        public const string brgametips = "game_tips";
        public const string brbalancedetails = "balance_details";
        public const string brfishenter = "fish_enter";

        public const string brsettingsaveaccount = "settingsaveaccount";
        public const string brsettingswitchaccount = "settingswitchaccount";
        public const string brsettingchangeapassword = "settingchangeapassword";
        public const string brsettingsignin = "settingsignin";
        public const string brsettingwithdraw = "settingwithdraw";
        public const string brsettingsecuritycenter = "settingsecuritycenter";
        public const string brsettingnews = "settingnews";
        public const string brsettinglegal = "settinglegal";
        public const string brsettinghowtoplay = "settinghowtoplay";
        public const string brsettingcontactus = "settingcontactus";
        public const string brsettingmusic = "settingmusic";
        public const string brsettingsfx = "settingsfx";
        public const string brsettingvibration = "settingvibration";
        public const string brsettingbalance = "settingbalance";
        public const string brsettingvipcenter = "settingvipcenter";
        public const string brsettingbalancedetails = "settingbalancedetails";
        public const string brsettingfaq = "settingfaq";

        public const string brscratchend = "scratch-end";
        public const string brwheelendlucky = "wheel-end1";

        public const string brwheelendgold = "wheel-end2";
        //-------- 按钮---------

        //-------- 充值---------
        public const string brisfirstpay = "is_first_pay";
        public const string brpayenterui = "pay_enter_ui";
        public const string brpayentername = "pay_enter_name";
        public const string brerror = "error";
        public const string brpayway = "pay_way";
        public const string brurlstep = "url_step";

        public const string brduration = "duration";
        //-------- 充值---------

        //-------- 房间---------

        public const string brgpsresult = "gps_result";
        public const string brenoughmoney = "enough_money";

        public const string brgamebegin = "game_begin";
        //-------- 房间---------

        //-------- 推送---------
        public const string brtotalgame = "total_game";
        //-------- 推送---------

        //-------- 活动---------
        public const string bractivityid = "activity_ID";
        public const string brissuggest = "is_suggest";
        public const string brcountdown = "count_down";

        public const string brshowamount = "show";
        //-------- 活动---------
    }

    public static class FunnelAF
    {
        public const string braftestvalue = "af_testvalue";
        public const string bradrevenue = "ad_revenue";
        public const string brafpurchase = "af_purchase";
    }

    public static class FunnelOrderURL
    {
        public const string checkoutnow_1 = "checkoutnow";
        public const string signin_2 = "signin";
        public const string authflow_3 = "authflow";
        public const string webapps_4 = "webapps";
        public const string return_5 = "return";
    }

    public class YZFunnelUtil
    {
        public static void SendYZEvent(string name)
        {
#if RELEASE || LOG
            if (!YZDebug.IsWhiteListTestDevice())
            {
                TrackYZThinkingEvent(name, null);
                TrackYZAppsflyerEvent(name, null);
            }
#endif
        }

        public static void SendYZEvent(string name, Dictionary<string, object> properties)
        {
#if RELEASE || LOG
            if (!YZDebug.IsWhiteListTestDevice())
            {
                TrackYZThinkingEvent(name, properties);
                TrackYZAppsflyerEvent(name, properties);
            }
#endif
        }

        #region AF

        private static void TrackYZAppsflyerEvent(string name, Dictionary<string, object> value = null)
        {
#if UNITY_ANDROID || UNITY_IOS
            YZDebug.Log("AppsFlyer event" + name);
            if (value != null)
            {
                Dictionary<string, string> sd = new Dictionary<string, string>();
                foreach (KeyValuePair<string, object> keyValuePair in value)
                {
                    sd.Add(keyValuePair.Key, keyValuePair.Value == null ? "" : keyValuePair.Value.ToString());
                }
                AppsFlyer.sendEvent(name, sd);
            }
            else
            {
                AppsFlyer.sendEvent(name, null);
            }
#endif
//            string platform = YZGameUtil.GetPlatform();
//            if (platform == YZPlatform.iOS)
//            {
//                JsonData dict = YZDictToJsonData(value);
//                JsonData data = new JsonData();
//                data["name"] = name;
//                if (dict != null)
//                {
//                    data["dict"] = dict;
//                }
//                var json = data.ToJson();
//                YZDebug.Log("AppsFlyer event\n" + json);
//                iOSCShapeAppsflyerTool.Shared.IOSSendAppsFlyerEvent(json);
//            }
//            else if (platform == YZPlatform.Android)
//            {
//                // Android
//#if UNITY_ANDROID
//                YZDebug.Log("AppsFlyer event" + name);
//                YZAndroidAppsflyerPlugin.Shared.AndroidSendAppsFlyerEvent(name, value);
//#endif
//            }
        }

        public static void SendYZAppsflyerValue(string name, string value)
        {
#if UNITY_ANDROID || UNITY_IOS
            AppsFlyer.sendEvent(name, new Dictionary<string, string>()
            {
                {AFInAppEvents.REVENUE, value},
                {AFInAppEvents.CURRENCY, "USD"}
            });
#endif
//            string platform = YZGameUtil.GetPlatform();
//            if (platform == YZPlatform.iOS)
//            {
//                // iOS
//                JsonData data = new JsonData();
//                data["name"] = name;
//                data["value"] = value;
//                // TODO ios
//                iOSCShapeAppsflyerTool.Shared.IOSSendAppsFlyerValue(data.ToJson());
//            }
//            else if (platform == YZPlatform.Android)
//            {
//                // Android
//#if UNITY_ANDROID
//                YZDebug.Log("AppsFlyer pay value = " + value);
//                YZAndroidAppsflyerPlugin.Shared.AndroidSendAppsFlyerValue(name, value);
//#endif
//            }
        }

        #endregion

        #region 数数

        public static string DistinctID
        {
            get
            {
                if (YZGameUtil.GetIsiOS())
                {
                    return iOSCShapeThinkTool.Shared.IOSGetDistinctID();
                }
                else
                {
#if UNITY_ANDROID || UNITY_EDITOR
                    return YZAndroidThinkPlugin.Shared.AndroidGetDistinctID();
#else
                return "";
#endif
                }
            }
        }

        // 数数，校准时间
        public static void ThinkingYZCalibrateTime()
        {
            if (YZGameUtil.GetIsiOS())
            {
                long servertime = YZServerApi.Shared.GetYZServerTime();
                iOSCShapeThinkTool.Shared.IOSCalibrateTime(YZString.Concat(servertime * 1000));
            }
            else
            {
#if UNITY_ANDROID || UNITY_EDITOR
                long servertime = YZServerApi.Shared.GetYZServerTime();
                YZAndroidThinkPlugin.Shared.AndroidCalibrateTime(YZString.Concat(servertime * 1000));
#endif
            }
        }

        // 数数，上传打点
        public static void FlushYZ()
        {
            if (YZGameUtil.GetIsiOS())
            {
                iOSCShapeThinkTool.Shared.IOSFlushDatas();
            }
            else
            {
#if UNITY_ANDROID || UNITY_EDITOR
                YZAndroidThinkPlugin.Shared.AndroidFlushDatas();
#endif
            }
        }

        // 数数，登录
        public static void ThinkingYZLogin(string id)
        {
            if (YZGameUtil.GetIsiOS())
            {
                iOSCShapeThinkTool.Shared.IOSLoginThink(id);
                iOSCShapeThinkTool.Shared.IOSStartAutoEvent(YZString.Concat(YZServerApi.Shared.GetYZServerTime()));
            }
            else
            {
#if UNITY_ANDROID || UNITY_EDITOR
                YZAndroidThinkPlugin.Shared.AndroidLoginThink(id);
                YZAndroidThinkPlugin.Shared.AndroidStartAutoEvent(
                    YZString.Concat(YZServerApi.Shared.GetYZServerTime()));
#endif
            }
        }

//         // 数数，更新打点
//         public static void ThinkingYZUpdateGameRounds()
//         {
//             if (YZGameUtil.GetIsiOS())
//             {
//                 iOSCShapeThinkTool.Shared.IOSSetGameRounds(YZString.Concat(GlobalVarManager.Shared.YZLaunchGameCount));
//             }
//             else
//             {
// #if UNITY_ANDROID || UNITY_EDITOR
//                 YZAndroidThinkPlugin.Shared.AndroidSetGameRounds(GlobalVarManager.Shared.YZLaunchGameCount);
// #endif
//             }
//         }

        public static void UserYZSet(Dictionary<string, object> properties)
        {
            if (YZGameUtil.GetIsiOS())
            {
                if (YZDefineUtil.GetIsFunnel && properties != null)
                {
                    JsonData data = YZDictToJsonData(properties);
                    iOSCShapeThinkTool.Shared.IOSThinkUserSet(data.ToJson());
                }
            }
            else
            {
                if (YZDefineUtil.GetIsFunnel && properties != null)
                {
#if UNITY_ANDROID || UNITY_EDITOR
                    YZAndroidThinkPlugin.Shared.AndroidThinkUserSet(properties);
#endif
                }
            }
        }

        public static void UserYZSetOnce(Dictionary<string, object> properties)
        {
            if (YZGameUtil.GetIsiOS())
            {
                if (YZDefineUtil.GetIsFunnel && properties != null)
                {
                    JsonData data = YZDictToJsonData(properties);
                    iOSCShapeThinkTool.Shared.IOSThinkUserSetOnce(data.ToJson());
                }
            }
            else
            {
                if (YZDefineUtil.GetIsFunnel && properties != null)
                {
#if UNITY_ANDROID || UNITY_EDITOR
                    YZAndroidThinkPlugin.Shared.AndroidThinkUserSetOnce(properties);
#endif
                }
            }
        }

        private static void TrackYZThinkingEvent(string name, Dictionary<string, object> value = null)
        {
            if (YZGameUtil.GetIsiOS())
            {
                if (YZDefineUtil.GetIsFunnel && !string.IsNullOrEmpty(name))
                {
                    var newName = YZString.Concat(YZServerUtil.GetYZThinkPreName(), name);
                    YZDebug.Log("Think send event " + newName);
                    ThinkingAnalyticsAPI.Track(name, value);
#if !RELEASE
                    // if (value != null && value.Any())
                    // {
                    //     var json = JsonConvert.SerializeObject(value);
                    //     YZDebug.Log("Think send event" + json);
                    // }      
#endif
                    
                    //iOSCShapeThinkTool.Shared.IOSThinkTrack(json);
                }
            }
            else
            {
                if (YZDefineUtil.GetIsFunnel && !string.IsNullOrEmpty(name))
                {
#if UNITY_ANDROID || UNITY_EDITOR
                    var newName = YZString.Concat(YZServerUtil.GetYZThinkPreName(), name);
                    YZDebug.Log("Think send event " + newName);
                    YZAndroidThinkPlugin.Shared.AndroidThinkTrack(newName, value);
#endif
                }
            }
        }

        #endregion

        #region 按钮相关打点

        [Obsolete]
        public static void SendButtonClick(string name)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>()
            {
                { FunnelEventParam.brpos, name }
            };
            SendYZEvent(FunnelEventID.brbuttonclick, properties);
        }

        [Obsolete]
        public static void SendDepositButtonClick(string position)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>()
            {
                { FunnelEventParam.brdeposit, position }
            };
            SendYZEvent(FunnelEventID.brbuttonclick, properties);
        }

        #endregion

        // #region 房间相关打点
        //
        // public static void SendGamePlayClick(ListItem room, bool succ, string auth)
        // {
        //     if (room == null)
        //     {
        //         return;
        //     }
        //
        //     var feeData = RoomManager.Shared.CheckYZRoomFeeEnough(room.id);
        //     Dictionary<string, object> properties;
        //     if (feeData.ticket_type != RewardType.Chips && !string.IsNullOrEmpty(auth))
        //     {
        //         properties = new Dictionary<string, object>()
        //         {
        //             { FunnelEventParam.brroomid, room.id },
        //             { FunnelEventParam.brgamebegin, succ },
        //             { FunnelEventParam.brenoughmoney, GetEnoughtNumber(room, feeData) },
        //             { FunnelEventParam.brgpsresult, auth }
        //         };
        //     }
        //     else
        //     {
        //         properties = new Dictionary<string, object>()
        //         {
        //             { FunnelEventParam.brroomid, room.id },
        //             { FunnelEventParam.brgamebegin, succ },
        //             { FunnelEventParam.brenoughmoney, GetEnoughtNumber(room, feeData) }
        //         };
        //     }
        //
        //     YZDebug.LogConcat("[打点]房间点击 room_id:", room.id);
        //     SendYZEvent(FunnelEventID.brroomplayclick, properties);
        // }
        //
        // private static int GetEnoughtNumber(ListItem roomItem, YZRoomEnough roomEnough)
        // {
        //     if (roomItem.sub_title.Equals(RoomType.cost) || roomItem.sub_title.Equals(RoomType.active_charge))
        //     {
        //         return roomEnough.enough ? 5 : 6;
        //     }
        //     else
        //     {
        //         if (roomEnough.ticket_type == RewardType.Money)
        //         {
        //             return roomEnough.enough ? 1 : 2;
        //         }
        //         else
        //         {
        //             return roomEnough.enough ? 3 : 4;
        //         }
        //     }
        // }
        //
        // #endregion

        //#region 充值相关打点

        // /// 充值开始
        // public static void SendOrderStart(object chargeId)
        // {
        //     Dictionary<string, object> properties = new Dictionary<string, object>()
        //     {
        //         {
        //             FunnelEventParam.brisfirstpay,
        //             PlayerManager.Shared.GetSavedCount(FunnelEventParam.brisfirstpay) <= 0
        //         },
        //         { FunnelEventParam.brpayentername, chargeId }
        //     };
        //     SendYZEvent(FunnelEventID.brorderstart, properties);
        // }
        //
        // /// 18岁弹窗打点
        // public static void Send18YearOld(object is_true)
        // {
        //     Dictionary<string, object> properties = new Dictionary<string, object>()
        //     {
        //         {
        //             FunnelEventParam.brisfirstpay,
        //             PlayerManager.Shared.GetSavedCount(FunnelEventParam.brisfirstpay) <= 0
        //         },
        //         { FunnelEventParam.bristure, is_true }
        //     };
        //     SendYZEvent(FunnelEventID.brorder18, properties);
        //     properties.Clear();
        //     properties.Add(FunnelEventID.brpermissionage, is_true);
        //     UserYZSet(properties);
        // }
        //
        /// 充值渠道UI入口  充值打点：确认渠道
        public static void SendOrderUIChannel(object channel, Charge_configsItem chargeItem)
        {
            Root.Instance.OpenChargeChannelTimeStamp = TimeUtils.Instance.UtcTimeNow;
            int duration = Root.Instance.OpenChargeChannelTimeStamp - Root.Instance.OpenChargeTimeStamp;
            Dictionary<string, object> properties = new Dictionary<string, object>()
            {
                {
                    FunnelEventParam.brisfirstpay,
                    YZDataUtil.GetYZInt(YZConstUtil.YZIsLastDepositSuccess, 0) <= 0
                },
                {"session_id", Root.Instance.SessionId},
                {"duration", duration},
                {"payment_type", channel},
                {"pay_enter_name", chargeItem.position},
                {"charge_id", chargeItem.id},
                {"is_suggest", true}
            };
            SendYZEvent(FunnelEventID.brorderuichannel, properties);
        }
        //
        // /// 打开充值界面
        // public static void SendOrderUIOpen()
        // {
        //     Dictionary<string, object> properties = new Dictionary<string, object>()
        //     {
        //         {
        //             FunnelEventParam.brisfirstpay,
        //             PlayerManager.Shared.GetSavedCount(FunnelEventParam.brisfirstpay) <= 0
        //         }
        //     };
        //     SendYZEvent(FunnelEventID.brorderuiopen, properties);
        // }
        //
        // /// 充值界面关闭
        // public static void SendOrderUIClose()
        // {
        //     Dictionary<string, object> properties = new Dictionary<string, object>()
        //     {
        //         {
        //             FunnelEventParam.brisfirstpay,
        //             PlayerManager.Shared.GetSavedCount(FunnelEventParam.brisfirstpay) <= 0
        //         }
        //     };
        //     SendYZEvent(FunnelEventID.brorderclose, properties);
        // }
        //
        // /// GPS打点
        // public static void SendOrderGPS(YZSafeType type, object is_true)
        // {
        //     if (type == YZSafeType.Charge)
        //     {
        //         Dictionary<string, object> properties = new Dictionary<string, object>()
        //         {
        //             {
        //                 FunnelEventParam.brisfirstpay,
        //                 PlayerManager.Shared.GetSavedCount(FunnelEventParam.brisfirstpay) <= 0
        //             },
        //             { FunnelEventParam.bristure, is_true }
        //         };
        //         SendYZEvent(FunnelEventID.brordergps, properties);
        //     }
        // }
        //
        // /// GPS不合法
        // public static void SendOrderGPSFail(YZSafeType type, object error)
        // {
        //     if (type == YZSafeType.Charge)
        //     {
        //         Dictionary<string, object> properties = new Dictionary<string, object>()
        //         {
        //             {
        //                 FunnelEventParam.brisfirstpay,
        //                 PlayerManager.Shared.GetSavedCount(FunnelEventParam.brisfirstpay) <= 0
        //             },
        //             { FunnelEventParam.brerror, error }
        //         };
        //         SendYZEvent(FunnelEventID.brordergpsreject, properties);
        //     }
        // }
        //
        // /// GPS成功
        // public static void SendOrderGPSSucc(YZSafeType type)
        // {
        //     if (type == YZSafeType.Charge)
        //     {
        //         Dictionary<string, object> properties = new Dictionary<string, object>()
        //         {
        //             {
        //                 FunnelEventParam.brisfirstpay,
        //                 PlayerManager.Shared.GetSavedCount(FunnelEventParam.brisfirstpay) <= 0
        //             }
        //         };
        //         SendYZEvent(FunnelEventID.brordergpssuccess, properties);
        //     }
        // }
        //
        // /// 设置用户属性
        // public static void SetUserState(YZPlacemark pl)
        // {
        //     if (pl != null)
        //     {
        //         Dictionary<string, object> dict = new Dictionary<string, object>();
        //         dict.Add(FunnelEventID.brgpsstate, pl.administrativeArea);
        //         UserYZSet(dict);
        //     }
        // }
        //
        // /// <summary>
        // /// 充值过程中的网页跳转
        // /// </summary>
        // /// <param name="url"></param>
        // public static void SendOrderURL(string url)
        // {
        //     if (string.IsNullOrEmpty(url))
        //         return;
        //
        //     var stepNumber = 0;
        //     if (url.Contains(FunnelOrderURL.checkoutnow_1))
        //     {
        //         stepNumber = 1;
        //     }
        //     else if (url.Contains(FunnelOrderURL.signin_2))
        //     {
        //         stepNumber = 2;
        //     }
        //     else if (url.Contains(FunnelOrderURL.authflow_3))
        //     {
        //         stepNumber = 3;
        //     }
        //     else if (url.Contains(FunnelOrderURL.webapps_4))
        //     {
        //         stepNumber = 4;
        //     }
        //     else if (url.Contains(FunnelOrderURL.return_5))
        //     {
        //         stepNumber = 5;
        //     }
        //
        //     if (stepNumber > 0)
        //     {
        //         Dictionary<string, object> properties = new Dictionary<string, object>()
        //         {
        //             { FunnelEventParam.brpayway, YZChargeUICtrler.Shared().CurrentChargeType },
        //             { FunnelEventParam.brurlstep, stepNumber }
        //         };
        //         SendYZEvent(FunnelEventID.brorderurl, properties);
        //     }
        // }
        //
        // /// <summary>
        // /// 充值失败的打点
        // /// </summary>
        // public static void SendOrderFail()
        // {
        //     Dictionary<string, object> properties = new Dictionary<string, object>()
        //     {
        //         {
        //             FunnelEventParam.brduration,
        //             YZServerApi.Shared.GetYZServerTime() - YZChargeUICtrler.Shared().OrderBeginTimestamp
        //         }
        //     };
        //     SendYZEvent(FunnelEventID.brorderfail, properties);
        // }
        //
        // #endregion
        //
        // #region 推送
        //
        // public static void SendPush(string name, int id = -1, bool istrue = false)
        // {
        //     if (name == FunnelEventID.brpushclose)
        //     {
        //         SendYZEvent(name);
        //     }
        //     else if (name == FunnelEventID.brpushpopup)
        //     {
        //         Dictionary<string, object> properties = new Dictionary<string, object>()
        //         {
        //             { FunnelEventParam.brtotalgame, PlayerManager.Shared.Player.CountData.game_end_count },
        //             { FunnelEventParam.brroomid, id }
        //         };
        //         SendYZEvent(name, properties);
        //     }
        //     else
        //     {
        //         Dictionary<string, object> properties = new Dictionary<string, object>()
        //         {
        //             { FunnelEventParam.bristure, istrue }
        //         };
        //         SendYZEvent(name, properties);
        //     }
        // }
        //
        // #endregion
        //
        // #region 活动
        //
        // /// 打开活动打点 eid，pos只需传一个，另一个传0即可
        // public static void SendChargeEvent(bool suggest, int pos)
        // {
        //     ActivitiesItem item =
        //         EventManager.Shared.GetYZEvent(ChargeManager.Shared.YZGetEventIdByChargePosition(pos));
        //     if (item != null)
        //     {
        //         // 活动入口弹出打点
        //         int time = EventManager.Shared.GetYZEventTime(item);
        //         Dictionary<string, object> properties = new Dictionary<string, object>()
        //         {
        //             { FunnelEventParam.bractivityid, pos },
        //             { FunnelEventParam.brissuggest, suggest },
        //             { FunnelEventParam.brcountdown, time },
        //             { FunnelEventParam.brshowamount, YZGetChargeEventTimes(pos) },
        //         };
        //         SendYZEvent(FunnelEventID.brchargeactivityshow, properties);
        //     }
        //     else
        //     {
        //         // 海报或无入口充值弹出出打点
        //         YZDebug.LogConcat("活动弹出打点Postion: ", pos);
        //         Dictionary<string, object> properties = new Dictionary<string, object>()
        //         {
        //             { FunnelEventParam.bractivityid, pos },
        //             { FunnelEventParam.brissuggest, suggest },
        //             { FunnelEventParam.brcountdown, -1 },
        //             { FunnelEventParam.brshowamount, YZGetChargeEventTimes(pos) },
        //         };
        //         SendYZEvent(FunnelEventID.brchargeactivityshow, properties);
        //     }
        // }
        //
        // #endregion
        //
        // public static void YZFunnelGuide(int step)
        // {
        //     Dictionary<string, object> properties = new Dictionary<string, object>()
        //     {
        //         { FunnelEventParam.brstep, step },
        //         { FunnelEventParam.brisorganic, YZServerApiOrganic.Shared.IsYZOrganic() ? 1 : 0 },
        //     };
        //     SendYZEvent(FunnelEventID.brguide, properties);
        // }

        public static void YZFunnelSpecialTrigger(int type)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>()
            {
                { FunnelEventParam.brtype, type },
            };
            SendYZEvent(FunnelEventID.brspecialtipstrigger, properties);
        }

        public static void YZFunnelTongdun(bool begin, int reson)
        {
            if (begin)
            {
                Dictionary<string, object> properties = new Dictionary<string, object>()
                {
                    { FunnelEventParam.brlimittype, "tongdun" },
                    { FunnelEventParam.brreasons, reson },
                };
                SendYZEvent(FunnelEventID.brtriggerlimit, properties);
            }
            else
            {
                Dictionary<string, object> properties = new Dictionary<string, object>()
                {
                    { FunnelEventParam.brlimittype, "tongdun" },
                    { FunnelEventParam.brresult, reson },
                };
                SendYZEvent(FunnelEventID.brtriggerreturn, properties);
            }
        }

        public static void YZFunnelKYC(bool begin, int reson)
        {
            if (begin)
            {
                Dictionary<string, object> properties = new Dictionary<string, object>()
                {
                    { FunnelEventParam.brlimittype, "kyc" },
                    { FunnelEventParam.brreasons, reson },
                };
                SendYZEvent(FunnelEventID.brtriggerlimit, properties);
            }
            else
            {
                Dictionary<string, object> properties = new Dictionary<string, object>()
                {
                    { FunnelEventParam.brlimittype, "kyc" },
                    { FunnelEventParam.brresult, reson },
                };
                SendYZEvent(FunnelEventID.brtriggerreturn, properties);
            }
        }

        public static void YZFunnelFaker(string reson)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>()
            {
                { FunnelEventParam.brmode, reson },
            };
            SendYZEvent(FunnelEventID.brwarning, properties);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="activityType"></param>
        /// <param name="charge_id"></param>
        /// <param name="page_id"></param>
        /// <param name="isauto">是否自动弹出</param>
        /// <param name="duration"></param>
        /// <param name="outtype">1 关闭按钮 2其他</param>
        /// <param name="switch_click"></param>
        public static void YZFunnelActivityPop(ActivityType activityType, int charge_id = -1,
            int page_id = 1, bool isauto = true,  int duration = 0, int outtype = 1, int switch_click = 0)
        {
            Dictionary<string, object> properties = new Dictionary<string, object>()
            {
                { "activity_id" , (int)activityType},
                //活动名称
                { "activity_name" , activityType.ToString() },
                //充值项id
                { "charge_id" , charge_id},
                //页面id
                { "page_id" , page_id},
                //是否自动弹出
                { "isauto" , isauto},
                //页面停留时间
                { "duration" , duration},
                //1 关闭按钮 2其他
                { "outtype" , outtype},
                //点击跳转到其他界面, 然后回来; 切换tab + 1
                { "switch_click" , switch_click},
            };
            SendYZEvent(FunnelEventID.pop_up, properties);
        }
                
        public static void YZFunnelClickActivity(ActivityType activityType, string button_name = "",string skip_name = "",
            string button_classify = "actvt", string button_location = "main")
        {
            Dictionary<string, object> properties = new Dictionary<string, object>()
            {
                { "button_name" , button_name},
                //活动名称
                { "activity_id" , (int)activityType},
                //按钮分类
                { "button_classify" , button_classify},
                //按钮位置
                { "button_location", button_location},
                //跳转页面名称
                { "skip_name" , skip_name}

            };
            SendYZEvent(FunnelEventID.button_click, properties);
        }
        
        // public static int YZGetChargeEventTimes(int pos)
        // {
        //     JsonData json = new JsonData();
        //     string str = PlayerPrefs.GetString(YZConstUtil.YZDailyChargeShowTimes, "");
        //     if (!string.IsNullOrEmpty(str))
        //     {
        //         json = YZGameUtil.JsonYZToObject<JsonData>(str);
        //     }
        //
        //     int show = 1;
        //     if (YZJsonUtil.ContainsYZKey(json, YZString.Concat(pos)))
        //     {
        //         int last = YZJsonUtil.GetYZInt(json, YZString.Concat(pos));
        //         show = last + 1;
        //         json[YZString.Concat(pos)] = show;
        //     }
        //     else
        //     {
        //         json[YZString.Concat(pos)] = show;
        //     }
        //
        //     PlayerPrefs.SetString(YZConstUtil.YZDailyChargeShowTimes, json.ToJson());
        //     return show;
        // }

        public static JsonData YZDictToJsonData(Dictionary<string, object> dict)
        {
            if (dict == null || dict.Keys.Count <= 0)
            {
                return default;
            }

            JsonData temp = new JsonData();
            foreach (string key in dict.Keys)
            {
                temp[key] = YZString.Concat(dict[key]);
            }

            return temp;
        }
    }
}