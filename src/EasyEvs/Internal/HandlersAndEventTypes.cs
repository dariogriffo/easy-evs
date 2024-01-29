namespace EasyEvs.Internal;

using System;
using System.Collections.Generic;
using System.Linq;
using Contracts.Internal;
using Microsoft.Extensions.DependencyInjection;

internal class HandlersAndEventTypes
{
    private Lazy<IReadOnlyDictionary<Type, Type>> _handlers = null!;
    private Lazy<IReadOnlyDictionary<Type, Type>> _pre = null!;
    private Lazy<IReadOnlyDictionary<Type, Type>> _post = null!;
    private Lazy<IReadOnlyDictionary<Type, Type>> _pipelines = null!;

    internal HandlersAndEventTypes(IServiceCollection services)
    {
        ScanServices(services);
    }

    private bool Implements(Type i, Type attributeType)
    {
        Type[] interfaces = i.GetInterfaces()
            .Where(x => x.Namespace == "EasyEvs.Contracts")
            .ToArray();
        return interfaces.Any(
            i => i.GetCustomAttributes(true).Any(a => a.GetType() == attributeType)
        );
    }

    private void ScanServices(IServiceCollection services)
    {
        _handlers = new Lazy<IReadOnlyDictionary<Type, Type>>(
            () =>
                services
                    .Where(
                        x =>
                            x.ImplementationType is not null
                            && Implements(x.ImplementationType, typeof(HandlesEventAttribute))
                    )
                    .Where(i => !i.ImplementationType!.IsAbstract)
                    .ToDictionary(
                        x => x.ServiceType.GenericTypeArguments.First(),
                        x => x.ServiceType
                    )
        );

        _pre = new Lazy<IReadOnlyDictionary<Type, Type>>(
            () =>
                services
                    .Where(
                        x =>
                            x.ImplementationType is not null
                            && Implements(x.ImplementationType, typeof(PreActionEventAttribute))
                    )
                    .Where(i => !i.ImplementationType!.IsAbstract)
                    .GroupBy(x => x.ServiceType.GenericTypeArguments.First())
                    .ToDictionary(x => x.Key, x => x.First().ServiceType)
        );

        _post = new Lazy<IReadOnlyDictionary<Type, Type>>(
            () =>
                services
                    .Where(
                        x =>
                            x.ImplementationType is not null
                            && Implements(x.ImplementationType, typeof(PostHandlerEventAttribute))
                    )
                    .Where(i => !i.ImplementationType!.IsAbstract)
                    .GroupBy(x => x.ServiceType.GenericTypeArguments.First())
                    .ToDictionary(x => x.Key, x => x.First().ServiceType)
        );

        ScanPipelines(services);
    }

    internal void ScanPipelines(IServiceCollection services)
    {
        _pipelines = new Lazy<IReadOnlyDictionary<Type, Type>>(
            () =>
                services
                    .Where(
                        x =>
                            x.ImplementationType is not null
                            && Implements(x.ImplementationType, typeof(PipelineHandlerAttribute))
                    )
                    .Where(i => !i.ImplementationType!.IsAbstract)
                    .GroupBy(x => x.ServiceType.GenericTypeArguments.First())
                    .ToDictionary(x => x.Key, x => x.First().ServiceType)
        );
    }

    internal IReadOnlyDictionary<Type, Type> RegisteredEventsAndHandlers => _handlers.Value;

    internal IReadOnlyDictionary<Type, Type> RegisteredPreActions => _pre.Value;

    internal IReadOnlyDictionary<Type, Type> RegisteredPostActions => _post.Value;

    internal IReadOnlyDictionary<Type, Type> RegisteredPipelines => _pipelines.Value;
}
