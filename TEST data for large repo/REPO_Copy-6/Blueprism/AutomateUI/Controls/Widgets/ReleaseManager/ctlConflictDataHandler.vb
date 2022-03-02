Imports BluePrism.AutomateAppCore

Public Class ctlConflictDataHandler
    Inherits UserControl

#Region " Private Members "

    Private mConflictDataHandler As ConflictDataHandler

    Private mConflict As Conflict

    Private mConflictOption As ConflictOption

    Private mDataHandlingControls As List(Of Control)

#End Region

#Region " Constructors "

    Public Sub New()
        InitializeComponent()
        Me.mDataHandlingControls = New List(Of Control)
    End Sub

#End Region

#Region " Properties "

    Public Property Conflict() As Conflict
        Get
            Return Me.mConflict
        End Get
        Set(ByVal value As Conflict)
            Me.mConflict = value
            Me.CreateControls()
        End Set
    End Property

    Public Property ConflictOption() As ConflictOption
        Get
            Return Me.mConflictOption
        End Get
        Set(ByVal value As ConflictOption)
            Me.mConflictOption = value
            Me.CreateControls()
        End Set
    End Property

    Public Property ConflictDataHandler() As ConflictDataHandler
        Get
            Return mConflictDataHandler
        End Get
        Set(ByVal value As ConflictDataHandler)
            mConflictDataHandler = value
            Me.CreateControls()
        End Set
    End Property

#End Region

#Region "Private Methods"
    ''' <summary>
    ''' Handles a process value control's value being changed.
    ''' </summary>
    Private Sub HandleProcessValueChanged(ByVal sender As Object, ByVal e As EventArgs)
        Dim src As Control = DirectCast(sender, Control)
        For Each ctl As Control In mDataHandlingControls
            If src Is ctl Then
                DirectCast(ctl.Tag, ConflictArgument).Value = DirectCast(src, IProcessValue).Value
            End If
        Next
    End Sub

    Private Sub CreateControls()
        If Me.mConflictDataHandler Is Nothing Or Me.mConflict Is Nothing Or Me.mConflictOption Is Nothing Then
            mDataHandlingControls.Clear()
            Me.mContents.Controls.Clear()
            Me.mContents.RowStyles.Clear()
            Return
        End If

        For Each arg As ConflictArgument In Me.mConflictDataHandler.Arguments
            Dim group As New GroupBox()
            group.Text = arg.CustomTitleOrDefault
            group.AutoSize = True
            group.Dock = DockStyle.Fill
            group.AutoSizeMode = AutoSizeMode.GrowOnly
            group.Tag = arg

            Dim ctl As Control = clsProcessValueControl.GetControl(arg.Value.DataType)
            ctl.AccessibleName = ctlConflict.GetAccessibleName(Me.mConflict.Component, Me.mConflictOption, arg.CustomTitleOrDefault)
            Dim ipv As IProcessValue = DirectCast(ctl, IProcessValue)
            ipv.Value = arg.Value
            AddHandler ipv.Changed, AddressOf HandleProcessValueChanged
            ctl.Tag = arg
            ctl.Dock = DockStyle.Top
            group.Controls.Add(ctl)
            mDataHandlingControls.Add(ctl)

            Me.mContents.RowCount = Me.mContents.RowCount + 1
            Me.mContents.Controls.Add(group)
        Next
    End Sub
#End Region
End Class
