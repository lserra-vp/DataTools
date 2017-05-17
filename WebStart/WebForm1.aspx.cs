using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.IO;
using System.Drawing;
using System.ComponentModel;
using System.Threading;
using ClosedXML.Excel;
using OfficeOpenXml;

using Microsoft.SqlServer.Server;

namespace WebStart
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        bool customQueryVisible = false;

        String strConnectionServerRoot = "Data Source=vpro-sql1;Integrated Security=True";
        String strConnectionMain = "Data Source=vpro-sql1;Initial Catalog=CapacityPlanning;Integrated Security=True";
        String sqlMainCommand = "SELECT * FROM KeyValuationActual";

        String strConnectionForUpdates = "Data Source=vpro-sql1;Initial Catalog=RE_INT_IE_DataQuality;Integrated Security=True";

        BackgroundWorker bg = new BackgroundWorker();
        DataSet ds;

        int completion = 0;
        int query_counter = 0;

        string[] sqlArray = new string[6] { "SELECT * FROM dqWFM_Valuation_Genesys", "SELECT * FROM dqWFM_Valuation_Impact360", "SELECT * FROM dqWFM_Valuation_Injixo", "SELECT * FROM dpState_Valuation_ICApp", "SELECT * FROM dqFiveNine_Phone_Valuation", "SELECT * FROM dqNFocus_Phone_Valuation" };

        //String with testing tables - To be quicker
        //string[] sqlArray = new string[5] { "SELECT * FROM genesys_phone_source", "SELECT * FROM genesys_source", "SELECT * FROM icapp_source", "SELECT * FROM impact360_source", "SELECT * FROM injixo_source"};

        /// <summary>
        ///  Constructor. Sets Asynchronous Mode to true.
        /// </summary>
        public WebForm1()
        {
            this.AsyncMode = true;
        }

        /// <summary>
        ///  When page loads calls for page elements initialization
        /// </summary>
        protected void Page_Load(object sender, EventArgs e)
        {
            GenesysValuationsExportBt.Visible = false;
            UserSqlQuery.Visible = false;

            //Check if the Custom SQL has any results on GridView after querying
            //After the query the page is refreshed
            if(CustomQueryView.Rows.Count > 0 )
            {
                UserSqlQuery.Visible = true;
            }

            InitializeElements();
        }

        /// <summary>
        ///  Initializes elements on page.
        ///  Adds click events to buttons
        /// </summary>
        protected void InitializeElements()
        {
            Message.InnerHtml = "Idle...";

            GetInitialDatabaseListForQuery();

            GenesysValuationsRequestBt.Click += new EventHandler(this.GenesysValuationsRequestBt_Click);
            GenesysValuationsExportBt.Click += new EventHandler(this.ExportToExcel_click);

            CustomSqlQueryBt.Click += new EventHandler(this.ShowCustomQueryArea);
            
            RunQuery.Click += new EventHandler(this.RunCustomQuery_click);
        }

        private void GetInitialDatabaseListForQuery()
        {
            List<String> databaseNames = new List<String>();

            using (var con = new SqlConnection(strConnectionServerRoot))
            {
                con.Open();

                DataTable dt = con.GetSchema("Databases");

                foreach(DataRow dr in dt.Rows)
                {
                    databaseNames.Add(dr.Field<String>("database_name"));
                }
            }

            databaseNames.Sort();

            addListToDropdown(databaseNames);
        }

        private void addListToDropdown(List<string> list)
        {
            foreach(string name in list)
            {
                DatabaseList.Items.Add(new ListItem(name));
            }
        }

        private void ShowCustomQueryArea(object sender, EventArgs e)
        {
            if (!customQueryVisible)
            {
                customQueryVisible = true;
                UserSqlQuery.Visible = true;
                CustomSqlQueryBt.Text = "Hide Custom Query";
            }
            else
            {
                customQueryVisible = false;
                UserSqlQuery.Visible = false;
                CustomSqlQueryBt.Text = "Show Custom Query";
            }
            
        }

        private void RunCustomQuery_click(object sender, EventArgs e)
        {

            var customSqlConnection = new SqlConnection("Server=vpro-sql1;Initial Catalog=" + DatabaseList.SelectedItem.Value + ";Trusted_Connection=True;");
            var queryText = Sqlquery.Text;

            SqlCommand cmd = new SqlCommand(Sqlquery.Text, customSqlConnection);

            try
            {

                customSqlConnection.Open();

                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = cmd;

                DataSet ds = new DataSet();

                da.Fill(ds);

                if (ds.Tables.Count > 0)//Checks if the DataSource is not empty
                {
                    CustomQueryView.DataSource = ds; //Sets the GridView Datasource with the queried table datasource
                    CustomQueryView.DataBind(); //Binds the DataSource with the visualGrid object
                }

                customSqlConnection.Close();

            }
            catch (Exception ex)
            {
                CustomQueryMessages.Text = (ex.Message);
            }

        }

        private List<string> GetDatabaseList()
        {
            List<string> list = new List<string>();

            using (SqlConnection con = new SqlConnection(strConnectionServerRoot))
            {
                con.Open();

                // Set up a command with the given query and associate
                // this with the current connection.
                using (SqlCommand cmd = new SqlCommand("SELECT name from sys.databases", con))
                {
                    using (IDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(dr[0].ToString());
                        }
                    }
                }
            }
            return list;

        }

        /// <summary>
        ///  Event when "Get Valuations" button is pressed
        /// </summary>
        protected void GenesysValuationsRequestBt_Click(object sender, EventArgs e)
        {

            Message.InnerHtml = "Deleting copy database";

            Thread.Sleep(500);
           
            //Delete destination table
            DeleteDataCopyTable();

            Message.InnerHtml = "Copying existing data";

            Thread.Sleep(500);

            //Copy existing table data to copy database
            CopyExistingDatabase();

            //Add new valuations data
            QueryArray(query_counter);
        }

        /// <summary>
        ///  Copies existing data to temporary database
        /// </summary>
        private void CopyExistingDatabase()
        {
            try
            {
                SqlConnection source = new SqlConnection(strConnectionMain); //strConnectionMain
                SqlConnection destination = new SqlConnection(strConnectionForUpdates);

                source.Open();
                destination.Open();

                SqlCommand cmd = new SqlCommand(sqlMainCommand, source);

                cmd.ExecuteNonQuery();

                //Execute reader
                SqlDataReader reader = cmd.ExecuteReader();

                //Create BulkCopy
                SqlBulkCopy bulkData = new SqlBulkCopy(destination);

                //Set destination table
                bulkData.DestinationTableName = "KeyValuationActual_destination";

                //Write Data
                bulkData.WriteToServer(reader);

                //Close Objects
                bulkData.Close();

                source.Close();
                destination.Close();
                

            }catch(Exception ex)
            {
                Console.WriteLine("Copying exising data exception information! : {0}", ex);
            }
        }

        /// <summary>
        ///  Deletes data from temporary table
        /// </summary>
        private void DeleteDataCopyTable()
        {
            try
            {
                SqlConnection conn = new SqlConnection(strConnectionForUpdates);
                conn.Open();

                SqlCommand cmd = new SqlCommand("TRUNCATE TABLE KeyValuationActual_destination", conn);
                cmd.ExecuteNonQuery();

                conn.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Delete data from copy table exception information! : {0}", ex);
            }
        }

        /// <summary>
        ///  Iterates the trigering of a new SQL query travlling through the SQL command query
        /// </summary>
        protected void NextQuery()
        {
            if (query_counter < sqlArray.Length)
            {
                UpdateProgressBar(completion);

                Thread.Sleep(500);

                QueryArray(query_counter);
                
            }
            else
            {
                if (QueryFinalTable().Tables.Count > 0)//Checks if the DataSource is not empty
                {
                    ValuationsView.DataSource = QueryFinalTable(); //Sets the GridView Datasource with the queried table datasource
                    ValuationsView.DataBind(); //Binds the DataSource with the visualGrid object
                    Message.InnerHtml = " ";
                }
                else
                {
                    Message.InnerHtml = "No data was returned by the query";
                }

                GenesysValuationsExportBt.Visible = true;
                
                Message.InnerHtml = "";
            }
        }

        /// <summary>
        ///  Gets the newly added valuations rows and returns a DataSource object
        /// </summary>
        private DataSet QueryFinalTable()
        {
            SqlConnection conn = new SqlConnection(strConnectionForUpdates);
            SqlDataAdapter sqlDA = new SqlDataAdapter("SELECT [ValuationID],[Post],[Location],[Department],[Role],[Language],[Work Group],[Shift],[Resource Utilisation Ref_ID],[Activity],[DataType],[SystemType] FROM KeyValuationActual_destination where [Unique ID] IS NULL", conn);

            ds = new DataSet();

            conn.Open();

            sqlDA.Fill(ds);

            conn.Close();

            return ds;
        }

        /// <summary>
        ///  Adds the new data to the temporary table
        /// </summary>
        private void CopyNewDataToDatabase(int counter)
        {
            try
            {
                SqlConnection source = new SqlConnection(strConnectionForUpdates);
                SqlConnection destination = new SqlConnection(strConnectionForUpdates);

                source.Open();
                destination.Open();

                SqlCommand cmd = new SqlCommand(sqlArray[counter], source);
                cmd.CommandTimeout = 600;

                cmd.ExecuteNonQuery();

                //Execute reader
                SqlDataReader reader = cmd.ExecuteReader();

                //Create BulkCopy
                SqlBulkCopy bulkData = new SqlBulkCopy(destination);

                //Set destination table
                bulkData.DestinationTableName = "KeyValuationActual_destination";

                //Write Data
                bulkData.WriteToServer(reader);

                //Close Objects
                bulkData.Close();

                source.Close();
                destination.Close();


            }
            catch (Exception ex)
            {
                Console.WriteLine("Copying exising data exception information! : {0}", ex);
            }
        }

        /// <summary>
        ///  After copying new rows to the temporary table, iterates NextQuery to run the next SQL query
        /// </summary>
        private void QueryArray(int counter)
        {
            try
            {
                CopyNewDataToDatabase(query_counter);

                completion++;
                query_counter++;

                NextQuery();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Update Views Exception information! : {0}", ex);
            }

        }

        /// <summary>
        ///  Attempts to update the progress bar in the waiting window
        /// </summary>
        private void UpdateProgressBar(int completion)
        {
            FillBar.Style.Add("width", (completion * 10) + "px");
        }

        /// <summary>
        ///  After a click at the "Export to excel" button, it triggers the process to gather the full data and export an Excel file
        /// </summary>
        protected void ExportToExcel_click(object sender, EventArgs e)
        {
            SqlConnection conn = new SqlConnection(strConnectionForUpdates);
            SqlCommand cmd = new SqlCommand("SELECT * FROM KeyValuationActual_destination", conn);
            DataTable dt = new DataTable();

            conn.Open();

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            da.Fill(dt);

            conn.Close();


            XLWorkbook workbook = new XLWorkbook();
            workbook.Worksheets.Add(dt, "Valuations");

            HttpResponse httpResponse = Response;
            httpResponse.Clear();
            //Response.Buffer = true;
            httpResponse.AddHeader("content-disposition", "attachment;filename=valuations_"+ DateTime.Today.ToString("yyyyMMdd") + ".xlsx");
            //Response.Charset = "";
            httpResponse.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            using(ExcelPackage xlpk = new ExcelPackage())
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
        }

        /// <summary>
        ///  Overide function to enable render in browser
        /// </summary>
        public override void VerifyRenderingInServerForm(Control control)
        {

        }

    }
}