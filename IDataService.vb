' NOTE: You can use the "Rename" command on the context menu to change the interface name "IService1" in both code and config file together.
<ServiceContract()>
Public Interface IDataService

    '*********************************************
    '*** Special functions related to the app ****
    '*********************************************
    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function Dashboard() As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function LoadData(ByRef whichData As String) As String
    '''' START SAVE ''''
    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function SaveRoom(ByRef roomID As String, ByRef roomName As String, ByRef roomCapacity As String,
                      ByRef roomFloor As String, ByRef roomStatus As String, ByRef roomCampusID As String) As String


    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function SaveCours(ByRef cours_ID As String, ByRef cours_Code As String, ByRef cours_Name As String,
                             ByRef cours_Credit As String, ByRef cours_Hours As String, ByRef cours_Semestre As String,
                              ByRef cours_Status As String, ByRef cours_Price As String, ByRef currency As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function SaveTeacher(ByRef teacher_ID As String, ByRef teacher_Name As String, ByRef teacher_familyName As String,
                                ByRef teacher_Diploma As String, ByRef teacher_Address As String, ByRef teacher_Expertise As String,
                                ByRef teacher_Code As String, ByRef user_Name As String, ByRef user_PhoneNumber As String,
                                ByRef user_Email As String, ByRef user_Password As String, ByRef user_logo As String,
                                ByRef user_Type As String, ByRef user_Status As String) As String

    <OperationContract>
    <FaultContract(GetType(String))>
    <WebInvoke(Method:="POST", BodyStyle:=WebMessageBodyStyle.Wrapped, ResponseFormat:=WebMessageFormat.Json)>
    Function SaveTeachersCoursesRooms(ByRef class_ID As String, ByRef room_ID As String, ByRef cours_ID As String,
                             ByRef teacher_ID As String, ByRef status As String, ByRef coursDate As String,
                              ByRef startTime As String, ByRef endTime As String) As String
    '''' END SAVE ''''
End Interface
