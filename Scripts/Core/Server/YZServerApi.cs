using UnityEngine;
using System.Collections.Generic;
using LitJson;
using System;
using System.Text.RegularExpressions;
using System.Text;
using Core.Controllers;
using Core.Controls;
using Core.Manager;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Third.I18N;
using UI;
using Utils;

namespace Core.Server
{
    public class LoginStatus
    {
        public const int none = 0; // 断网重连中...
        public const int check = 1; // 检查更新中...
        public const int login = 2; // 正在登录中...
        public const int config = 3; // 拉取配置中...
        public const int passday = 4; // 正在跨天中...
        public const int gameidle = 5; // 进入大厅
    }

    public class YZServerApiPath
    {
        public const string YZUpgradeCheck = "upgrade/check";
        public const string YZLoginGuest = "user/guest";
        public const string YZLoginAuto = "user/auto_login";
        public const string YZLoginPass = "user/auto_login_passday";
        public const string YZConfig = "configs";
        public const string YZSetOrganic = "support/set_user_organic";
        public const string YZSetIdfaAppsflyerId = "idfa_appsflyer_id";
        public const string YZInviteRegist = "invite/regist";
        public const string YZGetAddress = "state/get";
        public const string YZEditUser = "upload/set_info";
        public const string YZClaimVip = "vip/experience";
        public const string YZClaimBigMission = "sevendays/finish";
        public const string YZClaimMission = "sevendays/single";
        public const string YZClaimDaily = "sign/claim";
        public const string YZRecordIncomplete = "match/incomplete_history";
        public const string YZRecordComplete = "match/complete_history";
        public const string YZRecoreClaim = "match/claim";
        public const string YZRecordSingle = "match/single_history";
        public const string YZGetMessage = "message/list";
        public const string YZMarkMessage = "message/mark";
        public const string YZRecommond = "match/recommend";
        public const string YZUserDelete = "user/delete"; // 删除账号
        public const string YZDeblockRoom = "deblock/cartoon";
        public const string YZWelcomeBackClaim = "callback/claim"; // 领取回归奖励
        public const string YZMoneyboxInfo = "moneybox/info"; // 刷新存钱罐信息
        public const string YZMoneyboxClaim = "moneybox/claim"; // 领取存钱罐奖励
        public const string YZADBonus = "ad/bonus"; // 领取广告bonus奖励
        public const string YZADChips = "ad/diamond"; // 领取广告chips奖励
        public const string YZWithdrawCard = "redeem/card"; // 领取兑换码奖励
        public const string YZBounsRoom = "diamond/bonus_room";
        public const string YZStreamList = "cost/details";
        public const string YZUserDeviceInfo = "support/devices_details";
        public const string YZRefreshBalance = "refresh/balance";
        public const string YZCheckTongDun = "safe/tongdun";
        public const string YZCheckKyc = "safe/kyc";

        // ------Bingo Cup-----------
        public const string YZBingocupInfo = "bingo_cup/info";
        public const string YZBingocupList = "bingo_cup/list";
        public const string YZBingocupClaim = "bingo_cup/claim";

        public const string YZBingocupHistory = "bingo_cup/history";
        // ------Bingo Cup-----------

        // ------Bingo Party---------
        public const string YZBingoInfo = "collect_bingo/info";
        public const string YZBingoList = "collect_bingo/list";

        public const string YZClaimBingo = "collect_bingo/claim";
        // ------Bingo Party---------

        // ------Bingo Slots---------
        public const string YZBingoSlotsInfo = "slots_bingo/info";
        public const string YZBingoSlotsList = "slots_bingo/list";
        public const string YZClaimSlotsBingo = "slots_bingo/claim";
        public const string YZBingoSlotsBegin = "slots_bingo/slots";
        public const string YZBingoSlotsSkill = "slots_bingo/skill";

        public const string YZBingoSlotsNext = "slots_bingo/next";
        // ------Bingo Slots---------

        // ------Fish---------
        public const string YZFishReward = "fish/reward";
        // ------Fish---------

        // ------战斗匹配---------
        public const string YZMatchBegin = "match/begin";
        public const string YZMatchEnd = "match/end";
        public const string YZGameBegin = "game/begin";
        public const string YZGameEnd = "game/end";

        public const string YZMatchJoin = "match/join"; // 加入邀请比赛
        // ------战斗匹配---------

        // ------邮箱绑定---------
        public const string YZEmailLogin = "user/email_login";
        public const string YZChangePassword = "user/change_password";
        public const string YZBindEmail = "user/bind_email";

        public const string YZResendEmail = "user/resend_email";
        // ------邮箱绑定---------

        
        //public const string YZSERVER_URL_API = "https://app-bingo-gp.yatzybrawl.com/api/";

        // ------商城和充值---------
        public const string YZTimeRewardClaim =   "time_reward/claim";
        public const string YZChargeCenterChannel = "charge_center/channel";
        public const string YZChargeCenterCreate =   "charge_center/create";
        public const string YZChargeCenterQuery =   "charge_center/query";
        public const string YZChargePresentInfo = "charge_present/info";
        public const string YZChargePresentClaim = "charge_present/claim";
        public const string YZChargeRewardFlipCard = "charge_reward/flip";

        public const string YZChargeRewardBreakClaim = "charge_reward/break";
        // ------商城和充值---------


        // ------提现---------
        public const string YZHyperBindPhone = "cash/bind_phone/start";
        public const string YZHyperBindPhoneVerify = "cash/bind_phone/verify";
        public const string YZHyperRegister = "hyper/register";
        public const string YZHyperResend = "hyper/resend";
        public const string YZHyperCommit = "hyper/commit/info";
        public const string YZHyperApply = "cash/apply/hyper";
        public const string YZHyperHistory = "cash/history";

        public const string YZHyperCashFlow = "cash/flow";
        // ------提现---------

        // ------小游戏---------
        public const string YZMiniGameCardPlay = "card/play";
        public const string YZMiniGameWheelPlayLucky = "wheel/play_junior_b";
        public const string YZMiniGameWheelPlayGold = "wheel/play_senior_b";

        public const string YZMiniGameWheelBox = "wheel/box";
        // ------小游戏---------

        // ------年龄验证---------
        public const string YZSupportCenterAgeCheck = "safe/age";
        // ------年龄验证---------
    }

    public class YZServerApi : YZBaseController<YZServerApi>
    {
        [HideInInspector] public int YZStatus = LoginStatus.none;

        [HideInInspector] public readonly List<YZServerRequest> YZListRequest = new List<YZServerRequest>();

        public const string YZMethodGet = "get";
        public const string YZMethodPost = "post";

        public string YZTimeZone
        {
            get => YZTimeZoneIns;
        }

        public string YZLanguage
        {
            get => YZLanguageIns;
        }

        public string YZSessionID
        {
            get => YZSessionIdIns;
            set => YZSessionIdIns = value;
        }

        private long YZTimeDt = 0;
        private string YZTimeZoneIns;
        private string YZLanguageIns;
        private string YZSessionIdIns;
        private JsonData YZDeviceInfo;

        private readonly List<string> YZRetryApiList = new List<string>()
        {
            YZServerApiPath.YZMiniGameCardPlay,
            YZServerApiPath.YZRecordIncomplete,
            YZServerApiPath.YZRecordComplete,
            YZServerApiPath.YZMiniGameWheelBox,
            YZServerApiPath.YZClaimDaily,
            YZServerApiPath.YZClaimMission,
            YZServerApiPath.YZClaimBigMission,
            YZServerApiPath.YZClaimVip,
            YZServerApiPath.YZBingoInfo,
            YZServerApiPath.YZBingoList,
            YZServerApiPath.YZClaimBingo,
            YZServerApiPath.YZMiniGameWheelPlayGold,
            YZServerApiPath.YZMiniGameWheelPlayLucky,
            YZServerApiPath.YZBingocupClaim,
            YZServerApiPath.YZBingocupHistory,
            YZServerApiPath.YZBingocupInfo,
            YZServerApiPath.YZBingocupList,
            YZServerApiPath.YZADBonus,
            YZServerApiPath.YZDeblockRoom,
            YZServerApiPath.YZWelcomeBackClaim,
            YZServerApiPath.YZMoneyboxClaim,
            YZServerApiPath.YZMoneyboxInfo,
            YZServerApiPath.YZGetMessage,
            YZServerApiPath.YZTimeRewardClaim,
            YZServerApiPath.YZEditUser,
            YZServerApiPath.YZHyperBindPhone,
            YZServerApiPath.YZHyperBindPhoneVerify,
            YZServerApiPath.YZHyperApply,
            YZServerApiPath.YZHyperHistory,
        };

        private readonly List<string> YZIgnoreApiList = new List<string>()
        {
            YZServerApiPath.YZRecommond,
            YZServerApiPath.YZSetOrganic,
            YZServerApiPath.YZSetIdfaAppsflyerId,
            YZServerApiPath.YZMarkMessage,
            YZServerApiPath.YZHyperResend,
        };

        private void Start()
        {
            //InitYZDeviceInfo();
        }

        #region Devive & Time

        public string GetYZDeviceInfo()
        {
            return YZDeviceInfo == null ? string.Empty : YZDeviceInfo.ToJson();
        }

        public void SetYZServerTime(long time)
        {
            YZTimeDt = time - YZTimeUtil.GetYZTimestamp();
        }

        /// <summary>
        /// 获取服务器时间
        /// </summary>
        /// <returns></returns>
        public int GetYZServerTime()
        {
            return (int)(YZTimeUtil.GetYZTimestamp() + YZTimeDt);
        }

        // /// <summary>
        // /// 获取跨天时间
        // /// </summary>
        // /// <returns></returns>
        // public int GetYZPassTime()
        // {
        //     if (!PlayerManager.Shared.GetYZIsConfig())
        //     {
        //         return int.MaxValue;
        //     }
        //
        //     return PlayerManager.Shared.Config.RefreshData.login[0];
        // }

        private void InitYZDeviceInfo()
        {
            YZDeviceInfo = new JsonData
            {
                ["name"] = SystemInfo.deviceName,
                ["os"] = SystemInfo.operatingSystem
            };
            YZTimeZoneIns = YZNativeUtil.GetYZTimeZone();
            YZLanguageIns = YZNativeUtil.GetYZLanguage();
        }

        #endregion

        #region Request

        // public YZServerRequest CreateYZRequest(string url, string method)
        // {
        //     YZServerRequest YZRequest = new YZServerRequest(url, method, (http_code, content, request) =>
        //     {
        //         // 1.校验请求结果
        //         if (string.IsNullOrEmpty(content))
        //         {
        //             //YZTopControl.YZShowDebugAutoHideTips(YZString.Concat("服务器返回内容为空，提示用户重新登录: ", http_code));
        //             YZDebug.Log(YZString.Concat("服务器返回内容为空，提示用户重新登录: ", http_code));
        //             UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("lost_connect"));
        //             return;
        //         }
        //
        //         // 2.打印请求结果
        //         string baseUrl = YZServerUtil.GetYZServerURL();
        //         YZDebug.LogConcat("Request Ended: ", baseUrl, url, " Content: ", content);
        //         YZGameUtil.PrintBuglyLog(YZString.Concat("Request Ended: ", baseUrl, url));
        //         // 3.移除请求管理列表
        //         YZListRequest.Remove(request);
        //         // 4.开始解析
        //         if (http_code == 200)
        //         {
        //             JsonData data = null;
        //             try
        //             {
        //                 data = JsonMapper.ToObject(content);
        //             }
        //             catch
        //             {
        //                 YZDebug.Log(YZString.Concat("服务器返回数据解析失败: ", http_code, "~>", content,
        //                     "~>提示用户重登"));
        //                 UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("lost_connect"));
        //             }
        //
        //             bool has_data = YZJsonUtil.ContainsYZKey(data, "data");
        //             int api_code = YZJsonUtil.GetYZInt(data, "code", 1000);
        //             if (api_code == 0 && has_data)
        //             {
        //                 RequestYZSuccess(request, content);
        //             }
        //             else
        //             {
        //                 RequestYZFailure(request, api_code, content);
        //             }
        //         }
        //         else
        //         {
        //             // 重连，断网重连处理
        //             if (YZRetryApiList.Contains(request.YZUrlPath))
        //             {
        //                 if (request.YZRetryTimes < 3)
        //                 {
        //                     // 弹出重连弹框
        //                     //YZTopControl.YZRetryConnect(() => { request.RetrySend(); });
        //                 }
        //                 else
        //                 {
        //                     //YZTopControl.YZReloginConnect();
        //                 }
        //             }
        //             else if (YZIgnoreApiList.Contains(request.YZUrlPath))
        //             {
        //                 // 忽略，继续向上层抛出错误
        //                 request.ExecYZFailureHandler((int)http_code, content);
        //             }
        //             else
        //             {
        //                 // 弹出断网重连
        //                 YZDebug.Log(YZString.Concat("网络错误，提示用户重新登录: ", http_code));
        //                 UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("lost_connect"));
        //             }
        //         }
        //     });
        //     YZRequest.AddYZDefaultParam();
        //     YZListRequest.Add(YZRequest);
        //     return YZRequest;
        // }

        private void RequestYZFailure(YZServerRequest request, int api_code, string content)
        {
            if (api_code == 9999)
            {
                //YZTopControl.YZShowDebugAutoHideTips("服务器返回正在维护中");
                YZDebug.Log("服务器返回正在维护中");
                //YZTopControl.YZMaintain();
                return;
            }

            if (api_code == 9998)
            {
                YZDebug.Log("服务器返回弹404");
                //YZTopControl.YZShowDebugAutoHideTips("服务器返回弹404");
                // string txt_title = YZLocal.GetLocal(YZLocalID.key_notive);
                // string txt_tips = YZLocal.GetLocal(YZLocalID.key_http_code_9998);
                // string txt_btn = YZLocal.GetLocal(YZLocalID.key_contact_us);
                // YZTopControl.YZShowTips(txt_title, txt_tips, txt_btn, () => { Application.Quit(); },
                //     () => { YZNativeUtil.ContactYZUS(EmailPos.Code9998); }, true, false);
                return;
            }

            if (api_code == 9997)
            {
                YZDebug.Log("服务器返回弹断网");
                //YZTopControl.YZShowDebugAutoHideTips("服务器返回弹断网");
                // string txt_title = YZLocal.GetLocal(YZLocalID.key_notive);
                // string txt_tips = YZLocal.GetLocal(YZLocalID.key_http_code_9997);
                // string txt_btn = YZLocal.GetLocal(YZLocalID.key_contact_us);
                // YZTopControl.YZShowTips(txt_title, txt_tips, txt_btn, () => { Application.Quit(); },
                //     () => { YZNativeUtil.ContactYZUS(EmailPos.Code9997); }, true, false);
                return;
            }

            if (api_code == 1200)
            {
                YZDebug.Log("服务器返回被封号，直接封号弹框");
                //YZTopControl.YZShowDebugAutoHideTips("服务器返回被封号，直接封号弹框");
                // string txt_title = YZLocal.GetLocal(YZLocalID.key_ERROR);
                // string txt_tips = YZLocal.GetLocal(YZLocalID.key_http_code_1200);
                // string txt_btn = YZLocal.GetLocal(YZLocalID.key_ok_got_it);
                // YZTopControl.YZShowTips(txt_title, txt_tips, txt_btn, null, () => { Application.Quit(); });
                return;
            }

            if (api_code == 1100)
            {
                YZDebug.Log("服务器返回登录失效，直接弹断网重联");
                UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("lost_connect"));
                //YZTopControl.YZShowDebugAutoHideTips("服务器返回登录失效，直接弹断网重联");
                //YZTopControl.YZLostConnect();
                //PlayerManager.Shared.User.YZPlayerAuth = string.Empty;
                return;
            }

            if (api_code == 1102)
            {
                // 美金不足
                //YZTopControl.YZShowAutoHideTips(YZLocal.GetLocal(YZLocalID.key_http_code_1102));
                UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("lost_connect"));
            }
            else if (api_code == 1103)
            {
                // 筹码不足
                //YZTopControl.YZShowAutoHideTips(YZLocal.GetLocal(YZLocalID.key_http_code_1103));
                UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("lost_connect"));
            }
            else if (api_code == 1104)
            {
                // 手机号已被占用
                //YZTopControl.YZShowAutoHideTips(YZLocal.GetLocal(YZLocalID.key_http_code_1104));
                UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("lost_connect"));
            }
            else if (api_code == 1105)
            {
                // 验证码错误
                //YZTopControl.YZShowAutoHideTips(YZLocal.GetLocal(YZLocalID.key_http_code_1105));
                UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("lost_connect"));
            }
            else if (api_code == 1106)
            {
                // 邮箱已绑定了别的账号
                //YZTopControl.YZShowAutoHideTips(YZLocal.GetLocal(YZLocalID.key_http_code_1106));
                UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("lost_connect"));
            }
            else if (api_code == 1107)
            {
                // 输入信息错误
                //YZTopControl.YZShowAutoHideTips(YZLocal.GetLocal(YZLocalID.key_http_code_1107));
                UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("lost_connect"));
            }
            else if (api_code == 1108)
            {
                // 邮箱不存在
                //YZTopControl.YZShowAutoHideTips(YZLocal.GetLocal(YZLocalID.key_http_code_1108));
                UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("lost_connect"));
            }
            else if (api_code == 1109)
            {
                // 您已经完成了邮箱绑定
                //YZTopControl.YZShowAutoHideTips(YZLocal.GetLocal(YZLocalID.key_http_code_1109));
                UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("lost_connect"));
            }
            else if (api_code == 1110)
            {
                // 邮箱格式错误
                //YZTopControl.YZShowAutoHideTips(YZLocal.GetLocal(YZLocalID.key_http_code_1110));
                UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("lost_connect"));
            }
            else if (api_code == 1111)
            {
                // 邮箱登录，邮箱账号或密码错误
                //YZTopControl.YZShowAutoHideTips(YZLocal.GetLocal(YZLocalID.key_http_code_1111));
                UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("lost_connect"));
            }
            else if (api_code == 1114)
            {
                // 账号已不存在（登录已删除账号绑定邮箱时提示）
                //YZTopControl.YZShowAutoHideTips(YZString.Format(YZLocal.GetLocal(YZLocalID.key_security_no_account),
                //    YZDefineUtil.GetYZEmail()));
                UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("lost_connect"));
            }
            else if (api_code == 1301)
            {
                // 没有兑换次数了
                //YZTopControl.YZShowAutoHideTips(YZLocal.GetLocal(YZLocalID.key_http_code_1301));
                UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("lost_connect"));
            }
            else if (api_code == 1302)
            {
                // 兑换码无效，或已过期
                //YZTopControl.YZShowAutoHideTips(YZLocal.GetLocal(YZLocalID.key_http_code_1302));
                UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("lost_connect"));
            }
            else if (api_code == 1303)
            {
                // 信用卡信息输入错误(修改为充值中心后，不再使用)
            }
            else if (api_code == 2000)
            {
                // 作弊用户，提现失败
                //YZTopControl.YZShowAutoHideTips(YZLocal.GetLocal(YZLocalID.key_http_code_2000));
                UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("lost_connect"));
            }
            else
            {
                //YZTopControl.YZShowAutoHideTips(YZLocal.GetLocal(YZLocalID.key_http_code_unkonwn));
            }

            request.ExecYZFailureHandler(api_code, content);
        }

        public void RequestYZSuccess(YZServerRequest Request, string json)
        {
            JsonData data = YZGameUtil.JsonYZToObject<JsonData>(json);
            // 1.校准时间
            SetYZServerTime(long.Parse(data["time"].ToString()));
            // 2.AF打点
            if (YZJsonUtil.ContainsYZKey(data, "event"))
            {
                JsonData events = data["event"];
                int length = events.Count;
                for (int i = 0; i < length; i++)
                {
                    string eventName = events[i].ToString();
                    YZFunnelUtil.SendYZEvent(eventName);
                }
            }

            // 3.推送
            if (YZJsonUtil.ContainsYZKey(data, "tag"))
            {
                //YZNativeUtil.SetPushTags(data["tag"].ToJson());
            }

            // 4.刷新数据
            //PlayerManager.Shared.Refresh.YZRefreshResponceData(Request, json);
            // 5.通知上层，请求成功
            Request.ExecYZSuccessHandler(json);
        }

        #endregion

        #region 消息处理

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                YZFunnelUtil.FlushYZ();
                //YZServerApiMessage.Shared.YZAppLeave();
            }
            else
            {
                //YZServerApiMessage.Shared.YZAppEnter();
            }
        }

        #endregion

        #region Api

        // 通过经纬度获取地址
        // public YZServerRequest GetYZAddressLocation(string latitude, string longitude)
        // {
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZGetAddress, YZMethodPost);
        //     request.AddYZParam("lat", latitude);
        //     request.AddYZParam("lon", longitude);
        //     request.SendYZ();
        //     return request;
        // }

        // public YZServerRequest EditYZUserInfo(string file, string name, int index)
        // {
        //     User_info info = PlayerManager.Shared.Player.User.user_info;
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZEditUser, YZMethodPost);
        //     if (!string.IsNullOrEmpty(file))
        //     {
        //         request.AddYZParam("file", file);
        //     }
        //
        //     if (!string.IsNullOrEmpty(name))
        //     {
        //         request.AddYZParam("name", name);
        //     }
        //
        //     if (index > 0 && int.Parse(info.head_index) != index)
        //     {
        //         request.AddYZParam("index", index.ToString());
        //     }
        //
        //     request.AddYZSuccessHandler((str) =>
        //     {
        //         PlayerManager.Shared.Player.User.user_info = YZGameUtil.JsonYZToObject<UserEdit>(str).data;
        //         this.PostNotification(new YZNotifyObj(YZNotifyName.YZRefreshUserInfo));
        //     });
        //     request.SendYZ();
        //     return request;
        // }
        //
        // public YZServerRequest YZClaimVIPExperience(Action back)
        // {
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZClaimVip, YZMethodPost);
        //     request.AddYZSuccessHandler((str) => { YZEliteTipsUICtrler.Shared().YZShowEliteTipsUI(back); });
        //     request.AddYZFailureHandler((code, msg) => { back?.Invoke(); });
        //     request.SendYZ();
        //     return request;
        // }

        // public YZServerRequest YZClaimBigMission()
        // {
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZClaimBigMission, YZMethodPost);
        //     request.SendYZ();
        //     return request;
        // }
        //
        // public YZServerRequest YZClaimMission(int day, int index)
        // {
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZClaimMission, YZMethodPost);
        //     request.AddYZParam("days", day.ToString());
        //     request.AddYZParam("task_id", index.ToString());
        //     request.SendYZ();
        //     return request;
        // }

        // public YZServerRequest YZClaimDailyReward()
        // {
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZClaimDaily, YZMethodPost);
        //     request.YZIsRefreshBalance = false;
        //     request.SendYZ();
        //     return request;
        // }
        //
        // public YZServerRequest YZRequestIncompleteHistiory(string next)
        // {
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZRecordIncomplete, YZMethodPost);
        //     if (!string.IsNullOrEmpty(next))
        //     {
        //         request.AddYZParam("next", next);
        //     }
        //
        //     request.SendYZ();
        //     return request;
        // }
        //
        // public YZServerRequest YZRequestCompleteHistiory(string next)
        // {
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZRecordComplete, YZMethodPost);
        //     if (!string.IsNullOrEmpty(next))
        //     {
        //         request.AddYZParam("next", next);
        //     }
        //
        //     request.SendYZ();
        //     return request;
        // }
        //
        // public YZServerRequest YZRequestSingleHistiory(string id)
        // {
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZRecordSingle, YZMethodPost);
        //     request.AddYZParam("table_id", id);
        //     request.SendYZ();
        //     return request;
        // }
        //
        // public YZServerRequest YZClaimReward(string ids)
        // {
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZRecoreClaim, YZMethodPost);
        //     request.YZIsRefreshBalance = false;
        //     request.AddYZParam("match_id", ids);
        //     request.SendYZ();
        //     return request;
        // }
        //
        // public YZServerRequest YZRequestInviteInfo()
        // {
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZRecommond, YZMethodPost);
        //     request.SendYZ();
        //     return request;
        // }
        //
        // public YZServerRequest YZCollectBingoInfo()
        // {
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZBingoInfo, YZMethodPost);
        //     request.SendYZ();
        //     return request;
        // }
        //
        // public YZServerRequest YZCollectBingoList(string next)
        // {
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZBingoList, YZMethodPost);
        //     if (!string.IsNullOrEmpty(next))
        //     {
        //         request.AddYZParam("next", next);
        //     }
        //
        //     request.SendYZ();
        //     return request;
        // }
        //
        // public YZServerRequest YZCollectBingoClaim()
        // {
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZClaimBingo, YZMethodPost);
        //     request.SendYZ();
        //     return request;
        // }
        //
        // public YZServerRequest YZRequestCheckTongDun(string token)
        // {
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZCheckTongDun, YZMethodPost);
        //     request.AddYZParam("token", token);
        //     request.SendYZ();
        //     return request;
        // }
        //
        // public YZServerRequest YZRequestCheckKyc(int type)
        // {
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZCheckKyc, YZMethodPost);
        //     request.AddYZParam("type", type);
        //     request.SendYZ();
        //     return request;
        // }

        // #region BingoSlots
        //
        // public YZServerRequest YZCollectBingoSlotsInfo()
        // {
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZBingoSlotsInfo, YZMethodPost);
        //     request.SendYZ();
        //     return request;
        // }
        //
        // public YZServerRequest YZCollectBingoSlotsList(string next)
        // {
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZBingoSlotsList, YZMethodPost);
        //     if (!string.IsNullOrEmpty(next))
        //     {
        //         request.AddYZParam("next", next);
        //     }
        //
        //     request.SendYZ();
        //     return request;
        // }
        //
        // public YZServerRequest YZCollectBingoSlotsClaim()
        // {
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZClaimSlotsBingo, YZMethodPost);
        //     request.SendYZ();
        //     return request;
        // }
        //
        // /// <summary>
        // /// 获取slots结果
        // /// </summary>
        // /// <returns></returns>
        // public YZServerRequest YZBingoSlotsStartSlots()
        // {
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZBingoSlotsBegin, YZMethodPost);
        //     request.SendYZ();
        //     return request;
        // }
        //
        // /// <summary>
        // /// 发送技能使用结果
        // /// </summary>
        // /// <param name="id"></param>
        // /// <param name="index"></param>
        // /// <returns></returns>
        // public YZServerRequest YZBingoSlotsUseSkill(int id, int index)
        // {
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZBingoSlotsSkill, YZMethodPost);
        //     request.AddYZParam("skill", id);
        //     request.AddYZParam("index", index);
        //     request.SendYZ();
        //     return request;
        // }
        //
        // /// <summary>
        // /// 请求下一张bingo棋盘
        // /// </summary>
        // /// <returns></returns>
        // public YZServerRequest YZBingoSlotsNext()
        // {
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZBingoSlotsNext, YZMethodPost);
        //     request.SendYZ();
        //     return request;
        // }
        //
        // #endregion

        // public YZServerRequest YZDidShowUnlockRoom(int room_id)
        // {
        //     if (PlayerManager.Shared.Player.Other.deblock_cartoon != null)
        //     {
        //         PlayerManager.Shared.Player.Other.deblock_cartoon.Add(room_id);
        //     }
        //
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZDeblockRoom, YZMethodPost);
        //     request.AddYZSuccessHandler((str) =>
        //     {
        //         UnlockRoom data = YZGameUtil.JsonYZToObject<UnlockRoom>(str);
        //         if (data != null && data.data != null && data.data.deblock_cartoon != null)
        //         {
        //             PlayerManager.Shared.Player.Other.deblock_cartoon = data.data.deblock_cartoon;
        //         }
        //     });
        //     request.AddYZParam("room_id", room_id);
        //     request.SendYZ();
        //     return request;
        // }

        // public YZServerRequest YZUpdateBounsRoom()
        // {
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZBounsRoom, YZMethodPost);
        //     request.SendYZ();
        //     return request;
        // }
        //
        // public YZServerRequest YZSetUserDeviceInfo(string gps, string zone, string local, string isp, string language)
        // {
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZUserDeviceInfo, YZMethodPost);
        //     if (!string.IsNullOrEmpty(gps))
        //     {
        //         request.AddYZParam("gps", YZGameUtil.Base64YZEncode(gps));
        //     }
        //
        //     if (!string.IsNullOrEmpty(zone))
        //     {
        //         request.AddYZParam("time_zone", zone);
        //     }
        //
        //     if (!string.IsNullOrEmpty(local))
        //     {
        //         request.AddYZParam("location", local);
        //     }
        //
        //     if (!string.IsNullOrEmpty(isp))
        //     {
        //         string platform = YZGameUtil.GetPlatform();
        //         if (platform == YZPlatform.iOS)
        //         {
        //             JsonData data = YZGameUtil.JsonYZToObject<JsonData>(isp);
        //             if (data != null)
        //             {
        //                 foreach (string key in data.Keys)
        //                 {
        //                     if (key == "isp1")
        //                     {
        //                         request.AddYZParam("isp", YZGameUtil.Base64YZEncode(data[key].ToJson()));
        //                     }
        //                     else
        //                     {
        //                         request.AddYZParam(key, YZGameUtil.Base64YZEncode(data[key].ToJson()));
        //                     }
        //                 }
        //             }
        //         }
        //         else
        //         {
        //             request.AddYZParam("isp", YZGameUtil.Base64YZEncode(isp));
        //         }
        //     }
        //
        //     if (!string.IsNullOrEmpty(language))
        //     {
        //         request.AddYZParam("language", language);
        //     }
        //
        //     request.SendYZ();
        //     return request;
        // }
        //
        // /// <summary>
        // /// 领取回归奖励
        // /// </summary>
        // /// <returns></returns>
        // public YZServerRequest YZClaimWelcomeBackRewards()
        // {
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZWelcomeBackClaim, YZMethodPost);
        //     request.YZIsRefreshBalance = false;
        //     request.SendYZ();
        //     return request;
        // }
        //
        //
        // /// <summary>
        // /// 刷新存钱罐信息
        // /// </summary>
        // /// <returns></returns>
        // public YZServerRequest YZRefreshMoneyboxInfo()
        // {
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZMoneyboxInfo, YZMethodPost);
        //     request.SendYZ();
        //     return request;
        // }
        //
        // /// <summary>
        // /// 领取存钱罐奖励
        // /// </summary>
        // /// <returns></returns>
        // public YZServerRequest YZClaimMoneyboxRewards()
        // {
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZMoneyboxClaim, YZMethodPost);
        //     request.YZIsRefreshBalance = false;
        //     request.SendYZ();
        //     return request;
        // }
        //
        // /// <summary>
        // /// 看完广告之后，领取免费bonus奖励
        // /// </summary>
        // /// <returns></returns>
        // public YZServerRequest YZClaimADBonus()
        // {
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZADBonus, YZMethodPost);
        //     request.YZIsRefreshBalance = false;
        //     var ad_info = ADSManager.Shared.YZGetAdsInfo();
        //     if (!string.IsNullOrEmpty(ad_info))
        //     {
        //         request.AddYZParam("ad_bonus_info", ad_info);
        //     }
        //
        //     request.SendYZ();
        //     return request;
        // }
        //
        // /// <summary>
        // /// 看完广告之后，领取免费chips奖励
        // /// </summary>
        // /// <returns></returns>
        // public YZServerRequest YZClaimADChips()
        // {
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZADChips, YZMethodPost);
        //     request.YZIsRefreshBalance = false;
        //     var ad_info = ADSManager.Shared.YZGetAdsInfo();
        //     if (!string.IsNullOrEmpty(ad_info))
        //     {
        //         request.AddYZParam("ad_diamond_info", ad_info);
        //     }
        //
        //     request.SendYZ();
        //     return request;
        // }
        //
        //
        // /// <summary>
        // /// 提现验证码
        // /// </summary>
        // /// <param name="code"></param>
        // /// <returns></returns>
        // public YZServerRequest YZWithdrawPromoCode(string code)
        // {
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZWithdrawCard, YZMethodPost);
        //     request.YZIsRefreshBalance = false;
        //     request.AddYZParam("code", code);
        //     request.SendYZ();
        //     return request;
        // }
        //
        // /// <summary>
        // /// 获取明显列表
        // /// </summary>
        // /// <returns></returns>
        // public YZServerRequest YZStreamList()
        // {
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZStreamList, YZMethodPost);
        //     request.SendYZ();
        //     return request;
        // }


        // /// <summary>
        // /// 刷新货币
        // /// </summary>
        // /// <returns></returns>
        // public YZServerRequest YZRefreshBalance()
        // {
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZRefreshBalance, YZMethodPost);
        //     request.AddYZSuccessHandler((str) => { YZWaitingUICtrler.Shared().YZOnCloseUI(); });
        //     request.AddYZFailureHandler((code, str) => { YZWaitingUICtrler.Shared().YZOnCloseUI(); });
        //     request.SetYZUseAuth(true);
        //     request.SendYZ();
        //     YZWaitingUICtrler.Shared().YZOnOpenUI();
        //     return request;
        // }
        //
        // public YZServerRequest YZSendAgeCheckData(string jsonStr)
        // {
        //     YZServerRequest request = CreateYZRequest(YZServerApiPath.YZSupportCenterAgeCheck, YZMethodPost);
        //     request.AddYZParam("info", jsonStr);
        //     request.AddYZSuccessHandler((str) =>
        //     {
        //         LitJson.JsonData data = YZGameUtil.JsonYZToObject<LitJson.JsonData>(str);
        //         if (YZJsonUtil.ContainsYZKey(data, "data") && YZJsonUtil.ContainsYZKey(data["data"], "age_verify"))
        //         {
        //             PlayerManager.Shared.Player.data.other.safety_data.age_verify =
        //                 YZJsonUtil.GetYZInt(data["data"], "age_verify");
        //         }
        //     });
        //     request.SetYZUseAuth(true);
        //     request.SendYZ();
        //     return request;
        // }

        #endregion
    }
}