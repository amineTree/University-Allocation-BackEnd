Imports System.Data.SqlClient
Imports System.Web.Script.Serialization

Public Class VacAppReminder
    Inherits System.Web.UI.Page

    Private lcConSql As String = System.Configuration.ConfigurationManager.ConnectionStrings("ConnectionString").ConnectionString()
    Private ErrorLogPath As String = System.Web.Configuration.WebConfigurationManager.AppSettings.Item("ErrorLogsPath")
    Private lcWebUrl As String = System.Web.Configuration.WebConfigurationManager.AppSettings.Item("WebsiteUrl")

    Private Sub VacApprReminder_Load(sender As Object, e As EventArgs) Handles Me.Load
        Dim strsql As StringBuilder
        Dim dtCompanies, dtManagers, dtVacRequests As DataTable
        Dim da As SqlDataAdapter
        Dim distinctEmployees As ArrayList
        Dim finalObj As Dictionary(Of String, Object)
        Dim RemindBefore As Integer

        Try

            '1- get all companies available in the system
            '2- get all managers related to this company
            '3- get all vacation to approve related to the managers

            strsql = New StringBuilder()

            With strsql
                .Append("Select Set_Company, isNull(Set_VacReminderBefore, 0) as Set_VacReminderBefore from WebPay_Settings")
            End With

            dtCompanies = New DataTable
            da = New SqlDataAdapter(strsql.ToString, lcConSql)
            da.Fill(dtCompanies)

            For Each drComp In dtCompanies.Rows

                RemindBefore = drComp("Set_VacReminderBefore")

                If RemindBefore > 0 Then

                    With strsql
                        .Clear()
                        .Append("Select Pay01_Employees.Emp_RecID as [RecID], Emp_Nbr, ")
                        .Append("LTRIM(RTRIM(Pay01_Application.App_Firstname)) + ' ' +  LTRIM(RTRIM(Pay01_Application.App_family)) as [Name] ")
                        .Append("From pay02_VEmpAddOn ")
                        .Append("Inner Join Pay01_Employees on pay02_VEmpAddOn.VEmp_Emprecid = Pay01_Employees.Emp_RecID ")
                        .Append("Inner Join Pay01_Application on pay01_Employees.Emp_AppRecId = Pay01_Application.App_RecId ")
                        .Append("Where VEmp_Comp='" & drComp("Set_Company") & "' ")
                        .Append("and pay02_VEmpAddOn.VEmp_Manager = Pay01_Employees.Emp_RecID or pay02_VEmpAddOn.VEmp_Manager2 = Pay01_Employees.Emp_RecID ")
                    End With

                    dtManagers = New DataTable
                    da.SelectCommand.CommandText = strsql.ToString
                    da.Fill(dtManagers)

                    For Each drMan In dtManagers.Rows

                        With strsql
                            .Clear()
                            .Append("Select * from ( ")
                            .Append("Select min(VacID) as StartVacID, max(VacID) as EndVacID, ")
                            .Append("EmpID, ManualRef, Year, Type, AddDed, Sum(Pending) as [Pending], MonthNo, ApplicationDate, ")
                            .Append("min(EffectiveDate) as StartDate, max(EffectiveDate) as EndDate, count(EffectiveDate) as [NbrOfDays] from ( ")

                            .Append("Select vreq_recid as [VacID], Vreq_EmpRecid as [EmpID], Vreq_ManualRef as [ManualRef], Vreq_year as [Year], Vreq_type as [Type], Vreq_AddDed as [AddDed], vreq_effectiveDate as [EffectiveDate], ")
                            .Append("case when (VEmp_ANDOR = 'O' and isNull(Vreq_ApprovalDate, '') = '' and isNull(vreq_Rejectdate, '') = '') ")
                            .Append("or (VEmp_ANDOR = 'A' and isNull(vreq_Rejectdate, '') = '' and (isNull(Vreq_ApprovalDate, '') = '' or isNull(Vreq_ApprovalDate1, '') = '')) then 1 else 0 end Pending, ")
                            .Append("vreq_monthNo as [MonthNo], vreq_applicationDate as [ApplicationDate], Vemp_Manager as [Manager], VEmp_ANDOR as [AndOr], VEmp_Manager2 as [Manager2], ")
                            .Append("Vreq_ApprovalDate1 as [FirstApprovalDate], Vreq_ApprovalDate as [ApprovalDate], vreq_Rejectdate as [RejectDate] ")
                            .Append("from Pay01_VacRequests ")
                            .Append("inner join pay02_VEmpAddOn on Pay01_VacRequests.Vreq_EmpRecid = pay02_VEmpAddOn.VEmp_Emprecid ")
                            .Append("inner Join pay02_dedlink on Pay01_VacRequests.Vreq_type = pay02_dedlink.Dlnk_Vactype and pay02_dedlink.Dlnk_Comp = '" & drComp("Set_Company") & "' ")
                            .Append("where (VEmp_ANDOR = 'O' and isNull(Vreq_ApprovalDate, '') = '' and isNull(vreq_Rejectdate, '') = '') ")
                            .Append("or (VEmp_ANDOR = 'A' and isNull(vreq_Rejectdate, '') = '' and (isNull(Vreq_ApprovalDate, '') = '' or isNull(Vreq_ApprovalDate1, '') = '')) ")

                            .Append(")t ")
                            .Append("Where Pending > 0 and (Manager = '" & drMan("RecID") & "' or Manager2='" & drMan("RecID") & "') ")
                            .Append("Group By EmpID, ManualRef, Year, Type, AddDed, MonthNo, ApplicationDate ")

                            .Append(")finalTable ")
                            .Append("Where convert(varchar(10), DATEADD(day,-" & RemindBefore & ", StartDate), 126) = convert(varchar(10), getdate(), 126) ")
                            .Append("Order By EmpID, StartDate")
                        End With

                        dtVacRequests = New DataTable
                        da.SelectCommand.CommandText = strsql.ToString
                        da.Fill(dtVacRequests)

                        distinctEmployees = New ArrayList


                        For Each drVac In dtVacRequests.DefaultView.ToTable(True, "EmpID").Rows
                            distinctEmployees.Add(drVac("EmpID"))
                        Next drVac


                        finalObj = New Dictionary(Of String, Object)

                        For Each employee In distinctEmployees
                            createEmployeeRequest(finalObj, employee, dtVacRequests)
                        Next employee

                        SendEmailReminder(drMan("RecID"), finalObj, drComp("Set_Company"))

                    Next drMan

                End If

            Next drComp


        Catch ex As Exception
            addErrorLog(ex.ToString, strsql.ToString)
            Throw ex
        Finally

        End Try
    End Sub

    Private Sub createEmployeeRequest(ByRef finalObj As Dictionary(Of String, Object), ByRef employee As String, ByRef dt As DataTable)
        Dim row As Dictionary(Of String, Object)
        Dim rows As List(Of Dictionary(Of String, Object))

        Try

            rows = New List(Of Dictionary(Of String, Object))()

            For Each dr As DataRow In dt.Select("EmpID='" & employee & "'")

                row = New Dictionary(Of String, Object)()

                For Each col As DataColumn In dt.Columns
                    row.Add(col.ColumnName, dr(col))
                Next col

                rows.Add(row)

            Next dr

            finalObj.Add(employee, rows)

        Catch ex As Exception
            Throw ex
        End Try
    End Sub

    Private Sub SendEmailReminder(ByRef _ManagerRecID As String, ByRef _FinalObj As Dictionary(Of String, Object), ByRef _Company As String)
        Dim cmSql As SqlCommand
        Dim cnSql As SqlConnection
        Dim drsql As SqlDataReader
        Dim strsql, htmlPageStyle, body, dates As StringBuilder
        Dim Name, Email, Company As String

        Try

            strsql = New StringBuilder()

            With strsql
                .Append("Select LTrim(RTrim(Pay01_Application.App_Firstname)) + ' ' +  LTRIM(RTRIM(Pay01_Application.App_family)) as [name], Com_Folio.Fo_Email as [email], ")
                .Append("Com_Company.Comp_Name as [company] ")
                .Append("From pay02_VEmpAddOn ")
                .Append("Inner Join Pay01_Employees on pay02_VEmpAddOn.VEmp_Emprecid = Pay01_Employees.Emp_RecID ")
                .Append("Inner Join Pay01_Application on Pay01_Employees.Emp_AppRecId = Pay01_Application.App_RecId ")
                .Append("Inner Join Com_Folio on Pay01_Application.App_Folio1 = Com_Folio.Fo_Serno And Pay01_Application.App_Folio2 = Com_Folio.Fo_Subno ")
                .Append("Inner Join Com_Company on pay02_VEmpAddOn.VEmp_Comp = Com_Company.Comp_Code ")
                .Append("Where VEmp_Emprecid='" & _ManagerRecID & "' and pay02_VEmpAddOn.VEmp_Comp='" & _Company & "' ")
            End With

            cnSql = New SqlConnection(lcConSql)
            cnSql.Open()

            cmSql = New SqlCommand(strsql.ToString, cnSql)

            drsql = cmSql.ExecuteReader

            If drsql.Read Then

                Name = drsql("name")
                Email = drsql("email")
                Company = drsql("company")

                drsql.Close()


                body = New StringBuilder

                With body
                    .Append("Dear " & Name & ", <br/>")
                    .Append("please find below the pending vacation awaiting your approval.<br/><br/>")
                End With

                For Each employeeVacation In _FinalObj

                    dates = New StringBuilder

                    For Each vacDetails In employeeVacation.Value

                        If vacDetails("StartDate") = vacDetails("EndDate") Then
                            dates.Append(CDate(vacDetails("StartDate")).ToString("dd/MM/yyyy"))
                        Else
                            dates.Append(CDate(vacDetails("StartDate")).ToString("dd/MM/yyyy") & " -> " & CDate(vacDetails("EndDate")).ToString("dd/MM/yyyy") & " (Pending " & vacDetails("Pending") & " days)")
                        End If

                        dates.Append("<br/>")

                    Next vacDetails

                    With body
                        .Append("<b>" & getEmployeeName(employeeVacation.Key, _Company) & "</b>:<br/>")
                        .Append(dates.ToString & "<br/>")
                    End With

                Next employeeVacation


                htmlPageStyle = New StringBuilder()

                With htmlPageStyle
                    .Append("<!DOCTYPE html>")
                    .Append("<html>")
                    .Append("<head>")
                    .Append("<title></title>")
                    .Append("<meta charset='utf-8' />")
                    .Append("<style>body{font-size: 11pt;font-family:Calibri}</style>")
                    .Append("</head>")
                    .Append("<body>")
                    .Append(body)

                    .Append("Login <a href='" & lcWebUrl & "'>here</a><br/><br/>")
                    .Append("Regards,")

                    .Append("</body>")
                    .Append("</html>")
                End With

                With strsql
                    .Clear()
                    .Append("EXEC SendEmail '" & Email & "', null, 'Vacation Approval Reminder (" & Company & ")', '" & htmlPageStyle.ToString().Replace("'", "''") & "'")
                End With

                With cmSql
                    .CommandText = strsql.ToString() : .ExecuteNonQuery()
                End With

            End If

        Catch ex As Exception
            Throw ex
        Finally

            If Not cmSql Is Nothing Then
                cmSql.Dispose() : cmSql = Nothing
            End If

            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql.Dispose() : cnSql = Nothing
            End If
        End Try
    End Sub

    Private Function getEmployeeName(ByRef _EmpID As String, ByRef _Company As String) As String
        Dim strsql As StringBuilder
        Dim cnsql As SqlConnection
        Dim cmsql As SqlCommand

        Try

            strsql = New StringBuilder

            With strsql
                .Append("Select LTrim(RTrim(Pay01_Application.App_Firstname)) + ' ' +  LTRIM(RTRIM(Pay01_Application.App_family)) as [name] ")
                .Append("From pay02_VEmpAddOn ")
                .Append("Inner Join Pay01_Employees on pay02_VEmpAddOn.VEmp_Emprecid = Pay01_Employees.Emp_RecID ")
                .Append("Inner Join Pay01_Application on Pay01_Employees.Emp_AppRecId = Pay01_Application.App_RecId ")
                .Append("Inner Join Com_Company on pay02_VEmpAddOn.VEmp_Comp = Com_Company.Comp_Code ")
                .Append("Where VEmp_Emprecid='" & _EmpID & "' and pay02_VEmpAddOn.VEmp_Comp='" & _Company & "' ")
            End With

            cnsql = New SqlConnection(lcConSql)
            cnsql.Open()

            cmsql = New SqlCommand(strsql.ToString, cnsql)

            Return cmsql.ExecuteScalar

        Catch ex As Exception
            Throw ex
        Finally
            If Not cmsql Is Nothing Then
                cmsql.Dispose() : cmsql = Nothing
            End If

            If Not cnsql Is Nothing Then
                If cnsql.State = ConnectionState.Open Then cnsql.Close()
                cnsql.Dispose() : cnsql = Nothing
            End If
        End Try
    End Function

    Private Sub addErrorLog(ByRef _ErrorLine1 As String, ByRef _ErrorLine2 As String)
        Dim file As System.IO.StreamWriter

        Try

            If Not IO.Directory.Exists(ErrorLogPath) Then
                IO.Directory.CreateDirectory(ErrorLogPath)
            End If

            file = My.Computer.FileSystem.OpenTextFileWriter(ErrorLogPath & "\VacationApprovalReminder.txt", True)
            file.WriteLine(_ErrorLine1 & vbCrLf & _ErrorLine2 & vbCrLf & vbCrLf)
            file.Close()

        Catch ex As Exception

        End Try

    End Sub

End Class