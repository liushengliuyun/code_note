using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BrunoMikoski.AnimationsSequencer;
using Carbon.Util;
using CatLib.EventDispatcher;
using Coffee.UIEffects;
using Core.Extensions;
using Core.Extensions.UnityComponent;
using Core.Services.AudioService.API.Facade;
using Core.Services.NetService.API.Facade;
using Core.Services.PersistService.API.Facade;
using Core.Services.ResourceService.API.Facade;
using Core.Services.ResourceService.Internal.UniPooling;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using Core.Third.I18N;
using Cysharp.Threading.Tasks;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using MyBox;
using Newtonsoft.Json;
using Reactive.Bindings;
using Spine.Unity;
using UI.Animation;
using UI.Effect;
using UI.Mono;
using UniRx;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityTimer;
using Utils;
using Color = UnityEngine.Color;
using Image = UnityEngine.UI.Image;
using Sequence = DG.Tweening.Sequence;
using Text = UnityEngine.UI.Text;

namespace UI
{ 
    public class UIBingo : UIBase<UIBingo>
    {
        #region 绑定

        [SerializeField] private Button skipBtn;

        /// <summary>
        /// guide group 父节点
        /// </summary>
        [SerializeField] private Transform guideMask;

        //---------引导分割线---------
        [SerializeField] private Transform BingoTitleGroup;
        [SerializeField] private Image CallSlider;
        [SerializeField] private Image _4X1_CallSlider;

        [SerializeField] private Transform CallArrayParent;
        private Transform CallGroup => CallArrayParent.parent;

        /// <summary> 
        /// _4X1_CallSlider的父节点 
        /// </summary>
        [SerializeField] private Transform _4X1_CallArrayParent;

        [SerializeField] private Transform scoreStartTransform;


        [SerializeField] private GameObject finger;

        [SerializeField] private UIBingoPauseGroup pauseGroupMono;

        public MyButton BingoBtn;

        /// <summary>
        /// 已经匹配到bingo了， btn切换
        /// </summary>
        [SerializeField] private MyButton BingoedBtn;

        public Button PauseBtn;

        [FormerlySerializedAs("CanclePauseBtn")]
        public Button CancelPauseBtn;

        public Transform TopGroup;
        public Transform GridsGroup;
        public Transform BottomGroup;


        public Text ScoreText;


        // public SoftMaskScript SoftMaskScript;

        public Text TimeText;

        public Text LessPauseTimeText;

        public Slider PropSlider;

        //双倍剩余时间倒计时
        
        [SerializeField]  private Text DoubleScoreText;
        
        [SerializeField]  private Transform DoublePartical;

        /// <summary>
        /// 等待在ui上体现
        /// </summary>
        [FormerlySerializedAs("_2xSlider")] public Slider DoubleScoreSlider;

        [FormerlySerializedAs("DoubleSliderGroup")]
        public Transform DoubleScoreGroup;

        //style相关
        [SerializeField] private Image game_bg_1;
        [SerializeField] private Image game_bg_2;
        [SerializeField] private Image ballsPaltform;
        [SerializeField] private Image bingo_grids_bg;
        [SerializeField] private Image game_bg_dissolve;

        //道具 -------------------
        [SerializeField] private Image propSliderBg;

        /// <summary>
        /// 道具父节点
        /// </summary>
        public Transform PropTransform;

        public MyText PropCountDownText;

        public Image PropTextImage;

        public Transform _4X1Transform;
        [SerializeField] private Transform ChooseAnyGroup;
        [SerializeField] private Transform CrossGroup;

        [FormerlySerializedAs("PropUseingMask")] [SerializeField]
        private Image PropUsingMask;

        [SerializeField] private Transform PropUseTitle;

        [FormerlySerializedAs("PropText")] public Text PropEnergyText;
        //道具 end-------------

        [SerializeField] private Text BingoCountText;

        [SerializeField] private AnimationSequence BingoTextEffect;

        [SerializeField] private AnimationSequence AmazingTextEffect;

        [SerializeField] private Transform FullHouseEffect;

        [SerializeField] private AnimationSequence DoubleGroupEffect;

        [SerializeField] private AnimationSequence DoubleTextEffect;

        [SerializeField] private AnimationSequence TimeUpTextEffect;

        [SerializeField] private AnimationSequence _10sLeft;

        [SerializeField] private AnimationSequence _30sLeft;

        [SerializeField] private AnimationSequence beginEffect;

        [SerializeField] private AnimationSequence PropUsingMaskEffect;

        [SerializeField] private AnimationSequence _4x1GroupEffect;

        [SerializeField] private AnimationSequence PropUseTitleEffect;

        [SerializeField] private AnimationSequence PauseGroupEffect;

        #endregion 绑定----------------------------------------------------------

        #region 数据定义

        //Unity 对象
        /// <summary>
        /// 引导点击手指
        /// </summary>
        private GameObject finger1;

        private Canvas PauseCanvas => pauseGroupMono.transform.GetComponent<Canvas>();

        private Transform PauseGroup => pauseGroupMono.transform;


        //Unity 对象 end

        public static bool IsCheat;

        private int matchCountAtLogin = -1;

        /// <summary>
        /// bingo的次数
        /// </summary>
        private int bingoCount => bingoScore.BingoCount;

        /// <summary>
        /// 唱票队列长度
        /// </summary>
        private static int CallArrayLength = 5;

        /// <summary>
        /// 历史唱票数据
        /// </summary>
        private HashSet<int> HistorySet = new();

        //当前队列唱票数据
        public readonly byte[] CallArrayValues = new byte[CallArrayLength];

        /// <summary>
        /// 总的唱票数据,逐渐缩小的
        /// </summary>
        private List<byte> TotalCallArray;

        /// <summary>
        /// 变成0的时候， 开始
        /// </summary>
        private int callIndex = -1;

        /// <summary>
        /// 打成各种目标的记录
        /// </summary>
        private readonly Dictionary<string, int> statistical = new();

        //棋盘数据
        private byte[] bingo_grid_data;

        //6选1或者4选1的结果
        private readonly byte[] chooseOneArray = new byte[4];

        //道具列表
        private byte[] propIdArray;

        /// <summary>
        /// 四选一列表
        /// </summary>
        private string[] chooseArray;

        private List<int[]> operateList;

        //剩余时间
        private float lessTime = GlobalEnum.ONE_GAME_TIME;

        /// <summary>
        /// 剩余暂停时间
        /// </summary>
        private int lessPauseTime = GlobalEnum.TOTAL_PAUSE_TIME;

        private BingoGrid[] bingoGrids;

        //骰子上会出现多少种值
        private const int DICE_NUMBER_COUNT = 6;

        private const int DICE_COUNT = 5;

        /// <summary>
        /// 剩余时间得分系数
        /// </summary>
        private const int TIME_COEFFICIENT = 50;

        const int FULL_CLICKED_BONUS = 2000;

        //增加的能量点数
        const int ADD_ENERGY_COUNT = 7;

        //最多可以储存的道具数量
        const int MAX_PROP_COUNT = 3;

        /// <summary>
        /// 点错bingo 按钮扣分
        /// </summary>
        const int ERROR_CLICK_BINGO = 200;

        /// <summary>
        /// 点错棋子扣分
        /// </summary>
        const int ERROR_CLICK_GRID = 100;

        /// <summary>
        /// bingo额外加分
        /// </summary>
        const int BINGO_BONUS = 1000;

        /// <summary>
        /// 超过3s后， 分数减半
        /// </summary>
        const int BASE_MATCH_BONUS = 300;

        /// <summary>
        /// 分数减益系数
        /// </summary>
        const float NEGATIVE_RADIO = 0.5f;

        /// <summary>
        /// 使用道具选中加分
        /// </summary>
        const int PROP_MATCH_BONUS = 300;

        private int scoreMultiple = 1;

        //分数计算系数
        private int ScoreMultiple
        {
            set
            {
                if (IsInitEnd)
                {
                    //连续使用双倍道具的情况
                    if (value > 1)
                    {
                        SetDoubleAnimation();
                    }

                    if (value == scoreMultiple)
                    {
                        return;
                    }

                    if (value > 1)
                    {
                        DoubleScoreGroup.SetActive(true);
                    }
                    else
                    {
                        DoubleScoreGroup.HideUIByEffect(DoubleGroupEffect, callback: () =>
                        {
                            DoubleScoreGroup.SetActive( !isSendGameEnd && scoreMultiple > 1);
                        });
                    }
                }

                scoreMultiple = value;
            }
            get => scoreMultiple;
        }

        private int callArrayLength => TotalCallArray.Count;

        //使用四选一道具的次数
        private int useChooseOneCount;

        private readonly List<Prop> propList = new();

        //双倍积分的倒计时
        private float multipleScoreCountDown;

        /// <summary>
        /// 道具可以使用的倒计时
        /// </summary>
        private int propUseCountDown;

        private int leftCallValue;

        //道具用到哪一个了 
        private int propIndex;

        // private int frames;

        private bool isBegin;

        //Totween Seq 声明
        private Sequence bingoBtnSeq;

        private Sequence callArraySeq;

        private Sequence _4x1_callArraySeq;

        private Sequence doubleSliderSeq;

        private Sequence textMoveSeq;

        private bool inNormalAnimation;

        private bool inAnimation => inNormalAnimation || (textMoveSeq != null && textMoveSeq.IsPlaying());

        readonly WaitForSeconds waitTimeYield = new WaitForSeconds(0.5f);

        private RoomStyle roomStyle = RoomStyle.FreeBonus;
#if DAI_TEST && false
        private bool IsFirstGuide = false;
#else
        /// <summary>
        /// matchCountAtLogin <=1 即使保存了matchCountAtLogin, 如果在匹配过程中退出, 存不到这个值
        /// </summary>
        private bool IsFirstGuide => PlayerGuideType is BingoGuideType.None &&
                                     matchCountAtLogin <=1 &&
                                     Root.Instance.NotPassTheNewPlayerGuide(NewPlayerGuideStep
                                         .FIRST_ROOM_GUIDE_FINISH);
#endif

        private bool IsGuiding => IsFirstGuide || PlayerGuideType is BingoGuideType.Teaching;

        private int guideCount;

        private float AddCallTime;
        
        private static int first_guide_grid_index = 6;

        private static int second_guide_grid_index = 5;
            
        private static int three_guide_grid_index = 8;


        /// <summary>
        /// 前3局开启
        /// </summary>
        private bool IsSoftGuideOpen => false &&
                                        PlayerGuideType is BingoGuideType.None &&
                                        Root.Instance.MatchHistoryCount <= 2 && matchCountAtLogin <= 3 &&
                                        !IsFirstGuide;


        private int LastPopPauseTime;

        private bool IsShowPopGroup;

#if DAI_TEST
        private bool IsPropEffectOpen => false;
#else
        // private bool IsPropEffectOpen => Root.Instance.MatchHistoryCount <= 5 && matchCountAtLogin <= 6;
       private bool IsPropEffectOpen => false;
#endif

        private int prop_4x1_choose_value;

        private int grid_bingo_count;
        
        private float scoreDuration = 1f;

        /// <summary>
        /// 道具得分
        /// </summary>
        private bool is_prop_score;
        
        
        private bool is_double_score;
        
        #endregion 数据定义 END

        #region 生命周期函数

        //方便智能提示, 避免错误
        enum vname
        {
            /// <summary>
            /// 当前唱票位置
            /// </summary>
            VM_CallIndex,


            VM_CurrentChooseProp,

            /// <summary>
            /// 当前唱票队列
            /// </summary>
            VM_CurrentArray,


            /// <summary>
            /// 道具能量
            /// </summary>
            VM_PropEnergy,

            VM_Grids,

            /// <summary>
            /// 总分数
            /// </summary>
            VM_Score,

            /// <summary>
            /// 是否暂停
            /// </summary>
            VM_IsPause,

            VM_ChooseOneArray,

            /// <summary>
            /// 4x1道具选中的值
            /// </summary>
            VM_Choose_4X1_Value,

            /// <summary>
            /// 棋盘上的bingo次数
            /// </summary>
            VM_GRIDS_BINGO_COUNT
        }

        public override void InitVm()
        {
            //不在subscribe的时候自动调用一次
            vm[vname.VM_CallIndex.ToString()] =
                new ReactivePropertySlim<int>(callIndex, ReactivePropertyMode.DistinctUntilChanged);

            //当前显示的结果
            vm[vname.VM_CurrentArray.ToString()] = new ReactivePropertySlim<byte[]>(CallArrayValues);

            vm[vname.VM_Grids.ToString()] = new ReactivePropertySlim<BingoGrid[]>(bingoGrids);

            vm[vname.VM_PropEnergy.ToString()] = new ReactivePropertySlim<int>(propEnergy);

            //当前分数
            vm[vname.VM_Score.ToString()] = new ReactivePropertySlim<BingoScore>(bingoScore);

            //当前选择的道具
            vm[vname.VM_CurrentChooseProp.ToString()] = new ReactivePropertySlim<Prop>(currentChooseProp);

            vm[vname.VM_IsPause.ToString()] = new ReactivePropertySlim<bool>(isPause);

            vm[vname.VM_ChooseOneArray.ToString()] = new ReactivePropertySlim<byte[]>(chooseOneArray);

            vm[vname.VM_Choose_4X1_Value.ToString()] = new ReactivePropertySlim<int>(prop_4x1_choose_value);

            vm[vname.VM_GRIDS_BINGO_COUNT.ToString()] = new ReactivePropertySlim<int>(grid_bingo_count);
        }

        public override void InitBinds()
        {
            vm[vname.VM_Choose_4X1_Value.ToString()].ToIObservable<int>().Subscribe(value =>
            {
                if (value > 0)
                {
                    // TopGroup.SetActive(false);
                    CallGroup.SetActive(false);
                    BottomGroup.SetActive(false);
                    _4X1_CallArrayParent.SetActive(true);
                    Init4X1CallBall();
                }
                else
                {
                    _4X1_CallArrayParent.SetActive(false);
                    // TopGroup.SetActive(true);
                    CallGroup.SetActive(true);
                    BottomGroup.SetActive(true);
                }
            });

            int lastPropId = -1;
            vm[vname.VM_CurrentChooseProp.ToString()].ToIObservable<Prop>().Subscribe(prop =>
            {
                if (prop == null)
                {
                    HidePropUI();
                    //取消音效
                    CancelPropCountDown();

                    switch (lastPropId)
                    {
                        case Const.Cross:
                            //设置除中心点外的 可以被点击
                            SetAllGridClickState(false);
                            break;
                        case Const.ChooseOne:
                            ResetChooseOneArray();
                            break;
                    }

                    doubleSliderSeq?.Play();
                    lastPropId = -1;
                    return;
                }

                propUseCountDown = (int)prop.spanTime;

                Record(Operation.BeginProp, prop.id);

                lastPropId = prop.id;

                switch (prop.id)
                {
                    case Const.ChooseOne:
                        PropTextImage.sprite = MediatorBingo.Instance.GetSpriteByUrl("uibingo/uibingo_pick-a-ball");
                        //可能 没有剩余的球了
                        // _4X1Transform.SetActive(true);
                        SetChooseOneArray(out var success);
                        if (!success)
                        {
                            //todo
                            UserInterfaceSystem.That.ShowUI<UITip>("There's no place to use it");
                            return;
                        }

                        break;
                    case Const.ChooseAny:
                        if (IsAllClicked())
                        {
                            //todo
                            UserInterfaceSystem.That.ShowUI<UITip>("There's no place to use it");
                            return;
                        }

                        PropTextImage.sprite = MediatorBingo.Instance.GetSpriteByUrl("uibingo/uibingo_wild_daub");
                        ChooseAnyGroup.SetActive(true);
                        break;
                    case Const.Cross:
                        PropTextImage.sprite = MediatorBingo.Instance.GetSpriteByUrl("uibingo/uibingo_cross_clear");
                        CrossGroup.SetActive(true);
                        //设置除中心点外的 可以被点击
                        SetAllGridClickState(true);
                        break;
                    default:
                        return;
                }

                //选择道具的时候， 双倍应该暂停
                doubleSliderSeq?.Pause();
                PropCountDown();
            });

            vm[vname.VM_ChooseOneArray.ToString()].ToIObservable<byte[]>()
                .Subscribe(value => { InitChooseGroupCom(); });

            vm[vname.VM_IsPause.ToString()].ToIObservable<bool>().Subscribe(value =>
            {
                if (value)
                {
                    AudioSystem.That.PauseSound(SoundPack.Double_Scoreing);
                    AudioSystem.That.PauseSound(SoundPack.Prop_Use_Count_Down);
                    callArraySeq?.Pause();
                    _4x1_callArraySeq?.Pause();
                    doubleSliderSeq?.Pause();

                    LessPauseTimeText.text = TimeUtils.Instance.ToMinuteSecond(Convert.ToInt32(lessPauseTime));

                    IsShowPopGroup = true;
                }
                else
                {
                    AudioSystem.That.Resume(SoundPack.Double_Scoreing);
                    AudioSystem.That.Resume(SoundPack.Prop_Use_Count_Down);
                    callArraySeq?.Play();
                    _4x1_callArraySeq?.Play();
                    doubleSliderSeq?.Play();

                    if (PauseGroup.IsActive())
                    {
                        LastPopPauseTime = TimeUtils.Instance.UtcTimeNow;
                    }
                }

                if (!value)
                {
                    PauseGroup.HideUIByEffect(PauseGroupEffect);
                }
                else
                {
                    PauseGroup.SetActive(true);
                }
            });

            int lastScoreValue = 0;
            vm[vname.VM_Score.ToString()].ToIObservable<BingoScore>().Subscribe(value =>
            {
                if (!IsInitEnd)
                {
                    ScoreText.text = value.TotalScoreInGame.ToString();
                    lastScoreValue = value.TotalScoreInGame;
                    return;
                }

                if (lastScoreValue == value.TotalScoreInGame && value.AddPenaltyScore <= 0)
                {
                    return;
                }

                //虽然重复了， 但有历史因素
                var startValue = lastScoreValue;

                var diff = value.TotalScoreInGame - startValue;

                if (diff <= 0)
                {
                    diff = -value.AddPenaltyScore;
                }

                //不管用没用都清空
                value.AddPenaltyScore = 0;
                lastScoreValue = value.TotalScoreInGame;

                string textContent = "";

                string colorCode;
                string outColorCode = "";
                if (is_double_score)
                {
                    colorCode = "#fffec4";
                    outColorCode = "#d03442";
                }
                else if (is_prop_score)
                {
                    colorCode = "#71fdff";
                    outColorCode = "#1a79a1";
                }
                else if (diff > 0)
                {
                    colorCode = "#71ff78";
                    outColorCode = "#167a24";
                }
                else
                {
                    colorCode = "#fe4e4e";
                    outColorCode = "#841111";
                }
                
                if (diff > 0)
                {
                    textContent = "+" + diff;
                }
                else
                {
                    textContent = diff.ToString();
                }
                
                ColorUtility.TryParseHtmlString(colorCode, out var color);
                
                var scale = 1f;
                if (diff is >= 1000 and < 2000)
                {
                    scale = 2f;
                }
                else if (diff >= 2000)
                {
                    scale = 3f;
                }

                textMoveSeq = UIEffectUtils.Instance.CreatMoveText(scoreStartTransform, null,
                    textContent,
                    color,outColorCode,
                    scoreDuration,
                    scale);

                scoreDuration = 1f;
                this.AttachTimer(0.2f,
                    () =>
                    {
                        DOTween.To(() => startValue, x => startValue = x, lastScoreValue, 1f).OnUpdate(() =>
                        {
                            ScoreText.text = startValue.ToString();
                        }).SetEase(Ease.OutQuint);
                    });
            });

            vm[vname.VM_CallIndex.ToString()].ToIObservable<int>().Subscribe(value =>
            {
                if (value < 0)
                {
                    return;
                }

                Record(Operation.AddCallIndex, value);
                
                //设置数据
                SetNewCallValue();

                //添加时间
                AddCallTime = lessTime; 
                PlayBingoShiny(CallArrayValues[0]);
                
                MediatorBingo.Instance.PlayBingoSound(CallArrayValues[0]);

                //动画配合
                CallAnimation();

                SaveGameData();
            });

            vm[vname.VM_GRIDS_BINGO_COUNT.ToString()].ToIObservable<int>().Subscribe(value =>
            {
                BingoedBtn.SetActive(value > 0);
            });

            vm[vname.VM_PropEnergy.ToString()].ToIObservable<int>()
                .Subscribe((value) =>
                {
                    if (IsInitEnd)
                    {
                        Record(Operation.PropEnergyChange, value);
                        var canAddProp = TryAddProp();

                        value = (int)Math.Min(value, PropSlider.maxValue);
                        
                        if (value != PropSlider.value)
                        {
                            if (PropSlider.value >= PropSlider.maxValue)
                            {
                                //现在是从10 变为0 
                                PropSlider.value = 0;
                            }
                            
                            ChangeSliderValue(PropSlider, value, 0.2f, () =>
                            {
                                SetPropSlierTextState();
                                if (canAddProp)
                                {

                                    this.AttachTimer(0.1f, () =>
                                    {
                                        /*ChangeSliderValue(PropSlider,
                              vm[vname.VM_PropEnergy.ToString()].ToIObservable<int>().Value, 0.1f,
                              null,
                              () =>
                              {
                                  // PropEnergyText.text = $"{PropSlider.value}/{PropSlider.maxValue}";
                              });*/
                                        
                                        PropSlider.value = vm[vname.VM_PropEnergy.ToString()].ToIObservable<int>() .Value;
                                    });
                                }
                            }, () =>
                            {
                                // PropEnergyText.text = $"{PropSlider.value}/{PropSlider.maxValue}";
                            });
                        }
                    }
                    else
                    {
                        PropSlider.value = value;
                        // PropEnergyText.text = $"{PropSlider.value}/{PropSlider.maxValue}";
                    }
                });

            vm[vname.VM_CurrentArray.ToString()].ToIObservable<byte[]>().Subscribe((value) =>
            {
                if (!IsInitEnd)
                {
                    //还原布局
                    RestoreCallingUI();
                }
            });

            vm[vname.VM_Grids.ToString()].ToIObservable<BingoGrid[]>().Subscribe((bingoGrids) =>
            {
                InitGridComs();
                CheckBingoBtnState();
            });

            //游戏初始化的时候保存一次
            //是否来自 game begin的请求 ， 而不是重建
            bool fromRequest = GetTable()?["fromRequest"] is bool ? (bool)GetTable()?["fromRequest"] : false;
            if (!IsCloseing && !fromRequest)
            {
                SaveGameData();
            }
        }

        private void RestoreCallingUI()
        {
            for (int i = 0; i < CallArrayValues.Length; i++)
            {
                var value = CallArrayValues[i];
                if (value <= 0)
                {
                    break;
                }

                var ball = UIEffectUtils.Instance.GetCallBall();
                ball.name = (i - 1).ToString();
                ball.transform.SetParent(CallArrayParent);
                ball.transform.localScale = Vector3.one;
                InitCallBallObj(ball, value);
            }

            SetCallingBallAnimationSeq(0.1f);
        }

        private bool TryAddProp()
        {
            var value = GetVmValue(vname.VM_PropEnergy.ToString(), propEnergy, out var success);

            var canAddProp = value >= PropSlider.maxValue && propList.Count < MAX_PROP_COUNT;

            if (canAddProp)
            {
                //寻找空位
                var index = FindPropContainerIndex();

                if (index >= 0)
                {
                    var prop = AddNewProp(index);

                    if (success && !IsGuiding && !IsFakeGame())
                    {
                        MediatorBingo.Instance.CheckPropNeedTip(prop);
                    }
                }

                // vm[vname.VM_PropEnergy.ToString()].ToIObservable<int>().LatestValue =
                //     propList.Count >= MAX_PROP_COUNT ? 0 : Convert.ToInt32(value - PropSlider.maxValue);

                if (success)
                {
                    // vm[vname.VM_PropEnergy.ToString()].ToIObservable<int>().LatestValue = (int)PropSlider.maxValue;
                    vm[vname.VM_PropEnergy.ToString()].ToIObservable<int>().LatestValue = 0;
                }
                else
                {
                    // propEnergy = (int)PropSlider.maxValue;
                    propEnergy = 0;
                }
            }

            return canAddProp;
        }

        void InitCallBallObj(GameObject ball, int value)
        {
            var imageCom = ball.FindChild<Image>("root/Image");
            var bingoType = GetBingoType(value);
            imageCom.sprite = MediatorBingo.Instance.GetBingoBallImage(bingoType);

            var textCom = ball.FindChild<Text>("root/Text");
            textCom.text = value.ToString();

            textCom.color = GetBingoColor(bingoType);
        }

        private Color GetBingoColor(BingoType bingoType)
        {
            string colorStr = "#0a698a";
            switch (bingoType)
            {
                case BingoType.B:
                    colorStr = "#0a698a";
                    break;
                case BingoType.I:
                    colorStr = "#97112f";
                    break;
                case BingoType.N:
                    colorStr = "#0e8116";
                    break;
                case BingoType.G:
                    colorStr = "#751091";
                    break;
                case BingoType.O:
                    colorStr = "#953404";
                    break;
            }

            ColorUtility.TryParseHtmlString(colorStr, out var result);
            return result;
        }

        BingoType GetBingoType(int value)
        {
            if (value is > 0 and <= 15)
            {
                return BingoType.B;
            }

            if (value is > 15 and <= 30)
            {
                return BingoType.I;
            }

            if (value is > 30 and <= 45)
            {
                return BingoType.N;
            }

            if (value is > 45 and <= 60)
            {
                return BingoType.G;
            }

            if (value > 60)
            {
                return BingoType.O;
            }
            else
            {
                return BingoType.B;
            }
        }

        async UniTask Init4X1CallBall()
        {
            int value = vm[vname.VM_Choose_4X1_Value.ToString()].ToIObservable<int>().Value;
            if (value <= 0)
            {
                return;
            }

            PlayBingoShiny(value);
            MediatorBingo.Instance.PlayBingoSound(value);
            var ball = UIEffectUtils.Instance.GetCallBall();
            ball.transform.SetParent(_4X1_CallArrayParent);
            ball.transform.localScale = Vector3.one;
            InitCallBallObj(ball, value);

            var time1 = 0.5f;

            _4X1_CallSlider.fillAmount = 1f;

            var token = this.GetCancellationTokenOnDestroy();

            await UniTask.WaitUntil(() => isBegin, cancellationToken: token);

            var slider_tweener = DOTween.To(() => _4X1_CallSlider.fillAmount,
                    value => _4X1_CallSlider.fillAmount = value, 0, 3f)
                .OnStart(() => { _4X1_CallSlider.SetActive(true); });

            _4x1_callArraySeq = GetCallBallFallSeq(ball.transform, time1, true)
                .Append(slider_tweener).OnComplete(() =>
                {
                    //总感觉有点慢？
                    _4X1_CallSlider.SetActive(false);

                    Record(Operation.End4X1Choose);

                    End4X1Choose();

                    ball.Restore();
                });
        }

        private void End4X1Choose()
        {
            if (vm.Any())
            {
                vm[vname.VM_Choose_4X1_Value.ToString()].ToIObservable<int>().Value = 0;
            }
            else
            {
                prop_4x1_choose_value = 0;
            }
        }

        private void CallAnimation()
        {
            //创建一个下落的球
            var ball = UIEffectUtils.Instance.GetCallBall();
            //绑定索引
            ball.name = "-1";
            ball.transform.SetParent(CallArrayParent);
            ball.transform.localScale = Vector3.one;
            InitCallBallObj(ball, CallArrayValues[0]);


            //隐藏call time 进度条
            CallSlider.SetActive(false);
            CallSlider.fillAmount = 1;

            SetCallingBallAnimationSeq(0.5f);
        }

        private void SetCallingBallAnimationSeq(float duration)
        {
            var baseX = 85;
            var baseY = -35;
            var offsetX = -125;

            //找到各个球对应的索引
            callArraySeq?.Kill();
            callArraySeq = DOTween.Sequence();

            for (int i = 0; i < CallArrayParent.childCount; i++)
            {
                var child = CallArrayParent.GetChild(i);
                var name = child.name;
                var dataIndex = name.ToInt32() + 1;
                child.name = dataIndex.ToString();
                //动画
                if (dataIndex == 0)
                {
                    //先下落 
                    var seq = GetCallBallFallSeq(child, duration).OnComplete(() =>
                    {
                        //显示call time 进度条
                        CallSlider.SetActive(true);
                    });
                    callArraySeq.Join(seq);
                }

                //滚动360
                if (dataIndex == 1)
                {
                    var move = child.DOLocalMove(new Vector3(baseX, baseY, 0), duration).SetEase(Ease.OutCirc);
                    var size = child.DOScale(Vector3.one * 0.65f, duration);
                    child.localEulerAngles = Vector3.zero;

                    var rotate = child.DOLocalRotate(new Vector3(0, 0, 360), duration, RotateMode.FastBeyond360)
                        .SetEase(Ease.OutBack);

                    var ballValue = CallArrayValues[1];
                    var changeImage = DOTween.Sequence().OnComplete(() =>
                    {
                        child.gameObject.FindChild<Image>("root/Image").sprite =
                            MediatorBingo.Instance.GetBingoBallImageSmall(GetBingoType(ballValue));
                    });

                    //更换图片
                    callArraySeq.Join(changeImage).Join(move).Join(size).Join(rotate);
                    // callArraySeq.Join(move).Join(size).Join(rotate);
                }

                if (dataIndex is >= 2 and < 5)
                {
                    child.localScale = Vector3.one * 0.65f;
                    var move = child.DOLocalMove(new Vector3(baseX + offsetX * (dataIndex - 1), baseY, 0), duration)
                        .SetEase(Ease.OutCirc);

                    child.localEulerAngles = Vector3.zero;
                    var rotate = child.DOLocalRotate(new Vector3(0, 0, 360), duration, RotateMode.FastBeyond360)
                        .SetEase(Ease.OutBack);

                    callArraySeq.Join(move).Join(rotate);
                }

                if (dataIndex == 5)
                {
                    //滚远一点， 消失到屏幕外
                    var move = child.DOLocalMove(new Vector3(baseX + offsetX * (dataIndex - 1) - 100, baseY, 0),
                            duration)
                        .SetEase(Ease.OutCirc);

                    child.localEulerAngles = Vector3.zero;
                    var rotate = child.DOLocalRotate(new Vector3(0, 0, 360), duration, RotateMode.FastBeyond360)
                        .SetEase(Ease.OutBack);

                    callArraySeq.Join(move).Join(rotate).OnComplete(() =>
                    {
                        leftCallValue = 0;
                        Record(Operation.RemoveLast);
                        child.Restore();
                    });
                }
            }
        }

        private Sequence GetCallBallFallSeq(Transform child, float time1, bool is4x1 = false)
        {
            child.localPosition = new Vector3(is4x1 ? 0 : 265, 180, 0);
            var move = child.DOLocalMove(new Vector3(is4x1 ? 0 : 265, 0, 0), time1).SetEase(Ease.OutBack);
            var size = child.DOScale(Vector3.one, time1).SetEase(Ease.OutBack);
            var seq = DOTween.Sequence().Join(move).Join(size);
            return seq;
        }

        void InitUI()
        {
            HidePropUI();

            guideMask.SetActive(false);

            skipBtn.SetActive(false);

            CallSlider.SetActive(false);

            _4X1_CallSlider.SetActive(false);

            PropEnergyText.SetActive(false);

            // DoubleScoreText.SetActive(false);

            DoubleScoreGroup.SetActive(false);

            BingoTextEffect.SetActive(false);

            DoubleTextEffect.SetActive(false);

            TimeUpTextEffect.SetActive(false);

            FullHouseEffect.SetActive(false);

            _10sLeft.SetActive(false);

            _30sLeft.SetActive(false);

            //为了使得出现不突兀
            beginEffect.SetActive(true);

            PropUsingMaskEffect.AutoKill = false;

            _4x1GroupEffect.AutoKill = false;

            PropUseTitleEffect.AutoKill = false;

            PauseGroupEffect.AutoKill = false;

            DoubleGroupEffect.AutoKill = false;
        }

        private void HidePropUI()
        {
            PropUsingMask.transform.HideUIByEffect(PropUsingMaskEffect, callback: () =>
            {
                CrossGroup.SetActive(false);
                ChooseAnyGroup.SetActive(false);
            });

            PropUseTitle.HideUIByEffect(PropUseTitleEffect, timeScale: 3f);

            _4X1Transform.HideUIByEffect(_4x1GroupEffect);
        }

        new void Awake()
        {
            base.Awake();
            InitUI();
        }

        public override void OnStart()
        {
            IsCheat = false;

            var table = GetArgsByIndex<GameData>(0);
            PlayerGuideType = table?["guideType"] is BingoGuideType
                ? (BingoGuideType)table?["guideType"]
                : BingoGuideType.None;

            AudioSystem.That.PlayMusic("audio/battle_bg");


            InitData();

            //确定本次投掷的分数

            RegisterGridClick();

            if (PlayerGuideType is BingoGuideType.None && !IsFirstGuide)
            {
                LastPopPauseTime = TimeUtils.Instance.UtcTimeNow;
            }

            TimeText.text = TimeUtils.Instance.ToMinuteSecond((int)lessTime);

            #region ----------------------Interval--------------------------------------------

            Observable.Interval(TimeSpan.FromSeconds(1f)).Subscribe(
                l => { Interval(); }).AddTo(this);

            #endregion --------------------- Interval End---------------------------------

            //道具使用 mark
            for (int i = 0; i < MAX_PROP_COUNT; i++)
            {
                var propContainer = GetPropContainer(i);

                propContainer.GetComponent<MyButton>().SetClick(() =>
                {
                    if (!propContainer.IsActive)
                    {
                        return;
                    }
                    OnPropContainerClick(propContainer);
                });
            }

            #region----------------------- 组件事件注册-------------------------------------

            skipBtn.onClick.AddListener(GuideEnd);

            BingoBtn.SetClick(OnBingoBtnClick);
            BingoedBtn.SetClick(OnBingoBtnClick);

            // CloseBtn.OnClickAsObservable().Subscribe(unit => { PlayGameEndAnimation(); });

            //暂停
            PauseBtn.OnClickAsObservable().Subscribe(unit => { PopPausePanel(); });

            void CancelPause() => vm[vname.VM_IsPause.ToString()].ToIObservable<bool>().Value = false;

            CancelPauseBtn.onClick.AddListener(CancelPause);

            // _4X1CloseBtn.SetClick(CancelPropCountDown);

            pauseGroupMono.EndNowBtn.SetClick(() => PlayGameEndAnimation(BingoCloseType.EARLY_END, forceEnd: true));

            pauseGroupMono.ResumeBtn.SetClick(CancelPause);
            pauseGroupMono.HowToplayBtn.SetClick(OpenTuition);


            var musicOn = (bool)PersistSystem.That.GetValue<bool>(GlobalEnum.MUTE_MUSIC);
            var soundOn = (bool)PersistSystem.That.GetValue<bool>(GlobalEnum.MUTE_SOUND);
            var vibrateOn = !Root.Instance.VibrationON;

            musicVolume = (float)PersistSystem.That.GetValue<float>(GlobalEnum.MUSIC_VOLUME);
            soundVolume = (float)PersistSystem.That.GetValue<float>(GlobalEnum.SOUND_VOLUME);
            vibrationVolume = (float)PersistSystem.That.GetValue<float>(GlobalEnum.VIBRATION_VOLUME);

            pauseGroupMono.SoundSlider.value = soundVolume;
            pauseGroupMono.MusicSlider.value = musicVolume;
            pauseGroupMono.VibrationSlider.value = vibrationVolume;

            pauseGroupMono.MusicToggle.isOn = musicOn;
            pauseGroupMono.SoundToggle.isOn = soundOn;
            pauseGroupMono.VibrationToggle.isOn = !vibrateOn;

            pauseGroupMono.MusicToggle.transform.SetAlpha(!musicOn ? 1 : 0.5f);
            pauseGroupMono.SoundToggle.transform.SetAlpha(!soundOn ? 1 : 0.5f);
            pauseGroupMono.VibrationToggle.transform.SetAlpha(!vibrateOn ? 1 : 0.5f);

            pauseGroupMono.SoundToggle.onValueChanged.AddListener(value =>
            {
                if (value)
                {
                    AudioSystem.That.SetSoundVolume(GlobalEnum.MinPreSliderValue);
                }
                else
                {
                    AudioSystem.That.SetSoundVolume(soundVolume);
                }

                pauseGroupMono.SoundToggle.transform.SetAlpha(!value ? 1 : 0.5f);

                PersistSystem.That.SaveValue(GlobalEnum.MUTE_SOUND, value);
            });

            pauseGroupMono.MusicToggle.onValueChanged.AddListener(arg0 =>
            {
                if (arg0)
                {
                    AudioSystem.That.SetMusicVolume(GlobalEnum.MinPreSliderValue);
                }
                else
                {
                    AudioSystem.That.SetMusicVolume(musicVolume);
                }

                pauseGroupMono.MusicToggle.transform.SetAlpha(!arg0 ? 1 : 0.5f);

                PersistSystem.That.SaveValue(GlobalEnum.MUTE_MUSIC, arg0);
            });

            pauseGroupMono.VibrationToggle.onValueChanged.AddListener(value =>
            {
                Root.Instance.VibrationON = value;
                pauseGroupMono.VibrationToggle.transform.SetAlpha(!value ? 1 : 0.5f);
            });

            pauseGroupMono.SoundSlider.onValueChanged.AddListener(OnSoundSliderValueChanged);

            pauseGroupMono.MusicSlider.onValueChanged.AddListener(OnMusicSliderValueChanged);

            pauseGroupMono.VibrationSlider.onValueChanged.AddListener(value =>
            {
                YZGameUtil.OnVibrateSliderChanged(value, pauseGroupMono.VibrationToggle);
                vibrationVolume = value;
            });

            #endregion---------------------------- 组件事件注册 End---------------------

            if (IsFirstGuide)
            {
                GuideFirstGame();
            }

            //onstart 结束 mark
        }

        private void OnPropContainerClick(BingoPropContainer propContainer)
        {
            if (IsAllBingo())
            {
                return;
            }
            
            if (propContainer == null)
            {
                return;
            }

            var prop = propContainer.prop;
            if (prop != null)
            {
                //当前已经有正在使用的道具， 避免快速点击
                if (currentChooseProp != null)
                {
                    return;
                }

                PrepareUseProp(prop);
                AudioSystem.That.PlaySound(prop.Sound);

                propContainer.IsActive = false;

                // PropSlider.transform.Find("FullImage").SetActive(false);

                // PropEnergyText.SetActive(true);
            }
        }


        public override void InitEvents()
        {
            //断线重连已处理
            AddEventListener(Proto.GAME_END, (sender, eventArgs) =>
            {
                if (eventArgs is ProtoEventArgs { Result: ProtoResult.Fail })
                {
                    //服务器判断失败
                    Close();
                }
            });
        }

        #endregion 生命周期函数

        #region Unity生命周期函数

        // private void FixedUpdate()
        // {
        //     frames++;
        // }

        private void OnDestroy()
        {
            base.OnDestroy();

            AudioSystem.That.StopSound(SoundPack.Prop_Use_Count_Down);
            AudioSystem.That.StopSound(SoundPack.Double_Scoreing);
            AudioSystem.That.PlayMusic("audio/main_bg");

            _4x1_callArraySeq?.Kill();
            textMoveSeq?.Kill();
            bingoBtnSeq?.Kill();
            callArraySeq?.Kill();
            doubleSliderSeq?.Kill();
        }

        #endregion


        //grid的点击事件注册 mark
        private void RegisterGridClick()
        {
            for (int i = 0; i < GridsGroup.childCount; i++)
            {
                var cardButton = GetGridButton(i);
                var gridIndex = i;

                cardButton.SetClick(() =>
                {
                    //中间的不能选
                    if (gridIndex == 12)
                    {
                        return;
                    }

                    if (inNormalAnimation)
                    {
                        return;
                    }

                    if (finger1 != null)
                    {
                        finger1.SetActive(false);
                    }

                    var prop = vm[vname.VM_CurrentChooseProp.ToString()].ToIObservable<Prop>().Value;

                    if (prop is { isNeedChooseGrid : true })
                    {
                        GridUseProp(gridIndex, out var success);
                        //成功使用道具后， 不视作一次点击
                        if (success)
                        {
                            //成功才视为一次点击
                            Record(Operation.UseProp, ConvertIndex(gridIndex));
                            
                            if (IsAllClicked())
                            {
                                AutoFullHouse();
                            }
                        }
                    }
                    else
                    {
                        //先点击 ， 再加道具能量
                        Record(Operation.ClickGrid, ConvertIndex(gridIndex));

                        //这里记录加道具能量
                        ClickGrid(gridIndex);
                    }
                });
            }
        }

        private void AddPropEnergyValue(int gridIndex)
        {
            if (IsInitEnd && IsPropEffectOpen)
            {
                UIEffectUtils.Instance.SingleTrail(GetGridTransform(gridIndex), PropSlider.transform);

                this.AttachTimer(1f, () => TryAddPropEnergy());
            }
            else
            {
                TryAddPropEnergy();
            }
        }

        private void PopPausePanel()
        {
            if (lessPauseTime > 0)
            {
                vm[vname.VM_IsPause.ToString()].ToIObservable<bool>().Value = true;
            }
        }

        private void TryAddPropEnergy()
        {
            if (vm.Any())
            {
                if (vm[vname.VM_PropEnergy.ToString()].ToIObservable<int>().Value < PropSlider.maxValue &&
                    propList.Count < MAX_PROP_COUNT)
                {
                    vm[vname.VM_PropEnergy.ToString()].ToIObservable<int>().Value += ADD_ENERGY_COUNT;
                }
            }
            else
            {
                if (propEnergy < PropSlider.maxValue && propList.Count < MAX_PROP_COUNT)
                {
                    propEnergy += ADD_ENERGY_COUNT;
                }
            }
        }

        private void TryClearPropEnergy()
        {
            if (vm.Any())
            {
                vm[vname.VM_PropEnergy.ToString()].ToIObservable<int>().Value = 0;
            }
            else
            {
                propEnergy = 0;
            }
        }

        /// <summary>
        /// 引导点击BingoBtn
        /// </summary>
        async UniTask GuideClickBingoBtn()
        {
            if (IsSoftGuideOpen && callIndex == 0)
            {
                LoadFinger1(BingoBtn.transform.parent, new Vector3(0, 15, 0));

                await BingoBtn.OnClickAsync();
            }
        }

        #region -----------------------控制音效音量-----------------------

        private float soundVolume;
        private float musicVolume;
        private float vibrationVolume;

        private void OnSoundSliderValueChanged(float value)
        {
            PersistSystem.That.SaveValue(GlobalEnum.SOUND_VOLUME, value);

            soundVolume = value;
            if (value > GlobalEnum.MinPreSliderValue)
            {
                pauseGroupMono.SoundToggle.isOn = false;
            }


            AudioSystem.That.SetSoundVolume(value);
        }

        private void OnMusicSliderValueChanged(float value)
        {
            PersistSystem.That.SaveValue(GlobalEnum.MUSIC_VOLUME, value);

            musicVolume = value;
            if (value > GlobalEnum.MinPreSliderValue)
            {
                pauseGroupMono.MusicToggle.isOn = false;
            }

            AudioSystem.That.SetMusicVolume(value);
        }

        #endregion --------------------------控制音效音量

        IEnumerator BingoBtnBreath()
        {
            if (isBegin)
                yield break;

            var trans = BingoBtn.gameObject.transform;
            //在自身的大小上加上0.2倍
            Vector3 effectScale = trans.localScale + new Vector3(0.2f, 0.2f, 0.2f);
            //设置动画
            bingoBtnSeq = DOTween.Sequence();
            Tween t1 = trans.DOScale(effectScale, 1f);
            Tween t2 = trans.DOScale(Vector3.one, 1.2f);
            // float randomInterval = UnityEngine.Random.Range(0.5f, 1.3f);
            // Tween t3 = trans.DOScale(Vector3.one, randomInterval);

            bingoBtnSeq.Append(t1);
            bingoBtnSeq.Append(t2);
            // bingoBtnSeq.Append(t3);

            //设置动画loop属性
            bingoBtnSeq.SetLoops(-1, LoopType.Restart);
            if (!isBegin && !IsFirstGuide)
                bingoBtnSeq.Play();
        }

        /// <summary>
        /// 点击的棋盘位置是否匹配历史数据
        /// </summary>
        /// <returns></returns>
        bool GridIsMatch(int index, out bool isMatchFirst, out bool isMatch4X1)
        {
            var grid = bingoGrids[index];
            isMatchFirst = false;
            isMatch4X1 = false;

            float matchFirstGap = 2.3f;
            if (grid.Clicked)
            {
                return false;
            }

            int value = grid.Value;

            var _4x1_choose_value = GetChoosed4X1Value();

            if (_4x1_choose_value > 0)
            {
                isMatch4X1 = true;
                return value == _4x1_choose_value;
            }
            else
            {
                if (CallArrayValues[0] == value)
                {
                    YZLog.LogColor("点击第一个时间间隔 =" + (AddCallTime - lessTime));
                }
                
                if (CallArrayValues[0] == value && AddCallTime - lessTime <= matchFirstGap)
                {
                    isMatchFirst = true;
                    return true;
                }

                if (IsFakeGame() || IsCheat)
                {
                    return true;
                }

                return GridInCallingArray(index);
            }
        }

        int GetChoosed4X1Value()
        {
            if (!vm.Any())
            {
                return prop_4x1_choose_value;
            }

            return vm[vname.VM_Choose_4X1_Value.ToString()].ToIObservable<int>().Value;
        }

        /// <summary>
        /// 设置棋盘不可点击的状态，这里主要是音效 
        /// </summary>
        /// <param name="gridIndex"></param>
        /// <param name="open"></param>
        private void SetGridBtnClickState(int gridIndex, bool open)
        {
            var gridButton = GetGridButton(gridIndex);

            if (!open)
            {
                gridButton.transition = Selectable.Transition.None;

                gridButton.SoundPack = SoundPack.Button_Click_invalid;
            }
            else
            {
                gridButton.transition = Selectable.Transition.ColorTint;

                gridButton.SoundPack = SoundPack.Button_Click_valid;
            }
        }

        private MyButton GetGridButton(int index)
        {
            return GridsGroup.GetChild(index).GetComponent<MyButton>();
        }

        private Transform GetGridCom(int index)
        {
            return GridsGroup.GetChild(index);
        }

        void SetBingoGridClicked(int index, bool magicClick)
        {
            if (magicClick)
            {
                bingoGrids[index].MagicClicked = true;
            }
            else
            {
                bingoGrids[index].Clicked = true;
            }
        }

        void SetOneGridUIByGrid(BingoGrid grid)
        {
            var index = grid.Index;
            var textComponent = GetGridTextComponent(index);
            var bingoImageVisible = (grid.IsBingo || grid.MagicClicked);
            textComponent.SetActive(!bingoImageVisible);

            GetGridBingoImage(index).SetActive(bingoImageVisible);

            if (bingoImageVisible)
            {
                SetBingoImage(grid);
            }
            else
            {
                textComponent.color = GetGridTextColor(index);
                textComponent.text = grid.Value.ToString();
            }

            //设置背景
            if (grid.IsBingo)
            {
                GetGridNormalImage(index).sprite = MediatorBingo.Instance.GetSpriteByUrl($"uibingo/uibingo_bingo_grid");
            }
            else if (grid.Clicked)
            {
                GetGridNormalImage(index).sprite = MediatorBingo.Instance.GetSpriteByUrl("uibingo/uibingo_click_grid");
            }
            else
            {
                GetGridNormalImage(index).sprite = MediatorBingo.Instance.GetSpriteByUrl("uibingo/uibingo_not_click");
            }
        }
        
        void SetOneGridUI(int index)
        {
            var grid = bingoGrids[index];
            SetOneGridUIByGrid(grid);
        }

        void SetMagicClickImage(int index)
        {
            GetGridTextComponent(index).SetActive(false);

            var gridBingoImage = GetGridBingoImage(index);
            gridBingoImage.SetActive(true);
            gridBingoImage.sprite = MediatorBingo.Instance.GetSpriteByUrl("uibingo/bingo_force");
        }

        void SetGridBingo(int index)
        {
            SetOneGridUI(index);

            SetGridBtnClickState(index, false);
        }

        /// <summary>
        /// 非使用道具的情况下【4选1选中也会走这里】， 点击后 ， 设置该Card的状态
        /// </summary>
        private void ClickGrid(int index)
        {
            var grid = bingoGrids[index];
            if (grid.Clicked || grid.IsBingo)
            {
                return;
            }

            if (GridIsMatch(index, out var isMatchFirst, out var isMatch4X1))
            {
                //如果是4x1选中的， 需要移除4x1的状态
                if (isMatch4X1)
                {
                    YZLog.LogColor("四选一消除 ：" + grid.Value);
                    TotalCallArray.Remove(grid.Value);
                    _4x1_callArraySeq?.Kill(true);

                    is_prop_score = true;
                }

                SetGridClicked(index, isMatchFirst || isMatch4X1);

                //4选一选中的不能加道具点数
                if (!isMatch4X1)
                {
                    AddPropEnergyValue(index);
                }

                if (IsInitEnd)
                {
                    if (isMatchFirst || isMatch4X1)
                    {
                        if (ScoreMultiple > 1)
                        {
                            // CreatAmazing(index);
                        }
                        else
                        {
                            // CreatPrefect(index);
                        }
                    }
                    else
                    {
                        if (ScoreMultiple > 1)
                        {
                            // CreatPrefect(index);
                        }
                        else
                        {
                            // CreatGood(index);
                        }
                    }
                }
                
                if (IsAllClicked())
                {
                    AutoFullHouse();
                }
            }
            else
            {
                //点错棋子扣分
                bingoScore.PenaltyScore += ERROR_CLICK_GRID;

                TryClearPropEnergy();

                // CreatOops(index);
                RefreshScore(GetGridCom(index));

                if (IsInitEnd)
                {
                    AudioSystem.That.PlaySound(SoundPack.Not_Get_Score);
                }
            }
        }

        /// <summary>
        /// 4x1选择的不视为 isByProp了
        /// </summary>
        /// <param name="index"></param>
        /// <param name="isMatchFirstOr4x1"></param>
        /// <param name="isByProp"></param>
        private void SetGridClicked(int index, bool isMatchFirstOr4x1 = false, bool isByProp = false)
        {
            //是结构 struct 不能拷贝出来修改值
            if (bingoGrids[index].Clicked || bingoGrids[index].IsBingo)
            {
                return;
            }

            if (IsInitEnd)
            {
                AudioSystem.That.PlaySound(SoundPack.Get_Score);
            }

            SetBingoGridClicked(index, isByProp);

            ClickAnimation(index);

            int baseMatchBonus;
            int baseValue = isByProp ? PROP_MATCH_BONUS : BASE_MATCH_BONUS;
            if (isByProp)
            {
                //如果是道具消除的， 需要在之后的唱票队列中移除对应的值

                is_prop_score = true;
                var value = bingoGrids[index].Value;
                TotalCallArray.Remove(value);
            }

            if (isMatchFirstOr4x1 || isByProp)
            {
                baseMatchBonus = baseValue;
            }
            else
            {
                baseMatchBonus = (int)(baseValue * NEGATIVE_RADIO);
            }

            bingoScore.BaseScore += baseMatchBonus;
            if (ScoreMultiple > 1)
            {
                is_double_score = true;
            }
            bingoScore.WingBonus += (ScoreMultiple - 1) * baseMatchBonus;

            CheckBingoBtnState();
            RefreshScore(GetGridCom(index));

            // AddFullGridBonus();
        }

        void CreatAmazing(int index)
        {
            var fromTransform = GetGridCom(index);

            float x = 0;
            if (IsGridAtMostLeft(index))
            {
                x = 0.7f;
            }
            else if (IsGridAtMostRight(index))
            {
                x = -0.35f;
            }

            UIEffectUtils.Instance.CreatMoveImage(fromTransform, "uibingo/uibingo_amazing", 0.6f, 0.6f, x: x);
        }

        void CreatGood(int index)
        {
            var fromTransform = GetGridCom(index);
            UIEffectUtils.Instance.CreatMoveImage(fromTransform, "uibingo/uibingo_good");
        }

        void CreatPrefect(int index)
        {
            var fromTransform = GetGridCom(index);
            float x = 0;
            if (IsGridAtMostLeft(index))
            {
                x = 0.5f;
            }
            else if (IsGridAtMostRight(index))
            {
                x = -0.25f;
            }

            UIEffectUtils.Instance.CreatMoveImage(fromTransform, "uibingo/uibingo_perfect", 0.5f, 0.5f, x: x);
        }

        void CreatOops(int index)
        {
            if (IsInitEnd)
            {
                var fromTransform = GetGridCom(index);
                UIEffectUtils.Instance.CreatMoveImage(fromTransform, "uibingo/uibingo_oops");
            }
        }

        void CreatOops(Transform fromTransform)
        {
            if (IsInitEnd)
            {
                UIEffectUtils.Instance.CreatMoveImage(fromTransform, "uibingo/uibingo_oops", scale: 0.8f);
            }
        }

        private void SetGridListClicked(List<int> cross_indexes)
        {
            if (cross_indexes == null || !cross_indexes.Any())
            {
                return;
            }

            for (var i = 0; i < cross_indexes.Count; i++)
            {
                var index = cross_indexes[i];
                if (bingoGrids[index].IsBingo)
                {
                    continue;
                }

                if (bingoGrids[index].Clicked)
                {
                    //设置数字为星
                    if (i == 0)
                    {
                        SetBingoGridClicked(index, true);
                    }
                    continue;
                }

                SetBingoGridClicked(index, i == 0);

                if (ScoreMultiple > 1)
                {
                    is_double_score = true;
                }

                is_prop_score = true;
                bingoScore.BaseScore += BASE_MATCH_BONUS;
                bingoScore.WingBonus += (ScoreMultiple - 1) * BASE_MATCH_BONUS;

                var value = bingoGrids[index].Value;
                YZLog.LogColor("十字消消除 ：" + value);
                TotalCallArray.Remove(value);
            }

            AudioSystem.That.PlaySound(SoundPack.Get_Score);

            ClickAnimation(cross_indexes);

            CheckBingoBtnState();

            // AddFullGridBonus();

            RefreshScore(GetGridCom(cross_indexes[0]));
        }

        void CheckBingoBtnState()
        {
            var bingoMatchList = GetBingoMatch();
            if (vm.Any())
            {
                vm[vname.VM_GRIDS_BINGO_COUNT.ToString()].ToIObservable<int>().Value = bingoMatchList.Count;
            }
            else
            {
                grid_bingo_count = bingoMatchList.Count;
            }
        }

        public void GM_AddProp(int type)
        {
            var index = FindPropContainerIndex();
            if (index >= 0)
            {
                var newProp = new Prop(type);

                //绑定对象到Component
                var container = GetPropContainer(index);
                container.GetComponent<BingoPropContainer>().prop = newProp;

                propIndex++;
                propList.Insert(index, newProp);
            }
        }

        public void GM_SetNextRollResult(int[] value)
        {
            //改数据
        }

        private void AddCallIndex()
        {
            if (UseAllCallTime())
            {
                return;
            }

            vm[vname.VM_CallIndex.ToString()].ToIObservable<int>().Value++;
        }

        private void Interval()
        {
            if (!isBegin)
            {
                return;
            }

            var isPause = vm.Any() ? vm[vname.VM_IsPause.ToString()].ToIObservable<bool>().Value : this.isPause;

            //没有暂停
            if (!isPause)
            {
                if (!isBegin && !IsFirstGuide && PlayerGuideType is BingoGuideType.None)
                {
                    if (IsShowPopGroup)
                    {
                        lessPauseTime--;
                        LessPauseTimeText.text = TimeUtils.Instance.ToMinuteSecond(Convert.ToInt32(lessPauseTime));
                    }

                    if (TimeUtils.Instance.UtcTimeNow - LastPopPauseTime >= 10)
                    {
                        if (vm.Any())
                        {
                            vm[vname.VM_IsPause.ToString()].ToIObservable<bool>().Value = true;
                        }
                    }
                }
            }
            else
            {
                if (lessPauseTime > 0)
                {
                    lessPauseTime--;
                    LessPauseTimeText.text = TimeUtils.Instance.ToMinuteSecond(Convert.ToInt32(lessPauseTime));
                }
                else
                {
                    vm[vname.VM_IsPause.ToString()].ToIObservable<bool>().Value = false;
                    //暂停时间结束, 直接结束游戏
                    PlayGameEndAnimation(BingoCloseType.COUNTDOWN_END, true);
                }
            }
        }

        private void OnApplicationQuit()
        {
#if UNITY_EDITOR

#else
            SaveGameData();
#endif
        }

        private int saveDataTime;

        private void SaveGameData(bool isFinish = false)
        {
            saveDataTime = TimeUtils.Instance.UtcTimeNow;
            var data = GetStartData();
            if (data == null)
            {
                return;
            }

            if (ExtensionString.IsNullOrEmpty(data.GridSeed))
            {
                return;
            }

            if (data.Finish)
            {
                return;
            }

            data.Finish = isFinish;
            data.LessPauseTime = lessPauseTime;
            data.LessTime = (int)lessTime;
            data.OperateList = operateList;
            data.CacheTime = TimeUtils.Instance.UtcTimeNow;
            data.MultipleScoreCountDown = multipleScoreCountDown;
            data.PropUseCountDown = propUseCountDown;
            data.CallSliderFillAmount = CallSlider.fillAmount;

            PersistSystem.That.SaveValue(GlobalEnum.DB_YATZY, data, true);
        }

        private float lastAppPauseTime;

        private void OnApplicationPause(bool pause)
        {
            if (PlayerGuideType is BingoGuideType.None)
            {
                PauseGame(pause);
            }
        }

        // private void OnApplicationFocus(bool hasFocus)
        // {
        //     PasueGame(!hasFocus);
        // }

        private void PauseGame(bool pause)
        {
            if (pause)
            {
                SaveGameData();

                lastAppPauseTime = Time.realtimeSinceStartup;
            }
            else
            {
                if (lastAppPauseTime > 0)
                {
                    lessPauseTime -= Convert.ToInt32(Time.realtimeSinceStartup - lastAppPauseTime);
                }
                
                if (lessPauseTime < 0)
                {
                    //游戏倒计时扣除暂停超过的时间
                    lessTime += lessPauseTime;
                    if (lessTime < 0)
                    {
                        PlayGameEndAnimation(BingoCloseType.COUNTDOWN_END, true);
                    }

                    lessPauseTime = 0;
                }
                else
                {
                    PopPausePanel();
                }

                LessPauseTimeText.text = TimeUtils.Instance.ToMinuteSecond(Convert.ToInt32(lessPauseTime));
            }
        }

        private BingoGuideType PlayerGuideType;

        private BingoData startData;

        public BingoData GetStartData()
        {
            if (startData != null)
            {
                return startData;
            }

            var table = GetArgsByIndex<GameData>(0);

            var bingoData = table?["bingoData"] as BingoData;

            if (bingoData == null && PlayerGuideType is BingoGuideType.Teaching)
            {
                bingoData = new BingoData
                {
                    MatchCountAtLogin = 1,
                    CacheTime = TimeUtils.Instance.UtcTimeNow,
                    LessTime = GlobalEnum.ONE_GAME_TIME,
                    LessPauseTime = 5,
                    GridSeed = "AQ0CDAMYHRQcFyokKR8iMC44NjNKQT1HRA==",
                    CallSeed =
                        "LwQdIiFFKRIgD0orLAVHBzYzF0YYJj8xGRNCOxZLHDUuQSMQKkhAFD0CPBVEPhoJBggDAUMwJzQ5Gw0RHjgyCjcfSSUMJDoLDi0o",
                    PropsSeed = "AQECAgIDAQEBAwMBAQMBAgEBAQMBAgMB",
                    ChooseArray = new[]
                    {
                        "BidHSw==",
                        "AwYcOw==",
                        "GC46PA==",
                        "DEFCRw==",
                        "DSgvOw=="
                    }
                };
            }
            
#if DAI_TEST || true
            else if (table?["fakeData"] is true)
            {
                var cacheBingoData = PersistSystem.That.GetValue<BingoData>(GlobalEnum.DB_YATZY, true) as BingoData;
                string operationStr =
                    "[[237,0,0,0],[337,1,22,0],[337,2,7,300],[387,0,1,300],[127646,0,2,300],[127796,0,3,300],[127880,1,9,300],[127880,2,14,600],[127946,0,4,600],[128036,1,14,600],[128036,2,7,900],[128094,4,4,900],[128126,5,6,2400],[128126,0,5,2400],[128152,14,0,2400],[128203,1,10,2400],[128203,2,14,2700],[128276,0,6,2700],[128302,14,0,2700],[128423,0,7,2700],[128449,14,0,2700],[128573,0,8,2700],[128599,14,0,2700],[128720,0,9,2700],[128745,14,0,2700],[128870,0,10,2700],[128895,14,0,2700],[129016,0,11,2700],[129042,14,0,2700],[129099,1,18,2700],[129099,2,7,3000],[129166,0,12,3000],[129192,14,0,3000],[129261,7,0,3000],[129285,1,8,3000],[129285,2,14,3600],[129313,0,13,3600],[129332,4,3,3600],[129339,14,0,3600],[129353,5,4,4200],[129476,0,14,4200],[129502,14,0,4200],[129539,1,15,4200],[129539,2,7,4800],[129626,0,15,4800],[129652,14,0,4800],[129688,1,3,4800],[129688,2,14,5400],[129776,0,16,5400],[129802,14,0,5400],[129856,1,24,5400],[129856,2,7,6000],[129909,4,2,6000],[129977,9,70,6000],[130020,1,20,6000],[130020,10,0,6000],[130023,0,17,6600],[130049,14,0,6600],[130074,1,0,6600],[130074,2,14,7200],[130150,8,0,7200],[130173,0,18,7200],[130199,14,0,7200],[130247,7,0,7200],[130271,1,17,7200],[130271,2,7,7800],[130320,0,19,7800],[130345,14,0,7800],[130470,0,20,7800],[130495,14,0,7800],[130516,1,2,7800],[130516,2,14,8400],[130563,4,4,8400],[130611,5,18,10200],[130666,0,21,10200],[130692,14,0,10200],[130816,0,22,10200],[130842,14,0,10200],[130860,1,16,10200],[130860,2,7,10800],[130963,0,23,10800],[130989,14,0,10800],[131095,8,0,10800],[131113,0,24,10800],[131139,14,0,10800]]";
                                      // ",[131155,1,21,10800],[131155,2,14,11100],[131155,7,0,11100],[131395,11,17,53750],[131395,15,0,63550]]";
         
                if (cacheBingoData == null)
                {
                    bingoData = new BingoData
                    {
                        MatchCountAtLogin = 1,
                        CacheTime = TimeUtils.Instance.UtcTimeNow,
                        LessTime = 20,
                        // LessTime = GlobalEnum.ONE_GAME_TIME,
                        LessPauseTime = 6000,
                        GridSeed = "CQ0PBwUaFhETGS0kICkmOTc4MTRGSUhCRA==",
                        CallSeed =
                            "SBsSGSYtECsYPhwxExEBOQdECThGRyQPQTcaCwVJQiEySiw8BC8GLjs/Ix8nIjQlDhcIQDAzRToDKUseFD0dFjUMChVDAg0gNioo",
                        PropsSeed = "BAEDAgEEAQMBAw==",
                        ChooseArray = new[]
                        {
                            "IiUwNg==",
                        },
                        // OperateList = JsonConvert.DeserializeObject<List<int[]>>(operationStr),
                        CallSliderFillAmount = 1
                    };
                }
                else
                {
                    bingoData = cacheBingoData;
                }
            }
#endif
            startData = bingoData;
            return bingoData;
        }

        private void InitData()
        {
            var bingoData = GetStartData();

            InitSeed(bingoData);

            if (PlayerGuideType is BingoGuideType.None)
            {
                if (bingoData == null || ExtensionString.IsNullOrEmpty(bingoData.GridSeed))
                {
                    CarbonLogger.LogError("对局未生成 uid = " + Root.Instance.UserId);
                    Close();
                    return;
                }

                YZLog.LogColor(JsonConvert.SerializeObject(bingoData));

                matchCountAtLogin = bingoData.MatchCountAtLogin;

                /*TotalCallArray = new List<int>()
                {
                    1, 2, 3, 4, 5, 12, 16, 17, 18, 20, 33, 10,
                    20, 40, 65, 69, 68, 70, 74, 18, 19, 20,
                };


                bingo_grid_data = new[]
                {
                    1, 12, 33, 44, 65,
                    2, 16, 37, 48, 69,
                    3, 17,     46, 68, 
                    4, 18, 39, 50, 70, 
                    5, 20, 40, 51, 74
                };

                propIdArray = new byte[] { 1, 2, 3, 4 };*/

                // lessTime = Math.Max(bingoData.LessTime, 0);

                if (lessTime <= 10)
                {
                    TimeText.GetComponent<SizeUpDown>().enabled = true;
                    TimeText.color = Color.yellow;
                }

                lessPauseTime = bingoData.LessPauseTime;

                multipleScoreCountDown = bingoData.MultipleScoreCountDown;

                DoubleScoreText.text = multipleScoreCountDown.ToString();

                DoubleScoreSlider.value = multipleScoreCountDown;

                if (multipleScoreCountDown > 0)
                {
                    DoubleScoreGroup.SetActive(true);
                }

                propUseCountDown = bingoData.PropUseCountDown;

                CallSlider.fillAmount = bingoData.CallSliderFillAmount;

                PropCountDown();

                if (bingoData.OperateList != null)
                {
                    operateList = bingoData.OperateList;
                    RecurrenceOperation(bingoData.OperateList);
                    // if (bingoData.OperateList.Count > 0)
                    // {
                    //     frames = bingoData.OperateList[^1][0];
                    // }
                    ReShowPropIcon();
                }
                else
                {
                    operateList = new List<int[]>();
                }

                PlayShowAnimation();
            }
            else
            {
                lessTime = GlobalEnum.ONE_GAME_TIME;
                lessPauseTime = GlobalEnum.TOTAL_PAUSE_TIME;
                GuideTeaching().Forget();
            }

            LoadStyle();
        }

        private void InitSeed(BingoData bingoData)
        {
            TotalCallArray = Convert.FromBase64String(bingoData.CallSeed).ToList();

            bingo_grid_data = Convert.FromBase64String(bingoData.GridSeed);

            InitBingoGrids();

            propIdArray = Convert.FromBase64String(bingoData.PropsSeed);
            //解引用
            chooseArray = bingoData.ChooseArray.ToArray();
        }

        private void PlayShowAnimation()
        {
            game_bg_dissolve.SetActive(true);
            ForbidSelfClick();
            var uiTransion = game_bg_dissolve.GetComponent<UITransitionEffect>();
            uiTransion.effectFactor = 1;
            DOTween.To(() => uiTransion.effectFactor, x => uiTransion.effectFactor = x, 0f, 0.8f)
                .SetEase(Ease.OutQuint)
                .SetDelay(0.3f).OnComplete(
                    () => { game_bg_dissolve.SetActive(false); });

            //衔接快一些
            this.AttachTimer(0.6f, () =>
            {
                if (!IsFirstGuide)
                {
                    beginEffect.SetActive(true);
                    beginEffect.Play(() => { StartGame(); });
                }
            });

            /*if (!IsFirstGuide)
            {
                GetComponent<GraphicRaycaster>().enabled = false;
                beginEffect.SetActive(true);
                beginEffect.Play(() => { StartGame(); });
            }*/
        }

        async UniTask StartGame()
        {
            beginEffect.SetActive(false);

            var token = this.GetCancellationTokenOnDestroy();
            await UniTask.WaitUntil(() => vm.Any() && !vm[vname.VM_IsPause.ToString()].ToIObservable<bool>().Value,
                cancellationToken: token);

            if (CallArrayParent.childCount == 0)
            {
                AddCallIndex();
            }

            SetDoubleAnimation();

            Interval_50();

            GetComponent<GraphicRaycaster>().enabled = true;
        }

        private const int baseAdd = 1;

        async UniTask GuideFirstGame()
        {
            //隐藏暂停按钮
            PauseBtn.SetActive(false);

            //避免和引导遮罩重叠
            beginEffect.SetActive(false);

            await GuideProcess();

            if (IsFakeGame())
            {
                Root.Instance.Role.match_first_game_guide = (int)NewPlayerGuideStep.FIRST_ROOM_GUIDE_FINISH;
            }
            else
            {
                MediatorRequest.Instance.SendNewPlayerGuideStep(NewPlayerGuideStep.FIRST_ROOM_GUIDE_FINISH);
            }

            //游戏开始倒计时
            LastPopPauseTime = Int32.MaxValue;

            //为什么要等？
            // await UniTask.Delay(TimeSpan.FromMilliseconds(500),
            //     cancellationToken: this.GetCancellationTokenOnDestroy());
            
            PauseBtn.SetActive(true);
            
            AddCallIndex();

            StartGame();
        }

        private async Task GuideClickBingoBtn(CancellationToken token)
        {
            var guideMono = guideMask.GetComponent<BingoGuideMono>();
            var bingoBtnHighLight = BingoBtn.gameObject.TryAddComponent<AddHeightCanvas>();
            LoadFinger1(BingoBtn.transform);

            guideMono.RollAgain.SetActive(true);

            //再点击
            bingoBtnHighLight.enabled = true;
            await BingoBtn.OnClickAsync(token);
            bingoBtnHighLight.enabled = false;

            guideMono.RollAgain.SetActive(false);
            finger1.SetActive(false);

            //等待bingo动画
            await GuideWaitTime(token, 1200);
        }

        private async Task GuideWaitTime(CancellationToken token, int millseconds)
        {
            //显示1s骰子摇动动画
            guideMask.SetActive(false);
            await UniTask.Delay(TimeSpan.FromMilliseconds(millseconds), cancellationToken: token);
            guideMask.SetActive(true);
        }

        private async Task WaitAnimationEnd(CancellationToken token)
        {
            guideMask.SetActive(false);

            await UniTask.WaitUntil(() => !inNormalAnimation, cancellationToken: token);

            guideMask.SetActive(true);
        }

        /// <summary>
        /// how to play页 ， 重复金币局新手引导内容
        /// </summary>
        async UniTask GuideTeaching()
        {
            guideMask.GetComponent<BingoGuideMono>().HideChildren();
            guideMask.SetActive(true);

            skipBtn.SetActive(true);

            //避免和引导遮罩重叠
            beginEffect.SetActive(false);

            await GuideProcess();

            /*//等待点击动效
            await UniTask.Delay(TimeSpan.FromMilliseconds(800),
                cancellationToken: this.GetCancellationTokenOnDestroy());

            skipBtn.SetActive(false);
            
            //完成
            UserInterfaceSystem.That.ShowUI<UIConfirm>(new UIConfirmData
            {
                Type = UIConfirmData.UIConfirmType.OneBtn,
                confirmTitle = I18N.Get("key_ok_got_it"),
                desc = I18N.Get("key_guide_finish_tuition"),
                title = I18N.Get("key_well_done").ToUpper(),
                cancleCall = GuideEnd,
                HideCloseBtn = true,
                confirmCall = GuideEnd
            });*/

            Close();
        }

        async UniTask GuideProcess()
        {
            InitForFirstGuide();

            //引导点击6号位， 实现一个左上到右下的bingo
            var token = this.GetCancellationTokenOnDestroy();
            var guideMono = guideMask.GetComponent<BingoGuideMono>();

            await UniTask.WaitUntil(vm.Any, cancellationToken: token);

            Record(Operation.FirstGameGuide);

            //禁止除高亮外的点击
            GetComponent<GraphicRaycaster>().enabled = false;

            //等一下开场动画
            await UniTask.Delay(TimeSpan.FromMilliseconds(500), cancellationToken: token);

            AddCallIndex();

            //等一下下落动画
            await UniTask.Delay(TimeSpan.FromMilliseconds(500), cancellationToken: token);

            //里面把guideMask 置为ture了
            guideMono.Init();

            //第一个文本
            guideMono.FirstTip();
            //引导点击
            await GuideClickGrid(first_guide_grid_index, token);
            guideMono.HideChildren();

            await GuideClickBingoBtn(token);

            //弹出界面 ， 显示各种bingo类型

            UserInterfaceSystem.That.ShowUI<UIBingoRule>();
            
            await UniTask.WaitUntil(() =>
            {
                var ui = UserInterfaceSystem.That.Get<UIBingoRule>();
                return ui == null;
            }, cancellationToken: token);

            guideMono.SetActive(false);
            AddCallIndex();

            //等一下下落动画
            await UniTask.Delay(TimeSpan.FromMilliseconds(500), cancellationToken: token);
            //遮罩打开
            guideMono.SetActive(true);
            
            await GuideClickGrid(second_guide_grid_index, token);
            
            if (propList.Count > 0)
            {
                guideMono.HideChildren();

                //弹出十字消文字说明弹窗
                UserInterfaceSystem.That.ShowUI<UIShowProp>(propList[0]);

                await UniTask.WaitUntil(() =>
                {
                    var ui = UserInterfaceSystem.That.Get<UIShowProp>();
                    return ui == null;
                }, cancellationToken: token);


                guideMono.SecondTip();
                //引导点击道具
                await GuideClickProp(0, token);

                guideMono.HideChildren();
                //关闭说明弹窗后， 点击固定位置

                await GuideClickGrid(three_guide_grid_index, token);
            }

            await UniTask.Delay(TimeSpan.FromMilliseconds(500), cancellationToken: token);
            
            UserInterfaceSystem.That.ShowUI<UIBingoPropRule>();
            
            await UniTask.WaitUntil(() =>
            {
                var ui = UserInterfaceSystem.That.Get<UIBingoPropRule>();
                return ui == null;
            }, cancellationToken: token);
            
        
            guideMono.gameObject.FindChild("root/finnal").SetActive(true);
                
            await guideMono.PlayBtn.OnClickAsync(token);
            
            //引导结束
            guideMono.SetActive(false);

            //恢复点击
            GetComponent<GraphicRaycaster>().enabled = true;
        }

        private void InitForFirstGuide()
        {
            for (int i = 0; i < 5; i++)
            {
                if (i == 1 || i == 2)
                {
                    continue;
                }

                TotalCallArray?.Remove(bingoGrids[i + i * 5].Value);
                bingoGrids[i + i * 5].Clicked = true;
            }

            propEnergy = 0;

            propIdArray[0] = Const.Cross;

            guideCount = 2;
        }

        private async Task GuideClickGrid(int gridIndex, CancellationToken token)
        {
            var gridButton = GetGridButton(gridIndex);
            var cardHighCanvas = gridButton.gameObject.AddComponent<AddHeightCanvas>();
            LoadFinger1(gridButton.transform);
            await gridButton.OnClickAsync(token);

            finger1.SetActive(false);
            cardHighCanvas.enabled = false;
        }

        private async Task GuideClickProp(int propIndex, CancellationToken token)
        {
            var propContainer = GetPropContainer(propIndex);
            LoadFinger1(propContainer.transform);

            var propCanvas = propContainer.gameObject.AddComponent<AddHeightCanvas>();

            await propContainer.GetComponent<Button>().OnClickAsync(token);

            propCanvas.enabled = false;
        }

        private void AddHeightCanvasToBrand(int _baseAdd, int cardIndex)
        {
            var brand = GetGridTransform(cardIndex);
            brand.gameObject.AddComponent<AddHeightCanvas>().SetAddSortingOrder(_baseAdd);
        }

        private void RemoveHeightCanvasToBrand(int cardIndex)
        {
            var brand = GetGridTransform(cardIndex);
            brand.gameObject.TryGetComponent<AddHeightCanvas>(out var heightCanvas);
            if (heightCanvas != null)
            {
                heightCanvas.enabled = false;
            }
        }

        private void ShowFinger(int index)
        {
            finger.SetActive(true);
            finger.TryGetComponent<AddHeightCanvas>(out var canvas);

            if (canvas == null)
            {
                finger.AddComponent<AddHeightCanvas>().SetAddSortingOrder(baseAdd + 1);
            }

            StartCoroutine(FingerAnimation(index));
        }

        IEnumerator FingerAnimation(int index)
        {
            int curIndex = 0;

            int GetNextFingerIndex()
            {
                return (curIndex + 1) % index;
            }

            while (!CheckGuide1Step(index))
            {
                curIndex = GetNextFingerIndex();
                finger.transform.localScale = Vector3.one;
                finger.transform.localPosition = new Vector3(28, -37, 0);
                yield return new WaitForSeconds(0.5f);
            }

            finger.SetActive(false);
        }

        private void GuideEnd()
        {
            Close();
        }

        private bool CheckGuide1Step(int index)
        {
            return true;
        }


        private Transform GetGridTransform(int i)
        {
            return GridsGroup.GetChild(i);
        }

        /// <summary>
        /// 根据道具列表, 复现道具
        /// </summary>
        private void ReShowPropIcon()
        {
            for (int i = 0; i < 3; i++)
            {
                var propContainer = GetPropContainer(i);
                propContainer.IsActive = i < propList.Count;
                if (i < propList.Count)
                {
                    propContainer.prop = propList[i];
                }
            }
        }

        /// <summary>
        /// 获取道具 Component
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        private BingoPropContainer GetPropContainer(int i)
        {
            return PropTransform.GetChild(i).GetComponent<BingoPropContainer>();
        }

        /// <summary>
        ///    操作重现为游戏数据 mark
        /// </summary>
        /// <param name="bingoDataOperateList"></param>
        private void RecurrenceOperation(List<int[]> bingoDataOperateList)
        {
            try
            {
                foreach (var array in bingoDataOperateList)
                {
                    // time, type, args, VM_Score
                    Operation operation = (Operation)array[1];
                    int arg = array[2];
                    float _lessTime = array[0] / 100f;
                    lessTime = _lessTime;
                    switch (operation)
                    {
                        case Operation.FirstGameGuide:
                            InitForFirstGuide();
                            break;
                        case Operation.PropEnergyChange:
                            TryAddProp();
                            break;
                        case Operation.ClickBingo:
                            ClickBingo();
                            break;
                        case Operation.AddCallIndex:
                            callIndex++;
                            AddCallTime = lessTime;
                            SetNewCallValue();
                            break;
                        case Operation.ClickGrid:
                            int index = ConvertIndex(arg);
                            ClickGrid(index);
                            break;
                        case Operation.BeginProp:
                            var prop = propList.Find(prop1 => prop1.id == arg);
                            if (prop == null)
                            {
                                YZLog.LogColor("BeginProp 重建流程失败", "red");
                            }

                            currentChooseProp = prop;
                            switch (currentChooseProp.id)
                            {
                                case Const.ChooseOne:
                                    SetChooseOneArray(out var success);
                                    break;
                                //因该不会走这里了，双倍道具没有算入 currentProp了
                                case Const.DoubleScore:
                                    scoreMultiple = prop.value;
                                    break;
                            }

                            propList.Remove(prop);

                            break;
                        case Operation.UseProp:
                            if (currentChooseProp == null)
                            {
                                YZLog.LogColor("UseProp 重建流程失败", "red");
                            }

                            switch (currentChooseProp.id)
                            {
                                case Const.Cross:
                                case Const.ChooseAny:
                                    GridUseProp(ConvertIndex(arg), out var _);
                                    break;
                            }

                            currentChooseProp = null;
                            break;

                        case Operation.CancelProp:
                            switch (currentChooseProp?.id)
                            {
                                case Const.ChooseOne:
                                    ResetChooseOneArray();
                                    break;
                            }

                            currentChooseProp = null;
                            break;

                        case Operation.StartDouble:
                            var doubleScoreProp = propList.Find(prop1 => prop1.id == Const.DoubleScore);
                            propList.Remove(doubleScoreProp);
                            scoreMultiple = 2;
                            break;

                        case Operation.EndDouble:
                            scoreMultiple = 1;
                            break;
                        case Operation.End4X1Choose:
                            End4X1Choose();
                            break;

                        case Operation.Start4X1Choose:
                            ResetChooseOneArray();
                            prop_4x1_choose_value = arg;
                            currentChooseProp = null;
                            break;
                        case Operation.RemoveLast:
                            leftCallValue = 0;
                            break;
                    }
                }

                //如果玩家没有操作 ，则不暂停
                if (lessPauseTime > 0 && bingoDataOperateList is { Count: > 0 })
                {
                    // 有3 2 1 go 不用暂停
                    // isPause = true;
                }
            }
            catch (Exception e)
            {
                CarbonLogger.LogError("还原操作失败 " + e + "\n" + e.StackTrace);

                if (!string.IsNullOrEmpty(matchId))
                {
                    int gameScore = 0;

                    if (operateList != null && operateList.Any())
                    {
                        gameScore = operateList[^1][3];
                    }

                    MediatorRequest.Instance.GameEnd(matchId,
                        gameScore,
                        BingoCloseType.EARLY_END,
                        (int)lessTime,
                        operateList,
                        silence: true);
                }

                Close();
            }
        }

        private void ResetChooseOneArray()
        {
            for (int i = 0; i < chooseOneArray.Length; i++)
            {
                chooseOneArray[i] = 0;
            }
        }

        private void Record(Operation operation, int args = 0)
        {
            if (operateList == null)
            {
                return;
            }

            if (!IsInitEnd)
            {
                return;
            }

            if (PlayerGuideType is BingoGuideType.Teaching)
            {
                return;
            }

            switch (operation)
            {
            }

            //待定
            operateList.Add(new[]
            {
                // frames,
                Convert.ToInt32(lessTime * 100),
                operation.ToInt32(),
                args,
                bingoScore.TotalScore,
            });

            //模拟器上难以监测OnApplicationQuite
            SaveGameData();
        }


        /// <summary>
        /// 选择一个道具
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        private void PrepareUseProp(Prop prop)
        {
            //双倍道具用了就生效
            if (prop is { id : Const.DoubleScore })
            {
                DoubleScoreSlider.value = DoubleScoreSlider.maxValue = multipleScoreCountDown = prop.spanTime;
                //重置文本， 不要等到下一秒
                DoubleScoreText.text = multipleScoreCountDown.ToString();

                multipleScoreCountDown = prop.spanTime;

                ScoreMultiple = prop.value;

                PlayDoubleTextEffect();

                Record(Operation.StartDouble);
            }
            else
            {
                vm[vname.VM_CurrentChooseProp.ToString()].ToIObservable<Prop>().Value = prop;
            }

            propList?.Remove(prop);
        }

        private void SetDoubleAnimation()
        {
            if (multipleScoreCountDown <= 0)
            {
                doubleSliderSeq?.Kill(true);
                return;
            }

            doubleSliderSeq?.Kill();
            //施法动作结束
            // var sliderValueTween = ChangeSliderValue(DoubleScoreSlider, 0, multipleScoreCountDown,
            //     () => { DoubleSliderGroup.SetActive(false); });
            //
            // doubleSliderSeq = DOTween.Sequence().Append(sliderValueTween);

            var start = DOTween.Sequence().AppendInterval(1).AppendCallback(() =>
            {
                multipleScoreCountDown--;
                DoubleScoreText.text = multipleScoreCountDown.ToString();
                SaveGameData();
            }).SetLoops((int)multipleScoreCountDown);
            //为了让玩家 玩的爽， 在0s动画的时候也能达成双倍
            var append = DOTween.Sequence().AppendInterval(1);
            //必须要新建一个sequence 不然会继承setloop
            doubleSliderSeq = DOTween.Sequence().Append(start)
                    .Append(append)
                .OnComplete(() =>
                {
                    //游戏结束后， 双倍时间保留
                    if (!isSendGameEnd)
                    {
                        //双倍时间结束
                        Record(Operation.EndDouble);
                    }
                    
                    DoublePartical.SetActive(false);
                    ScoreMultiple = 1;
                })
                .OnStart(() =>
                {
                    DoublePartical.SetActive(true);
                })
                ;

            AudioSystem.That.PlaySound(SoundPack.Double_Scoreing, multipleScoreCountDown);
        }

        /// <summary>
        /// 是否使用了直选
        /// </summary>
        /// <returns></returns>
        bool IsUseChooseAny()
        {
            var prop = GetCurrentProp();
            return prop is { id : Const.ChooseAny };
        }

        /// <summary>
        /// 是否使用了十字消
        /// </summary>
        /// <returns></returns>
        bool IsUseCross()
        {
            var prop = GetCurrentProp();
            return prop is { id : Const.Cross };
        }

        Prop GetCurrentProp()
        {
            //mark 也许可以都返回currentChooseProp 
            if (!vm.Any())
            {
                return currentChooseProp;
            }

            return vm[vname.VM_CurrentChooseProp.ToString()].ToIObservable<Prop>().Value;
        }

        //道具生效
        private void GridUseProp(int gridIndex, out bool success)
        {
            var prop = GetVmValue(vname.VM_CurrentChooseProp.ToString(), currentChooseProp, out _);
            success = false;
            if (prop == null)
            {
                return;
            }

            var gridData = bingoGrids[gridIndex];
            switch (prop.id)
            {
                case Const.ChooseAny:
                    if (gridData.Clicked || gridData.IsBingo)
                    {
                        return;
                    }
                    success = true;
                    SetGridClicked(gridIndex, isByProp: true);
                   
                    break;

                case Const.Cross:
                    //最中间那个不能选
                    if (gridIndex == 12)
                    {
                        return;
                    }

                    //选择围绕中心的5个点
                    List<int> cross_indexes = new();
                    for (int i = 0; i < 5; i++)
                    {
                        int pick_index = -1;

                        if (i == 0)
                        {
                            //中心点
                            pick_index = gridIndex;
                            //必然加入 因为要给他变成星
                            cross_indexes.Add(pick_index);
                            continue;
                        }

                        if (i == 1)
                        {
                            pick_index = gridIndex - 5;
                        }

                        //右 不能在最右边
                        if (i == 2 && !IsGridAtMostRight(gridIndex))
                        {
                            pick_index = gridIndex + 1;
                        }

                        //下
                        if (i == 3)
                        {
                            pick_index = gridIndex + 5;
                        }

                        //左 不能在最左边
                        if (i == 4 && !IsGridAtMostLeft(gridIndex))
                        {
                            pick_index = gridIndex - 1;
                        }

                        if (IsGridIndex(pick_index))
                        {
                            var grid = bingoGrids[pick_index];
                            if (grid.Clicked || grid.IsBingo)
                            {
                                continue;
                            }

                            cross_indexes.Add(pick_index);
                        }
                    }

                    //十字消 没有可得分的对象
                    for (var i = 0; i < cross_indexes.Count; i++)
                    {
                        var index = cross_indexes[i];
                        if (bingoGrids[index].Clicked || bingoGrids[index].IsBingo)
                        {
                            continue;
                        }

                        success = true;
                    }

                    if (success)
                    {
                        SetGridListClicked(cross_indexes);
                    }
                    else
                    {
                        if (IsInitEnd)
                        {
                            UserInterfaceSystem.That.ShowUI<UITip>("There are no scoring objects around!");
                        }
                    }
                    break;
            }

            if (success)
            {
                if (vm.Any())
                {
                    vm[vname.VM_CurrentChooseProp.ToString()].ToIObservable<Prop>().Value = null;
                }
                else
                {
                    currentChooseProp = null;
                }
            }
        }

        bool IsGridIndex(int index)
        {
            return index >= 0 && index < bingoGrids.Length;
        }

        /// <summary>
        /// 错误后清空道具点数
        /// </summary>
        void ClearPropEnergy()
        {
            if (vm.Any())
            {
                vm[vname.VM_PropEnergy.ToString()].ToIObservable<int>().Value = 0;
            }
            else
            {
                propEnergy = 0;
            }
        }

        /// <summary>
        /// 棋子是否在最右边那一列
        /// </summary>
        /// <returns></returns>
        bool IsGridAtMostRight(int index)
        {
            // 4, 9, 14, 19, 24

            // 1 % 5 = 1
            return (index + 1) % 5 == 0;
        }

        /// <summary>
        /// 棋子是否在最左边那一列
        /// </summary>
        /// <returns></returns>
        bool IsGridAtMostLeft(int index)
        {
            // 0, 5, 10, 15,  20

            return index % 5 == 0;
        }

        /// <summary>
        /// 设置4选一数组的值
        /// </summary>
        private void SetChooseOneArray(out bool success)
        {
            var array = Get4X1ChooseData();

            success = false;
            if (array == null || array.Length == 0)
            {
                return;
            }

            success = true;
            for (int i = 0; i < chooseOneArray.Length; i++)
            {
                if (i < array.Length)
                {
                    chooseOneArray[i] = array[i];
                }
                else
                {
                    chooseOneArray[i] = 0;
                }
            }

            if (vm.Any())
            {
                //引用类型必须这样刷新
                vm[vname.VM_ChooseOneArray.ToString()].Refresh();
            }
        }

        /// <summary>
        ///  获取4X1提供选择的数据
        /// </summary>
        /// <returns></returns>
        private byte[] Get4X1ChooseData()
        {
            // var array = Convert.FromBase64String(chooseArray[useChooseOneCount % chooseArray.Length]);
            // useChooseOneCount++;

            //棋盘上剩余没点击的

            var not_click_list = GetGridNotClickList(out var notCallList);
            // not_click_list = new List<byte>() { 70, 73, 72, 5 };
            // notCallList = new List<byte>() { 70, 73, 5 };
            if (not_click_list.Count == 0)
            {
                return null;
            }
            YZLog.LogColor("not_click_list =" +  string.Join(", ", not_click_list));
            YZLog.LogColor("notCallList =" +  string.Join(", ", notCallList));
            //区分棋盘上唱票过的， 和棋盘上没有唱票过的， 优先选择棋盘上没有唱票过的
            var result = new byte[Math.Min(4, not_click_list.Count)];

            if (notCallList is { Count: > 0 })
            {
                notCallList.Shuffle();
                for (int i = 0; i < Math.Min(result.Length, notCallList.Count); i++)
                {
                    result[i] = notCallList[i];
                }
            }

            //超过4个了
            if (notCallList != null && notCallList.Count >= result.Length)
            {
                return result;
            }

            not_click_list.Shuffle();
            //从剩余的棋盘上的值随机出一个
            for (int i = 0; i < result.Length; i++)
            {
                //已经有值了
                if (result[i] > 0)
                {
                    continue;
                }

                var finded = not_click_list.Find(b => !result.Contains(b));
                if (finded > 0)
                {
                    result[i] = finded;
                }
            }

            return result;
        }

        /// <summary>
        /// 获取棋盘上还没有点击的值的队列
        /// </summary>
        /// <returns></returns>
        List<byte> GetGridNotClickList(out List<byte> notCallList)
        {
            var result = new List<byte>();
            notCallList = new List<byte>();
            for (int i = 0; i < bingoGrids.Length; i++)
            {
                var grid = bingoGrids[i];
                if (grid.Clicked || grid.IsBingo)
                {
                    continue;
                }

                result.Add(grid.Value);

                if (!GridInCallingArray(i))
                {
                    notCallList.Add(grid.Value);
                }
            }

            return result;
        }

        void InitChooseGroupCom()
        {
            if (chooseOneArray[0] <= 0)
            {
                return;
            }

            //防御，已经在选择道具的时候选了
            _4X1Transform.SetActive(true);

            for (int i = 0; i < chooseOneArray.Length; i++)
            {
                int value = chooseOneArray[i];
                //bingo棋盘的值都是>0的
                var diceTrans = _4X1Transform.Find("content").GetChild(i);
                if (value > 0)
                {
                    var bingoType = GetBingoType(value);

                    diceTrans.gameObject.FindChild<Image>("root/Image").sprite =
                        MediatorBingo.Instance.GetBingoBallImageSelect(bingoType);

                    diceTrans.gameObject.FindChild<Text>("root/Text").text = value.ToString();

                    diceTrans.GetComponent<MyButton>().SetClick(() =>
                    {
                        Record(Operation.Start4X1Choose, value);

                        vm[vname.VM_CurrentChooseProp.ToString()].ToIObservable<Prop>().Value = null;

                        //等待关闭动画
                        this.AttachTimer(0.3f,
                            () =>
                            {
                                if (vm.Any())
                                {
                                    vm[vname.VM_Choose_4X1_Value.ToString()].ToIObservable<int>().Value = value;
                                }
                            });
                    });
                }

                diceTrans.SetActive(value > 0);
            }
        }

        void BingoAnimation(IList<int> array, float dalaytime, float timeScale = 1, bool isDelay = true)
        {
            if (!IsInitEnd)
            {
                return;
            }
            
            //动画可以被暂停吗， 好像没必要
            for (int i = 0; i < array.Count; i++)
            {
                var gridIndex = array[i];
                var com = GetGridCom(gridIndex);

                var scale_tween = com.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);

                if (isDelay)
                {
                    DOTween.Sequence().AppendCallback(() =>
                        {
                            bingoGrids[gridIndex].AnimationBingoCount++;
                            //修改图片
                            SetGridBingo(gridIndex);

                            com.localScale = Vector3.zero;
                        }).Append(scale_tween) .SetDelay(dalaytime + 0.8f / timeScale / array.Count * i) ;
                }
                else
                {
                    DOTween.Sequence().AppendCallback(() =>
                    {
                        bingoGrids[gridIndex].AnimationBingoCount++;
                        //修改图片
                        SetGridBingo(gridIndex);

                        com.localScale = Vector3.zero;
                    }).Append(scale_tween);
                }
            }
        }

        void ClickAnimation(IList<int> array)
        {
            if (!IsInitEnd)
            {
                return;
            }

            //动画可以被暂停吗， 好像没必要
            for (int i = 0; i < array.Count; i++)
            {
                var gridIndex = array[i];
                ClickAnimation(gridIndex, 0.1f * i);
            }
        }

        private void ClickAnimation(int gridIndex, float delay = 0f)
        {
            if (!IsInitEnd)
            {
                return;
            }

            var com = GetGridCom(gridIndex);
            var grid = bingoGrids[gridIndex];
            var scale_tween = com.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            DOTween.Sequence().AppendCallback(() =>
            {
                //修改图片
                SetOneGridUIByGrid(grid);

                //容错， 前面已经设置过了
                SetGridBtnClickState(gridIndex, false);

                com.localScale = Vector3.zero;
            }).Append(scale_tween).SetDelay(delay);
        }

        /// <summary>
        /// 是否所有格子都bingo了
        /// </summary>
        /// <returns></returns>
        bool IsAllBingo()
        {
            for (int i = 0; i < bingoGrids.Length; i++)
            {
                if (!bingoGrids[i].IsBingo)
                {
                    return false;
                }
            }

            return true;
        }

        bool IsAllClicked()
        {
            for (int i = 0; i < bingoGrids.Length; i++)
            {
                if (!bingoGrids[i].Clicked)
                {
                    return false;
                }
            }

            return true;
        }

        private void OnBingoBtnClick()
        {
            //和引导冲突
            // if (!isBegin)
            // {
            //     return;
            // }

            ClickBingo();
            Record(Operation.ClickBingo);
        }

        private void ClickBingo()
        {
            if (IsAllBingo())
            {
                return;
            }
            var match_array_list = GetBingoMatch();
            var bingo_count = match_array_list.Count;
            if (bingo_count > 0)
            {
                //加分， 播放bingo动画
                SetGridsByBingoList(match_array_list, out var dalaytime);

                if (IsAllBingo())
                {
                    this.AttachTimer(dalaytime + 1f, () => { PlayGameEndAnimation(BingoCloseType.NORMAL_END); });
                }
            }
            else
            {
                //扣分
                bingoScore.PenaltyScore += ERROR_CLICK_BINGO;
                // CreatOops(BingoBtn.transform);
                RefreshScore(BingoBtn.transform);

                TryClearPropEnergy();
            }
        }

        private void SetGridsByBingoList(List<int[]> match_array_list, out float dalaytime, bool isTimeOut = false)
        {
            SetGetBingoScore(match_array_list);

            dalaytime = 0;
            AddBingoCount(match_array_list);

            bool isDelay = isTimeOut || IsAllBingo();
            
            for (var i = 0; i < match_array_list.Count; i++)
            {
                var array = match_array_list[i];
                var timeScale = 1 +  i / 2.2f;
                BingoAnimation(array,  dalaytime, timeScale, isDelay);
                if (isDelay)
                {
                    dalaytime += 0.8f / timeScale;
                }
            }
            
            BingoCountText.text = match_array_list.Count.ToString();

            SetVmValue(vname.VM_GRIDS_BINGO_COUNT.ToString(), ref grid_bingo_count, 0, out var _);

            if (IsAllBingo())
            {
                //自动使用双倍道具
                var doubleContainer = FindPropContainerByType(Const.DoubleScore);
                if (doubleContainer != null)
                {
                    OnPropContainerClick(doubleContainer);
                }

                //fullhouse + 1 bingo
                bingoScore.BingoCount += 1;
                
                AddFullHouseScore();
                
                doubleSliderSeq?.Pause();
                
                this.AttachTimer(Math.Max(0.6f, dalaytime - 0.5f), () =>
                {
                    PlayFullHouseEffect();
                });
            }
            else
            {
                PlayBingoTextEffect(match_array_list.Count);
            }
        }

        private void AddBingoCount(List<int[]> match_array_list)
        {
            for (var i = 0; i < match_array_list.Count; i++)
            {
                var array = match_array_list[i];

                //先改变数值， 再播放动画
                foreach (var index in array)
                {
                    bingoGrids[index].BingoCount++;
                }
            }
        }

        private void PlayFullHouseEffect()
        {
            if (IsAllBingo())
            {
                if (IsInitEnd)
                {
                    AudioSystem.That.PlaySound(SoundPack.Bingo_FullHouse);
                    FullHouseEffect.SetActive(true);
                    RefreshScore(new Vector3(0, -200, 0), 2f);
                }
            }
        }

        private void AddFullHouseScore()
        {
            if (bingoScore.FullGridBonus > 0)
            {
                return;
            }
            //受双倍时间影响
            bingoScore.FullGridBonus = FULL_CLICKED_BONUS;
            bingoScore.WingBonus += (ScoreMultiple - 1) * FULL_CLICKED_BONUS;

            statistical["fullhouse"] = 1;
        }

        private void SetGetBingoScore(List<int[]> match_array_list)
        {
            var getBingoScore = BINGO_BONUS * match_array_list.Count;
            bingoScore.GetBingoScore += getBingoScore;
            bingoScore.WingBonus += (ScoreMultiple - 1) * getBingoScore;
            bingoScore.BingoCount += match_array_list.Count;
        }

        /// <summary>
        /// 设置新的唱票数据
        /// </summary>
        private void SetNewCallValue()
        {
            //ui上是从右往左移动的
            var nextValue = GetNextCallValue();

            if (nextValue <= 0)
            {
                return;
            }

            YZLog.LogColor("叫号！ " + nextValue);

            leftCallValue = CallArrayValues[4];
            //设置唱票队列的值,唱票队列，唱票队列数组的值，整体往后移动一位
            for (int i = CallArrayValues.Length - 1; i >= 0; i--)
            {
                if (i == 0)
                {
                    //填入最右边
                    CallArrayValues[i] = nextValue;
                }
                else
                {
                    CallArrayValues[i] = CallArrayValues[i - 1];
                }
            }
        }

        //从操作列表还原游戏数据需要
        private int propEnergy;
        private bool isSelectingDice;
        private Prop currentChooseProp;

        private bool isPause;

        private BingoScore bingoScore = new BingoScore();


        /// <summary>
        /// 开始游戏 结束倒计时
        /// </summary>
        private void Interval_50()
        {
            TimeText.text = TimeUtils.Instance.ToMinuteSecond((int)lessTime);

            //如果有引导 ， 则不开始
            if (PlayerGuideType is BingoGuideType.Teaching)
            {
                return;
            }

            if (isBegin) return;

            isBegin = true;

            var interval = 0.05f;

            bool played10 = false;

            bool played15 = false;

            bool played30 = false;

            bool use2x = false;

            //掉落一个球需要的时间
            float useTime = 2.5f;

            float lastIntervalTime = Time.time;

            float delta = interval;
            
            Observable.Interval(TimeSpan.FromSeconds(interval))
                .First(l =>
                {
                    if (!vm.Any())
                    {
                        return false;
                    }

                    var isPause = vm[vname.VM_IsPause.ToString()].ToIObservable<bool>().Value;

                    var intervalTime = Time.time - lastIntervalTime;
                    // YZLog.LogColor(intervalTime);
                    // delta = Math.Min(intervalTime, interval + 0.03f);
                    delta = intervalTime;
                    
                    lastIntervalTime = Time.time;
                    
                    //总感觉满了
                    var offsetTime = 0.8f;
                    //全部bingo后， 时间也应该停止
                    if (!isSendGameEnd && !isPause && lessTime > 0 && !IsUsingProp() && !IsAllBingo() && IsTop())
                    {
                        lessTime -= delta;
                        var lessTimeToInt = (int)lessTime;
                        TimeText.text = TimeUtils.Instance.ToMinuteSecond(lessTimeToInt);

                        if (CallSlider.transform.IsActive())
                        {
                            if (useTime > 0)
                            {
                                CallSlider.fillAmount -= delta / useTime;
                            }
                        }

                        //最后3s只落一个球
                        if (CallSlider.fillAmount <= 0)
                        {
                            //计算useTime
                            var time = lessTime - offsetTime;

                            if (time > 3)
                            {
                                var count = (int)time / 3;

                                var remainderTime=  time % 3;

                                //多加1个
                                if (remainderTime > 1.5f)
                                {
                                    useTime = time / (count + 1) - 0.5f;
                                }
                                else
                                {
                                    useTime = time / count - 0.5f;
                                }
                            }
                            else
                            {
                                useTime = time - 0.5f;
                            }

                            if (useTime > 0)
                            {
                                AddCallIndex();
                            }
                        }

                        if (!played10 && lessTimeToInt == 10)
                        {
                            played10 = true;
                            Play10LeftTextEffect();
                            // TimeText.color = new Color(231 / 255.0f, 184 / 255.0f, 0 / 255.0f);
                            TimeText.color = Color.yellow;
                            TimeText.GetComponent<SizeUpDown>().enabled = true;
                        }

                        if (!played15 && lessTimeToInt == 15)
                        {
                            played15 = true;
                          
                        }
                        
                        if (!use2x && lessTimeToInt <= 15f && propList is { Count: > 0 })
                        {
                            var doubleContainer = FindPropContainerByType(Const.DoubleScore);
                            if (doubleContainer != null)
                            {
                                use2x = true;
                                OnPropContainerClick(doubleContainer);
                            }
                        }

                        if (!played30 && lessTimeToInt == 30)
                        {
                            played30 = true;
                            Play30LeftTextEffect();
                        }
                    }

                    return lessTime <= offsetTime;
                })
                .Subscribe(i =>
                {
                    TimeText.GetComponent<SizeUpDown>().enabled = false;
                    PlayGameEndAnimation(BingoCloseType.COUNTDOWN_END, true);
                })
                .AddTo(this);
        }

        /// <summary>
        /// 是否已经选择了一个4X1的值
        /// </summary>
        /// <returns></returns>
        bool Is4X1Time()
        {
            var _4x1_value = GetChoosed4X1Value();
            return _4x1_value > 0;
        }

        /// <summary>
        /// 是否正在使用道具， 使用道具时， 游戏时间【唱票时间】不流动
        /// </summary>
        /// <returns></returns>
        bool IsUsingProp()
        {
            if (!vm.Any())
            {
                return false;
            }

            var prop = vm[vname.VM_CurrentChooseProp.ToString()].ToIObservable<Prop>().Value;

            if (prop != null && prop.id != Const.DoubleScore)
            {
                return true;
            }

            if (Is4X1Time())
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// 设置道具进度条上的文字
        /// </summary>
        private void SetPropSlierTextState()
        {
            var full = propList.Count >= MAX_PROP_COUNT;
            PropSlider.transform.Find("FullImage").SetActive(full);
            // PropEnergyText.SetActive(!full);
        }

        private TweenerCore<float, float, FloatOptions> ChangeSliderValue(Slider slider, int value,
            float duration = 0.3f, Action onComplete = null, Action onUpdate = null)
        {
            return DOTween.To(() => slider.value,
                    x => slider.value = x,
                    value,
                    duration).SetEase(Ease.Linear)
                .OnUpdate(() =>
                    onUpdate?.Invoke()
                )
                .OnComplete(
                    () => onComplete?.Invoke()
                );
        }

        private IDisposable propCountDown;

        /// <summary>
        /// 道具倒计时
        /// </summary>
        private void PropCountDown()
        {
            if (IsGuiding)
            {
                return;
            }

            int tick = 0;
            int zeroTick = 0;


            PropUsingMask.SetActive(!_4X1Transform.IsActive() && propUseCountDown > 0);

            PropUseTitle.SetActive(propUseCountDown > 0);
            // PropCountDownText.SetActive(propUseCountDown > 0);
            if (propUseCountDown > 0)
            {
                PropCountDownText.text = propUseCountDown.ToString();
                // SoftMaskScript.CutOff = 0;
                propCountDown?.Dispose();

                //todo 恢复游戏时, 声音同步
                AudioSystem.That.PlaySound(SoundPack.Prop_Use_Count_Down);

                propCountDown = Observable.Interval(TimeSpan.FromSeconds(0.05f)).First(l =>
                    {
                        if (!isBegin)
                        {
                            return false;
                        }

                        if (vm[vname.VM_IsPause.ToString()].ToIObservable<bool>().Value)
                        {
                            return false;
                        }

                        tick++;

                        var interval = 16;
                        // SoftMaskScript.CutOff += 0.05f;

                        if (tick % interval == 0)
                        {
                            // SoftMaskScript.CutOff = 0;
                            propUseCountDown--;

                            SaveGameData();

                            if (propUseCountDown > 0)
                            {
                                PropCountDownText.text = propUseCountDown.ToString();
                            }
                            else
                            {
                                zeroTick = tick;
                                PropCountDownText.text = I18N.Get("key_time_out").ToUpper();
                            }
                        }

                        return zeroTick > 0 && tick >= zeroTick + interval - 1;
                    })
                    .Subscribe(
                        l =>
                        {
                            CancelPropCountDown();
                            if (GetVmValue<Prop>(vname.VM_CurrentChooseProp.ToString(), out _) != null)
                            {
                                vm[vname.VM_CurrentChooseProp.ToString()].ToIObservable<Prop>().Value = null;
                                Record(Operation.CancelProp);
                            }
                          
                        }).AddTo(this);
            }
        }

        private void CancelPropCountDown()
        {
            propCountDown?.Dispose();
            propUseCountDown = 0;
            AudioSystem.That.StopSound(SoundPack.Prop_Use_Count_Down);
        }

        /// <summary>
        /// 加入新道具
        /// </summary>
        /// <param name="index">插入的索引</param>
        /// <returns></returns>
        private Prop AddNewProp(int index = 0)
        {
            //获取下一个道具Id
            var nextPropId = GetNextPropId();
            var newProp = new Prop(nextPropId);

            index = Math.Clamp(index, 0, propList.Count);
            //绑定对象到Component
            //不能设值， 触发动画不会被打断
            if (IsInitEnd)
            {
                var container = GetPropContainer(index);
                container.prop = newProp;
            }
            propIndex++;
            propList.Insert(index, newProp);
            return newProp;
        }

        private int FindPropContainerIndex()
        {
            if (IsInitEnd)
            {
                for (int i = 0; i < MAX_PROP_COUNT; i++)
                {
                    var propContainer = PropTransform.GetChild(i).GetComponent<BingoPropContainer>();

                    if (propContainer.IsActive) continue;

                    return i;
                }
            }
            else
            {
                if (propList.Count < MAX_PROP_COUNT)
                {
                    return propList.Count;
                }
            }
            
            return -1;
        }

        private Color GetGridTextColor(int index)
        {
            if (bingoGrids[index].Clicked)
            {
                return new Color(226 / 255.0f, 253 / 255.0f, 255 / 255.0f);
            }

            return new Color(133 / 255.0f, 73 / 255.0f, 69 / 255.0f);
        }

        /// <summary>
        /// 如果当前有x2道具, 则跳过选择下一个
        /// </summary>
        /// <returns></returns>
        int GetNextPropId()
        {
            var propId = propIdArray[propIndex % propIdArray.Length];

            //处理有四选一道具， 但是没有四选一数组的情况
            var not4X1Array = propId == Const.ChooseOne && (chooseArray == null || chooseArray.Length == 0);

            if (not4X1Array)
            {
                YZLog.Error("四选一道具 列表为空");
                propIndex++;
                return GetNextPropId();
            }

            /*// 如果当前有x2道具, 则跳过选择下一个
            if (propId == Const.DoubleScore && HaveTheProp(Const.DoubleScore))
            {
                propIndex++;

                var containsChooseAny = propIdArray.Contains((byte)Const.ChooseAny);
                if (containsChooseAny
                    || (propIdArray.Contains((byte)Const.ChooseOne) && chooseArray is { Length: > 0 })
                   )
                {
                    return GetNextPropId();
                }
            }*/

            return propId;
        }

        bool HaveTheProp(int type)
        {
            for (int i = 0; i < propList.Count; i++)
            {
                if (propList[i].id == type)
                {
                    return true;
                }
            }

            return false;
        }

        //结算时
        private void AddEndScore(bool record = true)
        {
            var time = (int)lessTime;

            // 时间分数得分条件：提前完成fullhouse，时间剩余。主动退出不给时间分
            if (time > 0 && IsAllBingo())
            {
                bingoScore.TimeBonus = time * TIME_COEFFICIENT;
                if (record)
                {
                    Record(Operation.AddTimeScore, time);
                }
            }

            //需要超过一个
            if (bingoCount > 1)
            {
                bingoScore.WingBonus += (ScoreMultiple - 1) * bingoScore.MultiBingo;
                // if (record)
                // {
                //     Record(Operation.AddMultiBingoScore, bingoCount);
                // }
            }

            bingoScore.LessTime = time;
            statistical["bingo"] = bingoScore.BingoCount;
            if (record)
            {
                Record(Operation.EndGame);
            }
        }

        private bool AllGridClicked()
        {
            foreach (var grid in bingoGrids)
            {
                if (!grid.Clicked)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 唱票队列已经穷军
        /// </summary>
        /// <returns></returns>
        bool UseAllCallTime()
        {
            return callArrayLength <= 0;
        }

        Text GetGridTextComponent(int index)
        {
            return GridsGroup.GetChild(index).Find("Text").GetComponent<Text>();
        }

        Image GetGridNormalImage(int index)
        {
            return GridsGroup.GetChild(index).Find("bg").GetComponent<Image>();
        }

        Image GetGridBingoImage(int index)
        {
            return GridsGroup.GetChild(index).Find("bingo").GetComponent<Image>();
        }

        /// <summary>
        /// 数据层面
        /// </summary>
        void InitBingoGrids()
        {
            //服务器给了25个唱票数据
            bingoGrids = new BingoGrid[bingo_grid_data.Length];
            for (int i = 0; i < bingoGrids.Length; i++)
            {
                int dataIndex = 0;
                //正中心
                if (i == 12)
                {
                    bingoGrids[i].MagicClicked = true;
                }
                else
                {
                    var convertIndexToRow = ConvertIndex(i);
                    
                    bingoGrids[i].Value = bingo_grid_data[convertIndexToRow];
                }

                bingoGrids[i].Index = i;
                // bingoGrids[convertIndexToRow].Index = convertIndexToRow;
            }
        }

        /// <summary>
        /// 将索引由竖向转化为横向
        /// 0 5 10 15 20
        /// 1 6 11 16 21
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        int ConvertIndex(int index)
        {
            //index = 5 就应该是第二排了
            int x = index / 5;
            int y = index % 5;

            return x + y * 5;
        }

        bool CheckGridBingo(int i, int j, ref bool chance)
        {
            //如果没点击
            var bingoGrid = bingoGrids[i + j * 5];
            if (!bingoGrid.Clicked)
            {
                chance = false;
                return false;
            }

            //至少要有一个不是 bingoed
            if (!bingoGrid.IsBingo)
            {
                chance = true;
            }

            return true;
        }

        /// <summary>
        /// 是否存在bingo
        /// </summary>
        /// <returns></returns>
        List<int[]> GetBingoMatch()
        {
            var result = new List<int[]>();
            //检查横的
            for (int i = 0; i < 5; i++)
            {
                bool chance = false;
                var int_array = new int[5];
                for (int j = 0; j < 5; j++)
                {
                    if (!CheckGridBingo(i, j, ref chance))
                    {
                        break;
                    }

                    int_array[j] = i + j * 5;
                }

                if (chance)
                {
                    result.Add(int_array);
                }
            }

            //检查竖的
            for (int j = 0; j < 5; j++)
            {
                bool chance = false;
                var int_array = new int[5];
                for (int i = 0; i < 5; i++)
                {
                    if (!CheckGridBingo(i, j, ref chance))
                    {
                        break;
                    }

                    int_array[i] = i + j * 5;
                }

                if (chance)
                {
                    result.Add(int_array);
                }
            }

            //检查交叉
            //左上到右下
            bool leftUpToRightButt = false;
            var int_left_array = new int[5];
            for (int i = 0; i < 5; i++)
            {
                if (!CheckGridBingo(i, i, ref leftUpToRightButt))
                {
                    break;
                }

                int_left_array[i] = i + i * 5;
            }

            if (leftUpToRightButt)
            {
                result.Add(int_left_array);
            }

            //左下到右上
            bool leftButtToRightUp = false;
            var int_right_array = new int[5];
            for (int i = 0; i < 5; i++)
            {
                if (!CheckGridBingo(i, 5 - 1 - i, ref leftButtToRightUp))
                {
                    break;
                }

                int_right_array[i] = i + (4 - i) * 5;
            }

            if (leftButtToRightUp)
            {
                result.Add(int_right_array);
            }

            //检查周围4个点
            bool around = false;
            var int_around_array = new[] { 0, 4, 20, 24 };
            for (int i = 0; i < 4; i++)
            {
                if (i == 0)
                {
                    if (!CheckGridBingo(0, 0, ref around))
                    {
                        break;
                    }
                }
                else if (i == 1)
                {
                    if (!CheckGridBingo(4, 0, ref around))
                    {
                        break;
                    }
                }
                else if (i == 2)
                {
                    if (!CheckGridBingo(0, 4, ref around))
                    {
                        break;
                    }
                }
                else
                {
                    if (!CheckGridBingo(4, 4, ref around))
                    {
                        break;
                    }
                }
            }

            if (around)
            {
                result.Add(int_around_array);
            }

            return result;
        }

        private void RefreshScore(Transform from = null)
        {
            var key = vname.VM_Score.ToString();
            if (vm.ContainsKey(key))
            {
                if (from != null)
                {
                    scoreStartTransform.position = from.position;
                }
                else
                {
                    scoreStartTransform.localPosition = Vector3.zero;
                }

                //Refresh 是强制刷新的
                vm[key].Refresh();
            }
        }

        private void RefreshScore(Vector3 localposition, float duration = 1f)
        {
            var key = vname.VM_Score.ToString();
            if (vm.ContainsKey(key))
            {
                scoreDuration = duration;
                scoreStartTransform.localPosition = localposition;
                //Refresh 是强制刷新的
                vm[key].Refresh();
            }
        }

        /// <summary>
        /// 获取下一个唱票数据
        /// </summary>
        /// <returns></returns>
        byte GetNextCallValue()
        {
            if (callArrayLength <= 0)
            {
                //byte不能为负数
                return 0;
            }

            byte result;
            if (guideCount > 0)
            {
                if (bingoGrids[first_guide_grid_index].Clicked)
                {
                    result = bingoGrids[second_guide_grid_index].Value;
                }
                else
                {
                    result = bingoGrids[first_guide_grid_index].Value;
                }

                guideCount--;
                TotalCallArray.Remove(result);
            }
            else
            {
                result = TotalCallArray[0];
                TotalCallArray.RemoveAt(0);
            }

            HistorySet.Add(result);

            return result;
        }

        private bool isSendGameEnd;

        /// <summary>
        /// 游戏结束唯一指定接口
        /// </summary>
        /// <param name="bingoCloseType"></param>
        /// <param name="isTimeOut"></param>
        /// <param name="forceEnd"></param>
        async UniTask PlayGameEndAnimation(BingoCloseType bingoCloseType, bool isTimeOut = false, bool forceEnd = false)
        {
            if (isSendGameEnd)
            {
                return;
            }

            //发送后禁止点击
            ForbidSelfClick();

            isSendGameEnd = true;

            var token = this.GetCancellationTokenOnDestroy();

            //清空其他道具
            HidePropBesideDouble();

            var doubleContainer = FindPropContainerByType(Const.DoubleScore);

            //先拷贝一个score， 后面还原
            var originScore = new BingoScore(bingoScore);

            List<int[]> match_array_list = null;
            if (isTimeOut)
            {
                match_array_list = GetBingoMatch();
                if (match_array_list.Count > 0)
                {
                    if (doubleContainer != null)
                    {
                        scoreMultiple = 2;
                    }

                    SetGetBingoScore(match_array_list);
                }
            }

            if (IsAllBingo())
            {
                AddFullHouseScore();
            }

            AddEndScore();

            SaveGameData(true);

            bingoScore = originScore;

            if (forceEnd)
            {
                inNormalAnimation = false;
            }

            //双倍时间结束 但双倍要保留
            var scoreMul = ScoreMultiple;
            doubleSliderSeq?.Kill(true);
            //不触发动画
            scoreMultiple = scoreMul;

            //为了避免结束游戏的时候 ， 分数不统一
            if (inAnimation)
            {
                await UniTask.WhenAny(UniTask.WaitUntil(() => !inAnimation, cancellationToken: token),
                    UniTask.Delay(TimeSpan.FromSeconds(3f), cancellationToken: token));
            }

            if (isTimeOut)
            {
                // UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_time_out"));
                PlayTimeUpTextEffect();

                await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);

                //主动bingo
                match_array_list ??= GetBingoMatch();
                if (match_array_list.Count > 0)
                {
                    //使用双倍道具 , 上面的异步过程中玩家还没有使用双倍道具
                    //再判断一下， 确保玩家没有主动使用双倍道具
                    if (scoreMultiple <= 1 && doubleContainer != null && doubleContainer.IsActive)
                    {
                        OnPropContainerClick(doubleContainer);
                        await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: token);
                    }

                    SetGridsByBingoList(match_array_list, out var dalaytime, true);

                    await UniTask.Delay(TimeSpan.FromSeconds(dalaytime + 1.2), cancellationToken: token);
                }
            }

            SendGameEnd(bingoCloseType);
        }

        private BingoPropContainer FindPropContainerByType(int Type)
        {
            BingoPropContainer result = null;
            if (propList != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    var propContainer = GetPropContainer(i);
                    if (!propContainer.IsActive)
                    {
                        continue;
                    }

                    if (propContainer.prop == null)
                    {
                        continue;
                    }

                    if (propContainer.prop.id == Type)
                    {
                        result = propContainer;
                    }
                }
            }

            return result;
        }


        private void HidePropBesideDouble()
        {
            BingoPropContainer result = null;
            if (propList != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    var propContainer = GetPropContainer(i);
                    if (!propContainer.IsActive)
                    {
                        continue;
                    }

                    if (propContainer.prop is not { id: Const.DoubleScore })
                    {
                        propContainer.IsActive = false;
                    }
                }
            }
        }

        private string matchId => GetStartData().MatchId;

        public void SendGameEnd(BingoCloseType bingoCloseType)
        {
            AddEndScore(false);

            PauseCanvas.sortingOrder = UICanvas.sortingOrder + 2;

            if (IsFakeGame())
            {
                MediatorBingo.Instance.ClearBingoDB();
                ShowUIScore();
            }
            else
            {
                NetSystem.That.SetFailCallBack(s => { isSendGameEnd = false; });

                MediatorRequest.Instance.GameEnd(matchId, bingoScore.TotalScore,
                    bingoCloseType,
                    bingoScore.LessTime,
                    operateList,
                    statistical,
                    callBack: ShowUIScore);
            }
        }

        private bool IsFakeGame()
        {
            var table = GetTable();
            return table?["fakeData"] is true;
        }

        void ShowUIScore()
        {
            UserInterfaceSystem.That.ShowUI<UIShowScore>(new GameData()
            {
                ["bingoScore"] = bingoScore,
                ["matchId"] = matchId,
                ["isFake"] = IsFakeGame()
            });
            EventDispatcher.Root.Raise(GlobalEvent.ONE_GAME_END);
            Close();
        }

        void InitGridComs()
        {
            for (int i = 0; i < bingoGrids.Length; i++)
            {
                SetOneGridUI(i);
            }
        }

        /// <summary>
        /// 设置bingo image 的sprite
        /// </summary>
        /// <param name="index"></param>
        /// <param name="bingoGrid"></param>
        void SetBingoImage(BingoGrid bingoGrid)
        {
            if (bingoGrid.IsBingo)
            {
                SetBingoImageByCount(bingoGrid.Index, bingoGrid.AnimationBingoCount);
            }
            else if (bingoGrid.MagicClicked)
            {
                SetMagicClickImage(bingoGrid.Index);
            }
        }

        void SetBingoImageByCount(int index, int count)
        {
            if (count <= 0)
            {
                count = 1;
            }
            
            GetGridBingoImage(index).sprite = MediatorBingo.Instance.GetSpriteByUrl($"uibingo/bingo_count_{count}");
        }


        /// <summary>
        /// 打开教学面板
        /// </summary>
        private void OpenTuition()
        {
            UserInterfaceSystem.That.ShowAnotherUI<UIBingo>(new GameData()
            {
                ["guideType"] = BingoGuideType.Teaching,
                ["bingoData"] = GetStartData()
            });
        }

        void LoadFinger1(Transform parent)
        {
            finger1 ??= ResourceSystem.That.InstantiateGameObjSync("common/finger01");
            finger1.SetActive(true);
            finger1.transform.SetParent(parent);
            finger1.transform.localScale = new Vector3(1, 1, 1);
            finger1.transform.localPosition = new Vector3(-10, 10, 0);
        }

        void LoadFinger1(Transform parent, Vector3 localPosition)
        {
            finger1 ??= ResourceSystem.That.InstantiateGameObjSync("common/finger01");
            finger1.SetActive(true);
            finger1.transform.SetParent(parent);
            finger1.transform.localScale = new Vector3(1, 1, 1);
            finger1.transform.localPosition = localPosition;
        }

        /// <summary>
        /// UI反馈上设置除中心点外， 任意位置可以被点击
        /// </summary>
        void SetAllGridClickState(bool open)
        {
            for (int i = 0; i < bingoGrids.Length; i++)
            {
                if (i == 12)
                {
                    continue;
                }

                var grid = bingoGrids[i];

                if (grid.Clicked || grid.IsBingo)
                {
                    SetGridBtnClickState(i, open);
                }
            }
        }

        void PlayBingoTextEffect(int count)
        {
            if (IsInitEnd)
            {
                BingoTextEffect.SetActive(true);
                if (count >= 2)
                {
                    AudioSystem.That.PlaySound(SoundPack.Bingo_Mul_Count);
                }
                else
                {
                    AudioSystem.That.PlaySound(SoundPack.Bingo_Normal);
                }

                if (count >= 2)
                {
                    AmazingTextEffect.Play(() =>
                    {
                        RefreshScore(new Vector3(0,  -200 , 0), 1.2f);
                        BingoTextEffect.Play();
                    });
                }
                else
                {
                    RefreshScore(new Vector3(0,  -200, 0));
                    BingoTextEffect.Play();
                }
            }
        }

        void PlayDoubleTextEffect()
        {
            if (IsInitEnd)
            {
                DoubleTextEffect.SetActive(true);
                DoubleTextEffect.Play();
            }
        }

        void Play10LeftTextEffect()
        {
            if (IsInitEnd)
            {
                _10sLeft.SetActive(true);
                _10sLeft.Play();
            }
        }

        void Play30LeftTextEffect()
        {
            if (IsInitEnd)
            {
                _30sLeft.SetActive(true);
                _30sLeft.Play();
            }
        }

        void PlayTimeUpTextEffect()
        {
            TimeUpTextEffect.SetActive(true);
            TimeUpTextEffect.Play();
        }

        void LoadStyle()
        {
            int pathIndex = 1;
            var bingoData = GetStartData();
            roomStyle = bingoData.Style;
            switch (roomStyle)
            {
                //style1
                case RoomStyle.CastleStyle:
                case RoomStyle.VallyStyle:
                case RoomStyle.TreasurePlant:
                    ballsPaltform.SetActive(true);
                    game_bg_2.SetActive(false);
                    pathIndex = 1;
                    break;
                //style2 
                case RoomStyle.FreeBonus:
                case RoomStyle.SpringWater:
                case RoomStyle.Windstorm:
                    pathIndex = 2;
                    propSliderBg.color = Color.red.AlphaChange(128);
                    ballsPaltform.SetActive(false);
                    game_bg_2.SetActive(true);
                    break;
                //style3
                case RoomStyle.Battleground:
                case RoomStyle.ThornForest:
                    ballsPaltform.SetActive(true);
                    game_bg_2.SetActive(false);
                    pathIndex = 3;
                    break;
            }

            var sprite = MediatorBingo.Instance.GetSpriteByUrl($"style{pathIndex}/uibingo_gamebg_1");
            game_bg_dissolve.sprite = sprite;
            game_bg_1.sprite = sprite;
            if (ballsPaltform.IsActive())
            {
                ballsPaltform.sprite = MediatorBingo.Instance.GetSpriteByUrl($"style{pathIndex}/uibingo_ball_platform");
            }

            bingo_grids_bg.sprite = MediatorBingo.Instance.GetSpriteByUrl($"style{pathIndex}/uibingo_main_group_bg");

      /* 新版本UI不需要了*/
            // for (int i = 0; i < 3; i++)
            // {
            //     GetPropContainer(i).PropPlat.sprite =
            //         MediatorBingo.Instance.GetSpriteByUrl($"style{pathIndex}/uibingo_prop_platform");
            // }
        }

        bool GridInCallingArray(int index)
        {
            var grid = bingoGrids[index];

            if (CallArrayValues.Contains(grid.Value))
            {
                return true;
            }

            if (leftCallValue > 0 && leftCallValue == grid.Value)
            {
                return true;
            }

            return false;
        }

        bool GridIsCalled(int index)
        {
            var grid = bingoGrids[index];
            return HistorySet.Contains(grid.Value);
        }

        public void GM_Mark_Call()
        {
            for (int i = 0; i < bingoGrids.Length; i++)
            {
                if (bingoGrids[i].IsBingo || bingoGrids[i].Clicked)
                {
                    continue;
                }

                if (GridInCallingArray(i))
                {
                    GetGridNormalImage(i).color = Color.cyan;
                }
            }
        }

        bool IsTop()
        {
            var ui = UserInterfaceSystem.That.GetTopNormalUI();

            if (ui == null)
            {
                return false;
            }

            return ui.UIName == UIName;
        }

        public void GM_SetAllClick()
        {
            for (var index = 0; index < bingoGrids.Length; index++)
            {
                var bingoGrid = bingoGrids[index];
                if (bingoGrid.IsBingo || bingoGrid.Clicked)
                {
                    continue;
                }

                bingoGrids[index].Clicked = true;
                SetOneGridUI(index);
            }

            this.AttachTimer(0.5f, () => { AutoFullHouse(); });
        }

        private void AutoFullHouse()
        {
            var doubleContainer = FindPropContainerByType(Const.DoubleScore);
            if (doubleContainer != null)
            {
                OnPropContainerClick(doubleContainer);
            }

            ClickBingo();
        }

        void PlayBingoShiny(int value)
        {
            var bingoType = GetBingoType(value);
            BingoTitleGroup.GetChild((int)bingoType - 1).GetComponent<UIShiny>().Play();
        }
    }
}