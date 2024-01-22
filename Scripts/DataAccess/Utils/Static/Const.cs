namespace DataAccess.Utils.Static
{
    public class Const
    {
        #region 局内道具

        /// <summary>
        /// 双倍时间
        /// </summary>
        public const int DoubleScore = 1;

        /// <summary>
        /// 多选1
        /// </summary>
        public const int ChooseOne = 2;

        /// <summary>
        /// 任选
        /// </summary>
        public const int ChooseAny = 3;

        /// <summary>
        /// 十字消
        /// </summary>
        public const int Cross = 4;
        
        #endregion


        #region 道具

        /// <summary>
        /// 美金
        /// </summary>
        public const int Cash = 1;

        /// <summary>
        /// 钻石
        /// </summary>
        public const int Chips = 2;

        /// <summary>
        /// 
        /// </summary>
        public const int Bonus = 3;

        /// <summary>
        /// ？？？
        /// </summary>
        public const int Coin = 4;

        /// <summary>
        /// 再转一次
        /// </summary>
        public const int OneMoreTime = 800;

        #endregion
        
        
        /// <summary>
        /// 小额美金转盘的chargeId
        /// </summary>
        public const int PAY_SMALL = 25;
        
        public const int PAY_BIG = 26;
        
        /// <summary>
        /// 免费转盘门票消耗
        /// </summary>
        public const int FREE_WHEEL_COST = 5;
        
        
        
  /// <summary>
  /// 拒绝定位授权
  /// </summary>
        public const int LocateReject = 1;
        
        /// <summary>
        /// 同意定位权限
        /// </summary>
        public const int LocateAllow = 2;
        
        /// <summary>
        /// 成功获取gps信息
        /// </summary>
        public const string FakeGetGPSFlag = "Limit";
    }
}