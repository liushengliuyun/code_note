namespace UI.Effect
{
    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections.Generic;
    using System.Linq;

    [RequireComponent(typeof(Text))]
    public class Text2DOutline : BaseMeshEffect
    {
        private static string sOutlineShader = "TextEffect/Text2DOutline";
        private static string defalut = "UI/Default";


        [Header("===== 使用OutLine =====")] [SerializeField]
        private bool bUseOutLine = true;

        public bool BUseOutLine
        {
            get { return bUseOutLine; }
            set
            {
                bUseOutLine = value;
                if (base.graphic != null)
                    _Refresh();
            }
        }

        [Header("使用渐变")] [SerializeField] private bool bUseTextGradient = false;

        public bool BUseTextGradient
        {
            get { return bUseTextGradient; }
            set
            {
                bUseTextGradient = value;
                if (base.graphic != null)
                    _Refresh();
            }
        }

        [Header("文字颜色")] [SerializeField] private Gradient textGradient;

        public Gradient TextGradient
        {
            get { return textGradient; }
            set
            {
                textGradient = value;
                if (base.graphic != null)
                    _Refresh();
            }
        }

        [Header("描边颜色")] [SerializeField] private Color outlineColor = Color.white;

        public Color OutlineColor
        {
            get { return outlineColor; }
            set
            {
                outlineColor = value;
                if (base.graphic != null)
                    _Refresh();
            }
        }

        [Header("描边宽度"), Range(0, 8)] [SerializeField]
        private float outlineWidth = 0;

        public float OutlineWidth
        {
            get { return outlineWidth; }
            set
            {
                outlineWidth = value;
                if (base.graphic != null)
                    _Refresh();
            }
        }

        [Header("===== 阴影 =====")] [SerializeField]
        private bool bUseShadow = false;

        public bool BUseShadow
        {
            get { return bUseShadow; }
            set
            {
                bUseShadow = value;
                if (base.graphic != null)
                    _Refresh();
            }
        }

        [Header("使用独立透明度")] [SerializeField] private bool bShadowAlphaStand = false;

        public bool BShadowAlphaStand
        {
            get { return bShadowAlphaStand; }
            set
            {
                bShadowAlphaStand = value;
                if (base.graphic != null)
                    _Refresh();
            }
        }

        [Header("阴影文字过渡")] [SerializeField] private bool bUseShadowGradient = false;

        public bool BUseShadowGradient
        {
            get { return bUseShadowGradient; }
            set
            {
                bUseShadowGradient = value;
                if (base.graphic != null)
                    _Refresh();
            }
        }

        [Header("阴影文字颜色")] [SerializeField] private Gradient shadowGradient;

        public Gradient ShadowGradient
        {
            get { return shadowGradient; }
            set
            {
                shadowGradient = value;
                if (base.graphic != null)
                    _Refresh();
            }
        }

        [Header("阴影描边颜色")] [SerializeField] private Color shadowOutlineColor = Color.white;

        public Color ShadowOutlineColor
        {
            get { return shadowOutlineColor; }
            set
            {
                shadowOutlineColor = value;
                if (base.graphic != null)
                    _Refresh();
            }
        }

        [Header("阴影偏移")] [SerializeField] private Vector2 shadowOutlineOffset = new Vector2(0, 0);

        public Vector2 ShadowOutlineOffset
        {
            get { return shadowOutlineOffset; }
            set
            {
                shadowOutlineOffset = value;
                if (base.graphic != null)
                    _Refresh();
            }
        }

        [Header("阴影描边宽度"), Range(0, 8)] [SerializeField]
        private float shadowOutlineWidth = 0;

        public float ShadowOutlineWidth
        {
            get { return shadowOutlineWidth; }
            set
            {
                shadowOutlineWidth = value;
                if (base.graphic != null)
                    _Refresh();
            }
        }

        // #if UNITY_EDITOR || UNITY_STANDALONE_WIN
        int iMatHash = 0;

        bool bSetPreviewCanvas = false;
        // #endif

        protected override void OnEnable()
        {
            base.OnEnable();
            if (Application.isPlaying)
            {
                LoadShader();
            }

            if (CheckShader())
            {
                this.SetShaderChannels();
                this.SetShaderParams();
                this._Refresh();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            base.graphic.material = null;
        }


        bool CheckShader()
        {
            if (base.graphic == null)
            {
                Debug.LogError("No Graphic Component !");
                return false;
            }

            if (base.graphic.material == null)
            {
                Debug.LogError("No Material !");
                return false;
            }

            return true;
        }

        void SetShaderParams()
        {
            if (base.graphic.material != null)
            {
                base.graphic.material.SetColor("_OutlineColor", OutlineColor);
                base.graphic.material.SetFloat("_OutlineWidth", OutlineWidth);
                base.graphic.material.SetVector("_OutlineOffset", ShadowOutlineOffset);

                base.graphic.material.SetColor("_ShadowOutlineColor", ShadowOutlineColor);
                base.graphic.material.SetFloat("_ShadowOutlineWidth", ShadowOutlineWidth);
            }
        }

        void SetShaderChannels()
        {
            if (base.graphic.canvas)
            {
                var v1 = base.graphic.canvas.additionalShaderChannels;
                var v2 = AdditionalCanvasShaderChannels.TexCoord1;
                if ((v1 & v2) != v2)
                {
                    base.graphic.canvas.additionalShaderChannels |= v2;
                }

                v2 = AdditionalCanvasShaderChannels.TexCoord2;
                if ((v1 & v2) != v2)
                {
                    base.graphic.canvas.additionalShaderChannels |= v2;
                }

                v2 = AdditionalCanvasShaderChannels.TexCoord3;
                if ((v1 & v2) != v2)
                {
                    base.graphic.canvas.additionalShaderChannels |= v2;
                }
            }
        }
#pragma warning disable 0114
        // #if UNITY_EDITOR || UNITY_STANDALONE_WIN
        protected void OnValidate()
        {
            if (!bSetPreviewCanvas && Application.isEditor && gameObject.activeInHierarchy)
            {
                var can = GetComponentInParent<Canvas>();
                if (can != null)
                {
                    if ((can.additionalShaderChannels & AdditionalCanvasShaderChannels.TexCoord1) == 0 ||
                        (can.additionalShaderChannels & AdditionalCanvasShaderChannels.TexCoord2) == 0)
                    {
                        if (can.name.Contains("(Environment)"))
                        {
                            // 处于Prefab预览场景中(需要打开TexCoord1,2,3通道，否则Scene场景上会有显示问题(因为我们有用到uv1,uv2.uv3))
                            can.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1 |
                                                            AdditionalCanvasShaderChannels.TexCoord2 |
                                                            AdditionalCanvasShaderChannels.TexCoord3;
                        }
                        else
                        {
                            // 不是处于Prefab预览场景中，但父级Canvas的TexCoord1和TexCoord2通道没打开
                            UnityEngine.Debug.LogWarning(
                                "Text2DOutline may display incorrect if TexCoord1, TexCoord2 and TexCoord3 Channels are not open at Canvas where Text2DOutline object in.");
                        }
                    }

                    bSetPreviewCanvas = true;
                }
            }

            base.Invoke("OnValidate", 0);
            if (CheckShader())
            {
                this._Refresh();
            }

            LoadShader();
        }

        protected void Reset()
        {
            // base.Reset();
            base.Invoke("Reset", 0);
            LoadShader();
        }

        private void LoadShader()
        {
            if (base.graphic.material.shader.name != sOutlineShader)
            {
                base.graphic.material = new Material(Shader.Find(sOutlineShader));
                iMatHash = base.graphic.material.GetHashCode();
            }
        }
        // #endif
#pragma warning restore 0114

        public void Disable()
        {
            bUseOutLine = false;
            bUseShadow = false;
            _Refresh();

            base.graphic.material = new Material(Shader.Find(defalut));
            iMatHash = base.graphic.material.GetHashCode();
            enabled = false;
        }

        public void Enable()
        {
            bUseOutLine = true;
            bUseShadow = true;
            LoadShader();
            enabled = true;
            _Refresh();
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            var lVetexList = new List<UIVertex>();
            vh.GetUIVertexStream(lVetexList);
            if (bUseOutLine)
            {
                this._ProcessVertices(lVetexList, this.OutlineWidth);
            }


            List<UIVertex> lShadowVerts = new List<UIVertex>();
            if (bUseShadow)
            {
                vh.GetUIVertexStream(lShadowVerts);
                this._ProcessVertices(lShadowVerts, this.ShadowOutlineWidth);

                ApplayShadow(lShadowVerts, shadowGradient.colorKeys[0].color);
                if (bUseShadowGradient)
                    ApplyGradient(lShadowVerts, shadowGradient);
            }

            if (bUseTextGradient)
            {
                ApplyGradient(lVetexList, textGradient);
            }

            vh.Clear();
            // lVetexList = lShadowVerts.Concat(lVetexList).ToList();
            vh.AddUIVertexTriangleStream(lShadowVerts.Concat(lVetexList).ToList());
            lShadowVerts.Clear();
            lVetexList.Clear();
        }

        private void ApplayShadow(List<UIVertex> _lShadowVerts, Color32 _color)
        {
            for (var i = 0; i < _lShadowVerts.Count; i++)
            {
                UIVertex vt = _lShadowVerts[i];
                vt.position += new Vector3(ShadowOutlineOffset.x, ShadowOutlineOffset.y, 0);
                var newColor = _color;
                if (!bShadowAlphaStand)
                    newColor.a = (byte)((newColor.a * vt.color.a) / 255);
                vt.color = newColor;
                // uv3.x = -1用来传给Shader判断是文字还是阴影
                vt.uv3.x = -1;
                _lShadowVerts[i] = vt;
            }
        }

        private void ApplyGradient(List<UIVertex> _verts, Gradient _gradient)
        {
            if (_verts.Count == 0) return;
            float topY = _verts[0].position.y;
            float bottomY = _verts[0].position.y;
            for (var i = 0; i < _verts.Count; i++)
            {
                var y = _verts[i].position.y;
                if (y > topY)
                    topY = y;
                else if (y < bottomY)
                    bottomY = y;
            }

            float height = topY - bottomY;
            for (var i = 0; i < _verts.Count; i++)
            {
                var vt = _verts[i];
                Color32 color = _gradient.Evaluate((topY - vt.position.y) / height);
                // var color = Color32.Lerp (new Color(1, 1, 1, 0), new Color(1, 1, 1, 0), (vt.position.y - bottomY) / height);
                color.a = (byte)((color.a * vt.color.a) / 255);
                vt.color = color;
                _verts[i] = vt;
            }
        }

        // 添加描边后，为防止描边被网格边框裁切，需要将顶点外扩，同时保持UV不变
        private void _ProcessVertices(List<UIVertex> lVerts, float outlineWidth)
        {
            for (int i = 0, count = lVerts.Count - 3; i <= count; i += 3)
            {
                var v1 = lVerts[i];
                var v2 = lVerts[i + 1];
                var v3 = lVerts[i + 2];
                // 计算原顶点坐标中心点
                //
                var minX = _Min(v1.position.x, v2.position.x, v3.position.x);
                var minY = _Min(v1.position.y, v2.position.y, v3.position.y);
                var maxX = _Max(v1.position.x, v2.position.x, v3.position.x);
                var maxY = _Max(v1.position.y, v2.position.y, v3.position.y);
                var posCenter = new Vector2(minX + maxX, minY + maxY) * 0.5f;
                // 计算原始顶点坐标和UV的方向
                //
                Vector2 triX, triY, uvX, uvY;
                Vector2 pos1 = v1.position;
                Vector2 pos2 = v2.position;
                Vector2 pos3 = v3.position;
                if (Mathf.Abs(Vector2.Dot((pos2 - pos1).normalized, Vector2.right))
                    > Mathf.Abs(Vector2.Dot((pos3 - pos2).normalized, Vector2.right)))
                {
                    triX = pos2 - pos1;
                    triY = pos3 - pos2;
                    uvX = v2.uv0 - v1.uv0;
                    uvY = v3.uv0 - v2.uv0;
                }
                else
                {
                    triX = pos3 - pos2;
                    triY = pos2 - pos1;
                    uvX = v3.uv0 - v2.uv0;
                    uvY = v2.uv0 - v1.uv0;
                }

                // 计算原始UV框
                var uvMin = _Min(v1.uv0, v2.uv0, v3.uv0);
                var uvMax = _Max(v1.uv0, v2.uv0, v3.uv0);

                // 为每个顶点设置新的Position和UV，并传入原始UV框
                v1 = _SetNewPosAndUV(v1, outlineWidth, posCenter, triX, triY, uvX, uvY, uvMin, uvMax,
                    this.ShadowOutlineOffset);
                v2 = _SetNewPosAndUV(v2, outlineWidth, posCenter, triX, triY, uvX, uvY, uvMin, uvMax,
                    this.ShadowOutlineOffset);
                v3 = _SetNewPosAndUV(v3, outlineWidth, posCenter, triX, triY, uvX, uvY, uvMin, uvMax,
                    this.ShadowOutlineOffset);

                // 应用设置后的UIVertex
                //
                lVerts[i] = v1;
                lVerts[i + 1] = v2;
                lVerts[i + 2] = v3;
            }
        }


        private static UIVertex _SetNewPosAndUV(UIVertex pVertex, float pOutLineWidth,
            Vector2 pPosCenter,
            Vector2 pTriangleX, Vector2 pTriangleY,
            Vector2 pUVX, Vector2 pUVY,
            Vector2 pUVOriginMin, Vector2 pUVOriginMax, Vector2 offset)
        {
            // Position
            var pos = pVertex.position;
            var posXOffset = pos.x > pPosCenter.x ? pOutLineWidth : -pOutLineWidth;
            var posYOffset = pos.y > pPosCenter.y ? pOutLineWidth : -pOutLineWidth;
            pos.x += posXOffset;
            pos.y += posYOffset;
            pVertex.position = pos;
            // UV
            var uv = pVertex.uv0;
            var uvOffsetX = pUVX / pTriangleX.magnitude * posXOffset *
                            (Vector2.Dot(pTriangleX, Vector2.right) > 0 ? 1 : -1);
            var uvOffsetY = pUVY / pTriangleY.magnitude * posYOffset *
                            (Vector2.Dot(pTriangleY, Vector2.up) > 0 ? 1 : -1);
            uv.x += (uvOffsetX.x + uvOffsetY.x);
            uv.y += (uvOffsetX.y + uvOffsetY.y);
            pVertex.uv0 = uv;

            pVertex.uv1 = pUVOriginMin; //uv1 uv2 可用  tangent  normal 在缩放情况 会有问题
            pVertex.uv2 = pUVOriginMax;

            return pVertex;
        }

        private void _Refresh()
        {
            SetShaderParams();
            base.graphic.SetVerticesDirty();
        }

        private static float _Min(float pA, float pB, float pC)
        {
            return Mathf.Min(Mathf.Min(pA, pB), pC);
        }


        private static float _Max(float pA, float pB, float pC)
        {
            return Mathf.Max(Mathf.Max(pA, pB), pC);
        }


        private static Vector2 _Min(Vector2 pA, Vector2 pB, Vector2 pC)
        {
            return new Vector2(_Min(pA.x, pB.x, pC.x), _Min(pA.y, pB.y, pC.y));
        }


        private static Vector2 _Max(Vector2 pA, Vector2 pB, Vector2 pC)
        {
            return new Vector2(_Max(pA.x, pB.x, pC.x), _Max(pA.y, pB.y, pC.y));
        }
    }
}