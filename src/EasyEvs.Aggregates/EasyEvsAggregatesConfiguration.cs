namespace EasyEvs;

using System;
using Contracts;

/// <summary>
/// Configuration for the <see cref="ServiceCollectionExtensions.AddEasyEvsAggregates"/>
/// </summary>
public class EasyEvsAggregatesConfiguration
{
    /// <summary>
    /// If set, the implementation to use for the <see cref="IAggregateStreamResolver"/>
    /// </summary>
    public Type? AggregateStreamResolver { get; set; }
}
