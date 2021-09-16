
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports System.Globalization
Imports BluePrism.Core
Imports BluePrism.Core.Expressions
Imports BluePrism.Server.Domain.Models

<DebuggerDisplay("Query = {QueryString}")>
Public Class clsQuery

    ''' <summary>
    ''' This class defines known query parameters. The set of parameters defined here
    ''' is not the complete set, only those that are 'pre-defined'.
    ''' 
    ''' Although, like Identifiers, parameters can be passed in a query in any case,
    ''' they are always referred to internally as lower case Strings.
    ''' </summary>
    Public Class ParameterNames

        Public Const Relationship As String = "relationship"

        'Behaviour modifiers
        ''' <summary>
        ''' When getting a value, this specifies the desired data
        ''' type. Eg when set to "flag", a value of "1" is returned
        ''' as "True"
        ''' </summary>
        Public Const CastValueTo As String = "castvalueto"
        ''' <summary>
        ''' When retrieving multiple items (eg from listview or
        ''' combobox), determines whether to restrict to selected
        ''' items only.
        ''' </summary>
        Public Const SelectedItemsOnly As String = "selecteditemsonly"

        'Passed in values...
        Public Const NewText As String = "newtext"
        Public Const MouseButton As String = "mousebutton"
        Public Const PropName As String = "propname"
        Public Const TargetProp As String = "targetprop"
        Public Const RetProc As String = "retproc"
        Public Const TabText As String = "tabtext"
        Public Const TabIndex As String = "tabindex"
        Public Const ItemIndex As String = "itemindex"
        Public Const ItemText As String = "itemtext"
        'By convention, index is used for zero-based indices. See also Position
        Public Const Index As String = "index"
        'By convention, position is used for one-based indices. See also Index
        Public Const Position As String = "position"
        Public Const Length As String = "length"
        Public Const Timeout As String = "timeout"
        Public Const Interval As String = "interval"
        'A text value from {Equal, NotEqual, GreaterThan, LessThan} indicating the comparison type on a wait query
        Public Const WaitComparisonType As String = "waitcomparisontype"
        Public Const Action As String = "action"
        Public Const NumericValue As String = "numericvalue"
        'General value - text, flag, etc
        Public Const Value As String = "value"
        Public Const DateTimeValue As String = "datetimevalue"

        Public Const EventType As String = "eventtype"
        'Identifier name
        Public Const IDName As String = "idname"
        'Identifier type
        Public Const IDType As String = "idtype"

        'Specify font for char matching
        Public Const Font As String = "font"
        'Specify font colour for char matching
        Public Const Colour As String = "colour"
        Public Const BackgroundColour As String = "backgroundcolour"
        'Path of a sample image file
        Public Const ImageFile As String = "imagefile"
        'Path of a text file containing reference text
        Public Const ReferenceFile As String = "referencefile"
        'specifies char matching technique
        Public Const OrigAlgorithm As String = "origalgorithm"
        'specifies char matching technique
        Public Const Multiline As String = "multiline"
        'erases blocks of unwanted colour, such as listview row highlighting
        Public Const EraseBlocks As String = "eraseblocks"

        'Specifies scaling to use for OCR images
        Public Const Scale As String = "scale"
        'Specifies character whitelist for OCR 
        Public Const CharWhitelist As String = "charwhitelist"
        'Specifics language for OCR
        Public Const Language As String = "language"
        'Specifies diagnostics path for OCR 
        Public Const DiagsPath As String = "diagspath"
        'Specifies engine mode for OCR
        Public Const EngineMode As String = "engine"
        'Specifies page segmentation mode for OCR
        Public Const PageSegMode As String = "pagesegmode"

        ' Regions
        Public Const Rect As String = "rect"
        Public Const TargX As String = "targx"
        Public Const TargY As String = "targy"
        Public Const TargWidth As String = "targwidth"
        Public Const TargHeight As String = "targheight"

        Public Const StartX As String = "startx"
        Public Const StartY As String = "starty"
        Public Const EndX As String = "endx"
        Public Const EndY As String = "endy"
        Public Const LocationMethod As String = "locationmethod"
        Public Const RegionPosition As String = "regionposition"
        Public Const ImageSearchPadding As String = "imagesearchpadding"
        Public Const ColourTolerance As String = "colourtolerance"
        Public Const Greyscale As String = "greyscale"

        ' List regions
        Public Const ElementNumber As String = "elementnumber"
        Public Const FirstElement As String = "firstelement"
        Public Const LastElement As String = "lastelement"
        Public Const ListDirection As String = "listdirection"
        Public Const Padding As String = "padding"

        ' Grid regions
        Public Const RowNumber As String = "rownumber"
        Public Const FirstRowNumber As String = "firstrownumber"
        Public Const LastRowNumber As String = "lastrownumber"
        Public Const ColumnNumber As String = "columnnumber"
        Public Const GridSchema As String = "gridschema"

        ' Image matching
        Public Const ElementSnapshot As String = "elementsnapshot"
        Public Const FontName As String = "fontname"
        Public Const ImageValue As String = "imagevalue"

        'For internally selecting different methods of doing the same task. Used by
        'GetMsFlexGridContents, for example.
        Public Const Method As String = "method"

        'Application launching...
        Public Const Path As String = "path"
        Public Const WorkingDir As String = "workingdir"
        Public Const Hook As String = "hook"
        Public Const JAB As String = "jab"
        Public Const Options As String = "options"
        Public Const ChildIndex As String = "childindex"
        Public Const ExcludeHTC As String = "excludehtc"
        Public Const ProcessID As String = "processid"
        Public Const ActiveTabOnly As String = "activetabonly"
        Public Const TrackingId As String = "trackingid"
        Public Const BrowserLaunchTimeout As String = "browserlaunchtimeout"
        Public Const BrowserAttach As String = "browserattach"

        'Terminal-specific...
        Public Const CodePage As String = "codepage"
        Public Const SessionFile As String = "sessionfile"
        Public Const SessionID As String = "sessionid"
        Public Const TerminalType As String = "terminaltype"
        Public Const WaitSleepTime As String = "waitsleeptime"
        Public Const WaitTimeout As String = "waittimeout"
        Public Const SessionDLLName As String = "sessiondllname"
        Public Const SessionDLLEntryPoint As String = "sessiondllentrypoint"
        Public Const SessionConvention As String = "sessionconvention"
        Public Const SessionType As String = "sessiontype"
        'Receives the name of an attachmate variant. Valid values are EXTRA,KEA,KEA_HP,IRMA,ICONN,RALLY
        Public Const ATMVariant As String = "atmvariant"

        ' Used by verify actions
        Public Const Highlight As String = "highlight"

        'HTML specific
        'for invoking javascript method by name
        Public Const MethodName As String = "methodname"
        'for inserting new javascript methods
        Public Const FragmentText As String = "fragmenttext"
        'Arguments for a javascript invocation - an array of objects in JSON format
        Public Const Arguments As String = "arguments"
        Public Const HTMLCommandline As String = "htmlcommandline"

        'Spy and GetElement
        Public Const InitialSpyMode As String = "initialspymode"
        Public Const Mode As String = "mode"
        Public Const Root As String = "root"
        Public Const ElementIndex As String = "elementindex"
        Public Const IncludeWin32 = "includewin32"

        'DDE
        Public Const NoCheck As String = "nocheck"

        'Performance optimisation
        Public Const MatchIndex As String = "matchindex"
        Public Const MatchReverse As String = "matchreverse"

        'Include invisible elements in an element dump
        Public Const IncludeInvisible As String = "includeinvisible"

        'Scroll values
        Public Const BigStep As String = "bigstep"
        Public Const ScrollUp As String = "scrollup"

    End Class

    ''' <summary>
    ''' Information used to identify user interface elements.
    ''' </summary>
    Public Enum IdentifierTypes

        'Window identifiers...
        WindowText
        X
        Y
        Width
        Height
        Ordinal
        ChildCount
        Active
        Enabled
        Visible
        ScreenVisible
        ClassName
        WindowStyle
        TypeName
        CtrlID
        AncestorsText

        'Window identifiers based on parent...
        <ParentOf(WindowText)>
        pWindowText
        <ParentOf(X)>
        pX
        <ParentOf(Y)>
        pY
        <ParentOf(Width)>
        pWidth
        <ParentOf(Height)>
        pHeight
        <ParentOf(Ordinal)>
        pOrdinal
        <ParentOf(ChildCount)>
        pChildCount
        <ParentOf(Active)>
        pActive
        <ParentOf(CtrlID)>
        pCtrlID
        <ParentOf(Visible)>
        pVisible
        <ParentOf(ScreenVisible)>
        pScreenVisible
        <ParentOf(ClassName)>
        pClassName
        <ParentOf(Enabled)>
        pEnabled

        'Application launching...
        ProcessName
        WindowTitle
        WindowTitlesCollection
        Username

        'Terminal-specific...
        StartX
        StartY
        EndX
        EndY
        FieldType
        FieldText

        'MSAA and JAB only
        Busy
        Checked
        Collapsed
        Description
        Expanded
        Focusable
        Focused
        KeyBindings
        Multiselectable
        Name
        VirtualName
        Pressed
        Role
        Selected
        Selectable

        'JAB only
        AllowedActions
        Armed
        AncestorCount
        Editable
        Expandable
        Horizontal
        Iconified
        Modal
        MultipleLine
        Opaque
        Resizable
        Showing
        SingleLine
        JavaText    'The text (as opposed to the name, which often contains the text of labels, etc) of the JAB element
        Transient
        Vertical

        'MSAA only
        aX
        aY
        aWidth
        aHeight
        aAncestorCount
        ' Back-compatible way of ensuring AA 'Checked' property is tested in a query
        aChecked

        <ParentOf(aX)>
        paX
        <ParentOf(aY)>
        paY
        <ParentOf(aWidth)>
        paWidth
        <ParentOf(aHeight)>
        paHeight

        DefaultAction
        Help
        KeyboardShortcut
        State
        Value
        Value2
        ID          'Also used for SAP!
        ElementCount

        Unavailable
        Mixed
        [ReadOnly]
        Hottracked
        [Default]
        Floating
        Marqueed
        Animated
        Invisible
        Offscreen
        Sizeable
        Moveable
        SelfVoicing
        Linked
        Traversed
        Extselectable
        Alert_low
        Alert_medium
        Alert_high

        pName
        pDescription
        pDefaultAction
        pHelp
        pKeyboardShortcut
        pRole
        pState
        pValue
        pValue2
        pID
        pElementCount

        pUnavailable
        pSelected
        pFocused
        pPressed
        pChecked ' The parent's Win32 Checked attribute
        paChecked ' The parent's AA Checked attribute
        pMixed
        pReadOnly
        pHottracked
        pDefault
        pExpanded
        pCollapsed
        pBusy
        pFloating
        pMarqueed
        pAnimated
        pInvisible
        pOffscreen
        pSizeable
        pMoveable
        pSelfVoicing
        pFocusable
        pSelectable
        pLinked
        pTraversed
        pMultiselectable
        pExtselectable
        pAlert_low
        pAlert_medium
        pAlert_high

        'UIA
        uX
        uY
        uWidth
        uHeight
        uClassName
        uAutomationId
        uLocalizedControlType
        uControlType
        uMatchIndex
        uName
        uIsPassword
        uIsRequiredForForm
        uOrientation
        uItemStatus
        uItemType
        uLabeledBy
        uOffscreen
        uTopLevelWindowId
        uProcessId
        uEnabled
        uHelpText
        uHasKeyboardFocus
        uClickablePoint
        uAcceleratorKey
        uAccessKey
        
        <ParentOf(uControlType)>
        puControlType
        <ParentOf(uLocalizedControlType)>
        puLocalizedControlType
        <ParentOf(uClassName)>
        puClassName
        <ParentOf(uName)>
        puName
        <ParentOf(uAutomationId)>
        puAutomationId

        'Web
        wX
        wY
        wWidth
        wHeight
        wName
        wId
        wXPath
        wCssSelector
        wElementType
        wValue
        wPageAddress
        wClass
        wClientX
        wClientY
        wClientWidth
        wClientHeight
        wOffsetX
        wOffsetY
        wOffsetWidth
        wOffsetHeight
        wScrollX
        wScrollY
        wScrollWidth
        wScrollHeight
        wChildCount
        wIsEditable
        wStyle
        wTabIndex
        wInputType
        wAccessKey
        wInnerText
        wSource
        wTargetAddress
        wAlt
        wPattern
        wRel
        wLinkTarget
        wPlaceholder

        'HTML
        TagName
        Title
        Link
        InputType
        InputIdentifier
        InputIdentifier2
        Path        'It would be nice to rename this HTMLPath to avoid confusion with the file path concept, but this would cause a backwards compatibility headache
        pURL

        'DDE
        DDEServerName
        DDETopicName
        DDEItemName

    End Enum

    ''' <summary>
    ''' The command used in this query instance.
    ''' </summary>
    ''' <value>A value from the Commands Enum</value>
    Public ReadOnly Property Command() As String
        Get
            Return meCommand
        End Get
    End Property
    Private meCommand As String


    ''' <summary>
    ''' The conditions for this command, relevant only for a Wait command.
    ''' </summary>
    ''' <value>A list of clsSubQueryMatchTarget objects</value>
    Public ReadOnly Property Conditions() As IEnumerable(Of clsConditionMatchTarget)
        Get
            Return mConditions
        End Get
    End Property
    Private mConditions As List(Of clsConditionMatchTarget)

    ''' <summary>
    ''' Indicates the number of conditions attached to this query, if any.
    ''' Only relevant for a wait commmand.
    ''' </summary>
    Public ReadOnly Property ConditionCount() As Integer
        Get
            If mConditions IsNot Nothing Then
                Return mConditions.Count
            Else
                Return 0
            End If
        End Get
    End Property

    ''' <summary>
    ''' A dictionary of required parameter matches. The key is the parameter 
    ''' name, and the value is the string value supplied by the user.
    ''' </summary>
    ''' <remarks>Note that the dictionary exposed by this property is readonly.
    ''' Parameters cannot be modified in an existing query object.
    ''' </remarks>
    Public ReadOnly Property Parameters() As IDictionary(Of String, String)
        Get
            Return GetReadOnly.IDictionary(mParameters)
        End Get
    End Property
    Private mParameters As IDictionary(Of String, String)

    ''' <summary>
    ''' The collection of identifier match targets for the element referred to by
    ''' this query.
    ''' </summary>
    Public ReadOnly Property Identifiers() As ICollection(Of clsIdentifierMatchTarget)
        Get
            Return mIdentifiers.Values
        End Get
    End Property
    Private mIdentifiers As Dictionary(Of IdentifierTypes, clsIdentifierMatchTarget)

    ''' <summary>
    ''' Gets the identifier of the given type from this query or null if no such
    ''' identifier instance is found in this query.
    ''' </summary>
    ''' <param name="type">The type of identifier required</param>
    ''' <returns>The instance of the specified type of identifier in this query or
    ''' null if no such instance exists.</returns>
    Default Public ReadOnly Property Identifier(type As IdentifierTypes) _
     As clsIdentifierMatchTarget
        Get
            Dim id As clsIdentifierMatchTarget = Nothing
            If mIdentifiers.TryGetValue(type, id) Then Return id
            Return Nothing
        End Get
    End Property

    ''' <summary>
    ''' Gets the identifier value set in this query for the given identifier type,
    ''' or null if the given type is not set in this query
    ''' </summary>
    ''' <param name="type"></param>
    ''' <returns>The identifier match value associated with the given instance of the
    ''' identifier with the given type present in this query; or null if no such
    ''' instance is present in this query</returns>
    <Obsolete("Property no longer seems to be in use (unless it is being called using Reflection)")>
    Public ReadOnly Property IdentifierValue(type As IdentifierTypes) As String
        Get
            Dim id As clsIdentifierMatchTarget = Me(type)
            Return If(id Is Nothing, Nothing, id.MatchValue)
        End Get
    End Property

    ''' <summary>
    ''' Creates a new empty query.
    ''' </summary>
    Private Sub New()
        Me.New(Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new query based on the given query with the specified parameter
    ''' overrides in place.
    ''' </summary>
    ''' <param name="baseQuery">The query to draw this query from - the identifiers /
    ''' conditions and initial parameters are all drawn from this query, but the
    ''' query itself remains unchanged by this operation.</param>
    ''' <param name="paramOverrides">Parameters to set in this subquery, potentially
    ''' overriding existing values in the base query.</param>
    Private Sub New(ByVal baseQuery As clsQuery,
     ByVal paramOverrides As ICollection(Of clsParamSetting))
        ' Query instances are semantically immutable, but they can be extended and
        ' overridden, allowing params to be altered for subqueries.
        ' As such, we have to create a new dictionary for the parameters so
        ' that we don't inadvertently change the base query's parameters
        mParameters = New clsDefaultValueDictionary(Of String, String)

        ' If we are extending another query, we use the same collections for the
        ' identifiers and conditions - this should be safe since the identifiers and
        ' conditions for a query are immutable.
        If baseQuery Is Nothing Then
            mIdentifiers = New Dictionary(Of IdentifierTypes, clsIdentifierMatchTarget)
            mConditions = New List(Of clsConditionMatchTarget)
        Else
            mIdentifiers = baseQuery.mIdentifiers
            mConditions = baseQuery.mConditions
            ' We want to transfer all of the base query's parameters across
            For Each p As KeyValuePair(Of String, String) _
             In baseQuery.mParameters
                mParameters(p.Key) = p.Value
            Next
        End If

        ' Finally, we add the overrides if there are any.
        If paramOverrides IsNot Nothing Then
            For Each entry As clsParamSetting In paramOverrides
                mParameters(entry.ParamType) = entry.Value
            Next
        End If
    End Sub

    ''' <summary>
    ''' The query string from which this object
    ''' was parsed.
    ''' </summary>
    Public ReadOnly Property QueryString() As String
        Get
            Return msQueryString
        End Get
    End Property
    Private msQueryString As String

    ''' <summary>
    ''' A list of the comparison operators supported/expected.
    ''' </summary>
    Private Const AllOperators As String = "'=', '<', '<=', '>', '>=', '<>', '%=' '^=' '$='"

    ''' <summary>
    ''' Tests if this query is likely to represent a region. Effectively, this is a
    ''' shortcut to test if startx/y and endx/y parameters are present.
    ''' </summary>
    Public ReadOnly Property IsRegion() As Boolean
        Get
            Return HasAllParameters(
             ParameterNames.StartX, ParameterNames.StartY,
             ParameterNames.EndX, ParameterNames.EndY)
        End Get
    End Property

    ''' <summary>
    ''' Parse a query in text form and return the object model representation
    ''' of it.
    ''' </summary>
    ''' <param name="sQueryText">The query text</param>
    ''' <returns>A clsQuery object containing all the details of the query.
    ''' </returns>
    ''' <remarks>
    ''' Throws an ApplicationException if the query syntax is not valid.
    ''' </remarks>
    Public Shared Function Parse(ByVal sQueryText As String) As clsQuery

        Dim objQuery As New clsQuery, sWord As String
        Dim iIndex As Integer
        objQuery.msQueryString = sQueryText

        'Extract command...
        iIndex = sQueryText.IndexOf(" ")
        If iIndex < 0 Then
            'No space must mean a command with no parameters...
            sWord = sQueryText
            sQueryText = ""
        Else
            sWord = sQueryText.Substring(0, iIndex)
            sQueryText = sQueryText.Substring(iIndex + 1)
        End If

        'Store the command verb...
        objQuery.meCommand = sWord


        'Process any and all parameters...
        Do While Len(sQueryText) > 0
            If sQueryText.StartsWith(" ") Then
                'Skip over white space...
                sQueryText = sQueryText.Mid(2)
            ElseIf sQueryText.StartsWith("{") Then
                'Deal with a sub-query, as found in a wait query...
                'Just searching for this is ok, because they are escaped
                iIndex = sQueryText.IndexOf("}")
                If iIndex = -1 Then
                    Throw New InvalidOperationException(My.Resources.SubqueryNotTerminated)
                End If

                'After we have observed the comparison operator, we
                'need to know from which index to continue scanning
                Dim IndexFromWhichToContinue As Integer = iIndex + 2

                'Observe the comparison operator
                Dim ComparisonType As clsMatchTarget.ComparisonTypes
                Select Case sQueryText.Substring(iIndex + 1, 1)
                    Case "="
                        ComparisonType = clsMatchTarget.ComparisonTypes.Equal
                    Case ">"
                        Dim NextCharacter As String = sQueryText.Substring(IndexFromWhichToContinue, 1)
                        Select Case NextCharacter
                            Case "="
                                ComparisonType = clsMatchTarget.ComparisonTypes.GreaterThanOrEqual
                                IndexFromWhichToContinue += 1
                            Case Else
                                ComparisonType = clsMatchTarget.ComparisonTypes.GreaterThan
                        End Select
                    Case "<"
                        Dim NextCharacter As String = sQueryText.Substring(IndexFromWhichToContinue, 1)
                        Select Case NextCharacter
                            Case ">"
                                ComparisonType = clsMatchTarget.ComparisonTypes.NotEqual
                                IndexFromWhichToContinue += 1
                            Case "="
                                ComparisonType = clsMatchTarget.ComparisonTypes.LessThanOrEqual
                                IndexFromWhichToContinue += 1
                            Case Else
                                ComparisonType = clsMatchTarget.ComparisonTypes.LessThan
                        End Select
                    Case "%"
                        Dim NextCharacter As String = sQueryText.Substring(IndexFromWhichToContinue, 1)
                        Select Case NextCharacter
                            Case "="
                                ComparisonType = clsMatchTarget.ComparisonTypes.Wildcard
                                IndexFromWhichToContinue += 1
                            Case Else
                                Throw New InvalidOperationException(String.Format(My.Resources.SubqueryMustBeFollowedByOneOf0, AllOperators))
                        End Select
                    Case "^"
                        Dim NextCharacter As String = sQueryText.Substring(IndexFromWhichToContinue, 1)
                        Select Case NextCharacter
                            Case "="
                                ComparisonType = clsMatchTarget.ComparisonTypes.Range
                                IndexFromWhichToContinue += 1
                            Case Else
                                Throw New InvalidOperationException(String.Format(My.Resources.SubqueryMustBeFollowedByOneOf0, AllOperators))
                        End Select
                    Case "$"
                        Dim NextCharacter As String = sQueryText.Substring(IndexFromWhichToContinue, 1)
                        Select Case NextCharacter
                            Case "="
                                ComparisonType = clsMatchTarget.ComparisonTypes.RegEx
                                IndexFromWhichToContinue += 1
                            Case Else
                                Throw New InvalidOperationException(String.Format(My.Resources.SubqueryMustBeFollowedByOneOf0, AllOperators))
                        End Select
                    Case Else
                        Throw New InvalidOperationException(String.Format(My.Resources.SubqueryMustBeFollowedByOneOf0, AllOperators))
                End Select


                'Parse the subquery...
                Dim sSubQuery As String, objSubQuery As clsQuery
                sSubQuery = sQueryText.Substring(1, iIndex - 1)
                objSubQuery = clsQuery.Parse(sSubQuery)

                'Parse the comparison value...
                sQueryText = sQueryText.Substring(IndexFromWhichToContinue)
                Dim sVal As String
                sVal = ParseValue(sQueryText)

                Dim objCond As New clsConditionMatchTarget()
                objCond.ConditionQuery = objSubQuery
                objCond.MatchValue = sVal
                objCond.ComparisonType = ComparisonType

                Select Case ComparisonType
                    Case clsMatchTarget.ComparisonTypes.GreaterThan,
                       clsMatchTarget.ComparisonTypes.GreaterThanOrEqual,
                       clsMatchTarget.ComparisonTypes.LessThan,
                       clsMatchTarget.ComparisonTypes.LessThanOrEqual,
                       clsMatchTarget.ComparisonTypes.Range
                        objCond.DataType = clsMatchTarget.DataTypes.Number
                    Case Else
                        objCond.DataType = clsMatchTarget.DataTypes.Text
                End Select

                objQuery.mConditions.Add(objCond)
            Else
                'Must be a normal parameter...

                'Find out where the operator starts. This simplistic approach will work
                'since no identifiers or parameters contain any of these special characters
                Dim ComparisonType As clsMatchTarget.ComparisonTypes
                Dim DataType As clsMatchTarget.DataTypes
                Dim BracketIndex1 As Integer = sQueryText.IndexOf("<")
                If BracketIndex1 = -1 Then BracketIndex1 = Integer.MaxValue
                Dim BracketIndex2 As Integer = sQueryText.IndexOf(">")
                If BracketIndex2 = -1 Then BracketIndex2 = Integer.MaxValue
                Dim PercentIndex As Integer = sQueryText.IndexOf("%")
                If PercentIndex = -1 Then PercentIndex = Integer.MaxValue
                Dim CaretIndex As Integer = sQueryText.IndexOf("^")
                If CaretIndex = -1 Then CaretIndex = Integer.MaxValue
                Dim DollarIndex As Integer = sQueryText.IndexOf("$")
                If DollarIndex = -1 Then DollarIndex = Integer.MaxValue
                Dim EqualityIndex As Integer = sQueryText.IndexOf("=")
                If EqualityIndex = -1 Then EqualityIndex = Integer.MaxValue
                iIndex = Integer.MaxValue
                iIndex = Math.Min(iIndex, BracketIndex1)
                iIndex = Math.Min(iIndex, BracketIndex2)
                iIndex = Math.Min(iIndex, PercentIndex)
                iIndex = Math.Min(iIndex, CaretIndex)
                iIndex = Math.Min(iIndex, DollarIndex)
                iIndex = Math.Min(iIndex, EqualityIndex)
                If iIndex = Integer.MaxValue Then
                    Throw New InvalidOperationException(String.Format(My.Resources.ParameterFormatErrorMissingOperatorOneOf0Expected, AllOperators))
                End If

                Dim FirstOperatorChar As Char = sQueryText.Substring(iIndex, 1).ToCharArray()(0)
                Dim NextOperatorChar As Char
                If iIndex < sQueryText.Length - 1 Then
                    NextOperatorChar = sQueryText.Substring(iIndex + 1, 1).ToCharArray()(0)
                End If
                Dim ContinuationIndex As Integer
                Select Case FirstOperatorChar
                    Case "="c
                        ComparisonType = clsMatchTarget.ComparisonTypes.Equal
                        ContinuationIndex = iIndex + 1
                        DataType = clsMatchTarget.DataTypes.Text
                    Case "<"c
                        'check for one of "=" or ">" following it
                        Select Case NextOperatorChar
                            Case ">"c
                                ComparisonType = clsMatchTarget.ComparisonTypes.NotEqual
                                ContinuationIndex = iIndex + 2
                                DataType = clsMatchTarget.DataTypes.Text
                            Case "="c
                                ComparisonType = clsMatchTarget.ComparisonTypes.LessThanOrEqual
                                ContinuationIndex = iIndex + 2
                                DataType = clsMatchTarget.DataTypes.Number
                            Case Else
                                ComparisonType = clsMatchTarget.ComparisonTypes.LessThan
                                ContinuationIndex = iIndex + 1
                                DataType = clsMatchTarget.DataTypes.Number
                        End Select
                    Case ">"c
                        'check whether "=" follows it
                        Select Case NextOperatorChar
                            Case "="c
                                ComparisonType = clsMatchTarget.ComparisonTypes.GreaterThanOrEqual
                                ContinuationIndex = iIndex + 2
                            Case Else
                                ComparisonType = clsMatchTarget.ComparisonTypes.GreaterThan
                                ContinuationIndex = iIndex + 1
                        End Select
                        DataType = clsMatchTarget.DataTypes.Number
                    Case "%"c
                        'This must be followed by '='
                        If Not NextOperatorChar = "="c Then
                            Throw New NoSuchElementException(String.Format(My.Resources.InvalidOperator0ExpectedPercOneOf1, NextOperatorChar, AllOperators))
                        Else
                            ComparisonType = clsMatchTarget.ComparisonTypes.Wildcard
                            ContinuationIndex = iIndex + 2
                        End If
                        DataType = clsMatchTarget.DataTypes.Text
                    Case "^"c
                        'This must be followed by '='
                        If Not NextOperatorChar = "="c Then
                            Throw New NoSuchElementException(String.Format(My.Resources.InvalidOperator0ExpectedCircumflexOneOf1, NextOperatorChar, AllOperators))
                        Else
                            ComparisonType = clsMatchTarget.ComparisonTypes.Range
                            ContinuationIndex = iIndex + 2
                        End If
                        DataType = clsMatchTarget.DataTypes.Number
                    Case "$"c
                        'This must be followed by '='
                        If Not NextOperatorChar = "="c Then
                            Throw New NoSuchElementException(String.Format(My.Resources.InvalidOperator0ExpectedDollarOneOf1, NextOperatorChar, AllOperators))
                        Else
                            ComparisonType = clsMatchTarget.ComparisonTypes.RegEx
                            ContinuationIndex = iIndex + 2
                        End If
                        DataType = clsMatchTarget.DataTypes.Text
                    Case Else
                        Throw New NoSuchElementException(String.Format(My.Resources.InvalidOperator0ExpectedOneOf1, FirstOperatorChar, AllOperators))
                End Select

                'Get the two operands either side of the operator
                sWord = sQueryText.Substring(0, iIndex)
                sQueryText = sQueryText.Substring(ContinuationIndex)
                Dim ParsedValue As String = ParseValue(sQueryText)

                'We now have either an Identifier or a Parameter. As all Identifiers
                'are pre-defined, we check if it's one of those first. If not, it must
                'be a parameter!
                Dim ident As IdentifierTypes
                If clsEnum.TryParse(sWord, True, ident) Then
                    Dim Matcher As New clsIdentifierMatchTarget(ident)
                    Matcher.MatchValue = ParsedValue
                    Matcher.ComparisonType = ComparisonType
                    Matcher.DataType = DataType
                    objQuery.mIdentifiers.Add(ident, Matcher)
                End If
                'But wait! We have to add it as a parameter, even if it's also an
                'idenfifier. This is because there are things that are both!!!?? An
                'example is "path", but there may be others.
                objQuery.mParameters.Add(sWord.ToLower(CultureInfo.InvariantCulture), ParsedValue)
            End If
        Loop
        Return objQuery
    End Function


    ''' <summary>
    ''' Parse a value from the beginning of the given query fragment. On return, the
    ''' value is removed from the query fragment. Throws an ApplicationException if
    ''' a parsing error occurs.
    ''' 
    ''' The format of the value is that produced by EncodeValue().
    ''' </summary>
    ''' <param name="queryText">The query fragment being processed</param>
    ''' <returns>The value parsed.</returns>
    Public Shared Function ParseValue(ByRef queryText As String) As String
        Dim val As String, i As Integer

        If queryText.Length = 0 Then
            'Can be an empty value right at the end of the query
            val = ""
        ElseIf queryText.StartsWith("""") Then
            'Quoted value...
            val = ""
            i = 1
            Dim esc As Boolean = False
            Dim finished As Boolean = False
            While Not finished
                Dim c As Char = queryText(i)
                If esc Then
                    Select Case c
                        Case "\"c, """"c
                            val &= c
                        Case "r"c
                            val &= vbCr
                        Case "n"c
                            val &= vbLf
                        Case "c"c
                            val &= "}"c
                        Case Else
                            Throw New InvalidOperationException(String.Format(My.Resources.InvalidEscapeCharacter0, c))
                    End Select
                    esc = False
                Else
                    Select Case c
                        Case "\"c
                            esc = True
                        Case """"c
                            finished = True
                        Case Else
                            val &= c
                    End Select
                End If
                i += 1
                If Not finished AndAlso i >= queryText.Length() Then
                    Throw New InvalidOperationException(My.Resources.MissingQuote)
                End If
            End While
            If esc Then
                Throw New InvalidOperationException(My.Resources.UnterminatedEscapeSequence)
            End If
            If queryText.Length = i Then queryText = "" Else queryText = queryText.Substring(i + 1)

        Else
            'Normal value...
            i = queryText.IndexOf(" ")
            If i = -1 Then
                val = queryText
                queryText = ""
            Else
                val = Left(queryText, i)
                queryText = queryText.Mid(i + 2)
            End If
        End If
        Return val

    End Function


    ''' <summary>
    ''' Gets a subquery of this query with the specified parameters set in it.
    ''' </summary>
    ''' <param name="params">The parameter overrides to put in place for the
    ''' returned subquery</param>
    ''' <returns>A query object with all the same settings as this query, but with
    ''' the specified parameter overrides in place.</returns>
    Public Function WithParams(
     ByVal ParamArray params() As clsParamSetting) As clsQuery
        Return New clsQuery(Me, params)
    End Function

    ''' <summary>
    ''' Get the string supplied to particular parameter in the query.
    ''' </summary>
    ''' <param name="p">The parameter name - a lower case String.</param>
    ''' <returns>The string value corresponding to the supplied parameter,
    ''' or Nothing if no such parameter was specified in the query string.</returns>
    Public Function GetParameter(ByVal p As String) As String
        Dim val As String = Nothing
        mParameters.TryGetValue(p, val)
        Return val
    End Function

    ''' <summary>
    ''' Gets the boolean value of the given parameter in the query; False if the
    ''' parameter is unset.
    ''' </summary>
    ''' <param name="p">The parameter name - a lower case String.</param>
    ''' <returns>The parsed value associated with the given parameter in this query.
    ''' If the parameter does not exist in this query, or it is blank, this will
    ''' return false.
    ''' </returns>
    ''' <exception cref="InvalidValueException">If the parameter was found in the
    ''' query and the value was not empty, but could not be parsed into a boolean.
    ''' </exception>
    Public Function GetBoolParam(ByVal p As String) As Boolean
        ' Get the string value
        Dim pval As String = GetParameter(p)
        ' If it's not there, default to false
        If pval = "" Then Return False
        ' Try and parse it; if it succeeds parsing, return the resultant value
        Dim val As Boolean
        If Boolean.TryParse(pval, val) Then Return val
        ' Otherwise, parsing failed...
        Throw New InvalidValueException(
         My.Resources.InvalidFlagValue0For1Parameter, pval, p)
    End Function

    ''' <summary>
    ''' Gets the integer value of the given parameter in the query; Zero if the
    ''' parameter is unset.
    ''' </summary>
    ''' <param name="p">The parameter name - a lower case String.</param>
    ''' <returns>The parsed value associated with the given parameter in this query.
    ''' If the parameter does not exist in this query, or it is blank, this will
    ''' return 0.
    ''' </returns>
    ''' <remarks>This ultimately supports any conversion that CInt() will do on a
    ''' string - eg. <c>GetIntParam("2.5")</c> will round the value using banker's
    ''' rounding (prefer even numbers) and return the value - ie. 2 in that case.
    ''' </remarks>
    ''' <exception cref="InvalidValueException">If the parameter was found in the
    ''' query and the value was not empty, but could not be parsed into an integer.
    ''' </exception>
    Public Function GetIntParam(ByVal p As String) As Integer
        Return GetIntParam(p, True)
    End Function

    ''' <summary>
    ''' Gets the integer value of the given parameter in the query.
    ''' </summary>
    ''' <param name="p">The parameter name - a lower case String.</param>
    ''' <param name="allowEmpty">True will treat empty (or missing) param values as
    ''' zero; False will cause an error to be reported if the param value is empty
    ''' or missing.</param>
    ''' <returns>The parsed value associated with the given parameter in this query.
    ''' If the parameter does not exist in this query, or it is blank, this will
    ''' return 0.
    ''' </returns>
    ''' <remarks>This ultimately supports any conversion that CInt() will do on a
    ''' string - eg. <c>GetIntParam("2.5")</c> will round the value using banker's
    ''' rounding (prefer even numbers) and return the value - ie. 2 in that case.
    ''' </remarks>
    ''' <exception cref="InvalidValueException">If the parameter was found in the
    ''' query and the value was not empty, but could not be parsed into an integer
    ''' -or- if an empty value was found and <paramref name="allowEmpty"/> was false.
    ''' </exception>
    Public Function GetIntParam(ByVal p As String, ByVal allowEmpty As Boolean) _
     As Integer
        ' Get the parameter - return 0 if it does not exist
        Dim pval As String = GetParameter(p)
        If pval = "" Then
            If allowEmpty Then Return 0
            Throw New InvalidValueException(
             My.Resources.NoValueProvidedFor0Parameter, p)
        End If
        Try
            ' Convert using CInt() - it does other things than just a straight parse
            ' (eg. it rounds fractional values amongst other things - bug 8522)
            Return CInt(pval)

        Catch
            ' Throw an error if it fails
            Throw New InvalidValueException(
             My.Resources.InvalidIntegerValue0For1Parameter, pval, p)
        End Try

    End Function

    ''' <summary>
    ''' Gets the decimal value of the given parameter in the query; Zero if the
    ''' parameter is unset.
    ''' </summary>
    ''' <param name="p">The parameter name - a lower case String. Note that this
    ''' expects the value to be in <see cref="BPExpression.NormalForm">normal</see>
    ''' form</param>
    ''' <returns>The parsed value associated with the given parameter in this query.
    ''' If the parameter does not exist in this query, or it is blank, this will
    ''' return 0.
    ''' </returns>
    ''' <exception cref="InvalidValueException">If the parameter was found in the
    ''' query and the value was not empty, but could not be parsed into an decimal
    ''' </exception>
    Public Function GetDecimalParam(p As String) As Decimal
        Return GetDecimalParam(p, True)
    End Function

    ''' <summary>
    ''' Gets the decimal value of the given parameter in the query.
    ''' </summary>
    ''' <param name="p">The parameter name - a lower case String. Note that this
    ''' expects the value to be in <see cref="BPExpression.NormalForm">normal</see>
    ''' form</param>
    ''' <param name="allowEmpty">True will treat empty (or missing) param values as
    ''' zero; False will cause an error to be reported if the param value is empty
    ''' or missing.</param>
    ''' <returns>The parsed value associated with the given parameter in this query.
    ''' If the parameter does not exist in this query, or it is blank, this will
    ''' return 0 as long as <paramref name="allowEmpty"/> is true.</returns>
    ''' <exception cref="InvalidValueException">If the parameter was found in the
    ''' query and the value was not empty, but could not be parsed into an decimal
    ''' -or- if an empty value was found and <paramref name="allowEmpty"/> was false.
    ''' </exception>
    Public Function GetDecimalParam(p As String, allowEmpty As Boolean) As Decimal
        ' Get the parameter - return 0 if it does not exist
        Dim pval As String = GetParameter(p)
        If pval = "" Then
            If allowEmpty Then Return 0D
            Throw New InvalidValueException(
             My.Resources.NoValueProvidedFor0Parameter, p)
        End If

        Dim val As Decimal
        If InternalCulture.TryDec(pval, val) Then Return val

        ' If it failed to parse, explain the error.
        Throw New InvalidValueException(
         My.Resources.InvalidDecimalValue0For1Parameter, pval, p)

    End Function

    ''' <summary>
    ''' Gets the value of the given parameter in the query converted to a 
    ''' <see cref="clsPixRect">clsPixRect</see> image.
    ''' </summary>
    ''' <param name="p">The parameter name - a lower case String. Note that this
    ''' expects the value to be in <see cref="BPExpression.NormalForm">normal</see>
    ''' form</param>
    ''' <param name="allowEmpty">True will treat empty (or missing) param values as
    ''' empty image; False will cause an error to be reported if the param value is empty
    ''' or missing.</param>
    ''' <returns>The parsed image object associated with the given parameter in this query.
    ''' If the parameter does not exist in this query, or it is blank, this will
    ''' return a null reference as long as <paramref name="allowEmpty"/> is true.</returns>
    ''' <exception cref="InvalidValueException">If the parameter was found in the
    ''' query and the value was not empty, but could not be parsed into an image
    ''' -or- if an empty value was found and <paramref name="allowEmpty"/> was false.
    ''' </exception>
    Public Function GetImageParam(p As String, allowEmpty As Boolean) As clsPixRect
        ' Get the parameter - return 0 if it does not exist
        Dim pval As String = GetParameter(p)
        If pval = "" Then
            If allowEmpty Then Return Nothing
            Throw New InvalidValueException(
                My.Resources.NoValueProvidedFor0Parameter, p)
        End If
        Try
            Return New clsPixRect(pval)
        Catch ex As InvalidFormatException
            Throw New InvalidValueException(ex, String.Format(My.Resources.InvalidImageValueFor0Parameter, p))
        End Try

    End Function

    ''' <summary>
    ''' Tries to get the string supplied for the given parameter in this query.
    ''' </summary>
    ''' <param name="p">The parameter name - a lower case String.</param>
    ''' <param name="into">The string placeholder into which the parameter value
    ''' should be set.</param>
    ''' <returns>True if the parameter was found in this query and the value was set;
    ''' False if the parameter did not exist in this query.</returns>
    Public Function TryGetParameter(
     ByVal p As String, ByVal into As String) As Boolean
        Return mParameters.TryGetValue(p, into)
    End Function

    ''' <summary>
    ''' Checks if this query contains a value for the specified parameter
    ''' </summary>
    ''' <param name="p">The parameter name - a lower case String.</param>
    ''' <returns>True if this query contains an argument for the given parameter,
    ''' False otherwise.</returns>
    Public Function HasParameter(ByVal p As String) As Boolean
        Return mParameters.ContainsKey(p)
    End Function

    ''' <summary>
    ''' Checks if this query contains a value for all of the specified parameters.
    ''' </summary>
    ''' <param name="ps">The parameter names - lower case Strings.</param>
    ''' <returns>True if this query has a value set for all of the specified
    ''' parameter types; False if any of them are not represented in this query.
    ''' </returns>
    Public Function HasAllParameters(ByVal ParamArray ps() As String) As Boolean
        For Each tp As String In ps
            If Not mParameters.ContainsKey(tp) Then Return False
        Next
        Return True
    End Function

    ''' <summary>
    ''' Get the matcher associated with a particular identifier in the query.
    ''' </summary>
    ''' <param name="ident">The parameter ID</param>
    ''' <returns>The matcher corresponding to the supplied identifier,
    ''' or Nothing if no such identifier was specified in the query string.</returns>
    Public Function GetIdentifier(ByVal ident As IdentifierTypes) _
     As clsIdentifierMatchTarget
        Dim target As clsIdentifierMatchTarget = Nothing
        mIdentifiers.TryGetValue(ident, target)
        Return target
    End Function

    ''' <summary>
    ''' Gets the identifier match value for the given identifier or null if the
    ''' identifier is not present in this query
    ''' </summary>
    ''' <param name="ident">The type of identifier for which the match value is
    ''' required.</param>
    ''' <returns>The match value for the specified identifier in this query, or null
    ''' if this query does not contain the specified identifier.</returns>
    Public Function GetIdentifierValue(ByVal ident As IdentifierTypes) As String
        Dim tgt As clsIdentifierMatchTarget = GetIdentifier(ident)
        If tgt Is Nothing Then Return Nothing
        Return tgt.MatchValue
    End Function

    ''' <summary>
    ''' Gets the integer match value for the given identifier within this query.
    ''' </summary>
    ''' <param name="ident">The type of identifier for which the match value is
    ''' required.</param>
    ''' <returns>The integer match value for the specified identifier in this query
    ''' </returns>
    ''' <exception cref="InvalidValueException">If the specified identifer was not
    ''' present in this query or it could not be parsed into an integer.</exception>
    Public Function GetIdentifierIntValue(ByVal ident As IdentifierTypes) As Integer
        Dim sval As String = GetIdentifierValue(ident)
        If sval Is Nothing Then Throw New InvalidValueException(
         My.Resources.NoMatchValueFoundForIdentifier0, ident)
        Dim val As Integer
        If Not Integer.TryParse(sval, val) Then Throw New InvalidValueException(
         My.Resources.CouldNotConvert0IntoAnIntegerForIdentifier1, sval, ident)
        Return val
    End Function

    ''' <summary>
    ''' Tries to get the identifier match target corresponding to a specified
    ''' identifier within this query.
    ''' </summary>
    ''' <param name="ident">The ID of the identifier for which the matcher is
    ''' required.</param>
    ''' <param name="into">The placeholder match target object into which the
    ''' parameter value should be set.</param>
    ''' <returns>True if the identifier was found in this query and the match target
    ''' output was set; False if the identifier did not exist in this query</returns>
    Public Function TryGetIdentifier(ByVal ident As IdentifierTypes,
     ByVal into As clsIdentifierMatchTarget) As Boolean
        Return mIdentifiers.TryGetValue(ident, into)
    End Function

    ''' <summary>
    ''' Encode a value for use in a query. Any necessary escaping is carried out and
    ''' the value is placed in quotes if necessary.
    ''' </summary>
    ''' <param name="value">The value - the actual text value encoded is gleaned from
    ''' the value's <see cref="Object.ToString">ToString()</see> method.</param>
    ''' <returns>The value's string representation encoded as necessary.</returns>
    Public Shared Function EncodeValue(Of T)(value As T) As String
        Return If(value Is Nothing, "", EncodeValue(value.ToString()))
    End Function

    ''' <summary>
    ''' Encode a value for use in a query. Any necessary escaping is carried out and
    ''' the value is placed in quotes if necessary.
    ''' </summary>
    ''' <param name="value">The text, cannot be nothing</param>
    ''' <returns>The modified (if necessary) text.</returns>
    Public Shared Function EncodeValue(ByVal value As String) As String
        If value Is Nothing Then _
         Throw New ArgumentNullException(NameOf(value), My.Resources.CouldNotEncodeNullTextValue)

        If value.IndexOfAny(New Char() {Chr(10), Chr(13), " "c, """"c}) = -1 Then
            Return value
        End If
        value = value.Replace("\", "\\")
        value = value.Replace("}", "\c")
        value = value.Replace(vbCr, "\r")
        value = value.Replace(vbLf, "\n")
        value = value.Replace("""", "\""")
        value = """" & value & """"
        Return value
    End Function


    ''' <summary>
    ''' Parse a standard format query response into its two constituent parts.
    ''' The general format is RESPONSETTYPE:result. In this case, the two returned
    ''' values would be "RESPONSETYPE" and "result". It is also valid to have no
    ''' result type, just a lone type, e.g. "OK", in which case the values would
    ''' be "OK" and "" respectively.
    ''' </summary>
    ''' <param name="sResponse">The response to parse</param>
    ''' <param name="sResType">On exit, contains the response type</param>
    ''' <param name="sResult">On exit, contains the result text</param>
    Public Shared Sub ParseResponse(ByVal sResponse As String, ByRef sResType As String, ByRef sResult As String)
        Dim iIndex As Integer = CultureInfo.InvariantCulture.CompareInfo.IndexOf(sResponse ,":", System.Globalization.CompareOptions.IgnoreNonSpace)
        If iIndex = -1 Then
            sResType = sResponse
            sResult = ""
        Else
            sResType = sResponse.Substring(0, iIndex)
            sResult = sResponse.Substring(iIndex + 1)
        End If
    End Sub


    ''' <summary>
    ''' Splits a quoted comma delimited list into a list
    ''' </summary>
    ''' <param name="sResult">The comma delimited list to parse</param>
    ''' <returns>A list of strings parsed from the given encoded list</returns>
    Public Shared Function ParseValueList(ByVal sResult As String) As List(Of String)

        Dim state As Integer = 0        '0 - none, 1 - in item, 2 - in quoted item, 3 - in quotes in quoted item
        Dim list As New List(Of String)

        Dim word As String = ""
        For Each c As Char In sResult
            Select Case state
                Case 0
                    Select Case c
                        Case """"c
                            state = 2
                            word = ""
                        Case ","c
                            list.Add("")
                        Case Else
                            word &= c
                            state = 1
                    End Select
                Case 1
                    Select Case c
                        Case """"c
                            Throw New InvalidOperationException(My.Resources.BadListFormat)
                        Case ","c
                            list.Add(word)
                            state = 0
                            word = ""
                        Case Else
                            word &= c
                    End Select
                Case 2
                    Select Case c
                        Case """"c
                            state = 3
                        Case Else
                            word &= c
                    End Select
                Case 3
                    Select Case c
                        Case ","c
                            list.Add(word)
                            state = 0
                            word = ""
                        Case """"c
                            word &= c
                            state = 2
                        Case Else
                            Throw New InvalidOperationException(My.Resources.BadListFormat)
                    End Select
            End Select
        Next

        If state = 2 Then
            Throw New InvalidOperationException(My.Resources.MissingClosingQuoteInList)
        Else
            list.Add(word)
        End If

        Return list
    End Function

End Class
