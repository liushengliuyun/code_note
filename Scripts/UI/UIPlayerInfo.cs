using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using Core.Third.I18N;
using Cysharp.Threading.Tasks;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using Reactive.Bindings;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityTimer;

namespace UI
{
    public class UIPlayerInfo : UIBase<UIPlayerInfo>
    {
        [SerializeField] private Transform WithDrawGroup;
        [SerializeField] private Transform TwoBtns;
        [SerializeField] private Text AllDollarText;
        [SerializeField] private Text CanWithdrawCount;
        [SerializeField] private Text BonusCount;

        [SerializeField] private Image PlayerIcon;
        
        [SerializeField] private Transform NotBindBtns;
        [SerializeField] private MyButton DeleteBtn;
        [SerializeField] private MyButton SignBtn;
        [SerializeField] private MyButton SaveAccountBtn;
        [SerializeField] private MyButton SaveAccountBtn_Big;
        [SerializeField] private MyButton CloseBtn;

        [SerializeField] private MyButton WithDrawBtn;
        [FormerlySerializedAs("CancleWithDrawBtn")] [SerializeField] private MyButton CancelWithDrawBtn;
        [SerializeField] private MyButton EditBtn;
        [SerializeField] private MyButton Text_EditBtn;
        [SerializeField] private MyButton CashFlowBtn;

        [SerializeField] private Text NameText;
        [SerializeField] private Text IdText;
        
        /// <summary>
        /// 及时器是否注册
        /// </summary>
        private bool beenRegister;

        public override void OnStart()
        {
            void openSaveAccount()
            {
                UserInterfaceSystem.That.ShowUI<UIMailLogin>(MailLoginPanel.Bind);
            }

            void openEditPlayerInfo()
            {
                UserInterfaceSystem.That.ShowUI<UISubPlayerInfo>(SubPlayerInfoPanel.Edit);
            }

            WithDrawBtn.SetClick(() =>
            {
                if (WithDrawBtn.Gray)
                {
                    UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_cancel_withdraw_desc1"));
                    return;
                }
                YZWithdrawControl.YZOnOpenDetail();
            });
            SignBtn.SetClick(() => UserInterfaceSystem.That.ShowUI<UIMailLogin>(MailLoginPanel.Sign));
            CashFlowBtn.SetClick(()=> UserInterfaceSystem.That.ShowUI<UICashFlow>());

            SaveAccountBtn.SetClick(openSaveAccount);
            SaveAccountBtn_Big.SetClick(openSaveAccount);

            CloseBtn.SetClick(OnCloseBtnClick);
            EditBtn.SetClick(openEditPlayerInfo);
            Text_EditBtn.SetClick(openEditPlayerInfo);
            
            CancelWithDrawBtn.SetClick(OnCancelWithDrawBtnClick);
        }

        private void OnCancelWithDrawBtnClick()
        {
            UserInterfaceSystem.That.ShowUI<UIWithdrawCancel>();
        }

        private void OnRefresh()
        {
            CancelWithDrawBtn.SetActive(Root.Instance.WithdrawInProgress);

            void Action()
            {
                if (Root.Instance.UserInfo.InCancelCD)
                {
                    WithDrawBtn.title = TimeUtils.Instance.ToDayHourMinuteSecond(Root.Instance.UserInfo.lastCancelCashTime);
                }
                else
                {
                    WithDrawBtn.title = I18N.Get("key_withdraw");
                }

                WithDrawBtn.Gray = Root.Instance.UserInfo.InCancelCD;
            }
            if (Root.Instance.UserInfo.InCancelCD)
            {
                Action();
                if (!beenRegister)
                {
                    beenRegister = true;
                    RegisterInterval(1, Action );
                }
            
            }
        }

        enum vname
        {
            Role
        }

        public override void InitVm()
        {
            OnRefresh();
            vm[vname.Role.ToString()] = new ReactivePropertySlim<Role>(Root.Instance.Role);
        }

        public override void InitBinds()
        {
            vm[vname.Role.ToString()].ToIObservable<Role>().Subscribe(role =>
            {
                if (role == null)
                {
                    return;
                }

                //自然量用户不显示 任何与美金相关的东西
                WithDrawGroup.SetActive(!Root.Instance.IsNaturalFlow);
                NotBindBtns.SetActive(!Root.Instance.IsBindMail);
                DeleteBtn.SetActive(Root.Instance.IsBindMail);

                role.LoadIcon(PlayerIcon);

                AllDollarText.text = I18N.Get("key_money_count", GameUtils.TocommaStyle(role.GetDollars()));
                CanWithdrawCount.text =
                    I18N.Get("key_money_count", GameUtils.TocommaStyle(role.GetItemCount(Const.Cash)));
                BonusCount.text = I18N.Get("key_money_count", GameUtils.TocommaStyle(role.GetItemCount(Const.Bonus)));

                NameText.text = role.nickname;

                //玩家是否充值
                var isCharge = Root.Instance.ChargeSuccessCount > 0;
                TwoBtns.SetActive(!isCharge);
                SaveAccountBtn_Big.SetActive(isCharge);

                IdText.text = I18N.Get("key_id_is", role.user_id);
                
                DeleteBtn.SetClick(() =>
                {
                    UserInterfaceSystem.That.ShowUI<UIConfirm>(new UIConfirmData()
                    {
                        Type = UIConfirmData.UIConfirmType.TwoBtn,
                        HideCloseBtn = true,
                        desc = I18N.Get("key_delete_account"),
                        cancelTitle = I18N.Get("key_cancel"),
                        confirmTitle = I18N.Get("key_delete"),
                        title = I18N.Get("key_warning"),
                        confirmCall = () =>
                        {
                            MediatorRequest.Instance.DeleteAccount(this.GetCancellationTokenOnDestroy());
                        }
                    });
                   
                });
            });
        }

        public override void InitEvents()
        {
            AddEventListener(new[]
            {
                GlobalEvent.Sync_Role_Info,
                Proto.CANCLE_WITHDRAW,
                // Proto.HYPER_RIGISTER,
                Proto.HyperHistory
            }, (sender, eventArgs) => { Refresh(); });
            
            AddEventListener(GlobalEvent.Sync_Item, (sender, args) => { vm[vname.Role.ToString()].Refresh(); });
            
            AddEventListener(ProtoError.WithDrawError.ToString(), (sender, args) =>
            {
                CancelWithDrawBtn.SetActive(false);
            });
        }
    }
}