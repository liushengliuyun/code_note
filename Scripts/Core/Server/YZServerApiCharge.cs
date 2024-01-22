using iOSCShape;
using LitJson;
using System.Collections.Generic;
using Core.Controllers;
using Core.Controls;
using Core.Extensions;
using Core.Manager;
using Core.Models;
using Core.Services.NetService.API.Facade;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using UI;
using UI.UIChargeFlow;
using Utils;

namespace Core.Server
{
    public class YZServerApiCharge : YZBaseController<YZServerApiCharge>
    {
        public string brcurrentorderno;
        public int YZCurrentChargeABType = BestABType.X;

        public int YZCurrentChargeID
        {
            get => brchargeid;
            set => brchargeid = value;
        }
        
        #region 私有成员

        private int brchargeid;
        private bool brissyncchargeinfo;
        private string brcurrentapporderid;
        private JsonData brchargechannel;
        public JsonData brcurrentorderinfor;
        private JsonData brwebviewstepdata;
        private PaymentsDetails brcachepaymentinfo;

        private string paymentType;

        private int webviewStartTime;
        private int webviewCompleteTime;
        private string urlCurrent;

        #endregion

        #region Logic Functions

        public void YZInitializeDepositData()
        {
            brchargeid = 0;
            brchargechannel = null;
            brcurrentorderinfor = null;
            brissyncchargeinfo = false;
            brcachepaymentinfo = new PaymentsDetails();
        }

        /// <summary>
        /// 获取某种充值方式是否合法可用
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool YZIsValidChargeChannel(string type)
        {
            if (Root.Instance.ChargeMethodsList == null || 
                Root.Instance.ChargeMethodsList.Count <= 0)
            {
                return false;
            }

            foreach (var value in Root.Instance.ChargeMethodsList)
            {
                if (value.type.Equals(type))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 将收集的订单数据，转换为一个字符串变量，方便传递给充值中心
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private string YZConvertOrderInfoToString(string api)
        {
            if (brcachepaymentinfo == null)
                brcachepaymentinfo = new PaymentsDetails();

            if (YZJsonUtil.ContainsYZKey(brcurrentorderinfor, "charge_center_url"))
            {
                // url
                brcachepaymentinfo.charge_center_url =
                    YZString.Concat(brcurrentorderinfor["charge_center_url"].ToString(), api);
            }

            if (brcurrentorderinfor != null)
            {
                // 参数
                brcachepaymentinfo.details.Clear();
                foreach (var key in brcurrentorderinfor.Keys)
                {
                    if (!key.Equals("charge_center_url"))
                    {
                        var paramItem = new DetailItem()
                        {
                            param_name = key,
                            param_value = brcurrentorderinfor[key].ToString()
                        };
                        brcachepaymentinfo.details.Add(paramItem);
                    }
                }
            }

            return JsonMapper.ToJson(brcachepaymentinfo);
        }

        // /// <summary>
        // /// 解析展示充值获得
        // /// </summary>
        // /// <param name="balance"></param>
        // public void YZGetDepositRewardsList(ref List<YZReward> rewards, Charge_configsItem chargeData)
        // {
        //     rewards.Clear();
        //     //var chargeData = ChargeManager.Shared.YZGetChargeById(brchargeid);
        //     if (chargeData != null)
        //     {
        //         Dictionary<int, YZReward> dicRewards = new Dictionary<int, YZReward>();
        //         //--主奖励
        //         YZReward main = new YZReward(RewardType.Money, chargeData.amount.ToDouble());
        //         dicRewards.Add(RewardType.Money, main);
        //         // 额外奖励
        //         if (chargeData.extra_items != null)
        //         {
        //             foreach (var key in chargeData.extra_items.Keys)
        //             {
        //                 int type = int.Parse(key);
        //                 double value = YZJsonUtil.GetYZFloat(chargeData.extra_items, key);
        //                 if (dicRewards.TryGetValue(type, out YZReward tem))
        //                 {
        //                     tem.amount += value;
        //                 }
        //                 else
        //                 {
        //                     tem = new YZReward(type, value);
        //                     dicRewards.Add(type, tem);
        //                 }
        //             }
        //         }
        //
        //         // 礼物奖励
        //         if (chargeData.gift_items != null)
        //         {
        //             foreach (var key in chargeData.gift_items.Keys)
        //             {
        //                 int type = int.Parse(key);
        //                 double value = YZJsonUtil.GetYZFloat(chargeData.gift_items, key);
        //                 if (dicRewards.TryGetValue(type, out YZReward tem))
        //                 {
        //                     tem.amount += value;
        //                 }
        //                 else
        //                 {
        //                     tem = new YZReward(type, value);
        //                     dicRewards.Add(type, tem);
        //                 }
        //             }
        //         }
        //
        //         // 如果是翻卡充值159的DE组，还需要加上额外赠送的奖励
        //         if (chargeData.position.ToInt() == ChargePostion.best_value_159)
        //         {
        //             var flipInfo = PlayerManager.Shared.Player.Other.flip_charge_info;
        //             if (flipInfo != null && flipInfo.card_type == 2)
        //             {
        //                 for (int i = 0; i < flipInfo.charge_card.Count; ++i)
        //                 {
        //                     var temReward = flipInfo.charge_card[i].GetReward();
        //                     if (temReward != null)
        //                     {
        //                         if (dicRewards.TryGetValue(temReward.type, out YZReward tem))
        //                         {
        //                             tem.amount += temReward.amount;
        //                         }
        //                         else
        //                         {
        //                             tem = new YZReward(temReward.type, temReward.amount);
        //                             dicRewards.Add(temReward.type, tem);
        //                         }
        //                     }
        //                 }
        //             }
        //         }
        //
        //         // 如果是钓鱼充值163，还需要加上额外赠送的奖励
        //         if (chargeData.position.ToInt() == ChargePostion.best_value_163)
        //         {
        //             var fishCfg = PlayerManager.Shared.Config.data.other.fish;
        //             if (fishCfg != null)
        //             {
        //                 foreach (var key in fishCfg.charge_hook_extra.Keys)
        //                 {
        //                     int chargeID = key.ToInt();
        //                     int value = YZJsonUtil.GetYZInt(fishCfg.charge_hook_extra, key);
        //                     if (chargeID == chargeData.id)
        //                     {
        //                         if (dicRewards.TryGetValue(RewardType.FishHook, out YZReward tem))
        //                         {
        //                             tem.amount += value;
        //                         }
        //                         else
        //                         {
        //                             tem = new YZReward(RewardType.FishHook, value);
        //                             dicRewards.Add(RewardType.FishHook, tem);
        //                         }
        //
        //                         break;
        //                     }
        //                 }
        //             }
        //         }
        //
        //         // 将获取到的奖励返回
        //         foreach (var value in dicRewards.Values)
        //         {
        //             if (value != null)
        //             {
        //                 rewards.Add(value);
        //             }
        //         }
        //     }
        // }
        //
        // /// <summary>
        // /// 获取奖励弹窗上的描述文本
        // /// </summary>
        // private string YZGetRewardDescription(Charge_configsItem chargeData, List<YZReward> rewards)
        // {
        //     if (chargeData == null)
        //         return null;
        //
        //     int position = chargeData.position.ToInt();
        //     if (position == ChargePostion.best_value_159)
        //     {
        //         var flipInfo = PlayerManager.Shared.Player.Other.flip_charge_info;
        //         if (flipInfo != null)
        //         {
        //             if (flipInfo.card_type == 2)
        //             {
        //                 double totalBonus = 0;
        //                 for (int i = 0; i < rewards.Count; ++i)
        //                 {
        //                     if (rewards[i].type == RewardType.Bonus)
        //                         totalBonus += rewards[i].amount;
        //                 }
        //
        //                 return YZString.Format(YZLocal.GetLocal(YZLocalID.key_card_8),
        //                     YZNumberUtil.FormatYZMoney(totalBonus.ToString()));
        //             }
        //         }
        //     }
        //
        //     return null;
        // }

        /// <summary>
        /// 充值成功记录
        /// </summary>
        private void YZChargeSuccess(Charge_configsItem chargeData)
        {
            //-- 成功
            //YZTopControl.YZShowAutoHideTips(YZLocal.GetLocal(YZLocalID.key_deposit_succeed));
            UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("deposit_succeed"));
            //-- 尝试存储这一次的充值方式
            UIChargeCtrl.Shared().YZTryToSaveChargeType();
            //-- 打点
            //PlayerManager.Shared.AddSavedCount(FunnelEventParam.brisfirstpay);
            // //-- 刷新充值活动的皮肤
            // if (chargeData.position.ToInt() == ChargePostion.best_value_2001)
            // {
            //     ChargeManager.Shared.ChangeChagre2001SkinType();
            // }
        
            // //-- 刷新活动入口
            // YZMainUICtrler.Shared().YZRefeshTopEvents();
            // //-- 刷新个人信息
            // YZMineUICtrler.Shared().YZRefreshUI();
            // //-- 刷新商城界面
            // YZMallUICtrler.Shared().YZDidOpenUI();
            // //-- 刷新特殊充值
            // if (YZCurrentChargeABType == BestABType.A)
            // {
            //     YZDataUtil.SetYZInt(YZConstUtil.YZABChargeSuccInt, 1);
            //     YZDataUtil.SetYZInt(YZConstUtil.YZAChargeTimerInt, 0);
            // }
            // else if (YZCurrentChargeABType == BestABType.B)
            // {
            //     YZDataUtil.SetYZInt(YZConstUtil.YZBChargeTypeInt, 0);
            //     YZDataUtil.SetYZInt(YZConstUtil.YZBChargeTimerInt, 0);
            // }
            //
            // //-- 刷新风险状态
            // KYCManager.Shared.ResetKYCChargeSucc();
        }

        #endregion

        #region 与业务服通信

        // /// <summary>
        // /// 请求刷新可用的充值渠道
        // /// </summary>
        // /// <returns></returns>
        // public YZServerRequest YZRefreshChannel()
        // {
        //     UserInterfaceSystem.That.ShowUI<UIWaitingCtrler>();
        //     YZServerRequest request =
        //         YZServerApi.Shared.CreateYZRequest(YZServerApiPath.YZChargeCenterChannel, YZServerApi.YZMethodPost);
        //
        //     request.AddYZParam("country", LocationManager.Shared.GetLocation().ISOcountryCode);
        //     request.AddYZParam("state", LocationManager.Shared.GetLocation().administrativeArea);
        //     request.AddYZSuccessHandler((json) =>
        //     {
        //         UIWaitingCtrler.Shared().YZOnCloseUI();
        //         brchargechannel = JsonMapper.ToObject(json)["data"]["channel_info"];
        //     });
        //     request.AddYZFailureHandler((code, msg) => { UIWaitingCtrler.Shared().YZOnCloseUI(); });
        //     request.YZIsRefreshBalance = true;
        //     request.SetYZUseAuth(true);
        //     request.SendYZ();
        //     return request;
        // }

        /// <summary>
        /// 请求创建一个充值订单
        /// </summary>
        /// <returns></returns>
        public void YZRequestCreateOrder(int chargeId, int discountid, string discountAmount, string newPrice,
            System.Action callback, string payment_type)
        {
            paymentType = payment_type;

            UserInterfaceSystem.That.ShowUI<UIWaitingCtrler>();
            
            // 充值打点：提交订单
            var chargeItem = UIChargeCtrl.Shared().ChargeItem;
            Root.Instance.SubmitChargeOrderTimeStamp = TimeUtils.Instance.UtcTimeNow;
            int duration = Root.Instance.SubmitChargeOrderTimeStamp - Root.Instance.OpenChargeChannelTimeStamp;
            Dictionary<string, object> properties = new Dictionary<string, object>()
            {
                {
                    FunnelEventParam.brisfirstpay,
                    YZDataUtil.GetYZInt(YZConstUtil.YZIsLastDepositSuccess, 0) <= 0
                },
                {"session_id", Root.Instance.SessionId},
                {"duration", duration},
                {"pay_enter_name", chargeItem.position},
                {"charge_id", chargeItem.id}
            };
            YZFunnelUtil.SendYZEvent("order_submit", properties);
            
            NetSystem.That.SetFailCallBack(s => UserInterfaceSystem.That.RemoveUIByName(nameof(UIWaitingCtrler)));
            MediatorRequest.Instance.CreateCharegeOrder(chargeId, discountid, discountAmount, newPrice, callback, 
                chargeItem, payment_type);

            

            // YZServerRequest request =
            //     YZServerApi.Shared.CreateYZRequest(YZServerApiPath.YZChargeCenterCreate, YZServerApi.YZMethodPost);
            // request.AddYZParam("charge_id", discountid);
            // request.AddYZSuccessHandler((json) =>
            // {
            //     UserInterfaceSystem.That.RemoveUIByName("UIWaitingCtrler");
            //     //UIWaitingCtrler.Shared().YZOnCloseUI();
            //     // 缓存订单数据
            //     brchargeid = chargeId;
            //     var responceJson = JsonMapper.ToObject(json);
            //     if (responceJson == null || !YZJsonUtil.ContainsYZKey(responceJson, "data") ||
            //         !YZJsonUtil.ContainsYZKey(responceJson["data"], "order_info"))
            //     {
            //         return;
            //     }
            //
            //     brcurrentorderinfor = JsonMapper.ToObject(json)["data"]["order_info"];
            //     // 往订单数据中填入用户ID
            //     brcurrentorderinfor["app_user_id"] = PlayerManager.Shared.YZUserId;
            //     // 往订单数据中填入货币信息
            //     // 货币类型
            //     if (!YZJsonUtil.ContainsYZKey(brcurrentorderinfor, "currency"))
            //     {
            //         brcurrentorderinfor["currency"] = "USD";
            //     }
            //
            //     // 货币金额
            //     if (!string.IsNullOrEmpty(discountAmount) && !YZJsonUtil.ContainsYZKey(brcurrentorderinfor, "amount"))
            //     {
            //         brcurrentorderinfor["amount"] = discountAmount;
            //     }
            //
            //     //
            //     callback.Invoke();
            // });
            // request.AddYZFailureHandler((code, msg) =>
            // {
            //     UserInterfaceSystem.That.RemoveUIByName("UIWaitingCtrler");
            //     //UIWaitingCtrler.Shared().YZOnCloseUI();
            // });
            // //
            // request.SetYZUseAuth(true);
            // request.SendYZ();
            //
            // return request;
        }

        /// <summary>
        /// 请求查询订单状态
        /// </summary>
        /// <returns></returns>
        public void YZRequestQueryOrder(string orderId)
        {
            if (brissyncchargeinfo)
            {
                return ;
            }

            brissyncchargeinfo = true;
            UserInterfaceSystem.That.ShowUI<UIWaitingCtrler>();
            MediatorRequest.Instance.QueryOrder(orderId);
            //var chargeData = ChargeManager.Shared.YZGetChargeById(brchargeid);
            // YZServerRequest request =
            //     YZServerApi.Shared.CreateYZRequest(YZServerApiPath.YZChargeCenterQuery, YZServerApi.YZMethodPost);
            // request.AddYZParam("app_order_id", orderId);
            // request.AddYZSuccessHandler((json) =>
            // {
            //     UserInterfaceSystem.That.RemoveUIByName("UIWaitingCtrler");
            //     //UIWaitingCtrler.Shared().YZOnCloseUI();
            //     var data = JsonMapper.ToObject(json)["data"];
            //     var queryInfo = data["query_info"];
            //     var balance = data["balance"];
            //     var status = YZJsonUtil.GetYZInt(queryInfo, "status");
            //     if (status == 3)
            //     {
            //         //-- 1.回传通用后台
            //         if (YZJsonUtil.ContainsYZKey(queryInfo, "net_amount"))
            //         {
            //             YZServerCommon.Shared.SendYZPurchaseWorth(YZJsonUtil.GetYZString(queryInfo, "purchase_id"),
            //                 "Deposit", "1", YZJsonUtil.GetYZString(queryInfo, "net_amount"));
            //         }
            //
            //         //-- 2.更新数据
            //         if (YZJsonUtil.ContainsYZKey(data, "charge_present") && data["charge_present"] == null)
            //         {
            //             //PlayerManager.Shared.Player.Other.charge_present = null;
            //         }
            //
            //         //-- 3.充值成功
            //         //YZChargeSuccess(chargeData);
            //         //-- 4.弹出通用奖励展示界面
            //         // List<YZReward> rewards = new List<YZReward>();
            //         // YZGetDepositRewardsList(ref rewards, chargeData);
            //         // YZShowRewardUICtrler.Shared().OnOpenShowRewardUI(rewards,
            //         //     YZGameUtil.JsonYZToObject<Balance>(balance.ToJson()),
            //         //     YZGetRewardDescription(chargeData, rewards), () =>
            //         //     {
            //         //         YZChargeControl.RefreshUI();
            //         //         if (chargeData.position.ToInt() == ChargePostion.free_bonus_142)
            //         //         {
            //         //             RoomManager.Shared.OnYZClickPlayButton(RoomManager.Shared.YZGetChargeRoomData().id,
            //         //                 null);
            //         //             SFaceManager.Shared.YZSetShowFaceStop();
            //         //             YZChargeJoinRoomUICtrler.Shared().YZOnHideUI();
            //         //         }
            //         //         else if (YZMainUICtrler.Shared().YZCurrentIndex == 2)
            //         //         {
            //         //             YZGameUICtrler.Shared().YZRefreshYZUI();
            //         //             YZGameUICtrler.Shared().YZTyrShowUnlockRoomAnimation();
            //         //         }
            //         //         else
            //         //         {
            //         //             YZGameUICtrler.Shared().YZIsNeedCheckLock = true;
            //         //         }
            //         //     });
            //         // YZChargeUICtrler.Shared().YZOnPopUI();
            //     }
            //     else if (status == 1)
            //     {
            //         UserInterfaceSystem.That.ShowUI<UIWaitingCtrler>();
            //         YZGameUtil.DelayYZAction(() => { YZRequestQueryOrder(orderId); }, 1f);
            //     }
            //     else
            //     {
            //         UserInterfaceSystem.That.RemoveUIByName("UIWaitingCtrler");
            //         //UIWaitingCtrler.Shared().YZOnCloseUI();
            //         // YZTopControl.YZShowAutoHideTips(YZLocal.GetLocal(YZLocalID.key_deposit_failed));
            //         // YZFunnelUtil.SendOrderFail();
            //     }
            // });
            // request.AddYZFailureHandler((code, msg) =>
            // {
            //     UserInterfaceSystem.That.RemoveUIByName("UIWaitingCtrler");
            //     //UIWaitingCtrler.Shared().YZOnCloseUI();
            // });
            // request.SetYZUseAuth(true);
            // request.SendYZ();
            // return;
        }

        #endregion


        #region 与充值中心通信

        /// <summary>
        /// 发起充值请求:信用卡
        /// </summary>
        /// <param name="orderId"></param>
        public void YZDepositCreditCard(CreditCardDetails cardDetails)
        {
            if (cardDetails == null)
            {
                return;
            }

            // 组合支付需要的数据
            brcurrentorderinfor["card_number"] = cardDetails.card_number;
            brcurrentorderinfor["email"] = cardDetails.email;
            brcurrentorderinfor["cvc"] = cardDetails.cvc;
            brcurrentorderinfor["month"] = cardDetails.month;
            brcurrentorderinfor["year"] = cardDetails.year;
            brcurrentorderinfor["first_name"] = cardDetails.first_name;
            brcurrentorderinfor["last_name"] = cardDetails.last_name;

            string strDetails = YZConvertOrderInfoToString("/api/v1/payments/credit_card");

            if (Root.Instance.CardUserName == null)
                Root.Instance.CardUserName = new JsonData();
            
            Root.Instance.CardUserName["last_name"] = cardDetails.last_name;
            Root.Instance.CardUserName["first_name"] = cardDetails.first_name;

            //-- 调用充值中心接口，发起支付请求
            YZServerPaymentsCenter.Shared.RequestPayments(strDetails, YZDepositCallBack);

            //
            UserInterfaceSystem.That.ShowUI<UIWaitingCtrler>();
        }

        /// <summary>
        /// 发起充值请求:Paypal
        /// </summary>
        public void YZDepositPaypal()
        {
            // 组合支付需要的数据
            string strDetails = YZConvertOrderInfoToString("/api/v1/payments/paypal");

            //-- 调用充值中心接口，发起支付请求
            YZServerPaymentsCenter.Shared.RequestPayments(strDetails, YZDepositCallBack);

            //
            UserInterfaceSystem.That.ShowUI<UIWaitingCtrler>();
        }


        /// <summary>
        /// 发起充值请求:ApplePay
        /// </summary>
        public void YZDepositApplePay(string appleAuth64)
        {
            if (string.IsNullOrEmpty(appleAuth64))
            {
                return;
            }

            // 填入apple token数据
            brcurrentorderinfor["token_data"] = appleAuth64;

            // 组合支付需要的数据
            string strDetails = YZConvertOrderInfoToString("/api/v1/payments/apple_pay");

            //-- 调用充值中心接口，发起支付请求
            YZServerPaymentsCenter.Shared.RequestPayments(strDetails, YZDepositCallBack);

            //
            UserInterfaceSystem.That.ShowUI<UIWaitingCtrler>();
        }

        /// <summary>
        /// 提交额外的补充信息
        /// </summary>
        /// <param name="extraInfo"></param>
        public void YZDepositExtraInfoCommit(JsonData extraInfo)
        {
            if (extraInfo == null)
            {
                return;
            }

            // 填入额外数据
            foreach (var key in extraInfo.Keys)
            {
                brcurrentorderinfor[key] = extraInfo[key];
            }

            string strDetails = YZConvertOrderInfoToString("/api/v1/payments/fill_info");

            //-- 调用充值中心接口，发起支付请求
            YZServerPaymentsCenter.Shared.RequestPayments(strDetails, YZDepositCallBack);

            //
            UserInterfaceSystem.That.ShowUI<UIWaitingCtrler>();
        }

        /// <summary>
        /// 充值请求的回调
        /// </summary>
        /// <param name="code"></param>
        /// <param name="str"></param>
        public void YZDepositCallBack(int code, string str)
        {
            UserInterfaceSystem.That.RemoveUIByName("UIWaitingCtrler");
            //UIWaitingCtrler.Shared().YZOnCloseUI();

            brissyncchargeinfo = false;

            if (code != 200)
            {
                //-- 失败
                UserInterfaceSystem.That.ShowUI<UIChargeError>();
                //UIChargeError.Shared().YZOnOpenUI();
                return;
            }

            YZDebug.LogConcat("[Deposit] responce:", str);
            var responce = JsonMapper.ToObject(str);
            var data = responce["data"];
            int status = YZJsonUtil.GetYZInt(responce, "status");
            if (responce != null && status == 1 && data != null)
            {
                //-- 记录订单号
                brcurrentapporderid = data["app_order_id"].ToString();
                brcurrentorderno = data["order_no"].ToString();

                //-- 1.根据step不同派生逻辑分支
                var step = data["step"].ToString();
                if (step.Equals("complete"))
                {
                    //-- 1.1 充值完成：向业务服请求最新的订单状态
                    YZRequestQueryOrder(brcurrentapporderid);
                }
                else if (step.Equals("wait_form"))
                {
                    //-- 1.2 需要补充额外信息
                    //UIChargeExtraInfo.Shared().YZOnOpenUI(data);
                }
                else if (step.Equals("webview"))
                {
                    //-- 1.3 需要打开webview校验
                    if (YZJsonUtil.ContainsYZKey(data, "open_safari") && data["open_safari"].ToString().Equals("1"))
                    {
                        //YZNativeUtil.OpenYZSafariURL(data["url"].ToString());
                    }
                    else
                    {
                        string url = data["url"].ToString();
                        YZDebug.LogConcat("webview url:", url);

                        bool isSendYZEvent = false;   // 是否充值打点
#if UNITY_ANDROID
                        UserInterfaceSystem.That.ShowUI<UIChargeWebview>();
                        UIChargeWebview.Shared().webview_closed_callback = YZInAppWebViewPlayerClosed;
                        UIChargeWebview.Shared().webview_changed_callback = YZInAppWebViewURlChanged;
                        UserInterfaceSystem.That.ShowUI<UIChargeWebview>(url);
                        isSendYZEvent = true;
#elif UNITY_IOS
                        YZInAppWebViewParams viewParams = new YZInAppWebViewParams();
                        viewParams.url = url;
                        viewParams.title = I18N.Get("key_deposit");
                        viewParams.titleSize = "18";
                        viewParams.topBarColor = "#3D68F5";
                        viewParams.progressColor = "#FFBB49";
                        viewParams.progressBgColor = "#C4CCD8";
                        viewParams.tips = "tips";
                        //viewParams.tips = YZLocal.GetLocal(YZLocalID.key_not_app);
                        viewParams.ok = I18N.Get("key_ok");
                        YZNativeUtil.ShowYZWebView(viewParams, YZInAppWebViewPlayerClosed,
                            YZInAppWebViewURlChanged);
                        isSendYZEvent = true;
#endif
                        // 充值打点：充值-打开网页
                        if (isSendYZEvent)
                        {
                            var chargeItem = UIChargeCtrl.Shared().ChargeItem;
                            Dictionary<string, object> properties = new Dictionary<string, object>()
                            {
                                {
                                    FunnelEventParam.brisfirstpay,
                                    YZDataUtil.GetYZInt(YZConstUtil.YZIsLastDepositSuccess, 0) <= 0
                                },
                                {"session_id", Root.Instance.SessionId},
                                {"pay_enter_name", chargeItem.position},
                                {"charge_id", chargeItem.id},
                                {"deposite_type", paymentType},
                                {"deposite_order_id", brcurrentapporderid}
                            };
                            YZFunnelUtil.SendYZEvent("webview_start", properties);
                        }

                        ////
                        //if (YZGameUtil.GetIsiOS())
                        //{
                        //    // // iOS
                        //    // YZInAppWebViewParams viewParams = new YZInAppWebViewParams();
                        //    // viewParams.url = url;
                        //    // viewParams.title = YZLocal.GetLocal(YZLocalID.key_deposit);
                        //    // viewParams.titleSize = "18";
                        //    // viewParams.topBarColor = "#3D68F5";
                        //    // viewParams.progressColor = "#FFBB49";
                        //    // viewParams.progressBgColor = "#C4CCD8";
                        //    // viewParams.tips = YZLocal.GetLocal(YZLocalID.key_not_app);
                        //    // viewParams.ok = YZLocal.GetLocal(YZLocalID.key_OK);
                        //    // YZNativeUtil.ShowYZWebView(viewParams, YZInAppWebViewPlayerClosed,
                        //    //     YZInAppWebViewURlChanged);
                        //}
                        //else
                        //{
                        //    // Android
                        //    UserInterfaceSystem.That.ShowUI<UIChargeWebview>();
                        //    UIChargeWebview.Shared().webview_closed_callback = YZInAppWebViewPlayerClosed;
                        //    UIChargeWebview.Shared().webview_changed_callback = YZInAppWebViewURlChanged;
                        //    UserInterfaceSystem.That.ShowUI<UIChargeWebview>(url);
                        //    // UIChargeWebview.Shared().SetFuncs();
                        //    //UIChargeWebview.Shared().YZOnPushUI(url);
                            
                        //    // 充值打点：充值-打开网页
                        //    var chargeItem = UIChargeCtrl.Shared().ChargeItem;
                        //    Dictionary<string, object> properties = new Dictionary<string, object>()
                        //    {
                        //        {
                        //            FunnelEventParam.brisfirstpay,
                        //            YZDataUtil.GetYZInt(YZConstUtil.YZIsLastDepositSuccess, 0) <= 0
                        //        },
                        //        {"session_id", Root.Instance.SessionId},
                        //        {"pay_enter_name", chargeItem.position},
                        //        {"charge_id", chargeItem.id},
                        //        {"deposite_type", paymentType}, 
                        //        {"deposite_order_id", brcurrentapporderid}
                        //    };
                        //    YZFunnelUtil.SendYZEvent("webview_start", properties);
                        //}

                        //
                        brwebviewstepdata = data;
                    }
                }
            }
            else
            {
                //-- 失败
                var error = responce["error"];
                if (error != null && YZJsonUtil.ContainsYZKey(error, "error_code"))
                {
                    string error_code = error["error_code"].ToString();
                    //UIChargeError.Shared().YZOnOpenUI(error_code);
                    UserInterfaceSystem.That.ShowUI<UIChargeError>();
                }
                else
                {
                    //UIChargeError.Shared().YZOnOpenUI();
                    UserInterfaceSystem.That.ShowUI<UIChargeError>();
                }
            }
        }

        #endregion

        #region webview callback

        /// <summary>
        /// webview关闭回调
        /// </summary>
        /// <param name="msg"></param>
        public void YZInAppWebViewPlayerClosed(string msg)
        {
            YZDebug.LogConcat("webView PlayerClosed:", msg);
            YZRequestQueryOrder(brcurrentapporderid);
        }

        /// <summary>
        /// webview网页跳转的回调
        /// </summary>
        /// <param name="msg"></param>
        public void YZInAppWebViewURlChanged(string msg)
        {
            YZDebug.LogConcat("webView URlChanged:", msg);
            //--1.根据重定向加载状态派生逻辑分支
            var msgData = JsonMapper.ToObject(msg);
            string platform = YZGameUtil.GetPlatform();

            //--1.1.0 健壮性校验
            if (msgData == null || !YZJsonUtil.ContainsYZKey(msgData, "url"))
            {
                if (platform.Equals(YZPlatform.iOS))
                {
                    YZNativeUtil.CloseYZWebView();
                }
                else if (platform.Equals(YZPlatform.Android))
                {
                    UserInterfaceSystem.That.RemoveUIByName("UIChargeWebview");
                }
                else
                {

                }
                //if (YZGameUtil.GetIsiOS())
                //    YZNativeUtil.CloseYZWebView();
                //else
                //    UserInterfaceSystem.That.RemoveUIByName("UIChargeWebview");
                YZRequestQueryOrder(brcurrentapporderid);
                return;
            }
        
            //--1.1.1 判断是否已经完成
            string url = YZJsonUtil.GetYZString(msgData, "url");
            if (YZCheckWebviewIsCompleted(url))
            {
                YZDebug.Log("webView CheckWebviewIsCompleted: true");
                if (platform.Equals(YZPlatform.iOS))
                {
                    YZNativeUtil.CloseYZWebView();
                }
                else if (platform.Equals(YZPlatform.Android))
                {
                    UserInterfaceSystem.That.RemoveUIByName("UIChargeWebview");
                }
                else
                {

                }
                //if (YZGameUtil.GetIsiOS())
                //    YZNativeUtil.CloseYZWebView();
                //else
                //    UserInterfaceSystem.That.RemoveUIByName("UIChargeWebview");

                YZRequestQueryOrder(brcurrentapporderid);
                
                // 充值打点：充值完成
                SendWebViewEnd(url);
                
                // 给服务器传gps信息
                MediatorRequest.Instance.SendChargeGPS();
            }

            if (url.Contains("failure"))
            {
                // 失败处理
                UserInterfaceSystem.That.ShowUI<UIChargeError>();
                
                // 充值打点：充值完成
                SendWebViewEnd(url);
            }

            string loading = YZJsonUtil.GetYZString(msgData, "loading");
            if (loading.Equals("1"))
            {
                //--1.1 开始加载
                //--1.1.2 判断是否进行数据打点
                if (YZJsonUtil.ContainsYZKey(brwebviewstepdata, "event"))
                {
                    for (int i = 0; i < brwebviewstepdata["event"].Count; ++i)
                    {
                        var isEvent = url.Contains(brwebviewstepdata["event"][i]["sub_string"].ToString());
                        if (isEvent)
                        {
                            YZFunnelUtil.SendYZEvent(brwebviewstepdata["event"][i]["event_name"].ToString());
                        }
                    }
                }
        
                // 1.1.3 打网页跳转的点
                //YZFunnelUtil.SendOrderURL(url);

                YZLog.LogColor($"payurl = {url}");
                string keyword = url.Contains("glocash") ? "glocash" : "";
                if (keyword.IsNullOrEmpty())
                {
                    keyword = url.Contains("paypal") ? "paypal" : "";
                }
                
                // 充值打点：充值-加载网页:开始
                var chargeItem = UIChargeCtrl.Shared().ChargeItem;
                Dictionary<string, object> properties = new Dictionary<string, object>()
                {
                    {
                        FunnelEventParam.brisfirstpay,
                        YZDataUtil.GetYZInt(YZConstUtil.YZIsLastDepositSuccess, 0) <= 0
                    },
                    {"session_id", Root.Instance.SessionId},
                    {"pay_enter_name", chargeItem.position},
                    {"charge_id", chargeItem.id},
                    {"deposite_type", paymentType}, 
                    {"deposite_order_id", brcurrentapporderid},
                    {"status", "started"},
                    {"url", url},
                    {"url_keyword", keyword},
                    {"use_time", 0.0f}
                };
                YZFunnelUtil.SendYZEvent("webview_load", properties);
                webviewStartTime = TimeUtils.Instance.UtcTimeNow;

                urlCurrent = url;

            }
            else if (loading.Equals("0"))
            {
                //--1.2 加载完成
                //--1.2.1 判断是否执行js
                if (YZJsonUtil.ContainsYZKey(brwebviewstepdata, "helpers"))
                {
                    for (int i = 0; i < brwebviewstepdata["helpers"].Count; ++i)
                    {
                        var isCallJS = url.Contains(brwebviewstepdata["helpers"][i]["sub_string"].ToString());
                        if (isCallJS)
                        {
                            YZNativeUtil.EvaluateYZJavaScript(brwebviewstepdata["helpers"][i]["content"].ToString());
                        }
                    }
                }

                webviewCompleteTime = TimeUtils.Instance.UtcTimeNow;
                
                YZLog.LogColor($"payurl = {url}");
                string keyword = url.Contains("glocash") ? "glocash" : "";
                if (keyword.IsNullOrEmpty())
                {
                    keyword = url.Contains("paypal") ? "paypal" : "";
                }
                
                // 充值打点：充值-加载网页:完成
                var chargeItem = UIChargeCtrl.Shared().ChargeItem;
                Dictionary<string, object> properties = new Dictionary<string, object>()
                {
                    {
                        FunnelEventParam.brisfirstpay,
                        YZDataUtil.GetYZInt(YZConstUtil.YZIsLastDepositSuccess, 0) <= 0
                    },
                    {"session_id", Root.Instance.SessionId},
                    {"pay_enter_name", chargeItem.position},
                    {"charge_id", chargeItem.id},
                    {"deposite_type", paymentType}, 
                    {"deposite_order_id", brcurrentapporderid},
                    {"status", "finished"},
                    {"url", url},
                    {"url_keyword", keyword},
                    {"use_time", webviewCompleteTime - webviewStartTime}
                };
                YZFunnelUtil.SendYZEvent("webview_load", properties);
            }
        }

        private bool YZCheckWebviewIsCompleted(string url)
        {
            bool isComplted = false;
            for (int i = 0; i < brwebviewstepdata["monitor"]["sub_string"].Count; ++i)
            {
                var subStr = brwebviewstepdata["monitor"]["sub_string"][i].ToString();
                if (url.Contains(subStr))
                {
                    isComplted = true;
                    break;
                }
            }

            return isComplted;
        }

        #endregion

        public void SendWebViewEnd(string url = "")
        {
            if (url == "")
            {
                url = urlCurrent;
            }

            // 充值打点：充值完成
            var webTime = TimeUtils.Instance.UtcTimeNow - webviewCompleteTime;
            var chargeItem = UIChargeCtrl.Shared().ChargeItem;
            Dictionary<string, object> properties = new Dictionary<string, object>()
            {
                {
                    FunnelEventParam.brisfirstpay,
                    YZDataUtil.GetYZInt(YZConstUtil.YZIsLastDepositSuccess, 0) <= 0
                },
                {"session_id", Root.Instance.SessionId},
                {"pay_enter_name", chargeItem.position},
                {"charge_id", chargeItem.id},
                {"deposite_type", paymentType}, 
                {"deposite_order_id", brcurrentapporderid},
                {"url_numb", 1},
                {"url_start", url},
                {"url_finish", url},
                {"web_time", webTime}
            };
            YZFunnelUtil.SendYZEvent("webview_end", properties);
        }
    }
}