using System.IO;
using UnityEngine;
using zFramework.Extension;
namespace zFramework.Examples
{
    public class UpdateSpecifyOne : Base
    {
        public override string Title { get; } = "Update";
        public override string Description => IsChineseUser ?
            "加载 a = 1 的数据到 A 实例，修改并更新到 csv 文件 " :
            "Load data with a = 1 into instance A, modify and update it to the CSV file.";
        public override void Execute()
        {
            var csv = "a,bb,c,d,e\n1,2,3.1,true,11\n4,5,6.1,false,22\n7,8,9.0,true,33";
            System.IO.File.WriteAllText(File, csv);

            // test load specify one where a equal 1
            // e must equal default value 0, because it is ignored
            // return null if not find
            var item = CsvUtility.Read<A>(File, v => v.a == 1);
            Debug.Log($"Before csv = {csv}");
            Debug.Log($"Before : a = {item.a} , b = {item.b}, c = {item.c} , d = {item.d}, e = {item.e}");
            item.b = "bbb";
            item.c = 5.55f;
            CsvUtility.Write(item, File, v => v.a == 1, KeyinType.Update);
            // load again
            item = CsvUtility.Read<A>(File, v => v.a == 1);
            var csv2 = System.IO.File.ReadAllText(File);
            Debug.Log($"Before csv = {csv2}");
            Debug.Log($"After : a = {item.a} , b = {item.b}, c = {item.c} , d = {item.d}, e = {item.e}");
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