Imports System.Runtime.Serialization

Namespace clsServerPartialClasses.AuthenticationServerUserMapping
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class UserMappingResult
        <DataMember>
        Private ReadOnly mCode As UserMappingResultCode

        <DataMember>
        Private ReadOnly mStatus As UserMappingStatus

        <DataMember>
        Private ReadOnly mRecord As UserMappingRecord

        Private Sub New(userMappingRecord As UserMappingRecord, status As UserMappingStatus,
                        Optional code As UserMappingResultCode = UserMappingResultCode.None)
            mRecord = userMappingRecord
            mStatus = status
            mCode = code
        End Sub

        Public Shared Function Success(mappingRecord As UserMappingRecord) As UserMappingResult
            Return New UserMappingResult(mappingRecord, UserMappingStatus.Success)
        End Function

        Public Shared Function Failed(mappingRecord As UserMappingRecord, code As UserMappingResultCode) _
            As UserMappingResult
            Return New UserMappingResult(mappingRecord, UserMappingStatus.Failed, code)
        End Function

        Public ReadOnly Property ResultCode As UserMappingResultCode
            Get
                Return mCode
            End Get
        End Property

        Public ReadOnly Property Status As UserMappingStatus
            Get
                Return mStatus
            End Get
        End Property

        Public ReadOnly Property Record As UserMappingRecord
            Get
                Return mRecord
            End Get
        End Property
    End Class
End NameSpace
