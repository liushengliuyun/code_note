using System;
using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils.Static;
using Spine.Unity;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIStory : UIBase<UIStory>
    {
        [SerializeField] private Transform DollarEffect;
        [SerializeField] private MyButton SkipButton;
        [SerializeField] private Text ButtonTitle;
        [SerializeField] private Text Text1;
        [SerializeField] private Text Text2;
        [SerializeField] private SkeletonGraphic spine;

        private int clickCount;

        public override void OnStart()
        {
            Text1.SetActive(true);
            Text2.SetActive(false);

            MediatorRequest.Instance.GetRandomName();
            Text1.GetComponent<Typing>().EndCallBack = Next;
            Text2.GetComponent<Typing>().EndCallBack = Next;
            SkipButton.SetClick(Next);
            DollarEffect.SetActive(!Root.Instance.IsNaturalFlow);
        }
        
        private void Next()
        {
            if (clickCount >= 2)
            {
                return;
            }

            clickCount++;
            if (clickCount == 1)
            {
                var track = spine.AnimationState.SetAnimation(0, "animation", false);
                track.TimeScale = 2f;
                Text1.SetActive(false);
                spine.AnimationState.Complete += entry => { Text2.SetActive(true); };
            }
            else
            {
                Close();
                UserInterfaceSystem.That.ShowUI<UISubPlayerInfo>();
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
            AddEventListener(Proto.GM_SET_ORGANIC, (sender, eventArgs) =>
            {
                DollarEffect.SetActive(!Root.Instance.IsNaturalFlow);
            });
        }
    }
}