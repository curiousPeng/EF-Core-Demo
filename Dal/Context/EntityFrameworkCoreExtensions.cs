using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Dal.Context
{
    /// <summary>
    /// 执行sql语句，不够扩展自己再扩
    /// </summary>
    public static class EntityFrameworkCoreExtensions
    {
        private static DbCommand CreateCommand(DatabaseFacade facade, out DbConnection connection, DynamicSqlQuery sql)
        {
            var conn = facade.GetDbConnection();
            connection = conn;
            conn.Open();
            var cmd = conn.CreateCommand();

            cmd.CommandText = sql.Sql;
            cmd.Parameters.AddRange(sql.Parameters);

            return cmd;
        }

        private static int ExecuteNonQuery(this DatabaseFacade facade, DynamicSqlQuery sql)
        {
            var command = CreateCommand(facade, out DbConnection conn, sql);
            var result = command.ExecuteNonQuery();
            conn.Close();
            return result;
        }

        private static DbDataReader ExecuteReader(this DatabaseFacade facade, DynamicSqlQuery sql)
        {
            var command = CreateCommand(facade, out DbConnection conn, sql);
            return command.ExecuteReader();
        }

        public static List<string> QueryStringForList(this DatabaseFacade facade, DynamicSqlQuery sql)
        {
            var reader = ExecuteReader(facade, sql);
            var result = new List<string>();
            while (reader.Read())
            {
                result.Add(Convert.ToString(reader.GetString(0)));
            }
            reader.Close();
            facade.CloseConnection();
            return result;
        }
        public static T SqlQueryScalar<T>(this DatabaseFacade facade, DynamicSqlQuery sql)
        {
            var reader = ExecuteReader(facade, sql);
            reader.Close();
            facade.CloseConnection();
            return reader.GetFieldValue<T>(0);
        }
        public static T SqlQueryScalar<T>(this DatabaseFacade facade, string sql)
        {
            var reader = ExecuteReader(facade, DynamicSqlQuery.Create(sql));
            reader.Close();
            facade.CloseConnection();
            return reader.GetFieldValue<T>(0);
        }
        public static List<T> SqlQuery<T>(this DatabaseFacade facade, DynamicSqlQuery sql)
        {
            var reader = ExecuteReader(facade, sql);
            var result = new List<T>();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var t = reader.GetFieldValue<T>(i);
                result.Add(t);
            }
            reader.Close();
            facade.CloseConnection();
            return result;
        }

        public static List<T> SqlQuery<T>(this DatabaseFacade facade, string sql)
        {
            var reader = ExecuteReader(facade, DynamicSqlQuery.Create(sql));
            var result = new List<T>();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var t = reader.GetFieldValue<T>(i);
                result.Add(t);
            }
            reader.Close();
            facade.CloseConnection();
            return result;
        }


        public static List<T> SqlQueryStructForList<T>(this DatabaseFacade facade, DynamicSqlQuery sql) where T : struct
        {
            var reader = ExecuteReader(facade, sql);
            List<T> result = new List<T>();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var t = reader.GetFieldValue<T>(i);
                result.Add(t);
            }
            reader.Close();
            facade.CloseConnection();
            return result;
        }

        public static List<T> SqlQueryStructForList<T>(this DatabaseFacade facade, string sql) where T : struct
        {
            var reader = ExecuteReader(facade, DynamicSqlQuery.Create(sql));
            List<T> result = new List<T>();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                var t = reader.GetFieldValue<T>(i);
                result.Add(t);
            }
            reader.Close();
            facade.CloseConnection();
            return result;
        }

        /// <summary>
        /// 执行sql返回受影响的行数
        /// </summary>
        /// <param name="facade"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static int ExecuteSql(this DatabaseFacade facade, DynamicSqlQuery sql)
        {
            return ExecuteNonQuery(facade, sql);
        }

        public static decimal SqlQueryDecimalSingle(this DatabaseFacade facade, DynamicSqlQuery sql)
        {
            var reader = ExecuteReader(facade, sql);
            var result = reader.GetDecimal(0);
            reader.Close();
            facade.CloseConnection();
            return result;
        }

        public static decimal SqlQueryDecimalSingle(this DatabaseFacade facade, string sql)
        {
            var reader = ExecuteReader(facade, DynamicSqlQuery.Create(sql));
            var result = reader.GetDecimal(0);
            reader.Close();
            facade.CloseConnection();
            return result;
        }

        public static string SqlQueryStringSingle(this DatabaseFacade facade, DynamicSqlQuery sql)
        {
            var reader = ExecuteReader(facade, sql);
            var result = reader.GetString(0);
            reader.Close();
            facade.CloseConnection();
            return result;
        }

        public static string SqlQueryStringSingle(this DatabaseFacade facade, string sql)
        {
            var reader = ExecuteReader(facade, DynamicSqlQuery.Create(sql));
            var result = reader.GetString(0);
            reader.Close();
            facade.CloseConnection();
            return result;
        }

        public static int SqlQueryIntSingle(this DatabaseFacade facade, DynamicSqlQuery sql)
        {
            var reader = ExecuteReader(facade, sql);
            var result = reader.GetInt32(0);
            reader.Close();
            facade.CloseConnection();
            return result;
        }
    }
}
