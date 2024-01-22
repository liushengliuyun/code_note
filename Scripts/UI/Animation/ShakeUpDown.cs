using System;
using UnityEngine;

namespace UI.Animation
{
    public class ShakeUpDown : MonoBehaviour
    {
        [Range(10, 100f)]
        public float speed = 20f; // 移动速度
        [Range(0, 100f)]
        public float distance = 10f; // 移动距离
        public float delay = 0f; // 停留时间

        private Vector3 startPos; // 起始位置

        private void OnDisable()
        {
            transform.localEulerAngles = startPos;
        }

        private void Awake()
        {
            startPos = transform.localEulerAngles;
        }

        private void Update()
        {
            // 计算移动距离和方向
            float moveDistance = Mathf.PingPong(Time.time * speed, distance);
            Vector3 moveDirection = new Vector3(0, 0, moveDistance - distance / 2);

            // 更新物体位置
            transform.localEulerAngles = startPos + moveDirection;

            // 停留一段时间
            if (moveDistance == 0)
            {
                delay -= Time.deltaTime;
                if (delay <= 0)
                {
                    delay = 1.0f;
                }
            }
        }
    }
}