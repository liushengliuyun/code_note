using System;
using Spine.Unity;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UI.Effect
{
    public class SpineBreath : MonoBehaviour
    {
        [SerializeField] private float minInterval;
        [SerializeField] private float maxInterval;
        [SerializeField] private string animName;
        
        private SkeletonGraphic spineAnim;
        private float _currentInterval = 0.0f;
        private float _timer = 0.0f;

        private void Start()
        {
            spineAnim = GetComponent<SkeletonGraphic>();
            _currentInterval = Random.Range(minInterval, maxInterval);
        }

        private void Update()
        {
            if (_timer > _currentInterval)
            {
                // 重新计算 interval
                _currentInterval = Random.Range(minInterval, maxInterval);
                _timer = 0;

                spineAnim.AnimationState.SetAnimation(0, animName, false);
            }
            else
            {
                _timer += Time.deltaTime;
            }
        }
    }
}