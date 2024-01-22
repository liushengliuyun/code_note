using System;
using System.Collections.Generic;
using CatLib;
using Core.Services.NetService.Internal;
using Newtonsoft.Json.Linq;

namespace Core.Services.NetService.API
{
    public interface INetSystem : IReset
    {

        bool IsReLogin { set; get; }
        
        public List<ProtoRecord> SendedProtoQueue { set; get; }
        
        HttpRequestHandler CreateHttpRequestHandler(string url, string method, Action<bool, string> callBack);

        void SendGameRequest(string url, string method, Action<JObject> callBack,
            string json = null, bool silence = false, bool forceSend = false);
        
        void SetFailCallBack(Action<string> callback);

        void ShowNetWaitMask();

        public void GetCountryFromCoordinates(float latitude, float longitude, Action callback = null);
    }
}