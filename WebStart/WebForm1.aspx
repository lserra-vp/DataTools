<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="WebStart.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <!-- <link href="https://fonts.googleapis.com/css?family=Roboto+Condensed" rel="stylesheet" /> -->
    <script src="Scripts/jquery-3.2.1.min.js"></script>
    <link rel="stylesheet" href="Scripts/bootstrap.min.css" />
    <script src="Scripts/bootstrap.min.js"></script>
    <link rel="stylesheet" href="styles.css" />
    
    <script type="text/javascript">
        

        function ShowModal() {
            console.log("click from SHOWMODAL");

                
            $('#NoInteraction').show();
            $('#LoadingModal').show();

            var top = Math.max($(document).height() / 2 - $('#LoadingModal').outerHeight() / 2, 0);
            var left = Math.max($(document).width() / 2 - $('#LoadingModal').outerWidth() / 2, 0);

            $('#LoadingModal').css({ top: ($(document).height() / 2 - $('#LoadingModal').outerHeight() / 2), left: ($(document).width() / 2 - $('#LoadingModal').outerWidth() / 2) });
        };

        $(document).ready(function () {
            $('#GenesysValuationsRequestBt').click(function () {
                console.log("click from GET VALUATIONS");
                ShowModal();
            });
        });
    </script>
</head>
<body>
    
    <form id="form1" runat="server">
        <div class="container-fluid col-lg-12">
            <div class="row">
                <button id="GenesysValuationsRequestBt" runat="server" class="btn btn-sm btn-danger">Get Valuations</button>
                <button id="ExportBt" runat="server" onclick="ExportToExcel_click" class="btn btn-sm btn-primary">Export to Excel</button>
                <asp:GridView ID="ValuationsView" runat="server" Width="1500px" CssClass="table" BorderStyle="None" GridLines="None" ShowHeaderWhenEmpty="True">
                    <AlternatingRowStyle BackColor="#CCFFFF" />
                    <HeaderStyle BackColor="#3399FF" Wrap="False" />
                </asp:GridView>
            </div>
        </div>
        
    </form>

   <!-- <div id="modal" style="display:none;"> -->
        <div id="NoInteraction"></div>
        <div id="LoadingModal">
            <div id="QueryMessage" class="QueryMessage" runat="server">Querying. Please wait</div>
            <div id="ProgressBar" class="ProgressBar" style="width: 60px; height: 10px; border: 1px solid black;">
                <div style="width: 0px; height: 10px; background-color: green;" id="FillBar" runat="server">&nbsp;</div>
            </div>
            <img id="Hourglass" class="Hourglass" src="img/hourglass.svg" alt="" runat="server" />
            <button id="loadingdonebutton" onclick="CloseLoading_click" class="btn btn-sm btn-success" runat="server">Done!</button>
        </div>
    <!-- </div> -->
    
</body>
</html>
