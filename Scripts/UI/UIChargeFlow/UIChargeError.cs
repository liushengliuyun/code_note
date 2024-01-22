using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.Internal;
using Core.Third.I18N;
using MyBox;
using UnityEngine;
using UnityEngine.UI;

namespace UI.UIChargeFlow
{
    public class UIChargeError : UIBase<UIChargeError>
    {
        public override UIType uiType { get; set; } = UIType.Window;

        [SerializeField] private MyText title;
        [SerializeField] private MyText desc;
        
        [SerializeField] private Button closeBtn;
        public override void InitEvents()
        {
            //throw new System.NotImplementedException();
        }

        public override void OnStart()
        {
            var titleStr = GetArgsByIndex<string>(0);
            if (!titleStr.IsNullOrEmpty())
                title.text = I18N.Get(titleStr);

            var descStr = GetArgsByIndex<string>(1);
            if (!descStr.IsNullOrEmpty())
                desc.text = I18N.Get(descStr);
            
            closeBtn.onClick.AddListener(Close);
        }

        public override void InitVm()
        {
        }

        public override void InitBinds()
        {
        }
    }
}