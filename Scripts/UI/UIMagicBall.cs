using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Core.Extensions;
using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using Core.Third.I18N;
using Cysharp.Threading.Tasks;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils.Static;
using UI.Effect;
using UI.Mono;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Utils;

namespace UI
{
    public class UIMagicBall : UIBase<UIMagicBall>
    {
        public MyButton CloseBtn;

        public MyButton AbountBtn;

        public Transform Content;

        public Text MagicCountText;

        public HorizontalScrollSnap HorizontalScrollSnap;

        /// <summary>
        /// 配置
        /// </summary>
        private Dictionary<int, List<MagicBallData>> configs;

        public override UIType uiType { get; set; } = UIType.Window;

        private List<int> animationList = new();

        // private int chargeId;
        // private ActivityEnterType activityEnterTYpe;
        
        protected override void OnClose()
        {
            base.OnClose();
            YZFunnelUtil.YZFunnelActivityPop(ActivityType.MagicBall
                // , charge_id: chargeId
                , isauto: GetOpenType().IsAutoPop()
                , duration: (int)duration
                , outtype: closeByBtn ? 1 : 2
                , switch_click: SwitchClick
            );
        }
        
        private ActivityEnterType GetOpenType()
        {
            return GetTableValue<ActivityEnterType>("ActivityEnterType");
        }
        
        public override void OnStart()
        {
            CloseBtn.SetClick(OnCloseBtnClick);
            AbountBtn.SetClick(OnAboutBtnClick);

            if (Root.Instance.MagicBallInfo == null)
            {
                Close();
                return;
            }

            MediatorUnlock.Instance.RecordShowUI(ClassType);

            var currentPage = Root.Instance.MagicBallInfo.CurrentPage;

            HorizontalScrollSnap.StartingScreen = currentPage;

            HorizontalScrollSnap.OnSelectionPageChangedEvent.AddListener(page => InitPage(page));
        }

        /// <summary>
        /// 初始化数组， 
        /// </summary>
        /// <param name="ballsParents"></param>
        void InitAnimationList(Transform ballsParents)
        {
            animationList.Clear();
            for (int i = 0; i < ballsParents.childCount; i++)
            {
                var child = ballsParents.GetChild(i);
                child.TryGetComponent<MagicBallMono>(out var magicBallMono);
                if (magicBallMono != null)
                {
                    if (magicBallMono.data.IsClaimed)
                    {
                        animationList.Add(i);
                    }
                }
            }
        }

        private bool randomStart;
        private bool randomChange;

        //需要一个终止循环的条件
        //如何保证切换过来马上显示特效 todo
        async UniTask RandomPlayEffect()
        {
            randomStart = true;
            var token = this.transform.GetCancellationTokenOnDestroy();

            var pageCom = Content.GetChild(HorizontalScrollSnap.CurrentPage);

            var ballsParents = pageCom.Find("BallsParent");

            InitAnimationList(ballsParents);

            animationList.Shuffle();

            bool wait = false;

            foreach (var index in animationList)
            {
                var magicBallTransform = ballsParents.GetChild(index);

                magicBallTransform.TryGetComponent<MagicBallMono>(out var magicBallMono);

                if (!magicBallMono.data.IsClaimed)
                {
                    //如果换到了其他页， 直接结束
                    break;
                }

                magicBallMono.effect1.SetActive(false);
                magicBallMono.effect1.SetActive(true);

                wait = true;

                await UniTask.WhenAny(UniTask.WaitUntil(() => randomChange, cancellationToken: token),
                    UniTask.Delay(7000, cancellationToken: token));

                if (randomChange)
                {
                    break;
                }
            }

            if (!wait)
            {
                await UniTask.Delay(500, cancellationToken: token);
            }

            randomChange = false;
            RandomPlayEffect();
        }

        private void InitPage(int page, bool pageChange = true)
        {
            var pageCom = Content.GetChild(page);
            var info = Root.Instance.MagicBallInfo;
            if (info == null)
            {
                return;
            }

            var ballsParents = pageCom.Find("BallsParent");
            for (int index = 0; index < ballsParents.childCount; index++)
            {
                var child = ballsParents.GetChild(index);
                child.TryGetComponent<MagicBallMono>(out var magicBallMono);
                if (magicBallMono != null)
                {
                    var magicBallData = info.GetMagicBallData(page, index);
                    magicBallMono.data = magicBallData;
                    magicBallMono.Init();
                }
            }

            var totalBonus = pageCom.gameObject.FindChild<Text>("text1/TotalBonus");

            totalBonus.text = I18N.Get("key_money_count", info.PageTotalBonus(page));

            var addedBonus = pageCom.gameObject.FindChild<Text>("text2/AddedBonus");

            addedBonus.text = I18N.Get("key_money_count", info.PageAddedBonus(page));
            if (!randomStart)
            {
                RandomPlayEffect();
            }
            else
            {
                //改变了page 才强制中止循环
                if (pageChange)
                {
                    randomChange = true;
                }
            }
        }

        public override void InitVm()
        {
            MagicCountText.text = Root.Instance.MagicBallInfo.CurrentPoint.ToString();
        }

        public override void InitBinds()
        {
        }

        public override void InitEvents()
        {
            AddEventListener(GlobalEvent.SYNC_MAGIC_BALL_INFO, (sender, eventArgs) => { Refresh(); });

            AddEventListener(GlobalEvent.Add_Magic_Ball_Page, (sender, eventArgs) =>
            {
                if (sender is int page)
                {
                    if (HorizontalScrollSnap.CurrentPage < page)
                    {
                        ChangePage(page);
                    }
                }
            });
        }

        async UniTask ChangePage(int page)
        {
            var token = this.GetCancellationTokenOnDestroy();
            await UniTask.WaitUntil(() =>
            {
                var uiGetreward = UserInterfaceSystem.That.Get<UIGetRewards>();
                return uiGetreward == null;
            }, cancellationToken: token);

            await UniTask.Delay(100, cancellationToken: token);

            //有容错
            HorizontalScrollSnap.ChangePage(page);
        }

        public override void Refresh()
        {
            base.Refresh();
            InitPage(HorizontalScrollSnap.CurrentPage, false);
        }

        void OnAboutBtnClick()
        {
            UserInterfaceSystem.That.ShowUI<UIConfirm>(new UIConfirmData()
            {
                Type = UIConfirmData.UIConfirmType.OneBtn,
                // HideCloseBtn = true,
                desc = I18N.Get("key_magic_ball_tip1") + "\n\n" + I18N.Get("key_magic_ball_tip2"),
                confirmTitle = I18N.Get("key_ok"),
                AligmentType = TextAnchor.MiddleLeft,
                // Rect2D = new Vector2(650, 600),
                // Position = new Vector2(0, 15),
            });
        }
    }
}