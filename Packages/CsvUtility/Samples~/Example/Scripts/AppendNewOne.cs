using System.IO;
using UnityEngine;
using zFramework.Extension;
namespace zFramework.Examples
{
    public class AppendNewOne : Base
    {
        public override string Title { get; } = "Append";
        public override string Description => IsChineseUser ?
            "将 A 实例数据新增到 csv 文件最后一行" :
            "Add instance data A to the last row of the CSV file.";
        public override void Execute()
        {
            var csv = "a,bb,c,d,e\n1,2,3.1,true,11\n4,5,6.1,false,22\n7,8,9.0,true,33";
            System.IO.File.WriteAllText(File, csv);

            // as "e" is ignored , csv has no column named "e"
            // as "a" is for query, it must be unique
            var id = 10;
            var a = new A
            {
                a = id,
                b = "bbb",
                c = 5.55f,
                d = true,
                e = 100
            };
            Debug.Log($"{nameof(AppendNewOne)}:  before \n {csv}");
            CsvUtility.Write(a, File, v => v.a == id, KeyinType.Append);
            var csv2 = System.IO.File.ReadAllText(File);
            Debug.Log($"{nameof(AppendNewOne)}:  after \n {csv2}");
#if UNITY_EDITOR
            UnityEditor.EditorUtility.RevealInFinder(File);
#endif
        }

        class A
        {
            public int a;
            [Column("bb")]
            public string b;
            public float c;
            public bool d;
            [CsvIgnore]
            public int e;
        }
    }
}