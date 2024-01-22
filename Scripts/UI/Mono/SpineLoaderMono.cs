using System;
using System.Collections;
using System.Collections.Generic;
using Core.Extensions;
using DataAccess.Controller;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Serialization;
using YooAsset;

public class SpineLoaderMono : MonoBehaviour
{
    private GameObject spineObj;

    private AssetOperationHandle assetOperationHandle;

    private GameObject SpineObj => spineObj ??=
        MediatorBingo.Instance.LoadSpine(transform, out assetOperationHandle, Scale, position, faceLeft);

    public float Scale = 0.5f;

    public bool faceLeft = true;

    public Vector3 position;

    public string AnimationName = "animation";

    // Start is called before the first frame update
    void Start()
    {
        if (!AnimationName.IsNullOrEmpty())
        {
            SpineObj.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, AnimationName, true);
        }
    }

    public void PlayAnimation(string name)
    {
        SpineObj.GetComponent<SkeletonGraphic>().AnimationState.SetAnimation(0, name, true);
    }

    private void OnDestroy()
    {
        assetOperationHandle?.Release();

        assetOperationHandle = null;
    }
}