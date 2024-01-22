using System.Collections.Generic;
using Core.Controls;
using Core.Extensions;
using Core.Extensions.UnityComponent;
using Core.Manager;
using Core.Server;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils.Static;
using Reactive.Bindings;
using UI.UIChargeFlow;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.UIWithDrawFlow
{
    public class UIWithdrawDetails : UIBase<UIWithdrawDetails>
    {
        [SerializeField] private Button brbtnback;
        [SerializeField] private Button brbtnrecord;
        [SerializeField] private MyButton brbtnwithdraw;
        [SerializeField] private Button brbtnhelpbonus;
        [SerializeField] private Button brbtnhelpfee;

        [SerializeField] private Text brtxterrortips;
        [SerializeField] private Text brtxtaccountbalancevalue;
        [SerializeField] private Text brtxtbonusvalue;
        [SerializeField] private Text brtxtavailablevalue;
        [SerializeField] private Text brtxtprocessingfreevalue;
        [SerializeField] private Text brtxttotalwithdrawvalue;
        [SerializeField] private Text goldText;
        [SerializeField] private Text diamondText;
        [SerializeField] private InputField brinputwithdrawamount;

        [SerializeField] private GameObject brtxttitle;
        [SerializeField] private GameObject brtxtdetails;
        [SerializeField] private GameObject brtxtaccountbalance;
        [SerializeField] private GameObject brtxtbonuscash;
        [SerializeField] private GameObject brtxtavailable;
        [SerializeField] private GameObject brtxtprocessing;
        [SerializeField] private GameObject brtxttotal;
        [SerializeField] private GameObject brtxtforfeited;
        [SerializeField] private GameObject brtxtwithdraw;

        public string YZInputAmount;
        public float feeAmount;

        private bool b_withdraw_valid;
        private float f_cash_flow_value;
        private float f_today;
        private float f_withdraw_limit;


        private static UIWithdrawDetails Inst;

        public static UIWithdrawDetails Shared()
        {
            return Inst;
        }

        void Awake()
        {
            base.Awake();
            Inst = this;
        }
        
        public override void InitEvents()
        {
            AddEventListener(GlobalEvent.Withdraw_Detail,
                (sender, args) =>
                {
                    vm[vname.valid.ToString()].ToIObservable<bool>().Value = Root.Instance.WithdrawValid;
                    vm[vname.flow.ToString()].ToIObservable<float>().Value = Root.Instance.WithdrawCashFlow;
                    vm[vname.todoy.ToString()].ToIObservable<float>().Value = Root.Instance.WithdrawToday;
                });

        }

        public override void OnStart()
        {
            brbtnback.onClick.AddListener(() => { Close(); });

            brbtnrecord.onClick.AddListener(() =>
            {
                //YZWithdrawCashRecordUICtrler.Shared().YZOnPushUI();
                //UserInterfaceSystem.That.ShowUI<UIWithdrawRecord>();
                MediatorRequest.Instance.WithdrawHistory();
            });
            
            brbtnhelpbonus.onClick.AddListener(()=>
            {
                UserInterfaceSystem.That.ShowUI<UIWithdrawBonus>();
            });
            
            brbtnhelpfee.onClick.AddListener(() =>
            {
                UserInterfaceSystem.That.ShowUI<UIWithdrawProcessingFee>();
            });

            brbtnwithdraw.onClick.AddListener(() =>
            {
                // TODO 定位检查
                // LocationManager.Shared.IsLocationValid(YZSafeType.Withdraw, null,
                //     YZInputAmount.ToDouble(), () =>
                //     {
                //         
                //     });

                //b_withdraw_valid = true;
                
                if (b_withdraw_valid)
                {
                    bool canWitchdraw = true;
                    List<string> args = new List<string>();

                    if (Root.Instance.FortuneWheelInfo.HaveChance)
                    {
                        canWitchdraw = false;
                        args.Add(" \"Fortune Wheel\"");
                    }

                    if (Root.Instance.MagicBallInfo is { EnoughClaimReward : true })
                    {
                        canWitchdraw = false;
                        args.Add(" \"Wizard's Treasure\"");
                    }

                    var museumCount = Root.Instance.MuseumInfo?.RewardCount ?? 0;
                    if (museumCount > 0)
                    {
                        canWitchdraw = false;
                        args.Add(" \"Museum\"");
                    }

                    if (Root.Instance.CurTaskInfo.CanGetReward)
                    {
                        canWitchdraw = false;
                        args.Add(" \"Quest\"");
                    }
                    
                    // 需要检查 未领取的对局奖励
                    //todo
                    if (Root.Instance.MatchHistory.Exists(history => history.CanClaimWhenWithDraw
                            && (history.rewards.ContainsKey(Const.Bonus) || history.rewards.ContainsKey(Const.Cash))
                        ))
                    {
                        canWitchdraw = false;
                        args.Add(" \"Records\"");
                    }

                    if (args.Count > 0)
                    {
                        string[] argsArr = new string[5];
                        for (int i = 0; i < args.Count; ++i)
                            argsArr[i] = args[i];
                        for (int i = args.Count; i < 5; ++i)
                            argsArr[i] = "";

                        UserInterfaceSystem.That.ShowUI<UIConfirm>(new UIConfirmData
                        {
                            Type = UIConfirmData.UIConfirmType.OneBtn,
                            desc = I18N.Get("key_withdraw_record_hint", argsArr),
                            title = I18N.Get("key_opps"),
                            confirmTitle = I18N.Get("key_collect"),
                            HideCloseBtn = false,
                            confirmCall = JumpToRecord
                        });
                        
                        if (!canWitchdraw)
                            return;
                    }

                    //YZWithdrawConfirmUICtrler.Shared().YZOnOpenUI(YZInputAmount, feeAmount);
                    UserInterfaceSystem.That.ShowUI<UIWithdrawConfirm>(YZInputAmount, feeAmount);

                    Root.Instance.WithdrawAmount = YZInputAmount;
                }
                else
                {
                    //YZTopControl.YZShowAutoHideTips(YZLocal.GetLocal(YZLocalID.key_withdraw_service_fee_error));
                    UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_withdraw_service_fee_error"));
                    RefreshCashFlow();
                }
            });
            
            brinputwithdrawamount.onEndEdit.AddListener(YZOnInputTextChanged);
            
            RefreshUI();

            // 没有取到 withdrawInfo，说明需要从头开始绑定信息
            if (Root.Instance.Role.withdrawInfo != null)
            {
                // 更新本地信息
                YZLog.LogColor("validate: " + Root.Instance.Role.withdrawInfo.validate 
                                            + " email: " + Root.Instance.Role.withdrawInfo.email
                                            + " account_ready:" + Root.Instance.Role.withdrawInfo.account_ready);
                
                YZDataUtil.SetYZInt(YZConstUtil.YZWithdrawMailVerified, Root.Instance.Role.withdrawInfo.validate);
                YZDataUtil.SetYZString(YZConstUtil.YZWithdrawEmail, Root.Instance.Role.withdrawInfo.email);
                YZDataUtil.SetYZInt(YZConstUtil.YZWithdrawNameAndAddressVerified, Root.Instance.Role.withdrawInfo.account_ready);
            }
            
            // 计算limit
            f_withdraw_limit = Root.Instance.Role.TotalCharge >= 1000 ? 200 : 100;
        }

        private void JumpToRecord()
        {
            Close();
            UserInterfaceSystem.That.RemoveUIByName("UIPlayerInfo");
            UIMain.Shared().CloseSettingPanel();
            UIMain.Shared().HistoryToggle.isOn = true;
        }

        void OnEnable()
        {
            YZDebug.Log("UI withdraw enalbe");
            Refresh();
        }
        
        enum vname
        {
            valid,
            flow,
            todoy
        }


        public override void InitVm()
        {
            vm[vname.valid.ToString()] = new ReactivePropertySlim<bool>(Root.Instance.WithdrawValid);
            vm[vname.flow.ToString()] = new ReactivePropertySlim<float>(Root.Instance.WithdrawCashFlow);
            vm[vname.todoy.ToString()] = new ReactivePropertySlim<float>(Root.Instance.WithdrawToday);
        }

        public override void InitBinds()
        {
            vm[vname.valid.ToString()].ToIObservable<bool>().Subscribe(value=>
            {
                b_withdraw_valid = value;
                brbtnwithdraw.Gray = !b_withdraw_valid;
                brbtnwithdraw.interactable = b_withdraw_valid;

                YZRefreshFee();
            });
            vm[vname.flow.ToString()].ToIObservable<float>().Subscribe(value =>
            {
                f_cash_flow_value = value;
            });
            vm[vname.todoy.ToString()].ToIObservable<float>().Subscribe(value =>
            {
                f_today = value;
            });
        }

        private void YZOnInputTextChanged(string text)
        {
            if (float.TryParse(text, out float i))
            {
                YZInputAmount = text;
                if (i < 10f)
                {
                    // 单笔提现小于10
                    brtxterrortips.text = I18N.Get("key_tips_withdraw_minimum");
                    brbtnwithdraw.Gray = true;
                    brbtnwithdraw.interactable = false;
                }
                else if (!Mathf.Approximately(i, Root.Instance.Role.GetCash()) &&
                         i > Root.Instance.Role.GetCash())
                {
                    // 金额不足
                    brtxterrortips.text =  I18N.Get("key_cannot_exceed");
                    brbtnwithdraw.Gray = true;
                    brbtnwithdraw.interactable = false;
                }
                else if (i > f_withdraw_limit || i + f_today > f_withdraw_limit)
                {
                    brtxterrortips.text = I18N.Get("key_tips_withdraw_limit", f_withdraw_limit);
                    brbtnwithdraw.Gray = true;
                    brbtnwithdraw.interactable = false;
                }
                else if (i <= 0)
                {
                    // 单笔提现小于0
                    brtxterrortips.text = I18N.Get("key_tips_withdraw_minimum");
                    brbtnwithdraw.Gray = true;
                    brbtnwithdraw.interactable = false;
                }
                else
                {
                    brtxterrortips.text = "";
                    brbtnwithdraw.Gray = false;
                    brbtnwithdraw.interactable = true;
                }
            }
            else
            {
                YZInputAmount = "0";
                brtxterrortips.text = "";
                brbtnwithdraw.Gray = true;
                brbtnwithdraw.interactable = false;
            }
        
            YZRefreshFee();
        }
        
        private void YZRefreshFee()
        {
            float amount = YZInputAmount.ToFloat();
            float totalAmount;
            if (amount <= 0)
            {
                feeAmount = 0;
                totalAmount = 0;
            }
            else if (f_cash_flow_value < amount)
            {
                feeAmount = 0.1f * amount;
                totalAmount = amount - feeAmount;
                b_withdraw_valid = true;
            }else if (f_cash_flow_value > amount)
            {
                feeAmount = 0.4f;
                totalAmount = amount - feeAmount;
                b_withdraw_valid = true;
            }
            else
            {
                feeAmount = CalcFeeAmount(amount);
                totalAmount = amount - feeAmount;
            }
            string money_code ="$";
            brtxtprocessingfreevalue.text = YZString.Concat(money_code, feeAmount.ToString("0.00"));
            brtxttotalwithdrawvalue.text = YZString.Concat(money_code, Mathf.Max(0, totalAmount).ToString("0.00"));

            if (totalAmount == 0 && feeAmount == 0)
            {
                YZInputAmount = "0";
                brtxterrortips.text = I18N.Get("key_tips_withdraw_minimum");;
                brbtnwithdraw.Gray = true;
                brbtnwithdraw.interactable = false;
            }
        }
        
        /// <summary>
        /// 刷新流水
        /// </summary>
        private void RefreshCashFlow()
        {
            b_withdraw_valid = false;
            YZOnInputTextChanged("");
            
            MediatorRequest.Instance.GetCashFlow();
        }

        /// <summary>
        /// 计算手续费
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        private float CalcFeeAmount(float amount)
        {
            float result = amount * 0.1f;

            if (b_withdraw_valid && f_cash_flow_value >= amount)
                result = 0.4f;

            return result;
        }
        
        
        public void RefreshUI()
        {
            string money_code = "$";
            brtxtaccountbalancevalue.text = YZString.Concat(money_code, Root.Instance.Role.GetDollars().ToString("0.00"));
            brtxtbonusvalue.text = YZString.Concat(money_code,  Root.Instance.Role.GetBonus().ToString("0.00"));
            brtxtavailablevalue.text = YZString.Concat(money_code, Root.Instance.Role.GetCash().ToString("0.00"));
            goldText.text = Root.Instance.Role.GetItemCount(4).ToString("0");
            diamondText.text = Root.Instance.Role.GetItemCount(2).ToString("0");
            RefreshCashFlow();
        }
    }
}