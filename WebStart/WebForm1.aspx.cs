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

namespace WebStart
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        String strConnection = "Data Source=vpro-sql1;Initial Catalog=RE_INT_IE_DataQuality;Integrated Security=True";
        BackgroundWorker bg = new BackgroundWorker();
        DataSet ds = new DataSet();

        int completion = 0;
        int query_counter = 0;

        //string[] sqlArray = new string[2] { "SELECT * FROM dqWFM_Valuation_Genesys", "SELECT * FROM dqWFM_Valuation_Impact360"};
        string[] sqlArray = new string[6] { "SELECT * FROM dqWFM_Valuation_Genesys", "SELECT * FROM dqWFM_Valuation_Impact360", "SELECT * FROM dqWFM_Valuation_Injixo", "SELECT * FROM dpState_Valuation_ICApp", "SELECT * FROM dqFiveNine_Phone_Valuation", "SELECT * FROM dqNFocus_Phone_Valuation" };

        public WebForm1()
        {
            this.AsyncMode = true;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            ExportBt.Visible = false;
            
            InitializeBackgroundWorker();
        }

        private void InitializeBackgroundWorker()
        {
            bg.DoWork += new DoWorkEventHandler(bg_DoWork);
            //bg.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bg_Completed);
            bg.ProgressChanged += new ProgressChangedEventHandler(bg_ProgressChanged);

            this.bg.WorkerReportsProgress = true;
            this.bg.WorkerSupportsCancellation = true;
        }

        protected void GenesysValuationsRequestBt_Click(object sender, EventArgs e)
        {
            //string script = "$(document).ready(function () { $('[id*=btnSubmit]').click(); });";
            QueryArray(query_counter);

        }

        private void NextQuery()
        {
            if (query_counter < sqlArray.Length)
            {
                QueryArray(query_counter);
            }
            else
            {
                if (ds.Tables.Count > 0)
                {
                    ValuationsView.DataSource = ds;
                    ValuationsView.DataBind();
                }
                DbCounter.Text = "Done!";
                ExportBt.Visible = true;
            }
        }

        private void QueryArray(int counter)
        {
            try
            {
                //this.bg.RunWorkerAsync();
                SqlConnection conn = new SqlConnection(strConnection);
                //conn.Open();

                SqlDataAdapter sqlDA = new SqlDataAdapter(sqlArray[counter], conn);
                

                completion++;

                UpdateProgressBar(completion);

                sqlDA.SelectCommand.CommandTimeout = 600;

                sqlDA.Fill(ds);

                //conn.Close();

                query_counter++;

                NextQuery();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Excaption information! : {0}", ex);
            }

        }

        private void UpdateProgressBar(int completion)
        {
            DbCounter.Text = "" + completion;
            progressbar.Style.Add("width", (completion * 10) + "px");
        }

        protected void bg_DoWork(object sender, DoWorkEventArgs e)
        {
            

            // Get the BackgroundWorker that raised this event.
            BackgroundWorker worker = sender as BackgroundWorker;

            
        }

        private void bg_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            DbCounter.Text = "" + e.ProgressPercentage;
            progressbar.Style.Add("width", (e.ProgressPercentage * (100/sqlArray.Length)) + "px");
        }

        protected void OnPageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            ValuationsView.PageIndex = e.NewPageIndex;
        }

        protected void ExportToExcel_click(object sender, EventArgs e)
        {
            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment;filename=ValuationsExport.xls");
            Response.Charset = "";
            Response.ContentType = "application/vnd.ms-excel";

            using(StringWriter sw = new StringWriter())
            {
                HtmlTextWriter hw = new HtmlTextWriter(sw);

                ValuationsView.AllowPaging = false;

                ValuationsView.RenderControl(hw);

                Response.Output.Write(sw.ToString());
                Response.Flush();
                Response.End();

            }
        }

        public override void VerifyRenderingInServerForm(Control control)
        {

        }

    }
}