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

//using System.Data.SqlClient;

using Microsoft.SqlServer.Server;

namespace WebStart
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        bool customQueryVisible = false;

        String strConnectionServerRoot = "Data Source=vpro-sql1;Integrated Security=True";
        String strConnectionMain = "Data Source=vpro-sql1;Initial Catalog=CapacityPlanning;Integrated Security=True";
        String sqlMainCommand = "SELECT * FROM KeyValuationActual";

        String strConnectionForUpdates = "Data Source=vpro-sql1;Initial Catalog=RE_INT_IE_DataQuality;Integrated Security=True;MultipleActiveResultSets=True";

        BackgroundWorker bg = new BackgroundWorker();
        DataSet ds;
        //DataSet duplicatesDataSet;

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
            GenesysCheckDuplicatesBt.Click += new EventHandler(this.CheckDuplicates_Click);
            GenesysCheckKeysBt.Click += new EventHandler(this.CheckKeys_click);

            CustomSqlQueryBt.Click += new EventHandler(this.ShowCustomQueryArea);
            
            RunQuery.Click += new EventHandler(this.RunCustomQuery_click);
        }

        /// <summary>
        ///  Check and adds missing keys to lines automatically
        /// </summary>
        private void CheckKeys_click(object sender, EventArgs e)
        {
            CheckMissingKeys(true);
        }

        private void CheckMissingKeys(bool showKeyLessResults)
        {
            CheckAndAddValuationKeys keys = new CheckAndAddValuationKeys(QueryFinalTable(), strConnectionForUpdates);

            DataSet _keylessDataSet = new DataSet();
            DataTable _keylessDataTable = new DataTable();

            //_keylessDataTable = keys.getDataTable(); //Return rows with no key before trying to get them

            keys.getMissingKeys(); //Checks for rows with missing keys and attempts to add them

            _keylessDataTable = keys.getDataTable(); //Return the rows with the keys

            _keylessDataSet.Tables.Add(_keylessDataTable); //Converts DataTable to DataSet

            CopyNewKeysToDatabase(keys.getDataTable());

            if (showKeyLessResults)
            {
                ShowValuationsInGridView(_keylessDataSet);
                GenesysValuationsExportBt.Visible = true;
            }
        }

        /// <summary>
        ///  Triggered once the "Get Duplicates" button is clicked
        ///  Deletes the copy table and copies the actual data from the original table
        ///  Calls the method for checking duplicates
        /// </summary>
        private void CheckDuplicates_Click(object sender, EventArgs e)
        {
            DeleteDataCopyTable();

            Message.InnerHtml = "Copying existing data";

            Thread.Sleep(500);

            //Copy existing table data to copy database
            CopyExistingDatabase();

            CheckDuplicatesGridview();
        }

        /// <summary>
        ///  Queries the table and send results to gridview
        /// </summary>
        private void CheckDuplicatesGridview()
        {
            SqlConnection conn = new SqlConnection(strConnectionForUpdates);
            SqlCommand cmd = new SqlCommand("SELECT ValuationID, COUNT(*) as [Duplicates] FROM KeyValuationActual_destination GROUP BY ValuationID HAVING COUNT(*) > 1", conn);

            try
            {
                conn.Open();

                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = cmd;

                DataSet duplicates = new DataSet();

                da.Fill(duplicates);

                if (duplicates.Tables.Count > 0)//Checks if the DataSource is not empty
                {
                    ValuationsView.DataSource = duplicates; //Sets the GridView Datasource with the queried table datasource
                    ValuationsView.DataBind(); //Binds the DataSource with the visualGrid object
                }

                conn.Close();

            }
            catch (Exception ex)
            {
                CustomQueryMessages.Text = (ex.Message);
            }

        }

        private DataTable CheckDuplicatesToDelete()
        {
            SqlConnection conn = new SqlConnection(strConnectionForUpdates);
            SqlCommand cmd = new SqlCommand("SELECT ValuationID, COUNT(*) as [Duplicates] FROM KeyValuationActual_destination GROUP BY ValuationID HAVING COUNT(*) > 1", conn);
            DataTable dt = new DataTable();
            SqlDataAdapter da = new SqlDataAdapter();

            try
            {
                conn.Open();

                da.SelectCommand = cmd;

                da.Fill(dt);

                conn.Close();

            }
            catch (Exception ex)
            {
                CustomQueryMessages.Text = (ex.Message);
            }

            return dt;
        }

        /// <summary>
        ///  Queries the server to retrieve all databases.
        ///  Sends result to fill the dropdown
        /// </summary>
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

        /// <summary>
        ///  Populates the dropdown with the database list
        /// </summary>
        private void addListToDropdown(List<string> list)
        {
            foreach(string name in list)
            {
                DatabaseList.Items.Add(new ListItem(name));
            }
        }

        /// <summary>
        ///  Shows/Hide the Custom Query area
        /// </summary>
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

        /// <summary>
        ///  Starts the custom query and sends result to gridview
        /// </summary>
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

        /// <summary>
        ///  Retrieves the database list on server
        /// </summary>
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


            //Check for duplicates and deletes them before querying for new valuations (This way, deleted rows will be included in the query)
            CheckAndDeleteDuplicates();
            
            //Add new valuations data
            QueryArray();

            CheckMissingKeys(false);

            ShowValuationsInGridView(QueryFinalTable());

        }

        /// <summary>
        ///  Copies existing data to temporary database
        /// </summary>
        private void CopyExistingDatabase()
        {
            try
            {
                SqlConnection source = new SqlConnection(strConnectionForUpdates); //strConnectionForUpdatesstrConnectionMain
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

        private void CheckAndDeleteDuplicates()
        {
            DataTable dt = CheckDuplicatesToDelete();
            SqlConnection conn = new SqlConnection(strConnectionForUpdates);
            //SqlCommand cmd = new SqlCommand("SELECT ValuationID, COUNT(*) as [Duplicates] FROM KeyValuationActual_destination GROUP BY ValuationID HAVING COUNT(*) > 1", conn);


            if (dt.Rows.Count > 0)
            {
                conn.Open();
                foreach(DataRow row in dt.Rows)
                {
                    //string pattern = Regex.Escape(row["ValuationID"].ToString());

                    SqlCommand cmdDelete = new SqlCommand("DELETE FROM KeyValuationActual_destination WHERE ValuationID = @pattern", conn);
                    
                    SqlParameter[] parameters = new SqlParameter[1];
                    parameters[0] = new SqlParameter("@pattern", SqlDbType.VarChar, 255);
                    parameters[0].Value = row["ValuationID"].ToString();

                    cmdDelete.Parameters.AddRange(parameters);

                    Console.WriteLine("{0} deleted rows",cmdDelete.ExecuteNonQuery()); //Executes the command and returs the number of rows deleted
                }
                conn.Close();
            }
        }

        private int CountRowsThatMatchString(String match, String tableName, String columnName)
        {
            SqlConnection conn = new SqlConnection(strConnectionForUpdates);
            SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM " + tableName + " WHERE " + columnName + " = " + match, conn);
            SqlDataAdapter da = new SqlDataAdapter();
            DataTable dt = new DataTable();

            conn.Open();

            da.Fill(dt);

            conn.Close();

            return dt.Rows.Count;
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

                QueryArray();
                
            }
            else
            {
                if (QueryFinalTable().Tables.Count > 0)//Checks if the DataSource is not empty
                {
                    ShowValuationsInGridView(QueryFinalTable());
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


        private void ShowValuationsInGridView(DataSet ds)
        {

            ValuationsView.DataSource = ds; //Sets the GridView Datasource with the queried table datasource
            ValuationsView.DataBind(); //Binds the DataSource with the visualGrid object
        }
        

        /// <summary>
        ///  Gets the newly added valuations rows and returns a DataSource object
        /// </summary>
        private DataSet QueryFinalTable()
        {
            SqlConnection conn = new SqlConnection(strConnectionForUpdates);
            SqlDataAdapter sqlDA = new SqlDataAdapter("SELECT [ValuationID],[Post],[Location],[Department],[Role],[Language],[Work Group],[Shift],[Resource Utilisation Ref_ID],[Activity],[DataType],[SystemType], [Unique ID], [WFMTime#], [StateTime#], [ActualTime#], [WFMValuation#], [InvoiceValuation#], [ForecastTime#], [ForecastValuation#], [BillableValuationID] FROM KeyValuationActual_destination where [Unique ID] IS NULL", conn);

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
            completion++;

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

        public void CopyNewKeysToDatabase(DataTable newData)
        {
            try
            {
                SqlConnection destination = new SqlConnection(strConnectionForUpdates);
                SqlCommand sqlDelete = new SqlCommand("DELETE FROM KeyValuationActual_destination WHERE [WFMTime#] IS NULL", destination);

                destination.Open();

                sqlDelete.ExecuteNonQuery();

                //Create BulkCopy
                SqlBulkCopy bulkData = new SqlBulkCopy(destination);

                //Set destination table
                bulkData.DestinationTableName = "KeyValuationActual_destination";

                //Write Data
                bulkData.WriteToServer(newData);

                //Close Objects
                bulkData.Close();

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
        private void QueryArray()
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

        private void CompareAndGetMissingKeys()
        {

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