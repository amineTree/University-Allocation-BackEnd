Imports System.Data.SqlClient
Imports System.Web.Script.Serialization

' NOTE: You can use the "Rename" command on the context menu to change the class name "Service1" in code, svc and config file together.
' NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.vb at the Solution Explorer and start debugging.
Public Class DataService
    Implements IDataService

    Private lcConSql As String = System.Configuration.ConfigurationManager.ConnectionStrings("ConnectionString").ConnectionString()
    Private lcWebUrl As String = System.Web.Configuration.WebConfigurationManager.AppSettings.Item("WebsiteUrl")
    Private vacEmailProfile As String = System.Web.Configuration.WebConfigurationManager.AppSettings.Item("vacEmailProfile")
    Private hrEmailProfile As String = System.Web.Configuration.WebConfigurationManager.AppSettings.Item("hrEmailProfile")

    Private ErrorLogPath As String = System.Web.Configuration.WebConfigurationManager.AppSettings.Item("ErrorLogsPath")
    Private CompanyID As String = System.Web.Configuration.WebConfigurationManager.AppSettings.Item("CompanyID")

    '*********************************************
    '*** Special functions related to the app ****
    '*********************************************

    Public Function Dashboard() As String Implements IDataService.Dashboard

        Dim dt As DataTable
        Dim da As SqlDataAdapter
        Dim strsql As StringBuilder

        Try

            Dim serializer As New JavaScriptSerializer()
            serializer.MaxJsonLength = Int32.MaxValue

            strsql = New StringBuilder()
            Dim data As New Dictionary(Of String, Object)

            With strsql
                .Append("select room.room_ID as [id],class.class_ID, room.room_Name as [room], room.room_Capacity as [classCapacity], ")
                .Append("teacher.teacher_ID, teacher.teacher_Name+' '+teacher.teacher_familyName as [teacher], ")
                .Append("cours.cours_ID, cours.cours_Name as [course], room.room_Floor as [floor], ")
                .Append("schedule.date as [date], schedule.class_StartTime as [startTime], ")
                .Append("schedule.class_EndTime as [endTime], room.room_Status as [status] ")
                .Append("from room ")
                .Append("left join class on room.room_ID = class.room_ID ")
                .Append("left join teacher on class.teacher_ID  = teacher.teacher_ID  ")
                .Append("left join users on teacher.teacher_ID = users.teacher_ID ")
                .Append("inner join campus on room.room_Campus_ID = campus.campus_ID ")
                .Append("left join cours on class.cours_ID = cours.cours_ID ")
                .Append("left join class_Schedule on class.class_ID = class_Schedule.classC_ID ")
                .Append("left join schedule on class_Schedule.scheduleC_ID = schedule.schedule_ID ")
                ''''.Append("where teacher.teacher_ID = 1 ")
            End With

            da = New SqlDataAdapter(strsql.ToString(), lcConSql)

            dt = New DataTable()

            da.Fill(dt)

            If dt.Rows.Count > 0 Then

                Dim rows As New List(Of Dictionary(Of String, Object))()
                Dim row As Dictionary(Of String, Object)

                For Each dr As DataRow In dt.Rows

                    row = New Dictionary(Of String, Object)()

                    For Each col As DataColumn In dt.Columns
                        row.Add(col.ColumnName, dr(col))
                    Next col

                    rows.Add(row)

                Next dr

                data.Add("Dashboard", serializer.Serialize(rows))

            Else
                data.Add("Dashboard", Nothing)
            End If

            With strsql
                .Clear()
                .Append("select room_ID as [value], room_Name as [label] from room ")
            End With

            da.SelectCommand.CommandText = strsql.ToString
            dt = New DataTable()

            da.Fill(dt)

            If dt.Rows.Count > 0 Then

                Dim rows As New List(Of Dictionary(Of String, Object))()
                Dim row As Dictionary(Of String, Object)

                For Each dr As DataRow In dt.Rows

                    row = New Dictionary(Of String, Object)()

                    For Each col As DataColumn In dt.Columns
                        row.Add(col.ColumnName, dr(col))
                    Next col

                    rows.Add(row)

                Next dr

                data.Add("RoomsOption", serializer.Serialize(rows))

            Else
                data.Add("RoomsOption", Nothing)
            End If

            With strsql
                .Clear()
                .Append("select teacher_ID as [value], teacher_Name+' '+teacher_familyName as [label], ")
                .Append("teacher_Diploma as [diploma] from teacher ")
            End With

            da.SelectCommand.CommandText = strsql.ToString
            dt = New DataTable()

            da.Fill(dt)

            If dt.Rows.Count > 0 Then

                Dim rows As New List(Of Dictionary(Of String, Object))()
                Dim row As Dictionary(Of String, Object)

                For Each dr As DataRow In dt.Rows

                    row = New Dictionary(Of String, Object)()

                    For Each col As DataColumn In dt.Columns
                        row.Add(col.ColumnName, dr(col))
                    Next col

                    rows.Add(row)

                Next dr

                data.Add("TeachersOption", serializer.Serialize(rows))

            Else
                data.Add("TeachersOption", Nothing)
            End If

            With strsql
                .Clear()
                .Append("select cours_ID as [value], cours_Name+' ('+cours_Code+')' as [label] from cours ")
            End With

            da.SelectCommand.CommandText = strsql.ToString
            dt = New DataTable()

            da.Fill(dt)

            If dt.Rows.Count > 0 Then
                Dim rows As New List(Of Dictionary(Of String, Object))()
                Dim row As Dictionary(Of String, Object)

                For Each dr As DataRow In dt.Rows

                    Row = New Dictionary(Of String, Object)()

                    For Each col As DataColumn In dt.Columns
                        Row.Add(col.ColumnName, dr(col))
                    Next col

                    rows.Add(Row)

                Next dr

                data.Add("CoursesOption", serializer.Serialize(rows))

            Else
                data.Add("CoursesOption", Nothing)
            End If

            Return serializer.Serialize(data)
        Catch ex As Exception
            addErrorLog("Dashboard", ex.ToString, strsql.ToString)
            Throw ex
        Finally

            If Not dt Is Nothing Then
                dt.Dispose() : dt = Nothing
            End If

            If Not da Is Nothing Then
                da.Dispose() : da = Nothing
            End If

        End Try

    End Function

    Public Function LoadData(ByRef whichData As String) As String Implements IDataService.LoadData

        Dim dt As DataTable
        Dim da As SqlDataAdapter
        Dim strsql As StringBuilder

        Try

            strsql = New StringBuilder()

            If whichData = "courses" Then

                With strsql
                    .Append("select cours_ID,cours_Code,cours_Name,cours_Credit, ")
                    .Append("cours_Hours,cours_Semestre,cours_Status,cours_Price, ")
                    .Append("currency from cours ")
                End With

            ElseIf whichData = "teachers" Then

                With strsql
                    .Append("select teacher.teacher_ID, teacher.teacher_Code, teacher.teacher_Name,teacher.teacher_familyName, ")
                    .Append("users.user_ID,users.user_Name,users.user_Email,users.user_PhoneNumber,users.user_logo,users.user_Type,")
                    .Append("users.user_Status, teacher.teacher_Expertise,teacher.teacher_Diploma ")
                    .Append("from teacher ")
                    .Append("inner join users on teacher.teacher_ID = users.teacher_ID ")
                End With

            ElseIf whichData = "rooms" Then

                With strsql
                    .Append("select room_ID,room_Name,room_Capacity,room_Floor,room_Status ,room_Campus_ID, campus.campus_Name ")
                    .Append("from room ")
                    .Append("inner join campus on room.room_Campus_ID = campus.campus_ID ")
                End With

            End If

            da = New SqlDataAdapter(strsql.ToString(), lcConSql)

            dt = New DataTable()

            da.Fill(dt)

            If dt.Rows.Count > 0 Then

                Dim serializer As New JavaScriptSerializer()
                serializer.MaxJsonLength = Int32.MaxValue

                Dim rows As New List(Of Dictionary(Of String, Object))()
                Dim row As Dictionary(Of String, Object)

                For Each dr As DataRow In dt.Rows

                    row = New Dictionary(Of String, Object)()

                    For Each col As DataColumn In dt.Columns
                        row.Add(col.ColumnName, dr(col))
                    Next col

                    rows.Add(row)

                Next dr

                Return serializer.Serialize(rows)

            Else
                Return Nothing
            End If

        Catch ex As Exception
            addErrorLog("LoadData", ex.ToString, strsql.ToString)
            Throw ex
        Finally

            If Not dt Is Nothing Then
                dt.Dispose() : dt = Nothing
            End If

            If Not da Is Nothing Then
                da.Dispose() : da = Nothing
            End If

        End Try

    End Function

    '''' START SAVING ''''
    Public Function SaveRoom(ByRef roomID As String, ByRef roomName As String, ByRef roomCapacity As String,
                             ByRef roomFloor As String, ByRef roomStatus As String, ByRef roomCampusID As String) As String Implements IDataService.SaveRoom

        Dim strsql As StringBuilder
        Dim cmSql As SqlCommand
        Dim cnSql As SqlConnection

        Try

            strsql = New StringBuilder()
            cnSql = New SqlConnection(lcConSql)
            cnSql.Open()

            With strsql
                    .Append("Select top 1 room_ID from room order by room_ID Desc")
                End With
                cmSql = New SqlCommand(strsql.ToString, cnSql)
            roomID = cmSql.ExecuteScalar() + 1

            With strsql
                .Clear()
                .Append("select top 1 campus_ID  from campus")
            End With
            cmSql.CommandText = strsql.ToString()
            roomCampusID = cmSql.ExecuteScalar()

            With strsql
                .Clear()
                .Append("insert into room values ('" & roomID & "','" & roomName & "','" & roomCapacity & "', ")
                .Append("'" & roomFloor & "','" & roomStatus & "'," & roomCampusID & ")")
            End With
            cmSql.CommandText = strsql.ToString()
            If cmSql.ExecuteNonQuery() Then
                Return "success"
            Else
                Return Nothing
            End If

        Catch ex As Exception
            addErrorLog("SaveRoom", ex.ToString, strsql.ToString)
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

    End Function

    Public Function SaveCours(ByRef cours_ID As String, ByRef cours_Code As String, ByRef cours_Name As String,
                             ByRef cours_Credit As String, ByRef cours_Hours As String, ByRef cours_Semestre As String,
                              ByRef cours_Status As String, ByRef cours_Price As String, ByRef currency As String) As String Implements IDataService.SaveCours

        Dim strsql As StringBuilder
        Dim cmSql As SqlCommand
        Dim cnSql As SqlConnection

        Try

            strsql = New StringBuilder()
            cnSql = New SqlConnection(lcConSql)
            cnSql.Open()

            With strsql
                .Append("Select top 1 cours_ID from cours order by cours_ID Desc")
            End With
            cmSql = New SqlCommand(strsql.ToString, cnSql)
            cours_ID = cmSql.ExecuteScalar() + 1

            With strsql
                .Clear()
                .Append("insert into cours values ('" & cours_ID & "','" & cours_Code & "','" & cours_Name & "', ")
                .Append("'" & cours_Credit & "','" & cours_Hours & "','" & cours_Semestre & "', ")
                .Append("'" & cours_Status & "','" & cours_Price & "','" & currency & "')")
            End With
            cmSql.CommandText = strsql.ToString()
            If cmSql.ExecuteNonQuery() Then
                Return "Success"
            Else
                Return Nothing
            End If

        Catch ex As Exception
            addErrorLog("SaveCours", ex.ToString, strsql.ToString)
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

    End Function


    Public Function SaveTeacher(ByRef teacher_ID As String, ByRef teacher_Name As String, ByRef teacher_familyName As String,
                                ByRef teacher_Diploma As String, ByRef teacher_Address As String, ByRef teacher_Expertise As String,
                                ByRef teacher_Code As String, ByRef user_Name As String, ByRef user_PhoneNumber As String,
                                ByRef user_Email As String, ByRef user_Password As String, ByRef user_logo As String,
                                ByRef user_Type As String, ByRef user_Status As String) As String Implements IDataService.SaveTeacher

        Dim cmSql As SqlCommand
        Dim cnSql As SqlConnection
        Dim strsql As StringBuilder
        Dim trnSql As SqlTransaction
        Try

            Dim serializer As New JavaScriptSerializer()
            strsql = New StringBuilder()

            ''''username validation
            With strsql
                .Clear()
                .Append("select top 1 user_ID from users where user_Name = '" & user_Name & "' ")
            End With
            cnSql = New SqlConnection(lcConSql)
            cnSql.Open()
            cmSql = New SqlCommand(strsql.ToString, cnSql)

            If Not cmSql.ExecuteScalar() Is Nothing OrElse cmSql.ExecuteScalar() <> "" Then
                Return "Username already exists"
            End If

            '''' email validation
            With strsql
                .Clear()
                .Append("select top 1 user_ID from users where user_Email = '" & user_Email & "' ")
            End With
            cmSql.CommandText = strsql.ToString()

            If Not cmSql.ExecuteScalar() Is Nothing OrElse cmSql.ExecuteScalar() <> "" Then
                Return "Email already exists"
            End If

            ''''phone number validation
            With strsql
                .Clear()
                .Append("select top 1 user_ID from users where user_PhoneNumber = '" & user_PhoneNumber & "' ")
            End With
            cmSql.CommandText = strsql.ToString()

            If Not cmSql.ExecuteScalar() Is Nothing OrElse cmSql.ExecuteScalar() <> "" Then
                Return "Phone number already exists"
            End If

            trnSql = cnSql.BeginTransaction

            If teacher_ID Is Nothing OrElse teacher_ID = "" Then

                With strsql
                    .Clear()
                    .Append("select top 1 teacher_ID from teacher order by teacher_ID desc ")
                End With
                With cmSql
                    .CommandText = strsql.ToString()
                    .Transaction = trnSql
                End With

                teacher_ID = cmSql.ExecuteScalar() + 1

                With strsql
                    .Clear()
                    .Append("insert into teacher values (" & teacher_ID & ",'" & teacher_Code & "','" & teacher_Diploma & "', ")
                    .Append("'" & teacher_Expertise & "', '" & teacher_Name & "', '" & teacher_familyName & "', ")
                    .Append("'" & teacher_Address & "')")
                End With

                With cmSql
                    .CommandText = strsql.ToString()
                    .Transaction = trnSql
                    .ExecuteNonQuery()
                End With

                With strsql
                    .Clear()
                    .Append("insert into users values (" & teacher_ID & ",'" & teacher_ID & "','" & user_Name & "', ")
                    .Append("'" & user_Password & "', '" & user_Email & "', '" & user_PhoneNumber & "', ")
                    .Append("'" & user_logo & "','" & user_Type & "','" & user_Status & "')")
                End With

                With cmSql
                    .CommandText = strsql.ToString()
                    .Transaction = trnSql
                    .ExecuteNonQuery()
                End With

            Else
                ''''Update''''
            End If

            trnSql.Commit()

            Return "Successfully saved"

        Catch ex As Exception
            If Not trnSql Is Nothing AndAlso Not trnSql.Connection Is Nothing Then trnSql.Rollback()
            addErrorLog("SaveTeacher", strsql.ToString(), ex.ToString())
            Throw ex
        Finally

            If Not trnSql Is Nothing AndAlso Not trnSql.Connection Is Nothing Then
                trnSql.Rollback() : trnSql.Dispose() : trnSql = Nothing
            End If

            If Not cmSql Is Nothing Then
                cmSql.Dispose() : cmSql = Nothing
            End If

            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql.Dispose() : cnSql = Nothing
            End If

        End Try
    End Function

    Public Function SaveTeachersCoursesRooms(ByRef class_ID As String, ByRef room_ID As String, ByRef cours_ID As String,
                             ByRef teacher_ID As String, ByRef status As String, ByRef coursDate As String,
                              ByRef startTime As String, ByRef endTime As String) As String Implements IDataService.SaveTeachersCoursesRooms

        Dim strsql As StringBuilder
        Dim cmSql As SqlCommand
        Dim cnSql As SqlConnection
        Dim trnSql As SqlTransaction
        Dim schedule_ID As String
        Try

            strsql = New StringBuilder()
            cnSql = New SqlConnection(lcConSql)
            cnSql.Open()

            'Begin Transaction
            trnSql = cnSql.BeginTransaction

            If class_ID Is Nothing OrElse class_ID = "" Then

                With strsql
                    .Append("Select top 1 class_ID from class order by class_ID Desc")
                End With
                cmSql = New SqlCommand(strsql.ToString, cnSql, trnSql)
                class_ID = cmSql.ExecuteScalar() + 1

            Else

                With strsql
                    .Clear()
                    .Append("Update class set room_ID = '" & room_ID & "', cours_ID = '" & cours_ID & "', ")
                    .Append("teacher_ID = '" & teacher_ID & "' where class_ID = '" & class_ID & "'")
                End With
                cmSql = New SqlCommand(strsql.ToString, cnSql, trnSql)
                cmSql.ExecuteNonQuery()

            End If

            With strsql
                .Clear()
                .Append("insert into class values ('" & cours_ID & "','" & room_ID & "','" & cours_ID & "', ")
                .Append("'" & teacher_ID & "'")
            End With
            cmSql.CommandText = strsql.ToString()
            cmSql.ExecuteNonQuery()

            With strsql
                .Clear()
                .Append("select top 1 schedule_ID from schedule where ")
                .Append("class_StartTime = '" & startTime & "' and class_EndTime = '" & startTime & "' ")
                .Append("And date = '" & coursDate & "' ")
            End With
            cmSql.CommandText = strsql.ToString()
            schedule_ID = cmSql.ExecuteScalar()
            If schedule_ID Is Nothing OrElse schedule_ID = "" Then

                With strsql
                    .Clear()
                    .Append("select top 1 schedule_ID from schedule order by schedule_ID Desc")
                End With
                cmSql.CommandText = strsql.ToString()
                schedule_ID = cmSql.ExecuteScalar() + 1

                With strsql
                    .Clear()
                    .Append("insert into schedule values('" & schedule_ID & "','01','','" & startTime & "',")
                    .Append("'" & endTime & "','" & coursDate & "')")
                End With
                cmSql.CommandText = strsql.ToString()
                cmSql.ExecuteNonQuery()

            End If



            With strsql
                .Clear()
                .Append("insert into class_Schedule values ('" & class_ID & "', '" & schedule_ID & "'")
            End With
            cmSql.CommandText = strsql.ToString()
            cmSql.ExecuteNonQuery()

            trnSql.Commit()
            Return "success"

        Catch ex As Exception
            addErrorLog("SaveTeachersCoursesRooms", ex.ToString, strsql.ToString)
            If Not trnSql Is Nothing AndAlso Not trnSql.Connection Is Nothing Then trnSql.Rollback()
            Throw ex

        Finally

            If Not trnSql Is Nothing AndAlso Not trnSql.Connection Is Nothing Then
                trnSql.Rollback() : trnSql.Dispose() : trnSql = Nothing
            End If

            If Not cmSql Is Nothing Then
                cmSql.Dispose() : cmSql = Nothing
            End If

            If Not cnSql Is Nothing Then
                If cnSql.State = ConnectionState.Open Then cnSql.Close()
                cnSql.Dispose() : cnSql = Nothing
            End If
        End Try

    End Function


    '''' END SAVING ''''

    Private Sub addErrorLog(ByRef _FileName As String, ByRef _ErrorString As String, ByRef _ErrorStackTrace As String)
        Dim file As System.IO.StreamWriter

        Try

            If Not IO.Directory.Exists(ErrorLogPath) Then
                IO.Directory.CreateDirectory(ErrorLogPath)
            End If

            file = My.Computer.FileSystem.OpenTextFileWriter(ErrorLogPath & "\" & _FileName & ".txt", True)
            file.WriteLine(_ErrorString & vbCrLf & _ErrorStackTrace)
            file.Close()

        Catch ex As Exception

        End Try

    End Sub



End Class
