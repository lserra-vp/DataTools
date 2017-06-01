<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="WebStart.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link href="styles.css" rel="stylesheet" />
    <!-- <link href="https://fonts.googleapis.com/css?family=Roboto+Condensed" rel="stylesheet" /> -->
    <link href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css" rel="stylesheet"/>
    <script type="text/javascript" src="Scripts/jquery-1.9.1.min.js"></script>
    <script type="text/javascript" src="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.min.js"></script>
    <script type="text/javascript">

        var queryVisible = false;

        function ShowProgress() {
            console.log("ShowProgress Function");
            $('#no_interaction').show();
            $('#loading_done_button').hide();

            var up = Math.max($(window).height() / 2 - $("#loading_modal").outerHeight() / 2, 0);
            var side = Math.max($(window).width() / 2 - $("#loading_modal").outerWidth() / 2, 0);

            var styles = {
                top: up,
                left: side
            };

            $("#loading_modal").css(styles);
            $("#loading_modal").show();
        }

        
        $(document).ready(function () {
            $("#loading_modal").hide();
            $('#no_interaction').hide();
            
        });

        /*function toggleCustomQuery() {
            console.log("Custom SQL");

            if (!queryVisible) {
                queryVisible = true;
                $("#UserSqlQuery").show();
                $("#CustomSqlQueryBt").html("Hide Custom Query");
            } else {
                queryVisible = false;
                $("#UserSqlQuery").hide();
                $("#CustomSqlQueryBt").html("Show Custom Query");
            }
        };*/

    </script>
</head>
<body>
    

    <form id="results" runat="server">
        <nav class="navbar navbar-default">
            <div class="container-fluid">
                <div class="navbar-header">
                    <a class="navbar-brand abs" href="#"><img src="img/voxpro_data_logo.png" /></a>
                </div>
                <ul class="nav navbar-nav navbar-right">
                    <li>
                        <asp:Button ID="CustomSqlQueryBt" CssClass="btn btn-default" runat="server" Text="Show Custom Query" />
                    </li>
                </ul>
            </div>
        </nav>

        <div class="container-fluid col-lg-12">
            <asp:Button ID="GenesysValuationsRequestBt" runat="server" Text="Get Valuations" CssClass="btn btn-sm btn-danger" OnClientClick="ShowProgress();"/>
            <asp:Button ID="GenesysCheckDuplicatesBt" runat="server" Text="Check Duplicates" CssClass="btn btn-sm btn-warning" OnClientClick="ShowProgress();"/>
            <asp:Button ID="GenesysCheckKeysBt" runat="server" Text="Check Keys" CssClass="btn btn-sm btn-warning" OnClientClick="ShowProgress();"/>

            <asp:Button ID="GenesysValuationsExportBt" runat="server" Text="Export To Excel" CssClass="btn btn-sm btn-success"/>
            <div id="Message" runat="server"></div>
            <div class="test table-responsive">
                <asp:GridView ID="ValuationsView" runat="server" Width="1000px" CssClass="table table-striped table-bordered table-hover" BorderStyle="None" GridLines="None" ShowHeaderWhenEmpty="True" AlternatingRowStyle-Wrap="False" FooterStyle-Wrap="False" HeaderStyle-Wrap="False" PagerStyle-Wrap="False" SelectedRowStyle-Wrap="False" SortedAscendingCellStyle-Wrap="False" SortedAscendingHeaderStyle-Wrap="False" SortedDescendingHeaderStyle-Wrap="False">
                    <AlternatingRowStyle BackColor="#CCFFFF" Font-Bold="False" />
                    <HeaderStyle BackColor="#3399FF" Wrap="False" />
                </asp:GridView>
            </div>
            <div id="UserSqlQuery" runat="server">
                <p>Custom SQL Query</p>
                <asp:DropDownList ID="DatabaseList" runat="server"></asp:DropDownList>
                <asp:TextBox ID="Sqlquery" style="width:100%;" runat="server" BackColor="#FFFFCC" BorderColor="Silver" BorderStyle="Solid" BorderWidth="1" Text="Type your query here..." Height="50"></asp:TextBox>
                <div class="test table-responsive">
                    <asp:GridView ID="CustomQueryView" runat="server" Width="1000px" Height="500px" CssClass="table table-striped table-bordered table-hover" BorderStyle="None" GridLines="None" ShowHeaderWhenEmpty="True" AlternatingRowStyle-Wrap="False" FooterStyle-Wrap="False" HeaderStyle-Wrap="False" PagerStyle-Wrap="False" SelectedRowStyle-Wrap="False" SortedAscendingCellStyle-Wrap="False" SortedAscendingHeaderStyle-Wrap="False" SortedDescendingHeaderStyle-Wrap="False">
                        <AlternatingRowStyle BackColor="#CCFFFF" Font-Bold="False" />
                        <HeaderStyle BackColor="#3399FF" Wrap="False" />
                    </asp:GridView>
                </div>
                <asp:Button ID="RunQuery" runat="server" CssClass="btn btn-sm btn-primary" Text="Run Query" />
                <asp:Label ID="CustomQueryMessages" runat="server" Text="" Font-Bold="True" ForeColor="#990000"></asp:Label>
            </div>
        </div>
    </form>

    <div id="wait">
        <div id="no_interaction" runat="server"></div>
        <div id="loading_modal" runat="server">
            <div id="query_message" runat="server">Querying. Please wait</div>
            <div id="progress_bar" style="width: 60px; height: 10px; border: 1px solid black;">
                <div style="width: 0px; height: 10px; background-color: green;" id="FillBar" runat="server">&nbsp;</div>
            </div>
            <img id="hourglass" src="img/hourglass.svg" runat="server"/>
            <button id="loading_done_button" class="btn btn-sm btn-success" runat="server" style="display:none;">Done!</button>
        </div>
    </div>
    
</body>
</html>
