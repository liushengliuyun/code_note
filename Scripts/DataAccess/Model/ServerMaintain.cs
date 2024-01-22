using System;
using DataAccess.Utils;
using Newtonsoft.Json;

namespace DataAccess.Model
{
    public class ServerMaintain
    {
        public int id;
        public int min_version;
        public int max_version;
        public int status;
        public int type;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int start_time;
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int close_time;
        
        /// <summary>
        /// 维护开始时间
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int begin_time;
        
        /// <summary>
        /// 结束维护时间
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int end_time;

        public string text;
        public DateTime created_at;
        public DateTime updated_at;

        private int startTime => start_time;
        private int endTime => end_time;
        
        public bool InTime => TimeUtils.Instance.UtcTimeNow >= startTime && TimeUtils.Instance.UtcTimeNow <= endTime;

        public int LessTime => Math.Max(0, endTime - TimeUtils.Instance.UtcTimeNow);
    }
}