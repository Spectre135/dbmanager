#region using
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;
#endregion

namespace si.phv.dbmanager.DbService
{
    class MySqlDbService
    {
        private static MySqlConnection connection = null;

        public IDbConnection GetConnection()
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

            MySqlConnectionStringBuilder connStringBuilder = new MySqlConnectionStringBuilder(connection_string);

            connection = new MySqlConnection
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
                this.GetConnection();

                response = true;
            }
            catch (Exception)
            {

            }

            return response;


        }
    }
}
