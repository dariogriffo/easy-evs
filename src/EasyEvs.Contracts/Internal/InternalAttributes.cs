namespace EasyEvs.Contracts.Internal;

using System;

[AttributeUsage(AttributeTargets.Interface)]
internal sealed class HandlesEventAttribute : Attribute;

[AttributeUsage(AttributeTargets.Interface)]
internal sealed class PreActionEventAttribute : Attribute;

[AttributeUsage(AttributeTargets.Interface)]
internal sealed class PostHandlerEventAttribute : Attribute;

[AttributeUsage(AttributeTargets.Interface)]
internal sealed class PipelineHandlerAttribute : Attribute;
