using System;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using System.Collections;
using Core.Manager;
using LitJson;
using Utils;

namespace Core.Server
{
    public class YZServerRequest
    {
        public bool YZIsRefreshBalance = true;
        public bool YZIsRefreshTicket = true;
        public bool YZIsRefreshLevel = true;
        public bool YZIsRefreshVip = true;
        public bool YZIsCancel = false;

        public int YZRetryTimes = 0;

        public string YZUrlPath = "";

        private const int YZTimeoutNum = 10;

        private const string YZUdid = "udid";
        private const string YZBundleId = "bundle_id";
        private const string YZVersion = "version";
        private const string YZTimestamp = "timestamp";

        private int YZRetryRemain = 3;
        private bool YZUseAuth = true;

        private Action<long, string, YZServerRequest> YZHandler;
        private Coroutine YZCoroutineRequest;
        private UnityWebRequest YZRequest;

        private readonly string YZUrl;
        private readonly string YZMethod;
        private readonly JsonData YZHeaders;
        private readonly JsonData YZForms;

        private readonly List<Action<string>> YZSuccessHandler;
        private readonly List<Action<int, string>> YZFailureHandler;

        public YZServerRequest(string url, string method, Action<long, string, YZServerRequest> handler)
        {
            if (url.StartsWith("http"))
            {
                YZUrl = url;
            }
            else
            {
                YZUrl = YZString.Concat(YZServerUtil.GetYZServerURL(), url);
            }

            YZMethod = method;
            YZUrlPath = url;
            YZHandler = handler;
            YZSuccessHandler = new List<Action<string>>();
            YZFailureHandler = new List<Action<int, string>>();
            YZHeaders = new JsonData();
            YZForms = new JsonData();
        }

        public YZServerRequest(string url, string method)
        {
            if (url.StartsWith("http"))
            {
                YZUrl = url;
            }
            else
            {
                YZUrl = YZString.Concat(YZServerUtil.GetYZServerURL(), url);
            }

            YZMethod = method;
            YZHandler = null;
            YZUrlPath = url;
            YZSuccessHandler = new List<Action<string>>();
            YZFailureHandler = new List<Action<int, string>>();
            YZHeaders = new JsonData();
            YZForms = new JsonData();
        }

        // public void AddYZDefaultParam()
        // {
        //     AddYZParam(YZUdid, YZNativeUtil.GetYZUDID());
        //     AddYZParam(YZVersion, YZDefineUtil.GetYZVersionCode());
        //     AddYZParam(YZBundleId, YZNativeUtil.GetYZPackageName());
        //     AddYZParam(YZTimestamp, YZTimeUtil.GetYZTimestampUTC().ToString());
        // }

        // public void AddYZLoginParams()
        // {
        //     AddYZParam("timezone", YZServerApi.Shared.YZTimeZone);
        //     AddYZParam("language", YZServerApi.Shared.YZLanguage);
        //     AddYZParam("device_info", YZServerApi.Shared.GetYZDeviceInfo());
        //     AddYZParam("distinct_id", YZFunnelUtil.DistinctID);
        //     AddYZParam("appsflyer_id", YZNativeUtil.GetYZAFID());
        //     var matchId = YZDataUtil.GetLocaling(YZConstUtil.YZGameBeginMatchIdStr);
        //     if (!string.IsNullOrEmpty(YZPlayerDataUtil.Shared.YZBingoGameCache) && !string.IsNullOrEmpty(matchId))
        //     {
        //         JsonData gameCacheData = JsonMapper.ToObject(YZPlayerDataUtil.Shared.YZBingoGameCache);
        //         JsonData newData = new JsonData();
        //         newData["match_id"] = matchId;
        //         newData["game_score"] = gameCacheData["score"];
        //         newData["operations"] = gameCacheData["operations"];
        //         AddYZParam("game_cache", newData.ToJson());
        //     }
        //
        //     //上传本地归因数据
        //     if (string.Equals(PlayerManager.Shared.User.YZOrganic, YZOrganic.YZNONORGANIC))
        //         AddYZParam("is_organic", 0);
        //     else if (string.Equals(PlayerManager.Shared.User.YZOrganic, YZOrganic.YZORGANIC))
        //         AddYZParam("is_organic", 1);
        //     else
        //         AddYZParam("is_organic", -1);
        // }

        public void ClearYZ()
        {
            if (YZCoroutineRequest != null)
            {
                YZServerApi.Shared.StopCoroutine(YZCoroutineRequest);
            }

            YZHandler = null;
        }

        public YZServerRequest SetYZHeader(string name, string value)
        {
            YZHeaders[name] = value;
            return this;
        }

        public YZServerRequest AddYZParam(string name, string value)
        {
            YZForms[name] = value;
            return this;
        }

        public YZServerRequest AddYZParam(string name, int value)
        {
            YZForms[name] = value.ToString();
            return this;
        }

        public YZServerRequest SetYZUseAuth(bool use)
        {
            YZUseAuth = use;
            return this;
        }

        public YZServerRequest AddYZSuccessHandler(Action<string> handler)
        {
            YZSuccessHandler.Add(handler);
            return this;
        }

        public YZServerRequest AddYZFailureHandler(Action<int, string> handler)
        {
            YZFailureHandler.Add(handler);
            return this;
        }

        public void ExecYZSuccessHandler(string data)
        {
            foreach (Action<string> handler in YZSuccessHandler)
            {
                if (handler != null)
                {
#if UNITY_EDITOR
                    handler(data);
#else
                try
                {
                    handler(data);
                }
                catch (Exception e)
                {
                    YZDebug.LogConcat("Exception: ", e.Message);
                }
#endif
                }
            }
        }

        public void ExecYZFailureHandler(int code, string msg)
        {
            foreach (Action<int, string> handler in YZFailureHandler)
            {
                if (handler != null)
                {
                    try
                    {
                        handler(code, msg);
                    }
                    catch (Exception e)
                    {
                        YZDebug.LogConcat("Exception: ", e.Message);
                    }
                }
            }
        }

        public YZServerRequest SetYZRetryTime(int retry)
        {
            YZRetryRemain = retry;
            return this;
        }

        private void BuildYZHeader()
        {
            IDictionary dict = YZHeaders as IDictionary;
            foreach (string key in dict.Keys)
            {
                string value = (string)YZForms[key];
                YZRequest.SetRequestHeader(key, value);
            }
        }

        [Obsolete]
        private void BuildYZParams()
        {
            List<string> YZParamsStrings = new List<string>();
            List<string> YZKeys = new List<string>();
            IDictionary YZDict = YZForms as IDictionary;
            foreach (string YZKey in YZDict.Keys)
            {
                if (YZKey == "signature")
                {
                    continue;
                }

                string atsrvalue = (string)YZForms[YZKey];
                string atsrKeyValue = YZString.Concat(YZKey, "=", atsrvalue);
                if (YZKeys.Count == 0)
                {
                    YZKeys.Add(YZKey);
                    YZParamsStrings.Add(atsrKeyValue);
                }
                else
                {
                    bool YZInserted = false;
                    for (int i = 0; i < YZKeys.Count; i++)
                    {
                        if (YZKey.CompareTo(YZKeys[i]) < 0)
                        {
                            YZKeys.Insert(i, YZKey);
                            YZParamsStrings.Insert(i, atsrKeyValue);
                            YZInserted = true;
                            break;
                        }
                    }

                    if (YZInserted == false)
                    {
                        YZKeys.Add(YZKey);
                        YZParamsStrings.Add(atsrKeyValue);
                    }
                }
            }

            string YZToHash = YZString.Concat(string.Join("&", YZParamsStrings), YZDefineUtil.GetYZServerKey());
            // 编辑信息接口不能打印日志
            if (!YZUrl.Contains("upload/set_info"))
            {
                YZDebug.LogConcat("Request Begin: ", YZUrl, "?", YZToHash);
                YZGameUtil.PrintBuglyLog(YZString.Concat("Request Begin: ", YZUrl, "?", YZToHash));
            }

            if (YZUseAuth)
            {
                YZToHash = YZString.Concat(PlayerManager.Shared.User.YZPlayerAuth, YZToHash);
                //YZRequest.SetRequestHeader("authorization", PlayerManager.Shared.User.YZPlayerAuth);
            }

            string signature = Sha1YZSignature(YZToHash);
            YZForms["signature"] = signature;
            YZParamsStrings.Add(YZString.Concat("signature=", signature));
            YZRequest.method = YZMethod;
            if (YZMethod.ToLower().Equals("get"))
            {
                YZRequest.uri = new Uri(YZString.Concat(YZUrl, "?", string.Join("&", YZParamsStrings)));
            }
            else
            {
                YZRequest.uri = new Uri(YZUrl);
                YZRequest.SetRequestHeader("Content-Type", "application/json");
                YZRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(YZForms.ToJson()));
            }
        }

        private string Sha1YZSignature(string str)
        {
            var buffer = Encoding.UTF8.GetBytes(str);
            var data = SHA1.Create().ComputeHash(buffer);
            StringBuilder sub = YZString.GetShareStringBuilder();
            foreach (var t in data)
            {
                sub.Append(t.ToString("X2"));
            }

            return sub.ToString().ToLower();
        }

        [Obsolete]
        public void SendYZ(int timeout = YZTimeoutNum)
        {
            YZRequest = new UnityWebRequest();
            BuildYZHeader();
            BuildYZParams();
            YZRequest.redirectLimit = 64;
            YZRequest.timeout = timeout;
            YZRequest.downloadHandler = new DownloadHandlerBuffer();
            YZCoroutineRequest = YZServerApi.Shared.StartCoroutine(RequestYZ());
        }

        public void RetrySend()
        {
            YZRetryTimes += 1;
            //YZWaitingUICtrler.Shared().YZOnOpenUI();
            SendYZ();
        }

        public void CancelYZ()
        {
            YZIsCancel = true;
        }

        private IEnumerator RequestYZ()
        {
            yield return YZRequest.SendWebRequest();
            if (YZRetryTimes > 0)
            {
                //YZWaitingUICtrler.Shared().YZOnCloseUI();
            }

            if (!YZIsCancel)
            {
                if (IsYZFailured())
                {
                    if (YZRetryRemain > 0)
                    {
                        YZDebug.LogConcat("开始重试请求: ", YZTimeUtil.GetYZTimestamp(), ": ", YZUrl);
                        int timeout = YZRequest.timeout;
                        YZRequest.Dispose();
                        YZRequest = null;
                        YZRetryRemain--;
                        SendYZ(timeout);
                    }
                    else
                    {
                        HanderYZEvents(YZRequest, YZRequest.responseCode, YZRequest.error);
                    }
                }
                else
                {
                    HanderYZEvents(YZRequest, YZRequest.responseCode, YZRequest.downloadHandler.text);
                }
            }
            else
            {
                YZDebug.LogConcat("请求已被取消: ", YZRequest.url);
            }
        }

        private void HanderYZEvents(UnityWebRequest request, long code, string text)
        {
            if (YZHandler != null)
            {
                YZHandler?.Invoke(code, text, this);
            }

            request.Dispose();
        }

        /// 请求是否失败了
        private bool IsYZFailured()
        {
            if (YZRequest.isHttpError || // 返回错误
                YZRequest.isNetworkError || // 网络错误
                YZRequest.isDone == false || // 没有完成
                YZRequest.responseCode == 0 || // 响应码错误
                YZRequest.downloadHandler == null || // 没有返回内容
                YZRequest.downloadHandler.isDone == false || // 没有接收完成
                string.IsNullOrEmpty(YZRequest.downloadHandler.text)) // 没有返回文本                             
            {
                return true;
            }

            return false;
        }
    }
}