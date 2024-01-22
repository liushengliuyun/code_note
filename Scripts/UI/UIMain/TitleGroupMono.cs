using System;
using Core.Extensions.UnityComponent;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class TitleGroupMono : MonoBehaviour
    {
        public MyText Title;

        [SerializeField] private Image up;
        
        [FormerlySerializedAs("arrow")] [SerializeField] private Image down;

        [SerializeField] public MyButton Btn;
        private bool isDown = true;

        public bool IsDown
        {
            set
            {
                isDown = value;
                down.SetActive(value);
                up.SetActive(!value);
                // down.transform.localScale = new Vector3(1, value ? 1 : -1, 1);
            }

            get => isDown;
        }

        // private void Start()
        // {
        //     Btn.SetClick(() => IsDown = !IsDown);
        // }
    }
}