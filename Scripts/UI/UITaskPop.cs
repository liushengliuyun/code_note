//this file is auto created by QuickCode,you can edit it 
//do not need to care initialization of ui widget any more 
//------------------------------------------------------------------------------
/**
* @author :
* date    :
* purpose :
*/
//------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.API;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils.Static;
using UI;
using UnityEngine.UI;
using Utils;

public class UITaskPop : UIBase<UITaskPop>
{
    #region UI Variable Statement

    [SerializeField] private GameObject TaskGroup;
    [SerializeField] private Button CloseBtn;

    #endregion

    public override UIType uiType { get; set; } = UIType.Window;

    public override void InitEvents()
    {
        AddEventListener(Proto.TASK_CHOOSE_LEVEL, (sender, eventArgs) =>
        {
            if (eventArgs is ProtoEventArgs { Result: ProtoResult.Success })
            {
                Close();
            }
        });
    }

    protected override void OnClose()
    {
        base.OnClose();
        YZFunnelUtil.YZFunnelActivityPop(ActivityType.TaskSystem
            // , charge_id: chargeId
            , isauto: GetPopType() is 1 or 2
            , duration: (int)duration
            , outtype: closeByBtn ? 1 : 2
            , switch_click: SwitchClick
        );
    }

    /// <summary>
    ///  0玩家主动点击弹出；1：登录时弹出；2：对局结束后弹出
    /// </summary>
    private int GetPopType()
    {
        var table = GetArgsByIndex<GameData>(0);
        if (table?["pop_type"] != null)
        {
            var pop_type= (int)table["pop_type"];
            return pop_type;
        }

        return -1;
    }
    
    public override void OnStart()
    {
        var pop_type= GetPopType();
        if (pop_type >= 0)
        {
            MediatorTask.Instance.MarkOpenTask(pop_type);
        }
        
        CloseBtn.onClick.AddListener(Close);

        for (int i = 1; i <= 3; i++)
        {
            var level = i;
            var comtainer = GetTaskMonoByLevel(i);
            comtainer.RewardText.text = $"${MediatorTask.Instance.GetBonusCount(level)}";
            comtainer.ChooseBtn.onClick.AddListener(() => { UserInterfaceSystem.That.ShowUI<UITask>(new GameData()
            {
                ["level"] = level
            } ); });
            if (comtainer.ChooseBtn is MyButton myButton)
            {
                myButton.title = I18N.Get("TO_VIEW");
            }
        }
    }

    public override void InitVm()
    {
    }

    public override void InitBinds()
    {
    }


    TaskMono GetTaskMonoByLevel(int level)
    {
        return TaskGroup.transform.GetChild(level - 1).GetComponent<TaskMono>();
    }
}