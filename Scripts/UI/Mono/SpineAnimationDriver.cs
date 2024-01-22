using System;
using Core.Extensions;
using Spine.Unity;
using UnityEngine;
using UnityTimer;

namespace UI.Mono
{
    public class SpineAnimationDriver : MonoBehaviour
    {
        public float delay = 0.4f;
        
        public float sp = 0.6f;
        
        public float end = 2f;
        
        public String AnimationName;

        private Timer timer;
        
        private Timer timer2;
        private void OnEnable()
        {
            var spine = GetComponent<SkeletonGraphic>();
            if (spine != null && !AnimationName.IsNullOrEmpty())
            {
                var track = spine.AnimationState.SetAnimation(0, AnimationName, false);
                track.TimeScale = 0f;
                timer = this.AttachTimer(delay, () => track.TimeScale = sp);

                // timer2 = this.AttachTimer(end, () => track.TimeScale = 0f);
            }
        }

        private void OnDisable()
        {
            var spine = GetComponent<SkeletonGraphic>();
            timer?.Cancel();
            timer2?.Cancel();
            if (spine != null && !AnimationName.IsNullOrEmpty())
            {
                spine.AnimationState.ClearTrack(0);
            }
        }
    }
}