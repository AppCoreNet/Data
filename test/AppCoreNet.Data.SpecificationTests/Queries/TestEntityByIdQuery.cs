﻿// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using AppCoreNet.Data.Entities;

namespace AppCoreNet.Data.Queries;

public class TestEntityByIdQuery : IQuery<TestEntity, TestEntity?>
{
    public Guid Id { get; }

    public TestEntityByIdQuery(Guid id)
    {
        Id = id;
    }
}