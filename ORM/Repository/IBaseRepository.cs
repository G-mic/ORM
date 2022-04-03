using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM
{
    public interface IBaseRepository
    {
        T Find<T>(string id) where T : BaseContext;
        List<T> Find<T>() where T : BaseContext;
        bool Update<T>(T t) where T : BaseContext;
        bool Update<T>(List<T> ts) where T : BaseContext;
        bool Insert<T>(T t) where T : BaseContext;
        bool Insert<T>(List<T> ts) where T : BaseContext;
        bool Delete<T>(T t) where T : BaseContext;
        bool Delete<T>(List<T> ts) where T : BaseContext;
    }
}
