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
using System.Collections.Generic;
using AndroidCShape;
using CatLib.EventDispatcher;
using Core.Controllers;
using Core.Extensions;
using UnityEngine;
using Core.Extensions.UnityComponent;
using Core.Manager;
using Core.Services.NetService.API.Facade;
using Core.Services.PersistService.API.Facade;
using Core.Services.ResourceService.API.Facade;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using iOSCShape;

#if !RELEASE
using GameDevWare.Dynamic.Expressions;
using GameDevWare.Dynamic.Expressions.CSharp;
#endif

using Utils;
using Newtonsoft.Json;
using Reactive.Bindings;
using Tayx.Graphy;
using TMPro;
using UI;
using UI.Activity;


using UniRx;
using UnityEngine.UI;
using Text = UnityEngine.UI.Text;

using OneSignalSDK;

public class UITools : UIBase<UITools>
{
    #region UI Variable Statement
    [SerializeField] private Image bg;
    [SerializeField] private MyButton EndGameBtn;
    [SerializeField] private MyButton MessageLogBtn;
    [SerializeField] private MyButton TestBtn;
    [SerializeField] private MyButton TestBtn1;
    [SerializeField] private MyButton ConfirmRollResultBtn;
    [SerializeField] private TMP_InputField[] inputFields;
    //道具
    [SerializeField] private MyButton _4x1Btn;
    [SerializeField] private MyButton _6x1Btn;
    [SerializeField] private MyButton DoubleScoreBtn;
    [SerializeField] private MyButton addCrossBtn;
    
    [SerializeField] private Transform ShowInYaztyGame;
    [SerializeField] private MyButton ClearAllCahce;
    //校验开关
    [SerializeField] private Toggle CheckToggle;
    //作弊开关
    [SerializeField] private Toggle CheatToggle;
    
    [SerializeField] private Toggle[] ToggleGroup;
    [SerializeField] private MyButton passGuideBtn;
    
    [SerializeField] private Slider slider_ChargeSlider;
    [SerializeField] private MyButton mybutton_SubMitButton;
    [SerializeField] private Button closeBtn;
    [SerializeField] private Text text_Title;
    [SerializeField] private MyButton PassDayBtn;
    [SerializeField] private TMP_InputField inputFieldRank;
    [SerializeField] private TMP_InputField inputFieldGameScore;
    [SerializeField] private MyButton gameScoreBtn;
    [SerializeField] private TMP_Dropdown achieveDropdown;
    [SerializeField] private TMP_InputField inputFieldAchieveCount;
    [SerializeField] private MyButton AdTestBtn;
    [SerializeField] private MyButton OpenCalledBtn;
    #endregion

    public override UIType uiType { get; set; } = UIType.Top;

    public override void OnStart()
    {
        MessageLogBtn.SetClick(() =>
        {
            var queue = NetSystem.That.SendedProtoQueue;
            if (queue != null)
            {
                YZLog.LogColor(string.Join("\n", queue));
            }
        });
        
        closeBtn.onClick.AddListener(Close);

        CheckToggle.isOn = Root.Instance.IsReplayCheckOpen;
        
        CheatToggle.isOn = UIBingo.IsCheat;
        
        FindObjectOfType<GraphyManager>(true)?.transform.SetActive(true);

        CheckToggle.onValueChanged.AddListener(value =>
        {
            MediatorRequest.Instance.GM_GameCheckSwitch(value ? 0 : 1);
        });
        
        CheatToggle.onValueChanged.AddListener(value =>
        {
            UIBingo.IsCheat = value;
        });

        ClearAllCahce.SetClick(() => OnClearAllCacheBtnClick());
        slider_ChargeSlider.value = 100;
        RegisterInterval(1f, () =>
        {
            var uiBingo = UserInterfaceSystem.That.Get<UIBingo>();
            ShowInYaztyGame.SetActive(uiBingo != null);
        }, true);

        EndGameBtn.SetClick(() => OnEndGameBtnClick());
        _4x1Btn.SetClick(() => { AddProp(Const.ChooseOne); });

        _6x1Btn.SetClick(() => { AddProp(Const.ChooseAny); });
        DoubleScoreBtn.SetClick(() => { AddProp(Const.DoubleScore); });
        addCrossBtn.SetClick(() => { AddProp(Const.Cross); });
        
        AdTestBtn.SetClick(() =>
        {
#if UNITY_ANDROID
           YZAndroidPlugin.Shared.AndroidOpenMaxTestTool();  
#endif
        });
        
        
        TestBtn.SetClick(Test);
        TestBtn1.SetClick(Test1);
 

        ConfirmRollResultBtn.SetClick(() =>
        {
            var uiBingo = UserInterfaceSystem.That.Get<UIBingo>();
            if (uiBingo != null)
            {
                CloseGameCheck();
                uiBingo.GM_SetNextRollResult(GetInputRoll());
                Close();
            }
        });
        
        OpenCalledBtn.SetClick(() =>
        {
            var uiBingo = UserInterfaceSystem.That.Get<UIBingo>();
            if (uiBingo != null)
            {
                uiBingo.GM_Mark_Call();
                Close();
            }
        });

        mybutton_SubMitButton.SetClick(() =>
        {
            var itemType = vm[vname.itemType.ToString()].ToIObservable<int>().Value;
            MediatorRequest.Instance.GMAddMoney(itemType, (float)Math.Round(slider_ChargeSlider.value, 2));
            Close();
        });

        slider_ChargeSlider.OnValueChangedAsObservable().Subscribe(f => text_Title.text = Math.Round(f, 2).ToString());

        PassDayBtn.SetClick(PassDayBtnClick);

        passGuideBtn.SetClick(() => MediatorRequest.Instance.SendNewPlayerGuideStep(NewPlayerGuideStep.SECOND_BONUS_GAME));

        for (int i = 0; i < ToggleGroup.Length; i++)
        {
            var toggle = ToggleGroup[i];
            int type;
            if (i == 2)
            {
                type = Const.Coin;
            }
            else
            {
                type = i + 1;
            }

            toggle.onValueChanged.AddListener(arg0 =>
            {
                if (arg0)
                {
                    vm[vname.itemType.ToString()].ToIObservable<int>().Value = type;
                }
            });
        }
        
        gameScoreBtn.SetClick(() =>
        {
            int gameScore = int.Parse(inputFieldGameScore.text);
            int rank = int.Parse(inputFieldRank.text);
            string achieve = achieveDropdown.options[achieveDropdown.value].text;
            int count = int.Parse(inputFieldAchieveCount.text);
            Dictionary<string, int> statistical = new Dictionary<string, int> { { achieve, count } };
            var achieveJson = JsonConvert.SerializeObject(statistical);
            MediatorRequest.Instance.GMSetGameResult(rank, achieveJson, gameScore);
        });
    }

    private void OnClearAllCacheBtnClick()
    {
        Root.Instance.Role.match_first_game_guide = 0;
        PersistSystem.That.DeleteAll();
        MediatorBingo.Instance.ClearBingoDB();
    }

    private void OnEndGameBtnClick()
    {
        var uiBingo = UserInterfaceSystem.That.Get<UIBingo>();
        if (uiBingo != null)
        {
            MediatorRequest.Instance.SendNewPlayerGuideStep(NewPlayerGuideStep.FIRST_ROOM_GUIDE_FINISH);
            uiBingo.SendGameEnd(BingoCloseType.EARLY_END);
            Close();
        }
    }

    private void AddProp(int PropType)
    {
        var uiBingo = UserInterfaceSystem.That.Get<UIBingo>();
        if (uiBingo != null)
        {
            CloseGameCheck();
            uiBingo.GM_AddProp(PropType);
        }
    }

    private void CloseGameCheck()
    {
        if (Root.Instance.IsReplayCheckOpen)
        {
            //关闭对局校验
            MediatorRequest.Instance.GM_GameCheckSwitch(1);
        }
    }

    private void PassDayBtnClick()
    {
        MediatorRequest.Instance.PassDayLogin();
        // Test();
    }

    private void OnDisable()
    {
        FindObjectOfType<GraphyManager>()?.transform.SetActive(false);
    }

    enum vname
    {
        /// <summary>
        /// 要添加道具的类型
        /// </summary>
        itemType
    }

    public override void InitVm()
    {
        vm[vname.itemType.ToString()] = new ReactivePropertySlim<int>(Const.Cash);
    }

    public override void InitBinds()
    {
    }

    public override void InitEvents()
    {
        AddEventListener(Proto.ONLINE_CHARGE, (sender, eventArgs) =>
        {
            if (eventArgs is ProtoEventArgs { Result: ProtoResult.Success })
            {
                Close();
            }
        });

        AddEventListener(Proto.GM_GAME_REPLAY_CLOSE,
            (sender, eventArgs) => { CheckToggle.isOn = Root.Instance.IsReplayCheckOpen; });

        /*AddEventListener(GlobalEvent.GM_ROLL, (sender, eventArgs) =>
        {
            if (sender is byte[] value)
            {
                SetInputRoll(value);
            }
        });*/
    }

    private int[] GetInputRoll()
    {
        int[] result = new int[5];
        for (int i = 0; i < result.Length; i++)
        {
            var byteValue = Convert.ToInt32(inputFields[i].text);
            var min = 0;
            var max = 80;
            byteValue = Math.Clamp(byteValue, min, max);
            result[i] = byteValue;
        }

        return result;
    }

    private void Test()
    {
        // FakeChargeSuccess();
        // OpenUILucky();
        // syncSingleItem();
        // getMatchInfo();
        // FakeGame();
        // UserInterfaceSystem.That.ShowUI<UIConfirm>();
        // YZGameUtil.GetIpAddressByAmazon();
        YZSDKsController.Shared.IsBlockValid = false;
        LocationManager.Shared.RequestLocate();
        Close();
    }

    private void FakeChargeSuccess()
    {
        UserInterfaceSystem.That.CloseAllUI(new[] { nameof(UIMain) });
        var gameData = new GameData()
        {
            ["diff"] = new List<Item>
            {
                new Item(Const.Bonus, 5)
            }
        };

        UserInterfaceSystem.That.TopInQueue<UIGetRewards>(gameData);

        EventDispatcher.Root.Raise(GlobalEvent.CHARGE_SUCCESS);
    }

    private void Test1()
    {
        // MediatorRequest.Instance.RefreshMuseum();
        // FakeChargeSuccess();
        // SetAllClick();
        // setTask2Hard();
        // UserInterfaceSystem.That.RemoveUIByName(nameof(UIWaitingCtrler));
// #if !RELEASE
//         dynamicExpression();
// #endif
        // iOSCShapeTool.Shared.IOSYZRequestATT();
        // OpenUIPlayerSubPhone();
        // OpenUIDuelJoin();
        Close();
        // GameObject error = null;
        //
        // error.name = "";
        // CoordinatesTest();
        // DeviceInfoUtils.Instance.GetUserAgent();
        MediatorRequest.Instance.RefreshItem();
    }

    private void OpenUILucky()
    {
        UserInterfaceSystem.That.TopInQueue<UILuckyGuy>(new GameData()
        {
            ["enterType"] = ActivityEnterType.Click
        });
    }
 
     void MockPopFriendDuel()
    {
        UserInterfaceSystem.That.SingleTonQueue<UIFriendsDuel>(new GameData()
        {
            ["isTriggerPop"] = true
        }); 
    }

     private void PopAfterGame5()
     {
         if (Root.Instance.MonthCardInfo.NotBuy)
             UserInterfaceSystem.That.SingleTonQueue<UIMonthCardNew>(ActivityEnterType.FirstTenGame);
         else
             UserInterfaceSystem.That.SingleTonQueue<UIMonthCard>(ActivityEnterType.FirstTenGame);
                    
         UserInterfaceSystem.That.SingleTonQueue<UIOnlineReward>();
     }

     private void syncSingleItem()
     {
         EventDispatcher.Root.Raise(GlobalEvent.Sync_Single_Item, Const.Bonus);
     }

     private void getMatchInfo()
     {
         MediatorRequest.Instance.MatchInfo("7010000188894");
     }
     
     private void FakeGame()
     {
         UserInterfaceSystem.That.RemoveUIByName("UIBingo");
         UserInterfaceSystem.That.RemoveUIByName("UIShowScore");
         UserInterfaceSystem.That.ShowUI<UIBingo>(new GameData()
         {
             ["fakeData"] = true
         });
     }

     void SetAllClick()
     {
         var uiBingo = UserInterfaceSystem.That.Get<UIBingo>();
         if (uiBingo != null)
         {
             uiBingo.GM_SetAllClick();
         }
     }
     
     void setTask2Hard()
     {
         Root.Instance.CurTaskInfo.level = 3;
     }

#if !RELEASE
    void dynamicExpression()
    {
        AppDomain currentDomain = AppDomain.CurrentDomain;
        var typeResolver = new AssemblyTypeResolver(currentDomain.GetAssemblies());
        // var typeResolver = new AssemblyTypeResolver(typeof(UnityEngine.Application).Assembly);

        // const string expression = "UserInterfaceSystem.That.Get<UILogin>()";
         
        const string expression = "Time.timeScale";

        Debug.Log("Expression: " + expression);
        Debug.Log("Result: " + CSharpExpression.Evaluate<object>(expression, typeResolver));
    }
#endif

    void OpenUIPlayerSubPhone()
     {
         UserInterfaceSystem.That.ShowUI<UIPlayerSubPhone>();
     }
     
     void OpenUIDuelJoin()
     {
         UserInterfaceSystem.That.ShowUI<UIDuelJoin>();
     }

     void CoordinatesTest()
     {
         NetSystem.That.GetCountryFromCoordinates(42.77927f, -74.92676f);
     }
     
}