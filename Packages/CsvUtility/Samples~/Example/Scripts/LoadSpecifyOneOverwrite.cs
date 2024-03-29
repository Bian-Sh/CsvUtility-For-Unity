using System.IO;
using UnityEngine;
using zFramework.Extension;
namespace zFramework.Examples
{
    public class LoadSpecifyOneOverwrite : Base
    {
        public int a = 7;
        public override string Title { get; } = "Load";
        public override string Description => IsChineseUser ?
            "加载指定的某一行，在本例，加载 a = 1 这一行数据到已存在的 A 对象, 尝试分别输入 1、7、10 并点击 Load 查看结果" :
            "Load a specify column by filter, in this sample , load the data  where \"a =1\" into a exist \"A\" instance，Enter 1, 7, and 10, then click Load to perform a check.";
        public override void Execute()
        {
            var csv = "a,bb,c,d,e\n1,2,3.1,true,11\n4,5,6.1,false,22\n7,8,9.0,true,33\n7,81,9.1,true,331";
            System.IO.File.WriteAllText(File, csv);

            // test load specify one where a equal 1
            // will load the data where a = 7 into the exist A instance
            //  e must equal default value 111, because it is ignored
            // others will be overwrite
            var a = new A
            {
                a = this.a,
                b = "22",
                c = 3.11f,
                d = true,
                e = 111
            };
            Debug.Log($"Before： a = {a.a} , b = {a.b}, c = {a.c} , d = {a.d}, e = {a.e} ");
            var cached = a;
            CsvUtility.FromCsvOverwrite(File, a, v => v.a == this.a);
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