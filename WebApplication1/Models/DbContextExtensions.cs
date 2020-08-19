using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;

namespace WebApplication1.Models
{
    public static class DbContextExtensions
    {
        private static void CombineParams(ref DbCommand command, DatabaseFacade facade, params object[] parameters)
        {
            if (parameters != null)
            {
                if (facade.IsOracle())
                {
                    foreach (OracleParameter parameter in parameters)
                    {
                        if (!parameter.ParameterName.Contains(":"))
                            parameter.ParameterName = $":{parameter.ParameterName}";
                        command.Parameters.Add(parameter);
                    }
                }
            }
        }
        private static DbCommand CreateCommand(DatabaseFacade facade, string sql, out DbConnection dbConn, params object[] parameters)
        {
            var conn = facade.GetDbConnection();
            dbConn = conn;
            conn.Open();
            var cmd = conn.CreateCommand();
            if (facade.IsOracle())
            {
                cmd.CommandText = sql;
                CombineParams(ref cmd, facade, parameters);
            }
            return cmd;
        }

        public static DataTable SqlQuery(this DatabaseFacade facade, string sql, params object[] parameters)
        {
            var cmd = CreateCommand(facade, sql, out DbConnection conn, parameters);
            var reader = cmd.ExecuteReader();
            var dt = new DataTable();
            dt.Load(reader);
            reader.Close();
            conn.Close();
            return dt;
        }

        public static IEnumerable<T> SqlQuery<T>(this DatabaseFacade facade, string sql, params object[] parameters) where T : class, new()
        {
            var dt = SqlQuery(facade, sql, parameters);
            return dt.ToEnumerable<T>();
        }

        public static IEnumerable<T> ToEnumerable<T>(this DataTable dt) where T : class, new()
        {
            PropertyInfo[] propertyInfos = typeof(T).GetProperties();
            T[] ts = new T[dt.Rows.Count];
            int i = 0;
            foreach (DataRow row in dt.Rows)
            {
                T t = new T();
                foreach (PropertyInfo p in propertyInfos)
                {
                    if (dt.Columns.IndexOf(p.Name) != -1 && row[p.Name] != DBNull.Value)
                        p.SetValue(t, row[p.Name], null);
                }
                ts[i] = t;
                i++;
            }
            return ts;
        }
    }
}
