using System;
using UnityEngine;

public class YZBaseControl<T> : MonoBehaviour where T : MonoBehaviour
{
    public static object[] YZOrgs;

    public static Action<string> YZFunc;

    public static Action<string, bool> YZFuncP;

    public virtual void BTInit()
    {

    }

    public static T Init(GameObject obj, params object[] orgs)
    {
        YZOrgs = orgs;
        YZFunc = null;
        YZFuncP = null;
        T t = obj.AddComponent<T>();
        (t as YZBaseControl<T>).BTInit();
        return t;
    }

    public static T Init(GameObject obj, Action<string> func, params object[] orgs)
    {
        YZOrgs = orgs;
        YZFunc = func;
        YZFuncP = null;
        T t = obj.AddComponent<T>();
        (t as YZBaseControl<T>).BTInit();
        return t;
    }

    public static T Init(GameObject obj, Action<string, bool> func, params object[] orgs)
    {
        YZOrgs = orgs;
        YZFunc = null;
        YZFuncP = func;
        T t = obj.AddComponent<T>();
        (t as YZBaseControl<T>).BTInit();
        return t;
    }
}