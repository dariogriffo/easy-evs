namespace EasyEvs.Contracts;

using System;

/// <summary>
/// A attribute class to easily hydrate and dehydrate events from Aggregates
/// </summary>
/// <typeparam name="T">An Aggregate for your Events</typeparam>
[AttributeUsage(AttributeTargets.Class)]
public class AggregateAttribute<T> : Attribute
    where T : Aggregate;
