Option Strict On
Imports System.IO
Imports System.Threading
Imports System.Collections.Generic

Imports BluePrism.BPCoreLib
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities

Public Class frmAdvanced
    Inherits System.Windows.Forms.Form

#Region " Windows Form Designer generated code "

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents btnLaunch As System.Windows.Forms.Button
    Friend WithEvents treeModel As System.Windows.Forms.TreeView
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents label999 As System.Windows.Forms.Label
    Friend WithEvents txtQuery As System.Windows.Forms.TextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents txtReply As System.Windows.Forms.TextBox
    Friend WithEvents btnSendQuery As System.Windows.Forms.Button
    Friend WithEvents PropertyGrid1 As System.Windows.Forms.PropertyGrid
    Friend WithEvents cmbCommandLine As System.Windows.Forms.ComboBox
    Friend WithEvents btnSpyWin As System.Windows.Forms.Button
    Friend WithEvents chkLogToWindow As System.Windows.Forms.CheckBox
    Friend WithEvents chkUseHooks As System.Windows.Forms.CheckBox
    Friend WithEvents chkJAB As System.Windows.Forms.CheckBox
    Friend WithEvents btnScript As System.Windows.Forms.Button
    Friend WithEvents dlgOpenScript As System.Windows.Forms.OpenFileDialog
    Friend WithEvents lblModel As System.Windows.Forms.Label
    Friend WithEvents chkFreezeModel As System.Windows.Forms.CheckBox
    Friend WithEvents txtQueryInfo As System.Windows.Forms.TextBox
    Friend WithEvents cmbMode As System.Windows.Forms.ComboBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents txtLogWindow As System.Windows.Forms.RichTextBox
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmAdvanced))
        Me.btnLaunch = New System.Windows.Forms.Button()
        Me.treeModel = New System.Windows.Forms.TreeView()
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.label999 = New System.Windows.Forms.Label()
        Me.txtQuery = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.txtReply = New System.Windows.Forms.TextBox()
        Me.btnSendQuery = New System.Windows.Forms.Button()
        Me.PropertyGrid1 = New System.Windows.Forms.PropertyGrid()
        Me.cmbCommandLine = New System.Windows.Forms.ComboBox()
        Me.btnSpyWin = New System.Windows.Forms.Button()
        Me.chkLogToWindow = New System.Windows.Forms.CheckBox()
        Me.txtLogWindow = New System.Windows.Forms.RichTextBox()
        Me.chkUseHooks = New System.Windows.Forms.CheckBox()
        Me.chkJAB = New System.Windows.Forms.CheckBox()
        Me.btnScript = New System.Windows.Forms.Button()
        Me.dlgOpenScript = New System.Windows.Forms.OpenFileDialog()
        Me.lblModel = New System.Windows.Forms.Label()
        Me.chkFreezeModel = New System.Windows.Forms.CheckBox()
        Me.txtQueryInfo = New System.Windows.Forms.TextBox()
        Me.cmbMode = New System.Windows.Forms.ComboBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'btnLaunch
        '
        resources.ApplyResources(Me.btnLaunch, "btnLaunch")
        Me.btnLaunch.Name = "btnLaunch"
        '
        'treeModel
        '
        resources.ApplyResources(Me.treeModel, "treeModel")
        Me.treeModel.Name = "treeModel"
        '
        'Timer1
        '
        Me.Timer1.Enabled = True
        Me.Timer1.Interval = 1000
        '
        'label999
        '
        resources.ApplyResources(Me.label999, "label999")
        Me.label999.Name = "label999"
        '
        'txtQuery
        '
        resources.ApplyResources(Me.txtQuery, "txtQuery")
        Me.txtQuery.Name = "txtQuery"
        '
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.Name = "Label3"
        '
        'Label4
        '
        resources.ApplyResources(Me.Label4, "Label4")
        Me.Label4.Name = "Label4"
        '
        'txtReply
        '
        resources.ApplyResources(Me.txtReply, "txtReply")
        Me.txtReply.Name = "txtReply"
        '
        'btnSendQuery
        '
        resources.ApplyResources(Me.btnSendQuery, "btnSendQuery")
        Me.btnSendQuery.Name = "btnSendQuery"
        '
        'PropertyGrid1
        '
        resources.ApplyResources(Me.PropertyGrid1, "PropertyGrid1")
        Me.PropertyGrid1.LineColor = System.Drawing.SystemColors.ScrollBar
        Me.PropertyGrid1.Name = "PropertyGrid1"
        Me.PropertyGrid1.ToolbarVisible = False
        '
        'cmbCommandLine
        '
        resources.ApplyResources(Me.cmbCommandLine, "cmbCommandLine")
        Me.cmbCommandLine.Name = "cmbCommandLine"
        '
        'btnSpyWin
        '
        resources.ApplyResources(Me.btnSpyWin, "btnSpyWin")
        Me.btnSpyWin.Name = "btnSpyWin"
        '
        'chkLogToWindow
        '
        resources.ApplyResources(Me.chkLogToWindow, "chkLogToWindow")
        Me.chkLogToWindow.Checked = True
        Me.chkLogToWindow.CheckState = System.Windows.Forms.CheckState.Checked
        Me.chkLogToWindow.Name = "chkLogToWindow"
        '
        'txtLogWindow
        '
        resources.ApplyResources(Me.txtLogWindow, "txtLogWindow")
        Me.txtLogWindow.Name = "txtLogWindow"
        '
        'chkUseHooks
        '
        resources.ApplyResources(Me.chkUseHooks, "chkUseHooks")
        Me.chkUseHooks.Name = "chkUseHooks"
        Me.chkUseHooks.UseVisualStyleBackColor = True
        '
        'chkJAB
        '
        resources.ApplyResources(Me.chkJAB, "chkJAB")
        Me.chkJAB.Name = "chkJAB"
        Me.chkJAB.UseVisualStyleBackColor = True
        '
        'btnScript
        '
        resources.ApplyResources(Me.btnScript, "btnScript")
        Me.btnScript.Name = "btnScript"
        Me.btnScript.UseVisualStyleBackColor = True
        '
        'dlgOpenScript
        '
        Me.dlgOpenScript.InitialDirectory = "c:\BluePrism\ApplicationManager\UnitTest"
        resources.ApplyResources(Me.dlgOpenScript, "dlgOpenScript")
        '
        'lblModel
        '
        resources.ApplyResources(Me.lblModel, "lblModel")
        Me.lblModel.Name = "lblModel"
        '
        'chkFreezeModel
        '
        resources.ApplyResources(Me.chkFreezeModel, "chkFreezeModel")
        Me.chkFreezeModel.Name = "chkFreezeModel"
        Me.chkFreezeModel.UseVisualStyleBackColor = True
        '
        'txtQueryInfo
        '
        resources.ApplyResources(Me.txtQueryInfo, "txtQueryInfo")
        Me.txtQueryInfo.Name = "txtQueryInfo"
        Me.txtQueryInfo.ReadOnly = True
        '
        'cmbMode
        '
        Me.cmbMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbMode.FormattingEnabled = True
        Me.cmbMode.Items.AddRange(New Object() {resources.GetString("cmbMode.Items"), resources.GetString("cmbMode.Items1"), resources.GetString("cmbMode.Items2"), resources.GetString("cmbMode.Items3"), resources.GetString("cmbMode.Items4")})
        resources.ApplyResources(Me.cmbMode, "cmbMode")
        Me.cmbMode.Name = "cmbMode"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'frmAdvanced
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.cmbMode)
        Me.Controls.Add(Me.txtQueryInfo)
        Me.Controls.Add(Me.chkFreezeModel)
        Me.Controls.Add(Me.lblModel)
        Me.Controls.Add(Me.btnScript)
        Me.Controls.Add(Me.chkJAB)
        Me.Controls.Add(Me.chkUseHooks)
        Me.Controls.Add(Me.txtLogWindow)
        Me.Controls.Add(Me.chkLogToWindow)
        Me.Controls.Add(Me.txtReply)
        Me.Controls.Add(Me.txtQuery)
        Me.Controls.Add(Me.btnSpyWin)
        Me.Controls.Add(Me.cmbCommandLine)
        Me.Controls.Add(Me.PropertyGrid1)
        Me.Controls.Add(Me.btnSendQuery)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.label999)
        Me.Controls.Add(Me.treeModel)
        Me.Controls.Add(Me.btnLaunch)
        Me.Name = "frmAdvanced"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region

    Private WithEvents mTargetApp As clsTargetApp

    Public Sub New()
        MyBase.New()

        Application.EnableVisualStyles()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Initialise config and report any issues...
        Dim sErr As String = Nothing
        If Not ApplicationManagerUtilities.clsConfig.Init(sErr) Then
            MsgBox(String.Format(My.Resources.ConfigProblem0, sErr))
        End If

        dlgAppendText = New AppendDelegate(AddressOf txtLogWindow.AppendText)

        'We always need one of these, otherwise we can't issue 'startapplication'
        'or 'attachapplication' commands directly from the query box. If we don't
        'do that and start it via other UI means, we'll just create a new one
        'anyway. This means that just using a 'startapplication' query will always
        'get you an internal appman.
        mTargetApp = New clsLocalTargetApp()

        cmbMode.SelectedIndex = 0
#If DEBUG Then
        cmbCommandLine.Items.AddRange(New Object() {
         "calc",
         "java -jar \automate\applicationmanager\unittest\bpjavatest\bpjavatest.jar",
         "notepad",
         "\automate\BluePrism.Automate\bin\Automate.exe /resourcepc /public",
         "c:\Program Files\Internet Explorer\iexplore.exe",
         "\automate\qa\code\vb6 app\vb6automatedtestapp.exe",
         "\automate\qa\code\VB6 App 2\vb6automatedtestapp.exe",
         "\automate\qa\code\VS2005 App\QAAutomatedTests\bin\Debug\QAAutomatedTests.exe",
         "\automate\qa\code\datagrid\bin\debug\datagrid.exe",
         "\automate\qa\Code\SecureBrowser\bin\Debug\SecureBrowser.exe"})
#End If
    End Sub

    Private Sub RunUIScript(ByVal sFile As String)
        Try

            treeModel.Nodes.Clear()
            txtLogWindow.Clear()

            mTargetApp =
             clsTargetApp.GetTargetApp(clsEnum(Of ProcessMode).Parse(cmbMode.Text))
            Dim sLine As String, sResult As String = "", sCmd As String
            Dim sRepeatUntil As String = Nothing
            Dim iIndex As Integer
            Using sr As New StreamReader(sFile)
                sLine = sr.ReadLine()
                Do While Not sLine Is Nothing
                    If sLine <> "" And Not sLine.StartsWith("#") Then
                        If sLine.StartsWith("$") Then
                            sRepeatUntil = Nothing
                            iIndex = sLine.IndexOf(" ")
                            If iIndex = -1 Then
                                sCmd = sLine.Substring(1)
                                sLine = ""
                            Else
                                sCmd = sLine.Substring(1, iIndex - 1)
                                sLine = sLine.Substring(iIndex + 1)
                            End If
                            Select Case sCmd
                                Case "Exit"
                                    ConsoleWriteLine(My.Resources.Exiting)
                                    Exit Do
                                Case "AbortIf"
                                    If sLine = sResult Then
                                        ConsoleWriteLine(My.Resources.Abort)
                                        Exit Do
                                    Else
                                        ConsoleWriteLine(String.Format(My.Resources.NoAbort01_notEqual, sLine, sResult))
                                    End If
                                Case "AbortIfNot"
                                    If sLine <> sResult Then
                                        ConsoleWriteLine(String.Format(My.Resources.Abort01, sLine, sResult))
                                        Exit Do
                                    Else
                                        ConsoleWriteLine(String.Format(My.Resources.NoAbort01_equal, sLine, sResult))
                                    End If
                                Case "RepeatUntil"
                                    sRepeatUntil = sLine
                                Case "Sleep"
                                    Thread.Sleep(Integer.Parse(sLine))
                                Case Else
                                    ConsoleWriteLine(String.Format(My.Resources.InvalidCommand0, sCmd))
                                    Exit Do
                            End Select
                        Else
                            ConsoleWriteLine(String.Format(My.Resources.Query0, sLine))
                            Application.DoEvents()
                            If Not sRepeatUntil Is Nothing Then
                                Do
                                    sResult = mTargetApp.ProcessQuery(sLine)
                                    If sResult = sRepeatUntil Then Exit Do
                                    Thread.Sleep(100)
                                Loop
                                sRepeatUntil = Nothing
                            Else
                                sResult = mTargetApp.ProcessQuery(sLine)
                            End If
                            ConsoleWriteLine(String.Format(My.Resources.Result0, sResult))
                            If sResult.StartsWith("ERROR:") Then
                                ConsoleWriteLine(My.Resources.AbortingErrorDetected)
                                Exit Do
                            End If
                        End If
                    End If
                    sLine = sr.ReadLine()
                Loop
            End Using

            'Don't disconnect or close the target app - we want to be able to carry
            'on interactively after the script has completed.
            ConsoleWriteLine(My.Resources.SCRIPTFINISHED)

        Catch ex As Exception
            ConsoleWriteLine(String.Format(My.Resources.Exception0, ex.Message))
        End Try
    End Sub

    Private Sub Connect()

        mTargetApp =
         clsTargetApp.GetTargetApp(clsEnum(Of ProcessMode).Parse(cmbMode.Text))

        Dim query As String = "startapplication path=" & clsQuery.EncodeValue(cmbCommandLine.Text)
        If Not chkUseHooks.Checked Then query &= " hook=False"
        If chkJAB.Checked Then query &= " jab=True"
        Dim resp As String = mTargetApp.ProcessQuery(query)
        If resp <> "OK" Then MsgBox(String.Format(My.Resources.FailedToLaunch0, resp))

    End Sub


    Private Sub CommsLineReceived(ByVal sLine As String) Handles mTargetApp.LineReceived
        ConsoleWriteLine(sLine)
    End Sub
    Private Sub Disconnected() Handles mTargetApp.Disconnected
        ConsoleWriteLine(My.Resources.DISCONNECTED)
    End Sub

    Private Sub ConsoleWriteLine(ByVal sLine As String)
        If Me.Visible Then
            If Me.chkLogToWindow.Checked Then
                Me.Invoke(dlgAppendText, sLine & vbCrLf)
            End If

        End If
    End Sub

    Private dlgAppendText As AppendDelegate

    Private Delegate Sub AppendDelegate(ByVal text As String)


    ''' <summary>
    ''' Update the tree to reflect the current state of the model.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub UpdateModelTree()

        If mTargetApp Is Nothing Then Exit Sub

        'Don't update if frozen via UI...
        If chkFreezeModel.Checked Then Exit Sub

        'We can't update the model tree unless we're using a local target app!
        Dim ltarget As clsLocalTargetApp = TryCast(mTargetApp, clsLocalTargetApp)
        If ltarget Is Nothing Then Exit Sub

        ltarget.UpdateModel()

        treeModel.BeginUpdate()

        'Get the list of entities in the model...
        Dim entities As List(Of clsUIEntity) = ltarget.Model.GetAllEntities()

        For Each e As clsUIEntity In entities
            If TypeOf e Is clsUIWindow AndAlso e.Parent Is Nothing Then

                UpdateModelTree(CType(e, clsUIWindow), treeModel.Nodes, entities)

                For Each n3 As TreeNode In treeModel.Nodes
                    If NeedsRemoving(n3, entities) Then
                        n3.Remove()
                    End If
                Next

            End If
        Next
        treeModel.EndUpdate()

        lblModel.Text = String.Format(My.Resources.Model0Entities, entities.Count)

    End Sub

    ''' <summary>
    ''' Used internally by the main UpdateModelTree to recursively populate the tree.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <param name="root"></param>
    ''' <param name="entities">The entities in the model</param>
    Private Sub UpdateModelTree(ByVal e As clsUIEntity, ByVal root As TreeNodeCollection, ByVal entities As List(Of clsUIEntity))

        Dim thisnode As TreeNode
        If NeedsAdding(root, e) Then
            thisnode = root.Add(e.Name)
        Else
            thisnode = FindNode(e, root)
        End If

        'For safety, exit if the node wasn't added/found, although this should never
        'happen...
        If thisnode Is Nothing Then
            Exit Sub
        End If

        thisnode.Tag = e

        For Each e2 As clsUIEntity In e.Children

            If TypeOf e2 Is clsUIText Then
                Dim et As clsUIText = CType(e2, clsUIText)
                If NeedsAdding(thisnode.Nodes, e2) Then
                    Dim n2 As TreeNode = thisnode.Nodes.Add(et.Name)
                    n2.Tag = et
                End If
            ElseIf TypeOf e2 Is clsUIDC Then
                Dim ed As clsUIDC = CType(e2, clsUIDC)
                If NeedsAdding(thisnode.Nodes, e2) Then
                    Dim n2 As TreeNode = thisnode.Nodes.Add(ed.Name)
                    n2.Tag = ed
                End If
            ElseIf TypeOf e2 Is clsUIWindow AndAlso e2.Parent Is e Then
                UpdateModelTree(CType(e2, clsUIWindow), thisnode.Nodes, entities)
            End If
        Next

        For Each n3 As TreeNode In thisnode.Nodes
            If NeedsRemoving(n3, entities) Then
                If Not n3.Parent Is Nothing Then
                    n3.Remove()
                End If
            End If
        Next

    End Sub

    ''' <summary>
    ''' Find the tree node relating to the given entity.
    ''' </summary>
    ''' <param name="e">The clsUIEntity to search for</param>
    ''' <param name="nc">The collection of nodes to search in</param>
    ''' <returns>The treenode found, or Nothing if not found.</returns>
    Private Function FindNode(ByVal e As clsUIEntity, ByVal nc As TreeNodeCollection) As TreeNode
        For Each n As TreeNode In nc
            If HandlesMatch(e, n) Then Return n
            If n.Tag Is e Then Return n
        Next
        Return Nothing
    End Function

    Private Function NeedsAdding(ByVal nc As TreeNodeCollection, ByVal e As clsUIEntity) As Boolean
        For Each n2 As TreeNode In nc
            If HandlesMatch(e, n2) Then Return False
            If n2.Tag Is e Then Return False
        Next
        Return True
    End Function

    Private Function NeedsRemoving(ByVal n As TreeNode, ByVal c As List(Of clsUIEntity)) As Boolean
        For Each e2 As clsUIEntity In c
            If HandlesMatch(e2, n) Then Return False
            If e2 Is n.Tag Then Return False
        Next
        Return True
    End Function

    Private Function HandlesMatch(ByVal e As clsUIEntity, ByVal n As TreeNode) As Boolean
        If TypeOf e Is clsUIWindow AndAlso TypeOf n.Tag Is clsUIWindow Then
            Dim wn As clsUIWindow = TryCast(e, clsUIWindow)
            Dim wn2 As clsUIWindow = TryCast(n.Tag, clsUIWindow)
            If wn.Handle = wn2.Handle Then
                Return True
            End If
        End If
        Return False
    End Function

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        UpdateModelTree()
    End Sub

    Private Sub Form1_Closed(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Closed
        If Not mTargetApp Is Nothing Then
            Dim ltarget As clsLocalTargetApp = TryCast(mTargetApp, clsLocalTargetApp)
            If ltarget IsNot Nothing Then ltarget.Disconnect()
        End If
    End Sub

    Private Sub btnSendQuery_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSendQuery.Click
        SendQuery()
    End Sub

    Private Sub SendQuery()
        txtReply.Text = My.Resources.Working
        Dim t As Date = Date.Now()
        txtReply.Text = mTargetApp.ProcessQuery(txtQuery.Text)
        Dim ts As TimeSpan = Date.Now() - t
        txtQueryInfo.Text = String.Format(My.Resources.QueryTook0Ms, CInt(ts.TotalMilliseconds))
    End Sub

    Private Sub treeModel_AfterSelect(ByVal sender As System.Object, ByVal e As System.Windows.Forms.TreeViewEventArgs) Handles treeModel.AfterSelect
        Me.PropertyGrid1.SelectedObject = e.Node.Tag
    End Sub

    Private Sub btnSpyWin_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSpyWin.Click
        txtReply.Text = mTargetApp.ProcessQuery("Spy")
    End Sub


    Private Sub btnLaunch_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnLaunch.Click
        Try
            treeModel.Nodes.Clear()
            txtLogWindow.Clear()
            Connect()
        Catch ex As Exception
            MsgBox(String.Format(My.Resources.CouldNotConnect0, ex.Message))
        End Try
    End Sub



    Private Sub Form1_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    End Sub

    Private Sub btnScript_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnScript.Click
        If dlgOpenScript.ShowDialog() = Windows.Forms.DialogResult.OK Then
            RunUIScript(dlgOpenScript.FileName)
        End If
    End Sub

    Private Sub txtQuery_KeyPress(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtQuery.KeyPress
        If e.KeyChar = Chr(13) Then
            SendQuery()
            e.Handled = True
        End If
    End Sub
End Class


