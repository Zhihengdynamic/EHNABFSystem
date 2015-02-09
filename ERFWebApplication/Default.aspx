<%@ Page Title="EHNABF System" Language="C#" MasterPageFile="~/Site.master" AutoEventWireup="true"
    CodeBehind="Default.aspx.cs" Inherits="ERFWebApplication._Default" %>

<asp:Content ID="HeaderContent" runat="server" ContentPlaceHolderID="HeadContent">

</asp:Content>
<asp:Content ID="BodyContent" runat="server" ContentPlaceHolderID="MainContent">
    <h2>
        請輸入位置
    </h2>
    
    <p>
         <input type="text" size="60" name="address" value="Taichuang" />
         <input type="submit" value="Go!" />
    </p>
    <div id="warnings_panel"></div>
    
       <asp:Literal ID="js" runat="server"></asp:Literal><br />
       <div id="map_canvas" style="width: 100%; height: 600px"> </div>
</asp:Content>

