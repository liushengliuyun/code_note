using CatLib.EventDispatcher;
using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using Core.Third.I18N;
using UnityEngine;
using UnityEngine.UI;

namespace UI.UIChargeFlow
{
    public class UIChargeHint : UIBase<UIChargeHint>
    {
        public override UIType uiType { get; set; } = UIType.Window;
        
        [SerializeField] private Button closeBtn;

        [SerializeField] private MyText descText;

        [SerializeField] private Text btnText;

        [SerializeField] private Button yesBtn;
        
        //[SerializeField] private SpineLoaderMono spineLoader;
        
        public override void InitEvents()
        {
            //throw new System.NotImplementedException();
        }

        public override void OnStart()
        {
            //spineLoader.PlayAnimation("win");

            EventDispatcher.Root.Raise(nameof(UIChargeHint));
            
            var restricType = GetArgsByIndex<int>(0);
            
            switch (restricType)
            {
                case 0:
                    break;
                
                case 3:
                    descText.text = I18N.Get("key_charge_hint34");
                    btnText.text = I18N.Get("key_charge_hint_btn");
                    
                    closeBtn.onClick.AddListener(CancelCharge);
                    
                    yesBtn.onClick.AddListener(JumpToUIMainPlay);

                    break;
                            
                case 4:
                    descText.text = I18N.Get("key_charge_hint34");
                    btnText.text = I18N.Get("key_charge_hint_btn");
                    
                    closeBtn.onClick.AddListener(CancelCharge);
                    yesBtn.onClick.AddListener(JumpToUIMainPlay);

                    break;
                            
                default:
                    break;
            }
        }
        
        private void JumpToUIMainPlay()
        {
            UserInterfaceSystem.That.CloseAllUI(new []{ nameof(UIMain)});
            UIMain.Shared().roomToggle.isOn = true;
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