Imports BluePrism.AutomateProcessCore

Public Class ctlCollectionDefinition : Implements IActivatableProcessValue

    ''' <summary>
    ''' The value backing this control
    ''' </summary>
    Private mValue As clsProcessValue

    ''' <summary>
    ''' The default width applied to this control.
    ''' </summary>
    Private Const DefaultWidth As Integer = 120

    ''' <summary>
    ''' The default height applied to this control.
    ''' </summary>
    Private Const DefaultHeight As Integer = 24

    Public Event Activated(ByVal sender As IActivatableProcessValue, ByVal e As EventArgs) _
     Implements IActivatableProcessValue.Activated

    Public Event Changed(ByVal sender As Object, ByVal e As EventArgs) _
     Implements IProcessValue.Changed

    Public Sub SelectControl() Implements IProcessValue.SelectControl
        lnkDefinition.Focus()
    End Sub

    ''' <summary>
    ''' Bit odd, this, but necessary since everything is built up around
    ''' clsProcessValues, and a clsCollectionInfo doesn't quite fit.
    ''' This value represents a collection <em>with the definition which is being
    ''' edited by this control</em>. This control is not editing the collection
    ''' itself - only its definition.
    ''' </summary>
    Public Property Value() As clsProcessValue Implements IProcessValue.Value
        Get
            Return mValue
        End Get
        Set(ByVal value As BluePrism.AutomateProcessCore.clsProcessValue)
            mValue = value
            UpdateLabel()
        End Set
    End Property

    ''' <summary>
    ''' We store the actual definition that we're given, even though we're not
    ''' actually displaying that definition in the view - it has dependencies
    ''' that are lost when the data is transferred to the process value that
    ''' represents the definition in the view.
    ''' </summary>
    Private mActualDefinition As clsCollectionInfo

    ''' <summary>
    ''' Gets or sets the definition that this control is representing.
    ''' The definition is not changed within this control, and the one returned
    ''' will be the actual object passed in - the one used in the view has the
    ''' same attributes / data as that object but doesn't have the same
    ''' defined relationships (specifically, the same ancestry).
    ''' </summary>
    Public Property Definition() As clsCollectionInfo
        Get
            Return mActualDefinition
        End Get
        Set(ByVal value As clsCollectionInfo)
            mActualDefinition = value
            If value Is Nothing Then
                mValue = New clsProcessValue()

            Else
                Dim dummyColl As New clsCollection()
                For Each fld As clsCollectionFieldInfo In value
                    dummyColl.AddField(fld)
                Next
                dummyColl.SingleRow = value.SingleRow
                mValue = New clsProcessValue(dummyColl)
            End If
            UpdateLabel()

        End Set
    End Property

    Public Overrides Function GetPreferredSize(ByVal proposed As Size) As Size
        Return New Size(Math.Max(DefaultWidth, proposed.Width), DefaultHeight)
    End Function

    Private Sub UpdateLabel()
        Dim coll As clsCollection = mValue.Collection
        Dim label As String
        If mValue.DataType <> DataType.collection Then
            label = ""
        Else
            Dim defn As clsCollectionInfo = Nothing
            If coll IsNot Nothing Then defn = coll.Definition
            label = clsCollectionInfo.GetInfoLabel(defn)
        End If
        lnkDefinition.Text = label
    End Sub

    Private Sub HandleLinkClicked(ByVal sender As Object, ByVal e As LinkLabelLinkClickedEventArgs) _
     Handles lnkDefinition.LinkClicked
        RaiseEvent Activated(Me, EventArgs.Empty)
    End Sub

    Public Property [ReadOnly]() As Boolean Implements IProcessValue.ReadOnly
        Get
            Return True
        End Get
        Set(ByVal value As Boolean)
            'Do Nothing
        End Set
    End Property

    ''' <summary>
    ''' Commits the changes made in this control
    ''' </summary>
    Public Sub Commit() Implements IProcessValue.Commit
        ' This control doesn't alter the value in and of itself, so there's
        ' no work to do here
    End Sub

End Class

