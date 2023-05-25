# CsvUtility-For-Unity
a tiny csv read/wirte script, 单文件的 csv 读写工具类，我想他应该和 JsonUtiliy 一样简单好用。

# 开发环境
Unity 2021.3.16f1

# 功能

1. 读取 csv 文件内容到指定的对象列表

```csharp
List<DisplayConfiguration> result = CsvUtility.Read<DisplayConfiguration>(testCsvPath);
```

2. 将指定的对象列表存储成 csv

```csharp
 var data = new List<DisplayConfiguration>
 {
     new DisplayConfiguration { index = 2, size_x = 4.5f, size_y = 4.9f, width = 1922, height = 1082 },
     new DisplayConfiguration { index = 3, size_x = 5.5f, size_y = 5.9f, width = 1923, height = 1083 }
 };
 CsvUtility.Write(data, testCsvPath);

```

3. 根据指定的对象更新或者新增 csv 数据

```
更新数据
 var target = new DisplayConfiguration { index = 1, size_x = 4.5f, size_y = 4.9f, width = 1925, height = 1085 };
 CsvUtility.Write(target, testCsvPath, "index", KeyinType.Update);

新增数据
 var target = new DisplayConfiguration { index = 4, size_x = 1.1f, size_y = 6.6f, width = 1928, height = 1088 };
 CsvUtility.Write(target, testCsvPath, "index", KeyinType.Append);

```

4. 根据指定字段和值获取指定行的数据并返回一个对象

```csharp
DisplayConfiguration result = CsvUtility.Read<DisplayConfiguration>(testCsvPath, "index", 1);
```

5. 支持通过 CsvIgnoreAttribute 标记不需要处理的字段

```csharp
  public class DisplayConfiguration
  {
        [CsvIgnore]
        public string name;
        public int index;
  }
```


# Unit Test Result
![](doc/TestRunner.png)


