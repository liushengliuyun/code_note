
using System;
using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.Internal;
using UnityEngine;
using UnityEngine.UI.Extensions;
using Utils;

namespace UI
{
    [Obsolete]
    public class UIHighScore : UIBase<UIHighScore>
    {
        public MyButton[] CloseBtns;

        public Transform Content;
        
        public HorizontalScrollSnap HorizontalScrollSnap;
        
        public override UIType uiType { get; set; } = UIType.Window;

        public override void OnStart()
        {

            foreach (var closeBtn in CloseBtns)
            {
                closeBtn.SetClick(OnCloseBtnClick);
            }

            for (int i = 0; i < Content.childCount; i++)
            {
                Content.GetChild(i).SetActive(true);
            }
            
            //
            YZFunnelUtil.SendYZEvent("road_ui_popup");
        }

        protected override void OnClose()
        {
            base.OnClose();
            for (int i = 0; i < Content.childCount; i++)
            {
                if (i == HorizontalScrollSnap.CurrentPage)
                {
                    continue;
                }
                Content.GetChild(i).SetActive(false);
            }
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