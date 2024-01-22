using System.Collections.Generic;
using CatLib.EventDispatcher;
using Core.Extensions.UnityComponent;
using Core.Manager;
using Core.Models;
using Core.Services.UserInterfaceService.API.Facade;
using DataAccess.Controller;
using DataAccess.Model;
using DataAccess.Utils.Static;
using DG.Tweening;
using Shapes2D;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI.Activity
{
    public class DragonItemMono : MonoBehaviour
    {
        [SerializeField] private GameObject yellowBg;
        [SerializeField] private GameObject purpleBg;
        [SerializeField] private GameObject lockObj;

        [SerializeField] private GameObject rewardItemTemplate;

        [SerializeField] private MyButton freeBtn;

        [SerializeField] private MyButton purchaseBtn;

        private bool _isUnlocked = false;
        private int _index;

        public int CurrentBuyId = 0;

        public void InitDragon(List<DragonAwardItem> items, bool isFree, string price, DragonPurchaseItem buyItem,
            int index)
        {
            _index = index;
            
            freeBtn.SetActive(isFree);
            purchaseBtn.SetActive(!isFree);

            // 显示奖励道具
            for (int i = 0; i < items.Count; ++i)
            {
                var itemNew = Instantiate(rewardItemTemplate, Vector3.zero, Quaternion.identity, 
                    rewardItemTemplate.transform.parent);

                SetItem(itemNew, items[i]);
            }
            
            rewardItemTemplate.SetActive(false);

            if (!isFree)
            {
                CurrentBuyId = buyItem.id;
                
                // set price
                purchaseBtn.gameObject.transform.Find("Text").GetComponent<MyText>().text = "$" + 
                    YZNumberUtil.FormatYZMoney(price);
                purchaseBtn.SetClick(() =>
                {
                    if (_isUnlocked)
                    {
                        // 充值流程
                        Charge_configsItem chargeItemTest = new Charge_configsItem();
                        chargeItemTest.id = buyItem.id;
                        chargeItemTest.bonusValue = buyItem.show_bonus.ToFloat();
                        chargeItemTest.amount = buyItem.amount;
                        chargeItemTest.position = "Dragon";
                        LocationManager.Shared.IsLocationValid(YZSafeType.Charge, null, -1, () =>
                        {
                            if (LocationManager.Shared.IsCountryCanCharge(chargeItemTest, ActivityType.Dragon))
                                UserInterfaceSystem.That.ShowUI<UIChargeCtrl>(chargeItemTest);
                        });
                    }
                    else
                    {
                        UserInterfaceSystem.That.ShowUI<UITip>("Not yet unlocked");
                    }
                });
            }
            else
            {
                freeBtn.SetClick(() =>
                {
                    if (_isUnlocked)
                    {

                        GetRewards();

                    }
                    else
                    {
                        UserInterfaceSystem.That.ShowUI<UITip>("Not yet unlocked");
                    }
                });
            }
        }

        public void GetRewards()
        {
            MediatorRequest.Instance.ClaimDragon(_index);
        }

        private void SetItem(GameObject itemObj, DragonAwardItem awardItem)
        {
            itemObj.transform.Find("Icon").Find("1").SetActive(false);
            itemObj.transform.Find("Icon").Find("2").SetActive(false);
            itemObj.transform.Find("Icon").Find("3").SetActive(false);
            itemObj.transform.Find("Icon").Find("4").SetActive(false);
            
            itemObj.transform.Find("CountText").Find("1").SetActive(false);
            itemObj.transform.Find("CountText").Find("2").SetActive(false);
            itemObj.transform.Find("CountText").Find("3").SetActive(false);
            itemObj.transform.Find("CountText").Find("4").SetActive(false);
            
            switch (int.Parse(awardItem.type))
            {
                case 1:
                    itemObj.transform.Find("Icon").Find("1").SetActive(true);
                    itemObj.transform.Find("CountText").Find("1").SetActive(true);
                    itemObj.transform.Find("CountText").Find("1").GetComponent<MyText>().text = "$" + 
                        YZNumberUtil.FormatYZMoney(awardItem.amount);
                    
                    itemObj.transform.Find("TextTypeYellow").GetComponent<MyText>().text = "CASH";
                    itemObj.transform.Find("TextTypePurple").GetComponent<MyText>().text = "CASH";
                    break;
                
                case 2:
                    itemObj.transform.Find("Icon").Find("2").SetActive(true);
                    itemObj.transform.Find("CountText").Find("2").SetActive(true);
                    
                    itemObj.transform.Find("CountText").Find("2").GetComponent<MyText>().text = 
                        YZNumberUtil.FormatYZMoney(awardItem.amount);
                    
                    itemObj.transform.Find("TextTypeYellow").GetComponent<MyText>().text = "GEMS";
                    itemObj.transform.Find("TextTypePurple").GetComponent<MyText>().text = "GEMS";
                    break;
                
                case 3:
                    itemObj.transform.Find("Icon").Find("3").SetActive(true);
                    itemObj.transform.Find("CountText").Find("3").SetActive(true);
                    itemObj.transform.Find("CountText").Find("3").GetComponent<MyText>().text = "$" + 
                        YZNumberUtil.FormatYZMoney(awardItem.amount);
                    
                    itemObj.transform.Find("TextTypeYellow").GetComponent<MyText>().text = "BONUS";
                    itemObj.transform.Find("TextTypePurple").GetComponent<MyText>().text = "BONUS";
                    
                    break;
                
                case 4:
                    itemObj.transform.Find("Icon").Find("4").SetActive(true);
                    itemObj.transform.Find("CountText").Find("4").SetActive(true);
                    itemObj.transform.Find("CountText").Find("4").GetComponent<MyText>().text = 
                        YZNumberUtil.FormatYZMoney(awardItem.amount);
                    
                    itemObj.transform.Find("TextTypeYellow").GetComponent<MyText>().text = "COIN";
                    itemObj.transform.Find("TextTypePurple").GetComponent<MyText>().text = "COIN";
                    break;
            }
        }
        
        public void Unlock(bool isShowAnim)
        {
            if (isShowAnim)
            {
                Invoke( nameof(UnlockExe), 0.8f);
                lockObj.GetComponent<SkeletonGraphic>().AnimationState.
                    SetAnimation(0, "animation", false);
            }
            else
            {
                // 打开界面的时候
                UnlockExe();
                lockObj.SetActive(false);
            }

            _isUnlocked = true;
        }

        private void UnlockExe()
        {
            
            purpleBg.SetActive(false);
            yellowBg.SetActive(true);
        }

        public void Fade()
        {
            var images = transform.GetComponentsInChildren<Image>();
            var myTexts = transform.GetComponentsInChildren<MyText>();
            var texts= transform.GetComponentsInChildren<Text>();

            foreach (var img in images)
            {
                img.DOFade(0, 0.5f);
            }
            
            foreach (var txt in myTexts)
            {
                txt.DOFade(0, 0.5f).Complete();
            }
            
            foreach (var txt in texts)
            {
                txt.DOFade(0, 0.5f).Complete();
            }
            
            Invoke( nameof(SendToUIDragon), 0.5f);
        }

        private void SendToUIDragon()
        {
            EventDispatcher.Root.Raise(GlobalEvent.Dragon_Fade_Completed);
        }
    }
}