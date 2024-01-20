namespace EasyEvs.Contracts.Internal;

using System;

[AttributeUsage(AttributeTargets.Interface)]
internal class HandlesEventAttribute : Attribute;

[AttributeUsage(AttributeTargets.Interface)]
internal class PreActionEventAttribute : Attribute;

[AttributeUsage(AttributeTargets.Interface)]
internal class PostHandlerEventAttribute : Attribute;

[AttributeUsage(AttributeTargets.Interface)]
internal class PipelineHandlerAttribute : Attribute;
