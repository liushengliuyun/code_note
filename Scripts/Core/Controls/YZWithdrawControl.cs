using Core.Manager;
using Core.Server;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Third.I18N;
using DataAccess.Model;
using DataAccess.Utils.Static;
using UI;
using UI.UIWithDrawFlow;
using Utils;

public class YZWithdrawControl
{
    public static bool IsShowKYC = false;
    public static bool YZCheckCanOpen()
    {
        // 冻结的账号
        if (Root.Instance.Role.IsFreeze)
        {
            UserInterfaceSystem.That.ShowUI<UIConfirm>(new UIConfirmData()
            {
                Type = UIConfirmData.UIConfirmType.OneBtn,
                HideCloseBtn = false,
                desc = I18N.Get("key_unusual_activity"),
                confirmTitle = I18N.Get("key_contact_us"),
                WaitCloseCallback = true,
                confirmCall = () =>
                {
                    YZNativeUtil.ContactYZUS(EmailPos.Charge);
                },
                cancleCall = () =>
                {
                    UserInterfaceSystem.That.RemoveUIByName(nameof(UIConfirm));
                }
            });
            YZDebug.Log("账号冻结");
            return false;
        }
        
        if (Root.Instance.Role.NotBindLoginEmail)
        {
            UserInterfaceSystem.That.ShowUI<UIMailLogin>(MailLoginPanel.Bind, null, true);
            return false;
        }
        return true;
    }

    public static void YZOnOpenDetail()
    {
        // if (ABGroupManager.Shared.YZIsBCGroup(ABTagName.organic_0221) && !YZServerApiOrganic.Shared.IsYZShowMoney())
        // {
        //     YZWithdrawRedeemCardUICtrler.Shared().YZOnPushUI();
        // }
        // else if (YZCheckCanOpen())
        // {
        //     YZWithdrawDetailsUICtrler.Shared().YZOnPushUI();
        // }
        
        
        if (YZCheckCanOpen())
        {
            UserInterfaceSystem.That.ShowUI<UIWithdrawDetails>();    
        }
    }

    public static void YZOnOpenRecord()
    {
        // if (
        //     //ABGroupManager.Shared.YZIsBCGroup(ABTagName.organic_0221) && 
        //     !YZServerApiOrganic.Shared.IsYZShowMoney())
        // {
        //     YZWithdrawCashRecordUICtrler.Shared().YZOnPushUI();
        // }
        // else
        // {
        //     if (YZCheckCanOpen())
        //     {
        //         YZWithdrawCashRecordUICtrler.Shared().YZOnPushUI();
        //     }
        // }
    }

    public static void YZOnCloseAllUI()
    {
        // YZWithdrawDetailsUICtrler.Shared().YZOnCloseUI();
        // YZWithdrawCashAddressUICtrler.Shared().YZOnCloseUI();
        // YZWithdrawCashEmailUICtrler.Shared().YZOnCloseUI();
        // YZWithdrawCashNameUICtrler.Shared().YZOnCloseUI();
        // YZWithdrawCashRecordUICtrler.Shared().YZOnCloseUI();
        // YZWithdrawCashVerifyEmailUICtrler.Shared().YZOnCloseUI();
        // YZWithdrawCashVerifyFailedUICtrler.Shared().YZOnCloseUI();
        // YZWithdrawCashVerifySuccUICtrler.Shared().YZOnCloseUI();
        // YZWithdrawConfirmUICtrler.Shared().YZOnCloseUI();
        // YZWithdrawInputPhoneNumberUICtrler.Shared().YZOnCloseUI();
        // YZWithdrawVerifyPhoneCodeUICtrler.Shared().YZOnCloseUI();
        // YZWithdrawRedeemCardUICtrler.Shared().YZOnCloseUI();
    }

    // public static void YZOnHyperWallet()
    // {
    //     if (ABGroupManager.Shared.YZIsBCGroup(ABTagName.organic_0221) && !YZServerApiOrganic.Shared.IsYZShowMoney())
    //     {
    //         int status = PlayerManager.Shared.GetYZHyperWalletStatus();
    //         if (status == 0)
    //         {
    //             YZWithdrawCashEmailUICtrler.Shared().YZOnPushUI();
    //         }
    //         else if (status == 1)
    //         {
    //             YZWithdrawCashVerifyEmailUICtrler.Shared().YZOnPushUI(PlayerManager.Shared.Player.Other.hyper_wallet.email);
    //         }
    //         else if (status == 2)
    //         {
    //             YZWithdrawCashNameUICtrler.Shared().YZOnPushUI();
    //         }
    //         else if (!string.IsNullOrEmpty(YZWithdrawRedeemCardUICtrler.Shared().YZCashID))
    //         {
    //             YZServerApiWithdraw.Shared.YZHyperWithdraw(string.Empty, YZWithdrawRedeemCardUICtrler.Shared().YZCashID);
    //         }
    //     }
    //     else
    //     {
    //         int status = PlayerManager.Shared.GetYZHyperWalletStatus();
    //         if (status == 0)
    //         {
    //             YZWithdrawCashEmailUICtrler.Shared().YZOnPushUI();
    //         }
    //         else if (status == 1)
    //         {
    //             YZWithdrawCashVerifyEmailUICtrler.Shared().YZOnPushUI(PlayerManager.Shared.Player.Other.hyper_wallet.email);
    //         }
    //         else if (status == 2)
    //         {
    //             YZWithdrawCashNameUICtrler.Shared().YZOnPushUI();
    //         }
    //         else if (!string.IsNullOrEmpty(YZWithdrawDetailsUICtrler.Shared().YZInputAmount))
    //         {
    //             YZServerApiWithdraw.Shared.YZHyperWithdraw(YZWithdrawDetailsUICtrler.Shared().YZInputAmount, string.Empty);
    //         }
    //     }
    // }
}
