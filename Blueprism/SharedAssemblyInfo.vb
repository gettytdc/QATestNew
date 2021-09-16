imports System.Reflection

' General Information about an assembly is controlled through the following
' set of attributes. Change these attribute values to modify the information
' associated with an assembly.
<assembly: AssemblyCompany("Blue Prism Limited")>
<assembly: AssemblyProduct("Blue Prism")>
<assembly: AssemblyCopyright("Copyright © Blue Prism Limited 2004 - 2020")>
<assembly: AssemblyTrademark("")>

'Make it easy to distinguish Debug and Release (i.e. Retail) builds;
' for example, through the file properties window.
#if DEBUG
<assembly: AssemblyConfiguration("Debug")>
#Else
<assembly: AssemblyConfiguration("Release")>
#End If


<Assembly: AssemblyVersion("6.0.0.0")>
<Assembly: AssemblyFileVersion("6.0.0.0")>
