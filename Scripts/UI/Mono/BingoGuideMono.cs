using Core.Extensions;
using Core.Extensions.UnityComponent;
using Core.Third.I18N;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Mono
{
    public class BingoGuideMono : MonoBehaviour
    {
        public MyButton PlayBtn;
        
        public Transform RollAgain;
        
        public Transform Tip1;

        public Transform Tip2;
        
        public void Init()
        {
            transform.SetActive(true);

            HideChildren();
        }

        public void FirstTip()
        {
            Tip1.SetActive(true);
        }

        /// <summary>
        /// 点击道具
        /// </summary>
        public void SecondTip()
        {
            Tip1.SetActive(false);
            Tip2.SetActive(true);
        }
        

        public void HideChildren()
        {
            var root = transform.Find("root");
            root.HideChildren();
        }
    }
}