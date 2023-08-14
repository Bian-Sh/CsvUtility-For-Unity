using System.Collections.Generic;
using System.IO;
using UnityEngine;
using zFramework.Extension;
namespace zFramework.Examples
{
    public class SaveToCSV : Base
    {
        public override string Title { get; } = "Save";
        public override string Description =>IsChineseUser?
            "将一组对象存储到 .csv 文件中":
            "Save a group of objects to a .csv file";
        public override void Execute()
        {
            var list = new List<A>
        {
            new A
            {
                a = 1,
                b = "2",
                c = 3.1f,
                d = true
            },
            new A
            {
                a = 4,
                b = "5",
                c = 6.1f,
                d = false
            },
            new A
            {
                a = 7,
                b = "8",
                c = 9.0f,
                d = true
            }
        };

            CsvUtility.Write(list, File);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.RevealInFinder(File);
            Debug.Log($"{nameof(SaveToCSV)}: check the csv file ~");
#endif
            // test load
            var list2 = CsvUtility.Read<A>(File);
            foreach (var item in list2)
            {
                Debug.Log($" a = {item.a} , b = {item.b}, c = {item.c} , d = {item.d}");
            }
        }

        class A
        {
            public int a;
            public string b;
            public float c;
            public bool d;
        }
    }
}