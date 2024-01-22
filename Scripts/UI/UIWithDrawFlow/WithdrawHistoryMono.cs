using Core.Third.I18N;
using DataAccess.Utils.Static;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.UIWithDrawFlow
{
    public class WithdrawHistoryMono : MonoBehaviour
    {
        [SerializeField] private Text brtxtemail;
        [SerializeField] private Text brtxtdate;
        [SerializeField] private Text brtxtstate;
        [SerializeField] private Text brtxtamount;
        [SerializeField] private Text brtxttips;


        void OnStart()
        {
            
        }
        
        public void SetContent(WithdrawHistoryData item)
        {
            brtxtemail.text = item.address;
            brtxtdate.text = item.created_at;

            int status = item.status;
            brtxttips.text = "";
            if (status is 1 or 2)
            {
                brtxtstate.color = new Color(1, 127 / 255.0f, 0, 1);
                brtxtstate.text = I18N.Get("key_pending");
            }
            else if (status is 3 or WithDrawState.Successful_withdrawal)
            {
                brtxtstate.color = new Color(179 / 255.0f, 138 / 255.0f, 118 / 255.0f, 1);
                brtxtstate.text = I18N.Get("key_succeed");
            }
            else if (status is 4 or WithDrawState.fail_hyper or WithDrawState.Dispute_freeze)
            {
                brtxtstate.color = new Color(229 / 255.0f, 23 / 255.0f, 50 / 255.0f, 1);
                brtxtstate.text = I18N.Get("key_failed");
            }
            else if (status == 5)
            {
                brtxtstate.color = new Color(179 / 255.0f, 138 / 255.0f, 118 / 255.0f, 1);
                brtxtstate.text = I18N.Get("key_refunded");
                brtxttips.text = I18N.Get("key_refunded_des");
            }
            else if (status == WithDrawState.Cancel)
            {
                //缺  颜色
                brtxtstate.color = new Color(229 / 255.0f, 23 / 255.0f, 118 / 255.0f, 1);
                brtxtstate.text = I18N.Get("key_cancel");
                brtxttips.text = I18N.Get("");
            }
            else
            {
                brtxtstate.color = new Color(1, 127 / 255.0f, 0, 1);
                brtxtstate.text = I18N.Get("key_pending");
            }

            brtxtamount.text = YZString.Concat(I18N.Get("key_money_code"), item.freeze_amount.ToString("0.00"));
        }
    }
}