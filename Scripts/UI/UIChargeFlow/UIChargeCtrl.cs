// namespace UI
// {
//     using LitJson;
//     using UnityEngine;
//     using UnityEngine.UI;
//
//     public class YZChargeUICtrler : YZBaseUICtrler<YZChargeUICtrler>
//     {
//         #region 需要控制的子控件
//
//         private Button YZbtnback;
//         private Text YZtxtmaintitle;
//         private Button YZbtnpolicy;
//         private Button YZbtncontact;
//         private Text YZtxtdepositvalue;
//         private Text YZtxtdepositamount;
//         private Text YZtxtnewbalancevalue;
//         private Text YZtxtnewbalance;
//         private Text YZtxtbonusamount;
//         private Text YZtxtbonusvalue;
//         private Button YZbtnbonushelp;
//         private Button YZbtnapplepay;
//         private Button YZbtnpaypal;
//         private Button YZbtncreditcard;
//         private GameObject YZobjdepositwayspanel;
//         private GameObject YZobjdepositwaystxt;
//         private GameObject YZobjdirectdepositpanel;
//         private Text YZtxtdirectcardid;
//         private Button YZbtndirectdepositedit;
//         private Button YZbtndirectdeposit;
//         private Button YZbtnunbinding;
//         private Button YZbtnotherdepositway;
//         private Toggle YZtglyears;
//         private Button YZbtnterms;
//         private GameObject YZobj_discount;
//         private Text YZtxtdiscountvalue;
//
//         #endregion
//
//         #region 私有变量
//
//         private Charge_configsItem YZchargeitem;
//         private string YZcurrentchoosechargetype;
//
//         public int CurrentChargeID
//         {
//             get => YZchargeitem.id;
//         }
//
//         public string CurrentChargeType
//         {
//             get => YZcurrentchoosechargetype;
//         }
//
//         private CreditCardDetails YZcachecreditcarddetails;
//
//         private long YZorderbegintimestamp;
//
//         public long OrderBeginTimestamp
//         {
//             get => YZorderbegintimestamp;
//         }
//
//         public Toggle Tgl18YearsOld
//         {
//             get => YZtglyears;
//         }
//
//         private bool charge_0109_isBC;
//
//         #endregion
//
//         public override void YZViewDidInit()
//         {
//             base.YZViewDidInit();
//             // 子控件
//             YZUIUtil.YZInitializeCompont(ref YZbtnback, transform, "top_panel/btn_back");
//             YZUIUtil.YZInitializeCompont(ref YZtxtmaintitle, transform, "top_panel/bg_title/txt_title");
//             YZUIUtil.YZInitializeCompont(ref YZbtnpolicy, transform, "bottom_panel/btn_policy");
//             YZUIUtil.YZInitializeCompont(ref YZbtncontact, transform, "bottom_panel/btn_contact");
//             YZUIUtil.YZInitializeCompont(ref YZtglyears, transform, "bottom_panel/tgl_18years");
//             YZUIUtil.YZInitializeCompont(ref YZbtnterms, transform, "bottom_panel/btn_terms");
//             YZUIUtil.YZInitializeCompont(ref YZbtnapplepay, transform, "panel_btns/btn_apple_pay");
//             YZUIUtil.YZInitializeCompont(ref YZbtnpaypal, transform, "panel_btns/btn_paypal");
//             YZUIUtil.YZInitializeCompont(ref YZbtncreditcard, transform, "panel_btns/btn_credit_card");
//             YZUIUtil.YZInitializeGameObject(ref YZobjdepositwayspanel, transform, "panel_btns");
//             YZUIUtil.YZInitializeGameObject(ref YZobjdepositwaystxt, transform, "bg_amount/txt_choose_tips");
//             YZUIUtil.YZInitializeGameObject(ref YZobjdirectdepositpanel, transform, "panel_direct_deposit");
//             YZUIUtil.YZInitializeCompont(ref YZtxtdirectcardid, YZobjdirectdepositpanel.transform,
//                 "btn_edit/txt_card_id");
//             YZUIUtil.YZInitializeCompont(ref YZbtndirectdepositedit, YZobjdirectdepositpanel.transform, "btn_edit");
//             YZUIUtil.YZInitializeCompont(ref YZbtndirectdeposit, YZobjdirectdepositpanel.transform, "btn_deposit");
//             YZUIUtil.YZInitializeCompont(ref YZbtnunbinding, YZobjdirectdepositpanel.transform, "btn_unbinding");
//             YZUIUtil.YZInitializeCompont(ref YZbtnotherdepositway, YZobjdirectdepositpanel.transform,
//                 "btn_changeotherway");
//             YZUIUtil.YZInitializeGameObject(ref YZobj_discount, YZbtncreditcard.transform, "lable_discount");
//             YZUIUtil.YZInitializeCompont(ref YZtxtdiscountvalue, YZobj_discount.transform, "txt_discount");
//             // 按钮回调
//             YZbtnback.SetClick(() => { YZChargeDialogCloseUICtrler.Shared().YZOnOpenUI(); });
//             YZbtnpolicy.SetClick(() =>
//             {
//                 YZGameUtil.PreventYZMultipleClicks(YZbtnpolicy);
//                 YZNativeUtil.OpenYZPrivacyPolicyUrl();
//             });
//             YZbtncontact.SetClick(() =>
//             {
//                 YZGameUtil.PreventYZMultipleClicks(YZbtncontact);
//                 YZNativeUtil.ContactYZUS(EmailPos.Charge);
//             });
//             YZtglyears.onValueChanged.AddListener((isOn) =>
//             {
//                 YZDataUtil.SetYZInt(YZConstUtil.YZDeposit18YearOldInt, isOn ? 1 : 0);
//             });
//             YZbtnterms.SetClick(() =>
//             {
//                 YZGameUtil.PreventYZMultipleClicks(YZbtnterms);
//                 YZNativeUtil.OpenYZPrivacyServiceUrl();
//             });
//             YZbtnapplepay.SetClick(() =>
//             {
//                 YZGameUtil.PreventYZMultipleClicks(YZbtnapplepay);
//                 YZOnClickDepositTypeBtn(ChargeChannelType.ApplePay);
//                 YZFunnelUtil.SendOrderUIChannel(ChargeChannelType.ApplePay);
//             });
//             YZbtnpaypal.SetClick(() =>
//             {
//                 YZGameUtil.PreventYZMultipleClicks(YZbtnpaypal);
//                 YZOnClickDepositTypeBtn(ChargeChannelType.PayPal);
//                 YZFunnelUtil.SendOrderUIChannel(ChargeChannelType.PayPal);
//             });
//             YZbtncreditcard.SetClick(() =>
//             {
//                 YZGameUtil.PreventYZMultipleClicks(YZbtncreditcard);
//                 YZOnClickDepositTypeBtn(ChargeChannelType.CreditCard);
//                 YZFunnelUtil.SendOrderUIChannel(ChargeChannelType.CreditCard);
//             });
//             YZbtndirectdepositedit.SetClick(() =>
//             {
//                 YZGameUtil.PreventYZMultipleClicks(YZbtndirectdepositedit);
//                 YZOnClickDepositTypeBtn(ChargeChannelType.CreditCard);
//             });
//             YZbtndirectdeposit.SetClick(() =>
//             {
//                 YZGameUtil.PreventYZMultipleClicks(YZbtndirectdeposit);
//                 YZOnCreditCardDirectDeposit();
//                 YZFunnelUtil.SendOrderUIChannel(ChargeChannelType.CreditCard);
//             });
//             YZbtnunbinding.SetClick(() =>
//             {
//                 YZGameUtil.PreventYZMultipleClicks(YZbtnunbinding);
//                 //清空缓存
//                 YZDataUtil.SetYZString(YZConstUtil.YZDepositCreditCardDetailsStr, "");
//                 YZSetDirectDepositActive(false);
//             });
//             YZbtnotherdepositway.SetClick(() =>
//             {
//                 YZGameUtil.PreventYZMultipleClicks(YZbtnotherdepositway);
//                 YZSetDirectDepositActive(false);
//             });
//
//             // 多语言
//             YZUILanguage.Init(YZtxtmaintitle.gameObject, YZLocalID.key_deposit_amount);
//
//             //
//             InitUIABGroup(true);
//         }
//
//
//         /// <summary>
//         /// 初始化界面的AB分组
//         /// </summary>
//         /// <param name="isForce">是否强制初始化</param>
//         private void InitUIABGroup(bool isForce = false)
//         {
//             var isBC = ABGroupManager.Shared.YZIsBCGroup(ABTagName.charge_0109);
//             if (!isForce && isBC == charge_0109_isBC)
//                 return;
//
//             charge_0109_isBC = isBC;
//             //
//             GameObject YZobj_info_a = null;
//             GameObject YZobj_info_bc = null;
//             YZUIUtil.YZInitializeGameObject(ref YZobj_info_a, transform, "bg_amount");
//             YZUIUtil.YZInitializeGameObject(ref YZobj_info_bc, transform, "bg_amount_bc");
//             YZobj_info_a.SetActive(!isBC);
//             YZobj_info_bc.SetActive(isBC);
//             //
//             if (charge_0109_isBC)
//             {
//                 YZtxtdepositvalue = null;
//                 YZtxtdepositamount = null;
//                 YZtxtnewbalancevalue = null;
//                 YZtxtnewbalance = null;
//                 YZbtnbonushelp = null;
//                 YZtxtbonusamount = null;
//                 YZtxtbonusvalue = null;
//                 YZUIUtil.YZInitializeCompont(ref YZtxtdepositvalue, transform, "bg_amount_bc/deposit_amount_value");
//                 YZUIUtil.YZInitializeCompont(ref YZtxtdepositamount, transform, "bg_amount_bc/deposit_amount_txt");
//                 YZUIUtil.YZInitializeCompont(ref YZtxtbonusamount, transform, "bg_amount_bc/bonus_amount_txt");
//                 YZUIUtil.YZInitializeCompont(ref YZtxtbonusvalue, transform, "bg_amount_bc/bonus_amount_value");
//                 YZUIUtil.YZInitializeCompont(ref YZtxtnewbalancevalue, transform, "bg_amount_bc/new_balance_value");
//                 YZUIUtil.YZInitializeCompont(ref YZtxtnewbalance, transform, "bg_amount_bc/new_balance_txt");
//                 YZUIUtil.YZInitializeCompont(ref YZbtnbonushelp, transform, "bg_amount_bc/btn_bonus_rule");
//             }
//             else
//             {
//                 YZtxtdepositvalue = null;
//                 YZtxtdepositamount = null;
//                 YZtxtnewbalancevalue = null;
//                 YZtxtnewbalance = null;
//                 YZbtnbonushelp = null;
//                 YZtxtbonusamount = null;
//                 YZtxtbonusvalue = null;
//                 YZUIUtil.YZInitializeCompont(ref YZtxtdepositvalue, transform, "bg_amount/deposit_amount_value");
//                 YZUIUtil.YZInitializeCompont(ref YZtxtdepositamount, transform, "bg_amount/deposit_amount_txt");
//                 YZUIUtil.YZInitializeCompont(ref YZtxtnewbalancevalue, transform, "bg_amount/new_balance_value");
//                 YZUIUtil.YZInitializeCompont(ref YZtxtnewbalance, transform, "bg_amount/new_balance_txt");
//             }
//
//             // 按钮回调
//             if (YZbtnbonushelp != null)
//             {
//                 YZbtnbonushelp.onClick.RemoveAllListeners();
//                 YZbtnbonushelp.SetClick(() =>
//                 {
//                     string txt1 = YZLocal.GetLocal(YZLocalID.key_bonus_cash);
//                     string txt2 = YZLocal.GetLocal(YZLocalID.key_bonus_cash_cannot);
//                     string txt3 = YZLocal.GetLocal(YZLocalID.key_OK);
//                     YZTopControl.YZShowTips(txt1, txt2, txt3, null, null, false);
//                 });
//             }
//
//             // 多语言
//             YZUILanguage.Init(YZtxtdepositamount.gameObject, YZLocalID.key_deposit_amount);
//             YZUILanguage.Init(YZtxtnewbalance.gameObject, YZLocalID.key_new_balance);
//             if (YZtxtbonusamount != null) YZUILanguage.Init(YZtxtbonusamount.gameObject, YZLocalID.key_bonus_cash);
//         }
//
//         public override void YZViewWillShow(params object[] args)
//         {
//             base.YZViewWillShow(args);
//             //
//             InitUIABGroup();
//             //
//             YZDidOpenUI();
//         }
//
//         public override void YZViewDidShow(params object[] args)
//         {
//             base.YZViewDidShow(args);
//
//             // 2. 18岁提示
//             var is18YearsOld = YZDataUtil.GetYZInt(YZConstUtil.YZDeposit18YearOldInt, 1);
//             if (is18YearsOld == 0)
//             {
//                 YZChargeDialogOpenUICtrler.Shared().YZOnOpenUI();
//             }
//             else
//             {
//                 YZChargeDialogOpenUICtrler.Shared().YZOnCloseUI();
//                 YZFunnelUtil.Send18YearOld(true);
//
//                 // 2.1 尝试自动打开applepay充值
//                 YZTryToAutoClickApplePay();
//             }
//
//             YZtglyears.isOn = is18YearsOld == 1;
//         }
//
//         public override bool GetYZAutoDestroy()
//         {
//             return false;
//         }
//
//         public override void YZViewWillDisappear()
//         {
//             base.YZViewWillDisappear();
//
//             //关闭所有子界面
//             YZChargeCardInfoUICtrler.Shared().YZOnCloseUI();
//             YZChargeDialogOpenUICtrler.Shared().YZOnCloseUI();
//             YZChargeDialogCloseUICtrler.Shared().YZOnCloseUI();
//             YZChargeErrorUICtrler.Shared().YZOnCloseUI();
//             YZChargeExtraInfoUICtrler.Shared().YZOnCloseUI();
//         }
//
//         #region Logic Functions
//
//         /// <summary>
//         /// 打开界面
//         /// </summary>
//         /// <param name="chargeCfg"></param>
//         public void YZShowUI(Charge_configsItem chargeCfg, int abType = BestABType.X)
//         {
//             YZchargeitem = chargeCfg;
//             YZServerApiCharge.Shared.YZCurrentChargeABType = abType;
//             YZServerApiCharge.Shared.YZInitializeDepositData();
//             YZFunnelUtil.SendOrderStart(YZchargeitem.id);
//
//             LocationManager.Shared.IsLocationValid(YZSafeType.Charge, null, -1, () =>
//             {
//                 var request = YZServerApiCharge.Shared.YZRefreshChannel();
//                 if (request != null)
//                 {
//                     request.AddYZSuccessHandler((json) => { Shared().YZOnPushUI(); });
//                 }
//             });
//         }
//
//         /// <summary>
//         /// 执行打开界面的自定义逻辑
//         /// </summary>
//         private void YZDidOpenUI()
//         {
//             // 1. 载入数据
//             YZLoadData();
//
//             // 3. 尝试打开快速充值
//             YZTryToOpenDirectDeposit();
//
//             // 4. 打点
//             YZFunnelUtil.SendOrderUIOpen();
//         }
//
//         /// <summary>
//         /// 载入数据
//         /// </summary>
//         private void YZLoadData()
//         {
//             //-- 充值金额
//             YZtxtdepositvalue.text = YZString.Concat(YZLocal.GetLocal(YZLocalID.key_money_code),
//                 YZNumberUtil.FormatYZMoney(YZchargeitem.amount));
//
//             //-- 赠送的bonus
//             float bonusValue = 0;
//             if (YZtxtbonusvalue != null && charge_0109_isBC)
//             {
//                 if (YZchargeitem.extra_items != null)
//                 {
//                     foreach (var key in YZchargeitem.extra_items.Keys)
//                     {
//                         int type = int.Parse(key);
//                         if (type == RewardType.Bonus)
//                         {
//                             bonusValue = YZJsonUtil.GetYZFloat(YZchargeitem.extra_items, key);
//                         }
//                     }
//                 }
//
//                 YZtxtbonusvalue.text = YZString.Concat(YZLocal.GetLocal(YZLocalID.key_money_code),
//                     YZNumberUtil.FormatYZMoney(bonusValue.ToString()));
//             }
//
//             //-- 充值后金额
//             var currentMoney = PlayerManager.Shared.YZTotalMoney;
//             var newMoney = currentMoney + YZchargeitem.amount.ToDouble() + bonusValue;
//             YZtxtnewbalancevalue.text = YZString.Concat(YZLocal.GetLocal(YZLocalID.key_money_code),
//                 YZNumberUtil.FormatYZMoney(newMoney.ToString()));
//
//             //-- 可用的充值方式
//             YZbtnapplepay.gameObject.SetActive(
//                 YZServerApiCharge.Shared.YZIsValidChargeChannel(ChargeChannelType.ApplePay)
//                 && iOSCShape.iOSCShapeApplePayTool.Shared.IOSCanMakePayments());
//             YZbtnpaypal.gameObject.SetActive(YZServerApiCharge.Shared.YZIsValidChargeChannel(ChargeChannelType.PayPal));
//             YZbtncreditcard.gameObject.SetActive(
//                 YZServerApiCharge.Shared.YZIsValidChargeChannel(ChargeChannelType.CreditCard));
//
//             // 是否显示折扣，并替换充值id
//             if (PlayerManager.Shared.Player.CountData.total_charge_times <= 0 &&
//                 !string.IsNullOrEmpty(YZchargeitem.discount_id))
//             {
//                 YZobj_discount.SetActive(true);
//                 YZtxtdiscountvalue.text =
//                     YZString.Format(YZLocal.GetLocal(YZLocalID.key_discount), YZchargeitem.discount_amount);
//             }
//             else
//             {
//                 YZobj_discount.SetActive(false);
//                 YZtxtdiscountvalue.text = string.Empty;
//             }
//         }
//
//         /// <summary>
//         /// 尝试打开快速充值
//         /// </summary>
//         private void YZTryToOpenDirectDeposit()
//         {
//             //--是否有充过值的信用卡信息,并且信用卡渠道可用
//             var saveData = YZDataUtil.GetCreditCardData();
//             bool isValidSaveStr = CheckSaveCreditCardInfoValid(saveData);
//             //
//             if (!isValidSaveStr)
//             {
//                 //清空缓存
//                 YZDataUtil.SetYZString(YZConstUtil.YZDepositCreditCardDetailsStr, "");
//             }
//
//             //
//             var isShow =
//                 saveData != null &&
//                 YZServerApiCharge.Shared.YZIsValidChargeChannel(ChargeChannelType.CreditCard) &&
//                 isValidSaveStr;
//             //--
//             YZSetDirectDepositActive(isShow);
//             //--
//             if (isShow)
//             {
//                 var cardNumber = YZJsonUtil.GetYZString(saveData, "card_number");
//                 var totalStr = YZString.Concat("************", cardNumber.Substring(cardNumber.Length - 4, 4));
//                 YZtxtdirectcardid.text = totalStr;
//             }
//         }
//
//         /// <summary>
//         /// 
//         /// </summary>
//         /// <returns></returns>
//         private bool CheckSaveCreditCardInfoValid(JsonData src)
//         {
//             bool result = false;
//             //
//             if (src != null)
//             {
//                 var cardNumber = YZJsonUtil.GetYZString(src, "card_number");
//                 if (cardNumber.Length > 4)
//                 {
//                     result = true;
//                 }
//             }
//
//             return result;
//         }
//
//         private void YZSetDirectDepositActive(bool isShow)
//         {
//             YZobjdirectdepositpanel.SetActive(isShow);
//             YZobjdepositwayspanel.SetActive(!isShow);
//             YZobjdepositwaystxt.SetActive(!isShow);
//         }
//
//         /// <summary>
//         /// 调用信用卡的快捷充值
//         /// </summary>
//         public void YZOnCreditCardDirectDeposit()
//         {
//             YZcurrentchoosechargetype = ChargeChannelType.CreditCard;
//             YZorderbegintimestamp = YZServerApi.Shared.GetYZServerTime();
//
//             var saveData = YZDataUtil.GetCreditCardData();
//             if (saveData != null)
//             {
//                 //-- 提交信息
//                 var srcCardNumber = YZJsonUtil.GetYZString(saveData, "card_number");
//                 srcCardNumber = srcCardNumber.Replace("-", "");
//                 if (YZcachecreditcarddetails == null)
//                 {
//                     YZcachecreditcarddetails = new CreditCardDetails();
//                 }
//
//                 YZcachecreditcarddetails.card_number = srcCardNumber;
//                 YZcachecreditcarddetails.cvc = YZJsonUtil.GetYZString(saveData, "cvc");
//
//                 YZcachecreditcarddetails.year = YZJsonUtil.GetYZString(saveData, "year");
//                 YZcachecreditcarddetails.month = YZJsonUtil.GetYZString(saveData, "month");
//                 YZcachecreditcarddetails.first_name = YZJsonUtil.GetYZString(saveData, "first_name");
//                 YZcachecreditcarddetails.last_name = YZJsonUtil.GetYZString(saveData, "last_name");
//                 YZcachecreditcarddetails.email = YZJsonUtil.GetYZString(saveData, "email");
//
//                 YZServerApiCharge.Shared.YZRequestCreateOrder(CurrentChargeID, YZGetDiscountChargeID(),
//                     YZGetDiscountChargeAmount(),
//                     () => { YZServerApiCharge.Shared.YZDepositCreditCard(YZcachecreditcarddetails); });
//             }
//         }
//
//
//         /// <summary>
//         /// 执行信用卡解绑操作
//         /// </summary>
//         public void YZOnUnbinding()
//         {
//             //-- 1. 清空信用卡缓存
//             YZDataUtil.SetYZString(YZConstUtil.YZDepositCreditCardDetailsStr, "");
//             //-- 2. 跳转充值方式选择
//             YZOnChangeOtherDepositWay();
//         }
//
//
//         /// <summary>
//         /// 执行更换其他充值方式
//         /// </summary>
//         public void YZOnChangeOtherDepositWay()
//         {
//             YZobjdepositwayspanel.SetActive(true);
//             YZobjdepositwaystxt.SetActive(true);
//             YZobjdirectdepositpanel.SetActive(false);
//         }
//
//         /// <summary>
//         /// 点击不同类型的充值按钮
//         /// </summary>
//         /// <param name="type"></param>
//         public void YZOnClickDepositTypeBtn(string type)
//         {
//             YZcurrentchoosechargetype = type;
//             YZorderbegintimestamp = YZServerApi.Shared.GetYZServerTime();
//
//             if (type.Equals(ChargeChannelType.PayPal))
//             {
//                 YZServerApiCharge.Shared.YZRequestCreateOrder(CurrentChargeID, YZGetDiscountChargeID(),
//                     YZGetDiscountChargeAmount(), YZServerApiCharge.Shared.YZDepositPaypal);
//             }
//             else if (type.Equals(ChargeChannelType.CreditCard))
//             {
//                 YZChargeCardInfoUICtrler.Shared().YZOnPushUI();
//             }
//             else if (type.Equals(ChargeChannelType.ApplePay))
//             {
//                 if (iOSCShape.iOSCShapeApplePayTool.Shared.IOSCanMakePayments())
//                 {
//                     YZServerApiCharge.Shared.YZRequestCreateOrder(CurrentChargeID,
//                         YZGetDiscountChargeID(),
//                         YZGetDiscountChargeAmount(),
//                         () =>
//                         {
//                             YZDebug.LogConcat("[ApplePay]: charge id:", YZchargeitem.id, "; amount:",
//                                 YZchargeitem.amount);
//                             //
//                             if (double.TryParse(YZchargeitem.amount, out double amount))
//                             {
//                                 var apple = new iOSCShape.YZApplePay(amount);
//                                 iOSCShape.iOSCShapeApplePayTool.Shared.IOSStartPay(apple);
//                             }
//                         });
//                 }
//             }
//         }
//
//
//         public void YZTryToSaveChargeType()
//         {
//             YZDataUtil.SetYZString(YZConstUtil.YZDepositCharegeTypeStr, YZcurrentchoosechargetype);
//             if (YZcurrentchoosechargetype.Equals(ChargeChannelType.ApplePay))
//             {
//                 YZDataUtil.SetYZString(YZConstUtil.YZDepositCreditCardDetailsStr, "");
//                 YZSetDirectDepositActive(false);
//             }
//             else if (YZcurrentchoosechargetype.Equals(ChargeChannelType.CreditCard))
//             {
//                 YZChargeCardInfoUICtrler.Shared().YZTryToSaveCreditCardData();
//             }
//         }
//
//         /// <summary>
//         /// 尝试自动点击ApplePay充值渠道
//         /// </summary>
//         public void YZTryToAutoClickApplePay()
//         {
//             if (YZbtnapplepay.gameObject.activeSelf && iOSCShape.iOSCShapeApplePayTool.Shared.IOSCanSupportApplePay())
//             {
//                 // 先取信用卡直冲是否显示，只有不显示的时候，才自动调用applepay的充值。
//                 var saveData = YZDataUtil.GetCreditCardData();
//                 var isShow = saveData != null &&
//                              YZServerApiCharge.Shared.YZIsValidChargeChannel(ChargeChannelType.CreditCard);
//                 //
//                 if (!isShow)
//                     YZbtnapplepay.onClick?.Invoke();
//             }
//         }
//
//
//         /// <summary>
//         /// 获取折扣充值id
//         /// </summary>
//         /// <returns></returns>
//         public int YZGetDiscountChargeID()
//         {
//             if (PlayerManager.Shared.Player.CountData.total_charge_times <= 0
//                 && !string.IsNullOrEmpty(YZchargeitem.discount_id)
//                 && YZcurrentchoosechargetype.Equals(ChargeChannelType.CreditCard))
//             {
//                 int.TryParse(YZchargeitem.discount_id, out int newID);
//                 return newID == 0 ? YZchargeitem.id : newID;
//             }
//             else
//             {
//                 return YZchargeitem.id;
//             }
//         }
//
//         /// <summary>
//         /// 获取折扣充值id
//         /// </summary>
//         /// <returns></returns>
//         public string YZGetDiscountChargeAmount()
//         {
//             if (PlayerManager.Shared.Player.CountData.total_charge_times <= 0
//                 && !string.IsNullOrEmpty(YZchargeitem.discount_id)
//                 && YZcurrentchoosechargetype.Equals(ChargeChannelType.CreditCard))
//             {
//                 return YZchargeitem.discount_amount;
//             }
//             else
//             {
//                 return YZchargeitem.amount;
//             }
//         }
//
//         #endregion
//     }
// }


using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AndroidCShape;
using CatLib.EventDispatcher;
using Core.Extensions.UnityComponent;
using Core.Manager;
using Core.Models;
using Core.Server;
using Core.Services.UserInterfaceService;
using Core.Services.UserInterfaceService.Internal;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using LitJson;
using Reactive.Bindings;
using UI;
using UI.UIChargeFlow;
using UniRx;
using UnityEngine.UI;
using Utils;
using iOSCShape;
using UserInterfaceSystem = Core.Services.UserInterfaceService.API.Facade.UserInterfaceSystem;

public class UIChargeCtrl : UIBase<UIChargeCtrl>
{
    #region UI Variable Statement

    [SerializeField] private MyButton YZbtnback;
    [SerializeField] private Text YZtxtmaintitle;
    [SerializeField] private MyButton YZbtnpolicy;
    [SerializeField] private MyButton YZbtncontact;
    [SerializeField] private MyButton YZbtnterms;
    [SerializeField] private Text YZtxtdepositvalue;
    [SerializeField] private Text YZtxtdepositamount;
    [SerializeField] private Text YZtxtnewbalancevalue;
    [SerializeField] private Text YZtxtnewbalance;
    [SerializeField] private Text YZtxtbonusamount;
    [SerializeField] private Text YZtxtbonusvalue;

    [SerializeField] private MyButton YZbtnbonushelp;

    //private MyButton YZbtnapplepay;
    /// <summary>
    /// paypal付款按钮
    /// </summary>
    [SerializeField] private MyButton YZbtnpaypal;
    [SerializeField] private MyButton YZbtncreditcard;

    [SerializeField] private GameObject YZobjdepositwayspanel;
    [SerializeField] private GameObject YZobjdepositwaystxt;
    [SerializeField] private GameObject YZobjdirectdepositpanel;
    [SerializeField] private Text YZtxtdirectcardid;
    [SerializeField] private MyButton YZbtndirectdepositedit;
    [SerializeField] private MyButton YZbtndirectdeposit;
    [SerializeField] private MyButton YZbtnunbinding;
    [SerializeField] private MyButton YZbtnotherdepositway;
    [SerializeField] private Toggle YZtglyears;
    [SerializeField] private GameObject YZobj_discount;
    [SerializeField] private Text YZtxtdiscountvalue;

    [SerializeField] private ChargeDiscountMono paypalDiscountMono;
    [SerializeField] private ChargeDiscountMono cardDiscountMono;
    [SerializeField] private ChargeDiscountMono directDiscountMono;

    #endregion

    #region 私有变量

    private static UIChargeCtrl Inst;
    private Charge_configsItem YZchargeItem;
    private string YZcurrentChooseChargeType;

    public float cardNewPrice;
    public float paypalNewPrice;

    public static UIChargeCtrl Shared()
    {
        return Inst;
    }
    
    public int CurrentChargeID
    {
        get => YZchargeItem.id;
    }

    public string CurrentChargeType
    {
        get => YZcurrentChooseChargeType;
    }

    public Charge_configsItem ChargeItem => YZchargeItem;

    private CreditCardDetails YZcachecreditcarddetails;

    private long YZorderbegintimestamp;

    public long OrderBeginTimestamp
    {
        get => YZorderbegintimestamp;
    }

    public Toggle Tgl18YearsOld
    {
        get => YZtglyears;
    }

    private bool charge_0109_isBC;

    #endregion

    public override void OnStart()
    {
        EventDispatcher.Root.Raise(nameof(UIChargeCtrl));
        // 冻结的账号
        if (Root.Instance.Role.IsFreeze)
        {
            UserInterfaceSystem.That.ShowUI<UIConfirm>(new UIConfirmData()
            {
                Type = UIConfirmData.UIConfirmType.OneBtn,
                HideCloseBtn = false,
                desc = I18N.Get("key_unusual_activity"),
                confirmTitle = I18N.Get("key_contact_us"),
                WaitCloseCallback = true,
                confirmCall = () =>
                {
                    YZNativeUtil.ContactYZUS(EmailPos.Charge);
                },
                cancleCall = () =>
                {
                    UserInterfaceSystem.That.RemoveUIByName(nameof(UIConfirm));
                }
            });
            YZDebug.Log("账号冻结");
            Close();
            return;
        }
        
        // 按钮回调
        YZbtnback.SetClick(() =>
        {
            // 充值打点： 取消支付 关闭渠道确认弹窗
            Dictionary<string, object> properties = new Dictionary<string, object>()
            {
                {
                    FunnelEventParam.brisfirstpay,
                    YZDataUtil.GetYZInt(YZConstUtil.YZIsLastDepositSuccess, 0) <= 0
                },
                {"session_id", Root.Instance.SessionId},
                {"payment_type", Root.Instance.PaymentType},
                {"charge_id", ChargeItem.id},
                {"pay_enter_name", ChargeItem.position}
            };
            YZFunnelUtil.SendYZEvent("order_close", properties);
            
            //YZChargeDialogCloseUICtrler.Shared().YZOnOpenUI();
            UserInterfaceSystem.That.ShowUI<UIChargeClose>();
        });
        YZbtnpolicy.SetClick(() =>
        {
            YZGameUtil.PreventYZMultipleClicks(YZbtnpolicy);
            YZNativeUtil.OpenYZPrivacyPolicyUrl();
        });
        YZbtncontact.SetClick(() =>
        {
            YZGameUtil.PreventYZMultipleClicks(YZbtncontact);
            YZNativeUtil.ContactYZUS(EmailPos.Charge);
        });
        YZtglyears.onValueChanged.AddListener((isOn) =>
        {
            YZPlayerDataUtil.SetDeposit18YearOldInt(isOn);
            if (!isOn)
                UserInterfaceSystem.That.ShowUI<UIChargeOpen>();
        });
        YZbtnterms.SetClick(() =>
        {
            YZGameUtil.PreventYZMultipleClicks(YZbtnterms);
            YZNativeUtil.OpenYZPrivacyServiceUrl();
        });

        // YZbtnapplepay.SetClick(() =>
        // {
        //     YZGameUtil.PreventYZMultipleClicks(YZbtnapplepay);
        //     YZOnClickDepositTypeBtn(ChargeChannelType.ApplePay);
        //     YZFunnelUtil.SendOrderUIChannel(ChargeChannelType.ApplePay);
        // });

        YZbtnpaypal.gameObject.SetActive(!Root.Instance.IsNaturalFlow_InData);
        YZbtncreditcard.gameObject.SetActive(true);
        
        YZbtnpaypal.SetClick(() =>
        {
            var over18 = YZDataUtil.GetYZInt(YZConstUtil.YZDeposit18YearOldInt, 1);
            if (over18 == 0)
                UserInterfaceSystem.That.ShowUI<UIChargeOpen>();
            else
            {
                YZGameUtil.PreventYZMultipleClicks(YZbtnpaypal);
                YZOnClickDepositTypeBtn(ChargeChannelType.PayPal);
                YZFunnelUtil.SendOrderUIChannel(ChargeChannelType.PayPal, YZchargeItem);
            }
        });
        YZbtncreditcard.SetClick(() =>
        {
            var over18 = YZDataUtil.GetYZInt(YZConstUtil.YZDeposit18YearOldInt, 1);
            if (over18 == 0)
                UserInterfaceSystem.That.ShowUI<UIChargeOpen>();
            else
            {
                YZGameUtil.PreventYZMultipleClicks(YZbtncreditcard);
                YZOnClickDepositTypeBtn(ChargeChannelType.CreditCard);
                YZFunnelUtil.SendOrderUIChannel(ChargeChannelType.CreditCard, YZchargeItem);
            }
        });
        YZbtndirectdepositedit.SetClick(() =>
        {
            var over18 = YZDataUtil.GetYZInt(YZConstUtil.YZDeposit18YearOldInt, 1);
            if (over18 == 0)
                UserInterfaceSystem.That.ShowUI<UIChargeOpen>();
            else
            {

                YZGameUtil.PreventYZMultipleClicks(YZbtndirectdepositedit);
                YZOnClickDepositTypeBtn(ChargeChannelType.CreditCard);
            }
        });
        YZbtndirectdeposit.SetClick(() =>
        {
            var over18 = YZDataUtil.GetYZInt(YZConstUtil.YZDeposit18YearOldInt, 1);
            if (over18 == 0)
                UserInterfaceSystem.That.ShowUI<UIChargeOpen>();
            else
            {
                YZGameUtil.PreventYZMultipleClicks(YZbtndirectdeposit);
                YZOnCreditCardDirectDeposit();
                YZFunnelUtil.SendOrderUIChannel(ChargeChannelType.CreditCard, YZchargeItem);
            }
        });
        YZbtnunbinding.SetClick(() =>
        {
            YZGameUtil.PreventYZMultipleClicks(YZbtnunbinding);
            //清空缓存
            YZDataUtil.SetYZString(YZConstUtil.YZDepositCreditCardDetailsStr, "");
            YZSetDirectDepositActive(false);
        });
        YZbtnotherdepositway.SetClick(() =>
        {
            YZGameUtil.PreventYZMultipleClicks(YZbtnotherdepositway);
            YZSetDirectDepositActive(false);
        });
        
        YZbtnbonushelp.SetClick(()=>
        {
            UserInterfaceSystem.That.ShowUI<UIChargeBonus>();
        });

        // 第一次打开，默认勾上18岁
        if (YZDataUtil.GetYZInt(YZConstUtil.YZFirstOpenDeposit, 0) == 0)
        {
            YZtglyears.isOn = true;
            YZDataUtil.SetYZInt(YZConstUtil.YZFirstOpenDeposit, 1);
            YZPlayerDataUtil.SetDeposit18YearOldInt(true);
        }
        else
        {
            YZtglyears.isOn = YZDataUtil.GetYZInt(YZConstUtil.YZDeposit18YearOldInt, 0) == 1;
        }


        YZDidOpenUI();

        Inst = this;
        
        
        // 打点:打开充值
        Dictionary<string, object> properties = new Dictionary<string, object>()
        {
            { FunnelEventParam.brisfirstpay, YZDataUtil.GetYZInt(YZConstUtil.YZIsLastDepositSuccess, 0) <= 0 },
            {"session_id", Root.Instance.SessionId},
            {"pay_enter_name", YZchargeItem.position},
            {"charge_id", YZchargeItem.id},
            {"ip", YZNativeUtil.GetIPAdress()},
            {"ip_info", DeviceInfoUtils.Instance.GetIpInfoData()},
            {"gps_camouflage", DeviceInfoUtils.Instance.SelfGPSExtra.gps_camouflage},
            {"gps_reject", DeviceInfoUtils.Instance.SelfGPSExtra.gps_reject},
            {"gps", DeviceInfoUtils.Instance.SelfGPSExtra.gps},
            {"gps_info", DeviceInfoUtils.Instance.GetGPSInfoData()}
        };
        YZFunnelUtil.SendYZEvent(FunnelEventID.brorderuiopen, properties);

#if UNITY_ANDROID
        YZAndroidPlugin.Shared.AndroidForterSendEvent("ADD_TO_CART", properties.ToString());
#elif UNITY_IOS
        iOSCShapeForterTool.Shared.IOSForterTrackAction(@"Add_To_Cart", "");
        iOSCShapeRiskifiedTool.Shared.IOSRiskifiedLogRequest(@"Add_To_Cart");
#endif

        Root.Instance.OpenChargeTimeStamp = TimeUtils.Instance.UtcTimeNow;

#if UNITY_ANDROID
//这里有什么用
              UserInterfaceSystem.That.ShowUI<UIChargeWebview>();
        UserInterfaceSystem.That.RemoveUIByName("UIChargeWebview");
#endif
    }

    /// <summary>
    /// 执行打开界面的自定义逻辑
    /// </summary>
    private void YZDidOpenUI()
    {
        // 1. 载入数据
        YZLoadData();

        // 2.尝试打开18岁提示
        // var over18 = YZDataUtil.GetYZInt(YZConstUtil.YZDeposit18YearOldInt, 1);
        // if (over18 == 0)
        // {
        //     UserInterfaceSystem.That.ShowUI<UIChargeOpen>();
        // }

        // 3. 尝试打开快速充值
        YZTryToOpenDirectDeposit();

        // 4. 打点
        //YZFunnelUtil.SendOrderUIOpen();
        
    }

    /// <summary>
    /// 载入数据
    /// </summary>
    private void YZLoadData()
    {
        Charge_configsItem item = (Charge_configsItem)args[0];
        YZchargeItem = item;
        //-- 充值金额
        YZtxtdepositvalue.text = YZString.Concat("$",
            YZNumberUtil.FormatYZMoney(YZchargeItem.amount));

        //-- 赠送的bonus
        float bonusValue = item.bonusValue;
        
        
        YZtxtbonusvalue.text = YZString.Concat("$",
                YZNumberUtil.FormatYZMoney(bonusValue.ToString()));
        

        //-- 充值后金额
        var currentMoney = Root.Instance.Role.GetDollars();
        var newMoney = currentMoney + YZchargeItem.amount.ToDouble() + bonusValue;
        YZtxtnewbalancevalue.text = YZString.Concat("$",
            YZNumberUtil.FormatYZMoney(newMoney.ToString()));
        
        // 打折信息
        byte[] paypayBytes = Convert.FromBase64String(Root.Instance.UserInfo.discount_info.paypal_discount);
        int paypalDiscount = paypayBytes.Length == 2 ? paypayBytes[0] * 10 + paypayBytes[1] : 0;
        int paypalOff = 100 - paypalDiscount;
        
        byte[] glocashBytes = Convert.FromBase64String(Root.Instance.UserInfo.discount_info.glocash_discount);
        int glocashDiscount = glocashBytes.Length == 2 ? glocashBytes[0] * 10 + glocashBytes[1] : 0;
        int glocashOff = 100 - glocashDiscount;

        if (paypalDiscount > 0)
        {
            var newPrice = float.Parse(YZchargeItem.amount)
                * paypalDiscount / 100.0f;
            paypalNewPrice = (float)Math.Round(newPrice, 2);
            paypalDiscountMono.gameObject.SetActive(true);
            paypalDiscountMono.Init(paypalOff,  paypalNewPrice);
        }
        else
        {
            paypalNewPrice = float.Parse(YZchargeItem.amount);
            paypalDiscountMono.gameObject.SetActive(false);
        }

        if (glocashDiscount > 0)
        {
            var newPrice = float.Parse(YZchargeItem.amount)
                * glocashDiscount / 100.0f;
            cardNewPrice = (float)Math.Round(newPrice, 2);
            cardDiscountMono.gameObject.SetActive(true);
            cardDiscountMono.Init(glocashOff,  cardNewPrice);
            
            directDiscountMono.gameObject.SetActive(true);
            directDiscountMono.Init(glocashOff, cardNewPrice);
        }
        else
        {
            cardNewPrice = float.Parse(YZchargeItem.amount);
            cardDiscountMono.gameObject.SetActive(false);
            directDiscountMono.gameObject.SetActive(false);
        }

        // //-- 可用的充值方式
        // YZbtnapplepay.gameObject.SetActive(
        //     YZServerApiCharge.Shared.YZIsValidChargeChannel(ChargeChannelType.ApplePay)
        //     && iOSCShape.iOSCShapeApplePayTool.Shared.IOSCanMakePayments());
        // YZbtnpaypal.gameObject.SetActive(YZServerApiCharge.Shared.YZIsValidChargeChannel(ChargeChannelType.PayPal));
        // YZbtncreditcard.gameObject.SetActive(
        //     YZServerApiCharge.Shared.YZIsValidChargeChannel(ChargeChannelType.CreditCard));

        // // 是否显示折扣，并替换充值id
        // if (PlayerManager.Shared.Player.CountData.total_charge_times <= 0 &&
        //     !string.IsNullOrEmpty(YZchargeItem.discount_id))
        // {
        //     YZobj_discount.SetActive(true);
        //     YZtxtdiscountvalue.text =
        //         YZString.Format(YZLocal.GetLocal(YZLocalID.key_discount), YZchargeItem.discount_amount);
        // }
        // else
        // {
        //     YZobj_discount.SetActive(false);
        //     YZtxtdiscountvalue.text = string.Empty;
        // }
    }

    /// <summary>
    /// 点击不同类型的充值按钮  mark Paypal
    /// </summary>
    /// <param name="type"></param>
    public void YZOnClickDepositTypeBtn(string type)
    {
        YZcurrentChooseChargeType = type;
        YZorderbegintimestamp = YZServerApi.Shared.GetYZServerTime();

        if (type.Equals(ChargeChannelType.PayPal))
        {
            YZServerApiCharge.Shared.YZRequestCreateOrder(CurrentChargeID, YZGetDiscountChargeID(),
                YZGetDiscountChargeAmount(),
                paypalNewPrice.ToString(), YZServerApiCharge.Shared.YZDepositPaypal, "paypal");
        }
        else if (type.Equals(ChargeChannelType.CreditCard))
        {
            //YZChargeCardInfoUICtrler.Shared().YZOnPushUI();
            UserInterfaceSystem.That.ShowUI<UIChargeCardInfo>();
        }
        // else if (type.Equals(ChargeChannelType.ApplePay))
        // {
        //     if (iOSCShape.iOSCShapeApplePayTool.Shared.IOSCanMakePayments())
        //     {
        //         YZServerApiCharge.Shared.YZRequestCreateOrder(CurrentChargeID,
        //             YZGetDiscountChargeID(),
        //             YZGetDiscountChargeAmount(),
        //             () =>
        //             {
        //                 YZDebug.LogConcat("[ApplePay]: charge id:", YZchargeitem.id, "; amount:",
        //                     YZchargeitem.amount);
        //                 //
        //                 if (double.TryParse(YZchargeitem.amount, out double amount))
        //                 {
        //                     var apple = new iOSCShape.YZApplePay(amount);
        //                     iOSCShape.iOSCShapeApplePayTool.Shared.IOSStartPay(apple);
        //                 }
        //             });
        //     }
        // }
    }

    /// <summary>
    /// 尝试打开快速充值
    /// </summary>
    private void YZTryToOpenDirectDeposit()
    {
        //--是否有充过值的信用卡信息,并且信用卡渠道可用
        var saveData = YZDataUtil.GetCreditCardData();
        bool isValidSaveStr = CheckSaveCreditCardInfoValid(saveData);
        if (!isValidSaveStr)
        {
            //清空缓存
            YZDataUtil.SetYZString(YZConstUtil.YZDepositCreditCardDetailsStr, "");
        }
        var isShow =
            saveData != null &&
            YZServerApiCharge.Shared.YZIsValidChargeChannel(ChargeChannelType.CreditCard) &&
            isValidSaveStr;
        
        // 上次确实充值成功才显示
        isShow = isShow && YZDataUtil.GetYZInt(YZConstUtil.YZIsLastDepositSuccess, 0) > 0;
        
        YZSetDirectDepositActive(isShow);
        if (isShow)
        {
            var cardNumber = YZJsonUtil.GetYZString(saveData, "card_number");
            var totalStr = YZString.Concat("************", cardNumber.Substring(cardNumber.Length - 4, 4));
            YZtxtdirectcardid.text = totalStr;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private bool CheckSaveCreditCardInfoValid(JsonData src)
    {
        bool result = false;
        //
        if (src != null)
        {
            var cardNumber = YZJsonUtil.GetYZString(src, "card_number");
            if (cardNumber.Length > 4)
            {
                result = true;
            }
        }
        return result;
    }

    private void YZSetDirectDepositActive(bool isShow)
    {
        YZobjdirectdepositpanel.SetActive(isShow);
        YZobjdepositwayspanel.SetActive(!isShow);
        YZobjdepositwaystxt.SetActive(!isShow);
    }

    /// <summary>
    /// 调用信用卡的快捷充值
    /// </summary>
    public void YZOnCreditCardDirectDeposit()
    {
        YZcurrentChooseChargeType = ChargeChannelType.CreditCard;
        YZorderbegintimestamp = YZServerApi.Shared.GetYZServerTime();

        var saveData = YZDataUtil.GetCreditCardData();
        if (saveData != null)
        {
            //-- 提交信息
            var srcCardNumber = YZJsonUtil.GetYZString(saveData, "card_number");
            srcCardNumber = srcCardNumber.Replace("-", "");
            if (YZcachecreditcarddetails == null)
            {
                YZcachecreditcarddetails = new CreditCardDetails();
            }

            YZcachecreditcarddetails.card_number = srcCardNumber;
            YZcachecreditcarddetails.cvc = YZJsonUtil.GetYZString(saveData, "cvc");

            YZcachecreditcarddetails.year = YZJsonUtil.GetYZString(saveData, "year");
            YZcachecreditcarddetails.month = YZJsonUtil.GetYZString(saveData, "month");
            YZcachecreditcarddetails.first_name = YZJsonUtil.GetYZString(saveData, "first_name");
            YZcachecreditcarddetails.last_name = YZJsonUtil.GetYZString(saveData, "last_name");
            YZcachecreditcarddetails.email = YZJsonUtil.GetYZString(saveData, "email");

            YZServerApiCharge.Shared.YZRequestCreateOrder(CurrentChargeID, YZGetDiscountChargeID(),
                YZGetDiscountChargeAmount(), cardNewPrice.ToString(), 
                () => { YZServerApiCharge.Shared.YZDepositCreditCard(YZcachecreditcarddetails); }, 
                "credit_card");
        }
    }


    public void YZTryToSaveChargeType()
    {
        YZDataUtil.SetYZString(YZConstUtil.YZDepositCharegeTypeStr, YZcurrentChooseChargeType);
        if (YZcurrentChooseChargeType.Equals(ChargeChannelType.ApplePay))
        {
            YZDataUtil.SetYZString(YZConstUtil.YZDepositCreditCardDetailsStr, "");
            YZSetDirectDepositActive(false);
        }
        else if (YZcurrentChooseChargeType.Equals(ChargeChannelType.CreditCard))
        {
            UIChargeCardInfo.Shared().YZTryToSaveCreditCardData();
        }
    }

    /// <summary>
    /// 执行信用卡解绑操作
    /// </summary>
    public void YZOnUnbinding()
    {
        //-- 1. 清空信用卡缓存
        YZDataUtil.SetYZString(YZConstUtil.YZDepositCreditCardDetailsStr, "");
        //-- 2. 跳转充值方式选择
        YZOnChangeOtherDepositWay();
    }

    /// <summary>
    /// 获取折扣充值id
    /// </summary>
    /// <returns></returns>
    public int YZGetDiscountChargeID()
    {
        if (
            //PlayerManager.Shared.Player.CountData.total_charge_times <= 0
            //&&
            !string.IsNullOrEmpty(YZchargeItem.discount_id)
            && YZcurrentChooseChargeType.Equals(ChargeChannelType.CreditCard))
        {
            int.TryParse(YZchargeItem.discount_id, out int newID);
            return newID == 0 ? YZchargeItem.id : newID;
        }
        else
        {
            return YZchargeItem.id;
        }
    }
    
    void OnEnable()
    {
        YZDebug.Log("UI charege enalbe");
    }

    /// <summary>
    /// 获取折扣充值id
    /// </summary>
    /// <returns></returns>
    public string YZGetDiscountChargeAmount()
    {
        if (
            //PlayerManager.Shared.Player.CountData.total_charge_times <= 0
            //&& 
            !string.IsNullOrEmpty(YZchargeItem.discount_id)
            && YZcurrentChooseChargeType.Equals(ChargeChannelType.CreditCard))
        {
            return YZchargeItem.discount_amount;
        }
        else
        {
            return YZchargeItem.amount;
        }
    }

    /// <summary>
    /// 执行更换其他充值方式
    /// </summary>
    public void YZOnChangeOtherDepositWay()
    {
        YZobjdepositwayspanel.SetActive(true);
        YZobjdepositwaystxt.SetActive(true);
        YZobjdirectdepositpanel.SetActive(false);
    }

    public override void InitEvents()
    {
        AddEventListener(GlobalEvent.Toggle_Over18YearsOld,
            (sender, _) =>
            {
                YZtglyears.isOn = true;
                YZPlayerDataUtil.SetDeposit18YearOldInt(true);
                
            });
    }


    enum vname
    {
        chargeMethods,
    }


    public override void InitVm()
    {
        vm[vname.chargeMethods.ToString()] = new ReactivePropertySlim<List<ChargeMethods>>
            (Root.Instance.ChargeMethodsList);
    }

    public override void InitBinds()
    {
        vm[vname.chargeMethods.ToString()].ToIObservable<List<ChargeMethods>>().Subscribe(value =>
        {
            if (!Root.Instance.IsNaturalFlow_InData)
            {
                YZbtnpaypal.gameObject.SetActive(YZServerApiCharge.Shared.YZIsValidChargeChannel(ChargeChannelType.PayPal));
            }
            
            YZbtncreditcard.gameObject.SetActive(
                YZServerApiCharge.Shared.YZIsValidChargeChannel(ChargeChannelType.CreditCard));

        });
    }
}