﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 组件自动绑定工具
/// </summary>
public class ComponentAutoBindTool : MonoBehaviour
{
#if UNITY_EDITOR
    [Serializable]
    public class BindData
    {
        public BindData()
        {
        }

        public BindData(string name, Component bindCom)
        {
            Name = name;
            BindCom = bindCom;
        }

        public string Name;
        public Component BindCom;
    }

    public List<BindData> BindDatas = new List<BindData>();

    [SerializeField]
    private string m_ClassName;

    [SerializeField]
    private string m_Namespace;

    [SerializeField]
    private string m_CodePath;

    public string ClassName
    {
        get
        {
            return m_ClassName;
        }
    }

    public string Namespace
    {
        get
        {
            return m_Namespace;
        }
    }

    public string CodePath
    {
        get
        {
            return m_CodePath;
        }
    }

    public IAutoBindRuleHelper RuleHelper
    {
        get;
        set;
    }
#endif

    [SerializeField]
    public List<Component> bindComs = new List<Component>();


    public T FindComponent<T>(int index) where T : Component
    {
        if (index >= bindComs.Count)
        {
            Debug.LogError("索引无效");
            return null;
        }

        T bindCom = bindComs[index] as T;

        if (bindCom == null)
        {
            Debug.LogError("类型无效");
            return null;
        }

        return bindCom;
    }
}
