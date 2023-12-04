#region using
using MySql.Data.MySqlClient;
using si.phv.dbmanager.DAO;
using si.phv.dbmanager.Util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Text;
#endregion

namespace si.phv.dbmanager
{
    public class MySqlPagingQuery : IPagingQuery
    {
        private string GetPagingQuery(string query, int from, int to, string order, bool asc)
        {
            bool desc = false;
            if (!string.IsNullOrEmpty(order))
            {
                desc = order.StartsWith("-");
                if (desc)
                    order = order.Substring(1);
            }

            //če imamo order parameter brez - predznaka
            if (asc)
                desc = false;

            return string.Format(@"SELECT * FROM (
                                    SELECT x.*,(SELECT COUNT(*) total_rows FROM ({0}) y ) COUNT
                                    FROM (SELECT a.*, ROW_NUMBER() OVER(PARTITION BY NULL ORDER BY {1} {2}) AS RowNum FROM ({3}) a) x
                                ) t
                    WHERE
                    RowNum > {4} AND RowNum <= {5}", query, (string.IsNullOrEmpty(order) ? "NULL" : order ), (desc ? "DESC" : ""), query, from, to);

        }
        protected static string GetSearchFields<T>() where T : class, new()
        {

            Type objType = typeof(T);

            StringBuilder sb = new();

            foreach (PropertyInfo p in objType.GetProperties())
            {
                foreach (DataSearchAttribute a in p.GetCustomAttributes(typeof(DataSearchAttribute), false))
                {
                    if (a.ColumnSearch != null)
                        sb.Append(String.Format("coalesce({0},''),", a.ColumnSearch));
                }

            }
            //remove last char wich is +
            string response = sb.ToString().Substring(0, sb.Length - 1);
            response = " LOWER(CONCAT(" + response + "))";

            return response;

        }
        public IDbCommand PrepareQuery<T>(IDbConnection connection, List<KeyValuePair<string, object>> alParmValues, string query, string search,  int from, int to, string order, bool asc) where T : class, new()
        {

            MySqlCommand cmd = new();
            MySqlConnection conn = new();
            string sql = string.Empty;

            try
            {
                StringBuilder sqlFilter = new();

                //Search string
                if (!String.IsNullOrEmpty(search) && !search.ToLower().Equals("undefined"))
                {
                    sqlFilter.Append(" and ").Append(GetSearchFields<T>()).Append(" like @searchParam ");
                    alParmValues.Add(DbUtil.SetParam("searchParam", "%" + search.ToLower() + "%"));
                }


                sql = GetPagingQuery(new StringBuilder(query).Append(sqlFilter).ToString(), from, to, order, asc);

                conn = (MySqlConnection)connection;
                if (conn.State != ConnectionState.Open)
                    conn.Open();
                cmd.Connection = conn;

                cmd.CommandText = sql;
                cmd.CommandType = CommandType.Text;
                cmd = DbUtil.BindParameters(cmd, alParmValues);
            }
            catch (Exception ex)
            {
                conn.Close();
                throw new Exception("Napaka pri PrepareQuery/SQL:" + sql, ex);
            }

            return cmd;
        }
    }
}
