using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using Core.Third.I18N;
using DataAccess.Controller;
using UnityEngine;
using UnityEngine.UI;

namespace UI.UIChargeFlow
{
    public class UIChargeInduce : UIBase<UIChargeInduce>
    {
        public override UIType uiType { get; set; } = UIType.Window;
        
        [SerializeField] private Button closeBtn;

        [SerializeField] private MyText descText;

        [SerializeField] private Text btnText;

        [SerializeField] private Button yesBtn;
        
        //[SerializeField] private SpineLoaderMono spineLoader;

        private int _induceTpye = -1;
        
        public override void InitEvents()
        {
            //throw new System.NotImplementedException();
        }

        public override void OnStart()
        {
            var induceType = GetArgsByIndex<int>(0);
            if (induceType == 0)
            {
                btnText.text = "Deposit";
                
                yesBtn.onClick.AddListener(() =>
                {
                    Close();
                    // 是否有活动
                    if (!MediatorActivity.Instance.PopActivityRandom())
                    {
                        // 没活动跳转到商店页面
                        UIMain.Shared().StoreToggle.isOn = true;         
                    }
                });
            }
            else
            {
                btnText.text = "Go";
                yesBtn.onClick.AddListener(() =>
                {
                    Close();
                    UIMain.Shared().roomToggle.isOn = true;
                });
            }
            
            closeBtn.onClick.AddListener(CancelCharge);
        }

        private void CancelCharge()
        {
            Close();
        }

        public override void InitVm()
        {
        }

        public override void InitBinds()
        {
        }
    }
}