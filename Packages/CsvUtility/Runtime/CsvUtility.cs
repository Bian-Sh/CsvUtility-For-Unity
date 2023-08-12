using System;
using System.Collections.Generic;
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
                throw new FileLoadException($"CSV 文件 {Path.GetFileNameWithoutExtension(file)}数据不足以支持读取，请为 csv 文件添加有效数据！ \n文件路径： {file}");
            }
            string[] headers = ParseLine(lines[0]);
            var map = GetFieldInfoMap<T>();
            for (int i = 1; i < lines.Length; i++)
            {
                var values = ParseLine(lines[i]);
                result.Add(SetObjectFieldData<T>(headers, values, map));
            }
            return result;
        }


        /// <summary>
        /// 从CSV文件中读取筛选器`filter`匹配`filterValue`的一个对象，并返回该对象.
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="file">文件路径</param>
        /// <param name="filter">筛选器字段名</param>
        /// <param name="filterValue">筛选器字段值</param>
        /// <returns>一个筛选器匹配的对象</returns>
        public static T Read<T>(string file, string filter, object filterValue) where T : new()
        {
            var lines = ReadAllLines(file);
            if (lines.Length <= 1) // 数据量不够不予处理
            {
                throw new FileLoadException($"CSV 文件 {Path.GetFileNameWithoutExtension(file)}数据量不足以支持读取，请为 csv 文件添加有效数据！ \n文件路径： {file}");
            }
            string[] headers = ParseLine(lines[0]);
            if (!headers.Contains(filter))
            {
                throw new Exception($"CSV 表头中没找到用于断言的字段 {filter} ,请指定正确的 CSV 和数据类型！");
            }
            int headerIndex = Array.IndexOf(headers, filter);

            for (int i = 1; i < lines.Length; i++)
            {
                var values = ParseLine(lines[i]);
                if (values[headerIndex].Equals(filterValue.ToString()))
                {
                    var map = GetFieldInfoMap<T>();
                    return SetObjectFieldData<T>(headers, values, map);
                }
            }
            return default;
        }


        /// <summary>
        /// 从 CSV 中读取 filter 断言的行的数据并对指定的对象填充
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="target">目标对象</param>
        /// <param name="filter">用于确定取哪一行的字段名称</param>
        /// <param name="file">csv 文件路径</param>
        /// <exception cref="Exception">XXX</exception>
        public static void FromCsvOverwrite<T>(string file, T target, string filter) where T : new()
        {
            if (null == target)
            {
                throw new ArgumentNullException("传入的对象不得为空!");
            }
            var lines = ReadAllLines(file);
            var fileName = Path.GetFileName(file);
            if (lines.Length <= 1) // 数据量不够不予处理
            {
                throw new FileLoadException($"CSV 文件 {fileName}数据量不足以支持读取，请为 csv 文件添加有效数据！ \n文件路径： {file}");
            }
            string[] headers = ParseLine(lines[0]);
            if (!headers.Contains(filter))
            {
                throw new Exception($"CSV 表头中没找到用于断言的字段 {filter} ,请指定正确的 CSV 和数据类型！");
            }
            var map = GetFieldInfoMap<T>();
            int headerIndex = Array.IndexOf(headers, filter);
            if (map.TryGetValue(headers[headerIndex], out var field))
            {
                var filtervalue = field.GetValue(target).ToString();
                var values = lines.Select(line => ParseLine(line))
                                                    .Where(arr => arr[headerIndex].Equals(filtervalue))
                                                    .ToArray();
                if (values?.Length <= 0)
                {
                    Debug.LogWarning($"请留意，CSV 文件 {fileName} 中未找到 {field.Name} = {filtervalue} 的条目，请确信数据是否匹配！");
                }
                else
                {
                    if (values?.Length > 1)
                    {
                        Debug.LogWarning($"请留意，CSV 文件 {fileName} 中未找到 {values.Length} 条 {field.Name} = {filtervalue} 的条目，取第一条！");
                    }
                    SetObjectFieldData(headers, values[0], map, target);
                }
            }
            else
            {
                throw new Exception($"请留意，CSV 文件 {fileName} 表头信息与类型 {typeof(T).Name} 成员（含别名信息）均不匹配！ ");
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
                GenerateCSVData(item, headers, map, sb);
                sb.AppendLine();
            }
            Save(path, sb.ToString());
        }


        /// <summary>
        /// 将给定的类型T写入CSV文件。
        /// </summary>
        /// <typeparam name="T">要写入CSV文件的类型。</typeparam>
        /// <param name="target">要写入CSV文件的对象。</param>
        /// <param name="path">CSV文件的路径。</param>
        /// <param name="filter">筛选CSV文件中数据的过滤器。<see cref="KeyinType.Update"/> 模式下用于查找，<see cref="KeyinType.Append"/>  模式下用于去重</param>
        /// <param name="keyinType">数据键入的方式。</param>
        public static void Write<T>(T target, string path, string filter, KeyinType keyinType) where T : new()
        {
            var lines = ReadAllLines(path);
            var fileName = Path.GetFileNameWithoutExtension(path);
            if (lines.Length <= 1 && keyinType == KeyinType.Update)
            {
                Debug.LogError($"CSV 文件 {fileName}没有数据可供更新，请为 csv 文件添加有效数据！ \n文件路径： {path}");
                return;
            }
            var map = GetFieldInfoMap<T>();
            if (lines.Length == 0)
            {
                Array.Resize(ref lines, 1);
                lines[0] = string.Join(",", map.Keys);
            }
            string[] headers = ParseLine(lines[0]);
            if (!headers.Contains(filter))
            {
                throw new Exception($"用于断言的字段 {filter} 在 CSV 表头中没找到,请指定正确的 CSV 文件和正确的数据类型！");
            }
            int headerIndex = Array.IndexOf(headers, filter);
            bool found = false;
            if (map.TryGetValue(headers[headerIndex], out var field))
            {
                for (int i = 1; i < lines.Length; i++)
                {
                    var values = ParseLine(lines[i]);
                    if (values[headerIndex].Equals(field.GetValue(target).ToString()))
                    {
                        found = true;
                        if (keyinType == KeyinType.Update)
                        {
                            var sb = GenerateCSVData(target, headers, map);
                            lines[i] = sb.ToString();
                        }
                        else
                        {
                            throw new Exception("指定行数据已存在,如需更新数据请使用 KeyinType.Update");
                        }
                        break;
                    }
                }
                if (!found)
                {
                    if (keyinType == KeyinType.Update)
                    {
                        throw new Exception("指定行数据不存在,无法完成数据的更新，如需新增数据请使用 KeyinType.Append");
                    }
                    else
                    {
                        lines[^1] += "\n" + GenerateCSVData(target, headers, map);
                    }
                }
                File.WriteAllLines(path, lines, Encoding.UTF8);
            }
            else
            {
                throw new Exception($"请留意，CSV 文件 {fileName} 表头信息与类型 {typeof(T).Name} 成员（含别名信息）均不匹配！ ");
            }
        }
        #region Assistant Function
        private static string[] ReadAllLines(string file)
        {
            if (!File.Exists(file))
            {
                throw new FileNotFoundException($"CSV 文件{Path.GetFileNameWithoutExtension(file)}不存在，请检查文件路径！\n文件路径：{file}");
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
        private static T SetObjectFieldData<T>(string[] headers, string[] values, Dictionary<string, FieldInfo> map, T target = default) where T : new()
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
        private static StringBuilder GenerateCSVData<T>(T target, string[] headers, Dictionary<string, FieldInfo> map, StringBuilder sb = default)
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
