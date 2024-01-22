using Newtonsoft.Json;

namespace DataAccess.Model
{
    public class CountryInfo
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public string iso_code;
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public string province;
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public string city;
    }

    public class GPSExtra
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string gps = "";
        /// <summary>
        /// todo 判断是否为虚拟定位
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool gps_camouflage;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool gps_reject;
    }

    public class GPSInfo
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string country;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string province;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string city;
    }
}