using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.Internal;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils.Static;
using UnityEngine;

namespace UI
{
    public class UIWithdrawCancel : UIBase<UIWithdrawCancel>
    {

     [SerializeField]   private MyText ReturnText;
     [SerializeField]   private MyButton ComfirmBtn;
     [SerializeField]   private MyButton CancelBtn;
     [SerializeField]   private MyButton CloseBtn;
        
        
        public override void InitEvents()
        {
            AddEventListener(ProtoError.WithDrawError.ToString(), (sender, args) =>
            {
                Close();
            });
        }

        public override void OnStart()
        {
            CloseBtn.SetClick(() => { MediatorRequest.Instance.WithdrawHistory(false, OnCloseBtnClick); });

            CancelBtn.SetClick(() => { MediatorRequest.Instance.WithdrawHistory(false, OnCloseBtnClick); });
            
            ComfirmBtn.SetClick(OnComfirmBtnClick);

            float all = 0;

            foreach (var data in Root.Instance.WithdrawHistoryData)
            {
                if (data.InProgress())
                {
                    all += data.freeze_amount;
                }
            }

            ReturnText.text = I18N.Get("key_money_count", all);
        }

        private void OnComfirmBtnClick()
        {
            MediatorRequest.Instance.CancelWithdraw(Close);
        }

        public override void InitVm()
        {
            
        }

        public override void InitBinds()
        {
            
        }
    }
}