Imports System.Web.SessionState

Public Class Global_asax
    Inherits System.Web.HttpApplication

    Sub Application_Start(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires when the application is started
    End Sub

    Sub Session_Start(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires when the session is started
    End Sub

    Sub Application_BeginRequest(ByVal sender As Object, ByVal e As EventArgs)

        ' Fires at the beginning of each request

        Try

            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*")

            If HttpContext.Current.Request.HttpMethod = "OPTIONS" Then
                HttpContext.Current.Response.AddHeader("Cache-Control", "no-cache")
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "POST, PUT, DELETE")
                HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept")
                HttpContext.Current.Response.AddHeader("Access-Control-Max-Age", "1728000")
                HttpContext.Current.Response.[End]()
            End If

        Catch ex As Exception
        End Try

    End Sub

    Sub Application_AuthenticateRequest(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires upon attempting to authenticate the use
    End Sub

    Sub Application_Error(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires when an error occurs
    End Sub

    Sub Session_End(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires when the session ends
    End Sub

    Sub Application_End(ByVal sender As Object, ByVal e As EventArgs)
        ' Fires when the application ends
    End Sub

End Class