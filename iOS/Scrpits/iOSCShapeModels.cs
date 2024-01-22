using System;
using UnityEngine;
using Utils;

namespace iOSCShape
{
    [Serializable]
    public class ShareYZDefaultResponse
    {
        // 是否完成 1.完成 0.未完成
        public string completed;

        // 分享类型
        // UIActivityTypePostToFacebook
        // UIActivityTypePostToTwitter
        // UIActivityTypePostToWeibo
        // UIActivityTypeMessage
        // UIActivityTypeMail
        // UIActivityTypePrint
        // UIActivityTypeCopyToPasteboard
        // UIActivityTypeAssignToContact
        // UIActivityTypeSaveToCameraRoll
        // UIActivityTypeAddToReadingList
        // UIActivityTypePostToFlickr
        // UIActivityTypePostToVimeo
        // UIActivityTypePostToTencentWeibo
        // UIActivityTypeAirDrop
        // UIActivityTypeOpenInIBooks
        // UIActivityTypeMarkupAsPDF
        public string activityType;
    }

    [Serializable]
    public class ShareYZDefaultParams
    {
        public string content;
        public string title;
    }

    [Serializable]
    public class YZLocationParams
    {
        public YZLocationAccuracy accuracy;
        public float distance;
        public YZLocationParams(YZLocationAccuracy accuracy, float distance)
        {
            this.distance = distance;
            this.accuracy = accuracy;
        }
    }

    [Serializable]
    public class YZPlacemark
    {
        /// eg. Apple Inc.
        public string name;
        /// street name, eg. Infinite Loop
        public string thoroughfare;
        /// eg. 1
        public string subThoroughfare;
        /// city, eg. Cupertino
        public string locality;
        /// neighborhood, common name, eg. Mission District
        public string subLocality;
        /// state, eg. CA
        public string administrativeArea;
        /// county, eg. Santa Clara
        public string subAdministrativeArea;
        /// zip code, eg. 95014
        public string postalCode;
        /// eg. US
        public string ISOcountryCode;
        /// eg. United States
        public string country;
        /// eg. Lake Tahoe
        public string inlandWater;
        /// eg. Golden Gate Park
        public string ocean;

        public YZPlacemark(string area, string code)
        {
            ISOcountryCode = code;
            administrativeArea = area;
        }

        public YZPlacemark()
        {

        }
    }

    [Serializable]
    public class SYZPlacemark
    {
        public string address;
    }

    [Serializable]
    public class SYZAddress
    {
        public string road;
        public string county;
        public string state;
        public string postcode;
        public string country;
        public string country_code;
    }

    [Serializable]
    public class YZLocation
    {
        public double latitude;
        public double longitude;
    }

    [Serializable]
    public class YZInAppWebViewParams
    {
        public string url;
        public string topBarColor;
        public string title;
        public string titleSize;
        public string progressColor;
        public string progressBgColor;
        public string tips;
        public string ok;

        public YZInAppWebViewParams() { }
        public YZInAppWebViewParams(string url, string topBarColor, string title, string titleSize, string progressColor, string progressBgColor, string tips, string ok)
        {
            this.url = url;
            this.topBarColor = topBarColor;
            this.title = title;
            this.titleSize = titleSize;
            this.progressColor = progressColor;
            this.progressBgColor = progressBgColor;
            this.tips = tips;
            this.ok = ok;
        }
    }

    [Serializable]
    public class YZInAppSafariParams
    {
        public string url;
        public string barTintColor;
        public string controlTintColor;
        public YZInAppSafariParams(string url, string barTintColor, string controlTintColor)
        {
            this.url = url;
            this.barTintColor = barTintColor;
            this.controlTintColor = controlTintColor;
        }
    }

    [Serializable]
    public class YZInAppNewsParams
    {
        public string url1;
        public string url2;
        public string url3;
        public string top_txt;
        public string btn_txt_1;
        public string btn_txt_2;
        public string btn_txt_3;
    }

    [Serializable]
    public class YZLoadingStatus
    {
        public string url;
        public string loading;
    }

    [Serializable]
    public class YZPickerImageParam
    {
        public string choosecamare;
        public string choosephotos;
        public string cancel;
    }

    [Serializable]
    public class YZAppsflyer
    {
        public string status;
        public string source;
        public string invite;
        public string campaign;
    }

    [Serializable]
    public class YZKYCParam
    {
        public string token;
        public string debug;
    }

    [Serializable]
    public class YZApplePay
    {
        // 支付金额
        public double amount;
        // 商业id
        public string merchant;
        // 标题
        public string title;

        public YZApplePay(double cash)
        {
            amount = cash;
            merchant = YZDefineUtil.GetYZApplePayId();
            title = Application.productName;
        }
    }

    [Serializable]
    public class YZAppleAuth
    {
        public string str;
        public string bas;
    }

    [Serializable]
    public class YZiOSEmail
    {
        public string id;
        public string version;
        public string code;
        public string subject;
        public string title;
        public string email;
        public string pos;
    }

    public class YZVibrationType
    {
        public const string None = "None";      // 默认长振动

        public const string Light = "Light";    // 轻震动(手指放到按钮上)
        public const string Medium = "Medium";  // 中振动
        public const string Heavy = "Heavy";    // 重振动

        public const string Success = "Success";// 操作成功，振动反馈
        public const string Warning = "Warning";// 操作警告，振动反馈
        public const string Error = "Error";    // 操作失败，振动反馈

        public const string Changed = "Changed";// 手指移动，振动反馈
    }
}