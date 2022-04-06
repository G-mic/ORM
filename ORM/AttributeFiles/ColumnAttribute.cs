﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM
{
    /// <summary>
    /// 字段名标记特性类
    /// </summary>
    public class ColumnAttribute : BaseAttribute
    {
        public ColumnAttribute(string name) : base(name)
        {

        }
    }
}
