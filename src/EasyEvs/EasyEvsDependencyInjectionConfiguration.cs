namespace EasyEvs;

using System;
using System.Reflection;
using Contracts;
using Internal;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Configuration for the <see cref="ServiceCollectionExtensions.AddEasyEvs"/>
/// </summary>
public class EasyEvsDependencyInjectionConfiguration
{
    /// <summary>
    /// If set, the implementation to use to provide the json configuration for the <see cref="ISerializer"/>
    /// </summary>
    public Type? JsonSerializerOptionsProvider { get; set; }

    /// <summary>
    /// If set, the implementation to use to provide the json configuration for the <see cref="IStreamResolver"/>
    /// </summary>
    public Type? StreamResolver { get; set; }

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

    /// <summary>
    /// Defines if the default stream resolver with tagged <see cref="AggregateAttribute{T}"/> is used.
    /// </summary>
    public bool DefaultStreamResolver { get; set; } = false;
}
