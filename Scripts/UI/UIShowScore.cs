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
using Core.Extensions.UnityComponent;
using Core.Services.PersistService.API.Facade;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace UI
{
    public class UIShowScore : UIBase<UIShowScore>
    {
        #region UI Variable Statement
        
        [SerializeField] private Text text_TotalScoreValue;
        [SerializeField] private Text text_BaseScoreValue;
        [SerializeField] private Text text_SectionBonusValue;
        [SerializeField] private Text text_ExtraYatzyBonusValue;
        [SerializeField] private Text text_FullCardBonusValue;
        [SerializeField] private Text text_WingBonusValue;
        [SerializeField] private Text text_BestTodayValue;
        [SerializeField] private Text text_BestAllTimeValue;
        [SerializeField] private Text text_LessTimeText;
        [SerializeField] private Text text_TimeBonusValue;
        [SerializeField] private Text PenaltyCountText;
        
        [SerializeField] private MyButton mybutton_SubMitButton;
        [FormerlySerializedAs("image_SubMitBg")] [SerializeField] private Image SubMitBgMask;
        [SerializeField] private Transform new_1;
        [SerializeField] private Transform new_2;
        
        [SerializeField] private Transform score_parent_Trans;

        #endregion

        [SerializeField] private int subMitGiveTime = 10;

        private new void Awake()
        {
            base.Awake();
            //对动画没有影响
            score_parent_Trans.SetActive(false);
        }
        
        public override void OnStart()
        {
            var table = GetTable();
            var matchId = table?["matchId"] as string;
            // var operateList = args[2] as List<int[]>;
            
            if (table?["bingoScore"] is BingoScore bingoScore)
            {
                text_BaseScoreValue.text = GameUtils.TocommaStyle(bingoScore.BaseScore);
                
                text_SectionBonusValue.text = GameUtils.TocommaStyle(bingoScore.GetBingoScore);

                text_ExtraYatzyBonusValue.text = GameUtils.TocommaStyle(bingoScore.MultiBingo);

                text_FullCardBonusValue.text = GameUtils.TocommaStyle(bingoScore.FullGridBonus);

                text_WingBonusValue.text = GameUtils.TocommaStyle(bingoScore.WingBonus);

                var totalScore = bingoScore.TotalScore;
                text_TotalScoreValue.text = GameUtils.TocommaStyle(totalScore);

                text_TimeBonusValue.text = GameUtils.TocommaStyle(bingoScore.TimeBonus);
                text_LessTimeText.text = TimeUtils.Instance.ToMinuteSecond(bingoScore.LessTime);

                PenaltyCountText.text = GameUtils.TocommaStyle(bingoScore.PenaltyScore);
                
                var bestToady = Root.Instance.DailyMaxScore;

                new_1.SetActive(Root.Instance.IsDailyScoreHigher);

                text_BestTodayValue.text = GameUtils.TocommaStyle(Math.Max(bestToady, totalScore)).ToString();

                var bestHistory = Root.Instance.HistoryMaxScore;
                new_2.SetActive(Root.Instance.IsHistoryScoreHigher);

                text_BestAllTimeValue.text = GameUtils.TocommaStyle(Math.Max(bestHistory, totalScore)).ToString();
            }

            DOTween.To(() => SubMitBgMask.fillAmount,
                    x => SubMitBgMask.fillAmount = x,
                    1,
                    subMitGiveTime)
                .SetEase(Ease.Linear)
                .OnComplete(() => { SendEndRequest(matchId); });

            mybutton_SubMitButton.SetClick(() => { SendEndRequest(matchId); });
        }

        public override void InitVm()
        {
        }

        public override void InitBinds()
        {
        }

        public override void InitEvents()
        {
            AddEventListener(Proto.MATCH_INFO,
                (sender, args) =>
                {
                    if (args is ProtoEventArgs { Result: ProtoResult.Fail })
                    {
                        UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_get_match_info_error"));
                        UserInterfaceSystem.That.CloseAllUI(new[] { nameof(UIMain) });
                    }
                });

            AddEventListener(GlobalEvent.SYNC_SERVER_MAINTAIN, (sender, eventArgs) =>
            {
                if (Root.Instance.ServerMaintainInfo.InTime)
                {
                    UserInterfaceSystem.That.Reset();
                    UserInterfaceSystem.That.ShowUI<UILogin>(LoginPanel.ServerMaintain);
                }
            });
        }

        private bool IsFakeGame()
        {
            var table = GetTable();
            return table?["isFake"] is true;
        }
        
        private void SendEndRequest(string matchId)
        {
            if (IsFakeGame())
            {
                return;
            }
            MediatorRequest.Instance.MatchInfo(matchId, Close, true);
        }
    }
}