Imports System.Reflection
Imports System.Linq
Imports System.Runtime.Serialization
Imports BluePrism.BPCoreLib
Imports System.Text.RegularExpressions
Imports System.Collections.Concurrent

#Region "Attributes"

''' <summary>
''' Class attribute to indicate an external dependency, i.e. refers to something
''' defined outside of the process, and is backed by a corresponding database table.
''' Where the dependency relates to an Object or Process then the RunMode property
''' should be set so that the effective run mode of a process can be evaluated when
''' creating sessions.
''' </summary>
Friend Class ExternalDependency
    Inherits Attribute
    Public Property RunMode() As Boolean = False
End Class

#End Region

#Region "Dependency list class"

''' <summary>
''' Class providing a collection of dependencies within a given process.
''' </summary>
<Serializable>
<DataContract([Namespace]:="bp")>
Public Class clsProcessDependencyList

    'The run mode
    <DataMember()>
    Public Property RunMode() As BusinessObjectRunMode = BusinessObjectRunMode.Exclusive

    'Shared flag
    <DataMember()>
    Public Property IsShared() As Boolean = False

    'The collection of referenced items
    Public ReadOnly Property Dependencies() As IList(Of clsProcessDependency)
        Get
            Return mDependencies
        End Get
    End Property
    <DataMember()>
    Private mDependencies As New List(Of clsProcessDependency)

    ''' <summary>
    ''' Check if this dependency list already has a duplicate of the given
    ''' dependency.
    ''' </summary>
    ''' <param name="dep">The dependency to check against.</param>
    ''' <returns>True if the dependency exists in the list.</returns>
    Public Function Has(dep As clsProcessDependency) As Boolean
        For Each exdep As clsProcessDependency In mDependencies
            If dep.Duplicates(exdep) Then
                Return True
            End If
        Next
        Return False
    End Function

    ''' <summary>
    ''' Check if this dependency list already has a duplicate of the given
    ''' dependency, and if so return it.
    ''' </summary>
    ''' <param name="dep">The dependency to check against.</param>
    ''' <returns>The existing dependency, or nothing.</returns>
    Public Function Find(dep As clsProcessDependency) As clsProcessDependency
        For Each exdep As clsProcessDependency In mDependencies
            If dep.Duplicates(exdep) Then
                Return exdep
            End If
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Add a dependency to the collection. This method is used for creating a list
    ''' of dependencies from the database backing tables (i.e. where stage
    ''' details are not known).
    ''' </summary>
    ''' <param name="dep">The dependency to add.</param>
    Public Sub Add(dep As clsProcessDependency)
        'Add dependency if it is not a duplicate
        If Find(dep) Is Nothing Then
            mDependencies.Add(dep)
        End If
    End Sub

    ''' <summary>
    ''' Merges the passed list of dependencies with this one.
    ''' </summary>
    ''' <param name="deps"></param>
    Public Sub Merge(deps As clsProcessDependencyList)
        For Each d As clsProcessDependency In deps.Dependencies
            Add(d)
        Next
    End Sub

    ''' <summary>
    ''' Add a dependency, or (if it already exists) add the stage reference to it.
    ''' This method is used for creating a list of dependencies from a process (i.e.
    ''' where stage information is required).
    ''' </summary>
    ''' <param name="dep">The dependency to add.</param>
    ''' <param name="stg">The stage that references it.</param>
    Public Sub Add(dep As clsProcessDependency, stg As Guid)
        Dim exdep As clsProcessDependency = Find(dep)
        If exdep IsNot Nothing Then
            'Dependency already exists, so add stage to it
            exdep.AddStage(stg)
        Else
            'Dependency does not exist so add it
            dep.AddStage(stg)
            mDependencies.Add(dep)
        End If
    End Sub

    ''' <summary>
    ''' Returns a list of all dependencies of the passed type
    ''' </summary>
    ''' <param name="type">The type required</param>
    ''' <returns>List of dependencies of the passed type</returns>
    Public Function GetDependencies(type As String) As IList(Of clsProcessDependency)
        Return mDependencies.FindAll(Function(d) d.TypeName = type)
    End Function

End Class

#End Region

#Region "Dependency base class"

''' <summary>
''' This is the base class for all the different kinds of dependency we track.
''' 
''' Adding a new type of dependency: If you wanted to do this, you'd need to do the
''' following:
'''    1. Create a new derived class below, clsProcessXXXDependency
'''    2. If required mark it with the ExternalDependency attribute
'''    3. Ensure its relevant values are marked up with the DependencyValue attribute
'''    4. Create a corresponding database table
''' Note that the naming of all this stuff is important. Database storage, sync
''' and retreival will be handled correctly and automatically so long as you follow
''' the rules.
''' </summary>
<Serializable>
<DataContract([Namespace]:="bp")>
<KnownType(GetType(clsProcessIDDependency))>
<KnownType(GetType(clsProcessNameDependency))>
<KnownType(GetType(clsProcessParentDependency))>
<KnownType(GetType(clsProcessActionDependency))>
<KnownType(GetType(clsProcessElementDependency))>
<KnownType(GetType(clsProcessWebServiceDependency))>
<KnownType(GetType(clsProcessWebApiDependency))>
<KnownType(GetType(clsProcessQueueDependency))>
<KnownType(GetType(clsProcessCredentialsDependency))>
<KnownType(GetType(clsProcessEnvironmentVarDependency))>
<KnownType(GetType(clsProcessCalendarDependency))>
<KnownType(GetType(clsProcessFontDependency))>
<KnownType(GetType(clsProcessPageDependency))>
<KnownType(GetType(clsProcessDataItemDependency))>
<KnownType(GetType(clsProcessSkillDependency))>
Public MustInherit Class clsProcessDependency

    ''' <summary>
    ''' All properties of subclasses that are 'values' of the dependency need to be
    ''' annotated with this Attribute.
    ''' </summary>
    Friend Class DependencyValue
        Inherits Attribute
    End Class

    ''' <summary>
    ''' The database ID for this dependency, which is unique only among the
    ''' particular type of dependency this is. This is only valid if the list has
    ''' actually come from the database - otherwise it will be 0.
    ''' </summary>
    Public ReadOnly Property ID() As Integer
        Get
            Return mID
        End Get
    End Property
    <DataMember()>
    Friend mID As Integer = 0

    ''' <summary>
    ''' The stage IDs for this dependency. This is not used when identifying external
    ''' dependencies for storing in the database (since we only need to know that the
    ''' dependency exists), it is however needed when viewing references within a
    ''' particular process (e.g. "which stages reference this data item?").
    ''' </summary>
    Public ReadOnly Property StageIDs() As List(Of Guid)
        Get
            Return mStageIDs
        End Get
    End Property
    <DataMember()>
    Protected mStageIDs As New List(Of Guid)

    Private Const mCP As String = "BluePrism.AutomateProcessCore.cls"

    ''' <summary>
    ''' Create an instance of a clsProcessDependency subclass.
    ''' </summary>
    ''' <param name="type">The type of dependency, which must be one of the values
    ''' returned from the Types() property.</param>
    ''' <param name="values">The type-specific values. The first is always the
    ''' database ID (an integer), while the others are as expected to be those
    ''' returned by the ValueNames property, and in the same order.</param>
    ''' <returns>The created instance.</returns>
    Public Shared Function Create(type As String, values As Object()) As clsProcessDependency
        Dim t As Type = Assembly.GetExecutingAssembly.GetType(mCP & type)
        Dim dep As clsProcessDependency
        dep = CType(Activator.CreateInstance(t, {}), clsProcessDependency)
        dep.mID = CInt(values(0))
        Dim vnames As ISet(Of String) = ValueNames(type)
        For i As Integer = 0 To vnames.Count() - 1
            Dim f As FieldInfo = dep.GetType().GetField("m" & vnames(i),
                            BindingFlags.NonPublic Or BindingFlags.Instance)
            f.SetValue(dep, values(i + 1))
        Next
        Return dep
    End Function

    ''' <summary>A list of the different types of external dependency that are defined.
    ''' For example, "ProcessNameDependency", "ProcessElementDependency", ...
    ''' </summary>
    Public Shared ReadOnly Property ExternalTypes() As IList(Of String)
        Get
            If mCachedExternalTypes Is Nothing Then
                Dim tlist As List(Of Type) = Assembly.GetExecutingAssembly.GetTypes().Where(
                    Function(t) t IsNot GetType(clsProcessDependency) AndAlso
                        t.IsSubclassOf(GetType(clsProcessDependency)) AndAlso
                        t.GetCustomAttributes(GetType(ExternalDependency), True).Length > 0).ToList()
                mCachedExternalTypes = tlist.Select(Function(s) s.ToString().Substring(mCP.Length())).ToList()
            End If
            Return mCachedExternalTypes
        End Get
    End Property
    Private Shared mCachedExternalTypes As List(Of String) = Nothing

    ''' <summary>A list of the different types of external dependency that are used
    ''' to calculate the effective run mode (i.e. those that relate only to Processes
    ''' (or Objects).
    ''' </summary>
    Public Shared ReadOnly Property RunModeTypes As IList(Of String)
        Get
            If mCachedRunModeTypes Is Nothing Then
                Dim tlist As List(Of Type) = Assembly.GetExecutingAssembly.GetTypes().Where(
                    Function(t) t IsNot GetType(clsProcessDependency) AndAlso
                        t.IsSubclassOf(GetType(clsProcessDependency)) AndAlso
                        t.GetCustomAttributes(GetType(ExternalDependency), True).Where(
                            Function(p) CType(p, ExternalDependency).RunMode).Count > 0).ToList()
                mCachedRunModeTypes = tlist.Select(Function(s) s.ToString().Substring(mCP.Length())).ToList()
            End If
            Return mCachedRunModeTypes
        End Get
    End Property
    Private Shared mCachedRunModeTypes As List(Of String) = Nothing

    ''' <summary>The name of the type of dependency this instance is. Will be one of
    ''' the values returned from Types().
    ''' </summary>
    Public ReadOnly Property TypeName() As String
        Get
            Return Me.GetType().ToString().Substring(mCP.Length())
        End Get
    End Property

    ''' <summary>A list of value names for a particular type of dependency, where the
    ''' dependency type is one of the values returned by Types().
    ''' </summary>
    Public Shared ReadOnly Property ValueNames(type As String) As ISet(Of String)
        Get

            Dim lst As New SortedSet(Of String)
            If CachedValueNames.TryGetValue(type, lst) Then
                Return lst
            End If

            lst = New SortedSet(Of String)
            For Each p As PropertyInfo In Assembly.GetExecutingAssembly.GetType(mCP & type).GetProperties()
                If p.GetCustomAttributes(False).Any(Function(a) a.GetType() Is GetType(DependencyValue)) Then
                    lst.Add(p.Name)
                End If
            Next
            CachedValueNames(type) = lst
            Return lst
        End Get
    End Property
    Private Shared ReadOnly CachedValueNames As New ConcurrentDictionary(Of String, SortedSet(Of String))

    ''' <summary>
    ''' Get the values for this dependency.
    ''' </summary>
    ''' <returns>A dictionary of the values (as Object), keyed on the value name. 
    ''' This will contain all the same values returned by ValueNames().</returns>
    Public Function GetValues() As IDictionary(Of String, Object)
        Dim vals As New Dictionary(Of String, Object)
        For Each p As PropertyInfo In Me.GetType().GetProperties()
            If p.GetCustomAttributes(False).Any(Function(a) a.GetType() Is GetType(DependencyValue)) Then
                vals(p.Name) = p.GetValue(Me, Nothing)
            End If
        Next
        Return vals
    End Function

    ''' <summary>
    ''' Check if the given dependency duplicates one already in this list.
    ''' </summary>
    ''' <param name="obj">A subclass of clsProcessDependency to check.</param>
    ''' <returns>True if this list already contains a dependency with the same set
    ''' of values.</returns>
    Public Function Duplicates(obj As clsProcessDependency) As Boolean
        'Check if dependecies are same type
        If Me.GetType() IsNot obj.GetType() Then Return False

        'Check if all dependency values match
        For Each p As PropertyInfo In Me.GetType().GetProperties()
            If p.GetCustomAttributes(False).Any(Function(a) a.GetType() Is GetType(DependencyValue)) Then
                If Not p.GetValue(Me, Nothing).Equals(p.GetValue(obj, Nothing)) Then
                    Return False
                End If
            End If
        Next

        'Must be a duplicate
        Return True
    End Function

    ''' <summary>
    ''' Adds a stage to an existing dependency, providing it isn't present already.
    ''' </summary>
    ''' <param name="stg">The stage ID</param>
    Public Sub AddStage(stg As Guid)
        If Not mStageIDs.Contains(stg) Then
            mStageIDs.Add(stg)
        End If
    End Sub

    ''' <summary>
    ''' Returns a friendly name for this dependency object.
    ''' </summary>
    Public Function GetFriendlyName() As String
        For Each attr As Object In Me.GetType().GetCustomAttributes(False)
            If attr.GetType() Is GetType(FriendlyNameAttribute) Then
                Return CType(attr, FriendlyNameAttribute).Name
            End If
        Next
        Return String.Empty
    End Function

    ''' <summary>
    ''' Returns a localised friendly name for this dependency object.
    ''' </summary>
    Public Function GetLocalizedFriendlyName() As String
        Dim Attribute As String = Me.GetFriendlyName()
        Dim resxKey = "DependencyDefinitionAttribute_" & Regex.Replace(Attribute, " ", "")
        Dim res As String = My.Resources.Resources.ResourceManager.GetString($"{resxKey}")
        Return CStr(IIf(res Is Nothing, Attribute, res))
    End Function
End Class

Public Interface IIdBasedDependency
    Property Name As String
End Interface

#End Region

#Region "External dependency inner classes"

<ExternalDependency(RunMode:=True)>
<FriendlyName("Process")>
<Serializable>
<DataContract([Namespace]:="bp")>
Public Class clsProcessIDDependency
    Inherits clsProcessDependency
    Implements IIdBasedDependency

    <DependencyValue>
    Public ReadOnly Property RefProcessID() As Guid
        Get
            Return mRefProcessID
        End Get
    End Property
    <DataMember()>
    Private mRefProcessID As Guid

    <DataMember()>
    Public Property Name As String Implements IIdBasedDependency.Name

    Public Sub New()
    End Sub

    Public Sub New(refProcessID As Guid, Optional procName As String = Nothing)
        mRefProcessID = refProcessID
        Name = procName
    End Sub

End Class

<ExternalDependency(RunMode:=True)>
<FriendlyName("Object")>
<Serializable>
<DataContract([Namespace]:="bp")>
Public Class clsProcessNameDependency
    Inherits clsProcessDependency

    <DependencyValue>
    Public ReadOnly Property RefProcessName() As String
        Get
            Return mRefProcessName
        End Get
    End Property
    <DataMember()>
    Private mRefProcessName As String

    Public Sub New()
    End Sub

    Public Sub New(refProcessName As String)
        mRefProcessName = refProcessName
    End Sub

End Class

<ExternalDependency>
<FriendlyName("Parent Object")>
<Serializable>
<DataContract([Namespace]:="bp")>
Public Class clsProcessParentDependency
    Inherits clsProcessDependency

    <DependencyValue>
    Public ReadOnly Property RefParentName() As String
        Get
            Return mRefParentName
        End Get
    End Property
    <DataMember()>
    Private mRefParentName As String

    Public Sub New()
    End Sub

    Public Sub New(refParentName As String)
        mRefParentName = refParentName
    End Sub

End Class

<ExternalDependency>
<FriendlyName("Object Action")>
<Serializable>
<DataContract([Namespace]:="bp")>
Public Class clsProcessActionDependency
    Inherits clsProcessDependency

    <DependencyValue>
    Public ReadOnly Property RefProcessName() As String
        Get
            Return mRefProcessName
        End Get
    End Property
    <DataMember()>
    Private mRefProcessName As String

    <DependencyValue>
    Public ReadOnly Property RefActionName() As String
        Get
            Return mRefActionName
        End Get
    End Property
    <DataMember()>
    Private mRefActionName As String

    Public Sub New()
    End Sub

    Public Sub New(refProcessName As String, refActionName As String)
        mRefProcessName = refProcessName
        mRefActionName = refActionName
    End Sub

End Class

<ExternalDependency>
<FriendlyName("Model Element")>
<Serializable>
<DataContract([Namespace]:="bp")>
Public Class clsProcessElementDependency
    Inherits clsProcessDependency

    <DependencyValue>
    Public ReadOnly Property RefProcessName() As String
        Get
            Return mRefProcessName
        End Get
    End Property
    <DataMember()>
    Private mRefProcessName As String

    <DependencyValue>
    Public ReadOnly Property RefElementID() As Guid
        Get
            Return mRefElementID
        End Get
    End Property
    <DataMember()>
    Private mRefElementID As Guid

    Public Sub New()
    End Sub

    Public Sub New(refProcessName As String, refElementID As Guid)
        mRefProcessName = refProcessName
        mRefElementID = refElementID
    End Sub

End Class

<ExternalDependency>
<FriendlyName("Web Service")>
<Serializable>
<DataContract([Namespace]:="bp")>
Public Class clsProcessWebServiceDependency
    Inherits clsProcessDependency

    <DependencyValue>
    Public ReadOnly Property RefServiceName() As String
        Get
            Return mRefServiceName
        End Get
    End Property
    <DataMember()>
    Private mRefServiceName As String

    Public Sub New()
    End Sub

    Public Sub New(refServiceName As String)
        mRefServiceName = refServiceName
    End Sub

End Class

<ExternalDependency>
<FriendlyName("Web API")>
<Serializable>
<DataContract([Namespace]:="bp")>
Public Class clsProcessWebApiDependency
    Inherits clsProcessDependency

    <DependencyValue>
    Public ReadOnly Property RefApiName() As String
        Get
            Return mRefApiName
        End Get
    End Property
    <DataMember()>
    Private mRefApiName As String

    Public Sub New()
    End Sub

    Public Sub New(refApiName As String)
        mRefApiName = refApiName
    End Sub

End Class

<ExternalDependency>
<FriendlyName("Work Queue")>
<Serializable>
<DataContract([Namespace]:="bp")>
Public Class clsProcessQueueDependency
    Inherits clsProcessDependency

    <DependencyValue>
    Public ReadOnly Property RefQueueName() As String
        Get
            Return mRefQueueName
        End Get
    End Property
    <DataMember()>
    Private mRefQueueName As String

    Public Sub New()
    End Sub

    Public Sub New(refQueueName As String)
        mRefQueueName = refQueueName
    End Sub

End Class

<ExternalDependency>
<FriendlyName("Credentials")>
<Serializable>
<DataContract([Namespace]:="bp")>
Public Class clsProcessCredentialsDependency
    Inherits clsProcessDependency

    <DependencyValue>
    Public ReadOnly Property RefCredentialsName() As String
        Get
            Return mRefCredentialsName
        End Get
    End Property
    <DataMember()>
    Private mRefCredentialsName As String

    <DataMember()>
    Public Property GatewaysCredential() As Boolean
        Get
            Return mGatewaysCredential
        End Get
        Set(value As Boolean)
            mGatewaysCredential = value
        End Set
    End Property

    <DataMember()>
    Private mGatewaysCredential As Boolean

    <DataMember()>
    Public Property SharedCredential() As Boolean
        Get
            Return mSharedCredential
        End Get
        Set(value As Boolean)
            mSharedCredential = value
        End Set
    End Property

    <DataMember()>
    Private mSharedCredential As Boolean

    Public Sub New()
    End Sub

    Public Sub New(refCredentialsName As String)
        mRefCredentialsName = refCredentialsName
    End Sub

End Class

<ExternalDependency>
<FriendlyName("Environment Variable")>
<Serializable>
<DataContract([Namespace]:="bp")>
Public Class clsProcessEnvironmentVarDependency
    Inherits clsProcessDependency

    <DependencyValue>
    Public ReadOnly Property RefVariableName() As String
        Get
            Return mRefVariableName
        End Get
    End Property
    <DataMember()>
    Private mRefVariableName As String

    Public Sub New()
    End Sub

    Public Sub New(refVariableName As String)
        mRefVariableName = refVariableName
    End Sub

End Class

<ExternalDependency>
<FriendlyName("Calendars")>
<Serializable>
<DataContract([Namespace]:="bp")>
Public Class clsProcessCalendarDependency
    Inherits clsProcessDependency

    <DependencyValue>
    Public ReadOnly Property RefCalendarName() As String
        Get
            Return mRefCalendarName
        End Get
    End Property
    <DataMember()>
    Private mRefCalendarName As String

    Public Sub New()
    End Sub

    Public Sub New(refCalendarName As String)
        mRefCalendarName = refCalendarName
    End Sub

End Class

<ExternalDependency>
<FriendlyName("Fonts")>
<Serializable>
<DataContract([Namespace]:="bp")>
Public Class clsProcessFontDependency
    Inherits clsProcessDependency

    <DependencyValue>
    Public ReadOnly Property RefFontName() As String
        Get
            Return mRefFontName
        End Get
    End Property
    <DataMember()>
    Private mRefFontName As String

    Public Sub New()
    End Sub

    Public Sub New(refFontName As String)
        mRefFontName = refFontName
    End Sub

End Class

<ExternalDependency>
<FriendlyName("Skill")>
<Serializable>
<DataContract([Namespace]:="bp")>
Public Class clsProcessSkillDependency
    Inherits clsProcessDependency
    Implements IIdBasedDependency

    <DependencyValue>
    Public ReadOnly Property RefSkillId As Guid
        Get
            Return mRefSkillId
        End Get
    End Property

    <DataMember(Name:="n")>
    Public Property Name As String Implements IIdBasedDependency.Name

    <DataMember(EmitDefaultValue:=False, Name:="id")>
    Private mRefSkillId As Guid

    Public Sub New()
    End Sub

    Public Sub New(refSkillId As Guid, Optional skillName As String = Nothing)
        mRefSkillId = refSkillId
        Name = skillName
    End Sub

End Class

#End Region

#Region "Internal dependency inner classes"

<FriendlyName("Page")>
<Serializable>
<DataContract([Namespace]:="bp")>
Public Class clsProcessPageDependency
    Inherits clsProcessDependency

    <DependencyValue>
    Public ReadOnly Property RefPageID() As Guid
        Get
            Return mRefPageID
        End Get
    End Property
    <DataMember()>
    Private mRefPageID As Guid

    Public Sub New()
    End Sub

    Public Sub New(refPageID As Guid)
        mRefPageID = refPageID
    End Sub

End Class

<FriendlyName("Data item")>
<Serializable>
<DataContract([Namespace]:="bp")>
Public Class clsProcessDataItemDependency
    Inherits clsProcessDependency

    <DependencyValue>
    Public ReadOnly Property RefDataItemName() As String
        Get
            Return mRefDataItemName
        End Get
    End Property

    <DataMember()>
    Private mRefDataItemName As String

    <DependencyValue>
    Public ReadOnly Property RefSheetId() As Guid
        Get
            Return mRefSheetId
        End Get
    End Property

    <DataMember()>
    Private mRefSheetId As Guid

    Public Sub New()
    End Sub

    Public Sub New(refDataItemName As String)
        mRefDataItemName = refDataItemName
    End Sub

    Public Sub New(stage As clsProcessStage)
        mRefDataItemName = stage.Name
        mRefSheetId = stage.GetSubSheetID
    End Sub

End Class

#End Region
