#region using
using si.phv.dbmanager.model;
using si.phv.dbmanager.util;
using si.phv.dbmanager.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
#endregion

namespace si.phv.dbmanager.DAO
{
    public abstract class AbstractDAO : Connection
    {

        #region LoadObject,Parameters Values
        protected static TEntity LoadObject<TEntity>(IDataReader dr, PropertyInfo[] propertyInfos) where TEntity : class, new()
        {
            TEntity instanceToPopulate = new();

            try
            {
                //for each public property on the original
                foreach (PropertyInfo pi in propertyInfos)
                {

                    //this attribute is marked with AllowMultiple=false
                    if (pi.GetCustomAttributes(typeof(DataFieldAttribute), false) is DataFieldAttribute[] datafieldAttributeArray && datafieldAttributeArray.Length == 1)
                    {
                        DataFieldAttribute dfa = datafieldAttributeArray[0];

                        try
                        {
                            object dbValue = dr[dfa.ColumnName];

                            if (dbValue != null &&
                                !String.IsNullOrEmpty(dbValue.ToString()) &&
                                !String.IsNullOrWhiteSpace(dbValue.ToString()))
                            {

                                Type t = Nullable.GetUnderlyingType(pi.PropertyType) ?? pi.PropertyType;
                                object safeValue = (dbValue == null) ? null : Convert.ChangeType(dbValue, t);

                                pi.SetValue(instanceToPopulate, safeValue, null);
                            }
                        }
                        catch (Exception)
                        {
                            //do nothing --column not found
                        }
                    }
                }
            }
            catch (Exception)
            {
                //do nothing
            }

            return instanceToPopulate;

        }
        protected static List<KeyValuePair<string, object>> LoadParametersValue<T>(object values) where T : class, new()
        {
            List<KeyValuePair<string, object>> alParmValues = new();
            PropertyInfo[] propertyInfos = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            T src = (T)values;

            try
            {
                //for each public property on the original
                foreach (PropertyInfo pi in propertyInfos)
                {

                    //this attribute is marked with AllowMultiple=false
                    if (pi.GetCustomAttributes(typeof(ParamFieldAttribute), false) is ParamFieldAttribute[] fieldAttributeArray && fieldAttributeArray.Length == 1)
                    {
                        ParamFieldAttribute dfa = fieldAttributeArray[0];

                        try
                        {
                            object value = src.GetType().GetProperty(dfa.ColumnName).GetValue(src, null);
                            //check if we have null value for SQL parameter
                            if (src.GetType().GetProperty(dfa.ColumnName).PropertyType == typeof(string) && value == null)
                                value = string.Empty;

                            alParmValues.Add(new KeyValuePair<string, object>(String.Format("@{0}", dfa.ColumnName), value));

                        }
                        catch (Exception)
                        {
                            //do nothing --value not found
                        }
                    }
                }
            }
            catch (Exception)
            {
                //do nothing
            }

            return alParmValues;

        }
        #endregion

        #region GetData,SaveData
        protected static MResponse GetData<T>(string connectionString, string sql, List<KeyValuePair<string, object>> alParmValues = null) where T : class, new()
        {
            MResponse mResponse = new();
            List<T> list = new();
            
            try
            {
                PropertyInfo[] propertyInfos = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                IDbConnection connection = GetConnection(connectionString);
                Command cmd = new();
                using IDbCommand command = cmd.CreateCommand(connection, alParmValues, sql);
                using IDataReader reader = command.ExecuteReader();

                while (reader.Read())
                    list.Add(LoadObject<T>(reader, propertyInfos));

                mResponse.DataList = list;
                mResponse.RowsCount = list.Count;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }

            return mResponse;
        }
        protected static MResponse GetPagingDataWithProcedure<T>(string connectionString, string sql, List<KeyValuePair<string, object>> alParmValues = null) where T : class, new()
        {
            MResponse mResponse = new();
            List<T> list = new();

            try
            {
                PropertyInfo[] propertyInfos = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                IDbConnection connection = GetConnection(connectionString);
                Command cmd = new();
                using IDbCommand command = cmd.CreateCommand(connection, alParmValues, sql);
                using IDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    list.Add(LoadObject<T>(reader, propertyInfos));
                    if (list.Count == 1)
                        mResponse.RowsCount = PagingQuery.GetPageCount(reader);
                }

                mResponse.DataList = list;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }

            return mResponse;
        }
        protected static MResponse GetPagingData<T>(string connectionString, string sql, string search, int from, int to, string orderby,
            List<KeyValuePair<string, object>> alParmValues = null) where T : class, new()
        {
            MResponse mResponse = new();
            List<T> list = new();

            try
            {
                PropertyInfo[] propertyInfos = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

                alParmValues ??= new List<KeyValuePair<string, object>>();

                IDbConnection connection = GetConnection(connectionString);

                PagingQuery pq = new(connection, sql,search, alParmValues, orderby, false);
                pq.PrepareQuery<T>(from, to);

                using IDataReader reader = pq.GetReader();
                while (reader.Read())
                {
                    list.Add(LoadObject<T>(reader, propertyInfos));
                    if (list.Count == 1)
                        mResponse.RowsCount = PagingQuery.GetPageCount(reader);
                }

                mResponse.DataList = list;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }

            return mResponse;
        }
        protected static void SaveData(string connectionString, string sql, List<KeyValuePair<string, object>> alParmValues)
        {
            IDbTransaction transaction = null;
            try
            {

                IDbConnection connection = GetConnection(connectionString);
                Command cmd = new();
                transaction = connection.BeginTransaction();
                IDbCommand command = cmd.CreateCommand(connection, alParmValues, sql);
                command.ExecuteNonQuery();

                transaction.Commit();

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception(ex.Message, ex.InnerException);
            }
        }
        protected static object SaveDataWithResponse(string connectionString, string sql, List<KeyValuePair<string, object>> alParmValues)
        {
            IDbTransaction transaction = null;
            try
            {

                IDbConnection connection = GetConnection(connectionString);
                Command cmd = new();
                transaction = connection.BeginTransaction();
                IDbCommand command = cmd.CreateCommand(connection, alParmValues, sql);
                var response = command.ExecuteScalar();

                transaction.Commit();

                return response;

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw new Exception(ex.Message, ex.InnerException);
            }
        }
        #endregion

        #region Date from to
        public static string GetDateFromToParam(string sql, string field, string fromDate, string toDate, List<KeyValuePair<string, object>> alParmValues, out List<KeyValuePair<string, object>> _alParmValues, bool oracle=false)
        {
            bool reverse = AppUtils.IsGreaterThan(fromDate, toDate);

            if (!String.IsNullOrEmpty(fromDate))
            {
                alParmValues.Add(DbUtil.SetParam(oracle ? ":DateFrom" : "@DateFrom", reverse ? AppUtils.ParseDatum(toDate) : AppUtils.ParseDatum(fromDate)));
                sql += oracle ? String.Format(" AND TRUNC({0}) >= TRUNC(:DateFrom) ", field): String.Format(" AND DATE({0}) >= DATE(@DateFrom) ", field);
            }

            if (!String.IsNullOrEmpty(toDate))
            {
                alParmValues.Add(DbUtil.SetParam(oracle ? ":DateTo" : "@DateTo", reverse ? AppUtils.ParseDatum(fromDate) : AppUtils.ParseDatum(toDate)));
                sql += oracle ? String.Format(" AND TRUNC({0}) <= TRUNC(:DateTo) ", field):String.Format(" AND DATE({0}) <= DATE(@DateTo) ", field);
            }

            _alParmValues = alParmValues;

            return sql;
        }
        public static string GetDateFromToParamWithReplaceSQL(string sql, string field, string fromDate, string toDate, List<KeyValuePair<string, object>> alParmValues, out List<KeyValuePair<string, object>> _alParmValues)
        {

            bool reverse = AppUtils.IsGreaterThan(fromDate, toDate);

            if (!String.IsNullOrEmpty(fromDate))
                alParmValues.Add(DbUtil.SetParam("@DateFrom", reverse ? AppUtils.ParseDatum(toDate) : AppUtils.ParseDatum(fromDate)));

            if (!String.IsNullOrEmpty(toDate))
                alParmValues.Add(DbUtil.SetParam("@DateTo", reverse ? AppUtils.ParseDatum(fromDate) : AppUtils.ParseDatum(toDate)));

            string _sql = String.Format(sql,
                String.IsNullOrEmpty(fromDate) ? String.Empty : String.Format(" AND DATE({0}) >= DATE(@DateFrom) ", field),
                String.IsNullOrEmpty(toDate) ? String.Empty : String.Format(" AND DATE({0}) <= DATE(@DateTo) ", field));

            _alParmValues = alParmValues;

            return _sql;
        }
        #endregion

        #region Sequences
        protected static string GetTableName<T>() where T : class, new()
        {
            Type objType = typeof(T);
            foreach (PropertyInfo p in objType.GetProperties())
            {
                foreach (TableNameAttribute a in p.GetCustomAttributes(typeof(TableNameAttribute), false).Cast<TableNameAttribute>())
                {
                    if (a.TableName != null)
                        return a.TableName;
                }
            }

            return null;
        }
        protected static int GetSeqNextValue<T>(string connectionString, string sql) where T : class, new()
        {
            int seqval = 0;
            List<KeyValuePair<string, object>> alParmValues = new()
            {
                new KeyValuePair<string, object>("@TableName", GetTableName<T>())
            };
            IDbConnection connection = GetConnection(connectionString);
            Command cmd = new();
            using IDbCommand command = cmd.CreateCommand(connection, alParmValues, sql);
            using IDataReader reader = command.ExecuteReader();

            if (reader.Read())
                seqval = reader.GetInt32(0);

            return seqval++;

        }
        #endregion
    }
}
