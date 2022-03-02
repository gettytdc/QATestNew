
Imports System.Collections.Generic
Imports BluePrism.BPCoreLib.Collections
Imports My.Resources

Namespace BluePrism.ApplicationManager.AMI
    ''' Project  : AMI
    ''' Class    : clsActionTypeInfo
    ''' 
    ''' <summary>
    ''' This class is used to return information to the client about a particular
    ''' Action Type. It's also the base class for clsConditionTypeInfo.
    ''' 
    ''' Note that this class is functionally immutable - ie. once an instance of this
    ''' class is created, it cannot be modified externally.
    ''' </summary>
    ''' 
    Public Class clsActionTypeInfo
        Private mID As String
        Private mCommandId As String
        Private mName As String
        Private mHelpText As String
        Private mReqdAttrs As ICollection(Of String)
        Private mArguments As Dictionary(Of String, clsArgumentInfo)
        Private mOutputs As Dictionary(Of String, clsArgumentInfo)
        Private mRequiresFocus As Boolean
        Private mReturnType As String
        Private mDeprecatedChecker As Predicate(Of clsElementTypeInfo)

        ''' <summary>
        ''' The identifier for this Action Type. Except for cases handled specially in
        ''' clsAMI.DoAction() or actions with a different command ID, this ID is also
        ''' used as the query command when translating a DoAction request.
        ''' </summary>
        Public ReadOnly Property ID() As String
            Get
                Return mID
            End Get
        End Property

        ''' <summary>
        ''' Gets a delegate which can check if this action is deprecated for a given
        ''' element type.
        ''' </summary>
        Public Property DeprecatedChecker As Predicate(Of clsElementTypeInfo)
            Get
                If mDeprecatedChecker IsNot Nothing Then Return mDeprecatedChecker
                Return Function(e) False
            End Get
            Friend Set(value As Predicate(Of clsElementTypeInfo))
                mDeprecatedChecker = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the label to display for this action, based on the element that the
        ''' action is operating on.
        ''' </summary>
        ''' <param name="elem">The element on which this action is operating.</param>
        ''' <returns>The label to display for this action being performed on the given
        ''' element.</returns>
        Public Function GetLabel(elem As clsElementTypeInfo) As String
            If elem Is Nothing OrElse Not DeprecatedChecker(elem) Then Return Name
            Return String.Format(DeprecatedActionLabelTemplate, Name)
        End Function

        ''' <summary>
        ''' The ID to use as the query command for this action. This is typically the
        ''' same as <see cref="ID"/>, but may differ for certain actions.
        ''' </summary>
        Public ReadOnly Property CommandID() As String
            Get
                If mCommandId = "" Then Return mID
                Return mCommandId
            End Get
        End Property

        ''' <summary>
        ''' The name of this action type, which is in 'friendly' form for display to the
        ''' user.
        ''' </summary>
        Public ReadOnly Property Name() As String
            Get
                Return mName
            End Get
        End Property

        ''' <summary>
        '''Help text for the action.
        ''' to the user.
        ''' </summary>
        Public ReadOnly Property HelpText() As String
            Get
                Return mHelpText
            End Get
        End Property

        ''' <summary>
        ''' The arguments required for this action mapped against their IDs.
        ''' </summary>
        ''' <value>A collection of clsArgumentInfo objects, each describing one
        ''' of the required arguments.</value>
        Public ReadOnly Property Arguments() As IDictionary(Of String, clsArgumentInfo)
            Get
                Return GetReadOnly.IDictionary(mArguments)
            End Get
        End Property

        Public ReadOnly Property Outputs As IDictionary(Of String, clsArgumentInfo)
            Get
                Return GetReadOnly.IDictionary(mOutputs)
            End Get
        End Property

        ''' <summary>
        ''' True if the action requires the target application to be focussed (activated)
        ''' for it to succeed.
        ''' </summary>
        Public ReadOnly Property RequiresFocus() As Boolean
            Get
                Return mRequiresFocus
            End Get
        End Property

        ''' <summary>
        ''' The attributes that are required, if present, by this action. For example,
        ''' grid regions require that a grid schema is present for any grid-specific
        ''' actions.
        ''' </summary>
        Public ReadOnly Property RequiredAttributes() As ICollection(Of String)
            Get
                If mReqdAttrs Is Nothing Then Return GetEmpty.ICollection(Of String)()
                Return GetReadOnly.ICollection(mReqdAttrs)
            End Get
        End Property

        ''' <summary>
        ''' The return type of the action, if the action retrieves data.
        ''' </summary>
        ''' <value>The value must either be blank, or match one of the internal data type
        ''' names in Automate (eg 'number' or 'flag')</value>
        ''' <returns>The data type of the returned data, or a null/empty string if no
        ''' data is returned, or if the callee is free to interpret their own data
        ''' type.</returns>
        Public ReadOnly Property ReturnType() As String
            Get
                Return mReturnType
            End Get
        End Property

        ''' <summary>
        ''' Creates a new action with no return type
        ''' </summary>
        ''' <param name="id">The ID of the action</param>
        ''' <param name="commandId">The ID of the command to carry out when this action
        ''' is executed. Typically, this would be left as null to indicate that the
        ''' action ID is also the command ID</param>
        ''' <param name="name">The (display) name of the action</param>
        ''' <param name="helpText">Help text describing the action</param>
        ''' <param name="requiresFocus">True to indicate that this action requires the
        ''' target window to be focused, False if the action can be performed without
        ''' focus.</param>
        ''' <param name="reqdAttributes">The attributes which should be sent for this
        ''' action if the element in question has them available. These are typically
        ''' not element matching attributes like WindowText or X/Y location - more likely
        ''' they are metadata about the element for use in the operation being performed
        ''' - the schema of a grid region, or the font name specified in a region.
        ''' </param>
        ''' <param name="returnType">The type of data returned from the action, in string
        ''' form. An empty string indicates that no data is returned from this action
        ''' (and thus there is no data type)</param>
        ''' <param name="args">The arguments that this action requires</param>
        Friend Sub New(
         ByVal id As String, ByVal commandId As String, ByVal name As String,
         ByVal helpText As String, ByVal requiresFocus As Boolean,
         ByVal returnType As String, ByVal reqdAttributes As ICollection(Of String),
         ByVal ParamArray args() As clsArgumentInfo)
            mID = id
            mCommandId = commandId
            mName = name
            mHelpText = helpText

            Dim attrs As New clsSet(Of String)
            If reqdAttributes IsNot Nothing Then attrs.Union(reqdAttributes)
            mReqdAttrs = GetReadOnly.ICollection(attrs)

            mArguments = New Dictionary(Of String, clsArgumentInfo)
            mOutputs = New Dictionary(Of String, clsArgumentInfo)
            mRequiresFocus = requiresFocus
            mReturnType = returnType
            For Each arg As clsArgumentInfo In args
                AddArgument(arg)
            Next
        End Sub

        ''' <summary>
        ''' Add an argument. Only to be used when constructing the object.
        ''' </summary>
        ''' <param name="objArg">The argument to add.</param>
        Private Sub AddArgument(ByVal objArg As clsArgumentInfo)
            mArguments.Add(objArg.ID, objArg)
        End Sub

        Public Sub AddOutput(ByVal objArg As clsArgumentInfo)
            If Not mOutputs.ContainsKey(objArg.ID) Then
                mOutputs.Add(objArg.ID, objArg)
            Else
                mOutputs.Remove(objArg.ID)
                mOutputs.Add(objArg.ID, objArg)
            End If
        End Sub

        ''' <summary>
        ''' Creates a specific launch action from this action, adding application
        ''' parameters from the app info object, if one is given.
        ''' The returned action will not be part of AMI and the reference must be
        ''' maintained separately.
        ''' </summary>
        ''' <param name="app">The application info object which contains the specific
        ''' parameters for the action. Null will result in a clone of this action
        ''' being returned with no embellishments.</param>
        ''' <returns>A clone of this action with arguments representing the application
        ''' parameters found in the given application object appended to it.</returns>
        Public Function CreateSpecificLaunchAction(ByVal app As clsApplicationTypeInfo) _
         As clsActionTypeInfo

            'For design explanations, and in particular to see why we clone this member,
            'see http://cagney:8086/index.php?title=AMI_Runtime_Application_Parameters
            Dim copy As _
             New clsActionTypeInfo(mID, mCommandId, mName, mHelpText, mRequiresFocus,
              mReturnType, GetEmpty.ICollection(Of String))
            For Each arg As clsArgumentInfo In mArguments.Values
                copy.AddArgument(arg.Clone())
            Next
            ' If we have no application object (can that happen?)
            ' just return the action as is
            If app Is Nothing Then Return copy

            'Here we add whatever parameters are defined as part of the application
            'type. These appear in Automate's Application Modeller wizard, for example.
            For Each p As clsApplicationParameter In app.Parameters
                If p.Enabled Then copy.AddArgument(New clsArgumentInfo(
                 p.Name, p.FriendlyName, p.DataTypeEquivalent, p.HelpText, True))
            Next

            For Each p As clsArgumentInfo In app.Outputs
                copy.AddOutput(p)
            Next

            Return copy

        End Function

        Public Function CreateSpecificAttachAction(app As clsApplicationTypeInfo) As clsActionTypeInfo
            Dim copy As New clsActionTypeInfo(mID, mCommandId, mName, mHelpText, mRequiresFocus,
                                          mReturnType, GetEmpty.ICollection(Of String))

            For Each arg As clsArgumentInfo In mArguments.Values
                copy.AddArgument(arg.Clone())
            Next

            If app Is Nothing Then Return copy

            For Each p As clsArgumentInfo In app.Outputs
                copy.AddOutput(p)
            Next

            Return copy

        End Function

    End Class
End Namespace
