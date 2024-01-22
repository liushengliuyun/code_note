namespace DataAccess.Utils.Static
{
    public static class WithDrawState
    {
        /// <summary>
        /// 发起提现
        /// </summary>
        public const int Initiating_withdrawal = 1;

        /// <summary>
        /// 审核成功
        /// </summary>
        public const int Successful_audit = 103;


        /// <summary>
        ///  已提交到hyper服
        /// </summary>
        public const int Successful_hyper = 106;

        /// <summary>
        ///  hyper服审核通过
        /// </summary>
        public const int Successful_hyper_success = 113;

        /// <summary>
        /// 取消
        /// </summary>
        public const int Cancel = 101;
        
        /// <summary>
        /// 提交到hyper服失败
        /// </summary>
        public const int fail_hyper = 104;
        
        
        /// <summary>
        /// 争议冻结
        /// </summary>
        public const int Dispute_freeze = 114;
        
        /// <summary>
        /// 已提交到第三方打款
        /// </summary>
        public const int submitted_to_a_third_party = 116;
        
        /// <summary>
        /// 提现成功（充值退款的形式）
        /// </summary>
        public const int Successful_withdrawal = 200;
        

        public static bool IsInProgress(int state)
        {
            return state is Initiating_withdrawal
                or Successful_audit
                or Successful_hyper
                or Successful_hyper_success;
        }
    }
}