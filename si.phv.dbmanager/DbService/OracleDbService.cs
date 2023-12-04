using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;

namespace si.phv.dbmanager.DbService
{
    class OracleDbService
    {
        private static OracleConnection connection = null;

        public static IDbConnection GetConnection()
        {
            try
            {
                if (connection == null)
                    throw new Exception("Connection string je prazen ! Pravilna uporaba knjižnice GetConnection().Connect()");

            }
            catch (Exception ex)
            {
                throw new Exception("Napaka pri povezovanju na bazo:", ex);
            }

            return connection;
        }
        public void SetConnection(string connection_string)
        {

            OracleConnectionStringBuilder connStringBuilder = new OracleConnectionStringBuilder(connection_string);

            connection = new OracleConnection
            {
                ConnectionString = connStringBuilder.ConnectionString
            };

        }
        public bool CanHandle(string connection_string)
        {
            bool response = false;

            try
            {
                this.SetConnection(connection_string);
                GetConnection();

                response = true;
            }
            catch (Exception)
            {

            }

            return response;


        }
    }
}
