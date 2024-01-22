using System;
using System.Collections.Generic;
using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.UIWithDrawFlow
{
    public class UIWithdrawName: UIBase<UIWithdrawName>
    {
        [SerializeField] private Button brbtnback;
        [SerializeField] private Button brbtnwrongemail;
        [SerializeField] private Text brtxtemail;

        [SerializeField] private MyButton brbutton;

        [SerializeField] private InputField inputField1;
        [SerializeField] private InputField inputField2;

        private GameObject brtxttitle;
        private GameObject brtxtdesc;
        private GameObject brtxtwrong;
        private GameObject brtxtcontinue;

        private InputField brtxtinputbirthdate;
        [SerializeField] private Dropdown brdroplist_year;
        [SerializeField] private Dropdown brdroplist_month;
        [SerializeField] private Dropdown brdroplist_day;

        [SerializeField] private GameObject birthError;

        public string brfirstname;

        public string brlastname;

        private string bryear;
        private string brmonth;
        private string brday;

        private static UIWithdrawName Inst;

        public static UIWithdrawName Shared()
        {
            return Inst;
        }

        public override void InitEvents()
        {
        }

        void Awake()
        {
            base.Awake();
            Inst = this;
        }

        public override void OnStart()
        {
            brbutton.Gray = true;
            
            inputField1.onEndEdit.AddListener(OnFirstName);
            inputField2.onEndEdit.AddListener(OnLastName);
            
            brdroplist_year.onValueChanged.AddListener(YZOnValueChangeBirthDate);
            brdroplist_month.onValueChanged.AddListener(YZOnValueChangeBirthDate);
            brdroplist_day.onValueChanged.AddListener(YZOnValueChangeBirthDate);
            
            brbtnback.onClick.AddListener(() => {
                Close();
            });

            brbtnwrongemail.onClick.AddListener(() => {
                Close();
                UserInterfaceSystem.That.ShowUI<UIWithdrawEmail>();
            });

            brbutton.onClick.AddListener(() => {
                if (OldThan18())
                {
                    UserInterfaceSystem.That.ShowUI<UIWithdrawAddress>();
                    birthError.SetActive(false);
                }
                else
                {
                    birthError.SetActive(true);
                }
            });
            
            brtxtemail.text = YZDataUtil.GetLocaling(YZConstUtil.YZWithdrawEmail);
            
            InitDropListBirthDate();
            
            birthError.SetActive(false);

            OnFirstName("");
        }
        
        private void InitDropListBirthDate()
        {
            // year
            List<string> ps = new List<string>();
            var thisYear = DateTime.Now.Year - 18;
            for (int i = 0; i < 83; ++i)
            {
                ps.Add(YZString.Concat(thisYear - i));
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
            // day
            ps.Clear();
            for (int i = 1; i <= 31; ++i)
            {
                ps.Add(i.ToString("D2"));
            }
            brdroplist_day.ClearOptions();
            brdroplist_day.AddOptions(ps);
        }
        
        
        public string GetBirthDate()
        {
            return YZString.Concat(brdroplist_year.captionText.text, "-", brdroplist_month.captionText.text, "-", brdroplist_day.captionText.text);
        }
        
        /// <summary>
        /// 生日日期下拉选择框
        /// </summary>
        /// <param name="int"></param>
        public void YZOnValueChangeBirthDate(int index)
        {
            var invalid = !CheckBirthDate();
            brbutton.Gray = (string.IsNullOrEmpty(brfirstname) || string.IsNullOrEmpty(brlastname) || invalid);
        }
        
        private void OnFirstName(string text)
        {
            brfirstname = text;
            brbutton.Gray = (string.IsNullOrEmpty(brfirstname) || string.IsNullOrEmpty(brlastname) || !CheckBirthDate());
        }

        private void OnLastName(string text)
        {
            brlastname = text;
            brbutton.Gray = (string.IsNullOrEmpty(brfirstname) || string.IsNullOrEmpty(brlastname) || !CheckBirthDate());
        }
        
        private bool CheckBirthDate()
        {
            var thisYear = DateTime.Now.Year;
            var thisMonth = DateTime.Now.Month;
            var thisDay = DateTime.Now.Day;
            var valid = true;
            var isthisMonth = false;
            //-- 1.先检测年份
            var intYear = brdroplist_year.captionText.text.ToInt();
            bool isthisYear;
            if (intYear == thisYear)
            {
                isthisYear = intYear == thisYear;
            }
            else
            {
                valid = true;
                return valid;
            }
            //-- 2.再检测月份
            if (valid)
            {
                var intMonth = brdroplist_month.captionText.text.ToInt();
                if (isthisYear)
                {
                    if (intMonth > thisMonth)
                    {
                        valid = false;
                    }
                    else if (intMonth == thisMonth)
                    {
                        isthisMonth = true;
                    }
                }
                //-- 2.1.再检测日
                var intDay = brdroplist_day.captionText.text.ToInt();
                if (isthisMonth)
                {
                    if (intDay > thisDay)
                        valid = false;
                }
            }
            return valid;
        }

        private bool OldThan18()
        {
            var intYear = brdroplist_year.captionText.text.ToInt();
            var intMonth = brdroplist_month.captionText.text.ToInt();
            var intDay = brdroplist_day.captionText.text.ToInt();
            if (intYear + 18 == DateTime.Now.Year)
            {
                if (intMonth > DateTime.Now.Month)
                {
                    return false;
                }
                else if (intDay > DateTime.Now.Day)
                {
                    return false;
                }
            }

            return true;
        }

        public override void InitVm()
        {
            
        }

        public override void InitBinds()
        {
            
        }
    }
}