
Imports System.Runtime.Serialization
Imports BluePrism.AutomateProcessCore.My.Resources

<DataContract([Namespace]:="bp"), Serializable>
Public Class WebConnectionBaseSettings

    Private Const ConnectionLimit_MinimumValue As Integer = 2
    Private Const MaxIdleTime_MinimumValue As Integer = 5
    Private Const ConnectionLeaseTimeout_MinimumValue As Integer = 1

    Public ReadOnly Property ConnectionLeaseTimeout As Integer?
        Get
            Return mConnectionLeaseTimeout
        End Get
    End Property

    Public ReadOnly Property ConnectionLimit As Integer
        Get
            Return mConnectionLimit
        End Get
    End Property

    Public ReadOnly Property MaxIdleTime As Integer
        Get
            Return mMaxIdleTime
        End Get
    End Property

    <DataMember>
    Private mConnectionLimit As Integer
    <DataMember>
    Private mMaxIdleTime As Integer
    <DataMember>
    Private mConnectionLeaseTimeout As Integer?

    Public Sub New()
        Me.New(ConnectionLimit_MinimumValue, MaxIdleTime_MinimumValue, Nothing)
    End Sub


    Public Sub New(connectionLimit As Integer, maxIdleTime As Integer, connectionLeaseTimeout As Integer?)
        If connectionLimit < ConnectionLimit_MinimumValue Then
            Throw New ArgumentException(String.Format(WebConnectionSettingsResources.Error_InvalidConnectionLimit_Template, connectionLimit))
        End If
        mConnectionLimit = connectionLimit

        If maxIdleTime < MaxIdleTime_MinimumValue Then
            Throw New ArgumentException(String.Format(WebConnectionSettingsResources.Error_InvalidMaxIdleTime_Template, maxIdleTime))
        End If
        mMaxIdleTime = maxIdleTime

        If connectionLeaseTimeout.HasValue AndAlso connectionLeaseTimeout < ConnectionLeaseTimeout_MinimumValue Then
            Throw New ArgumentException(String.Format(WebConnectionSettingsResources.Error_InvalidConnectionLeaseTimeout_Template,
                                                      connectionLeaseTimeout))
        End If
        mConnectionLeaseTimeout = connectionLeaseTimeout

    End Sub

End Class
