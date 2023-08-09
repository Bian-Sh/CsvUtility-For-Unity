using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using zFramework.Extension;
namespace zFramework.Examples
{
    public class SaveToCSVWithIgnore : Base
    {
        public override string Title { get; } = "Save";

        public override string Description =>IsChineseUser?
            "将一组对象存储到 .csv 文件中，字段标记 [CSVIgnore] 不会被保存,在本例字段 b 被忽略" :
            "Store a group of objects into a .csv file, and fields marked with [CSVIgnore] will not be saved. In this example, field \"b\" is ignored.";

        public override void Execute()
        {
            var file = Path.Combine(Application.persistentDataPath, "b.csv");
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

            CsvUtility.Write(list, file);
#if UNITY_EDITOR
            UnityEditor.EditorUtility.RevealInFinder(file);
#endif
            // test load
            var list2 = CsvUtility.Read<A>(file);
            foreach (var item in list2)
            {
                Debug.Log($" a = {item.a} , b = {item.b}, c = {item.c} , d = {item.d}");
            }
        }

        class A
        {
            public int a;
            [CsvIgnore]
            public string b;
            public float c;
            public bool d;
        }
    }
}