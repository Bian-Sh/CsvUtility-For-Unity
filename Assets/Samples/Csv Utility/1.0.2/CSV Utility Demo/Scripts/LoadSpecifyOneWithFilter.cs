using System.IO;
using UnityEngine;
using zFramework.Extension;
namespace zFramework.Examples
{
    public class LoadSpecifyOneWithFilter : Base
    {
        public override string Title { get; } = "Load";
        public override string Description => IsChineseUser ?
            "加载指定的某一行，在本例，分别加载 a = 1 、bb =5 、e = 33 数据到 A 对象" :
            "Load the specified line, in this example, separate loading data a = 1, bb = 5, e = 33 into object A.";
        public override void Execute()
        {
            var csv = "a,bb,c,d,e\n1,2,3.1,true,11\n4,5,6.1,false,22\n7,8,9.0,true,33";
            System.IO.File.WriteAllText(File, csv);

            // test load specify one where a equal 1
            var item = CsvUtility.Read<A>(File, v => v.a == 1);
            Debug.Log($" a = {item.a} , b = {item.b}, c = {item.c} , d = {item.d}, e = {item.e}");

            item = CsvUtility.Read<A>(File, v => v.b == "5");
            Debug.Log($" a = {item.a} , b = {item.b}, c = {item.c} , d = {item.d}, e = {item.e}");

            // can not use "e" for querying  as "e" always equals to default value 0, because it is ignored , return null 
            item = CsvUtility.Read<A>(File, v => v.e == 33);
            Debug.Log($" a = {item?.a} , b = {item?.b}, c = {item?.c} , d = {item?.d}, e = {item?.e} , item is null: {item == null}");
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