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
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BrunoMikoski.AnimationsSequencer;
using CatLib.EventDispatcher;
using Core.Extensions;
using Core.Extensions.UnityComponent;
using Core.Services.NetService.API.Facade;
using Core.Services.ResourceService.API.Facade;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using Reactive.Bindings;
using Spine.Unity;
using UI;
using UI.Effect;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityTimer;
using Utils;

public class UIGameResult : UIBase<UIGameResult>
{
    #region UI Variable Statement

    [SerializeField] private ScoreItem[] ScoreItems;

    [SerializeField] private Transform _1v1ScoreGroup;

    [SerializeField] private Transform _5SeatScoreGroup;

    [SerializeField] private Transform TaskBtnTrans;

    [SerializeField] private Transform cashTransform;
    [SerializeField] private Text RoomNameText;
    [SerializeField] private Text MatchIdText;

    [SerializeField] private Transform wait_group;
    [SerializeField] private Transform RewardGroup;

    [SerializeField] private MyButton normalNextBtn;

    [SerializeField] private MyButton collectButton;

    [SerializeField] private ScoreItem otherScoreItem;
    [SerializeField] private ScoreItem selfScoreItem;
    [SerializeField] private Text text_DiamondText;
    [SerializeField] private Text text_TScoreText;
    [SerializeField] private Text text_GoldText;

    [SerializeField] private MyButton CloseBtn;

    [SerializeField] private Transform win_group;
    [SerializeField] private Transform lose_group;

    [SerializeField] private Transform WinRankGroup;
    [SerializeField] private Transform LoseRankGroup;

    // [SerializeField] private SpineLoaderMono spineLoader;

    #endregion

    private string matchId;

    /// <summary>
    /// 是否是局内ui 跳转过来的
    /// </summary>
    private bool isFromGame;

    private MatchTable matchTable;
    
    // private List<Item> reward;
    public override void OnStart()
    {
        var table = GetTable();
        matchId = table?["matchId"] as string;
        isFromGame = table != null && (bool)table["isFromGame"];
        matchTable = table?["matchTable"] as MatchTable;
        
        var isShowTaskEntry = GetTaskBtnActive();

        TaskBtnTrans.SetActive(isShowTaskEntry);
        if (isShowTaskEntry)
        {
            var taskBtn = TaskBtnTrans.GetComponent<MyButton>();
            taskBtn.SetClick(() =>
            {
                YZFunnelUtil.YZFunnelClickActivity(ActivityType.TaskSystem, taskBtn.name, nameof(UITask), button_location: nameof(UIGameResult));
                MediatorTask.Instance.PopTaskSystem(0);
            });
            var textCom = TaskBtnTrans.gameObject.FindChild<Text>("TextGroup/Text");
            textCom.text = TimeUtils.Instance.ToHourMinuteSecond(Root.Instance.CurTaskInfo.LessTime);
            RegisterInterval(1f,
                () => { textCom.text = TimeUtils.Instance.ToHourMinuteSecond(Root.Instance.CurTaskInfo.LessTime); });
        }

        MatchIdText.text = I18N.Get("key_match_id_is", matchId);

        collectButton.SetClick(TryClaim);
        normalNextBtn.SetClick(TryClaim);
        CloseBtn.SetClick(OnCloseBtnClick);

        collectButton.SetActive(false);
        normalNextBtn.SetActive(false);
        _1v1ScoreGroup.SetActive(false);
        _5SeatScoreGroup.SetActive(false);

        lose_group.SetActive(false);
        win_group.SetActive(false);
        WinRankGroup.SetActive(false);
        LoseRankGroup.SetActive(false);

        cashTransform.SetActive(!Root.Instance.IsNaturalFlow);
    }

    private bool GetTaskBtnActive()
    {
        var isShowTaskEntry = isFromGame &&
                              (Root.Instance.CurTaskInfo.CanGetReward || Root.Instance.CurTaskInfo.IsProgressBigger());

        return isShowTaskEntry;
    }

    private void TryClaim()
    {
        //拉取最新的活动信息
        if (isFromGame
            && MediatorActivity.Instance.IsActivityOpen(ActivityType.StartPacker)
            && !MediatorActivity.Instance.IsActivityBegin(ActivityType.StartPacker))
        {
            //用网络遮罩挡住uimain, 避免弹出uimain
            // NetSystem.That.ShowNetWaitMask();

            Root.Instance.StarterPackInfo = null;
            MediatorRequest.Instance.GetStarterPackInfo();
        }

        if (myInfo.HavaReward && !myInfo.IsClaimed && matchTable.IsFinish)
        {
            var roomInfo = GetRoomInfo();
            if (roomInfo is { IsLuckyRoom: true } && Root.Instance.LuckyGuyInfo is { IsChargeSuccess: false })
            {
                Close();
                
                EventDispatcher.Root.Raise(GlobalEvent.LUCKY_GUY_FAKE_NEWS, matchId);
            }
            else
            {
                MediatorRequest.Instance.MatchClaim(matchId);
            }
        }
        else
        {
            UserInterfaceSystem.That.CloseAllUI(new[] { nameof(UIMain), nameof(UIWaitNet), nameof(UIGetRewards) });
        }
    }

    enum ShowState
    {
        Win,

        /// <summary>
        /// 
        /// </summary>
        Lose,
        Wait,

        /// <summary>
        /// 正在寻找对手
        /// </summary>
        LookingRival,
    }

    enum vname
    {
        matchHistory,
        State
    }

    public override void InitVm()
    {
        vm[vname.matchHistory.ToString()] =
            new ReactivePropertySlim<List<MatchHistory>>(Root.Instance.GetMatchHistoryList(matchId));

        vm[vname.State.ToString()] =
            new ReactivePropertySlim<ShowState>();
    }

    private MatchHistory myInfo, otherInfo;

    private SkeletonGraphic _loseSpine;

    private SkeletonGraphic loseSpine => _loseSpine ??= lose_group.Find("losespine").GetComponent<SkeletonGraphic>();
    
    
    private SkeletonGraphic _winSpine;
    private SkeletonGraphic winSpine => _winSpine ??= win_group.Find("winspine").GetComponent<SkeletonGraphic>();

    public override void InitBinds()
    {
        vm[vname.matchHistory.ToString()].ToIObservable<List<MatchHistory>>().Subscribe(value =>
        {
            if (value == null) return;

            myInfo = value.Find(history => history.user_id == Root.Instance.UserId);
            otherInfo = value.Find(history => history.user_id != Root.Instance.UserId);

            RoomNameText.text = myInfo?.room_name;

            if (myInfo == null)
            {
                myInfo = new MatchHistory()
                {
                    user_id = Root.Instance.UserId
                };
            }

            var btnGroup = collectButton.transform.parent.rectTransform();

            var isFiveSeatRoom = IsFiveSeatRoom();
            if (!isFiveSeatRoom)
            {
                btnGroup.anchoredPosition = new Vector2(0, -438);
            }
            else
            {
                btnGroup.anchoredPosition = new Vector2(0, -585);
            }

            ShowState state;

            // 如果是邀请对战的对局,且状态是对局中，需要把头像和名字提前赋值
            if (Root.Instance.DuelStatusInfo != null &&
                (Root.Instance.DuelStatusInfo.status == 2 || Root.Instance.DuelStatusInfo.status == 1))
            {
                otherInfo ??= new MatchHistory();
                //otherInfo.status = (int)Status.Game_End;
                otherInfo.head_url = Root.Instance.DuelStatusInfo.competitor.head_url;
                otherInfo.head_index = Root.Instance.DuelStatusInfo.competitor.head_index;
                otherInfo.nickname = Root.Instance.DuelStatusInfo.competitor.nickname;

                Root.Instance.DuelStatusInfo = null;
            }

            if (otherInfo == null && !myInfo.IsTableMatchExpire)
            {
                state = ShowState.LookingRival;
            }
            else if (myInfo.IsWait || otherInfo is { IsWait: true } || !matchTable.IsFinish)
            {
                state = ShowState.Wait;
            }
            else
            {
                var otherHaveReward =
                    value.Find(history => history.user_id != Root.Instance.UserId && history.HavaReward);

                //获得第一名且其他人 没有奖励
                if (myInfo.win_result == 1 && otherHaveReward == null && !isFiveSeatRoom)
                {
                    state = ShowState.Win;

                    winSpine.SetActive(true);
                }
                //不是第一名 且 自己没有奖励
                else
                {
                    if (myInfo.win_result != 1 && !myInfo.HavaReward && !isFiveSeatRoom)
                    {
                        loseSpine.SetActive(true);
                        state = ShowState.Lose;
                    }
                    else
                    {
                        //两人场 有奖励
                        if (!isFiveSeatRoom)
                        {
                            state = myInfo.IsTopOne ? ShowState.Win : ShowState.Lose;
                        }
                        else
                        {
                            state = myInfo.win_result <= 3 ? ShowState.Win : ShowState.Lose;
                        }

                        if (state == ShowState.Win)
                        {
                            winSpine.SetActive(false);

                            LoadRankGroup(WinRankGroup.gameObject);
                        }
                        else
                        {
                            loseSpine.SetActive(false);

                            LoadRankGroup(LoseRankGroup.gameObject);
                        }
                    }
                }
            }

            vm[vname.State.ToString()].ToIObservable<ShowState>().Value = state;
        });

        vm[vname.State.ToString()].ToIObservable<ShowState>().Subscribe(value =>
        {
            SetCommonUIState(value);

            if (!IsFiveSeatRoom())
            {
                Set1V1(value);
            }
            else
            {
                Set5Seat(value);
            }
        });
    }

    /// <summary>
    /// 设置通用的UI状态
    /// </summary>
    /// <param name="value"></param>
    private void SetCommonUIState(ShowState value)
    {
        if (myInfo.IsClaimed || !myInfo.HavaReward)
        {
            normalNextBtn.SetActive(true);
        }
        else
        {
            collectButton.SetActive(true);
        }

        switch (value)
        {
            case ShowState.Win:
                win_group.SetActive(true);

                // spineLoader.PlayAnimation("win");
                SetReward(myInfo.RewardsList);

                break;
            case ShowState.Lose:
                lose_group.SetActive(true);
                // if (!myInfo.HavaReward)
                // {
                //     spineLoader.PlayAnimation("death");
                // }
                // else
                // {
                //     spineLoader.PlayAnimation("idea");
                // }

                SetReward(myInfo.RewardsList);

                break;
            case ShowState.Wait:
            case ShowState.LookingRival:
                wait_group.SetActive(true);
                RewardGroup.localPosition += new Vector3(0, -50, 0);
                // spineLoader.PlayAnimation("win");

                //需要用到额外的房间信息
                var room = GetRoomInfo();
                if (room != null)
                {
                    var winReward = room.GetRankReward(1);
                    SetReward(winReward);
                }

                break;
        }
    }

    private Room GetRoomInfo()
    {
        if (myInfo == null)
        {
            return null;
        }

        return Root.Instance.GetRoomById(myInfo.room_id);
    }

    private void LoadRankGroup(GameObject rankGroup)
    {
        rankGroup.SetActive(true);
        GameObject rankSpine;
        if (IsFiveSeatRoom())
        {
            var rank = Math.Clamp(myInfo.win_result, 1, 5);
            //失败
            if (rank > 3)
            {
                if (rank == 4)
                {
                    loseSpine.initialSkinName = "fourth";
                }
                else
                {
                    loseSpine.initialSkinName = "fifth";
                }
                rankSpine = loseSpine.gameObject;
            }
            else
            {
                if (rank == 1)
                {
                    winSpine.initialSkinName = "fist";
                }
                else if (rank == 2)
                {
                    winSpine.initialSkinName = "second";
                }
                else
                {
                    winSpine.initialSkinName = "third";
                }

                winSpine.AnimationState.Complete += entry =>
                {
                    winSpine.AnimationState.SetAnimation(0, "victory2", true);
                };
                rankSpine = winSpine.gameObject;
            }
        
        }
        else
        {
            rankSpine = rankGroup.FindChild("1v1rankSpine");
        }

        if (rankSpine != null)
        {
            rankSpine.SetActive(true);
        }
    }

    private void Set1V1(ShowState value)
    {
        _1v1ScoreGroup.SetActive(true);

        InitSelfScoreItem(selfScoreItem);

        InitOtherScoreItem(otherScoreItem, otherInfo);

        selfScoreItem.RankGroup.SetActive(true);
        otherScoreItem.RankGroup.SetActive(otherInfo != null);

        //设置排名
        switch (value)
        {
            case ShowState.Win:
                selfScoreItem.image_RankIcon.sprite =
                    ResourceSystem.That.LoadAssetSync<Sprite>($"{ClassType.Name}/rank_1");
                otherScoreItem.image_RankIcon.sprite =
                    ResourceSystem.That.LoadAssetSync<Sprite>($"{ClassType.Name}/rank_2");
                break;
            case ShowState.Lose:
                selfScoreItem.image_RankIcon.sprite =
                    ResourceSystem.That.LoadAssetSync<Sprite>($"{ClassType.Name}/rank_2");
                otherScoreItem.image_RankIcon.sprite =
                    ResourceSystem.That.LoadAssetSync<Sprite>($"{ClassType.Name}/rank_1");
                break;
            case ShowState.Wait:
            case ShowState.LookingRival:
                selfScoreItem.image_RankIcon.SetActive(false);
                otherScoreItem.image_RankIcon.SetActive(false);
                break;
        }

        if (otherInfo is { win_result: > 0 and <= 2 })
        {
            otherScoreItem.image_RankIcon.sprite =
                ResourceSystem.That.LoadAssetSync<Sprite>($"{ClassType.Name}/rank_{otherInfo.win_result}");
        }

        // 我和对手的排名
        if (value is ShowState.Lose)
        {
            otherScoreItem.transform.SetAsFirstSibling();
        }
        else
        {
            selfScoreItem.transform.SetAsFirstSibling();
        }
    }

    private void Set5Seat(ShowState value)
    {
        _5SeatScoreGroup.SetActive(true);

        //排序
        var matchHistory = vm[vname.matchHistory.ToString()].ToIObservable<List<MatchHistory>>().Value;
        matchHistory.Sort((a, b) =>
        {
            if (a.win_result < b.win_result)
            {
                return -1;
            }

            if (a.win_result > b.win_result)
            {
                return 1;
            }

            return b.game_score.CompareTo(a.game_score);
        });

        for (int i = 0; i < ScoreItems.Length; i++)
        {
            MatchHistory history = null;
            if (i < matchHistory.Count)
            {
                history = matchHistory[i];
            }

            var scoreItem = ScoreItems[i];

            if (isFromGame)
            {
                scoreItem.transform.localScale = Vector3.zero;
                this.AttachTimer(0.03f * i, () =>
                {
                    scoreItem.GetComponent<AnimationSequence>().Play();
                });
            }
            
            if (history != null && history.user_id == Root.Instance.UserId)
            {
                InitSelfScoreItem(scoreItem);
            }
            else
            {
                InitOtherScoreItem(scoreItem, history);
            }

            switch (value)
            {
                case ShowState.Win:
                case ShowState.Lose:
                    var rankValue = i + 1;
                    if (history is { win_result: > 0 })
                    {
                        rankValue = history.win_result;
                    }

                    scoreItem.image_RankIcon.sprite =
                        ResourceSystem.That.LoadAssetSync<Sprite>($"uimain/rank{rankValue}");
                    break;
                case ShowState.Wait:
                case ShowState.LookingRival:
                    scoreItem.image_RankIcon.SetActive(false);
                    break;
            }

            scoreItem.image_RankIcon.SetNativeSize();
        }
    }

    public override void InitEvents()
    {
        AddEventListener(Proto.MATCH_CLIAM, (sender, eventArgs) =>
        {
            if (eventArgs is ProtoEventArgs { Result: ProtoResult.Fail })
            {
                UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_match_cliam_error"));
                UserInterfaceSystem.That.CloseAllUI(new[] { nameof(UIMain), nameof(UIGetRewards) });
            }
        });

        AddEventListener(GlobalEvent.Sync_TaskInfo, (sender, eventArgs) =>
        {
            if (Root.Instance.CurTaskInfo.level == 0)
            {
                TaskBtnTrans.SetActive(false);
            }
        });
    }

    private void SetReward(List<Item> reward)
    {
        if (reward == null)
        {
            text_GoldText.text = "0";
            text_TScoreText.text = "0";
            text_DiamondText.text = "0";
            return;
        }

        var gold = reward.Find(item => item.id is Const.Cash or Const.Bonus);
        text_GoldText.text = gold != null ? GameUtils.TocommaStyle(gold.Count) : "0";

        var chips = reward.Find(item => item.id == Const.Chips);
        text_DiamondText.text = chips != null ? GameUtils.TocommaStyle(chips.Count) : "0";

        var coin = reward.Find(item => item.id == Const.Coin);

        text_TScoreText.text = coin != null ? GameUtils.TocommaStyle(coin.Count) : "0";
    }

    /// <summary>
    /// 设置自己的信息
    /// </summary>
    void InitSelfScoreItem(ScoreItem scoreItem)
    {
        if (myInfo == null)
        {
            return;
        }

        scoreItem.text_PlayerName.text = I18N.Get("key_you");

        Root.Instance.Role.LoadIcon(scoreItem.image_PlayerIcon);

        if (myInfo.sub_status == "game_expire")
        {
            scoreItem.text_ScoreText.text = I18N.Get("key_game_over_time");
        }
        else
        {
            scoreItem.text_ScoreText.text = GameUtils.TocommaStyle(myInfo.game_score);
        }

        ColorUtility.TryParseHtmlString("#c14e10", out var color);
        
        scoreItem.text_PlayerName.color = color;

        scoreItem.text_ScoreText.color = color;

        scoreItem.image_bg.sprite = ResourceSystem.That.LoadAssetSync<Sprite>($"{ClassType.Name}/bg_rank_self");
    }

    void InitOtherScoreItem(ScoreItem scoreItem, MatchHistory history)
    {
        var state = vm[vname.State.ToString()].ToIObservable<ShowState>().Value;
        ShowState showState = state;

        if (history == null)
        {
            showState = ShowState.LookingRival;
        }
        else if (IsFiveSeatRoom())
        {
            if (!history.IsGameEnd)
            {
                showState = ShowState.Wait;
            }
            else
            {
                showState = ShowState.Win;
            }
        }

        switch (showState)
        {
            case ShowState.Win:
            case ShowState.Lose:
                scoreItem.text_ScoreText.text = GameUtils.TocommaStyle(history.game_score);
                scoreItem.text_PlayerName.text = history.nickname;
                scoreItem.SearchingGroup.SetActive(false);
                break;
            case ShowState.Wait:
                scoreItem.text_ScoreText.text = I18N.Get("WAIT_RIVAL");
                scoreItem.text_PlayerName.text = history.nickname;
                scoreItem.SearchingGroup.SetActive(false);
                break;
            case ShowState.LookingRival:
                scoreItem.image_PlayerIcon.sprite =
                    MediatorBingo.Instance.GetSpriteByUrl("common/playericon_holder");
                scoreItem.SearchingGroup.SetActive(true);
                //隐藏分数
                scoreItem.text_ScoreText.SetActive(false);
                scoreItem.text_PlayerName.SetActive(false);
                scoreItem.desc_text.text = I18N.Get("SEARCHING_RIVAL");
                break;
        }

        switch (showState)
        {
            case ShowState.Win:
            case ShowState.Lose:
            case ShowState.Wait:
                if (!string.IsNullOrEmpty(history.head_url))
                {
                    scoreItem.image_PlayerIcon.ServerUrl(history.head_url, false);
                }
                else
                {
                    var sprite = Root.Instance.LoadPlayerIconByIndex(history.head_index);
                    if (sprite != null)
                    {
                        scoreItem.image_PlayerIcon.sprite = sprite;
                    }
                }

                break;
            case ShowState.LookingRival:
                scoreItem.image_PlayerIcon.sprite =
                    MediatorBingo.Instance.GetSpriteByUrl("common/playericon_holder");
                break;
        }

        // scoreItem.text_PlayerName.GetComponent<Text2DOutline>().Disable();
        // scoreItem.text_ScoreText.GetComponent<Text2DOutline>().Disable();

        scoreItem.image_bg.sprite = ResourceSystem.That.LoadAssetSync<Sprite>($"{ClassType.Name}/bg_rank_other");
    }

    private bool IsFiveSeatRoom()
    {
        var matchHistory = vm[vname.matchHistory.ToString()].ToIObservable<List<MatchHistory>>().Value;
        if (matchHistory.Count > 2)
        {
            return true;
        }

        var room = GetRoomInfo();
        if (room is { seat: > 2 })
        {
            return true;
        }

        return false;
    }

    protected override void OnClose()
    {
        base.OnClose();
        if (isFromGame && myInfo is { win_result: not 1 } && GetRoomInfo() is { IsLuckyRoom: true })
        {
            if (Root.Instance.LuckyGuyInfo is { play_chance: < 2 })
            {
                EventDispatcher.Root.Raise(GlobalEvent.Rigister_Lucky_guy_fail);
            }
        }
    }
}