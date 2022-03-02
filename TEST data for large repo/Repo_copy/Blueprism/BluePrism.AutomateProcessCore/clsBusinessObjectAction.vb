Imports System.Text.RegularExpressions
Imports BluePrism.AutomateProcessCore.My.Resources

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsBusinessObjectAction
''' 
''' <summary>
''' A class representing a particular action on a Business Object.
''' Instances of this class are retrieved via clsBusinessObject - once
''' retrieved, information such as the parameters, preconditions etc.
''' can be retrieved.
''' </summary>
Public MustInherit Class clsBusinessObjectAction
    Implements IComparable

#Region " Static members and class constructor "

    ''' <summary>
    ''' A single-element collection for use in the GetPreConditions() implementations
    ''' that represent 'no preconditions'.
    ''' <strong>Note: </strong> This should be treated as read-only by subclasses -
    ''' unfortunately we can't make a VB collection read-only, so we have to go on
    ''' convention to ensure it's safely handled.
    ''' </summary>
    Protected Shared ReadOnly EmptyPreConditions As Collection = SingletonPreCondition("None")

    ''' <summary>
    ''' Helper method to retrieve the localised Empty Pre Condition element.
    ''' </summary>
    ''' <returns>A collection with a single element, that being the given
    ''' empty precondition.</returns>
    Protected Shared Function EmptyPreCondition() As Collection
        Dim c As New Collection()
        c.Add(My.Resources.Resources.clsBusinessObjectAction_EmptyPreConditions)
        Return c
    End Function

    ''' <summary>
    ''' Utility method to get a collection for pre-conditions containing a single
    ''' element.
    ''' </summary>
    ''' <param name="cond">The single precondition that the required collection
    ''' should contain.</param>
    ''' <returns>A collection with a single element, that being the given text
    ''' precondition.</returns>
    Protected Shared Function SingletonPreCondition(ByVal cond As String) As Collection
        Dim c As New Collection()
        c.Add(cond)
        Return c
    End Function

    ''' <summary>
    ''' Utility method to collate a number of lines into a collection. Useful when
    ''' specifying the preconditions in subclasses.
    ''' </summary>
    ''' <param name="lines">The lines, each of which will be represented by a
    ''' different element in the collection.</param>
    ''' <returns>A collection containing the given lines.</returns>
    Protected Shared Function BuildCollection(ByVal ParamArray lines() As String) As Collection
        Dim c As New Collection()
        For Each line As String In lines
            c.Add(line)
        Next
        Return c
    End Function

#End Region

    ''' <summary>
    ''' Provides access to localized friendlyname.
    ''' </summary>
    ''' <value></value>
    Public Property FriendlyName() As String
        Get
            Return If(mFriendlyName <> Nothing, mFriendlyName, mName)
        End Get
        Set(ByVal Value As String)
            mFriendlyName = Value
        End Set
    End Property
    ' A localized friendly name used in the UI.
    Private mFriendlyName As String

    Public Property DefaultLoggingInhibitMode() As LogInfo.InhibitModes

    ''' <summary>
    ''' Comparison function used for sorting. Comparison is alphabetically by name.
    ''' </summary>
    Public Function CompareTo(ByVal obj As Object) As Integer Implements IComparable.CompareTo
        Return mName.CompareTo(CType(obj, clsBusinessObjectAction).mName)
    End Function

    ''' <summary>
    ''' The name of this action
    ''' </summary>
    Private mName As String

    ''' <summary>
    ''' An array of clsProcessParameter objects representing
    ''' the parameters for this action.
    ''' </summary>
    Protected mParameters As IList(Of clsProcessParameter)

    ''' <summary>
    ''' Initializes a new instance of the clsBusinessObjectAction class. 
    ''' </summary>
    Public Sub New()
        mParameters = New List(Of clsProcessParameter)
        mName = My.Resources.Resources.clsBusinessObjectAction_NewAction
        DefaultLoggingInhibitMode = LogInfo.InhibitModes.OnSuccess
    End Sub

    ''' <summary>
    ''' Sets the name of the business object action.
    ''' </summary>
    ''' <param name="sName"></param>
    ''' <param name="arg"></param>
    Public Sub SetName(ByVal sName As String, ByVal arg As DataType)
        mName = sName
        mFriendlyName = sName
        Dim friendlyName = IboResources.ResourceManager.GetString(sName, New Globalization.CultureInfo("en"))
        If (friendlyName IsNot Nothing) Then
            mName = String.Format(friendlyName, clsProcessDataTypes.GetFriendlyName(arg, False, False))
            mFriendlyName = String.Format(IboResources.ResourceManager.GetString(sName), clsProcessDataTypes.GetFriendlyName(arg, False, True))
        End If
    End Sub

    ''' <summary>
    ''' Sets the name of the business object action.
    ''' </summary>
    ''' <param name="sName"></param>
    Public Sub SetName(ByVal sName As String)
        mName = sName
        mFriendlyName = sName
        Dim friendlyName = IboResources.ResourceManager.GetString(sName, New Globalization.CultureInfo("en"))
        If (friendlyName IsNot Nothing) Then
            mName = friendlyName
            mFriendlyName = IboResources.ResourceManager.GetString(sName)
        End If
    End Sub

    ''' <summary>
    ''' Gets the name of the business object action
    ''' </summary>
    ''' <returns></returns>
    Public Function GetName() As String
        GetName = mName
    End Function

    ''' <summary>
    ''' Gets the localized friendly name for Action IBO according to the current culture.
    ''' </summary>
    ''' <param name="actionName">The column name string</param>
    ''' <param name="iboType">The IBO namespace</param>
    ''' <param name="prefix">The item to retrieve, Action or Param</param>
    ''' <returns>The localised IBO Action or Param string for the current culture</returns>
    Public Shared Function GetLocalizedFriendlyName(actionName As String, iboType As String, prefix As String) As String
        If String.IsNullOrEmpty(iboType) Then Return actionName
        Dim localizedName As String
        Dim ibo As String = iboType.Substring(If(iboType.LastIndexOf(".") > 0, iboType.LastIndexOf(".") + 1, 0))
        Dim resxKey As String = ibo & "_" & prefix & "_" & Regex.Replace(actionName, "\s*", "")
        'Special handling needed for EncryptionBusinessObject_Action_Encrypt: Encrypt {0}
        If ibo.Equals("EncryptionBusinessObject") AndAlso prefix.Equals("Action") Then
            Dim dataTypeStr As String = actionName.Substring(If(actionName.LastIndexOf(" ") > 0, actionName.LastIndexOf(" ") + 1, 0))
            If clsProcessDataTypes.DataTypeNames.Contains(dataTypeStr.ToLower) Then
                Dim dataType As DataType = clsProcessDataTypes.Parse(dataTypeStr)
                resxKey = ibo & "_" & prefix & "_" & actionName.Substring(0, 7) 'Encrypt or Decrypt
                localizedName = IboResources.ResourceManager.GetString(resxKey)
                Return If(localizedName Is Nothing, actionName, String.Format(localizedName, clsDataTypeInfo.GetLocalizedFriendlyName(dataType)))
            End If
        End If
        localizedName = IboResources.ResourceManager.GetString(resxKey)
        Return If(localizedName Is Nothing, actionName, localizedName)
    End Function

    ''' <summary>
    ''' Get information about a particular parameter for this action.
    ''' </summary>
    ''' <param name="name">The parameter name.</param>
    ''' <param name="dir">The direction of the parameter.</param>
    ''' <returns>The clsProcessParameter describing the parameter requested, or
    ''' Nothing if there is no such parameter.</returns>
    Public Overridable Function GetParameter(ByVal name As String, ByVal dir As ParamDirection) As clsProcessParameter

        For Each p As clsProcessParameter In mParameters
            If p.Name = name AndAlso p.Direction = dir Then
                Return p
            End If
        Next
        Return Nothing

    End Function


    ''' <summary>
    ''' Get information about the parameters for this action
    ''' </summary>
    ''' <returns>An array of clsProcessParameter objects describing the parameters.
    ''' </returns>
    Public Overridable Function GetParameters() As IList(Of clsProcessParameter)
        GetParameters = mParameters
    End Function

    ''' <summary>
    ''' Add a parameter
    ''' </summary>
    ''' <param name="p">A clsProcessParameter object describing the paramter.</param>
    Public Sub AddParameter(ByVal p As clsProcessParameter)
        mParameters.Add(p)
    End Sub

    ''' <summary>
    ''' Shorthand way for subclasses to add a parameter with the specified values
    ''' </summary>
    ''' <param name="name">The name of the parameter</param>
    ''' <param name="type">The data type of the parameter</param>
    ''' <param name="dirn">The direction of the parameter</param>
    ''' <param name="desc">The narrative describing the parameter</param>
    ''' <returns>The parameter object which was created as a result of this call.
    ''' </returns>
    Protected Function AddParameter(
     ByVal name As String, ByVal type As DataType, ByVal dirn As ParamDirection, ByVal desc As String) _
     As clsProcessParameter
        Dim p As New clsProcessParameter(name, type, dirn, desc)
        AddParameter(p)
        Return p
    End Function

    ''' <summary>
    ''' Shorthand way for subclasses to add a parameter with the specified values
    ''' </summary>
    ''' <param name="name">The name of the parameter</param>
    ''' <param name="type">The data type of the parameter</param>
    ''' <param name="dirn">The direction of the parameter</param>
    ''' <param name="desc">The narrative describing the parameter</param>
    ''' <param name="validator">The validator for the parameter, can be null</param>
    ''' <returns></returns>
    Protected Function AddParameter(
     ByVal name As String, ByVal type As DataType, ByVal dirn As ParamDirection, ByVal desc As String, validator As IParameterValidation) As clsProcessParameter
        Dim p As New clsProcessParameter(name, type, dirn, desc, validator)
        AddParameter(p)
        Return p
    End Function



    ''' <summary>
    ''' Shorthand way for subclasses to add a parameter with the specified values
    ''' </summary>
    ''' <param name="name">The name of the parameter</param>
    ''' <param name="type">The data type of the parameter</param>
    ''' <param name="dirn">The direction of the parameter</param>
    ''' <param name="desc">The narrative, with formatting placeholders as defined in
    ''' <see cref="String.Format"/> describing the parameter</param>
    ''' <param name="args">The arguments for the narrative</param>
    ''' <returns>The parameter object which was created as a result of this call.
    ''' </returns>
    Protected Function AddParameter(ByVal name As String, ByVal type As DataType, _
     ByVal dirn As ParamDirection, ByVal desc As String, ByVal ParamArray args() As Object) _
     As clsProcessParameter
        Dim p As New clsProcessParameter(name, type, dirn, String.Format(desc, args))
        AddParameter(p)
        Return p
    End Function

    ''' <summary>
    ''' Shorthand way for subclasses to add a collection parameter with the specified
    ''' values
    ''' </summary>
    ''' <param name="name">The name of the collection parameter</param>
    ''' <param name="dirn">The direction of the parameter</param>
    ''' <param name="info">The collection info object describing the structure of
    ''' the collection parameter</param>
    ''' <param name="desc">The narrative describing the parameter</param>
    ''' <returns>The parameter object which was created as a result of this call.
    ''' </returns>
    Protected Function AddParameter( _
     ByVal name As String, _
     ByVal dirn As ParamDirection, _
     ByVal info As clsCollectionInfo, _
     ByVal desc As String) _
     As clsProcessParameter
        Dim p As clsProcessParameter = AddParameter(name, DataType.collection, dirn, desc)
        p.CollectionInfo = info
        Return p
    End Function



    ''' <summary>
    ''' Get a description of the endpoint for the action.
    ''' </summary>
    ''' <returns>A string containing the endpoint description</returns>
    Public MustOverride Function GetEndpoint() As String

    ''' <summary>
    ''' Get a description of the preconditions for the action.
    ''' </summary>
    ''' <returns>A Collection of strings, each describing an precondition
    ''' </returns>
    Public MustOverride Function GetPreConditions() As Collection

    ''' <summary>
    ''' Holds the narrative for the web service action.
    ''' </summary>
    Private msNarrative As String

    ''' <summary>
    ''' Get the narrative for the action
    ''' </summary>
    ''' <returns></returns>
    Public Function GetNarrative() As String
        Return msNarrative
    End Function

    ''' <summary>
    ''' Set the Narrative for the action
    ''' </summary>
    ''' <param name="sNarrative"></param>
    Public Sub SetNarrative(ByVal sNarrative As String)
        msNarrative = sNarrative
    End Sub

    ''' <summary>
    ''' Sets the narrative for this action
    ''' </summary>
    ''' <param name="narr">The narrative, with optional placeholders for arguments.
    ''' </param>
    ''' <param name="args">The arguments to insert into the given format string.
    ''' </param>
    Public Sub SetNarrative(ByVal narr As String, ByVal ParamArray args() As Object)
        SetNarrative(String.Format(narr, args))
    End Sub

End Class
