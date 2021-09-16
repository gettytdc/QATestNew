namespace BluePrism.Caching
{
    using System;

    public delegate void OnRefreshRequiredDelegate(object sender, EventArgs e);

    public interface IRefreshCache<in TKey, TValue> : ICache<TKey, TValue> where TKey : IEquatable<TKey>
    {
        event OnRefreshRequiredDelegate OnRefreshRequired;
    }
}