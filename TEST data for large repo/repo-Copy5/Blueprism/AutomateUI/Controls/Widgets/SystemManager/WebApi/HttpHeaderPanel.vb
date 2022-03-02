Imports AutomateControls
Imports BluePrism.AutomateProcessCore.WebApis

Namespace Controls.Widgets.SystemManager.WebApi
    ''' <summary>
    ''' Panel used to view and edit HTTP Headers
    ''' </summary>
    Friend Class HttpHeaderPanel : Implements IGuidanceProvider

        ' The headers being edited in this panel
        Private mHeaders As WebApiCollection(Of HttpHeader)

        ''' <summary>
        ''' Gets the guidance text for this panel.
        ''' </summary>
        <Browsable(False),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public ReadOnly Property GuidanceText As String _
         Implements IGuidanceProvider.GuidanceText
            Get
                Return If(mHeaders?.ActionSpecific,
                    WebApi_Resources.GuidanceActionSpecificHttpHeaderPanel,
                    WebApi_Resources.GuidanceHttpHeaderPanel
                )
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the headers in this panel.
        ''' </summary>
        <Browsable(False),
         DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property Headers As WebApiCollection(Of HttpHeader)
            Get
                UpdateCollection()
                Return mHeaders
            End Get
            Set(value As WebApiCollection(Of HttpHeader))
                mHeaders = value
                gridHeaders.Rows.Clear()
                If value Is Nothing Then Return
                For Each header In value
                    Dim index = gridHeaders.Rows.Add(header.Name, header.Value)
                    gridHeaders.Rows(index).Tag = header
                Next
            End Set
        End Property

        ''' <summary>
        ''' Updates the collection with the data from this panel
        ''' </summary>
        Private Sub UpdateCollection()
            mHeaders.Clear()
            Dim coll As New List(Of HttpHeader)
            For Each row As DataGridViewRow In gridHeaders.Rows

                Dim header = TryCast(row.Tag, HttpHeader)
                ' If no Action Parameter stored in the tag it must mean a new row, so
                ' set the id to be 0
                Dim id = If(header IsNot Nothing, header.Id, 0)
                Dim name = row.GetStringValue(colHeaderName)
                Dim value = If(row.GetStringValue(colHeaderValue), String.Empty)
                If Not String.IsNullOrEmpty(name) Then mHeaders.Add(New HttpHeader(id, name, value))
            Next
        End Sub

        ''' <summary>
        ''' Handles the data grid view being validated, ensuring that the underlying
        ''' collection is updated with the modified data
        ''' </summary>
        Private Sub HandleDataGridViewValidated(sender As Object, e As EventArgs) _
         Handles gridHeaders.Validated
            UpdateCollection()
        End Sub

    End Class

End Namespace
