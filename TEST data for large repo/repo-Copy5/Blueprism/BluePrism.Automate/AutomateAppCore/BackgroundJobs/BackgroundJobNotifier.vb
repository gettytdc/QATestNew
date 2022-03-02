Imports System.Runtime.Serialization

Namespace BackgroundJobs
    ''' <summary>
    ''' Signals that updates have been made as a background job progresses. This allows 
    ''' direct connections and server connections that accept callbacks to update
    ''' immediately rather than polling for changes.
    ''' </summary>
    <DataContract([Namespace]:="bp"), Serializable>
    Public Class BackgroundJobNotifier : Inherits MarshalByRefObject

        ''' <summary>
        ''' Event fired when new data for a background job is available
        ''' </summary>
        Public Event Updated(sender As Object, args As EventArgs)

        ''' <summary>
        ''' Triggers the Updated event
        ''' </summary>
        Public Overridable Sub Notify()
            RaiseEvent Updated(Me, EventArgs.Empty)
        End Sub
    
    End Class
End NameSpace