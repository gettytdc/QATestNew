Imports System.Xml
Imports System.Drawing
Imports System.Runtime.Serialization
Imports BluePrism.Server.Domain.Models
Imports BluePrism.Common.Security
Imports BluePrism.BPCoreLib

Namespace Stages

    ''' Project  : AutomateProcessCore
    ''' Class    : AutomateProcessCore.Stages.clsDataStage
    ''' 
    ''' <summary>
    ''' This stage represents data items. Data items can store values and have an 
    ''' associated datatype that enforces the type of data that can be stored in the
    ''' data item.
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class clsDataStage
        Inherits clsProcessStage
        Implements IDataField

        ''' <summary>
        ''' This is the DataType of the data item.
        ''' </summary>
        <DataMember>
        Private mDataType As DataType

        ''' <summary>
        ''' The initial value for the data item.
        ''' </summary>
        <DataMember>
        Private mInitVal As clsProcessValue

        ''' <summary>
        ''' This is the current value for the data item.
        ''' </summary>
        <DataMember>
        Protected mValue As clsProcessValue

        <DataMember>
        Private mExposure As StageExposureType

        <DataMember>
        Private mPrivate As Boolean

        <DataMember>
        Protected mAlwaysInit As Boolean

        ''' <summary>
        ''' The fully qualified name of this field which can be used to recognise
        ''' it within an expression.
        ''' </summary>
        Public ReadOnly Property FullyQualifiedName() As String Implements IDataField.FullyQualifiedName
            Get
                Return GetName()
            End Get
        End Property

        ''' <summary>
        ''' The data type that this data stage represents.
        ''' </summary>
        Public Property DataType() As DataType Implements IDataField.DataType
            Get
                Return GetDataType()
            End Get
            Set(ByVal value As DataType)
                SetDataType(value)
            End Set
        End Property

        ''' <summary>
        ''' How the data item should be exposed.
        ''' </summary>
        Public Property Exposure() As StageExposureType
            Get
                Return mExposure
            End Get
            Set(ByVal value As StageExposureType)
                mExposure = value
            End Set
        End Property

        ''' <summary>
        ''' True if this data item is only visible within the page where it resides.
        ''' </summary>
        Public Property IsPrivate() As Boolean
            Get
                Return mPrivate
            End Get
            Set(ByVal value As Boolean)
                mPrivate = value
            End Set
        End Property

        ''' <summary>
        ''' True if this data item should always be initialised when the page it
        ''' resides on is executed. Obviously not relevant to data items on the main
        ''' page since that only executes once.
        ''' </summary>
        ''' <remarks>The default state when creating a new stage is True - however,
        ''' prior to V3, this property didn't exist and the default was therefore
        ''' False, and this is preserved when loading processes created prior to this
        ''' addition.
        ''' </remarks>
        Public Property AlwaysInit() As Boolean
            Get
                Return mAlwaysInit
            End Get
            Set(ByVal value As Boolean)
                mAlwaysInit = value
            End Set
        End Property

        ''' <summary>
        ''' Get the environment variable associated with this data item.
        ''' </summary>
        ''' <returns>The value of the environment variable represented by this stage.
        ''' </returns>
        ''' <exception cref="NoSuchElementException">If no environment variable was
        ''' found with the name defined in this stage.</exception>
        Private Function GetEnv() As clsProcessValue
            clsAPC.ProcessLoader.GetEnvVarSingle(GetName(), True)

            Dim var As clsArgument = Nothing
            If mParent.EnvVars.TryGetValue(GetName(), var) Then Return var.Value
            Throw New NoSuchElementException(My.Resources.Resources.clsDataStage_EnvironmentVariableNotDefined)
        End Function

        ''' <summary>
        ''' Set the initial value for this stage. Relevant only for data stages.
        ''' Throws an ApplicationException if the data type of the value
        ''' passed does not match that of the stage.
        ''' </summary>
        ''' <param name="val">The initial value, as a clsProcessValue</param>
        Public Sub SetInitialValue(ByVal val As clsProcessValue)
            If val.DataType <> DataType Then Throw New InvalidValueException(
             My.Resources.Resources.clsDataStage_AttemptToSetInitialValueOf01WithMismatchedDataTypeOf2, Name, DataType, val.DataType)
            mInitVal = val
        End Sub

        ''' <summary>
        ''' Get the initial value for this stage.
        ''' </summary>
        ''' <returns>The initial value as a clsProcessValue</returns>
        Public Overridable Function GetInitialValue() As clsProcessValue
            Return mInitVal
        End Function

        ''' <summary>
        ''' The initial value for this stage.
        ''' The return value from this data stage should never be null, though the
        ''' returned value may represent a 'null' value.
        ''' </summary>
        ''' <remarks>This is largely here to allow the initial value to be easily
        ''' seen in the VS Debugger - it delegates directly to the Get and Set
        ''' InitialValue() methods.</remarks>
        Public Property InitialValue() As clsProcessValue
            Get
                If mExposure = StageExposureType.Environment Then
                    Return GetEnv()
                End If
                Return GetInitialValue()
            End Get
            Set(ByVal value As clsProcessValue)
                If mExposure = StageExposureType.Environment Then
                    Throw New InvalidOperationException(My.Resources.Resources.clsDataStage_AttemptToSetInitialValueOnAnEnvironmentVariable)
                End If
                SetInitialValue(value)
            End Set
        End Property

        ''' <summary>
        ''' Gets the short value (&lt;1000 characters) of the stage when this stage
        ''' is a data item and when the value is of datatype text, otherwise just
        ''' returns the value unmodified, or if this stage is not even a data item
        ''' just returns an empty string.
        ''' </summary>
        ''' <param name="initValue">True to retrieve the 'short' version of the initial
        ''' value; False to retrieve the current value.
        ''' the current value</param>
        ''' <returns>A string representing the short version of the required value of
        ''' this data stage.</returns>
        Public Overrides Function GetShortValue(ByVal initValue As Boolean) As String

            Dim pval As clsProcessValue
            Dim sValue As String
            Try
                If initValue Then pval = InitialValue Else pval = GetValue()
                If pval Is Nothing Then Throw New BluePrismException(
                 My.Resources.Resources.clsDataStage_UnexpectedNullReferenceWhenAccessingStageSValueStage0Initialvalue1, GetName(), initValue)

                sValue = pval.FormattedValue

                'Bug 4810, item A). If by some other bug (such as in 4810 again)
                'password data item contains a clsProcessValue of text or or other,
                'we should make sure that the Short Value does not reveal the
                'plain text value.
                If GetDataType() = DataType.password _
                 AndAlso pval.DataType <> DataType.password Then
                    'PW. Rather than silently ignoring this problem at runtime,
                    'should we throw an exception?
                    Debug.Fail("Bug 4810 regression detected")
                    Try
                        sValue = pval.CastInto(DataType.password).FormattedValue
                    Catch
                        sValue = ""
                    End Try
                End If

                If pval.DataType = DataType.text AndAlso sValue.Length > 1000 Then
                    sValue = String.Format(My.Resources.Resources.clsDataStage_0Characters, sValue.Length)
                End If

            Catch ex As Exception
                sValue = String.Format(My.Resources.Resources.clsDataStage_ErrorGettingValue0, ex.Message)

            End Try

            Return sValue

        End Function

        ''' <summary>
        ''' Creates a data stage within the given process for the given data type,
        ''' handling collection types appropriately.
        ''' </summary>
        ''' <param name="proc">The process on which to create the stage</param>
        ''' <param name="dtype">The data type for the stage required - if this is
        ''' <see cref="DataType.collection"/> a collection stage will be created;
        ''' otherwise a data stage, initialised with the datatype will be created.
        ''' </param>
        ''' <returns>The resultant data stage with the data type set in it.
        ''' </returns>
        Public Shared Function Create(ByVal proc As clsProcess, ByVal dtype As DataType) As clsDataStage
            If dtype = DataType.collection Then
                Return DirectCast(proc.AddStage(StageTypes.Collection, ""), clsDataStage)
            Else
                Dim stg As clsDataStage =
                 DirectCast(proc.AddStage(StageTypes.Data, ""), clsDataStage)
                stg.SetDataType(dtype)
                Return stg
            End If
        End Function

        ''' <summary>
        ''' Determines if the supplied string contains any illegal characters for a
        ''' data item name.
        ''' </summary>
        ''' <param name="ProposedName">The string to be tested.</param>
        ''' <param name="sErr">Carries back an explanation, if the
        ''' supplied string is found to be invalid.</param>
        ''' <returns>Returns True if the supplied string is valid, False otherwise.
        ''' </returns>
        ''' <remarks>This method does not check for existing data items
        ''' with a conflicting name, or any other validation of that sort.</remarks>
        Public Shared Function IsValidDataName(ByVal ProposedName As String, ByRef sErr As String) As Boolean

            If String.IsNullOrEmpty(ProposedName) Then
                sErr = My.Resources.Resources.clsDataStage_DataItemNamesMustNotBeBlank
                Return False
            End If

            If ProposedName.Contains("[") OrElse ProposedName.Contains("]") Then
                sErr = My.Resources.Resources.clsDataStage_DataItemNamesMustNotContainSquareBrackets
                Return False
            End If

            If ProposedName.Contains(".") Then
                sErr = My.Resources.Resources.clsDataStage_DataItemNamesMustNotContainTheFullStopCharacter
                Return False
            End If

            If ProposedName.Contains("""") Then
                sErr = My.Resources.Resources.clsDataStage_DataItemNamesMustNotContainQuotationMarks
            End If

            Return True
        End Function

        ''' <summary>
        ''' Resets the value in this data stage to its initial value.
        ''' </summary>
        Public Sub ResetValue()

            'Environment variables don't get reset...
            If mExposure = StageExposureType.Environment Then Return

            'Session variables - if they already exist, nothing happens. If they
            'don't exist, they get created using the initial value.
            'Session may be null if we're being called from ValidateProcess() - BG-6184
            If mExposure = StageExposureType.Session AndAlso
                mParent.Session IsNot Nothing Then
                Dim cursval As clsProcessValue = mParent.Session.GetVar(GetName())
                If cursval Is Nothing Then
                    Dim val As clsProcessValue = GetInitialValue().Clone()
                    val.Description = GetNarrative()
                    mParent.Session.SetVar(GetName(), val)
                ElseIf cursval.DataType <> mDataType Then
                    'If someone reports this error, it probably means they have a
                    'subprocess that uses the same session variable as a parent,
                    'but the data types defined in the two do not match, which they
                    'must!
                    Throw New InvalidOperationException(My.Resources.Resources.clsDataStage_SessionVariableDefinitionDoesNotMatchValue)
                End If
                Return
            End If

            SetValue(GetInitialValue().Clone())

        End Sub

        ''' <summary>
        ''' Set the value for this stage. Relevant for a data stage only. The value
        ''' set must be consistent with the data type set on this stage, and be
        ''' valid. Note that validity of the data contained within the value is
        ''' not checked, and must not be until issues at Co-op regarding misuse of
        ''' the Date datatype have been resolved.
        ''' Throws an ApplicationException if the data type of the value passed does
        ''' not match that of the stage.
        ''' </summary>
        ''' <param name="val">The value to set, as a clsProcessValue.
        ''' Must not be null.</param>
        ''' <remarks>
        ''' See also CanSetValue, which determines if a value would be accepted by
        ''' this stage, based on the data type. By amending these two functions later,
        ''' we can hopefully restore the automatic casting functionality which has
        ''' been inadvertantly lost by V2 UI changes.
        ''' </remarks>
        Public Sub SetValue(ByVal val As clsProcessValue)
            If val Is Nothing Then Throw New ArgumentNullException(NameOf(val))

            If mExposure = StageExposureType.Environment Then Throw New InvalidStateException(
             My.Resources.Resources.clsDataStage_AttemptToSetValueOnAnEnvironmentVariable)

            If mExposure = StageExposureType.Session Then
                'There can be no session, if in process studio and not debugging
                'yet - in that case, we can just do nothing...
                If mParent.Session Is Nothing Then Return

                Dim clone As clsProcessValue = val.Clone()
                clone.Description = GetNarrative()
                mParent.Session.SetVar(Name, clone)
                Return
            End If

            'remember old value so that we can check if it has changed
            Dim oldVal As clsProcessValue = mValue

            'The new value must have the correct data type...
            If val.DataType <> mDataType Then Throw New InvalidValueException(
             My.Resources.Resources.clsDataStage_AttemptToSetValueOf01WithMismatchedDataTypeOf2, Name, mDataType, val.DataType)

            'Assign new value
            mValue = val

            'check if breakpoint condition is met and raise breakpoint
            'if needs be
            If Me.HasBreakPoint Then
                Select Case True
                    Case (Me.BreakPoint.BreakPointType And clsProcessBreakpoint.BreakEvents.WhenDataValueChanged) > 0
                        If Not oldVal.Equals(mValue) Then
                            'value has changed so raise breakpoint
                            mParent.RaiseBreakPoint(New clsProcessBreakpointInfo(Me.BreakPoint, clsProcessBreakpoint.BreakEvents.WhenDataValueChanged, Nothing))
                        End If
                    Case (Me.BreakPoint.BreakPointType And clsProcessBreakpoint.BreakEvents.WhenConditionMet) > 0
                        If Not oldVal.Equals(mValue) Then
                            Try
                                If Me.BreakPoint.IsConditionMet(mParent) Then
                                    'value changed, and condition met so raise breakpoint
                                    mParent.RaiseBreakPoint(New clsProcessBreakpointInfo(Me.BreakPoint, clsProcessBreakpoint.BreakEvents.WhenConditionMet, Nothing))
                                End If
                            Catch ex As Exception
                                'error evaluating expression, raise breakpoint anyway containing error message
                                mParent.RaiseBreakPoint(New clsProcessBreakpointInfo(Me.BreakPoint, clsProcessBreakpoint.BreakEvents.WhenConditionMet, ex.Message))
                            End Try
                        End If
                End Select
            End If

        End Sub

        ''' <summary>
        ''' Determines if it would be valid to give this data item
        ''' a particular value, based on the data type.
        ''' </summary>
        ''' <param name="objValue">A clsProcessValue with the data type
        ''' to test</param>
        ''' <returns>True if value can be set, False otherwise</returns>
        ''' <remarks>See also SetValue</remarks>
        Public Function CanSetValue(ByVal objValue As clsProcessValue) As Boolean
            If StageType <> StageTypes.Data Then
                Return False
            End If
            If objValue.DataType <> mDataType Then
                Return False
            End If
            Return True
        End Function


        ''' <summary>
        ''' Get the value for this data item.
        ''' </summary>
        ''' <returns>The value of the stage, as a clsProcessValue</returns>
        Public Overridable Function GetValue() As clsProcessValue

            'see if breakpoint should be raised on access
            If Me.HasBreakPoint Then
                If (Me.BreakPoint.BreakPointType And clsProcessBreakpoint.BreakEvents.WhenDataValueRead) > 0 Then
                    mParent.RaiseBreakPoint(New clsProcessBreakpointInfo(Me.BreakPoint, clsProcessBreakpoint.BreakEvents.WhenDataValueRead, Nothing))
                End If
            End If

            If mExposure = StageExposureType.Environment Then
                Return GetEnv()
            End If

            If mExposure = StageExposureType.Session Then
                If mParent.Session IsNot Nothing Then
                    Return mParent.Session.GetVar(GetName())
                End If
                'There can be no session - e.g. in Process Studio when viewing the process,
                'which hasn't been started yet. In this case we'll just return the initial
                'value defined in this stage
                Return GetInitialValue()
            End If

            Return mValue
        End Function

        ''' <summary>
        ''' The current value for this stage.
        ''' The return value from this data stage may be null.
        ''' </summary>
        ''' <remarks>This is largely here to allow the current value to be easily
        ''' seen in the VS Debugger - it delegates directly to the Get and Set
        ''' InitialValue() methods.</remarks>
        Public Property Value() As clsProcessValue Implements IDataField.Value
            Get
                Return GetValue()
            End Get
            Set(ByVal value As clsProcessValue)
                SetValue(value)
            End Set
        End Property


        ''' <summary>
        ''' Determines if this stage lies within the scope of the stage supplied.
        ''' </summary>
        ''' <param name="objScopeStage">The stage which will be used to determine
        ''' whether this stage is in scope. Must not be null.</param>
        ''' <returns>Returns true if this stage is not private; otherwise
        ''' returns true if and only if stages share the same page.</returns>
        ''' <remarks>This isn't currently reflective - eg. if a.IsInScope(b) is true,
        ''' it doesn't necessarily follow that b.IsInScope(a) will be true - if one
        ''' is private and the other isn't this will return true for the public one,
        ''' false for the private one (assuming different subsheets).
        ''' Surely this is incorrect?
        ''' </remarks>
        Public Function IsInScope(ByVal objScopeStage As clsProcessStage) As Boolean
            'Unless this stage is private, it is in scope from anywhere...
            If Not mPrivate Then Return True
            ' Check if the other stage is a public data stage - if it is, then it
            ' falls into this stage's scope too
            ' If TypeOf objScopeStage Is clsDataStage AndAlso _
            '  Not DirectCast(objScopeStage,clsDataStage).mPrivate Then Return True

            'Otherwise, we must be on the same page...
            If objScopeStage.GetSubSheetID().Equals(GetSubSheetID()) Then
                Return True
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' Finds name conflicts with this stage in the owning process.
        ''' This checks the process for stages which have the same name as this stage
        ''' and a type which would cause a conflict.
        ''' Data stages cannot have the same name as other data stages within the same
        ''' scope.
        ''' </summary>
        ''' <param name="proposedName">The proposed name for this code stage.</param>
        ''' <returns>A collection of process stages which have a naming conflict with
        ''' this stage - an empty collection means no conflicts.</returns>
        Public Overrides Function FindNamingConflicts(ByVal proposedName As String) _
         As ICollection(Of clsProcessStage)
            Dim stages As New List(Of clsProcessStage)
            For Each m As clsDataStage In Process.GetStages(
             proposedName, StageTypes.Data, StageTypes.Collection)
                If m.GetStageID() <> GetStageID() AndAlso (m.IsInScope(Me) OrElse IsInScope(m)) Then
                    stages.Add(m)
                End If
            Next
            Return stages
        End Function

        ''' <summary>
        ''' The data type for the item represented by this stage.
        ''' </summary>
        ''' <remarks>This property is primarily here so the data type can be easily
        ''' seen in the VisStudio debugger</remarks>
        Public Property StageDataType() As DataType
            Get
                Return GetDataType()
            End Get
            Set(ByVal value As DataType)
                SetDataType(value)
            End Set
        End Property

        ''' <summary>
        ''' Returns items referred to by this stage, so externally defined things
        ''' (such as environment variables).
        ''' </summary>
        ''' <param name="inclInternal">Indicates internal references required</param>
        ''' <returns>List of dependencies</returns>
        Public Overrides Function GetDependencies(inclInternal As Boolean) As List(Of clsProcessDependency)
            Dim deps As List(Of clsProcessDependency) = MyBase.GetDependencies(inclInternal)

            If Exposure = StageExposureType.Environment AndAlso Name <> String.Empty Then
                deps.Add(New clsProcessEnvironmentVarDependency(Name))
            End If

            Return deps
        End Function

        ''' <summary>
        ''' Set the data type for this data item.
        ''' </summary>
        ''' <param name="dtType">The data type.</param>
        ''' <remarks>If the data type changes, any existing value or
        ''' initial value will be lost.</remarks>
        Public Sub SetDataType(ByVal dtType As DataType)
            mDataType = dtType

            If StageType = StageTypes.Collection Then
                mDataType = DataType.collection
            End If

            'If we set a data type, and the existing value does not match
            'it, the value must get reset.
            If mValue.DataType <> mDataType Then
                mValue = New clsProcessValue(mDataType)
            End If
            'Same goes for the initial value...
            If mInitVal.DataType <> mDataType Then
                mInitVal = New clsProcessValue(mDataType)
            End If
        End Sub

        ''' <summary>
        ''' Get the data type of this data item.
        ''' </summary>
        Public Function GetDataType() As DataType
            Return mDataType
        End Function

        ''' <summary>
        ''' Creates a new clsDataStage and sets its parent. When we create a new data
        ''' item we create a new initial value and current value.
        ''' </summary>
        ''' <param name="parent"></param>
        Public Sub New(ByVal parent As clsProcess)
            MyBase.New(parent)
            mDataType = DataType.unknown
            mInitVal = New clsProcessValue()
            mValue = New clsProcessValue()
            mExposure = StageExposureType.None

            'Data items default to private in both object
            'studio and process studio, as per bug 2288.
            mPrivate = True

            mAlwaysInit = True

        End Sub

        ''' <summary>
        ''' A factory method that creates the correct type of object for the clone
        ''' method
        ''' </summary>
        ''' <returns>A new instance of a data item</returns>
        Public Overrides Function CloneCreate() As clsProcessStage
            Return New clsDataStage(mParent)
        End Function

        ''' <summary>
        ''' Creates a deep copy of the data item.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function Clone() As clsProcessStage
            Dim copy As clsDataStage = CType(MyBase.Clone, clsDataStage)
            copy.mDataType = mDataType
            copy.mValue = mValue.Clone()
            copy.mInitVal = mInitVal.Clone()
            copy.mExposure = mExposure
            copy.mPrivate = mPrivate
            copy.mAlwaysInit = mAlwaysInit
            Return copy
        End Function

        ''' <summary>
        ''' Get the type of this stage.
        ''' </summary>
        Public Overrides ReadOnly Property StageType() As StageTypes
            Get
                Return StageTypes.Data
            End Get
        End Property


        Public Overrides Function Execute(ByRef gRunStageID As Guid, logger As CompoundLoggingEngine) As StageResult
            Return New StageResult(False, "Internal", String.Format(My.Resources.Resources.clsDataStage_CanTExecuteStage0, GetName()))
        End Function

        Public Overrides Sub FromXML(ByVal e2 As XmlElement)
            MyBase.FromXML(e2)
            Dim dtype As DataType = DataType.unknown

            'We default to public when loading from xml, because
            'by default data items used to be public
            mPrivate = False
            'Same for AlwaysInit...
            mAlwaysInit = False

            For Each e3 As XmlElement In e2.ChildNodes
                Select Case e3.Name

                    Case "datatype"
                        ' Replace references to currency datatype with number and
                        ' allow old processes that have something invalid here to
                        ' default to 'unknown'
                        If e3.InnerText = "currency" _
                         Then dtype = DataType.number _
                         Else clsProcessDataTypes.TryParse(e3.InnerText, dtype)

                        SetDataType(dtype)

                    Case "initialvalue"
                        'Note we expect to have already parsed a 'datatype' attribute
                        'by this point

                        ' Collection stages handle their own initial value.
                        If StageType = StageTypes.Collection Then Continue For
                        SetInitialValue(New clsProcessValue(dtype, e3.InnerText))

                    Case "initialvalueenc"
                        'Note we expect to have already the datatype attribute here
                        SetInitialValue(New clsProcessValue(
                         clsProcess.UngarblePassword(e3.InnerText)))
                    Case "statistic"
                        mExposure = StageExposureType.Statistic
                    Case "exposure"
                        clsEnum.TryParse(e3.InnerText, mExposure)
                    Case "private"
                        mPrivate = True
                    Case "alwaysinit"
                        mAlwaysInit = True
                End Select
            Next
        End Sub
        Public Overrides Sub ToXml(ByVal doc As XmlDocument, ByVal stgElem As XmlElement, ByVal bSelectionOnly As Boolean)
            MyBase.ToXml(doc, stgElem, bSelectionOnly)
            Dim e2 As XmlElement
            'Note it is important that the 'datatype'
            'comes before the 'initialvalue'....
            e2 = doc.CreateElement("datatype")
            e2.AppendChild(doc.CreateTextNode(GetDataType().ToString))
            stgElem.AppendChild(e2)

            'Make sure we only write the initial value if this is a data and not a collection
            'which has its own method for writing the initial value.
            If StageType = StageTypes.Data Then
                If GetDataType() = DataType.password Then
                    e2 = doc.CreateElement("initialvalueenc")
                    Dim pwd = CType(GetInitialValue(), SafeString)
                    e2.AppendChild(doc.CreateTextNode(clsProcess.GarblePassword(pwd)))
                Else
                    e2 = doc.CreateElement("initialvalue")
                    Dim objValue As clsProcessValue = GetInitialValue()
                    Dim sVal As String = objValue.EncodedValue
                    If sVal <> "" Then
                        If objValue.DataType = DataType.text Then
                            Dim a As XmlAttribute = doc.CreateAttribute("xml", "space", "http://www.w3.org/XML/1998/namespace")
                            a.Value = "preserve"
                            e2.Attributes.Append(a)
                        End If
                        e2.AppendChild(doc.CreateTextNode(sVal))
                    End If
                End If
            End If

            stgElem.AppendChild(e2)
            If mExposure <> StageExposureType.None Then
                e2 = doc.CreateElement("exposure")
                Dim t As Xml.XmlText = doc.CreateTextNode(mExposure.ToString)
                e2.AppendChild(t)
                stgElem.AppendChild(e2)
            End If

            If mPrivate Then
                e2 = doc.CreateElement("private")
                stgElem.AppendChild(e2)
            End If

            If mAlwaysInit Then
                e2 = doc.CreateElement("alwaysinit")
                stgElem.AppendChild(e2)
            End If

        End Sub

        Public Overrides Function CheckForErrors(ByVal bAttemptRepair As Boolean, ByVal SkipObjects As Boolean) As ValidationErrorList
            Dim Errors As ValidationErrorList = MyBase.CheckForErrors(bAttemptRepair, SkipObjects)

            'Check for environment/session variables that are missing
            'And check related environment variable for mismatch in data type
            Select Case Me.Exposure
                Case StageExposureType.Environment
                    clsAPC.ProcessLoader.GetEnvVarSingle(Name, True)

                    If Not Process.EnvVars.ContainsKey(Name) Then
                        Errors.Add(New ValidateProcessResult(Me, "frmStagePropertiesData.htm", 115, Name))
                    Else

                        ' If there is a mismatch of data types between stage and enviornment variable
                        If CompareStageDataTypeToEnvironmentVariableDataType() Then

                            If bAttemptRepair Then
                                ' If the process should attempt to repair the mismatch 
                                ' (this will always work (contrary to the name 'attempt')
                                ' as there is no failure logic for setting of a data type) 
                                RepairStageDataTypeToEnvironmentVariableDataType()
                            Else
                                ' If the process should not attempt to repair the mismatch, return an error
                                Errors.Add(AddStageDataTypeToEnvironmentVariableDataTypeError())
                            End If
                        End If
                    End If
                Case StageExposureType.Session
                    ' Can't really test for nonexistent session items - they don't exist
                    ' until they are created in, well, in a data stage.
            End Select

            'Look for data items with conflicting names
            For Each objDuplicate As clsDataStage In mParent.GetStages(Of clsDataStage)()
                Debug.Assert(objDuplicate.StageType = StageTypes.Data OrElse objDuplicate.StageType = StageTypes.Collection)
                Dim objDuplicateData As clsDataStage = CType(objDuplicate, clsDataStage)
                If (objDuplicate.GetName = Me.GetName) Then
                    If objDuplicate.GetStageID <> Me.GetStageID Then
                        If objDuplicate.IsInScope(Me) OrElse Me.IsInScope(objDuplicate) Then
                            Dim LocalPageName As String = mParent.GetSubSheetName(Me.GetSubSheetID)
                            Dim RemotePageName As String = mParent.GetSubSheetName(objDuplicate.GetSubSheetID)
                            Errors.Add(New ValidateProcessResult(Me, 95, GetName(), LocalPageName, objDuplicate.GetName(), RemotePageName))
                        End If
                    End If
                End If
            Next

            'Check for data items with an undefined type
            If Me.GetDataType = DataType.unknown Then
                Errors.Add(New ValidateProcessResult(Me, 96))
            End If

            Return Errors
        End Function

        ''' <summary>
        ''' Return a repairable validation error to notify that there is a data type
        ''' mismatch between the stage's data type and environment variable data type
        ''' </summary>
        ''' <returns>Instance of RepairableValidateProcessResult</returns>
        Private Function AddStageDataTypeToEnvironmentVariableDataTypeError() As RepairableValidateProcessResult

            ' Get the environment variable
            Dim envVariable = Process.EnvVars(Name)

            ' Get the environment variable's data type for comparison
            Dim envVariableDataType = envVariable.Value.DataType

            ' Get the data stage's data type for comparisson
            Dim dataStageDataType = GetDataType()
            
            ' Instantiate and populate a validation result to be consumed by the application
            Return New RepairableValidateProcessResult(Me,
                                                       "frmStagePropertiesData.htm",
                                                       ValidationCheckType.DataStageDataTypeMissMatchEnvVariableDataType,
                                                       Name,
                                                       dataStageDataType.ToLocalisedString,
                                                       envVariableDataType.ToLocalisedString)
        End Function

        ''' <summary>
        ''' Compare the data type of the data stage with the data type of the 
        ''' associated environment variable and add an error to the error list
        ''' if they do not match
        ''' </summary>
        Private Function CompareStageDataTypeToEnvironmentVariableDataType() As Boolean

            ' Get the environment variable
            Dim envVariable = Process.EnvVars(Name)

            ' Get the environment variable's data type for comparison
            Dim envVariableDataType = envVariable.Value.DataType

            ' Get the data stage's data type for comparison
            Dim dataStageDataType = GetDataType()

            ' If the data types don't match return false
            Return Not envVariableDataType.Equals(dataStageDataType)

        End Function

        ''' <summary>
        ''' Compare the data type of the data stage with the data type of the 
        ''' associated environment variable and add an error to the error list
        ''' if they do not match
        ''' </summary>
        Private Sub RepairStageDataTypeToEnvironmentVariableDataType()

            ' Get the environment variable
            Dim envVariable = Process.EnvVars(Name)

            ' Get the environment variable's data type
            Dim envVariableDataType = envVariable.Value.DataType

            SetDataType(envVariableDataType)
        End Sub

        Friend Shared Sub DrawParallelogram(ByVal r As IRender, ByVal b As RectangleF)
            Dim p(3) As PointF
            With b
                p(0).X = .X + (.Width / 4)
                p(0).Y = .Y

                p(1).X = .X + .Width
                p(1).Y = .Y

                p(2).X = .X + .Width - (.Width / 4)
                p(2).Y = .Y + .Height

                p(3).X = .X
                p(3).Y = .Y + .Height
            End With

            r.FillPolygon(p)
            r.DrawPolygon(p)
        End Sub

        Public Overrides Function GetShortText() As String
            'When the process is not running, data boxes
            'always show the intial value. When running, they
            'show the current value.
            If mParent.RunState = ProcessRunState.Off Then
                Return GetName() & vbCrLf & GetShortValue(True)
            Else
                Return GetName() & vbCrLf & GetShortValue(False)
            End If
        End Function
    End Class
End Namespace
