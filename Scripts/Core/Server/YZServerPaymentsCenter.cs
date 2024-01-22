using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Core.Controllers;
using DataAccess.Model;
using DataAccess.Utils;
using LitJson;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Utils;

namespace Core.Server
{
    public class YZServerPaymentsCenter : YZBaseController<YZServerPaymentsCenter>
    {
        private Dictionary<string, string> YZDicForms = new Dictionary<string, string>();

        private string YZSignature = null;
        private string YZFinalUrl = null;

        private PaymentCallBack onPaymentCallBack;

        private void CreateYZSignature()
        {
            // 1. 将所有参数按字母顺序排序
            YZDicForms = YZDicForms.OrderBy(p => p.Key).ToDictionary(p => p.Key, o => o.Value);
            // 2. 遍历所有参数，base64编码之后，拼接在一起
            string old = string.Empty;
            string str64 = string.Empty;
            foreach (string key in YZDicForms.Keys)
            {
                old = YZString.Concat(old, key, "=", WebUtility.UrlEncode(YZDicForms[key]), "&");
                str64 = YZString.Concat(str64, YZGameUtil.Base64YZEncode(key),
                    YZGameUtil.Base64YZEncode(YZDicForms[key]), "&");
            }

            old = YZString.Concat(old, YZServerUtil.GetYZChargeCenterAppSecret());
            str64 = YZString.Concat(str64, YZServerUtil.GetYZChargeCenterAppSecret());
            YZDebug.LogConcat("Request Begin Despoit Url: ", YZFinalUrl, "?", old);
            YZGameUtil.PrintBuglyLog(YZString.Concat("Request Begin Despoit Url: ", YZFinalUrl, "?", old));
            YZSignature = YZGameUtil.EncryptYZString(str64).ToLower();
        }

        private WWWForm CreatYZWWWForm()
        {
            WWWForm atsform = new WWWForm();
            foreach (string key in YZDicForms.Keys)
            {
                string value = YZDicForms[key];
                atsform.AddField(key, value);
            }

            atsform.AddField("signature", YZSignature);
            return atsform;
        }

        private void AddYZDefaultParams()
        {
            YZDicForms.Add("nonce", YZGameUtil.GetYZRandomString(6, 16));
            YZDicForms.Add("timezone", YZNativeUtil.GetYZTimeZone());
            YZDicForms.Add("timestamp", YZTimeUtil.GetYZTimestampUTC().ToString());
            YZDicForms.Add("app_key", YZServerUtil.GetYZChargeCenterAppKey());
            
            // 风控需要的参数
            var extra_params = new JsonData
            {
                ["user_agent"] = YZServerUtil.GetYZUserAgent(),
                ["platform"] = YZServerUtil.GetYZOperatingSystem(),
//#if UNITY_IOS
//                ["platform"] = "iOS",
//#elif UNITY_ANDROID
//                ["platform"] = "Android",
//#endif
                ["app_version"] = Application.version,
                //设备品牌，例如: Samsung
                //["device_brand"] = "",
                ["device_model"] = SystemInfo.deviceModel,
                //顶级行政区划 - 州/省/部门/等。可以是缩写格式或全名。例如:NY / New York
                ["region"] = DeviceInfoUtils.Instance.GPSInfo != null ? DeviceInfoUtils.Instance.GPSInfo.province : string.Empty,
                ["city"] = DeviceInfoUtils.Instance.GPSInfo != null ? DeviceInfoUtils.Instance.GPSInfo.city : string.Empty,
                //邮政编码，例如: 12345
                //["zip"] = "",
                ["register_time"] = Root.Instance.RegisterTime.ToString(),
                //用户支付成功的笔数，例如:30
                ["past_orders_count"] = Root.Instance.ChargeSuccessCount.ToString(),
                //用户支付成功的总金额,单位:美元，例如:500
                ["past_orders_amount"] = Root.Instance.ChargeInfo.success_total.ToString(),
                ["last_login_ip"] = YZDebug.GetLastIP(),
                //注册IP，例如:4.3.2.1
                ["register_ip"] = Root.Instance.Role.first_ip,
                ["uuid"] = YZDebug.GetForterRelatedUUID(),
                ["charge_id"] = UIChargeCtrl.Shared().CurrentChargeID.ToString(),
                //抵扣券的code，例如:FATHERSDAY2015
                //["coupon_used"] = "",
                ["account_id"] = YZDebug.GetAccountId(),
                ["character_name"] = Root.Instance.Role.nickname,
                ["player_level"] = "0",
                ["vip_level"] = Root.Instance.Role.VipLevel.ToString(),
                ["game_name"] = YZDebug.GetGameNameLower(),
                ["game_item_amount"] = UIChargeCtrl.Shared().ChargeItem.amount,
                ["mobile_uid"] = YZDebug.GetForterId(),
                ["language"] = DeviceInfoUtils.Instance.GetLanguage(),
                //客户的生日，例如:2022-11-10
                //["date_of_birth"] = "",
                ["client_ips"] = YZDebug.GetIPV4V6(),
                ["email"] = Root.Instance.UserInfo.email,
                //从同盾客户端SDK取得的black box参数的值 (短字符串形式)。接同盾风控的应用必传
                //["black_box"] = "",
                //用户注册至今，一共参加游戏的场数。接同盾支付风控的应用必传
                //["total_game_count"] = "",
                //用户注册至今，获得胜利的游戏场数。接同盾支付风控的应用必传
                //["winning_game_count"] = "",
                //用户注册至今的胜率，胜利场数/游戏场数。接同盾支付风控的应用必传
                //["winning_rate"] = "",
                //用户注册至今，赢得金币的总量。接同盾支付风控的应用必传
                //["winning_coin_count"] = "",
                //枚举值:
                //-1 KYC未通过 (例如:做了KYC，但未能通过)
                //0暂无KYC信息 (例如:还没有做过KYC)
                //1普通信息KYC(例如:仅通过KYC-AGE的验证，如goecomply的信息验证)
                //2有证件的KYC(例如:通过KYC-ID的验证，如sumsub传证件和人脸的)
                //3有地址的KYC(例如:通过KYC-POA的验证，如sumsub带地址证明的)
                //["kyc_status"] = "",
            };
            
            YZDicForms.Add("extra_params", extra_params.ToJson());
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

            YZDebug.LogConcat("Request Ended Despoit Url: ", request.url, " Content: ", text);
            YZGameUtil.PrintBuglyLog(YZString.Concat("Request Ended Despoit Url: ", request.url, " Content: ", text));
            finish?.Invoke(code, text);
            request.Dispose();
        }


        //-------------------------------充值中心 start--------------------------------
        /// 请求一次支付
        public void RequestPayments(string strDetails, PaymentCallBack CallBack)
        {
            // 情况缓存信息并添加默认参数
            YZDicForms.Clear();
            AddYZDefaultParams();
            // 载入特有参数
            AddPaymentsParams(strDetails);
            // 拼接url并签名
            CreateYZSignature();
            // 发送请求
            WWWForm form = CreatYZWWWForm();
            SendYZRequest(YZFinalUrl, (int code, string response) =>
            {
                YZDebug.LogConcat("向充值中心发送数据的返回: ", response);
                CallBack.Invoke(code, response);
            }, form);
        }

        /// 载入支付信息
        private void AddPaymentsParams(string strParams)
        {
            // 1. 解析
            PaymentsDetails details = YZGameUtil.JsonYZToObject<PaymentsDetails>(strParams);
            if (details != null)
            {
                // 1.1 获取url
                YZFinalUrl = details.charge_center_url;

                // 1.2 获取各个参数
                for (int i = 0; i < details.details.Count; ++i)
                {
                    YZDicForms.Add(details.details[i].param_name, details.details[i].param_value);
                }
            }
        }
        //-------------------------------充值中心 end----------------------------------
    }

    [Serializable]
    public class PaymentsDetails
    {
        public string charge_center_url;
        public List<DetailItem> details;

        public PaymentsDetails()
        {
            details = new List<DetailItem>();
        }
    }

    [Serializable]
    public class DetailItem
    {
        public string param_name;
        public string param_value;
    }
}