Imports System.Xml
Imports BluePrism.Server.Domain.Models
Imports BluePrism.AMI
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.ApplicationManager.AMI
Imports System.Runtime.Serialization

''' <summary>
''' Class representing a read step - a single step within a read stage.
''' </summary>
<Serializable, DataContract([Namespace]:="bp")>
Public Class clsReadStep : Inherits clsStep

    ' The stage that this step forms a part of
    Private mStage As String

    ''' <summary>
    ''' Creates a new empty read step
    ''' </summary>
    Public Sub New(ByVal stg As clsReadStage)
        Me.New(stg, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new read step initialised from the given XML element.
    ''' </summary>
    ''' <param name="el">The element from which to draw the read step data. If this
    ''' is null, an empty read step is created.</param>
    Public Sub New(ByVal stg As clsReadStage, ByVal el As XmlElement)
        MyBase.New(stg)
        If el IsNot Nothing Then FromXML(el)
    End Sub

    ''' <summary>
    ''' The name of the stage into which the result of this read
    ''' will be stored.
    ''' </summary>
    Public Property Stage() As String
        Get
            Return mStage
        End Get
        Set(ByVal value As String)
            mStage = value
        End Set
    End Property

    ''' <summary>
    ''' Populates this read step from the given XML element.
    ''' </summary>
    ''' <param name="e">The element (which should be a 'step' element) which contains
    ''' the data from which to draw the settings for this read step</param>
    ''' <exception cref="NoSuchElementException">If an action ID was specified in
    ''' the given XML element which AMI did not recognise</exception>
    Public Overrides Sub FromXML(ByVal e As XmlElement)

        If e.Name <> "step" Then Return

        MyBase.FromXML(e)

        For Each child As XmlElement In e.ChildNodes
            If child.Name <> "action" Then Continue For

            For Each grandchild As XmlElement In child.ChildNodes
                Select Case grandchild.Name
                    Case "id"
                        ' Get the ID of the read action for this step
                        Dim id As String = grandchild.FirstChild.Value

                        ' Discover if there is an alternative to the read step, and
                        ' implement it if there is one.
                        ' In order to determine this, we need the application
                        ' definition from the process that ultimately owns this step,
                        ' so we know what element type and application type we are
                        ' dealing with and therefore, what actions are available
                        ' to us.
                        Dim altId As String = Nothing
                        Dim altArgs As IDictionary(Of String, String) = Nothing
                        Dim appInfo As clsApplicationTypeInfo = Nothing
                        Dim elTypeInfo As clsElementTypeInfo = Nothing

                        Dim appDefn _
                         As clsApplicationDefinition = ApplicationDefinition

                        Dim el As clsApplicationElement = Nothing

                        If appDefn IsNot Nothing Then _
                         el = appDefn.FindElement(ElementId)

                        If el IsNot Nothing Then
                            appInfo = appDefn.ApplicationInfo
                            elTypeInfo = el.Type

                            If id IsNot Nothing AndAlso clsAMI.IsValidReadAction(id, _
                             elTypeInfo, appInfo, altId, altArgs, Nothing, Nothing) Then
                                If altId IsNot Nothing Then
                                    id = altId
                                    For Each argId As String In altArgs.Keys
                                        ArgumentValues(argId) = altArgs(argId)
                                    Next
                                End If
                            End If

                        Else
                            ' The element can be empty if the element has been
                            ' deleted from the model but there are still stages
                            ' referring to it.
                            ' Very little we can do - just leave it as is

                        End If

                        Action = clsAMI.GetActionTypeInfo(id)

                        If Action Is Nothing Then Throw New NoSuchElementException(
                         CStr(IIf(altId IsNot Nothing, My.Resources.Resources.clsReadStep_AMIDidNotRecogniseTheAlternativeActionType0, My.Resources.Resources.clsReadStep_AMIDidNotRecogniseTheActionType0)), id)

                    Case "arguments"
                        clsActionStep.AddArgumentsFromXML(Me, grandchild)

                End Select
            Next

        Next

        Stage = e.GetAttribute("stage")

        ' backwards compatibility code
        If Action Is Nothing Then _
         Action = clsAMI.GetActionTypeInfo("ReadCurrentValue")

    End Sub

    ''' <summary>
    ''' Writes this step data out into the given element, within the specified
    ''' document.
    ''' </summary>
    ''' <param name="doc">The XML document to which this step is being written.
    ''' </param>
    ''' <param name="stepEl">The XML element representing this step in the document.
    ''' </param>
    Public Overrides Sub ToXML(ByVal doc As XmlDocument, ByVal stepEl As XmlElement)
        Dim child As XmlElement = Nothing

        'Add target element
        If Not Me.ElementId.Equals(Guid.Empty) Then
            child = doc.CreateElement("element")
            child.SetAttribute("id", Me.ElementId.ToString)
            'Add parameters
            If Not ((Me.Parameters Is Nothing) OrElse (Me.Parameters.Count = 0)) Then
                For Each p As clsApplicationElementParameter In Me.Parameters
                    child.AppendChild(p.ToXML(doc))
                Next
            End If
            stepEl.AppendChild(child)
        End If

        'Add data stage
        If mStage <> "" Then stepEl.SetAttribute("stage", mStage)

        'Add read action
        If Action IsNot Nothing Then
            child = doc.CreateElement("action")

            'Add(ID)
            Dim child2 As XmlElement = doc.CreateElement("id")
            child2.InnerText = ActionId
            child.AppendChild(child2)

            'Add arguments
            clsActionStep.ArgumentsToXML(Me, doc, child)
            stepEl.AppendChild(child)
        End If
    End Sub

    ''' <summary>
    ''' Checks for errors.
    ''' </summary>
    ''' <param name="parentStage">The stage owning this step.</param>
    ''' <param name="rowNo">A text-representation of the one-based index of this
    ''' step, in the parent stage's collection.</param>
    ''' <param name="attemptRepair">If true then any detected errors will be automatically
    ''' repaired, where possible.</param>
    ''' <returns>Returns a List of errors found, which may be empty but will never
    ''' be null.</returns>
    Friend Function CheckForErrors(ByVal parentStage As clsAppStage, ByVal rowNo As Integer, ByVal attemptRepair As Boolean) As ValidationErrorList
        Dim errors As New ValidationErrorList()

        If ActionId = "" Then
            errors.Add(New ValidateProcessResult(parentStage, "frmStagePropertiesRead.htm", 78, rowNo))
        Else
            Dim targetElement As clsApplicationElement = parentStage.Process.ApplicationDefinition.FindElement(Me.ElementId)
            If targetElement IsNot Nothing Then

                'Check read action is valid
                Dim appInfo As clsApplicationTypeInfo = parentStage.Process.ApplicationDefinition.ApplicationInfo
                Dim altId As String = Nothing,
                 helpMessage As String = Nothing, helpTopic As Integer
                Dim altArgs As IDictionary(Of String, String) = Nothing

                If Not clsAMI.IsValidReadAction(ActionId, targetElement.Type,
                 appInfo, altId, altArgs, helpMessage, helpTopic) Then
                    errors.Add(New ValidateProcessResult(parentStage,
                     "frmStagePropertiesRead.htm", 79, rowNo))
                Else
                    'Check whether alternative read action was suggested
                    If altId IsNot Nothing Then
                        If attemptRepair Then
                            Action = clsAMI.GetActionTypeInfo(altId, appInfo)
                            For Each pair As KeyValuePair(Of String, String) In altArgs
                                ArgumentValues(pair.Key) = pair.Value
                            Next
                        Else
                            Dim helpRef As String
                            If helpTopic > 0 Then
                                helpRef = "helpTopicsByNumber.htm#Topic" & helpTopic.ToString
                            Else
                                helpRef = "frmStagePropertiesNavigate.htm"
                            End If
                            errors.Add(New RepairableValidateProcessResult(parentStage, helpRef, 80, rowNo))
                        End If
                    End If

                    'Check 'store-in' location
                    Dim theAction = clsAMI.GetActionTypeInfo(ActionId)
                    If theAction.ReturnType = "collection" Then
                        Dim targetColStage = parentStage.Process.GetCollectionStage(Me.Stage, Nothing, False)
                        If targetColStage Is Nothing Then
                            errors.Add(New ValidateProcessResult(parentStage, "frmStagePropertiesRead.htm", 81, rowNo, Me.Stage))
                        ElseIf targetColStage.Definition IsNot Nothing Then
                            errors.Add(New ValidateProcessResult(parentStage, "helpCollections.htm", 82, rowNo, Me.Stage))
                        End If
                    Else
                        If String.IsNullOrEmpty(Me.Stage) Then
                            errors.Add(New ValidateProcessResult(parentStage, "helpCollections.htm", 83, rowNo))
                        Else
                            Dim bOutOfScope As Boolean
                            Dim TargetStage As clsDataStage = parentStage.Process.GetDataStage(Me.Stage, parentStage, bOutOfScope)
                            If TargetStage Is Nothing Then
                                'maybe got here because storing in a collection.
                                'If contains a dot, assume collection, which is ok
                                If String.IsNullOrEmpty(Me.Stage) Then
                                    Dim hint As String = My.Resources.Resources.clsReadStep_StageNameIsNullOrEmpty
                                    errors.Add(New ValidateProcessResult(parentStage, "frmStagePropertiesData.htm", 86, rowNo, Me.Stage, hint))
                                Else
                                    Dim dotIndex As Integer = Me.Stage.IndexOf(".")

                                    If dotIndex = -1 Then
                                        Dim Hint As String = String.Empty
                                        If Me.Stage.StartsWith("[") AndAlso Me.Stage.EndsWith("]") Then
                                            Hint = My.Resources.Resources.clsReadStep_NoteThatDataItemsShouldNotBeSurroundedBySquareBracketsInStoreInFields
                                        End If
                                        errors.Add(New ValidateProcessResult(parentStage, "frmStagePropertiesRead.htm", 84, rowNo, Me.Stage, Hint))
                                    ElseIf dotIndex <> -1 AndAlso bOutOfScope Then
                                        ' rerun through GetDataStage with a the collection parent name 
                                        ' to determine if target is still out of scope
                                        Dim collectionParentName As String = Me.Stage.Substring(0, dotIndex)
                                        parentStage.Process.GetDataStage(collectionParentName, parentStage, bOutOfScope)

                                        If bOutOfScope Then errors.Add(New ValidateProcessResult(parentStage, "frmStagePropertiesData.htm", 85, rowNo, Me.Stage))

                                    ElseIf bOutOfScope Then
                                        errors.Add(New ValidateProcessResult(parentStage, "frmStagePropertiesData.htm", 85, rowNo, Me.Stage))
                                    End If
                                End If
                            Else
                                Select Case TargetStage.Exposure
                                    Case StageExposureType.Environment
                                        errors.Add(New ValidateProcessResult(parentStage, 7))
                                End Select
                            End If
                        End If
                    End If

                    'Check for argument and parameter errors
                    errors.AddRange(clsStep.CheckForArgumentErrors(Me, parentStage, clsAMI.GetActionTypeInfo(ActionId, appInfo).Arguments, rowNo, attemptRepair))
                    errors.AddRange(CheckForElementParameterErrors(parentStage, rowNo))
                End If
            Else
                errors.Add(New ValidateProcessResult(parentStage, "frmStagePropertiesRead.htm", 86, rowNo))
            End If
        End If

        Return errors
    End Function

End Class
