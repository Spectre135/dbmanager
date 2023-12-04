using Oracle.ManagedDataAccess.Client;
using si.phv.dbmanager.Util;
using System;
using System.Collections.Generic;
using System.Data;

namespace si.phv.dbmanager
{
    class OraCommand : ICommand
    {
        public IDbCommand CreateCommand(IDbConnection connection, List<KeyValuePair<string, object>> alParmValues, string query)
        {
            OracleCommand command = new OracleCommand();
            OracleConnection _connection = new OracleConnection();

            try
            {

                _connection = (OracleConnection)connection;
                command.Connection = _connection;
                if (_connection.State != ConnectionState.Open)
                    _connection.Open();
                command.CommandText = query;
                command.BindByName = true;
                command.CommandType = CommandType.Text;
                command = DbUtil.BindParameters(command, alParmValues);

            }
            catch (Exception ex)
            {
                _connection.Close();
                throw new Exception("Napaka pri CommandQuery", ex);
            }

            return command;
        }
    }
}
