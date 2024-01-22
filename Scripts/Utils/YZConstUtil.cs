using DataAccess.Model;

namespace Utils
{
    public delegate void PaymentCallBack(int code, string response);
    public delegate void WebViewCallBack(string msg);
    
    public static class YZConstUtil
    {
        // integer
        public static string YZServerType => Root.Instance.Role.user_id + "YZServerType";
        public static string YZFirstOpenDeposit => Root.Instance.Role.user_id + "FirstOpenDeposit";                // 第一次打开充值界面
        public static string YZDeposit18YearOldInt => Root.Instance.Role.user_id + "Deposit18YearOld";             //-- 是否18岁
        public static string YZOrganic => Root.Instance.Role.user_id + "YZOrganic";
        public static string YZMediaSource => Root.Instance.Role.user_id + "YZMediaSource";
        
        public static string YZNotifyWindowPopDay => "YZNotifyWindowPopDay";

        public static string YZInducePopDay => Root.Instance.Role.user_id + "YZInducePopDay";
        public static string YZInducePopCount => Root.Instance.Role.user_id + "YZInducePopCount";
        
        public static string YZWithdrawMailVerified => Root.Instance.Role.user_id + "YZWithdrawMailVerified";   //-- 提现邮箱已验证
        public static string YZWithdrawNameAndAddressVerified => Root.Instance.Role.user_id + "YZWithdrawNameAndAddressVerified"; // --提现地址姓名已验证

        public static string YZDailyMissionHaveReward => Root.Instance.Role.user_id + "YZDailyMissionHaveReward";

        public static string YZInducePopTime => Root.Instance.Role.user_id + "YZInducePopTime";

        public static string YZDuelShowDay => Root.Instance.Role.user_id + "YZDuelShowDay";
        public static string YZDuelShowCount => Root.Instance.Role.user_id + "YZDuelShowCount";
        public static string YZDuelShowTime => Root.Instance.Role.user_id + "YZDuelShowTime";

        public static string YZLastDuelRoomNo => Root.Instance.Role.user_id + "YZLastDuelRoomNo";

        public static string YZBuyWeekCardNum => Root.Instance.Role.user_id + "YZBuyWeekCardNum";
        
        public static string YZEverNotOrganic => Root.Instance.Role.user_id + "YZEverNotOrganic";

        public static string YZDailyFirstWinMatchId => Root.Instance.Role.user_id + "YZDailyFirstWinMatchId";
        
        public static string YZDeniedNotifyPermission =>  Root.Instance.Role.user_id + "YZDeniedNotifyPermission";
        
        // string
        public static string YZDepositCreditCardDetailsStr => Root.Instance.Role.user_id + "DepositCreditCardDetails"; //-- 充值信用卡信息
        public static string YZDepositCharegeTypeStr => Root.Instance.Role.user_id + "DepositCharegeTypeStr";          //-- 上一次充值的渠道类型
        public static string YZGpsDebugInfo => Root.Instance.Role.user_id + "GpsDebugInfo";                            //-- GPS Debug 上传信息
        public static string YZIsLastDepositSuccess => Root.Instance.Role.user_id + "IsLastDepositSuccess";            //-- 上次支付成功
        public static string YZWithdrawEmail => Root.Instance.Role.user_id + "YZWithdrawEmail";                        //-- 提现邮箱
        public static string YZDailyMissionBalance => Root.Instance.Role.user_id + "YZDailyMissionBalance";
        
        public static string YZWithdrawFirstName => Root.Instance.Role.user_id + "YZWithdrawFirstName";
        public static string YZWithdrawLastName => Root.Instance.Role.user_id + "YZWithdrawLastName";
        
        public static string YZCountryCode =  "YZCountryCode";
        public static string YZAreaCode = "YZAreaCode";
        public static string YZCity = "YZCity";
    }
    
    /// <summary>
    /// 充值渠道类型
    /// </summary>
    public static class ChargeChannelType
    {
        
        public const string ApplePay = "apple_pay";
        public const string PayPal = "paypal";
        public const string CreditCard = "credit_card";
    }
    
    public static class BestABType
    {
        public const int A = 1;
        public const int B = 2;
        public const int X = 3;
    }
    
    public class ServerType
    {
        public const int Test = 1;
        public const int Backup = 2;
        public const int Release = 3;
    }
}