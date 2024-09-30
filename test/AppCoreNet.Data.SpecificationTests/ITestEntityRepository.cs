// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using AppCoreNet.Data.Entities;

namespace AppCoreNet.Data;

public interface ITestEntityRepository : IRepository<Guid, TestEntity>
{
}