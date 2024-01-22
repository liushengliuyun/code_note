using System;
using Core.Extensions.UnityComponent;
using Core.Third.I18N;
using UnityEngine;

namespace UnityEditor.UI
{
    /// <summary>
    /// Author：qingqing.zhao (569032731@qq.com)
    /// Date：2021/5/27 15:02
    /// Desc：扩展Unity的Text Inspector
    /// </summary>
    [CustomEditor(typeof(MyButton), true)]
    [CanEditMultipleObjects]
    public class MyButtonEditor : UnityEditor.UI.ButtonEditor
    {
        private string[] args;
        private SerializedProperty clickSound;
        private SerializedProperty AudioClip;
        private SerializedProperty soundPack;
        private MyButton button;

        protected override void OnEnable()
        {
            base.OnEnable();
            button = target as MyButton;
            clickSound = serializedObject.FindProperty("clickSound");
            AudioClip = serializedObject.FindProperty("audioClip");
            soundPack = serializedObject.FindProperty("soundPack");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            clickSound.boolValue = EditorGUILayout.Toggle("是否有点击音效:", clickSound.boolValue);
            if (clickSound.boolValue)
            {
                AudioClip.objectReferenceValue =
                    EditorGUILayout.ObjectField("音效资源", AudioClip.objectReferenceValue, typeof(AudioClip), true);

                soundPack.enumValueIndex =
                    (int)(SoundPack)EditorGUILayout.EnumPopup("已配置的音效", (SoundPack)soundPack.enumValueIndex);
            }

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}