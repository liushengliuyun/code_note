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
using CatLib;
using CatLib.EventDispatcher;
using Core.Extensions.UnityComponent;
using Core.Services.ResourceService.API.Facade;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
//using UIWidgets.WidgetGeneration;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace UI
{
    public class MatchHistoryItem : MonoBehaviour
    {
        #region UI Variable Statement

        [SerializeField] private Transform NotReward;
        [SerializeField] private Transform HaveReward;

        [SerializeField] private Text GotText;

        [SerializeField] private MyButton ItemBtn;

        [SerializeField] private Transform EnterFee;

        [SerializeField] private Text FreeEnterText;

        [SerializeField] private Text RoomNumText;

        [SerializeField] private Text RoomName;

        [SerializeField] private MyText TimeText;

        [SerializeField] private MyButton CollectBtn;

        /// <summary>
        /// 门票类型
        /// </summary>
        [SerializeField] private Image EnterFeeIcon;

        /// <summary>
        /// 门票数量
        /// </summary>
        [SerializeField] private Text EnterFeeText;

        [SerializeField] private Text ScoreText;

        [SerializeField] private Transform InProgress;

        [SerializeField] private Transform MatchTimeOut;

        [SerializeField] private Transform HaveResult;

        [FormerlySerializedAs("Rank1")] [SerializeField]
        private Transform RankIconTransform;

        [SerializeField] private Text CashText;

        /// <summary>
        /// 除了bonus奖励外的其他奖励
        /// </summary>
        [SerializeField] private Transform RewardOther;

        #endregion

        private void Reset()
        {
            HaveResult.SetActive(false);
            InProgress.SetActive(false);
            MatchTimeOut.SetActive(false);
            RankIconTransform.SetActive(false);
            FreeEnterText.SetActive(false);
            EnterFee.SetActive(false);
            CashText.SetActive(false);
            RewardOther.SetActive(false);
        }

        public void Init(MatchHistory history)
        {
            Reset();
            var room = Root.Instance.GetRoomById(history.room_id);
            var matchId = history.match_id;

            var costItem = room?.GetInItem();
            TimeText.text = I18N.Get("TIME_PASS", TimeUtils.Instance.PassTimeFormat(history.begin_time));

            RoomName.text = history.room_name;
            RoomNumText.text = room == null ? "2" : room.seat.ToString();

            if (costItem != null && room is { IsLuckyRoom: false})
            {
                EnterFee.SetActive(true);
                EnterFeeIcon.sprite = costItem.GetIcon();
                EnterFeeText.text = GameUtils.TocommaStyle(costItem.Count);
            }
            else
            {
                FreeEnterText.SetActive(true);
            }

            ScoreText.text = I18N.Get("key_show_score", GameUtils.TocommaStyle(history.game_score));
            if (history.IsWait)
            {
                InProgress.SetActive(true);
            }
            else
            {
                HaveResult.SetActive(true);

                RankIconTransform.SetActive(true);
                var image = RankIconTransform.GetComponent<Image>();
                var historyWinResult = Math.Clamp(history.win_result, 1, 5) ;
                image.sprite =
                    ResourceSystem.That.LoadAssetSync<Sprite>($"uimain/rank{historyWinResult}");
 
                image.SetNativeSize();
                HaveReward.SetActive(history.HavaReward);
                NotReward.SetActive(!history.HavaReward);
                //有奖励
                if (history.HavaReward)
                {
                    CollectBtn.SetActive(!history.IsClaimed);
                    GotText.SetActive(history.IsClaimed);
                    if (history.rewards != null)
                    {
                        foreach (var reward in history.rewards)
                        {
                            if (reward.Key is Const.Bonus or Const.Cash)
                            {
                                CashText.SetActive(true);
                                CashText.text = I18N.Get("key_money_code") + reward.Value;
                            }
                            else
                            {
                                RewardOther.SetActive(true);
                                RewardOther.Find("icon").GetComponent<Image>().sprite =
                                    MediatorItem.Instance.GetItemSprite(reward.Key);
                                RewardOther.GetComponentInChildren<Text>().text = GameUtils.TocommaStyle(reward.Value);
                            }
                        }
                    }
                }

                if (history.sub_status == "game_expire")
                {
                    ScoreText.text = I18N.Get("key_game_over_time");
                }
            }

            ItemBtn.SetClick(() => MediatorRequest.Instance.MatchInfo(matchId));

            CollectBtn.SetClick(() =>
            {
                if (room is {IsLuckyRoom: true} && Root.Instance.LuckyGuyInfo is {IsChargeSuccess: false })
                {
                    EventDispatcher.Root.Raise(GlobalEvent.LUCKY_GUY_FAKE_NEWS, matchId);
                }
                else
                {
                    MediatorRequest.Instance.MatchClaim(matchId);
                }
            });
        }
    }
}