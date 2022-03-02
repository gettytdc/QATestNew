Namespace Groups
    ''' <summary>
    ''' Outlines the ability for a GroupMember to query its own properties and 
    ''' on demand load data about itself.
    ''' </summary>
    Public Interface IMemberMetaData
        ''' <summary>
        ''' Check if the any of the basic information about this member is default and so 
        ''' yet to be pulled from the database
        ''' </summary>
        ReadOnly Property HasMetaData As Boolean

        Sub UpdateMetaInfo(info As ProcessMetaInfo)
    End Interface
End Namespace