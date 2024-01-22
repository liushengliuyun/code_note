using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using MyBox;
using UI.Mono;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UI.Effect
{
    public class RollEffect : MonoBehaviour
    {
        [Range(360, 3600)] public float rotateX;

        [Range(360, 3600)] public float rotateY;

        [Range(360, 3600)] public float rotateZ;

        [Range(0.5f, 2)] public float time;

        private Vector3[] rotate = new[]
        {
            new Vector3(90, 180, 0),
            new Vector3(90, 90, 0),
            new Vector3(180, 0, 0),
            Vector3.zero,
            new Vector3(90, 270, 0),
            new Vector3(90, 0, 0),
        };

        private float min = 360;

        public Action OnRollEnd;

        TweenerCore<Quaternion, Vector3, QuaternionOptions> tween;
        private TweenerCore<Color, Color, ColorOptions> tween2;

        private float alphaChangeDuration = 0.8f;
        private float delayTime = 0.2f;

        public void SetRollTarget(int index)
        {
            var material = ResetAnimationState();

            tween = transform
                .DORotate(
                    new Vector3(Random.Range(min, rotateX), Random.Range(min, rotateY), Random.Range(min, rotateZ)),
                    time, RotateMode.FastBeyond360).SetEase(Ease.Linear).OnComplete(() =>
                {
                    transform.DORotate(rotate[index], 0.5f, RotateMode.Fast)
                        .SetEase(Ease.OutQuart)
                        .OnComplete(() => { RollEnd(material); });

                    // 使用DOVirtual.DelayedCall方法设置延迟回调
                    DOTween.To(() => 0, x => { OnRollEnd?.Invoke(); }, 0, delayTime);
                });
        }

        public void QuickRoll(int index)
        {
            var material = ResetAnimationState();

            tween = transform.DORotate(rotate[index], 0.5f, RotateMode.Fast).SetEase(Ease.OutQuint).OnComplete(() =>
            {
                RollEnd(material);
            });
        }

        private Material ResetAnimationState()
        {
            //先随机摇
            tween?.Kill();
            tween2?.Kill();

            transform.GetChild(0).SetActive(true);

            Material material = transform.GetChild(0).GetComponent<MeshRenderer>().material;
            material.color = material.color.WithAlphaSetTo(1);
            return material;
        }

        private void RollEnd(Material material)
        {
            // tween2 = material.DOColor(material.color.WithAlphaSetTo(0), alphaChangeDuration).SetEase(Ease.OutBack)
            //     .OnComplete(() => { transform.GetChild(0).SetActive(false); });
        }

        [InspectorButton("测试滚动")]
        private void TestRoll()
        {
            var index = Random.Range(0, 6);
            SetRollTarget(index);
        }
    }
}