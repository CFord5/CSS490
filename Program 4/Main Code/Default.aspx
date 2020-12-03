<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Program4._Default" Async="true" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <h1>CSS 490: CLOUD COMPUTING</h1>
    <p class="lead">Program 4: Website + Storage</p>

    <asp:Button ID="Button1" runat="server" Text="Load Data" OnCommand="Button1_Click" />
    <asp:Button ID="Button2" runat="server" Text="Clear Data" OnCommand="Button2_Click" />
    <asp:Button ID="Button3" runat="server" Text="Query" OnCommand="Button3_Click" />
    <br />
    <br />
    <asp:Label ID="Label4" runat="server" Text="Status and Query Results: " Font-Bold ="true"></asp:Label>
    <br />
    <asp:TextBox ID ="TextBox3" textmode="multiline" runat="server" style="OVERFLOW:auto; min-width:520px; height:200px;" Wrap = "true"></asp:TextBox>
    <br />
    <br />
    <asp:Label ID="Label1" runat="server" Text="First Name" Font-Bold ="true"></asp:Label>
    <br />
    <asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>
    <br />
    <asp:Label ID="Label2" runat="server" Text="Last Name" Font-Bold ="true"></asp:Label>
    <br />
    <asp:TextBox ID="TextBox2" runat="server"></asp:TextBox>
</asp:Content>
