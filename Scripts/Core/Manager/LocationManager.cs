using System;
using System.Collections.Generic;
using AndroidCShape;
using Core.Controllers;
using Core.Controls;
using Core.Extensions;
using Core.Models;
using Core.Server;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils.Static;
using iOSCShape;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UI;
using Utils;

namespace Core.Manager
{
    public class LocationManager
    {
        public static YZLocations Shared
        {
            get => PlayerManager.Shared.Location;
        }
    }

    public enum YZSafeType
    {
        Money = 1,

        Charge = 2,

        Withdraw = 3,

        AiHelp = 4,
    }
    
    [Serializable]
    public class YZPlacemark
    {
        /// eg. Apple Inc.
        public string name;
        /// street name, eg. Infinite Loop
        public string thoroughfare;
        /// eg. 1
        public string subThoroughfare;
        /// city, eg. Cupertino
        public string locality;
        /// neighborhood, common name, eg. Mission District
        public string subLocality;
        /// state, eg. CA
        public string administrativeArea;
        /// county, eg. Santa Clara
        public string subAdministrativeArea;
        /// zip code, eg. 95014
        public string postalCode;
        /// eg. US
        public string ISOcountryCode;
        /// eg. United States
        public string country;
        /// eg. Lake Tahoe
        public string inlandWater;
        /// eg. Golden Gate Park
        public string ocean;

        public YZPlacemark(string area, string code)
        {
            ISOcountryCode = code;
            administrativeArea = area;
        }

        public YZPlacemark()
        {

        }
    }

    public class YZLocations
    {
        private YZPlacemark YZLocation;

        public double bramount;

        public YZSafeType brtype;

        public ListItem brroom;
        
        public bool IsLocationChecked = false;

        public YZPlacemark GetLocation()
        {
            return YZLocation ??= new YZPlacemark("", "US");
        }

        /// <summary>
        /// 保存定位数据
        /// </summary>
        /// <returns></returns>
        public void SaveLocateData(string countryCode, string area , string city)
        {
            YZLog.LogColor($"Country: {countryCode}, area: {area}, city: {city}" );
            //保存定位数据
            LocationManager.Shared.GetLocation().ISOcountryCode = countryCode;
            //行政区域
            LocationManager.Shared.GetLocation().administrativeArea = area;
            //地区
            LocationManager.Shared.GetLocation().locality = city;

            // // 更新本地保存的定位
            YZDataUtil.SetYZString(YZConstUtil.YZCountryCode, countryCode);
            YZDataUtil.SetYZString(YZConstUtil.YZAreaCode, area);
            YZDataUtil.SetYZString(YZConstUtil.YZCity, city);

            if (countryCode.ToUpper().Contains("US"))
            {
                // 有过一次定位为美国
                LocationManager.Shared.IsLocationChecked = true;
            }
        }

        /// <summary>
        /// 清空定位
        /// </summary>
        public void ClearLocateData()
        {
            SaveLocateData("", "", "");
        }

        /// <summary>
        /// 请求定位权限
        /// </summary>
        public void RequestLocate()
        {
            LocationManager.Shared.IsLocationValid(YZSafeType.Money, null, -1, () =>
            {
                YZNativeUtil.GetYZLocation(true, (location) =>
                {
                    YZDebug.LogConcat("登录时更新真机定位: ", location);
                    if (!location.IsNullOrEmpty() && location != "Limit")
                    {
                        var locationDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(location);
                        locationDic.TryGetValue("ISOcountryCode", out var countryCode);
                        locationDic.TryGetValue("administrativeArea", out var administrativeArea);
            
                        if (!countryCode.IsNullOrEmpty())
                            YZDataUtil.SetYZString(YZConstUtil.YZCountryCode, countryCode);
                        if (!administrativeArea.IsNullOrEmpty())
                            YZDataUtil.SetYZString(YZConstUtil.YZAreaCode, administrativeArea);
                    }
                });
            }, null, "", false);
        }
        
        public void SaveLocateData(string json)
        {
            var localJson = JObject.Parse(json);
            
            string countryCode = localJson.SelectToken("ISOcountryCode")?.ToString();
            string area = localJson.SelectToken("administrativeArea")?.ToString();
            string city = localJson.SelectToken("locality")?.ToString();

            if (!countryCode.IsNullOrEmpty() || !area.IsNullOrEmpty() || !city.IsNullOrEmpty())
            {
                SaveLocateData(countryCode, area, city);
                
            }
            
#if UNITY_IOS
            iOSCShapeLocationTool.Shared.iOSLocateHaveResult = true;     
#endif
        }
        
        // 1.定位，检查账号
        public void IsLocationValid(YZSafeType type, ListItem room, double amount, Action func,
            Action<YZPlacemark> func_location = null, string needCloseUi = "", bool showWaiting = true)
        {
            // brroom = room;
            // brtype = type;
            // bramount = amount;
            // YZDebug.Log("[定位] 1.0 检查账号 Ai Help不检查冻结状态");
            // if (PlayerManager.Shared.Player.User.frozen == 1 && type != YZSafeType.AiHelp)
            // {
            //     YZDebug.Log("[定位] 1.1 检查账号 被冻结了");
            //     string txt_title = YZLocal.GetLocal(YZLocalID.key_notive);
            //     string txt_tips = YZLocal.GetLocal(YZLocalID.key_unusual_activty);
            //     string txt_btn = YZLocal.GetLocal(YZLocalID.key_contact_us);
            //     YZTopControl.YZShowTips(txt_title, txt_tips, txt_btn, () => { },
            //         () => { YZNativeUtil.ContactYZUS(EmailPos.Frozen); });
            //     return;
            // }

            YZLog.LogColor("IsLocationChecked = " + IsLocationChecked);
            
//             if (IsLocationChecked
// #if !RELEASE
//                 || Root.Instance.Role.white == 1
// #endif
//                 
//                 //|| YZDebug.GetBundleId().Contains("test")
//                )
//             {
//                 func?.Invoke();
//                 return;
//             }     
            
            
#if UNITY_ANDROID
                        // 现在测试包都直接跳过定位
            if (YZAndroidPlugin.Shared.AndroidIsEmulator() )
            {
                func?.Invoke();
                return;
            }
#endif
            
            if (showWaiting)
            {
                // 先弹出等待界面
                UserInterfaceSystem.That.ShowUI<UIWaitingCtrler>();
            }

            /*Action wrapFunc = () =>
            {
                MediatorRequest.Instance.SendGMCheck();
                MediatorRequest.Instance.SendGMCheckIlleage(() =>
                {
                    func?.Invoke();
                });
            };*/
            
            IsLocationAuthed(func, func_location, needCloseUi);
        }

        // 2.定位，检查权限
        private void IsLocationAuthed(Action func, Action<YZPlacemark> func_location = null, string needCloseUi = "")
        {
            YZDebug.Log("[定位] 2.0 检查权限");
            YZNativeUtil.RequestYZLocationAuthorization((code) =>
            {
                YZDebug.LogConcat("[定位] 2.0.5 code = ", code.ToString());
                
#if UNITY_EDITOR || UNITY_STANDALONE || NO_SDK
                //同意了定位权限
                code = Const.LocateAllow;
#endif
                if (code == Const.LocateReject)
                {
                    YZDebug.Log("[定位] 2.1 检查权限 用户已拒绝");
#if UNITY_ANDROID
                    YZAndroidLocationPlugin.Shared.RejectLocate = true;   
#endif

#if UNITY_IOS
                    iOSCShapeLocationTool.Shared.iOSLocateHaveResult = true;     
#endif
                    
#if UNITY_IOS && !UNITY_EDITOR
//还没登录
                    // if (Root.Instance.UserId == 0)
                    // {
                    //     YZSDKsController.Shared.SetupAppsFlyer();
                    // }

                    iOSCShapeTool.Shared.IOSYZRequestATT();
#endif
                    // 清空定位
                    YZDebug.Log("用户拒绝定位权限，清空定位");
                    ClearLocateData();

                    // YZFunnelUtil.SendOrderGPS(brtype, false);
                    // YZFunnelUtil.SendOrderGPSFail(brtype, "denial");
                    // YZFunnelUtil.SendGamePlayClick(brroom, false, "denial");
                    //YZTopControl.YZShowAutoHideTips(YZLocal.GetLocal(YZLocalID.key_tips_location_failed));
                    UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_tips_location_failed"));
                    if (needCloseUi != "")
                    {
                        UserInterfaceSystem.That.RemoveUIByName(needCloseUi);
                    }
                }
                else
                {
                    YZDebug.Log("[定位] 2.2 检查权限 用户已授权，开始区分类型");
                    //YZFunnelUtil.SendOrderGPS(brtype, true);
                    IsLocationNeeded(func, func_location);
                }
            });
        }
        
        // 3.定位，区分类型
        private void IsLocationNeeded(Action func, Action<YZPlacemark> func_location = null)
        {
            YZDebug.Log("[定位] 3.0 区分类型");
            // if (brtype == YZSafeType.AiHelp)
            // {
            //     YZDebug.Log("[定位] 3.1 区分类型 AIHelp，获取位置后，直接返回，不需要同盾、KYC");
            //     YZWaitingUICtrler.Shared().YZOnOpenUI();
            //     GetLocation((location) =>
            //     {
            //         YZWaitingUICtrler.Shared().YZOnCloseUI();
            //         func_location?.Invoke(location);
            //     });
            //     return;
            // }

#if NO_SDK
            GetLocation((location) =>
            { 
                func?.Invoke();
            });  
            return;
#endif
            //如果是自然量? 或者在编辑器下， 传入Limit字符串没用影响
            
            YZLog.LogColor("YZServerApiOrganic.Shared.IsYZOrganic() = " + YZServerApiOrganic.Shared.IsYZOrganic());
            if (!YZServerApiOrganic.Shared.IsYZOrganic() || YZGameUtil.GetPlatform() == YZPlatform.Editor)
            {
                YZDebug.Log("[定位] 3.2 区分类型 非自然，开始同盾，并上传定位");
                GetLocation((location) =>
                { 
                    func?.Invoke();
                });
                //func?.Invoke();
                //BeginCheckTongdun(func);
            }
            else
            {
                YZDebug.Log("[定位] 3.3 区分类型 自然量，开始定位");
                //YZWaitingUICtrler.Shared().YZOnOpenUI();
                UserInterfaceSystem.That.ShowUI<UIWaitingCtrler>();
                GetLocation((location) =>
                {
                    //定位成功
                    if (location != null)
                    {
                        AnalysisLocation(func);
                        YZDebug.LogConcat("定位国家: ", location.ISOcountryCode);
                        YZDebug.LogConcat("定位州: ", location.administrativeArea);
                        
                        // 赋值给YZLocation
                        YZDebug.Log("赋值给YZLocation");
                        //YZLocation = location;
                    }
                    else
                    {
                        /*定位失败*/
                        //YZWaitingUICtrler.Shared().YZOnCloseUI();
                        //YZTopControl.YZShowAutoHideTips(YZLocal.GetLocal(YZLocalID.key_tips_location_failed));

#if UNITY_ANDROID
                        UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_tips_location_failed")); 
#endif
                    }
                    UserInterfaceSystem.That.RemoveUIByName(nameof(UIWaitingCtrler));
                });
            }
        }
        
        // 4.定位，自然量 解析数据
        private void AnalysisLocation(Action func)
        {
            // 临时代码
            UserInterfaceSystem.That.RemoveUIByName("UIWaitingCtrler");
            // YZDebug.Log("[定位] 4.0 解析数据 自然量");
            // if (IsYZLocationWhitelistVerification(YZLocation))
            // {
            //     YZDebug.Log("[定位] 4.1 解析数据 自然量，白名单，开始检查同盾");
            //     //BeginCheckTongdun(func);
            // }
            // else
            // {
            //     YZDebug.Log("[定位] 4.2 解析数据 自然量，黑名单，提示并中断流程");
            //     YZWaitingUICtrler.Shared().YZOnCloseUI();
            //     YZTopControl.YZShowAutoHideTips(YZLocal.GetLocal(YZLocalID.key_cash_games_cannot));
            // }
        }
        //
        // // 5.定位，检查同盾
        // private void BeginCheckTongdun(Action func)
        // {
        //     YZDebug.Log("[定位] 5.0 检查同盾");
        //     if (PlayerManager.Shared.GetYZNeedTongdun(brtype))
        //     {
        //         YZDebug.Log("[定位] 5.1 检查同盾 开始");
        //         YZWaitingUICtrler.Shared().YZOnOpenUI();
        //         YZFunnelUtil.YZFunnelTongdun(true, GetSafeFunnelStatus());
        //         YZNativeUtil.GetTongDunBlackBox((box) =>
        //         {
        //             if (box == "pass")
        //             {
        //                 YZDebug.Log("[定位] 5.2 检查同盾 编辑器，直接通过");
        //                 YZWaitingUICtrler.Shared().YZOnCloseUI();
        //                 YZFunnelUtil.YZFunnelTongdun(false, 1);
        //                 BeginCheckKYCNeed(func);
        //             }
        //             else
        //             {
        //                 YZDebug.Log("[定位] 5.3 检查同盾 开始验证Token");
        //                 YZServerRequest request = YZServerApi.Shared.YZRequestCheckTongDun(box);
        //                 request.AddYZSuccessHandler((str) =>
        //                 {
        //                     YZWaitingUICtrler.Shared().YZOnCloseUI();
        //                     YZTongdunResponse data = YZGameUtil.JsonYZToObject<YZTongdunResponse>(str);
        //                     if (data != null && data.data != null && data.data.tongdun != null)
        //                     {
        //                         PlayerManager.Shared.Player.Other.safety_data.tongdun = data.data.tongdun;
        //                         PlayerManager.Shared.Player.User.frozen = data.data.tongdun.frozen;
        //                         if (data.data.tongdun.block == 1)
        //                         {
        //                             YZDebug.Log("[定位] 5.4 检查同盾 被封号了，退出登录");
        //                             YZFunnelUtil.SendGamePlayClick(brroom, false, "frozen");
        //                             YZFunnelUtil.YZFunnelTongdun(false, 2);
        //                             YZTopControl.YZMaintain();
        //                         }
        //                         else if (data.data.tongdun.mocklocation == 1)
        //                         {
        //                             YZDebug.Log("[定位] 5.5 检查同盾 开启了虚拟定位");
        //                             YZFunnelUtil.SendGamePlayClick(brroom, false, "virtual");
        //                             YZFunnelUtil.YZFunnelTongdun(false, 2);
        //                             YZTopControl.YZShowAutoHideTips(YZLocal.GetLocal(YZLocalID.key_cash_games_cannot));
        //                         }
        //                         else
        //                         {
        //                             YZDebug.Log("[定位] 5.6 检查同盾 通过");
        //                             YZFunnelUtil.YZFunnelTongdun(false, 1);
        //                             BeginCheckKYCNeed(func);
        //                         }
        //                     }
        //                     else
        //                     {
        //                         YZDebug.Log("[定位] 5.7 检查同盾 解析服务器数据失败");
        //                         YZFunnelUtil.SendGamePlayClick(brroom, false, "tongdun fail");
        //                         YZTopControl.YZShowAutoHideTips(YZLocal.GetLocal(YZLocalID.key_tips_location_failed));
        //                     }
        //                 });
        //                 request.AddYZFailureHandler((code, msg) =>
        //                 {
        //                     YZDebug.Log("[定位] 5.8 检查同盾 请求服务器判断失败");
        //                     YZWaitingUICtrler.Shared().YZOnCloseUI();
        //                     YZFunnelUtil.SendGamePlayClick(brroom, false, "tongdun fail");
        //                     YZTopControl.YZShowAutoHideTips(YZLocal.GetLocal(YZLocalID.key_tips_location_failed));
        //                 });
        //             }
        //         });
        //     }
        //     else
        //     {
        //         YZDebug.Log("[定位] 5.9 检查同盾 不需要");
        //         YZWaitingUICtrler.Shared().YZOnCloseUI();
        //         BeginCheckKYCNeed(func);
        //     }
        // }
        //
        // // 6.定位，检查是否需要KYC
        // private void BeginCheckKYCNeed(Action func)
        // {
        //     YZDebug.Log("[定位] 6.0 是否需要KYC");
        //     if (PlayerManager.Shared.Player.Other.safety_data.kyc.is_force == 1)
        //     {
        //         YZDebug.Log("[定位] 6.1 是否需要KYC 争议用户，强制进入KYC");
        //         BeginCheckKYC(YZKYCType.Dispute, func);
        //         return;
        //     }
        //
        //     if (GetMoneyChargeNeedKYC())
        //     {
        //         YZDebug.Log("[定位] 6.2 是否需要KYC 美金场、充值，需要强制KYC");
        //         BeginCheckKYC(GetStartKYCStatus(), func);
        //         return;
        //     }
        //
        //     if (GetWithdrawNeedKYC())
        //     {
        //         YZDebug.Log("[定位] 6.3 是否需要KYC 提现，需要强制KYC");
        //         BeginCheckKYC(GetStartKYCStatus(), func);
        //         return;
        //     }
        //
        //     YZDebug.Log("[定位] 6.4 是否需要KYC 进入弱KYC检查流程");
        //     BeginCheckKYCTips(func);
        // }
        //
        // // 7.定位，开始强制KYC
        // private void BeginCheckKYC(YZKYCType kycstatus, Action func)
        // {
        //     YZDebug.Log("[定位] 7.0 开始强制KYC");
        //     KYCManager.Shared.BeginCheckKYC(kycstatus, (status) =>
        //     {
        //         if (status == YZKYCStatus.Pass)
        //         {
        //             YZDebug.Log("[定位] 7.1 开始强制KYC 已通过，继续游戏");
        //             func?.Invoke();
        //         }
        //         else if (status == YZKYCStatus.Fail)
        //         {
        //             if (brtype == YZSafeType.Withdraw)
        //             {
        //                 YZDebug.Log("[定位] 7.2 开始强制KYC 已经失败，提现触发");
        //                 string txt_title = YZLocal.GetLocal(YZLocalID.key_notive);
        //                 string txt_tips = YZLocal.GetLocal(YZLocalID.key_kyc_withdraw_fail);
        //                 string txt_btn = YZLocal.GetLocal(YZLocalID.key_contact_us);
        //                 YZTopControl.YZShowTips(txt_title, txt_tips, txt_btn, () => { },
        //                     () => { YZNativeUtil.ContactYZUS(EmailPos.Withdraw); }, true, false);
        //             }
        //             else if (brtype == YZSafeType.Money || brtype == YZSafeType.Charge)
        //             {
        //                 YZDebug.Log("[定位] 7.3 开始强制KYC 已经失败，美金场、充值触发");
        //                 string txt_title = YZLocal.GetLocal(YZLocalID.key_notive);
        //                 string txt_tips = YZLocal.GetLocal(YZLocalID.key_kyc_play_fail);
        //                 string txt_btn = YZLocal.GetLocal(YZLocalID.key_contact_us);
        //                 YZTopControl.YZShowTips(txt_title, txt_tips, txt_btn, () => { },
        //                     () => { YZNativeUtil.ContactYZUS(EmailPos.ChargePlay); }, true, false);
        //                 YZFunnelUtil.SendGamePlayClick(brroom, false, "kyc fail");
        //             }
        //             else
        //             {
        //                 YZDebug.Log("[定位] 7.4 开始强制KYC 已经失败，未知触发");
        //             }
        //         }
        //         else if (status == YZKYCStatus.Done)
        //         {
        //             YZDebug.Log("[定位] 7.6 开始强制KYC 等待审核，提示KYC验证中");
        //             YZTopControl.YZShowAutoHideTips(YZLocal.GetLocal(YZLocalID.key_kyc_deposit_wait));
        //             YZFunnelUtil.SendGamePlayClick(brroom, false, "kyc done");
        //         }
        //         else if (status == YZKYCStatus.NetworkError)
        //         {
        //             YZDebug.Log("[定位] 7.8 开始强制KYC 网络错误");
        //             YZFunnelUtil.SendGamePlayClick(brroom, false, "kyc error");
        //             YZTopControl.YZShowAutoHideTips(YZLocal.GetLocal(YZLocalID.key_tips_location_failed));
        //         }
        //         else
        //         {
        //             YZDebug.Log("[定位] 7.9 开始强制KYC 需要，但是又没有做");
        //             YZFunnelUtil.SendGamePlayClick(brroom, false, "kyc error");
        //         }
        //     });
        // }
        //
        // // 8.定位，开始KYC，弱流程
        // private void BeginCheckKYCTips(Action func)
        // {
        //     YZDebug.Log("[定位] 8.0 开始弱KYC");
        //     if (brtype == YZSafeType.Withdraw)
        //     {
        //         var chargeSum = PlayerManager.Shared.Player.CountData.charge_sum;
        //         var cashSum = PlayerManager.Shared.Player.CountData.cash_sum;
        //         var factor = cashSum / chargeSum;
        //         if (chargeSum >= 1000 && factor >= 0.5f)
        //         {
        //             YZDebug.Log("[定位] 8.1 开始弱KYC 累充>=1000&提现金额>=50%的用户，进入如KYC流程");
        //             KYCManager.Shared.BeginCheckKYC(YZKYCType.TotalChagre, (status) => { func?.Invoke(); });
        //             return;
        //         }
        //     }
        //
        //     if (brroom != null)
        //     {
        //         YZRoomEnough enough = RoomManager.Shared.CheckYZRoomFeeEnough(brroom.id);
        //         if (GlobalVarManager.Shared.YZ62RoomKYCTimes <= 0 && enough != null &&
        //             enough.ticket_type == RewardType.Money && PlayerManager.Shared.IsHighRiskPlayer() &&
        //             enough.ticket_money >= 62)
        //         {
        //             YZDebug.Log("[定位] 8.2 开始弱KYC 62刀高风险触发，进入如KYC流程");
        //             GlobalVarManager.Shared.YZ62RoomKYCTimes += 1;
        //             KYCManager.Shared.BeginCheckKYC(YZKYCType.HighRisk, (status) => { func?.Invoke(); });
        //             return;
        //         }
        //     }
        //
        //     YZDebug.Log("[定位] 8.3 开始弱KYC，没有触发，继续游戏");
        //     func?.Invoke();
        // }
        
        // 定位，白名单校验
        // private bool IsYZLocationWhitelistVerification(YZPlacemark pl)
        // {
        //     if (pl == null)
        //     {
        //         YZDebug.Log("定位 数据解析错误");
        //         return false;
        //     }
        //
        //     bool available = false;
        //     foreach (string contry in PlayerManager.Shared.Config.data.charge.available_country)
        //     {
        //         if (pl.ISOcountryCode == contry)
        //         {
        //             available = true;
        //         }
        //     }
        //
        //     if (!available)
        //     {
        //         YZDebug.Log("定位 国家没在白名单");
        //         Dictionary<string, object> properties = new Dictionary<string, object>();
        //         properties.Add(FunnelEventParam.brblacklistcountry, pl.country);
        //         properties.Add(FunnelEventParam.brblacklistarea, pl.administrativeArea);
        //         properties.Add(FunnelEventParam.brwhitelistnumber,
        //             PlayerManager.Shared.Config.data.charge.available_country.Count);
        //         YZFunnelUtil.SendYZEvent(FunnelEventID.brblacklist, properties);
        //         return false;
        //     }
        //
        //     if (PlayerManager.Shared.Config.data.charge.not_available_states != null)
        //     {
        //         foreach (string contry in PlayerManager.Shared.Config.data.charge.not_available_states)
        //         {
        //             if (contry == pl.administrativeArea)
        //             {
        //                 YZDebug.Log("定位 所在州在黑名单");
        //                 Dictionary<string, object> properties = new Dictionary<string, object>();
        //                 properties.Add(FunnelEventParam.brblacklistcountry, pl.country);
        //                 properties.Add(FunnelEventParam.brblacklistarea, pl.administrativeArea);
        //                 properties.Add(FunnelEventParam.brwhitelistnumber,
        //                     PlayerManager.Shared.Config.data.charge.available_country.Count);
        //                 YZFunnelUtil.SendYZEvent(FunnelEventID.brblacklist, properties);
        //                 return false;
        //             }
        //         }
        //     }
        //
        //     return true;
        // }
        
        private void GetLocation(Action<YZPlacemark> location)
        {
            // if (YZLocation != null)
            // {
            //     YZDebug.Log("[定位] 已完成定位，直接返回");
            //     location?.Invoke(YZLocation);
            //     return;
            // }
        
            YZDebug.Log("[定位] 开始定位");
            //传入“Limit”
            YZNativeUtil.GetYZLocation(false, (str) =>
            {
                YZDebug.LogConcat("[定位] 定位完成: ", str);
                //这里本来是想传解析后的经纬度信息, 但是后续这个信息
                if (!string.IsNullOrEmpty(str) && str != Const.FakeGetGPSFlag)
                {
                    YZLocation = YZGameUtil.JsonYZToObject<YZPlacemark>(str);
                    //YZServerApi.Shared.YZSetUserDeviceInfo(str, null, null, null, null);
                    //YZFunnelUtil.SetUserState(YZLocation);
                    //YZFunnelUtil.SendOrderGPSSucc(brtype);
                    location?.Invoke(YZLocation);
                }
                else
                {
                    //为空或者 Limit
                    //YZFunnelUtil.SendOrderGPSFail(brtype, "GPS Fail");
                    //YZFunnelUtil.SendGamePlayClick(brroom, false, "success");
                    location?.Invoke(null);
                }
            });
        }
        //
        // // 获取开始KYC的类型
        // private YZKYCType GetStartKYCStatus()
        // {
        //     if (PlayerManager.Shared.Player.Other.safety_data.kyc.is_force == 1)
        //     {
        //         return YZKYCType.Dispute;
        //     }
        //
        //     if (brtype == YZSafeType.Money)
        //     {
        //         return YZKYCType.Money;
        //     }
        //
        //     if (brtype == YZSafeType.Withdraw)
        //     {
        //         return YZKYCType.Withdraw;
        //     }
        //
        //     if (brtype == YZSafeType.AiHelp)
        //     {
        //         return YZKYCType.None;
        //     }
        //
        //     return YZKYCType.Charge;
        // }
        //
        // // 美金或充值是否会触发KYC
        // private bool GetMoneyChargeNeedKYC()
        // {
        //     if (brtype == YZSafeType.Money || brtype == YZSafeType.Charge)
        //     {
        //         return PlayerManager.Shared.GetYZNeedKYCChagreMoney(GetLocation());
        //     }
        //
        //     return false;
        // }
        //
        // // 获取提现是否会触发KYC
        // private bool GetWithdrawNeedKYC()
        // {
        //     if (brtype == YZSafeType.Withdraw)
        //     {
        //         return PlayerManager.Shared.GetYZNeedKYCWithdraw(bramount);
        //     }
        //
        //     return false;
        // }

        // 获取安全打点的状态
        private int GetSafeFunnelStatus()
        {
            if (brtype == YZSafeType.Charge)
            {
                return 1002;
            }

            if (brtype == YZSafeType.Money)
            {
                return 2;
            }

            return 1000;
        }
        
        // 充值定位检查
        public bool IsCountryCanCharge(Charge_configsItem chargeConfigsItem, ActivityType activityType = ActivityType.None)
        {
            // 关闭等待界面
            UserInterfaceSystem.That.RemoveUIByName("UIWaitingCtrler");
            
#if RELEASE || RTEST
       // 检查具体定位
            string countryCode = YZDataUtil.GetLocaling(YZConstUtil.YZCountryCode).ToUpper();

            if (countryCode != "US" && Root.Instance.Role.white == 0)
            {
                // 定位非美国  且不在白名单内
                UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_region_deposit_limit"));
                return false;
            }
#endif
            
            // 查询可用的充值渠道
            MediatorRequest.Instance.GetChargeMethods(true, chargeConfigsItem, activityType);
            
            
            // 固定返回 false，后续逻辑在充值渠道 request 内处理
            return false;
        }
    }
}