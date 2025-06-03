// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using NSubstitute;

namespace AppCoreNet.Data.EntityFramework;

public static class EntityMapper
{
    public static readonly IEntityMapper Instance;

    static EntityMapper()
    {
        Instance = Substitute.For<IEntityMapper>();

        Instance.Map<DAO.TestDao>(Arg.Any<Entities.TestEntity>())
              .Returns(
                  ci =>
                  {
                      var entity = ci.ArgAt<Entities.TestEntity>(0);
                      return new DAO.TestDao()
                      {
                          Id = entity.Id,
                          Name = entity.Name,
                          ChangeToken = entity.ChangeToken,
                      };
                  });

        Instance.When(m => m.Map(Arg.Any<Entities.TestEntity>(), Arg.Any<DAO.TestDao>())).Do(
            ci =>
            {
                var from = ci.ArgAt<Entities.TestEntity>(0);
                var to = ci.ArgAt<DAO.TestDao>(1);

                to.Id = from.Id;
                to.Name = from.Name;
                to.ChangeToken = from.ChangeToken;
            });

        Instance.Map<Entities.TestEntity>(Arg.Any<DAO.TestDao>())
              .Returns(
                  ci =>
                  {
                      var document = ci.ArgAt<DAO.TestDao>(0);
                      return new Entities.TestEntity()
                      {
                          Id = document.Id,
                          Name = document.Name,
                          ChangeToken = document.ChangeToken,
                      };
                  });

        Instance.Map<DAO.TestDao2>(Arg.Any<Entities.TestEntity2>())
              .Returns(
                  ci =>
                  {
                      var entity = ci.ArgAt<Entities.TestEntity2>(0);
                      return new DAO.TestDao2()
                      {
                          Id = entity.Id.Id,
                          Version = entity.Id.Version,
                          Name = entity.Name,
                          ChangeToken = entity.ChangeToken,
                      };
                  });

        Instance.Map<Entities.TestEntity2>(Arg.Any<DAO.TestDao2>())
              .Returns(
                  ci =>
                  {
                      var document = ci.ArgAt<DAO.TestDao2>(0);
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