// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

using FluentAssertions;
using Xunit;

namespace AppCore.Data
{
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
}
