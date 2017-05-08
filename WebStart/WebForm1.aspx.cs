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

namespace WebStart
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        String strConnectionMain = "Data Source=vpro-sql1;Initial Catalog=CapacityPlanning;Integrated Security=True";
        String sqlMainCommand = "SELECT * FROM KeyValuationActual";


        String strConnectionUpdates = "Data Source=vpro-sql1;Initial Catalog=RE_INT_IE_DataQuality;Integrated Security=True";

        BackgroundWorker bg = new BackgroundWorker();
        DataSet ds = new DataSet();

        int completion = 0;
        int query_counter = 0;

        string[] sqlArray = new string[2] { "SELECT * FROM dqWFM_Valuation_Genesys", "SELECT * FROM dqWFM_Valuation_Impact360"};
        //string[] sqlArray = new string[6] { "SELECT * FROM dqWFM_Valuation_Genesys", "SELECT * FROM dqWFM_Valuation_Impact360", "SELECT * FROM dqWFM_Valuation_Injixo", "SELECT * FROM dpState_Valuation_ICApp", "SELECT * FROM dqFiveNine_Phone_Valuation", "SELECT * FROM dqNFocus_Phone_Valuation" };

        public WebForm1()
        {
            this.AsyncMode = true;
        }

        protected void Page_Load(object sender, EventArgs e)
        {

            GenesysValuationsExportBt.Visible = false;

            InitializeElements();
            
        }

        protected void InitializeElements()
        {
            Message.InnerHtml = "Idle...";

            GenesysValuationsRequestBt.Click += new EventHandler(this.GenesysValuationsRequestBt_Click);
            GenesysValuationsExportBt.Click += new EventHandler(this.ExportToExcel_click);
        }

        private void CallWaitTimer(object sender, EventArgs e)
        {
            NextQuery();
        }

        protected void GenesysValuationsRequestBt_Click(object sender, EventArgs e)
        {
            Message.InnerHtml = "";

            //waitTimer.Start();
            Thread.Sleep(500);

            QueryArray(query_counter);
        }

        protected void CreatewaitLoadingModalDiv()
        {

        }

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
                if (ds.Tables.Count > 0)
                {
                    ValuationsView.DataSource = ds;
                    ValuationsView.DataBind();
                }

                GenesysValuationsExportBt.Visible = true;
                
                Message.InnerHtml = "";
                //LoadingDoneButton.Visible = true;
            }
        }

        private void QueryMainTable()
        {
            try
            {

                SqlConnection conn = new SqlConnection(strConnectionMain);

                SqlDataAdapter sqlDA = new SqlDataAdapter(sqlMainCommand, conn);

                sqlDA.SelectCommand.CommandTimeout = 600;

                sqlDA.Fill(ds);

                QueryArray(query_counter);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Main Table Exception information! : {0}", ex);
            }
        }

        private void QueryArray(int counter)
        {
            try
            {
                
                SqlConnection conn = new SqlConnection(strConnectionUpdates);

                SqlDataAdapter sqlDA = new SqlDataAdapter(sqlArray[counter], conn);
                
                completion++;


                sqlDA.SelectCommand.CommandTimeout = 600;

                sqlDA.Fill(ds);

                query_counter++;

                NextQuery();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Update Views Exception information! : {0}", ex);
            }

        }

        private void UpdateProgressBar(int completion)
        {

            FillBar.Style.Add("width", (completion * 10) + "px");
        }

        protected void OnPageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            ValuationsView.PageIndex = e.NewPageIndex;
        }

        protected void ExportToExcel_click(object sender, EventArgs e)
        {
            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment;filename=valuations_"+DateTime.Today.ToString("d").Replace("/","")+".xls");
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