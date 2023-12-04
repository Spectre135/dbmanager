using System.Collections.Generic;

namespace si.phv.dbmanager.model
{
    //Common model to use for data response from API
    public class MResponse
    {
        public IEnumerable<object> DataList { get; set; }
        public long RowsCount { get; set; }
    }
}
