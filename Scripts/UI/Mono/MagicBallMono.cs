using CatLib.EventDispatcher;
using Core.Extensions;
using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Third.I18N;
using Cysharp.Threading.Tasks;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils.Static;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Mono
{
    public class MagicBallMono : MonoBehaviour
    {
        public GameObject effect1;
        
        [SerializeField]
        private GameObject effect_translation;
        
        
        public MagicBallData data;

        private int MagicOrder
        {
            get
            {
                if (data == null)
                {
                    return -1;
                }

                return data.order;
            }
        }

        public Transform LockGroup;

        public MyButton MagicBallBtn;
        
        public void Init()
        {
            if (data == null)
            {
                return;
            }

            MagicBallBtn.SetClick(OnMagicBallBtnClick);


            // LockGroup.SetActive(!data.IsUnLock);

            gameObject.FindChild("point_group").SetActive(data.IsUnLock);
            
            var costText = gameObject.FindChild<Text>("point_group/CostText");

            costText.text = "x" + data.weight.ToString();


            // var itemCount = gameObject.FindChild<Text>("ItemCount");
            // MediatorItem.Instance.SetItemText(data.type, itemCount);
            // itemCount.text = data.amount.ToString();

            // var itemIcon = gameObject.FindChild<Image>("ItemIcon");
            // itemIcon.sprite = MediatorItem.Instance.GetItemSprite(data.type);


            var ballIcon = gameObject.FindChild<Image>("BallIcon");

            if (data.IsUnLock)
            {
                if (data.IsClaimed)
                {
                    ballIcon.sprite = MediatorBingo.Instance.GetSpriteByUrl($"uimagicball/{data.page + 1}");
                    MagicBallBtn.transition = Selectable.Transition.None;
                    
                    MediatorItem.Instance.SetItemText(data.type, costText);
                    costText.text = data.amount.ToString();
                    
                    var item_image = gameObject.FindChild<Image>("point_group/Image");
                    item_image.sprite = MediatorItem.Instance.GetItemSprite(data.type);
                }
                else
                {
                    MagicBallBtn.transition = Selectable.Transition.ColorTint;
                    ballIcon.sprite = MediatorBingo.Instance.GetSpriteByUrl("uimagicball/magicball_can_click");
                }
            }
            else
            {
                MagicBallBtn.transition = Selectable.Transition.None;
                ballIcon.sprite = MediatorBingo.Instance.GetSpriteByUrl("uimagicball/magicball_lock");
            }
        }

        //封印状态不可点击
        void OnMagicBallBtnClick()
        {
            if (data == null)
            {
                return;
            }

            if (data.IsUnLock && !data.IsClaimed)
            {
                if (Root.Instance.MagicBallInfo.CurrentPoint >= data.weight)
                {
                    TryGetMagicReward();
                }
                else
                {
                    UserInterfaceSystem.That.ShowUI<UIConfirm>(new UIConfirmData()
                    {
                        Type = UIConfirmData.UIConfirmType.OneBtn,
                        // HideCloseBtn = true,
                        desc = I18N.Get("key_magic_ball_tip3"),
                        confirmTitle = I18N.Get("key_play_now"),
                        // AligmentType = TextAnchor.MiddleLeft,
                        Rect2D = new Vector2(600, 550),
                        // Position = new Vector2(0, 15),
                    });
                }
            }
        }

        async UniTask TryGetMagicReward()
        {
            var token = transform.GetCancellationTokenOnDestroy();
            var willGetAddedBonus = data.WillGetAddedBonus;

            //增加点击特效后 ，再弹出奖励
            effect_translation.SetActive(true);

            await UniTask.Delay(500, cancellationToken: token);
            
            MediatorRequest.Instance.ClaimMagicBall(MagicOrder, willGetAddedBonus, () =>
            {
                if (willGetAddedBonus)
                {
                    EventDispatcher.Root.Raise(GlobalEvent.Add_Magic_Ball_Page, data.page + 1);
                }
            });
        }
    }
}