using System;
using System.Collections.Generic;
using DataAccess.Utils;

namespace DataAccess.Model
{
    public class RoomAdInfo
    {
        /// <summary>
        /// 进入对局的时候减
        /// </summary>
        public int ad_room_chance;

        /// <summary>
        /// 最后一次看广告的时间
        /// </summary>
        public int ad_end_time;

        /// <summary>
        /// 房间广告观看次数 key 房间ID
        /// </summary>
        public Dictionary<int, int> ad_room_watch;

        public bool IsLock => ad_room_chance <= 0;

        public int LessTime => Math.Max(0, TimeUtils.Instance.EndDayTimeStamp - TimeUtils.Instance.UtcTimeNow);

        public bool RoomCanPlay(int roomid)
        {
            if (ad_room_watch == null)
            {
                return false;
            }

            if (IsLock)
            {
                return false;
            }
            
            ad_room_watch.TryGetValue(roomid, out var count);
            return count > 0;

        }
    }
}