using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JerryMouse.UI
{
    public class UIEraseEffect : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public RawImage MaskTex;
        public int BrushSize = 50;
        public int TriggerEndPercent = 90;
        public Action EraserBeginEvent;
        public Action EraserEndEvent;
        
        private Texture2D _internalTex;
        private int _texWidth;
        private int _texHeight;
        private bool isEraseBegin;
        private bool isEraseEnd;
        private float totalPixels;
        private float _transparentPixels;
        private bool startDraw = false;
        private bool twoPoints = false;
        private Vector2 lastPos;//最后一个点
        private Vector2 penultPos;//倒数第二个点
        private float radius = 12f;
        private float distance = 1f;
        private double progress;
        
        void Awake()
        {
            Texture2D tex = (Texture2D)MaskTex.mainTexture;
            _internalTex = new Texture2D(tex.width, tex.height, TextureFormat.ARGB32, false);
            _internalTex.SetPixels(tex.GetPixels());
            _internalTex.Apply();
            MaskTex.texture = _internalTex;
            _texWidth = _internalTex.width;
            _texHeight = _internalTex.height;
            isEraseBegin = false;
            isEraseEnd = false;
            totalPixels = _internalTex.GetPixels().Length;
            _transparentPixels = 0;
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (isEraseEnd)
            {
                return;
            }
            startDraw = true;
            penultPos = eventData.position;
            ErasePoint(penultPos);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (isEraseEnd)
            {
                return;
            }
            if (twoPoints && Vector2.Distance(eventData.position, lastPos) > distance)
            {
                Vector2 pos = eventData.position;
                float dis = Vector2.Distance(lastPos, pos);
                ErasePoint(eventData.position);
                int segments = (int)(dis / radius);                                             
                segments = segments < 1 ? 1 : segments;
                if (segments >= 10) { segments = 10; }
                Vector2[] points = Beizier(penultPos, lastPos, pos, segments);
                for (int i = 0; i < points.Length; i++)
                {
                    ErasePoint(points[i]);
                }
                lastPos = pos;
                if (points.Length > 2)
                {
                    penultPos = points[points.Length - 2];
                }
            }
            else
            {
                twoPoints = true;
                lastPos = eventData.position;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (isEraseEnd)
            {
                return;
            }
            //CheckPoint(eventData.position);
            startDraw = false;
            twoPoints = false;
        }

        void ErasePoint(Vector3 pScreenPos)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(pScreenPos);
            Vector3 localPos = MaskTex.gameObject.transform.InverseTransformPoint(worldPos);

            RectTransform maskTexRectTransform = MaskTex.rectTransform;
            float scaleX = _texWidth / maskTexRectTransform.sizeDelta.x;
            float scaleY = _texHeight / maskTexRectTransform.sizeDelta.y;
            localPos.x *= scaleX;
            localPos.y *= scaleY;
            localPos.z = 0;

            if (localPos.x > -_texWidth / 2 && localPos.x < _texWidth / 2 && localPos.y > -_texHeight / 2 && localPos.y < _texHeight / 2)
            {
                for (int i = (int)localPos.x - BrushSize; i < (int)localPos.x + BrushSize; i++)
                {
                    for (int j = (int)localPos.y - BrushSize; j < (int)localPos.y + BrushSize; j++)
                    {
                        if (Mathf.Pow(i - localPos.x, 2) + Mathf.Pow(j - localPos.y, 2) > Mathf.Pow(BrushSize, 2))
                        {
                            continue;
                        }
                        
                        if (i < 0) { if (i < -_texWidth / 2) { continue; } }
                        if (i > 0) { if (i > _texWidth / 2) { continue; } }
                        if (j < 0) { if (j < -_texHeight / 2) { continue; } }
                        if (j > 0) { if (j > _texHeight / 2) { continue; } }

                        Color col = _internalTex.GetPixel(i + (int)_texWidth / 2, j + (int)_texHeight / 2);
                        if (col.a != 0f)
                        {
                            col.a = 0.0f;
                            _transparentPixels++;
                            _internalTex.SetPixel(i + (int)_texWidth / 2, j + (int)_texHeight / 2, col);
                        }
                    }
                }
                if (!isEraseBegin)
                {
                    isEraseBegin = true;
                    InvokeRepeating("CheckProgress", 0f, 0.2f);
                    EraserBeginEvent ?.Invoke();
                }
                _internalTex.Apply();
            }
        }
        
        /// <summary> 
        /// 检测当前刮刮卡进度
        /// </summary>
        /// <returns></returns>
        public void CheckProgress()
        {
            if (isEraseEnd)
            {
                return;
            }
            progress = _transparentPixels / totalPixels * 100;
            progress = (float)Math.Round(progress, 2);
            if (progress >= TriggerEndPercent)
            {
                isEraseEnd = true;
                CancelInvoke("CheckProgress");
                MaskTex.gameObject.SetActive(false);
                EraserEndEvent?.Invoke();
            }
        }
        
        
        
        
        
        /// <summary>
        /// 贝塞尔平滑
        /// </summary>
        /// <param name="start">起点</param>
        /// <param name="mid">中点</param>
        /// <param name="end">终点</param>
        /// <param name="segments">段数</param>
        /// <returns></returns>
        public Vector2[] Beizier(Vector2 start, Vector2 mid, Vector2 end, int segments)
        {
            float d = 1f / segments;
            Vector2[] points = new Vector2[segments - 1];
            for (int i = 0; i < points.Length; i++)
            {
                float t = d * (i + 1);
                points[i] = (1 - t) * (1 - t) * mid + 2 * t * (1 - t) * start + t * t * end;
            }
            List<Vector2> rps = new List<Vector2>();
            rps.Add(mid);
            rps.AddRange(points);
            rps.Add(end);
            return rps.ToArray();
        }
        
        
        
        
        
        
    }
}
