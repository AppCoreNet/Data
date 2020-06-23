// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AppCore.Diagnostics;
using AppCore.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AppCore.Data.EntityFrameworkCore
{
    /// <summary>
    /// Provides a <see cref="DbContext"/> based implementation of the <see cref="IRepository{TId,TEntity}"/> interface.
    /// </summary>
    /// <typeparam name="TId">The type of the entity id.</typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
    /// <typeparam name="TDbEntity">The type of the database entity.</typeparam>
    public class DbContextRepository<TId, TEntity, TDbContext, TDbEntity> : IRepository<TId, TEntity>
        where TEntity : class, IEntity<TId>
        where TDbContext : DbContext
        where TDbEntity : class
    {
        private readonly ITokenGenerator _tokenGenerator;
        private readonly IEntityMapper _mapper;
        private readonly ILogger _logger;
        private readonly DbModelProperties _modelProperties;

        /// <summary>
        /// Gets the <see cref="DbSet{TEntity}"/>.
        /// </summary>
        protected DbSet<TDbEntity> Set { get; }

        /// <summary>
        /// Gets the <see cref="IDbContextDataProvider{TDbContext}"/> which owns the repository.
        /// </summary>
        public IDbContextDataProvider<TDbContext> Provider { get; }

        IDataProvider IRepository<TId, TEntity>.Provider => Provider;

        public DbContextRepository(
            IDbContextDataProvider<TDbContext> provider,
            ITokenGenerator tokenGenerator,
            IEntityMapper entityMapper,
            ILogger logger)
        {
            Ensure.Arg.NotNull(provider, nameof(provider));
            Ensure.Arg.NotNull(tokenGenerator, nameof(tokenGenerator));
            Ensure.Arg.NotNull(entityMapper, nameof(entityMapper));
            Ensure.Arg.NotNull(logger, nameof(logger));

            TDbContext context = provider.GetContext();
            _tokenGenerator = tokenGenerator;
            _mapper = entityMapper;
            _logger = logger;
            _modelProperties = DbModelProperties.Get(typeof(TDbContext), typeof(TDbEntity), context.Model, typeof(TEntity));

            Provider = provider;
            Set = context.Set<TDbEntity>();
        }

        private Expression<Func<TDbEntity, bool>> FindByIdExpression(TId id)
        {
            return e => EF.Property<TId>(e, _modelProperties.PrimaryKeyPropertyName).Equals(id);
        }

        protected virtual IQueryable<TDbEntity> ApplyIncludes(IQueryable<TDbEntity> queryable)
        {
            return queryable;
        }

        private void UpdateConcurrencyToken(EntityEntry<TDbEntity> entry, TEntity entity)
        {
            if (entity is IHasConcurrencyToken concurrentEntity)
            {
                PropertyEntry<TDbEntity, string> concurrencyToken =
                    entry.Property<string>(_modelProperties.ConcurrencyTokenPropertyName);

                concurrencyToken.OriginalValue = concurrentEntity.ConcurrencyToken;
                if (entry.State != EntityState.Deleted)
                {
                    concurrencyToken.CurrentValue = _tokenGenerator.Generate();
                }
            }
        }

        protected virtual async Task<TDbEntity> FindCoreAsync(TId id, CancellationToken cancellationToken)
        {
            TDbEntity dbEntity = await ApplyIncludes(Set)
                                       .AsNoTracking()
                                       .Where(FindByIdExpression(id))
                                       .FirstOrDefaultAsync(cancellationToken);

            return dbEntity;
        }

        /// <inheritdoc />
        public async Task<TEntity> FindAsync(TId id, CancellationToken cancellationToken)
        {
            Ensure.Arg.NotNull(id, nameof(id));

            TDbEntity dbEntity = await FindCoreAsync(id, cancellationToken);
            return _mapper.Map<TEntity>(dbEntity);
        }

        protected virtual ValueTask<EntityEntry<TDbEntity>> SaveCoreAsync(
            TEntity entity,
            TDbEntity dbEntity,
            CancellationToken cancellationToken)
        {
            return new ValueTask<EntityEntry<TDbEntity>>(Set.Update(dbEntity));
        }

        /// <inheritdoc />
        public async Task<TEntity> SaveAsync(TEntity entity, CancellationToken cancellationToken)
        {
            Ensure.Arg.NotNull(entity, nameof(entity));

            _logger.EntitySaving(entity);

            TDbEntity dbEntity = null;
            if (!entity.IsTransient())
            {
                dbEntity = await ApplyIncludes(Set)
                                 .Where(FindByIdExpression(entity.Id))
                                 .FirstOrDefaultAsync(cancellationToken);
            }

            if (dbEntity != null)
            {
                _mapper.Map(entity, dbEntity);
            }
            else
            {
                dbEntity = _mapper.Map<TDbEntity>(entity);
            }

            using (Provider.BeginChangeScope())
            {
                EntityEntry<TDbEntity> dbEntry = await SaveCoreAsync(entity, dbEntity, cancellationToken);
                dbEntity = dbEntry.Entity;
                UpdateConcurrencyToken(dbEntry, entity);
                await Provider.SaveChangesAsync(cancellationToken);
            }

            entity = _mapper.Map<TEntity>(dbEntity);
            _logger.EntitySaved(entity);

            return entity;
        }

        protected virtual ValueTask<EntityEntry<TDbEntity>> DeleteCoreAsync(
            TEntity entity,
            TDbEntity dbEntity,
            CancellationToken cancellationToken)
        {
            return new ValueTask<EntityEntry<TDbEntity>>(Set.Remove(dbEntity));
        }

        /// <inheritdoc />
        public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken)
        {
            Ensure.Arg.NotNull(entity, nameof(entity));

            _logger.EntityDeleting(entity);

            var dbEntity = Activator.CreateInstance<TDbEntity>();
            _mapper.Map(entity, dbEntity);

            using (Provider.BeginChangeScope())
            {
                EntityEntry<TDbEntity> dbEntry = await DeleteCoreAsync(entity, dbEntity, cancellationToken);
                UpdateConcurrencyToken(dbEntry, entity);
                await Provider.SaveChangesAsync(cancellationToken);
            }

            _logger.EntityDeleted(entity);
        }
    }
}
