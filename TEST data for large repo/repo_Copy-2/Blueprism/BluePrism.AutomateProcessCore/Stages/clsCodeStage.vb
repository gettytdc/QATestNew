Imports System.Runtime.Serialization

Namespace Stages
    ''' Project  : AutomateProcessCore
    ''' Class    : AutomateProcessCore.Stages.clsCodeStage
    ''' 
    ''' <summary>
    ''' The code stage allows code fragments to be placed into the process, these are
    ''' compiled and run as part of the business object.
    ''' 
    ''' Inputs to the stage are set in base class via SetInputsXML as usual; similarly
    ''' for outputs.
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class clsCodeStage
        Inherits clsLinkableStage

        ''' <summary>
        ''' Creates a new instance of the clsCodeStage class and sets its parent.
        ''' </summary>
        ''' <param name="parent"></param>
        Public Sub New(ByVal parent As clsProcess)
            MyBase.New(parent)
        End Sub

        ''' <summary>
        ''' A factory method that creates the correct type of object for the clone
        ''' method</summary>
        ''' <returns>A new instance of a Code stage</returns>
        Public Overrides Function CloneCreate() As clsProcessStage
            Return New clsCodeStage(mParent)
        End Function

        ''' <summary>
        ''' Get the type of this stage.
        ''' </summary>
        Public Overrides ReadOnly Property StageType() As StageTypes
            Get
                Return StageTypes.Code
            End Get
        End Property

        ''' <summary>
        ''' Gets the language effective for this code stage. This is actually a
        ''' process-wide setting and so, if this stage is separate from a process or
        ''' its process has no process info stage, it can't know the language that
        ''' it represents.
        ''' </summary>
        ''' <returns>The language that is effective for this code stage, or null if
        ''' the information is not available to determine that.</returns>
        Public ReadOnly Property Language() As String
            Get
                If mParent Is Nothing Then Return Nothing
                Dim pInfo As clsProcessInfoStage = mParent.InfoStage
                If pInfo Is Nothing Then Return Nothing
                Return pInfo.Language
            End Get
        End Property

        Public Overrides Function Execute(ByRef gRunStageID As Guid, logger As CompoundLoggingEngine) As StageResult

            Dim sErr As String = Nothing

            If Not Compiled Then
                Return New StageResult(False, "Internal", My.Resources.Resources.clsCodeStage_ThisCodeStageCouldNotBeRunBecauseItWasAddedOrModifiedSinceTheObjectWasStarted)
            End If

            'Get the inputs for the code stage
            Dim inputs As clsArgumentList = Nothing
            If Not mParent.ReadDataItemsFromParameters(GetInputs(), Me, inputs, sErr, False, False) Then
                Return New StageResult(False, "Internal", sErr)
            End If

            CodePrologue(logger, inputs)

            'Create the right number of empty output args
            Dim outputs As New clsArgumentList
            For Each objParam As clsProcessParameter In GetOutputs()
                Dim objValue As New clsProcessValue
                objValue.DataType = objParam.GetDataType
                Dim objArg As New clsArgument(objParam.Name, objValue)
                outputs.Add(objArg)
            Next

            If mParent.CompilerRunner.Execute(Me, inputs, outputs, sErr) Then

                'Get the outputsXML for the code stage

                If Not mParent.StoreParametersInDataItems(Me, outputs, sErr, True) Then _
                 Return StageResult.InternalError(sErr)

                'Set the next stage
                If Not mParent.UpdateNextStage(gRunStageID, LinkType.OnSuccess, sErr) Then
                    Return New StageResult(False, "Internal", sErr)
                End If

                CodeEpilogue(logger, inputs, outputs)

                Return New StageResult(True)
            Else
                Return New StageResult(False, "Internal", sErr)
            End If
        End Function

        Private Sub CodePrologue(logger As CompoundLoggingEngine, ByVal inputs As clsArgumentList)
            Dim info = GetLogInfo()
            logger.CodePrologue(info, Me, inputs)
        End Sub

        Private Sub CodeEpilogue(logger As CompoundLoggingEngine, ByVal inputs As clsArgumentList, ByVal outputs As clsArgumentList)
            Dim info = GetLogInfo()
            logger.CodeEpiLogue(info, Me, inputs, outputs)
        End Sub

        ''' <summary>
        ''' The textual body of the subroutine that this code stage represents.
        ''' The header "Public Sub DoStuff(Input1 as string, .... etc .. )"
        ''' is implied from the inputs to the stage (see SetInputsXML in base
        ''' class) so are not included in this string. Similarly the line
        ''' "End Sub" is not included.
        ''' When the code changed the mbCompiled flag is reset to false
        ''' </summary>
        Public Property CodeText() As String
            Get
                Return mCodeText
            End Get
            Set(ByVal value As String)
                If Not value = mCodeText Then
                    Compiled = False
                End If

                mCodeText = value
            End Set
        End Property
        <DataMember>
        Private mCodeText As String


        ''' <summary>
        ''' Indicates whether this code stage has been compiled. This property
        ''' is deliberately not saved to XML so that when the stage
        ''' is pasted it defaults to false.
        ''' </summary>
        Public Property Compiled() As Boolean
            Get
                Return mCompiled
            End Get
            Set(ByVal value As Boolean)
                mCompiled = value
            End Set
        End Property
        <DataMember>
        Private mCompiled As Boolean


        ''' <summary>
        ''' Stores the height of the function header of the code in lines.
        ''' </summary>
        Public Property HeaderHeight() As Integer
            Get
                Return mHeaderHeight
            End Get
            Set(ByVal value As Integer)
                mHeaderHeight = value
            End Set
        End Property
        <DataMember>
        Private mHeaderHeight As Integer


        ''' <summary>
        ''' Finds name conflicts with this stage in the owning process.
        ''' This checks the process for stages which have the same name as this stage
        ''' and a type which would cause a conflict.
        ''' Code stages cannot have the same name as other code stages within the
        ''' process.
        ''' </summary>
        ''' <param name="proposedName">The proposed name for this code stage.</param>
        ''' <returns>A collection of process stages which have a naming conflict with
        ''' this stage - an empty collection means no conflicts.</returns>
        Public Overrides Function FindNamingConflicts(ByVal proposedName As String) _
         As ICollection(Of clsProcessStage)
            Dim stages As New List(Of clsProcessStage)
            For Each m As clsCodeStage In Process.GetStages(proposedName, StageTypes.Code)
                If m.GetStageID() <> GetStageID() Then stages.Add(m)
            Next
            Return stages
        End Function

        ''' <summary>
        ''' Creates a name for the supplied parameter that is convenient for the .NET
        ''' compiler. e.g. removes spaces and leading digits.
        ''' </summary>
        ''' <param name="p">The parameter for which a name should be generated.</param>
        ''' <returns>The nice parameter name.</returns>
        Public Function GetParameterName(ByVal p As clsProcessParameter) As String
            Return Compilation.CodeCompiler.GetIdentifier(p.Name)
        End Function

        ''' <summary>
        ''' Gets the name that is given to the method in this code stage. This may
        ''' differ by a number at the end to the one given in the method declaration.
        ''' 
        ''' E.g. if there are two stages with the name DoStuff, each will return
        ''' "DoStuff" when this method is called, but when GenerateMethodDeclaration
        ''' is  called, one declaration will contain the name "DoStuff" and the other
        ''' will contain the name "DoStuff1"
        ''' </summary>
        ''' <returns>The method name.</returns>
        ''' <remarks>The name of the method is based on the name of the stage, with
        ''' certain characters (e.g. spaces) replaced, etc.</remarks>
        Public Function GetMethodName() As String
            Return Compilation.CodeCompiler.GetIdentifier(Me.GetName)
        End Function

        ''' <summary>
        ''' Performs a deep clone.
        ''' </summary>
        ''' <returns>Returns a clsCodeStage.</returns>
        Public Overrides Function Clone() As clsProcessStage
            Dim copy As clsCodeStage = CType(MyBase.Clone, clsCodeStage)

            copy.CodeText = Me.CodeText
            copy.Compiled = Me.Compiled
            Return copy
        End Function

        Public Overrides Sub FromXML(ByVal e2 As System.Xml.XmlElement)
            MyBase.FromXML(e2)
            For Each e3 As Xml.XmlElement In e2.ChildNodes
                Select Case e3.Name
                    Case "code"
                        Me.CodeText = e3.InnerText
                End Select
            Next
        End Sub

        Public Overrides Sub ToXml(ByVal ParentDocument As System.Xml.XmlDocument, ByVal StageElement As System.Xml.XmlElement, ByVal bSelectionOnly As Boolean)
            MyBase.ToXml(ParentDocument, StageElement, bSelectionOnly)
            Dim e2 As Xml.XmlElement = ParentDocument.CreateElement("code")
            Dim e3 As Xml.XmlCDataSection = ParentDocument.CreateCDataSection(Me.CodeText)
            e2.AppendChild(e3)
            StageElement.AppendChild(e2)
        End Sub

    End Class
End Namespace
