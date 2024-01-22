using System;
using Core.Extensions;
using UnityEngine;

namespace UI.Effect
{
    public class MakeSelfGary : MonoBehaviour
    {
        private void Start()
        {
            transform.SetGray(true);
        }
    }
}