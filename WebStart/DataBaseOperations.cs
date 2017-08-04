using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using ClosedXML.Excel;
using OfficeOpenXml;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace WebStart
{
    public class DataBaseOperations
    {

        /// <summary>
        ///  Object constructor. Receives a DataSet and a String connection so that it can work on getting the missing keys for the new records
        /// </summary>
        public DataBaseOperations()
        {

        }

        public void TableBulkCopy(String SourceConection, String DestinationConnection, String SourceTableName, String DestinationTableName)
        {

            try
            {
                SqlConnection source = new SqlConnection(SourceConection);
                SqlConnection destination = new SqlConnection(DestinationConnection);

                source.Open();
                destination.Open();

                String sqlMainCommand = "SELECT * FROM " + SourceTableName;

                SqlCommand cmd = new SqlCommand(sqlMainCommand, source);

                cmd.ExecuteNonQuery();

                //Execute reader
                SqlDataReader reader = cmd.ExecuteReader();

                //Create BulkCopy
                SqlBulkCopy bulkData = new SqlBulkCopy(destination);

                //Set destination table
                bulkData.DestinationTableName = DestinationTableName;

                //Write Data
                bulkData.WriteToServer(reader);

                //Close Objects
                bulkData.Close();

                source.Close();
                destination.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine("DataBaseOperations:TableBulkCopy:: Copying exising data exception information! : {0}", ex);
            }

        }

        public void DeleteAllRows(String SourceConnection, String TableName)
        {
            try
            {
                SqlConnection conn = new SqlConnection(SourceConnection);
                conn.Open();

                SqlCommand cmd = new SqlCommand("TRUNCATE TABLE "+TableName, conn);

                cmd.ExecuteNonQuery();

                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("DataBaseOperations:DeleteAllRows:: Delete data from table exception information! : {0}", ex);
            }
        }

        public int CountRows(String SourceConnection, String TableName, String ColumnName, String MatchString)
        {
            SqlConnection conn = new SqlConnection(SourceConnection);
            SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM " + TableName + " WHERE " + ColumnName + " = " + MatchString, conn);
            SqlDataAdapter da = new SqlDataAdapter();

            DataTable dt = new DataTable();

            conn.Open();

            da.Fill(dt);

            conn.Close();

            return dt.Rows.Count;
        }

        public DataSet QueryDS(String SourceConnection, String Query)
        {
            SqlConnection conn = new SqlConnection(SourceConnection);
            SqlDataAdapter sqlDA = new SqlDataAdapter("SELECT [ValuationID],[Post],[Location],[Department],[Role],[Language],[Work Group],[Shift],[Resource Utilisation Ref_ID],[Activity],[DataType],[SystemType], [Unique ID], [WFMTime#], [StateTime#], [ActualTime#], [WFMValuation#], [InvoiceValuation#], [ForecastTime#], [ForecastValuation#], [BillableValuationID] FROM KeyValuationActual_destination where [Unique ID] IS NULL", conn);

            DataSet ds = new DataSet();

            conn.Open();

            sqlDA.Fill(ds);

            conn.Close();

            return ds;
        }

        public void TableBulkCopySQL(String SourceConection, String DestinationConnection, String DestinationTableName, String Query)
        {

            try
            {
                SqlConnection source = new SqlConnection(SourceConection);
                SqlConnection destination = new SqlConnection(DestinationConnection);

                source.Open();
                destination.Open();

                SqlCommand cmd = new SqlCommand(Query, source);
                cmd.CommandTimeout = 600;

                cmd.ExecuteNonQuery();

                //Execute reader
                SqlDataReader reader = cmd.ExecuteReader();

                //Create BulkCopy
                SqlBulkCopy bulkData = new SqlBulkCopy(destination);

                //Set destination table
                bulkData.DestinationTableName = DestinationTableName;

                //Write Data
                bulkData.WriteToServer(reader);

                //Close Objects
                bulkData.Close();

                source.Close();
                destination.Close();


            }
            catch (Exception ex)
            {
                Console.WriteLine("DataBaseOperations:TableBulkCopySQL:: Copying existing data exception information! : {0}", ex);
            }

        }

        /*public void ExportToExcel(String SourceConection, String SourceTable)
        {
            SqlConnection conn = new SqlConnection(SourceConection);
            SqlCommand cmd = new SqlCommand("SELECT * FROM "+ SourceTable, conn);
            DataTable dt = new DataTable();

            conn.Open();

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dt);

            conn.Close();


            XLWorkbook workbook = new XLWorkbook();
            workbook.Worksheets.Add(dt, "Valuations");

            //HttpResponse httpResponse = Response;  <================================================ FIX THIS
            httpResponse.Clear();
            //Response.Buffer = true;
            httpResponse.AddHeader("content-disposition", "attachment;filename=valuations_" + DateTime.Today.ToString("yyyyMMdd") + ".xlsx");
            //Response.Charset = "";
            httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            using (ExcelPackage xlpk = new ExcelPackage())
            {
                ExcelWorksheet ws = xlpk.Workbook.Worksheets.Add("Valuations");
                ws.Cells["A1"].LoadFromDataTable(dt, true);

                var ms = new System.IO.MemoryStream();
                xlpk.SaveAs(ms);
                ms.WriteTo(httpResponse.OutputStream);
            }
            HttpContext.Current.Response.Flush();
            HttpContext.Current.Response.End();

            da.Dispose();
        }*/

        public bool Success()
        {
            return true;
        }

        public bool Failure()
        {
            return false;
        }
    }
}