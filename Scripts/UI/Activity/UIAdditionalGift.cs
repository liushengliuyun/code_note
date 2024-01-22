using Core.Extensions.UnityComponent;
using Core.Manager;
using Core.Models;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using DataAccess.Controller;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using DG.Tweening;
using UnityEngine;
using Utils;
using Root = DataAccess.Model.Root;

namespace UI.Activity
{
    public class UIAdditionalGift : UIBase<UIAdditionalGift>
    {
        public override UIType uiType { get; set; } = UIType.Window;
        
        [SerializeField] private MyButton buyBtn;
        [SerializeField] private MyButton closeBtn;

        [SerializeField] private MyText priceText;

        [SerializeField] private MyText cashText;
        [SerializeField] private MyText bonusText;

        private int chargeId = -1;
        public override void InitEvents()
        {
            AddEventListener(GlobalEvent.CHARGE_SUCCESS,
                (sender, args) =>
                {
                    // 判断一下 id 是不是属于补充充值的
                    int eventChargeId = int.Parse((string)sender);
                    if (chargeId == eventChargeId)
                        Close();
                });
        }

        protected override void OnClose()
        {
            base.OnClose();
            YZFunnelUtil.YZFunnelActivityPop(ActivityType.AddCharge
                , charge_id: chargeId
                //没有点击进入
                , isauto: true
                , duration: (int)duration
                , outtype: closeByBtn ? 1 : 2
                , switch_click: SwitchClick
            );
        }
        
        public override void OnStart()
        {
            if (Root.Instance.Role.AdditionalGiftInfo != null)
            {
                var chargeConfig = Root.Instance.AdditionalGiftConfigs[0];
                var cash = Root.Instance.Role.AdditionalGiftInfo.charge_amount;

                priceText.text = "$" + YZNumberUtil.FormatYZMoney(cash.ToString());
                cashText.text = "$" + YZNumberUtil.FormatYZMoney(cash.ToString());
                bonusText.text = "$" + YZNumberUtil.FormatYZMoney(cash.ToString());

                chargeId = chargeConfig.id;
                
                buyBtn.SetClick(() =>
                {
                    // 充值流程
                    Charge_configsItem chargeItemTest = new Charge_configsItem
                    {
                        id = chargeConfig.id,
                        bonusValue = cash,
                        amount = cash.ToString(),
                        position = "AddCharge"
                    };
                    LocationManager.Shared.IsLocationValid(YZSafeType.Charge, null, -1, () =>
                    {
                        if (LocationManager.Shared.IsCountryCanCharge(chargeItemTest, ActivityType.AddCharge))
                            UserInterfaceSystem.That.ShowUI<UIChargeCtrl>(chargeItemTest);
                    });
                });
            }
            
            closeBtn.SetClick(OnCloseBtnClick);
            
        }

        public override void InitVm()
        {
        }

        public override void InitBinds()
        {
        }
        
        protected override void OnAnimationIn()
        {
            transform.GetChild(1).localScale = new Vector3(0.3f, 0.3f, 0.3f);
            transform.GetChild(1).DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack)
                .OnComplete(() => {  });
        }

        protected override void OnAnimationOut()
        {
            transform.GetChild(1).DOScale(Vector3.one * 0.8f, 0.1f).SetEase(Ease.OutCubic)
                .SetId(UIName);
        }
        
    }
}