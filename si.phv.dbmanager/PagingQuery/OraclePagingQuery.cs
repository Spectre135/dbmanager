#region using
using Oracle.ManagedDataAccess.Client;
using si.phv.dbmanager.DAO;
using si.phv.dbmanager.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
#endregion

namespace si.phv.dbmanager
{
    public class OraclePagingQuery : IPagingQuery
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

            StringBuilder tmpSQL = new StringBuilder();

            tmpSQL.AppendFormat("WITH SQL as ({0}) ", query);
            tmpSQL.AppendLine(" SELECT * FROM (SELECT a.*, ROWNUM AS rnum");
            tmpSQL.AppendLine(" FROM (SELECT  sql.*,MAX (ROWNUM) OVER (ORDER BY NULL) COUNT FROM sql ");
            tmpSQL.AppendFormat(" ORDER BY {0} {1} NULLS LAST) a", (string.IsNullOrEmpty(order) ? "NULL" : order), (desc ? "DESC" : "ASC"));
            tmpSQL.AppendFormat(" WHERE ROWNUM < {0})  WHERE rnum > {1} ", to, from);

            return tmpSQL.ToString();
        }
        protected static string GetSearchFields<T>() where T : class, new()
        {

            Type objType = typeof(T);

            StringBuilder sb = new StringBuilder();

            foreach (PropertyInfo p in objType.GetProperties())
            {
                foreach (DataSearchAttribute a in p.GetCustomAttributes(typeof(DataSearchAttribute), false))
                {
                    if (a.ColumnSearch != null)
                        sb.Append(String.Format("{0} ||", a.ColumnSearch));
                }

            }
            //remove last char wich is +
            //string response = sb.ToString().Substring(0, sb.Length - 1);
            string response = sb.ToString().Substring(0, sb.Length - 2);
            response = " LOWER(" + response + ") ";

            return response;

        }
        public IDbCommand PrepareQuery<T>(IDbConnection connection, List<KeyValuePair<string, object>> alParmValues, string query,string search, int from, int to, string order, bool asc) where T : class, new()
        {

            OracleCommand cmd = new OracleCommand();
            OracleConnection conn = new OracleConnection();
            string sql = string.Empty;

            try
            {

                StringBuilder sqlFilter = new StringBuilder();

                //Search string
                if (!String.IsNullOrEmpty(search) && !search.ToLower().Equals("undefined"))
                {
                    sqlFilter.Append(" and ").Append(GetSearchFields<T>()).Append(" like :searchParam ");
                    alParmValues.Add(DbUtil.SetParam("searchParam", "%" + search.ToLower() + "%"));
                }


                sql = GetPagingQuery(new StringBuilder(query).Append(sqlFilter).ToString(), from, to, order, asc);

                conn = (OracleConnection)connection;
                if (conn.State != ConnectionState.Open)
                    conn.Open();
                cmd.Connection = conn;

                cmd.CommandText = sql;
                cmd.BindByName = true;
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
