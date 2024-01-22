using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UI.Effect
{
    public class Outline8 : Shadow
    {
        protected List<UIVertex> list = new List<UIVertex>();

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!this.IsActive())
                return;

            list.Clear();
            vh.GetUIVertexStream(list);

            ModifyVertices(list);

            vh.Clear();
            vh.AddUIVertexTriangleStream(list);
        }

        private void ModifyVertices(List<UIVertex> verts)
        {
            if (!IsActive())
                return;

            int neededCapacity = verts.Count * 9;
            if (verts.Capacity < neededCapacity)
                verts.Capacity = neededCapacity;

            int original = verts.Count;
            int count = 0;
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (!(x == 0 && y == 0))
                    {
                        int next = count + original;
                        ApplyShadow(verts, effectColor, count, next, effectDistance.x * x, effectDistance.y * y);
                        count = next;
                    }
                }
            }
        }
    }
}