Imports System.Runtime.Serialization
Imports BluePrism.AutomateProcessCore.Processes

Namespace Stages

    ''' Project  : AutomateProcessCore
    ''' Class    : AutomateProcessCore.Stages.clsProcessInfoStage
    ''' 
    ''' <summary>
    ''' The process info stage shows information about the process on the process
    ''' diagram. The actual data that the process info stage stores is not stored in
    ''' the stage class but in the clsProcess object itself.
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class clsProcessInfoStage
        Inherits clsProcessStage


        ''' <summary>
        ''' Creates a new instance of the clsActionStage class and sets its parent
        ''' </summary>
        ''' <param name="parent"></param>
        Public Sub New(ByVal parent As clsProcess)
            MyBase.New(parent)
        End Sub

        ''' <summary>
        ''' A factory method that creates the correct type of object for the clone
        ''' method
        ''' </summary>
        ''' <returns>A new instance of an Process Info stage</returns>
        Public Overrides Function CloneCreate() As clsProcessStage
            Return New clsProcessInfoStage(mParent)
        End Function

        ''' <summary>
        ''' Get the type of this stage.
        ''' </summary>
        Public Overrides ReadOnly Property StageType() As StageTypes
            Get
                Return StageTypes.ProcessInfo
            End Get
        End Property

        ''' <summary>
        ''' The language used for code stages.
        ''' </summary>
        ''' <remarks>This is only relevant when the parent process has type Business
        ''' Object</remarks>
        Public Property Language() As String
            Get
                Return mLanguage
            End Get
            Set(ByVal value As String)
                mLanguage = value
            End Set
        End Property
        <DataMember>
        Private mLanguage As String = "visualbasic"


        ''' <summary>
        ''' A list of namespace imports to be included whilst building the parent
        ''' business object. 
        ''' </summary>
        ''' <remarks>This is only relevant when the parent process has type Business
        ''' Object</remarks>
        Public ReadOnly Property NamespaceImports() As List(Of String)
            Get
                Return mNamespaceImports
            End Get
        End Property
        <DataMember>
        Private mNamespaceImports As List(Of String) = New List(Of String)


        ''' <summary>
        ''' A list of assembly references to be included whilst building the parent
        ''' business object. 
        ''' </summary>
        ''' <remarks>This is only relevant when the parent process has type Business
        ''' Object</remarks>
        Public ReadOnly Property AssemblyReferences() As List(Of String)
            Get
                Return mAssemblyReferences
            End Get
        End Property
        <DataMember>
        Private mAssemblyReferences As List(Of String) = New List(Of String)


        ''' <summary>
        ''' Statements for the global code, at the namespace level
        ''' </summary>
        ''' <remarks>Only relevant when parent process is of type Object.</remarks>
        Public Property GlobalCode() As String
            Get
                Return mGlobalCode
            End Get
            Set(ByVal value As String)
                mGlobalCode = value
            End Set
        End Property
        <DataMember>
        Private mGlobalCode As String


        ''' <summary>
        ''' The variable declarations and class methods.
        ''' The leading statement "Public Class [BusinessObjectName]"
        ''' and the trailing statement "End Class" are assumed
        ''' and must be omitted.
        ''' </summary>
        ''' <remarks>Relevant only when the parent process is of type Object.
        ''' </remarks>
        Public Property CodeText() As String
            Get
                Return mCodeText
            End Get
            Set(ByVal value As String)
                mCodeText = value
            End Set
        End Property
        <DataMember>
        Private mCodeText As String

        ''' <summary>
        ''' Creates a deep copy of the process info stage.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function Clone() As clsProcessStage
            Dim copy As clsProcessInfoStage = CType(MyBase.Clone, clsProcessInfoStage)
            If MyBase.mParent.ProcessType = DiagramType.Object Then
                copy.CodeText = Me.CodeText
                copy.GlobalCode = Me.GlobalCode
                copy.mAssemblyReferences = Me.mAssemblyReferences
                copy.mNamespaceImports = Me.mNamespaceImports
                copy.Language = Me.Language
            End If

            Return copy
        End Function

        Public Overrides Sub ToXml(ByVal ParentDocument As System.Xml.XmlDocument, ByVal StageElement As System.Xml.XmlElement, ByVal bSelectionOnly As Boolean)
            MyBase.ToXml(ParentDocument, StageElement, bSelectionOnly)
            If mParent.ProcessType = DiagramType.Object Then
                Dim e1 As Xml.XmlElement = ParentDocument.CreateElement("references")
                For Each s As String In Me.AssemblyReferences
                    Dim e2 As Xml.XmlElement = ParentDocument.CreateElement("reference")
                    e2.AppendChild(ParentDocument.CreateTextNode(s))
                    e1.AppendChild(e2)
                Next
                StageElement.AppendChild(e1)

                e1 = ParentDocument.CreateElement("imports")
                For Each s As String In Me.NamespaceImports
                    Dim e2 As Xml.XmlElement = ParentDocument.CreateElement("import")
                    e2.AppendChild(ParentDocument.CreateTextNode(s))
                    e1.AppendChild(e2)
                Next
                StageElement.AppendChild(e1)

                e1 = ParentDocument.CreateElement("language")
                Dim lang As Xml.XmlText = ParentDocument.CreateTextNode(Me.Language)
                e1.AppendChild(lang)
                StageElement.AppendChild(e1)

                e1 = ParentDocument.CreateElement("globalcode")
                Dim cdata As Xml.XmlCDataSection = ParentDocument.CreateCDataSection(Me.GlobalCode)
                e1.AppendChild(cdata)
                StageElement.AppendChild(e1)

                e1 = ParentDocument.CreateElement("code")
                cdata = ParentDocument.CreateCDataSection(Me.CodeText)
                e1.AppendChild(cdata)

                StageElement.AppendChild(e1)
            End If
        End Sub

        Public Overrides Sub FromXML(ByVal e2 As System.Xml.XmlElement)
            MyBase.FromXML(e2)
            If mParent.ProcessType = DiagramType.Object Then
                For Each e3 As Xml.XmlElement In e2.ChildNodes
                    Select Case e3.Name
                        Case "references"
                            For Each e4 As Xml.XmlElement In e3.ChildNodes
                                Select Case e4.Name
                                    Case "reference"
                                        Me.AssemblyReferences.Add(e4.InnerText)
                                End Select
                            Next
                        Case "imports"
                            For Each e4 As Xml.XmlElement In e3.ChildNodes
                                Select Case e4.Name
                                    Case "import"
                                        Me.NamespaceImports.Add(e4.InnerText)
                                End Select
                            Next
                        Case "language"
                            Me.Language = e3.InnerText
                        Case "globalcode"
                            Me.GlobalCode = e3.InnerText
                        Case "code"
                            Me.CodeText = e3.InnerText
                    End Select
                Next
            End If
        End Sub

        Public Overrides Function Execute(ByRef gRunStageID As Guid, logger As CompoundLoggingEngine) As StageResult
            Return New StageResult(False, "Internal", "Can't execute stage " & GetName())
        End Function

        Public Overrides Function GetShortText() As String
            Return mParent.Name
        End Function
    End Class

End Namespace
