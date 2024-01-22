using System.Collections.Generic;
using Core.Extensions.UnityComponent;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using UnityEngine;
using UnityEngine.UI;

namespace UI.UIWithDrawFlow
{
    public class UIWithdrawRecord : UIBase<UIWithdrawRecord>
    {
        [SerializeField] private GameObject brtxttitle;
        [SerializeField] private GameObject brtxtno;
        [SerializeField] private GameObject brobjnorecord;

        [SerializeField] private GameObject objList;

        [SerializeField] private Button brbtnback;
        //private YZListView brtable;

        private List<WithdrawHistoryData> YZDataSource = new List<WithdrawHistoryData>();

        private bool brloadcell = true;
        public override void InitEvents()
        {
        }

        public override void OnStart()
        {
            //YZDataSource.Clear();

            YZDataSource = GetArgsByIndex<List<WithdrawHistoryData>>(0);

            if (YZDataSource != null && YZDataSource.Count > 0)
            {
                var templateTrans = objList.transform.GetChild(0);
                for (int i = 0; i < YZDataSource.Count; ++i)
                {
                    var tNew = Instantiate(templateTrans.gameObject, Vector3.one, Quaternion.identity);
                    tNew.SetActive(true);
                    tNew.GetComponent<WithdrawHistoryMono>().SetContent(YZDataSource[i]);
                    tNew.transform.SetParent(objList.transform);
                    tNew.GetComponent<RectTransform>().localScale = Vector3.one;
                }
                
                brobjnorecord.SetActive(false);
            }
            else
            {
                brobjnorecord.SetActive(true);
            }
            
            
            brbtnback.onClick.AddListener(()=>
            {
                Close();
                UserInterfaceSystem.That.RemoveUIByName("UIWithdrawDetails");
                UserInterfaceSystem.That.RemoveUIByName("UIWithdrawAddress");
                UserInterfaceSystem.That.RemoveUIByName("UIWithdrawConfirm");
                UserInterfaceSystem.That.RemoveUIByName("UIWithdrawEmail");
                UserInterfaceSystem.That.RemoveUIByName("UIWithdrawName");
                UserInterfaceSystem.That.RemoveUIByName("UIWithdrawProcessingFee");
                UserInterfaceSystem.That.RemoveUIByName("UIWithdrawVerifyEmail");
                UserInterfaceSystem.That.RemoveUIByName("UIWithdrawVerifyFail");
                UserInterfaceSystem.That.RemoveUIByName("UIWithdrawVerifySuccess");
                
                // 重新打开，以便刷新数据
                UserInterfaceSystem.That.ShowUI<UIWithdrawDetails>();
            });
        }

        public override void InitVm()
        {
        }

        public override void InitBinds()
        {
        }
        
        // private void RequestData()
        // {
        //     YZServerRequest request = YZServerApiWithdraw.Shared.YZHyperHistory();
        //     request.AddYZSuccessHandler((str) => {
        //         YZHyperHistory data = YZGameUtil.JsonYZToObject<YZHyperHistory>(str);
        //         if (data.data != null)
        //         {
        //             YZDataSource = data.data;
        //         }
        //         RefreshUI();
        //     });
        //     request.AddYZFailureHandler((code, msg) => {
        //         RefreshUI();
        //     });
        // }
        //
        // private void RefreshUI()
        // {
        //     brloadcell = false;
        //     brtable.ReloadYZData(0);
        //     brobjnorecord.SetActive(YZDataSource == null || YZDataSource.Count <= 0);
        // }
    }
}