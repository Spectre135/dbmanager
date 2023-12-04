#region using
using MySql.Data.MySqlClient;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
#endregion

namespace si.phv.dbmanager.Util
{
    public class DbUtil
    {
        private static readonly int oracleReturnValueDefaultSize = 250; //velikost return value(varchar2)  ko kličemo Oracle function

        #region Set Paramteres
        public static KeyValuePair<string, object> SetParam(string name, object value)
        {
            return new KeyValuePair<string, object>(name, value);
        }
        public static KeyValuePair<object, ParameterDirection> SetParamIn(object value)
        {
            return new KeyValuePair<object, ParameterDirection>(value, ParameterDirection.Input);
        }
        public static KeyValuePair<object, ParameterDirection> SetParamOut(object value)
        {
            return new KeyValuePair<object, ParameterDirection>(value, ParameterDirection.Output);
        }
        public static KeyValuePair<object, ParameterDirection> SetReturnValue()
        {
            return new KeyValuePair<object, ParameterDirection>(null, ParameterDirection.ReturnValue);
        }
        public static KeyValuePair<object, ParameterDirection> SetCursorOut()
        {
            return new KeyValuePair<object, ParameterDirection>("ref_cursor", ParameterDirection.Output);
        }
        public static KeyValuePair<object, ParameterDirection> SetCursorReturnValue()
        {
            return new KeyValuePair<object, ParameterDirection>("ref_cursor_return_value", ParameterDirection.ReturnValue);
        }
        private static Dictionary<string, object[]> GetParamsValue(List<KeyValuePair<string, object>> parms)
        {

            Dictionary<string, object[]> paramsValue = new Dictionary<string, object[]>();

            string key = String.Empty;
            List<object> values = new List<object>();
            int i = 0;

            //sortiramo po ključu 
            parms = parms.OrderBy(o => o.Key).ToList();

            foreach (KeyValuePair<string, object> p in parms)
            {
                key = i++ == 0 ? p.Key : key;

                if (key.Equals(p.Key))
                {
                    key = p.Key;
                    values.Add(p.Value);
                }
                else
                {
                    paramsValue.Add(key, values.ToArray());
                    //next param
                    values = new List<object>();
                    key = p.Key;
                    values.Add(p.Value);
                }

            }

            //last 
            paramsValue.Add(key, values.ToArray());

            return paramsValue;

        }
        #endregion

        #region Oracle stuff
        public static OracleCommand BindParameters(OracleCommand oracleCommand, List<KeyValuePair<string, object>> alParmValues)
        {
            try
            {
                StringBuilder str = new StringBuilder();
                oracleCommand.Parameters.Clear();

                if (alParmValues != null)
                {
                    foreach (var item in alParmValues)
                    {
                        /*bool object parsamo v 01 ker Oracle nima bool param*/
                        //pogledamo če imamo byte da damo BLOB param
                        if (item.Value is byte[])
                            oracleCommand.Parameters.Add(item.Key, OracleDbType.Blob, item.Value, ParameterDirection.Input);
                        else
                            oracleCommand.Parameters.Add(item.Key, item.Value is bool ? (bool.Parse(item.Value.ToString()) ? 1 : 0) : item.Value);

                        str.Append(string.Format("{0}:{1}", item.Key, item.Value));
                    }
                }

                //Logger.INFO(MethodBase.GetCurrentMethod(), "Dejanski izvedeni select:" + sql + "\r\n" + str.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception("Napaka pri dodajanju paramterov", ex);
            }

            return oracleCommand;

        }
        public static OracleCommand BindParameters(OracleCommand oracleCommand, Dictionary<string, KeyValuePair<object, ParameterDirection>> alParmValues)
        {
            try
            {
                StringBuilder str = new StringBuilder();

                oracleCommand.Parameters.Clear();

                foreach (var item in alParmValues)
                {
                    string refCursor = item.Value.Key == null ? " " : item.Value.Key.ToString();

                    if (refCursor.Equals("ref_cursor"))
                        oracleCommand.Parameters.Add(item.Key, OracleDbType.RefCursor, ParameterDirection.Output);
                    else if (refCursor.Equals("ref_cursor_return_value"))
                        oracleCommand.Parameters.Add(item.Key, OracleDbType.RefCursor, ParameterDirection.ReturnValue);
                    else if (item.Value.Value == ParameterDirection.ReturnValue)
                        oracleCommand.Parameters.Add(item.Key, OracleDbType.Varchar2, oracleReturnValueDefaultSize, item.Key, ParameterDirection.ReturnValue);
                    else if (item.Value.Value == ParameterDirection.Output && item.Value.Key is string)
                        oracleCommand.Parameters.Add(item.Key, OracleDbType.Varchar2, oracleReturnValueDefaultSize, item.Key, ParameterDirection.Output);
                    else if (item.Value.Key is byte[])
                        oracleCommand.Parameters.Add(item.Key, OracleDbType.Blob, item.Value.Key, item.Value.Value).Direction = item.Value.Value;
                    else
                        oracleCommand.Parameters.Add(item.Key, item.Value.Key is bool ? (bool.Parse(item.Value.Key.ToString()) ? 1 : 0) : item.Value.Key).Direction = item.Value.Value;

                    str.Append(string.Format("{0}:{1}", item.Value.Key, item.Value.Value));
                }

                //Logger.INFO(MethodBase.GetCurrentMethod(), "Dejanski izvedeni select:" + sql + "\r\n" + str.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception("Napaka pri dodajanju paramterov", ex);
            }

            return oracleCommand;

        }
        public static OracleCommand BindParametersBatch(OracleCommand oracleCommand, List<KeyValuePair<string, object>> alParmValues)
        {
            try
            {
                StringBuilder str = new StringBuilder();
                oracleCommand.Parameters.Clear();

                Dictionary<string, object[]> parmValues = GetParamsValue(alParmValues);

                foreach (var item in parmValues)
                {
                    oracleCommand.Parameters.Add(item.Key, OracleDbType.Varchar2, item.Value.ToArray(), ParameterDirection.Input);
                    str.Append(string.Format("Parameter{0}:{1}", item.Key, item.Value.ToArray()));
                }

                //Logger.INFO(MethodBase.GetCurrentMethod(), "Dejanski izvedeni select:" + sql + "\r\n" + str.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception("Napaka pri dodajanju parametrov " + ex.Message, ex);
            }

            return oracleCommand;

        }
        private static bool IsOracleCommand(IDbCommand command)
        {
            try
            {
                OracleCommand comm = (OracleCommand)command;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region MySql stuff
        public static MySqlCommand BindParameters(MySqlCommand cmd, List<KeyValuePair<string, object>> alParmValues)
        {
            try
            {
                cmd.Parameters.Clear();
                if (alParmValues != null)
                {
                    foreach (var item in alParmValues)
                        cmd.Parameters.AddWithValue(item.Key, item.Value ?? DBNull.Value);
                }
                return cmd;
            }
            catch (Exception ex)
            {
                throw new Exception("Napaka pri dodajanju parametrov MySqlCommand", ex);
            }
        }
        #endregion

    }

}
