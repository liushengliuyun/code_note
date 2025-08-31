using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SlotX.Effect
{
    public class BatterOutline : Shadow
    {
        protected List<UIVertex> Vertices = new List<UIVertex>();

        private void UpdateVertices(List<UIVertex> uiVertices)
        {
            if (!IsActive())
                return;

            int neededCapacity = uiVertices.Count * 9;
            if (uiVertices.Capacity < neededCapacity)
                uiVertices.Capacity = neededCapacity;

            int original = uiVertices.Count;
            int count = 0;
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;
                    int next = count + original;
                    ApplyShadow(uiVertices, effectColor, count, next, effectDistance.x * x, effectDistance.y * y);
                    count = next;
                }
            }
        }
        
        public override void ModifyMesh(VertexHelper vertexHelper)
        {
            if (!IsActive())
                return;

            Vertices.Clear();
            vertexHelper.GetUIVertexStream(Vertices);

            UpdateVertices(Vertices);

            vertexHelper.Clear();
            vertexHelper.AddUIVertexTriangleStream(Vertices);
        }
    }
}