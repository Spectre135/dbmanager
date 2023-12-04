using System.Collections.Generic;
using System.Data;

namespace si.phv.dbmanager
{
    interface IPagingQuery
    {
        IDbCommand PrepareQuery<T>(IDbConnection connection, List<KeyValuePair<string, object>> alParmValues, string query, string serach, int from, int to, string order, bool asc) where T : class, new();
    }
}
