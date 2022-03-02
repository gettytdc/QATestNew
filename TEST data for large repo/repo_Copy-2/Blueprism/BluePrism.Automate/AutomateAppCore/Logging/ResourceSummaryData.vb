
Imports System.Runtime.Serialization
Imports BluePrism.AutomateProcessCore
Imports BluePrism.Core.Resources

Namespace Logging
    <Serializable>
    <DataContract([Namespace]:="bp")>
    Public Class ResourceSummaryData

        <DataMember>
        Private ReadOnly mName As String

        <DataMember>
        Private ReadOnly mFullyQualifiedDomainName As String

        <DataMember>
        Private ReadOnly mResourceId As String

        <DataMember>
        Private ReadOnly mAttributeId As ResourceAttribute

        <DataMember>
        Private ReadOnly mPool As String

        <DataMember>
        Private ReadOnly mIsController As Boolean

        <DataMember>
        Private ReadOnly mUserId As String

        <DataMember>
        Private ReadOnly mLoggingLevel As clsAPC.Diags

        <DataMember>
        Private ReadOnly mSchedules As String

        Public Sub New(name As String, fullyQualifiedDomainName As String,
                       resourceId As String, attributeId As ResourceAttribute,
                       pool As String, controller As String,
                       userId As String, logginglevel As clsAPC.Diags,
                       schedules As String)

            mName = name
            mFullyQualifiedDomainName = fullyQualifiedDomainName
            mResourceId = resourceId
            mAttributeId = attributeId
            mPool = pool
            mIsController = (controller = resourceId)
            mUserId = userId
            mLoggingLevel = logginglevel
            mSchedules = schedules

        End Sub

        Public ReadOnly Property Name As String
            Get
                Return mName
            End Get
        End Property

        Public ReadOnly Property FullyQualifiedDomainName As String
            Get
                Return mFullyQualifiedDomainName
            End Get
        End Property

        Public ReadOnly Property ResourceId As String
            Get
                Return mResourceId
            End Get
        End Property

        Public ReadOnly Property AttributeId As ResourceAttribute
            Get
                Return mAttributeId
            End Get
        End Property

        Public ReadOnly Property Pool As String
            Get
                Return mPool
            End Get
        End Property

        Public ReadOnly Property IsController As Boolean
            Get
                Return mIsController
            End Get
        End Property

        Public ReadOnly Property UserId As String
            Get
                Return mUserId
            End Get
        End Property

        Public ReadOnly Property LoggingLevel As clsAPC.Diags
            Get
                Return mLoggingLevel
            End Get
        End Property

        Public ReadOnly Property Schedules As String
            Get
                Return mSchedules
            End Get
        End Property

    End Class
End Namespace