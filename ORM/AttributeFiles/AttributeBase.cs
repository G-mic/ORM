using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM
{
    /// <summary>
    /// 特性父类 用于标记不同类表名和不同属性字段名
    /// </summary>
    public class AttributeBase : Attribute
    {
        public static string Name { get; set; } = null;
        public AttributeBase(string name)
        {
            Name = name;
        }
        public string GetName()
        {
            return Name;
        }
    }
}
