Imports System.Runtime.Serialization

<Flags, DataContract([Namespace]:="bp")>
Public Enum TileTypes
    <EnumMember> Chart = 1
    '<EnumMember> Table = 2
    '<EnumMember> Browser = 4
End Enum
