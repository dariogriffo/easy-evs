namespace EasyEvs
{
    using System;
    using System.Reflection;
    using Contracts;
    using Internal;

    /// <summary>
    /// Configuration for the <see cref="ServiceCollectionExtensions.AddEasyEvs"/>
    /// </summary>
    public struct EasyEvsDependencyInjectionConfiguration
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
    }
}
