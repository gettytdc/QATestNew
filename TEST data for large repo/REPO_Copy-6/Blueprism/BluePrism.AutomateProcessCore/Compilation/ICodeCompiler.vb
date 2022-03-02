Namespace Compilation

    ''' <summary>
    ''' Compiles code at runtime into an assembly
    ''' </summary>
    Public Interface ICodeCompiler 
        Inherits IDisposable

        ''' <summary>
        ''' Compiles all of the code in the specified assembly data into an assembly.
        ''' </summary>
        ''' <param name="assemblyDefinition">Defines the code to be added to the 
        ''' assembly</param>
        ''' <param name="cache">An optional cache used to store and retrieve compiled 
        ''' assemblies based on the generated source code</param>
        ''' <returns>A <see cref="CompiledCodeResult"/> object containing the result of the 
        ''' compilation</returns>
        Function Compile(assemblyDefinition As AssemblyDefinition, cache As IObjectCache) As CompiledCodeResult

    End Interface
End NameSpace