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

            /*$('#GenesysValuationsRequestBt').click(function () {
                ShowProgress();
                return false;
            });*/

            /*$('#loading_done_button').click(function () {
                $("#loading_modal").hide();
                $('#no_interaction').hide();
            });*/
        });
    </script>
</head>
<body>
    
    <form id="results" runat="server">
        <div class="container-fluid col-lg-12">
            <div class="row">
                <asp:Button ID="GenesysValuationsRequestBt" runat="server" Text="Get Valuations" CssClass="btn btn-sm btn-danger" OnClientClick="ShowProgress();"/>
                <asp:Button ID="GenesysValuationsExportBt" runat="server" CssClass="btn btn-sm btn-success" Text="Export To Excel"/>
                <asp:GridView ID="ValuationsView" runat="server" Width="1500px" CssClass="table" BorderStyle="None" GridLines="None" ShowHeaderWhenEmpty="True" AlternatingRowStyle-Wrap="False" FooterStyle-Wrap="False" HeaderStyle-Wrap="False" PagerStyle-Wrap="False" SelectedRowStyle-Wrap="False" SortedAscendingCellStyle-Wrap="False" SortedAscendingHeaderStyle-Wrap="False" SortedDescendingHeaderStyle-Wrap="False">
                    <AlternatingRowStyle BackColor="#CCFFFF" Font-Bold="False" />
                    <HeaderStyle BackColor="#3399FF" Wrap="False" />
                </asp:GridView>
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
    <div id="Message" runat="server"></div>
</body>
</html>
