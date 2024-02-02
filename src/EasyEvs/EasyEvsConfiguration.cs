namespace EasyEvs;

using System;
using System.Reflection;
using Contracts;
using Internal;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Configuration for the <see cref="ServiceCollectionExtensions.AddEasyEvs"/>
/// </summary>
public class EasyEvsConfiguration
{
    /// <summary>
    /// If set, the implementation to use to provide the json configuration for the <see cref="ISerializer"/>
    /// </summary>
    public Type? JsonSerializerOptionsProviderType { get; set; }

    /// <summary>
    /// If set, the implementation to use for the <see cref="IEventsStreamResolver"/>
    /// Not required you don't plan to manually Append events via <see cref="IEventStore.Append{T}(string,T,System.Threading.CancellationToken)"/>
    /// If you plan to <see cref="UseAggregates"/> you can skip this
    /// </summary>
    public Type? EventsStreamResolverType { get; set; }

    /// <summary>
    /// If set to true <see cref="IAggregateStreamResolver"/> must be set to allow <see cref="Aggregate"/>
    /// subclasses to use the Aggregate Store
    /// </summary>
    public bool UseAggregates { get; set; }

    /// <summary>
    /// If set, the implementation to use for the <see cref="IAggregateStreamResolver"/>
    /// Only used if <see cref="UseAggregates"/> is true
    /// </summary>
    public Type? AggregateStreamResolver { get; set; }

    /// <summary>
    /// If set, the implementation to use for the <see cref="IRetryStrategy"/>
    /// </summary>
    public Type? ConnectionRetryStrategyType { get; set; }

    /// <summary>
    /// The assemblies to scan for <see cref="IHandlesEvent{T}"/>
    /// </summary>
    public Assembly[]? Assemblies { get; set; }

    /// <summary>
    /// The default life time for auto registered <see cref="IHandlesEvent{T}"/>
    /// </summary>
    public ServiceLifetime DefaultHandlesLifetime { get; set; } = ServiceLifetime.Scoped;

    /// <summary>
    /// The default life time for auto registered <see cref="IPostHandlesEventAction{T}"/>
    /// </summary>
    public ServiceLifetime DefaultPostActionsLifetime { get; set; } = ServiceLifetime.Scoped;

    /// <summary>
    /// The default life time for auto registered <see cref="IPreHandlesEventAction{T}"/>
    /// </summary>
    public ServiceLifetime DefaultPreActionsLifetime { get; set; } = ServiceLifetime.Scoped;
}
