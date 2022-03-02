namespace BluePrism.Core.ActiveDirectory.DirectoryServices
{
    public interface ISearchResult
    {
        string DistinguishedName { get; }
        string Sid { get; }
        string UserPrincipalName { get; }
    }
}