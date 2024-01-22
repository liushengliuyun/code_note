using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.Internal;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using UnityEngine.UI;

namespace UI
{
    public class UINotice : UIBase<UINotice>
    {
        public MyButton ReLoginBtn;
        public Text Content;

        public override UIType uiType { get; set; } = UIType.Top;

        public override void OnStart()
        {
            var data = GetArgsByIndex<ServerMaintain>(0);
            Content.text = data.text;
            
            // RegisterInterval(1f, () =>
            // {
            //     ReLoginBtn.Gray = data.InTime;
            //     ReLoginBtn.title = TimeUtils.Instance.ToHourMinuteSecond(data.LessTime);
            // }, true);
            
            ReLoginBtn.SetClick(GameUtils.ExitGame);
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