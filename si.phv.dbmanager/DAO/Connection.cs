using si.phv.dbmanager.DbService;
using System;
using System.Data;

namespace si.phv.dbmanager.DAO
{
    public class Connection : IDisposable
    {
        private static IDbConnection OracleConnection;
        private static IDbConnection MySqlConnection;
        private static string _conn;

        public static IDbConnection GetConnection(string conn)
        {

            //poiščemo katero konekcijo naj uporabimo
            try
            {

                MySqlDbService mySqlDb = new();
                OracleDbService oracleDb = new();

                if (mySqlDb.CanHandle(conn))
                {
                    if (MySqlConnection == null)
                    {
                        MySqlConnection = mySqlDb.GetConnection();
                    }
                    else if (!_conn.Equals(conn))//check if we have the same database
                    {
                        Close();
                        MySqlConnection = mySqlDb.GetConnection();
                    }

                    if (MySqlConnection.State != ConnectionState.Open)
                        MySqlConnection.Open();

                    _conn = conn;

                    return MySqlConnection;
                }
                else if (oracleDb.CanHandle(conn))
                {
                    if (OracleConnection == null) { 
                        OracleConnection = OracleDbService.GetConnection();
                    }
                    else if (!_conn.Equals(conn))//check if we have the same database
                    {
                        Close();
                        OracleConnection = OracleDbService.GetConnection();
                    }

                    if (OracleConnection.State != ConnectionState.Open)
                        OracleConnection.Open();

                    _conn = conn;

                    return OracleConnection;
                }
                

                return null;
                
            }
            catch (Exception)
            {
                return null;
            }

        }
        public void Dispose()
        {
            Close();
        }
        private static void Close()
        {
            try
            {
                if (OracleConnection != null)
                {
                    if (OracleConnection.State != ConnectionState.Executing && OracleConnection.State != ConnectionState.Fetching)
                        OracleConnection.Close();
                }

                if (MySqlConnection != null)
                {
                    if (MySqlConnection.State != ConnectionState.Executing && MySqlConnection.State != ConnectionState.Fetching)
                        MySqlConnection.Close();
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
