using Spine.Unity;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SlotX.Effect
{
    public class SpineBreath : MonoBehaviour
    {
        [SerializeField] private float minInterval;
        [SerializeField] private float maxInterval;
        [SerializeField] private string animName;
        
        private SkeletonGraphic spineAnim;
        private float _currentInterval = 0.0f;
        private float _timer = 0.0f;
        private bool _init;
        
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
                var track = spineAnim.AnimationState.GetCurrent(0);
                if (_init == false)
                {
                    _init = true;
                    if (track == null || track.Animation.Name != animName)
                    {
                        spineAnim.AnimationState.SetAnimation(0, animName, false);
                    }
                    else
                    {
                        track.TrackTime = 0;
                    }
                }
                else
                {
                    track.TrackTime = 0;
                }
            }
            else
            {
                _timer += Time.deltaTime;
            }
        }
    }
}