<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebApplication1.Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Search News@SEU</title>
    <style type="text/css">
    body
    {
        font-family: Verdana, Droid Sans, Tahoma;
    }
    .url
    {
        font-size:9px;
    }
    li
    {
        margin-bottom:5px;
    }
    .editbox
    {
        background: #ffffff;
        border: 1px solid #b7b7b7;
        color: #003366;
        cursor: text;
        font-family: Verdana, Droid Sans, Tahoma;
        font-size: 15px;
        height: 23px;
        width:500px;
        vertical-align:middle;
        /*padding: 1px;*/
    }
    </style>
    <style type="text/css">
    .searchbtn 
    {
	    -moz-box-shadow:inset 0px 1px 0px 0px #ffffff;
	    -webkit-box-shadow:inset 0px 1px 0px 0px #ffffff;
	    box-shadow:inset 0px 1px 0px 0px #ffffff;
	    background:-webkit-gradient( linear, left top, left bottom, color-stop(0.05, #ededed), color-stop(1, #dfdfdf) );
	    background:-moz-linear-gradient( center top, #ededed 5%, #dfdfdf 100% );
	    filter:progid:DXImageTransform.Microsoft.gradient(startColorstr='#ededed', endColorstr='#dfdfdf');
	    background-color:#ededed;
	    border:1px solid #dcdcdc;
	    display:inline-block;
	    color:#777777;
	    font-family:arial;
	    font-size:15px;
	    font-weight:bold;
	    padding:5px 24px;
	    text-decoration:none;
	    text-shadow:1px 1px 0px #ffffff;
	    vertical-align:middle;
    }
    .searchbtn:hover 
    {
	    background:-webkit-gradient( linear, left top, left bottom, color-stop(0.05, #dfdfdf), color-stop(1, #ededed) );
	    background:-moz-linear-gradient( center top, #dfdfdf 5%, #ededed 100% );
	    filter:progid:DXImageTransform.Microsoft.gradient(startColorstr='#dfdfdf', endColorstr='#ededed');
	    background-color:#dfdfdf;
    }
    .searchbtn:active 
    {
	    position:relative;
	    top:1px;
    }
</style>
</head>
<body>
<form id="form1" runat="server">
<asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>            
            <div>
                <asp:TextBox ID="txtQuery" runat="server" class="editbox"></asp:TextBox> 
                <asp:Button ID="btnSearch"
                    runat="server" Text="Search" onclick="btnSearch_Click" class="searchbtn"/>
            </div>
            <div>
            <asp:UpdateProgress ID="updateprogress2" runat="server">
                <ProgressTemplate>
                <div style="padding-left:100px; margin-top:15px; margin-bottom:15px">
                    Searching...
                </div>
                </ProgressTemplate>
            </asp:UpdateProgress>
            </div>
            <asp:Literal ID="ltlResult" runat="server"></asp:Literal>
        </ContentTemplate>
    </asp:UpdatePanel>
</form>
</body>
</html>
