using System.Collections.Generic;
using Core.Extensions;
using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.Internal;
using DataAccess.Controller;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.UIWithDrawFlow
{
    public class UIWithdrawAddress : UIBase<UIWithdrawAddress>
    {
        [SerializeField] private Button brbtnback;
        [SerializeField] private Button brbtnsubmit;
        [SerializeField] private Dropdown brdropdownstate;
        [SerializeField] private Text brtxtcountry;

        private GameObject brtxttitle;
        private GameObject brtxtselectcountry;
        private GameObject brtxtsubmit;

        [SerializeField] private MyButton brbutton;

        [SerializeField] private InputField inputfield1;
        [SerializeField] private InputField inputfield2;
        [SerializeField] private InputField inputfield3;
        [SerializeField] private InputField inputfield4;

        //[SerializeField] private Text zipCodeValid;
        
        private string brtowncity;
        private string braddressone;
        private string braddresstwo;
        private string brzipcode;
        public override void InitEvents()
        {
        }

        public override void OnStart()
        {
            inputfield1.onEndEdit.AddListener(OnAddressOne);
            inputfield2.onEndEdit.AddListener(OnAddressTwo);
            inputfield3.onEndEdit.AddListener(OnTownCity);
            inputfield4.onEndEdit.AddListener(OnZipCode);
            
            brbtnback.onClick.AddListener(() =>
            {
                Close();
            });


            string country = "US";
            OnInitDropDown();
            
            brbutton.SetClick(() =>
            {
                YZDataUtil.SetYZString(YZConstUtil.YZWithdrawFirstName, UIWithdrawName.Shared().brfirstname);
                YZDataUtil.SetYZString(YZConstUtil.YZWithdrawLastName, UIWithdrawName.Shared().brlastname);
                MediatorRequest.Instance.WithdrawSendInfo(country,
                    GetState(), this.brtowncity, this.braddressone,
                    this.braddresstwo,
                    this.brzipcode,
                    UIWithdrawName.Shared().brfirstname,
                    UIWithdrawName.Shared().brlastname,
                    UIWithdrawName.Shared().GetBirthDate());
            });
            
            OnChangeButtonStates();
        }
        
        private void OnAddressOne(string text)
        {
            braddressone = text;
            OnChangeButtonStates();
        }

        private void OnAddressTwo(string text)
        {
            braddresstwo = text;
            OnChangeButtonStates();
        }

        private void OnTownCity(string text)
        {
            brtowncity = text;
            OnChangeButtonStates();
        }

        private void OnZipCode(string text)
        {
            // bool isNumber = text.IsNumeric();
            // if (!isNumber)
                
            brzipcode = text;
            OnChangeButtonStates();
        }
        
        private void OnChangeButtonStates()
        {
            if (string.IsNullOrEmpty(braddressone) ||
                string.IsNullOrEmpty(braddresstwo) ||
                string.IsNullOrEmpty(brtowncity) ||
                string.IsNullOrEmpty(brzipcode) ||
                string.IsNullOrEmpty(GetState()))
            {
                brbutton.Gray = true;
            }
            else
            {
                brbutton.Gray = false;
            }
        }

        public override void InitVm()
        {
        }

        public override void InitBinds()
        {
        }
        
        private void OnInitDropDown()
        {
            // TODO 下拉选择州
            // if (PlayerManager.Shared.Config.HyperData == null || PlayerManager.Shared.Config.HyperData.hyper_province == null)
            // {
            //     return;
            // }
            // LitJson.JsonData province = PlayerManager.Shared.Config.HyperData.hyper_province;
            
            
            
            List<string> ps = new List<string>();
            // foreach (string key in province.Keys)
            // {
            //     ps.Add(key);
            // }
            
            ps.Add("Alabama");
            ps.Add("Alaska");
            ps.Add("Arizona");
            ps.Add("Arkansas");
            ps.Add("California");
            ps.Add("Colorado");
            ps.Add("Connecticut");
            ps.Add("Delaware");
            ps.Add("Florida");
            ps.Add("Georgia");
            
            ps.Add("Hawaii");
            ps.Add("Idaho");
            ps.Add("Illinois");
            ps.Add("Indiana");
            ps.Add("Iowa");
            ps.Add("Kansas");
            ps.Add("Kentucky");
            ps.Add("Lousiana");
            ps.Add("Maine");
            ps.Add("Maryland");
            
            ps.Add("Massachusetts");
            ps.Add("Michigan");
            ps.Add("Minnesota");
            ps.Add("Mississippi");
            ps.Add("Missouri");
            ps.Add("Montana");
            ps.Add("Nebraska");
            ps.Add("Nevada");
            ps.Add("New Hampshire");
            ps.Add("New Jersey");
            
            ps.Add("New Mexico");
            ps.Add("New York");
            ps.Add("North Carolina");
            ps.Add("North Dakota");
            ps.Add("Ohio");
            ps.Add("Oklahoma");
            ps.Add("Oregon");
            ps.Add("Pennsylvania");
            ps.Add("Rhode Island");
            ps.Add("South Carolina");
            
            ps.Add("South Dakota");
            ps.Add("Tennessee");
            ps.Add("Texas");
            ps.Add("Utah");
            ps.Add("Vermont");
            ps.Add("Virginia");
            ps.Add("Washington");
            ps.Add("West Virginia");
            ps.Add("Wisconsin");
            ps.Add("Wyoming");

            brdropdownstate.ClearOptions();
            brdropdownstate.AddOptions(ps);
        }

        private string GetState()
        {
            return brdropdownstate.captionText.text;
        }
    }
}