<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebForm1.aspx.cs" Inherits="WebStart.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <!-- <link href="https://fonts.googleapis.com/css?family=Roboto+Condensed" rel="stylesheet" /> -->
    <link href="Content/bootstrap.min.css" rel="stylesheet" />
    <link href="StyleSheet1.css" rel="stylesheet" />
    <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jquery/1.8.3/jquery.min.js"></script>
    <script type="text/javascript">
        function ShowProgress() {
            setTimeout(function () {
                var modal = $('<div />');
                modal.addClass("modal");
                $('body').append(modal);
                var loading = $(".loading");
                loading.show();
                var top = Math.max($(window).height() / 2 - loading[0].offsetHeight / 2, 0);
                var left = Math.max($(window).width() / 2 - loading[0].offsetWidth / 2, 0);
                loading.css({ top: top, left: left });
            }, 200);
        }
        $('form').live("submit", function () {
            ShowProgress();
        });
    </script>
</head>
<body>
    

    <form id="form1" runat="server">
        <div class="container-fluid" CssClass="col-lg-12">
            <div class="row">
                <asp:Button ID="GenesysValuationsRequestBt" runat="server" OnClick="GenesysValuationsRequestBt_Click" Text="Get Valuations" CssClass="btn btn-sm btn-danger"/>
                <div style="width: 60px; height: 10px; border: 1px solid black;" runat="server">
                    <div style="width: 0px; height: 10px; background-color: green;" id="progressbar" runat="server">&nbsp;</div>
                </div>
                <asp:Label ID="DbCounter" runat="server" Text="0"></asp:Label>
                <asp:GridView ID="ValuationsView" runat="server" Width="1150px" CssClass="table">
                </asp:GridView>
                <asp:Button ID="ExportBt" runat="server" OnClick="ExportToExcel_click" Text="Export to Excel" CssClass="btn btn-sm btn-primary"/>
            </div>
        </div>

    </form>
    <div class="loading" align="center" style="display: none;">
        Querying. Please wait<br />
        <img src="img/hourglass.svg" alt="" />
    </div>
    <script src="Scripts/bootstrap.min.js"></script>
</body>
</html>
