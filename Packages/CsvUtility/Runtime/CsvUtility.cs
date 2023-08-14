using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace zFramework.Extension
{
    /// <summary>
    /// CSV 文件处理工具类
    /// </summary>
    public static class CsvUtility
    {
        /// <summary>
        ///  从csv文件中读取所有行返回指定类型对象列表
        /// </summary>
        /// <typeparam name="T">指定的类型</typeparam>
        /// <param name="file">csv 文件路径</param>
        /// <returns>指定类型的对象列表</returns>
        public static List<T> Read<T>(string file) where T : new()
        {
            var lines = ReadAllLines(file);
            var result = new List<T>();
            if (lines?.Length <= 1)
            {
                throw new FileLoadException($"{Path.GetFileName(file)} 数据不足以支持读取，请为 csv 文件添加有效数据！ \n文件路径： {file}");
            }
            string[] headers = ParseLine(lines[0]);
            var map = GetFieldInfoMap<T>();
            for (int i = 1; i < lines.Length; i++)
            {
                var values = ParseLine(lines[i]);
                result.Add(ToObject<T>(headers, values, map));
            }
            return result;
        }


        /// <summary>
        /// 从 CSV 文件中读取一个对象，需要提供一个 lamba 表达式来匹配目标对象
        /// </summary>
        /// <typeparam name="T">读取的对象类型</typeparam>
        /// <param name="file">CSV 文件路径</param>
        /// <param name="predicate">用于匹配目标对象的 lamba 表达式</param>
        /// <returns>一个匹配的 T 类型对象，如果不存在则返回 default</returns>
        /// <exception cref="FileNotFoundException">CSV 文件未找到</exception>  
        /// <exception cref="FileLoadException">CSV 文件数据量或路径不正确</exception>  
        /// <exception cref="InvalidCastException">CSV 文件数据类型转换失败</exception>  
        public static T Read<T>(string file, [NotNull] Predicate<T> predicate) where T : new()
        {
            var lines = ReadAllLines(file);
            var filename = Path.GetFileName(file);
            if (lines.Length <= 1) // 数据量不够不予处理
            {
                throw new FileLoadException($"{filename} 数据量不足以支持读取，请为 csv 文件添加有效数据！ \n文件路径： {file}");
            }
            string[] headers = ParseLine(lines[0]);
            var map = GetFieldInfoMap<T>();
            var target = new T();
            var arr = lines.Select(line => ParseLine(line))
                .Skip(1) //跳过 header
                .Select(values => ToObject<T>(headers, values, map))
                .Where(v => predicate?.Invoke(v) == true)
                .ToArray();
            switch (arr.Length)
            {
                case 0:
                    Debug.LogWarning($"{filename} 中不存在匹配的对象！ \n文件路径： {file}");
                    return default;
                case > 1:
                    Debug.LogWarning($"{filename} 中存在多个匹配的对象，取第一个！ \n文件路径： {file}");
                    break;
            }
            return arr[0];
        }


        /// <summary>
        /// 从 CSV 文件中读取数据，并根据传递的 T 类型的对象的属性来填充它。
        /// </summary>
        /// <typeparam name="T">要填充的对象的类型。</typeparam>
        /// <param name="file">CSV 文件的路径。</param>
        /// <param name="target">要填充的对象。</param>
        /// <param name="predicate">可选的断言，用于指定匹配对象。</param>
        /// <exception cref="ArgumentNullException">传入的对象不得为空</exception>
        /// <exception cref="FileLoadException">CSV 文件数据量不足以支持读取</exception>
        /// <exception cref="FileNotFoundException">未找到相应的文件</exception>
        /// <exception cref="InvalidCastException">类型转换出现错误</exception>
        public static void FromCsvOverwrite<T>(string file, [NotNull] T target, [NotNull] Predicate<T> predicate) where T : new()
        {
            if (null == target)
            {
                throw new ArgumentNullException("传入的对象不得为空!");
            }
            if (null == predicate)
            {
                throw new ArgumentNullException("predicate 必须有意义！");
            }
            var temp = Read<T>(file, predicate);
            if (temp != null)
            {
                Clone(temp, target);
            }
        }

        /// <summary>
        /// 将一组实例写入csv文件，如果存在则覆盖
        /// </summary>
        /// <typeparam name="T">实例类型</typeparam>
        /// <param name="target">将要保存的实例</param>
        /// <param name="path">csv 路径</param>
        public static void Write<T>(List<T> target, string path) where T : new()
        {
            var map = GetFieldInfoMap<T>();
            var headers = map.Keys.ToArray();
            StringBuilder sb = new();
            for (int i = 0; i < headers.Length; i++)
            {
                sb.Append(headers[i]);
                if (i < headers.Length - 1)
                {
                    sb.Append(",");
                }
            }
            sb.AppendLine();
            foreach (var item in target)
            {
                ToCSV(item, headers, map, sb);
                sb.AppendLine();
            }
            Save(path, sb.ToString());
        }


        /// <summary>
        /// 将给定的类型T写入CSV文件
        /// </summary>
        /// <typeparam name="T">要写入CSV文件的类型。</typeparam>
        /// <param name="target">要写入CSV文件的对象。</param>
        /// <param name="path">CSV文件的路径。</param>
        /// <param name="predicate">筛选CSV文件中数据的过滤器。<see cref="KeyinType.Update"/> 模式下用于查找，<see cref="KeyinType.Append"/>  模式下用于去重</param>
        /// <param name="keyinType">数据键入的方式。</param>
        public static void Write<T>([NotNull] T target, string path, [NotNull] Predicate<T> predicate, KeyinType keyinType) where T : new()
        {
            if (null == target)
            {
                throw new ArgumentNullException("传入的对象不得为空!");
            }
            if (null == predicate)
            {
                throw new ArgumentNullException("predicate 必须有意义！");
            }
            var lines = ReadAllLines(path);
            var fileName = Path.GetFileName(path);
            var map = GetFieldInfoMap<T>();

            if (lines.Length == 0 && keyinType == KeyinType.Append) // header 都没有? 追加模式下自主写入
            {
                lines = new string[] { string.Join(",", map.Keys) };
            }

            var headers = ParseLine(lines[0]);
            if (lines.Length == 1) //处理只有 header 的情况
            {
                if (keyinType == KeyinType.Append) // 未找到且为追加模式
                {
                    lines[^1] += $"\n{ToCSV(target, headers, map)}"; //  追加到文件末尾
                    File.WriteAllLines(path, lines);
                    return;
                }
                else
                {
                    throw new Exception($"{fileName} 找不到特征数据,无法完成更新，新增数据请使用 KeyinType.Append！\n文件路径： {path}");
                }
            }

            // 为了做断言而加载完整的数据，这也是为啥推荐使用 sqlite 平替的原因
            // 在数据量较小的情况下，方便就好，还要什么自行车
            var datas = Read<T>(path);
            var index = datas.FindIndex(predicate);

            if (index == -1)
            {
                if (keyinType == KeyinType.Append) // 未找到且为追加模式
                {
                    lines[^1] += $"\n{ToCSV(target, headers, map)}"; //  追加到文件末尾
                }
                else
                {
                    throw new Exception($"{fileName} 找不到特征数据,无法完成更新，新增数据请使用 KeyinType.Append！\n文件路径： {path}");
                }
            }
            else
            {
                if (keyinType == KeyinType.Update)   //  找到了且为更新模式
                {
                    // 仅更新指定行数据，避免 CSVIgnore 标记影响到 csv 文件
                    //  由于datas 是跳过了 header 的，因此 lines 的索引需要 +1
                    lines[index + 1] = ToCSV(target, headers, map).ToString();
                }
                else
                {
                    throw new Exception("指定行数据已存在,如需更新数据请使用 KeyinType.Update");
                }
            }
            File.WriteAllLines(path, lines);
        }
        #region Assistant Function

        private static void Clone<T>(T source, T target)
        {
            var fields = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public)
                .Where(v => v.GetCustomAttribute<CsvIgnoreAttribute>() == null); // CSVIgnore 标记的字段不进行复制
            foreach (var field in fields)
            {
                field.SetValue(target, field.GetValue(source));
            }
        }

        private static string[] ReadAllLines(string file)
        {
            if (!File.Exists(file))
            {
                throw new FileNotFoundException($"{Path.GetFileName(file)} 不存在，请检查文件路径！\n文件路径：{file}");
            }
            var temp = Path.GetTempFileName();
            File.Copy(file, temp, true);
            return File.ReadAllLines(temp);
        }
        private static string[] ParseLine(string line)
        {
            List<string> result = new();
            bool inQuotes = false;
            StringBuilder currentValue = new();
            for (int i = 0; i < line.Length; i++)
            {
                char currentChar = line[i];
                if (currentChar == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (currentChar == ',' && !inQuotes)
                {
                    result.Add(currentValue.ToString());
                    currentValue.Clear();
                }
                else
                {
                    currentValue.Append(currentChar);
                }
            }
            result.Add(currentValue.ToString());
            return result.ToArray();
        }
        private static T ToObject<T>(string[] headers, string[] values, Dictionary<string, FieldInfo> map, T target = default) where T : new()
        {
            target ??= new T();
            for (int i = 0; i < headers.Length; i++)
            {
                if (!map.ContainsKey(headers[i]))
                    continue;
                var fieldInfo = map[headers[i]];
                try
                {
                    fieldInfo.SetValue(target, Convert.ChangeType(values[i], fieldInfo.FieldType));
                }
                catch (Exception)
                {
                    throw new InvalidCastException($"{nameof(CsvUtility)}: 字段 {headers[i]} 指定的数据{values[i]} 不是 {fieldInfo.FieldType} 类型，请修改csv中数据！");
                }
            }
            return target;
        }

        // 必须传入 headers ，否则无法判断csv 列的顺序，无法判断 csv 中忽略的列
        private static StringBuilder ToCSV<T>(T target, string[] headers, Dictionary<string, FieldInfo> map, StringBuilder sb = default)
        {
            sb ??= new();
            for (int i = 0; i < headers.Length; i++)
            {
                if (map.TryGetValue(headers[i], out var field))
                {
                    var value = field.GetValue(target);
                    if (value != null && value.ToString().Contains(","))
                    {
                        sb.Append("\"" + value + "\"");
                    }
                    else
                    {
                        sb.Append(value);
                    }
                    if (i < headers.Length - 1)
                    {
                        sb.Append(",");
                    }
                }
                else //如果 csv 中列名在类型中不存在，则忽略该列，直接跳过
                {
                    sb.Append(",");
                }
            }
            return sb;
        }
        // 过滤带有 CsvIgnoreAttribute 属性的字段
        // 创建一个字典来存储字段名称或别名与 FieldInfo之间的映射关系
        // 约定：字段名称或别名必须与 csv 表头中的字段名称一致
        private static Dictionary<string, FieldInfo> GetFieldInfoMap<T>() where T : new()
        {
            return typeof(T).GetFields()
                   .Where(f => f.GetCustomAttribute<CsvIgnoreAttribute>() == null)
                   .ToDictionary(f => f.GetCustomAttribute<ColumnAttribute>()?.name ?? f.Name, f => f);
        }
        private static void Save(string path, string content)
        {
            var folder = Path.GetDirectoryName(path);
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            File.WriteAllText(path, content, Encoding.UTF8);
        }
        #endregion
    }

    #region Assistant Type & Struct
    public enum KeyinType
    {
        Update,
        Append
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class CsvIgnoreAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Field)]
    public class ColumnAttribute : Attribute
    {
        public string name;
        public ColumnAttribute(string name) => this.name = name;
    }
    #endregion
}
