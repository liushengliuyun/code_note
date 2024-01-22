namespace DataAccess.Utils.Static
{
    public enum IPAddressType
    {
        IPv4,
        IPv6,
    }

    public enum ProtoResult
    {
        Success,
        /// <summary>
        /// 通信成功, 但服务器没通过
        /// </summary>
        Fail,
        /// <summary>
        /// 未通信成功
        /// </summary>
        NotConnect
    }

    public enum WheelType
    {
        Free = 1,

        /// <summary>
        /// 小额美金转盘
        /// </summary>
        PaySmall,

        /// <summary>
        /// 大额美金转盘
        /// </summary>
        PayBig
    }
    
    public enum ActivityEnterType
    {
        Login, 
        
        Click,
     
        AfterGame,
        /// <summary>
        /// 生涯前10局
        /// </summary>
        FirstTenGame,
        /// <summary>
        /// 跳转时随机弹出
        /// </summary>
        Random,
        Refresh,
        /// <summary>
        /// 特殊触发
        /// </summary>
        Trigger
    }
    
    public static class GlobalEnum
    {
        public static bool IsAutoPop(this ActivityEnterType activityEnterType)
        {
            return activityEnterType is not ActivityEnterType.Click;
        }

        public static class HttpRequestType
        {
            public static string Get = "Get";
            public static string POST = "Post";
            public static string HEAD = "HEAD";
        }

        public const string CHINESE = "zh";
        public const string ENGLISH = "en";
        public const string JSON_SUCCESS = "success";
        public const string JSON_FAIL = "fail";
        
        public const float ONE_GAME_TIME = 90f;

        public const int MAGIC_BALL_STEP = 4;

#if DAI_TEST
        public const int Lucky_guy_show_interval = 10;
#else
       public const int Lucky_guy_show_interval = 120;
#endif

        /// <summary>
        /// 最小分贝
        /// </summary>
        public const int MinPreSliderValue = -99;

        /// <summary>
        /// 音效修正
        /// </summary>
        public const int SliderValueOffset = 99;

        /// <summary>
        /// 总共暂停时间
        /// </summary>
        public const int TOTAL_PAUSE_TIME = 3600;

        public const string APP_VERSION = "en";

        public const int DefaultImageId = 1;

        #region -------------------缓存的key ----------------------

        /// <summary>
        /// 在商店页面弹出活动的时间
        /// </summary>
        public const string PopActivityStore = "PopActivityStore";
        
        /// <summary>
        /// 存在数据库里面的名字
        /// </summary>
        public const string DB_YATZY = "YATZY";

        /// <summary>
        /// 支付门票后， 未开始就退出的MatchID
        /// </summary>
        public const string LAST_MATCH_BEGIN_ID = "LAST_MATCH_BEGIN_ID";

        /// <summary>
        /// 上局游戏的MatchID
        /// </summary>
        public const string LAST_GAME_ID = "LAST_GAME_ID";
        
        /// <summary>
        /// 支付门票， 匹配开始的时间
        /// </summary>
        public const string LAST_MATCH_BEGIN_TIME = "LAST_MATCH_BEGIN_TIME";


        /// <summary>
        /// 支付门票后， 未开始就退出的RoomID
        /// </summary>
        public const string LAST_MATCH_ROOM_ID = "LAST_MATCH_ROOM_ID";

        public const string AUTHORIZATION_DEBUG = "AUTHORIZATION_DEBUG";

        public const string AUTHORIZATION_RELAESE = "AUTHORIZATION_RELAESE";

        public const string LAST_LOGIN_TIME = "LAST_LOGIN_TIME";

        /// <summary>
        /// 测试用， 是否强制刷新为非自然量
        /// </summary>
        public const string SET_NOT_NATURAL = "SET_NOT_NATURAL";
        
        /// <summary>
        /// 首充活动开始的时间
        /// </summary>
        public const string START_PACKER_BEGIN_TIME = "START_PACKER_BEGIN_TIME";
        
        /// <summary>
        /// 缓存的设备ID, 用于客户端登陆
        /// </summary>
        public const string ClientUID = "UID";

        public const string PUSH_ALARM = "PUSH_ALARM";

        /// <summary>
        /// 是否开启了震动
        /// </summary>
        public const string VIBRATION = "VIBRATION";
        
        public const string IP = "IP";
        public const string LastIP = "LastIP";
        public const string UserAgent = "UserAgent";
        public const string ClickHowToPlay = "ClickHowToPlay";

        public const string MUSIC_VOLUME = "MUSIC_VOLUME";

        public const string SOUND_VOLUME = "SOUND_VOLUME";
        
        /// <summary>
        /// 震动强度 时长
        /// </summary>
        public const string VIBRATION_VOLUME = "SOUND_VOLUME";
        
        public const string MUTE_MUSIC = "MUTE_MUSIC";

        public const string MUTE_SOUND = "MUTE_SOUND";

        public const string IS_FIRST_PAY = "IS_FIRST_PAY";


        /// <summary>
        /// 上一次转转盘的时间
        /// </summary>
        public const string POPED_PAY_WHEEL_TIME = "POPED_PAY_WHEEL_TIME";

        
        /// <summary>
        /// 记录上一次转动的转盘
        /// </summary>
        public const string LAST_WHEEL_TYPE = "LAST_WHEEL_TYPE";
        
        /// <summary>
        /// 是否第一次使用双倍道具
        /// </summary>
        public const string IS_FIRST_USE_DOUBLE_PROP = "IS_FIRST_USE_DOUBLE_PROP";


        /// <summary>
        /// 是否第一次使用四选一道具
        /// </summary>
        public const string IS_FIRST_USE_4X1 = "IS_FIRST_USE_4X1";


        /// <summary>
        /// 是否第一次使用直选道具
        /// </summary>
        public const string IS_FIRST_USE_6X1 = "IS_FIRST_USE_6X1";
        
        public const string IS_FIRST_USE_CROSS = "IS_FIRST_USE_CROSS";

        #endregion -------------------缓存的key  End ----------------------
    }


    public class EmailPos
    {
        public const string Charge = "Charge";
        public const string Rate = "Rate";
        public const string Setting = "Setting";
        public const string Loading = "Loading";
        public const string Frozen = "Frozen";
        public const string Code9997 = "9997";
        public const string Code9998 = "9998";
        public const string Withdraw = "Withdraw";
        public const string ChargePlay = "ChargePlay";
        public const string KYC = "KYC";
        public const string FAQ = "FAQ";
    }
}