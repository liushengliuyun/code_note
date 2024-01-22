using System;
using System.Collections.Generic;
using Castle.Core.Internal;
using Core.Extensions.UnityComponent;
using Core.Manager;
using Core.Models;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Third.I18N;
using DataAccess.Controller;
using DataAccess.Model;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Random = UnityEngine.Random;

namespace UI.Activity
{
    public class CardMono : MonoBehaviour
    {
        [SerializeField] private Text textDesc;
        [SerializeField] private Text textGain;
        [SerializeField] private Text textBonus;
        [SerializeField] private Text textBtn;
        [SerializeField] private MyButton button;
        [SerializeField] private MyButton bSideBtn;
        [SerializeField] private GameObject aSideObj;
        [SerializeField] private GameObject bSideObj;
        
        private int _discount;
        private int _index;
        private int _position;
        private int _id;
        
        private float _bonus;
        private float _amount;
        
        public void InitCard(bool isOpened, float realPrice, float cash, float bonus, int index, Action func, int id)
        {
            _discount = (int) Math.Ceiling((1.0f - (realPrice / (cash + bonus))) * 100.0f);
            _index = index;
            _bonus = bonus;
            _amount = realPrice;

            _id = id;

            if (isOpened)
            {
                bSideObj.SetActive(false);

                textDesc.text = "-" + _discount + "%";
                textGain.text = I18N.Get("key_money_code") + cash;
                textBonus.text = I18N.Get("key_money_code") + bonus;

                float originPrice = cash + bonus;
                textBtn.text = I18N.Get("key_money_code") + originPrice + "  " +
                               I18N.Get("key_money_code") + realPrice;
            }
            else
            {
                bSideObj.SetActive(true);
                bSideBtn.SetClick(() =>
                {
                    int remainCount = Root.Instance.Role.luckyCardInfo.lucky_card_choose_list.
                        FindAll(id => id == 0).Length - 1;
                    if (remainCount > 0 && Root.Instance.CanLuckyCardClick)
                    {
                        Root.Instance.CanLuckyCardClick = false;
                        FlipCard(func);
                    }
                    else if (remainCount <= 0)
                    {
                        UserInterfaceSystem.That.ShowUI<UITip>(I18N.Get("key_lucky_card_no_chance"));
                    }
                });
            }

            button.SetClick(Charge);
        }

        private void Charge()
        {
            Charge_configsItem chargeItemTest = new Charge_configsItem();
            chargeItemTest.id = _id;
            chargeItemTest.bonusValue = _bonus;
            chargeItemTest.amount = _amount.ToString();
            chargeItemTest.position = "LuckyCard";
            LocationManager.Shared.IsLocationValid(YZSafeType.Charge, null, -1, () =>
            {
                if (LocationManager.Shared.IsCountryCanCharge(chargeItemTest, ActivityType.LuckyCard))
                    UserInterfaceSystem.That.ShowUI<UIChargeCtrl>(chargeItemTest);
            });
        }

        private void FlipCard(Action func)
        {
            aSideObj.transform.SetLocalScale(new Vector3(0, 1, 1));
            
            var seq = DOTween.Sequence();
            seq.Append(bSideObj.transform.DOScaleX(0f, 0.3f));
            seq.Append(aSideObj.transform.DOScaleX(1f, 0.3f));
            seq.Play();
            
            GetPositionAndId();
            // 发给服务器
            MediatorRequest.Instance.SendChooseLuckyCard(_index + 1, _id);

            seq.onComplete = () =>
            {
                bSideObj.SetActive(false);

                //YZDataUtil.SetYZInt(YZConstUtil.YZLuckyCardOpenState + _index, 1);

                // 刷新UI
                func?.Invoke();
            };
        }

        private void GetPositionAndId()
        {
            var level = Root.Instance.Role.luckyCardInfo.lucky_card_level;
            int remainCount = Root.Instance.Role.luckyCardInfo.lucky_card_choose_list.
                FindAll(id => id == 0).Length - 1;
            if (level == 1)
            {
                if (remainCount == 3)
                {
                    _position = 1;
                }
                else if (remainCount == 2)
                {
                    _position = 2;
                }
                else if (remainCount == 1)
                {
                    _position = 3;
                }
                
                var configs = Root.Instance.LuckyCardConfigs[level];
                var card = configs.Find(match: cardConfig => Math.Abs(float.Parse(cardConfig.position) 
                                                                      - (float)_position) < 0.01f);
                _id = card.id;
            }
            else if (level == 2)
            {
                var choosed = Root.Instance.Role.luckyCardInfo.lucky_card_choose_list.FindAll(id => id != 0);
                var level2Cofigs = new List<LuckyCardConfig>();

                foreach (var ele in Root.Instance.LuckyCardConfigs[level])
                {
                    level2Cofigs.Add(ele);
                }

                // 配置表里剔除掉已经选的
                if (choosed != null && choosed.Length > 0)
                {
                    foreach (var ele in choosed)
                    {
                        var toRemove = level2Cofigs.Find(match: config => config.id == ele);
                        level2Cofigs.Remove(toRemove);
                    }
                }
                
                int rand = Random.Range(0, level2Cofigs.Count);
                _id = level2Cofigs[rand].id;
            }
        }

    }
}