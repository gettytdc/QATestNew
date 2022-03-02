Imports System.Xml
Imports System.Runtime.Serialization

Imports BluePrism.BPCoreLib.Collections

Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Processes

''' <summary>
''' Class representing a single validation check and its state (ie. category,
''' severity, enabled status)
''' </summary>
<Serializable()>
<DataContract(Name:="vi", [Namespace]:="")>
Public Class clsValidationInfo

    ' Flag indicating if this 'validation info' is enabled or not
    <DataMember(Name:="e", EmitDefaultValue:=False)>
    Private mEnabled As Boolean

    ''' <summary>
    ''' The ID of the validation rule that this check corresponds to.
    ''' </summary>
    <DataMember(Name:="i")>
    Public CheckID As Integer

    ''' <summary>
    ''' The severity assigned to this check
    ''' </summary>
    <DataMember(Name:="t", EmitDefaultValue:=False)>
    Public TypeID As Types

    ''' <summary>
    ''' The category in which this validation check resides.
    ''' </summary>
    <DataMember(Name:="c", EmitDefaultValue:=False)>
    Public CatID As Categories

    ''' <summary>
    ''' The error message to display for validation failures for this check.
    ''' </summary>
    <DataMember(Name:="m")>
    Public Message As String

    ''' <remarks>
    ''' Fortunately for now the highest action id is also the most restrictive
    ''' Currently we depend on this when finding the most restrictive action
    ''' within a set of errors.
    ''' </remarks>
    Public Enum Actions
        Ignore = 0
        Validate = 1
        PreventPublication = 2
        PreventSave = 3
    End Enum

    ''' <summary>
    ''' The categories for a set of validation checks
    ''' </summary>
    Public Enum Categories
        StageValidation = 0
        FlowValidation = 1
        DocumentationControl = 2
    End Enum

    ''' <summary>
    ''' The severities for validation checks
    ''' </summary>
    Public Enum Types
        [Error] = 0
        Warning = 1
        Advice = 2
    End Enum

    ''' <summary>
    ''' Creates a new validation check object.
    ''' </summary>
    Public Sub New()
        mEnabled = True
    End Sub

    ''' <summary>
    ''' Flag indicating whether this check is enabled or not.
    ''' </summary>
    Public Property Enabled() As Boolean
        Get
            Return mEnabled
        End Get
        Set(ByVal value As Boolean)
            mEnabled = value
        End Set
    End Property

    ''' <summary>
    ''' Writes this validation check out as XML.
    ''' </summary>
    ''' <param name="doc">The XML document to which this check should be appended.
    ''' </param>
    ''' <param name="category">The XML element representing the category into which
    ''' this validation check should be appended.</param>
    Public Sub ToXML(ByVal doc As XmlDocument, ByVal category As XmlElement)
        Dim e As XmlElement = doc.CreateElement("check")
        e.SetAttribute("id", CheckID.ToString())
        e.SetAttribute("name", Message)
        ' Assume enabled by default, add a disabled attribute if not.
        If Not mEnabled Then e.SetAttribute("disabled", XmlConvert.ToString(True))
        Dim e2 As XmlElement = doc.CreateElement("type")
        e2.SetAttribute("ref", CInt(TypeID).ToString())
        e.AppendChild(e2)
        category.AppendChild(e)
    End Sub

    ''' <summary>
    ''' Reads a validation check object from the given XML.
    ''' </summary>
    ''' <param name="check">The XML element representing the validation check that
    ''' should be loaded.</param>
    ''' <returns>A validation check object representing the value gleaned from the
    ''' supplied XML.</returns>
    Public Shared Function FromXML(ByVal check As XmlElement) As clsValidationInfo
        Dim v As New clsValidationInfo()
        v.CheckID = Integer.Parse(check.GetAttribute("id"))
        ' If the enabled attribute is there, set it accordingly
        Dim disabled As String = check.GetAttribute("disabled")
        If disabled <> "" Then v.mEnabled = Not XmlConvert.ToBoolean(disabled)
        For Each e As XmlElement In check.ChildNodes
            If e.Name = "type" Then
                v.TypeID = CType(Integer.Parse(e.GetAttribute("ref")), Types)
            End If
        Next
        Return v
    End Function

    ''' <summary>
    ''' Gets the filtered validation results for the given process, not attempting to
    ''' repair and including errors about missing objects. Or at least, I think
    ''' that's what the bNoObjects parameter in
    ''' <see cref="clsProcess.ValidateProcess"/> does... it's not crystal clear.
    ''' </summary>
    ''' <param name="proc">The process to validate</param>
    ''' <param name="validationInfo">The map of validation checks to their IDs
    ''' </param>
    ''' <param name="exceptionTypes">A list of the exception types currently stored 
    ''' in the database</param>
    ''' <returns>A collection of ValidateProcessResults with all disabled checks
    ''' filtered out representing the errors in the given process.</returns>
    Public Shared Function FilteredValidateProcess(
     ByVal proc As clsProcess,
     ByVal validationInfo As IDictionary(Of Integer, clsValidationInfo),
     ByVal exceptionTypes As ICollection(Of String)) _
     As ICollection(Of ValidateProcessResult)
        Return FilteredValidateProcess(proc, validationInfo, False, False,
                                       exceptionTypes)

    End Function

    ''' <summary>
    ''' Gets the filtered validation results for the given process.
    ''' </summary>
    ''' <param name="proc">The process to validate</param>
    ''' <param name="validationInfo">The map of validation checks to their IDs
    ''' </param>
    ''' <param name="attemptRepair">If True, attempts will be made to repair errors
    ''' where possible</param>
    ''' <param name="noObjects">If True, checks for installed Business Objects and
    ''' similar are skipped</param>
    ''' <param name="exceptionTypes">A list of exception types currently defined in 
    ''' the db</param>
    ''' <returns>A collection of ValidateProcessResults with all disabled checks
    ''' filtered out representing the errors in the given process.</returns>
    Public Shared Function FilteredValidateProcess(
     ByVal proc As clsProcess,
     ByVal validationInfo As IDictionary(Of Integer, clsValidationInfo),
     ByVal attemptRepair As Boolean,
     ByVal noObjects As Boolean,
     ByVal exceptionTypes As ICollection(Of String)) _
     As ICollection(Of ValidateProcessResult)

        ' If this is an object then look for any references to it's appplication
        ' model in other objects (shared models)
        Dim elRefs As clsProcessDependencyList = Nothing
        If proc.ProcessType = DiagramType.Object AndAlso proc.ParentObject Is Nothing Then
            elRefs = gSv.GetSharedModelReferences(proc.Name)
        End If

        Dim results As New clsSortedSet(Of ValidateProcessResult)(
         New InitialResultComparer(validationInfo),
         proc.ValidateProcess(attemptRepair, noObjects, exceptionTypes, New DependencyPermissionChecker(gSv), elRefs))
        
        ' Now filter out any disabled checks
        Dim toDelete As New List(Of ValidateProcessResult)
        For Each res As ValidateProcessResult In results
            If Not validationInfo(res.CheckID).Enabled Then toDelete.Add(res)
        Next
        results.Subtract(toDelete)

        Return results

    End Function


    ''' <summary>
    ''' Comparer to sort the process validation results into their preferred initial
    ''' sort order, which is severity, then page name, then stage name, then category
    ''' </summary>
    Private Class InitialResultComparer : Implements IComparer(Of ValidateProcessResult)
        Private mInfo As IDictionary(Of Integer, clsValidationInfo)

        Public Sub New(ByVal validationInfo As IDictionary(Of Integer, clsValidationInfo))
            mInfo = validationInfo
        End Sub

        Public Function Compare( _
         ByVal x As ValidateProcessResult, ByVal y As ValidateProcessResult) As Integer _
         Implements IComparer(Of ValidateProcessResult).Compare

            ' The basics... reference / value equality / null checks
            If x Is y OrElse Object.Equals(x, y) Then Return 0
            If y Is Nothing Then Return 1
            If x Is Nothing Then Return -1

            ' Get the validation info for each check
            Dim xi As clsValidationInfo = mInfo(x.CheckID)
            Dim yi As clsValidationInfo = mInfo(y.CheckID)
            Dim diff As Integer

            ' Test severity first - that trumps other concerns
            diff = CInt(xi.TypeID).CompareTo(CInt(yi.TypeID))
            If diff <> 0 Then Return diff

            ' Then test page name
            diff = x.PageName.CompareTo(y.PageName)
            If diff <> 0 Then Return diff

            ' Then by stage name
            diff = x.StageName.CompareTo(y.StageName)
            If diff <> 0 Then Return diff

            ' ...category
            diff = CInt(xi.CatID).CompareTo(CInt(yi.CatID))
            If diff <> 0 Then Return diff

            ' ...error ID
            diff = xi.CheckID.CompareTo(yi.CheckID)
            If diff <> 0 Then Return diff

            ' Otherwise, there's not a lot to go on.
            ' Go with the object hash codes... that's all we have left to differentiate on
            Return x.GetHashCode().CompareTo(y.GetHashCode())

        End Function
    End Class

End Class
