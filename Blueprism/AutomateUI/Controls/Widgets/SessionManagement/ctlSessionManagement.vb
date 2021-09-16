Imports System.Text.RegularExpressions
Imports System.Threading
Imports System.Timers
Imports System.Xml
Imports AutomateControls
Imports AutomateControls.Forms
Imports AutomateUI.My.Resources.AutomateUI_Controls_Widgets_SessionManagement
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.Sessions
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.Sessions.SessionSortInfo
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.ClientServerResources.Core.Enums
Imports BluePrism.ClientServerResources.Core.Events
Imports BluePrism.Core.Resources
Imports BluePrism.Core.Xml
Imports BluePrism.Server.Domain.Models
Imports BluePrism.UIAutomation.Classes.SearchBar
Imports Internationalisation
Imports LocaleTools
Imports NLog
Imports BluePrism.Utilities.Functional
Imports BluePrism.AutomateAppCore.Sessions

' "Timer" is ambiguous since there is one in Windows.Forms and Threading namespaces
' We mean this one.
Imports Timer = System.Windows.Forms.Timer

''' Project  : Automate
''' Class    : ctlControlRoom
'''
''' <summary>
''' The control that represents the control room
''' </summary>
Friend Class ctlSessionManagement
    Inherits UserControl
    Implements IHelp, IPermission, IChild, IRefreshable

#Region " Class-scope Members (Inner classes, enums, statics) "

    ''' <summary>
    ''' Class containing the names of the columns in the environment listview.
    ''' </summary>
    Private Class ColumnNames
        Public Const Id As String = "ID"
        Public Const Process As String = "Process"
        Public Const Resource As String = "Resource"
        Public Const Status As String = "Status"
        Public Const User As String = "User"
        Public Const StartTime As String = "Start Time"
        Public Const EndTime As String = "End Time"
        Public Const LastStage As String = "Latest Stage"
        Public Const LastUpdated As String = "Stage Started"
    End Class

    ''' <summary>
    ''' Cache localisations for the SessionStatus enum. This may or may not be used in StartSelectedProcesses()
    ''' but if it is used at all - this lookup will likely save non-trivial hits to LTools.
    ''' </summary>
    Private Shared ReadOnly SessionStatusLocalisationLookup As Lazy(Of Dictionary(Of SessionStatus, String)) =
        New Lazy(Of Dictionary(Of SessionStatus, String))(Function()
                                                              Dim dict = New Dictionary(Of SessionStatus, String)
                                                              Dim loc = Options.Instance.CurrentLocale
                                                              For Each status As SessionStatus In [Enum].GetValues(GetType(SessionStatus))
                                                                  dict(status) = LTools.Get($"{status}", "misc", loc, "status")
                                                              Next
                                                              Return dict
                                                          End Function)

    ''' <summary>
    ''' Exception thrown when the background worker's actual work thread has
    ''' discovered a cancellation pending and it is cancelling its work.
    ''' </summary>
    <Serializable>
    Private Class WorkCancelledException : Inherits Exception : End Class

    ''' <summary>
    ''' Values which represent no filtering when entered into a filter column
    ''' </summary>
    ''' <remarks></remarks>
    Private Shared NullFilterValues As ICollection(Of String) =
        GetReadOnly.ICollectionFrom("", "All")
    Friend WithEvents pnlTopHalf As AutomateControls.SplitContainers.HighlightingSplitContainer
    Friend WithEvents pnlMainSplitter As AutomateControls.SplitContainers.HighlightingSplitContainer
    Friend WithEvents pnlBottomHalf As System.Windows.Forms.SplitContainer
    Private WithEvents llSessionVariables As System.Windows.Forms.LinkLabel
    Friend WithEvents pnlSessionVariables As System.Windows.Forms.Panel
    Friend WithEvents lvSessionVariables As AutomateUI.clsAutomateListView
    Friend WithEvents ColumnHeader1 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader2 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader3 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader4 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader5 As System.Windows.Forms.ColumnHeader
    Private WithEvents btnSaveView As AutomateControls.Buttons.StandardStyledButton
    Private WithEvents tvProcesses As AutomateUI.ProcessBackedMemberTreeListView
    Friend WithEvents cbFilterUsingSelectedProcess As CheckBox
    Friend WithEvents cbFilterUsingSelectedResource As CheckBox
    Friend WithEvents lblSessionRowLimit As Windows.Forms.Label
    Friend WithEvents ServerToolTip As ToolTip
    Private Shared ReadOnly Log As ILogger = LogManager.GetCurrentClassLogger()
    Friend WithEvents lblConnectionVia As Label
    Friend WithEvents FilterTextAndDropdown As FilterTextAndDropdown
    Friend WithEvents cmbSessionRowLimit As ComboBox



    ''' <summary>
    ''' Exception fired when a refresh of the environment is aborted due to a user
    ''' having a filter combo box open.
    ''' </summary>
    <Serializable>
    Private Class RefreshAbortedException
        Inherits ApplicationException
    End Class

    ''' <summary>
    ''' The maximum amount of time which should be allowed between environment view
    ''' refreshes.
    ''' </summary>
    Private ReadOnly Property MaxRefreshInterval As TimeSpan
        Get
            Return mConnManager.MaxRefreshInterval
        End Get
    End Property

    ''' <summary>
    ''' Delegate to use to retrieve an enumerator. Necessary to get the enumerator
    ''' for the items in the environment listview outside the UI thread.
    ''' </summary>
    ''' <returns>An enumerator</returns>
    Private Delegate Function EnumeratorGetter() As IEnumerator

    ''' <summary>
    ''' Extracts a lower bound date or datetime to use to represent the given text,
    ''' relative to the base date time passed in. This understands text in the form
    ''' "Today", "Last n Minutes", "Last n Hour/s"
    ''' or "Last n Days" where n is a positive integer.
    ''' </summary>
    ''' <param name="txt">The text to check against.</param>
    ''' <param name="base">The DateTime to use as a base for the relative terms
    ''' in the filter; typically this would be the current date and time</param>
    ''' <returns>A date or datetime to use as a lower bound which matches the given
    ''' days/hours/minutes to filter on in relation to the base date.</returns>
    Friend Shared Function ExtractDate(ByVal base As Date, ByVal txt As String) As Date

        If txt = "Today" Then Return base.ToMidnight()
        Dim m = Regex.Match(txt, "(Last|Over)\s(\d+)\s(Minute|Hour|Day)s?", RegexOptions.Singleline)
        If Not m.Success Then Return Nothing

        Dim quantity As Integer
        If Not Integer.TryParse(m.Groups(2).Value, quantity) Then Return Nothing

        Dim units = m.Groups(3).Value
        Select Case units
            Case "Minute"
                Return base - TimeSpan.FromMinutes(quantity)
            Case "Hour"
                Return base - TimeSpan.FromHours(quantity)
            Case "Day"
                Return base.Date - TimeSpan.FromDays(quantity)
            Case Else
                Debug.Print("Unrecognised unit: {0}", units)
                Return Nothing
        End Select

    End Function


#End Region

#Region "Windows Form Designer Generated Code"
    ''' <summary>
    ''' Releases the unmanaged resources used by the Component and optionally
    ''' releases the managed resources.
    ''' </summary>
    ''' <param name="disposing">true to release both managed and unmanaged resources;
    ''' false to release only unmanaged resources.</param>
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)

        If disposing Then
            ' Remove the WithEvents variables so we are disassociate from them
            ' (and are not trying to handle events after we've been disposed)
            mParent = Nothing
            If components IsNot Nothing Then components.Dispose()
            If mRefreshTimer IsNot Nothing Then mRefreshTimer.Dispose()
            If mResourceViewer IsNot Nothing Then mResourceViewer.Dispose()
            If mTimer IsNot Nothing Then
                RemoveHandler mTimer.Elapsed, AddressOf OnTimedEvent
                mTimer.Dispose()
            End If
            ' Potentially remove ascr event proxy
            If TypeOf mConnManager Is ServerConnectionManager Then
                RemoveHandler DirectCast(mConnManager, ServerConnectionManager).OnCallbackError, AddressOf CallbackChannelError
            End If
            mConnManager = Nothing
        End If

        mSessionStarter?.Dispose()
        mSessionCreator?.Dispose()
        mSessionDeleter?.Dispose()
        mSessionStopper?.Dispose()


        MyBase.Dispose(disposing)

    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    Private WithEvents lnkStart As System.Windows.Forms.LinkLabel
    Private WithEvents lnkStop As System.Windows.Forms.LinkLabel
    Friend WithEvents lDescription1 As System.Windows.Forms.Label
    Friend WithEvents pnlEnvironment As System.Windows.Forms.Panel
    Friend WithEvents lvEnvironment As AutomateUI.clsAutomateListView
    Friend WithEvents lProcesses As System.Windows.Forms.Label
    Friend WithEvents lEnvironment As System.Windows.Forms.Label
    Friend WithEvents mResourceViewer As AutomateUI.ctlResourceView
    Friend WithEvents lResource As System.Windows.Forms.Label
    Private WithEvents llBack As System.Windows.Forms.LinkLabel
    Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
    Friend WithEvents TableLayoutPanel2 As System.Windows.Forms.TableLayoutPanel
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlSessionManagement))
        Dim TreeListViewItemCollectionComparer1 As AutomateControls.TreeList.TreeListViewItemCollection.TreeListViewItemCollectionComparer = New AutomateControls.TreeList.TreeListViewItemCollection.TreeListViewItemCollectionComparer()
        Dim TreeListViewItemCollectionComparer2 As AutomateControls.TreeList.TreeListViewItemCollection.TreeListViewItemCollectionComparer = New AutomateControls.TreeList.TreeListViewItemCollection.TreeListViewItemCollectionComparer()
        Me.llBack = New System.Windows.Forms.LinkLabel()
        Me.ServerToolTip = New System.Windows.Forms.ToolTip(Me.components)
        Me.pnlMainSplitter = New AutomateControls.SplitContainers.HighlightingSplitContainer()
        Me.pnlTopHalf = New AutomateControls.SplitContainers.HighlightingSplitContainer()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.lDescription1 = New System.Windows.Forms.Label()
        Me.lProcesses = New System.Windows.Forms.Label()
        Me.cbFilterUsingSelectedProcess = New System.Windows.Forms.CheckBox()
        Me.TableLayoutPanel2 = New System.Windows.Forms.TableLayoutPanel()
        Me.cbFilterUsingSelectedResource = New System.Windows.Forms.CheckBox()
        Me.lResource = New System.Windows.Forms.Label()
        Me.lblConnectionVia = New System.Windows.Forms.Label()
        Me.FilterTextAndDropdown = New AutomateControls.FilterTextAndDropdown()
        Me.pnlBottomHalf = New System.Windows.Forms.SplitContainer()
        Me.pnlEnvironment = New System.Windows.Forms.Panel()
        Me.cmbSessionRowLimit = New System.Windows.Forms.ComboBox()
        Me.lblSessionRowLimit = New System.Windows.Forms.Label()
        Me.btnSaveView = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.lEnvironment = New System.Windows.Forms.Label()
        Me.llSessionVariables = New System.Windows.Forms.LinkLabel()
        Me.lnkStart = New System.Windows.Forms.LinkLabel()
        Me.lnkStop = New System.Windows.Forms.LinkLabel()
        Me.pnlSessionVariables = New System.Windows.Forms.Panel()
        Me.tvProcesses = New AutomateUI.ProcessBackedMemberTreeListView()
        Me.mResourceViewer = New AutomateUI.ctlResourceView()
        Me.lvEnvironment = New AutomateUI.clsAutomateListView()
        Me.lvSessionVariables = New AutomateUI.clsAutomateListView()
        Me.ColumnHeader1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader2 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader3 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader4 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader5 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        CType(Me.pnlMainSplitter, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlMainSplitter.Panel1.SuspendLayout()
        Me.pnlMainSplitter.Panel2.SuspendLayout()
        Me.pnlMainSplitter.SuspendLayout()
        CType(Me.pnlTopHalf, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlTopHalf.Panel1.SuspendLayout()
        Me.pnlTopHalf.Panel2.SuspendLayout()
        Me.pnlTopHalf.SuspendLayout()
        Me.TableLayoutPanel1.SuspendLayout()
        Me.TableLayoutPanel2.SuspendLayout()
        CType(Me.pnlBottomHalf, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.pnlBottomHalf.Panel1.SuspendLayout()
        Me.pnlBottomHalf.Panel2.SuspendLayout()
        Me.pnlBottomHalf.SuspendLayout()
        Me.pnlEnvironment.SuspendLayout()
        Me.pnlSessionVariables.SuspendLayout()
        Me.SuspendLayout()
        '
        'llBack
        '
        resources.ApplyResources(Me.llBack, "llBack")
        Me.llBack.BackColor = System.Drawing.Color.White
        Me.llBack.Name = "llBack"
        Me.llBack.TabStop = True
        '
        'pnlMainSplitter
        '
        Me.pnlMainSplitter.Cursor = System.Windows.Forms.Cursors.Default
        Me.pnlMainSplitter.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        resources.ApplyResources(Me.pnlMainSplitter, "pnlMainSplitter")
        Me.pnlMainSplitter.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.pnlMainSplitter.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.pnlMainSplitter.GripVisible = False
        Me.pnlMainSplitter.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184, Byte), Integer), CType(CType(201, Byte), Integer), CType(CType(216, Byte), Integer))
        Me.pnlMainSplitter.MouseLeaveColor = System.Drawing.Color.White
        Me.pnlMainSplitter.Name = "pnlMainSplitter"
        '
        'pnlMainSplitter.Panel1
        '
        Me.pnlMainSplitter.Panel1.Controls.Add(Me.pnlTopHalf)
        '
        'pnlMainSplitter.Panel2
        '
        Me.pnlMainSplitter.Panel2.Controls.Add(Me.pnlBottomHalf)
        Me.pnlMainSplitter.TabStop = False
        Me.pnlMainSplitter.TextColor = System.Drawing.Color.Black
        '
        'pnlTopHalf
        '
        Me.pnlTopHalf.Cursor = System.Windows.Forms.Cursors.Default
        Me.pnlTopHalf.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        resources.ApplyResources(Me.pnlTopHalf, "pnlTopHalf")
        Me.pnlTopHalf.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.pnlTopHalf.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.pnlTopHalf.GripVisible = False
        Me.pnlTopHalf.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184, Byte), Integer), CType(CType(201, Byte), Integer), CType(CType(216, Byte), Integer))
        Me.pnlTopHalf.MouseLeaveColor = System.Drawing.Color.White
        Me.pnlTopHalf.Name = "pnlTopHalf"
        '
        'pnlTopHalf.Panel1
        '
        Me.pnlTopHalf.Panel1.Controls.Add(Me.TableLayoutPanel1)
        '
        'pnlTopHalf.Panel2
        '
        Me.pnlTopHalf.Panel2.Controls.Add(Me.TableLayoutPanel2)
        Me.pnlTopHalf.TabStop = False
        Me.pnlTopHalf.TextColor = System.Drawing.Color.Black
        '
        'TableLayoutPanel1
        '
        resources.ApplyResources(Me.TableLayoutPanel1, "TableLayoutPanel1")
        Me.TableLayoutPanel1.Controls.Add(Me.lDescription1, 1, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.lProcesses, 0, 0)
        Me.TableLayoutPanel1.Controls.Add(Me.tvProcesses, 0, 1)
        Me.TableLayoutPanel1.Controls.Add(Me.cbFilterUsingSelectedProcess, 0, 2)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        '
        'lDescription1
        '
        Me.lDescription1.AutoEllipsis = True
        resources.ApplyResources(Me.lDescription1, "lDescription1")
        Me.lDescription1.BackColor = System.Drawing.Color.White
        Me.TableLayoutPanel1.SetColumnSpan(Me.lDescription1, 3)
        Me.lDescription1.ForeColor = System.Drawing.Color.Black
        Me.lDescription1.Name = "lDescription1"
        '
        'lProcesses
        '
        resources.ApplyResources(Me.lProcesses, "lProcesses")
        Me.lProcesses.BackColor = System.Drawing.Color.White
        Me.lProcesses.ForeColor = System.Drawing.Color.Black
        Me.lProcesses.Name = "lProcesses"
        '
        'cbFilterUsingSelectedProcess
        '
        resources.ApplyResources(Me.cbFilterUsingSelectedProcess, "cbFilterUsingSelectedProcess")
        Me.TableLayoutPanel1.SetColumnSpan(Me.cbFilterUsingSelectedProcess, 2)
        Me.cbFilterUsingSelectedProcess.Name = "cbFilterUsingSelectedProcess"
        Me.cbFilterUsingSelectedProcess.UseVisualStyleBackColor = True
        '
        'TableLayoutPanel2
        '
        resources.ApplyResources(Me.TableLayoutPanel2, "TableLayoutPanel2")
        Me.TableLayoutPanel2.Controls.Add(Me.cbFilterUsingSelectedResource, 0, 3)
        Me.TableLayoutPanel2.Controls.Add(Me.mResourceViewer, 0, 2)
        Me.TableLayoutPanel2.Controls.Add(Me.lResource, 0, 0)
        Me.TableLayoutPanel2.Controls.Add(Me.lblConnectionVia, 1, 0)
        Me.TableLayoutPanel2.Controls.Add(Me.FilterTextAndDropdown, 0, 1)
        Me.TableLayoutPanel2.Name = "TableLayoutPanel2"
        '
        'cbFilterUsingSelectedResource
        '
        resources.ApplyResources(Me.cbFilterUsingSelectedResource, "cbFilterUsingSelectedResource")
        Me.TableLayoutPanel2.SetColumnSpan(Me.cbFilterUsingSelectedResource, 3)
        Me.cbFilterUsingSelectedResource.Name = "cbFilterUsingSelectedResource"
        Me.cbFilterUsingSelectedResource.UseVisualStyleBackColor = True
        '
        'lResource
        '
        resources.ApplyResources(Me.lResource, "lResource")
        Me.lResource.BackColor = System.Drawing.Color.White
        Me.lResource.ForeColor = System.Drawing.Color.Black
        Me.lResource.Name = "lResource"
        '
        'lblConnectionVia
        '
        resources.ApplyResources(Me.lblConnectionVia, "lblConnectionVia")
        Me.TableLayoutPanel2.SetColumnSpan(Me.lblConnectionVia, 2)
        Me.lblConnectionVia.Name = "lblConnectionVia"
        Me.lblConnectionVia.UseCompatibleTextRendering = True
        '
        'FilterTextAndDropdown
        '
        Me.TableLayoutPanel2.SetColumnSpan(Me.FilterTextAndDropdown, 3)
        resources.ApplyResources(Me.FilterTextAndDropdown, "FilterTextAndDropdown")
        Me.FilterTextAndDropdown.DropDownHeight = 200
        Me.FilterTextAndDropdown.DropDownWidth = 369
        Me.FilterTextAndDropdown.IsDroppedDown = False
        Me.FilterTextAndDropdown.MaxDropDownItems = 6
        Me.FilterTextAndDropdown.Name = "FilterTextAndDropdown"
        Me.FilterTextAndDropdown.SelectedIndex = -1
        '
        'pnlBottomHalf
        '
        resources.ApplyResources(Me.pnlBottomHalf, "pnlBottomHalf")
        Me.pnlBottomHalf.Name = "pnlBottomHalf"
        '
        'pnlBottomHalf.Panel1
        '
        Me.pnlBottomHalf.Panel1.Controls.Add(Me.pnlEnvironment)
        '
        'pnlBottomHalf.Panel2
        '
        Me.pnlBottomHalf.Panel2.Controls.Add(Me.pnlSessionVariables)
        Me.pnlBottomHalf.Panel2Collapsed = True
        '
        'pnlEnvironment
        '
        Me.pnlEnvironment.Controls.Add(Me.cmbSessionRowLimit)
        Me.pnlEnvironment.Controls.Add(Me.lblSessionRowLimit)
        Me.pnlEnvironment.Controls.Add(Me.btnSaveView)
        Me.pnlEnvironment.Controls.Add(Me.lEnvironment)
        Me.pnlEnvironment.Controls.Add(Me.llSessionVariables)
        Me.pnlEnvironment.Controls.Add(Me.lnkStart)
        Me.pnlEnvironment.Controls.Add(Me.lvEnvironment)
        Me.pnlEnvironment.Controls.Add(Me.lnkStop)
        resources.ApplyResources(Me.pnlEnvironment, "pnlEnvironment")
        Me.pnlEnvironment.Name = "pnlEnvironment"
        '
        'cmbSessionRowLimit
        '
        resources.ApplyResources(Me.cmbSessionRowLimit, "cmbSessionRowLimit")
        Me.cmbSessionRowLimit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbSessionRowLimit.FormattingEnabled = True
        Me.cmbSessionRowLimit.Name = "cmbSessionRowLimit"
        Me.cmbSessionRowLimit.TabStop = False
        '
        'lblSessionRowLimit
        '
        resources.ApplyResources(Me.lblSessionRowLimit, "lblSessionRowLimit")
        Me.lblSessionRowLimit.Name = "lblSessionRowLimit"
        '
        'btnSaveView
        '
        Me.btnSaveView.BackColor = System.Drawing.Color.White
        Me.btnSaveView.FlatAppearance.BorderColor = System.Drawing.SystemColors.ButtonShadow
        resources.ApplyResources(Me.btnSaveView, "btnSaveView")
        Me.btnSaveView.Image = Global.AutomateUI.My.Resources.ToolImages.Save_16x16
        Me.btnSaveView.Name = "btnSaveView"
        Me.btnSaveView.UseVisualStyleBackColor = False
        '
        'lEnvironment
        '
        resources.ApplyResources(Me.lEnvironment, "lEnvironment")
        Me.lEnvironment.ForeColor = System.Drawing.Color.Black
        Me.lEnvironment.Name = "lEnvironment"
        '
        'llSessionVariables
        '
        resources.ApplyResources(Me.llSessionVariables, "llSessionVariables")
        Me.llSessionVariables.BackColor = System.Drawing.Color.White
        Me.llSessionVariables.Name = "llSessionVariables"
        Me.llSessionVariables.TabStop = True
        '
        'lnkStart
        '
        resources.ApplyResources(Me.lnkStart, "lnkStart")
        Me.lnkStart.BackColor = System.Drawing.Color.White
        Me.lnkStart.Name = "lnkStart"
        Me.lnkStart.TabStop = True
        '
        'lnkStop
        '
        resources.ApplyResources(Me.lnkStop, "lnkStop")
        Me.lnkStop.BackColor = System.Drawing.Color.White
        Me.lnkStop.Name = "lnkStop"
        Me.lnkStop.TabStop = True
        '
        'pnlSessionVariables
        '
        Me.pnlSessionVariables.Controls.Add(Me.lvSessionVariables)
        resources.ApplyResources(Me.pnlSessionVariables, "pnlSessionVariables")
        Me.pnlSessionVariables.Name = "pnlSessionVariables"
        '
        'tvProcesses
        '
        Me.tvProcesses.AllowDrop = True
        Me.tvProcesses.AllowDropOnGroups = False
        Me.TableLayoutPanel1.SetColumnSpan(Me.tvProcesses, 4)
        TreeListViewItemCollectionComparer1.Column = 0
        TreeListViewItemCollectionComparer1.SortOrder = System.Windows.Forms.SortOrder.Ascending
        Me.tvProcesses.Comparer = TreeListViewItemCollectionComparer1
        resources.ApplyResources(Me.tvProcesses, "tvProcesses")
        Me.tvProcesses.FocusedItem = Nothing
        Me.tvProcesses.HideSelection = False
        Me.tvProcesses.ManagePermissions = False
        Me.tvProcesses.MultiLevelSelect = True
        Me.tvProcesses.Name = "tvProcesses"
        Me.tvProcesses.SaveExpandedGroups = True
        Me.tvProcesses.ShowDescription = True
        Me.tvProcesses.ShowDocumentLiteralFlag = False
        Me.tvProcesses.ShowEmptyGroups = False
        Me.tvProcesses.ShowExposedWebServiceName = False
        Me.tvProcesses.TreeType = BluePrism.AutomateAppCore.Groups.GroupTreeType.Processes
        Me.tvProcesses.UpdateTreeFromStore = False
        Me.tvProcesses.UseCompatibleStateImageBehavior = False
        Me.tvProcesses.UseLegacyNamespaceFlag = False
        '
        'mResourceViewer
        '
        Me.mResourceViewer.AllowDrop = True
        Me.mResourceViewer.AllowDropOnGroups = False
        Me.mResourceViewer.BackColor = System.Drawing.Color.White
        Me.TableLayoutPanel2.SetColumnSpan(Me.mResourceViewer, 3)
        TreeListViewItemCollectionComparer2.Column = 0
        TreeListViewItemCollectionComparer2.SortOrder = System.Windows.Forms.SortOrder.Ascending
        Me.mResourceViewer.Comparer = TreeListViewItemCollectionComparer2
        resources.ApplyResources(Me.mResourceViewer, "mResourceViewer")
        Me.mResourceViewer.FocusedItem = Nothing
        Me.mResourceViewer.HideSelection = False
        Me.mResourceViewer.MultiLevelSelect = True
        Me.mResourceViewer.Name = "mResourceViewer"
        Me.mResourceViewer.SaveExpandedGroups = True
        Me.mResourceViewer.ShowEmptyGroups = False
        Me.mResourceViewer.TreeType = BluePrism.AutomateAppCore.Groups.GroupTreeType.Resources
        Me.mResourceViewer.UseCompatibleStateImageBehavior = False
        '
        'lvEnvironment
        '
        resources.ApplyResources(Me.lvEnvironment, "lvEnvironment")
        Me.lvEnvironment.FullRowSelect = True
        Me.lvEnvironment.HideSelection = False
        Me.lvEnvironment.Name = "lvEnvironment"
        Me.lvEnvironment.ShowGroups = False
        Me.lvEnvironment.UseCompatibleStateImageBehavior = False
        Me.lvEnvironment.View = System.Windows.Forms.View.Details
        '
        'lvSessionVariables
        '
        resources.ApplyResources(Me.lvSessionVariables, "lvSessionVariables")
        Me.lvSessionVariables.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1, Me.ColumnHeader2, Me.ColumnHeader3, Me.ColumnHeader4, Me.ColumnHeader5})
        Me.lvSessionVariables.FullRowSelect = True
        Me.lvSessionVariables.HideSelection = False
        Me.lvSessionVariables.Name = "lvSessionVariables"
        Me.lvSessionVariables.UseCompatibleStateImageBehavior = False
        Me.lvSessionVariables.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader1
        '
        resources.ApplyResources(Me.ColumnHeader1, "ColumnHeader1")
        '
        'ColumnHeader2
        '
        resources.ApplyResources(Me.ColumnHeader2, "ColumnHeader2")
        '
        'ColumnHeader3
        '
        resources.ApplyResources(Me.ColumnHeader3, "ColumnHeader3")
        '
        'ColumnHeader4
        '
        resources.ApplyResources(Me.ColumnHeader4, "ColumnHeader4")
        '
        'ColumnHeader5
        '
        resources.ApplyResources(Me.ColumnHeader5, "ColumnHeader5")
        '
        'ctlSessionManagement
        '
        Me.BackColor = System.Drawing.Color.White
        Me.Controls.Add(Me.pnlMainSplitter)
        Me.Name = "ctlSessionManagement"
        resources.ApplyResources(Me, "$this")
        Me.pnlMainSplitter.Panel1.ResumeLayout(False)
        Me.pnlMainSplitter.Panel2.ResumeLayout(False)
        CType(Me.pnlMainSplitter, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlMainSplitter.ResumeLayout(False)
        Me.pnlTopHalf.Panel1.ResumeLayout(False)
        Me.pnlTopHalf.Panel2.ResumeLayout(False)
        CType(Me.pnlTopHalf, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlTopHalf.ResumeLayout(False)
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.TableLayoutPanel1.PerformLayout()
        Me.TableLayoutPanel2.ResumeLayout(False)
        Me.TableLayoutPanel2.PerformLayout()
        Me.pnlBottomHalf.Panel1.ResumeLayout(False)
        Me.pnlBottomHalf.Panel2.ResumeLayout(False)
        CType(Me.pnlBottomHalf, System.ComponentModel.ISupportInitialize).EndInit()
        Me.pnlBottomHalf.ResumeLayout(False)
        Me.pnlEnvironment.ResumeLayout(False)
        Me.pnlEnvironment.PerformLayout()
        Me.pnlSessionVariables.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
#End Region

#Region " Published Events "

    ''' <summary>
    ''' Event raised when the 'Save View' function has been invoked by the user.
    ''' </summary>
    Friend Event SaveViewClick As EventHandler

    ''' <summary>
    ''' Event raised when the view state property has been set to a new value. Note
    ''' that this is not raised if a user changes an individual element of that view
    ''' state but if the <see cref="ViewStateEncoded"/> property has been set, either
    ''' internally from within this class, or directly from a different object.
    ''' </summary>
    Friend Event ViewStateChanged As ViewStateChangedEventHandler

#End Region

#Region " Member Variables "

    ''' <summary>
    ''' Lock object to ensure only one thread is attempting to refresh the
    ''' environment list at any one time.
    ''' </summary>
    Private ReadOnly mRefreshLock As New Object()

    ''' <summary>
    ''' The background worker responsible for refreshing the environment list
    ''' </summary>
    Private WithEvents mRefresher As BackgroundWorker

    ''' <summary>
    ''' The connection manager that this control room uses to view / maintain
    ''' connections to the resources in the environment.
    ''' </summary>
    Private WithEvents mConnManager As IResourceConnectionManager

    ''' <summary>
    ''' The ListView sorting class
    ''' </summary>
    Private WithEvents mSorter As clsListViewSorter
    Private mSortInfo As SessionSortInfo

    ''' <summary>
    ''' The session variables ListView sorting class
    ''' </summary>
    Private mVarSorter As clsListViewSorter

    ''' <summary>
    ''' Used to cause periodic refreshes.
    ''' </summary>
    Private WithEvents mRefreshTimer As Timer

    ''' <summary>
    ''' Date/time when the environment list was last refreshed. (Always UTC to avoid
    ''' any daylight-saving changeover issues)
    ''' </summary>
    Private mLastRefreshTime As DateTime

    ''' <summary>
    ''' A list of sessions that are going to start. Used to affect the environment
    ''' list context menu when the session is slow to start.
    ''' </summary>
    Private mSessionsAboutToRun As New List(Of clsProcessSession)

    ' Flag indicating whether the filter resizing is enabled for this control or not.
    Private mFilterResizeEnabled As Boolean

    ' Flag indicating that an environment update has been detected from the
    ' connection manager
    Private mRefreshQueued As Boolean

    ' Flag indicating that a resource status update has been detected from the
    ' connection manager
    Private mResourceUpdateQueued As Boolean
    Private mSessionVariablesUpdatedLock As New Object()
    Private mSessionVariablesUpdated As Boolean = False
    Private mShowSessionVariables As Boolean

    ' Flag indicating that filter change listeners should not update when set
    Private mSuspendFilterUpdates As Boolean

    Private mHasLoaded As Boolean = False
    Private mFirstViewStateSet As Boolean = True

    Private Const DefaultSessionRowLimit = 500
    Private Const TimerInterval As Integer = 4000
    Private ReadOnly mSessionRowLimitComboBoxValues As Object() = {500, 1000, 2500, 5000, 10000}
    Private mSessionRowLimit As Integer
    Private mSessionRowLimitLock As New Object()
    Private Event SessionRowLimitUpdated As SessionRowLimitChangedEventHandler
    Private mTimer As System.Timers.Timer
    Private WithEvents mSessionStarter As SessionStarter
    Private WithEvents mSessionCreator As SessionCreator
    Private WithEvents mSessionDeleter As SessionDeleter
    Private WithEvents mSessionStopper As SessionStopper
    Private ReadOnly mLog As Logger = LogManager.GetCurrentClassLogger()
    Private mSearchFilter As FilterQuery

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Default constructor fo the control room.
    ''' </summary>
    Public Sub New()

        ' Ensure that the filters are not being resized while we're still building.
        mFilterResizeEnabled = False

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        If LicenseManager.UsageMode = LicenseUsageMode.Runtime Then

            mSorter = New clsListViewSorter(lvEnvironment)
            ' Columns :-
            ' ID           : Integer
            ' ProcessName  : String
            ' ResourceName : String
            ' UserName     : String
            ' Status       : String
            ' StartTime    : Date
            ' EndTime      : Date

            mSorter.ColumnDataTypes = New Type() {
             GetType(Integer),
             GetType(String), GetType(String), GetType(String), GetType(String),
             GetType(Date), GetType(Date)}

            SessionSortInfo = SessionSortInfo.GetDefaultSortInfo()

            mVarSorter = New clsListViewSorter(lvSessionVariables)
            mVarSorter.ColumnDataTypes = New Type() {
             GetType(Integer), GetType(String), GetType(String), GetType(String), GetType(String)}
            mVarSorter.SortColumn = 1
            mVarSorter.Order = SortOrder.Ascending

            tvProcesses.Filter = ProcessGroupMember.PublishedAndNotRetiredFilter

            mResourceViewer.WithoutAttributes = ResourceAttribute.Retired Or ResourceAttribute.Debug
            PrepareEnvironment()
            BuildDropDownFilters()
            PopulateDropDownFilters()
            PopulateSessionRowLimitDropDown()

            'Create and start the refresh timer...
            mRefreshTimer = New Timer With {
                .Interval = 60000,
                .Enabled = False
            }

            'Store the default view state for the list, so that it can be returned to
            lvEnvironment.Tag = ViewStateEncoded

        End If

        AddHandler FilterTextAndDropdown.FilterTextChangedEventHandler, AddressOf SearchBar_TextChanged

        mFilterResizeEnabled = True
        CheckForIllegalCrossThreadCalls = False
        CreateTimer(TimerInterval)
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets the permission level for the control.
    ''' </summary>
    ''' <value>The permission level</value>
    Public ReadOnly Property RequiredPermissions() As ICollection(Of Permission) _
     Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName(Permission.Resources.ImpliedViewResource)
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the selected session in the Environment view. Note that if
    ''' multiple sessions are selected, this will return the first one in the list
    ''' </summary>
    Public Property SelectedSession() As clsProcessSession
        Get
            If lvEnvironment.SelectedItems.Count = 0 Then Return Nothing
            Return DirectCast(lvEnvironment.SelectedItems(0).Tag, clsProcessSession)
        End Get
        Set(ByVal value As clsProcessSession)
            lvEnvironment.SelectedItems.Clear()
            If value Is Nothing Then Return
            For Each item As ListViewItem In lvEnvironment.Items
                If value.Equals(item.Tag) Then item.Selected = True : Return
            Next
        End Set
    End Property

    ''' <summary>
    ''' Gets all of the currently selected sessions in the environment listview.
    ''' </summary>
    Private ReadOnly Property SelectedSessions As ICollection(Of clsProcessSession)
        Get
            Dim sessions As New List(Of clsProcessSession)
            For Each item As ListViewItem In lvEnvironment.SelectedItems
                sessions.Add(DirectCast(item.Tag, clsProcessSession))
            Next
            Return sessions
        End Get
    End Property

    ''' <summary>
    ''' The current sort information for the session list view
    ''' </summary>
    ''' <remarks>The setter ensures that the list view's underlying sorter is kept
    ''' in sync with the member variable</remarks>
    Private Property SessionSortInfo As SessionSortInfo
        Get
            Return mSortInfo
        End Get
        Set(value As SessionSortInfo)
            mSortInfo = value
            mSorter.SortColumn = mSortInfo.Column
            mSorter.Order = If(mSortInfo.Direction = SortDirection.Descending, SortOrder.Descending, SortOrder.Ascending)
        End Set
    End Property

    Private Property DataRefreshInProgress As Boolean = False
    Private Const MaxItemsInSessionActionBatch As Integer = 50

#End Region

    ''' <summary>
    ''' On the load event, we update the position of the session variables, and
    ''' reload the session management view state.
    ''' </summary>
    Protected Overrides Sub OnLoad(ByVal e As EventArgs)
        MyBase.OnLoad(e)
        UpdateSessionVariablesLayout()

        ' Get the values of the match selection filter checkboxes
        cbFilterUsingSelectedProcess.Checked = gSv.GetPref(PreferenceNames.Session.SessionFilterSelectedProcess, False)
        cbFilterUsingSelectedResource.Checked = gSv.GetPref(PreferenceNames.Session.SessionFilterSelectedResource, False)

        ' Waiting for the refresh to complete in the background
        While tvProcesses.Columns.Count = 0
            If IsDisposed Then Return
            Application.DoEvents()
        End While

        tvProcesses.Columns(0).Width = 200

        mRefreshQueued = True
        RefreshPendingViews()

        Try
            mResourceViewer.Columns(0).Width = 200
            mResourceViewer.Columns(1).Width = 100
            mResourceViewer.Columns(2).Width = 200
            mResourceViewer.Columns(3).Width = 100
            mResourceViewer.Columns(4).Width = 150
            mResourceViewer.Columns(5).Width = 300
        Catch
            'Do nothing
        End Try

        If mConnManager.UsingAppServerConnection Then
            lblConnectionVia.Text = String.Format(My.Resources.ConnectedViaAppServer0, mConnManager.ServerName)
        Else
            lblConnectionVia.Hide()
        End If

        mSessionStarter = New SessionStarter(mConnManager, gSv, AddressOf QueueRefresh, AddressOf UserMessage.Show)
        mSessionStopper = New SessionStopper(gSv, mConnManager, AddressOf QueueRefresh)
        mSessionCreator = New SessionCreator(mConnManager, gSv, AddressOf QueueRefresh, AddressOf UserMessage.Show)
        mSessionDeleter = New SessionDeleter(gSv, mConnManager, AddressOf QueueRefresh)
    End Sub


    ''' <summary>
    ''' When switching to another screen, save the session management view state.
    ''' </summary>
    ''' <param name="e"></param>
    Protected Overrides Sub OnVisibleChanged(e As EventArgs)
        MyBase.OnVisibleChanged(e)

        ' Save the 'match selection' filter checkbox states.
        gSv.SetUserPref(PreferenceNames.Session.SessionFilterSelectedProcess, cbFilterUsingSelectedProcess.Checked)
        gSv.SetUserPref(PreferenceNames.Session.SessionFilterSelectedResource, cbFilterUsingSelectedResource.Checked)
    End Sub

#Region "User interface updating methods"

    ''' <summary>
    ''' Refreshes all controls underneath this session management panel, optionally
    ''' showing a progress bar while updating the environment list. This function
    ''' forces an update of all views and ignores any flags which indicate a refresh
    ''' was pending.
    ''' </summary>
    Public Sub RefreshView() Implements IRefreshable.RefreshView
        'Refresh the processes view
        tvProcesses.UpdateView(True)
        tvProcesses.Refresh()
        'Refresh the other views
        mRefreshQueued = True
        mResourceUpdateQueued = True
        RefreshPendingViews()
    End Sub
    ''' <summary>
    ''' Updates all pending views and ensures that the environment is updated at
    ''' least as often as the interval defined in: <see cref="MaxRefreshInterval"/>.
    ''' The views will only be refreshed as needed, which is indicated by various
    ''' update queued flags.
    ''' </summary>
    Private Sub RefreshPendingViews()
        ' Check if the environment list needs updating.
        If mRefreshQueued OrElse DateTime.UtcNow - mLastRefreshTime > MaxRefreshInterval Then
            RefreshEnvironment(False, Nothing)

            'stop the thread, as we are going to refresh anyway.
            mTimer.Stop()

            ' Always update the resource viewer if the session list has been updated.
            ' Changes is session states will need to be updated in the resource status.
            mResourceUpdateQueued = True
            ' The mRefreshQueued flag is reset at the end of a refresh environment
            ' call so that user-requested refreshes reset the flag too.
        End If

        ' Check if the resource viewer needs updating
        If mResourceUpdateQueued Then
            mConnManager.GetLatestDBResourceInfo(ResourceAttribute.Retired Or ResourceAttribute.Debug)
            mResourceViewer.RefreshView()
            mResourceUpdateQueued = False
        End If

        'Update session variables list if anything has changed...
        If mShowSessionVariables Then
            Dim needUpdate As Boolean = False
            SyncLock mSessionVariablesUpdatedLock
                If mSessionVariablesUpdated Then
                    mSessionVariablesUpdated = False
                    'Just flag it here and do the actual update outside the lock so
                    'we don't hold it any longer than we need to.
                    needUpdate = True
                End If


            End SyncLock

            If needUpdate Then RefreshSessionVariables()
        End If

    End Sub

    ''' <summary>
    ''' Refreshes this control, ensuring that the data is updated at the same time
    ''' </summary>
    Public Shadows Sub Refresh()
        RefreshView()
        MyBase.Refresh()
    End Sub

    ''' <summary>
    ''' Builds the column headers for the environment view
    ''' </summary>
    Private Sub PrepareEnvironment()
        lvEnvironment.Columns.Add(ColumnNames.Id, My.Resources.ctlSessionManagement_ID, 30)
        lvEnvironment.Columns.Add(ColumnNames.Process, My.Resources.ctlSessionManagement_ProcessColHdr, 200)
        lvEnvironment.Columns.Add(ColumnNames.Resource, My.Resources.ctlSessionManagement_ResourceColHdr, 125)
        lvEnvironment.Columns.Add(ColumnNames.User, My.Resources.ctlSessionManagement_UserColHdr, 90)
        lvEnvironment.Columns.Add(ColumnNames.Status, My.Resources.ctlSessionManagement_StatusColHdr, 85)
        lvEnvironment.Columns.Add(ColumnNames.StartTime, My.Resources.ctlSessionManagement_StartTimeColHdr, 150)
        lvEnvironment.Columns.Add(ColumnNames.EndTime, My.Resources.ctlSessionManagement_EndTimeColHdr, 150)
        lvEnvironment.Columns.Add(ColumnNames.LastStage, My.Resources.ctlSessionManagement_LatestStageColHdr, 100)
        lvEnvironment.Columns.Add(ColumnNames.LastUpdated, My.Resources.ctlSessionManagement_StageStartedColHdr, 150)
        For i As Integer = lvEnvironment.Columns.Count - 1 To 1 Step -1
            With lvEnvironment.Columns.Item(i)
                .TextAlign = HorizontalAlignment.Left
            End With
        Next
    End Sub

#End Region

    ''' <summary>
    ''' Gets the sessions highlighted in the listview.
    ''' </summary>
    ''' <param name="omitPending">True to only return sessions which are not in a
    ''' pending state, False to return all selected sessions.</param>
    ''' <returns>Returns a collection containing a ProcessSession object
    ''' for each selected row in the environment listview.</returns>
    Public Function GetSelectedSessions(ByVal omitPending As Boolean) As List(Of ISession)
        Dim coll As New List(Of ISession)
        For Each lvi As ListViewItem In Me.lvEnvironment.SelectedItems
            Dim s As clsProcessSession = CType(lvi.Tag, clsProcessSession)
            If Not (omitPending AndAlso s.Status = SessionStatus.Pending) Then
                coll.Add(s)
            End If
        Next
        Return coll
    End Function

    ''' <summary>
    ''' Gets all selected sessions in the environment except any which are pending
    ''' </summary>
    ''' <returns>A list of currently selected non-pending sessions</returns>
    Public Function GetSelectedSessions() As List(Of ISession)
        Return GetSelectedSessions(True)
    End Function

    ''' <summary>
    ''' Determines if the supplied session has at least one parameter which is not
    ''' of type collection.
    ''' </summary>
    ''' <param name="sess">The session of interest.</param>
    ''' <returns>Returns true if the session has at least one parameter whose
    ''' data type is not collection; false otherwise.</returns>
    Private Function HasAnyStartupParams(ByVal sess As clsProcessSession) As Boolean
        Dim params = gSv.GetBlankProcessArguments(sess.ProcessID)
        Return params IsNot Nothing AndAlso
            params.Exists(Function(p) p.Value.DataType <> DataType.collection)
    End Function

    ''' <summary>
    ''' Gets a context menu based on the current selection.
    ''' Note: that this function calls cacheprocess, and then looks at the
    ''' Process object model to see if it has any start params.
    ''' </summary>
    ''' <param name="x">The x coordinate where the listview was right clicked</param>
    ''' <param name="y">The y coordinate where the listview was right clicked</param>
    ''' <returns></returns>
    Private Function GetContextMenu(ByVal x As Integer, ByVal y As Integer) As ContextMenu

        Dim ctxMenu As ContextMenu = New ContextMenu()

        Dim menuItems As Menu.MenuItemCollection = ctxMenu.MenuItems

        Dim hasSessionParams As Boolean
        Dim hasAnyPending As Boolean
        Dim hasAnyRunning As Boolean
        Dim hasAnyStalled As Boolean
        Dim hasAnyCompleted As Boolean
        Dim hasAnyFailed As Boolean
        Dim hasAnyStopped As Boolean
        Dim isSingleSession As Boolean
        Dim hasAnyPendingStop As Boolean
        Dim hasViewLogPermission As Boolean = User.Current.HasPermission("Audit - Process Logs")
        Dim canViewProcessDefinition As Boolean = False

        Dim lvi As ListViewItem = lvEnvironment.GetItemAt(x, y)
        Dim canControl As Boolean = True
        If lvi Is Nothing Then
            lvEnvironment.ContextMenu = Nothing

        ElseIf lvEnvironment.SelectedItems.Count <= 1 Then

            Dim sess = CType(lvi.Tag, clsProcessSession)

            If mSessionsAboutToRun.Contains(sess) _
             AndAlso sess.Status = SessionStatus.Pending Then
                hasAnyPending = False

            Else
                Select Case sess.Status
                    Case SessionStatus.Pending : hasAnyPending = True
                    Case SessionStatus.Running : hasAnyRunning = True
                    Case SessionStatus.Stalled : hasAnyStalled = True
                    Case SessionStatus.Completed : hasAnyCompleted = True
                    Case SessionStatus.Terminated : hasAnyFailed = True
                    Case SessionStatus.Stopped : hasAnyStopped = True
                    Case SessionStatus.StopRequested : hasAnyPendingStop = True
                End Select

            End If

            If sess.Status <> SessionStatus.Pending Then
                mSessionsAboutToRun.Remove(sess)
            End If

            canViewProcessDefinition = sess.ProcessPermissions.HasPermission(User.Current,
                                                                      Permission.ProcessStudio.ImpliedViewProcess)
            hasSessionParams = HasAnyStartupParams(sess)
            If Not CanControlResource(sess.ResourcePermissions, False) Then canControl = False
            isSingleSession = True

        Else

            Dim iPendingSessionsAboutToRun As Integer

            For Each lv As ListViewItem In lvEnvironment.SelectedItems

                Dim sess = CType(lv.Tag, clsProcessSession)

                If mSessionsAboutToRun.Contains(sess) _
                 AndAlso sess.Status = SessionStatus.Pending Then
                    iPendingSessionsAboutToRun += 1
                Else

                    'create a new context menu and reference to its item collection
                    Select Case sess.Status
                        Case SessionStatus.Pending : hasAnyPending = True
                        Case SessionStatus.Running : hasAnyRunning = True
                        Case SessionStatus.Stalled : hasAnyStalled = True
                        Case SessionStatus.Completed : hasAnyCompleted = True
                        Case SessionStatus.Terminated : hasAnyFailed = True
                        Case SessionStatus.Stopped : hasAnyStopped = True
                        Case SessionStatus.StopRequested : hasAnyPendingStop = True
                    End Select

                End If

                If sess.Status <> SessionStatus.Pending Then
                    mSessionsAboutToRun.Remove(sess)
                End If

                hasSessionParams = hasSessionParams OrElse HasAnyStartupParams(sess)
                If Not CanControlResource(sess.ResourcePermissions, False) Then canControl = False
            Next

            If iPendingSessionsAboutToRun > 0 Then
                hasAnyPending = False
            End If

        End If


        'populate the menu item collection
        Dim item As MenuItem
        item = menuItems.Add(My.Resources.ctlSessionManagement_GetContextMenu_Start, AddressOf HandleStartSelected)
        item.Enabled = hasAnyPending

        item = menuItems.Add(My.Resources.ctlSessionManagement_GetContextMenu_ImmediateStop, AddressOf HandleStopSelected)
        item.Enabled = canControl AndAlso hasAnyRunning OrElse hasAnyPendingStop OrElse hasAnyStalled

        item = menuItems.Add(My.Resources.ctlSessionManagement_RequestStop, AddressOf HandleStopRequestSelected)
        item.Enabled = canControl AndAlso hasAnyRunning OrElse hasAnyStalled

        menuItems.Add(My.Resources.ctlSessionManagement_GetContextMenu_Separator)

        item = menuItems.Add(My.Resources.ctlSessionManagement_GetContextMenu_Delete, AddressOf lnkDelete_context)
        item.Enabled = hasAnyPending

        item = menuItems.Add(My.Resources.ctlSessionManagement_GetContextMenu_ViewProcess, AddressOf PreviewProcess)
        item.Enabled = isSingleSession AndAlso
            User.Current.HasPermission(Permission.ProcessStudio.ImpliedViewProcess) AndAlso
            (hasAnyPending OrElse hasAnyRunning OrElse hasAnyCompleted OrElse hasAnyFailed OrElse hasAnyStopped) AndAlso
            canViewProcessDefinition

        menuItems.Add(My.Resources.ctlSessionManagement_GetContextMenu_Separator)

        item = menuItems.Add(My.Resources.ctlSessionManagement_GetContextMenu_StartupParameters, AddressOf ShowStartUpParameters)
        item.Enabled = hasSessionParams AndAlso hasAnyPending

        item = menuItems.Add(My.Resources.ctlSessionManagement_GetContextMenu_ViewLog, AddressOf ViewLog)
        item.Enabled = isSingleSession AndAlso hasViewLogPermission AndAlso
         (hasAnyRunning OrElse hasAnyCompleted OrElse hasAnyFailed OrElse hasAnyStopped OrElse hasAnyStalled)

        Return ctxMenu

    End Function

    ''' <summary>
    ''' The event handler called by the context menu for showing startup parameters
    ''' </summary>
    Private Sub ShowStartUpParameters(ByVal sender As Object, ByVal e As EventArgs)
        ShowStartUpParametersForm(False)
    End Sub


    ''' <summary>
    ''' Shows the startup parameters form.
    ''' </summary>
    Private Sub ShowStartUpParametersForm(ByVal starting As Boolean)

        ' Build up a list of sessions which we want to show the startup params dialog
        ' for. Keep track of the list view items corresponding to those sessions
        Dim sessions As New List(Of ISession)
        Dim hasParams As Boolean
        Dim canControl As Boolean = True
        Dim itemMap As New Dictionary(Of Guid, ListViewItem)
        For Each item As ListViewItem In lvEnvironment.SelectedItems
            Dim sess = CType(item.Tag, clsProcessSession)
            CacheProcess(sess)
            If sess.Status = SessionStatus.Pending Then
                If HasAnyStartupParams(sess) Then
                    hasParams = True
                    ' Clone the sessions for the dialog rather than adding the originals
                    Dim id As Guid = sess.SessionID
                    sessions.Add(sess.Clone())
                    itemMap(id) = item
                End If
                If Not CanControlResource(sess.ResourcePermissions, False) Then canControl = False
            End If
        Next

        ' If we're actually starting the sessions and none of them have any (valid)
        ' startup parameters, skip the form and just start them
        If Not hasParams AndAlso starting Then
            If canControl Then
                StartSelectedProcesses() : Return
            Else
                UserMessage.Show(
                ctlSessionManagement_ShowStartUpParametersForm_NoPermissionToControlResources)
                Return
            End If
        End If

        If sessions.Count > 0 Then
            Using paramsDlg As New frmStartParams()
                paramsDlg.SetEnvironmentColours(mParent)
                ' Set the sessions to display
                paramsDlg.Sessions = sessions

                ' And show the dialog
                paramsDlg.ReadOnly = Not canControl
                paramsDlg.ShowInTaskbar = False
                Dim res As DialogResult = paramsDlg.ShowDialog()

                ' If they are good to go
                If res = DialogResult.OK OrElse res = DialogResult.Yes Then
                    If canControl Then
                        For Each sess As clsProcessSession In paramsDlg.Sessions
                            itemMap(sess.SessionID).Tag = sess
                            Try
                                Dim token = gSv.RegisterAuthorisationToken(sess.ProcessID)
                                gSv.SetProcessStartParamsAs(token.ToString,
                                sess.SessionID, sess.ArgumentsXml)
                            Catch
                            End Try
                        Next
                    Else
                        UserMessage.Show(
                        ctlSessionManagement_ShowStartUpParametersForm_NoPermissionToControlResources)
                        Return
                    End If
                End If

                If paramsDlg.DialogResult = DialogResult.Yes Then
                    If canControl Then
                        StartSelectedProcesses()
                    Else
                        UserMessage.Show(
                        ctlSessionManagement_ShowStartUpParametersForm_NoPermissionToControlResources)
                        Return
                    End If
                End If

            End Using
        Else
            UserMessage.Show(
             ctlSessionManagement_ShowStartUpParametersForm_OnlyPendingProcessesCanBeStarted)
        End If
    End Sub

    ''' <summary>
    ''' Handles a 'view log' event, either from a double click or from a click on the
    ''' 'View Log' context menu item.
    ''' </summary>
    Private Sub ViewLog(ByVal sender As Object, ByVal e As EventArgs) _
     Handles lvEnvironment.DoubleClick
        If Not User.Current.HasPermission("Audit - Process Logs") Then Return
        Dim s As clsProcessSession = SelectedSession
        If s IsNot Nothing Then mParent.StartForm(New frmLogViewer(s.SessionID))
    End Sub

    ''' <summary>
    ''' We create the dropdown filters here they are populated later on
    ''' </summary>
    Private Sub BuildDropDownFilters()
        Const ComboBoxHeight As Integer = 23

        Dim offset As Integer = lvEnvironment.Left
        For Each col As ColumnHeader In lvEnvironment.Columns
            Dim combo As Control
            Select Case col.Name 'Text
                Case "Resource"
                    combo = New GroupMemberComboBox() With
                        {.TreeType = GroupTreeType.Resources,
                        .DropDownStyle = ComboBoxStyle.DropDownList,
                        .Checkable = True,
                        .NoSelectionAllowed = True,
                        .NoSelectionText = My.Resources.ctlSessionManagement_All,
                        .MaxDropDownItems = 16,
                        .Filter = Function(y)
                                      Dim rgm = TryCast(y, ResourceGroupMember)

                                      If rgm IsNot Nothing Then
                                          'Add unless the resource is both local and remote
                                          If rgm.Attributes.HasFlag(ResourceAttribute.Local) _
                                              AndAlso rgm.Name <> ResourceMachine.GetName() Then
                                              Return False
                                          End If

                                          Return Not (rgm.Attributes.HasFlag(ResourceAttribute.Retired) OrElse
                                                        rgm.Attributes.HasFlag(ResourceAttribute.Debug) OrElse
                                                        rgm.Attributes.HasFlag(ResourceAttribute.Pool))
                                      End If
                                      Return True
                                  End Function}

                Case "ID"
                    combo = Nothing
                Case "Process"
                    combo = New GroupMemberComboBox() With {
                        .TreeType = GroupTreeType.Processes,
                        .DropDownStyle = ComboBoxStyle.DropDownList,
                        .NoSelectionAllowed = True,
                        .NoSelectionText = My.Resources.ctlSessionManagement_All,
                        .MaxDropDownItems = 16,
                        .Checkable = True,
                        .Filter = ProcessGroupMember.PublishedAndNotRetiredFilter
                    }
                Case "User"
                    combo = New GroupMemberComboBox() With {
                        .TreeType = GroupTreeType.Users,
                        .DropDownStyle = ComboBoxStyle.DropDownList,
                        .NoSelectionAllowed = True,
                        .NoSelectionText = My.Resources.ctlSessionManagement_All,
                        .MaxDropDownItems = 16,
                        .Checkable = True
                    }
                Case Else
                    combo = New ComboBox() With {
                        .DropDownStyle = ComboBoxStyle.DropDownList
                    }
            End Select

            ' Set the combo box into the column header for easy access.
            col.Tag = combo

            If combo IsNot Nothing Then
                combo.Width = col.Width - 1
                ' This does nothing, height is determined by font size - combo.Height = ComboBoxHeight
                combo.Top = lvEnvironment.Top - ComboBoxHeight
                combo.Name = col.Name 'Text
                combo.Text = col.Text
                combo.Left = offset
                pnlEnvironment.Controls.Add(combo)
                combo.BringToFront()
            End If

            offset += col.Width
        Next
    End Sub

    ''' <summary>
    ''' Refreshes the Dropdown, called by the event handler of listviews columnheader
    ''' resize event.
    ''' </summary>
    Private Sub RefreshDropDowns()

        If Not mFilterResizeEnabled Then Return

        Dim offset As Integer = lvEnvironment.Left

        For Each colHeader As ColumnHeader In lvEnvironment.Columns
            Dim ctl As Control = TryCast(colHeader.Tag, Control)
            If ctl IsNot Nothing Then
                ctl.Left = offset
                ctl.Width = colHeader.Width - 1
            End If
            offset += colHeader.Width
        Next

    End Sub


    ''' <summary>
    ''' Gets or sets the current view state of this control
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend Property ViewStateEncoded As String
        Get
            Dim doc = New XmlDocument()
            Dim outer = doc.AppendChild(doc.CreateElement("viewstate"))
            Dim list = outer.AppendChild(doc.CreateElement("filters"))
            For Each ctl In pnlEnvironment.Controls.OfType(Of ComboBox)()
                Dim filter = doc.CreateElement("filter")
                filter.SetAttribute("name", ctl.Name)
                If TypeOf (ctl) Is GroupBasedComboBox Then
                    filter.InnerXml = DirectCast(ctl, GroupBasedComboBox).SelectedItemsXML
                Else
                    filter.InnerText = ctl.Text
                End If
                list.AppendChild(filter)
            Next

            outer.AppendChild(SessionSortInfo.ToXmlElement(doc))

            Return doc.OuterXml
        End Get

        Set(value As String)
            ' If there's no view state then use the list's default view state
            If value = "" Then Return

            ' If we already have this value, ignore it
            If value = ViewStateEncoded Then Return

            mSuspendFilterUpdates = True
            Try
                Dim xdoc As New ReadableXmlDocument(value)

                For Each element In xdoc.GetElementsByTagName("filter")
                    Dim xmlElement = CType(element, XmlElement)
                    Dim control = pnlEnvironment.Controls(xmlElement.Attributes("name").Value)
                    If control IsNot Nothing Then
                        If TypeOf (control) Is GroupBasedComboBox Then
                            DirectCast(control, GroupBasedComboBox).SelectedItemsXML = xmlElement.InnerXml
                        Else
                            ' Skip the cmbSessionRowLimit value setting as we want it to go back to the default value every time the control is loaded.
                            If Not TryCast(control, ComboBox)?.Equals(cmbSessionRowLimit) Then
                                control.Text = xmlElement.InnerText
                            End If
                        End If
                    End If
                Next

                Dim sortInfoNode = xdoc.GetElementsByTagName("viewstate")?.
                                    Item(0).
                                    ChildNodes.
                                    OfType(Of XmlElement).
                                    FirstOrDefault(Function(x) x.LocalName = "sortinfo")

                SessionSortInfo = If(sortInfoNode Is Nothing, SessionSortInfo.GetDefaultSortInfo(), SessionSortInfo.FromXmlElement(sortInfoNode))
                PopulateSessionRowLimitDropDown()
            Catch ex As Exception ' ignore in release builds - non-fatal error
                Debug.Fail("Invalid XML found for view state; Error: " & ex.Message,
                           "Failed ViewStateEncoded value: " & value)

            Finally
                If mFirstViewStateSet Then mFirstViewStateSet = False
                mSuspendFilterUpdates = False
            End Try
            OnViewStateChanged(New ViewStateChangedEventArgs(value))
        End Set
    End Property

    ''' <summary>
    ''' This function populates the dropdown filters
    ''' </summary>
    Private Sub PopulateDropDownFilters()

        For Each ctl As Control In Me.pnlEnvironment.Controls
            If ctl Is cmbSessionRowLimit Then Continue For

            Dim c As ComboBox = TryCast(ctl, ComboBox)
            If c Is Nothing Then Continue For
            Select Case c.Name
                Case ColumnNames.Resource, ColumnNames.Process
                    ' Loaded by control directly
                Case ColumnNames.Status
                    c.DisplayMember = "Text"
                    c.ValueMember = "Value"
                    Dim range() As String = {
                    "All", "Running", "Pending", "Completed", "Terminated", "Stopped", "StopRequested"
                    }
                    c.DataSource = AddL10nRange(range)
                    c.Text = My.Resources.ctlSessionManagement_All

                Case ColumnNames.StartTime, ColumnNames.EndTime
                    c.DisplayMember = "Text"
                    c.ValueMember = "Value"
                    Dim range() As String = {
                     "All", "Last 15 Minutes", "Last 30 Minutes", "Last 1 Hour", "Last 2 Hours", "Last 4 Hours", "Last 8 Hours", "Last 12 Hours",
                                    "Last 18 Hours", "Last 24 Hours", "Today", "Last 3 Days", "Last 7 Days", "Last 31 Days"
                    }
                    c.DataSource = AddL10nRange(range)
                    c.Text = If(c.Name = "Start Time", My.Resources.ctlSessionManagement_Today, My.Resources.ctlSessionManagement_All)

                Case ColumnNames.LastStage
                    c.Items.Add(My.Resources.ctlSessionManagement_All)
                    c.Text = My.Resources.ctlSessionManagement_All

                Case ColumnNames.LastUpdated
                    c.DisplayMember = "Text"
                    c.ValueMember = "Value"
                    Dim range() As String = {
                     "All", "Over 5 Minutes", "Over 10 Minutes", "Over 15 Minutes", "Over 30 Minutes", "Over 1 Hour", "Over 2 Hours"
                    }
                    c.DataSource = AddL10nRange(range)
                    c.Text = My.Resources.ctlSessionManagement_All
            End Select

            AddHandler c.SelectedIndexChanged, AddressOf FilterChanged

        Next
    End Sub

    ''' <summary>
    ''' Creates localized DataTable for Filter ComboBoxes.
    ''' </summary>
    ''' <returns>Localized DataTable</returns>
    ''' <param name="range">ComboBox AddRange strings</param>
    Private Function AddL10nRange(ByVal range As String()) As DataTable
        Dim tb As New DataTable
        tb.Columns.Add("Text", GetType(String))
        tb.Columns.Add("Value", GetType(String))
        For Each value As String In range
            Dim resxKey As String = "ctlSessionManagement_" & Regex.Replace(value, "\s*", "")
            Dim localizedText As String = My.Resources.ResourceManager.GetString(resxKey, My.Resources.Culture)
            If (localizedText Is Nothing) Then
                localizedText = value
            End If
            tb.Rows.Add(localizedText, value)
        Next
        Return tb
    End Function

    ''' <summary>
    ''' Extracts a lower bound date or datetime to use to represent the given text.
    ''' This understands text in the form "Today", "Last n Minutes", "Last n Hour/s"
    ''' or "Last n Days" where n is a positive integer.
    ''' </summary>
    ''' <returns>A date or datetime to use as a lower bound which matches the given
    ''' days/hours/minutes to filter on.</returns>
    ''' <param name="txt">The text to check against</param>
    Private Function ExtractDate(ByVal txt As String) As Date
        Return ExtractDate(Date.UtcNow, txt)
    End Function

    ''' <summary>
    ''' Gets the currently set filter values into a map.
    ''' </summary>
    ''' <returns>A map containing the following keys and values:-<list>
    ''' <item>resource: A string containing the resource name to filter on</item>
    ''' <item>status: A <see cref="BluePrism.Server.Domain.Models.SessionStatus"/> value containing the session
    ''' status to filter on.</item>
    ''' <item>user: A string containing the user name to search for</item>
    ''' <item>start: A DateTime containing the start date/time from when all sessions
    ''' should be shown - ie. all session started after this date/time are included.
    ''' </item>
    ''' <item>end: A DateTime containing the end date/time from when all sessions
    ''' should be shown - ie. all sessions ended after this date/time are included.
    ''' </item>
    ''' </list>
    ''' If an entry does not exist in the list, then it should not be filtered on at
    ''' all.
    ''' </returns>
    Private Function GetFilterValues() As IDictionary(Of String, Object)
        Dim map As New Dictionary(Of String, Object)
        For Each ctl As Control In pnlEnvironment.Controls

            ' Check the filter isn't dropped down - if it is, then
            ' the user has opened a dropdown - abort this refresh by
            ' throwing an exception that will be caught in RefreshEnvironment.
            If (IsDroppedDown(ctl)) Then Throw New RefreshAbortedException()

            Dim val As String = Nothing 'clsUserInterfaceUtils.GetValue(ctl).Trim()
            If (ctl.Name = ColumnNames.StartTime Or ctl.Name = ColumnNames.EndTime Or
                ctl.Name = ColumnNames.Status Or ctl.Name = ColumnNames.LastUpdated) Then
                Dim combo As ComboBox = TryCast(ctl, ComboBox)
                If combo IsNot Nothing Then val = CStr(combo.SelectedValue).Trim()
            End If
            Dim nullValues() As String = {"", My.Resources.ctlSessionManagement_All}
            Dim txt As String = clsUserInterfaceUtils.GetText(ctl).Trim()
            If Not (ctl.Name = ColumnNames.Resource OrElse
                    ctl.Name = ColumnNames.Process OrElse
                    ctl.Name = ColumnNames.User) AndAlso
                    nullValues.Contains(txt) Then Continue For

            Select Case ctl.Name
                Case ColumnNames.Process
                    Dim combobox = DirectCast(ctl, GroupMemberComboBox)
                    If combobox.NoSelectionSelected Then
                        map("process") = {"All"}.ToList
                    Else
                        map("process") = combobox.GetCheckedItems()
                    End If
                Case ColumnNames.Resource
                    Dim combobox = DirectCast(ctl, GroupMemberComboBox)
                    If combobox.NoSelectionSelected Then
                        map("resource") = {"All"}.ToList
                    Else
                        map("resource") = combobox.GetCheckedItems()
                    End If
                Case ColumnNames.User
                    Dim combobox = DirectCast(ctl, GroupMemberComboBox)
                    If combobox.NoSelectionSelected Then
                        map("user") = {"All"}.ToList
                    Else
                        Dim checkedUsers = combobox.GetCheckedItems()
                        RemoveSquareBracketsFromCheckedSystemUserNames(checkedUsers)
                        map("user") = checkedUsers
                    End If

                Case ColumnNames.StartTime : map("start") = ExtractDate(val)
                Case ColumnNames.EndTime : map("end") = ExtractDate(val)
                Case ColumnNames.LastUpdated : map("lastupdated") = ExtractDate(val)
                Case ColumnNames.Status
                    If (val IsNot Nothing) Then
                        map("status") = clsEnum.Parse(val, True, SessionStatus.All)
                    End If
            End Select
        Next
        Return map
    End Function

    Private Sub RemoveSquareBracketsFromCheckedSystemUserNames(ByRef checkedUsers As List(Of String))

        Dim checkedSystemUsers = checkedUsers.Where(Function(x) x = "[Scheduler]" OrElse x = "[Anonymous Resource]")
        If Not checkedSystemUsers.Any() Then Return

        Dim systemUserNamesWithoutSquareBrackets As New List(Of String)

        For Each user In checkedSystemUsers
            systemUserNamesWithoutSquareBrackets.Add(user.Substring(1, user.Length - 2))
        Next

        checkedUsers.RemoveAll(Function(x) x = "[Scheduler]" OrElse x = "[Anonymous Resource]")
        checkedUsers.AddRange(systemUserNamesWithoutSquareBrackets)

    End Sub


    ''' <summary>
    ''' Function to check if a combo box is currently dropped down or not.
    ''' This is separated in order to be able to check it from a different thread.
    ''' </summary>
    ''' <param name="ctl">The control to check to see if it is a dropped down combo
    ''' box or not.</param>
    ''' <returns>True if the given control is a dropped down combo box, false
    ''' otherwise.</returns>
    Private Function IsDroppedDown(ByVal ctl As Object) As Boolean
        If TypeOf ctl Is ComboBox OrElse TypeOf ctl Is GroupMemberComboBox Then
            Return clsUserInterfaceUtils.GetProperty(Of Boolean)(
                DirectCast(ctl, Control), "DroppedDown")
        End If
        Return False
    End Function

    ''' <summary>
    ''' Returns sessions matching the filter list criteria.
    ''' </summary>
    ''' <returns>A datatable of session data</returns>
    ''' <exception cref="RefreshAbortedException">If the user has activated a combo
    ''' box and the refresh has been aborted as a result.</exception>
    Private Function GetFilteredSessions(maxSessionCount As Integer) As ICollection(Of clsProcessSession)
        Dim process As List(Of String) = Nothing
        Dim resource As List(Of String) = Nothing
        Dim users As List(Of String) = Nothing

        Dim status As SessionStatus = SessionStatus.All
        Dim startDate As Date
        Dim endDate As Date
        Dim lastUpdated As Date
        Dim sErr As String = Nothing
        Dim map As IDictionary(Of String, Object) = GetFilterValues()
        Dim o As Object = Nothing

        If map.TryGetValue("process", o) Then process = DirectCast(o, List(Of String))
        If map.TryGetValue("resource", o) Then resource = DirectCast(o, List(Of String))
        If map.TryGetValue("user", o) Then users = DirectCast(o, List(Of String))
        If map.TryGetValue("status", o) Then status = DirectCast(o, SessionStatus)
        If map.TryGetValue("start", o) Then startDate = DirectCast(o, DateTime)
        If map.TryGetValue("end", o) Then endDate = DirectCast(o, DateTime)
        If map.TryGetValue("lastupdated", o) Then
            lastUpdated = DirectCast(o, DateTime)
            ' When filtering on the last updated column it only
            ' makes sense for running sessions
            status = SessionStatus.Running
        End If

        Try
            Dim filteredSessions = gSv.GetFilteredSessions(process, resource,
             users, status, startDate, endDate, lastUpdated, False, False, maxSessionCount, SessionSortInfo)

            CheckToLocaliseSystemUserNames(filteredSessions)

            Return filteredSessions
        Catch ex As Exception
            ShowMessage(String.Format(
             My.Resources.ctlSessionManagement_AnErrorOccurredWhileRefreshingTheEnvironmentList0,
             ex.Message), ex)

            Return GetEmpty.ICollection(Of clsProcessSession)()

        End Try
    End Function

    Private Sub CheckToLocaliseSystemUserNames(filteredSessions As ICollection(Of clsProcessSession))

        Dim sessionsRunBySystemAccounts = filteredSessions.Where(Function(x) x.UserName = "[Scheduler]" OrElse x.UserName = "[Anonymous Resource]")

        If sessionsRunBySystemAccounts.Any() Then

            Dim translatedScheduler = ResMan.GetString("ctlControlRoom_tv_scheduler")
            Dim translatedAnonymousResource = My.Resources.AnonymousResource

            For Each session In sessionsRunBySystemAccounts
                If session.UserName = "[Scheduler]" Then
                    session.UserName = $"[{translatedScheduler}]"
                    Continue For
                End If
                session.UserName = $"[{translatedAnonymousResource}]"
            Next

        End If

    End Sub

    ''' <summary>
    ''' Delegate to allow user messages to be shown by non-UI threads without too much
    ''' trouble.
    ''' </summary>
    ''' <param name="msg">The message to display to the user.</param>
    ''' <param name="ex">The exception to show in the user message.</param>
    Private Delegate Sub MessageHandler(ByVal msg As String, ByVal ex As Exception)

    ''' <summary>
    ''' Shows the user the given message, ensuring that the dialog is shown in the UI
    ''' thread so that OEM calls (eg. copying the message text to the clipboard) work
    ''' as expected.
    ''' </summary>
    ''' <param name="msg">The message to display to the user.</param>
    Private Sub ShowMessage(ByVal msg As String)
        ShowMessage(msg, Nothing)
    End Sub

    ''' <summary>
    ''' Shows the user a message indicating the given exception and prefix,
    ''' ensuring that the dialog is shown in the UI thread so that OEM calls (eg.
    ''' copying the message text to the clipboard) work as expected.
    ''' </summary>
    ''' <param name="msg">The message to display to the user.</param>
    ''' <param name="ex">The exception to display in the user message box. Null if
    ''' no exception should be included in the message</param>
    Private Sub ShowMessage(ByVal msg As String, ByVal ex As Exception)
        If InvokeRequired Then _
         Invoke(New MessageHandler(AddressOf ShowMessage), msg, ex) : Return

        UserMessage.Show(msg, ex)
    End Sub

    ''' <summary>
    ''' Refreshes the environment list, showing a progress bar indicating how much
    ''' progress has been made.
    ''' </summary>
    Private Sub RefreshEnvironment()
        RefreshEnvironment(True, Nothing)
    End Sub

    ''' <summary>
    ''' Refreshes the environment list, showing a progress bar as specified.
    ''' </summary>
    ''' <param name="userRequest">True if the user requested the refresh - either by
    ''' hitting the refresh toolbar item, or changing a filter - this results in the
    ''' control being locked until the refresh is complete and a progress bar being
    ''' displayed. False otherwise.
    ''' Note that if the parent form is unset, it is assumed that this panel is not
    ''' currently on the screen, so no progress bar is displayed - also, in that
    ''' case, the work is done in the current thread rather than a background thread.
    ''' </param>
    ''' <param name="statuses">The statuses mapped against the resources whose status
    ''' has changed. A null dictionary will cause all sessions to be refreshed
    ''' </param>
    Private Sub RefreshEnvironment(ByVal userRequest As Boolean,
     ByVal statuses As IDictionary(Of String, ResourceStatusChange))

        ' Refreshing during long loading periods was causing a performance degredation.
        If Not mHasLoaded Then Return

        ' If we can't get a lock then something is already refreshing the environment,
        ' just leave it to it.
        ' NOTE: While it's nice having the statuses in the dictionary and all, with
        ' this line here, it's impossible to use them - since some changes may just
        ' get discarded if they come in while others are being processed.
        ' We need some kind of synchronized queueing to really be able to handle
        ' specific resources updating rather than the broad brush currently in use.
        If Not Monitor.TryEnter(mRefreshLock, 10) Then Return

        ' If this page hasn't been loaded yet, there's no need to use the progress
        ' bar (in fact it won't work since it requires a loaded control to anchor
        ' against), so just refresh it in the background.
        Try
            lvEnvironment.BeginUpdate()
            ' If the user requested a refresh, clear the current items.
            ' This ensures that any items get their Y position set correctly. (bug 5712)
            If userRequest Then lvEnvironment.Items.Clear()

            If mParent Is Nothing OrElse Not userRequest Then
                PerformEnvironmentRefresh(False, statuses)
            Else
                ' Create the background worker if it doesn't already exist
                If mRefresher Is Nothing Then
                    mRefresher = New BackgroundWorker()
                    mRefresher.WorkerReportsProgress = True
                    mRefresher.WorkerSupportsCancellation = True
                End If
                ' And call it via the progress dialog.
                Dim res As RunWorkerCompletedEventArgs = ProgressDialog.Show(
                 Me, mRefresher, statuses, My.Resources.ctlSessionManagement_EnvironmentList, My.Resources.ctlSessionManagement_RefreshingData)
            End If

        Finally
            lvEnvironment.EndUpdate()
            mRefreshQueued = False
            Monitor.Exit(mRefreshLock)
            mRefreshTimer.Interval = 1000
        End Try

    End Sub
    Private mIsRefreshing As Boolean = False

    ''' <summary>
    ''' Handles the doing of the work for the refresh background worker.
    ''' </summary>
    Private Sub HandleRefresherDoWork(
     ByVal sender As Object, ByVal e As DoWorkEventArgs) Handles mRefresher.DoWork
        Try
            Dim map As IDictionary(Of String, ResourceStatusChange) =
             TryCast(e.Argument, IDictionary(Of String, ResourceStatusChange))
            PerformEnvironmentRefresh(True, map)

        Catch cancelled As WorkCancelledException
            e.Cancel = True
        End Try
    End Sub

    ''' <summary>
    ''' Checks if there is a cancellation pending in the refresher background
    ''' worker object, if it exists.
    ''' </summary>
    Private Sub UpdateBackgroundWorker(ByVal updateProgress As Boolean, ByVal progress As Integer)
        If mRefresher IsNot Nothing Then
            If mRefresher.CancellationPending Then Throw New WorkCancelledException()
            If updateProgress Then mRefresher.ReportProgress(progress)
        End If
    End Sub

    ''' <summary>
    ''' Non-destructively refresh the environment list to reflect the current
    ''' situation. Items and selections remain intact throughout the process.
    ''' </summary>
    Private Sub PerformEnvironmentRefresh(
     ByVal updateProgress As Boolean,
     ByVal statuses As IDictionary(Of String, ResourceStatusChange))

        Try
            If mIsRefreshing Then Return

            mIsRefreshing = True

            UpdateBackgroundWorker(updateProgress, 10)

            Dim filteredSessions = GetFilteredSessions(mSessionRowLimit)

            UpdateBackgroundWorker(updateProgress, 25)

            ' Get everything out of the datatable into a process session.
            Dim sessionsToAdd As New clsOrderedDictionary(Of Guid, clsProcessSession)
            For Each session In filteredSessions
                sessionsToAdd(session.SessionID) = session
            Next

            UpdateBackgroundWorker(updateProgress, 30)

            Dim itemsToRemove As New List(Of ListViewItem)
            Dim itemsToUpdate As New Dictionary(Of ListViewItem, clsProcessSession)
            Dim itemsToInsert As New List(Of ListViewItem)

            ' OK, all the sessions are now in the list of sessions.
            ' Go through all our list view items and check if they exist in the retrieved
            ' sessions. If they don't add them to a list to remove, if they do, update with
            ' the latest data.

            ' Unfortunately getting an enumerator on the listview item collection must be
            ' done on the UI thread (?!), so we need to invoke it.
            Dim enu As IEnumerator
            If InvokeRequired Then
                enu = DirectCast(
                 lvEnvironment.Invoke(New EnumeratorGetter(AddressOf GetEnvironmentEnumerator)),
                 IEnumerator)
            Else
                enu = lvEnvironment.Items.GetEnumerator()
            End If

            While enu.MoveNext

                Dim item As ListViewItem = DirectCast(enu.Current, ListViewItem)

                Dim sess As clsProcessSession = DirectCast(item.Tag, clsProcessSession)
                If Not sessionsToAdd.ContainsKey(sess.SessionID) Then
                    itemsToRemove.Add(item)
                Else
                    Dim updatedSession As clsProcessSession = sessionsToAdd(sess.SessionID)
                    sessionsToAdd.Remove(sess.SessionID)
                    If Not sess.Equals(updatedSession) Then itemsToUpdate(item) = updatedSession
                End If
            End While

            UpdateBackgroundWorker(updateProgress, 40)

            ' Anything left in sessionsToInsert is now to be inserted into the listview.
            ' Build up the collection.
            Dim configOptions = Options.Instance

            For Each sess As clsProcessSession In sessionsToAdd.Values
                Dim item As ListViewItem = New ListViewItem(sess.SessionNum.ToString)
                item.Tag = sess
                With item.SubItems
                    .Add(sess.ProcessName)
                    .Add(sess.ResourceName)
                    .Add(sess.UserName)
                    .Add(LTools.Get(sess.GetUpdatedStatusText(mConnManager), "misc", configOptions.CurrentLocale, "status"))
                    .Add(sess.SessionStartText)
                    .Add(sess.SessionEndText)
                    .Add(LTools.Get(sess.LastStageText, "misc", configOptions.CurrentLocale, "stage"))
                    .Add(sess.LastUpdatedText)
                End With
                SetColour(item)
                itemsToInsert.Add(item)
            Next

            UpdateBackgroundWorker(updateProgress, 50)

            ' OK, let's get to work on the listview proper
            ' Once we start doing this, we can't cancel any more
            If InvokeRequired Then
                Invoke(New ThreadSafeListViewRefresher(AddressOf RefreshListView),
                 itemsToRemove, itemsToUpdate, itemsToInsert)
            Else
                RefreshListView(itemsToRemove, itemsToUpdate, itemsToInsert)
            End If

            ' We don't use UpdateBackgroundWorker() here because cancellation at
            ' this point would be a bit pointless
            If updateProgress Then mRefresher.ReportProgress(95)

        Catch aborted As RefreshAbortedException
            ' User had a filter combo open - skip the refresh since the user is
            ' about to force a new refresh anyway
            mIsRefreshing = False

        Catch ex As Exception
            ShowMessage(My.Resources.ctlSessionManagement_AnErrorOccurredWhileRefreshingTheEnvironment &
             vbCrLf & ex.Message, ex)
            mIsRefreshing = False

        Finally
            'Record when we last refreshed...
            mLastRefreshTime = DateTime.UtcNow

        End Try

    End Sub

    ''' <summary>
    ''' Function to get the enumerator for the list view items in the environment
    ''' list view.
    ''' </summary>
    ''' <returns>The List View Item enumerator for the environment listview</returns>
    Private Function GetEnvironmentEnumerator() As IEnumerator
        Return lvEnvironment.Items.GetEnumerator()
    End Function

    ''' <summary>
    ''' Function to get the enumerator for the list view selected items in the environment
    ''' list view.
    ''' </summary>
    ''' <returns>The List View Item enumerator for the environment listview</returns>
    Private Function GetEnvironmentSelectedItemsEnumerator() As IEnumerator
        Return lvEnvironment.SelectedItems.GetEnumerator()
    End Function


    ''' <summary>
    ''' Delegate describing a method which updates the list view with the
    ''' given items.
    ''' </summary>
    ''' <param name="itemsToRemove">The list view items which should be removed
    ''' from the environment list view.</param>
    ''' <param name="itemsToUpdate">A map of list view items which should be
    ''' udpated on the environment list view mapped against by the data with
    ''' which the items should be updated.</param>
    ''' <param name="itemsToInsert">The list view items which should be inserted
    ''' onto the environment list view.</param>
    Private Delegate Sub ThreadSafeListViewRefresher(
     ByVal itemsToRemove As ICollection(Of ListViewItem),
     ByVal itemsToUpdate As IDictionary(Of ListViewItem, clsProcessSession),
     ByVal itemsToInsert As ICollection(Of ListViewItem))

    ''' <summary>
    ''' Refreshes the list view representing the sessions.
    ''' </summary>
    ''' <param name="itemsToRemove">The items to remove from the listview</param>
    ''' <param name="itemsToUpdate">The items to update in the listview with the
    ''' associated session information to update them with</param>
    ''' <param name="itemsToInsert">The items to insert into the listview</param>
    Private Sub RefreshListView(
     ByVal itemsToRemove As ICollection(Of ListViewItem),
     ByVal itemsToUpdate As IDictionary(Of ListViewItem, clsProcessSession),
     ByVal itemsToInsert As ICollection(Of ListViewItem))


        ' First get rid of all the ones we don't need
        For Each item As ListViewItem In itemsToRemove
            lvEnvironment.Items.Remove(item)
        Next

        ' Then update the ones that are there, but have changed.
        Dim configOptions = Options.Instance
        For Each pair As KeyValuePair(Of ListViewItem, clsProcessSession) In itemsToUpdate
            Dim item As ListViewItem = pair.Key
            Dim sess As clsProcessSession = pair.Value

            item.SubItems(SessionManagementColumn.ResourceName).Text = sess.ResourceName
            item.SubItems(SessionManagementColumn.ProcessName).Text = sess.ProcessName
            item.SubItems(SessionManagementColumn.UserName).Text = sess.UserName
            item.SubItems(SessionManagementColumn.Status).Text = LTools.Get(sess.GetUpdatedStatusText(mConnManager), "misc", configOptions.CurrentLocale, "status")
            item.SubItems(SessionManagementColumn.StartTime).Text = sess.SessionStartText
            item.SubItems(SessionManagementColumn.EndTime).Text = sess.SessionEndText
            item.SubItems(SessionManagementColumn.LastStage).Text = LTools.Get(sess.LastStageText, "misc", configOptions.CurrentLocale, "stage")
            item.SubItems(SessionManagementColumn.LastUpdated).Text = sess.LastUpdatedText
            item.Tag = sess
            SetColour(item)
        Next

        ' Finally, insert the new ones.
        Dim itemArray(itemsToInsert.Count - 1) As ListViewItem
        itemsToInsert.CopyTo(itemArray, 0)
        lvEnvironment.Items.AddRange(itemArray)
        RefreshDropDowns()

        lvEnvironment.ListViewItemSorter = mSorter
        lvEnvironment.SetSortIcon(mSorter.SortColumn, mSorter.Order)

        If lvEnvironment.FocusedItem IsNot Nothing Then
            lvEnvironment.FocusedItem.EnsureVisible()
        ElseIf lvEnvironment.SelectedItems.Count > 0 Then
            lvEnvironment.SelectedItems(0).EnsureVisible()
        ElseIf lvEnvironment.Items.Count > 0 Then
            lvEnvironment.ScrollPosition = New Point(0, 0)
        End If


        mIsRefreshing = False

    End Sub

    ''' <summary>
    ''' Update the colors and font of the given item.
    ''' </summary>
    ''' <param name="item">The item whose colour is to be set.</param>
    Private Sub SetColour(ByVal item As ListViewItem)
        Select Case CType(item.Tag, clsProcessSession).Status

            Case SessionStatus.Pending : item.ForeColor = Color.Orange
            Case SessionStatus.StopRequested : item.ForeColor = Color.DarkGreen
            Case SessionStatus.Terminated : item.ForeColor = Color.Black
            Case SessionStatus.Stopped : item.ForeColor = Color.Red
            Case SessionStatus.Completed : item.ForeColor = Color.Blue
            Case SessionStatus.Stalled : item.ForeColor = Color.Purple
            Case SessionStatus.Running
                item.ForeColor = Color.Green
                'Bug 3423 - override colour for "Not Responding" situation
                Dim subitem = item.SubItems(SessionManagementColumn.Status)
                If subitem IsNot Nothing AndAlso subitem.Text = "Not Responding" Then
                    item.ForeColor = Color.Black
                End If
        End Select
        item.UseItemStyleForSubItems = True
        item.Font = lvEnvironment.Font

    End Sub

    ''' <summary>
    ''' Get the filename for the helpfile
    ''' </summary>
    ''' <returns></returns>
    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "frmControlRoom.htm"
    End Function

    Private mParent As frmApplication
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            If IsDisposed Then Exit Property
            mParent = value
            mResourceViewer.ParentAppForm = mParent
            If mParent IsNot Nothing Then
                mConnManager = mParent.ConnectionManager

                ' Handle the event proxy for ASCR callback errors
                ' The alternatives are over engineering an interface hierarchy between these managers or keep adding dud IResourceConnectionManager methods.
                ' We cannot just rely on ASCR pref as PersistentConnectionMangaer will be used for direct db connections
                If TypeOf mConnManager Is ServerConnectionManager Then
                    AddHandler DirectCast(mConnManager, ServerConnectionManager).OnCallbackError, AddressOf CallbackChannelError
                End If
            End If
        End Set
    End Property

    ''' <summary>
    ''' Event Handler for the Start Link
    ''' </summary>
    Private Sub lnkStart_LinkClicked(
     ByVal sender As Object, ByVal e As LinkLabelLinkClickedEventArgs) Handles lnkStart.LinkClicked
        HandleStartSelected(sender, e)
    End Sub


    ''' <summary>
    ''' Event Handler for starting the current selected sessions
    ''' </summary>
    Private Sub HandleStartSelected(ByVal sender As Object, ByVal e As EventArgs)
        If CanControlResource() Then ShowStartUpParametersForm(True)
    End Sub

    ''' <summary>
    ''' Event Handler for the Stop Link
    ''' </summary>
    Private Sub lnkStop_LinkClicked(
     ByVal sender As Object, ByVal e As LinkLabelLinkClickedEventArgs) Handles lnkStop.LinkClicked
        HandleStopSelected(sender, e)
    End Sub

    ''' <summary>
    ''' Checks if the user has the <c>Control Resource</c>
    ''' permission, optionally displaying a user message if they do not.
    ''' </summary>
    ''' <param name="permissions">The permissions for the resource, if one is selected</param>
    ''' <param name="showError">True to show an error message if the user does not
    ''' have the appropriate permission; False to omit the message</param>
    ''' <returns>True if the user has the necessary permission; false otherwise.
    ''' </returns>
    Private Function CanControlResource(
     Optional permissions As IMemberPermissions = Nothing,
     Optional showError As Boolean = True) As Boolean
        If User.Current.HasPermission(Permission.Resources.ControlResource) Then
            ' If no resource context, then role-level permission is enough
            If permissions Is Nothing Then Return True
            ' Otherwise check group-level permission
            If permissions.HasPermission(User.Current, Permission.Resources.ControlResource) Then Return True
        End If

        ' Else... they do not have permission
        If showError Then UserMessage.ShowPermissionMessage()
        Return False
    End Function

    ''' <summary>
    ''' Event Handler for stopping the selected sessions
    ''' </summary>
    Private Sub HandleStopSelected(ByVal sender As Object, ByVal e As EventArgs)
        If CanControlResource() Then StopSelectedProcesses()
    End Sub

    ''' <summary>
    ''' Handles the 'Request Safe Stop' menu item being chosen.
    ''' </summary>
    Private Sub HandleStopRequestSelected(sender As Object, e As EventArgs)
        If CanControlResource() Then RequestStopOnSelectedSessions()
    End Sub

    ''' <summary>
    ''' Sets a stop request on all selected sessions, indicating to the user which
    ''' sessions the request had an effect on.
    ''' </summary>
    Private Sub RequestStopOnSelectedSessions()
        ' Keep a list of the session nos for which the stop request had an effect
        Dim succeeded As New List(Of Integer)

        ' Keep a count of all selected sessions
        Dim count As Integer = 0

        ' Go through each session and request a stop, saving any which had an effect
        ' into the 'requested' list
        For Each sess As clsProcessSession In SelectedSessions
            If gSv.RequestStopSession(sess.SessionNum) Then succeeded.Add(sess.SessionNum)
            count += 1
        Next

        ' If any of the selected sessions weren't "stop request"ed, tell the user
        If count <> succeeded.Count Then

            ' begin a message for the user.
            Dim msg As String = ""
            If succeeded.Count = 0 Then
                msg = My.Resources.ctlSessionManagement_NoStopRequestsWereEffected

            ElseIf succeeded.Count = 1 Then
                msg = String.Format(My.Resources.ctlSessionManagement_StopRequestedOnSession0, succeeded(0))

            Else
                msg = String.Format(My.Resources.ctlSessionManagement_StopRequestedOnSessions0,
                    CollectionUtil.Join(succeeded, My.Resources.ctlSessionManagement_JoinComma))

            End If

            ' Now compare the 'requested' list vs. the 'count' of selected sessions
            ' to see if any stop requests had no effect.
            If count <> succeeded.Count Then
                msg &= vbCrLf &
                    My.Resources.ctlSessionManagement_AnyOtherSelectedSessionsAreEitherNotRunningOrAStopRequestIsAlreadyInPlaceForThem
            End If

            UserMessage.Show(msg)
        End If

        ' And refresh the environment to pick up the new status (if any requests
        ' succeeded - pointless otherwise)
        If succeeded.Count > 0 Then RefreshEnvironment()

    End Sub

    ''' <summary>
    ''' Event Handler for the Delete link in the context menu
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub lnkDelete_context(ByVal sender As Object, ByVal e As EventArgs)
        If CanControlResource() Then DeletePendingSessions()
    End Sub

    ''' <summary>
    ''' Event Handler for the Filters
    ''' </summary>
    Private Sub FilterChanged(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If Not mSuspendFilterUpdates AndAlso Not IsDroppedDown(sender) Then _
             RefreshEnvironment()

        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlSessionManagement_UnexpectedError0, ex.Message), ex)

        End Try
    End Sub

    Private Sub HandleBeforeEnvironmentSort(sender As Object, e As clsListViewSorter.BeforeSortEventArgs) Handles mSorter.BeforeSort
        Dim sortOrder = If(e.SortOrder = System.Windows.Forms.SortOrder.Descending, SortDirection.Descending, SortDirection.Ascending)
        SessionSortInfo = New SessionSortInfo(GetSessionManagementColumn(e.ColumnIndex), sortOrder)
        RefreshEnvironment(True, Nothing)
        e.CancelSort = True
    End Sub

    Private Function GetSessionManagementColumn(columnIndex As Integer) As SessionManagementColumn
        Dim column As SessionManagementColumn
        [Enum].TryParse(columnIndex.ToString(), column)
        Return column
    End Function


    ''' <summary>
    ''' Event Handler for drag events
    ''' </summary>
    Private Sub lvResourceList_DragEnter(ByVal sender As Object, ByVal e As DragEventArgs)
        If CanControlResource(Nothing, False) Then
            If (e.Data.GetDataPresent(DataFormats.Text)) Then
                e.Effect = DragDropEffects.Move
            Else
                e.Effect = DragDropEffects.None
            End If
        End If
    End Sub

    ''' <summary>
    ''' Event Handler for the Preview Process link
    ''' </summary>
    Private Sub mProcessLocator_PreviewProcessID(
     ByVal sName As String, ByVal gProcessID As Guid)
        If CanControlResource() Then
            mParent.StartForm(
                New frmProcess(ProcessViewMode.PreviewProcess, gProcessID, "", ""))
        End If
    End Sub


    ''' <summary>
    ''' Event Handler for the Status changed callback that ctlResourceView calls,
    ''' when a process starts...stops...etc....
    ''' </summary>
    Private Sub HandleResourceStatusChanged(
     sender As Object, ByVal e As ResourcesChangedEventArgs) _
     Handles mConnManager.ResourceStatusChanged

        Try
            mLog.Trace($"ResourceStatusChanged")
            'The status on one or more resources changed, so refresh the
            'environment view as processes may have started, stopped,
            'completed, etc...

            'Also update the resource view to update number of pending / running sessions etc
            If (e.OverallChange And ResourceStatusChange.EnvironmentChange) <> 0 Then
                mRefreshQueued = True
                mResourceUpdateQueued = True
            End If

            If (e.OverallChange And ResourceStatusChange.OnlineOrOfflineChange) <> 0 Then
                mResourceUpdateQueued = True
            End If

            'Also display any pending user messages...
            If (e.OverallChange And ResourceStatusChange.UserMessageWaiting) <> 0 Then
                Dim msg As String = Nothing
                While mConnManager.TryGetNextUserMessage(msg)
                    ShowMessage(msg)
                End While
            End If

        Catch ex As Exception
            UserMessage.Show(My.Resources.ctlSessionManagement_AnUnexpectedErrorOccurredWhilstUpdatingTheUserInterfacePleaseContactBluePrismFo &
             String.Format(My.Resources.ctlSessionManagement_TheErrorMessageWas0, ex.Message), ex)

        End Try
    End Sub


    ''' <summary>
    ''' Event handler that is called when a process is requested to be previewed
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub PreviewProcess(ByVal sender As Object, ByVal e As EventArgs)
        If lvEnvironment.SelectedItems.Count > 0 Then
            Dim item As ListViewItem = lvEnvironment.SelectedItems(0)
            Dim sess As clsProcessSession = CType(item.Tag, clsProcessSession)
            mParent.StartForm(
                New frmProcess(ProcessViewMode.PreviewProcess, sess.ProcessID, "", ""))
        End If
    End Sub

    ''' <summary>
    ''' Event handler for the Environment MouseDown (show the context menu)
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub lvEnvironment_MouseDown(ByVal sender As System.Object, ByVal e As MouseEventArgs) Handles lvEnvironment.MouseDown

        If e.Button = System.Windows.Forms.MouseButtons.Right Then
            Dim objSessions As New List(Of ISession)
            For Each lv As ListViewItem In lvEnvironment.SelectedItems
                Dim objSession As clsProcessSession = CType(lv.Tag, clsProcessSession)
                objSessions.Add(objSession)
            Next

            lvEnvironment.ContextMenu = GetContextMenu(e.X, e.Y)
        Else

            'The ForeColor of the listview is applied to the selected item
            'when the listview loses focus or when the user clicks in open
            'space in the list. These lines are an attempt to maintain
            'the colours of each item.
            If lvEnvironment.GetItemAt(e.X, e.Y) Is Nothing Then
                'clicked in open space
                If Not lvEnvironment.FocusedItem Is Nothing Then
                    'items are selected, so apply the colour of the first selected item
                    lvEnvironment.ForeColor = lvEnvironment.FocusedItem.ForeColor
                End If
            Else
                'clicked on an item so apply its colour to the list
                lvEnvironment.ForeColor = lvEnvironment.GetItemAt(e.X, e.Y).ForeColor
            End If

        End If

    End Sub


    ''' <summary>
    ''' Event handler for SessionVariables MouseUp (show the context menu)
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub lvSessionVariables_MouseUp(ByVal sender As System.Object, ByVal e As MouseEventArgs) Handles lvSessionVariables.MouseUp

        If e.Button = MouseButtons.Right Then
            Dim menu As ContextMenu = New ContextMenu()
            Dim menuItems As Menu.MenuItemCollection = menu.MenuItems

            Dim item As MenuItem
            item = menuItems.Add(LTools.Format(My.Resources.ctlSessionManagement_plural_EditValues, "COUNT", lvSessionVariables.SelectedItems.Count), New System.EventHandler(AddressOf lnkEditValue_context))
            item = menuItems.Add(My.Resources.ctlSessionManagement_plural_RefreshValues, New System.EventHandler(AddressOf RefreshValue_context))
            item.Enabled = lvSessionVariables.SelectedItems.Count > 0

            lvSessionVariables.ContextMenu = menu
        End If

    End Sub

    ''' <summary>
    ''' Handles resizing of the last column of the list view
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub CtlResourceView_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles mResourceViewer.Resize
        Dim resourceList = CType(sender, ListView)
        If (resourceList.Columns.Count > 0) Then
            resourceList.AutoResizeColumn(resourceList.Columns.Count - 1, ColumnHeaderAutoResizeStyle.HeaderSize)
        End If
    End Sub

    ''' <summary>
    ''' Event Handler for the Edit Value item in the session variables context menu.
    ''' </summary>
    Private Sub lnkEditValue_context(ByVal sender As Object, ByVal e As EventArgs)

        'Must have Control Resource to be able to edit the values.
        'Note that we still make the context item menu available, but block the
        'access here with a message. This seems better then getting "why Is it greyed
        'out?" questions
        If Not CanControlResource() Then Return

        If lvSessionVariables.SelectedItems.Count = 0 Then Return
        Dim uniquevars As New Dictionary(Of String, clsSessionVariable)
        Dim allvars As New List(Of clsSessionVariable)
        For Each li As ListViewItem In lvSessionVariables.SelectedItems
            Dim var As clsSessionVariable = CType(li.Tag, clsSessionVariable)
            allvars.Add(var)
            'Need a deep clone of the clsSessionVariable, because the controls in the
            'edit form are going to modify it (even if changes aren't applied!)
            var = var.DeepClone()
            Dim key As String = var.Name & "]" & var.Value.DataType.ToString()
            If uniquevars.ContainsKey(key) Then
                'We've already got this name/datatype combination before. We just
                'need to check if the value differs from the existing one. (Unless
                'a previous one has already differed, in which case it's irrelevant)
                If Not uniquevars(key).Indeterminate Then
                    If Not var.Value.Equals(uniquevars(key).Value) Then
                        uniquevars(key).Indeterminate = True
                    End If
                End If
            Else
                'Haven't seen this name/datatype combination before, so just add it.
                uniquevars(key) = var
            End If
        Next
        Dim f As New frmSessionVariables()
        f.ShowInTaskbar = False
        f.SetEnvironmentColoursFromAncestor(ParentForm)
        f.Vars = New List(Of clsSessionVariable)(uniquevars.Values)
        If f.ShowDialog(Me) = DialogResult.OK Then
            Dim sessionVarsToUpdate As New Queue(Of clsSessionVariable)
            Dim errors As New List(Of String)

            For Each editFormSessionVariable As clsSessionVariable In f.Vars
                For Each var As clsSessionVariable In allvars
                    If var.Name = editFormSessionVariable.Name AndAlso Not var.Value.Equals(editFormSessionVariable.Value) Then
                        'Modifying the value on this instance will modify the one used by
                        'our ListView but it doesn't really matter.
                        var.Value = editFormSessionVariable.Value.Clone
                        If var.ResourceID = Guid.Empty Then
                            errors.Add(My.Resources.ctlSessionManagement_DonTKnowWhichResourceToTalkTo)
                        Else
                            sessionVarsToUpdate.Enqueue(var)
                        End If
                    End If
                Next
            Next

            If sessionVarsToUpdate.Count > 0 Then
                Try
                    For Each sessVarGroup In sessionVarsToUpdate.GroupBy(Function(s) s.SessionID)
                        Dim sessionVarQueue = New Queue(Of clsSessionVariable)(sessVarGroup.ToList())
                        mConnManager.SendSetSessionVariable(sessionVarQueue.First.ResourceID, sessionVarQueue)
                    Next

                    SyncLock mSessionVariablesUpdatedLock
                        mSessionVariablesUpdated = True
                    End SyncLock
                Catch ex As Exception
                    errors.Add(ex.Message)
                End Try
            End If
            If errors.Count <> 0 Then
                Dim msg As String =
                 String.Format(My.Resources.ctlSessionManagement_0ErrorsOccurredWhileSetting1SessionVariables, errors.Count, f.Vars.Count)
                For Each err As String In errors
                    msg &= vbCrLf & err
                Next
                UserMessage.Show(msg)
            End If
        End If
        f.Dispose()
    End Sub


    ''' <summary>
    ''' Links up the process that was cached with the sessions in the lietview.
    ''' </summary>
    ''' <param name="objProcess"></param>
    Private Sub linkupProcess(ByVal objProcess As clsProcess)
        For Each lv As ListViewItem In lvEnvironment.Items
            Dim objSession As clsProcessSession = CType(lv.Tag, clsProcessSession)
            If objSession.ProcessID.Equals(objProcess.Id) Then
                objSession.Process = objProcess
            End If
        Next
    End Sub

    ''' <summary>
    ''' Loads a process object by pulling the XML from the database and de-serializing
    ''' the XML, This function is quite expensive, so if the process already exists in
    ''' the cache the processes will not be fetched or de-serialised.
    ''' </summary>
    ''' <param name="sess"></param>
    Private Sub CacheProcess(ByRef sess As clsProcessSession)
        Dim sErr As String = Nothing

        If sess.Process Is Nothing Then
            Dim sXML As String
            Try
                sXML = gSv.GetProcessXML(sess.ProcessID)
            Catch ex As Exception
                'Errors are ignored here currently
                sXML = ""
            End Try

            sess.Process = clsProcess.FromXML(
             Options.Instance.GetExternalObjectsInfo(), sXML, False, sErr)
            sess.Process.Id = sess.ProcessID
            sess.Process.Name = sess.ProcessName
        End If

        linkupProcess(sess.Process)
    End Sub

    ''' <summary>
    ''' Handle the controls load event.
    ''' </summary>
    Private Sub ctlSessionManagement_Load(sender As Object, e As EventArgs) Handles Me.Load
        'Update the views even before the refresh timer ticks
        mHasLoaded = True
        mRefreshTimer.Enabled = True

        RefreshEnvironment(False, Nothing)
    End Sub

    ''' <summary>
    ''' Event handler that sets the context menu to nothing, if somewhere else in the
    ''' control room is clicked
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub frmControlRoom_MouseDown(ByVal sender As Object, ByVal e As MouseEventArgs) Handles MyBase.MouseDown
        If e.Button = System.Windows.Forms.MouseButtons.Right Then
            Me.ContextMenu = Nothing
        End If
    End Sub

    ''' <summary>
    ''' Captures the delete key press, which then deletes a pending process.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub lvEnvironment_KeyDown(ByVal sender As System.Object, ByVal e As KeyEventArgs) Handles lvEnvironment.KeyDown
        If e.KeyCode = Keys.Delete Then
            lnkDelete_context(sender, e)
        End If
    End Sub

    ''' <summary>
    ''' Starts all selected processes
    ''' </summary>
    Private Sub StartSelectedProcesses()

        Cursor.Current = Cursors.WaitCursor()
        Try
            If lvEnvironment.SelectedItems.Count = 0 Then
                UserMessage.Show(My.Resources.ctlSessionManagement_NoPendingProcessesHaveBeenSelected)
                Exit Sub
            End If

            Dim selectedItems = lvEnvironment.SelectedItems.Cast(Of ListViewItem)()
            Dim selectedProcesses = selectedItems.Select(Function(item) CType(item.Tag, clsProcessSession))

            Dim nonPendingProcesses = selectedProcesses.Where(Function(f) f.Status <> SessionStatus.Pending)
            If nonPendingProcesses.Any() Then
                nonPendingProcesses.ForEach(Sub(x)
                                                UserMessage.Show(String.Format(
                        My.Resources.ctlSessionManagement_CannotStart0BecauseItIs1,
                        x.ProcessName,
                        SessionStatusLocalisationLookup.Value(x.Status)))
                                            End Sub)
            Else
                DataRefreshInProgress = True
                Task.Run(Sub()
                             mSessionStarter.StartSession(selectedProcesses)
                         End Sub).ContinueWith(Sub()
                                                   If mShowSessionVariables Then
                                                       HandleSessionVariablesUpdated()
                                                       SendGetSessionVariableCommand(True)
                                                   End If
                                               End Sub)
            End If
        Finally
            Cursor.Current = Cursors.Default
        End Try

    End Sub

    Private Sub SessionStarting(s As Object, e As SessionStartingEventArgs) Handles mSessionStarter.SessionStarting
        mSessionsAboutToRun.AddRange(e.Sessions)
    End Sub


    ''' <summary>
    ''' Deletes all pending sessions which have been selected in the environment list.
    ''' </summary>
    Private Sub DeletePendingSessions()
        Dim invalidDelete = False
        Try
            Dim sessions As New List(Of clsProcessSession)
            For Each envItem As ListViewItem In lvEnvironment.SelectedItems
                Dim session = CType(envItem.Tag, clsProcessSession)

                If session.Status <> SessionStatus.Pending Then
                    invalidDelete = True
                    Continue For
                End If
                sessions.Add(session)
            Next

            mSessionDeleter.DeleteSessions(sessions)

        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlSessionManagement_AnErrorOccurredTryingToDeleteAPendingSession, ex.Message), ex)
        Finally
            If invalidDelete Then
                UserMessage.Show(My.Resources.ctlSessionManagement_ItIsOnlyPermissibleToDeletePendingSessions)
            End If
        End Try
    End Sub

    ''' <summary>
    ''' Stops all processes that have been selected
    ''' </summary>
    Private Sub StopSelectedProcesses()
        If lvEnvironment.SelectedItems.Count = 0 Then
            UserMessage.Show(My.Resources.ctlSessionManagement_NoProcessesWereSelected)
            Exit Sub
        End If

        Dim Item As ListViewItem
        Dim sErr As String = Nothing

        Dim sessions As New List(Of clsProcessSession)
        For Each Item In lvEnvironment.SelectedItems
            ' Get the object used to start the session
            Dim objSession As clsProcessSession = CType(Item.Tag, clsProcessSession)
            If Not objSession.Status = SessionStatus.Pending Then
                sessions.Add(objSession)
            Else
                UserMessage.Show(My.Resources.ctlSessionManagement_CannotStopAPendingProcessDeleteItInstead)
            End If
        Next

        If sessions.Any() Then
            Try
                DataRefreshInProgress = True
                mSessionStopper.StopProcesses(sessions)
            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.ctlSessionManagement_FailedToStopProcess0, ex.Message))
            End Try
        End If
    End Sub

    Private Sub CreateSessions(processes As ICollection(Of IGroupMember), resources As ICollection(Of IGroupMember))
        If processes Is Nothing OrElse processes.Count = 0 Then Exit Sub
        If resources Is Nothing OrElse resources.Count = 0 Then Exit Sub

        Dim totalcount As Integer = (resources.Count * processes.Count)

        If Not gSv.CanCreateSessions(totalcount) Then
            clsUserInterfaceUtils.ShowOperationDisallowedMessage(
                Licensing.SessionLimitReachedMessage)
            Return
        End If

        DataRefreshInProgress = True
        Task.Run(Sub()
                     mSessionCreator.CreateSessions(processes, resources)
                 End Sub)
    End Sub

    Private Sub CallbackChannelError(e As FailedCallbackOperationEventArgs)
        UserMessage.Show(e.Message)
    End Sub


    Private Sub QueueRefresh()
        mRefreshQueued = True
        DataRefreshInProgress = False
    End Sub

    ''' <summary>
    ''' Event handler for the listviews column resize event.
    ''' </summary>
    ''' <param name="sender"></param>
    Private Sub lvEnvironment_ColumnsResized(ByVal sender As System.Object) Handles lvEnvironment.ColumnsResized
        RefreshDropDowns()
    End Sub

    ''' <summary>
    ''' Handles the list view being resized.
    ''' </summary>
    ''' <param name="sender">The source of the event.</param>
    ''' <param name="e">The arguments detailing the event.</param>
    Private Sub HandleListViewResize(ByVal sender As Object, ByVal e As EventArgs) Handles lvEnvironment.Resize
        ' Width of -2 indicates at least largest of header / data width and filling
        ' up the remainder of the listview if there is any more space.
        If lvEnvironment.Columns.Count > SessionManagementColumn.EndTime Then _
         lvEnvironment.Columns(SessionManagementColumn.LastUpdated).Width = -2
    End Sub

    ''' <summary>
    ''' This simple function just draws a line above the environment view, this
    ''' graphically indicates to the user that the environment view can be resized.
    ''' </summary>
    Private Sub pnlEnvironment_Paint(ByVal sender As Object, ByVal e As PaintEventArgs) Handles pnlEnvironment.Paint
        e.Graphics.DrawLine(SystemPens.FromSystemColor(SystemColors.ActiveBorder), 0, 0, pnlEnvironment.Width - 1, 0)
    End Sub

    ''' <summary>
    ''' Handles the refresh timer.
    ''' </summary>
    Private Sub RefreshTimer_Tick(ByVal sender As Object, ByVal e As EventArgs) _
     Handles mRefreshTimer.Tick
        RefreshPendingViews()
    End Sub

    Private Sub ResourceViewerRequestRefresh(ByVal sender As Object, ByVal e As EventArgs) Handles mResourceViewer.RequestRefresh
        mTimer.Start()
    End Sub

    ''' <summary>
    ''' Create a timer for the refresh
    ''' </summary>
    ''' <param name="interval"></param>
    Private Sub CreateTimer(interval As Double)
        mTimer = New Timers.Timer(interval)
        AddHandler mTimer.Elapsed, AddressOf OnTimedEvent
        mTimer.AutoReset = False
        mTimer.Enabled = False
    End Sub

    ''' <summary>
    ''' Timer has expired and not been reset.
    ''' </summary>
    ''' <param name="source"></param>
    ''' <param name="e"></param>
    Private Sub OnTimedEvent(source As Object, e As ElapsedEventArgs)
        'set the system to update
        mResourceUpdateQueued = True
    End Sub

    Private Function GetEnvironmentProcessSession() As IEnumerable(Of clsProcessSession)
        Dim list As List(Of clsProcessSession) = New List(Of clsProcessSession)
        For Each li As ListViewItem In lvEnvironment.SelectedItems
            Dim sess As clsProcessSession = TryCast(li.Tag, clsProcessSession)
            list.Add(sess)
        Next
        Return list
    End Function


    Private Sub SendGetSessionVariables(forceUpdate As Boolean)
        For Each sess As clsProcessSession In GetEnvironmentProcessSession()
            If sess.Status = SessionStatus.Running OrElse forceUpdate Then
                SendGetSessionVariablesCommand(sess)
            End If
        Next
    End Sub

    ''' <summary>
    ''' Refresh (non-destructively) the session variables ListView. Call only on the
    ''' GUI thread.
    ''' </summary>
    Private Sub RefreshSessionVariables()

        'Don't bother to refresh when the session variables are not visible
        If Not mShowSessionVariables Then Return

        'Get a list of sessions that are selected. At the same time, make it easy to
        'map between the ID and the Identifier..
        Dim selectedSessions As New List(Of Guid)
        Dim ids As New Dictionary(Of Guid, Integer)
        For Each sess As clsProcessSession In GetEnvironmentProcessSession()
            Debug.Assert(sess IsNot Nothing)
            selectedSessions.Add(sess.SessionID)
            ids(sess.SessionID) = sess.SessionNum
        Next

        'Get a list of just the session variables we're going to display, by matching
        'the list of all known session variables against the selected sessions...
        Dim vars As New List(Of clsSessionVariable)
        For Each var As clsSessionVariable In mConnManager.SessionVariables.Values
            If selectedSessions.Contains(var.SessionID) Then
                var.SessionIdentifier = ids(var.SessionID).ToString()
                vars.Add(var)
            End If
        Next

        Dim sortRequired As Boolean = False

        'Make a list of items we're going to delete from the ListView. Initially, all of them!
        'We'll take them out of here as we go along...
        Dim deleteThese As New List(Of ListViewItem)
        For Each li As ListViewItem In lvSessionVariables.Items
            deleteThese.Add(li)
        Next

        'Update or add items to the ListView as appropriate
        For Each var As clsSessionVariable In vars
            Dim found As Boolean = False
            For Each li As ListViewItem In lvSessionVariables.Items
                If CType(li.Tag, clsSessionVariable).SessionID = var.SessionID AndAlso CType(li.Tag, clsSessionVariable).Name = var.Name Then
                    'Already exists, just update the value...
                    If li.SubItems(3).Text <> var.Value.FormattedValue Then
                        li.SubItems(3).Text = var.Value.FormattedValue
                        'Sort only needed if sorting on this column...
                        If mVarSorter.SortColumn = 3 Then sortRequired = True
                    End If
                    'Also update the tag so we have an up to date value there...
                    li.Tag = var
                    deleteThese.Remove(li)
                    found = True
                End If
            Next
            If Not found Then
                'New item...
                Dim lv As New ListViewItem(var.SessionIdentifier)
                lv.SubItems.Add(var.Name)
                lv.SubItems.Add(clsProcessDataTypes.GetFriendlyName(var.Value.DataType))
                lv.SubItems.Add(var.Value.FormattedValue)
                'Only want the first line of the description...
                Dim desc As String = var.Value.Description
                Dim index As Integer = desc.IndexOf(vbCr)
                If index <> -1 Then desc = desc.Substring(0, index)
                lv.SubItems.Add(desc)
                lvSessionVariables.Items.Add(lv)
                lv.Tag = var
                sortRequired = True
            End If
        Next

        'Remove items from the ListView that shouldn't be there any more...
        For Each li As ListViewItem In deleteThese
            lvSessionVariables.Items.Remove(li)
        Next

        'Re-sort if anything's changed...
        If sortRequired Then
            lvSessionVariables.ListViewItemSorter = mVarSorter
            lvSessionVariables.Sort()
        End If

    End Sub

    Private Sub SendGetSessionVariablesCommand(session As clsProcessSession)
        Dim err As String = String.Empty
        If session.SessionID <> Guid.Empty Then
            Dim resid As Guid
            Dim sErr As String = String.Empty
            If Not session.GetTargetResourceID(resid, sErr) Then
                UserMessage.Show(sErr)
                Exit Sub
            End If
            mConnManager.SendGetSessionVariables(resid, session.SessionID, session.ProcessID, err)
        End If
    End Sub

    ''' <summary>
    ''' Raises the <see cref="SaveViewClick"/> event
    ''' </summary>
    ''' <param name="e">The event args detailing the save view click event</param>
    Protected Overridable Sub OnSaveViewClick(e As EventArgs)
        RaiseEvent SaveViewClick(Me, e)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="ViewStateChanged"/> event
    ''' </summary>
    ''' <param name="e">The event args detailing the event</param>
    Protected Overridable Sub OnViewStateChanged(e As ViewStateChangedEventArgs)
        RaiseEvent ViewStateChanged(Me, e)
    End Sub

    Private Sub HandleSessionVariablesUpdated() Handles mConnManager.SessionVariablesUpdated
        SyncLock mSessionVariablesUpdatedLock
            mSessionVariablesUpdated = True
        End SyncLock
    End Sub

    Private Sub UpdateSessionVariablesLayout()
        pnlBottomHalf.Panel2Collapsed = Not mShowSessionVariables
        If mShowSessionVariables Then
            pnlBottomHalf.SplitterDistance = pnlBottomHalf.Height \ 2
            llSessionVariables.Text = My.Resources.ctlSessionManagement_HideSessionVariables
        Else
            pnlBottomHalf.SplitterDistance = pnlBottomHalf.Height - 25
            llSessionVariables.Text = My.Resources.ctlSessionManagement_ShowSessionVariables
        End If
    End Sub

    Private Sub ToggleSessionVariables() Handles llSessionVariables.LinkClicked
        mShowSessionVariables = Not mShowSessionVariables
        pnlBottomHalf.Panel2Collapsed = Not mShowSessionVariables
        mConnManager.MonitorSessionVariables = mShowSessionVariables
        mConnManager.ToggleShowSessionVariables(mShowSessionVariables)
        UpdateSessionVariablesLayout()
        HandleSessionVariablesUpdated()
        SendGetSessionVariableCommand()
    End Sub

    Private Sub pnlBottomHalf_SplitterMoving(ByVal sender As Object, ByVal e As SplitterCancelEventArgs) Handles pnlBottomHalf.SplitterMoving
        If Not mShowSessionVariables Then e.Cancel = True
    End Sub

    Private Sub lvEnvironment_ItemSelectionChanged() Handles lvEnvironment.ItemSelectionChanged
        If mShowSessionVariables Then RefreshSessionVariables()
    End Sub

    ''' <summary>
    ''' Handles the 'Save View' button being clicked. This just passes on the message
    ''' to any objects listening to the <see cref="SaveViewClick"/> event
    ''' </summary>
    Private Sub HandleSaveButtonClick() Handles btnSaveView.Click
        OnSaveViewClick(EventArgs.Empty)
    End Sub

    ''' <summary>
    ''' Handle when a resource is dropped onto an process
    ''' </summary>
    ''' <param name="sender">the object that raised the event</param>
    ''' <param name="e">Data about the item being dragged</param>
    Private Sub tvProcesses_GroupMemberDropped(sender As Object, e As GroupMemberDropEventArgs) Handles tvProcesses.GroupMemberDropped
        If CanControlResource() Then
            Dim tgt = TryCast(e.Target, ProcessGroupMember)
            Dim cts = TryCast(e.Contents, ICollection(Of IGroupMember))

            If Not tgt.Permissions.HasPermission(User.Current, Permission.ProcessStudio.ImpliedExecuteProcess) OrElse
              cts.OfType(Of ResourceGroupMember).Any(Function(x) Not CanControlResource(x.Permissions, False)) Then
                UserMessage.ShowCantExecuteProcessMessage()
            Else
                CreateSessions(GetSingleton.ICollection(Of IGroupMember)(tgt), cts)
            End If
        End If
    End Sub

    ''' <summary>
    ''' Handle when a process is dropped onto a resource
    ''' </summary>
    ''' <param name="sender">the object that raised the event</param>
    ''' <param name="e">Data about the item being dragged</param>
    Private Sub mResourceViewer_GroupMemberDropped(sender As Object, e As GroupMemberDropEventArgs) Handles mResourceViewer.GroupMemberDropped
        Dim tgt = TryCast(e.Target, ResourceGroupMember)
        If CanControlResource(tgt?.Permissions) Then
            Dim cts = TryCast(e.Contents, ICollection(Of IGroupMember))

            If cts.ToList().Exists(Function(x) Not x.Permissions.HasPermission(User.Current, Permission.ProcessStudio.ImpliedExecuteProcess)) Then
                UserMessage.ShowCantExecuteProcessMessage()
            Else
                CreateSessions(cts, GetSingleton.ICollection(Of IGroupMember)(tgt))
            End If
        End If
    End Sub

    ''' <summary>
    ''' When a resource is selected, sets the session filters to the selected
    ''' resource. (If the 'Filter using selected resource' checkbox is checked)
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub mResourceViewer_SelectedIndexChanged(sender As Object, e As EventArgs) Handles mResourceViewer.SelectedIndexChanged
        If (mResourceViewer.SelectedIndices.Count = 0) Then Return
        If cbFilterUsingSelectedResource.Checked Then FilterSessionsBySelection(
            DirectCast(GetFilterComboBox(ColumnNames.Resource), GroupMemberComboBox),
            mResourceViewer.GetSelectedResources())
    End Sub

    ''' <summary>
    ''' Handle the selection changed event and trigger a refresh of the sessions if
    ''' filtering is enabled.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub tvProcesses_SelectedIndexChanged(sender As Object, e As EventArgs) Handles tvProcesses.SelectedIndexChanged
        If (tvProcesses.SelectedIndices.Count = 0) Then Return
        If cbFilterUsingSelectedProcess.Checked Then FilterSessionsBySelection(
            DirectCast(GetFilterComboBox(ColumnNames.Process), GroupMemberComboBox),
            tvProcesses.GetSelected())
    End Sub

    ''' <summary>
    ''' Filter the session list by selected processes and resources.
    ''' </summary>
    Private Sub FilterSessionsBySelection(comboBox As GroupMemberComboBox, selections As List(Of String))
        Dim modified = False

        ' Update the filter combo with the latest selection

        If selections IsNot Nothing AndAlso selections.Count > 0 Then
            comboBox.ClearCheckedItems()

            For Each r In selections
                comboBox.CheckItem(r)
            Next
            modified = True
        End If

        ' Trigger a refresh if we changed anything
        If modified AndAlso
            Not mSuspendFilterUpdates AndAlso
            Not IsDroppedDown(comboBox) Then
            RefreshEnvironment(False, Nothing)
        End If
    End Sub

    ''' <summary>
    ''' Sets the session filter to filter by all resources.
    ''' </summary>
    Private Sub FilterSessionsByAll(comboBox As GroupMemberComboBox)

        comboBox.ClearCheckedItems()
        comboBox.CheckItem(My.Resources.ctlSessionManagement_All)
        If Not mSuspendFilterUpdates AndAlso Not IsDroppedDown(comboBox) Then RefreshEnvironment(False, Nothing)

    End Sub

    ''' <summary>
    ''' Changes the session filters when the 'filter using resource' checkbox is toggled.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub cbFilterUsingSelectedResource_CheckedChanged(sender As Object, e As EventArgs) Handles cbFilterUsingSelectedResource.CheckedChanged

        Dim comboBox =
            DirectCast(GetFilterComboBox(ColumnNames.Resource), GroupMemberComboBox)

        If cbFilterUsingSelectedResource.Checked Then
            FilterSessionsBySelection(comboBox, mResourceViewer.GetSelectedResources())
        Else
            FilterSessionsByAll(comboBox)
        End If


    End Sub

    ''' <summary>
    ''' Enable filtering of the sessions list by selected processes
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub cbFilterUsingSelectedProcess_CheckedChanged(sender As Object, e As EventArgs) Handles cbFilterUsingSelectedProcess.CheckedChanged

        Dim comboBox =
            DirectCast(GetFilterComboBox(ColumnNames.Process), GroupMemberComboBox)

        If cbFilterUsingSelectedProcess.Checked Then
            FilterSessionsBySelection(comboBox, tvProcesses.GetSelected())
        Else
            FilterSessionsByAll(comboBox)
        End If

    End Sub

    ''' <summary>
    ''' Returns the ComboBox control for the session filter
    ''' </summary>
    ''' <param name="columnName">The column name.</param>
    ''' <returns></returns>
    Private Function GetFilterComboBox(columnName As String) As ComboBox
        For Each ctl As Control In pnlEnvironment.Controls
            If ctl.Name = columnName Then
                Return CType(ctl, ComboBox)
            End If
        Next
        Return Nothing
    End Function

    Private Sub PopulateSessionRowLimitDropDown()
        cmbSessionRowLimit.Items.Clear()
        cmbSessionRowLimit.Items.AddRange(mSessionRowLimitComboBoxValues)

        RaiseEvent SessionRowLimitUpdated(Me, New SessionRowLimitChangedEventArgs(DefaultSessionRowLimit, refreshRequired:=True))
    End Sub

    Private Sub HandleSessionRowLimitSelectedValueChanged(sender As Object, e As EventArgs) Handles cmbSessionRowLimit.SelectedValueChanged
        Dim newSessionRowLimit = CType(cmbSessionRowLimit.SelectedItem, Integer?)

        If Not newSessionRowLimit.HasValue Then Return
        If mSessionRowLimit = newSessionRowLimit Then Return

        RaiseEvent SessionRowLimitUpdated(Me, New SessionRowLimitChangedEventArgs(newSessionRowLimit.Value, True))
    End Sub

    Private Sub HandlesSessionRowLimitUpdated(sender As Object, e As SessionRowLimitChangedEventArgs) Handles Me.SessionRowLimitUpdated
        Dim refreshList = e.RefreshRequired
        InvokeUpdateSessionRowLimit(e.RowLimit, refreshList)
    End Sub

    Private Sub InvokeUpdateSessionRowLimit(rowLimit As Integer, requiresRefresh As Boolean)
        If InvokeRequired Then
            Invoke(New ThreadSafeUpdateSessionRowLimit(AddressOf UpdateSessionRowLimit), rowLimit, requiresRefresh)
        Else
            UpdateSessionRowLimit(rowLimit, requiresRefresh)
        End If
    End Sub

    Private Sub UpdateSessionRowLimit(rowLimit As Integer, requiresRefresh As Boolean)
        SyncLock mSessionRowLimitLock
            mSessionRowLimit = rowLimit

            RemoveHandler cmbSessionRowLimit.SelectedValueChanged, AddressOf HandleSessionRowLimitSelectedValueChanged
            cmbSessionRowLimit.SelectedItem = rowLimit
            AddHandler cmbSessionRowLimit.SelectedValueChanged, AddressOf HandleSessionRowLimitSelectedValueChanged

        End SyncLock

        If requiresRefresh Then RefreshEnvironment(True, Nothing)

    End Sub

    Private Sub ShowCallbackUserMessage(sender As Object, ByVal e As String) Handles mConnManager.ShowUserMessage
        ShowMessage(e)
    End Sub

    Private Delegate Sub SessionRowLimitChangedEventHandler(sender As Object, e As SessionRowLimitChangedEventArgs)

    Private Class SessionRowLimitChangedEventArgs : Inherits EventArgs
        Public ReadOnly Property RowLimit As Integer
        Public ReadOnly Property RefreshRequired As Boolean
        Public Sub New(rowLimit As Integer, refreshRequired As Boolean)
            Me.RowLimit = rowLimit
            Me.RefreshRequired = refreshRequired
        End Sub
    End Class

    Private Delegate Sub ThreadSafeUpdateSessionRowLimit(rowLimit As Integer, requiresRefresh As Boolean)

    Private Sub SendGetSessionVariableCommand(Optional forceUpdate As Boolean = False)
        If mShowSessionVariables Then
            SendGetSessionVariables(forceUpdate)
        End If
    End Sub

    Private Sub Environment_MouseClick(sender As Object, e As MouseEventArgs) Handles lvEnvironment.MouseClick
        SendGetSessionVariableCommand()
    End Sub

    ''' <summary>
    ''' Handle the right click refresh
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Public Sub RefreshValue_context(ByVal sender As Object, ByVal e As EventArgs)
        SendGetSessionVariableCommand()
    End Sub

    Private Sub SearchBar_TextChanged(sender As Object, e As EventArgs) Handles FilterTextAndDropdown.TextChanged

        If Not String.IsNullOrWhiteSpace(FilterTextAndDropdown.FilterText) Then
            Dim autos As List(Of String) = mResourceViewer.FitlerResources(FilterTextAndDropdown.FilterText)

            Dim str As String = FilterTextAndDropdown.FilterText

            If autos.Any() Then
                FilterTextAndDropdown.Items.Clear()
                FilterTextAndDropdown.Items.AddRange(autos.ToArray())
                FilterTextAndDropdown.IsDroppedDown = True
            Else
                FilterTextAndDropdown.IsDroppedDown = False
            End If
        Else
            FilterTextAndDropdown.Items.Clear()
            FilterTextAndDropdown.IsDroppedDown = False
            mResourceViewer.ResetSearch()
        End If
    End Sub

    Public Sub ToggleDisplayResources()
        Dim connectionManager = TryCast(mConnManager, PersistentConnectionManager)
        If connectionManager IsNot Nothing Then
            connectionManager.ToggleDisableConnection()
        End If
        Refresh()
    End Sub
End Class
