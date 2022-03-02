
Imports BluePrism.AutomateProcessCore

''' Project  : Automate
''' Class    : clsProcessValueControl
''' 
''' <summary>
''' Class that deals with the various ProcessValue controls. All these controls
''' implement the IProcessValue interface.
''' </summary>
Friend Class clsProcessValueControl

    ''' <summary>
    ''' Get a control appropriate for editing the given data type. The control
    ''' will be populated with a default value.
    ''' </summary>
    ''' <param name="t">The DataType to be edited.</param>
    ''' <returns>A Control (that implements an IProcessValue interface).</returns>
    ''' <remarks>If a text datatype is given, this will return a <em>multi-line</em>
    ''' supporting text control</remarks>
    Public Shared Function GetControl(ByVal t As DataType) As Control
        Return GetControl(t, True)
    End Function

    ''' <summary>
    ''' Get a control appropriate for editing the given data type. The control
    ''' will be populated with a default value.
    ''' </summary>
    ''' <param name="t">The DataType to be edited.</param>
    ''' <returns>A Control (that implements an IProcessValue interface).</returns>
    Public Shared Function GetControl( _
     ByVal t As DataType, ByVal multiline As Boolean) As Control
        Dim c As IProcessValue
        Select Case t
            Case DataType.number
                c = New ctlProcessNumber()
                c.Value = 0
            Case DataType.text
                c = New ctlProcessText(multiline)
                c.Value = ""
            Case DataType.password
                c = New ctlProcessPassword()
                c.Value = New clsProcessValue(t)
            Case DataType.datetime
                c = New ctlProcessDateTime()
                c.Value = New clsProcessValue(t, Now)
            Case DataType.date
                c = New ctlProcessDate()
                c.Value = New clsProcessValue(t, Now)
            Case DataType.time
                c = New ctlProcessTime()
                c.Value = New clsProcessValue(t, Now)
            Case DataType.timespan
                c = New ctlProcessTimespan()
                c.Value = TimeSpan.Zero
            Case DataType.image
                c = New ctlProcessImage()
                c.Value = New clsProcessValue(t)
            Case DataType.binary
                c = New ctlProcessBinary()
                c.Value = New clsProcessValue(t)
            Case DataType.flag
                c = New ctlProcessFlag()
                c.Value = False
            Case DataType.collection
                c = New ctlProcessCollection()
                c.Value = New clsCollection()
            Case Else
                c = New ctlProcessUnknown()
        End Select
        Return CType(c, Control)

    End Function

End Class
