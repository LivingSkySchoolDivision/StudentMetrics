<%@ Page Title="" Language="C#" MasterPageFile="~/Templates/MainTemplate.master" AutoEventWireup="true" CodeFile="index.aspx.cs" Inherits="index" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="pageBody" Runat="Server">
    <h1>Student Data Charts</h1>
    <ul>
        <li><a href="/Charts/AverageAttendanceRates.aspx">Attendance Rates for last 30 days</a> (Test/Example chart)</li>
        <li><a href="/Charts/TargetAttendanceRates.aspx">Target Attendance Rate</a> % Students with at least x% attendance rate</li>
    </ul>
</asp:Content>

