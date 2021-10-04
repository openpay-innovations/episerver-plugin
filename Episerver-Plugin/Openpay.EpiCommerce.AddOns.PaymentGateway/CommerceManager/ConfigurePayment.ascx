<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ConfigurePayment.ascx.cs" Inherits="Openpay.EpiCommerce.AddOns.PaymentGateway.CommerceManager.ConfigurePayment" %>
<style type="text/css">
    .RadioButtonWidth label {  margin-left:5px; } 
    .RadioButtonWidth td { padding-right: 25px !important; }
</style>
<div id="DataForm">
    <table cellpadding="0" cellspacing="2">
        <tr>
            <td class="FormLabelCell" colspan="2">
                <b><asp:Literal ID="Literal1" runat="server" Text="Configure Openpay" /></b>
            </td>
        </tr>
    </table>
    <br />
    <b><asp:Literal ID="SuccessMessage" runat="server"  Text=""/></b>
    <b style="color: red"><asp:Literal ID="ErrorMessage" runat="server"  Text=""/></b>
    <table class="DataForm">
        <tr>
            <td class="FormLabelCell">
                <asp:Literal ID="Literal9" runat="server" Text="Region" /> :</td>
            <td class="FormFieldCell">
                <asp:DropDownList ID="Region" runat="server">
                </asp:DropDownList>
                <asp:RequiredFieldValidator ControlToValidate="Region" Display="dynamic" Font-Name="verdana" Font-Size="9pt"
                                            ErrorMessage="Region required" runat="server" ID="Requiredfieldvalidator6"></asp:RequiredFieldValidator>
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell"></td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Literal ID="Literal2" runat="server" Text="Description" /> :</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="Description" Width="300px" MaxLength="250"></asp:TextBox><br />
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell"></td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Literal ID="Literal3" runat="server" Text="Environment" />:</td>
            <td class="FormFieldCell">
                <asp:RadioButtonList ID="Environment" runat="server" RepeatDirection="Horizontal" CssClass="RadioButtonWidth" >
                </asp:RadioButtonList>
                <asp:RequiredFieldValidator ControlToValidate="Environment" Display="dynamic" Font-Name="verdana" Font-Size="9pt"
                                            ErrorMessage="Environment required" runat="server" ID="Requiredfieldvalidator7"></asp:RequiredFieldValidator>
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell"></td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Literal ID="Literal4" runat="server" Text="Openpay Username" /> :</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="Username" Width="300px" MaxLength="250"></asp:TextBox><br />
                <asp:RequiredFieldValidator ControlToValidate="Username" Display="dynamic" Font-Name="verdana" Font-Size="9pt"
                    ErrorMessage="Username is required" runat="server" ID="Requiredfieldvalidator2"></asp:RequiredFieldValidator>
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Literal ID="Literal10" runat="server" Text="Openpay Password" /> :</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" ID="Password" Width="300px" MaxLength="250"></asp:TextBox><br />
                <asp:RequiredFieldValidator ControlToValidate="Password" Display="dynamic" Font-Name="verdana" Font-Size="9pt"
                                            ErrorMessage="Password required" runat="server" ID="Requiredfieldvalidator1"></asp:RequiredFieldValidator>
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell"></td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Literal ID="Literal7" runat="server" Text="Excluded Product Type Config Item ID" /> :</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" type="number" ID="ExcludedProductConfigItem" Width="300px" MaxLength="250"></asp:TextBox><br />
            </td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Literal ID="Literal8" runat="server" Text="Call Back Item ID" /> :</td>
            <td class="FormFieldCell">
                <asp:TextBox runat="server" type="number" ID="CallBackItemId" Width="300px" MaxLength="250"></asp:TextBox><br />
                <asp:RequiredFieldValidator ControlToValidate="CallBackItemId" Display="dynamic" Font-Name="verdana" Font-Size="9pt"
                                            ErrorMessage="CallBackItemId is required" runat="server" ID="Requiredfieldvalidator5"></asp:RequiredFieldValidator>
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell"></td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Literal ID="Literal5" runat="server" Text="Minimum Purchase Limit" /> :</td>
            <td class="FormFieldCell">
                <asp:TextBox type="number" runat="server" ID="MinPurchaseLimit" ReadOnly="true" Width="300px" MaxLength="250"></asp:TextBox><br />
                <asp:RequiredFieldValidator ControlToValidate="MinPurchaseLimit" Display="dynamic" Font-Name="verdana" Font-Size="9pt"
                    ErrorMessage="API Min Purchase Limit required" runat="server" ID="Requiredfieldvalidator3"></asp:RequiredFieldValidator>
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell"></td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Literal ID="Literal6" runat="server" Text="Maximum Purchase Limit" /> :</td>
            <td class="FormFieldCell">
                <asp:TextBox type="number" runat="server" ID="MaxPurchaseLimit" ReadOnly="true" Width="300px" MaxLength="250"></asp:TextBox><br />
                <asp:RequiredFieldValidator ControlToValidate="MaxPurchaseLimit" Display="dynamic" Font-Name="verdana" Font-Size="9pt"
                    ErrorMessage="API Max Purchase Limit required" runat="server" ID="Requiredfieldvalidator4"></asp:RequiredFieldValidator>
            </td>
        </tr>
        <tr>
            <td colspan="2" class="FormSpacerCell"></td>
        </tr>
        <tr>
            <td class="FormLabelCell">
                <asp:Literal ID="Literal11" runat="server" Text="Get Min/Max Values" />
            </td>
            <td class="FormLabelCell">
                <asp:Button ID="RunPurchaseLimitsApiBtn"
                            Text=" Run Min/Max!"
                            OnClick="RunPurchaseLimitsAPI"
                            runat="server" />
            </td>
        </tr>
    </table>
</div>
