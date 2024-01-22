//this file is auto created by QuickCode,you can edit it 
//do not need to care initialization of ui widget any more 
//------------------------------------------------------------------------------
/**
* @author :
* date    :
* purpose :
*/
//------------------------------------------------------------------------------

using System;
using CatLib.EventDispatcher;
using Core.Controllers;
using Core.Extensions;
using UnityEngine;
using Core.Extensions.UnityComponent;
using Core.Services.ResourceService.API.Facade;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using Core.Services.UserInterfaceService.UIExtensions.Scripts.Utilities;
using Core.Third.I18N;
using Cysharp.Threading.Tasks;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using DG.Tweening;
using Reactive.Bindings;
using UI;
using UI.Animation;
using UI.Mono;

using UniRx;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Utils;
using YooAsset;
using Button = UnityEngine.UI.Button;
using Text = UnityEngine.UI.Text;

public class UIOnlineReward : UIBase<UIOnlineReward>
{
    #region UI Variable Statement
    [SerializeField] private Transform SpineParent;

    [SerializeField] private GameObject go_Content;
    [SerializeField] private Button CloseBtn;
    [SerializeField] private MyButton GetRewardBtn;
    [SerializeField] private MyButton GuangGaoBtn;

    [SerializeField] private Transform leftParent;
    [SerializeField] private Transform rightParent;
    [SerializeField] private Transform ChargeOpen;
    [SerializeField] private Transform ChargeNotOpen;

    [SerializeField] private Text descText;

    [SerializeField] private Text TimeText;

    [SerializeField] private ScrollRect ChargeNotOpenScroll;
    [SerializeField] private ScrollRect ChargeOpenScroll;

    [SerializeField] private RectTransform topRectTransform;
    [SerializeField] private Transform BasicTag;
    [SerializeField] private Transform PremiumTag;
    
    [SerializeField] private Transform GuideBonusImage;
    [SerializeField] private MyText GuideBonusText;

    public override UIType uiType { get; set; } = UIType.Window;

    #endregion

    private bool isAutoPop;

    public override void InitEvents()
    {
        AddEventListener(GlobalEvent.Sync_OnlineRewardInfo,
            (sender, args) =>
            {
                vm[vname.data.ToString()].ToIObservable<OnlineRewardInfo>().Value = Root.Instance.OnlineRewardInfo;
            });

        AddEventListener(GlobalEvent.Pass_Day,
            (sender, args) => { vm[vname.data.ToString()].ToIObservable<OnlineRewardInfo>().Refresh(); });
    }

    private AssetOperationHandle assetOperationHandle;
    
    private ActivityEnterType GetOpenType()
    {
        return GetTableValue<ActivityEnterType>("ActivityEnterType");
    }
    
    public override void OnStart()
    {
        if (args != null)
        {
            isAutoPop = GetArgsByIndex<bool>(0);
        }

        LoadSpine(SpineParent, out assetOperationHandle, 0.5f, new Vector3(0, -22, 0));

        CloseBtn.OnClickAsObservable().Subscribe(unit =>
        {
            if (isAutoPop)
            {
                EventDispatcher.Root.Raise(GlobalEvent.AutoPop_OnlineReward_Close);
            }

            Close();
        });
        GetRewardBtn.SetClick(TryGetReward);
        GuangGaoBtn.SetClick(TryGetADReward);

        if (GetTable()?["IsFirstShow"] is true)
        {
            GuideBonusImage.SetActive(true);
            GuideBonusText.text = Math.Round(Root.Instance.OnlineRewardInfo.AllBonus, 2).ToString();
            
            ChargeNotOpenScroll.onValueChanged.AddListener(value =>
            {
                if (value.y > 0.9f)
                {
                    GuideBonusImage.SetActive(false);
                }
            });
        }
        else
        {
            GuideBonusImage.SetActive(false);
        }
        
        
    }

    /// <summary>
    /// 是否展示  下一等级的奖励
    /// </summary>
    private bool IsShowNext => Root.Instance.ChargeOpen && !Root.Instance.IsReachMaxVipLevel;

    private WireMono curWire;

    WireMono GetWireMono(int index)
    {
        // var index = vm[vname.curIndex.ToString()].ToIObservable<int>().Value;
        Transform wireGroup;
        if (IsShowNext)
        {
            wireGroup = ChargeOpenScroll.content.transform.Find("wire_group");
        }
        else
        {
            wireGroup = ChargeNotOpenScroll.content.transform.Find("wire_group");
        }


        var wireGroupChildCount = wireGroup.childCount - index - 1;


        if (wireGroupChildCount < 0 || wireGroupChildCount > wireGroup.childCount - 1)
        {
            return null;
        }

        return wireGroup.GetChild(wireGroupChildCount).GetComponent<WireMono>();
    }


    /// <summary>
    /// 获取当前的 奖励对象
    /// </summary>
    /// <returns></returns>
    Transform GetCurrentRewardTrans()
    {
        int index = vm[vname.data.ToString()].ToIObservable<OnlineRewardInfo>().Value.CurrentIndex;
        return GetOnlineRewardTrans(index);
    }

    Transform GetOnlineRewardTrans(int index, bool left = true)
    {
        if (IsShowNext)
        {
            if (left)
            {
                return leftParent.GetChild(index);
            }
            else
            {
                return rightParent.GetChild(index);
            }
        }
        else
        {
            return go_Content.transform.GetChild(index);
        }
    }


    private IDisposable observer;


    /// <summary>
    /// 
    /// </summary>
    void SetTargetState()
    {
        int coolDown = GetCoolDown();

        if (Root.Instance.OnlineRewardInfo.GetAllReward)
        {
            GetRewardBtn.Gray = true;
            curWire.FillAmount = 1;
            TimeText.text = I18N.Get("key_get_all_reward");
        }
        else if (Root.Instance.OnlineRewardInfo.LessTime > 0)
        {
            TimeText.text = TimeUtils.Instance.ToMinuteSecond(Root.Instance.OnlineRewardInfo.LessTime);
            observer?.Dispose();
            curWire.FillAmount = 1 - (float)Root.Instance.OnlineRewardInfo.LessTime / coolDown;
            //播放缩放 动画 
            observer = Observable.Interval(TimeSpan.FromSeconds(1f))
                .TakeWhile(l => Root.Instance.OnlineRewardInfo.LessTime > 0)
                .Subscribe(
                    l =>
                    {
                        curWire.FillAmount = 1 - (float)Root.Instance.OnlineRewardInfo.LessTime / coolDown;
                        TimeText.text = TimeUtils.Instance.ToMinuteSecond(Root.Instance.OnlineRewardInfo.LessTime);
                    },
                    () => { WhenCanGetOnlineReward(); }).AddTo(this);
        }
        else
        {
            WhenCanGetOnlineReward();
        }
    }

    private void WhenCanGetOnlineReward()
    {
        curWire.FillAmount = 1;
        GetRewardBtn.Gray = false;
        TimeText.text = I18N.Get("key_can_claim");

        var currentReward = GetCurrentRewardTrans();
        var rewardItemMono = currentReward.GetComponent<RewardItemMono>();
        var sizeUpDown = rewardItemMono.Icon.GetComponent<SizeUpDown>();
        var rotateMono = rewardItemMono.gameObject.FindChild<RotateAroundCenter>("light");
        sizeUpDown.enabled = true;
        rotateMono.enabled = true;
    }

    private int GetCoolDown()
    {
        var data = vm[vname.data.ToString()].ToIObservable<OnlineRewardInfo>().Value;
        return data.cool_down_time;
    }

    enum vname
    {
        data,
        curIndex
    }


    private int rewardCount;

    public override void InitVm()
    {
        rewardCount = Root.Instance.OnlineRewardInfo.RewardCount;
        vm[vname.data.ToString()] = new ReactivePropertySlim<OnlineRewardInfo>(Root.Instance.OnlineRewardInfo);
        vm[vname.curIndex.ToString()] = new ReactivePropertySlim<int>(Root.Instance.OnlineRewardInfo.CurrentIndex);
    }

    public override void InitBinds()
    {
        vm[vname.curIndex.ToString()].ToIObservable<int>().Subscribe(value =>
        {
            for (int i = 0; i < rewardCount; i++)
            {
                var wire = GetWireMono(i);
                if (wire == null)
                {
                    continue;
                }

                if (i < value)
                {
                    wire.FillAmount = 1;
                }
                else if (i > value)
                {
                    wire.FillAmount = 0;
                }
            }

            curWire = GetWireMono(value);
        });

        vm[vname.data.ToString()].ToIObservable<OnlineRewardInfo>().Subscribe(value =>
        {
            if (value == null)
            {
                return;
            }

            GetRewardBtn.Gray = !value.CanGetReward;

            var currentIndex = value.CurrentIndex;
            var vipLevel = Root.Instance.Role.VipLevel;
            vm[vname.curIndex.ToString()].ToIObservable<int>().Value = currentIndex;

            for (int i = 0; i < rewardCount; i++)
            {
                var rewardTrans = GetOnlineRewardTrans(i);

                InitItem(rewardTrans, i, vipLevel, currentIndex);

                if (i == currentIndex)
                {
                    //当前要领取的奖励
                    SetTargetState();
                    //移动到目标
                    ScrollRect current = GetCurrentScrollRect();
                    current.ScrollToObject(rewardTrans.GetComponent<RectTransform>());
                }
            }

            //是否展示下
            if (IsShowNext)
            {
                for (int i = 0; i < rewardCount; i++)
                {
                    var rewardTrans = GetOnlineRewardTrans(i, false);
                    InitItem(rewardTrans, i, vipLevel + 1, -1, true);
                }
            }

            BasicTag.SetActive(IsShowNext);
            PremiumTag.SetActive(IsShowNext);
            topRectTransform.SetHeight(IsShowNext ? 237 : 180);

            ChargeNotOpen.SetActive(!IsShowNext);
            ChargeOpen.SetActive(IsShowNext);

            // descText.SetActive(Root.Instance.ChargeOpen);

            var lessVipUpNeed = Root.Instance.GetLessVipUpNeed();
            if (Root.Instance.IsReachMaxVipLevel)
            {
                descText.text = I18N.Get("REACH_VIP_MAX");
            }
            else
            {
                if (IsShowNext && lessVipUpNeed > 0)
                {
                    descText.text = I18N.Get("CHARGE_TIP", lessVipUpNeed);
                }
                else
                {
                    descText.text = I18N.Get("NEXT_GIFT_TIP");
                }
            }
        });
    }

    private ScrollRect GetCurrentScrollRect()
    {
        return IsShowNext ? ChargeOpenScroll : ChargeNotOpenScroll;
    }

    void TryGetReward()
    {
        if (CanGetOnlineReward())
        {
            MediatorRequest.Instance.GetOnlineRewardClaim();
        }
        else
        {
            if (Root.Instance.OnlineRewardInfo.GetAllReward)
            {
                UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_online_reward_get_all"));
            }
            else
            {
                UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_online_reward_in_count_down"));
            }
        }
    }

    private bool CanGetOnlineReward()
    {
        var info = vm[vname.data.ToString()].ToIObservable<OnlineRewardInfo>().Value;
        return info.CanGetReward;
    }

    void TryGetADReward()
    {
        ADSManager.Shared.YZShowReward("OnlineReward", status =>
        {
            if (status == AdsStatus.REWARD)
                MediatorRequest.Instance.WatchOnlineRewardAD();
        });
    }

    private void InitItem(Transform rewardTrans, int i, int vipLevel, int currentIndex, bool isPremium = false)
    {
        var rewardMono = rewardTrans.GetComponent<RewardItemMono>();
        var data = Root.Instance.OnlineActiveConfig[vipLevel][i];

        var sizeUpDown = rewardMono.Icon.GetComponent<SizeUpDown>();
        var rotateMono = rewardMono.gameObject.FindChild<RotateAroundCenter>("light");

        var canGetOnlineReward = i == currentIndex && CanGetOnlineReward();
        sizeUpDown.enabled = canGetOnlineReward;
        rotateMono.enabled = canGetOnlineReward;
        MediatorItem.Instance.SetItemText(data.type, rewardMono.ItemCountText);
        rewardMono.ItemCountText.text = data.amount.ToString();
        rewardMono.Icon.sprite = MediatorItem.Instance.GetItemSprite(data.type);
        rewardMono.VipText.text = vipLevel > 0 ? I18N.Get("VIP_LEVEL", vipLevel) : "";
        rewardMono.IsLock = i > currentIndex;
        rewardMono.IsGet = i < Root.Instance.OnlineRewardInfo.claimed;
        // rewardMono.IsPremium = isPremium;
        
        rewardMono.Button.enabled = true;
        rewardMono.Button.SetClick(() =>
        {
            if (i == currentIndex)
            {
                TryGetReward();
            }
        });
    }

    protected override void OnClose()
    {
        base.OnClose();
        assetOperationHandle?.Release();
        assetOperationHandle = null;
        
        YZFunnelUtil.YZFunnelActivityPop(ActivityType.OnlineReward
            // , charge_id: chargeId
            , isauto: GetOpenType().IsAutoPop()
            , duration: (int)duration
            , outtype: closeByBtn ? 1 : 2
            , switch_click: SwitchClick
        );
    }
    
    protected override void OnAnimationOut()
    {
        var uimain = UserInterfaceSystem.That.Get<UIMain>();
#if DAI_TEST
        if(true)
#else
                  if (!uimain.StoreToggle.isOn)
#endif
        {
            // var root = transform.GetChild(0);
            // var tween1 = root.DOScale(Vector3.one * 0.18f, 0.5f).SetEase(Ease.InBack);
            //
            // var tween2_1 = root.DOScale(Vector3.one * 0.05f, 1f).SetEase(Ease.OutCubic).SetDelay(0.1f);
            //
            // var tween2 = root.DOMove(uimain.StoreToggle.transform.position, 1f).SetEase(Ease.OutCubic).SetDelay(0.1f);
            //
            // DOTween.Sequence().Append(tween1).Append(tween2_1).Join(tween2).SetId(UIName);
            
            //等待一帧 【至少】 让摄像机绘制rendertexture
            DOTween.Sequence().SetDelay(0.1f).SetId(UIName);
            UIEffectUtils.Instance.CaptureAndShrink(transform.GetChild(0), UICanvas, uimain.StoreToggle.transform);
        }
    }
    
}