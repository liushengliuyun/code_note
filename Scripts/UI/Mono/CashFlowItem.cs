using System;
using System.Collections.Generic;
using DataAccess.Model;
using HT.InfiniteList;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Mono
{
    public class CashFlowItem : InfiniteListElement
    {
        [SerializeField] private Text txtType;
        [SerializeField] private Text txtDate;
        [SerializeField] private Text txtAmountGreen;
        [SerializeField] private Text txtAmountRed;
        
        [SerializeField] private GameObject overLengthText;
        [SerializeField] private GameObject line;
        
        private Dictionary<int, string> _flowType = new Dictionary<int, string>();
        
        //元素属于的无限列表对象
        private InfiniteListScrollRect _scrollRect;

        private CashFlow _data;
        
        public override void OnUpdateData(InfiniteListScrollRect scrollRect, InfiniteListData data)
        {
            base.OnUpdateData(scrollRect, data);

            _scrollRect = scrollRect;
            _data = data as CashFlow;

            if (_data.IsOverLengthSign)
            {
                OnClearData();
                overLengthText.SetActive(true);
                line.SetActive(false);
                return;
            }

            if (_flowType.Count == 0)
            {
                _flowType[100] = "Withdraw";
                _flowType[101] = "Notice";
                _flowType[102] = "Free Gift";
                _flowType[103] = "Daily Rewards";
                _flowType[104] = "Invited";
                _flowType[105] = "Ad Bonus";
                _flowType[106] = "Shop";
                _flowType[107] = "Free Gift";
                _flowType[108] = "Entry Fee";
                _flowType[109] = "Win";
                _flowType[110] = "";
                _flowType[111] = "";
                _flowType[112] = "";
                _flowType[113] = "";
                _flowType[114] = "";
                _flowType[115] = "Fortune Wheel";
                _flowType[116] = "Free Bonus Game";
                _flowType[117] = "Deposit";
                _flowType[118] = "Deposit";
                _flowType[119] = "Deposit";
                _flowType[120] = "Deposit";
                _flowType[121] = "Deposit";
                _flowType[122] = "Deposit";
                _flowType[123] = "Fortune Wheel";
                _flowType[124] = "Fortune Wheel";
                _flowType[125] = "Fortune Wheel";
                _flowType[126] = "Timing Bonus";
                _flowType[127] = "Quest";
                _flowType[128] = "New Player Rewards";
                _flowType[129] = "Daily Gift";
                _flowType[130] = "Piggy Bank";
                _flowType[131] = "Deposit";
                _flowType[132] = "Withdraw Fail";
                _flowType[133] = "Daily Mission";
                _flowType[134] = "Infinite Grail";
                _flowType[135] = "Museum";
                _flowType[136] = "Friends Duel";
                _flowType[137] = "Wizard Treasure";
                _flowType[138] = "Lucky You";
                _flowType[139] = "Special Offer";
                _flowType[140] = "Add Charge";
                //提现退款
                _flowType[141] = "Cash Refund";
                //取消提现
                _flowType[142] = "Cancel Cash";
            }

            if (_data != null)
            {
                txtDate.text = _data.created_at;
                txtType.text = _flowType[_data.type];

                float moneyChange = _data.money.ToFloat();
                float bonusChange = _data.bonus.ToFloat();
                float amount = moneyChange + bonusChange;


                if (amount < 0)
                {
                    txtAmountRed.text = "-$" + YZNumberUtil.FormatYZMoney((amount * -1.0f).ToString());
                    txtAmountRed.SetActive(true);
                    txtAmountGreen.SetActive(false);
                }
                else
                {
                    txtAmountGreen.text = "+$" + YZNumberUtil.FormatYZMoney(amount.ToString());
                    txtAmountGreen.SetActive(true);
                    txtAmountRed.SetActive(false);
                }
            }
        }

        public override void OnClearData()
        {
            base.OnClearData();
            
            txtDate.text = "";
            txtType.text = "";
            txtAmountRed.SetActive(false);
            txtAmountGreen.SetActive(false);
            
            overLengthText.SetActive(false);
            line.SetActive(true);

            _scrollRect = null;
            _data = null;
        }
        
    }
}