Imports System.ComponentModel

Public Class CaptionedControl : Inherits TableLayoutPanel

#Region " Class-scope Declarations "

    ''' <summary>
    ''' ControlCollection geared towards working with the CaptionControl.
    ''' </summary>
    Private Class CaptionControlCollection : Inherits TableLayoutControlCollection

        ' The label which we should always have in this collection
        Private mLabel As Label

        ''' <summary>
        ''' Creates a new caption control collection for the given caption control
        ''' </summary>
        ''' <param name="container">The control which this collection is for</param>
        Public Sub New(ByVal container As CaptionedControl, ByVal lbl As Label)
            MyBase.New(container)
            mLabel = lbl
            Add(mLabel, 0, 1)
        End Sub

        ''' <summary>
        ''' Adds the given control to this control collection, removing any other
        ''' controls currently present (except the caption label) first.
        ''' </summary>
        ''' <param name="ctl">The control to add to this collection</param>
        ''' <remarks>If the given control already exists in this collection, this
        ''' call is ignored.</remarks>
        Public Overrides Sub Add(ByVal ctl As Control)
            Add(ctl, 0, 0)
        End Sub

        ''' <summary>
        ''' Adds the given control to this collection at the given location, removing
        ''' any other controls currently present (except the caption label) first.
        ''' </summary>
        ''' <param name="ctl">The control to add</param>
        ''' <param name="col">The column at which to add the control.</param>
        ''' <param name="row">The row at which to add the control.</param>
        ''' <exception cref="ArgumentException">If the column value is anything other
        ''' than zero (a caption control has a single column), and if the row value
        ''' is anything other than 1 (a caption control has 2 rows) -or- if a row
        ''' value of 1 is given and the control is anything other than the caption
        ''' label held in this collection.</exception>
        Public Overrides Sub Add(ByVal ctl As Control, ByVal col As Integer, ByVal row As Integer)
            If col > 0 OrElse row > 1 Then Throw New ArgumentException(String.Format(
             My.Resources.InvalidColumnRowForCaptionControlCol0Row1, col, row))

            If row = 1 AndAlso ctl IsNot mLabel Then Throw New ArgumentException(String.Format(
             My.Resources.ControlInRow1MustBeTheCaptionLabelAttemptedToSetTo0, ctl))

            If Contains(ctl) Then Return ' already there, ignore the add.

            If Count > 1 Then ' We have a control to remove first
                ' Mark each control which is not the caption label for removal
                Dim toRemove As New List(Of Control)
                For Each currCtl As Control In Me
                    If currCtl IsNot mLabel Then toRemove.Add(currCtl)
                Next
                ' And then remove it
                For Each remCtl As Control In toRemove
                    Remove(remCtl)
                Next
            End If

            ' Now set up the control as we need it
            If ctl Is mLabel Then ctl.Dock = DockStyle.Fill Else ctl.Dock = DockStyle.Top
            ' Er, is that it?

            ' We can now add the control
            MyBase.Add(ctl, col, row)
        End Sub

    End Class

#End Region

#Region " Member Variables "

    ' The caption label
    Private lblCaption As Label

    ' The control which is to be captioned
    Private WithEvents mControl As Control

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new, empty caption control.
    ''' </summary>
    Public Sub New()
        lblCaption = New Label()
        lblCaption.AutoSize = False
        lblCaption.Dock = DockStyle.Fill
        lblCaption.Font = New Font( _
         "Segoe UI", 7.5!, FontStyle.Regular, GraphicsUnit.Point, Byte.MinValue)

        ColumnCount = 1
        RowCount = 2
        ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0!))
        RowStyles.Add(New RowStyle())
        RowStyles.Add(New RowStyle(SizeType.Percent, 100.0!))
        Dock = DockStyle.Fill

    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Overloads the RowStyles property to hide it from the designer / editor
    ''' </summary>
    <Browsable(False), EditorBrowsable(EditorBrowsableState.Never)> _
    Public Overloads ReadOnly Property RowStyles As TableLayoutRowStyleCollection
        Get
            Return MyBase.RowStyles
        End Get
    End Property

    ''' <summary>
    ''' Overloads the ColumnStyles property to hide it from the designer / editor
    ''' </summary>
    <Browsable(False), EditorBrowsable(EditorBrowsableState.Never)> _
    Public Overloads ReadOnly Property ColumnStyles As TableLayoutColumnStyleCollection
        Get
            Return MyBase.ColumnStyles
        End Get
    End Property

    ''' <summary>
    ''' Overloads the RowCount property to hide it from the designer / editor
    ''' </summary>
    <Browsable(False), EditorBrowsable(EditorBrowsableState.Never)> _
    Public Overloads Property RowCount() As Integer
        Get
            Return MyBase.RowCount
        End Get
        Set(ByVal value As Integer)
            MyBase.RowCount = value
        End Set
    End Property

    ''' <summary>
    ''' Overloads the ColumnCount property to hide it from the designer / editor
    ''' </summary>
    <Browsable(False), EditorBrowsable(EditorBrowsableState.Never)> _
    Public Overloads Property ColumnCount() As Integer
        Get
            Return MyBase.ColumnCount
        End Get
        Set(ByVal value As Integer)
            MyBase.ColumnCount = value
        End Set
    End Property

    ''' <summary>
    ''' The control being captioned in this control
    ''' </summary>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
    Public Property Control() As Control
        Get
            Return mControl
        End Get
        Set(ByVal value As Control)
            If value Is mControl Then Return
            If mControl IsNot Nothing Then Controls.Remove(mControl)
            mControl = value
            If mControl IsNot Nothing Then Controls.Add(value, 0, 0)
        End Set
    End Property

    ''' <summary>
    ''' The caption displayed beneath the text box.
    ''' </summary>
    <Category("Appearance")> _
    Public Property Caption() As String
        Get
            Return lblCaption.Text
        End Get
        Set(ByVal value As String)
            lblCaption.Text = value
        End Set
    End Property

    ''' <summary>
    ''' The font of the caption label
    ''' </summary>
    <Category("Appearance")> _
    Public Property CaptionFont() As Font
        Get
            Return lblCaption.Font
        End Get
        Set(ByVal value As Font)
            lblCaption.Font = value
        End Set
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Creates a new control collection for this caption control
    ''' </summary>
    ''' <returns>A ControlCollection for use by this caption control.</returns>
    Protected Overrides Function CreateControlsInstance() As ControlCollection
        Return New CaptionControlCollection(Me, lblCaption)
    End Function

    Private Function ShouldSerializeRowCount() As Boolean
        Return False
    End Function

    Private Function ShouldSerializeColumnCount() As Boolean
        Return False
    End Function
#End Region

End Class
