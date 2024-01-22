using LitJson;
using System;
using System.Collections.Generic;
using Core.Extensions.UnityComponent;
using Core.Models;
using Core.Server;
using Core.Services.UserInterfaceService.Internal;
using DataAccess.Controller;
using DataAccess.Model;
using Reactive.Bindings;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.UIChargeFlow
{
    public class UIChargeCardInfo : UIBase<UIChargeCardInfo>
    {
        #region 需要控制的子控件

        [SerializeField] private MyButton brbtnback;
        [SerializeField] private Text brtxttitle;
        [SerializeField] private MyButton brbtnsubmit;
        [SerializeField] private Text brtxtbtnsubmit;
        [SerializeField] private InputField brinputfirstname;
        [SerializeField] private Text brtxtfirstnameplaceholder;
        [SerializeField] private InputField brinputlastname;
        [SerializeField] private Text brtxtlastnameplaceholder;
        [SerializeField] private InputField brinputcardnumber;
        [SerializeField] private Text brtxtcardnumberplaceholder;
        [SerializeField] private InputField brinputmmyy;
        [SerializeField] private InputField brinputcvc;
        [SerializeField] private Text brtxtcvcplaceholder;
        [SerializeField] private InputField brinputemail;
        [SerializeField] private Text brtxtemailplaceholder;
        [SerializeField] private Text brtxterrorcvc;
        [SerializeField] private Text brtxterrormmyy;
        [SerializeField] private Dropdown brdroplist_year;
        [SerializeField] private Dropdown brdroplist_month;

        #endregion

        #region 私有变量

        private static UIChargeCardInfo Inst;
        private CreditCardDetails brcachedetails;
        private string brcardnumber;
        private string bryear;
        private string brmonth;
        private bool bristhisyear;

        #endregion

        public static UIChargeCardInfo Shared()
        {
            return Inst;
        }

        public override void InitEvents()
        {
        }

        public override void OnStart()
        {
            //按钮回调
            brbtnback.onClick.AddListener(Close);
            brbtnsubmit.onClick.AddListener(YZOnClickSubmitBtn);
            brinputfirstname.onEndEdit.AddListener(YZOnInputEditEnd);
            brinputfirstname.onValueChanged.AddListener(YZOnValueChangeFirstName);
            brinputlastname.onEndEdit.AddListener(YZOnInputEditEnd);
            brinputlastname.onValueChanged.AddListener(YZOnValueChangeLastName);
            brinputcardnumber.onEndEdit.AddListener(YZOnInputEditEnd);
            brinputcardnumber.onValueChanged.AddListener(YZOnValueChangeCardnumber);
            brinputmmyy.onEndEdit.AddListener(YZOnInputEditEnd);
            brinputmmyy.onValueChanged.AddListener(YZOnValueChangeMmyy);
            brinputcvc.onEndEdit.AddListener(YZOnInputEditEnd);
            brinputemail.onEndEdit.AddListener(YZOnInputEditEnd);
            brinputemail.onValueChanged.AddListener(YZOnValueChangeEmail);
            brdroplist_year.onValueChanged.AddListener(YZOnValueChangeYearMonth);
            brdroplist_month.onValueChanged.AddListener(YZOnValueChangeYearMonth);
            DidOpenUI();

            Inst = this;
        }
        
        public override void InitVm()
        {
        }

        public override void InitBinds()
        {
        }

        public void YZOnInputEditEnd(string str)
        {
            YZCheckCanSubmit();
        }


        public void DidOpenUI()
        {
            // 查询可用的充值渠道
            MediatorRequest.Instance.GetChargeMethods();
            
            //
            brtxterrorcvc.gameObject.SetActive(false);
            brtxterrormmyy.gameObject.SetActive(false);

            //-- 初始化
            this.brinputfirstname.text = "";
            this.brinputlastname.text = "";
            this.brinputcardnumber.text = "";
            this.brinputmmyy.text = "";
            this.brinputcvc.text = "";
            this.brinputemail.text = "";
            InitDropList();
            //-- 是否有充过值的信用卡信息
            var saveData = YZDataUtil.GetCreditCardData();
            if (saveData != null)
            {
                brinputfirstname.text = YZJsonUtil.GetYZString(saveData, "first_name");
                brinputlastname.text = YZJsonUtil.GetYZString(saveData, "last_name");
                brinputcardnumber.text = YZJsonUtil.GetYZString(saveData, "card_number");
                //brinputmmyy.text = YZJsonUtil.GetYZString(saveData, "mmyy");
                brinputcvc.text = YZJsonUtil.GetYZString(saveData, "cvc");
                brinputemail.text = YZJsonUtil.GetYZString(saveData, "email");
                //add 20230412
                SetDropListValue(saveData);
            }

            YZCheckCanSubmit();
        }

        /// <summary>
        /// 初始化下拉列表
        /// </summary>
        private void InitDropList()
        {
            // year
            List<string> ps = new List<string>();
            var thisYear = DateTime.Now.Year;
            for (int i = 0; i < 10; ++i)
            {
                ps.Add(YZString.Concat(thisYear + i));
            }

            brdroplist_year.ClearOptions();
            brdroplist_year.AddOptions(ps);
            // month
            ps.Clear();
            for (int i = 1; i <= 12; ++i)
            {
                ps.Add(i.ToString("D2"));
            }

            brdroplist_month.ClearOptions();
            brdroplist_month.AddOptions(ps);

            SetDropListValue(null);
        }

        /// <summary>
        /// 用缓存的信用卡数据初始化下拉列表
        /// </summary>
        /// <param name="json"></param>
        private void SetDropListValue(JsonData json)
        {
            if (json != null)
            {
                brdroplist_year.SetValueWithoutNotify(YZJsonUtil.GetYZInt(json, "yearValue"));
                brdroplist_month.SetValueWithoutNotify(YZJsonUtil.GetYZInt(json, "monthValue"));
            }
        }

        /// <summary>
        /// 检查是否可以上传
        /// </summary>
        private bool YZCheckCanSubmit()
        {
            bool isCanSubmit = false;

            if (this.YZCheckData(brinputcardnumber) &&
                this.YZCheckData(brinputcvc) &&
                this.CheckYearMonth() &&
                this.YZCheckData(brinputfirstname) &&
                this.YZCheckData(brinputlastname) &&
                this.YZCheckData(brinputemail))
                isCanSubmit = true;


            brbtnsubmit.interactable = isCanSubmit;

            return isCanSubmit;
        }

        private bool CheckYearMonth()
        {
            var thisYear = DateTime.Now.Year;
            var thisMonth = DateTime.Now.Month;
            var valid = true;
            //-- 1.先检测年份
            var intYear = brdroplist_year.captionText.text.ToInt();
            if (intYear == thisYear)
            {
                bristhisyear = intYear == thisYear;
            }
            else
            {
                valid = true;
                brtxterrormmyy.gameObject.SetActive(!valid);
                return valid;
            }

            //-- 2.再检测月份
            if (valid)
            {
                var intMonth = brdroplist_month.captionText.text.ToInt();
                if (bristhisyear)
                {
                    if (intMonth < thisMonth)
                    {
                        valid = false;
                    }
                }
            }

            brtxterrormmyy.gameObject.SetActive(!valid);
            return valid;
        }

        private bool YZCheckData(InputField input)
        {
            if (input == brinputcardnumber)
            {
                if (string.IsNullOrEmpty(brcardnumber)) return false;
            }
            else if (input == brinputcvc)
            {
                var strLength = brinputcvc.text.Length;
                if (strLength < 3 || strLength > 4)
                {
                    brtxterrorcvc.gameObject.SetActive(true);
                    return false;
                }

                brtxterrorcvc.gameObject.SetActive(false);
            }
            else if (input == brinputfirstname)
            {
                return !string.IsNullOrEmpty(brinputfirstname.text);
            }
            else if (input == brinputlastname)
            {
                return !string.IsNullOrEmpty(brinputlastname.text);
            }
            else if (input == brinputemail)
            {
                return !string.IsNullOrEmpty(brinputemail.text);
            }

            return true;
        }

        public void YZTryToSaveCreditCardData()
        {
            if (YZCheckCanSubmit())
            {
                var saveData = new JsonData();
                saveData["first_name"] = brinputfirstname.text;
                saveData["last_name"] = brinputlastname.text;
                saveData["card_number"] = brinputcardnumber.text;
                //saveData["mmyy"] = brinputmmyy.text;
                saveData["cvc"] = brinputcvc.text;
                saveData["email"] = brinputemail.text;
                //add 20230412
                saveData["year"] = brdroplist_year.captionText.text;
                saveData["yearValue"] = brdroplist_year.value;
                saveData["month"] = brdroplist_month.captionText.text;
                saveData["monthValue"] = brdroplist_month.value;

                YZDataUtil.SetYZString(YZConstUtil.YZDepositCreditCardDetailsStr, saveData.ToJson());

                // //todo:判断是否需要年龄验证
                // if (PlayerManager.Shared.Player.data.other.safety_data.age_verify == 0)
                // {
                //     YZServerSupportCenter.Shared.RequestAgeCheck(brcardnumber, brinputmmyy.text, brinputemail.text, YZNativeUtil.GetYZLocalCountryCode());
                // }
            }
        }

        /// <summary>
        /// 执行点击确认按钮的逻辑
        /// </summary>
        public void YZOnClickSubmitBtn()
        {
            //--提交信息
            if (brcachedetails == null)
            {
                brcachedetails = new CreditCardDetails();
            }

            brcachedetails.card_number = brcardnumber;
            brcachedetails.cvc = brinputcvc.text;
            brcachedetails.year = brdroplist_year.captionText.text;
            brcachedetails.month = brdroplist_month.captionText.text;
            brcachedetails.first_name = brinputfirstname.text;
            brcachedetails.last_name = brinputlastname.text;
            brcachedetails.email = brinputemail.text;

            YZServerApiCharge.Shared.YZRequestCreateOrder(UIChargeCtrl.Shared().CurrentChargeID,
                UIChargeCtrl.Shared().YZGetDiscountChargeID(), 
                UIChargeCtrl.Shared().YZGetDiscountChargeAmount(),
                UIChargeCtrl.Shared().cardNewPrice.ToString(),
                () =>
                {
                    YZServerApiCharge.Shared.YZDepositCreditCard(brcachedetails);
                }, "credit_card");
            
            // 优化功能？保存这次提交的信用卡信息 (之前只有成功充值了才保存）
            YZTryToSaveCreditCardData();
        }

        public void YZOnValueChangeFirstName(string text)
        {
            var srcStr = System.Text.RegularExpressions.Regex.Replace(text, @"[\s|']", "");
            brinputfirstname.text = srcStr.ToString();
            brinputfirstname.caretPosition = srcStr.Length;
            YZCheckCanSubmit();
        }

        public void YZOnValueChangeLastName(string text)
        {
            var srcStr = System.Text.RegularExpressions.Regex.Replace(text, @"[\s|']", "");
            brinputlastname.text = srcStr.ToString();
            brinputlastname.caretPosition = srcStr.Length;
            YZCheckCanSubmit();
        }

        public void YZOnValueChangeEmail(string text)
        {
            var srcStr = System.Text.RegularExpressions.Regex.Replace(text, @"\s", "");
            brinputemail.text = srcStr.ToString();
            brinputemail.caretPosition = srcStr.Length;
            YZCheckCanSubmit();
        }

        /// <summary>
        /// 信用卡输入框的输入回调
        /// </summary>
        /// <param name="text"></param>
        public void YZOnValueChangeCardnumber(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                brcardnumber = "";
                YZCheckCanSubmit();
                return;
            }

            //var srcStr = text.Replace("-", "");
            var srcStr = System.Text.RegularExpressions.Regex.Replace(text, @"[^\d]*", "");
            this.brcardnumber = srcStr;

            var srcStrLength = srcStr.Length;
            var insertIndex = 0;
            var targetStr = YZString.GetShareStringBuilder();

            if (srcStrLength >= 4)
            {
                while (insertIndex < srcStrLength)
                {
                    var endIndex = insertIndex + 4;
                    var subStr = string.Empty;
                    if (endIndex >= srcStrLength)
                    {
                        subStr = srcStr.Substring(insertIndex);
                    }
                    else
                    {
                        subStr = srcStr.Substring(insertIndex, 4);
                    }

                    if (string.IsNullOrEmpty(subStr))
                    {
                        break;
                    }
                    else
                    {
                        if (targetStr.Length > 0)
                        {
                            targetStr.Append("-");
                        }

                        targetStr.Append(subStr);
                    }

                    insertIndex += 4;
                }
            }
            else
            {
                targetStr.Append(srcStr);
            }

            brinputcardnumber.text = targetStr.ToString();
            brinputcardnumber.caretPosition = targetStr.Length;
            YZCheckCanSubmit();
        }

        /// <summary>
        /// 到期时间输入框回调
        /// </summary>
        /// <param name="text"></param>
        public void YZOnValueChangeMmyy(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                bryear = null;
                brmonth = null;
                YZCheckCanSubmit();
                return;
            }

            var srcStr = text.Replace("/", "");
            //
            var subOne = "";
            var subTwo = "";
            if (srcStr.Length > 2)
            {
                subOne = srcStr.Substring(0, 2);
                //
                if (srcStr.Length > 4)
                    subTwo = srcStr.Substring(2, 2);
                else
                    subTwo = srcStr.Substring(2);
            }
            else
                subOne = srcStr.Substring(0);

            var targetStr = YZString.GetShareStringBuilder();
            targetStr.Append(subOne);
            if (!string.IsNullOrEmpty(subTwo))
            {
                targetStr.Append("/").Append(subTwo);
            }

            brinputmmyy.text = targetStr.ToString();
            brinputmmyy.caretPosition = targetStr.Length;
            brmonth = subOne;
            bryear = YZString.Concat("20", subTwo);


            YZCheckCanSubmit();
        }

        /// <summary>
        /// 到期时间年月
        /// </summary>
        /// <param name="text"></param>
        public void YZOnValueChangeYearMonth(int index)
        {
            YZCheckCanSubmit();
        }
    }
}