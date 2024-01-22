using System;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using AndroidCShape;
using Carbon.Util;
using Castle.Core.Internal;
using CatLib;
using CatLib.EventDispatcher;
using Core.Controllers;
using Core.Extensions;
using Core.Manager;
using Core.Models;
using Core.Server;
using Core.Services.NetService;
using Core.Services.PersistService.API.Facade;
using Core.Services.ResourceService.API.Facade;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Third.I18N;
using Cysharp.Threading.Tasks;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using iOSCShape;
using LitJson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ThinkingAnalytics;
using UI;
using UI.Activity;
using UI.UIChargeFlow;
using UI.UIWithDrawFlow;
using UnityEngine;
using Utils;
using Application = UnityEngine.Application;
using NetSystem = Core.Services.NetService.API.Facade.NetSystem;
using Random = System.Random;
using Timer = UnityTimer.Timer;

namespace DataAccess.Controller
{
    public class MediatorRequest : global::Utils.Runtime.Singleton<MediatorRequest>, IRequestSender
    {
        /// <summary>
        /// 加密
        /// </summary>
        private const string AUTH_KEY = "oTSZTWFrC7LJ2HrRFtTgyR2yQo0sK0hd";

        private const string testPictrue = "";

        private static int MSG_INDEX;
        
        private static int LOGIN_COUNT;
        
        SortedDictionary<string, object> loginParam;

        private SortedDictionary<string, object> LoginParam
        {
            get { return loginParam ??= GetAllParams(); }
        }

        private LoginToServerData loginToServerData;

        private SortedDictionary<string, object> paramHandler;

        public SortedDictionary<string, object> ParamHandler
        {
            get => paramHandler ??= new SortedDictionary<string, object>();
        }

        private LoginToServerData LoginToServerData
        {
            get
            {
                if (loginToServerData == null)
                {
                    loginToServerData = InitLoginData();
                }
                else
                {
                    RefreshLoginDate();
                }
                return loginToServerData;
            }
        }


        /// <summary>
        /// 更新udid 
        /// </summary>
        public void UpdateUdid()
        {
            LoginToServerData.udid = DeviceInfoUtils.Instance.GetEquipmentId();
        }
        
        /// <summary>
        /// 玩家登陆
        /// </summary>
        public void PlayerLogin(Action callback = null)
        {
            var authorization = GetAuthorization();
            string url;
            if (string.IsNullOrEmpty(authorization))
            {
                url = Proto.VISITOR_LOGIN;
            }
            else
            {
                YZLog.LogColor($"token = {authorization}");
                url = Proto.PLAYER_LOGIN;
            }

            YZSDKsController.Shared.CurrentUrl = url;

            NetSystem.That.SetFailCallBack(content =>
            {
                if (content != null && content.Contains("timestamp error"))
                {
                    SendLoginRequest(callback, url, authorization);
                }
            });
            SendLoginRequest(callback, url, authorization);
        }

        private void SendLoginRequest(Action callback, string url, string authorization)
        {
            NetSystem.That.SendGameRequest(url,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    var data = jObject.SelectToken("data")!.ToString();
                    var role = JsonConvert.DeserializeObject<Role>(data);
                    Root.Instance.Role = role;

                    var jsonProtoRecords = PersistSystem.That.GetValue<string>(GlobalEvent.PROTO_LIST, true) as string;

                    if (!jsonProtoRecords.IsNullOrEmpty())
                    {
                        try
                        {
                            NetSystem.That.SendedProtoQueue =
                                JsonConvert.DeserializeObject<List<ProtoRecord>>(jsonProtoRecords);
                        }
                        catch (Exception e)
                        {
                            // Console.WriteLine(e);
                            // throw;
                        }
                    }

                    if (NetSystem.That.SendedProtoQueue == null)
                    {
                        NetSystem.That.SendedProtoQueue = new List<ProtoRecord>();
                    }

                    // 不在白名单内
                    if (Root.Instance.Role.white == 0)
                    {
                        // 禁登生效
                        if (YZSDKsController.Shared.IsBlockValid)
                        {
                            int lockCN = YZJsonUtil.DeserializeJObject<int>("data.lock_cn", jObject);
                            if (lockCN != 0 || YZSDKsController.Shared.ClientBlock)
                            {
                                // if (YZSDKsController.Shared.ClientBlock)
                                //     YZDebug.LogError("YZSDKsController.Shared.ClientBlock = true");

                                UserInterfaceSystem.That.ShowUI<UIConfirm>(new UIConfirmData()
                                {
                                    Type = UIConfirmData.UIConfirmType.OneBtn,
                                    HideCloseBtn = true,
                                    desc = I18N.Get("key_http_code_9997", Root.Instance.UserId),
                                    confirmTitle = I18N.Get("key_ok"),
                                    WaitCloseCallback = true,
                                    cancleCall = () => { UserInterfaceSystem.That.RemoveUIByName(nameof(UIConfirm)); },
                                    confirmCall = () => { GameUtils.ExitGame(); }
                                });
                                YZDebug.Log("封禁地区");
                                return;
                            }
                        }
                    }


                    Root.Instance.RegisterTime = YZJsonUtil.DeserializeJObject<int>("tag.register_time", jObject);

                    //充值信息
                    Root.Instance.ChargeInfo =
                        YZJsonUtil.DeserializeJObject<ChargeInfo>("data.chargeInfo", jObject);

                    //充值金额
                    Root.Instance.Role.TotalCharge =
                        Math.Round(YZJsonUtil.DeserializeJObject<double>("data.vipInfo.success_total", jObject), 2);

                    Root.Instance.WheelFreeTicket =
                        YZJsonUtil.DeserializeJObject<int>("data.fortuneWheelInfo.wheel_free_ticket", jObject);

                    Root.Instance.PiggyBankInfo =
                        YZJsonUtil.DeserializeJObject<PiggyBankInfo>("data.piggyBankInfo", jObject);
                    
                    Root.Instance.ProvinceValid = YZJsonUtil.DeserializeJObject<int>("data.ip_gps", jObject);
                    
                    int is_organic = jObject.SelectToken("data.userInfo.is_organic").Value<int>();

                    if (is_organic == 0)
                    {
                        YZDataUtil.SetYZInt(YZConstUtil.YZEverNotOrganic, 1);
                    }

                    SyncLoginData(jObject);

                    EventDispatcher.Root.Raise(GlobalEvent.Login_Success);

                    SuccessLoginLog();

                    SetOrganic();

                    // 非自然量传 source
                    if (!YZSDKsController.Shared.MediaSource.IsNullOrEmpty())
                    {
                        SendMediaSource(YZSDKsController.Shared.MediaSource);
                    }

                    SendAFID();

                    // 提前加载玩家头像
                    Root.Instance.Role.LoadIcon(UILogin.Inst.GetInvisibleIcon());

                    // 推送传Tag onesignal
                    if (jObject.SelectToken("tag") != null)
                    {
                        var tagStr = jObject.SelectToken("tag")?.ToString();
                        YZNativeUtil.SetPushTags(tagStr);
                    }


                    callback?.Invoke();

#if UNITY_ANDROID
                    // Forter设置账号
                    YZAndroidPlugin.Shared.AndroidForterSetAccount();

                    // 风控打点
                    YZAndroidPlugin.Shared.AndroidForterSendEvent(YZSDKsController.Shared.CurrentUrl.Contains("guest")
                        ? "ACCOUNT_ID_ADDED"
                        : "ACCOUNT_LOGIN");
#elif UNITY_IOS
                    // 风控打点
                    string eventName = YZSDKsController.Shared.CurrentUrl.Contains("guest")
                        ? @"Account_Id_Added"
                        : @"Account_Login";
                    iOSCShapeForterTool.Shared.IOSForterTrackAction(eventName, "");
                    iOSCShapeRiskifiedTool.Shared.IOSRiskifiedLogRequest(eventName);
#endif
                    
                    // 启动SDK, 里面会发消息,必须在储存token之后
#if (UNITY_ANDROID || UNITY_IOS) && !NO_SDK
                    YZSDKsController.Shared.YZInitSDKAndConfig();
#endif
                    
                },
                GetJsonByLoginData(authorization)
            );
        }

        public void SetOrganic()
        {
            if (YZDataUtil.GetYZInt(YZConstUtil.YZEverNotOrganic, 0) == 1)
            {
                YZLog.LogColor(" YZSDKsController.Shared.AF_ORGANIC = false; ");
                YZSDKsController.Shared.AF_ORGANIC = false;
            }

            // 归因 0 是非自然量 
            if (!YZSDKsController.Shared.AF_ORGANIC || !Root.Instance.IsNaturalFlow_InData)
            {
                if (Root.Instance.ProvinceValid == 3 && Root.Instance.Role.white == 0)
                {
                    // 非法州 非法IP
                    YZLog.LogColor("非法州 非法IP YZSDKsController.Shared.AF_ORGANIC = true;");
                    YZSDKsController.Shared.AF_ORGANIC = true;
                }

                GMSetOrganic();
            }
        }

        private static void SuccessLoginLog()
        {
#if DEBUG
            YZLog.LogColor($"DEBUG 宏打开");
#endif

            if (Debug.isDebugBuild)
            {
                YZLog.LogColor($"Debug.isDebugBuild 为 ture");
            }

            var appVersion = Application.version;

            YZLog.LogColor($"App version ={Application.version}");

            YZLog.LogColor($"设备ID ={DeviceInfoUtils.Instance.GetEquipmentId()}");

            var cacheVersion = PersistSystem.That.GetValue<string>(GlobalEnum.APP_VERSION);

            if (appVersion != (string)cacheVersion)
            {
                YZLog.LogColor("新版本");
            }


            YZLog.LogColor("旧版本");
        }

        /// <summary>
        /// 更新一些 登陆时数据
        /// </summary>
        /// <param name="jObject"></param>
        private void SyncLoginData(JObject jObject)
        {
            SaveToken(jObject);

            Root.Instance.LuckyGuyInfo =
                YZJsonUtil.DeserializeJObject<LuckyGuyInfo>("data.luckyYouInfo", jObject);
            
            //签到信息
            Root.Instance.SignInfo =
                YZJsonUtil.DeserializeJObject<SignInfo>("data.signInfo", jObject);
            
            Root.Instance.MuseumInfo =
                YZJsonUtil.DeserializeJObject<MuseumInfo>("data.museumInfo", jObject);

            Root.Instance.FortuneWheelInfo =
                YZJsonUtil.DeserializeJObject<FortuneWheelInfo>("data.fortuneWheelInfo", jObject);

            Root.Instance.MagicBallInfo =
                YZJsonUtil.DeserializeJObject<MagicBallInfo>("data.wizardInfo", jObject);

            Root.Instance.DailyRewardChance = YZJsonUtil.DeserializeJObject<int>( "data.shopInfo.shop_daily_reward_chance", jObject);
            
            //首充礼包 不准确
            // Root.Instance.StarterPackInfo =
            //     YZJsonUtil.DeserializeJObject<StarterPackInfo>("data.starterPackInfo", jObject);

            //任务信息
            Root.Instance.CurTaskInfo =
                YZJsonUtil.DeserializeJObject<TaskInfo>("data.castleTaskInfo", jObject);

            // 广告房间信息
            Root.Instance.RoomAdInfo =
                YZJsonUtil.DeserializeJObject<RoomAdInfo>("data.adRoomInfo", jObject);

            Root.Instance.OnlineRewardInfo =
                YZJsonUtil.DeserializeJObject<OnlineRewardInfo>("data.onlineActiveInfo", jObject);

            TimeUtils.Instance.LoginTimeStamp = jObject.SelectToken("time").Value<int>();
            TimeUtils.Instance.EndDayTimeStamp = jObject.SelectToken("data.day_end_timestamp").Value<int>();
            Root.Instance.UserInfo = YZJsonUtil.DeserializeJObject<UserInfo>("data.userInfo", jObject);

            if (jObject.SelectToken("data.withdrawInfo") != null)
                Root.Instance.Role.withdrawInfo =
                    YZJsonUtil.DeserializeJObject<WithdrawInfo>("data.withdrawInfo", jObject);

            Root.Instance.Role.luckyCardInfo =
                YZJsonUtil.DeserializeJObject<LuckyCardInfo>("data.luckyCardInfo", jObject);
            if (Root.Instance.Role.luckyCardInfo != null)
                Root.Instance.Role.luckyCardInfo.end_timestamp =
                    Root.Instance.Role.luckyCardInfo.lucky_card_begin_time + 24 * 3600;

            Root.Instance.Role.dragonInfo = YZJsonUtil.DeserializeJObject<DragonInfo>("data.oneStopInfo", jObject);
            if (Root.Instance.Role.dragonInfo != null)
                Root.Instance.Role.dragonInfo.end_timestamp =
                    Root.Instance.Role.dragonInfo.one_stop_begin_time + 24 * 3600;

            Root.Instance.Role.specialGiftInfo = YZJsonUtil.DeserializeJObject<SpecialGiftInfo>(
                "data.specialGiftInfo", jObject);
            if (Root.Instance.Role.specialGiftInfo != null)
            {
                var spLessTime = TimeUtils.Instance.EndDayTimeStamp -
                                 Root.Instance.Role.specialGiftInfo.special_gift_create_time;
                if (spLessTime / 3600 <= 24 && spLessTime > 0)
                    Root.Instance.Role.specialGiftInfo.special_gift_end_time = TimeUtils.Instance.EndDayTimeStamp;
            }

            Root.Instance.FreeBonusInfo = YZJsonUtil.DeserializeJObject<FreeBonusInfo>(
                "data.freeBonusInfo", jObject);

            Root.Instance.dailyTaskInfo = YZJsonUtil.DeserializeJObject<DailyTaskInfo>("data.dailyTaskInfo", jObject);

            Root.Instance.Role.SpecialOfferInfo =
                YZJsonUtil.DeserializeJObject<SpecialOfferInfo>("data.specialOfferInfo", jObject);
            
            Root.Instance.Role.AdditionalGiftInfo = YZJsonUtil.DeserializeJObject<AdditionalGiftInfo>(
                "data.addChargeInfo", jObject);

            // Root.Instance.Role.first_ip = YZJsonUtil.DeserializeJObject<string>("data.first_ip", jObject);
        }

        private static void SaveToken(JObject jObject)
        {
            if (Proto.SERVER_URL.Contains("https"))
            {
                Root.Instance.AuthorizationRelease =
                    jObject.SelectToken("data.authorization")?.Value<string>();
            }
            else
            {
                Root.Instance.AuthorizationDebug =
                    jObject.SelectToken("data.authorization")?.Value<string>();
            }
        }

        /// <summary>
        /// 跨天登陆
        /// </summary>
        public void PassDayLogin()
        {
            NetSystem.That.SendGameRequest(Proto.PASSDAY_LOGIN,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    SyncLoginData(jObject);
                    //重新拉取配置
                    GetConfigs(callback: () => EventDispatcher.Root.Raise(GlobalEvent.Pass_Day));
                },
                GetJsonByLoginData(GetAuthorization())
            );
        }

        public void GetRoomList()
        {
            ParamHandler.Clear();
            NetSystem.That.SendGameRequest(Proto.GET_ROOM_LIST,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    string room_list = jObject.SelectToken("data.room_list")!.ToString();
                    YZLog.LogColor("config room list = " + room_list);
                    Root.Instance.RoomList = JsonConvert.DeserializeObject<List<Room>>(room_list);
                    YZSDKsController.Shared.SetupAds();
                },
                GetBaseJson(ParamHandler));
        }

        public void GetConfigs(bool isShowUIMain = false, Action callback = null)
        {
            NetSystem.That.SendGameRequest(Proto.GET_CONFIGS, GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    Root.Instance.MagicBallDatas =
                        YZJsonUtil.DeserializeJObject<List<MagicBallData>>(
                            "data.configs.wizard_config.wizard_item", jObject);

                    Root.Instance.MagicPageRewards =
                        YZJsonUtil.DeserializeJObject<List<MagicPageReward>>(
                            "data.configs.wizard_config.wizard_all_item", jObject);
                    
                    Root.Instance.NewPlayerBonus = YZJsonUtil.DeserializeJObject<Dictionary<string, float>>(
                        "data.configs.room_guide_reward", jObject);

                    Root.Instance.MonthCardBonusInfos =
                        YZJsonUtil.DeserializeJObject<Dictionary<int, MonthCardBonusInfo>>(
                            "data.configs.infinite_grail_config.bonus_info", jObject);

                    Root.Instance.WeekCardChargeInfos =
                        YZJsonUtil.DeserializeJObject<List<ChargeGoodInfo>>(
                            "data.configs.infinite_grail_config.charge_info.week", jObject);

                    Root.Instance.MonthCardChargeInfos =
                        YZJsonUtil.DeserializeJObject<List<ChargeGoodInfo>>(
                            "data.configs.infinite_grail_config.charge_info.month", jObject);

                    Root.Instance.WeekConfig = YZJsonUtil.DeserializeJObject<InfiniteWeekConfig>(
                        "data.configs.infinite_week_config", jObject);

                    Root.Instance.MuseumItems =
                        YZJsonUtil.DeserializeJObject<List<MuseumItem>>("data.configs.museum_config", jObject);

                    Root.Instance.OnlineActiveConfig =
                        YZJsonUtil.DeserializeJObject<YZReward[][]>("data.configs.online_active_config", jObject);
                    Root.Instance.ChargeVersion =
                        YZJsonUtil.DeserializeJObject<int>("data.configs.charge_version", jObject);
                    Root.Instance.VipConfig =
                        YZJsonUtil.DeserializeJObject<Dictionary<int, float>>("data.configs.vip_config", jObject);
                    Root.Instance.ChargeVersion =
                        YZJsonUtil.DeserializeJObject<int>("data.configs.charge_version", jObject);

                    Root.Instance.FreeWheelList =
                        YZJsonUtil.DeserializeJObject<List<YZReward>>("data.configs.wheel_config.free_list",
                            jObject);

                    Root.Instance.SmallPayWheelList =
                        YZJsonUtil.DeserializeJObject<List<YZReward>>("data.configs.wheel_config.pay_list",
                            jObject);

                    Root.Instance.BigPayWheelList =
                        YZJsonUtil.DeserializeJObject<List<YZReward>>("data.configs.wheel_config.pay_big_list",
                            jObject);
                    
                    Root.Instance.WheelChargeInfos = YZJsonUtil.DeserializeJObject<Dictionary<int, WheelChargeInfo>>(
                        "data.configs.wheel_config.charge_info",
                        jObject);

                    Root.Instance.SignAwardsList =
                        YZJsonUtil.DeserializeJObject<List<YZReward>>("data.configs.sign_config.rewards", jObject);

                    Root.Instance.SignHeapAwardsList =
                        YZJsonUtil.DeserializeJObject<List<YZReward>>("data.configs.sign_config.heap_rewards",
                            jObject);

                    Root.Instance.TaskConfigs =
                        YZJsonUtil.DeserializeJObject<Dictionary<int, List<TaskConfig>>>("data.configs.castle_task",
                            jObject);

                    Root.Instance.ShopConfig =
                        YZJsonUtil.DeserializeJObject<List<ShopInfo>>("data.configs.shop_config.shop.shop",
                            jObject);

                    Root.Instance.StarterPackConfig =
                        YZJsonUtil.DeserializeJObject<Dictionary<int, List<ChargeGoodInfo>>>(
                            "data.configs.starter_pack_config",
                            jObject);

                    //存钱罐信息
                    Root.Instance.PiggyBankConfig = YZJsonUtil.DeserializeJObject<PiggyBankConfig>(
                        "data.configs.piggy_bank_config",
                        jObject);

                    Root.Instance.DailyRewardList = YZJsonUtil.DeserializeJObject<List<DailyReward>>(
                        "data.configs.shop_config.daily_reward",
                        jObject);

                    Root.Instance.LuckyCardConfigs = YZJsonUtil.DeserializeJObject<Dictionary<int,
                        List<LuckyCardConfig>>>("data.configs.lucky_card_config", jObject);

                    Root.Instance.DragonConfig = YZJsonUtil.DeserializeJObject<DragonConfig>
                        ("data.configs.one_stop_config", jObject);


                    Root.Instance.FreeBonusConfig = YZJsonUtil.DeserializeJObject<List<ChargeGoodInfo>>
                        ("data.configs.free_bonus_config", jObject);

                    Root.Instance.LuckyGuyConfig = YZJsonUtil.DeserializeJObject<List<ChargeGoodInfo>>
                        ("data.configs.lucky_you_config", jObject);
                    
                    Root.Instance.DailyMissionConfigs = YZJsonUtil.DeserializeJObject<DailyMission>(
                        "data.configs.daily_task_config", jObject);

                    Root.Instance.FriendsDualConfigs = YZJsonUtil.DeserializeJObject<List<FriendsDuelConfig>>(
                        "data.configs.friends_duel_config", jObject);

                    Root.Instance.SpecialOfferConfig =
                        YZJsonUtil.DeserializeJObject<List<SpecialOfferConfig>>("data.configs.special_offer_config", jObject);

                    Root.Instance.AdditionalGiftConfigs = YZJsonUtil.DeserializeJObject <List<AdditionalGiftConfig>>
                        ("data.configs.add_charge_config", jObject);

                    Root.Instance.ChargeConfig = YZJsonUtil.DeserializeJObject<ChargeConfig>
                        ("data.configs.charge_config", jObject);

                    //注意下面代码要写两份  ！！！
#if DEBUG
                    Framework.Instance.StartCoroutine(CorGetInfoAfterConfig());         
#else
                    GetInfoAfterConfig();
#endif
                    
                    EventDispatcher.Root.Raise(GlobalEvent.FinishConfigDeserialize);

                    if (isShowUIMain)
                    {
                        UserInterfaceSystem.That.CloseAllUI();
                        UserInterfaceSystem.That.ShowUI<UIMain>();
                    }

                    callback?.Invoke();
                },
                GetBaseJson()
            );
        }
        
        IEnumerator CorGetInfoAfterConfig()
        {
            var waitForSeconds = new WaitForSeconds(0.5f);
            
            GetRoomList();
        
            yield return waitForSeconds;
            
            // GetMonthCardInfo();
            yield return waitForSeconds;
            GetWeekCardInfo();
            yield return waitForSeconds;
            GetStarterPackInfo();
            yield return waitForSeconds;
            GetRoomChargeInfo();
            yield return waitForSeconds;
            GetFriendsDuelInfo();
            yield return waitForSeconds;
            //请求历史记录数据
            GetCompleteHistory();
            yield return waitForSeconds;
            GetInCompleteHistory();
            yield return waitForSeconds;
            MediatorActivity.Instance.CheckStartPacker();
        }
        
        private void GetInfoAfterConfig()
        {
            GetRoomList();

            // GetMonthCardInfo();

            GetWeekCardInfo();

            GetStarterPackInfo();

            GetRoomChargeInfo();

            GetFriendsDuelInfo();

            //请求历史记录数据
            GetCompleteHistory();
            
            GetInCompleteHistory();

            MediatorActivity.Instance.CheckStartPacker();
        }

        public string GetAuthorization()
        {
            if (Proto.SERVER_URL.Contains("https"))
            {
                return Root.Instance?.AuthorizationRelease ??
                       PersistSystem.That.GetValue<string>(GlobalEnum.AUTHORIZATION_RELAESE) as string;
            }
            else
            {
                return Root.Instance?.AuthorizationDebug ??
                       PersistSystem.That.GetValue<string>(GlobalEnum.AUTHORIZATION_DEBUG) as string;
            }
        }

        string GetJsonByLoginData(string authorization = "")
        {
            RefreshTimestamp();
            LoginToServerData.login_count = LOGIN_COUNT ++ ;
            LoginToServerData.msg_index = MSG_INDEX++;
            LoginToServerData.signature = GetSignature(GetAllParams(), authorization);
            //忽略null
            return GameUtils.Object2Json(LoginToServerData);
        }

        public string GetBaseJson(SortedDictionary<string, object> dic = null, bool withToken = true)
        {
            if (dic == null)
            {
                dic = new SortedDictionary<string, object>();
            }

            dic["timestamp"] = TimeUtils.Instance.UtcTimeNow.ToString();
            dic["msg_index"] = MSG_INDEX++;
            dic["signature"] = GetSignature(dic, withToken ? GetAuthorization() : null);
            return JsonConvert.SerializeObject(dic);
        }

        SortedDictionary<string, object> GetSortedParams(object obj)
        {
            SortedDictionary<string, object> sortedDictionary = new SortedDictionary<string, object>();

            Type type = obj.GetType();
            FieldInfo[] fields = type.GetFields();

            foreach (var field in fields)
            {
                var value = field.GetValue(obj);
                if (value != null)
                {
                    if ("signature" == field.Name)
                    {
                        continue;
                    }

                    sortedDictionary.Add(field.Name, value);
                }
            }

            return sortedDictionary;
        }

        string GetSignature(object obj, string authorization = null)
        {
            return GetSignature(GetSortedParams(obj), authorization);
        }

        //可能会多次签名
        public string GetSignature(SortedDictionary<string, object> param, string authorization = null)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(authorization);
            foreach (var data in param)
            {
                stringBuilder.Append($"{data.Key}={data.Value}");

                stringBuilder.Append("&");
            }

            //删除最后一次拼接
            stringBuilder.Remove(stringBuilder.Length - 1, 1);
            stringBuilder.Append(AUTH_KEY);
            string builderResult = stringBuilder.ToString();
            return Encryption(builderResult);
        }

        string Encryption(string clear)
        {
            // using (HashAlgorithm hashAlgorithm = new SHA1CryptoServiceProvider())
            {
                byte[] utf8Bytes = Encoding.UTF8.GetBytes(clear);
                // byte[] hashBytes2 = hashAlgorithm.ComputeHash(utf8Bytes);
                var hashBytes = SHA1.Create().ComputeHash(utf8Bytes);
                //x 小写，X 大写
                var result = string.Join("", hashBytes.Select(b => $"{b:x2}"));
                return result;
            }
        }

        void RefreshTimestamp()
        {
            if (loginToServerData != null)
            {
                var timestamp = TimeUtils.Instance.UtcTimeNow.ToString();
                loginToServerData.Timestamp = timestamp;
                YZLog.LogColor(timestamp, "yellow");
            }
        }

        void RefreshLoginDate()
        {
            if (loginToServerData != null)
            {
                loginToServerData.country = YZDataUtil.GetLocaling(YZConstUtil.YZCountryCode);
                loginToServerData.ip = YZNativeUtil.GetIPAdress();
                loginToServerData.gps_extra = DeviceInfoUtils.Instance.GetGPSJson();
                
                var gps = new JsonData
                {
                    ["ISOcountryCode"] = YZDataUtil.GetLocaling(YZConstUtil.YZCountryCode),
                    ["administrativeArea"] = YZDataUtil.GetLocaling(YZConstUtil.YZAreaCode)
                };
                
                loginToServerData.gps = gps.ToJson();
            }
        }
        
        SortedDictionary<string, object> GetAllParams()
        {
            return GetSortedParams(LoginToServerData);
        }

        /// <summary>
        /// 支付门票，开始匹配
        /// </summary>
        /// <param name="roomId"></param>
        /// <param name="isGuideRunning"></param>
        public void MatchBegin(int roomId, bool isGuideRunning = false)
        {
            ParamHandler.Clear();
            ParamHandler["room_id"] = roomId;
            paramHandler["gps_extra"] = DeviceInfoUtils.Instance.GetGPSJson();
            var json = GetBaseJson(ParamHandler);
            NetSystem.That.SetFailCallBack(s => EventDispatcher.Root.Raise(GlobalEvent.Sync_Item));
            NetSystem.That.SendGameRequest(Proto.MATCH_BEGIN,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    var matchId = ProcessMatchBeginData(roomId, jObject);

                    Root.Instance.tool_game_replay_close =
                        YZJsonUtil.DeserializeJObject<int>("data.tool_game_replay_close", jObject);

                    UserInterfaceSystem.That.ShowUI<UIMatch>(matchId, roomId, isGuideRunning);
                },
                json
            );
        }

        public void MatchBegin(Room room, bool isGuideRunning = false, Action callback = null)
        {
            ParamHandler.Clear();
            ParamHandler["room_id"] = room.id;
            paramHandler["gps_extra"] = DeviceInfoUtils.Instance.GetGPSJson();
            var json = GetBaseJson(ParamHandler);
            NetSystem.That.SetFailCallBack(s => EventDispatcher.Root.Raise(GlobalEvent.Sync_Item));
            NetSystem.That.SendGameRequest(Proto.MATCH_BEGIN,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    if (room.IsAboutMoney && Root.Instance.Role.IsFreeze)
                    {
                        UserInterfaceSystem.That.ShowUI<UIConfirm>(new UIConfirmData()
                        {
                            Type = UIConfirmData.UIConfirmType.OneBtn,
                            HideCloseBtn = false,
                            desc = I18N.Get("key_unusual_activity"),
                            confirmTitle = I18N.Get("key_contact_us"),
                            WaitCloseCallback = true,
                            confirmCall = () => { YZNativeUtil.ContactYZUS(EmailPos.Charge); },
                            cancleCall = () => { UserInterfaceSystem.That.RemoveUIByName(nameof(UIConfirm)); }
                        });
                        return;
                    }
                    var matchId = ProcessMatchBeginData(room.id, jObject);

                    Root.Instance.tool_game_replay_close =
                        YZJsonUtil.DeserializeJObject<int>("data.tool_game_replay_close", jObject);
                    callback?.Invoke();
                    UserInterfaceSystem.That.ShowUI<UIMatch>(matchId, room, isGuideRunning);
                },
                json
            );
        }

        private string ProcessMatchBeginData(int roomId, JObject jObject)
        {
            if (Root.Instance.NotPassTheNewPlayerGuide(NewPlayerGuideStep.FIRST_ROOM_GAME))
            {
                SendNewPlayerGuideStep(NewPlayerGuideStep.FIRST_ROOM_GAME);
            }

            MediatorActivity.Instance.AddAllGameInterval();

            YZLog.LogColor("MatchBegin Success", "blue");
            //todo 维护matchList 数据
            var matchJson = jObject.SelectToken("data.match").ToString();
            var match = JsonConvert.DeserializeObject<Match>(matchJson);
            Root.Instance.Role.MatchList.Add(match);

            var last_cash_room = jObject.SelectToken("data.last_cash_room")?.Value<int>();

            if (last_cash_room > 0)
            {
                Root.Instance.UserInfo.last_cash_room = (int)last_cash_room;
            }

            var matchId = match.match_id;

            MediatorBingo.Instance.SaveMatchId(matchId, roomId);

            var balance = jObject.SelectToken("data.balance").ToString();
            SyncItem(balance);
            return matchId;
        }

        public void SyncItem(string balance, bool showDiff = true, bool isShowAll = false, bool fireEvent = true)
        {
            SyncItem(balance, out var diff, showDiff, showAll: isShowAll, fireEvent: fireEvent);
        }

        private void SyncItem(JObject jObject, bool showDiff = true, bool onlyAnimation = false, bool fireEvent = true)
        {
            SyncItem(jObject.SelectToken("data.balance")?.ToString(), out var diff, showDiff,
                onlyAnimation: onlyAnimation, fireEvent: fireEvent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="balance"></param>
        /// <param name="diff"></param>
        /// <param name="showDiff"></param>
        /// <param name="insertItems"></param>
        /// <param name="delayTime"></param>
        /// <param name="showAll">是否分开显示bonus 和 cash</param>
        /// <param name="fireEvent"></param>
        /// <param name="onlyAnimation"></param>
        private void SyncItem(string balance, out List<Item> diff, bool showDiff = true, List<Item> insertItems = null
            , float delayTime = 0f, bool showAll = false, bool fireEvent = true, bool onlyAnimation = false,
            bool wait_uimain = false)
        {
            try
            {
                diff = insertItems;
                if (balance.IsNullOrEmpty())
                {
                    return;
                }

                YZLog.LogColor("道具同步 " + balance, "red");
                var dic = JsonConvert.DeserializeObject<Dictionary<string, float>>(balance);

                foreach (var item in Root.Instance.Role.Items)
                {
                    //是否有四种道具之一
                    var success = dic.TryGetValue(item.name, out var syncValue);
                    if (success) 
                    {
                        diff ??= new List<Item>();
                        if (syncValue - item.Count > 0)
                        {
                            diff.Add(new Item(item.id, (float)Math.Round(syncValue - item.Count, 2)));
                        }
                        
                        //因为美金的来源是两个道具， 所以美金必须小于才同步
                        if (syncValue - item.Count < 0)
                        {
                            item.Count = syncValue;
                            EventDispatcher.Root.Raise(GlobalEvent.Sync_Single_Item, item.id);
                        }
                        //如果不是美金， 相等也同步
                        else if(syncValue - item.Count == 0 && (item.id != Const.Bonus && item.id != Const.Cash))
                        {
                            EventDispatcher.Root.Raise(GlobalEvent.Sync_Single_Item, item.id);
                        }

                        item.Count = syncValue;
                    }
                }

                bool diffNotEmpty = diff is { Count: > 0 };

                if (showDiff && diffNotEmpty)
                {
                    if (onlyAnimation)
                    {
                        foreach (var item in diff)
                        {
                            EventDispatcher.Root.Raise(GlobalEvent.GetItems, (Vector3.zero, item));
                        }
                    }
                    else
                    {
                        var gameData = new GameData()
                        {
                            ["diff"] = diff,
                            ["showAll"] = showAll
                        };

                        if (delayTime == 0)
                        {
                            if (wait_uimain)
                            {
                                UserInterfaceSystem.That.ShowQueue<UIGetRewards>(() =>
                                {
                                    var topUI = UserInterfaceSystem.That.GetTopNormalUI();
                                    if (topUI == null)
                                    {
                                        return false;
                                    }

                                    return topUI.ClassType == typeof(UIMain);
                                }, gameData);
                            }
                            else
                            {
                                UserInterfaceSystem.That.TopInQueue<UIGetRewards>(gameData);
                            }
                        }
                        else
                        {
                            TinyTimer.StartTimer(
                                () => { UserInterfaceSystem.That.TopInQueue<UIGetRewards>(gameData); },
                                delayTime);
                        }
                    }
                }
                else
                {
                    if (fireEvent)
                    {
                        EventDispatcher.Root.Raise(GlobalEvent.Sync_Item);
                    }
                }
            }
            catch (Exception e)
            {
                YZLog.LogError(e.ToString());
                diff = null;
            }
        }

        /// <summary>
        /// 结束匹配， 准备开始游戏, 获取游戏数据
        /// </summary>
        public void MatchEnd(string match_id, Room room)
        {
            ParamHandler.Clear();
            ParamHandler["match_id"] = match_id;

            NetSystem.That.SendGameRequest(Proto.MATCH_END,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    //游戏种子数据
                    string gridSeed = jObject.SelectToken("data.table.seed_info.chess_board").ToString();
                    string callSeed = jObject.SelectToken("data.table.seed_info.vote_numbers").ToString();
                    string propsList = jObject.SelectToken("data.table.props_list.props").ToString();
                    var randJson = jObject.SelectToken("data.table.props_list.rand_list").ToString();

                    MediatorBingo.Instance.GridSeed = gridSeed;
                    MediatorBingo.Instance.CallSeed = callSeed;
                    MediatorBingo.Instance.Props = propsList;
                    //四选一列表
                    MediatorBingo.Instance.ChooseArray = JsonConvert.DeserializeObject<string[]>(randJson);

                    var syncMatchList = GetSyncMatchList(jObject);
                    if (syncMatchList is { Count: > 1 })
                    {
                        //获取最新匹配上的玩家
                        if (Root.Instance.MatchMap.TryGetValue(match_id, out var list))
                        {
                            foreach (var match in syncMatchList)
                            {
                                if (list.Exists(savedMatch => savedMatch.user_id == match.user_id))
                                {
                                    continue;
                                }

                                EventDispatcher.Root.Raise(GlobalEvent.Sync_New_Match, match);
                            }
                        }
                        else
                        {
                            foreach (var match in syncMatchList)
                            {
                                if (match.user_id != Root.Instance.Role.user_id)
                                {
                                    EventDispatcher.Root.Raise(GlobalEvent.Sync_New_Match, match);
                                }
                            }
                        }

                        Root.Instance.MatchMap[match_id] = syncMatchList;
                    }
                },
                GetBaseJson(ParamHandler)
            );
        }

        private static List<Match> GetSyncMatchList(JObject jObject)
        {
            var matchJson = jObject.SelectToken("data.matches").ToString();
            var syncMatchList = JsonConvert.DeserializeObject<List<Match>>(matchJson);
            return syncMatchList;
        }

        /// <summary>
        /// 服务器有超时判断， 游戏开始后， 超过1个小时算超时
        /// </summary>
        /// <param name="match_id"></param>
        /// <param name="room"></param>
        public void GameBegin(string match_id, Room room)
        {
            ParamHandler.Clear();
            ParamHandler["match_id"] = match_id;

            //记录任务进度
            Root.Instance.CurTaskInfo.RecordProgress();

            NetSystem.That.SendGameRequest(Proto.GAME_BEGIN,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    if (MediatorBingo.Instance.GridSeed.IsNullOrEmpty())
                    {
                        //游戏种子数据
                        string seed = jObject.SelectToken("data.table.seed_info.chess_board")?.ToString();
                        MediatorBingo.Instance.GridSeed = seed;
                    }

                    if (MediatorBingo.Instance.CallSeed.IsNullOrEmpty())
                    {
                        //游戏种子数据
                        string seed = jObject.SelectToken("data.table.seed_info.vote_numbers")?.ToString();
                        MediatorBingo.Instance.CallSeed = seed;
                    }
                    
                    if (MediatorBingo.Instance.Props.IsNullOrEmpty())
                    {
                        string propsList = jObject.SelectToken("data.table.props_list.props")?.ToString();
                        MediatorBingo.Instance.Props = propsList;
                    }

                    if (MediatorBingo.Instance.ChooseArray == null)
                    {
                        var randJson = jObject.SelectToken("data.table.props_list.rand_list")?.ToString();
                        if (randJson != null)
                            MediatorBingo.Instance.ChooseArray = JsonConvert.DeserializeObject<string[]>(randJson);
                    }

                    InitBingoData(match_id, room, out var data);

                    MediatorBingo.Instance.ClearMatchId();
                    MediatorBingo.Instance.ClearBingoBeginData();

                    SendGameBeginEventToServer(match_id, room);

                    UserInterfaceSystem.That.ShowUI<UIBingo>(new GameData()
                    {
                        ["bingoData"] = data,
                        ["fromRequest"] = true
                    });
                },
                GetBaseJson(ParamHandler)
            );
        }

        private void InitBingoData(string match_id, Room room, out BingoData data)
        {
            data = new BingoData();
            data.MatchCountAtLogin = Root.Instance.MatchCountAtLoginTime;
            
            data.GridSeed = MediatorBingo.Instance.GridSeed;
            data.CallSeed  = MediatorBingo.Instance.CallSeed;
            data.PropsSeed = MediatorBingo.Instance.Props;
            data.ChooseArray = MediatorBingo.Instance.ChooseArray;
            data.Style = room.Style;
            data.MatchId = match_id;
            data.CacheTime = TimeUtils.Instance.UtcTimeNow;

            PersistSystem.That.SaveValue(GlobalEnum.DB_YATZY, data, true);
        }

        private void SendGameBeginEventToServer(string matchID, Room room)
        {
            // 向通用后台发送事件
            var roomData = new RoomCommonData();
            roomData.room_id = room.id;
            roomData.event_name = "game_play";
            roomData.event_id = matchID;
            if (room.in_items != null)
            {
                foreach (var item in room.in_items)
                {
                    roomData.event_cost_type = item.Key.ToString();
                    roomData.event_cost = item.Value.ToString();
                }
            }

            var roomDataDic = new Dictionary<string, object>();
            roomDataDic["room_id"] = room.id;
            roomDataDic["event_id"] = matchID;

            if (room.in_items != null)
                foreach (var item in room.in_items)
                {
                    roomDataDic["event_cost_type"] = item.Key.ToString();
                    roomDataDic["event_cost"] = item.Value.ToString();
                }

#if RELEASE || LOG
            YZServerCommon.Shared.SendYZGameEvent(JsonMapper.ToJson(roomData));
#endif
            YZFunnelUtil.SendYZEvent("game_play", roomDataDic);
        }

        /// <summary>
        /// 初始化login data 请求gps信息
        /// </summary>
        /// <returns></returns>
        public LoginToServerData InitLoginData()
        {
            LoginToServerData data = new LoginToServerData();

            data.udid = DeviceInfoUtils.Instance.GetEquipmentId();
            data.timezone = YZNativeUtil.GetYZTimeZone();
            data.timestamp = TimeUtils.Instance.UtcTimeNow.ToString();
#if UNITY_STANDALONE
            data.bundle_id = "com.bingo.bliss";
#else
            data.bundle_id = Application.identifier;
#endif

            data.device_info = DeviceInfoUtils.Instance.GetDeviceInfo();
            data.language = DeviceInfoUtils.Instance.GetLanguage();
            data.version = YZNativeUtil.GetVersionCode();
            data.sim_info = YZNativeUtil.GetYZSimInfo();
            data.country = YZDataUtil.GetLocaling(YZConstUtil.YZCountryCode);
            data.ip = YZNativeUtil.GetIPAdress();
            data.appsflyer_id = YZNativeUtil.GetYZAFID();
            data.gps_extra = DeviceInfoUtils.Instance.GetGPSJson();

            YZDebug.Log("发送 URL = " + YZSDKsController.Shared.CurrentUrl);
            YZDebug.Log("Pay_App = " + YZSDKsController.Shared.Pay_App);

            if (YZSDKsController.Shared.CurrentUrl.Equals(Proto.VISITOR_LOGIN))
                data.pay_app = YZSDKsController.Shared.Pay_App;

            var gps = new JsonData
            {
                ["ISOcountryCode"] = YZDataUtil.GetLocaling(YZConstUtil.YZCountryCode),
                ["administrativeArea"] = YZDataUtil.GetLocaling(YZConstUtil.YZAreaCode)
            };

            data.gps = gps.ToJson();
            // data.picture = GetPictrue(testPictrue);

            if (!data.sim_info.IsNullOrEmpty() && data.sim_info != "siminfo")
            {
                try
                {
                    Dictionary<string, string> dicSim;
                    if (data.sim_info.Contains("isp1"))
                    { 
                        var simJ = JsonConvert.DeserializeObject(data.sim_info) as JObject;
                        dicSim = YZJsonUtil.DeserializeJObject<Dictionary<string, string>>("isp1", simJ);
                    }
                    else
                    {
                        dicSim = JsonConvert.DeserializeObject<Dictionary<string, string>>(data.sim_info);
                    }
                   
                    dicSim.TryGetValue("sim_country_code", out var simCountryCode);
                    if (simCountryCode == null)
                        simCountryCode = "";
                    
                    string country = YZDataUtil.GetLocaling(YZConstUtil.YZCountryCode).ToUpper();
                    if (data.language == "CN" || simCountryCode.ToUpper().Contains("CN") ||
                        country.Contains("CN") || country.Contains("IL") || simCountryCode.ToUpper().Contains("IL"))
                    {
                        if (YZSDKsController.Shared.IsBlockValid)
                        {
                            YZSDKsController.Shared.ClientBlock = true;

                            // data.language = "CN";
                        }
                    }
                    else
                    {
                        YZSDKsController.Shared.ClientBlock = false;
                    }
                }
                catch (Exception e)
                {
                    CarbonLogger.LogError("siminfo = "  +  data.sim_info);
                }
            }

            data.PropertyChanged += (sender, args) => { loginParam = GetAllParams(); };
        
            return data;
        }

        string GetTimeZone()
        {
            // return "America/Los_Angeles";
            return TimeZoneInfo.Local.Id;
        }

        string GetPictrue(string raw)
        {
            return raw.Replace("&", "%26");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matchID"></param>
        /// <param name="gameScore"></param>
        /// <param name="bingoCloseType">close_type：1-玩家提前结束，2-倒计时完了结束，3-游戏完成结束，4-服务器超时(客户端不用关心）</param>
        /// <param name="remainTime"></param>
        /// <param name="operations"></param>
        /// <param name="statistical"></param>
        /// <param name="callBack"></param>
        /// <param name="silence">是否触发事件</param>
        public void GameEnd(string matchID,
            int gameScore,
            BingoCloseType bingoCloseType,
            int remainTime = 0,
            List<int[]> operations = null,
            Dictionary<string, int> statistical = null,
            Action callBack = null,
            bool silence = false)
        {
            ParamHandler.Clear();
            ParamHandler["match_id"] = matchID;
            ParamHandler["game_score"] = gameScore;
            ParamHandler["remain_time"] = remainTime;
            ParamHandler["close_type"] = (int)bingoCloseType;
            paramHandler["gps_extra"] = DeviceInfoUtils.Instance.GetGPSJson();
            if (operations != null)
            {
                ParamHandler["operations"] = JsonConvert.SerializeObject(operations);
            }

            if (statistical != null)
            {
                ParamHandler["achieve"] = JsonConvert.SerializeObject(statistical);
            }

            NetSystem.That.SendGameRequest(Proto.GAME_END,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    // PersistSystem.That.DeletePrefsValue(GlobalEnum.LAST_GAME_ID, true);
                    MediatorBingo.Instance.ClearBingoDB();

                    SyncMatchList(matchID, jObject, out var syncMatchList);

                    if (syncMatchList != null)
                    {
                        foreach (var match in syncMatchList)
                        {
                            var history =
                                Root.Instance.MatchHistory.Find(history => history.match_id == match.match_id);
                            if (history != null)
                            {
                                history.status = match.status;
                            }
                        }
                    }
                    
                    Root.Instance.DailyMaxScore =
                        YZJsonUtil.DeserializeJObject<int>("data.max_game_score.room_daily_max_game_score", jObject);

                    Root.Instance.HistoryMaxScore =
                        YZJsonUtil.DeserializeJObject<int>("data.max_game_score.room_max_game_score", jObject);

                    Root.Instance.HistoryMaxType =
                        YZJsonUtil.DeserializeJObject<int>("data.max_game_score.get_max", jObject);

                    callBack?.Invoke();
                },
                GetBaseJson(ParamHandler), silence: silence
            );


            Root.Instance.GameRounds++;
        }

        private static void SyncMatchList(string matchID, JObject jObject, out List<Match> syncMatchList)
        {
            syncMatchList = GetSyncMatchList(jObject);
            if (syncMatchList is { Count: > 1 })
            {
                /*var tableID = syncMatchList[1].table_id;
                var roomID = syncMatchList[1].room_id;
                var room = Root.Instance.RoomList.Find(room1 => room1.id == roomID);
                room.MatchMap[tableID] = syncMatchList;*/

                Root.Instance.MatchMap[matchID] = syncMatchList;
            }
        }

        public void MatchClaim(List<MatchHistory> list, bool silence = false, bool forceSend = false, Action callback = null)
        {
            var matchIds = string.Join(",", list.Select(history => history.match_id));
            if (matchIds.IsNullOrEmpty())
            {
                return;
            }

            MatchClaim(matchIds, silence, forceSend, callback);
        }

        public void MatchClaim(string matchID, bool silence = false, bool forceSend = false, Action callback = null, bool showAll = false)
        {
            ParamHandler.Clear();
            ParamHandler["match_id"] = matchID;

            if (!YZDataUtil.GetLocaling(YZConstUtil.YZDailyFirstWinMatchId, "").IsNullOrEmpty()
                && matchID.Contains(YZDataUtil.GetLocaling(YZConstUtil.YZDailyFirstWinMatchId, "")))
                Root.Instance.IsNeedRequestNotification = true;
                            
            NetSystem.That.SendGameRequest(Proto.MATCH_CLIAM,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    var balance = jObject.SelectToken("data.balance").ToString();

                    Root.Instance.IsInduceReady = CheckInduce(balance);

                    SyncItem(balance, isShowAll: showAll);

                    var list = GetMatchHistories(jObject);
                    //更新相关状态
                    Root.Instance.MatchHistory =
                        Root.Instance.MatchHistory.ReplaceItems(list, history => history.id);

                    // Root.Instance.MatchHistoryMap[matchID] = list;
                    
                    EventDispatcher.Root.Raise(GlobalEvent.Sync_History);
                    
                    if (!silence)
                    {
                        //todo 看写在这里是否合适
                        UserInterfaceSystem.That.CloseAllUI(new[]
                            { nameof(UIMain), nameof(UIGetRewards), nameof(UIWaitNet) });
                    }
                    
                    callback?.Invoke();
                },
                GetBaseJson(ParamHandler),
                silence: silence,
                forceSend: forceSend
            );
        }

        public void TryPopInduceWindow()
        {
            // 不在历史记录界面不显示
            if (!UIMain.Shared().HistoryToggle.isOn)
                return;

            var lastShowTime = YZDataUtil.GetYZInt(YZConstUtil.YZInducePopTime, 0);
            if (TimeUtils.Instance.UtcTimeNow - lastShowTime > 600)
            {
                YZDataUtil.SetYZInt(YZConstUtil.YZInducePopTime, TimeUtils.Instance.UtcTimeNow);

                if (YZDataUtil.GetYZInt(YZConstUtil.YZInducePopDay, 0) != TimeUtils.Instance.Today)
                {
                    // 今天第一次弹
                    YZDataUtil.SetYZInt(YZConstUtil.YZInducePopDay, TimeUtils.Instance.Today);
                    YZDataUtil.SetYZInt(YZConstUtil.YZInducePopCount, 1);
                    TinyTimer.StartTimer(
                        () =>
                        {
                            UserInterfaceSystem.That.ShowQueue<UIChargeInduce>(Root.Instance.Role.GetDollars() < 1
                                ? 0
                                : 1);
                        }, 0.5f);
                }
                else
                {
                    // 不是第一次
                    int todayCount = YZDataUtil.GetYZInt(YZConstUtil.YZInducePopCount, 0);
                    if (todayCount < 3)
                    {
                        YZDataUtil.SetYZInt(YZConstUtil.YZInducePopCount, todayCount + 1);
                        TinyTimer.StartTimer(
                            () =>
                            {
                                UserInterfaceSystem.That.ShowQueue<UIChargeInduce>(
                                    Root.Instance.Role.GetDollars() < 1 ? 0 : 1);
                            }, 0.5f);
                    }
                }
            }
        }

        // 美金局诱导弹窗
        public bool CheckInduce(string balance)
        {
            if (Root.Instance.IsNaturalFlow)
                return false;

            var dic = JsonConvert.DeserializeObject<Dictionary<string, float>>(balance);
            bool haveChipsOrCoins = true;
            foreach (var item in Root.Instance.Role.Items)
            {
                if (item.id == Const.Bonus || item.id == Const.Cash)
                    //是否有美金奖励
                {
                    var success = dic.TryGetValue(item.name, out var syncValue);
                    if (success && item.Count != syncValue)
                    {
                        haveChipsOrCoins = false;
                        break;
                    }
                }
            }

            return haveChipsOrCoins;
        }

        public void MatchInfo(string matchID, Action callback = null, bool isFromGame = false , bool onlyGetInfo = false)
        {
            ParamHandler.Clear();
            ParamHandler["match_id"] = matchID;

            NetSystem.That.SendGameRequest(Proto.MATCH_INFO,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    var list = GetMatchHistories(jObject);

                    Root.Instance.MatchHistoryMap[matchID] = list;

                    //更新相关状态
                    Root.Instance.MatchHistory =
                        Root.Instance.MatchHistory.ReplaceItems(list, history => history.id);

                    Root.Instance.MatchHistoryMap[matchID] = GetMatchHistories(jObject, isAll: true);

                    EventDispatcher.Root.Raise(GlobalEvent.Sync_History);

                    callback?.Invoke();
                    if (!onlyGetInfo)
                    {
                        UserInterfaceSystem.That.ShowUI<UIGameResult>( new GameData()
                        {
                            ["matchId"] = matchID,
                            ["isFromGame"] = isFromGame,
                            ["matchTable"] = YZJsonUtil.DeserializeJObject<MatchTable>("data.table", jObject)
                        });
                    }
                },
                GetBaseJson(ParamHandler)
            );
        }

        private List<MatchHistory> GetMatchHistories(JObject jObject, string json = null, bool isAll = false)
        {
            var jsonstring = json ?? jObject.SelectToken("data.matches").ToString();
            //begin_time可能为null
            var list = JsonConvert.DeserializeObject<List<MatchHistory>>(jsonstring)
                .Where(history => isAll || history.user_id == Root.Instance.UserId)
                .ToList();
            return list;
        }

        bool CheckIsNewCursor(string newCursor, string oldCursor)
        {
            var splitNew = newCursor.Split('@');
            var splitOld = oldCursor.Split('@');
            //第二位数字的序号不是有序的
            if (splitNew[0].ToInt() <= splitOld[0].ToInt() && splitNew[1].ToInt() != splitOld[1].ToInt())
            {
                return true;
            }

            return false;
        }


        private HashSet<string> sendedCompleteCursor = new();
        private HashSet<string> sendedInCompleteCursor = new();

        //TODO 重复代码 优化
        /// <summary>
        /// 获取未完成的历史记录
        /// </summary>
        /// <param name="needNew"></param>
        public void GetInCompleteHistory(bool needNew = false, Action callBack = null)
        {
            ParamHandler.Clear();
            string sendCursor = null;

            if (!needNew)
            {
                sendCursor = Root.Instance.LastInCompleteCursor;
                if (!sendCursor.IsNullOrEmpty())
                {
                    if (sendedInCompleteCursor.Contains(sendCursor))
                    {
                        return;
                    }

                    ParamHandler["cursor"] = sendCursor;
                }
            }

            NetSystem.That.SendGameRequest(Proto.GET_INCOMPLETE_HISTORY,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    var historyJson = jObject.SelectToken("data.match_history").ToString();

                    var list = GetMatchHistories(jObject, historyJson);

                    if (!sendCursor.IsNullOrEmpty())
                    {
                        sendedInCompleteCursor.Add(sendCursor);
                    }

                    //失败的对局， 视为已完成， 放到已完成对局里， 避免重复拉取
                    Root.Instance.MatchHistory = Root.Instance.MatchHistory.ReplaceItems(
                        list,
                        history => history.id
                    );

                    var failList = Root.Instance.MatchHistory
                        .Where(history => !history.HavaReward && history.status == (int)Status.CanClime
                        ).ToList();

                    MatchClaim(failList, true, true);

                    //如果对局没有结束， 帮助玩家结束
                    // var notEndHistory = Root.Instance.MatchHistory
                    //     .Where(history => history.status == (int)Status.Game_Begin
                    //     ).ToList();
                    //
                    // var yatayCache = PersistSystem.That.GetValue<BingoData>(GlobalEnum.DB_YATZY) as BingoData;
                    // foreach (var history in notEndHistory)
                    // {
                    //     if (yatayCache != null && yatayCache.MatchId == history.match_id)
                    //     {
                    //         continue;
                    //     }
                    //
                    //     GameEnd(history.match_id, 0, BingoCloseType.EARLY_END, silence: true);
                    // }

                    //设置分页标志
                    var newCursor = jObject.SelectToken("data.cursor")?.ToString();

                    if (!newCursor.IsNullOrEmpty())
                    {
                        if (Root.Instance.LastInCompleteCursor.IsNullOrEmpty())
                        {
                            Root.Instance.LastInCompleteCursor = newCursor;
                        }
                        else
                        {
                            var isnew = CheckIsNewCursor(newCursor, Root.Instance.LastInCompleteCursor);
                            if (isnew)
                            {
                                Root.Instance.LastInCompleteCursor = newCursor;
                            }
                        }
                    }
                    else
                    {
                        Root.Instance.InCompleteRecordsOver = true;
                    }

                    callBack?.Invoke();
                    EventDispatcher.Root.Raise(GlobalEvent.Sync_History);
                },
                GetBaseJson(ParamHandler)
            );
        }

        /// <summary>
        /// 获取已完成的历史记录
        /// </summary>
        public void GetCompleteHistory(bool needNew = false, Action callBack = null)
        {
            ParamHandler.Clear();
            string sendCursor = null;
            if (!needNew)
            {
                sendCursor = Root.Instance.LastCompleteCursor;
                if (!sendCursor.IsNullOrEmpty())
                {
                    if (sendedCompleteCursor.Contains(sendCursor))
                    {
                        return;
                    }

                    ParamHandler["cursor"] = sendCursor;
                }
            }

            NetSystem.That.SendGameRequest(Proto.GET_COMPLETE_HISTORY,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    var matchList = jObject.SelectToken("data.match_history").ToString();

                    var receiveList = JsonConvert.DeserializeObject<List<MatchHistory>>(matchList);
                    if (!sendCursor.IsNullOrEmpty())
                    {
                        sendedCompleteCursor.Add(sendCursor);
                    }

                    Root.Instance.MatchHistory =
                        Root.Instance.MatchHistory.ReplaceItems(receiveList, history => history.id);


                    var newCursor = jObject.SelectToken("data.cursor")?.ToString();
                    if (!newCursor.IsNullOrEmpty())
                    {
                        if (Root.Instance.LastCompleteCursor.IsNullOrEmpty())
                        {
                            Root.Instance.LastCompleteCursor = newCursor;
                        }
                        else
                        {
                            var isnew = CheckIsNewCursor(newCursor, Root.Instance.LastCompleteCursor);
                            if (isnew)
                            {
                                Root.Instance.LastCompleteCursor = newCursor;
                            }
                        }
                    }
                    //拉不到分页标记就是结束
                    else
                    {
                        Root.Instance.CompleteRecordsOver = true;
                    }

                    callBack?.Invoke();
                    EventDispatcher.Root.Raise(GlobalEvent.Sync_User_Cash_Flow);
                },
                GetBaseJson(ParamHandler)
            );
        }

        public void GetOnlineRewardInfo(bool isAutoPop = false)
        {
            ParamHandler.Clear();
            NetSystem.That.SendGameRequest(Proto.GET_ONLINE_REWARD_INFO,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    Root.Instance.OnlineRewardInfo =
                        YZJsonUtil.DeserializeJObject<OnlineRewardInfo>("data.onlineActiveInfo", jObject);
                    // UserInterfaceSystem.That.ShowQueue<UIOnlineReward>(isAutoPop);
                },
                GetBaseJson(ParamHandler)
            );
        }

        public void GetOnlineRewardClaim()
        {
            ParamHandler.Clear();
            NetSystem.That.SendGameRequest(Proto.GET_ONLINE_REWARD_CLAIM,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    Root.Instance.OnlineRewardInfo =
                        YZJsonUtil.DeserializeJObject<OnlineRewardInfo>("data.onlineActiveInfo", jObject);
                    SyncItem(jObject.SelectToken("data.balance")?.ToString());
                },
                GetBaseJson(ParamHandler)
            );
        }

        public void WatchOnlineRewardAD()
        {
            ParamHandler.Clear();
            NetSystem.That.SendGameRequest(Proto.WATCH_AD,
                GlobalEnum.HttpRequestType.POST,
                (jObject) => { },
                GetBaseJson(ParamHandler)
            );
        }

        /// <summary>
        /// 观看广告房间
        /// </summary>
        public void WatchADRoomAD(int roomId, Action callback = null)
        {
            ParamHandler.Clear();
            ParamHandler["room_id"] = roomId;
            NetSystem.That.SendGameRequest(Proto.AD_ROOM_WATCHED,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    MatchBegin(roomId);
                    callback?.Invoke();
                },
                GetBaseJson(ParamHandler)
            );
        }

        public void GetVipInfo()
        {
            ParamHandler.Clear();
            NetSystem.That.SendGameRequest(Proto.GET_VIP_INFO,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    // OnlineRewardInfo 也包含vipLevel ?
                    // Root.Instance.Role.VipLevel = DeserializeJObject<int>("data.vipInfo.vip_level", jObject);

                    Root.Instance.Role.TotalCharge =
                        Math.Round(YZJsonUtil.DeserializeJObject<double>("data.vipInfo.success_total", jObject), 2);
                },
                GetBaseJson(ParamHandler)
            );
        }

        public void GetFreeWheelInfo()
        {
            ParamHandler.Clear();
            NetSystem.That.SendGameRequest(Proto.GET_FREE_WHEEL_INFO,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    Root.Instance.WheelFreeTicket =
                        YZJsonUtil.DeserializeJObject<int>("data.fortuneWheelInfo.wheel_free_ticket", jObject);

                    Root.Instance.FortuneWheelInfo =
                        YZJsonUtil.DeserializeJObject<FortuneWheelInfo>("data.fortuneWheelInfo", jObject);
                },
                GetBaseJson(ParamHandler)
            );
        }

        public void FreeWheel()
        {
            ParamHandler.Clear();
            var oldFreeTicket = Root.Instance.WheelFreeTicket;
            NetSystem.That.SendGameRequest(Proto.FREE_WHEEL,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    SyncItem(jObject, false, fireEvent: false);
                    var syncWheelFreeTicket =
                        YZJsonUtil.DeserializeJObject<int>("data.fortuneWheelInfo.wheel_free_ticket", jObject);
                    var order = YZJsonUtil.DeserializeJObject<int>("data.wheel_result.order", jObject);
                    bool isOneMore = false;
                    if (Root.Instance.FreeWheelList != null)
                    {
                        var type = Root.Instance.FreeWheelList[order - 1].type;
                        if (type >= Const.OneMoreTime && oldFreeTicket == syncWheelFreeTicket)
                        {
                            isOneMore = true;
                            syncWheelFreeTicket -= 5;
                        }
                    }

                    if (isOneMore)
                    {
                        Timer.Register(0.1f, () =>
                        {
                            //修改为假数据
                            EventDispatcher.Root.Raise(GlobalEvent.Change_Wheel_Free_Ticket, syncWheelFreeTicket);
                        });
                    }
                    else
                    {
                        Root.Instance.WheelFreeTicket = syncWheelFreeTicket;
                    }

                    EventDispatcher.Root.Raise(GlobalEvent.Start_Wheel, (order, WheelType.Free)
                    );
                },
                GetBaseJson(ParamHandler)
            );
        }

        /// <summary>
        /// charge_id 充值转盘的充值id
        /// </summary>
        /// <param name="chargeId"></param>
        public void PayWheel(int chargeId, WheelType wheelType)
        {
            ParamHandler.Clear();
            if (chargeId < 0)
            {
                return;
            }

            paramHandler["charge_id"] = chargeId;
            NetSystem.That.SendGameRequest(Proto.PAY_WHEEL,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    SyncItem(jObject, false, fireEvent: false);

                    Root.Instance.FortuneWheelInfo =
                        YZJsonUtil.DeserializeJObject<FortuneWheelInfo>("data.fortuneWheelInfo", jObject);

                    EventDispatcher.Root.Raise(GlobalEvent.Start_Wheel, (
                        YZJsonUtil.DeserializeJObject<int>("data.wheel_result.order", jObject), wheelType));
                },
                GetBaseJson(ParamHandler)
            );
        }

        public void GetSignInfo(bool isAutoPop = true)
        {
            ParamHandler.Clear();
            NetSystem.That.SendGameRequest(Proto.SIGN_INFO,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    Root.Instance.SignInfo =
                        YZJsonUtil.DeserializeJObject<SignInfo>("data.signInfo", jObject);
                    // 当天还没签到才弹窗
                    if (Root.Instance.SignInfo.sign_chance == 1)
                    {
                        UserInterfaceSystem.That.SingleTonQueue<UISign>(() =>
                        {
                            var topUI = UserInterfaceSystem.That.GetTopNormalUI();
                            if (topUI == null)
                            {
                                return false;
                            }

                            var uimain = topUI as UIMain;

                            //在uimain 且 不在博物馆页面

                            return uimain != null && !uimain.IsUIMainInAnimation && uimain.CollectionToggle.isOn != true;
                        });
                    }
                },
                GetBaseJson(ParamHandler)
            );
        }

        // ReSharper disable Unity.PerformanceAnalysis
        // ReSharper disable Unity.PerformanceAnalysis
        public void Sign()
        {
            var wheelTickOld = Root.Instance.WheelFreeTicket;
            ParamHandler.Clear();
            NetSystem.That.SendGameRequest(Proto.SIGN_SIGN,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    Root.Instance.SignInfo =
                        YZJsonUtil.DeserializeJObject<SignInfo>("data.signInfo", jObject);

                    var notifications = YZJsonUtil.DeserializeJObject<List<Notification>>("notification", jObject);
                    if (notifications != null && notifications.Count > 0)
                    {
                        var wheelTickNew = int.Parse(notifications[0].value);
                        int tikcNum = wheelTickNew - wheelTickOld;
                        if (tikcNum > 0)
                        {
                            Item tickItem = new Item();
                            tickItem.id = 800;
                            tickItem.Count = tikcNum;
                            List<Item> list = new List<Item>();
                            list.Add(tickItem);
                            SyncItem(jObject.SelectToken("data.balance")?.ToString(), out _,
                                true, list, 0.5f);
                        }
                    }
                    else
                    {
                        if (jObject.SelectToken("data.balance") != null)
                        {
                            var dic = JsonConvert.DeserializeObject<Dictionary<string, float>>(
                                jObject.SelectToken("data.balance").ToString());
                            float diffDollar = 0;
                            foreach (var item in Root.Instance.Role.Items)
                            {
                                var syncData = dic[item.name];
                                if (item.id is Const.Bonus or Const.Cash)
                                {
                                    if (syncData - item.Count > 0)
                                    {
                                        diffDollar += syncData - item.Count;
                                    }
                                }
                            }

                            //需要先同步数据, 但不fire event
                            SyncItem(jObject.SelectToken("data.balance")?.ToString(), showDiff: false,
                                fireEvent: false);

                            UISign.Instance?.FlyTodayReward();

                            if (diffDollar > 1.1f)
                            {
                                UISign.Instance?.FlyHeapReward();
                            }
                        }
                    }
                },
                GetBaseJson(ParamHandler)
            );
        }

        /// <summary>
        /// 上传新手引导进度
        /// </summary>
        /// <param name="newPlayerGuideStep"></param>
        public void SendNewPlayerGuideStep(NewPlayerGuideStep newPlayerGuideStep)
        {
            if (!Root.Instance.NotPassTheNewPlayerGuide(newPlayerGuideStep))
            {
                return; 
            }
            ParamHandler.Clear();
            var guideStepValue = (int)newPlayerGuideStep;
            ParamHandler["first_game_guide"] = guideStepValue;
            NetSystem.That.SendGameRequest(Proto.NEW_PLAYER_GUIDE_SUB,
                GlobalEnum.HttpRequestType.POST,
                (jObject) => { SyncItem(jObject.SelectToken("data.balance")?.ToString()); },
                GetBaseJson(ParamHandler),
                forceSend: true
            );
            Root.Instance.Role.match_first_game_guide = guideStepValue;

            var dic = new Dictionary<string, object>();
            dic["step"] = (int)newPlayerGuideStep;
            dic["name"] = newPlayerGuideStep.ToString();
            dic["is_closed"] = false;
            ThinkingAnalyticsAPI.Track("guide", dic);
        }

        /// <summary>
        /// 触发式引导
        /// </summary>
        /// <param name="guideStep"></param>
        public void SendTriggerGuideStep(TriggerGuideStep guideStep)
        {
            if (Root.Instance.UserId <= 0)
            {
                return;
            }
            ParamHandler.Clear();
            var guideStepValue = (int)guideStep;
            ParamHandler["guide"] = guideStepValue;
            NetSystem.That.SendGameRequest(Proto.TRIGGER_GUIDE_SUB,
                GlobalEnum.HttpRequestType.POST,
                (jObject) => { Root.Instance.GameGuideList = jObject.SelectToken("data.game_guide_list")?.ToString(); },
                GetBaseJson(ParamHandler),
                forceSend: true
            );
            //避免判定延迟
            Root.Instance.GameGuideList += "," + guideStepValue;
            var dic = new Dictionary<string, object>();
            dic["step"] = (int)guideStep;
            dic["name"] = guideStep.ToString();
            dic["is_closed"] = false;
            ThinkingAnalyticsAPI.Track("trigger_guide", dic);
        }

        private HashSet<string> randomName;

        public void GetRandomName()
        {
            ParamHandler.Clear();
            NetSystem.That.SendGameRequest(Proto.GET_RANDOW_NAME,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    randomName ??= new HashSet<string>();
                    var name = jObject.SelectToken("data.nickname")?.ToString();
                    randomName.Add(name);
                    EventDispatcher.Root.Raise(GlobalEvent.Sync_RandomName, name);
                },
                GetBaseJson(ParamHandler)
            );
        }

        public string GetNameExceptInput(string inputString = null)
        {
            if (randomName == null)
            {
                return default;
            }

            var values = randomName.ToArray();
            //找不到不同于输入的值
            if (values.Find(s => s != inputString).IsNullOrEmpty())
            {
                return default;
            }

            Random random = new Random();
            int randomIndex = random.Next(0, values.Length);
            while (values[randomIndex] == inputString)
            {
                randomIndex = random.Next(0, values.Length);
            }

            return values[randomIndex];
        }

        public void SubMitPlayerInfo(string name, Texture2D texture, int headIndex = 0)
        {
            ParamHandler.Clear();
            ParamHandler["nickname"] = name;
            ParamHandler["head_index"] = headIndex;
            ParamHandler["file"] = texture != null ? Convert.ToBase64String(texture.EncodeToPNG()) : "";
            // ParamHandler["wizard"] = wizard;
            NetSystem.That.SendGameRequest(Proto.USER_INFO_UPLOAD,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    var roleHeadURL = jObject.SelectToken("data.head_url")?.Value<string>();
                    if (texture != null)
                    {
                        ImageExt.SaveCache(roleHeadURL, texture);
                    }

                    Root.Instance.Role.head_url = roleHeadURL;

                    Root.Instance.Role.nickname = name;
                    Root.Instance.Role.head_index = headIndex;
                    EventDispatcher.Root.Raise(GlobalEvent.Sync_Role_Info);
                },
                GetBaseJson(ParamHandler)
            );
        }

        /// <summary>
        ///  绑定邮箱, 开始绑定没有 确认也不能再绑定了
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="callback"></param>
        public void BindMail(string email, string password, Action callback = null)
        {
            ParamHandler.Clear();
            ParamHandler["email"] = email;
            ParamHandler["password"] = password;
            NetSystem.That.SendGameRequest(Proto.BIND_EMAIL,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    //游戏内账号需要切换为正式账号
                    Root.Instance.UserInfo.email = email;
                    EventDispatcher.Root.Raise(GlobalEvent.Sync_Role_Info);
                    callback?.Invoke();
                },
                GetBaseJson(ParamHandler)
            );
        }

        public void BindVIPInfo(string email, string phone, Action callback = null)
        {
            ParamHandler.Clear();
            ParamHandler["email"] = email;
            ParamHandler["phone"] = phone;
            NetSystem.That.SendGameRequest(Proto.BIND_VIP_INFO,
                GlobalEnum.HttpRequestType.POST,
                (jObject) => { },
                GetBaseJson(ParamHandler)
            );
        }

        /// <summary>
        /// 重发确认邮件
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        public void ResendMail(string email, string password)
        {
            ParamHandler.Clear();
            ParamHandler["email"] = email;
            ParamHandler["password"] = password;
            NetSystem.That.SendGameRequest(Proto.RESEND_EMAIL,
                GlobalEnum.HttpRequestType.POST,
                (jObject) => { },
                GetBaseJson(ParamHandler)
            );
        }

        /// <summary>
        /// 重置密码
        /// </summary>
        /// <param name="email"></param>
        public void SendChangePsswordMail(string email)
        {
            ParamHandler.Clear();
            ParamHandler["email"] = email;
            ParamHandler["user_id"] = Root.Instance.UserId;
            NetSystem.That.SendGameRequest(Proto.CHANGE_PASSWORD,
                GlobalEnum.HttpRequestType.POST,
                (jObject) => { },
                GetBaseJson(ParamHandler)
            );
        }

        /// <summary>
        /// 邮箱登陆
        /// </summary>
        /// <param name="email"></param>
        public void EmailLogin(string email, string password)
        {
            ParamHandler.Clear();
            ParamHandler["email"] = email;
            ParamHandler["password"] = password;
            ParamHandler["bundle_id"] = Application.identifier;
            NetSystem.That.SendGameRequest(Proto.EMAIL_LOGIN,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    var authorization = jObject.SelectToken("data.authorization")?.Value<string>();
                    if (authorization.IsNullOrEmpty())
                    {
                        UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_http_code_1000"));
                        return;
                    }

                    //删除本地设备id
                    PersistSystem.That.DeletePrefsValue(GlobalEnum.ClientUID);
                    SaveToken(jObject);
                    LoginWithProcess(false);
                },
                GetBaseJson(ParamHandler)
            );
        }

        public void LoginWithProcess(bool onlyGetConfig = true)
        {
            // NetSystem.That.IsReLogin = true;
            //可能会多次重连
            UserInterfaceSystem.That.CloseAllUI(new[] { nameof(UILogin) });

            UserInterfaceSystem.That.ShowUI<UILogin>(LoginPanel.NormalLogin);
            if (onlyGetConfig)
            {
                GetConfigs(isShowUIMain: false, () =>
                {
                    // NetSystem.That.IsReLogin = false;
                });
            }
            else
            {
                ResetEnvironment();

                PlayerLogin(() => { NetSystem.That.IsReLogin = false; });
            }
        }

        public static void ResetEnvironment()
        {
            //这个是重启场景
            // Main.Instance.Restart();
            EventDispatcher.Root.Raise(GlobalEvent.RELOGIN);
            Root.Reset();
            MediatorActivity.Reset();
            MediatorGuide.Reset();
            MediatorItem.Reset();
            MediatorTask.Reset();
            MediatorBingo.Reset();
            GuideSystem.Reset();
            Reset();
            UserInterfaceSystem.That.Reset();
            NetSystem.That.Reset();
            YZSDKsController.Shared.Reset();
            // ResourceSystem.That.ForceUnloadAllAssets();
        }

        public void DeleteAccount(CancellationToken token)
        {
            ParamHandler.Clear();
            
            NetSystem.That.SendGameRequest(Proto.DELETE_ACCOUNT,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    UserInterfaceSystem.That.CloseAllUI(new[] { nameof(UIPlayerInfo) });
                    UserInterfaceSystem.That.ShowUI<UIWaitingCtrler>();
                    var name = Root.Instance.Role.nickname;
                    var headIndex = Root.Instance.Role.head_index;
                    ResetEnvironment();
                    Proto.ClearToken();
                    PlayerLogin(() => GetConfigs(callback: () =>
                    {
                        SubMitPlayerInfo(name, null, headIndex);
                        SendNewPlayerGuideStep(NewPlayerGuideStep.SECOND_BONUS_GAME);
                        //触发式引导 todo
                        AfterDelete(token);
                    }));
                    
                },
                GetBaseJson(ParamHandler)
            );
        }

        async UniTask AfterDelete(CancellationToken cancellationToken)
        {
            await UniTask.WaitUntil(() => Root.Instance.IsSetUp(), cancellationToken: cancellationToken);
            UserInterfaceSystem.That.ShowUI<UIMain>(new GameData()
            {
                ["dontPopWindows"] = false    
            });   
            UserInterfaceSystem.That.CloseAllUI(new[] { nameof(UIMain) });
        } 
                
        /// <summary>
        /// 拉取所有的notify
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        public void PUSH_NOTIFY()
        {
            ParamHandler.Clear();

            NetSystem.That.SendGameRequest(Proto.PUSH_NOTIFY,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    //YZDebug.Log(jObject.ToString());
                    if (jObject.SelectToken("data.message_ist") != null)
                    {
                        var msg_list = YZJsonUtil.DeserializeJObject<List<Message>>
                            ("data.message_ist", jObject);


                        if (msg_list != null)
                        {
                            foreach (var msg in msg_list)
                            {
                                switch (msg.type, msg.text)
                                {
                                    // 提现邮箱验证
                                    case ("hyper_validate_email", "validate_success"):
                                        YZDataUtil.SetYZInt(YZConstUtil.YZWithdrawMailVerified, 1);
                                        break;
                                    //绑定邮箱验证成功
                                    case ("email_verify", "email validate success"):
                                        EventDispatcher.Root.Raise("email validate success");
                                        UserInterfaceSystem.That.ShowUI<UIMailLoginConfirm>(MailLoginConfirmPanel
                                            .VerifiedGroup);
                                        Root.Instance.UserInfo.validata = 1;
                                        break;
                                }

                                MARK_NOTIFY(msg.id);
                            }
                        }
                    }
                },
                GetBaseJson(ParamHandler)
            );
        }


        /// <summary>
        /// 标记message已收到
        /// </summary>
        /// <param name="massage_id"></param>
        private void MARK_NOTIFY(int message_id)
        {
            ParamHandler.Clear();
            ParamHandler["message_id"] = message_id;
            NetSystem.That.SendGameRequest(Proto.MARK_NORIFY,
                GlobalEnum.HttpRequestType.POST,
                (jObject) => { },
                GetBaseJson(ParamHandler)
            );
        }

        /// <summary>
        /// 领取每日奖励
        /// </summary>
        public void ClaimDailyReward(int order, Action callBack = null)
        {
            ParamHandler.Clear();
            ParamHandler["order"] = order;
            NetSystem.That.SetFailCallBack(s =>
            {
                if (s.Contains("Received today"))
                {
                    Root.Instance.DailyRewardChance = 0;
                }
            });
            NetSystem.That.SendGameRequest(Proto.DAILY_REWARD_CLAIM,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    Root.Instance.DailyRewardChance = YZJsonUtil.DeserializeJObject<int>(
                        "data.shop_info.shop.shop_daily_reward_chance",
                        jObject);
                    SyncItem(jObject.SelectToken("data.balance")?.ToString());
                    callBack?.Invoke();
                },
                GetBaseJson(ParamHandler)
            );
        }

        /// <summary>
        /// 创建一个充值订单
        /// </summary>
        /// <param name="chargeId"></param>
        /// <param name="discountid"></param>
        /// <parm  name="discountAmount"></param>
        public void CreateCharegeOrder(int chargeId, int discountid, string discountAmount, string newPrice, 
            System.Action callback, Charge_configsItem chargeItem, string payment_type)
        {
            ParamHandler.Clear();
            ParamHandler["charge_id"] = chargeId;
            ParamHandler["discount_id"] = discountid;
            ParamHandler["discount_amount"] = discountAmount;

            var channel = "";
            if (payment_type == ChargeChannelType.CreditCard)
                channel = "glocash_credit_discount";
            else
                channel = "paypal_wallet_slave_discount";
            ParamHandler["channel"] = channel;
            ParamHandler["amount"] = newPrice;
            
            var gps = GetGPSJson();

            paramHandler["gps"] = gps.ToJson();
            paramHandler["session_id"] = DeviceInfoUtils.Instance.GetChargeSessionId();

            YZDebug.Log("CreateCharegeOrder Start");
            NetSystem.That.SendGameRequest(Proto.CREATE_CHARGE_ORDER,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    YZDebug.Log("CreateCharegeOrder Success");

                    UserInterfaceSystem.That.RemoveUIByName("UIWaitingCtrler");
                    //UIWaitingCtrler.Shared().YZOnCloseUI();
                    // 缓存订单数据
                    YZServerApiCharge.Shared.YZCurrentChargeID = chargeId;
                    var responceJson = JsonMapper.ToObject(jObject.ToString());
                    if (responceJson == null || !YZJsonUtil.ContainsYZKey(responceJson, "data") ||
                        !YZJsonUtil.ContainsYZKey(responceJson["data"], "charge_order"))
                    {
                        return;
                    }

                    YZServerApiCharge.Shared.brcurrentorderinfor = new JsonData();
                    // 往订单数据中填入用户ID
                    YZServerApiCharge.Shared.brcurrentorderinfor["app_user_id"] = Root.Instance.Role.user_id;

                    // app_order_id
                    YZServerApiCharge.Shared.brcurrentorderinfor["app_order_id"] =
                        jObject.SelectToken("data.charge_order.app_order_id").ToString();

                    // 货币金额
                    if (!string.IsNullOrEmpty(newPrice))
                    {
                        YZServerApiCharge.Shared.brcurrentorderinfor["amount"] = newPrice;
                    }
                    else if (jObject.SelectToken("data.charge_order.amount") != null)
                    {
                        YZServerApiCharge.Shared.brcurrentorderinfor["amount"] =
                            jObject.SelectToken("data.charge_order.amount").ToString();
                    }


                    // 往订单数据中填入货币信息
                    // 货币类型
                    if (!YZJsonUtil.ContainsYZKey(YZServerApiCharge.Shared.brcurrentorderinfor, "currency"))
                    {
                        YZServerApiCharge.Shared.brcurrentorderinfor["currency"] = "USD";
                    }

                    YZServerApiCharge.Shared.brcurrentorderinfor["callback_url"] =
                        jObject.SelectToken("data.charge_callback_url").ToString();

                    YZServerApiCharge.Shared.brcurrentorderinfor["charge_center_url"] =
                        jObject.SelectToken("data.charge_center_url").ToString();

#if UNITY_EDITOR
                    YZServerApiCharge.Shared.brcurrentorderinfor["country"] = "US";
#else
// 真实定位
                    YZServerApiCharge.Shared.brcurrentorderinfor["country"] =
                        YZDataUtil.GetLocaling(YZConstUtil.YZCountryCode, "");
#endif

                    // 充值打点：开始充值，提交订单时
                    Root.Instance.OrderStartTimeStamp = TimeUtils.Instance.UtcTimeNow;
                    int duration = Root.Instance.OrderStartTimeStamp - Root.Instance.SubmitChargeOrderTimeStamp;
                    // 充值打点：提交订单
                    Dictionary<string, object> properties = new Dictionary<string, object>()
                    {
                        { "payment_type", payment_type },
                        { "session_id", Root.Instance.SessionId },
                        { "duration", duration },
                        {
                            FunnelEventParam.brisfirstpay,
                            YZDataUtil.GetYZInt(YZConstUtil.YZIsLastDepositSuccess, 0) <= 0
                        },
                        { "pay_enter_name", chargeItem.position },
                        { "charge_id", chargeItem.id }
                    };
                    YZFunnelUtil.SendYZEvent("order_start", properties);

#if UNITY_ANDROID
                   YZAndroidPlugin.Shared.AndroidForterSendEvent("PAYMENT_INFO", properties.ToString());
#elif UNITY_IOS
                    iOSCShapeForterTool.Shared.IOSForterTrackAction(@"Payment_Info", "");
                    iOSCShapeRiskifiedTool.Shared.IOSRiskifiedLogRequest(@"Payment_Info");
#endif

                    Root.Instance.PaymentType = payment_type;

                    //call back
                    callback.Invoke();
                },
                GetBaseJson(ParamHandler)
            );
        }

        private JsonData GetGPSJson()
        {
            var gps = new JsonData();
#if UNITY_EDITOR
            gps["ISOcountryCode"] = "US";
            gps["administrativeArea"] = "NY";
#else
            // 真实定位
            gps["ISOcountryCode"] = YZDataUtil.GetLocaling(YZConstUtil.YZCountryCode, "");
            gps["administrativeArea"] = YZDataUtil.GetLocaling(YZConstUtil.YZAreaCode, "");
#endif
            return gps;
        }

        /// <summary>
        /// 查询可用的充值渠道
        /// </summary>
        public void GetChargeMethods(bool isActivityCharge = false, Charge_configsItem chargeConfigsItem = null,
            ActivityType activityType = ActivityType.None, bool silence = false)
        {
            ParamHandler.Clear();
#if UNITY_EDITOR
            ParamHandler["country"] = "US";
            paramHandler["state"] = "NY";
#else
            // 真实定位
            ParamHandler["country"] = YZDataUtil.GetLocaling(YZConstUtil.YZCountryCode, "");
            ParamHandler["state"] = YZDataUtil.GetLocaling(YZConstUtil.YZAreaCode, "");
#endif


            YZDebug.LogConcat("定位国家111: ", LocationManager.Shared.GetLocation().ISOcountryCode);
            YZDebug.LogConcat("定位州111: ", LocationManager.Shared.GetLocation().administrativeArea);

            void ConfirmCallBack()
            {
                UserInterfaceSystem.That.RemoveUIByName(nameof(UIChargeHintConfirm));
                UserInterfaceSystem.That.ShowUI<UIChargeCtrl>(chargeConfigsItem);
                if (activityType is ActivityType.RoomCharge)
                {
                    EventDispatcher.Root.Raise(GlobalEvent.ROOM_CHARGE);
                }
            }

            NetSystem.That.SendGameRequest(Proto.GET_CHARGE_METHODS,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    Root.Instance.ChargeMethodsList = YZJsonUtil.DeserializeJObject<List<ChargeMethods>>
                        ("data.charge_methods", jObject);

                    if (isActivityCharge)
                    {
                        if (jObject.SelectToken("data.charge_restrict_type") != null)
                        {
                            int restricType = jObject.SelectToken("data.charge_restrict_type").Value<int>();

                            if (restricType != 0)
                            {
                                if (restricType == 1)
                                {
                                    UserInterfaceSystem.That.ShowUI<UIChargeHintConfirm>(new UIChargeHintConfirmData()
                                    {
                                        Type = UIChargeHintConfirmData.UIChargeHintConfirmType.TwoBtn,
                                        HideCloseBtn = false,
                                        desc = I18N.Get("key_charge_hint1"),
                                        confirmTitle = I18N.Get("key_deposit"),
                                        cancelTitle = "Maybe Later",
                                        WaitCloseCallback = true,
                                        confirmCall = ConfirmCallBack,
                                        cancleCall = () =>
                                        {
                                            UserInterfaceSystem.That.RemoveUIByName(nameof(UIChargeHintConfirm));
                                        }
                                    });
                                    
                                }
                                else if (restricType == 2)
                                {
                                    UserInterfaceSystem.That.ShowUI<UIChargeHintConfirm>(new UIChargeHintConfirmData()
                                    {
                                        Type = UIChargeHintConfirmData.UIChargeHintConfirmType.TwoBtn,
                                        HideCloseBtn = false,
                                        desc = I18N.Get("key_charge_hint2"),
                                        confirmTitle = I18N.Get("key_deposit"),
                                        cancelTitle = "Maybe Later",
                                        WaitCloseCallback = true,
                                        confirmCall = ConfirmCallBack,
                                        cancleCall = () =>
                                        {
                                            UserInterfaceSystem.That.RemoveUIByName(nameof(UIChargeHintConfirm));
                                        }
                                    });
                                    
                                }
                                else
                                {
                                    UserInterfaceSystem.That.ShowUI<UIChargeHint>(restricType);
                                }
                                YZFunnelUtil.SendYZEvent(FunnelEventID.brchargeactivityshow,
                                    new Dictionary<string, object>()
                                    {
                                        { "activity_ID", 5000 + restricType }
                                    });
                            }
                            else
                            {
                                UserInterfaceSystem.That.ShowUI<UIChargeCtrl>(chargeConfigsItem);
                                if (activityType == ActivityType.RoomCharge)
                                {
                                    EventDispatcher.Root.Raise(GlobalEvent.ROOM_CHARGE);
                                }
                                
                                YZFunnelUtil.SendYZEvent(FunnelEventID.brchargeactivityshow,
                                    new Dictionary<string, object>()
                                    {
                                        { "activity_ID", (int)activityType },
                                        { "is_suggest", MediatorActivity.Instance.IsSuggest(activityType) },
                                        { "count_down", MediatorActivity.Instance.GetActivityLessTime(activityType) },
                                        { "remain_times", MediatorActivity.Instance.GetRemainTimes(activityType) },
                                    });
                            }
                        }
                    }
                },
                GetBaseJson(ParamHandler),
                silence
            );
        }

        /// <summary>
        /// 请求查询订单状态
        /// </summary>
        public void QueryOrder(string order_id)
        {
            if (Root.Instance.IsQuerying)
                return;
            
            Root.Instance.IsQuerying = true;
            
            ParamHandler.Clear();
            paramHandler["app_order_id"] = order_id;
            NetSystem.That.SetFailCallBack(s =>
            {
                var o = JsonConvert.DeserializeObject(s);
                int unKnow = 0;
                int errorCode = unKnow;
                string error_msg = "";
                if (o is JObject jObject)
                {
                     errorCode = YZJsonUtil.DeserializeJObject<int>("code", jObject);
                    var errorStr = YZJsonUtil.DeserializeJObject<string[]>("errors",jObject);
            
                    if (errorStr is {Length: > 0})
                    {
                        error_msg = errorStr[0];
                    }
                    
                }
              
                Send_order_result(error_code: errorCode , error_msg: error_msg);
            });
            NetSystem.That.SendGameRequest(Proto.CHARGE_ORDER_QUERY, GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    UserInterfaceSystem.That.RemoveUIByName("UIWaitingCtrler");
                    Root.Instance.IsQuerying = false;
                    var data = jObject.SelectToken("data");
                    var queryInfo = data["charge_order"];
                    var status = queryInfo.SelectToken("status").Value<int>();

                    // ECheck 检查
                    if (queryInfo.SelectToken("type") != null && queryInfo.SelectToken("fail_content") != null)
                    {
                        var sourceName = queryInfo.SelectToken("type").Value<string>();
                        var failContent = YZJsonUtil.DeserializeJObject<
                            Dictionary<string, object>>("data.charge_order.fail_content", jObject);
                        if (failContent != null && failContent.ContainsKey("name"))
                        {
                            if (failContent["name"].Equals("ECHECK") && sourceName.Equals("paypal_wallet_slave"))
                            {
                                // 不支持 ECheck，弹出弹窗并返回
                                UserInterfaceSystem.That.ShowUI<UIChargeError>("key_deposit_fail", "key_echeck");
                                return;
                            }
                        }
                    }
                    
                    var appData = data["app_data"];
                    string cardBin = appData == null ? "" : appData.SelectToken("card_bin").Value<string>();
                    int cardId = appData == null ? 0 : appData.SelectToken("card_id").Value<int>();

                    Send_order_result(cardBin, cardId, status);

                    if (status == 3)
                    {
                        // 充值成功过一次
                        YZDataUtil.SetYZInt(YZConstUtil.YZIsLastDepositSuccess, 1);

                        var chargeId = queryInfo.SelectToken("charge_id").Value<string>();

                        YZDebug.Log("app_order_id = " + order_id);

#if RELEASE || LOG
                        //-- 1.回传通用后台
                        if (queryInfo.SelectToken("net_amount") != null && !YZDebug.IsWhiteListTestDevice())
                        {
                            float revenue = float.Parse(queryInfo.SelectToken("net_amount").Value<string>());
                            // 内购事件
                            YZServerCommon.Shared.SendYZPurchaseWorth(
                                order_id,
                                "Deposit", "1", queryInfo.SelectToken("net_amount").Value<string>());
                            // firebase
                            var sourceName = queryInfo.SelectToken("type").Value<string>();
                            int type = sourceName.Equals("glocash_credit_discount") ? 1 : 2;
                            YZFirebaseController.Shared.TrackPurchaseEvent(type, revenue);
                        }
#endif

                        // var eventData = new Dictionary<string, object>();
                        // eventData["charge_id"] = chargeId;
                        // eventData["app_order_id"] = order_id;

                        //YZFunnelUtil.SendYZEvent("purchase_success", eventData);
                        //YZFunnelUtil.SendYZAppsflyerValue("purchase_success", queryInfo.SelectToken("amount").Value<string>());

                        // 关闭UI
                        UserInterfaceSystem.That.RemoveUIByName("UIChargeCardInfo");
                        UserInterfaceSystem.That.RemoveUIByName("UIChargeCtrl");
                        UserInterfaceSystem.That.RemoveUIByName("UIChargeWebView");


                        Root.Instance.ChargeInfo =
                            YZJsonUtil.DeserializeJObject<ChargeInfo>("data.chargeInfo", jObject);

                        if (Root.Instance.ChargeInfo is { success_count: 0 })
                        {
                            Root.Instance.ChargeInfo.success_count++;
                        }

                        //-- 2.更新数据
                        SyncItem(jObject.SelectToken("data.balance")?.ToString());

                        EventDispatcher.Root.Raise(GlobalEvent.CHARGE_SUCCESS, chargeId);
                    }
                }, GetBaseJson(paramHandler));
        }

        private void Send_order_result(string cardBin = "error", int cardId = -1, int status = -1,
         int error_code = -1, string error_msg = ""
        )
        {
            // 充值打点：充值结果 客户端获取到充值回调
            var chargeItem = UIChargeCtrl.Shared().ChargeItem;
            Dictionary<string, object> properties = new Dictionary<string, object>()
            {
                { "af_id", YZNativeUtil.GetYZAFID() },
                { "card_user_name", Root.Instance.CardUserName },
                { "card_bin", cardBin },
                { "card_id", cardId },
                { "payment_type", Root.Instance.PaymentType },
                { "session_id", Root.Instance.SessionId },
                { "duration", TimeUtils.Instance.UtcTimeNow - Root.Instance.OrderStartTimeStamp },
                {
                    FunnelEventParam.brisfirstpay,
                    YZDataUtil.GetYZInt(YZConstUtil.YZIsLastDepositSuccess, 0) <= 0
                },
                { "pay_enter_name", chargeItem.position },
                { "charge_id", chargeItem.id },
                { "is_true", status == 3 },
#if UNITY_ANDROID
                         { "ip", YZAndroidPlugin.Shared.AndroidGetLastIP()},
#endif
                { "ip_info", DeviceInfoUtils.Instance.GetIpInfoData() },
                { "gps_camouflage", DeviceInfoUtils.Instance.SelfGPSExtra.gps_camouflage },
                { "gps_reject", DeviceInfoUtils.Instance.SelfGPSExtra.gps_reject },
                { "gps", DeviceInfoUtils.Instance.SelfGPSExtra.gps },
                { "gps_info", DeviceInfoUtils.Instance.GetGPSInfoData() },
                { "error_code", error_code },
                { "error_msg", error_msg }
            };
            YZFunnelUtil.SendYZEvent("order_result", properties);
        }

        /// <summary>
        /// 提现-绑定手机号
        /// </summary>
        public void WithdrawBindPhoneStart()
        {
        }

        /// /// <summary>
        /// 提现-手机号验证
        /// </summary>
        public void WithdrawPhoneVerify()
        {
        }

        public void GetCashFlow()
        {
            UserInterfaceSystem.That.ShowUI<UIWaitingCtrler>();
            ParamHandler.Clear();
            NetSystem.That.SetFailCallBack(content =>
            {
                // 获取流水失败
                UserInterfaceSystem.That.RemoveUIByName("UIWaitingCtrler");
                UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("lost_connect"));
                UserInterfaceSystem.That.RemoveUIByName("UIWithdrawDetails");
            });
            NetSystem.That.SendGameRequest(Proto.HyperCashFlow,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    UserInterfaceSystem.That.RemoveUIByName("UIWaitingCtrler");
                    Root.Instance.WithdrawCashFlow =
                        jObject.SelectToken("data.cash_enter_fee_sum_after_cash").Value<float>();
                    Root.Instance.WithdrawToday = jObject.SelectToken("data.cash_today_total").Value<float>();
                    Root.Instance.WithdrawValid = true;
                    //YZDebug.Log(str);

                    EventDispatcher.Root.Raise(GlobalEvent.Withdraw_Detail);
                },
                GetBaseJson(ParamHandler)
            );
        }

        public void WithdrawBindEmail(string email)
        {
            UserInterfaceSystem.That.ShowUI<UIWaitingCtrler>();
            ParamHandler.Clear();
            paramHandler["email"] = email;
            NetSystem.That.SetFailCallBack(content => { UserInterfaceSystem.That.RemoveUIByName("UIWaitingCtrler"); });
            NetSystem.That.SendGameRequest(Proto.HYPER_RIGISTER,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    UserInterfaceSystem.That.RemoveUIByName("UIWaitingCtrler");
                    var data = jObject.SelectToken("data");
                    if (data != null && data.SelectToken("validate") != null)
                    {
                        var validate = jObject.SelectToken("data.validate").Value<int>();
                        var email = jObject.SelectToken("data.email")?.Value<string>();

                        YZDataUtil.SetYZInt(YZConstUtil.YZWithdrawMailVerified, validate);
                        if (!email.IsNullOrEmpty())
                            YZDataUtil.SetYZString(YZConstUtil.YZWithdrawEmail, email);

                        UserInterfaceSystem.That.ShowUI<UIWithdrawVerifyEmail>();
                        //YZDebug.Log(str);
                    }
                    else if (data != null && data.SelectToken("validate") == null && data.SelectToken("wait") != null)
                    {
                        // 提交了不变的邮箱，那么直接打开下一个界面
                        UserInterfaceSystem.That.ShowUI<UIWithdrawVerifyEmail>();
                    }
                },
                GetBaseJson(ParamHandler)
            );
        }

        public void WithdrawResendEmail(string email)
        {
            UserInterfaceSystem.That.ShowUI<UIWaitingCtrler>();
            ParamHandler.Clear();
            paramHandler["email"] = email;
            NetSystem.That.SetFailCallBack(content => { UserInterfaceSystem.That.RemoveUIByName("UIWaitingCtrler"); });

            NetSystem.That.SendGameRequest(Proto.HyperResend,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    UserInterfaceSystem.That.RemoveUIByName("UIWaitingCtrler");
                    var hyperWallet = YZJsonUtil.DeserializeJObject<Hyper_wallet>
                        ("data.Hyper_wallet", jObject);

                    YZDataUtil.SetYZInt(YZConstUtil.YZWithdrawMailVerified, 0);
                    if (!email.IsNullOrEmpty())
                        YZDataUtil.SetYZString(YZConstUtil.YZWithdrawEmail, hyperWallet.email);
                    //YZDebug.Log(str);
                },
                GetBaseJson(ParamHandler)
            );
        }

        public void WithdrawSendInfo(string country, string state, string city, string addresslin1, string addresslin2,
            string zipcode, string firstName, string lastName, string birthday)
        {
            UserInterfaceSystem.That.ShowUI<UIWaitingCtrler>();
            ParamHandler.Clear();
            ParamHandler["country"] = country;
            ParamHandler["province"] = state;
            ParamHandler["city"] = city;
            ParamHandler["address_line1"] = addresslin1;
            ParamHandler["address_line2"] = addresslin2;
            ParamHandler["postal_code"] = zipcode;
            ParamHandler["first_name"] = firstName;
            ParamHandler["last_name"] = lastName;
            ParamHandler["birthday"] = birthday;
            NetSystem.That.SetFailCallBack(content =>
            {
                UserInterfaceSystem.That.RemoveUIByName("UIWaitingCtrler");
                UserInterfaceSystem.That.ShowUI<UIWithdrawVerifyFail>();
            });
            NetSystem.That.SendGameRequest(Proto.HyperCommit,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    UserInterfaceSystem.That.RemoveUIByName("UIWaitingCtrler");

                    YZDataUtil.SetYZInt(YZConstUtil.YZWithdrawNameAndAddressVerified, 1);

                    WithdrawApply(Root.Instance.WithdrawAmount);
                },
                GetBaseJson(ParamHandler)
            );
        }

        public void WithdrawApply(string Amount)
        {
            UserInterfaceSystem.That.ShowUI<UIWaitingCtrler>();
            ParamHandler.Clear();
            paramHandler["amount"] = Amount;
            paramHandler["email"] = YZDataUtil.GetLocaling(YZConstUtil.YZWithdrawEmail);
            paramHandler["user_name"] = YZDataUtil.GetLocaling(YZConstUtil.YZWithdrawFirstName) + " " + 
                                        YZDataUtil.GetLocaling(YZConstUtil.YZWithdrawLastName);

            NetSystem.That.SetFailCallBack(content => { UserInterfaceSystem.That.RemoveUIByName("UIWaitingCtrler"); });
            NetSystem.That.SendGameRequest(Proto.HyperApply,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    UserInterfaceSystem.That.RemoveUIByName("UIWaitingCtrler");

                    // 打开提现记录
                    WithdrawHistory();

                    if (jObject.SelectToken("data.balance") != null)
                        SyncItem(jObject.SelectToken("data.balance")?.ToString(), isShowAll: true);
                    
                    RefreshWithDraw();
                },
                GetBaseJson(ParamHandler)
            );
        }

        public void RefreshWithDraw()
        {
            // 更新对局记录
            GetCompleteHistory(true);
            GetInCompleteHistory(true);

            // 更新博物馆info
            ParamHandler.Clear();

            NetSystem.That.SendGameRequest(Proto.MUSUM_INFO,
                GlobalEnum.HttpRequestType.POST,
                (jObject1) =>
                {
                    var museumInfo = YZJsonUtil.DeserializeJObject<MuseumInfo>("data.museumInfo", jObject1);
                    if (museumInfo != null)
                    {
                        museumInfo.dont_sync_add = true;
                        Root.Instance.MuseumInfo = museumInfo;
                    }
                },
                GetBaseJson(ParamHandler));

            // 更新巫师秘宝
            ParamHandler.Clear();
            NetSystem.That.SendGameRequest(Proto.MAGIC_BALL_INFO,
                GlobalEnum.HttpRequestType.POST,
                (jObject2) =>
                {
                    var magicBallInfo = YZJsonUtil.DeserializeJObject<MagicBallInfo>("data.wizardInfo", jObject2);
                    if (magicBallInfo != null)
                    {
                        magicBallInfo.dont_sync_add = true;
                        Root.Instance.MagicBallInfo = magicBallInfo;
                    }
                   
                    EventDispatcher.Root.Raise(GlobalEvent.Refresh_Room_List);
                },
                GetBaseJson(ParamHandler));
        }

        public void WithdrawHistory(bool openWithdrawRecord = true, Action callback = null)
        {
            UserInterfaceSystem.That.ShowUI<UIWaitingCtrler>();
            ParamHandler.Clear();
            NetSystem.That.SetFailCallBack(content => { UserInterfaceSystem.That.RemoveUIByName("UIWaitingCtrler"); });
            NetSystem.That.SendGameRequest(Proto.HyperHistory,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    UserInterfaceSystem.That.RemoveUIByName("UIWaitingCtrler");
                    
                    if (jObject.SelectToken("data.cash_history") != null)
                    {
                        
                        var orders = YZJsonUtil.DeserializeJObject<List<WithdrawHistoryData>>
                            ("data.cash_history", jObject);

                        Root.Instance.WithdrawHistoryData = orders;
                        if (openWithdrawRecord)
                        {
                            // 打开提现记录
                            UserInterfaceSystem.That.ShowUI<UIWithdrawRecord>(orders);
                        }
                        callback?.Invoke();
                    }
                    //YZDebug.Log(jObject.ToString());
                },
                GetBaseJson(ParamHandler)
            );
        }

        public void SendThinkingSessionId()
        {
            ParamHandler.Clear();
            ParamHandler["ta_udid"] = Root.Instance.SessionId;
            paramHandler["udid"] = DeviceInfoUtils.Instance.GetEquipmentId();
            NetSystem.That.SendGameRequest(Proto.Thinking_Session_ID,
                GlobalEnum.HttpRequestType.POST, (jObject) => { }, GetBaseJson(ParamHandler));
        }

        private void GMSetOrganic()
        {
            ParamHandler.Clear();

            //ParamHandler["is_organic"] = Root.Instance.IsSetNoNatural() ? 0 : 1;

            paramHandler["is_organic"] = YZSDKsController.Shared.AF_ORGANIC ? 1 : 0;
            YZDebug.Log("YZSDKsController.Shared.AF_ORGANIC = " + YZSDKsController.Shared.AF_ORGANIC);

            EventDispatcher.Root.Raise(GlobalEvent.SEND_GM_SET_ORGINIC);

            NetSystem.That.SendGameRequest(Proto.GM_SET_ORGANIC,
                GlobalEnum.HttpRequestType.POST, (jObject) =>
                {
                    if (jObject.SelectToken("data.is_organic") != null)
                    {
                        if (Root.Instance.UserInfo != null)
                        {
                            Root.Instance.UserInfo.is_organic = jObject.SelectToken("data.is_organic").Value<int>();
                            // EventDispatcher.Root.Raise(GlobalEvent.Refresh_Room_List);
                            
                            if (Root.Instance.UserInfo.is_organic == 0)
                            {
                                YZDataUtil.SetYZInt(YZConstUtil.YZEverNotOrganic, 1);
                            }
                        }
                    }
                }, GetBaseJson(ParamHandler));
        }

        public void GetShopInfo()
        {
            ParamHandler.Clear();
            NetSystem.That.SendGameRequest(Proto.GET_SHOP_INFO,
                GlobalEnum.HttpRequestType.POST, (jObject) =>
                {
                    Root.Instance.ShopConfig =
                        YZJsonUtil.DeserializeJObject<List<ShopInfo>>("data.shopInfo.shop", jObject);
                }, GetBaseJson(ParamHandler));
        }

        public void PushNewStartPackerGear(int level)
        {
            ParamHandler.Clear();
            ParamHandler["level"] = level;
            NetSystem.That.SendGameRequest(Proto.PUSH_NEW_STARTPACKER_GEAR,
                GlobalEnum.HttpRequestType.POST, (jObject) =>
                {
                    Root.Instance.StarterPackInfo =
                        YZJsonUtil.DeserializeJObject<StarterPackInfo>("data.starterPackInfo", jObject);
                }, GetBaseJson(ParamHandler));
        }

        public void UpdateLuckyCardInfo()
        {
            ParamHandler.Clear();
            NetSystem.That.SendGameRequest(Proto.LUCKY_CARD_INFO,
                GlobalEnum.HttpRequestType.POST, (jObject) =>
                {
                    Root.Instance.Role.luckyCardInfo =
                        YZJsonUtil.DeserializeJObject<LuckyCardInfo>("data.luckyCardInfo", jObject);

                    if (Root.Instance.Role.luckyCardInfo != null)
                        Root.Instance.Role.luckyCardInfo.end_timestamp =
                            Root.Instance.Role.luckyCardInfo.lucky_card_begin_time + 24 * 3600;

                    EventDispatcher.Root.Raise(GlobalEvent.Sync_LuckyCard);
                }, GetBaseJson(ParamHandler));
        }

        public void SendChooseLuckyCard(int position, int id)
        {
            ParamHandler.Clear();
            paramHandler["position"] = position;
            paramHandler["id"] = id;
            NetSystem.That.SendGameRequest(Proto.LUCKY_CARD_CHOOSE,
                GlobalEnum.HttpRequestType.POST, (jObject) =>
                {
                    Root.Instance.Role.luckyCardInfo =
                        YZJsonUtil.DeserializeJObject<LuckyCardInfo>("data.luckyCardInfo", jObject);

                    if (Root.Instance.Role.luckyCardInfo != null)
                        Root.Instance.Role.luckyCardInfo.end_timestamp =
                            Root.Instance.Role.luckyCardInfo.lucky_card_begin_time + 24 * 3600;

                    Root.Instance.CanLuckyCardClick = true;

                    EventDispatcher.Root.Raise(GlobalEvent.Sync_LuckyCard);
                }, GetBaseJson(ParamHandler));
        }

        public void GMAddMoney(int type, float amount)
        {
            ParamHandler.Clear();
            paramHandler["type"] = type;
            paramHandler["amount"] = amount;
            NetSystem.That.SendGameRequest(Proto.GM_ADD_MONEY,
                GlobalEnum.HttpRequestType.POST, (jObject) => { SyncItem(jObject, onlyAnimation: true); },
                GetBaseJson(ParamHandler));
        }

        public void UpdateDragonInfo()
        {
            ParamHandler.Clear();
            NetSystem.That.SendGameRequest(Proto.DRAGON_INFO,
                GlobalEnum.HttpRequestType.POST, (jObject) =>
                {
                    Root.Instance.Role.dragonInfo = YZJsonUtil.DeserializeJObject<DragonInfo>(
                        "data.oneStopInfo", jObject);
                    if (Root.Instance.Role.dragonInfo != null)
                        Root.Instance.Role.dragonInfo.end_timestamp =
                            Root.Instance.Role.dragonInfo.one_stop_begin_time + 24 * 3600;

                    EventDispatcher.Root.Raise(GlobalEvent.Sync_Dragon);
                }, GetBaseJson(ParamHandler));
        }


        public void GetStarterPackInfo()
        {
            ParamHandler.Clear();
            NetSystem.That.SendGameRequest(Proto.STARTER_PACK_INFO,
                GlobalEnum.HttpRequestType.POST, (jObject) =>
                {
                    Root.Instance.StarterPackInfo =
                        YZJsonUtil.DeserializeJObject<StarterPackInfo>("data.starterPackInfo", jObject);
                }, GetBaseJson(ParamHandler));
        }

        public void ClaimDragon(int index)
        {
            ParamHandler.Clear();
            paramHandler["index"] = index;
            NetSystem.That.SendGameRequest(Proto.DRAGON_CLAIM,
                GlobalEnum.HttpRequestType.POST, (jObject) =>
                {
                    Root.Instance.Role.dragonInfo = YZJsonUtil.DeserializeJObject<DragonInfo>(
                        "data.oneStopInfo", jObject);
                    if (Root.Instance.Role.dragonInfo != null)
                        Root.Instance.Role.dragonInfo.end_timestamp =
                            Root.Instance.Role.dragonInfo.one_stop_begin_time + 24 * 3600;

                    //需要先同步数据, 但不fire event
                    SyncItem(jObject.SelectToken("data.balance")?.ToString(), showDiff: true,
                        fireEvent: true);

                    TinyTimer.StartTimer(() => { EventDispatcher.Root.Raise(GlobalEvent.DragonGetSuccess); }, 0.5f);
                }, GetBaseJson(ParamHandler));
        }

        public void UpdateSpecialGiftInfo()
        {
            ParamHandler.Clear();
            NetSystem.That.SendGameRequest(Proto.SPECIAL_GIFT,
                GlobalEnum.HttpRequestType.POST, (jObject) =>
                {
                    Root.Instance.Role.specialGiftInfo = YZJsonUtil.DeserializeJObject<SpecialGiftInfo>(
                        "data.specialGiftInfo", jObject);

                    if (Root.Instance.Role.specialGiftInfo != null)
                    {
                        var spLessTime = TimeUtils.Instance.EndDayTimeStamp -
                                         Root.Instance.Role.specialGiftInfo.special_gift_create_time;
                        if (spLessTime / 3600 <= 24 && spLessTime > 0)
                            Root.Instance.Role.specialGiftInfo.special_gift_end_time =
                                TimeUtils.Instance.EndDayTimeStamp;
                    }

                    EventDispatcher.Root.Raise(GlobalEvent.Sync_Special_Gift);
                }, GetBaseJson(ParamHandler));
        }

        public void GetSpecialOfferInfo()
        {
            ParamHandler.Clear();
            NetSystem.That.SendGameRequest(Proto.SPECIAL_OFFER,
                GlobalEnum.HttpRequestType.POST, (jObject) =>
                {
                    Root.Instance.Role.SpecialOfferInfo = YZJsonUtil.DeserializeJObject<SpecialOfferInfo>(
                        "data.specialOfferInfo", jObject);

                    EventDispatcher.Root.Raise(GlobalEvent.Sync_Special_Offer);
                    
                    // 跨天弹出
                    var lessTime = Root.Instance.Role.SpecialOfferInfo.show_time + 3600 - TimeUtils.Instance.UtcTimeNow;
                    if (Root.Instance.Role.SpecialOfferInfo != null &&
                        Root.Instance.Role.SpecialOfferInfo.show_time > 0 && 
                        lessTime > 0 &&
                        Root.Instance.ChargeInfo.success_total <= 0)
                        UserInterfaceSystem.That.SingleTonQueue<UISpecialOffer>();
                }, GetBaseJson(ParamHandler));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="state">0 开启 1 关闭</param>
        public void GM_GameCheckSwitch(int state)
        {
            if (Root.Instance.UserId <= 0)
            {
                return;
            }
            ParamHandler.Clear();
            ParamHandler["tool_game_replay_close"] = state;
            NetSystem.That.SendGameRequest(Proto.GM_GAME_REPLAY_CLOSE,
                GlobalEnum.HttpRequestType.POST, (jObject) =>
                {
                    Root.Instance.tool_game_replay_close =
                        YZJsonUtil.DeserializeJObject<int>("data.tool_game_replay_close", jObject);
                }, GetBaseJson(ParamHandler));
        }

        public void GetRoomChargeInfo(int roomId = 0)
        {
            ParamHandler.Clear();
            if (roomId > 0)
            {
                ParamHandler["room_id"] = roomId;
            }

            NetSystem.That.SendGameRequest(Proto.GET_ROOM_CHARGE_INFO,
                GlobalEnum.HttpRequestType.POST, (jObject) =>
                {
                    Root.Instance.RoomChargeInfo =
                        YZJsonUtil.DeserializeJObject<RoomChargeInfo>("data.roomChargeInfo", jObject);

                    if (Root.Instance.RoomChargeInfo == null)
                    {
                        return;
                    }

                    var chargeGoodInfoA =
                        YZJsonUtil.DeserializeJObject<ChargeGoodInfo>("data.charge_info.charge_info_a", jObject);
                    var chargeGoodInfoB =
                        YZJsonUtil.DeserializeJObject<ChargeGoodInfo>("data.charge_info.charge_info_b", jObject);
                    Root.Instance.RoomChargeInfo.AChargeInfo = chargeGoodInfoA;
                    Root.Instance.RoomChargeInfo.BChargeInfo = chargeGoodInfoB;

                    if (roomId > 0)
                    {
                        //去充值
                        Charge(chargeGoodInfoA, ActivityType.RoomCharge);
                    }
                }, GetBaseJson(ParamHandler));
        }

        public void SetRoomChargeBeginTime(string type, Action callback)
        {
            const string A = "A";
            const string B = "B";
            if (type is not (A or B))
            {
                return;
            }

            ParamHandler.Clear();
            ParamHandler["name"] = type;
            NetSystem.That.SendGameRequest(Proto.SET_CHARGE_INFO_BEGIN_TIME,
                GlobalEnum.HttpRequestType.POST, (jObject) =>
                {
                    if (type is A)
                    {
                        Root.Instance.RoomChargeInfo.room_charge_A_begin_time = TimeUtils.Instance.UtcTimeNow;
                    }
                    else
                    {
                        Root.Instance.RoomChargeInfo.room_charge_B_begin_time = TimeUtils.Instance.UtcTimeNow;
                    }

                    callback?.Invoke();
                }, GetBaseJson(ParamHandler));
        }


        /// <summary>
        ///  玩家流水相关
        /// </summary>
        private HashSet<string> sendedCashFlowCursor = new HashSet<string>();

        private List<CashFlow> GetCashFlows(JObject jObject, string json = null, bool isAll = false)
        {
            var jsonstring = json ?? jObject.SelectToken("data.flow_list")?.ToString();
            // 过滤掉金钱变化为0的数据
            var list = JsonConvert.DeserializeObject<List<CashFlow>>(jsonstring)
                .Where(flow => isAll || (flow.user_id == Root.Instance.UserId &&
                                         flow.money.ToFloat() + flow.bonus.ToFloat() != 0))
                .ToList();
            return list;
        }

        // 获得玩家现金流水
        public void GetUserCashFlow(bool needNew = false, Action callBack = null)
        {
            ParamHandler.Clear();
            string sendCursor = null;

            if (!needNew)
            {
                sendCursor = Root.Instance.LastCashFlowCursor;
                if (!sendCursor.IsNullOrEmpty())
                {
                    if (sendedCashFlowCursor.Contains(sendCursor))
                    {
                        return;
                    }

                    ParamHandler["cursor"] = sendCursor;
                }
            }

            NetSystem.That.SendGameRequest(Proto.USER_CASH_FLOW,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    var flowListJson = jObject.SelectToken("data.flow_list").ToString();

                    var list = GetCashFlows(jObject, flowListJson);

                    if (!sendCursor.IsNullOrEmpty())
                    {
                        sendedCashFlowCursor.Add(sendCursor);
                    }

                    var temp = new List<CashFlow>();
                    foreach (var t in Root.Instance.CashFlow)
                    {
                        temp.Add(t);
                    }

                    // 更新玩家流水列表
                    for (int i = 0; i < list.Count; ++i)
                    {
                        var i1 = i;
                        if (temp.Find(x => x.id == list[i1].id) != null)
                        {
                            continue;
                        }

                        temp.Add(list[i]);
                    }

                    // 按日期和ID排序
                    var temp1 = temp.OrderByDescending(x => x.created_at).ThenBy(x => x.id).ToList();
                    var temp2 = new List<CashFlow>();
                    // 超过200个，删掉老的
                    int newLength = temp1.Count < 200 ? temp1.Count : 200;
                    for (int i = 0; i < newLength; ++i)
                    {
                        temp2.Add(temp1[i]);
                    }

                    if (temp2.Count == 200)
                    {
                        // 末尾添加一条空的，只显示OverSize提示
                        CashFlow flowOverLength = new CashFlow
                        {
                            id = -1,
                            IsOverLengthSign = true
                        };
                        temp2.Add(flowOverLength);
                    }

                    Root.Instance.CashFlow = temp2;

                    //设置分页标志
                    var newCursor = jObject.SelectToken("data.cursor")?.ToString();

                    if (!newCursor.IsNullOrEmpty())
                    {
                        if (Root.Instance.LastCashFlowCursor.IsNullOrEmpty())
                        {
                            Root.Instance.LastCashFlowCursor = newCursor;
                        }
                        else
                        {
                            var isnew = CheckIsNewCursor(newCursor, Root.Instance.LastCashFlowCursor);
                            if (isnew)
                            {
                                Root.Instance.LastCashFlowCursor = newCursor;
                            }
                        }
                    }
                    else
                    {
                        Root.Instance.CashFlowRecordsOver = true;
                    }

                    callBack?.Invoke();
                    EventDispatcher.Root.Raise(GlobalEvent.Sync_User_Cash_Flow);
                },
                GetBaseJson(ParamHandler)
            );
        }

        // 业务服 上报AFID  IDFA
        public void SendAFID()
        {
            ParamHandler.Clear();
            ParamHandler["idfa"] = YZNativeUtil.GetYZIDFA();
            paramHandler["appsflyer_id"] = YZNativeUtil.GetYZAFID();

            NetSystem.That.SendGameRequest(Proto.SEND_AF_ID,
                GlobalEnum.HttpRequestType.POST,
                (jObject) => { YZDebug.Log("Send Server AFID success, appsflyer_id = " + YZNativeUtil.GetYZAFID()); },
                GetBaseJson(ParamHandler));
        }

        private void SendMediaSource(string media_source)
        {
            ParamHandler.Clear();
            ParamHandler["media_source"] = media_source;
            paramHandler["udid"] = DeviceInfoUtils.Instance.GetEquipmentId();

            NetSystem.That.SendGameRequest(Proto.SEND_MEDIA_SOURCE,
                GlobalEnum.HttpRequestType.POST,
                (jObject) => { YZDebug.Log("Send Server media_source success, media_source = " + media_source); },
                GetBaseJson(ParamHandler));
        }

        [Obsolete]
        public void GetMonthCardInfo()
        {
            ParamHandler.Clear();
            NetSystem.That.SendGameRequest(Proto.MONTH_CARD_INFO,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    // Root.Instance.MonthCardInfo =
                    //     YZJsonUtil.DeserializeJObject<MonthCardInfo>("data.infiniteGrailInfo", jObject);
                },
                GetBaseJson(ParamHandler));
        }

        /// <summary>
        /// 领取类型：1-周卡，2-月卡
        /// </summary>
        /// <param name="type"></param>
        [Obsolete]
        public void GetMonthCardReward(int type)
        {
            ParamHandler.Clear();
            ParamHandler["type"] = type;
            NetSystem.That.SendGameRequest(Proto.MONTH_CARD_CLAIM,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    SyncItem(jObject);
                    UserInterfaceSystem.That.ShowUI<UIMonthCardNew>();
                },
                GetBaseJson(ParamHandler));
        }

        [Obsolete]
        public void GetWeekCardInfo()
        {
            ParamHandler.Clear();
            NetSystem.That.SendGameRequest(Proto.WEEK_CARD_INFO,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    Root.Instance.WeekInfo = 
                    YZJsonUtil.DeserializeJObject<InfiniteWeekInfo>("data.infiniteWeekInfo", jObject);
                    
                    EventDispatcher.Root.Raise(GlobalEvent.SYNC_WEEK_CARD_INFO);
                },
                GetBaseJson(ParamHandler));
        }

        public void GetWeekCardReward()
        {
            ParamHandler.Clear();
            NetSystem.That.SendGameRequest(Proto.WEEK_CARD_CLAIM,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    SyncItem(jObject);
                    Root.Instance.WeekInfo =
                        YZJsonUtil.DeserializeJObject<InfiniteWeekInfo>("data.infiniteWeekInfo", jObject);
                    EventDispatcher.Root.Raise(GlobalEvent.SYNC_WEEK_CARD_INFO);
                },
                GetBaseJson(ParamHandler));
        }

        public void ClaimMuseumReward(int index)
        {
            ParamHandler.Clear();
            ParamHandler["index"] = index - 1;

            NetSystem.That.SendGameRequest(Proto.MUSEUM_CLAIM,
                GlobalEnum.HttpRequestType.POST,
                jObject => HandleClaimMuseumReward(jObject),
                GetBaseJson(ParamHandler));
        }

        public void ClaimMuseumReward(List<int> indexes)
        {
            if (indexes is not { Count: > 0 })
            {
                return;
            }
            ParamHandler.Clear();
            ParamHandler["index"] = string.Join(",", indexes);

            NetSystem.That.SendGameRequest(Proto.MUSEUM_CLAIM,
                GlobalEnum.HttpRequestType.POST,
                jObject => HandleClaimMuseumReward(jObject, false),
                GetBaseJson(ParamHandler));
        }

        private void HandleClaimMuseumReward(JObject jObject, bool onlyAnimation = true)
        {
            if (!MediatorGuide.Instance.IsTriggerGuidePass(TriggerGuideStep.MUSEUM_GUIDE_3))
            {
                EventDispatcher.Root.Raise(TriggerGuideStep.MUSEUM_GUIDE_3.ToString());
            }

            SyncItem(jObject, onlyAnimation: onlyAnimation);
            Root.Instance.MuseumInfo = YZJsonUtil.DeserializeJObject<MuseumInfo>("data.museumInfo", jObject);
        }

        public void RefreshMuseum()
        {
            ParamHandler.Clear();

            NetSystem.That.SendGameRequest(Proto.MUSEUM_REFRESH,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    SyncItem(jObject.SelectToken("data.balance")?.ToString(), out var diff, wait_uimain: true);
                    Root.Instance.MuseumInfo = YZJsonUtil.DeserializeJObject<MuseumInfo>("data.museumInfo", jObject);
                },
                GetBaseJson(ParamHandler));
        }
        
        /// <summary>
        /// 同步每日任务信息
        /// </summary>
        public void GetDailyTaskInfo(bool openUi = false)
        {
            if (Root.Instance.IsNaturalFlow)
            {
                return;
            }

            if (Root.Instance.DailyTaskInfoLock)
                return;
            // 加锁 防止重复发送
            Root.Instance.DailyTaskInfoLock = true;

            ParamHandler.Clear();
            NetSystem.That.SendGameRequest(Proto.DAILY_TASK_INFO,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    // 解锁
                    Root.Instance.DailyTaskInfoLock = false;

                    if (openUi)
                    {
                        UserInterfaceSystem.That.RemoveUIByName("UIWaitingCtrler");
                        UserInterfaceSystem.That.ShowUI<UIDailyMission>(true);
                    }


                    Root.Instance.dailyTaskInfo =
                        YZJsonUtil.DeserializeJObject<DailyTaskInfo>("data.dailyTaskInfo", jObject);
                    EventDispatcher.Root.Raise(GlobalEvent.Sync_Daily_Mission);
                },
                GetBaseJson(ParamHandler));
        }

        public void RefreshSuperDailyTask(int type)
        {
            //   1 - 到期后自动刷新
            //   2 - 消耗免费机会刷新
            //   3 - 钻石刷新

            if (Root.Instance.IsNaturalFlow)
            {
                return;
            }

            ParamHandler.Clear();
            ParamHandler["refresh_type"] = type;
            UserInterfaceSystem.That.ShowUI<UIWaitingCtrler>();
            NetSystem.That.SendGameRequest(Proto.DAILY_TASK_SUPER_REFESH,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    UserInterfaceSystem.That.RemoveUIByName("UIWaitingCtrler");

                    if (Root.Instance.dailyTaskInfo != null)
                        Root.Instance.dailyTaskInfo.supper = YZJsonUtil.DeserializeJObject<int>("data.supper", jObject);

                    var balance = jObject.SelectToken("data.balance").ToString();
                    if (!balance.IsNullOrEmpty())
                        SyncItem(balance, false);

                    // 清空超级任务进度
                    Root.Instance.dailyTaskInfo.superMissionCompleted = 0;

                    GetDailyTaskInfo();
                },
                GetBaseJson(ParamHandler));
        }

        public void ClaimDailyTaskTotal(int index)
        {
            if (Root.Instance.IsNaturalFlow)
            {
                return;
            }

            if (Root.Instance.DailyTaskClaimLock)
                return;
            // 加锁 防止重复发送
            Root.Instance.DailyTaskClaimLock = true;

            ParamHandler.Clear();
            ParamHandler["total_idx"] = index;
            NetSystem.That.SendGameRequest(Proto.DAILY_TASK_CLAIM_TOTAL,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    // 解锁
                    Root.Instance.DailyTaskClaimLock = false;

                    Root.Instance.dailyTaskInfo =
                        YZJsonUtil.DeserializeJObject<DailyTaskInfo>("data.dailyTaskInfo", jObject);

                    var balance = jObject.SelectToken("data.balance").ToString();
                    if (!balance.IsNullOrEmpty())
                    {
                        SyncItem(balance);

                        EventDispatcher.Root.Raise(GlobalEvent.Sync_Daily_Mission);
                    }
                },
                GetBaseJson(ParamHandler));
        }

        public void ClaimDailyTask(bool openUi = false)
        {
            if (Root.Instance.IsNaturalFlow)
            {
                return;
            }

            if (Root.Instance.DailyTaskGetRewardLock)
                return;
            // 加锁 防止重复发送
            Root.Instance.DailyTaskGetRewardLock = true;

            if (openUi)
            {
                UserInterfaceSystem.That.RemoveUIByName("UIWaitingCtrler");
                if (YZDataUtil.GetYZInt(YZConstUtil.YZDailyMissionHaveReward, 0) == 1)
                    UserInterfaceSystem.That.SingleTonQueue<UIDailyMission>();
            }
            else
            {
                ClaimDailyTaskOnOpen();
            }
        }

        public void ClaimDailyTaskOnOpen()
        {
            ParamHandler.Clear();
            NetSystem.That.SendGameRequest(Proto.DAILY_TASK_CALIM,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    //Root.Instance.dailyTaskInfo =
                    //  YZJsonUtil.DeserializeJObject<DailyTaskInfo>("data.dailyTaskInfo", jObject);

                    Root.Instance.DailyTaskGetRewardLock = false;

                    int idx = YZJsonUtil.DeserializeJObject<int>("data.complete_idx", jObject);
                    if (idx != 0)
                    {
                        var balance = jObject.SelectToken("data.balance").ToString();

                        YZDataUtil.SetYZString(YZConstUtil.YZDailyMissionBalance, balance);

                        if (idx == 1)
                        {
                            Root.Instance.dailyTaskInfo.missonCompleted = 1;
                        }
                        else if (idx == 2)
                        {
                            Root.Instance.dailyTaskInfo.superMissionCompleted = 1;
                        }
                        else if (idx == 3)
                        {
                            Root.Instance.dailyTaskInfo.missonCompleted = 1;
                            Root.Instance.dailyTaskInfo.superMissionCompleted = 1;
                        }
                        else
                        {
                            Root.Instance.dailyTaskInfo.missonCompleted = 0;
                            Root.Instance.dailyTaskInfo.superMissionCompleted = 0;
                        }


                        //SyncItem(balance);

                        EventDispatcher.Root.Raise(GlobalEvent.Sync_Daily_Mission);
                    }
                    else
                    {
                        // 没有奖励
                        Root.Instance.dailyTaskInfo.missonCompleted = 0;
                        Root.Instance.dailyTaskInfo.superMissionCompleted = 0;
                        YZDataUtil.SetYZString(YZConstUtil.YZDailyMissionBalance, "");
                    }
                },
                GetBaseJson(ParamHandler));
        }

        public void GetFriendsDuelInfo()
        {
            ParamHandler.Clear();
            NetSystem.That.SendGameRequest(Proto.FRIENDS_DUEL_INFO,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    Root.Instance.DuelInfo = YZJsonUtil.DeserializeJObject<DuelInfo>(
                        "data.friendsDuelInfo", jObject);
                    EventDispatcher.Root.Raise(GlobalEvent.Sync_Friends_Duel_Info);

                    EventDispatcher.Root.Raise(GlobalEvent.Duel_Red_Point);
                }, GetBaseJson(ParamHandler));
        }

        public void CreateFriendsDuelRoom(int room_id)
        {
            ParamHandler.Clear();
            ParamHandler["room_id"] = room_id;
            UserInterfaceSystem.That.ShowUI<UIWaitingCtrler>();
            NetSystem.That.SendGameRequest(Proto.FRIENDS_DUEL_CREATE_ROOM,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    UserInterfaceSystem.That.RemoveUIByName("UIWaitingCtrler");
                    Root.Instance.DuelData = YZJsonUtil.DeserializeJObject<DuelData>(
                        "data.friends_duel_data", jObject);

                    YZDataUtil.SetYZString(YZConstUtil.YZLastDuelRoomNo, Root.Instance.DuelData.room_no);

                    if (Root.Instance.DuelData != null)
                        UserInterfaceSystem.That.ShowUI<UIDuelReady>();
                }, GetBaseJson(ParamHandler));
        }

        public void CloseFriendsDuelRoom(int duelId, int status)
        {
            // status 3-取消匹配  4-超时
            ParamHandler.Clear();
            ParamHandler["id"] = duelId;
            paramHandler["status"] = status;
            NetSystem.That.SetFailCallBack((code) =>
            {
                if (code.Contains("1605") || code.Contains("1602"))
                {
                    // 玩家B已经进入房间，玩家A也会被强制拉入房间
                    DuelMatchBegin(Root.Instance.DuelStatusInfo.room_id,
                        Root.Instance.DuelStatusInfo.room_no);
                }
            });
            NetSystem.That.SendGameRequest(Proto.FRIENDS_DUEL_CLOSE_ROOM,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    // 这里可以不做其他的处理，返回的状态就是客户端发送的状态，房间会被关闭
                    // 清空房间状态
                    Root.Instance.DuelStatusInfo = null;
                }, GetBaseJson(ParamHandler));
        }

        public void GetFriendsDuelRoomStatus(int duelId)
        {
            ParamHandler.Clear();
            ParamHandler["id"] = duelId;
            NetSystem.That.SendGameRequest(Proto.FRIENDS_DUEL_ROOM_STATUS,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    if (UserInterfaceSystem.That.Get<UIDuelReady>() != null)
                    {
                        Root.Instance.DuelStatusInfo = YZJsonUtil.DeserializeJObject<DuelStatusInfo>(
                            "data.status_info", jObject);
                        EventDispatcher.Root.Raise(GlobalEvent.Sync_Friends_Duel_Room);
                    }
                }, GetBaseJson(ParamHandler));
        }

        public void JoinFriendsDuelRoom(string room_no)
        {
            ParamHandler.Clear();
            ParamHandler["room_no"] = room_no;

            UserInterfaceSystem.That.ShowUI<UIWaitingCtrler>();

            NetSystem.That.SetFailCallBack((string fail) =>
            {
                UserInterfaceSystem.That.RemoveUIByName("UIWaitingCtrler");
                UserInterfaceSystem.That.RemoveUIByName("UIDuelJoin");

                if (fail.Contains("1102"))
                    MediatorItem.Instance.ResNotEnoughGoTo(1);
                else
                    UserInterfaceSystem.That.ShowUI<UIDuelFail>();
            });

            NetSystem.That.SendGameRequest(Proto.FRIENDS_DUEL_JOIN_ROOM,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    UserInterfaceSystem.That.RemoveUIByName("UIWaitingCtrler");

                    Root.Instance.DuelStatusInfo = YZJsonUtil.DeserializeJObject<DuelStatusInfo>(
                        "data.status_info", jObject);

                    // 判断房间状态
                    if (Root.Instance.DuelStatusInfo != null)
                    {
                        if (Root.Instance.DuelStatusInfo.status == 1)
                        {
                            DuelMatchBegin(Root.Instance.DuelStatusInfo.room_id,
                                Root.Instance.DuelStatusInfo.room_no);

                            // 成功了就关闭join界面
                            UserInterfaceSystem.That.RemoveUIByName("UIDuelJoin");
                        }
                        else
                        {
                            UserInterfaceSystem.That.ShowUI<UIDuelFail>();
                        }
                    }
                }, GetBaseJson(ParamHandler));
        }

        public void DuelMatchBegin(int roomId, string duelId, bool isGuideRunning = false, bool showMatchUi = true)
        {
            // 与 match_begin 走同一个接口，多了 duel_code 参数
            ParamHandler.Clear();
            ParamHandler["duel_code"] = duelId;
            ParamHandler["room_id"] = roomId;
            var json = GetBaseJson(ParamHandler);
            NetSystem.That.SendGameRequest(Proto.MATCH_BEGIN,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    var matchId = ProcessMatchBeginData(roomId, jObject);

                    Root.Instance.DuelOfflineMatchId = matchId;

                    Root.Instance.tool_game_replay_close =
                        YZJsonUtil.DeserializeJObject<int>("data.tool_game_replay_close", jObject);

                    if (showMatchUi)
                        UserInterfaceSystem.That.ShowUI<UIMatch>(matchId, roomId, isGuideRunning);
                },
                json
            );
        }

        public void ClaimFriendsDuel()
        {
            ParamHandler.Clear();
            NetSystem.That.SendGameRequest(Proto.FRIENDS_DUEL_CLAIM,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    Root.Instance.DuelInfo = YZJsonUtil.DeserializeJObject<DuelInfo>(
                        "data.friendsDuelInfo", jObject);

                    var balance = jObject.SelectToken("data.balance").ToString();
                    SyncItem(balance);

                    EventDispatcher.Root.Raise(GlobalEvent.Claim_Duel_Reward);
                    EventDispatcher.Root.Raise(GlobalEvent.Duel_Red_Point);
                }, GetBaseJson(ParamHandler));
        }

        public void GMSetGameResult(int wantRank, string achieve, int gameScore)
        {
            ParamHandler.Clear();
            paramHandler["want_rank"] = wantRank;
            paramHandler["achieve"] = achieve;
            paramHandler["game_score"] = gameScore;
            var matchId = UserInterfaceSystem.That.Get<UIBingo>().GetStartData().MatchId;
            paramHandler["match_id"] = matchId;
            NetSystem.That.SendGameRequest(Proto.GM_SET_GAME_RESULT,
                GlobalEnum.HttpRequestType.POST,
                (jObject) => { },
                GetBaseJson(ParamHandler));
        }

        /// <summary>
        /// 领取刷新奖励
        /// </summary>
        public void RefreshMagicBall()
        {
            ParamHandler.Clear();
            NetSystem.That.SendGameRequest(Proto.MAGIC_BALL_REFRESH,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    Root.Instance.MagicBallInfo =
                        YZJsonUtil.DeserializeJObject<MagicBallInfo>("data.wizardInfo", jObject);
                    
                    EventDispatcher.Root.Raise(GlobalEvent.Refresh_Room_List);
                    SyncItem(jObject.SelectToken("data.balance")?.ToString(), out var diff, wait_uimain: true);
                },
                GetBaseJson(ParamHandler));
        }

        public void ClaimMagicBall(int order, bool willGetAddedBonus, Action callBack = null)
        {
            if (order <= 0)
            {
                return;
            }

            ParamHandler.Clear();
            ParamHandler["index"] = order - 1;
            NetSystem.That.SendGameRequest(Proto.MAGIC_BALL_CLAIM,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    Root.Instance.MagicBallInfo =
                        YZJsonUtil.DeserializeJObject<MagicBallInfo>("data.wizardInfo", jObject);
                    var data = Root.Instance.MagicBallDatas[order - 1];
                    if (willGetAddedBonus && data != null)
                    {
                        SyncItem(jObject, fireEvent: false, showDiff: false);
                       
                        UserInterfaceSystem.That.ShowUI<UIGetRewards>(new GameData()
                        {
                            ["diff"] = new List<Item>
                            {
                                new(data.type, data.amount),
                                data.PageReward
                            },
                            ["showAll"] = false
                        });
                    }
                    else
                    {
                        SyncItem(jObject);
                    }
                    callBack?.Invoke();
                },
                GetBaseJson(ParamHandler));
        }
        
        public void RefreshLuckyGuy()
        {
            ParamHandler.Clear();
            NetSystem.That.SendGameRequest(Proto.LUCKY_GUY_INFO,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    Root.Instance.LuckyGuyInfo =
                        YZJsonUtil.DeserializeJObject<LuckyGuyInfo>("data.luckyYouInfo", jObject);

                    if (Root.Instance.LuckyGuyInfo is {IsOpen: true})
                    {
                        //加入显示优先级 最后
                        UserInterfaceSystem.That.SingleTonQueue<UILuckyGuy>(
                            () =>
                            {
                                var topUI = UserInterfaceSystem.That.GetTopNormalUI();
                                if (topUI == null)
                                {
                                    return false;
                                }

                                var uimain = topUI as UIMain;

                                return uimain != null && !uimain.IsUIMainInAnimation && uimain.roomToggle.isOn;
                            },
                            new GameData()
                            {
                                ["enterType"] = ActivityEnterType.Refresh,
                                ["queueOrder"] = -10
                            });
                    }
                },
                GetBaseJson(ParamHandler));
        }

        public void GetCountryInfo()
        {
            ParamHandler.Clear();
            NetSystem.That.SendGameRequest(Proto.COUNTRY_INFO,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    DeviceInfoUtils.Instance.CountryInfo = YZJsonUtil.DeserializeJObject<CountryInfo>(
                        "data", jObject);

                    if (DeviceInfoUtils.Instance.CountryInfo == null)
                    {
                        DeviceInfoUtils.Instance.CountryInfo = new CountryInfo
                        {
                            iso_code = "",
                            province = "",
                            city = ""
                        };
                    }

                    DeviceInfoUtils.Instance.GPSInfo = new GPSInfo
                    {
                        country = DeviceInfoUtils.Instance.CountryInfo.iso_code,
                        province =  DeviceInfoUtils.Instance.CountryInfo.province,
                        city =  DeviceInfoUtils.Instance.CountryInfo.city
                    };
                },
                GetBaseJson(ParamHandler));
        }

        public void SendChargeGPS()
        {
            ParamHandler.Clear();
            ParamHandler["gps_extra"] = DeviceInfoUtils.Instance.GetGPSJson();
            NetSystem.That.SendGameRequest(Proto.CHARGE_GPS,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    
                },
                GetBaseJson(ParamHandler));
        }
        
        public void SendGMCheck()
        {
            if (!LocationManager.Shared.IsLocationChecked)
            {
                ParamHandler.Clear();
                ParamHandler["gps"] = GetGPSJson().ToJson();
                NetSystem.That.SendGameRequest(Proto.GM_SET_GPS_CHECK,
                    GlobalEnum.HttpRequestType.POST,
                    (jObject) =>
                    {
                        
                        LocationManager.Shared.IsLocationChecked = true;
                    },
                    GetBaseJson(ParamHandler));
            }
        }
        
        
        public void SendGMCheckIlleage(Action callback = null)
        {
            
            if (Root.Instance.IPIllegal )
            {
                ParamHandler.Clear();
                ParamHandler["gps_extra"] = DeviceInfoUtils.Instance.GetGPSJson();
                NetSystem.That.SendGameRequest(Proto.GM_SET_GPS_CHECK_Illegal,
                    GlobalEnum.HttpRequestType.POST,
                    (jObject) =>
                    {
                        int is_organic = jObject.SelectToken("data.is_organic").Value<int>();
                        Root.Instance.UserInfo.is_organic = is_organic;
                        
                        int gps_illegal = jObject.SelectToken("data.gps_illegal").Value<int>();

                        Root.Instance.ProvinceValid = gps_illegal;

                        if (!Root.Instance.illegalityPeople)
                        {
                            callback?.Invoke();
                        }
                    },
                    GetBaseJson(ParamHandler));
            }
            else
            {
                callback?.Invoke();
            }

        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="roomCharge">是否是房间直充</param>
        public void Charge(ChargeGoodInfo data, ActivityType activityType = ActivityType.None )
        {
            if (data == null)
            {
                return;
            }

            if (Root.Instance.illegalityPeople)
            {
                UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_illegalityPeople"));
                return;
            }

            Charge_configsItem chargeItemTest = new Charge_configsItem();
            chargeItemTest.id = data.id;
            chargeItemTest.bonusValue = data.ShowBonus;
            chargeItemTest.amount = data.amount.ToString();
            LocationManager.Shared.IsLocationValid(YZSafeType.Charge, null, -1,
                () =>
                {
                    if (LocationManager.Shared.IsCountryCanCharge(chargeItemTest, activityType: activityType))
                    {
                        UserInterfaceSystem.That.ShowUI<UIChargeCtrl>(chargeItemTest);
                        if (activityType is ActivityType.RoomCharge)
                        {
                            EventDispatcher.Root.Raise(GlobalEvent.ROOM_CHARGE);
                        }
                    }
                });
        }
        
        
        public void CancelWithdraw(Action callback = null)
        {
       
            ParamHandler.Clear();
            NetSystem.That.SendGameRequest(Proto.CANCLE_WITHDRAW,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    int cash_cancel_time = jObject.SelectToken("data.cash_cancel_time").Value<int>();
                    Root.Instance.UserInfo.last_cancel_cash_time = cash_cancel_time;
                    SyncItem(jObject);
                    callback?.Invoke();
                },
                GetBaseJson(ParamHandler));
        }
        
        public void RefreshItem(Action callback = null)
        {
            ParamHandler.Clear();
            NetSystem.That.SendGameRequest(Proto.REFRESH_ITEM,
                GlobalEnum.HttpRequestType.POST,
                (jObject) =>
                {
                    SyncItem(jObject);
                },
                GetBaseJson(ParamHandler));
        }
    }
}