using System;
using System.Collections.Generic;
using System.Linq;
using CatLib.EventDispatcher;
using Core.Extensions;
using Core.Services.AudioService.API.Facade;
using Core.Services.PersistService.API.Facade;
using Core.Services.ResourceService.API.Facade;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Third.I18N;
using Cysharp.Threading.Tasks;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using UI;
using UnityEngine;
using Utils;
using YooAsset;

namespace DataAccess.Controller
{
    
    public class MediatorBingo : global::Utils.Runtime.Singleton<MediatorBingo>
    {
        /// <summary>
        /// 棋盘数据
        /// </summary>
        public string GridSeed { get; set; }

        /// <summary>
        /// 唱票数据
        /// </summary>
        public string CallSeed { get; set; }
        
        public string Props { get; set; }

        public string[] ChooseArray { get; set; }
        
        public Sprite GetBingoBallImage(BingoType bingoType)
        {
            return ResourceSystem.That.LoadAssetSync<Sprite>($"UIBingo/gameball_{(int)bingoType}".ToLower());
        }
        
        public Sprite GetBingoBallImageSmall(BingoType bingoType)
        {
            return ResourceSystem.That.LoadAssetSync<Sprite>($"UIBingo/ball{(int)bingoType}".ToLower());
        }
        
        public Sprite GetBingoBallImageSelect(BingoType bingoType)
        {
            return ResourceSystem.That.LoadAssetSync<Sprite>($"UIBingo/s_ball{(int)bingoType}".ToLower());
        }
        
        public Sprite GetSpriteByUrl(string url)
        {
            if (url.IsNullOrEmpty())
            {
                return null;
            }

            return ResourceSystem.That.LoadAssetSync<Sprite>(url);
        }

        public void SaveMatchId(string matchId, int roomId)
        {
            YZLog.LogColor($"保存MatchId = {matchId}");

            PersistSystem.That.SaveValue(GlobalEnum.LAST_MATCH_ROOM_ID, roomId, true);
            PersistSystem.That.SaveValue(GlobalEnum.LAST_MATCH_BEGIN_TIME, TimeUtils.Instance.UtcTimeNow, true);
            PersistSystem.That.SaveValue(GlobalEnum.LAST_MATCH_BEGIN_ID, matchId, true);

            PersistSystem.That.SaveValue(GlobalEnum.LAST_GAME_ID, matchId, true);
        }

        /// <summary>
        /// 游戏开始后， 删除Match ID
        /// </summary>
        public void ClearMatchId()
        {
            PersistSystem.That.DeletePrefsValue(GlobalEnum.LAST_MATCH_ROOM_ID, true);
            PersistSystem.That.DeletePrefsValue(GlobalEnum.LAST_MATCH_BEGIN_TIME, true);
            PersistSystem.That.DeletePrefsValue(GlobalEnum.LAST_MATCH_BEGIN_ID, true);
        }

        public void ClearBingoBeginData()
        {
            GridSeed = null;
            Props = null;
            ChooseArray = null;
            CallSeed = null;
        }

        public void ClearBingoDB()
        {
            //删了只能恢复一次游戏
            // PersistSystem.That.DeletePrefsValue(GlobalEnum.LAST_GAME_ID, true);
            PersistSystem.That.DeleteDB<BingoData>(GlobalEnum.DB_YATZY, true);
        }

        public bool IsCanClaim(List<MatchHistory> matchList)
        {
            var canClaim = false;
            if (matchList != null)
            {
                var rid = Root.Instance.Role.user_id;
                var myMatch = matchList.Find(match => match.user_id == rid);
                if (myMatch is { status: (int)Status.CanClime })
                {
                    canClaim = true;
                }
            }

            return canClaim;
        }

        public GameObject LoadSpine(Transform parent, out AssetOperationHandle handle, float scale = 1,
            Vector3 position = default, bool faceLeft = true)
        {
            var path = "DogRole";
            handle = ResourceSystem.That.GetAssetHandle($"spine/{path}");
            var obj = handle.InstantiateSync();
            obj.transform.SetParent(parent);
            obj.transform.localPosition = position;
            if (scale > 0)
            {
                obj.transform.localScale = Vector3.one * scale;
            }

            //如果面朝右边
            if (!faceLeft)
            {
                var old = obj.transform.localScale;
                obj.transform.localScale = new Vector3(-old.x, old.y, old.z);
            }

            return obj;
        }

        /// <summary>
        /// 如果有强退 游戏遗留的数据， 帮助玩家提交 , 超过剩余暂停时间才提交
        /// </summary>
        public void ReplayBingoGame()
        {
            //强退游戏， 按照真实对局分数提交
            var bingoData = PersistSystem.That.GetValue<BingoData>(GlobalEnum.DB_YATZY, true) as BingoData;
            if (bingoData == null)
            {
                return;
            }

            var lastMatchId = PersistSystem.That.GetValue<string>(GlobalEnum.LAST_GAME_ID, true) as string;
            if (bingoData.MatchId != lastMatchId) return;

            int gameScore = 0;

            var timeSpan = TimeUtils.Instance.UtcTimeNow - bingoData.CacheTime;

            if (bingoData.OperateList != null && bingoData.OperateList.Any())
            {
                gameScore = bingoData.OperateList[^1][3];
            }

            void SendGameEnd()
            {
                EventDispatcher.Root.Raise(GlobalEvent.CANCEL_GAME_END);
                MediatorRequest.Instance.GameEnd(bingoData.MatchId,
                    gameScore,
                    BingoCloseType.EARLY_END,
                    (int)bingoData.LessTime,
                    bingoData.OperateList, callBack : () =>
                    {
                        MediatorRequest.Instance.MatchInfo(lastMatchId, onlyGetInfo : true);
                    }
                );

                if (bingoData.IsLuckRoom())
                {
                    EventDispatcher.Root.Raise(GlobalEvent.Rigister_Lucky_guy_fail, "lucky_room");
                }
            }

            //暂停时间超过游戏暂停时间, 不重开
            if (bingoData.LessPauseTime <= timeSpan || bingoData.Finish || bingoData.GridSeed.IsNullOrEmpty())
            {
                SendGameEnd();
                // ClearBingoDB();
                return;
            }

            //没有通过新手引导
            if (bingoData.MatchCountAtLogin == 0 &&
                Root.Instance.NotPassTheNewPlayerGuide(NewPlayerGuideStep.FIRST_ROOM_GUIDE_FINISH))
            {
                bingoData.OperateList?.Clear();
            }

            if (bingoData.MatchCountAtLogin != 0)
            {
                bingoData.MatchCountAtLogin = Root.Instance.MatchCountAtLoginTime;
            }

            if (bingoData.LessPauseTime > timeSpan)
            {
                bingoData.LessPauseTime -= timeSpan;
            }
            else
            {
                //计算双倍倒计时
                if (bingoData.MultipleScoreCountDown < 0)
                {
                    bingoData.MultipleScoreCountDown = 0;
                }

                //计算道具使用倒计时
                if (bingoData.PropUseCountDown < 0)
                {
                    bingoData.PropUseCountDown = 0;
                }

                bingoData.LessPauseTime = 0;
            }

            UserInterfaceSystem.That.ShowUI<UIConfirm>(new UIConfirmData
            {
                confirmCall = () => UserInterfaceSystem.That.ShowUI<UIBingo>(new GameData()
                {
                    ["bingoData"] = bingoData
                }),
                desc = I18N.Get("restore_game_desc"),
                cancleCall = SendGameEnd
            });

            //玩家杀进程， 将不能恢复
            // ClearBingoDB();
        }

        /// <summary>
        /// 支付门票后， 未开始就退出的， 才还原
        /// </summary>
        /// <returns>玩家是否成功恢复游戏</returns>
        public async UniTask RestoreGame()
        {
            //支付了门票， 但未完成的MatchId
            var lastMatchId = PersistSystem.That.GetValue<string>(GlobalEnum.LAST_MATCH_BEGIN_ID, true) as string;

            var roomId = (int)PersistSystem.That.GetValue<int>(GlobalEnum.LAST_MATCH_ROOM_ID, true);

            var lastDuelRoomNo = YZDataUtil.GetLocaling(YZConstUtil.YZLastDuelRoomNo, "");

            if (lastMatchId.IsNullOrEmpty() && Root.Instance.DuelInfo != null &&
                !Root.Instance.DuelInfo.wait_match.IsNullOrEmpty() && !lastDuelRoomNo.IsNullOrEmpty())
            {
                MediatorRequest.Instance.DuelMatchBegin(Root.Instance.DuelInfo.room_id,
                    Root.Instance.DuelInfo.wait_match,
                    false, false); // 先不显示 match Ui
                // 重新进之前的邀请对战房间
                UserInterfaceSystem.That.ShowQueue<UIConfirm>(new UIConfirmData
                {
                    confirmCall = () =>
                    {
                        // 玩家点击确认，再显示 match ui
                        UserInterfaceSystem.That.ShowUI<UIMatch>(Root.Instance.DuelOfflineMatchId,
                            Root.Instance.DuelInfo.room_id, false);
                    },
                    desc = I18N.Get("restore_game_desc"),
                    cancleCall = () =>
                    {
                        // 清空上次创建的room
                        YZDataUtil.SetYZString(YZConstUtil.YZLastDuelRoomNo, "");
                        ClearMatchId();
                        MediatorRequest.Instance.GameEnd(Root.Instance.DuelOfflineMatchId, 0, BingoCloseType.EARLY_END, callBack : () =>
                        {
                            MediatorRequest.Instance.MatchInfo(lastMatchId, onlyGetInfo : true);
                        });
                    }
                });
                return;
            }

            YZLog.LogColor($"LastMatchId = {lastMatchId}");

            if (!string.IsNullOrEmpty(lastMatchId))
            {
                var lastBeginMatchTime = (int)PersistSystem.That.GetValue<int>(GlobalEnum.LAST_MATCH_BEGIN_TIME, true);
                var oneHourTime = GlobalEnum.TOTAL_PAUSE_TIME;

                void ZeroSubMit()
                {
                    ClearMatchId();
                    MediatorRequest.Instance.GameEnd(lastMatchId, 0, BingoCloseType.EARLY_END, callBack : () =>
                    {
                        MediatorRequest.Instance.MatchInfo(lastMatchId, onlyGetInfo : true);
                    });
                    EventDispatcher.Root.Raise(GlobalEvent.CANCEL_GAME_END);
                    var history = Root.Instance.MatchHistory.Find(matchHistory => matchHistory.match_id == lastMatchId);
                    if (history is { IsLuckyRoom: true})
                    {
                        EventDispatcher.Root.Raise(GlobalEvent.Rigister_Lucky_guy_fail);
                    }
                }
                
                //如果时间超过1小时， 直接走Game End流程
                //todo check roomId 是否可能为0
                if (TimeUtils.Instance.UtcTimeNow - lastBeginMatchTime > oneHourTime || roomId <= 0)
                {
                    ZeroSubMit();
                }
                else
                {
                    try
                    {
                        UserInterfaceSystem.That.ShowQueue<UIConfirm>(new UIConfirmData
                        {
                            confirmCall = () => UserInterfaceSystem.That.ShowUI<UIMatch>(lastMatchId, roomId),
                            desc = I18N.Get("restore_game_desc"),
                            cancleCall = ZeroSubMit
                        });
                    }
                    catch (Exception e)
                    {
                        YZLog.Error(e.ToString());
                    }
                }
            }
            else
            {
                //逻辑上二者应该只存在一个的
                ReplayBingoGame();
            }
        }

        public void PlayBingoSound(int index)
        {
            if (index <= 0 || index > 74)
            {
                return;
            }
            AudioSystem.That.PlaySound($"uibingo/{index}");
        }
        
        /// <summary>
        /// 检查是否需要 道具提示
        /// </summary>
        public void CheckPropNeedTip(Prop prop)
        {
            if (prop == null)
            {
                return;
            }

            //第一局弹总的规则就不弹了
            if (Root.Instance.MatchHistoryCount == 0 || Root.Instance.MatchCountAtLoginTime == 1)
            {
                return;
            }
            
            /*//只在前3局显示   
            if (Root.Instance.MatchHistoryCount > 2 || Root.Instance.MatchCountAtLoginTime > 3)
            {
                return;
            }*/

            string key = null;
            switch (prop.id)
            {
                case Const.DoubleScore:
                    key = GlobalEnum.IS_FIRST_USE_DOUBLE_PROP;
                    break;
                case Const.ChooseAny:
                    key = GlobalEnum.IS_FIRST_USE_4X1;
                    break;
                case Const.ChooseOne:
                    key = GlobalEnum.IS_FIRST_USE_6X1;
                    break;
                case Const.Cross:
                    key = GlobalEnum.IS_FIRST_USE_CROSS;
                    break;
            }

            if (key.IsNullOrEmpty())
            {
                return;
            }

            var isNeedTip = !(bool)PersistSystem.That.GetValue<bool>(key, true);
            if (isNeedTip)
            {
                PersistSystem.That.SaveValue(key, true, true);

                //理论上， 可能会连续弹出 
                /*不要用queue , 可能跨天的时候被排在后面*/
                UserInterfaceSystem.That.ShowUI<UIShowProp>(prop);
            }
        }
    }
}