using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM
{
    public class BaseContext //: DBContext
    {
        [IdentityAttribute]
        /// <summary>
        /// 所有子类的父类 非自增GUID
        /// </summary>
        public string Id { get; set; }
    }
}
