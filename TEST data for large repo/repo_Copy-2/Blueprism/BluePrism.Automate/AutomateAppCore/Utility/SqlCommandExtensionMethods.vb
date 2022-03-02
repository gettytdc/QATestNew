Imports System.Data.SqlClient
Imports System.Runtime.CompilerServices
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.Server.Domain.Models.DataFilters
Imports BluePrism.Utilities.Functional

Namespace Utility
    Public Module SqlCommandExtensionMethods

        ''' <summary>
        ''' Returns a new parameterized SQL IN statement that can then be used in the 
        ''' command text for this sql command, with the parameters and values added 
        ''' to the command
        ''' </summary>
        ''' <typeparam name="T">
        ''' The type of values that will be included in the SQL IN statement
        ''' </typeparam>
        ''' <param name="cmd">
        ''' The sql command where the SQL IN statement will be used
        ''' </param>
        ''' <param name="columnName">
        ''' The name of the SQL column that the SQL IN statement applies to
        ''' </param>
        ''' <param name="values">
        ''' A collection of values that will be used in the SQL IN statment
        ''' </param>
        ''' <returns>
        ''' Returns a SQL IN statement such as, 
        ''' 'in (@param0, @param1, @param2....,@paramx)',  where each parameter 
        ''' relates to each of the values supplied. If <paramref name="values"/> is 
        ''' empty then this function will return <![CDATA[0<>0]]> instead.
        ''' </returns>
        <Extension()>
        Public Function BuildSqlInStatement(Of T)(cmd As SqlCommand, columnName As String,
                                                  values As IEnumerable(Of T)) As String
            Return BuildSqlInStatement(cmd, columnName, values, "param")
        End Function


        ''' <summary>
        ''' Returns a new parameterized SQL IN statement that can then be used in the 
        ''' command text for this sql command, with the parameters and values added 
        ''' to the command.
        ''' </summary>
        ''' <typeparam name="T">
        ''' The type of values that will be included in the SQL IN statement
        ''' </typeparam>
        ''' <param name="cmd">
        ''' The sql command where the SQL IN statement will be used
        ''' </param>
        ''' <param name="columnName">
        ''' The name of the SQL column that the SQL IN statement applies to
        ''' </param>
        ''' <param name="values">
        ''' A collection of values that will be used in the SQL IN statment
        ''' </param>
        ''' <param name="paramPrefix">The prefix used at the beginning of the 
        ''' paramater name. This can be used to ensure command parameter names are 
        ''' unique when building multiple SQL IN statements for a single sql command
        ''' </param>
        ''' <returns>
        ''' Returns a SQL IN statement such as, 
        ''' 'in (@param0, @param1, @param2....,@paramx)', where each parameter 
        ''' relates to each of the values supplied, and the parameter name is the 
        ''' parameter prefix supplied plus a number. <paramref name="values"/> is 
        ''' empty then this function will return <![CDATA[0<>0]]> instead.
        ''' </returns>
        <Extension()>
        Public Function BuildSqlInStatement(Of T)(cmd As SqlCommand, columnName As String,
                                                  values As IEnumerable(Of T),
                                                  paramPrefix As String) As String
            Return values.
                        Select(Function(value, index) New With {.paramName = $"@{paramPrefix}{index.ToString()}",
                                                                .paramValue = value}).
                        ForEach(Sub(x) cmd.Parameters.AddWithValue(x.paramName, x.paramValue)).
                        Select(Function(x) x.paramName).
                        Map(Function(x) String.Join(", ", x)).
                        Map(Function(params) If(params.Length = 0,
                                            "0 <> 0",
                                            $"{columnName} in ({params})"))

        End Function


        <Extension()>
        Public Function GetQueryAndSetParameters(mteQuery As IMteSqlGenerator, user As IUser, cmd As SqlCommand) As String

            If user.Roles.Any(Function(r) r.SystemAdmin OrElse r.RuntimeResource) Then
                Return mteQuery.BuildQueryString(cmd)
            End If

            ' If process visible in tree, then sessions running that process should be visible
            Dim processPermissions = Permission.ByName(Permission.ProcessStudio.AllProcessPermissionsAllowingTreeView)
            ' Replicates server-side check done when restricting sessions
            Dim resourcePermissions = Permission.ByName(Permission.Resources.ImpliedViewResource)
            Dim roles = user.Roles

            Return mteQuery.BuildQueryString(cmd,
                         roles.Select(Function(r) r.Id).ToList(),
                         resourcePermissions.Select(Function(r) r.Id).ToList(),
                         processPermissions.Select(Function(r) r.Id).ToList())
        End Function

        <Extension()>
        Public Function GetResourceQueryAndSetParameters(mteQuery As IMteResourceSqlGenerator, user As IUser, cmd As SqlCommand) As String

            If user.Roles.Any(Function(r) r.SystemAdmin OrElse r.RuntimeResource) Then
                Return mteQuery.ReplaceTokenAndAddParameters(cmd)
            End If

            Dim resourcePermissions = Permission.ByName(Permission.Resources.ImpliedViewResource)
            Dim roles = user.Roles

            Return mteQuery.ReplaceTokenAndAddParameters(cmd,
                         roles.Select(Function(r) r.Id).ToList(),
                         resourcePermissions.Select(Function(r) r.Id).ToList())
        End Function

    End Module

End Namespace
