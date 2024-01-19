// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using FluentAssertions;
using Xunit;

namespace AppCoreNet.Data;

public class EntityExtensionsTests
{
    [Fact]
    public void IsTransientReturnsTrueIfIdHasDefaultValue()
    {
        var entity = new EntityWithSimpleId();
        entity.IsTransient()
              .Should()
              .BeTrue();
    }

    [Fact]
    public void IsTransientReturnsFalseIfIdDoesNotHaveDefaultValue()
    {
        var entity = new EntityWithSimpleId { Id = 1 };
        entity.IsTransient()
              .Should()
              .BeFalse();
    }

    [Fact]
    public void IsTransientReturnsTrueIfComplexIdHasDefaultValue()
    {
        var entity = new EntityWithComplexId();
        entity.IsTransient()
              .Should()
              .BeTrue();
    }

    [Fact]
    public void IsTransientReturnsFalseIfComplexIdDoesNotHaveDefaultValue()
    {
        var entity = new EntityWithComplexId { Id = new VersionId(1, 0) };
        entity.IsTransient()
              .Should()
              .BeFalse();
    }
}