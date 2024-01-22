using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.Internal;
using DataAccess.Controller;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIDuelJoin : UIBase<UIDuelJoin>
    {
        public override UIType uiType { get; set; } = UIType.Window;
        
        [SerializeField] private InputField roomNoInputField;
        [SerializeField] private MyButton okBtn;
        [SerializeField] private MyButton closeBtn;
        
        private string _roomNo;
        public override void InitEvents()
        {
        }

        public override void OnStart()
        {
            roomNoInputField.onEndEdit.AddListener((string input) =>
            {
                _roomNo = input;
            });
            
            closeBtn.SetClick(OnCloseBtnClick);
            
            okBtn.SetClick(() =>
            {
                MediatorRequest.Instance.JoinFriendsDuelRoom(_roomNo);
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