using System;
using System.Threading;
using AssetKits.ParticleImage;
using Carbon.Util;
using Coffee.UIEffects;
using Core.Extensions;
using Core.Services.ResourceService.API.Facade;
using Core.Services.ResourceService.Internal.UniPooling;
using Core.Services.UserInterfaceService.API.Facade;
using Core.Services.UserInterfaceService.Internal;
using Cysharp.Threading.Tasks;
using DataAccess.Controller;
using DataAccess.Model;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UI;
using UI.Effect;
using UI.Mono;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using Timer = UnityTimer.Timer;

namespace DataAccess.Utils
{
    public class UIEffectUtils : global::Utils.Runtime.Singleton<UIEffectUtils>
    {
        private MyPool pool = new MyPool();


        public GameObject GetCallBall()
        {
            return pool.Spawner.SpawnSync("uibingo/bingoball").GameObj;
        }

        public Sequence CreatMoveText(Transform from, Transform to, string content, Color color, string outColorCode,
            float duration = 0.5f,
            float scale = 1f,
            Action callback = null)
        {
            // YZLog.LogColor("CreatMoveText");
            var obj = pool.Spawner.SpawnSync("common/scoretext").GameObj;
     
            //from不能移动【移动会导致 obj跟着移动】, 所以设置到父节点去
            obj.transform.SetParent(from.parent);
            // 不需要更高的层级
            // obj.TryAddComponent<AddHeightCanvas>();
            obj.transform.localPosition = from.localPosition;
            var textCom = obj.GetComponent<Text>();
            textCom.text = content;
            
            var sequence = DOTween.Sequence();
            var rt = obj.transform.rectTransform();
            var originScale = Vector3.one * scale;
            rt.localScale = originScale;

            if (!outColorCode.IsNullOrEmpty())
            {
                ColorUtility.TryParseHtmlString(outColorCode, out var outcolor);
                obj.GetComponent<Outline8>().effectColor = outcolor;
            }
            
            if (to != null)
            {
                textCom.color = color;
                textCom.color = textCom.color.AlphaChange(0);
            }
            else
            {
                textCom.color = Color.white;
            }
      
            if (to != null)
            {
                var moveUp = rt.DOMoveY(rt.position.y + 1, 0.5f);
                var moveToEnd = rt.DOMove(to.position, duration).SetEase(Ease.InOutCirc);
                var size = rt.DOScale(originScale * 2, 0.5f);
                var size2 = rt.DOScale(originScale * 0.5f, duration);
                var alpha = textCom.DOColor(textCom.color.AlphaChange(1), 0.5f);
                return sequence.Join(moveUp).Join(size).Join(alpha).Append(moveToEnd).Join(size2)
                    .OnComplete(() =>
                    {
                        obj.Restore();
                        callback?.Invoke();
                    }).SetTarget(obj);
            }
            else
            {
                // var outline = textCom.GetComponent<Outline8>();
                // var shadow = textCom.GetComponent<Shadow>();
                // obj.SetActive(false);
                var moveUp = rt.DOMoveY(rt.position.y + 0.55f, 0.5f).SetEase(Ease.OutBack);
                var alpha = textCom.DOColor(color, 0.3f).SetDelay(0.2f);
                
                // var alphatoOne = textCom.DOColor(textCom.color.AlphaChange(1), 0.1f);
                var scaleToZero = rt.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
                return sequence.Append(moveUp)
                    // .Join(alphatoOne)
                    .Join(alpha)
                    .AppendInterval(Math.Max(duration - 1, 0.1f))
                    .Append(scaleToZero)
                    .OnComplete(() =>
                    {
                        // //避免残影
                        // textCom.text = "";
                        // outline.enabled = false;
                        // shadow.enabled = false;
                        obj.Restore();
                        callback?.Invoke();
                    })
                    .SetTarget(obj);
            }
        }

        public void CreatMoveImage(Transform from, string path, float duration = 0.5f, float scale = 0.4f, float x = 0,
            float y = 0.5f,
            Action callback = null)
        {
            var obj = pool.Spawner.SpawnSync("common/imagecontainer").GameObj;
            obj.transform.SetParent(from);
            obj.AddComponent<AddHeightCanvas>();
            obj.transform.localPosition = Vector3.zero;
            var imageCom = obj.GetComponent<Image>();
            imageCom.color = imageCom.color.AlphaChange(0);
            imageCom.sprite = MediatorBingo.Instance.GetSpriteByUrl(path);
            imageCom.SetNativeSize();

            var sequence = DOTween.Sequence();
            var rt = obj.transform.rectTransform();
            var originScale = Vector3.one * scale;
            rt.localScale = originScale;

            var moveUp = rt.DOMove(rt.position + new Vector3(x, y), duration).SetEase(Ease.OutBack);
            var size = rt.DOScale(originScale, duration);
            // var size2 = rt.DOScale(originScale * 0.5f, duration);
            var alpha = imageCom.DOColor(imageCom.color.AlphaChange(1), duration);
            var alphatozero = imageCom.DOColor(imageCom.color.AlphaChange(0), 1.5f);
            sequence.Join(moveUp).Join(size).Join(alpha).Append(alphatozero)
                .OnComplete(() =>
                {
                    obj.Restore();
                    callback?.Invoke();
                }).SetTarget(obj);
        }

        public void CreatResCollect(Vector3 worldPosition, Transform to, Texture texture, Action callback = null)
        {
            string devicesInfo = DeviceInfoUtils.Instance.GetDeviceInfo().ToLower();
            YZLog.LogColor("devicesInfo = " + devicesInfo);
            //one plus 测试
#if DEBUG
            if (devicesInfo.Contains("de2118") || devicesInfo.Contains("oneplus"))
#else
                 if (devicesInfo.Contains("de2118"))
#endif
            {
                CarbonLogger.LogError("机型 " + devicesInfo);
                for (int i = 0; i < 5; i++)
                {
                    var obj = pool.Spawner.SpawnSync("common/resicon").GameObj;
                    if (obj == null)
                    {
                        continue;
                    }

                    //为什么scale会变化吗
                    obj.transform.SetParent(UserInterfaceSystem.That.EffectPanel.transform);

                    obj.FindChild<RawImage>("rawImage").texture = texture;

                    var randomVector3 = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                    obj.transform.position = worldPosition;
                    obj.transform.localScale = Vector3.one * 0.6f;
                    var moveTween1 = obj.transform.DOMove(worldPosition + randomVector3, 0.2f).SetEase(Ease.OutBack);

                    var moveTween2 = obj.transform.DOMove(to.position, Random.Range(0, 0.8f)).OnComplete(() =>
                    {
                        obj.Restore();
                    }).SetEase(Ease.InOutCubic);

                    DOTween.Sequence().Append(moveTween1).Append(moveTween2);
                }

                DOTween.Sequence().SetDelay(1.01f).OnComplete(() => { callback?.Invoke(); });
            }
            else
            {
                var obj = pool.Spawner.SpawnSync("effect/collectres").GameObj;
                if (obj == null)
                {
                    return;
                }

                //setparent可以解决缩放问题
                obj.transform.SetParent(UserInterfaceSystem.That.EffectPanel.transform);
                var particleImage = obj.GetComponent<ParticleImage>();
                obj.transform.position = worldPosition;
                obj.transform.localScale = Vector3.one;
                particleImage.attractorTarget = to;
                particleImage.texture = texture;

                particleImage.Play();
                particleImage.onStop.RemoveAllListeners();
                particleImage.onStop.AddListener(() =>
                {
                    obj.Restore();
                    callback?.Invoke();
                });
            }
        }

        public void SingleTrail(Transform from, Transform to, Action callback = null)
        {
            // int random = Random.Range(0, 2);
            // if (random == 0)
            // {
            //     OldSingleTrail(from, to);
            //     return;
            // }
            var obj = pool.Spawner.SpawnSync("effect/eff_trail_01").GameObj;
            if (obj == null)
            {
                return;
            }

            obj.transform.parent = UserInterfaceSystem.That.EffectPanel.transform;
            obj.transform.position = from.position;
            obj.transform.localScale = Vector3.one;
            obj.transform.DOMove(to.position, 1f)
                .SetEase(Ease.InCubic)
                .OnComplete(() =>
                {
                    //回收后层级好像有问题
                    Timer.Register(0.5f, () => { obj.Discard(); });

                    callback?.Invoke();
                });
        }

        public void OldSingleTrail(Transform from, Transform to, Action callback = null)
        {
            var obj = pool.Spawner.SpawnSync("effect/singletrail").GameObj;
            if (obj == null)
            {
                return;
            }

            obj.transform.parent = UserInterfaceSystem.That.EffectPanel.transform;
            var particleImage = obj.GetComponent<ParticleImage>();
            obj.transform.position = from.position;
            obj.transform.localScale = Vector3.one;
            particleImage.attractorTarget = to;

            particleImage.Play();
            particleImage.onStop.RemoveAllListeners();
            particleImage.onStop.AddListener(() =>
            {
                obj.Restore();
                callback?.Invoke();
            });
        }

        private async UniTask WaitRenderTexture(Camera captureCamera, Transform hideCom, Transform target, bool isRight)
        {
            // await UniTask.WaitUntil(() => null != sampleCameraFilter.rt, cancellationToken: token);

            var uimain = UserInterfaceSystem.That.Get<UIMain>();

            var token = uimain.transform.GetCancellationTokenOnDestroy();

            //必须等待一帧 rendertexture才能被绘制【不等就是 灰色】
            await UniTask.NextFrame(token);

            var rawImage = UserInterfaceSystem.That.EffectPanel.FindChild<RawImage>("raw Image");

            rawImage.texture = captureCamera.targetTexture;

            rawImage.SetActive(true);

            // rawImage.GetComponent<Canvas>().enabled = true;

            rawImage.transform.TryGetComponent<CUIGraphic>(out var cuiGraphic);

            //重置
            cuiGraphic.FixTextToRectTrans();
            cuiGraphic.SyncRefCurvesControlRatioPints();

            // cuiGraphic.ShrinkToTarget(uimain.StoreToggle.transform, out var duration);

            var exEffect = ResourceSystem.That.InstantiateGameObjSync("common/my_fx_rotate_light06", target);

            exEffect.transform.localPosition = Vector3.one;

            var siblingIndex = target.parent.GetSiblingIndex();

            exEffect.transform.SetParent(uimain.transform);

            exEffect.transform.SetSiblingIndex(siblingIndex);

            cuiGraphic.ShrinkToTarget(target, isRight, out var duration);

            hideCom.SetActive(false);

            captureCamera.SetActive(false);

            await UniTask.Delay(TimeSpan.FromSeconds(duration), cancellationToken: token);

            rawImage.SetActive(false);

            captureCamera.gameObject.Destroy();

            var canvasGroup = exEffect.GetComponent<CanvasGroup>();

            DOTween.To(() => canvasGroup.alpha, value => canvasGroup.alpha = value, 0f, 1f).OnComplete(() =>
            {
                exEffect.Destroy();
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hideCom"></param>
        /// <param name="canvas"></param>
        /// <param name="target"></param>
        /// <param name="isRight">向右边吸入</param>
        public void CaptureAndShrink(Transform hideCom, Canvas canvas, Transform target, bool isRight = true)
        {
            var captureCamera = ResourceSystem.That.InstantiateGameObjSync("common/capturecamera",
                UserInterfaceSystem.That.EffectPanel.transform);
            //坐标异常
            // captureCamera.transform.localPosition = new Vector3(0, 0, -10);

            canvas.renderMode = RenderMode.WorldSpace;

            captureCamera.TryGetComponent<SampleCameraFilter>(out var sampleCameraFilter);

            if (sampleCameraFilter != null)
            {
                WaitRenderTexture(captureCamera.GetComponent<Camera>(), hideCom, target, isRight);
            }
        }

        public void CaptureAndDoSomething(Transform hideCom, Canvas canvas)
        {
            var captureCamera = ResourceSystem.That.InstantiateGameObjSync("common/capturecamera",
                UserInterfaceSystem.That.EffectPanel.transform);
            //坐标异常
            // captureCamera.transform.localPosition = new Vector3(0, 0, -10);

            canvas.renderMode = RenderMode.WorldSpace;

            captureCamera.TryGetComponent<SampleCameraFilter>(out var sampleCameraFilter);

            if (sampleCameraFilter != null)
            {
                WaitEffectRenderTexture(captureCamera.GetComponent<Camera>(), hideCom);
            }
        }

        private async UniTask WaitEffectRenderTexture(Camera captureCamera, Transform hideCom)
        {
            // await UniTask.WaitUntil(() => null != sampleCameraFilter.rt, cancellationToken: token);

            var uimain = UserInterfaceSystem.That.Get<UIMain>();

            var token = uimain.transform.GetCancellationTokenOnDestroy();

            //必须等待一帧 rendertexture才能被绘制【不等就是 灰色】
            await UniTask.NextFrame(token);

            var rawImage = UserInterfaceSystem.That.EffectPanel.FindChild<RawImage>("raw Image");

            rawImage.texture = captureCamera.targetTexture;

            rawImage.SetActive(true);

            // rawImage.GetComponent<Canvas>().enabled = true;

            rawImage.transform.TryGetComponent<CUIGraphic>(out var cuiGraphic);

            var uiTransition = rawImage.gameObject.TryAddComponent<UITransitionEffect>();

            uiTransition.enabled = true;

            uiTransition.effectMode = UITransitionEffect.EffectMode.Fade;

            uiTransition.effectFactor = 1f;

            DOTween.To(() => uiTransition.effectFactor, value => uiTransition.effectFactor = value, 0.1f, 0.5f)
                .SetEase(Ease.OutCirc).OnComplete(() =>
                {
                    rawImage.SetActive(false);

                    uiTransition.enabled = false;

                    captureCamera.gameObject.Destroy();
                });

            //重置
            cuiGraphic.FixTextToRectTrans();
            cuiGraphic.SyncRefCurvesControlRatioPints();

            // cuiGraphic.ShrinkToTarget(uimain.StoreToggle.transform, out var duration);

            hideCom.SetActive(false);

            captureCamera.SetActive(false);
        }
    }
}