
Imports System.Runtime.Serialization
''' <summary>
''' Simple class used to monitor progress - perhaps across application domain
''' boundaries.
''' </summary>
<DataContract(Namespace:="bp")>
<Serializable>
Public Class clsProgressMonitor : Inherits MarshalByRefObject

    ''' <summary>
    ''' Event fired when progress has change.
    ''' </summary>
    ''' <param name="value">The value to which the progress should be set</param>
    ''' <param name="data">The callback data with any progress information - this
    ''' will be determined by the context of the event.</param>
    Public Event ProgressChanged(ByVal value As Integer, ByVal data As Object)

    ' The last reported progress value
    Protected mLastValue As Integer

    ' A cancel request flag
    Protected mCancel As Boolean

    ''' <summary>
    ''' The last progress value fired in this progress monitor
    ''' </summary>
    Public ReadOnly Property LastValue() As Integer
        Get
            Return mLastValue
        End Get
    End Property

    ''' <summary>
    ''' Whether a cancel request has been made on this progress monitor
    ''' </summary>
    Public ReadOnly Property IsCancelRequested() As Boolean
        Get
            Return mCancel
        End Get
    End Property

    ''' <summary>
    ''' In-class handler for the progress changed event.
    ''' The base implementation just records the last value set and fires the event
    ''' </summary>
    ''' <param name="value">The value to which the progress should be set</param>
    ''' <param name="data">The callback data with any progress information - this
    ''' will be determined by the context of the event.</param>
    Protected Overridable Sub OnProgressChanged( _
     ByVal value As Integer, ByVal data As Object)
        mLastValue = value
        RaiseEvent ProgressChanged(value, data)
    End Sub

    ''' <summary>
    ''' Requests a cancellation on this progress monitor.
    ''' </summary>
    Public Overridable Sub RequestCancel()
        mCancel = True
    End Sub

    ''' <summary>
    ''' Fires the progress changed event with the given values.
    ''' </summary>
    ''' <param name="value">The value to which progress should be set. As a rule of
    ''' thumb this should be between 0 and 100, though this rule is not enforced at
    ''' this level.</param>
    ''' <param name="data">The data associated with the progress change. This may
    ''' be null.</param>
    Public Sub FireProgressChange(ByVal value As Integer, ByVal data As Object)
        OnProgressChanged(value, data)
    End Sub

    ''' <summary>
    ''' Fires the progress changed event with the given values.
    ''' </summary>
    ''' <param name="value">The value to which progress should be set. As a rule of
    ''' thumb this should be between 0 and 100, though this rule is not enforced at
    ''' this level.</param>
    Public Sub FireProgressChange(ByVal value As Integer)
        FireProgressChange(value, Nothing)
    End Sub

End Class
