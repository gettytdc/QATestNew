Namespace DataMonitor

    ''' <summary>
    ''' A holding class for the constants representing the data names for the data
    ''' which is monitorable on the database using an <see cref="IDataMonitor"/>.
    ''' </summary>
    Public Class DataNames

        ''' <summary>
        ''' The data name for the versioned scheduler data
        ''' </summary>
        Public Const Scheduler As String = "Scheduler"

        ''' <summary>
        ''' The data name for the versioned font data
        ''' </summary>
        Public Const Font As String = "Font"

        ''' <summary>
        ''' The data name for the versioned roles data
        ''' </summary>
        Public Const Roles As String = "Roles"

        ''' <summary>
        ''' The data name for the versioned Preferences
        ''' </summary>
        Public Const Preferences As String = "Preferences"

        Public Const ConfiguredSnapshots As String = "Configured Snapshots"

        Public Const Licensing As String = "Licensing"
    End Class

End Namespace
