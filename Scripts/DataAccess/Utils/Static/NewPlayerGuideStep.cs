namespace DataAccess.Utils.Static
{
    public enum NewPlayerGuideStep
    {
        /// <summary>
        /// 玩家名称上传
        /// </summary>
        BACKGROUND_STORY = 10,

        /// <summary>
        /// 加新手引导资源
        /// </summary>
        BEFORE_ENTER_ROOM = 35,

        /// <summary>
        /// 第一局游戏
        /// </summary>
        FIRST_ROOM_GAME = 60,
        
        /// <summary>
        /// 第一局游戏内的引导
        /// </summary>
        FIRST_ROOM_GUIDE_FINISH = 61,
        
        /// <summary>
        /// 第二局游戏的引导
        /// </summary>
        SECOND_BONUS_GAME = 65,
        
    }
    
    /// <summary>
    /// 触发式引导
    /// </summary>
    public enum TriggerGuideStep
    {
        /// <summary>
        ///  玩家获得点数并收集满首个藏品的进度时
        /// </summary>
        MUSEUM_GUIDE_1 = 1,
        
        /// <summary>
        /// 
        /// </summary>
        MUSEUM_GUIDE_2 = 2,
        
        
        MUSEUM_GUIDE_3 = 3,
        
        
        Lucky_Guy_Played_Effect = 4,
    }
}