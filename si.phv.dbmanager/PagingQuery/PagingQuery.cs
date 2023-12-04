using si.phv.dbmanager.Util;
using System;
using System.Collections.Generic;
using System.Data;

namespace si.phv.dbmanager
{
    public class PagingQuery
    {
        //-----------------------------------------------------------------------------------------
        private readonly string query = null;
        private readonly string search = null;
        private readonly string order = null;
        private readonly List<KeyValuePair<string, object>> alParmValues = new List<KeyValuePair<string, object>>();
        private int from = 0;
        private int to = 0;
        private readonly bool asc = false;
        private readonly IDbConnection connection;
        private IDbCommand command;
        private readonly string sql = string.Empty;
        //-----------------------------------------------------------------------------------------

        /**
          * Konstruktor objekta PagingQuery
          * @param conn konekcija na bazo
          * @param query SQL stavek
          * @param order del SQL stavka, ki vsebuje sintakso za sortiranje podatkov
          * @param alParmValues parametri, ki nadomestijo ? v SQL stavku (v ArrayListu se morajo pojaviti v
          *                     enakem vrstnem redu, kot si sledijo v SQL stavku)
          * @throws AppException  v primeru napake pri aktiviranju konstruktorja
          */
        public PagingQuery(IDbConnection connection, string query,string search, List<KeyValuePair<string, object>> alParmValues, string order, bool asc)
        {
            this.query = query;
            this.search = search;
            this.order = order;
            this.asc = asc;
            this.alParmValues = alParmValues;
            this.connection = connection;
        }
        /**
           * Izveda poizvedbe na bazi
           * @param from zaporedna številka vrstice zapisa v naboru, ki bo rezultat poizvedbe (OD)
           * @param to   zaporedna številka vrstice zapisa v naboru, ki bo rezultat poizvedbe (DO)
           * @throws AppException v primeru napake pri izvedbi poizvedbe na bazi
        */
        public void PrepareQuery<T>(int from, int to) where T : class, new()
        {
            this.from = from;
            this.to = to;
            IPagingQuery pagingQuery = InitClass.GetPagingClass<IPagingQuery>(connection);
            command = pagingQuery.PrepareQuery<T>(connection, alParmValues, query, search, from, to, order, asc);
        }
        /** 
         * Vrnemo število vseh zapisov, ki jih SQL prebere
        */
        public static long GetPageCount(IDataReader reader)
        {
            try
            {
                string count = reader["COUNT"].ToString();

                return long.Parse(count);
            }
            catch (Exception)
            {
                return 0;
            }
        }
        /** 
         * Vračanje Oracle Data Reader v IDataReader obliki
         * V metodi naredimo commit transakcije in zapremo query
        */
        public IDataReader GetReader()
        {
            try
            {
                IDataReader reader = command.ExecuteReader();

                return reader;
            }
            catch (Exception ex)
            {
                throw new Exception("Napaka pri GetReader", ex);
            }

        }
    }
}
