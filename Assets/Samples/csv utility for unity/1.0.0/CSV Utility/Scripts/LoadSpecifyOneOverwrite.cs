using System.IO;
using UnityEngine;
using zFramework.Extension;
namespace zFramework.Examples
{
    public class LoadSpecifyOneOverwrite : Base
    {
        public override string Title { get; } = "Load";
        public override string Description => IsChineseUser ?
            "加载指定的某一行，在本例，加载 a = 1 这一行数据到已存在的 A 对象" :
            "Load a specify column by filter, in this sample , load the data  where \"a =1\" into a exist \"A\" instance";
        public override void Execute()
        {
            var file = Path.Combine(Application.persistentDataPath, "la.csv");
            var csv = "a,bb,c,d,e\n1,2,3.1,true,11\n4,5,6.1,false,22\n7,8,9.0,true,33";
            File.WriteAllText(file, csv);

            // test load specify one where a equal 1
            // will load the data where a = 7 into the exist A instance
            //  e must equal default value 11, because it is ignored
            // others will be overwrite
            var a = new A
            {
                a = 7,
                b = "2",
                c = 3.1f,
                d = true,
                e = 11
            };
            Debug.Log($"Before： a = {a.a} , b = {a.b}, c = {a.c} , d = {a.d}, e = {a.e} ");
            var cached = a;
            CsvUtility.FromCsvOverwrite(file, a, "a");
            Debug.Log($"After： a = {a.a} , b = {a.b}, c = {a.c} , d = {a.d}, e = {a.e} , a is not a new one: {cached == a} ");
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