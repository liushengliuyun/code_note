using Core.Manager;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using Core.Third.I18N;
using DataAccess.Controller;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Root = DataAccess.Model.Root;

namespace UI.UIWithDrawFlow
{
    public class UIWithdrawConfirm : UIBase<UIWithdrawConfirm>
    {
        public override UIType uiType { get; set; } = UIType.Window;
        
        [SerializeField] private Button brbtnclose;
        [SerializeField] private Button brbtnconfirm;
        [SerializeField] private Button brbtnback;

        [SerializeField] private Text brtxtwithdrawvalue;
        [SerializeField] private Text brtxtfeevalue;
        [SerializeField] private GameObject brbgbonusvalue;
        [SerializeField] private Text brtxtbonusvalue;
        [SerializeField] private Text brtxtreceivevalue;

        [SerializeField] private GameObject brpanelemail;
        [SerializeField] private Text brtxtemail;

        [SerializeField] private GameObject brtxttitle;
        [SerializeField] private GameObject brtxtwithdrawamount;
        [SerializeField] private GameObject brtxtprocessing;
        [SerializeField] private GameObject brtxtreceive;
        [SerializeField] private GameObject brtxtback;
        [SerializeField] private GameObject brtxtconfirm;
        
        public override void InitEvents()
        {
        }

        public override void OnStart()
        {
            brbtnclose.onClick.AddListener(() => 
            {
                Close();
                if (UserInterfaceSystem.That.Get<UIWithdrawDetails>() != null)
                {
                    UserInterfaceSystem.That.ShowUI<UIWithdrawDetails>();
                }
            });
            
            brbtnback.onClick.AddListener(() => {
                // 返回大厅
                Close();
                UserInterfaceSystem.That.RemoveUIByName("UIWithdrawDetails");
            });
            
            float amount = args[0].ToString().ToFloat();
            float feeAmount = (float)args[1];

            string money_code = "$";
            bool isShowBonus = Root.Instance.Role.GetBonus() > 0;

            bool isEmailVerified = Root.Instance.GetHyperMailVerified() > 0;
            bool isNameAddAddressVerified = Root.Instance.GetHyperNameAndAddressVerified() > 0;

            brpanelemail.SetActive(isEmailVerified);
            brbgbonusvalue.SetActive(isShowBonus);
            brtxtfeevalue.text = YZString.Concat(money_code, feeAmount.ToString("0.00"));
            brtxtwithdrawvalue.text = YZString.Concat(money_code, amount.ToString("0.00"));
            brtxtreceivevalue.text = YZString.Concat(money_code, (amount - feeAmount).ToString("0.00"));

            if (isShowBonus)
            {
                string bouns = YZString.Concat(money_code, Root.Instance.Role.GetBonus().ToString("0.00"));
                brtxtbonusvalue.text = YZString.Format(I18N.Get("key_bouns_cash"), bouns);
            }

            if (isEmailVerified)
            {
                brtxtemail.text = YZDataUtil.GetLocaling(YZConstUtil.YZWithdrawEmail);
            }
            else
            {
                brtxtemail.text = "";
            }
            
            brbtnconfirm.onClick.AddListener(() => {
                // 验证邮箱
                if (!isEmailVerified)
                    UserInterfaceSystem.That.ShowUI<UIWithdrawEmail>();
                else if (!isNameAddAddressVerified)
                    UserInterfaceSystem.That.ShowUI<UIWithdrawName>();
                else
                    // 全都验证过了
                    //UserInterfaceSystem.That.ShowUI<UIWithdrawRecord>();
                    MediatorRequest.Instance.WithdrawApply(Root.Instance.WithdrawAmount);
            });
        }

        public override void InitVm()
        {
        }

        public override void InitBinds()
        {
        }
    }
}