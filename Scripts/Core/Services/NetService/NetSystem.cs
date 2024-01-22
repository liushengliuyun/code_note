using System;
using System.Collections;
using System.Collections.Generic;
using Carbon.Util;
using CatLib;
using CatLib.EventDispatcher;
using Core.Extensions;
using Core.Manager;
using Core.Services.NetService.API;
using Core.Services.NetService.Internal;
using Core.Services.PersistService.API.Facade;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils;
using DataAccess.Utils.Static;
using iOSCShape;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UI;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using UnityTimer;
using Utils;

namespace Core.Services.NetService
{
    public class ProtoRecord
    {
        public string url;
        public string result = "unkonw";
        public string error;
        public int sendTime;

        public override string ToString()
        {
            if (error.IsNullOrEmpty())
            {
                return $"url = {url}, result = {result}, sendTime = {sendTime}";
            }

            return $"url = {url}, result = {result}, sendTime = {sendTime} , error = {error}";
        }
    }

    public class NetSystem : INetSystem
    {
        public bool IsReLogin { get; set; }

        private Action<string> _FailCallBack;

        public HttpRequestHandler CreateHttpRequestHandler(string url, string method, Action<bool, string> callBack)
        {
            return new HttpRequestHandler(url, method, callBack);
        }

        private HashSet<string> sendingRequest;

        private bool _IsReconnecting;

        private List<HttpRequestHandler> reconnectHandlerPool;

        public List<ProtoRecord> SendedProtoQueue { get; set; }

        /// <summary>
        /// 断网后必须 relogin, 其余弹出relogin 或者 tryAgain
        /// </summary>
        private HashSet<string> needReconnectProto = new HashSet<string>()
        {
            Proto.PLAYER_LOGIN,
            Proto.VISITOR_LOGIN,
            Proto.MATCH_END,
        };

        /// <summary>
        /// 显示等待动画的
        /// </summary>
        //todo 添加白名单
        private HashSet<string> needShowNetMask = new HashSet<string>()
        {
            Proto.MATCH_BEGIN,
            
            Proto.AD_ROOM_WATCHED,

            Proto.GAME_END,

            Proto.MATCH_INFO,

            Proto.USER_INFO_UPLOAD,
            
            Proto.DELETE_ACCOUNT,
            
            Proto.GET_CHARGE_METHODS,
            
            Proto.GM_SET_GPS_CHECK_Illegal
        };

        
        public void GetCountryFromCoordinates(float latitude, float longitude, Action callback = null)
        {
            string requestUrl = $"{Proto.geoNamesApiUrl}?lat={Math.Round(latitude, 1)}&lng={Math.Round(longitude, 1)}&username={Proto.geoNamesApiKey}";
         
            YZLog.LogColor(requestUrl);
            
            Framework.Instance.StartCoroutine(GetCountryCoroutine(requestUrl, callback));
        }
        
        private IEnumerator GetCountryCoroutine(string url, Action callback = null)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();

                try
                {
                    if (www.isNetworkError || www.isHttpError)
                    {
                        Debug.LogError($"Error: {www.error}");
                    }
                    else
                    {
                        string jsonResponse = www.downloadHandler.text;
                        YZLog.LogColor($"定位地址json = {jsonResponse}");
                        var jObject =  JsonConvert.DeserializeObject(jsonResponse) as JObject;

                        if (jObject == null)
                        {
                            yield break;
                        }
                        
                        var address = YZJsonUtil.DeserializeJObject<address>("address", jObject);;

                        if (address != null)
                        {
                            // Access country information from countryResponse object
                            string countryCode = address.countryCode;
                            string area = address.adminCode1;
                            string city = address.locality;

                            LocationManager.Shared.SaveLocateData(countryCode, area, city);
                        }
                    }
                }
                catch (Exception e)
                {
                    CarbonLogger.LogError("请求定位异常!" + e + "\n" + e.StackTrace);
                }
                
                callback?.Invoke();
#if UNITY_IOS
                iOSCShapeLocationTool.Shared.iOSLocateHaveResult = true;     
#endif
                
            }
        }
        
        private class address
        {
            // public string countryName;
            public string countryCode;
            /// <summary>
            /// 省
            /// </summary>
             public string adminCode1;
            /// <summary>
            /// 市
            /// </summary>
            public string locality;
        }
        
        public void SendGameRequest(string url, string method, Action<JObject> callBack,
            string json = null, bool silence = false, bool forceSend = false)
        {
            var failCallBack = _FailCallBack;
            _FailCallBack = null;

            if (_IsReconnecting)
            {
                return;
            }

            sendingRequest ??= new HashSet<string>();

            if (!forceSend && sendingRequest.Contains(url))
            {
                return;
            }

            if (!silence && needShowNetMask.Contains(url))
            {
                ShowNetWaitMask();
            }

            var record = new ProtoRecord()
            {
                url = url,
                sendTime = TimeUtils.Instance.UtcTimeNow
            };

            if (SendedProtoQueue != null)
            {
                SendedProtoQueue.Add(record);

                if (SendedProtoQueue.Count > 50)
                {
                    SendedProtoQueue.RemoveAt(0);
                }

                SaveProtoRecord();
            }

            sendingRequest.Add(url);

            Framework.Instance.StartCoroutine(
                SendRequestCor(url, method, callBack, json, failCallBack, silence, record));
        }

        private void SaveProtoRecord()
        {
#if DEBUG
            if (SendedProtoQueue == null)
            {
                return;
            }

            var jsonRecord = JsonConvert.SerializeObject(SendedProtoQueue);
            PersistSystem.That.SaveValue(GlobalEvent.PROTO_LIST, jsonRecord, true);
#endif
        }

        public void SetFailCallBack(Action<string> callback)
        {
            if (_FailCallBack == null)
            {
                _FailCallBack = callback;
            }
            else
            {
                var lastCallBack = _FailCallBack;
                _FailCallBack =  (content) =>
                {
                    lastCallBack(content);
                    callback?.Invoke(content);
                };
            }
        }

        public void ShowNetWaitMask()
        {
            //插入到队列头
            UserInterfaceSystem.That.TopInQueue<UIWaitNet>();
        }

        IEnumerator SendRequestCor(string url, string method, Action<JObject> successCallBack,
            string json, Action<string> failCallBack, bool silence, ProtoRecord record)
        {
            bool? httpSuccess = null;
            ProtoResult protoResult = ProtoResult.NotConnect;
            string cacheContent = null;

            bool isReconnecting = _IsReconnecting;
            var sendCount = _IsReconnecting ? 1 : 3;

            for (int i = 0; i < sendCount; i++)
            {
                httpSuccess = null;
                HttpRequestHandler handler = null;
                string createUrl = url;
                handler = CreateHttpRequestHandler(url, method,
                    (success, content) =>
                    {
                        cacheContent = content;
                        httpSuccess = success;
                        ReceiveData(createUrl, successCallBack, content, success, record, ref protoResult);
                    });

                //照片数据很大 ， 设置长点, 通常为5s
                if (url == Proto.USER_INFO_UPLOAD
                    || url == Proto.GET_ROOM_LIST
                    || url == Proto.GET_CONFIGS
                    || url == Proto.GM_SET_ORGANIC
                   )
                {
                    handler.TimeOut = 10;
                }

                if (method == GlobalEnum.HttpRequestType.POST)
                {
                    handler.SetPostJson(json);
                }

                var authorization = MediatorRequest.Instance.GetAuthorization();
                SetAuthorization(handler, authorization);

                if (!string.IsNullOrEmpty(json))
                {
                    YZLog.LogColor(
                        $"次数 = {i + 1}, url = {url}, json = {json} \n token = {authorization}",
                        "dark");
                }

                if (_IsReconnecting)
                {
                    reconnectHandlerPool ??= new List<HttpRequestHandler>();
                    reconnectHandlerPool.Add(handler);
                }

                handler.Start();

                while (httpSuccess == null)
                {
                    yield return null;
                }

                if (httpSuccess == true)
                {
                    break;
                }
                else
                {
                    yield return new WaitForSeconds(1);
                }
            }

            record.result = protoResult.ToString();
            SaveProtoRecord();

            sendingRequest.Remove(url);
            UserInterfaceSystem.That.RemoveUIByName(nameof(UIWaitNet));

            //包括了服务器返回失败 和 网络失败
            if (protoResult is not ProtoResult.Success)
            {
                failCallBack?.Invoke(cacheContent);
            }

            if (!silence)
            {
                // 特殊处理，由于UILogin界面在编译期间完成 AddEventListener(Proto.GET_CONFIGS)
                string urlName = url;

                if (url.Contains("api/configs"))
                {
                    urlName = GlobalEvent.Get_Config_Success;
                }

                EventDispatcher.Root.Raise(urlName, null,
                    new ProtoEventArgs(protoResult));
            }

            //断网， 针对特定的协议， 应该重连 
            if (httpSuccess == false)
            {
                if (!_IsReconnecting)
                {
                    _IsReconnecting = true;
                    record.result += " 开始重连";
                    SaveProtoRecord();

                    if (needReconnectProto.Contains(url))
                    {
                        UserInterfaceSystem.That.ShowUI<UIConfirm>(new UIConfirmData()
                        {
                            Type = UIConfirmData.UIConfirmType.OneBtn,
                            // HideCloseBtn = true,
                            IsNetWorkError = true,
                            confirmTitle = I18N.Get("key_relogin"),
                            desc = I18N.Get("key_net_relogin"),
                            confirmCall = () => { ReLogin(); }
                        });
                    }
                    else
                    {
                        //重试 try again
                        UserInterfaceSystem.That.ShowUI<UIConfirm>(new UIConfirmData
                        {
                            Type = UIConfirmData.UIConfirmType.TwoBtn,
                            // HideCloseBtn = true,
                            confirmTitle = I18N.Get("key_relogin"),
                            cancelTitle = I18N.Get("key_try_again"),
                            desc = I18N.Get("key_net_disconnect"),
                            confirmCall = () => { ReLogin(); },
                            WaitCloseCallback = true,
                            IsNetWorkError = true,
                            ShowTryAgain = true,
                            SendTryAgain = () =>
                            {
                                //failCallBack已触发过一次, 就不触发了
                                Framework.Instance.StartCoroutine(SendRequestCor(url, method,
                                    o =>
                                    {
                                        //关闭UIConfirm
                                        UserInterfaceSystem.That.RemoveUIByName(nameof(UIConfirm));
                                        successCallBack?.Invoke(o);
                                    },
                                    json,
                                    null,
                                    silence, record));
                            },
                            cancleCall = () => { EventDispatcher.Root.Raise(GlobalEvent.Try_Again); }
                        });
                    }
                }
            }
            else
            {
                //重连成功
                if (isReconnecting && reconnectHandlerPool != null)
                {
                    foreach (var requestHandler in reconnectHandlerPool)
                    {
                        requestHandler.Dispose();
                    }

                    reconnectHandlerPool.Clear();

                    record.result += " 重连成功";
                    SaveProtoRecord();
                }

                _IsReconnecting = false;
                EventDispatcher.Root.Raise(GlobalEvent.ReConnectSuccess);
            }
        }

        /// <summary>
        /// 处理接受到的数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="successCallBack"></param>
        /// <param name="content"></param>
        /// <param name="success"></param>
        /// <param name="record"></param>
        /// <param name="httpSuccess"></param>
        /// <param name="protoResult"></param>
        private void ReceiveData(string url, Action<JObject> successCallBack, string content, bool success,
            ProtoRecord record,
            ref ProtoResult protoResult)
        {
            protoResult = ProtoResult.NotConnect;
            if (!success) return;

            //在回调中可能再发送网络通信
            sendingRequest.Remove(url);
            var o = JsonConvert.DeserializeObject(content);
            if (o is JObject jObject)
            {
                string message = jObject.SelectToken("message")?.Value<string>();
                TimeUtils.Instance.SetTimeOffset(jObject);

                if (message == GlobalEnum.JSON_SUCCESS)
                {
                    protoResult = ProtoResult.Success;

                    PrecessServerMaintain(jObject, out var serverMaintain);

                    if (serverMaintain is { InTime: true })
                    {
                        return;
                    }
                    
                    ProcessNotification(jObject, true);
                    
                    successCallBack?.Invoke(jObject);
                  
                    ProcessNotification(jObject, false);
                }
                else
                {
                    protoResult = ProtoResult.Fail;
                    //错误处理
                    var errorCode = YZJsonUtil.DeserializeJObject<int>("code", jObject);
                    var errorStr = YZJsonUtil.DeserializeJObject<string[]>("errors",jObject);
                    HandleError(url, errorCode, errorStr);

                    record.error = content;
                    // CarbonLogger.LogError(url + " 服务器返回fail：" + content);
                }
            }
            else
            {
                protoResult = ProtoResult.Fail;
                record.error = "jObject 解析失败 " + content;
                CarbonLogger.LogError(url + " 数据解析错误 =" + content);
            }

            SaveProtoRecord();
        }

        /// <summary>
        /// 处理服务器维护
        /// </summary>
        /// <param name="jObject"></param>
        private void PrecessServerMaintain(JObject jObject, out ServerMaintain data)
        {
            var str = jObject.SelectToken("system_maintenance")?.ToString();
            if (str.IsNullOrEmpty())
            {
                data = null;
                return;
            }

            data = JsonConvert.DeserializeObject<ServerMaintain>(str);

            if (data == null)
            {
                return;
            }

            Root.Instance.ServerMaintainInfo = data;

            var topUI = UserInterfaceSystem.That.GetTopNormalUI();

            if (data.InTime)
            {
                if (topUI.ClassType == typeof(UIBingo) || topUI.ClassType == typeof(UIShowScore))
                {
                    data = null;
                    return;
                }

                UserInterfaceSystem.That.Reset();
                UserInterfaceSystem.That.ShowUI<UILogin>(LoginPanel.ServerMaintain);
            }
        }

        /// <summary>
        /// 处理服务器返回错误码
        /// </summary>
        /// <param name="url"></param>
        /// <param name="errorCode"></param>
        /// <param name="errorStr"></param>
        private void HandleError(string url, int errorCode, string[] errorStr)
        {
            if (Enum.IsDefined(typeof(ProtoError), errorCode))
            {
                switch ((ProtoError)errorCode)
                {
                    case ProtoError.MailFormat:
                        break;
                    case ProtoError.MailNotExizt:
                        UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_http_code_1108"));
                        break;
                    case ProtoError.MailBinded:
                        UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_http_code_1109"));
                        break;
                    case ProtoError.MailLoginFail:
                        if (errorStr is {Length: > 0})
                        {
                            UserInterfaceSystem.That.ShowUI<UITip>(errorStr[0]);
                        }
                        else
                        {
                            UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_http_code_1111"));
                        }
                        break;
                    case ProtoError.PlayerDeleted:
                        if (!url.Equals(Proto.EMAIL_LOGIN))
                        {
                            LoginFail();
                        }

                        UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_http_code_1114"));
                        break;
                    case ProtoError.PlayerLock:
                        UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_http_code_9997", Root.Instance.UserId));
                        break;
                    case ProtoError.MailBindByOther:
                        UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_http_code_1106"));
                        break;
                    case ProtoError.TokenFail:
                        LoginFail();
                        UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_http_code_1100"));
                        break;
                    case ProtoError.Common:
                        break;
                    case ProtoError.WithDrawError:
                    case ProtoError.WithDrawError2:
                        //更新提现记录
                        MediatorRequest.Instance.WithdrawHistory(false, () =>
                        {
                            //保证这个事件在后面一点触发
                            Timer.Register(0.05f, () =>
                            {
                                EventDispatcher.Root.Raise(ProtoError.WithDrawError.ToString());
                            });
                        });
                       
                        if (errorStr is {Length: > 0})
                        {
                            UserInterfaceSystem.That.ShowUI<UITip>(errorStr[0]);
                        }
                        break;
                    default:
                        if (errorStr is {Length: > 0})
                        {
                            UserInterfaceSystem.That.ShowUI<UITip>(errorStr[0]);
                        }
                        break;
                }
            }
        }

        public void Reset()
        {
            sendingRequest = null;
            _IsReconnecting = false;
            reconnectHandlerPool = null;
            _FailCallBack = null;
            IsReLogin = true;
        }

        private void ReLogin()
        {
            MediatorRequest.Instance.LoginWithProcess(false);
        }

        private void LoginFail()
        {
            //清空token ， 走游客登陆
            // PersistSystem.That.DeletePrefsValue(GlobalEnum.ClientUID);
            Proto.ClearToken();
            ReLogin();
        }

        private void ProcessNotification(JObject jObject, bool before)
        {
            var notificationJson = jObject.SelectToken("notification")?.ToString();
            if (!notificationJson.IsNullOrEmpty())
            {
                var jsonArray = JArray.Parse(notificationJson);
                var notification = new Notification();
                foreach (var jToken in jsonArray)
                {
                    notification.name = jToken.SelectToken("name")?.ToString();
                    bool shouldBefore = notification.name == "user_block";
                    //异或
                    if (before ^ shouldBefore)
                    {
                        continue;
                    }
                    notification.value = jToken.SelectToken("value")?.ToString();
                    notification.Command();
                }
            }
        }

        void SetAuthorization(HttpRequestHandler handler, string authorization)
        {
            if (!string.IsNullOrEmpty(authorization))
            {
                handler.SetHeader("Authorization", authorization);
            }
        }
    }
}