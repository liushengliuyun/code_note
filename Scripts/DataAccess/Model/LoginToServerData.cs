using System.ComponentModel;
using Newtonsoft.Json;

namespace DataAccess.Model
{
    public class LoginToServerData : INotifyPropertyChanged
    {
        public string udid;
        public string bundle_id;
        public int version;
        public string language;

        public string timezone;

        public string sim_info;
        public string gps;
        public string country;
        public string ip;
        public string appsflyer_id;
        public string gps_extra;

        public int pay_app;


        public string timestamp;

        [JsonIgnore]
        public string Timestamp
        {
            get => timestamp;
            set
            {
                timestamp = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Timestamp)));
            }
        }

        //json string
        public string device_info;

        public string signature;

        public string picture;

        /// <summary>
        /// 本次游戏登陆的次数
        /// </summary>
        public int login_count;
        
        public int msg_index;
        
        public LoginToServerData()
        {
        }

        public LoginToServerData(string udid, string bundleID, int version, string language, string timezone,
            string timestamp = null,
            string deviceInfo = null)
        {
            this.udid = udid;
            bundle_id = bundleID;
            this.version = version;
            this.language = language;
            this.timezone = timezone;
            device_info = deviceInfo;
            this.timestamp = timestamp;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}