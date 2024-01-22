using Core.Extensions;
using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using DG.Tweening;
using UI.UIChargeFlow;

using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class UILuckyGuy : UIBase<UILuckyGuy>
    {
        public Transform BravoTransform;

        public Transform LuckyYouTransfrom;

        public Transform LightParent;

        public MyButton FightBtn;

        public MyButton CloseBtn;

        public MyButton BuyBtn;

        public RoomItemMono roomItemMono;

        public Transform DisplayGroup;

        public Text CashCountText;

        public Text BonusCountText;

        public override UIType uiType { get; set; } = UIType.Window;


        private GameData table;
        private bool isLuckyGuyActive;

        void OnFightBtnClick()
        {
            roomItemMono.Button.onClick?.Invoke();
        }

        void OnBuyBtnClick()
        {
            var chargeGoodInfo = Root.Instance.LuckyGuyInfo.LuckGood;
            MediatorRequest.Instance.Charge(chargeGoodInfo, ActivityType.LuckyGuy);
        }

        void InitDisPlayGroup()
        {
            if (Root.Instance.LuckyGuyInfo == null)
            {
                Close();
                return;
            }
            
            var chargeGoodInfo = Root.Instance.LuckyGuyInfo.LuckGood;
            if (chargeGoodInfo == null)
                return;


            chargeId = Root.Instance.LuckyGuyInfo.LuckGood.id;
            if (Root.Instance.LuckyGuyInfo.LuckyRoom == null)
            {
                return;
            }

            float cashCount = chargeGoodInfo.amount;
            float bonusCount = Root.Instance.LuckyGuyInfo.LuckyRoom.PrizePool;
            var cashTextText = I18N.Get("key_money_count", cashCount.ToString());
            CashCountText.text = cashTextText;

            var cashText = DisplayGroup.gameObject.FindChild<Text>("Content/TextGroup/Cash/Text");
            cashText.text = cashTextText;

            var bonusTextText = I18N.Get("key_money_count", bonusCount.ToString());
            BonusCountText.text = bonusTextText;
            var bonusText = DisplayGroup.gameObject.FindChild<Text>("Content/TextGroup/Bonus/Text");
            bonusText.text = bonusTextText;
        }

        void InitLights()
        {
            int timeCounter = 0;

            void Move()
            {
                for (int i = 0; i < LightParent.childCount; i++)
                {
                    var visible = i % 2 == timeCounter % 2;
                    LightParent.GetChild(i).Find("on").SetActive(visible);
                }
            }

            RegisterInterval(0.4f, () =>
            {
                timeCounter++;

                Move();
            }, true);
        }

        
        private int chargeId;
        private ActivityEnterType activityEnterTYpe;
        
        protected override void OnClose()
        {
            base.OnClose();
            YZFunnelUtil.YZFunnelActivityPop(ActivityType.LuckyGuy
                , charge_id: chargeId
                , isauto: activityEnterTYpe.IsAutoPop()
                , duration: (int)duration
                , outtype: closeByBtn ? 1 : 2
                , switch_click: SwitchClick
            );
        }
        
        
        public override void OnStart()
        {
            table = GetTable();

            var bravoActive = table?["bravo"] != null;

            isLuckyGuyActive = !bravoActive;
            if (table?["enterType"] is ActivityEnterType entryType)
            {
                activityEnterTYpe = entryType;
                
                if (entryType is not (ActivityEnterType.Refresh or ActivityEnterType.Click or ActivityEnterType.Trigger))
                {
                    MediatorActivity.Instance.AddPopCount(ActivityType.LuckyGuy, entryType);
                }
            }

            BravoTransform.SetActive(bravoActive);
            LuckyYouTransfrom.SetActive(!bravoActive);

            InitLights();

            InitDisPlayGroup();

            InitRoom();

            FightBtn.SetClick(OnFightBtnClick);

            BuyBtn.SetClick(OnBuyBtnClick);

            CloseBtn.SetClick(OnCloseBtnClick);
        }

        public override void InitVm()
        {
        }

        public override void InitBinds()
        {
        }

        public override void InitEvents()
        {
            AddEventListener(Proto.MATCH_BEGIN, (sender, eventArgs) =>
            {
                if (isLuckyGuyActive && eventArgs is ProtoEventArgs { Result: ProtoResult.Success })
                {
                    UserInterfaceSystem.That.RemoveAllQueue();
                    Close();
                }
            });

            AddEventListener(GlobalEvent.CHARGE_SUCCESS, (sender, eventArgs) =>
            {
                YZLog.LogColor("GlobalEvent.CHARGE_SUCCESS");
                var table = GetTable();
                if (table?["matchId"] is string matchId)
                {
                    YZLog.LogColor("matchId = " + matchId);
                    if (Root.Instance.LuckyGuyInfo != null)
                    {
                        //充值成功
                        Root.Instance.LuckyGuyInfo.pay_status = 3;
                    }

                    MediatorRequest.Instance.MatchClaim(matchId, true, showAll: true);
                }

                Close();
            });
            
            AddEventListener(new[] {nameof(UIChargeHint) }, (sender, eventArgs) => { Close(); });
        }

        void InitRoom()
        {
            var uimain = UserInterfaceSystem.That.Get<UIMain>();
            if (uimain == null)
            {
                return;
            }

            uimain.RenderRoom(roomItemMono, Root.Instance.LuckyGuyInfo.LuckyRoom, true);
            roomItemMono.CostText.text = I18N.Get("key_money_count", 0);
        }
        
        protected override void OnAnimationOut()
        {
            var uimain = UserInterfaceSystem.That.Get<UIMain>();
            var root = transform;
            if (!isLuckyGuyActive)
            {
                root.GetChild(0).SetActive(false);
                var panel = root.GetChild(1);
                var tween1 = panel.DOScale(Vector3.one * 0.18f, 0.3f).SetEase(Ease.InBack);
                
                var tweenScale = panel.DOScale(Vector3.one * 0.08f, 0.5f).SetEase(Ease.InOutCubic).SetDelay(0.2f);
                
                var tween2 = panel.DOMove(uimain.HistoryToggle.transform.position, 0.5f).SetEase(Ease.InOutCubic).SetDelay(0.2f);
                
                DOTween.Sequence().Append(tween1).Append(tweenScale).Join(tween2).SetId(UIName);

                // DOTween.Sequence().SetDelay(0.1f).SetId(UIName);
                //不好看
                // UIEffectUtils.Instance.CaptureAndShrink(panel, UICanvas, uimain.HistoryToggle.transform);
            }
            else
            {
                root.GetChild(1).DOScale(Vector3.one * 0.8f, 0.1f).SetEase(Ease.OutCubic)
                    .SetId(UIName);
            }
        }
        
    }
}