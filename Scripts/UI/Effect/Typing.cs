using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Typing : MonoBehaviour
{
    public Text contentText;

    private string content; // 需要显示的文字

    public float TypingTime = 0.1f; // 延迟时间

    private int nowLength = 0; // 当前打印的字数

    public Action EndCallBack;

    public bool Loop;

    public void EndNow()
    {
    }

    void Start()
    {
        contentText ??= GetComponent<Text>();
        content = contentText.text; // 记录
        contentText.text = ""; // 置空
        // InvokeRepeating(methodName, time, repeatRate)
        // 程序开始 time 秒后，每经过 repeatRate 秒就自动调用 methodName 函数
        InvokeRepeating("DelayTyping", 0.0f, TypingTime); // 每经过 repeatTime 就自动打印一个字
    }

    void DelayTyping()
    {
        ++nowLength;
        // Substring(startIndex, length)
        // 从startIndex开始，截取 length 个字符
        contentText.text = content.Substring(0, nowLength);
        if (nowLength >= content.Length) // 打印完毕
        {
            if (Loop)
            {
                contentText.text = "";
                nowLength = 0;
            }
            else
            {
                CancelInvoke(); // 取消自动调用
                StartCoroutine(CallBack());
            }
        }
    }

    IEnumerator CallBack()
    {
        yield return new WaitForSeconds(2);
        EndCallBack?.Invoke();
    }
}