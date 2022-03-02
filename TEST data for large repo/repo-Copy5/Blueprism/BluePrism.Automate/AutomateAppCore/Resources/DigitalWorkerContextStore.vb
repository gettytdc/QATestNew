Namespace Resources

    ''' <summary>
    ''' Supports updates to DigitalWorkerContext at startup after IOC container has been initialized.
    ''' </summary>
    ''' <remarks>It would be more elegant to use Autofac scopes. However there is only a single
    ''' instance per running application and DigitalWorkerContextStore is not accessed directly by
    ''' any components.</remarks>
    Public Module DigitalWorkerContextStore

        Private _current As DigitalWorkerContext = CreateDefault()

        Private Function CreateDefault() As DigitalWorkerContext
            Dim startUpOptions = New DigitalWorkerStartUpOptions With {
                .Name = New DigitalWorkerName("Default")
            }
            Return New DigitalWorkerContext(startUpOptions)
        End Function

        Public Property Current As DigitalWorkerContext
            Get
                Return _current
            End Get
            Set
                _current = If(Value, CreateDefault())
            End Set
        End Property
    End Module
End NameSpace
