using System;
using System.Collections.Generic;
using Core.Services.ResourceService.API.Facade;
using Core.Third.I18N;
using DataAccess.Utils.Static;
using UnityEngine;

namespace DataAccess.Model
{
    //游戏初始化需要的数据
    //要支持litedb序列化与反序列化, 只支持属性
    public class BingoData
    {
        /// <summary>
        /// 支持litedb序列化与反序列化需要
        /// </summary>
        public int Id { get; set; }

        //是否结束
        public bool Finish { get; set; }

        /// <summary>
        /// 登陆时候的 匹配次数
        /// </summary>
        public int MatchCountAtLogin { get; set; }

        public string MatchId { get; set; }

        public int CacheTime { get; set; }

        public float MultipleScoreCountDown { get; set; }

        public int PropUseCountDown { get; set; }

        //这个应该算的出来
        // public int BingoCount { get; set; }

        //base64加密的
        /// <summary>
        /// 生成棋盘数据
        /// </summary>
        public string GridSeed { get; set; }

        public RoomStyle Style { get; set; }

        /// <summary>
        /// 唱票数据
        /// </summary>
        public string CallSeed { get; set; }

        //道具列表
        public string PropsSeed { get; set; }

        //四选一列表
        public string[] ChooseArray { get; set; }

        /// <summary>
        /// 剩余游戏时间
        /// </summary>
        public float LessTime { get; set; } = GlobalEnum.ONE_GAME_TIME;

        /// <summary>
        /// 剩余可以使用的暂停时间
        /// </summary>
        public int LessPauseTime { get; set; } = GlobalEnum.TOTAL_PAUSE_TIME;

        public float CallSliderFillAmount { get; set; }

        //历史操作数据
        public List<int[]> OperateList { get; set; }

        public bool IsLuckRoom()
        {
            var history = Root.Instance.MatchHistory.Find(matchHistory => matchHistory.match_id == MatchId);
            if (history == null)
            {
                return false;
            }

            return history.IsLuckyRoom;
        }

        public BingoData(string gridSeed, string propsSeed, string[] chooseArray, int lessTime, int lessPauseTime,
            List<int[]> operateList)
        {
            GridSeed = gridSeed;
            PropsSeed = propsSeed;
            ChooseArray = chooseArray;
            LessTime = lessTime;
            LessPauseTime = lessPauseTime;
            OperateList = operateList;
        }

        public BingoData()
        {
        }
    }

    /// <summary>
    /// bingo棋盘上的格子
    /// </summary>
    public struct BingoGrid
    {
        private bool isNormalClicked;

        public int Index;

        /// <summary>
        /// int 数值
        /// </summary>
        public byte Value;

        /// <summary>
        /// 选中
        /// </summary>
        public bool Clicked
        {
            set { isNormalClicked = value; }
            get => isNormalClicked || MagicClicked;
        }

        public bool IsBingo => BingoCount > 0;

        /// <summary>
        /// 重复bingo的次数
        /// </summary>
        public int BingoCount;

        
        public int AnimationBingoCount;
        
        /// <summary>
        /// 是否是道具点击的【中心点也认为是道具点击】
        /// </summary>
        public bool MagicClicked;
    }

    public class Prop
    {
        public int id;

        public string name => I18N.Get($"key_prop_name_{id}");

        public string desc => I18N.Get($"key_prop_desc_{id}");

        //参数
        public int value;

        public Sprite sprite;

        //持续时间
        public float spanTime;

        /// <summary>
        /// 是否需要选择一个棋子
        /// </summary>
        public bool isNeedChooseGrid;

        public SoundPack Sound;

        public Prop(int id)
        {
            this.id = id;
            MakeSelf();
        }

        private void MakeSelf()
        {
            switch (id)
            {
                //双倍积分
                case Const.DoubleScore:
                    sprite = ResourceSystem.That.LoadAssetSync<Sprite>("uibingo/item01");
                    spanTime = 15;
                    value = 2;
                    Sound = SoundPack.Double_Use;
                    break;
                //多选一
                case Const.ChooseOne:
                    sprite = ResourceSystem.That.LoadAssetSync<Sprite>("uibingo/item02");
                    spanTime = 5;
                    Sound = SoundPack._4x1;
                    break;
                //直选
                case Const.ChooseAny:
                    sprite = ResourceSystem.That.LoadAssetSync<Sprite>("uibingo/item03");
                    isNeedChooseGrid = true;
                    spanTime = 5;
                    Sound = SoundPack._6x1;
                    break;
                case Const.Cross:
                    sprite = ResourceSystem.That.LoadAssetSync<Sprite>("uibingo/item04");
                    isNeedChooseGrid = true;
                    spanTime = 5;
                    Sound = SoundPack.Cross;
                    break;
            }
        }
    }

    public class BingoScore
    {
        /// <summary>
        /// 超过1个bingo时， 额外加分， 对局结束后加分
        /// </summary>
        const int MULTI_BINGO_BONUS = 700;
        
        /// <summary>
        /// 双倍得分
        /// </summary>
        private int wingBonus;

        /// <summary>
        /// 点选目标的得分
        /// </summary>
        public int BaseScore;

        /// <summary>
        /// 达成bingo的分数
        /// </summary>
        public int GetBingoScore;

        public int BingoCount;

        //双倍奖励
        public int WingBonus
        {
            get => wingBonus;
            set => wingBonus = value;
        }

        /* ----------上面是Card上可以抽象出的分数-----------*/

        /// <summary>
        /// 从第二个bingo开始， 增加的每个得分【这个是结算时的得分， 进行中不算分】
        /// </summary>
        public int MultiBingo
        {
            get
            {
                if (BingoCount < 2)
                {
                    return 0;
                }

                return MULTI_BINGO_BONUS * BingoCount;
            }
        }

        /// <summary>
        /// 选中所有目标的奖励
        /// </summary>
        public int FullGridBonus;

        /// <summary>
        /// 
        /// </summary>
        public int TimeBonus;

        /// <summary>
        /// 
        /// </summary>
        public int LessTime;

        private int penaltyScore;

        /// <summary>
        /// 扣除的分数, 不受双倍影响 , 总分不能被扣为负数
        /// </summary>
        public int PenaltyScore
        {
            get { return penaltyScore; }
            set
            {
                //最多总分被扣到0
                AddPenaltyScore = value - penaltyScore;
                penaltyScore = Math.Min(value, TotalScore + penaltyScore);
            }
        }

        public int AddPenaltyScore;
        
        public int TotalScore => TotalScoreInGame + TimeBonus + MultiBingo;

        public int TotalScoreInGame
        {
            get { return WingBonus + BaseScore + GetBingoScore - PenaltyScore + FullGridBonus; }
        }

        public BingoScore()
        {
          
        }
        
        public BingoScore(BingoScore bingoScore)
        {
            wingBonus = bingoScore.wingBonus;
            BaseScore = bingoScore.BaseScore;
            GetBingoScore = bingoScore.GetBingoScore;
            BingoCount = bingoScore.BingoCount;
            FullGridBonus = bingoScore.FullGridBonus;
            TimeBonus = bingoScore.TimeBonus;
            LessTime = bingoScore.LessTime;
            penaltyScore = bingoScore.penaltyScore;
        }
    }

    public enum Operation
    {
        /// <summary>
        /// 增加唱票
        /// </summary>
        AddCallIndex = 0,

        /// <summary>
        /// 点击棋盘, 可能成功， 可能失败， 失败扣分
        /// </summary>
        ClickGrid,

        /// <summary>
        /// 改变道具能量
        /// </summary>
        PropEnergyChange,

        /// <summary>
        /// 点击bingo
        /// </summary>
        ClickBingo,

        /// <summary>
        /// 开始使用除双倍之外的道具
        /// </summary>
        BeginProp,

        /// <summary>
        /// 使用了要作用于棋盘上的道具，比如十字消和任选
        /// </summary>
        UseProp,

        /// <summary>
        /// 道具倒计时间到了， 或者自己取消【目前还没有自己取消】
        /// </summary>
        CancelProp,
        
        StartDouble,
        
        EndDouble,

        /// <summary>
        /// 从4选1界面， 选中了1个数字
        /// </summary>
        Start4X1Choose,

        /// <summary>
        /// 结束可以选择从 4选1界面选中的那个数字
        /// </summary>
        End4X1Choose,

        AddTimeScore,

        /// <summary>
        /// 获取多个bingo奖励
        /// </summary>
        AddMultiBingoScore,
        
        /// <summary>
        /// 第一局引导
        /// </summary>
        FirstGameGuide,
        
        /// <summary>
        /// 移除最左边的唱票值
        /// </summary>
        RemoveLast,
        
        EndGame
    }

    public enum BingoType
    {
        B = 1,
        I = 2,
        N = 3,
        G = 4,
        O = 5
    }
}