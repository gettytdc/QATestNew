Imports AutomateControls
Imports AutomateControls.Forms
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.AutomateProcessCore.Stages
Imports LocaleTools

Public Class ctlExceptionTypes : Implements IHelp, IChild, IMode, IPermission

    Private mMode As ProcessType
    Private mSorter As clsListViewSorter

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        mSorter = New clsListViewSorter(lvExceptionTypes)
        mSorter.ColumnDataTypes = New System.Type() {GetType(String), GetType(String)}
        mSorter.SortColumn = 0
        mSorter.Order = SortOrder.Ascending
        lvExceptionTypes.ListViewItemSorter = mSorter
    End Sub

    Public Property Mode() As ProcessType Implements IMode.Mode
        Get
            Return mMode
        End Get
        Set(ByVal value As ProcessType)
            mMode = value
            If Mode.IsProcess Then
                SetProcessResources()
            Else
                SetObjectResources()
            End If
        End Set
    End Property

    ''' <summary>
    ''' BackgroundWorker class which scans the processes to see which exception
    ''' types occur in which processes.
    ''' Inside a background worker because it can take quite a long time depending
    ''' on how many and how large the processes in the database are.
    ''' </summary>
    Private Class ProcessScanner
        Inherits BackgroundWorker

        ''' <summary>
        ''' The outer instance which created this scanner
        ''' </summary>
        Private Outer As ctlExceptionTypes

        ''' <summary>
        ''' The exception map mapping a compound key of process ID and exception
        ''' type to the ExceptionProcessMapping object.
        ''' </summary>
        Public ExceptionMap As _
         clsGeneratorDictionary(Of String, clsCounterMap(Of String))

        ''' <summary>
        ''' Creates a new ProcessScanner from the given ctlDataBase instance
        ''' </summary>
        ''' <param name="outerCtl">The instance of ctlDatabase which 'owns' this
        ''' instance of the scanner.</param>
        Public Sub New(ByVal outerCtl As ctlExceptionTypes)
            WorkerReportsProgress = True
            WorkerSupportsCancellation = True
            Outer = outerCtl
        End Sub

        ''' <summary>
        ''' Performs the work of scanning the processes. Most of the actual
        ''' work is delegated to the ScanProcess() method in the outer
        ''' class, but this reports the progress to any active listeners.
        ''' </summary>
        ''' <param name="args">The work event arguments</param>
        Protected Overrides Sub OnDoWork(ByVal args As DoWorkEventArgs)
            Dim map As _
             New clsGeneratorDictionary(Of String, clsCounterMap(Of String))(
                 StringComparer.CurrentCultureIgnoreCase)

            For Each dr As DataRow In gSv.GetProcesses(False).Rows
                Outer.ScanProcess(CType(dr!processid, Guid), map)
            Next

            Dim dt As DataTable = gSv.GetProcesses(True)
            Dim max As Double = CDbl(dt.Rows.Count)
            Dim curr As Double = 0.0

            For Each dr As DataRow In dt.Rows
                If Me.CancellationPending Then
                    args.Cancel = True
                    Exit For
                End If
                Outer.ScanProcess(CType(dr!processid, Guid), map)
                curr += 1.0
                ' Math.Floor() rather than round - over the max causes an
                ' exception, under the max on completion does not.
                Me.ReportProgress(CInt(Math.Floor(100.0 * (curr / max))))
            Next

            Me.ExceptionMap = map

        End Sub

    End Class

    ''' <summary>
    ''' Populates the Exception Types List
    ''' </summary>
    Private Sub PopulateExceptionList()
        Try
            Dim exceptionTypes = gSv.GetExceptionTypes()
            lvExceptionTypes.Items.Clear()
            For Each exceptionType As String In exceptionTypes
                lvExceptionTypes.Items.Add(exceptionType, exceptionType, 0)
            Next
        Catch ex As Exception
            UserMessage.ShowExceptionMessage(ex)
        End Try
    End Sub

    ''' <summary>
    ''' Handles the event handler for the Scan Processes Link
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub llScanProcesses_LinkClicked(ByVal sender As Object, _
     ByVal e As LinkLabelLinkClickedEventArgs) Handles llScanProcesses.LinkClicked
        ScanProcesses()
    End Sub

    ''' <summary>
    ''' Updates the given list view item with details about the specified exception
    ''' type and process/count map.
    ''' </summary>
    ''' <param name="item">The list view item to update</param>
    ''' <param name="extype">The exception type name</param>
    ''' <param name="counter">The counter of occurrences in each process (keyed on
    ''' process name)</param>
    Private Sub UpdateExceptionTypeItem(ByVal item As ListViewItem, _
     ByVal extype As String, ByVal counter As clsCounterMap(Of String))
        item.ImageIndex = 1
        item.Tag = True

        For Each proc As String In counter.Keys
            ' 'proc' is "{procid}|{procname}" hence the split
            Dim txt As String = _
             proc.Split("|"c)(1) & "(" & counter(proc) & ")"

            If item.SubItems.Count > 1 Then
                item.SubItems(1).Text &= ", " & txt
            Else
                item.SubItems.Add(txt)
            End If
        Next

    End Sub


    ''' <summary>
    ''' Scans the processes and objects and determines which exception types are
    ''' being used.
    ''' </summary>
    Private Sub ScanProcesses()
        PopulateExceptionList()
        ' Populate the exception map asynchronously using a background worker
        Dim scanner As New ProcessScanner(Me)
        If Not ProgressDialog.Show(Me, scanner, My.Resources.ctlExceptionTypes_ScanningProcesses).Cancelled Then

            Dim map As clsGeneratorDictionary(Of String, clsCounterMap(Of String)) =
            scanner.ExceptionMap

            For Each item As ListViewItem In lvExceptionTypes.Items

                ' Each item represents an exception type... the text is the
                ' text, since the map is keyed primarily on exception type...
                If map.ContainsKey(item.Text) Then
                    UpdateExceptionTypeItem(item, item.Text, map(item.Text))

                    ' Remove it from the map so we can identify exception types which
                    ' are not registered in the database but exist within the process
                    map.Remove(item.Text)

                Else
                    item.ImageIndex = 2
                    item.Tag = False
                    If item.SubItems.Count > 1 Then
                        item.SubItems(0).Text = My.Resources.ctlExceptionTypes_None
                    Else
                        item.SubItems.Add(My.Resources.ctlExceptionTypes_None)
                    End If

                End If
            Next

            ' If there are any left in the map, add them into the database and
            ' then into the ListView
            For Each etype As String In map.Keys
                gSv.AddExceptionType(etype)
                UpdateExceptionTypeItem(
                 lvExceptionTypes.Items.Add(etype), etype, map(etype))
            Next

        End If
    End Sub

    ''' <summary>
    ''' Scans a single process, storing any mappings into the given dictionary
    ''' </summary>
    ''' <param name="procID">The process ID</param>
    ''' <param name="mappings">The mapping of exceptions to processes is stored in
    ''' this list. The key is the exception type, and the key of the counter map
    ''' is the process, in the form "{process id|process name}"</param>
    Private Sub ScanProcess(
     ByVal procID As Guid,
     ByVal mappings As clsGeneratorDictionary(Of String, clsCounterMap(Of String)))
        Try
            Dim xml As String
            Try
                xml = gSv.GetProcessXML(procID)
            Catch ex As Exception
                Return
            End Try

            Using proc As clsProcess = clsProcess.FromXml(
             Options.Instance.GetExternalObjectsInfo(), xml, False, True)
                For Each stg As clsExceptionStage In proc.GetStages(StageTypes.Exception)
                    ' Mappings are on : {ExceptionType}{Process}{Count}
                    Dim key As String = procID.ToString() & "|" & proc.Name
                    If stg.ExceptionType IsNot Nothing Then
                        mappings(stg.ExceptionType.Trim())(key) += 1
                    End If
                Next
            End Using
        Catch ' Ignore errors - we're only scanning for exception types, it's not fatal
        End Try
    End Sub

    ''' <summary>
    ''' Event handler for the delete exception type link
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub llDeleteExceptionType_LinkClicked(ByVal sender As Object,
     ByVal e As LinkLabelLinkClickedEventArgs) Handles llDeleteExceptionType.LinkClicked

        Dim removed As New List(Of ListViewItem)
        For Each item As ListViewItem In lvExceptionTypes.SelectedItems
            If CBool(item.Tag) Then
                UserMessage.Show(My.Resources.ctlExceptionTypes_SelectionContainsExceptionsThatAreBeingUsed)
                Exit Sub
            Else
                removed.Add(item)
            End If
        Next

        For Each item As ListViewItem In removed
            If gSv.DeleteExceptionType(item.Text) Then _
             lvExceptionTypes.Items.Remove(item)
        Next
    End Sub

    Private mParent As frmApplication
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property

    Private Sub ctlExceptionTypes_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        mMode.ChangeText(Me)

        PopulateExceptionList()
    End Sub

    Public ReadOnly Property RequiredPermissions() As System.Collections.Generic.ICollection(Of BluePrism.AutomateAppCore.Auth.Permission) Implements BluePrism.AutomateAppCore.Auth.IPermission.RequiredPermissions
        Get
            Return Permission.ByName(mMode.GetPermissionString("Exception Types"))
        End Get
    End Property

    Private Sub SetProcessResources()
        llScanProcesses.Text = My.Resources.ctlExceptionTypes_ScanP
        ColumnHeader14.Text = My.Resources.frmWizard_ProcessesUC
    End Sub

    Private Sub SetObjectResources()
        llScanProcesses.Text = My.Resources.ctlExceptionTypes_ScanBO
        ColumnHeader14.Text = My.Resources.ProcessType_BusinessObjectsC
    End Sub

    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "helpExceptions.htm"
    End Function
End Class
