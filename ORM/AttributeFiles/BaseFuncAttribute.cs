using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ORM
{
    /// <summary>
    /// 基类特性扩展方法
    /// </summary>
    public static class BaseFuncAttribute
    {
        /// <summary>
        /// 获取特性标记表名
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetTableMappingName(this Type type)
        {
            if (type.IsDefined(typeof(BaseAttribute), true))
                return type.GetCustomAttribute<TableAttribute>().GetName();  //获取特性中的名字
            else
                return type.Name;
        }

        /// <summary>
        /// 获取特性标记字段名
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public static string GetColumnMappingName(this PropertyInfo info)
        {
            if (info.IsDefined(typeof(BaseAttribute), true))
                return info.GetCustomAttribute<ColumnAttribute>().GetName();
            else
                return info.Name;
        }
    }
}
