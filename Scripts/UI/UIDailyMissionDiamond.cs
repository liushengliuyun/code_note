using System;
using Core.Services.UserInterfaceService.Internal;
using DataAccess.Controller;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [Obsolete]
    public class UIDailyMissionDiamond : UIBase<UIDailyMissionDiamond>
    {
        public override UIType uiType { get; set; } = UIType.Window;

        
        [SerializeField] private Button closeBtn;
        [SerializeField] private Button buttonConfirmBtn;
        [SerializeField] private Button cancelBtn;
        public override void InitEvents()
        {
        }

        public override void OnStart()
        {
            cancelBtn.onClick.AddListener(Close);
            closeBtn.onClick.AddListener(Close);
            buttonConfirmBtn.onClick.AddListener(() =>
            {
                MediatorRequest.Instance.RefreshSuperDailyTask(3);
                Close();
            });
        }

        public override void InitVm()
        {
        }

        public override void InitBinds()
        {
        }
        
        protected override void OnAnimationIn()
        {
            transform.GetChild(1).localScale = new Vector3(0.3f, 0.3f, 0.3f);
            transform.GetChild(1).DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        }

        protected override void OnAnimationOut()
        {
            transform.GetChild(1).DOScale(Vector3.one * 0.8f, 0.1f).SetEase(Ease.OutCubic)
                .OnComplete(() => base.Close());
        }
    }
}