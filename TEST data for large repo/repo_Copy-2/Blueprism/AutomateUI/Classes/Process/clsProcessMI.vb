Imports System.Xml
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.AutomateAppCore
Imports BluePrism.Core.Xml
Imports LocaleTools
Imports System.Globalization
Imports BluePrism.BPCoreLib
Imports BluePrism.Server.Domain.Models

''' Project: Automate
''' Class: clsProcessMI
''' <summary>
''' A data reading and storage class to work in conjunction with 
''' frmProcessMI and clsRenderer. Search parameters are passed in 
''' from the form and session data is read from the DB. This data
''' is then used to display MI on the process diagram. The data 
''' can also be saved to a file.
''' </summary>
Public Class clsProcessMI


    ''' <summary>
    ''' The data document
    ''' </summary>
    Private mxDocument As XmlDocument

    ''' <summary>
    ''' The previously referenced element of the xml document.
    ''' </summary>
    Private mxePreviousStage As XmlElement


    Private meFilters As XmlElement

    ''' <summary>
    ''' The process object
    ''' </summary>
    Public ReadOnly Property Process() As clsProcess
        Get
            Return mProcess
        End Get
    End Property
    Private mProcess As clsProcess

    ' Look up tables for localised friendly names
    Private mOldText As String()
    Private mNewText As String()

    ''' <summary>
    ''' Indicates that there is MI data in the object.
    ''' </summary>
    Public DataExists As Boolean

    Public Enum AttributeType
        count
        average
        total
        [true]
        [false]
        maximum
        minimum
    End Enum


    ''' <summary>
    ''' Sets up an xml document to hold the MI data.
    ''' </summary>
    ''' <param name="proc">The process</param>
    Public Sub New(ByVal proc As clsProcess, oldText As String(), newText As String())

        Dim oCalculationStage As clsCalculationStage
        Dim oResultStage As clsDataStage

        mProcess = proc
        mOldText = oldText
        mNewText = newText

        mxDocument = New XmlDocument

        'Build up the xml like this.
        'search
        '  resources
        '    resource
        '  sessions
        '    session
        '  process
        '    pages
        '      page
        '        actions
        '          action
        '        calculations
        '          calculation
        '        multiplecalculations
        '          multiplecalculation
        '        decisions
        '          decision
        '        subprocesses
        '          subprocess
        '        subsheets
        '          subsheet
        '        choices
        '          choice
        '        starts
        '          start
        '        ends
        '          end
        '        alerts
        '          alert
        '        reads
        '          read
        '        writes
        '          write
        '        navigates
        '          navigate
        '        waits
        '          wait
        '       exceptions
        '           exception
        '       recovers
        '           recover
        '       resumes
        '           resume

        Dim xeMI As XmlElement = Nothing
        Dim xeResources As XmlElement = Nothing
        Dim xeSessions As XmlElement = Nothing
        Dim xeProcess As XmlElement = Nothing
        Dim xePages As XmlElement = Nothing
        Dim xePage As XmlElement = Nothing
        Dim xeStage As XmlElement = Nothing

        Dim objActions As XmlNode = Nothing
        Dim objSkills As XmlNode = Nothing
        Dim objCalculations As XmlNode = Nothing
        Dim objMultipleCalculations As XmlNode = Nothing
        Dim objDecisions As XmlNode = Nothing
        Dim objSubProcesses As XmlNode = Nothing
        Dim objSubSheets As XmlNode = Nothing
        Dim objChoices As XmlNode = Nothing
        Dim objStarts As XmlNode = Nothing
        Dim objEnds As XmlNode = Nothing
        Dim objAlerts As XmlNode = Nothing
        Dim objCodes As XmlNode = Nothing
        Dim objReads As XmlNode = Nothing
        Dim objWrites As XmlNode = Nothing
        Dim objNavigates As XmlNode = Nothing
        Dim objWaits As XmlNode = Nothing
        Dim objExceptions As XmlNode = Nothing
        Dim objRecovers As XmlNode = Nothing
        Dim objResumes As XmlNode = Nothing

        xeMI = mxDocument.CreateElement("search")
        xeMI.SetAttribute("start", "")
        xeMI.SetAttribute("end", "")
        mxDocument.AppendChild(xeMI)

        xeResources = mxDocument.CreateElement("resources")
        xeMI.AppendChild(xeResources)

        xeSessions = mxDocument.CreateElement("sessions")
        xeMI.AppendChild(xeSessions)

        meFilters = mxDocument.CreateElement("filters")
        xeMI.AppendChild(meFilters)

        'Add a set of group nodes to each page in the process
        For Each oSubSheet As clsProcessSubSheet In proc.SubSheets
            xePage = mxDocument.CreateElement("page")
            xePage.SetAttribute("id", oSubSheet.ID.ToString)
            xePage.SetAttribute("name", LTools.GetC(proc.GetSubSheetName(oSubSheet.ID), "misc", "page"))
            xePage.SetAttribute("inuse", "")
            meFilters.AppendChild(xePage)
            Dim xeGroup As XmlElement
            xeGroup = mxDocument.CreateElement("actions")
            xeGroup.SetAttribute("inuse", "")
            objActions = xePage.AppendChild(xeGroup)

            xeGroup = mxDocument.CreateElement("skills")
            xeGroup.SetAttribute("inuse", "")
            objSkills = xePage.AppendChild(xeGroup)

            xeGroup = mxDocument.CreateElement("calculations")
            xeGroup.SetAttribute("inuse", "")
            objCalculations = xePage.AppendChild(xeGroup)

            xeGroup = mxDocument.CreateElement("multiplecalculations")
            xeGroup.SetAttribute("inuse", "")
            objMultipleCalculations = xePage.AppendChild(xeGroup)

            xeGroup = mxDocument.CreateElement("decisions")
            xeGroup.SetAttribute("inuse", "")
            objDecisions = xePage.AppendChild(xeGroup)

            xeGroup = mxDocument.CreateElement("subprocesses")
            xeGroup.SetAttribute("inuse", "")
            objSubProcesses = xePage.AppendChild(xeGroup)

            xeGroup = mxDocument.CreateElement("subsheets")
            xeGroup.SetAttribute("inuse", "")
            objSubSheets = xePage.AppendChild(xeGroup)

            xeGroup = mxDocument.CreateElement("choices")
            xeGroup.SetAttribute("inuse", "")
            objChoices = xePage.AppendChild(xeGroup)

            xeGroup = mxDocument.CreateElement("starts")
            xeGroup.SetAttribute("inuse", "")
            objStarts = xePage.AppendChild(xeGroup)

            xeGroup = mxDocument.CreateElement("ends")
            xeGroup.SetAttribute("inuse", "")
            objEnds = xePage.AppendChild(xeGroup)

            xeGroup = mxDocument.CreateElement("alerts")
            xeGroup.SetAttribute("inuse", "")
            objAlerts = xePage.AppendChild(xeGroup)

            xeGroup = mxDocument.CreateElement("codes")
            xeGroup.SetAttribute("inuse", "")
            objCodes = xePage.AppendChild(xeGroup)

            xeGroup = mxDocument.CreateElement("reads")
            xeGroup.SetAttribute("inuse", "")
            objReads = xePage.AppendChild(xeGroup)

            xeGroup = mxDocument.CreateElement("writes")
            xeGroup.SetAttribute("inuse", "")
            objWrites = xePage.AppendChild(xeGroup)

            xeGroup = mxDocument.CreateElement("navigates")
            xeGroup.SetAttribute("inuse", "")
            objNavigates = xePage.AppendChild(xeGroup)

            xeGroup = mxDocument.CreateElement("waits")
            xeGroup.SetAttribute("inuse", "")
            objWaits = xePage.AppendChild(xeGroup)

            xeGroup = mxDocument.CreateElement("exceptions")
            xeGroup.SetAttribute("inuse", "")
            objExceptions = xePage.AppendChild(xeGroup)

            xeGroup = mxDocument.CreateElement("recovers")
            xeGroup.SetAttribute("inuse", "")
            objRecovers = xePage.AppendChild(xeGroup)

            xeGroup = mxDocument.CreateElement("resume")
            xeGroup.SetAttribute("inuse", "")
            objResumes = xePage.AppendChild(xeGroup)


            '############################
            '
            'Additional stage types here.
            '
            '############################
        Next

        For Each oStage As clsProcessStage In mProcess.GetStages()
            For Each nodPage As XmlNode In meFilters.ChildNodes
                If oStage.GetSubSheetID.ToString = nodPage.Attributes("id").Value Then
                    xeStage = mxDocument.CreateElement(oStage.StageType.ToString().ToLower())
                    xeStage.SetAttribute("id", oStage.GetStageID.ToString)
                    xeStage.SetAttribute("name", LTools.GetC(oStage.GetName, "misc", "stage"))
                    xeStage.SetAttribute("inuse", "")
                    Select Case oStage.StageType
                        Case StageTypes.Action
                            objActions.AppendChild(xeStage)
                        Case StageTypes.Skill
                            objSkills.AppendChild(xeStage)
                        Case StageTypes.Alert
                            objAlerts.AppendChild(xeStage)
                        Case StageTypes.Calculation
                            objCalculations.AppendChild(xeStage)
                        Case StageTypes.MultipleCalculation
                            objMultipleCalculations.AppendChild(xeStage)
                        Case StageTypes.ChoiceStart
                            objChoices.AppendChild(xeStage)
                        Case StageTypes.Code
                            objCodes.AppendChild(xeStage)
                        Case StageTypes.Decision
                            objDecisions.AppendChild(xeStage)
                        Case StageTypes.End
                            objEnds.AppendChild(xeStage)
                        Case StageTypes.Exception
                            objExceptions.AppendChild(xeStage)
                        Case StageTypes.Navigate
                            objNavigates.AppendChild(xeStage)
                        Case StageTypes.Process
                            objSubProcesses.AppendChild(xeStage)
                        Case StageTypes.Read
                            objReads.AppendChild(xeStage)
                        Case StageTypes.Recover
                            objRecovers.AppendChild(xeStage)
                        Case StageTypes.Resume
                            objResumes.AppendChild(xeStage)
                        Case StageTypes.Start
                            objStarts.AppendChild(xeStage)
                        Case StageTypes.SubSheet
                            objSubSheets.AppendChild(xeStage)
                        Case StageTypes.WaitStart
                            objWaits.AppendChild(xeStage)
                        Case StageTypes.Write
                            objWrites.AppendChild(xeStage)
                    End Select
                End If
            Next
        Next


        xeProcess = mxDocument.CreateElement("process")
        xeProcess.SetAttribute("name", proc.Name)
        xeMI.AppendChild(xeProcess)

        xePages = mxDocument.CreateElement("pages")
        xeProcess.AppendChild(xePages)



        'Add a set of group nodes to each page in the process
        For Each oSubSheet As clsProcessSubSheet In proc.SubSheets
            xePage = mxDocument.CreateElement("page")
            xePage.SetAttribute("id", oSubSheet.ID.ToString)
            xePage.SetAttribute("name", LTools.GetC(proc.GetSubSheetName(oSubSheet.ID), "misc", "page"))
            xePages.AppendChild(xePage)

            objActions = xePage.AppendChild(mxDocument.CreateElement("actions"))
            objSkills = xePage.AppendChild(mxDocument.CreateElement("skills"))
            objCalculations = xePage.AppendChild(mxDocument.CreateElement("calculations"))
            objMultipleCalculations = xePage.AppendChild(mxDocument.CreateElement("multiplecalculations"))
            objDecisions = xePage.AppendChild(mxDocument.CreateElement("decisions"))
            objSubProcesses = xePage.AppendChild(mxDocument.CreateElement("subprocesses"))
            objSubSheets = xePage.AppendChild(mxDocument.CreateElement("subsheets"))
            objChoices = xePage.AppendChild(mxDocument.CreateElement("choices"))
            objStarts = xePage.AppendChild(mxDocument.CreateElement("starts"))
            objEnds = xePage.AppendChild(mxDocument.CreateElement("ends"))
            objAlerts = xePage.AppendChild(mxDocument.CreateElement("alerts"))
            objCodes = xePage.AppendChild(mxDocument.CreateElement("codes"))
            objReads = xePage.AppendChild(mxDocument.CreateElement("reads"))
            objWrites = xePage.AppendChild(mxDocument.CreateElement("writes"))
            objNavigates = xePage.AppendChild(mxDocument.CreateElement("navigates"))
            objWaits = xePage.AppendChild(mxDocument.CreateElement("waits"))
            objExceptions = xePage.AppendChild(mxDocument.CreateElement("exceptions"))
            objRecovers = xePage.AppendChild(mxDocument.CreateElement("recovers"))
            objResumes = xePage.AppendChild(mxDocument.CreateElement("resumes"))

            '############################
            '
            'Additional stage types here.
            '
            '############################
        Next

        For Each oStage As clsProcessStage In mProcess.GetStages()

            If oStage.StageType = StageTypes.Action _
            Or oStage.StageType = StageTypes.Skill _
            Or oStage.StageType = StageTypes.Calculation _
            Or oStage.StageType = StageTypes.MultipleCalculation _
            Or oStage.StageType = StageTypes.Decision _
            Or oStage.StageType = StageTypes.Process _
            Or oStage.StageType = StageTypes.SubSheet _
            Or oStage.StageType = StageTypes.ChoiceStart _
            Or oStage.StageType = StageTypes.Start _
            Or oStage.StageType = StageTypes.End _
            Or oStage.StageType = StageTypes.Alert _
            Or oStage.StageType = StageTypes.Code _
            Or oStage.StageType = StageTypes.Read _
            Or oStage.StageType = StageTypes.Write _
            Or oStage.StageType = StageTypes.Navigate _
            Or oStage.StageType = StageTypes.WaitStart _
            Or oStage.StageType = StageTypes.Exception _
            Or oStage.StageType = StageTypes.Recover _
            Or oStage.StageType = StageTypes.Resume _
            Then

                '############################
                '
                'Additional stage types here.
                '
                '############################

                For Each nodPage As XmlNode In xePages.ChildNodes

                    If oStage.GetSubSheetID.ToString = nodPage.Attributes("id").Value Then

                        Select Case oStage.StageType

                            Case StageTypes.Action, StageTypes.Skill
                                xeStage = mxDocument.CreateElement(oStage.StageType.ToString().ToLower())
                                xeStage.SetAttribute("id", oStage.GetStageID.ToString)
                                xeStage.SetAttribute("name", oStage.GetName)
                                xeStage.SetAttribute("count", "")
                                xeStage.SetAttribute("total", "")
                                xeStage.SetAttribute("average", "")
                                objActions.AppendChild(xeStage)

                            Case StageTypes.Decision
                                xeStage = mxDocument.CreateElement(oStage.StageType.ToString().ToLower())
                                xeStage.SetAttribute("id", oStage.GetStageID.ToString)
                                xeStage.SetAttribute("name", oStage.GetName)
                                xeStage.SetAttribute("count", "")
                                xeStage.SetAttribute("true", "")
                                xeStage.SetAttribute("false", "")
                                objDecisions.AppendChild(xeStage)

                            Case StageTypes.Calculation

                                xeStage = mxDocument.CreateElement(oStage.StageType.ToString().ToLower())
                                xeStage.SetAttribute("id", oStage.GetStageID.ToString)
                                xeStage.SetAttribute("name", oStage.GetName)
                                xeStage.SetAttribute("count", "")

                                oCalculationStage = CType(oStage, Stages.clsCalculationStage)

                                Dim aResultStages As clsProcessStage() = mProcess.GetStages(oCalculationStage.StoreIn)

                                oResultStage = Nothing
                                If Not aResultStages Is Nothing Then
                                    For j As Integer = 0 To aResultStages.Length - 1
                                        If aResultStages(j).StageType = StageTypes.Data Then
                                            oResultStage = CType(aResultStages(j), Stages.clsDataStage)
                                            Exit For
                                        End If
                                    Next
                                End If

                                If oResultStage Is Nothing Then
                                    xeStage.SetAttribute("dataType", "unknown")
                                Else
                                    xeStage.SetAttribute("dataType", oResultStage.GetDataType.ToString)
                                End If

                                xeStage.SetAttribute("true", "")
                                xeStage.SetAttribute("false", "")
                                xeStage.SetAttribute("maximum", "")
                                xeStage.SetAttribute("minimum", "")
                                xeStage.SetAttribute("total", "")
                                objCalculations.AppendChild(xeStage)

                            Case StageTypes.MultipleCalculation

                                xeStage = mxDocument.CreateElement(oStage.StageType.ToString().ToLower())
                                xeStage.SetAttribute("id", oStage.GetStageID.ToString)
                                xeStage.SetAttribute("name", oStage.GetName)
                                xeStage.SetAttribute("count", "")
                                objMultipleCalculations.AppendChild(xeStage)

                            Case StageTypes.Process
                                xeStage = mxDocument.CreateElement("subprocess")
                                xeStage.SetAttribute("id", oStage.GetStageID().ToString())
                                xeStage.SetAttribute("name", oStage.GetName)
                                xeStage.SetAttribute("count", "")
                                xeStage.SetAttribute("total", "")
                                xeStage.SetAttribute("average", "")
                                objSubProcesses.AppendChild(xeStage)

                            Case StageTypes.SubSheet
                                xeStage = mxDocument.CreateElement(oStage.StageType.ToString().ToLower())
                                xeStage.SetAttribute("id", oStage.GetStageID.ToString)
                                xeStage.SetAttribute("name", LTools.Get(oStage.GetName, "misc", "stage"))
                                xeStage.SetAttribute("count", "")
                                xeStage.SetAttribute("total", "")
                                xeStage.SetAttribute("average", "")
                                objSubSheets.AppendChild(xeStage)

                            Case StageTypes.ChoiceStart

                                'ChoiceStarts are handled slightly differently. To 
                                'create a unique id for the choices, the choice position 
                                'number is appended to the guid string. The ChoiceEnd is
                                'treated as another one of the choices.
                                'NB May need to review this arrangement.
                                Dim oChoiceStart As clsChoiceStartStage
                                Dim oChoice As clsChoice
                                Dim oChoiceEnd As clsChoiceEndStage

                                oChoiceStart = CType(oStage, Stages.clsChoiceStartStage)
                                For c As Integer = 1 To oChoiceStart.Choices.Count
                                    oChoice = oChoiceStart.Choices(c - 1)
                                    xeStage = mxDocument.CreateElement("choice")
                                    xeStage.SetAttribute("id", oChoiceStart.GetStageID.ToString & CStr(c))
                                    xeStage.SetAttribute("name", oChoice.Name)
                                    xeStage.SetAttribute("count", "")
                                    xeStage.SetAttribute("true", "")
                                    objChoices.AppendChild(xeStage)
                                Next
                                oChoiceEnd = oChoiceStart.Process.GetChoiceEnd(oChoiceStart)
                                If Not oChoiceEnd Is Nothing Then
                                    xeStage = mxDocument.CreateElement("choice")
                                    xeStage.SetAttribute("id", oChoiceStart.GetStageID.ToString & CStr(oChoiceStart.Choices.Count + 1))
                                    xeStage.SetAttribute("name", oChoiceEnd.GetName)
                                    xeStage.SetAttribute("count", "")
                                    xeStage.SetAttribute("true", "")
                                    objChoices.AppendChild(xeStage)
                                End If


                            Case StageTypes.Start
                                xeStage = mxDocument.CreateElement("start")
                                xeStage.SetAttribute("id", oStage.GetStageID.ToString)
                                xeStage.SetAttribute("name", LTools.GetC(oStage.GetName, "misc", "stage"))
                                xeStage.SetAttribute("count", "")
                                objStarts.AppendChild(xeStage)


                            Case StageTypes.End
                                xeStage = mxDocument.CreateElement("end")
                                xeStage.SetAttribute("id", oStage.GetStageID.ToString)
                                xeStage.SetAttribute("name", LTools.GetC(oStage.GetName, "misc", "stage"))
                                xeStage.SetAttribute("count", "")
                                objEnds.AppendChild(xeStage)

                            Case StageTypes.Alert
                                xeStage = mxDocument.CreateElement("alert")
                                xeStage.SetAttribute("id", oStage.GetStageID.ToString)
                                xeStage.SetAttribute("name", oStage.GetName)
                                xeStage.SetAttribute("count", "")
                                objAlerts.AppendChild(xeStage)

                            Case StageTypes.Code
                                xeStage = mxDocument.CreateElement("code")
                                xeStage.SetAttribute("id", oStage.GetStageID.ToString)
                                xeStage.SetAttribute("name", oStage.GetName)
                                xeStage.SetAttribute("count", "")
                                objCodes.AppendChild(xeStage)

                            Case StageTypes.Read
                                xeStage = mxDocument.CreateElement("read")
                                xeStage.SetAttribute("id", oStage.GetStageID.ToString)
                                xeStage.SetAttribute("name", oStage.GetName)
                                xeStage.SetAttribute("count", "")
                                objReads.AppendChild(xeStage)

                            Case StageTypes.Write
                                xeStage = mxDocument.CreateElement("write")
                                xeStage.SetAttribute("id", oStage.GetStageID.ToString)
                                xeStage.SetAttribute("name", oStage.GetName)
                                xeStage.SetAttribute("count", "")
                                objWrites.AppendChild(xeStage)

                            Case StageTypes.Navigate
                                xeStage = mxDocument.CreateElement("navigate")
                                xeStage.SetAttribute("id", oStage.GetStageID.ToString)
                                xeStage.SetAttribute("name", oStage.GetName)
                                xeStage.SetAttribute("count", "")
                                objNavigates.AppendChild(xeStage)

                            Case StageTypes.WaitStart

                                'WaitStarts are handled slightly differently. To 
                                'create a unique id for the choices, the choice position 
                                'number is appended to the guid string. The WaitEnd is
                                'treated as another one of the choices.
                                'NB May need to review this arrangement.
                                Dim oWaitStart As clsWaitStartStage
                                Dim oChoice As clsChoice
                                Dim gWaitEnd As Guid

                                oWaitStart = CType(oStage, Stages.clsWaitStartStage)
                                For c As Integer = 1 To oWaitStart.Choices.Count
                                    oChoice = oWaitStart.Choices(c - 1)
                                    xeStage = mxDocument.CreateElement("wait")
                                    xeStage.SetAttribute("id", oWaitStart.GetStageID.ToString & CStr(c))
                                    xeStage.SetAttribute("name", oChoice.Name)
                                    xeStage.SetAttribute("count", "")
                                    xeStage.SetAttribute("true", "")
                                    objWaits.AppendChild(xeStage)
                                Next
                                gWaitEnd = oWaitStart.Process.GetChoiceEnd(oWaitStart).GetStageID
                                If Not gWaitEnd.Equals(Guid.Empty) Then
                                    xeStage = mxDocument.CreateElement("wait")
                                    xeStage.SetAttribute("id", oWaitStart.GetStageID.ToString & CStr(oWaitStart.Choices.Count + 1))
                                    xeStage.SetAttribute("name", oWaitStart.Process.GetChoiceEnd(oWaitStart).GetName)
                                    xeStage.SetAttribute("count", "")
                                    xeStage.SetAttribute("true", "")
                                    objWaits.AppendChild(xeStage)
                                End If

                            Case StageTypes.Exception
                                xeStage = mxDocument.CreateElement("exception")
                                xeStage.SetAttribute("id", oStage.GetStageID.ToString)
                                xeStage.SetAttribute("name", oStage.GetName)
                                xeStage.SetAttribute("count", "")
                                objExceptions.AppendChild(xeStage)

                            Case StageTypes.Recover
                                xeStage = mxDocument.CreateElement("recover")
                                xeStage.SetAttribute("id", oStage.GetStageID.ToString)
                                xeStage.SetAttribute("name", oStage.GetName)
                                xeStage.SetAttribute("count", "")
                                objRecovers.AppendChild(xeStage)

                            Case StageTypes.Resume
                                xeStage = mxDocument.CreateElement("resume")
                                xeStage.SetAttribute("id", oStage.GetStageID.ToString)
                                xeStage.SetAttribute("name", oStage.GetName)
                                xeStage.SetAttribute("count", "")
                                objResumes.AppendChild(xeStage)

                                '############################
                                '
                                'Additional stage types here.
                                '
                                '############################


                        End Select

                        Exit For
                    End If
                Next
            End If
        Next

    End Sub

    ''' <summary>
    ''' Sets a filter to inuse
    ''' </summary>
    ''' <param name="sStageID">The stage to set in use</param>
    ''' <param name="bValue">The value of the inuse attribute</param>
    Public Sub SetInUse(ByVal sStageID As String, ByVal bValue As Boolean)
        Dim xeStage As XmlElement

        If Not mxePreviousStage Is Nothing AndAlso mxePreviousStage.Attributes("id").Value = sStageID Then
            xeStage = mxePreviousStage
        Else
            xeStage = GetElementFromID("filters", sStageID)
        End If

        If xeStage IsNot Nothing Then
            If xeStage.Attributes("inuse") Is Nothing Then
                Throw New InvalidOperationException(My.Resources.InuseAttributeNotFound)
            Else
                xeStage.Attributes("inuse").Value = bValue.ToString
            End If
            mxePreviousStage = xeStage
        End If
    End Sub

    ''' <summary>
    ''' Gets whether the filter is inuse
    ''' </summary>
    ''' <param name="sStageID">The stage to get in use</param>
    ''' <returns>The value of the inuse attribute</returns>
    Public Function GetInUse(ByVal sStageID As String) As Boolean
        Dim xeStage As XmlElement
        Dim bResult As Boolean

        If Not mxePreviousStage Is Nothing AndAlso mxePreviousStage.Attributes("id").Value = sStageID Then
            xeStage = mxePreviousStage
        Else
            xeStage = GetElementFromID("filters", sStageID)
        End If

        If xeStage IsNot Nothing Then
            If xeStage.Attributes("inuse") Is Nothing Then
                Throw New InvalidOperationException(My.Resources.InuseAttributeNotFound)
            Else
                bResult = Boolean.Parse(xeStage.Attributes("inuse").Value)
            End If
            mxePreviousStage = xeStage
        End If

        Return bResult
    End Function

    ''' <summary>
    ''' Gets the xml portion representing the filters.
    ''' </summary>
    ''' <returns></returns>
    Public Function GetFilterXML() As String
        Return meFilters.OuterXml
    End Function

    ''' <summary>
    ''' Sets the xml of the filter portion of the document.
    ''' </summary>
    ''' <param name="sXML">The xml representing which filters are in use.</param>
    Public Sub SetFilterXML(ByVal sXML As String)
        Dim x As New ReadableXmlDocument(sXML)

        For Each xNode As XmlNode In x.SelectNodes("/filters/page//*[@id]")
            If xNode.Attributes("inuse") Is Nothing Then
            Else
                Dim sStageID As String = xNode.Attributes("id").Value
                Dim bInUse As Boolean = Boolean.Parse(xNode.Attributes("inuse").Value)
                Me.SetInUse(sStageID, bInUse)
            End If
        Next
    End Sub


    ''' <summary>
    ''' Extracts an attribute value from the xml.
    ''' </summary>
    ''' <param name="gStageID">The node id</param>
    ''' <param name="iAttributeType">The attribute type</param>
    ''' <returns></returns>
    Public Function GetData(ByVal gStageID As Guid, ByVal iAttributeType As AttributeType) As String
        Return GetData(gStageID.ToString, iAttributeType)
    End Function

    ''' <summary>
    ''' Extracts an attribute value from the xml.
    ''' </summary>
    ''' <param name="sStageID">The node id</param>
    ''' <param name="iAttributeType">The attribute type</param>
    ''' <returns></returns>
    Public Function GetData(ByVal sStageID As String, ByVal iAttributeType As AttributeType) As String

        Dim xeStage As XmlElement
        Dim sAttribute As String

        If Not mxePreviousStage Is Nothing AndAlso mxePreviousStage.Attributes("id").Value = sStageID Then
            xeStage = mxePreviousStage
        Else
            xeStage = GetElementFromID("process", sStageID)
        End If

        If xeStage Is Nothing Then
            Return ""
        Else
            sAttribute = iAttributeType.ToString
            If xeStage.Attributes(sAttribute) Is Nothing Then
                Throw New InvalidOperationException(String.Format(My.Resources.Attribute0NotFound, sAttribute))
            Else
                mxePreviousStage = xeStage
                Return xeStage.Attributes(sAttribute).Value
            End If
        End If

    End Function

    ''' <summary>
    ''' Helper function that uses an xpath to get an element via its stageid
    ''' </summary>
    ''' <param name="sStageID">The stage id</param>
    ''' <returns>The element</returns>
    Private Function GetElementFromID(ByVal sSection As String, ByVal sStageID As String) As XmlElement
        Return TryCast(mxDocument.SelectSingleNode("/search/" & sSection & "//*[@id='" & sStageID & "']"), XmlElement)
    End Function


    ''' <summary>
    ''' Sets an attribute value in the xml.
    ''' </summary>
    ''' <param name="sStageID">The node id.</param>
    ''' <param name="iAttributeType">The attribute type</param>
    ''' <param name="sValue"></param>
    Public Sub SetData(ByVal sStageID As String, ByVal iAttributeType As AttributeType, ByVal sValue As String)

        Dim xeStage As XmlElement
        Dim sAttribute As String

        If Not mxePreviousStage Is Nothing AndAlso mxePreviousStage.Attributes("id").Value = sStageID Then
            xeStage = mxePreviousStage
        Else
            xeStage = GetElementFromID("process", sStageID)
        End If

        If xeStage IsNot Nothing Then
            sAttribute = iAttributeType.ToString
            If xeStage.Attributes(sAttribute) Is Nothing Then
                Throw New InvalidOperationException(String.Format(My.Resources.Attribute0NotFound, sAttribute))
            Else
                xeStage.Attributes(sAttribute).Value = sValue
            End If
            mxePreviousStage = xeStage
        End If

    End Sub


    ''' <summary>
    ''' Removes all attribute data except the node ids and names.
    ''' </summary>
    Public Sub ClearData()

        Dim xnlPages As XmlNodeList

        xnlPages = mxDocument.GetElementsByTagName("pages")

        For i As Integer = 0 To xnlPages.Count - 1
            For Each xePage As XmlElement In xnlPages(i).ChildNodes
                For Each xeStageType As XmlElement In xePage.ChildNodes
                    For Each xeStage As XmlElement In xeStageType.ChildNodes
                        For Each xaStage As XmlAttribute In xeStage.Attributes
                            If xaStage.Name = "id" Or xaStage.Name = "name" Then
                                'Keep these attributes
                            Else
                                xaStage.Value = ""
                            End If
                        Next
                    Next
                Next
            Next
        Next
        DataExists = False

    End Sub


    ''' <summary>
    ''' Updates an xml attribute value.
    ''' </summary>
    ''' <param name="sStageID">The node id</param>
    ''' <param name="oMaximum">The value</param>
    Private Sub SetDataMaximum(ByVal sStageID As String, ByVal oMaximum As Object)

        Dim xeStage As XmlElement

        If Not mxePreviousStage Is Nothing AndAlso mxePreviousStage.Attributes("id").Value = sStageID Then
            xeStage = mxePreviousStage
        Else
            xeStage = GetElementFromID("process", sStageID)
        End If

        If xeStage IsNot Nothing Then

            If TypeOf oMaximum Is Integer Then

                Dim iNew As Integer = CInt(oMaximum)
                Dim sCurrent As String = GetData(sStageID, AttributeType.maximum)

                If sCurrent = "" OrElse iNew < CInt(sCurrent) Then
                    SetData(sStageID, AttributeType.maximum, CStr(iNew))
                End If

            ElseIf TypeOf oMaximum Is Double Then

                Dim dNew As Double = CDbl(oMaximum)
                Dim sCurrent As String = GetData(sStageID, AttributeType.maximum)

                If sCurrent = "" OrElse dNew < CDbl(sCurrent) Then
                    SetData(sStageID, AttributeType.maximum, CStr(dNew))
                End If

            ElseIf TypeOf oMaximum Is Date Then

                Dim dNew As Date = CDate(oMaximum)
                Dim sCurrent As String = GetData(sStageID, AttributeType.maximum)

                If sCurrent = "" OrElse dNew < CDate(sCurrent) Then
                    SetData(sStageID, AttributeType.maximum, CStr(dNew))
                End If

            Else
                Throw New ArgumentException(My.Resources.SetDataMaximumWrongDataType)
            End If

        End If

    End Sub


    ''' <summary>
    ''' Updates an xml attribute value.
    ''' </summary>
    ''' <param name="sStageID">The node id</param>
    ''' <param name="oMinimum">The value</param>
    Private Sub SetDataMinimum(ByVal sStageID As String, ByVal oMinimum As Object)

        Dim xeStage As XmlElement

        If Not mxePreviousStage Is Nothing AndAlso mxePreviousStage.Attributes("id").Value = sStageID Then
            xeStage = mxePreviousStage
        Else
            xeStage = GetElementFromID("process", sStageID)
        End If

        If xeStage IsNot Nothing Then

            If TypeOf oMinimum Is Integer Then

                Dim iNew As Integer = CInt(oMinimum)
                Dim sCurrent As String = GetData(sStageID, AttributeType.minimum)

                If sCurrent = "" OrElse iNew > CInt(sCurrent) Then
                    SetData(sStageID, AttributeType.minimum, CStr(iNew))
                End If

            ElseIf TypeOf oMinimum Is Double Then

                Dim dNew As Double = CDbl(oMinimum)
                Dim sCurrent As String = GetData(sStageID, AttributeType.minimum)

                If sCurrent = "" OrElse dNew > CDbl(sCurrent) Then
                    SetData(sStageID, AttributeType.minimum, CStr(dNew))
                End If

            ElseIf TypeOf oMinimum Is Date Then

                Dim dNew As Date = CDate(oMinimum)
                Dim sCurrent As String = GetData(sStageID, AttributeType.minimum)

                If sCurrent = "" OrElse dNew > CDate(sCurrent) Then
                    SetData(sStageID, AttributeType.minimum, CStr(dNew))
                End If

            Else
                Throw New InvalidArgumentException(My.Resources.SetDataMinimumWrongDataType)
            End If
        End If

    End Sub


    ''' <summary>
    ''' Updates an xml attribute value.
    ''' </summary>
    ''' <param name="sStageID">The node id</param>
    ''' <param name="iAttributeType">The attribute type</param>
    ''' <param name="iIncrement">The increment</param>
    Private Sub IncrementData(ByVal sStageID As String, ByVal iAttributeType As AttributeType, ByVal iIncrement As Integer)

        Dim xeStage As XmlElement
        Dim sAttribute As String

        If Not mxePreviousStage Is Nothing AndAlso mxePreviousStage.Attributes("id").Value = sStageID Then
            xeStage = mxePreviousStage
        Else
            xeStage = GetElementFromID("process", sStageID)
        End If

        If xeStage IsNot Nothing Then
            sAttribute = iAttributeType.ToString
            If xeStage.Attributes(sAttribute) Is Nothing Then
                Throw New InvalidOperationException(String.Format(My.Resources.Attribute0NotFound, sAttribute))
            Else
                If xeStage.Attributes(sAttribute).Value = "" Then
                    xeStage.Attributes(sAttribute).Value = CStr(iIncrement)
                Else
                    xeStage.Attributes(sAttribute).Value = CStr(CInt(xeStage.Attributes(sAttribute).Value) + iIncrement)
                End If
            End If
            mxePreviousStage = xeStage
        End If

    End Sub



    ''' <summary>
    ''' Indicates whether the process has any stages that support MI.
    ''' </summary>
    ''' <returns>True if MI type stages exist</returns>
    Public Function StagesExist() As Boolean

        Dim xnlPages As XmlNodeList
        xnlPages = mxDocument.GetElementsByTagName("pages")

        For i As Integer = 0 To xnlPages.Count - 1
            For Each xePage As XmlElement In xnlPages(i).ChildNodes
                For Each xeStageType As XmlElement In xePage.ChildNodes
                    If xeStageType.ChildNodes.Count > 0 Then
                        Return True
                    End If
                Next
            Next
        Next
        Return False

    End Function


    ''' <summary>
    ''' Gets the ids of all stages in the xml.
    ''' </summary>
    ''' <returns>A list of guids</returns>
    Public Function GetStages() As List(Of clsProcessStage)

        Dim aStages As New List(Of clsProcessStage)
        Dim xnlPages As XmlNodeList
        Dim sStageID As String
        Dim oStage As clsProcessStage

        xnlPages = mxDocument.GetElementsByTagName("pages")

        For i As Integer = 0 To xnlPages.Count - 1
            For Each xePage As XmlElement In xnlPages(i).ChildNodes
                For Each xeStageType As XmlElement In xePage.ChildNodes
                    For Each xeStage As XmlElement In xeStageType.ChildNodes
                        sStageID = xeStage.Attributes("id").Value
                        If sStageID.Length = 36 Then
                            oStage = mProcess.GetStage(New Guid(sStageID))
                            aStages.Add(oStage)
                        Else
                            'NB Choice nodes have ID = Guid & Name, so trim the name off.
                            sStageID = sStageID.Substring(0, 36)
                            oStage = mProcess.GetStage(New Guid(sStageID))
                            If Not oStage Is Nothing AndAlso Not aStages.Contains(oStage) Then
                                aStages.Add(oStage)
                            End If
                        End If
                    Next
                Next
            Next
        Next
        Return aStages

    End Function

    ''' <summary>
    ''' Adds the details of a session to the xml.
    ''' </summary>
    ''' <param name="sResource">The session resource name</param>
    ''' <param name="sStart">The start date</param>
    ''' <param name="sEnd">The end date</param>
    ''' <param name="sStatus">The status</param>
    Public Sub AddSession(ByVal sResource As String, ByVal sStart As String, ByVal sEnd As String, ByVal sStatus As String)

        Dim xnlSessions As XmlNodeList
        Dim xnSessions As XmlNode
        Dim xeSession As XmlElement

        xnlSessions = mxDocument.GetElementsByTagName("sessions")
        xnSessions = xnlSessions(0)
        xeSession = mxDocument.CreateElement("session")

        xeSession.SetAttribute("resource", sResource)
        xeSession.SetAttribute("start", sStart)
        xeSession.SetAttribute("end", sEnd)
        xeSession.SetAttribute("status", sStatus)
        xnSessions.AppendChild(xeSession)

    End Sub


    ''' <summary>
    ''' Removes all session details from the xml
    ''' </summary>
    Public Sub ClearSessions()

        Dim xnlSessions As XmlNodeList
        Dim xnSessions As XmlNode

        xnlSessions = mxDocument.GetElementsByTagName("sessions")
        xnSessions = xnlSessions(0)
        xnSessions.RemoveAll()

    End Sub


    ''' <summary>
    ''' Applies the search parameters to the xml data.
    ''' </summary>
    ''' <param name="resources"></param>
    ''' <param name="startDate"></param>
    ''' <param name="endDate"></param>
    Public Sub SetSearchParameters(ByVal resources As ICollection(Of String),
     ByVal startDate As Date, ByVal endDate As Date)

        Dim resNode As XmlNode = mxDocument.GetElementsByTagName("resources")(0)
        Dim newElem As XmlElement = mxDocument.CreateElement("resource")

        ' FIXME: This adds the same element each time - is that valid in XMLDocument?
        For Each sName As String In resources
            newElem.SetAttribute("name", sName)
            resNode.AppendChild(newElem)
        Next

        With mxDocument.GetElementsByTagName("search")(0)
            .Attributes("start").Value = startDate.ToString("u")
            .Attributes("end").Value = endDate.ToString("u")
        End With
    End Sub


    ''' <summary>
    ''' Removes all search details from the xml
    ''' </summary>
    Public Sub ClearSearchParameters()

        Dim xnlResources, xnlMI As XmlNodeList
        Dim xnResources, xnMI As XmlNode

        xnlResources = mxDocument.GetElementsByTagName("resources")
        xnResources = xnlResources(0)
        xnResources.RemoveAll()

        xnlMI = mxDocument.GetElementsByTagName("search")
        xnMI = xnlMI(0)
        xnMI.Attributes("start").Value = ""
        xnMI.Attributes("end").Value = ""

    End Sub


    ''' <summary>
    ''' Gets the xml data.
    ''' Localise the node names and attributes using a working copy
    ''' </summary>
    ''' <returns></returns>
    Public Function GetXML() As String
        Dim workingCopy As XmlNode = mxDocument.Clone()
        LocaliseXML(workingCopy)
        Return workingCopy.OuterXml
    End Function

    ''' <summary>
    ''' Recursively localise a supplied xml document
    ''' </summary>
    ''' <param name="xnSearch"></param>
    Public Sub LocaliseXML(xnSearch As XmlNode)

        For i As Integer = 0 To xnSearch.ChildNodes.Count - 1
            Dim currentNode = xnSearch.ChildNodes(i)
            LocaliseXML(currentNode)
            RenameNode(currentNode, LocalisedNewName(currentNode.Name).Replace(" ", "").ToLower)
        Next

    End Sub

    ''' <summary>
    ''' Rename an individual node by producing a copy as Name is read only
    ''' </summary>
    ''' <param name="node"></param>
    ''' <param name="newName"></param>
    ''' <returns></returns>
    Public Function RenameNode(ByVal node As XmlNode, ByVal newName As String) As XmlNode
        If node.NodeType = XmlNodeType.Element Then
            Dim oldElement As XmlElement = CType(node, XmlElement)
            Dim newElement As XmlElement = node.OwnerDocument.CreateElement(newName, "")

            While oldElement.HasAttributes
                RenameNode(oldElement.Attributes(0), LocalisedNewName(oldElement.Attributes(0).Name).Replace(" ", "").ToLower)
                newElement.SetAttributeNode(oldElement.RemoveAttributeNode(oldElement.Attributes(0)))
            End While

            While oldElement.HasChildNodes
                newElement.AppendChild(oldElement.FirstChild)
            End While

            If oldElement.ParentNode IsNot Nothing Then
                oldElement.ParentNode.ReplaceChild(newElement, oldElement)
            End If

            Return newElement
        Else
            Return Nothing
        End If
    End Function

    ''' <summary>
    ''' Gets a CSV representation of an xml node.
    ''' </summary>
    ''' <param name="xNode"></param>
    ''' <param name="sIndent"></param>
    ''' <param name="bIncludeAttributeNames"></param>
    ''' <returns></returns>
    Private Function GetCSV(ByVal xNode As XmlNode, Optional ByVal sIndent As String = "", Optional ByVal bIncludeAttributeNames As Boolean = False) As String

        Dim sCSV As String = ""
        Dim sAttributeNames As String = ""
        Dim sAttributeValues As String = ""


        If bIncludeAttributeNames Then

            For Each xeAttribute As XmlAttribute In xNode.Attributes
                If xeAttribute.Name <> "id" Then
                    sAttributeNames &= LocalisedNewName(xeAttribute.Name).ToUpper & ","
                End If
            Next
            If sAttributeNames <> "" Then
                sCSV &= sIndent & "," & sAttributeNames & vbCrLf
            End If
        End If

        sCSV &= sIndent & LocalisedNewName(xNode.Name).ToUpper & ","
        For Each xeAttribute As XmlAttribute In xNode.Attributes
            If xeAttribute.Name <> "id" Then
                sAttributeValues &= xeAttribute.Value & ","
            End If
        Next
        If sAttributeValues <> "" Then
            sCSV &= sAttributeValues
        End If

        If xNode.ChildNodes.Count > 0 Then
            For i As Integer = 1 To xNode.ChildNodes.Count
                sCSV &= vbCrLf & GetCSV(xNode.ChildNodes(i - 1), sIndent & ",", (i = 1))
                If i = xNode.ChildNodes.Count Then
                    sCSV &= vbCrLf
                End If
            Next
            sCSV &= vbCrLf
        End If

        Return sCSV

    End Function

    ''' <summary>
    ''' Gets a CSV representation of the xml document.
    ''' </summary>
    ''' <returns></returns>
    Public Function GetCSV() As String

        Dim xnSearch As XmlNode
        Dim sCSV As String

        xnSearch = mxDocument.GetElementsByTagName("search")(0)


        sCSV = LocalisedNewName(xnSearch.Name).ToUpper & ","
        For Each xa As XmlAttribute In xnSearch.Attributes
            sCSV &= LocalisedNewName(xa.Name).ToUpper & "," & xa.Value & ","
        Next
        sCSV &= vbCrLf

        For i As Integer = 0 To xnSearch.ChildNodes.Count - 1
            sCSV &= vbCrLf & GetCSV(xnSearch.ChildNodes(i), ",", True)
        Next

        Return sCSV

    End Function

    Public Function LocalisedNewName(oldName As String) As String
        Dim index = Array.FindIndex(mOldText, Function(x) x.Equals(oldName, StringComparison.InvariantCultureIgnoreCase))
        If (index = -1) Then
            index = Array.FindIndex(mOldText, Function(x) x.Replace(" ", "").Equals(oldName, StringComparison.InvariantCultureIgnoreCase))
        End If

        If index > -1 Then
            Return mNewText(index)
        Else
            Dim result = LTools.GetC(oldName, "misc", "process_def")
            If (result = oldName) Then
                result = LTools.GetC(CultureInfo.CurrentCulture.TextInfo.ToTitleCase(oldName), "misc", "process_def")
            End If
            Return result
        End If
    End Function



    ''' <summary>
    ''' Queries the DB for data on a number of sessions.
    ''' </summary>
    ''' <param name="sessions">The session ids to search for</param>
    ''' <param name="iStageType">The stage types to search for</param>
    ''' <param name="stages">The stage ids to search for, or Nothing for all
    ''' stages.</param>
    Public Sub ReadSessionData(ByVal sessions As Integer(), ByVal iStageType As StageTypes, Optional ByVal stages As Guid() = Nothing)

        If sessions.Length = 0 Then
            DataExists = False
        Else

            Try
                If (iStageType And StageTypes.Decision) > 0 Then
                    ReadDecisionData(sessions, stages)
                End If
                If (iStageType And StageTypes.Calculation) > 0 Then
                    ReadCalculationData(sessions, stages)
                End If
                If (iStageType And StageTypes.MultipleCalculation) > 0 Then
                    ReadStageData(StageTypes.MultipleCalculation, sessions, stages)
                End If
                If (iStageType And StageTypes.Action) > 0 Then
                    ReadActionData(sessions, stages)
                End If
                If (iStageType And StageTypes.Skill) > 0 Then
                    ReadSkillData(sessions, stages)
                End If
                If (iStageType And StageTypes.Process) > 0 Then
                    ReadProcessData(sessions, stages)
                End If
                If (iStageType And StageTypes.SubSheet) > 0 Then
                    ReadSubSheetData(sessions, stages)
                End If
                If (iStageType And StageTypes.ChoiceStart) > 0 Then
                    ReadChoiceStartData(sessions, stages)
                End If
                If (iStageType And StageTypes.Start) > 0 Then
                    ReadStageData(StageTypes.Start, sessions, stages)
                End If
                If (iStageType And StageTypes.End) > 0 Then
                    ReadStageData(StageTypes.End, sessions, stages)
                End If
                If (iStageType And StageTypes.Alert) > 0 Then
                    ReadStageData(StageTypes.Alert, sessions, stages)
                End If
                If (iStageType And StageTypes.Code) > 0 Then
                    ReadStageData(StageTypes.Code, sessions, stages)
                End If
                If (iStageType And StageTypes.Read) > 0 Then
                    ReadStageData(StageTypes.Read, sessions, stages)
                End If
                If (iStageType And StageTypes.Write) > 0 Then
                    ReadStageData(StageTypes.Write, sessions, stages)
                End If
                If (iStageType And StageTypes.Navigate) > 0 Then
                    ReadStageData(StageTypes.Navigate, sessions, stages)
                End If
                If (iStageType And StageTypes.WaitStart) > 0 Then
                    ReadWaitStartData(sessions, stages)
                End If
                If (iStageType And StageTypes.Exception) > 0 Then
                    ReadStageData(StageTypes.Exception, sessions, stages)
                End If
                If (iStageType And StageTypes.Recover) > 0 Then
                    ReadStageData(StageTypes.Recover, sessions, stages)
                End If
                If (iStageType And StageTypes.Resume) > 0 Then
                    ReadStageData(StageTypes.Resume, sessions, stages)
                End If


                '############################
                '
                'Additional stage types here.
                '
                '############################

            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.TheFollowingErrorHasOccurredReadingProcessMIDataFromTheDatabase0, ex.Message))
            End Try


        End If

    End Sub


    ''' <summary>
    ''' Reads decison stage log data.
    ''' </summary>
    ''' <param name="sessions">An array of session numbers to include</param>
    ''' <param name="stages">An array of stage IDs to include, or Nothing to include
    ''' all of them.</param>
    Private Sub ReadDecisionData(ByVal sessions As Integer(), ByVal stages() As Guid)

        Dim dtStages As DataTable = gSv.MIGetDecisionData(sessions, stages)
        Dim sStageID As String
        Dim iTrueCount, iFalseCount As Integer
        For Each r As DataRow In dtStages.Rows
            sStageID = CStr(r("StageID")).ToLower
            iTrueCount = CInt(r("TrueCount"))
            iFalseCount = CInt(r("FalseCount"))

            IncrementData(sStageID, AttributeType.true, iTrueCount)
            IncrementData(sStageID, AttributeType.false, iFalseCount)
            IncrementData(sStageID, AttributeType.count, iTrueCount + iFalseCount)

            DataExists = True
        Next

    End Sub


    ''' <summary>
    ''' Reads calculation stage log data.
    ''' </summary>
    ''' <param name="sessions">An array of session numbers to include</param>
    ''' <param name="stages">An array of stage IDs to include, or Nothing to include
    ''' all of them.</param>
    Private Sub ReadCalculationData(ByVal sessions As Integer(), ByVal stages() As Guid)


        Dim dtStages As DataTable = gSv.MIGetCalculationData(sessions, stages)
        Dim drStageData As DataRow
        Dim sStageID As String

        Dim iDataType As DataType
        Dim iTrueCount, iFalseCount As Integer
        Dim dMaximumDate, dMinimumDate As Date
        Dim iCount, iSum As Integer
        Dim iMaximum, iMinimum As Integer
        For Each drStageData In dtStages.Rows
            sStageID = CStr(drStageData("StageID")).ToLower
            iCount = CInt(drStageData("Count"))
            iDataType = CType(drStageData("DataType"), DataType)

            Select Case iDataType
                Case DataType.number
                    iMaximum = CInt(drStageData("NumberMaximum"))
                    iMinimum = CInt(drStageData("NumberMinimum"))
                    iSum = CInt(drStageData("NumberSum"))

                    SetDataMaximum(sStageID, iMaximum)
                    SetDataMinimum(sStageID, iMinimum)
                    IncrementData(sStageID, AttributeType.total, iSum)
                    IncrementData(sStageID, AttributeType.count, iCount)

                Case DataType.text
                    'Only need iteration count
                    IncrementData(sStageID, AttributeType.total, iCount)

                Case DataType.flag
                    iTrueCount = CInt(drStageData("TrueCount"))
                    iFalseCount = CInt(drStageData("FalseCount"))

                    IncrementData(sStageID, AttributeType.true, iTrueCount)
                    IncrementData(sStageID, AttributeType.false, iFalseCount)
                    IncrementData(sStageID, AttributeType.count, iCount)

                Case DataType.datetime
                    dMaximumDate = CDate(drStageData("DateTimeMaximum"))
                    dMinimumDate = CDate(drStageData("DateTimeMinimum"))

                    SetDataMaximum(sStageID, dMaximumDate)
                    SetDataMinimum(sStageID, dMinimumDate)
                    IncrementData(sStageID, AttributeType.count, iCount)

                Case DataType.date
                    dMaximumDate = CDate(drStageData("DateMaximum"))
                    dMinimumDate = CDate(drStageData("DateMinimum"))

                    SetDataMaximum(sStageID, dMaximumDate)
                    SetDataMinimum(sStageID, dMinimumDate)
                    IncrementData(sStageID, AttributeType.count, iCount)

                Case DataType.time
                    '? need to know time format
                    IncrementData(sStageID, AttributeType.count, iCount)

                Case Else
                    IncrementData(sStageID, AttributeType.count, iCount)

            End Select

            DataExists = True

        Next

    End Sub


    ''' <summary>
    ''' Reads action stage log data.
    ''' </summary>
    ''' <param name="sessions">An array of session numbers to include</param>
    ''' <param name="stages">An array of stage IDs to include, or Nothing to include
    ''' all of them.</param>
    Private Sub ReadActionData(ByVal sessions As Integer(), ByVal stages() As Guid)
        ReadReturnStageData(StageTypes.Action, sessions, stages)
    End Sub

    Private Sub ReadSkillData(sessions As Integer(), stages() As Guid)
        ReadReturnStageData(StageTypes.Skill, sessions, stages)
    End Sub

    ''' <summary>
    ''' Reads process stage log data.
    ''' </summary>
    ''' <param name="sessions">An array of session numbers to include</param>
    ''' <param name="stages">An array of stage IDs to include, or Nothing to include
    ''' all of them.</param>
    Private Sub ReadProcessData(ByVal sessions As Integer(), ByVal stages() As Guid)
        ReadReturnStageData(StageTypes.Process, sessions, stages)
    End Sub

    ''' <summary>
    ''' Reads subsheet stage log data.
    ''' </summary>
    ''' <param name="sessions">An array of session numbers to include</param>
    ''' <param name="stages">An array of stage IDs to include, or Nothing to include
    ''' all of them.</param>
    Private Sub ReadSubSheetData(ByVal sessions As Integer(), ByVal stages() As Guid)
        ReadReturnStageData(StageTypes.SubSheet, sessions, stages)
    End Sub


    ''' <summary>
    ''' Reads log data for stages that have a start and end date.
    ''' </summary>
    ''' <param name="iStageType">The stage type</param>
    ''' <param name="sessions">An array of session numbers to include</param>
    ''' <param name="stages">An array of stage IDs to include, or Nothing to include
    ''' all of them.</param>
    Private Sub ReadReturnStageData(ByVal iStageType As StageTypes, ByVal sessions As Integer(), ByVal stages() As Guid)


        Dim dtStages As DataTable = gSv.MIGetReturnStageData(iStageType, sessions, stages)
        Dim drStageData As DataRow
        Dim sStageID As String
        Dim iCount, iTotal, iAverage As Integer
        For Each drStageData In dtStages.Rows
            sStageID = CStr(drStageData("StageID")).ToLower
            iCount = CInt(drStageData("Count"))
            iTotal = CInt(drStageData("Total"))
            iAverage = CInt(drStageData("Average"))

            IncrementData(sStageID, AttributeType.count, iCount)
            IncrementData(sStageID, AttributeType.total, iTotal)
            IncrementData(sStageID, AttributeType.average, iAverage)

            DataExists = True

        Next

    End Sub


    ''' <summary>
    ''' Reads choice stage log data.
    ''' </summary>
    ''' <param name="sessions">An array of session numbers to include</param>
    ''' <param name="stages">An array of stage IDs to include, or Nothing to include
    ''' all of them.</param>
    ''' <remarks>ChoiceStarts are handled slightly differently. To 
    ''' create a unique id for the choices, the choice position 
    ''' number is appended to the guid string. The ChoiceEnd is
    ''' treated as another one of the choices.
    ''' NB May need to review this arrangement.</remarks>
    Private Sub ReadChoiceStartData(ByVal sessions As Integer(), ByVal stages() As Guid)

        Dim dtStages As DataTable = gSv.MIGetChoiceStartData(StageTypes.ChoiceStart, sessions, stages)
        Dim drStageData As DataRow
        Dim sStageID As String
        Dim sNumber As String
        Dim iCount, iTrueCount As Integer

        If dtStages IsNot Nothing AndAlso dtStages.Rows.Count > 0 Then
            iCount = CInt(dtStages.Compute("SUM([TrueCount])", ""))
        Else
            iCount = 0
        End If

        For Each drStageData In dtStages.Rows
            sStageID = CStr(drStageData("StageID")).ToLower
            sNumber = CStr(drStageData("Number"))
            iTrueCount = CInt(drStageData("TrueCount"))
            Try
                IncrementData(sStageID & sNumber, AttributeType.true, iTrueCount)
                IncrementData(sStageID & sNumber, AttributeType.count, iCount)
            Catch ex As Exception
                'The choices in the logs no longer exist in the process.
            End Try

            DataExists = True
        Next

    End Sub


    ''' <summary>
    ''' Reads wait stage log data.
    ''' </summary>
    ''' <param name="sessions">An array of session numbers to include</param>
    ''' <param name="stages">An array of stage IDs to include, or Nothing to include
    ''' all of them.</param>
    ''' <remarks>WaitStarts are handled slightly differently. To 
    ''' create a unique id for the choices, the choice position 
    ''' number is appended to the guid string. The WaitEnd is
    ''' treated as another one of the choices.
    ''' NB May need to review this arrangement.</remarks>
    Private Sub ReadWaitStartData(ByVal sessions As Integer(), ByVal stages() As Guid)

        Dim dtStages As DataTable = gSv.MIGetChoiceStartData(StageTypes.WaitStart, sessions, stages)
        Dim drStageData As DataRow
        Dim sStageID As String
        Dim sNumber As String
        Dim iCount, iTrueCount As Integer

        If dtStages IsNot Nothing AndAlso dtStages.Rows.Count > 0 Then
            iCount = CInt(dtStages.Compute("SUM([TrueCount])", ""))
        Else
            iCount = 0
        End If

        For Each drStageData In dtStages.Rows
            sStageID = CStr(drStageData("StageID")).ToLower
            sNumber = CStr(drStageData("Number"))
            iTrueCount = CInt(drStageData("TrueCount"))
            Try
                IncrementData(sStageID & sNumber, AttributeType.true, iTrueCount)
                IncrementData(sStageID & sNumber, AttributeType.count, iCount)
            Catch ex As Exception
                'The choices in the logs no longer exist in the process.
            End Try

            DataExists = True
        Next

    End Sub


    ''' <summary>
    ''' Reads generic stage data
    ''' </summary>
    ''' <param name="iStageType">The stage type</param>
    ''' <param name="sessions">An array of session numbers to include</param>
    ''' <param name="stages">An array of stage IDs to include, or Nothing to include
    ''' all of them.</param>
    Private Sub ReadStageData(ByVal iStageType As StageTypes, ByVal sessions As Integer(), ByVal stages() As Guid)

        Dim dtStages As DataTable = gSv.MIGetStageData(iStageType, sessions, stages)
        Dim drStageData As DataRow
        Dim sStageID As String
        Dim iCount As Integer
        For Each drStageData In dtStages.Rows
            sStageID = CStr(drStageData("StageID")).ToLower
            iCount = CInt(drStageData("Count"))
            IncrementData(sStageID, AttributeType.count, iCount)

            DataExists = True
        Next

    End Sub


End Class
