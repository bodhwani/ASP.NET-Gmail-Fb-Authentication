<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Register.aspx.cs" Inherits="naah2.Register" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
        .auto-style1 {
            width: 100%;
        }
        .auto-style2 {
            width: 397px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <table class="auto-style1">
            <tr>
                <td class="auto-style2">id</td>
                <td>
                    <asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td class="auto-style2">email</td>
                <td>
                    <asp:TextBox ID="TextBox2" runat="server"></asp:TextBox>
                </td>
            </tr>
        </table>
        <p>
            <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="submit" />
        </p>
        <div>
        </div>
        <%--<asp:Button ID="btnGoogleLogin" runat="server" OnClick="Button2_Click" Text="Google Login" />--%>
    </form>
    <input type="button" id="btnGoogleLogin" value="Login with Google" class="btn btn-success" />
    
    <script src="Scripts/jquery-1.10.2.min.js"></script>
    <script type="text/javascript">
        $document.ready(fuction(){
            $('#btnGoogleLogin').click(function () {
                window.location.href = "/api/Account/ExternalLogin?provider=Google&response_type=token&client_id=self&redirect_uri=http%3A%2F%2Flocalhost%3A50462%2FManager.aspx&state=GerGr5JlYx4t_KpsK57GFSxVueteyBunu02xJTak5m01";
            });
        });
    </script>
</body>
    
</html>
