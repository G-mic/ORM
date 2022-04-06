using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM
{
    /// <summary>
    /// 表名特性类
    /// </summary>
    public class TableAttribute : BaseAttribute
    {
        public TableAttribute(string name) : base(name)
        {

        }
    }
}
