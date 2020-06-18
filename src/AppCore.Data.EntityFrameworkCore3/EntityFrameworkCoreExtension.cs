// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

using System;
using System.Collections.Generic;
using AppCore.DependencyInjection;
using AppCore.DependencyInjection.Facilities;
using Microsoft.EntityFrameworkCore;

namespace AppCore.Data.EntityFrameworkCore
{
    public class EntityFrameworkCoreExtension<TTag, TDbContext> : FacilityExtension<IDataFacility>
        where TDbContext : DbContext
    {
        private readonly IList<Action<IComponentRegistry, IDataFacility>> _registrationCallbacks =
            new List<Action<IComponentRegistry, IDataFacility>>();

        protected override void RegisterComponents(IComponentRegistry registry, IDataFacility facility)
        {
            registry.Register<IDbContextDataProvider<TDbContext>>()
                    .Add<DbContextDataProvider<TTag, TDbContext>>()
                    .PerScope()
                    .IfNoneRegistered();

            registry.Register<IDataProvider>()
                    .Add(c => c.Resolve<IDbContextDataProvider<TDbContext>>())
                    .PerScope()
                    .IfNotRegistered();

            registry.Register<ITransactionManager>()
                    .Add(c => c.Resolve<IDbContextDataProvider<TDbContext>>().TransactionManager)
                    .PerScope()
                    .IfNotRegistered();

            foreach (Action<IComponentRegistry, IDataFacility> registrationCallback in _registrationCallbacks)
            {
                registrationCallback(registry, facility);
            }
        }

        /*
        public void RegisterRepository<TEntity>()
            where TEntity : class, IEntity
        {
            _registrationCallbacks.Add(
                (r, f) =>
                {
                    r.Register<IRepository<TEntity>>()
                     .Add(c => c.Resolve<IDataProvider<TTag>>().Repository<TEntity>())
                     .PerScope()
                     .IfNoneRegistered();
                });
        }
        */
    }
}