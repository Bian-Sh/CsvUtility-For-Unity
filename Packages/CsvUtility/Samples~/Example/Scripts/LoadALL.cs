using System.IO;
using UnityEngine;
using zFramework.Extension;
namespace zFramework.Examples
{
    public class LoadALL : Base
    {
        public override string Title { get; } = "Load";
        public override string Description => IsChineseUser ?
            "加载全部的 csv 数据到列表中" :
            "Load all CSV data into a list.";
        public override void Execute()
        {
            var file = Path.Combine(Application.persistentDataPath, "la.csv");
            var csv = "a,bb,c,d,e\n1,2,3.1,true,11\n4,5,6.1,false,22\n7,8,9.0,true,33";
            File.WriteAllText(file, csv);

            // test load specify one where a equal 1
            // e must equal default value 0, because it is ignored
            // return null if not find
            var arr = CsvUtility.Read<A>(file);
            foreach (var item in arr)
            {
                Debug.Log($" a = {item.a} , b = {item.b}, c = {item.c} , d = {item.d}, e = {item.e}");
            }
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