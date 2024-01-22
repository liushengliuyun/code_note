using Core.Extensions;
using Core.Services.UserInterfaceService.Internal;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils.Static;
using HT.InfiniteList;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UICashFlow : UIBase<UICashFlow>
    {
        [SerializeField] private GameObject brtxttitle;
        [SerializeField] private GameObject brtxtno;
        [SerializeField] private GameObject brobjnorecord;

        [SerializeField] private Button brbtnback;

        [SerializeField] private InfiniteListScrollRect infiniteList;

        public override void InitEvents()
        {
            AddEventListener(GlobalEvent.Sync_User_Cash_Flow, (sender, args) => { RefreshCashFlows(); });
        }

        public override void OnStart()
        {
            infiniteList.ClearData();
            
            MediatorRequest.Instance.GetUserCashFlow(true);
            
            brbtnback.onClick.AddListener(()=>
            {
                Close();
            });

            RefreshCashFlows();
            
            infiniteList.onValueChanged.AddListener((offset) =>
            {
                //Debug.Log("scroll value = " + offset.y);

                MediatorRequest.Instance.GetUserCashFlow();
                
            });
        }

        public override void InitVm()
        {
        }

        public override void InitBinds()
        {
        }

        private void RefreshCashFlows()
        {
            if (Root.Instance.CashFlow == null || Root.Instance.CashFlow.Count == 0)
            {
                return;
            }
                
            brobjnorecord.SetActive(Root.Instance.CashFlow == null || Root.Instance.CashFlow.Count <= 0);
            
            infiniteList.ClearData();
            // 添加数据到尾部（初始化、 Sync_User_Cash_Flow 都会触发）
            infiniteList.AddDatas(Root.Instance.CashFlow);

            // 一页的内容都不够的时候，继续请求
            if (!Root.Instance.LastCashFlowCursor.IsNullOrEmpty() && Root.Instance.CashFlow.Count < 10
                && infiniteList.DataCount < 200)
            {
                MediatorRequest.Instance.GetUserCashFlow();
            }
        }
        
    }
}