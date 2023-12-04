using System;
using System.Globalization;

namespace si.phv.dbmanager.util
{
    class AppUtils
    {
        public static string RemoveFirstChar(string charToRemove, string value)
        {
            try
            {
                if (value.StartsWith(charToRemove))
                    value = value.Remove(0, 1);
                return value;

            }
            catch (Exception)
            {
                return value;
            }
        }
        public static DateTime ParseDatum(string datum)
        {
            string format = "yyyy-MM-ddTHH:mm:ssZ";
            string format2 = "yyyy-MM-ddTHH:mm:sssZ";
            string format3 = "yyyy-MM-ddTHH:mm:ss";

            //preverimo ali imamo datum v obliki ticks-ov
            try
            {
                double ticks = double.Parse(datum);
                TimeSpan time = TimeSpan.FromMilliseconds(ticks);
                return new DateTime(1970, 1, 1) + time;
            }
            catch (Exception) { }

            if (datum.Contains("."))
                datum = datum.Substring(0, datum.LastIndexOf("."));

            try
            {
                return DateTime.ParseExact(datum, format, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                try
                {
                    return DateTime.ParseExact(datum, format2, CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    try
                    {
                        return DateTime.ParseExact(datum, format3, CultureInfo.InvariantCulture);
                    }
                    catch (Exception) { }
                }

                throw new Exception("Napaka pri parsanju datuma " + datum + "// zahtevan format = " + format, null);
            }
        }
        public static bool IsGreaterThan(string fromDate, string toDate)
        {
            try
            {
                if (!String.IsNullOrEmpty(fromDate) && !String.IsNullOrEmpty(toDate))
                {
                    DateTime d1 = ParseDatum(fromDate);
                    DateTime d2 = ParseDatum(toDate);
                    if (DateTime.Compare(d1, d2) > 0)
                        return true;

                }

                return false;

            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
