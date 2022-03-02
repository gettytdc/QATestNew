Imports System.Xml
Imports System.Linq
Imports BluePrism.Utilities.Functional
Imports BluePrism.Core.Expressions
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.AutomateProcessCore.My.Resources
Imports System.Runtime.Serialization

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsProcessParameter
''' 
''' <summary>
''' A class representing a parameter within a process - both input and output
''' parameters are covered. This class is used for parameters in various scenarios.
''' </summary>
<Serializable, DataContract([Namespace]:="bp")>
Public Class clsProcessParameter
    Implements ICloneable

    ''' <summary>
    ''' The parameter direction localized friendly name
    ''' </summary>
    Public Shared Function GetLocalizedDirectionFriendlyName(type As ParamDirection) As String
        Return IboResources.ResourceManager.GetString($"clsProcessParameter_Direction_{type}")
    End Function

    ''' <summary>
    ''' The parameter direction - one of clsProcessParameter.Directions
    ''' </summary>
    Public Property Direction() As ParamDirection
        Get
            Return mDirection
        End Get
        Set(ByVal value As ParamDirection)
            mDirection = value
        End Set
    End Property
    <DataMember>
    Private mDirection As ParamDirection

    ''' <summary>
    ''' Provides access to localized friendlyname.
    ''' </summary>
    ''' <value></value>
    Public Property FriendlyName() As String
        Get
            Return If(mFriendlyName <> Nothing, mFriendlyName, Name)
        End Get
        Set(ByVal Value As String)
            mFriendlyName = Value
        End Set
    End Property
    <DataMember>
    Private mFriendlyName As String

    ''' <summary>
    ''' The parameter name.
    ''' </summary>
    Public Property Name() As String
        Get
            Return mName
        End Get
        Set(ByVal value As String)
            mName = value
        End Set
    End Property

    <DataMember>
    Private mName As String

    <DataMember>
    Private mMapType As MapType

    ''' <summary>
    ''' Mapping information, dependent on the value of msMapType.
    ''' For "Stage", it is a stage ID, which must be a stage of type "Data".
    ''' For "Const" it is a constant value.
    ''' </summary>
    <DataMember>
    Private mMap As String

    ''' <summary>
    ''' The data type
    ''' </summary>
    <DataMember>
    Private mDataType As DataType

    ''' <summary>
    ''' Parameter value validator, reference can be null
    ''' </summary>
    Private mParameterValidator As IParameterValidation

    ''' <summary>
    ''' The process this parameter belongs to, or Nothing if unknown/not relevant.
    ''' </summary>
    Public Property Process() As clsProcess
        Get
            Return mProcess
        End Get
        Set(ByVal value As clsProcess)
            mProcess = value
        End Set
    End Property
    <NonSerialized> ''TODO -do we want to serialize this?
    Private mProcess As clsProcess

    ''' <summary>
    ''' Gets the expression represented by this process parameter, or
    ''' <see cref="BPExpression.Empty"/> if it does not represent an expression.
    ''' </summary>
    Public ReadOnly Property Expression() As BPExpression
        Get
            If mMapType <> MapType.Expr Then Return BPExpression.Empty
            Return BPExpression.FromNormalised(mMap)
        End Get
    End Property

    Public Sub New()
        mMapType = MapType.None
        mName = ""
        mNarrative = ""
    End Sub

    ''' <summary>
    ''' Constructor which sets the name, data type and direction.
    ''' </summary>
    ''' <param name="name">The name of the parameter</param>
    ''' <param name="datatype">The datatype of the parameter</param>
    ''' <param name="direction">The direction of the parameter</param>
    Public Sub New(ByVal name As String, ByVal datatype As DataType, ByVal direction As ParamDirection)
        Me.New(name, datatype, direction, "")
    End Sub

    ''' <summary>
    ''' Additional constructor which, in addition to setting the name, data type and
    ''' direction, also allows the narrative to be set.
    ''' </summary>
    ''' <param name="name">The name of the parameter</param>
    ''' <param name="datatype">The datatype of the parameter</param>
    ''' <param name="direction">The direction of the parameter</param>
    ''' <param name="narrative">The narrative describing the parameter</param>
    Public Sub New(ByVal name As String, ByVal datatype As DataType,
     ByVal direction As ParamDirection, ByVal narrative As String, Optional ByVal sFriendlyName As String = Nothing)
        mMapType = MapType.None
        mName = name
        mFriendlyName = name
        mDataType = datatype
        mDirection = direction
        mNarrative = narrative
        If (sFriendlyName Is Nothing) Then
            Dim sName = IboResources.ResourceManager.GetString(name, New Globalization.CultureInfo("en"))
            If (sName IsNot Nothing) Then
                mFriendlyName = IboResources.ResourceManager.GetString(name)
                mName = sName
            End If
        Else
            mFriendlyName = sFriendlyName
        End If
    End Sub

    ''' <summary>
    ''' Additional constructor which, in addition to setting the name, data type and
    ''' direction, also allows the narrative to be set, and the value validator
    ''' </summary>
    ''' <param name="name">The name of the parameter</param>
    ''' <param name="datatype">The datatype of the parameter</param>
    ''' <param name="direction">The direction of the parameter</param>
    ''' <param name="narrative">The narrative describing the parameter</param>
    ''' <param name="validator">The value validator</param>
    ''' <param name="sFriendlyName">The display name</param>
    Public Sub New(ByVal name As String, ByVal datatype As DataType,
     ByVal direction As ParamDirection, ByVal narrative As String, validator As IParameterValidation, Optional ByVal sFriendlyName As String = Nothing)
        Me.New(name, datatype, direction, narrative, sFriendlyName)
        mParameterValidator = validator
    End Sub

    ''' <summary>
    ''' Set the mapping type.
    ''' </summary>
    ''' <param name="dtMap">The mapping type - see the
    ''' documentation in this file for msMapType</param>
    Public Sub SetMapType(ByVal dtMap As MapType)
        mMapType = dtMap
    End Sub

    ''' <summary>
    ''' Get mapping type.
    ''' </summary>
    ''' <returns>Mapping type - see the documentation in this file
    ''' for msMap</returns>
    Public Function GetMapType() As MapType
        GetMapType = mMapType
    End Function


    ''' <summary>
    ''' Set the mapping information.
    ''' </summary>
    ''' <param name="sMap">The mapping information - see the
    ''' documentation in this file for msMap</param>
    Public Sub SetMap(ByVal sMap As String)
        mMap = sMap
    End Sub

    ''' <summary>
    ''' Set the validator, value of null is valid 
    ''' </summary>
    ''' <param name="parameter"></param>
    Public Sub SetValidator(parameter As IParameterValidation)
        mParameterValidator = parameter
    End Sub

    ''' <summary>
    ''' Get mapping information.
    ''' </summary>
    ''' <returns>Mapping information</returns>
    Public Function GetMap() As String
        GetMap = mMap
    End Function

    ''' <summary>
    ''' Set the data type for this parameter.
    ''' </summary>
    ''' <param name="dtType">The new data type</param>
    Public Sub SetDataType(ByVal dtType As DataType)
        mDataType = dtType
    End Sub

    ''' <summary>
    ''' Get the data type for this parameter.
    ''' </summary>
    ''' <returns>The internal name for the data type, as defined
    ''' in clsProcessDataTypes.</returns>
    Public Function GetDataType() As DataType
        Return mDataType
    End Function

    ''' <summary>
    ''' The data type for the parameter.
    ''' </summary>
    Public Property ParamType() As DataType
        Get
            Return GetDataType()
        End Get
        Set(ByVal value As DataType)
            SetDataType(value)
        End Set
    End Property

    ''' <summary>
    ''' Gets the equivalent .NET type for this parameter, or null if there is no
    ''' equivalent.
    ''' </summary>
    Public ReadOnly Property NativeType() As System.Type
        Get
            Return clsProcessDataTypes.GetFrameworkEquivalentFromDataType(mDataType)
        End Get
    End Property

    ''' <summary>
    ''' The narrative for the parameter, i.e. a short description for documentation
    ''' purposes.
    ''' </summary>
    Public Property Narrative() As String
        Get
            Return mNarrative
        End Get
        Set(ByVal value As String)
            mNarrative = value
        End Set
    End Property

    Public Property Validator As IParameterValidation
        Get
            Return mParameterValidator
        End Get
        Set(value As IParameterValidation)
            mParameterValidator = value
        End Set

    End Property

    <DataMember>
    Public mNarrative As String


    ''' <summary>
    ''' Used for creating a copy of this object.
    ''' </summary>
    ''' <returns>A Copy of this clsProcessParameter Object</returns>
    Public Function Clone() As Object Implements System.ICloneable.Clone
        Dim copy As New clsProcessParameter()
        copy.Process = Process
        copy.Name = mName
        copy.FriendlyName = mFriendlyName
        copy.mNarrative = mNarrative
        copy.mMapType = mMapType
        copy.mMap = mMap
        copy.mDirection = mDirection
        copy.mDataType = mDataType
        copy.mCollectionInfo = mCollectionInfo
        copy.mParameterValidator = mParameterValidator?.Clone()
        Return copy
    End Function


    ''' <summary>
    ''' Provides access to information about a collection when the parameter is 
    ''' of datatype collection.
    ''' </summary>
    Public Property CollectionInfo() As clsCollectionInfo
        Get
            ' If this represents a collection, ensure that the definition exists.
            If mCollectionInfo Is Nothing AndAlso Me.ParamType = DataType.collection Then
                mCollectionInfo = New clsCollectionInfo()
            End If
            Return mCollectionInfo
        End Get
        Set(ByVal Value As clsCollectionInfo)
            mCollectionInfo = Value
        End Set
    End Property
    <DataMember>
    Private mCollectionInfo As clsCollectionInfo

    ''' <summary>
    ''' Checks if this object represents a parameter with a defined collection, ie.
    ''' a collection with fields defined on it.
    ''' </summary>
    ''' <returns>True if this parameter is configured for a collection with defined
    ''' fields; False otherwise.</returns>
    Public Function HasDefinedCollection() As Boolean
        Return mDataType = DataType.collection AndAlso
         mCollectionInfo IsNot Nothing AndAlso mCollectionInfo.Count > 0
    End Function


    ''' <summary>
    ''' Generates an XML element representing this instance.
    ''' </summary>
    ''' <param name="parentDocument">The parent document with which to create the
    ''' element.</param>
    ''' <returns>Returns an XML element.</returns>
    Public Function ToXML(ByVal parentDocument As XmlDocument) As XmlElement
        Dim retval As XmlElement

        'Set direction
        If mDirection = ParamDirection.In Then
            retval = parentDocument.CreateElement("input")
        Else
            retval = parentDocument.CreateElement("output")
        End If

        'Set name, type
        retval.SetAttribute("type", Me.GetDataType().ToString)
        If mName <> "" Then
            retval.SetAttribute("name", mName)
        End If
        If mFriendlyName <> "" Then
            retval.SetAttribute("friendlyname", mFriendlyName)
        End If
        If mNarrative <> "" Then
            retval.SetAttribute("narrative", mNarrative)
        End If

        'Set map and maptype
        Dim map As String = Me.GetMap()
        Select Case Me.GetMapType
            Case MapType.Stage
                retval.SetAttribute("stage", map)
            Case MapType.Expr
                retval.SetAttribute("expr", map)
        End Select

        If Validator IsNot Nothing Then
            retval.AppendChild(Validator.ToXML(parentDocument))
        End If

        Return retval
    End Function

    ''' <summary>
    ''' Returns a clsProcessParameter, as represented by the supplied xml.
    ''' </summary>
    ''' <param name="e">An XML node, with name "parameter", representing
    ''' a clsProcessParameter.</param>
    ''' <returns>Returns a new clsProcessParameter, or Nothing if the supplied XML
    ''' node is not suitable.</returns>
    Public Shared Function FromXML(ByVal e As XmlElement) As clsProcessParameter

        Dim mtype As MapType = MapType.None
        Dim mapping As String = ""
        Dim direction As ParamDirection
        Dim name As String = ""
        Dim friendlyname As String = ""
        Dim narrative As String = ""
        Dim datatype As DataType


        Select Case e.Name
            Case "input"
                direction = ParamDirection.In
            Case "output"
                direction = ParamDirection.Out
            Case Else
                Return Nothing
        End Select

        For Each a As XmlAttribute In e.Attributes
            Select Case a.Name
                Case "stage"
                    mtype = MapType.Stage
                    mapping = a.Value
                Case "expr"
                    mtype = MapType.Expr
                    mapping = a.Value
                Case "name"
                    name = a.Value
                Case "friendlyname"
                    friendlyname = a.Value
                Case "narrative"
                    narrative = a.Value
                Case "type"
                    Dim stype As String = a.Value
                    'Legacy data type no longer supported
                    If stype = "currency" Then stype = "number"
                    datatype = clsProcessDataTypes.DataTypeId(stype)

            End Select
        Next

        Dim validator As IParameterValidation = ObtainValidator(e)

        Dim p As New clsProcessParameter(name, datatype, direction, narrative, validator, friendlyname)
        p.SetMapType(mtype)
        p.SetMap(mapping)
        Return p

    End Function

    Private Shared Function ObtainValidator(e As XmlElement) As IParameterValidation
        Dim validator As IParameterValidation = Nothing
        For Each node As XmlNode In e.ChildNodes
            If node.Name = "validator" Then
                Dim theType = ""
                Dim param = ""
                For Each a As XmlAttribute In node.Attributes
                    Select Case a.Name
                        Case "type"
                            theType = a.Value
                        Case "parameter"
                            param = a.Value
                    End Select
                Next
                Dim t = Type.GetType(theType)
                validator = CType(Activator.CreateInstance(t), IParameterValidation)
                validator.Parameter = param
            End If
        Next

        Return validator
    End Function

    Protected Function UtilityStringUsedMessages() As String
        Dim loc As String = String.Empty
        Select Case mDirection
            Case ParamDirection.In
                loc = String.Format(My.Resources.Resources.clsProcessParameter_ForInputParameter0, mName)
            Case ParamDirection.Out
                loc = String.Format(My.Resources.Resources.clsProcessParameter_ForOutputParameter0, mName)
        End Select
        Return loc
    End Function

    ''' <summary>
    ''' Validates the parameter.
    ''' </summary>
    ''' <param name="parentStage">The stage owning this parameter. Used as the scope
    ''' stage when evaluating expressions.</param>
    ''' <param name="bAttemptRepair">Set to true to have errors automatically
    ''' repaired, where possible.</param>
    ''' <returns>A List of clsProcess.ValidateProcessResult objects containing
    ''' details of any errors found.</returns>
    Friend Function CheckForErrors(ByVal parentStage As clsProcessStage, ByVal bAttemptRepair As Boolean) As ValidationErrorList
        Dim errors As New ValidationErrorList

        'Utility string used in messages
        Dim loc As String = UtilityStringUsedMessages()

        'The maptype that we would expect, based on the nature of
        'the parameter (eg direction of parameter, and type of parent stage).
        'However, due to bugs which resulted in bad processes being saved,
        'this is not necessarily what we encounter in real life
        Dim expectedMapType As MapType = MapType.None

        'Determine the expected map type
        Dim st As StageTypes = parentStage.StageType
        Select Case st
            Case StageTypes.Action, StageTypes.Skill, StageTypes.Process, StageTypes.SubSheet, StageTypes.Code,
             StageTypes.Read, StageTypes.Write, StageTypes.Navigate
                If mDirection = ParamDirection.In Then
                    expectedMapType = MapType.Expr
                ElseIf mDirection = ParamDirection.Out Then
                    expectedMapType = MapType.Stage
                End If
            Case StageTypes.Start
                If mDirection = ParamDirection.In Then
                    expectedMapType = MapType.Stage
                End If
            Case StageTypes.End
                If mDirection = ParamDirection.Out Then
                    expectedMapType = MapType.Stage
                End If
        End Select

        'Check actual map type against expected map type, and perform
        'further validation as required
        Select Case expectedMapType
            Case MapType.None
                errors.Add(New ValidateProcessResult(parentStage, 31))

            Case MapType.Expr
                If GetMapType() <> MapType.Expr AndAlso GetMapType() <> MapType.None Then
                    If bAttemptRepair Then
                        SetMapType(MapType.Expr)
                        SetMap("[" & GetMap() & "]")
                    Else
                        errors.Add(New RepairableValidateProcessResult(parentStage, 33))
                    End If
                End If

                ' Validate the input expression
                Dim expr As String = GetMap()
                Dim exprInfo As New clsExpressionInfo()
                If expr = "" Then
                    errors.Add(
                     New ValidateProcessResult(parentStage, 136, loc))
                Else
                    errors.AddRange(clsExpression.CheckExpressionForErrors(
                     expr, parentStage, GetDataType(), loc, exprInfo, Nothing))

                    errors.AddRange(ValidateParameterWithinRange(parentStage, loc))
                    CheckWebApiActionInputCollectionErrors(parentStage, exprInfo, loc, errors)
                End If

            Case MapType.Stage
                If GetMapType() <> MapType.Stage And GetMapType() <> MapType.None Then
                    errors.Add(New ValidateProcessResult(parentStage, 38))
                End If

                'Need to determine if we're reading or writing. (i.e. this might be a 'get from'
                'rather than 'store in', which affects the checking slightly...
                Dim reading As Boolean = (
                 parentStage.StageType = StageTypes.End OrElse (
                 parentStage.StageType <> StageTypes.Start AndAlso mDirection = ParamDirection.In)
                )

                Dim colInfo As clsCollectionInfo = Nothing
                If HasDefinedCollection() Then colInfo = CollectionInfo

                errors.AddRange(Me.Process.CheckStoreInForErrors(
                 GetMap(), GetDataType(), parentStage, loc, colInfo, reading, True))

        End Select

        'If we're *defining* a parameter (as opposed to using one in a call) then
        'we should be providing a narrative...
        If st = StageTypes.Start OrElse st = StageTypes.End Then
            If mNarrative.Length = 0 Then
                errors.Add(New ValidateProcessResult(parentStage, 133, mName))
            End If
        End If

        Return errors
    End Function


    Protected Overridable Function ValidateParameterWithinRange(parentStage As clsProcessStage, loc As String) As IEnumerable(Of ValidateProcessResult)

        Dim errors = New List(Of ValidateProcessResult)()
        If mParameterValidator Is Nothing Then
            Return errors
        End If
        If mParameterValidator IsNot Nothing AndAlso
            Not mParameterValidator.Validate(GetMap) Then

            Dim number As Long
            Long.TryParse(GetMap(), number)
            errors.Add(New ValidateProcessResult(parentStage, 148, loc, number, mParameterValidator.Message))
        End If
        Return errors
    End Function

    Public Overridable Function ValidateParameter() As (Boolean, String)
        If mParameterValidator IsNot Nothing AndAlso
            Not mParameterValidator.Validate(GetMap) Then

            Return (False, mParameterValidator.Message)
        End If
        Return (True, String.Empty)
    End Function

    ''' <summary>
    ''' Validates the parameter, specifically checking that if the parameter is an
    ''' Input Parameter for a Web API Business Object Action and is a Collection 
    ''' input with a schema defined, then the schema of the collection data item
    ''' used to populate the input parameter can map into the input parameter's
    ''' schema. It also checks whether the data stage has a schema defined.
    ''' </summary>
    ''' <param name="parentStage">The stage owning this parameter</param>
    ''' <param name="expressionInfo">Expression Info for the process parameter</param>
    ''' <param name="location">The location of the error. This can either be an empty
    ''' string, or a description in the form, for example, " in row 1". Note the
    ''' leading space!</param>
    ''' <param name="errors">The list of validation errors to add the mismatch error to</param>
    ''' <remarks>
    ''' This has been implemented specifically to help customers attempting
    ''' to send multiple files via a Web API request ensure they have the correct
    ''' schema defined in their process that defines the collection of files.</remarks>
    Public Sub CheckWebApiActionInputCollectionErrors(parentStage As clsProcessStage,
                                                      expressionInfo As clsExpressionInfo,
                                                      location As String,
                                                      errors As ValidationErrorList)

        Dim webApiActionInput = parentStage.
                                    Map(Function(x) TryCast(x, clsActionStage))?.
                                    Map(Function(x) GetWebApiBusinessObjectAction(x))?.
                                    GetParameter(Name, ParamDirection.In)

        Dim hasSchemaToValidate = If(webApiActionInput?.CollectionInfo?.FieldDefinitions.Any(), False)
        If Not hasSchemaToValidate Then Return

        Dim collectionStage = expressionInfo.
                                        DataItems.
                                        Select(Function(x) Process.GetStage(x))?.
                                        OfType(Of clsCollectionStage)().
                                        FirstOrDefault()

        If collectionStage Is Nothing Then Return

        Dim collectionStageSchema = collectionStage.Definition

        If collectionStageSchema Is Nothing Then
            errors.Add(New ValidateProcessResult(parentStage, 145, location, String.Format(My.Resources.Resources.clsProcessParameter_0HasNoDefinedFields, collectionStage.Name)))
        Else
            Dim reasonForMismatch = String.Empty
            Dim stageMapsIntoInput = collectionStageSchema.
                                            CanMapInto(webApiActionInput.CollectionInfo,
                                                        reasonForMismatch,
                                                        $"'{collectionStage.Name}'",
                                                        String.Format(My.Resources.Resources.clsProcessParameter_TheInputParameter0, webApiActionInput.Name))

            If Not stageMapsIntoInput Then _
                errors.Add(New ValidateProcessResult(parentStage, 144, location, reasonForMismatch))

        End If

    End Sub

    ''' <summary>
    ''' Get the the Web API Business Object Action from an action stage. If the 
    ''' action stage does not reference a Web API Business object action then return
    ''' Nothing.
    ''' </summary>
    ''' <param name="actionStage">The action stage to check</param>
    ''' <returns>The Web API Business Object Action reference by an Action Stage</returns>
    Public Function GetWebApiBusinessObjectAction(actionStage As clsActionStage) As WebApiBusinessObjectAction
        Dim businessObject As clsBusinessObject
        Dim businessObjectName As String = Nothing, actionName As String = Nothing
        actionStage.GetResource(businessObjectName, actionName)

        businessObject = Process.GetBusinessObjectRef(businessObjectName)

        Dim webApiAction = TryCast(businessObject?.GetAction(actionName), WebApiBusinessObjectAction)

        Return webApiAction

    End Function

End Class
