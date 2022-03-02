Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib
Imports BluePrism.Common.Security
Imports BluePrism.AutomateProcessCore.My.Resources

''' Project  : AutomateAppCore
''' Class    : clsCredentialsBusinessObject
''' 
''' <summary>
''' This class represents the Credentials Internal Business Object
''' </summary>
Public Class clsCredentialsBusinessObject
    Inherits clsInternalBusinessObject

    ''' <summary>
    ''' The new constructor just creates the Internal Business Object Actions
    ''' </summary>
    ''' <param name="process">A reference to the process calling the object</param>
    ''' <param name="session">The session the object is running under</param>
    Public Sub New(ByVal process As clsProcess, ByVal session As clsSession)
        MyBase.New(process, session,
        "Blueprism.Automate.clsCredentialsActions", IboResources.clsCredentialsActions_Credentials)

        Narrative = IboResources.clsCredentialsActions_ThisInternalBusinessObjectProvidesTheAbilityForProcessesToInteractWithTheCreden
        AddAction(New clsCredentialsGet(Me))
        AddAction(New clsCredentialsSet(Me))
        AddAction(New clsCredentialsGenerateAndSet(Me))
        AddAction(New clsCredentialsList(Me))
        AddAction(New clsCredentialsGeneratePassword(Me))
        AddAction(New clsCredentialsGetProperty(Me))
        AddAction(New clsCredentialsMarkAsInvalid(Me))
        AddAction(New clsCredentialsSetProperty(Me))
    End Sub

    Public Overrides Function CheckLicense() As Boolean
        Return Licensing.License.CanUse(LicenseUse.Credentials)
    End Function

End Class
''' <summary>
''' Class to hold the parameter names as constants.
''' </summary>
Public Class Params

    Public Shared CredentialsName As String = NameOf(IboResources.clsCredentialsActions_Params_CredentialsName)
    Public Shared Username As String = NameOf(IboResources.clsCredentialsActions_Params_Username)
    Public Shared Password As String = NameOf(IboResources.clsCredentialsActions_Params_Password)
    Public Shared ExpiryDate As String = NameOf(IboResources.clsCredentialsActions_Params_ExpiryDate)
    Public Shared Status As String = NameOf(IboResources.clsCredentialsActions_Params_Status)

    Public Shared Length As String = NameOf(IboResources.clsCredentialsActions_Params_Length)
    Public Shared UseUpperCase As String = NameOf(IboResources.clsCredentialsActions_Params_UseUpperCase)
    Public Shared UseLowerCase As String = NameOf(IboResources.clsCredentialsActions_Params_UseLowerCase)

    Public Shared UseNumeric As String = NameOf(IboResources.clsCredentialsActions_Params_UseNumeric)
    Public Shared AdditionalCharacters As String = NameOf(IboResources.clsCredentialsActions_Params_AdditionalCharacters)
    Public Shared Credentials As String = NameOf(IboResources.clsCredentialsActions_Params_Credentials)

    Public Shared PropertyName As String = NameOf(IboResources.clsCredentialsActions_Params_PropertyName)
    Public Shared PropertyValue As String = NameOf(IboResources.clsCredentialsActions_Params_PropertyValue)

    Public Shared Function _T(ByVal param As String) As String
        Return IboResources.ResourceManager.GetString(param, New Globalization.CultureInfo("en"))
    End Function

End Class

''' Project  : AutomateAppCore
''' Class    : clsCredentialsGet
''' 
''' <summary>
''' Implements the "Get" action for the "Credentials" Internal Business Object.
''' </summary>
Public Class clsCredentialsGet
    Inherits clsInternalBusinessObjectAction

    ''' <summary>
    ''' Constructor - sets the details of the action.
    ''' </summary>
    Public Sub New(ByVal parent As clsInternalBusinessObject)
        MyBase.New(parent)
        SetName(NameOf(IboResources.clsCredentialsActions_Action_Get))
        SetNarrative(IboResources.clsCredentialsActions_clsCredentialsGet_GetTheSpecifiedSetOfCredentialsAnExceptionWillOccurIfAccessToTheCredentialsIsNo)

        AddParameter(Params.CredentialsName, DataType.text, ParamDirection.In,
         IboResources.clsCredentialsActions_clsCredentialsGet_TheNameOfTheCredentialsToGet)
        AddParameter(Params.Username, DataType.text, ParamDirection.Out,
         IboResources.clsCredentialsActions_clsCredentialsGet_TheUserName)
        AddParameter(Params.Password, DataType.password, ParamDirection.Out,
         IboResources.clsCredentialsActions_clsCredentialsGet_ThePassword)
        AddParameter(Params.ExpiryDate, DataType.date, ParamDirection.Out,
         IboResources.clsCredentialsActions_clsCredentialsGet_TheExpiryDate)
        AddParameter(Params.Status, DataType.text, ParamDirection.Out,
         IboResources.clsCredentialsActions_clsCredentialsGet_TheStatus)

    End Sub

    ''' <summary>
    ''' Get the endpoint text for this action.
    ''' </summary>
    ''' <returns>The endpoint as a String.</returns>
    Public Overrides Function GetEndpoint() As String
        Return IboResources.clsCredentialsActions_clsCredentialsGet_TheCredentialsAreReturned
    End Function

    ''' <summary>
    ''' Get the preconditions for this action.
    ''' </summary>
    ''' <returns>The preconditions as a Collection containing a String for each
    ''' precondition.</returns>
    Public Overrides Function GetPreConditions() As Collection
        Return BuildCollection(
         IboResources.clsCredentialsActions_clsCredentialsGet_ACredentialsKeyMustBeDefinedWithinThisBluePrismEnvironment,
         IboResources.clsCredentialsActions_clsCredentialsGet_CredentialsWithTheSpecifiedNameMustExist,
         IboResources.clsCredentialsActions_clsCredentialsGet_TheCredentialsMustBeAccessibleToTheRunningUserProcessAndResource)
    End Function

    ''' <summary>
    ''' Perform the action.
    ''' </summary>
    ''' <param name="process">A reference to the process making the call, or Nothing
    ''' if unknown.</param>
    ''' <param name="sess">The session under which the call is being made.</param>
    ''' <param name="stg">The stage used to resolve the scope.</param>
    ''' <param name="sErr">On return, an error message if unsuccessful</param>
    ''' <returns>True if successful</returns>
    Public Overrides Function [Do](
     ByVal process As clsProcess, ByVal sess As clsSession,
     ByVal stg As clsProcessStage, ByRef sErr As String) As Boolean
        Dim name As String = CStr(Inputs.GetValue(Params._T(Params.CredentialsName)))
        If name = "" Then Return SendError(sErr, IboResources.clsCredentialsActions_clsCredentialsGet_CredentialsNameNotSpecified)

        ' Attempt to get the credential details
        Dim cred As clsCredential = Nothing
        Try
            cred = gSv.RequestCredential(sess.ID, name)
        Catch ex As Exception
            Return SendError(sErr, ex.Message)
        End Try

        ' Pack output parameters and return
        If cred.CurrentStatus = clsCredential.Status.Valid Then
            AddOutput(Params._T(Params.Username), cred.Username)
            AddOutput(Params._T(Params.Password), cred.Password)
            AddOutput(Params._T(Params.ExpiryDate), DataType.date, cred.ExpiryDate)
        Else
            AddOutput(Params._T(Params.Username), "")
            AddOutput(Params._T(Params.Password), New SafeString())
            AddOutput(Params._T(Params.ExpiryDate), DataType.date, Date.MinValue)
        End If
        AddOutput(Params._T(Params.Status), cred.CurrentStatus.ToString())

        Return True
    End Function

End Class

''' Project  : AutomateAppCore
''' Class    : clsCredentialsSet
''' 
''' <summary>
''' Implements the "Set" action for the "Credentials" Internal Business Object.
''' </summary>
Public Class clsCredentialsSet
    Inherits clsInternalBusinessObjectAction

    ''' <summary>
    ''' Constructor - sets the details of the action.
    ''' </summary>
    Public Sub New(ByVal parent As clsInternalBusinessObject)
        MyBase.New(parent)
        SetName(NameOf(IboResources.clsCredentialsActions_Action_Set))
        SetNarrative(IboResources.clsCredentialsActions_clsCredentialsSet_SetTheSpecifiedCredentialsAnExceptionWillOccurIfAccessToTheseCredentialsIsNotAl)

        AddParameter(Params.CredentialsName, DataType.text, ParamDirection.In,
         IboResources.clsCredentialsActions_clsCredentialsSet_TheNameOfTheCredentialsToSet)
        AddParameter(Params.Username, DataType.text, ParamDirection.In,
         IboResources.clsCredentialsActions_clsCredentialsSet_TheUserName)
        AddParameter(Params.Password, DataType.password, ParamDirection.In,
         IboResources.clsCredentialsActions_clsCredentialsSet_ThePassword)
        AddParameter(Params.ExpiryDate, DataType.date, ParamDirection.In,
         IboResources.clsCredentialsActions_clsCredentialsSet_TheExpiryDate)

    End Sub

    ''' <summary>
    ''' Perform the action.
    ''' </summary>
    ''' <param name="process">A reference to the process making the call, or Nothing
    ''' if unknown.</param>
    ''' <param name="sess">The session under which the call is being made.</param>
    ''' <param name="stg">The stage used to resolve the scope.</param>
    ''' <param name="sErr">On return, an error message if unsuccessful</param>
    ''' <returns>True if successful</returns>
    Public Overrides Function [Do](
     ByVal process As clsProcess, ByVal sess As clsSession,
     ByVal stg As clsProcessStage, ByRef sErr As String) As Boolean
        Dim credName As String = ""
        Dim username As String = ""
        Dim password As New SafeString()
        Dim expirydate As Date = Date.MinValue

        ' Unpack passed parameters
        For Each arg As clsArgument In Inputs
            Select Case arg.Name
                Case "Credentials Name" : credName = CStr(arg.Value)
                Case "Username" : username = CStr(arg.Value)
                Case "Password" : password = CType(arg.Value, SafeString)
                Case "Expiry Date" : expirydate = CDate(arg.Value)
            End Select
        Next

        ' Validate parameter values
        If credName = "" Then Return SendError(sErr,
         IboResources.clsCredentialsActions_clsCredentialsSet_CredentialsNameNotSpecified)
        If username.Length > 64 Then Return SendError(sErr,
         IboResources.clsCredentialsActions_clsCredentialsSet_UsernameLengthCannotBeGreaterThan64)
        If password.IsEmpty Then Return SendError(sErr,
         IboResources.clsCredentialsActions_clsCredentialsSet_PasswordLengthCannotBeZero)
        If expirydate <> Date.MinValue AndAlso expirydate < Date.Today Then Return _
            SendError(sErr, IboResources.clsCredentialsActions_clsCredentialsSet_ExpiryDateCannotBeInThePast)

        ' Attempt to update the credential (and set as valid)
        Try
            gSv.RequestCredentialSet(sess.ID, credName, username, password, expirydate)
        Catch ex As Exception
            Return SendError(sErr, ex.Message)
        End Try

        Return True
    End Function

    ''' <summary>
    ''' Get the endpoint text for this action.
    ''' </summary>
    ''' <returns>The endpoint as a String.</returns>
    Public Overrides Function GetEndpoint() As String
        Return IboResources.clsCredentialsActions_clsCredentialsSet_TheCredentialsWillHaveBeenUpdatedAndSetAsValid
    End Function

    ''' <summary>
    ''' Get the preconditions for this action.
    ''' </summary>
    ''' <returns>The preconditions as a Collection containing a String for each
    ''' precondition.</returns>
    Public Overrides Function GetPreConditions() As Collection
        Return BuildCollection(
         IboResources.clsCredentialsActions_clsCredentialsSet_ACredentialsKeyMustBeDefinedWithinThisBluePrismEnvironment,
         IboResources.clsCredentialsActions_clsCredentialsSet_CredentialsWithTheSpecifiedNameMustExist,
         IboResources.clsCredentialsActions_clsCredentialsSet_TheCredentialsMustBeAccessibleToTheRunningUserProcessAndResource)
    End Function
End Class

''' Project  : AutomateAppCore
''' Class    : clsCredentialsGenerateAndSet
''' 
''' <summary>
''' Implements the "Generate And Set" action for the "Credentials" Internal Business 
''' Object.
''' </summary>
Public Class clsCredentialsGenerateAndSet
    Inherits clsInternalBusinessObjectAction

    ''' <summary>
    ''' Constructor - sets the details of the action.
    ''' </summary>
    Public Sub New(ByVal parent As clsInternalBusinessObject)
        MyBase.New(parent)

        SetName(NameOf(IboResources.clsCredentialsActions_Action_GenerateandSet))
        SetNarrative(String.Format(IboResources.clsCredentialsActions_clsCredentialsGenerateAndSet_SetTheSpecifiedCredentialsWithARandomPassword00AnExceptionWillOccurIfAccessToTh, vbCrLf))

        AddParameter(Params.CredentialsName, DataType.text, ParamDirection.In,
         IboResources.clsCredentialsActions_clsCredentialsGenerateAndSet_TheNameOfTheCredentials)
        AddParameter(Params.Username, DataType.text, ParamDirection.In,
         IboResources.clsCredentialsActions_clsCredentialsGenerateAndSet_TheUserName)
        AddParameter(Params.ExpiryDate, DataType.date, ParamDirection.In,
         IboResources.clsCredentialsActions_clsCredentialsGenerateAndSet_TheExpiryDate)
        AddParameter(Params.Length, DataType.number, ParamDirection.In,
         IboResources.clsCredentialsActions_clsCredentialsGenerateAndSet_TheLengthOfThePasswordMustBeBetween1And20)
        AddParameter(Params.UseUpperCase, DataType.flag, ParamDirection.In,
         IboResources.clsCredentialsActions_clsCredentialsGenerateAndSet_WhetherToUseUpperCaseLetters)
        AddParameter(Params.UseLowerCase, DataType.flag, ParamDirection.In,
         IboResources.clsCredentialsActions_clsCredentialsGenerateAndSet_WhetherToUseLowerCaseLetters)
        AddParameter(Params.UseNumeric, DataType.flag, ParamDirection.In,
         IboResources.clsCredentialsActions_clsCredentialsGenerateAndSet_WhetherToUseNumericCharacters)
        AddParameter(Params.AdditionalCharacters, DataType.text, ParamDirection.In,
         IboResources.clsCredentialsActions_clsCredentialsGenerateAndSet_AnyAdditionalCharactersOneOfWhichShouldBeUsedAtLeastOnceAllCharactersGivenInThi)
        AddParameter(Params.Password, DataType.password, ParamDirection.Out,
         IboResources.clsCredentialsActions_clsCredentialsGenerateAndSet_TheGeneratedPassword)

    End Sub

    ''' <summary>
    ''' Perform the action.
    ''' </summary>
    ''' <param name="process">A reference to the process making the call, or Nothing
    ''' if unknown.</param>
    ''' <param name="sess">The session under which the call is being made.</param>
    ''' <param name="scopestage">The stage used to resolve the scope.</param>
    ''' <param name="sErr">On return, an error message if unsuccessful</param>
    ''' <returns>True if successful</returns>
    Public Overrides Function [Do](
     ByVal process As clsProcess, ByVal sess As clsSession,
     ByVal ScopeStage As clsProcessStage, ByRef sErr As String) As Boolean
        Dim cred As String = ""
        Dim username As String = ""
        Dim password As SafeString
        Dim expirydate As Date = Date.MinValue
        Dim useUpper, useLower, useNumeric As Boolean
        Dim extras As String = ""
        Dim length As Integer

        ' Unpack passed parameters
        For Each arg As clsArgument In Inputs
            Select Case arg.Name
                Case "Credentials Name" : cred = CStr(arg.Value)
                Case "Username" : username = CStr(arg.Value)
                Case "Expiry Date" : expirydate = CDate(arg.Value)
                Case "Length" : length = CInt(arg.Value)
                Case "Use Lower Case" : useLower = CBool(arg.Value)
                Case "Use Upper Case" : useUpper = CBool(arg.Value)
                Case "Use Numeric" : useNumeric = CBool(arg.Value)
                Case "Additional Characters" : extras = CStr(arg.Value)
            End Select
        Next

        ' Validate parameter values
        If cred = "" Then Return SendError(sErr,
         IboResources.clsCredentialsActions_clsCredentialsGenerateAndSet_CredentialsNameNotSpecified)
        If username.Length > 64 Then Return SendError(sErr,
         IboResources.clsCredentialsActions_clsCredentialsGenerateAndSet_UsernameLengthCannotBeGreaterThan64)
        If expirydate <> Date.MinValue AndAlso expirydate < Date.Today Then _
            Return SendError(sErr, IboResources.clsCredentialsActions_clsCredentialsGenerateAndSet_ExpiryDateCannotBeInThePast)

        ' Attempt to generate new password
        Try
            password = clsCredential.GeneratePassword(
             length, useUpper, useLower, useNumeric, extras)
        Catch ex As Exception
            Return SendError(sErr,
             IboResources.clsCredentialsActions_clsCredentialsGenerateAndSet_CannotGeneratePassword0, ex.Message)
        End Try
        Debug.Assert(password.Length = length)

        ' Attempt to update credential (and set as valid)
        Try
            gSv.RequestCredentialSet(sess.ID, cred, username, password, expirydate)
        Catch ex As Exception
            Return SendError(sErr, ex.Message)
        End Try

        ' Pack output parameters and return
        AddOutput(Params._T(Params.Password), password)
        Return True
    End Function

    ''' <summary>
    ''' Get the endpoint text for this action.
    ''' </summary>
    ''' <returns>The endpoint as a String.</returns>
    Public Overrides Function GetEndpoint() As String
        Return IboResources.clsCredentialsActions_clsCredentialsGenerateAndSet_TheCredentialsWillHaveBeenUpdatedAndSetAsValidAndTheNewlyGeneratedPasswordRetur
    End Function

    ''' <summary>
    ''' Get the preconditions for this action.
    ''' </summary>
    ''' <returns>The preconditions as a Collection containing a String for each
    ''' precondition.</returns>
    Public Overrides Function GetPreConditions() As Collection
        Return BuildCollection(
         IboResources.clsCredentialsActions_clsCredentialsGenerateAndSet_ACredentialsKeyMustBeDefinedWithinThisBluePrismEnvironment,
         IboResources.clsCredentialsActions_clsCredentialsGenerateAndSet_CredentialsWithTheSpecifiedNameMustExist,
         IboResources.clsCredentialsActions_clsCredentialsGenerateAndSet_TheCredentialsMustBeAccessibleToTheRunningUserProcessAndResource)
    End Function
End Class

''' Project  : AutomateAppCore
''' Class    : clsCredentialsGeneratePassword
''' 
''' <summary>
''' Implements the "Generate Password" action for the "Credentials" Internal Business 
''' Object.
''' </summary>
Public Class clsCredentialsGeneratePassword
    Inherits clsInternalBusinessObjectAction

    ''' <summary>
    ''' Constructor - sets the details of the action.
    ''' </summary>
    Public Sub New(ByVal parent As clsInternalBusinessObject)
        MyBase.New(parent)

        SetName(NameOf(IboResources.clsCredentialsActions_Action_GeneratePassword))
        SetNarrative(IboResources.clsCredentialsActions_clsCredentialsGeneratePassword_GenerateAndReturnARandomPassword)

        AddParameter(Params.Length, DataType.number, ParamDirection.In,
         IboResources.clsCredentialsActions_clsCredentialsGeneratePassword_TheLengthOfThePasswordMustBeBetween1And20)
        AddParameter(Params.UseUpperCase, DataType.flag, ParamDirection.In,
         IboResources.clsCredentialsActions_clsCredentialsGeneratePassword_WhetherToUseUpperCaseLetters)
        AddParameter(Params.UseLowerCase, DataType.flag, ParamDirection.In,
         IboResources.clsCredentialsActions_clsCredentialsGeneratePassword_WhetherToUseLowerCaseLetters)
        AddParameter(Params.UseNumeric, DataType.flag, ParamDirection.In,
         IboResources.clsCredentialsActions_clsCredentialsGeneratePassword_WhetherToUseNumericCharacters)
        AddParameter(Params.AdditionalCharacters, DataType.text, ParamDirection.In,
         IboResources.clsCredentialsActions_clsCredentialsGeneratePassword_AnyAdditionalCharactersThatOneOfShouldBeUsedAtLeastOnce)
        AddParameter(Params.Password, DataType.password, ParamDirection.Out,
         IboResources.clsCredentialsActions_clsCredentialsGeneratePassword_TheGeneratedPassword)

    End Sub

    ''' <summary>
    ''' Get the endpoint text for this action.
    ''' </summary>
    ''' <returns>The endpoint as a String.</returns>
    Public Overrides Function GetEndpoint() As String
        Return IboResources.clsCredentialsActions_clsCredentialsGeneratePassword_TheGeneratedPasswordIsReturned
    End Function

    ''' <summary>
    ''' Get the preconditions for this action.
    ''' </summary>
    ''' <returns>The preconditions as a Collection containing a String for each
    ''' precondition.</returns>
    Public Overrides Function GetPreConditions() As Collection
        Return EmptyPreCondition()
    End Function

    ''' <summary>
    ''' Perform the action.
    ''' </summary>
    ''' <param name="process">A reference to the process making the call, or Nothing
    ''' if unknown.</param>
    ''' <param name="session">The session under which the call is being made.</param>
    ''' <param name="stg">The stage used to resolve the scope.</param>
    ''' <param name="sErr">On return, an error message if unsuccessful</param>
    ''' <returns>True if successful</returns>
    Public Overrides Function [Do](
     ByVal process As clsProcess, ByVal session As clsSession,
     ByVal stg As clsProcessStage, ByRef sErr As String) As Boolean
        Dim useUpper, useLower, useNumeric As Boolean
        Dim extras As String = ""
        Dim length As Integer

        ' Unpack passed parameters
        For Each arg As clsArgument In Inputs
            Select Case arg.Name
                Case "Length" : length = CInt(arg.Value)
                Case "Use Lower Case" : useLower = CBool(arg.Value)
                Case "Use Upper Case" : useUpper = CBool(arg.Value)
                Case "Use Numeric" : useNumeric = CBool(arg.Value)
                Case "Additional Characters" : extras = CStr(arg.Value)
            End Select
        Next

        ' Attempt to generate password
        Dim password As SafeString
        Try
            password = clsCredential.GeneratePassword(
             length, useUpper, useLower, useNumeric, extras)
        Catch ex As Exception
            Return SendError(sErr, IboResources.clsCredentialsActions_clsCredentialsGeneratePassword_CannotGeneratePassword0,
             ex.Message)
        End Try
        Debug.Assert(password.Length = length)

        ' Pack outputs and return
        AddOutput(Params._T(Params.Password), password)

        Return True
    End Function

End Class

''' Project  : AutomateAppCore
''' Class    : clsCredentialsList
''' 
''' <summary>
''' Implements the "List" action for the "Credentials" Internal Business Object.
''' </summary>
Public Class clsCredentialsList
    Inherits clsInternalBusinessObjectAction

    ''' <summary>
    ''' Constructor - sets the details of the action.
    ''' </summary>
    Public Sub New(ByVal parent As clsInternalBusinessObject)
        MyBase.New(parent)
        SetName(NameOf(IboResources.clsCredentialsActions_Action_List))
        SetNarrative(String.Format(IboResources.clsCredentialsActions_clsCredentialsList_ListsAvailableCredentials00IfNoStatusIsSpecifiedThenAllCredentialsWillBeReturned, vbCrLf))

        AddParameter(Params.Status, DataType.text, ParamDirection.In,
         IboResources.clsCredentialsActions_clsCredentialsList_CredentialStatusValidInvalidOrExpired)

        Dim ci As New clsCollectionInfo()
        ci.AddField("Credentials Name", DataType.text)
        ci.GetField("Credentials Name").Description = IboResources.clsCredentialsActions_clsCredentialsList_TheNameOfTheCredential
        ci.GetField("Credentials Name").DisplayName = IboResources.clsCredentialsActions_Params_CredentialsName

        ci.AddField("Description", DataType.text)
        ci.GetField("Description").Description = IboResources.clsCredentialsActions_clsCredentialsList_TheCredentialDescription
        ci.GetField("Description").DisplayName = IboResources.clsCredentialsActions_Params_Description

        ci.AddField("Expiry Date", DataType.date)
        ci.GetField("Expiry Date").Description = IboResources.clsCredentialsActions_clsCredentialsList_TheExpiryDate
        ci.GetField("Expiry Date").DisplayName = IboResources.clsCredentialsActions_Params_ExpiryDate

        ci.AddField("Status", DataType.text)
        ci.GetField("Status").Description = IboResources.clsCredentialsActions_clsCredentialsList_TheCredentialStatus
        ci.GetField("Status").DisplayName = IboResources.clsCredentialsActions_Params_Status

        AddParameter(Params.Credentials, ParamDirection.Out, ci,
         IboResources.clsCredentialsActions_clsCredentialsList_ACollectionOfCredentialsMatchingSpecifiedStatus)
    End Sub

    ''' <summary>
    ''' Get the endpoint text for this action.
    ''' </summary>
    ''' <returns>The endpoint as a String.</returns>
    Public Overrides Function GetEndpoint() As String
        Return IboResources.clsCredentialsActions_clsCredentialsList_ACollectionOfCredentialsMatchingTheSpecifiedStatusIsReturned
    End Function

    ''' <summary>
    ''' Get the preconditions for this action.
    ''' </summary>
    ''' <returns>The preconditions as a Collection containing a String for each
    ''' precondition.</returns>
    Public Overrides Function GetPreConditions() As Collection
        Return EmptyPreCondition()
    End Function

    ''' <summary>
    ''' Perform the action.
    ''' </summary>
    ''' <param name="process">A reference to the process making the call, or Nothing
    ''' if unknown.</param>
    ''' <param name="session">The session under which the call is being made.</param>
    ''' <param name="stg">The stage used to resolve the scope.</param>
    ''' <param name="sErr">On return, an error message if unsuccessful</param>
    ''' <returns>True if successful</returns>
    Public Overrides Function [Do](
     ByVal process As clsProcess, ByVal session As clsSession,
     ByVal stg As clsProcessStage, ByRef sErr As String) As Boolean
        Dim status As clsCredential.Status

        ' Unpack passed parameters
        For Each arg As clsArgument In Inputs
            Dim stat As String = Inputs.GetString(Params._T(Params.Status))
            If stat = "" _
             Then status = clsCredential.Status.All _
             Else If Not clsEnum.TryParse(CStr(arg.Value), True, status) Then _
              Return SendError(sErr, IboResources.clsCredentialsActions_clsCredentialsList_UnrecognisedCredentialStatus0, stat)
        Next

        ' Retrieve list of matching credentials
        Dim credList As ICollection(Of clsCredential)
        Try
            credList = gSv.RequestCredentialsList(session.ID, status)
        Catch ex As Exception
            Return SendError(sErr, ex.Message)
        End Try

        ' Pack output parameters and return
        Dim credentials As New clsCollection()
        For Each cred As clsCredential In credList
            Dim row As New clsCollectionRow()
            row.Add("Credentials Name", cred.Name)
            row.Add("Description", cred.Description)
            row.Add("Expiry Date", New clsProcessValue(DataType.date, cred.ExpiryDate))
            row.Add("Status", cred.CurrentStatus.ToString())
            credentials.Add(row)
        Next
        AddOutput(Params._T(Params.Credentials), credentials)

        Return True
    End Function

End Class

''' Project  : AutomateAppCore
''' Class    : clsCredentialsGetProperty
''' 
''' <summary>
''' Implements the "Get Property" action for the "Credentials" Internal Business Object.
''' </summary>
Public Class clsCredentialsGetProperty
    Inherits clsInternalBusinessObjectAction

    ''' <summary>
    ''' Constructor - sets the details of the action.
    ''' </summary>
    Public Sub New(ByVal parent As clsInternalBusinessObject)
        MyBase.New(parent)
        SetName(NameOf(IboResources.clsCredentialsActions_Action_GetProperty))
        SetNarrative(String.Format(IboResources.clsCredentialsActions_clsCredentialsGetProperty_ReturnsTheValueOfTheSpecifiedCredentialsProperty00AnExceptionWillOccurIfAccessT, vbCrLf))

        AddParameter(Params.CredentialsName, DataType.text, ParamDirection.In,
         IboResources.clsCredentialsActions_clsCredentialsGetProperty_TheNameOfTheCredentials)
        AddParameter(Params.PropertyName, DataType.text, ParamDirection.In,
         IboResources.clsCredentialsActions_clsCredentialsGetProperty_TheNameOfTheProperty)
        AddParameter(Params.PropertyValue, DataType.password, ParamDirection.Out,
         IboResources.clsCredentialsActions_clsCredentialsGetProperty_TheValueOfTheProperty)

    End Sub

    ''' <summary>
    ''' Get the endpoint text for this action.
    ''' </summary>
    ''' <returns>The endpoint as a String.</returns>
    Public Overrides Function GetEndpoint() As String
        Return IboResources.clsCredentialsActions_clsCredentialsGetProperty_TheValueOfTheRequestedPropertyIsReturned
    End Function

    ''' <summary>
    ''' Get the preconditions for this action.
    ''' </summary>
    ''' <returns>The preconditions as a Collection containing a String for each
    ''' precondition.</returns>
    Public Overrides Function GetPreConditions() As Collection
        Return BuildCollection(
         IboResources.clsCredentialsActions_clsCredentialsGetProperty_ACredentialsKeyMustBeDefinedWithinThisBluePrismEnvironment,
         IboResources.clsCredentialsActions_clsCredentialsGetProperty_CredentialsWithTheSpecifiedNameMustExist,
         IboResources.clsCredentialsActions_clsCredentialsGetProperty_TheCredentialsMustBeAccessibleToTheRunningUserProcessAndResource,
         IboResources.clsCredentialsActions_clsCredentialsGetProperty_APropertyWithTheSpecifiedNameMustBeAssociatedWithTheCredentials)
    End Function

    ''' <summary>
    ''' Perform the action.
    ''' </summary>
    ''' <param name="process">A reference to the process making the call, or Nothing
    ''' if unknown.</param>
    ''' <param name="session">The session under which the call is being made.</param>
    ''' <param name="scopestage">The stage used to resolve the scope.</param>
    ''' <param name="sErr">On return, an error message if unsuccessful</param>
    ''' <returns>True if successful</returns>
    Public Overrides Function [Do](ByVal process As clsProcess, ByVal session As clsSession,
     ByVal scopestage As clsProcessStage, ByRef sErr As String) As Boolean
        Dim credName As String = ""
        Dim propName As String = ""

        ' Unpack passed parameters
        For Each arg As clsArgument In Inputs
            Select Case arg.Name
                Case "Credentials Name" : credName = CStr(arg.Value)
                Case "Property Name" : propName = CStr(arg.Value)
            End Select
        Next

        ' Validate parameter values
        If credName = "" Then Return SendError(sErr,
         IboResources.clsCredentialsActions_clsCredentialsGetProperty_CredentialsNameNotSpecified)

        If propName = "" Then Return SendError(sErr,
         IboResources.clsCredentialsActions_clsCredentialsGetProperty_PropertyNameNotSpecified)

        ' Attempt to retrieve property for this credential
        Dim propValue As SafeString
        Try
            propValue = gSv.RequestCredentialProperty(session.ID, credName, propName)
        Catch ex As Exception
            Return SendError(sErr, ex.Message)
        End Try

        ' Pack output parameters and return
        AddOutput(Params._T(Params.PropertyValue), New clsProcessValue(propValue))

        Return True
    End Function

End Class

''' Project  : AutomateAppCore
''' Class    : clsCredentialsSetProperty
''' 
''' <summary>
''' Implements the "Get Property" action for the "Credentials" Internal Business Object.
''' </summary>
Public Class clsCredentialsSetProperty
    Inherits clsInternalBusinessObjectAction

    ''' <summary>
    ''' Constructor - sets the details of the action.
    ''' </summary>
    Public Sub New(ByVal parent As clsInternalBusinessObject)
        MyBase.New(parent)
        SetName(NameOf(IboResources.clsCredentialsActions_Action_SetProperty))
        SetNarrative(String.Format(IboResources.clsCredentialsActions_clsCredentialsSetProperty_SetsTheValueOfTheSpecifiedCredentialsProperty00AnExceptionWillOccurIfAccessToTh, vbCrLf))

        AddParameter(Params.CredentialsName, DataType.text, ParamDirection.In,
         IboResources.clsCredentialsActions_clsCredentialsSetProperty_TheNameOfTheCredentials)
        AddParameter(Params.PropertyName, DataType.text, ParamDirection.In,
         IboResources.clsCredentialsActions_clsCredentialsSetProperty_TheNameOfTheProperty)
        AddParameter(Params.PropertyValue, DataType.password, ParamDirection.In,
         IboResources.clsCredentialsActions_clsCredentialsSetProperty_TheValueOfTheProperty)

    End Sub

    ''' <summary>
    ''' Get the endpoint text for this action.
    ''' </summary>
    ''' <returns>The endpoint as a String.</returns>
    Public Overrides Function GetEndpoint() As String
        Return IboResources.clsCredentialsActions_clsCredentialsSetProperty_TheRequestedPropertyIsSetToTheSpecifiedValue
    End Function

    ''' <summary>
    ''' Get the preconditions for this action.
    ''' </summary>
    ''' <returns>The preconditions as a Collection containing a String for each
    ''' precondition.</returns>
    Public Overrides Function GetPreConditions() As Collection
        Return BuildCollection(
         IboResources.clsCredentialsActions_clsCredentialsSetProperty_ACredentialsKeyMustBeDefinedWithinThisBluePrismEnvironment,
         IboResources.clsCredentialsActions_clsCredentialsSetProperty_CredentialsWithTheSpecifiedNameMustExist,
         IboResources.clsCredentialsActions_clsCredentialsSetProperty_TheCredentialsMustBeAccessibleToTheRunningUserProcessAndResource)
    End Function

    ''' <summary>
    ''' Perform the action.
    ''' </summary>
    ''' <param name="process">A reference to the process making the call, or Nothing
    ''' if unknown.</param>
    ''' <param name="session">The session under which the call is being made.</param>
    ''' <param name="scopestage">The stage used to resolve the scope.</param>
    ''' <param name="sErr">On return, an error message if unsuccessful</param>
    ''' <returns>True if successful</returns>
    Public Overrides Function [Do](ByVal process As clsProcess, ByVal session As clsSession,
     ByVal scopestage As clsProcessStage, ByRef sErr As String) As Boolean
        Dim credName As String = ""
        Dim propName As String = ""
        Dim propValue As SafeString = Nothing

        ' Unpack passed parameters
        For Each arg As clsArgument In Inputs
            Select Case arg.Name
                Case "Credentials Name" : credName = CStr(arg.Value)
                Case "Property Name" : propName = CStr(arg.Value)
                Case "Property Value" : propValue = CType(arg.Value, SafeString)
            End Select
        Next

        ' Validate parameter values
        If credName = "" Then Return SendError(sErr,
         IboResources.clsCredentialsActions_clsCredentialsSetProperty_CredentialsNameNotSpecified)

        If propName = "" Then Return SendError(sErr,
         IboResources.clsCredentialsActions_clsCredentialsSetProperty_PropertyNameNotSpecified)

        If propValue Is Nothing Then Return SendError(sErr,
         IboResources.clsCredentialsActions_clsCredentialsSetProperty_PropertyValueNotSpecified)

        ' Attempt to set the property value for this credential
        Try
            gSv.RequestSetCredentialProperty(session.ID, credName, propName, propValue)

        Catch ex As Exception
            Return SendError(sErr, ex.Message)
        End Try

        Return True
    End Function

End Class

''' Project  : AutomateAppCore
''' Class    : clsCredentialsMarkAsInvalid
''' 
''' <summary>
''' Implements the "Mark as Invalid" action for the "Credentials" Internal Business
''' Object.
''' </summary>
Public Class clsCredentialsMarkAsInvalid
    Inherits clsInternalBusinessObjectAction

    ''' <summary>
    ''' Constructor - sets the details of the action.
    ''' </summary>
    Public Sub New(ByVal parent As clsInternalBusinessObject)
        MyBase.New(parent)
        SetName(NameOf(IboResources.clsCredentialsActions_Action_MarkasInvalid))
        SetNarrative(String.Format(IboResources.clsCredentialsActions_clsCredentialsMarkAsInvalid_SetsTheStatusOfTheseCredentialsToInvalid00AnExceptionWillOccurIfAccessToTheseCr, vbCrLf))

        AddParameter(Params.CredentialsName, DataType.text, ParamDirection.In,
         IboResources.clsCredentialsActions_clsCredentialsMarkAsInvalid_TheNameOfTheCredentials)

    End Sub

    ''' <summary>
    ''' Get the endpoint text for this action.
    ''' </summary>
    ''' <returns>The endpoint as a String.</returns>
    Public Overrides Function GetEndpoint() As String
        Return IboResources.clsCredentialsActions_clsCredentialsMarkAsInvalid_TheCredentialsAreMarkedAsInvalid
    End Function

    ''' <summary>
    ''' Get the preconditions for this action.
    ''' </summary>
    ''' <returns>The preconditions as a Collection containing a String for each
    ''' precondition.</returns>
    Public Overrides Function GetPreConditions() As Collection
        Return BuildCollection(
         IboResources.clsCredentialsActions_clsCredentialsMarkAsInvalid_ACredentialsKeyMustBeDefinedWithinThisBluePrismEnvironment,
         IboResources.clsCredentialsActions_clsCredentialsMarkAsInvalid_CredentialsWithTheSpecifiedNameMustExist,
         IboResources.clsCredentialsActions_clsCredentialsMarkAsInvalid_TheCredentialsMustBeAccessibleToTheRunningUserProcessAndResource)
    End Function

    ''' <summary>
    ''' Perform the action.
    ''' </summary>
    ''' <param name="process">A reference to the process making the call, or Nothing
    ''' if unknown.</param>
    ''' <param name="session">The session under which the call is being made.</param>
    ''' <param name="stg">The stage used to resolve the scope.</param>
    ''' <param name="sErr">On return, an error message if unsuccessful</param>
    ''' <returns>True if successful</returns>
    Public Overrides Function [Do](
     ByVal process As clsProcess, ByVal session As clsSession,
     ByVal stg As clsProcessStage, ByRef sErr As String) As Boolean

        ' Get the name arg and check that it's populated
        Dim name As String = Inputs.GetString(Params._T(Params.CredentialsName))
        If name = "" Then Return SendError(sErr, IboResources.clsCredentialsActions_clsCredentialsMarkAsInvalid_CredentialsNameNotSpecified)

        ' Attempt to invalidate the credential
        Try
            gSv.RequestCredentialInvalidated(session.ID, name)
        Catch ex As Exception
            Return SendError(sErr, ex.Message)
        End Try

        Return True
    End Function

End Class
