using Core.Extensions;
using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.Internal;
using DataAccess.Model;
using UnityEngine;
using UnityEngine.UI.Extensions;

namespace UI
{
    public class UIHowToPlay : UIBase<UIHowToPlay>
    {
        [SerializeField] private HorizontalScrollSnap horizontalScrollSnap;
        [SerializeField] private MyButton DoneBtn;
        [SerializeField] private MyButton[] CloseBtns;

        public override void OnStart()
        {
            Root.Instance.ClickHowToPlay = true;
            horizontalScrollSnap.OnSelectionPageChangedEvent.AddListener(page =>
            {
                if (page == 3)
                {
                    DoneBtn.SetActive(true);
                }
                else
                {
                    DoneBtn.SetActive(false);
                }
            });
            DoneBtn.SetClick(Close);
            foreach (var closeBtn in CloseBtns)
            {
                closeBtn.SetClick(OnCloseBtnClick);
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