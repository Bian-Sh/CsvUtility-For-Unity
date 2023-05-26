# CsvUtility-For-Unity

此工具是我与 AI 共同打造的小巧单文件，可用于 CSV 读写。本工具易于使用，支持自定义。

This tool is a compact single file co-created by me and AI for CSV read/write. It is user-friendly, customizable, and supports bilingual usage.

我想他应该和 JsonUtiliy 一样简单好用。

I think it should be as easy to use as JsonUtiliy.

# 开发环境
Unity 2021.3.16f1

# 功能简介
1. 简单来说本工具支持读取所有行，读取指定行，根据csv数据更新指定对象，根据指定对象更新或者新增csv数据。

    Simply put, this tool supports reading all rows, reading specified rows, updating specified objects based on csv data, and updating or adding csv data based on specified objects.

 2. 支持使用 [CsvIgnoreAttribute] 标记不需要处理的字段。

	Support using [CsvIgnoreAttribute] to mark fields that do not need to be processed.

# 使用方法
* 将 CsvUtility.cs 放到你的项目中

    Put CsvUtility.cs into your project

* 参考测试用例就好啦,以下代码均来自于测试用例

    Refer to the test case, the following code is from the test case

* 如果数据量过大，为避免主线程卡死，以下操作可以在 Task 中进行。

    If the data volume is too large, in order to avoid the main thread from being stuck, the following operations can be performed in the Task.


>1. 读取 csv 文件内容到指定的对象列表

```csharp
List<DisplayConfiguration> result = CsvUtility.Read<DisplayConfiguration>(testCsvPath);
```

>2. 将指定的对象列表存储成 csv

```csharp
 var data = new List<DisplayConfiguration>
 {
     new DisplayConfiguration { index = 2, size_x = 4.5f, size_y = 4.9f, width = 1922, height = 1082 },
     new DisplayConfiguration { index = 3, size_x = 5.5f, size_y = 5.9f, width = 1923, height = 1083 }
 };
 CsvUtility.Write(data, testCsvPath);

```

>3. 更新给定的对象中的数据

 ```csharp

    var target = new DisplayConfiguration() { index = 1 };
    CsvUtility.FromCsvOverwrite(testCsvPath, target, nameof(target.index));
            
```


>4. 根据指定的对象更新或者新增 csv 数据

```csharp
//更新数据，会根据指定的字段和值查找到对应的行，然后更新该行的数据

 var target = new DisplayConfiguration { index = 1, size_x = 4.5f, size_y = 4.9f, width = 1925, height = 1085 };
 CsvUtility.Write(target, testCsvPath, "index", KeyinType.Update);

//新增数据，会在 csv 的最后一行新增一行数据，如果指定的字段的值在 csv 中已经存在，则会抛出异常

 var target = new DisplayConfiguration { index = 4, size_x = 1.1f, size_y = 6.6f, width = 1928, height = 1088 };
 CsvUtility.Write(target, testCsvPath, "index", KeyinType.Append);

```

>5. 根据指定字段和值获取指定行的数据并返回一个对象

```csharp
DisplayConfiguration result = CsvUtility.Read<DisplayConfiguration>(testCsvPath, "index", 1);
```

>6. 支持通过 CsvIgnoreAttribute 标记不需要处理的字段

```csharp
  public class DisplayConfiguration
  {
        [CsvIgnore]
        public string name;
        public int index;
  }
```


# 单例测试
![](doc/TestRunner.png)


