// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using NSubstitute;

namespace AppCoreNet.Data.EntityFrameworkCore;

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

        Instance.When(m => m.Map(Arg.Any<Entities.TestEntity>(), Arg.Any<DAO.TestEntity>())).Do(
            ci =>
            {
                var from = ci.ArgAt<Entities.TestEntity>(0);
                var to = ci.ArgAt<DAO.TestEntity>(1);

                to.Id = from.Id;
                to.Name = from.Name;
                to.ChangeToken = from.ChangeToken;
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
                          Id = entity.Id.Id,
                          Version = entity.Id.Version,
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
                              Id = document.Id,
                              Version = document.Version,
                          },
                          Name = document.Name,
                          ChangeToken = document.ChangeToken,
                          ExpectedChangeToken = document.ChangeToken,
                      };
                  });
    }
}