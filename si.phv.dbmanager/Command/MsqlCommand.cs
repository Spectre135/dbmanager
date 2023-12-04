using MySql.Data.MySqlClient;
using si.phv.dbmanager.Util;
using System;
using System.Collections.Generic;
using System.Data;

namespace si.phv.dbmanager
{
    class MsqlCommand : ICommand
    {
        public IDbCommand CreateCommand(IDbConnection connection, List<KeyValuePair<string, object>> alParmValues, string sql)
        {
                MySqlCommand command = new MySqlCommand();
                MySqlConnection _connection = new MySqlConnection();

                try
                {

                    _connection = (MySqlConnection)connection;
                    command.Connection = _connection;
                    if (_connection.State != ConnectionState.Open)
                        _connection.Open();
                    command.CommandText = sql;
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
