Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.AutomateProcessCore
Imports System.Text.RegularExpressions
Imports BluePrism.AutomateProcessCore.Stages

''' <summary>
''' Searches a process
''' </summary>
Public Class clsProcessSearcher

    ''' <summary>
    ''' The process being searched.
    ''' </summary>
    ''' <remarks>The lifetime of this variable
    ''' must match the lifetime of this object.
    ''' That is, once created this object should
    ''' never change processes.</remarks>
    Private mProcess As clsProcess


    Public Sub New(ByVal process As clsProcess)
        Me.mProcess = process
    End Sub

    ''' <summary>
    ''' Private member to store public property LastSearchString()
    ''' </summary>
    Private mLastSearchString As String
    ''' <summary>
    ''' The last search string used
    ''' </summary>
    Public Property LastSearchString() As String
        Get
            Return mLastSearchString
        End Get
        Set(ByVal value As String)
            mLastSearchString = value
        End Set
    End Property

    Public Structure SearchParameters
        ''' <summary>
        ''' The string used in the search.
        ''' </summary>
        Public SearchText As String
        ''' <summary>
        ''' Indicates a forward search. When
        ''' false, a backward search is performed.
        ''' </summary>
        Public ForwardSearch As Boolean
        ''' <summary>
        ''' Indicates a new search. When false,
        ''' searching resumes from the last stage.
        ''' </summary>
        Public NewSearch As Boolean
        ''' <summary>
        ''' Indicates the search type.
        ''' </summary>
        Public SearchType As SearchTypes
        ''' <summary>
        ''' Indicates a whole word match. When
        ''' true, the search string will only be
        ''' matched against whole words.
        ''' </summary>
        Public MatchWholeWord As Boolean
        ''' <summary>
        ''' Indicates whether a case-sensitive search
        ''' should be performed.
        ''' </summary>
        Public MatchCase As Boolean

        ''' <summary>
        ''' Indicates whether we are doing an element based search
        ''' </summary>
        Public SearchElements As Boolean

        Public Sub New(ByVal searchText As String, ByVal forwardSearch As Boolean, ByVal newSearch As Boolean, ByVal searchType As SearchTypes, ByVal wholeWord As Boolean)
            Me.SearchText = searchText
            Me.ForwardSearch = forwardSearch
            Me.NewSearch = newSearch
            Me.SearchType = searchType
            Me.MatchWholeWord = MatchWholeWord
        End Sub

        ''' <summary>
        ''' The types of search available.
        ''' </summary>
        Public Enum SearchTypes
            ''' <summary>
            ''' A normal string search.
            ''' </summary>
            Normal
            ''' <summary>
            ''' A regular expression search.
            ''' </summary>
            Regex
            ''' <summary>
            ''' A wildcard search
            ''' </summary>
            WildCard
        End Enum
    End Structure


    ''' <summary>
    ''' Searches the process for references to the element
    ''' with the specified text.
    ''' </summary>
    ''' <param name="sErr">Carries an error message in the event
    ''' of an error.</param>
    ''' <param name="searchParams">The search parameters to use.</param>
    ''' <param name="searchResult">Any matching stage found. It is acceptable
    ''' to pass null here. May be null on return.
    ''' </param>
    ''' <returns>Returns true on success; false in the event of
    ''' an error.</returns>
    Public Function FindElement(ByVal searchParams As SearchParameters, ByRef searchResult As SearchResult, ByRef sErr As String) As Boolean
        Try

            If mProcess.ProcessType <> DiagramType.Object Then
                sErr = My.Resources.ElementSearchesAreOnlyValidOnBusinessObjects
                Return False
            End If

            'Used to remember the last point at which the search stopped
            Static LastSearchIndex As Integer

            'Find list of matching elements, and cache it using static variable
            Static MatchingElements As List(Of clsApplicationElement)
            If (MatchingElements Is Nothing) OrElse searchParams.NewSearch Then
                MatchingElements = SearchElementsRecursively(searchParams, mProcess.ApplicationDefinition.RootApplicationElement)
                LastSearchIndex = 0
            End If

            Dim AllApplicationStages As List(Of clsProcessStage) = mProcess.GetStages(StageTypes.Read Or StageTypes.Write Or StageTypes.Navigate Or StageTypes.WaitStart)

            If AllApplicationStages.Count > 0 Then

                'Determine the index we will start and stop on, depending on the search direction
                Dim StartIndex As Integer, EndIndex, StepIncrement As Integer
                If Not searchParams.NewSearch Then
                    If searchParams.ForwardSearch Then
                        StartIndex = AllApplicationStages.Count + (LastSearchIndex + 1)
                        EndIndex = StartIndex + (AllApplicationStages.Count - 1)
                        StepIncrement = 1
                    Else
                        StartIndex = AllApplicationStages.Count + (LastSearchIndex - 1)
                        EndIndex = StartIndex - (AllApplicationStages.Count - 1)
                        StepIncrement = -1
                    End If
                Else
                    StartIndex = 0
                    EndIndex = AllApplicationStages.Count - 1
                    StepIncrement = 1
                End If

                'Loop through each stage, looking for an element in our match list
                For StageIndex As Integer = StartIndex To EndIndex Step StepIncrement
                    Dim InRangeIndex As Integer = StageIndex Mod AllApplicationStages.Count
                    Dim stage As clsProcessStage = AllApplicationStages(InRangeIndex)
                    Dim AppStage As clsAppStage = TryCast(stage, clsAppStage)
                    If Not AppStage Is Nothing Then
                        For Each st As clsStep In AppStage.Steps
                            For Each Element As clsApplicationElement In MatchingElements
                                If Element.ID = st.ElementID Then
                                    searchResult = New SearchResult(AppStage)
                                    LastSearchIndex = InRangeIndex
                                    Return True
                                End If
                            Next
                        Next
                    End If
                    Dim WaitStage As clsWaitStartStage = TryCast(stage, clsWaitStartStage)
                    If Not WaitStage Is Nothing Then
                        For Each ch As clsWaitChoice In WaitStage.Choices
                            For Each element As clsApplicationElement In MatchingElements
                                If element.ID = ch.ElementID Then
                                    searchResult = New SearchResult(WaitStage)
                                    LastSearchIndex = InRangeIndex
                                    Return True
                                End If
                            Next
                        Next
                    End If

                Next
            End If


            'Nothing found, but return true to indicate absence of error
            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try
    End Function


    ''' <summary>
    ''' The last used index in a Stage Search
    ''' </summary>
    Private mPreSearchIndex As Integer

    ''' <summary>
    ''' This is the main search function.  It loops through all the stages, in index order (the order
    ''' they were added to the process and does a normal, wildcard, or regular expressions search 
    ''' in the Name and Narrative for the search text (sText).
    ''' If blnStartNew = False it will continue search from where it left
    ''' If blnForward = False it will search backwards
    ''' If it reaches the last stage in the process it will loop around to the first.  The search continues
    ''' until either a stage with sText is found OR the start stage of the search is reached again.
    ''' </summary>
    ''' <param name="searchParams">The search parameters to be used.</param>
    ''' <param name="foundStage">Returns the found clsProcessClass (if found)</param>
    ''' <returns>True if search string is found, False if not found.</returns>
    Public Function StageSearch(ByVal searchParams As SearchParameters, ByRef foundStage As clsProcessStage) As Boolean

        'first lets be Dim
        Dim i As Integer, blnSkipLoop As Boolean, iStep As Integer, iEndIndex As Integer
        Dim blnNoLoop As Boolean = False
        Dim stages As List(Of clsProcessStage) = mProcess.GetStages()
        Dim selection As clsProcessSelectionContainer = mProcess.SelectionContainer

        'Statics required as user can keep going next/previous on same search
        Static iStartIndex As Integer, iNextEndindex As Integer, iPrevEndIndex As Integer
        Static iLoopAround As Integer

        'If this is a new search set up variables for search loop
        If searchParams.NewSearch Then
            iLoopAround = 0
            mPreSearchIndex = 0
            'we need to start searching at the stage after the currently selected stage
            'if there is one.  If there is more than one then deselect them all...
            If selection.Count = 1 Then
                Dim ps As clsProcessSelection = CType(selection.PrimarySelection, clsProcessSelection)              'there can be only one
                mPreSearchIndex = mProcess.GetStageIndex(ps.mgStageID)
                If mPreSearchIndex < 0 Then
                    mPreSearchIndex = 0
                    blnNoLoop = True
                End If
            ElseIf selection.Count > 1 Then
                'we cant start our search at more than one place so..... unselect all
                mProcess.SelectNone()
                blnNoLoop = True
            Else
                blnNoLoop = True
            End If

            iNextEndindex = stages.Count - 1
            iPrevEndIndex = 0
            If searchParams.ForwardSearch Then
                If mPreSearchIndex > 0 Then iStartIndex = mPreSearchIndex + 1 Else iStartIndex = 0
                If iStartIndex > stages.Count - 1 Then
                    iStartIndex = 0
                    blnNoLoop = True                      'starting at beginning so no need to loop
                End If
            Else
                If mPreSearchIndex > 0 Then iStartIndex = mPreSearchIndex - 1 Else iStartIndex = stages.Count - 1
                If iStartIndex < 0 Then
                    iStartIndex = stages.Count - 1
                    blnNoLoop = True                      'starting at beginning so no need to loop
                End If
            End If
        Else
            If iNextEndindex > stages.Count - 1 Then iNextEndindex = stages.Count - 1
            If iStartIndex > stages.Count - 1 Then iStartIndex = stages.Count - 1
        End If

        If searchParams.ForwardSearch Then
            iStep = 1
            iEndIndex = iNextEndindex
        Else
            iStep = -1
            iEndIndex = iPrevEndIndex
        End If

        If blnNoLoop Then iLoopAround = 1

        'outer iLoopAround loop is required to loop around back to start point
        For iLoopAround = iLoopAround To 1
            If Not searchParams.NewSearch Then
                iStartIndex += iStep
                If searchParams.ForwardSearch Then
                    If iStartIndex > iEndIndex Then
                        'need to skip rest of loop
                        blnSkipLoop = True
                    End If
                Else
                    If iStartIndex < iEndIndex Then
                        'need to skip rest of loop
                        blnSkipLoop = True
                    End If
                End If
                searchParams.NewSearch = True               ' set to ensure don't come in here next loop
            Else
                If iLoopAround > 0 And blnNoLoop = False Then
                    If mPreSearchIndex < stages.Count - 1 Then iPrevEndIndex = mPreSearchIndex Else iPrevEndIndex = iStartIndex
                    If mPreSearchIndex > 0 Then iNextEndindex = mPreSearchIndex Else iNextEndindex = iStartIndex
                    If searchParams.ForwardSearch Then
                        iStartIndex = 0
                        iEndIndex = iNextEndindex
                    Else
                        iStartIndex = stages.Count - 1
                        iEndIndex = iPrevEndIndex
                    End If
                End If
            End If
            'this inner iStartIndex loop is doing all the searching...
            If Not blnSkipLoop Then
                For i = iStartIndex To iEndIndex Step iStep
                    If SearchForParameters(stages(i), searchParams) Then
                        foundStage = stages(i)
                        iStartIndex = i                      'set static to i for find next/previous
                        Return True
                    End If
                Next
            End If
            blnSkipLoop = False          ' unset so not skipped next loop
            'If iPreSearchIndex = 0 Then iLoopAround = 1 ' don't loop again
        Next iLoopAround
        Return False          ' we don't have a match
    End Function


    ''' <summary>
    ''' Calls the textsearch function for each parameter within the stage until
    ''' a the textsearch returns true or there are no parameters left.
    ''' </summary>
    ''' <param name="stage">The Stage object to search in</param>
    ''' <param name="searchParams">The search parameters to be used.</param>
    ''' <returns>True if textsearch succeeds in finding a match, false otherwise.</returns>
    Public Function SearchForParameters(ByVal stage As clsProcessStage, ByVal searchParams As SearchParameters) As Boolean
        Try
            Dim bSuccess As Boolean = False

            'For all stages, search the name, description and any parameters
            bSuccess = bSuccess OrElse TextSearch(stage.GetName, searchParams)
            bSuccess = bSuccess OrElse TextSearch(stage.GetNarrative, searchParams)
            For Each objParameter As clsProcessParameter In stage.GetParameters
                If TextSearch(objParameter.Name, searchParams) _
                  OrElse TextSearch(clsExpression.NormalToLocal(objParameter.GetMap), searchParams) Then
                    bSuccess = True
                    Exit For
                End If
            Next

            'Now search for special features of each stage
            Select Case stage.StageType
                Case StageTypes.Calculation
                    Dim calcStage As clsCalculationStage = CType(stage, clsCalculationStage)
                    bSuccess = bSuccess OrElse TextSearch(calcStage.Expression.LocalForm, searchParams)
                    bSuccess = bSuccess OrElse TextSearch(calcStage.StoreIn, searchParams)
                Case StageTypes.Decision
                    bSuccess = bSuccess OrElse TextSearch(CType(stage, clsDecisionStage).Expression.LocalForm, searchParams)
                Case StageTypes.Action
                    Dim sObject As String = Nothing
                    Dim sAction As String = Nothing
                    CType(stage, clsActionStage).GetResource(sObject, sAction)
                    bSuccess = bSuccess OrElse TextSearch(sObject, searchParams)
                    bSuccess = bSuccess OrElse TextSearch(sAction, searchParams)
                Case StageTypes.Skill
                    Dim skill = CType(stage, clsSkillStage)
                    bSuccess = bSuccess OrElse TextSearch(skill.ActionName, searchParams)
                Case StageTypes.LoopStart
                    Dim LoopStart As clsLoopStartStage = CType(stage, clsLoopStartStage)
                    Select Case LoopStart.LoopType
                        Case "ForEach"
                            Dim Collection As String = LoopStart.LoopData
                            bSuccess = bSuccess OrElse TextSearch(Collection, searchParams)
                        Case Else
                            Debug.Assert(False, "Please fix me. No other loop types existed at the time of writing.")
                    End Select
                Case StageTypes.ChoiceStart
                    For Each choice As clsChoice In CType(stage, clsChoiceStartStage).Choices
                        bSuccess = bSuccess OrElse TextSearch(choice.Expression.LocalForm, searchParams)
                    Next
                Case StageTypes.MultipleCalculation
                    For Each calc As clsCalcStep In CType(stage, clsMultipleCalculationStage).Steps
                        bSuccess = bSuccess OrElse TextSearch(calc.Expression.LocalForm, searchParams)
                        bSuccess = bSuccess OrElse TextSearch(calc.StoreIn, searchParams)
                    Next
                Case StageTypes.Exception
                    Dim exceptionStage As clsExceptionStage = CType(stage, clsExceptionStage)
                    bSuccess = bSuccess OrElse TextSearch(exceptionStage.ExceptionType, searchParams)
                    bSuccess = bSuccess OrElse TextSearch(clsExpression.NormalToLocal(exceptionStage.ExceptionDetail), searchParams)
                Case StageTypes.Collection
                    Dim collectionStage As clsCollectionStage = CType(stage, clsCollectionStage)
                    Dim info As clsCollectionInfo = collectionStage.Definition
                    If info IsNot Nothing Then
                        For Each field As clsCollectionFieldInfo In info
                            bSuccess = bSuccess OrElse TextSearch(field.Name, searchParams)
                        Next
                    End If
                Case StageTypes.SubSheet
                    Dim subStage As clsSubSheetRefStage = CType(stage, clsSubSheetRefStage)

                    For Each objSub As clsProcessSubSheet In subStage.Process.SubSheets
                        If subStage.ReferenceId = objSub.ID Then
                            bSuccess = bSuccess OrElse TextSearch(objSub.Name, searchParams)
                            Exit For
                        End If
                    Next
                Case StageTypes.Navigate
                    Dim navigateStage As clsNavigateStage = CType(stage, clsNavigateStage)
                    For Each navigateStep As clsNavigateStep In navigateStage.Steps
                        For Each s As String In navigateStep.ArgumentValues.Values
                            bSuccess = bSuccess OrElse TextSearch(clsExpression.NormalToLocal(s), searchParams)
                        Next

                        If Not navigateStep.Parameters Is Nothing Then
                            For Each p As clsApplicationElementParameter In navigateStep.Parameters
                                bSuccess = bSuccess OrElse TextSearch(p.Expression.LocalForm, searchParams)
                            Next
                        End If
                    Next
                Case StageTypes.Read
                    Dim readStage As clsReadStage = CType(stage, clsReadStage)
                    For Each readStep As clsStep In readStage.Steps
                        If Not readStep.Parameters Is Nothing Then
                            For Each p As clsApplicationElementParameter In readStep.Parameters
                                bSuccess = bSuccess OrElse TextSearch(p.Expression.LocalForm, searchParams)
                            Next
                        End If
                    Next
                Case StageTypes.Write
                    Dim writeStage As clsWriteStage = CType(stage, clsWriteStage)
                    For Each writeStep As clsStep In writeStage.Steps
                        If Not writeStep.Parameters Is Nothing Then
                            For Each p As clsApplicationElementParameter In writeStep.Parameters
                                bSuccess = bSuccess OrElse TextSearch(p.Expression.LocalForm, searchParams)
                            Next
                        End If
                    Next
                Case StageTypes.WaitStart
                    Dim waitStart As clsWaitStartStage = CType(stage, clsWaitStartStage)
                    For Each waitChoice As clsWaitChoice In waitStart.Choices
                        If Not waitChoice.Parameters Is Nothing Then
                            For Each p As clsApplicationElementParameter In waitChoice.Parameters
                                bSuccess = bSuccess OrElse TextSearch(p.Expression.LocalForm, searchParams)
                            Next
                        End If
                    Next
            End Select

            Return bSuccess
        Catch ex As Exception
            UserMessage.OK(ex.Message)
            Return False
        End Try
    End Function



    ''' <summary>
    ''' Searches for a string in a larger string using regular expressions
    ''' </summary>
    ''' <param name="searchInText">The string you are searching in</param>
    ''' <param name="searchParams">The search parameters to be used.</param>
    ''' <returns>True if sSearchForText string is found, False if not found</returns>
    Private Function TextSearch(ByVal searchInText As String, ByVal searchParams As SearchParameters) As Boolean
        Try
            Dim searchForText As String = searchParams.SearchText
            If String.IsNullOrEmpty(searchInText) OrElse String.IsNullOrEmpty(searchForText) Then Return False

            Dim searchPattern As String = ""
            Select Case searchParams.SearchType
                Case SearchParameters.SearchTypes.Normal
                    searchPattern = System.Text.RegularExpressions.Regex.Escape(searchForText)
                Case SearchParameters.SearchTypes.WildCard
                    Dim wild As String = System.Text.RegularExpressions.Regex.Escape(searchForText)
                    wild = Replace(wild, "\*", ".*")
                    wild = Replace(wild, "\?", ".")
                    wild = Replace(wild, "\#", "\d")

                    searchPattern = wild

                Case SearchParameters.SearchTypes.Regex
                    searchPattern = searchForText
            End Select

            If searchParams.MatchWholeWord Then searchPattern = "\b" & searchPattern & "\b"

            Dim options As RegexOptions = RegexOptions.Singleline Or RegexOptions.Compiled
            If Not searchParams.MatchCase Then
                options = options Or RegexOptions.IgnoreCase
            End If

            'The following allows us to keep hold of a compiled
            'regex across calls.
            Static lastSearchPattern As String
            Static regExpression As Regex
            If lastSearchPattern <> searchPattern & " " & searchParams.MatchCase.ToString & " " & searchParams.MatchWholeWord.ToString Then
                regExpression = New Regex(searchPattern, options)
            End If
            lastSearchPattern = searchPattern & " " & searchParams.MatchCase.ToString & " " & searchParams.MatchWholeWord.ToString

            'Do the search
            Return regExpression.Match(searchInText, 0).Success

        Catch ex As Exception
            UserMessage.OK(ex.Message)
            Return False

        End Try
    End Function



    ''' <summary>
    ''' Searches the tree of members recursively, looking
    ''' for elements with matching names.
    ''' </summary>
    ''' <param name="Params">The search parameters.</param>
    ''' <param name="RootMember">The member at which to begin the
    ''' search.</param>
    ''' <returns>A list of matching elements.</returns>
    ''' <remarks>Matches only elements - not members.</remarks>
    Private Function SearchElementsRecursively(ByVal Params As SearchParameters, ByVal RootMember As clsApplicationMember) As List(Of clsApplicationElement)
        Dim Results As New List(Of clsApplicationElement)

        'See if the root element matches
        If TypeOf RootMember Is clsApplicationElement Then
            If TextSearch(RootMember.Name, Params) Then
                Results.Add(CType(RootMember, clsApplicationElement))
            End If
        End If

        'Check each of the children recursively
        For Each child As clsApplicationMember In RootMember.ChildMembers
            Results.AddRange(SearchElementsRecursively(Params, child))
        Next

        Return Results
    End Function

    ''' <summary>
    ''' Represents a search result.
    ''' </summary>
    Public Class SearchResult
        Public Sub New(ByVal Stage As clsProcessStage)
            Me.Stage = Stage
        End Sub

        ''' <summary>
        ''' The stage which matches the search result,
        ''' if any.
        ''' </summary>
        ''' <remarks>May be null.</remarks>
        Public Stage As clsProcessStage
    End Class

End Class
