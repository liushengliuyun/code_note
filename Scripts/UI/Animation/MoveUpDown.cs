using System;
using BrunoMikoski.AnimationsSequencer;
using UnityEngine;
using UnityEngine.Serialization;

namespace UI.Animation
{
    public class MoveUpDown : MonoBehaviour
    {
        public float speed = 1.0f; // 移动速度
        public float distance = 1.0f; // 移动距离

        public float delay = 0f; // 
        // public float hold = 0f; // 停留时间

        private Vector3 startPos; // 起始位置

        //public AnimationSequence ControlSequence;

        private bool init;

        // private float currentHoldTime;
        private float currentDelay;

        private float speedTime;

        private void OnEnable()
        {
            // currentHoldTime = hold;
            currentDelay = delay;
            speedTime = 0;
        }

        private void InitStartPos()
        {
            if (init)
            {
                return;
            }

            init = true;
            startPos = transform.position;
        }

        private void Update()
        {
            // if (ControlSequence is { IsPlaying: true })
            // {
            //     return;
            // }

            if (currentDelay > 0)
            {
                currentDelay -= Time.deltaTime;
                return;
            }

            InitStartPos();
            // 计算移动距离和方向
            float moveDistance = -distance / 2 + Mathf.PingPong(distance / 2 + speedTime * speed, distance);
            speedTime += Time.deltaTime;
            // 停留一段时间  bug moveDistance 显然不会为0
            // if (moveDistance == 0)
            // {
            //     currentHoldTime -= Time.deltaTime;
            //     if (currentHoldTime <= 0)
            //     {
            //         currentHoldTime = hold;
            //     }
            //     return;
            // }

            Vector3 moveDirection = new Vector3(0, moveDistance, 0);

            // 更新物体位置
            transform.position = startPos + moveDirection;
        }

        private void OnDisable()
        {
            if (init)
            {
                transform.position = startPos;
                init = false;
            }
        }
    }
}