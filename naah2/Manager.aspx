<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Manager.aspx.cs" Inherits="naah2.Manager" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
        </div>
        <asp:SqlDataSource ID="SqlDataSourceRegisteration" runat="server" ConnectionString="<%$ ConnectionStrings:RegisterConnectionString %>" SelectCommand="SELECT * FROM [UserData]"></asp:SqlDataSource>
        <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" DataKeyNames="id" DataSourceID="SqlDataSourceRegisteration">
            <Columns>
                <asp:BoundField DataField="id" HeaderText="id" ReadOnly="True" SortExpression="id" />
                <asp:BoundField DataField="email" HeaderText="email" SortExpression="email" />
                <asp:BoundField DataField="name" HeaderText="name" SortExpression="name" />
                <asp:BoundField DataField="picture" HeaderText="picture" SortExpression="picture" />
                <asp:BoundField DataField="timestamp" HeaderText="timestamp" SortExpression="timestamp" />
            </Columns>
        </asp:GridView>
    </form>
</body>
</html>
