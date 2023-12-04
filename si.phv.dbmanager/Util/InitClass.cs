#region using
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
#endregion

namespace si.phv.dbmanager.Util
{
    class InitClass
    {
        private static OracleConnection oOracleCon;
        private static MySqlConnection oMySqlCon;

        public static T GetPagingClass<T>(IDbConnection connection)
        {
            oMySqlCon = connection as MySqlConnection;
            oOracleCon = connection as OracleConnection;

            if (oMySqlCon != null)
                return (T)Activator.CreateInstance(Type.GetType("si.phv.dbmanager.MySqlPagingQuery"));
            else if (oOracleCon != null)
                return (T)Activator.CreateInstance(Type.GetType("si.phv.dbmanager.OraclePagingQuery"));

            throw new Exception("Class is not initialized.");
        }
        public static T GetCommandClass<T>(IDbConnection connection)
        {

            oOracleCon = connection as OracleConnection;
            oMySqlCon = connection as MySqlConnection;


            if (oMySqlCon != null)
                return (T)Activator.CreateInstance(Type.GetType("si.phv.dbmanager.MsqlCommand"));
            else if (oOracleCon != null)
                return (T)Activator.CreateInstance(Type.GetType("si.phv.dbmanager.OraCommand"));

            throw new Exception("Class is not initialized.");
        }

    }
}
