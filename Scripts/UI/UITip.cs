using System;
using System.Linq;
using System.Text;
using Core.Services.UserInterfaceService.API;
using Core.Services.UserInterfaceService.Internal;
using UIWidgets;
using UniRx;
using UnityEngine.UI;

namespace UI
{
    public class UITip : UIBase<UITip>
    {
        public Text text;
        public VerticalLayoutGroup Layout;
        private const int Time = 2;

        public override UIType uiType { get; set; } = UIType.Top;

        public override void OnStart()
        {
            if (args != null && args.Any())
            {
                var content = GetArgsByIndex<string>(0);

                Compatibility.SetLayoutChildControlsSize(Layout, content.Length < 38, true);

                text.text = content;
            }
            else
            {
                Close();
                return;
            }

            Observable.Timer(TimeSpan.FromSeconds(Time))
                .Subscribe(l => { Close(); })
                .AddTo(this);
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