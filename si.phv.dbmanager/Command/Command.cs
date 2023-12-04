using si.phv.dbmanager.Util;
using System.Collections.Generic;
using System.Data;

namespace si.phv.dbmanager
{
    public class Command : ICommand
    {
        public IDbCommand CreateCommand(IDbConnection connection, List<KeyValuePair<string, object>> alParmValues, string sql)
        {  
            ICommand command = InitClass.GetCommandClass<ICommand>(connection);
            return command.CreateCommand(connection, alParmValues, sql);
        }
    }
}
