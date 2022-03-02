Imports AutomateControls
Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateAppCore
Imports BluePrism.Images

''' Project  : Automate
''' Class    : frmStartParams
''' 
''' <summary>
''' Sets the start parametersXML for a given session
''' </summary>
Friend Class frmStartParams
    Inherits frmForm
    Implements IHelp, IEnvironmentColourManager

#Region " Member Variables "

    ' Used to indicate whether the form is still loading.
    Private mLoading As Boolean = True

    ' Flag indicating if this the checking of a tree node is user-initiated
    Private mUserInitiatedCheck As Boolean

    ' The splitter position as a proportion of the width of the form.
    ' Used when resizing the form to mainain the proportions of the two sections.
    Private mSplitterPosition As Double

    ' All the sessions which require start parameters set
    Private mAllSessions As ICollection(Of ISession)

    ' The tick icon image used for 'checking' a session and the edit controls
    Private mTickIcon As Image

    ' Flag to indicate form is in read-only mode
    Private mReadOnly As Boolean

#End Region

#Region "Windows Forms Designer Generated Code"
    Private WithEvents btnHelp As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents pnlMain As System.Windows.Forms.Panel
    Friend WithEvents mobjBlueIconBar As AutomateControls.TitleBar
    Friend WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnStart As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents tvSessions As System.Windows.Forms.TreeView
    Friend WithEvents Splitter1 As System.Windows.Forms.Splitter
    Friend WithEvents panMain As System.Windows.Forms.Panel
    Friend WithEvents btnSave As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents imgTicks As System.Windows.Forms.ImageList
    Private components As System.ComponentModel.IContainer


    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmStartParams))
        Me.btnHelp = New AutomateControls.Buttons.StandardStyledButton()
        Me.mobjBlueIconBar = New AutomateControls.TitleBar()
        Me.pnlMain = New System.Windows.Forms.Panel()
        Me.panMain = New System.Windows.Forms.Panel()
        Me.Splitter1 = New System.Windows.Forms.Splitter()
        Me.tvSessions = New System.Windows.Forms.TreeView()
        Me.imgTicks = New System.Windows.Forms.ImageList(Me.components)
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnStart = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnSave = New AutomateControls.Buttons.StandardStyledButton()
        Me.pnlMain.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnHelp
        '
        resources.ApplyResources(Me.btnHelp, "btnHelp")
        Me.btnHelp.Name = "btnHelp"
        '
        'mobjBlueIconBar
        '
        resources.ApplyResources(Me.mobjBlueIconBar, "mobjBlueIconBar")
        Me.mobjBlueIconBar.Name = "mobjBlueIconBar"
        '
        'pnlMain
        '
        resources.ApplyResources(Me.pnlMain, "pnlMain")
        Me.pnlMain.Controls.Add(Me.panMain)
        Me.pnlMain.Controls.Add(Me.Splitter1)
        Me.pnlMain.Controls.Add(Me.tvSessions)
        Me.pnlMain.Name = "pnlMain"
        '
        'panMain
        '
        resources.ApplyResources(Me.panMain, "panMain")
        Me.panMain.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.panMain.Name = "panMain"
        '
        'Splitter1
        '
        resources.ApplyResources(Me.Splitter1, "Splitter1")
        Me.Splitter1.Name = "Splitter1"
        Me.Splitter1.TabStop = False
        '
        'tvSessions
        '
        resources.ApplyResources(Me.tvSessions, "tvSessions")
        Me.tvSessions.HideSelection = False
        Me.tvSessions.ImageList = Me.imgTicks
        Me.tvSessions.Name = "tvSessions"
        '
        'imgTicks
        '
        Me.imgTicks.ImageStream = CType(resources.GetObject("imgTicks.ImageStream"), System.Windows.Forms.ImageListStreamer)
        Me.imgTicks.TransparentColor = System.Drawing.Color.Transparent
        Me.imgTicks.Images.SetKeyName(0, "")
        Me.imgTicks.Images.SetKeyName(1, "")
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.Name = "btnCancel"
        '
        'btnStart
        '
        resources.ApplyResources(Me.btnStart, "btnStart")
        Me.btnStart.Name = "btnStart"
        '
        'btnSave
        '
        resources.ApplyResources(Me.btnSave, "btnSave")
        Me.btnSave.Name = "btnSave"
        '
        'frmStartParams
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.btnSave)
        Me.Controls.Add(Me.btnStart)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.pnlMain)
        Me.Controls.Add(Me.mobjBlueIconBar)
        Me.Controls.Add(Me.btnHelp)
        Me.Name = "frmStartParams"
        Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide
        Me.pnlMain.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
#End Region

    ''' <summary>
    ''' We initialise the form, and create new collection objects to hold the sessions
    ''' and processes
    ''' </summary>
    Public Sub New()
        InitializeComponent()
        mAllSessions = New List(Of ISession)
    End Sub

    ''' <summary>
    ''' This returns the help file for the form
    ''' </summary>
    ''' <returns></returns>
    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "frmStartParams.htm"
    End Function

    ''' <summary>
    ''' This property allows the sessions in the start parameters form to be accessed
    ''' </summary>
    ''' <remarks>Note that this copies the sessions from the given collection; it
    ''' does not attempt to keep the reference passed into it.</remarks>
    Public Property Sessions() As ICollection(Of ISession)
        Get
            If mAllSessions Is Nothing Then mAllSessions = New List(Of ISession)
            Return mAllSessions
        End Get
        Set(ByVal value As ICollection(Of ISession))
            Sessions.Clear()
            If value Is Nothing Then Return
            For Each sess As ISession In value : Sessions.Add(sess) : Next
        End Set
    End Property

    ''' <summary>
    ''' This property allows the form to be created as read-only
    ''' </summary>
    Public Property [ReadOnly]() As Boolean
        Get
            Return mReadOnly
        End Get
        Set(ByVal value As Boolean)
            mReadOnly = value
        End Set
    End Property

    ''' <summary>
    ''' On for load we get the unique processes populate the treeview and populate
    ''' the parameters panel.
    ''' </summary>
    Private Sub frmStartParams_Load(ByVal sender As Object, ByVal e As EventArgs) _
     Handles MyBase.Load
        mLoading = True

        mTickIcon = ToolImages.Tick_16x16

        PopulateTreeView()
        tvSessions.Select()
        GetSelectedSession()
        mSplitterPosition = Splitter1.Left / Me.Width


        Me.btnStart.Enabled = Not mReadOnly
        Me.btnSave.Enabled = Not mReadOnly
        mLoading = False
    End Sub

    ''' <summary>
    ''' Gets a map of the sessions, keyed on the ID of the process that they're
    ''' running
    ''' </summary>
    ''' <returns>A map of collections of sessions, keyed against the process ID of
    ''' the top level process that they are running</returns>
    Private Function GetSessionMap() As IDictionary(Of Guid, ICollection(Of ISession))
        Dim map As New Dictionary(Of Guid, ICollection(Of ISession))
        For Each sess As ISession In mAllSessions
            Dim coll As ICollection(Of ISession) = Nothing
            If Not map.TryGetValue(sess.ProcessID, coll) Then
                coll = New List(Of ISession)
                map(sess.ProcessID) = coll
            End If
            coll.Add(sess)
        Next
        Return map
    End Function

    ''' <summary>
    ''' Populates the Treeview with sessions, grouped by process.
    ''' </summary>
    Private Sub PopulateTreeView()
        For Each pair As _
         KeyValuePair(Of Guid, ICollection(Of ISession)) In GetSessionMap()

            Dim name As String = gSv.GetProcessNameByID(pair.Key)
            Dim n As TreeNode = tvSessions.Nodes.Add(name)
            n.Tag = pair.Key
            For Each sess As ISession In pair.Value
                With n.Nodes.Add(sess.ResourceName)
                    .Tag = sess
                    .Checked = sess.Arguments.AreAnyArgsSet
                End With
            Next
        Next
        tvSessions.ExpandAll()
    End Sub

    ''' <summary>
    ''' Gets the parameters for the current selection to use in PopulateParameters
    ''' </summary>
    Private Sub GetSelectedSession()

        Dim selNode As TreeNode = tvSessions.SelectedNode
        If selNode Is Nothing Then Return

        If TypeOf selNode.Tag Is Guid Then ' A process, child nodes are sessions

            For Each node As TreeNode In selNode.Nodes
                If Not node.Checked Then
                    Dim sess As ISession = CType(node.Tag, ISession)
                    sess.Arguments = GetBlankArguments(sess)
                    node.Tag = sess
                End If
            Next

            If mAllSessions.Count = 1 Then
                PopulateParameters(CType(selNode.Nodes(0).Tag, ISession), selNode)

            Else
                Dim sess As ISession = CType(selNode.Nodes(0).Tag, ISession)
                Dim clone As ISession = CType(sess.Clone(), ISession)

                For Each p As clsArgument In clone.Arguments
                    p.Value = New clsProcessValue(p.Value.DataType)
                Next

                PopulateParameters(clone, selNode)
            End If

        ElseIf TypeOf selNode.Tag Is ISession Then ' A session
            PopulateParameters(CType(selNode.Tag, ISession), selNode)

        End If

    End Sub

    ''' <summary>
    ''' Builds a panel of ctlProcessValueEdit controls which allow you to edit process
    ''' parameters
    ''' </summary>
    Private Sub PopulateParameters(ByVal sess As ISession, ByVal node As TreeNode)

        panMain.Controls.Clear()
        panMain.SuspendLayout()
        panMain.Visible = False

        If sess.Arguments.Count = 0 Then _
         sess.Arguments = GetBlankArguments(sess)

        For Each p As clsArgument In sess.Arguments
            Dim c = New ctlProcessValueEdit(p.Name, p.Value.DataType, mTickIcon, True, "ctlProcessValueEdit")
            CType(c, IProcessValue).ReadOnly = mReadOnly
            CType(c, IProcessValue).Value = p.Value
            c.Tag = node
            panMain.Controls.Add(c)
        Next

        Dim i As Integer = 8
        For Each c As Control In Me.panMain.Controls
            c.Top = i
            c.Left = 8
            c.Width = Me.panMain.ClientSize.Width - 16
            c.Anchor = AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Top
            i += c.Height + 8
        Next

        'add a dummy padding space to make a gap at the bottom of the list
        Dim pnl As New Panel
        pnl.Height = 0
        pnl.Top = i
        pnl.Left = 8
        pnl.Anchor = AnchorStyles.Left Or AnchorStyles.Right Or AnchorStyles.Top
        pnl.Width = Me.panMain.ClientSize.Width - 16
        Me.panMain.Controls.Add(pnl)


        If panMain.Controls.Count > 0 Then panMain.Controls(0).Focus()

        panMain.ResumeLayout(True)
        panMain.Visible = True
    End Sub

    ''' <summary>
    ''' Inspects the sessions process, and returns a list of blank arguments based 
    ''' on the processes parameters
    ''' </summary>
    ''' <param name="session"></param>
    Private Function GetBlankArguments(ByVal session As ISession) As clsArgumentList
        Dim argList = New clsArgumentList()
        Dim args = gSv.GetBlankProcessArguments(session.ProcessID)
        argList.AddRange(args)
        Return argList

    End Function

    ''' <summary>
    ''' Finds the treenode corresponding to the specified session and selects it.
    ''' The space listing all parameters in that session will also be refreshed.
    ''' </summary>
    ''' <param name="sess">The session to select</param>
    Private Sub SelectSession(ByVal sess As ISession)
        If sess Is Nothing Then Return

        ' The first level of nodes is the processes
        For Each n As TreeNode In tvSessions.Nodes

            ' We can ignore any process other than this session's process
            If DirectCast(n.Tag, Guid) <> sess.ProcessID Then Continue For

            ' Otherwise find our session within the process's child nodes
            For Each nn As TreeNode In n.Nodes

                ' This isn't the session you're looking for <hand-wave>
                If nn.Tag IsNot sess Then Continue For

                ' We've found the session node so we'll be returning at this point
                ' regardless, we just now have to decide whether to set the selected
                ' node or not.
                Dim selNode As TreeNode = tvSessions.SelectedNode

                ' If it or its parent is already selected, no need to bother
                If selNode Is nn OrElse selNode Is n Then Return

                ' Select this node - the handler will deal with the rest
                tvSessions.SelectedNode = nn

            Next
        Next

    End Sub

    ''' <summary>
    ''' Tests the name of the parameter against all those listed in the main
    ''' editing area of the form. If any such matches then it selects that
    ''' control. If no matches are found then no action is taken.
    ''' </summary>
    ''' <param name="objParameter">The parameter to be selected.</param>
    Private Sub SelectParameter(ByVal objParameter As clsArgument)
        Me.SelectParameter(objParameter.Name)
    End Sub

    ''' <summary>
    ''' Tests the name of the parameter against all those listed in the main
    ''' editing area of the form. If any such matches then it selects that
    ''' control. If no matches are found then no action is taken.
    ''' </summary>
    ''' <param name="sParameterName">The name of the parameter to be selected.
    ''' </param>
    Private Sub SelectParameter(ByVal sParameterName As String)
        For Each PvEdit As ctlProcessValueEdit In Me.panMain.Controls.OfType(Of ctlProcessValueEdit)
            If PvEdit.Title.Equals(sParameterName) Then
                PvEdit.Select()
                Exit Sub
            End If
        Next
    End Sub

    ''' <summary>
    ''' Checks for blank parameters and prompts the user if any blank 
    ''' parameters are found.
    ''' </summary>
    ''' <returns>Returns true if no blank paramaters found (considered a success);
    ''' returns true if there exist blank parameters but the user doesn't care;
    ''' returns false if there are blank parameters and the user would like
    ''' to change them.</returns>
    Private Function AreAllArgumentsValid() As Boolean
        'See if there are any bad parameters.
        Dim editControls = Controls.Find("ctlProcessValueEdit", True)
        Dim badParam As String = Nothing
        Dim badSess As ISession = Nothing

        For Each session As ISession In Me.Sessions
            If session.Arguments Is Nothing Then
                badSess = session
                Exit For
            End If
            badParam = editControls.
                Cast(Of ctlProcessValueEdit)().
                FirstOrDefault(Function(x) String.IsNullOrWhiteSpace(x.Value.EncodedValue))?.
                Title
        Next

        ' If we have a blank entry and the user cancels the starting, select
        ' the session and retain the form by returning false.
        If (badSess IsNot Nothing OrElse badParam IsNot Nothing) AndAlso
         UserMessage.YesNo(
          My.Resources.SomeParametersWithBlankInputValuesHaveBeenDetectedAreYouSureYouWantToStartThese) = MsgBoxResult.No Then

            If badSess IsNot Nothing Then SelectSession(badSess)
            If badParam IsNot Nothing Then SelectParameter(badParam)
            Return False

        End If

        'If we get to here then either there are no blank
        'parameters or the user doesn't care.
        Return True

    End Function

    ''' <summary>
    ''' Event handler for the cancel button (closes the form)
    ''' </summary>
    Private Sub btnCancel_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnCancel.Click
        DialogResult = DialogResult.Cancel
    End Sub

    ''' <summary>
    ''' The event handler for the treeview before check event. Users cannot check the
    ''' checkboxes, but if they are already checked then unchecking them is allowed.
    ''' unchecking the checkboxes clears the values.
    ''' </summary>
    Private Sub TreeView1_BeforeCheck(
     ByVal sender As Object, ByVal e As TreeViewCancelEventArgs) _
     Handles tvSessions.BeforeCheck

        ' If this was a programmatic action, we don't need to process it
        If Not mUserInitiatedCheck Then Return

        ' If it's a user action then cancel any 'checking' actions
        If Not e.Node.Checked Then e.Cancel = True : Return

        If TypeOf e.Node.Tag Is ISession Then
            ' If the actioned node is a session, clear its arguments
            CType(e.Node.Tag, ISession).Arguments.Clear()
        ElseIf TypeOf e.Node.Tag Is Guid Then
            ' If the actioned node is a process, uncheck the sessions too
            For Each node As TreeNode In e.Node.Nodes
                ' Note that the handler for each session node will deal with the args
                ' since 'userInitiatedCheck' is still true
                node.Checked = False
            Next
        End If

        If e.Node.IsSelected Then GetSelectedSession()

    End Sub

    ''' <summary>
    ''' Event handler to capture the select event of the treeview
    ''' </summary>
    Private Sub TreeView1_AfterSelect(ByVal sender As Object, ByVal e As TreeViewEventArgs) Handles tvSessions.AfterSelect
        Try
            GetSelectedSession()
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.InternalError0PleaseContactBluePrismForSupportIfThisProblemPersists, ex.Message))
        End Try
    End Sub

    ''' <summary>
    ''' Procedure which is called based on the dialog result of the form to store
    ''' the entered parameters into the session.
    ''' </summary>
    Private Sub UpdateSessionArguments()
        Try
            Dim editCtls = Controls.Find("ctlProcessValueEdit", True)

            For Each control As ctlProcessValueEdit In editCtls
                Dim ctlValue As clsProcessValue = control.Value
                Dim ctlName As String = control.Title
                Dim n = CType(control.Tag, TreeNode)

                If TypeOf n.Tag Is Guid Then 'Process
                    For Each subnode As TreeNode In n.Nodes
                        Dim arg As clsArgument =
                         CType(subnode.Tag, ISession).Arguments.GetArgumentByName(ctlName)
                        If arg Is Nothing Then Continue For

                        arg.Value = ctlValue.Clone()
                        subnode.Checked = True
                    Next
                    n.Checked = True

                ElseIf TypeOf n.Tag Is ISession Then 'Session
                    Dim sess = CType(n.Tag, ISession)
                    Dim arg As clsArgument = sess.Arguments.GetArgumentByName(ctlName)
                    If arg IsNot Nothing Then arg.Value = ctlValue

                    n.Checked = sess.Arguments.AreAnyArgsSet
                End If
            Next
        Catch ex As Exception
            UserMessage.Err(
             My.Resources.InternalError0PleaseContactBluePrismForSupportIfThisProblemPersists,
             ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' The event handler for the start button
    ''' </summary>
    Private Sub btnStart_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnStart.Click
        If AreAllArgumentsValid() Then
            UpdateSessionArguments()
            DialogResult = DialogResult.Yes
        End If
    End Sub

    ''' <summary>
    ''' The event handler for the save button.
    ''' </summary>
    Private Sub btnSave_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnSave.Click
        If AreAllArgumentsValid() Then
            UpdateSessionArguments()
            DialogResult = DialogResult.OK
        End If
    End Sub

    ''' <summary>
    ''' Handles the checking of the node by altering the image index to match it.
    ''' </summary>
    Private Sub HandleNodeChecked( _
     ByVal sender As Object, ByVal e As TreeViewEventArgs) _
     Handles tvSessions.AfterCheck
        With e.Node
            Dim ind As Integer = CInt(IIf(.Checked, 1, 0))
            .ImageIndex = ind
            .SelectedImageIndex = ind
        End With
    End Sub

    ''' <summary>
    ''' The event handler for the help button
    ''' </summary>
    Private Sub btnHelp_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnHelp.Click
        Try
            OpenHelpFile(Me, GetHelpFile)
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub

    ''' <summary>
    ''' Handles the mousedown event to pass clicking of the picture through to the 
    ''' Before_Check event handler
    ''' </summary>
    Private Sub tvSessions_MouseDown( _
     ByVal sender As Object, ByVal e As MouseEventArgs) Handles tvSessions.MouseDown
        Dim n As TreeNode = tvSessions.GetNodeAt(e.X, e.Y)
        If n Is Nothing OrElse mReadOnly Then Return

        Dim fullrect As Rectangle = n.Bounds
        Dim picturerect As Rectangle = New Rectangle(fullrect.X - 16, fullrect.Y, 16, 16)

        If picturerect.Contains(e.X, e.Y) Then _
         mUserInitiatedCheck = True : n.Checked = False : mUserInitiatedCheck = False

    End Sub

    Private Sub frmStartParams_Resize(ByVal sender As Object, ByVal e As EventArgs) _
     Handles MyBase.Resize
        ' Ignore when loading the form
        If mLoading Then Return

        tvSessions.Width = Math.Max(CInt(Me.Width * Me.mSplitterPosition), 60)
        mSplitterPosition = Me.Splitter1.Left / Me.Width

    End Sub

    Public Property EnvironmentBackColor As Color Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return mobjBlueIconBar.BackColor
        End Get
        Set(value As Color)
            mobjBlueIconBar.BackColor = value
        End Set
    End Property

    Public Property EnvironmentForeColor As Color Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return mobjBlueIconBar.TitleColor
        End Get
        Set(value As Color)
            mobjBlueIconBar.TitleColor = value
        End Set
    End Property
End Class
