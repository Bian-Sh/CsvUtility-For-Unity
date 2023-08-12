using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using zFramework.Extension;
namespace zFramework.Examples
{
    public class SaveToCSVWithColAliasName : Base
    {
        public override string Title { get; } = "Save";

        public override string Description =>IsChineseUser?
            "将一组对象存储到 .csv 文件中，将数据保存为带有列别名的CSV文件，使用 [Column] 标记" :
            "Store a group of objects into a .csv file, saving the data as a CSV file with column aliases using the [Column] attribute.";

        public override void Execute()
        {
            var file = Path.Combine(Application.persistentDataPath, "c.csv");
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
            [Column("ColumnB")]
            public string b;
            public float c;
            public bool d;
        }
    }
}