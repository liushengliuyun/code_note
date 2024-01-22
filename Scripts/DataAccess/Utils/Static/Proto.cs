using System;
using Core.Services.PersistService.API.Facade;

namespace DataAccess.Utils.Static
{
    public class ProtoEventArgs : EventArgs
    {
        public readonly ProtoResult Result;

        public ProtoEventArgs(ProtoResult result)
        {
            Result = result;
        }
    }

    public enum ProtoError
    {
        /// <summary>
        /// 很多错误都会返回这个错误码
        /// </summary>
        Common = 1000,

        /// <summary>
        /// Token验证不通过
        /// </summary>
        TokenFail = 1100,

        /// <summary>
        /// 邮箱被其他人绑定了
        /// </summary>
        MailBindByOther = 1106,

        /// <summary>
        /// 邮箱格式不正确
        /// </summary>
        MailFormat = 1110,

        /// <summary>
        /// 邮箱不存在
        /// </summary>
        MailNotExizt = 1108,

        /// <summary>
        /// 邮箱被绑定
        /// </summary>
        MailBinded = 1109,

        /// <summary>
        /// 邮箱登陆失败
        /// </summary>
        MailLoginFail = 1111,

        /// <summary>
        /// 用户已删除
        /// </summary>
        PlayerDeleted = 1114,
        
        /// <summary>
        /// 服务器内部错误， 比如冻结后删号
        /// </summary>
        EmailBindServerError = 1113,
        
        /// <summary>
        ///  用户被封停
        /// </summary>
        PlayerLock = 1200,
        
        /// <summary>
        /// 不存在提现订单, 取消提现时间未到3天
        /// </summary>
        WithDrawError = 10001,
        
        /// <summary>
        /// 业务服存在状态为116的订单
        /// </summary>
        WithDrawError2 = 10002,
        
    }

    public static class Proto
    {
#if RELEASE
#if UNITY_IOS
        public static string SERVER_URL = "https://app-ios.apexdynamics.club/";
#else
        public static string SERVER_URL = "https://app-gp.superdrawl.club/";
#endif
#else
        public static string SERVER_URL = "http://54.215.42.182:8090/";
#endif

        
        public const string geoNamesApiKey = "liushengliuyun";
        
        public const string geoNamesApiUrl = "http://api.geonames.org/addressJSON";

        public static void ClearToken()
        {
            var key = SERVER_URL.Contains("https")
                ? GlobalEnum.AUTHORIZATION_RELAESE
                : GlobalEnum.AUTHORIZATION_DEBUG;
            PersistSystem.That.DeletePrefsValue(key);
        }

        public static string VISITOR_LOGIN => SERVER_URL + "api/user/guest";

        public static string PLAYER_LOGIN => SERVER_URL + "api/user/auto_login";
        public static string DELETE_ACCOUNT => SERVER_URL + "api/user/delete_self";

        /// <summary>
        /// 跨天登陆
        /// </summary>
        public static string PASSDAY_LOGIN => SERVER_URL + "api/user/auto_login_passday";

        public static string GET_CONFIGS => SERVER_URL + "api/configs";

        public static string MATCH_BEGIN => SERVER_URL + "api/match/begin";

        public static string MATCH_END => SERVER_URL + "api/match/end";

        public static string GAME_BEGIN => SERVER_URL + "api/game/begin";

        public static string GAME_END => SERVER_URL + "api/game/end";

        public static string MATCH_CLIAM => SERVER_URL + "api/match/claim";

        public static string MATCH_INFO => SERVER_URL + "api/match/info";

        public static string GET_INCOMPLETE_HISTORY => SERVER_URL + "api/match/incomplete_history";

        public static string GET_COMPLETE_HISTORY => SERVER_URL + "api/match/complete_history";

        public static string GET_ONLINE_REWARD_INFO => SERVER_URL + "api/online_active/info";

        public static string GET_ONLINE_REWARD_CLAIM => SERVER_URL + "api/online_active/claim";

        public static string ONLINE_CHARGE => SERVER_URL + "api/online_active/charge";

        public static string GET_VIP_INFO => SERVER_URL + "api/vip/info";

        public static string GET_FREE_WHEEL_INFO => SERVER_URL + "api/fortune_wheel/info";

        public static string FREE_WHEEL => SERVER_URL + "api/fortune_wheel/free_whirl";

        public static string PAY_WHEEL => SERVER_URL + "api/fortune_wheel/pay_whirl";

        public static string SIGN_INFO => SERVER_URL + "api/sign/info";

        public static string SIGN_SIGN => SERVER_URL + "api/sign/sign";

        public static string GET_TASK_INFO => SERVER_URL + "api/castle_task/info";

        public static string TASK_CHOOSE_LEVEL => SERVER_URL + "api/castle_task/choose";

        public static string GET_TASK_REWARD => SERVER_URL + "api/castle_task/claim";

        public static string GET_RANDOW_NAME => SERVER_URL + "api/user/rand_nickname";

        /// <summary>
        /// 上传玩家信息
        /// </summary>
        public static string USER_INFO_UPLOAD => SERVER_URL + "api/user/user_info_upload";

        /// <summary>
        /// 新手引导上报
        /// </summary>
        public static string NEW_PLAYER_GUIDE_SUB => SERVER_URL + "api/match/first_game_guide";
        
        /// <summary>
        /// 触发式引导上报
        /// </summary>
        public static string TRIGGER_GUIDE_SUB => SERVER_URL + "api/museum/guide";

        /// <summary>
        /// 看完了广告
        /// </summary>
        public static string WATCH_AD => SERVER_URL + "api/online_active/ad";

        /// <summary>
        /// 绑定邮箱
        /// </summary>
        public static string BIND_EMAIL => SERVER_URL + "api/login/bind_email";

         public static string BIND_VIP_INFO => SERVER_URL + "api/login/bind_v_email";
         
        /// <summary>
        /// 重发邮件
        /// </summary>
        public static string RESEND_EMAIL => SERVER_URL + "api/login/resend_email";

        /// <summary>
        /// 邮箱登陆
        /// </summary>
        public static string EMAIL_LOGIN => SERVER_URL + "api/user/email_login";

        /// <summary>
        /// 发送修改密码邮件
        /// </summary>
        public static string CHANGE_PASSWORD => SERVER_URL + "api/user/change_password";

        /// <summary>
        /// 拉取所有的notify
        /// </summary>
        public static string PUSH_NOTIFY => SERVER_URL + "api/message/list";

        /// <summary>
        /// 标记PUSH_NOTIFY 拉去的消息，下次拉的时候， 拉不到标记的
        /// </summary>
        public static string MARK_NORIFY => SERVER_URL + "api/message/mark";

        /// <summary>
        /// 充值相关
        /// </summary>
        public static string CREATE_CHARGE_ORDER => SERVER_URL + "api/charge/create";

        public static string GET_CHARGE_METHODS => SERVER_URL + "api/charge/methods";
        public static string CHARGE_ORDER_QUERY => SERVER_URL + "api/charge/query";

        /// <summary>
        /// 提现相关
        /// </summary>
        // public const string CASH_BIND_PHONE_START = "cash/bind_phone/start";
        // public const string CASH_BIND_PHONE_VERIFY = "cash/bind_phone/verify";
        // 绑定邮箱
        public static string HYPER_RIGISTER => SERVER_URL + "api/hyper/register";

        // 重发验证码
        public static string HyperResend => SERVER_URL + "api/hyper/resend";

        // 提交信息
        public static string HyperCommit => SERVER_URL + "api/hyper/commit/info";

        // 发起提现
        public static string HyperApply => SERVER_URL + "api/cash/hyper";

        // 提现记录
        public static string HyperHistory => SERVER_URL + "api/cash/history";

        // 提现流水
        public static string HyperCashFlow => SERVER_URL + "api/cash/flow";

        public static string DAILY_REWARD_CLAIM => SERVER_URL + "api/shop/daily_reward_claim";

        /// <summary>
        /// 数数相关
        /// </summary>
        public static string Thinking_Session_ID => SERVER_URL + "api/support/ta_udid";

        /// <summary>
        /// 获取商店信息
        /// </summary>
        public static string GET_SHOP_INFO => SERVER_URL + "api/shop/info";


        /// <summary>
        /// 上报新挡位
        /// </summary>
        public static string PUSH_NEW_STARTPACKER_GEAR => SERVER_URL + "api/starter_pack/change_level";


        public static string STARTER_PACK_INFO => SERVER_URL + "api/starter_pack/info";

        /// <summary>
        ///  幸运卡信息
        /// </summary>
        public static string LUCKY_CARD_INFO => SERVER_URL + "api/lucky_card/info";

        /// <summary>
        ///  幸运卡翻牌
        /// </summary>
        public static string LUCKY_CARD_CHOOSE => SERVER_URL + "api/lucky_card/choose";

        /// <summary>
        ///  一条龙信息
        /// </summary>
        public static string DRAGON_INFO => SERVER_URL + "api/one_stop/info";


        /// <summary>
        ///  领取一条龙奖励
        /// </summary>
        public static string DRAGON_CLAIM => SERVER_URL + "api/one_stop/claim";

        /// <summary>
        ///  特殊礼物信息
        /// </summary>
        public static string SPECIAL_GIFT => SERVER_URL + "api/special_gift/info";

        public static string SPECIAL_OFFER => SERVER_URL + "api/special_offer/info";

        /// <summary>
        ///  房间直冲信息 获取房间充值配置
        /// </summary>
        public static string GET_ROOM_CHARGE_INFO => SERVER_URL + "api/room_charge/charge_info";

        public static string SET_CHARGE_INFO_BEGIN_TIME => SERVER_URL + "api/room_charge/set_begin_time";
        /// <summary>
        /// 获取广告场信息
        /// </summary>
        public static string GET_AD_ROOM_INFO => SERVER_URL + "api/ad_room/info";

        /// <summary>
        /// 上报观看广告数据
        /// </summary>
        public static string AD_ROOM_WATCHED => SERVER_URL + "api/ad_room/watch_ads";
        
        /// <summary>
        ///  玩家流水
        /// </summary>
        public static string USER_CASH_FLOW => SERVER_URL + "api/user/cash_flow";
        
        // 上报AFID
        public static string SEND_AF_ID => SERVER_URL + "api/support/idfa_appsflyer_id";

        public static string SEND_MEDIA_SOURCE => SERVER_URL + "api/support/media_source";
        
        public static string MUSEUM_CLAIM => SERVER_URL + "api/museum/claim";

        public static string MUSUM_INFO => SERVER_URL + "api/museum/info";

        public static string MUSEUM_REFRESH => SERVER_URL + "api/museum/refresh";

        /// <summary>
        /// 获取周卡，月卡信息
        /// </summary>
        public static string MONTH_CARD_INFO => SERVER_URL + "api/infinite_grail/info";
        
        public static string MONTH_CARD_CLAIM => SERVER_URL + "api/infinite_grail/claim";
        
        /// <summary>
        /// 获取新周卡信息
        /// </summary>
        public static string WEEK_CARD_INFO => SERVER_URL + "api/infinite_week/info";
        
        public static string WEEK_CARD_CLAIM => SERVER_URL + "api/infinite_week/claim";
        
        /// <summary>
        /// 每日任务
        /// </summary>
        public static string DAILY_TASK_INFO => SERVER_URL + "api/daily_task/info";
        public static string DAILY_TASK_SUPER_REFESH => SERVER_URL + "api/daily_task/refresh";
        public static string DAILY_TASK_CLAIM_TOTAL => SERVER_URL + "api/daily_task/claim_total";
        public static string DAILY_TASK_CALIM => SERVER_URL + "api/daily_task/claim";
        public static string GET_ROOM_LIST => SERVER_URL + "api/room/list";

        /// <summary>
        /// 邀请对战
        /// </summary>
        public static string FRIENDS_DUEL_INFO => SERVER_URL + "api/friends_duel/info";
        public static string FRIENDS_DUEL_CREATE_ROOM => SERVER_URL + "api/friends_duel/create_room";
        public static string FRIENDS_DUEL_ROOM_STATUS => SERVER_URL + "api/friends_duel/room_status";
        public static string FRIENDS_DUEL_CLOSE_ROOM => SERVER_URL + "api/friends_duel/close_room";
        public static string FRIENDS_DUEL_JOIN_ROOM => SERVER_URL + "api/friends_duel/join_room";
        public static string FRIENDS_DUEL_CLAIM => SERVER_URL + "api/friends_duel/claim";
        
        /// <summary>
        /// 
        /// </summary>
        public static string MAGIC_BALL_CLAIM => SERVER_URL + "api/wizard/claim";
        
        public static string MAGIC_BALL_REFRESH => SERVER_URL + "api/wizard/refresh";

        public static string MAGIC_BALL_INFO => SERVER_URL + "api/wizard/info";
        
        public static string LUCKY_GUY_INFO => SERVER_URL + "api/lucky_you/info";

        public static string COUNTRY_INFO => SERVER_URL + "api/support/country_info";

        public static string CHARGE_GPS => SERVER_URL + "api/charge/set_gps";
        
        
        public static string CANCLE_WITHDRAW => SERVER_URL + "api/cash/cancel";
        
        /// <summary>
        /// 刷新道具
        /// </summary>
        public static string REFRESH_ITEM => SERVER_URL + "api/user/balance";
        
        
        #region ------------------GM 接口---------------------

        /// <summary>
        /// 修改自然量
        /// </summary>
        public static string GM_SET_ORGANIC => SERVER_URL + "api/tool/set_organic";

        /// <summary>
        /// 添加货币
        /// </summary>
        public static string GM_ADD_MONEY => SERVER_URL + "api/tool/add_money";

        /// <summary>
        ///  对局校验开关
        /// </summary>
        public static string GM_GAME_REPLAY_CLOSE => SERVER_URL + "api/tool/game_replay_close";

        /// <summary>
        ///  修改对局结果
        /// </summary>
        public static string GM_SET_GAME_RESULT => SERVER_URL + "api/tool/set_game_result";
        
        /// <summary>
        /// 校验禁登
        /// </summary>
        public static string GM_SET_GPS_CHECK => SERVER_URL + "api/user/gps_check";
        
        /// <summary>
        /// 
        /// </summary>
        public static string GM_SET_GPS_CHECK_Illegal => SERVER_URL + "api/user/gps_check_illegal";
        
        
        #endregion
    }
}