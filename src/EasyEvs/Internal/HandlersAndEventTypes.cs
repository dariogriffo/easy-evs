namespace EasyEvs.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Contracts;
    using Microsoft.Extensions.DependencyInjection;

    internal class HandlersAndEventTypes
    {

        private readonly Lazy<IReadOnlyDictionary<Type, Type>> _handlers;
        private readonly Lazy<IReadOnlyDictionary<Type, Type>> _pre;
        private readonly Lazy<IReadOnlyDictionary<Type, Type>> _post;
        private readonly Lazy<IReadOnlyDictionary<Type, Type>> _pipelines;

        internal HandlersAndEventTypes(IServiceCollection services)
        {
            bool Implements(Type i, Type targetType)
            {
                return i.GenericTypeArguments != null &&
                       i.GetInterfaces().Any(gi => gi == targetType) &&
                       i.GenericTypeArguments.Any(g => g.GetInterfaces().Any(gi => gi == typeof(IEvent)));
            }

            _handlers = new Lazy<IReadOnlyDictionary<Type, Type>>(() => services
                .Where(x => Implements(x.ServiceType, typeof(IHandlesEvent)))
                .Where(i => !i.ImplementationType!.IsAbstract)
                .ToDictionary(x => x.ServiceType.GenericTypeArguments.First(), x => x.ServiceType));

            _pre = new Lazy<IReadOnlyDictionary<Type, Type>>(() => services
                .Where(x => Implements(x.ServiceType, typeof(IPreHandlesEventAction)))
                .Where(i => !i.ImplementationType!.IsAbstract)
                .GroupBy(x => x.ServiceType.GenericTypeArguments.First())
                .ToDictionary(x => x.Key, x => x.First().ServiceType));

            _post = new Lazy<IReadOnlyDictionary<Type, Type>>(() => services
                .Where(x => Implements(x.ServiceType, typeof(IPostHandlesEventAction)))
                .Where(i => !i.ImplementationType!.IsAbstract)
                .GroupBy(x => x.ServiceType.GenericTypeArguments.First())
                .ToDictionary(x => x.Key, x => x.First().ServiceType));

            _pipelines = new Lazy<IReadOnlyDictionary<Type, Type>>(() => services
                .Where(x => Implements(x.ServiceType, typeof(IPipelineHandlesEventAction)))
                .Where(i => !i.ImplementationType!.IsAbstract)
                .GroupBy(x => x.ServiceType.GenericTypeArguments.First())
                .ToDictionary(x => x.Key, x => x.First().ServiceType));
        }

        internal IReadOnlyDictionary<Type, Type> RegisteredEventsAndHandlers => _handlers.Value;

        internal IReadOnlyDictionary<Type, Type> RegisteredPreActions => _pre.Value;
        
        internal IReadOnlyDictionary<Type, Type> RegisteredPostActions => _post.Value;

        internal IReadOnlyDictionary<Type, Type> RegisteredPipelines => _pipelines.Value;
    }
}
