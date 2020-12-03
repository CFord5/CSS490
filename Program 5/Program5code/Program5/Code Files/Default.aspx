<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Program5._Default" Async="true" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>BUILD YOUR OWN MOVIE DATABASE</h1>
    <p class="lead">CSS 490: Cloud Computing - Program 5</p>
    <br />
    <p class="lead">Store movie information from the OMDb API into your own database.</p>
    <br />
    <asp:Button ID="Button1" runat="server" Text="Store Movie From OMDb" OnCommand="Button1_Click" />
    <asp:TextBox ID="TextBox1" runat="server"></asp:TextBox>
    <br />
    <br />
    <asp:Label ID="Label3" runat="server" Text="JSON and Status Information."></asp:Label>
    <br />
    <asp:TextBox ID ="TextBox3" textmode="multiline" runat="server" style="OVERFLOW:auto; min-width:520px; height:200px;" Wrap = "true"></asp:TextBox>
    <video style="margin: auto; top: 0px; left:0px; bottom: 0px; right: 0px; max-width: 30%; max-height: 30%;" src="https://program5storageaccount.blob.core.windows.net/asset-93e4b7d0-7238-45cf-a314-49640efdd989/lets_all_go_to_the_lobby.mp4?sv=2017-04-17&sr=c&si=4d310bcb-5b36-4f80-b009-401ab883f0f8&sig=xaZQRJ9VvaisiaY0ZZ4NpVEeY%2Bty6VferIHxQP%2BbkiM%3D&st=2018-12-08T07%3A42%3A24Z&se=2118-12-08T07%3A42%3A24Z" controls=""></video>
    <br />
    <br />
    <asp:Label ID="Label1" runat="server" Text="Current Movies in Your Database."></asp:Label>
    <asp:Table ID="Table1" runat="server" BorderWidth ="1" GridLines="Both" BorderStyle="Solid" ></asp:Table>
    <br />
    <asp:Label ID="Label2" runat="server" Text="Enter title exactly as it appears in your database database and a target attribute from the JSON (i.e. Year, Runtime, Director)"></asp:Label>
    <br />
    <asp:TextBox ID="MovieTitleTextBox" placeholder="Enter movie title" runat="server"></asp:TextBox>
    <asp:TextBox ID="AttributeTextBox" placeholder="Enter movie attribute" runat="server"></asp:TextBox>
    <asp:Button ID="Button2" runat="server" Text="Search Attribute in Database" OnCommand="Button2_Click" />
    <asp:TextBox ID="ResultTextBox" runat="server" Wrap = "true"></asp:TextBox>
    <br />
    <br />
    <asp:Button ID="Button3" runat="server" Text="Clear Data" OnCommand="Button3_Click" />
</asp:Content>
