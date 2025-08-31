// using System;
// using JerryMouse.Extensions;
// using DG.Tweening;
// using JerryMouse.Controller;
// using JerryMouse.Model.JsonConfig;
// using SlotX;
// using TMPro;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace JerryMouse.Utils
// {
//     public class UIEffectUtils : Singleton.XSingleton<UIEffectUtils>
//     {
//         public void TextRollEffect(Text textCom, int from, int target, float duration = 0.5f, Action callBack = null)
//         {
//             if (from == target)
//             {
//                 textCom.text = MainGameUtils.ToCommaStyle(target);
//                 return;
//             }
//             
//             DOTween.To(() => from, x => from = x, target, duration).OnUpdate(() =>
//             {
//                 if (textCom)
//                 {
//                     textCom.text = MainGameUtils.ToCommaStyle(from);
//                 }
//              
//             }).SetEase(Ease.OutQuint).SetTarget(textCom).OnKill(() =>
//             {
//                 if (textCom)
//                 {
//                     textCom.text = MainGameUtils.ToCommaStyle(target);
//                     callBack?.Invoke();
//                 }
//             });
//         }
//         
//         public void TextRollEffect(Text textCom, float from, float target, string symbol = "",  float duration = 0.5f)
//         {
//             target = (float)Math.Round(target, 2);
//             if (Math.Abs(from - target) < ResConfig.Instance.MiniFloatGap)
//             {
//                 textCom.text = symbol + MainGameUtils.ToCommaStyle(target);
//                 return;
//             }
//
//             var dicimal = Math.Min(2, target.ToString().NumOfDecimal());
//             DOTween.To(() => from, x => from = x, target, duration).OnUpdate(() =>
//             {
//                 if (textCom)
//                 {
//                     textCom.text = symbol + MainGameUtils.ToCommaStyle(Math.Round(from, dicimal));
//                 }
//             }).SetEase(Ease.OutQuint).SetTarget(textCom).OnKill(() =>
//             {
//                 if (textCom)
//                 {
//                     textCom.text = symbol + MainGameUtils.ToCommaStyle(target);
//                 }
//             });
//         }
//         
//         private Sequence CreatMoveText(Transform from, Transform to, string content, Color color, 
//             float duration = 1f,
//             float scale = 1f,
//             Action callback = null,
//             string bgResName = ""
//             )
//         {
//             return CreatMoveText(from, from.localPosition, to, content, color, duration, scale, callback, bgResName);
//         }
//
//         public Sequence CreatMoveText(Transform parent, Vector3 fromLocal, Transform to, string content, Color color,
//             float duration = 1f,
//             float scale = 1f,
//             Action callback = null,
//             string bgResName = "")
//         {
//             // XLog.LogColor("CreatMoveText");
//             var obj = ResourceController.Instance.GetObjSync("CommonPrefab/ScoreText", cache: true);
//      
//             //from不能移动【移动会导致 obj跟着移动】, 所以设置到父节点去
//             if (parent == null)
//             {
//                 return null;
//             }
//             
//             obj.transform.SetParent(parent);
//             // 不需要更高的层级
//             // obj.TryAddComponent<AddHeightCanvas>();
//             obj.transform.localPosition = fromLocal;
//             obj.transform.localScale = Vector3.one;
//             var textGameObj = obj.FindChild("Text");
//             var rt = obj.transform.RectTransform();
//             var originScale = Vector3.one * scale;
//             //文字大小
//             textGameObj.transform.localScale = originScale;
//             var textCom = textGameObj.GetComponent<TextMeshProUGUI>();
//             
//             var sequence = DOTween.Sequence();
//             
//             if (!bgResName.IsNullOrEmpty())
//             {
//                 var sprite = MediatorGame.Instance.GetSpriteByUrl(bgResName);
//                 if (sprite != null)
//                 {
//                     var image = obj.FindChild<Image>("Icon");
//                     image.sprite = sprite;
//                     // image.SetNativeSize();
//                 }
//             }
//             
//             textCom.text = content;
//             if (to != null)
//             {
//                 textCom.color = color;
//                 textCom.color = textCom.color.AlphaChange(0);
//                 
//                 Vector3 lp = obj.transform.InverseTransformPoint(to.position) + fromLocal;
//                 var moveUp = rt.DOMoveY(rt.position.y + 1, 0.5f);
//                 var moveToEnd = rt.DOLocalMove(new Vector3(lp.x, lp.y, 0), duration).SetEase(Ease.InSine);
//                 var size_big = rt.DOScale(originScale * 2, 0.1f);
//                 var size_small = rt.DOScale(originScale * 0.7f, duration);
//                 var alpha = textCom.DOColor(textCom.color.AlphaChange(1), 0.5f);
//                 return sequence.Join(moveUp).Join(size_big).Join(alpha)
//                     .AppendInterval(0.5f)
//                     .Append(moveToEnd)
//                     .Join(size_small)
//                     .OnComplete(() =>
//                     {
//                         obj.Destroy();
//                         callback?.Invoke();
//                     }).SetTarget(obj);
//             }
//             else
//             {
//                 textCom.color = Color.white;
//                 
//                 var moveUp = rt.DOMoveY(rt.position.y + 0.55f, 0.5f).SetEase(Ease.OutBack);
//                 var alpha = textCom.DOColor(color, 0.3f).SetDelay(0.2f);
//                 
//                 // var alphatoOne = textCom.DOColor(textCom.color.AlphaChange(1), 0.1f);
//                 var scaleToZero = rt.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
//                 return sequence.Append(moveUp)
//                     // .Join(alphatoOne)
//                     .Join(alpha)
//                     .AppendInterval(Math.Max(duration - 1, 0.1f))
//                     .Append(scaleToZero)
//                     .OnComplete(() =>
//                     {
//                         textCom.color = Color.white;
//                         obj.Release();
//                         callback?.Invoke();
//                     })
//                     .SetTarget(obj);
//             }
//         }
//
//         public bool ResCollecting { get; set; }
//         public void CreatResCollect(Vector3 worldPosition, Transform to, Texture texture, bool isInUISlotMachine ,  int count = 10, Action callback = null, float scale = 0.4f)
//         {
//             ResCollecting = true;
//
//             var single = count == 1;
//
//             for (int i = 0; i < count; i++)
//             {
//                 var obj = ResourceController.Instance.GetObjSync("CommonPrefab/ResIcon", cache: true);
//                 if (obj == null)
//                 {
//                     continue;
//                 }
//                 
//                 if (isInUISlotMachine)
//                 {
//                     obj.transform.SetParent(FatherUISlotMachine.Instance.transform);
//                 }
//                 else
//                 {
//                     obj.transform.SetParent(UIController.Instance.EffectPanel.transform);
//                 }
//
//                 var rawImage = obj.FindChild<RawImage>("rawImage");
//                 
//                 rawImage.texture = texture;
//
//                 float aspectRatio = (float)texture.width / texture.height;
//
//                 var rectTransform = rawImage.transform.RectTransform();
//                 rectTransform.SetHeight(rectTransform.rect.width / aspectRatio);
//                 // LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
//                 obj.transform.position = worldPosition;
//                 var localPosition = obj.transform.localPosition;
//                 obj.transform.localPosition = new Vector3(localPosition.x, localPosition.y, 0);
//                 obj.transform.localScale = Vector3.one * scale;
//
//                 if (single)
//                 {
//                     obj.transform.DOMove(to.position, 1f)
//                         .SetEase(Ease.InOutCubic)
//                         .OnComplete(() =>
//                         {
//                             ResCollecting = false;
//                             callback?.Invoke();
//                             obj.Release();
//                         });
//                 }
//                 else
//                 {
//                     var randomVector3 = new Vector3(MainGameUtils.GetRandomValue(-3f, 3f),
//                         MainGameUtils.GetRandomValue(-3f, 3f));
//                     var randomPosition = worldPosition + randomVector3;
//                     var moveTween1 = obj.transform
//                         .DOMove(randomPosition, MainGameUtils.GetRandomValue(0.2f, 0.4f))
//                         .SetEase(Ease.OutBack);
//                     var randomStayDelay = MainGameUtils.GetRandomValue(0.1f, 0.3f);
//                     var moveTween2 = obj.transform
//                         .DOMoveY(randomPosition.y + randomStayDelay, randomStayDelay)
//                         .SetEase(Ease.OutQuint);
//                 
//                     var moveTween3 = obj.transform.DOMove(to.position, MainGameUtils.GetRandomValue(0.3f, 0.6f))
//                         .OnComplete(() => { obj.Release(); }).SetEase(Ease.InOutCubic);
//
//                     var randomStartDelay = MainGameUtils.GetRandomValue(0, 1);
//                     
//                     DOTween.Sequence().AppendInterval(randomStartDelay).Append(moveTween1).Append(moveTween2)
//                         .Append(moveTween3);
//                 }
//             }
//
//             if (!single)
//             {
//                 DOTween.Sequence().AppendInterval(1.5f).OnComplete(() =>
//                 {
//                     ResCollecting = false;
//                     callback?.Invoke();
//                 });
//             }
//         }
//
//         public void SingleTrail(Transform from, Transform to, Action callback = null)
//         {
//             var obj = ResourceController.Instance.GetObjSync("Anima/eff_Energy_Trail", cache: true);
//             if (obj == null)
//             {
//                 return;
//             }
//
//             obj.transform.SetParent(UIController.Instance.EffectPanel.transform);
//             obj.transform.localPosition = Vector3.zero;
//             var fromLp = obj.transform.InverseTransformPoint(from.position);
//             obj.transform.localPosition = new Vector3(fromLp.x, fromLp.y, 0);
//             obj.transform.localScale = Vector3.one;
//
//             Vector3 lp = obj.transform.InverseTransformPoint(to.position);
//             obj.transform.DOLocalMove(new Vector3(lp.x, lp.y, 0), 1f)
//                 .SetEase(Ease.InCubic)
//                 .OnComplete(() =>
//                 {
//                     //回收后层级好像有问题
//                     DOTween.Sequence().AppendInterval(0.5f).AppendCallback(() => { obj.Release(); });
//
//                     callback?.Invoke();
//                 });
//         }
//
//         public void PlayFullScreenFire(Action callback = null)
//         {
//             var obj = ResourceController.Instance.GetObjSync("Anima/eff_Confetti_vfx", cache: true);
//             if (obj == null)
//             {
//                 return;
//             }
//             
//             obj.transform.SetParent(UIController.Instance.EffectPanel.transform);
//             obj.transform.localScale = Vector3.one;
//             obj.transform.localPosition = Vector3.one;
//             var rectTransform = obj.transform.RectTransform();
//             rectTransform.anchorMin = new Vector2(1, 0);
//             rectTransform.anchorMax = new Vector2(1, 0);
//             rectTransform.anchoredPosition = Vector2.zero;
//             DOTween.Sequence().AppendInterval(3f).AppendCallback(() =>
//             {
//                 obj.Release();
//                 callback?.Invoke();
//             });
//         }
//     }
// }