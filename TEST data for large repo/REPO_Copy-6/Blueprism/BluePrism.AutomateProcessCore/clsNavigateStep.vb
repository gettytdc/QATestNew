Imports BluePrism.Server.Domain.Models
Imports BluePrism.AMI
Imports System.Xml
Imports BluePrism.AutomateProcessCore.Stages
Imports System.Runtime.Serialization
Imports BluePrism.ApplicationManager.AMI

<Serializable, DataContract([Namespace]:="bp")>
Public Class clsNavigateStep : Inherits clsStep

    ''' <summary>
    ''' Creates a new empty nav step
    ''' </summary>
    Public Sub New(ByVal stg As clsNavigateStage)
        Me.New(stg, Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new nav step initialised from the given XML element.
    ''' </summary>
    ''' <param name="el">The element from which to draw the nav step data. If this
    ''' is null, an empty nav step is created.</param>
    Public Sub New(ByVal stg As clsNavigateStage, ByVal el As XmlElement)
        Me.New(stg, el, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new nav step initialised from the given XML element.
    ''' </summary>
    ''' <param name="el">The element from which to draw the nav step data. If this
    ''' is null, an empty nav step is created.</param>
    ''' <param name="appType">The application info providing the context that this
    ''' step will reside in</param>
    Public Sub New(ByVal stg As clsNavigateStage, _
     ByVal el As XmlElement, ByVal appType As clsApplicationTypeInfo)
        MyBase.New(stg)
        If el IsNot Nothing Then FromXML(el, appType)
    End Sub

    ''' <summary>
    ''' Parses the supplied xml element and returns the corresponding navigation
    ''' action.
    ''' </summary>
    ''' <param name="e">An xml element with name "NavigationAction".</param>
    Public Overloads Overrides Sub FromXML(ByVal e As XmlElement)
        FromXML(e, Nothing)
    End Sub

    ''' <summary>
    ''' Parses the supplied xml element and returns the corresponding navigation
    ''' action.
    ''' </summary>
    ''' <param name="e">An xml element with name "NavigationAction".</param>
    ''' <param name="appInfo">The application info providing the context that this
    ''' step will reside in</param>
    Public Overloads Sub FromXML( _
     ByVal e As XmlElement, ByVal appInfo As clsApplicationTypeInfo)
        If e.Name = "step" Then

            MyBase.FromXML(e)

            For Each child As XmlElement In e.ChildNodes
                If child.Name <> "action" Then Continue For
                For Each ee As XmlElement In child.ChildNodes
                    Select Case ee.Name
                        Case "id"
                            Dim id As String = ee.FirstChild.Value
                            Action = clsAMI.GetActionTypeInfo(id, appInfo)
                            If Action Is Nothing Then _
                             Throw New NoSuchElementException(
                             My.Resources.Resources.clsNavigateStep_AMIDidNotRecogniseTheActionType0, id)

                        Case "arguments"
                            clsActionStep.AddArgumentsFromXML(Me, ee)
                        Case "outputs"
                            clsActionStep.AddOutputsFromXML(Me, ee)

                    End Select
                Next
            Next
        End If
    End Sub


    ''' <summary>
    ''' Generates an xml element representing the current instance.
    ''' </summary>
    ''' <param name="doc">The document with which to create the returned
    ''' value.</param>
    Public Overrides Sub ToXML(ByVal doc As XmlDocument, ByVal StepElement As XmlElement)
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
            StepElement.AppendChild(child)
        End If

        'Add action
        If Not Me.Action Is Nothing Then
            child = doc.CreateElement("action")
            Dim child2 As Xml.XmlElement

            'Add(ID)
            child2 = doc.CreateElement("id")
            child2.InnerText = ActionId
            child.AppendChild(child2)

            'Add arguments
            clsActionStep.ArgumentsToXML(Me, doc, child)
            clsActionStep.OutputsToXML(Me, doc, child)
            StepElement.AppendChild(child)
        End If
    End Sub


    ''' <summary>
    ''' Checks for errors.
    ''' </summary>
    ''' <param name="parentStage">The stage owning this step.</param>
    ''' <param name="rowNo">The one-based index of this step, in the parent stage's
    ''' collection.</param>
    ''' <param name="attemptRepair">If True then any detected errors will be
    ''' automatically repaired, where possible.</param>
    ''' <returns>Returns a List of errors found, which may be empty but will never
    ''' be Nothing.</returns>
    Friend Function CheckForErrors(ByVal parentStage As clsAppStage, ByVal rowNo As Integer, ByVal attemptRepair As Boolean) As ValidationErrorList
        Dim errors As New ValidationErrorList()

        'Check that the chosen action is permitted for the chosen element
        If ElementId <> Guid.Empty Then
            Dim targetElement As clsApplicationElement = parentStage.Process.ApplicationDefinition.FindElement(Me.ElementId)
            If targetElement IsNot Nothing Then

                If Action IsNot Nothing Then
                    'Make sure the chosen action is allowed for this element type
                    If targetElement.Type IsNot Nothing Then
                        Dim appInfo As clsApplicationTypeInfo = parentStage.Process.ApplicationDefinition.ApplicationInfo
                        Dim alternative As String = Nothing, helpMessage As String = Nothing, helpTopic As Integer
                        If Not clsAMI.IsValidAction(Me.ActionId, targetElement.Type, appInfo, alternative, helpMessage, helpTopic) Then
                            errors.Add(New ValidateProcessResult(parentStage, 15, rowNo))
                        Else
                            If alternative IsNot Nothing Then
                                If attemptRepair Then
                                    Action = clsAMI.GetActionTypeInfo(alternative, appInfo)
                                Else
                                    Dim helpRef As String
                                    If helpTopic <> 0 Then
                                        helpRef = "helpTopicsByNumber.htm#Topic" & helpTopic.ToString()
                                    Else
                                        helpRef = "frmStagePropertiesNavigate.htm"
                                    End If
                                    errors.Add(New RepairableValidateProcessResult( _
                                     parentStage, helpRef, 16, rowNo))
                                End If
                            End If

                            'Finally, check for argument and parameter errors
                            errors.AddRange(clsStep.CheckForArgumentErrors(Me, parentStage, clsAMI.GetActionTypeInfo(Me.ActionId, appInfo).Arguments, rowNo, attemptRepair))
                            errors.AddRange(CheckForElementParameterErrors(parentStage, rowNo))
                        End If
                    Else
                        errors.Add(New ValidateProcessResult(parentStage, 17, rowNo))
                    End If
                Else
                    errors.Add(New ValidateProcessResult(parentStage, 18, rowNo))
                End If
            Else
                errors.Add(New ValidateProcessResult(parentStage, 19, rowNo, Me.ElementId, Me))
            End If
        Else
            errors.Add(New ValidateProcessResult(parentStage, 20, rowNo))
        End If

        Return errors
    End Function


End Class
