Imports System.IO

Public Interface IConfigLocator
    ReadOnly Property LocationTypeName As String
    ReadOnly Property MachineConfigDirectory As DirectoryInfo
    ReadOnly Property UserConfigDirectory As DirectoryInfo
End Interface
