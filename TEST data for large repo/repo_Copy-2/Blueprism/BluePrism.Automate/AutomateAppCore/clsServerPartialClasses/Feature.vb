Imports System.Runtime.Serialization

Namespace clsServerPartialClasses

    <DataContract([Namespace]:="bp")>
    Public Enum Feature
        <EnumMember> None = 0
        <EnumMember> DocumentProcessing
    End Enum
End NameSpace