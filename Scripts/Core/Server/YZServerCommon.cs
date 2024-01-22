using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Core.Controllers;
using Core.Extensions;
using Core.Manager;
using DataAccess.Model;
using Facebook.Unity;
using UnityEngine;
using UnityEngine.Networking;
using Utils;

namespace Core.Server
{
    public class YZServerCommon : YZBaseController<YZServerCommon>
    {
        private Dictionary<string, string> YZDicForms = new Dictionary<string, string>();

        private string YZSignature = null;

        private void CreateYZSignature(string url)
        {
            YZDicForms = YZDicForms.OrderBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);
            List<string> YZParams = new List<string>();
            List<string> YZKeys = new List<string>();
            foreach (string key in YZDicForms.Keys)
            {
                string value = WebUtility.UrlEncode(YZDicForms[key]);
                string keyValue = YZString.Concat(key, "=", value);
                YZParams.Add(keyValue);
            }

            string atsAppkey = YZString.Concat("app_key", "=", YZDefineUtil.GetYZGenaralAppKey());
            YZParams.Add(atsAppkey);
            string atsOrigStr = string.Join("&", YZParams);
            YZDebug.LogConcat("Request Begin Server Url: ", url, " params: ", atsOrigStr);
            YZGameUtil.PrintBuglyLog(YZString.Concat("Request Begin Server Url: ", url, " params: ", atsOrigStr));
            string string_to_sign = YZString.Concat(atsOrigStr, YZDefineUtil.GetYZGenaralSecretKey()).ToLower();
            YZSignature = YZGameUtil.EncryptYZString(string_to_sign).ToUpper();
        }

        private WWWForm CreatYZWWWForm()
        {
            WWWForm YZForm = new WWWForm();
            foreach (string key in YZDicForms.Keys)
            {
                string value = YZDicForms[key];
                YZForm.AddField(key, value);
            }

            YZForm.AddField("app_key", YZDefineUtil.GetYZGenaralAppKey());
            YZForm.AddField("signature", YZSignature);
            return YZForm;
        }

        private void AddYZDefaultParams()
        {
            YZDicForms.Add("idfa", YZNativeUtil.GetYZIDFA());
            YZDicForms.Add("nonce", YZGameUtil.GetYZRandomString(6, 16));
            YZDicForms.Add("af_id", YZNativeUtil.GetYZAFID());
            YZDicForms.Add("version", YZNativeUtil.GetVersionCode().ToString());
            YZDicForms.Add("timezone", YZNativeUtil.GetYZTimeZone());
            YZDicForms.Add("bundle_id", YZNativeUtil.GetYZPackageName());
            YZDicForms.Add("timestamp", YZTimeUtil.GetYZTimestampUTC().ToString());
            YZDicForms.Add("user_id", Root.Instance.Role.user_id.ToString());
        }

        private UnityWebRequest GetYZRequest(string url, WWWForm form = null)
        {
            if (form == null)
                return UnityWebRequest.Get(url);
            else
                return UnityWebRequest.Post(url, form);
        }

        private void SendYZRequest(string url, Action<int, string> callback, WWWForm form = null)
        {
            var request = GetYZRequest(url, form);
            // YZLog.LogColor("请求通用后台信息 :" + request.url);
            StartCoroutine(RequestYZ(request, callback));
        }

        private IEnumerator RequestYZ(UnityWebRequest request, Action<int, string> finish = null)
        {
            yield return request.SendWebRequest();
            string text = request.downloadHandler.text ?? "";
            int code;
            if (request.isNetworkError || request.isHttpError)
            {
                code = 0;
                text = request.error;
            }
            else if (text == null)
            {
                code = 1;
            }
            else
            {
                code = 200;
            }

            YZDebug.LogConcat("Request Ended Server Url: ", request.url, " Content: ", text);
            YZGameUtil.PrintBuglyLog(YZString.Concat("Request Ended Server Url: ", request.url, " Content: ", text));
            finish?.Invoke(code, text);
            request.Dispose();
        }
        

        /// 登录通用后台
        public void SendYZLogin(Action<string> callback = null)
        {
            YZDicForms.Clear();
            AddYZDefaultParams();

            string finalURL = YZString.Concat(YZDefineUtil.GetYZGenaralURL(), "api/login");
            CreateYZSignature(finalURL);

            WWWForm form = CreatYZWWWForm();
            SendYZRequest(finalURL, (int code, string response) =>
            {
                YZDebug.LogConcat("server response: ", response);
                callback?.Invoke(response);
            }, form);
        }

        /// 充值
        /// purchase_id     内购事件ID，如：123456789
        /// purchase_name   内购事件名称，如：hammer_3
        /// type            内购类型，如：1，具体分类数值将根据不同的应用制定
        /// revenue         本次内购产生的收入金额，如：0.5。精确到小数点后6位（超出会舍入）
        public void SendYZPurchaseWorth(string id, string name, string type, string revenue)
        {
            // // 测试服，不上传
            // if (!YZServerUtil.GetNeedSendValue())
            //     return;

            // 上传AF打点
            YZFunnelUtil.SendYZAppsflyerValue(FunnelAF.brafpurchase, revenue);

            // // 上传AF首冲打点
            // if (PlayerManager.Shared.Player.Other.count.total_charge_times <= 1)
            //     YZFunnelUtil.SendYZEvent(FunnelEventID.brfirstpay);

            // 上传后台
            YZDicForms.Clear();
            YZDicForms.Add("purchase_id", id);
            YZDicForms.Add("purchase_name", name);
            YZDicForms.Add("type", type);
            YZDicForms.Add("revenue", revenue);
            AddYZDefaultParams();

            string finalURL = YZString.Concat(YZDefineUtil.GetYZGenaralURL(), "api/purchase_worth");
            CreateYZSignature(finalURL);

            WWWForm form = CreatYZWWWForm();
            SendYZRequest(finalURL, (int code, string response) => { YZDebug.LogConcat("充值通用后台返回: ", response); },
                form);

            // 上传FB
            Dictionary<string, object> iapParameters = new Dictionary<string, object>();
            iapParameters["mygame_packagename"] = YZNativeUtil.GetYZPackageName();
            float rev = float.Parse(revenue);
            Debug.Log("wjs facebook log purchase : packagename = " + iapParameters["mygame_packagename"] + " revenue = " + rev);
            FB.LogPurchase(rev, "USD", iapParameters);
        }

        /// <summary>
        /// 广告价值
        /// </summary>
        /// <param name="adpos"></param>
        /// <param name="revenue"></param>
        /// <param name="mediation"></param>
        public void SendYZWorth(string adpos, string revenue, string mediation = "mopub")
        {
            // 上传af
            YZFunnelUtil.SendYZAppsflyerValue(FunnelAF.bradrevenue, revenue);

            // // 上传后台
            // if (string.IsNullOrEmpty(impression))
            // {
            //     impression = "{}";
            // }
            //
            // YZDicForms.Clear();
            // YZDicForms.Add("impression_data", impression);
            // YZDicForms.Add("type", adpos.ToString());
            // YZDicForms.Add("mediation", mediation);
            // AddYZDefaultParams();
            //
            // string finalURL = YZString.Concat(YZDefineUtil.GetYZGenaralURL(), "api/ad_worth");
            // CreateYZSignature(finalURL);
            //
            // WWWForm form = CreatYZWWWForm();
            // SendYZRequest(finalURL, (int code, string response) => { YZDebug.LogConcat(">>>>>回传广告价值返回: ", response); },
            //     form);
        }

        /// 进入一局游戏时，发送游戏事件
        public void SendYZGameEvent(string room_param)
        {
            YZDicForms.Clear();
            YZDicForms.Add("content", room_param);

            AddYZDefaultParams();

            string finalURL = YZString.Concat(YZDefineUtil.GetYZGenaralURL(), "api/game_event");
            CreateYZSignature(finalURL);

            WWWForm form = CreatYZWWWForm();
            // YZLog.LogColor("通用后台 SendYZRequest");
            SendYZRequest(finalURL,
                (int code, string response) => { YZDebug.LogConcat("向通用后台发送游戏事件数据的返回: ", response); }, form);
        }
    }
}