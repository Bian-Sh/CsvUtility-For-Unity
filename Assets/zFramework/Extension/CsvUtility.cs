using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

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
            if (lines?.Length <= 0)
            {
                throw new FileLoadException($"CSV 文件{Path.GetFileNameWithoutExtension(file)}不含任何数据，请为 csv 文件添加有效数据！\n文件路径：{file}");
            }
            string[] headers = ParseLine(lines[0]);
            for (int i = 1; i < lines.Length; i++)
            {
                var values = ParseLine(lines[i]);
                result.Add(SetObjectFieldData<T>(headers, values));
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
                    return SetObjectFieldData<T>(headers, values);
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
        /// <param name="path">csv 文件路径</param>
        /// <exception cref="Exception">XXX</exception>
        public static void FromCsvOverwrite<T>(string path, T target, string filter) where T : new()
        {
            var lines = ReadAllLines(path);
            string[] headers = ParseLine(lines[0]);
            if (!headers.Contains(filter))
            {
                throw new Exception($"CSV 表头中没找到用于断言的字段 {filter} ,请指定正确的 CSV 和数据类型！");
            }
            int headerIndex = Array.IndexOf(headers, filter);
            for (int i = 1; i < lines.Length; i++)
            {
                var values = ParseLine(lines[i]);
                if (values[headerIndex].Equals(target.GetType().GetField(filter).GetValue(target).ToString()))
                {
                    SetObjectFieldData(headers, values, target);
                    break;
                }
            }
        }


        /// <summary>
        /// 将一组实例写入csv文件
        /// </summary>
        /// <typeparam name="T">实例类型</typeparam>
        /// <param name="target">将要保存的实例</param>
        /// <param name="path">csv 路径</param>
        public static void Write<T>(List<T> target, string path)
        {
            var fields = typeof(T).GetFields().Where(f => !f.IsDefined(typeof(CsvIgnoreAttribute))).ToArray();
            StringBuilder sb = new();
            for (int i = 0; i < fields.Length; i++)
            {
                sb.Append(fields[i].Name);
                if (i < fields.Length - 1)
                {
                    sb.Append(",");
                }
            }
            sb.AppendLine();
            foreach (var item in target)
            {
                for (int i = 0; i < fields.Length; i++)
                {
                    var value = fields[i].GetValue(item);
                    if (value != null && value.ToString().Contains(","))
                    {
                        sb.Append("\"" + value + "\"");
                    }
                    else
                    {
                        sb.Append(value);
                    }
                    if (i < fields.Length - 1)
                    {
                        sb.Append(",");
                    }
                }
                sb.AppendLine();
            }
            File.WriteAllText(path, sb.ToString());
        }

        /// <summary>
        /// 将给定的类型T写入CSV文件。
        /// </summary>
        /// <typeparam name="T">要写入CSV文件的类型。</typeparam>
        /// <param name="target">要写入CSV文件的对象。</param>
        /// <param name="path">CSV文件的路径。</param>
        /// <param name="filter">筛选CSV文件中数据的过滤器。<see cref="KeyinType.Update"/> 模式下用于查找，<see cref="KeyinType.Append"/>  模式下用于去重</param>
        /// <param name="keyinType">数据键入的方式。</param>
        public static void Write<T>(T target, string path, string filter, KeyinType keyinType)
        {
            var lines = ReadAllLines(path);
            string[] headers = ParseLine(lines[0]);
            if (!headers.Contains(filter))
            {
                throw new Exception($"用于断言的字段 {filter} 在 CSV 表头中没找到,请指定正确的 CSV 文件和正确的数据类型！");
            }

            int headerIndex = Array.IndexOf(headers, filter);
            bool found = false;
            for (int i = 1; i < lines.Length; i++)
            {
                var values = ParseLine(lines[i]);
                if (values[headerIndex].Equals(target.GetType().GetField(filter).GetValue(target).ToString()))
                {
                    found = true;
                    if (keyinType == KeyinType.Update)
                    {
                        lines[i] = GenerateCSVData(target, headers);
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
                    lines[lines.Length - 1] += "\n" + GenerateCSVData(target, headers);
                }
            }
            File.WriteAllLines(path, lines);
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
        private static T SetObjectFieldData<T>(string[] headers, string[] values, T target = default) where T : new()
        {
            target ??= new();
            for (int j = 0; j < headers.Length; j++)
            {
                var field = typeof(T).GetField(headers[j]);
                try
                {
                    var ignore = field?.GetCustomAttribute<CsvIgnoreAttribute>(false);
                    if (field != null && ignore == null)
                    {
                        field.SetValue(target, Convert.ChangeType(values[j], field.FieldType));
                    }
                }
                catch (Exception)
                {
                    throw new InvalidCastException($"{nameof(CsvUtility)}: 字段 {headers[j]} 指定的数据{values[j]} 不是 {field.FieldType} 类型，请修改csv中数据！");
                }
            }
            return target;
        }
        private static string GenerateCSVData<T>(T target, string[] headers)
        {
            StringBuilder sb = new();
            for (int j = 0; j < headers.Length; j++)
            {
                var field = typeof(T).GetField(headers[j]);
                if (field != null)
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
                    if (j < headers.Length - 1)
                    {
                        sb.Append(",");
                    }
                }
            }
            return sb.ToString();
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
    #endregion
}
