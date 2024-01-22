using System;
using Core.Services.UserInterfaceService.API.Facade;
using DataAccess.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;

namespace UI
{
    public class DuelEntry : MonoBehaviour
    {
        [SerializeField] private GameObject bubble;

        private const float POP_DELAY_TIME = 8.0f;
        private float _popDelayTimer;

        public void TryToShowDuelBubble()
        {
            // 进入主界面后一段时间后再弹
            if (_popDelayTimer < POP_DELAY_TIME)
                return;
            
            var topUI = UserInterfaceSystem.That.GetTopNormalUI();
            if (topUI == null)
            {
                return;
            }

            if (bubble.activeSelf)
                return;
            
            bool isTopUIMain = topUI.ClassType == typeof(UIMain);
            
            // 栈内是否有UI
            if (!UserInterfaceSystem.That.HaveUIInQueue() && isTopUIMain)
            {
                // 10分钟弹一次
                int time = YZDataUtil.GetYZInt(YZConstUtil.YZDuelShowTime, 0);
                if (TimeUtils.Instance.UtcTimeNow - time > 600)
                {
                    YZDataUtil.SetYZInt(YZConstUtil.YZDuelShowTime, TimeUtils.Instance.UtcTimeNow);
                }
                else
                {
                    return;
                }
                
                if (YZDataUtil.GetYZInt(YZConstUtil.YZDuelShowDay, 0) != TimeUtils.Instance.Today)
                {
                    // 今天第一次弹
                    YZDataUtil.SetYZInt(YZConstUtil.YZDuelShowDay, TimeUtils.Instance.Today);
                    YZDataUtil.SetYZInt(YZConstUtil.YZDuelShowCount, 1);

                    YZDebug.Log("TryToShowBubble  true");
                    
                    bubble.SetActive(true);
                }
                else
                {
                    // 不是第一次
                    int todayCount = YZDataUtil.GetYZInt(YZConstUtil.YZDuelShowCount, 0);
                    if (todayCount <= 3)
                    {
                        YZDataUtil.SetYZInt(YZConstUtil.YZDuelShowCount, todayCount + 1);
                        bubble.SetActive(true);
                        
                        YZDebug.Log("TryToShowBubble  true");
                    }
                }
            }
        }

        void Update()
        {
            _popDelayTimer += Time.deltaTime;
            
            if (Input.touchCount > 0)
            {
                //YZDebug.Log("触摸大于0");
                
                if (Input.GetTouch(0).phase == TouchPhase.Moved ||
                    Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    //YZDebug.Log("关闭邀请对战入口气泡");
                    
                    TinyTimer.StartTimer(() =>
                    {
                        if (bubble != null)
                            bubble.SetActive(false);
                    }, 0.15f);
                    
                }
            }
        }
    }
}