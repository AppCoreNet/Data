// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using NSubstitute;

namespace AppCoreNet.Data.MongoDB;

public static class EntityMapper
{
    public static readonly IEntityMapper Instance;

    static EntityMapper()
    {
        Instance = Substitute.For<IEntityMapper>();

        Instance.Map<DAO.TestEntity>(Arg.Any<Entities.TestEntity>())
              .Returns(
                  ci =>
                  {
                      var entity = ci.ArgAt<Entities.TestEntity>(0);
                      return new DAO.TestEntity()
                      {
                          Id = entity.Id,
                          Name = entity.Name,
                          ChangeToken = entity.ChangeToken,
                      };
                  });

        Instance.Map<Entities.TestEntity>(Arg.Any<DAO.TestEntity>())
              .Returns(
                  ci =>
                  {
                      var document = ci.ArgAt<DAO.TestEntity>(0);
                      return new Entities.TestEntity()
                      {
                          Id = document.Id,
                          Name = document.Name,
                          ChangeToken = document.ChangeToken,
                      };
                  });

        Instance.Map<DAO.TestEntity2>(Arg.Any<Entities.TestEntity2>())
              .Returns(
                  ci =>
                  {
                      var entity = ci.ArgAt<Entities.TestEntity2>(0);
                      return new DAO.TestEntity2()
                      {
                          Id = new DAO.ComplexId
                          {
                              Id = entity.Id.Id,
                              Version = entity.Id.Version,
                          },
                          Name = entity.Name,
                          ChangeToken = entity.ChangeToken,
                      };
                  });

        Instance.Map<Entities.TestEntity2>(Arg.Any<DAO.TestEntity2>())
              .Returns(
                  ci =>
                  {
                      var document = ci.ArgAt<DAO.TestEntity2>(0);
                      return new Entities.TestEntity2()
                      {
                          Id = new Entities.ComplexId
                          {
                              Id = document.Id.Id,
                              Version = document.Id.Version,
                          },
                          Name = document.Name,
                          ChangeToken = document.ChangeToken,
                          ExpectedChangeToken = document.ChangeToken,
                      };
                  });
    }
}