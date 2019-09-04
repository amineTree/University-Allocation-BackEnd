Imports System.Data.SqlClient
Imports System.Web.Script.Serialization

Public Class ServerSent
    Inherits System.Web.UI.Page

    Public Shared lcConSql As String = System.Configuration.ConfigurationManager.ConnectionStrings("ConnectionString").ConnectionString()
    Private ErrorLogPath As String = System.Web.Configuration.WebConfigurationManager.AppSettings.Item("ErrorLogsPath")

    Private Sub ServerSent_Load(sender As Object, e As EventArgs) Handles Me.Load
        Try

        Catch ex As Exception
            Throw ex
        End Try
    End Sub

End Class