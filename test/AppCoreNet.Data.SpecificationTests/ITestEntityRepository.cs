using System;
using AppCoreNet.Data.Entities;

namespace AppCoreNet.Data;

public interface ITestEntityRepository : IRepository<Guid, TestEntity>
{
}