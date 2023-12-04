using System.Collections.Generic;
using System.Data;

namespace si.phv.dbmanager
{
    interface ICommand
    {
        IDbCommand CreateCommand(IDbConnection connection, List<KeyValuePair<string, object>> alParmValues, string sql);
    }
}
