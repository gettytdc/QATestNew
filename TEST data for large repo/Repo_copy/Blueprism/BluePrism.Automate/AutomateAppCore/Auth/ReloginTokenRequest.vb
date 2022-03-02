Imports System.Runtime.Serialization
Imports BluePrism.Common.Security

Namespace Auth

    <DataContract([Namespace]:="bp")>
    <Serializable>
    Public Class ReloginTokenRequest

        <DataMember>
        Private ReadOnly mMachineName As String

	    <DataMember>
	    Private ReadOnly mProcessId As Integer

        <DataMember>
        Private ReadOnly mPreviousReloginToken As SafeString

        Public ReadOnly Property MachineName As String
            Get
                Return mMachineName
            End Get
        End Property
	    
	    Public ReadOnly Property ProcessId As Integer
		    Get
			    Return mProcessId
		    End Get
	    End Property

        Public ReadOnly Property PreviousReloginToken As SafeString
            Get
                Return mPreviousReloginToken 
            End Get
        End Property

        Public Sub New(machineName As String, processId As Integer, previousReloginToken As SafeString)
            mMachineName = machineName
            mProcessId = processId
            mPreviousReloginToken = previousReloginToken 
        End Sub
    End Class
End Namespace
