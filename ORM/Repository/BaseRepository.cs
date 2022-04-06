using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace ORM
{
    public class BaseRepository : IBaseRepository
    {
        private static string connStr = ConfigurationManager.ConnectionStrings["connString"].ToString();

        public T Find<T>(string id) where T : BaseContext
        {
            try
            {
                Type type = typeof(T);
                string sql = $"SELECT {string.Join(",", type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public).Select(p => $"[{p.GetColumnMappingName()}]"))} FROM [{type.GetTableMappingName()}] WHERE Id=@Id'";
                object obj = Activator.CreateInstance(type);
                var proPertyArray = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
                var sqlParameters = proPertyArray.Select(p => new SqlParameter($"@{p.GetColumnMappingName()}", p.GetValue(obj) ?? DBNull.Value));
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddRange(sqlParameters.ToArray());
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                        return Trans<T>(type, reader);
                    else
                        return default(T);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public List<T> Find<T>() where T : BaseContext
        {
            Type type = typeof(T);
            string sql = $"SELECT {string.Join(",", type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public).Select(p => p.GetColumnMappingName()))} FROM {type.GetTableMappingName()}";
            List<T> list = new List<T>();
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(Trans<T>(type, reader));
                }
            }
            return list;
        }

        public bool Update<T>(T t) where T : BaseContext
        {
            try
            {
                Type type = typeof(T);
                StringBuilder sb = new StringBuilder();
                sb.Append($" UPDATE {type.GetTableMappingName()} SET ");
                var proPertyList = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public).ToList();
                foreach (PropertyInfo property in proPertyList)
                {
                    if ((property.IsDefined(typeof(RequiredAttribute), true) || property.IsDefined(typeof(PrimaryKeyAttribute), true)) && property.GetValue(t) == null) //非空验证 主键和被标记的字段不能为空
                        return false;
                    if (property.IsDefined(typeof(IdentityAttribute), true)) //移除自增字段
                        proPertyList.Remove(property);
                    else
                        sb.Append($" {property}=@{property} ");
                }
                sb.Append($" WHERE Id=@Id ");
                var sqlParameters = proPertyList.Select(p => new SqlParameter($"@{p.GetColumnMappingName()}", p.GetValue(t) ?? DBNull.Value));
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    SqlCommand cmd = new SqlCommand(sb.ToString(), conn);
                    cmd.Parameters.AddRange(sqlParameters.ToArray());
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool Update<T>(List<T> ts) where T : BaseContext
        {
            try
            {
                if (ts.Count <= 0) return false;
                Type type = typeof(T);
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.Transaction = conn.BeginTransaction();
                    bool result = true;
                    foreach (T t in ts)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append($" UPDATE {type.GetTableMappingName()} SET ");
                        var proPertyList = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public).ToList();
                        foreach (PropertyInfo property in proPertyList)
                        {
                            if ((property.IsDefined(typeof(RequiredAttribute), true) || property.IsDefined(typeof(PrimaryKeyAttribute), true)) && property.GetValue(t) == null) //非空验证 主键和被标记的字段不能为空
                                return false;
                            if (property.IsDefined(typeof(IdentityAttribute), true)) //移除自增字段
                                proPertyList.Remove(property);
                            else
                                sb.Append($" {property}=@{property} ");
                        }
                        sb.Append($" WHERE Id=@Id ");
                        var sqlParameters = (proPertyList.Where(p => p.GetColumnMappingName() != "Id").Select(p => new SqlParameter($"@{p.GetColumnMappingName()}", p.GetValue(t) ?? DBNull.Value)));
                        cmd.CommandText = sb.ToString();
                        cmd.Parameters.AddRange(sqlParameters.ToArray());
                        if (cmd.ExecuteNonQuery() <= 0)
                        {
                            result = false;
                            //回滚
                            cmd.Transaction.Rollback();
                            //结束
                            break;
                        };
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool Insert<T>(T t) where T : BaseContext
        {
            try
            {
                Type type = typeof(T);
                StringBuilder sb = new StringBuilder();
                sb.Append($" INSERT INTO {type.GetTableMappingName()} ");
                var proPertyList = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public).ToList();
                foreach (PropertyInfo property in proPertyList)
                {
                    if ((property.IsDefined(typeof(RequiredAttribute), true) || property.IsDefined(typeof(PrimaryKeyAttribute), true)) && property.GetValue(t) == null) //非空验证 主键和被标记的字段不能为空
                        return false;
                    if (property.IsDefined(typeof(IdentityAttribute), true)) //移除自增字段
                        proPertyList.Remove(property);
                }
                sb.Append($" ({string.Join(",", proPertyList.Select(p => $"{p.GetColumnMappingName()}"))})");
                sb.Append($" ({string.Join(",", proPertyList.Select(p => $"@{p.GetColumnMappingName()}"))})");
                var sqlParameters = proPertyList.Select(p => new SqlParameter($"@{p.GetColumnMappingName()}", p.GetValue(t) ?? DBNull.Value));
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    SqlCommand cmd = new SqlCommand(sb.ToString(), conn);
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddRange(sqlParameters.ToArray());
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch(Exception ex)
            {
                throw;
            }
        }

        public bool Insert<T>(List<T> ts) where T : BaseContext
        {
            try
            {
                if (ts.Count <= 0) return false;
                Type type = typeof(T);
                using(SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.Transaction = conn.BeginTransaction();
                    bool result = true;
                    foreach (T t in ts)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append($" INSERT INTO {type.GetTableMappingName()} ");
                        var proPertyList = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public).ToList();
                        foreach (PropertyInfo property in proPertyList)
                        {
                            if ((property.IsDefined(typeof(RequiredAttribute), true) || property.IsDefined(typeof(PrimaryKeyAttribute), true)) && property.GetValue(t) == null) //非空验证 主键和被标记的字段不能为空
                                return false;
                            if (property.IsDefined(typeof(IdentityAttribute), true)) //移除自增字段
                                proPertyList.Remove(property);
                        }
                        sb.Append($" ({string.Join(",", proPertyList.Select(p => $"{p.GetColumnMappingName()}"))})");
                        sb.Append($" ({string.Join(",", proPertyList.Select(p => $"@{p.GetColumnMappingName()}"))})");
                        var sqlParameters = proPertyList.Select(p => new SqlParameter($"@{p.GetColumnMappingName()}", p.GetValue(t) ?? DBNull.Value));
                        cmd.CommandText = sb.ToString();
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddRange(sqlParameters.ToArray());
                        if (cmd.ExecuteNonQuery() <= 0)
                        {
                            result = false;
                            //回滚
                            cmd.Transaction.Rollback();
                            //结束
                            break;
                        };
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool Delete<T>(T t) where T : BaseContext
        {
            try
            {
                Type type = typeof(T);
                string sql = $" DELETE FROM {type.GetTableMappingName()} WHERE Id=@Id";
                var sqlParameter = new SqlParameter("Id", type.GetProperty("Id").GetValue(t) ?? DBNull.Value);
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.Add(sqlParameter);
                    conn.Open();
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public bool Delete<T>(List<T> ts) where T : BaseContext
        {
            try
            {
                if (ts.Count <= 0) return false;
                Type type = typeof(T);
                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    cmd.Transaction = conn.BeginTransaction();
                    bool result = true;
                    foreach (T t in ts)
                    {
                        string sql = $" DELETE FROM {type.GetTableMappingName()} WHERE Id=@Id";
                        var sqlParameter = new SqlParameter("Id", type.GetProperty("Id").GetValue(t) ?? DBNull.Value);
                        cmd.CommandText = sql;
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddRange(sqlParameters.ToArray());
                        if (cmd.ExecuteNonQuery() <= 0)
                        {
                            result = false;
                            //回滚
                            cmd.Transaction.Rollback();
                            //结束
                            break;
                        };
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private T Trans<T>(Type type, SqlDataReader redaer)
        {
            try
            {
                object obj = Activator.CreateInstance(type);
                foreach (PropertyInfo property in type.GetProperties())
                {
                    // a??b a为空则向右合并 否则a
                    property.SetValue(obj, redaer[property.GetColumnMappingName()] ?? DBNull.Value);
                }
                return (T)obj;
            }
            catch(Exception ex)
            {
                throw;
            }
        }
    }
}
