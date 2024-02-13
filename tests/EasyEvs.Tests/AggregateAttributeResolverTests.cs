namespace EasyEvs.Tests;

using Aggregates;
using FluentAssertions;
using Internal;
using Xunit;

public class AggregateAttributeResolverTests
{
    [Fact]
    public void StreamForAggregateWithObject_WithValidId_ReturnsValidStreamName()
    {
        var aggregate = User.Create("UserId");
        var sut = new AggregateAttributeResolver();
        var result = sut.StreamForAggregate(aggregate);
        result.Should().Be("user_UserId");
    }

    [Fact]
    public void StreamForAggregateWithGeneric_WithValidId_ReturnsValidStreamName()
    {
        var aggregate = User.Create("UserId");
        var sut = new AggregateAttributeResolver();
        var result = sut.StreamForAggregate<User>(aggregate.Id);
        result.Should().Be("user_UserId");
    }

    [Fact]
    public void AggregateIdForStream_WithValidStreamName_ReturnsTheAggregateId()
    {
        var sut = new AggregateAttributeResolver();
        var result = sut.AggregateIdForStream("user_UserId");
        result.Should().Be("UserId");
    }
}
