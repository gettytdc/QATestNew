Imports System.Runtime.Serialization

<Serializable, DataContract([Namespace]:="bp")>
Public Class clsTableEncryption

#Region " Member Variables "

    <DataMember>
    Private mEncryptionId As Integer

    <DataMember>
    Private mTableName As String

#End Region

#Region " Properties "

    ''' <summary>
    ''' Internal database identifier of this encryption scheme (only valid for
    ''' schemes retrieved from the database).
    ''' </summary>
    Public ReadOnly Property EncryptionId() As Integer
        Get
            Return mEncryptionId
        End Get
    End Property

    ''' <summary>
    ''' The name of the tables that the database is being used by
    ''' </summary>
    Public ReadOnly Property Name() As String
        Get
            Return mTableName
        End Get
    End Property
    
    ''' <summary>
    ''' Text that describes the function of the table
    ''' </summary>
    Public ReadOnly Property [Function]() As String
        Get
            Return TableFunction(mTableName)
        End Get
    End Property

#End Region

#Region " Constructor "

    ''' <summary>
    ''' Constructor for use when retrieving encrypted tables from the database.
    ''' </summary>
    ''' <param name="id">Scheme ID</param>
    ''' <param name="name">Names of table the encrytion is used by</param>
    Public Sub New(id As Integer, name As String)
        mEncryptionId = id
        mTableName = name
    End Sub

#End Region


#Region "Methods"

    Private Shared Function TableFunction(tableName As String) As String
        Dim functionText = ""
        Select Case tableName
            Case "BPACredentials"
                functionText = My.Resources.Credentials
            Case "BPADataPipelineProcessConfig"
                functionText = My.Resources.DataPipelineProcessConfig
            Case "BPAKeyStore"
                functionText = My.Resources.KeyStore
            Case "BPAScreenshot"
                functionText = My.Resources.Screenshot
            Case "BPASysConfig"
                functionText = My.Resources.SysConfig
            Case "BPAWorkQueue"
                functionText = My.Resources.WorkQueue
            Case "BPAWorkQueueItem"
                functionText = My.Resources.WorkQueueItem
        End Select
        Return functionText
    End Function

#End Region
End Class
