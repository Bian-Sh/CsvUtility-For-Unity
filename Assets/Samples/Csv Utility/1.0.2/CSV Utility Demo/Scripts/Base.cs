using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace zFramework.Examples
{
    using System.IO;
    using System.Threading;
#if UNITY_EDITOR
    using UnityEditor;
    [CustomEditor(typeof(Base), editorForChildClasses: true)]
    class BaseEditor : Editor
    {
        Base @base;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (@base == null)
            {
                @base = target as Base;
            }
            if (!string.IsNullOrEmpty(@base.Description))
            {
                EditorGUILayout.HelpBox(@base.Description, MessageType.Info);
            }
            if (GUILayout.Button(@base.Title))
            {
                @base.Execute();
            }
        }
    }
#endif

    public abstract class Base : MonoBehaviour
    {
        public string File => Path.Combine(Application.persistentDataPath, "sample.csv");
        public abstract string Title { get; }
        public abstract string Description { get; }
        public bool IsChineseUser => Thread.CurrentThread.CurrentCulture.Name == "zh-CN";
        public abstract void Execute();
    }

}