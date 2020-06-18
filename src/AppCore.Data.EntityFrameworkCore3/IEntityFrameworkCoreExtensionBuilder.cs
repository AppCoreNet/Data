// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

using AppCore.DependencyInjection.Facilities;
using Microsoft.EntityFrameworkCore;

namespace AppCore.Data.EntityFrameworkCore
{
    public interface IEntityFrameworkCoreExtensionBuilder<TTag, TDbContext>
        : IFacilityExtensionBuilder<IDataFacility, EntityFrameworkCoreExtension<TTag, TDbContext>>
        where TDbContext : DbContext
    {
        /*
         IEntityFrameworkCoreExtensionBuilder<TTag, TDbContext> WithRepository<TEntity>()
            where TEntity : class, IEntity;
        */
    }
}