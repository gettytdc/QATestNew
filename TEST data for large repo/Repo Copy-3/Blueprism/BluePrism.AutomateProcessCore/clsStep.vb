Imports System.Linq
Imports System.Runtime.Serialization
Imports System.Xml
Imports BluePrism.AMI
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.ApplicationManager.AMI
Imports BluePrism.AutomateProcessCore

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsStep
''' 
''' <summary>
''' Represents a 'step' as used in the Read, Write and Navigate stages.
''' </summary>
<Serializable, DataContract([Namespace]:="bp")>
Public MustInherit Class clsStep : Implements IActionStep

#Region " Member Variables "
    <DataMember>
    Protected mElementId As Guid

    <DataMember>
    Protected mAmiAction As AmiAction

    <DataMember>
    Private mParams As New List(Of clsApplicationElementParameter)

    <DataMember>
    Private mArgumentValues As New Dictionary(Of String, String)

    <DataMember>
    Private mOutputValues As New Dictionary(Of String, String)

    <NonSerialized>
    Private mOwner As clsAppStage

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new empty step
    ''' </summary>
    Private Sub New()
        Me.New(Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new step owned by the given application stage.
    ''' </summary>
    Public Sub New(ByVal _owner As clsAppStage)
        Owner = _owner
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' A List of clsProcessParameter objects that go with the element ID. This will
    ''' be an empty list unless the Application Definition specifies that the element
    ''' is 'parameterised' in some way.
    ''' </summary>
    Public ReadOnly Property Parameters() As List(Of clsApplicationElementParameter) _
     Implements IActionStep.Parameters
        Get
            Return mParams
        End Get
    End Property

    ''' <summary>
    ''' See interface for documentation.
    ''' </summary>
    Public Overridable Property Action() As clsActionTypeInfo _
     Implements IActionStep.Action
        Get
            Return mAmiAction?.Action
        End Get
        Set(ByVal value As clsActionTypeInfo)
            mAmiAction = If(value Is Nothing, Nothing, New AmiAction(value))
        End Set
    End Property

    ''' <summary>
    ''' The unique ID of the element on which this step operates
    ''' </summary>
    Public Property ElementId() As Guid Implements IActionStep.ElementId
        Get
            Return mElementId
        End Get
        Set(ByVal value As Guid)
            mElementId = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the action ID for this step or null if no action has been set in this
    ''' step.
    ''' </summary>
    Public Overridable ReadOnly Property ActionId() As String _
     Implements IActionStep.ActionId
        Get
            Return Action?.ID
        End Get
    End Property

    ''' <summary>
    ''' Gets the action name for this step or null if no action has been set in it
    ''' </summary>
    Public Overridable ReadOnly Property ActionName() As String
        Get
            Return Action?.Name
        End Get
    End Property

    ''' <summary>
    ''' Gets the action data type for this step or the default
    ''' (<see cref="DataType.unknown"/>) if no data type is set in the action, or if
    ''' no action has been set in this step
    ''' </summary>
    Public Overridable ReadOnly Property ActionDataType() As DataType
        Get
            If Action IsNot Nothing AndAlso me.GetType() <> GetType(clsWriteStep) Then Return clsProcessDataTypes.Parse(Action.ReturnType)

            If ElementId <> Guid.Empty Then
                Dim element = Me.ApplicationDefinition.FindElement(ElementId)
                If element IsNot Nothing Then Return element.DataType
            End If

            Return DataType.unknown
        End Get
    End Property

    ''' <summary>
    ''' The values that have been assigned to arguments. A dictionary with the key
    ''' being the Argument ID, and the value being the value, in Automate-encoded
    ''' format, matching the datatype of the argument.
    ''' </summary>
    ''' <remarks>Currently not used on write stages at all</remarks>
    Public Overridable ReadOnly Property ArgumentValues() As IDictionary(Of String, String) _
     Implements IActionStep.ArgumentValues
        Get
            Return mArgumentValues
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the owner of this step - ie. the application stage that this
    ''' step exists within
    ''' </summary>
    Public Property Owner() As clsAppStage
        Get
            Return mOwner
        End Get
        Set(ByVal value As clsAppStage)
            mOwner = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the application definition associated with the process which ultimately
    ''' holds this step or null if none can be found - ie. this step has no owning
    ''' stage, or the stage has no owning process, or the process has no application
    ''' definition associated with it.
    ''' </summary>
    Protected ReadOnly Property ApplicationDefinition() As clsApplicationDefinition
        Get
            Return Owner?.Process?.ApplicationDefinition
        End Get
    End Property

    Public Overridable ReadOnly Property OutputValues As IDictionary(Of String, String) _
        Implements IActionStep.OutputValues
        Get
            Return mOutputValues
        End Get
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Deep clones this step and returns the copy
    ''' </summary>
    ''' <returns>A deep clone of this step</returns>
    Public Overridable Function Clone() As clsStep

        Dim st As clsStep = DirectCast(MemberwiseClone(), clsStep)

        ' The collections need to be recreated and repopulated - the params must
        ' use clones of this object's params; the arg values are string/string so
        ' only the dictionary itself needs to be recreated.

        ' The list of parameters required by this step
        st.mParams = New List(Of clsApplicationElementParameter)(Parameters.Select(Function(p) p.Clone()))

        ' The arguments
        st.mArgumentValues = New Dictionary(Of String, String)(mArgumentValues)
        st.mOutputValues = New Dictionary(Of String, String)(mOutputValues)

        Return st

    End Function

    ''' <summary>
    ''' Get information about a step from the XML definition
    ''' </summary>
    ''' <param name="e">The XML element to read from</param>
    Public Overridable Sub FromXML(ByVal e As XmlElement)
        If e.Name = "step" Then

            Dim eid As Guid
            Dim Params As New List(Of clsApplicationElementParameter)

            For Each child As XmlElement In e.ChildNodes
                Select Case child.Name
                    Case "element"
                        eid = New Guid(child.GetAttribute("id"))
                        For Each grandchild As XmlElement In child.ChildNodes
                            Select Case grandchild.Name
                                Case "elementparameter"
                                    Params.Add(clsApplicationElementParameter.FromXML(grandchild))
                                Case "input"
                                    'Backwards compatibility for when we used to use
                                    ' Process parameters instead of element 
                                    ' parameters. The change occured because we added
                                    ' wildcard matching.
                                    Dim p As clsProcessParameter =
                                     clsProcessParameter.FromXML(grandchild)
                                    Params.Add(New clsApplicationElementParameter( _
                                     p.Name, p.GetDataType, p.Expression, _
                                     clsAMI.ComparisonTypes.Equal))
                            End Select
                        Next
                End Select
            Next

            Me.ElementId = eid
            Me.Parameters.AddRange(Params)

        End If
    End Sub

    ''' <summary>
    ''' Generate XML definition of the step
    ''' </summary>
    Public MustOverride Sub ToXML(ByVal doc As XmlDocument, ByVal StepElement As XmlElement)

    ''' <summary>
    ''' Checks the arguments for errors, in a step that implements the
    ''' IActionArgumentStep interface.
    ''' </summary>
    ''' <param name="stage">The step to check.</param>
    ''' <param name="parentStage">The stage owning this step.</param>
    ''' <param name="officialArguments">The arguments published by AMI for the
    ''' action of interest.</param>
    ''' <param name="rowNo">A text-representation of the one-based index of this
    ''' step, in the parent stage's collection.</param>
    ''' <returns>Returns a list of errors, which may be empty but not null.</returns>
    Friend Shared Function CheckForArgumentErrors(ByVal stage As IActionStep, _
     ByVal parentStage As clsAppStage, _
     ByVal officialArguments As IDictionary(Of String, clsArgumentInfo), _
     ByVal rowNo As Integer, ByVal bAttemptRepair As Boolean) _
     As ValidationErrorList

        Dim errors As New ValidationErrorList

        'Check that values supplied to arguments are valid.
        For Each arg As clsArgumentInfo In officialArguments.Values
            If stage.ArgumentValues.ContainsKey(arg.ID) Then
                'Check the expression supplied
                Dim expression As String = stage.ArgumentValues(arg.ID)
                If Not String.IsNullOrEmpty(expression) Then
                    Dim expressionInfo As clsExpressionInfo = Nothing, res As clsProcessValue = Nothing
                    Dim loc As String = " argument " & arg.Name & " in row " & rowNo
                    errors.AddRange(clsExpression.CheckExpressionForErrors( _
                     expression, parentStage, clsProcessDataTypes.Parse(arg.DataType), _
                     loc, expressionInfo, res))
                Else
                    If Not arg.IsOptional Then
                        errors.Add(New ValidateProcessResult(parentStage, 90, rowNo, arg.Name))
                    End If
                End If
            Else
                If Not arg.IsOptional Then
                    errors.Add(New ValidateProcessResult(parentStage, 91, rowNo, arg.Name))
                End If
            End If
        Next arg

        'Now check that all the arguments in this step actually 
        'exist in the list of OfficialArguments
        For Each argID As String In New List(Of String)(stage.ArgumentValues.Keys)
            If Not officialArguments.ContainsKey(argID) Then
                If bAttemptRepair Then
                    stage.ArgumentValues.Remove(argID)
                Else
                    errors.Add(New RepairableValidateProcessResult(parentStage, 92, rowNo, argID))
                End If
            End If
        Next

        Return errors
    End Function

    ''' <summary>
    ''' Gets the label to display for the action in this step.
    ''' </summary>
    ''' <param name="model">The application model in which to search for the element
    ''' referred to by this step.</param>
    ''' <returns>The label to show for the action in this step. This may be suffixed
    ''' with a "(deprecated)" if the action is deprecated for the type of the element
    ''' currently assigned in this step.</returns>
    Public Function GetActionLabel(model As clsApplicationDefinition) As String
        If model Is Nothing OrElse Action Is Nothing Then Return ActionName

        Dim elem As clsApplicationElement = model.FindElement(ElementId)
        If elem Is Nothing Then Return ActionName

        Return Action.GetLabel(elem.Type)

    End Function


    ''' <summary>
    ''' Checks any element parameters present for errors.
    ''' </summary>
    ''' <param name="parentStage">The stage to which this step belongs.</param>
    ''' <param name="rowNo">A text representation of the row number on
    ''' which this step resides, indexed from one.</param>
    ''' <returns>Returns a list of errors found. May be empty, but never null.</returns>
    Public Function CheckForElementParameterErrors(ByVal parentStage As clsAppStage, ByVal rowNo As Integer) As ValidationErrorList
        Dim errors As New ValidationErrorList

        Dim targetElement As clsApplicationElement = parentStage.Process.ApplicationDefinition.FindElement(Me.ElementId)
        If targetElement IsNot Nothing Then
            For Each a As clsApplicationAttribute In targetElement.Attributes
                If a.Dynamic AndAlso a.InUse Then
                    'Find the parameter supplying a value to this attribute
                    Dim matchingParameter As clsApplicationElementParameter = Nothing
                    For Each p As clsApplicationElementParameter In Me.Parameters
                        If p.Name = a.Name Then
                            matchingParameter = p
                            Exit For
                        End If
                    Next

                    'Validate the expression to the parameter, if found
                    If matchingParameter IsNot Nothing Then
                        Dim loc As String = String.Format(" for attribute '{0}' in row {1}", a.Name, rowNo)
                        errors.AddRange(clsExpression.CheckExpressionForErrors(matchingParameter.Expression, parentStage, matchingParameter.DataType, loc, Nothing, Nothing))
                    Else
                        errors.Add(New ValidateProcessResult(parentStage, 94, a.Name, rowNo))
                    End If

                End If
            Next
        End If

        Return errors
    End Function

#End Region

End Class
