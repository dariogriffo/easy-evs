namespace EasyEvs.Contracts;

using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Configuration for EasyEvs
/// </summary>
public class EasyEvsConfiguration
{
    /// <summary>
    /// If set, the implementation to use to provide the json configuration for the <see cref="IJsonSerializerOptionsProvider"/>
    /// </summary>
    public Type? JsonSerializerOptionsProviderType { get; set; }

    /// <summary>
    /// If set, the implementation to use for the <see cref="IEventsStreamResolver"/>
    /// Not required you don't plan to manually Append events via <see cref="IEventStore.Append{T}(string,T,System.Threading.CancellationToken)"/>
    /// </summary>
    public Type? EventsStreamResolverType { get; set; }

    /// <summary>
    /// If set, the implementation to use for the <see cref="IConnectionStrategy"/>
    /// </summary>
    public Type? ReconnectionStrategyType { get; set; }

    /// <summary>
    /// The assemblies to scan for <see cref="IHandlesEvent{T}"/>
    /// </summary>
    public Assembly[]? AssembliesToScanForHandlers { get; set; }

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

    /// <summary>
    /// Configure if the events don't have a registered handler to log a warning message and Park them
    /// </summary>
    public bool TreatMissingHandlersAsErrors { get; set; } = false;
}
