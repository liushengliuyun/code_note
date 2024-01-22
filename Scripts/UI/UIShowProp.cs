using Core.Services.ResourceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using DataAccess.Model;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIShowProp : UIBase<UIShowProp>
    {
        [SerializeField] private Image PropImage;
        [SerializeField] private Text PropName;
        [SerializeField] private Text PropDesc;
        [SerializeField] private Button btn;


        public override UIType uiType { get; set; } = UIType.Window;

        public override void OnStart()
        {
            var prop = GetArgsByIndex<Prop>(0);

            PropName.text = prop.name;

            PropDesc.text = prop.desc;

            PropImage.sprite = ResourceSystem.That.LoadAssetSync<Sprite>($"uibingo/item0{prop.id}_b");

            PropImage.SetNativeSize();
            
            btn.onClick.AddListener(Close);
        }

        public override void InitVm()
        {
        }

        public override void InitBinds()
        {
        }

        public override void InitEvents()
        {
        }
    }
}