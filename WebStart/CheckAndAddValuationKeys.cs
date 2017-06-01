using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Data.Sql;

namespace WebStart
{
    public class CheckAndAddValuationKeys
    {
        DataSet _ds = new DataSet();
        DataTable _dt = new DataTable();
        DataTable _result = new DataTable();
        String _connection;
        

        /// <summary>
        ///  
        /// </summary>
        public CheckAndAddValuationKeys(DataSet ds, String connection)
        {
            _ds = ds;
            _connection = connection;

            getRowsWithNoKeys();
        }

        /// <summary>
        ///  
        /// </summary>
        private void getRowsWithNoKeys()
        {
            _dt = _ds.Tables[0];

            //var rowsWithNoKeys = _dt.AsEnumerable().Where(x => x.Field<float?>("WFMTime#") == null);
            var rowsWithNoKeys = _dt.Select("[WFMTime#] IS NULL");

            _result = rowsWithNoKeys.CopyToDataTable<DataRow>();

        }

        /// <summary>
        ///  Per row with no key, ask KeyValuationActual_destination for the 1st line that has all the other data
        /// </summary>
        public void getMissingKeys()
        {
            SqlConnection conn = new SqlConnection(_connection);
            string sqlSearch = "SELECT TOP 1 "+
                        "[WFMTime#], [StateTime#], [ActualTime#], "+
                        "[WFMValuation#], [InvoiceValuation#], [ForecastTime#], "+
                        "[ForecastValuation#], [BillableValuationID] "+
                        "FROM KeyValuationActual_destination "+
                        "WHERE [SystemType] = @system "+
                        "AND [DataType] = @data "+
                        "AND [Location] LIKE @location "+
                        "AND [Department] LIKE @department "+
                        "AND [Role] LIKE @role "+
                        "AND [Language] LIKE @language "+
                        "AND [Shift] LIKE @shift "+
                        "AND [Resource Utilisation Ref_ID] = @resource "+
                        "AND [Activity] LIKE @activity "+
                        "AND [Unique ID] IS NOT NULL";

            try {
                foreach (DataRow dr in _result.Rows)
                {
                    conn.Open();

                    SqlDataAdapter da = new SqlDataAdapter(sqlSearch, conn);

                    da.SelectCommand.Parameters.AddWithValue("@location", dr[2]);
                    da.SelectCommand.Parameters.AddWithValue("@department", dr[3]);
                    da.SelectCommand.Parameters.AddWithValue("@role", dr[4]);
                    da.SelectCommand.Parameters.AddWithValue("@language", dr[5]);
                    da.SelectCommand.Parameters.AddWithValue("@shift", dr[7]);
                    da.SelectCommand.Parameters.AddWithValue("@resource", dr[8]);
                    da.SelectCommand.Parameters.AddWithValue("@activity", dr[9]);
                    da.SelectCommand.Parameters.AddWithValue("@data", dr[10]);
                    da.SelectCommand.Parameters.AddWithValue("@system", dr[11]);

                    DataTable dataTable = new DataTable();
                    da.Fill(dataTable);

                    if(dataTable != null)
                    {
                        foreach(DataRow keyRow in dataTable.Rows)
                        {
                            dr[13] = keyRow["WFMTime#"];
                            dr[14] = keyRow["StateTime#"];
                            dr[15] = keyRow["ActualTime#"];
                            dr[16] = keyRow["WFMValuation#"];
                            dr[17] = keyRow["InvoiceValuation#"];
                            dr[18] = keyRow["ForecastTime#"];
                            dr[19] = keyRow["ForecastValuation#"];
                            dr[20] = keyRow["BillableValuationID"];
                        }

                    }
                conn.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Querying keys exception information! : {0}", ex);
            }
            
        }

        /// <summary>
        ///  
        /// </summary>
        public DataTable getDataTable()
        {
            return _result;
        }
    }
}