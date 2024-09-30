// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using AppCoreNet.Data.Entities;

namespace AppCoreNet.Data.Queries;

public class TestEntity2ByIdQuery : IQuery<TestEntity2, TestEntity2?>
{
    public ComplexId Id { get; }

    public TestEntity2ByIdQuery(ComplexId id)
    {
        Id = id;
    }
}