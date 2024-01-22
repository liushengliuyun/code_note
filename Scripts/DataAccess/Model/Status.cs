namespace DataAccess.Model
{
    public enum Status
    {
        Match_Begin = 1,
        Match_End,
        Game_Begin,

        /// <summary>
        /// 等待对手出分或超时
        /// </summary>
        Game_End = 4,
        
        /// <summary>
        /// 所有人出分或者超时
        /// </summary>
        CanClime = 5,

        /// <summary>
        /// 最终流程
        /// </summary>
        Claimed
    }

    public enum BingoCloseType
    {
        /// <summary>
        /// 玩家自己提前结束
        /// </summary>
        EARLY_END = 1,

        /// <summary>
        ///倒计时完了
        /// </summary>
        COUNTDOWN_END,

        /// <summary>
        /// 填完所有的结束
        /// </summary>
        NORMAL_END
    }

    public enum BingoGuideType
    {
        /// <summary>
        /// 没有引导
        /// </summary>
        None,

        /// <summary>
        /// 已经在局内
        /// </summary>
        Teaching
    }
}