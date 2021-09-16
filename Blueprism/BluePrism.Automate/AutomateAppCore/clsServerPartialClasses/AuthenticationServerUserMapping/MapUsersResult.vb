Imports System.Runtime.Serialization

Namespace clsServerPartialClasses.AuthenticationServerUserMapping

    <Serializable, DataContract([Namespace]:="bp")>
    Public Class MapUsersResult

        <DataMember>
        Private ReadOnly mErrorCode As MapUsersErrorCode

        <DataMember>
        Private ReadOnly mStatus As MapUsersStatus

        <DataMember>
        Private ReadOnly mSuccessfullyMappedRecordsCount As Integer

        <DataMember>
        Private ReadOnly mRecordsThatFailedToMap As List(Of UserMappingResult)

        Public Shared Function Completed(results As List(Of UserMappingResult)) As MapUsersResult
            Return New MapUsersResult(MapUsersErrorCode.None, results)
        End Function

        Public Shared Function Failed(errorCode As MapUsersErrorCode) As MapUsersResult
            Return New MapUsersResult(errorCode, Nothing)
        End Function

        Private Sub New(errorCode As MapUsersErrorCode, results As List(Of UserMappingResult))
            If errorCode = MapUsersErrorCode.None Then
                mRecordsThatFailedToMap = results.Where(Function(record) record.Status <> UserMappingStatus.Success).ToList()
                mSuccessfullyMappedRecordsCount = results.Where(Function(x) x.Status = UserMappingStatus.Success).Count
                mStatus = If(mRecordsThatFailedToMap.Any(), MapUsersStatus.CompletedWithErrors, MapUsersStatus.Completed)
            Else
                mErrorCode = errorCode
                mStatus = MapUsersStatus.Failed
                mRecordsThatFailedToMap = New List(Of UserMappingResult)()
            End If
        End Sub

        Public ReadOnly Property SuccessfullyMappedRecordsCount As Integer
            Get
                Return mSuccessfullyMappedRecordsCount
            End Get
        End Property

        Public ReadOnly Property RecordsThatFailedToMap As List(Of UserMappingResult)
            Get
                Return mRecordsThatFailedToMap
            End Get
        End Property

        Public ReadOnly Property Status As MapUsersStatus
            Get
                Return mStatus
            End Get
        End Property

        Public ReadOnly Property ErrorCode As MapUsersErrorCode
            Get
                Return mErrorCode
            End Get
        End Property
    End Class
End NameSpace
