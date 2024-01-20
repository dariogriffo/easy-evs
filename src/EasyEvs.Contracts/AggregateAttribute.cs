namespace EasyEvs.Contracts;

using System;

/// <summary>
/// A class to easily dehydrate events into Aggregates
/// </summary>
/// <typeparam name="T">An Aggregate for your Events</typeparam>
[AttributeUsage(AttributeTargets.Class)]
public class AggregateAttribute<T> : Attribute
    where T : Aggregate;
