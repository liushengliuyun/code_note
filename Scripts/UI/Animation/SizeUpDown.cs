using System;
using BrunoMikoski.AnimationsSequencer;
using UnityEngine;

namespace UI.Animation
{
    public class SizeUpDown : MonoBehaviour
    {
        [Range(0, 2f)] public float speed = 0.5f; // 移动速度
        [Range(0, 0.5f)] public float distance = 0.2f; // 移动距离
        public float delay = 0f; // 停留时间

        private Vector3 startScale = Vector3.one; // 起始位置

        public AnimationSequence ControlSequence;
        
        private void OnDisable()
        {
            transform.localScale = startScale;
        }

        private void Awake()
        {
            startScale = transform.localScale;
        }

        private void Update()
        {
            if (ControlSequence is {IsPlaying: true})
            {
                return;
            }
            
            float moveDistance = Mathf.PingPong(Time.time * speed, distance);

            // 更新物体位置
            transform.localScale = (moveDistance + 1) * startScale;

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