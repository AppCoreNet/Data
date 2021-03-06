// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

using System;
using System.Linq;
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
        where TEntity : IEntity<TId>
        where TDbContext : DbContext
        where TDbEntity : class
    {
        private static readonly EntityModelProperties<TId, TEntity> EntityModelProperties =
            new EntityModelProperties<TId, TEntity>();

        private readonly ITokenGenerator _tokenGenerator;
        private readonly IEntityMapper _mapper;
        private readonly ILogger _logger;
        private readonly DbModelProperties _modelProperties;

        /// <summary>
        /// Gets the <see cref="DbContext"/>.
        /// </summary>
        protected TDbContext Context { get; }

        /// <summary>
        /// Gets the <see cref="DbSet{TDbEntity}"/>.
        /// </summary>
        protected DbSet<TDbEntity> Set { get; }

        /// <summary>
        /// Gets the <see cref="IDbContextDataProvider{TDbContext}"/> which owns the repository.
        /// </summary>
        public IDbContextDataProvider<TDbContext> Provider { get; }

        IDataProvider IRepository<TId, TEntity>.Provider => Provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbContextRepository{TId,TEntity,TDbContext,TDbEntity}"/> class.
        /// </summary>
        /// <param name="provider">The data provider.</param>
        /// <param name="tokenGenerator">The token generator used for generating change tokens.</param>
        /// <param name="entityMapper">The entity mapper.</param>
        /// <param name="logger">The logger.</param>
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

            _modelProperties = DbModelProperties.Get(
                typeof(TDbContext),
                typeof(TDbEntity),
                context.Model,
                typeof(TEntity));

            Provider = provider;
            Context = context;
            Set = context.Set<TDbEntity>();
        }

        /// <summary>
        /// Can be overridden to get the primary key from the specified entity id.
        /// </summary>
        /// <param name="id">The unique entity id.</param>
        /// <returns>The primary key values.</returns>
        protected virtual object[] GetPrimaryKey(TId id)
        {
            return EntityModelProperties.GetIdValues(id);
        }

        /// <summary>
        /// Can be overridden to apply the query expression used when searching for entities by
        /// it's primary key.
        /// </summary>
        /// <param name="queryable">The queryable.</param>
        /// <param name="id">The unique entity id.</param>
        /// <returns>The queryable filtered by primary key.</returns>
        protected virtual IQueryable<TDbEntity> ApplyPrimaryKeyExpression(IQueryable<TDbEntity> queryable, TId id)
        {
            object[] primaryKey = GetPrimaryKey(id);

            if (primaryKey.Length == 1)
            {
                string primaryKeyPropertyName = _modelProperties.PrimaryKeyPropertyNames[0];
                object keyValue = primaryKey[0];
                return queryable.Where(e => EF.Property<TId>(e, primaryKeyPropertyName).Equals(keyValue));
            }

            for (int i = 0; i < primaryKey.Length; i++)
            {
                string idPropertyName = EntityModelProperties.IdPropertyNames[i];
                string primaryKeyPropertyName = _modelProperties.PrimaryKeyPropertyNames[i];

                if (!string.Equals(idPropertyName, primaryKeyPropertyName, StringComparison.OrdinalIgnoreCase))
                    throw new NotSupportedException("Entity.Id property names do not match the primary key property names.");

                object keyValue = primaryKey[i];
                queryable = queryable.Where(
                    e => EF.Property<object>(e, primaryKeyPropertyName).Equals(keyValue));
            }

            return queryable;
        }

        protected virtual IQueryable<TDbEntity> ApplyIncludes(IQueryable<TDbEntity> queryable)
        {
            return queryable;
        }

        private void UpdateChangeToken(EntityEntry<TDbEntity> entry, TEntity entity)
        {
            if (_modelProperties.HasConcurrencyToken)
            {
                PropertyEntry<TDbEntity, string> concurrencyToken =
                    entry.Property<string>(_modelProperties.ConcurrencyTokenPropertyName);

                if (entity is IHasChangeTokenEx expectedChangeToken)
                {
                    concurrencyToken.OriginalValue = expectedChangeToken.ExpectedChangeToken;
                    if (entry.State != EntityState.Deleted)
                    {
                        string changeToken = expectedChangeToken.ChangeToken;
                        if (string.IsNullOrWhiteSpace(changeToken)
                            || string.Equals(changeToken, expectedChangeToken.ExpectedChangeToken))
                        {
                            changeToken = _tokenGenerator.Generate();
                        }

                        concurrencyToken.CurrentValue = changeToken;
                    }
                }
                else if (entity is IHasChangeToken changeToken)
                {
                    concurrencyToken.OriginalValue = changeToken.ChangeToken;
                    if (entry.State != EntityState.Deleted)
                    {
                        concurrencyToken.CurrentValue = _tokenGenerator.Generate();
                    }
                }
            }
        }

        private IQueryable<TDbEntity> GetQueryable(TId id)
        {
            return ApplyPrimaryKeyExpression(ApplyIncludes(Set), id);
        }

        protected virtual async Task<TDbEntity> FindCoreAsync(TId id, CancellationToken cancellationToken)
        {
            TDbEntity dbEntity = await GetQueryable(id)
                                       .AsNoTracking()
                                       .FirstOrDefaultAsync(cancellationToken);

            return dbEntity;
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> FindAsync(TId id, CancellationToken cancellationToken)
        {
            Ensure.Arg.NotNull(id, nameof(id));

            TDbEntity dbEntity = await FindCoreAsync(id, cancellationToken);
            return _mapper.Map<TEntity>(dbEntity);
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> LoadAsync(TId id, CancellationToken cancellationToken)
        {
            TEntity entity = await FindAsync(id, cancellationToken);
            if (entity == null)
            {
                throw new EntityNotFoundException(typeof(TEntity), id);
            }

            return entity;
        }

        protected virtual ValueTask<EntityEntry<TDbEntity>> CreateCoreAsync(
            TEntity entity,
            TDbEntity dbEntity,
            CancellationToken cancellationToken)
        {
            return new ValueTask<EntityEntry<TDbEntity>>(Set.Add(dbEntity));
        }
        
        /// <inheritdoc />
        public virtual async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken)
        {
            Ensure.Arg.NotNull(entity, nameof(entity));

            _logger.EntitySaving(entity);

            var dbEntity = _mapper.Map<TDbEntity>(entity);

            using (Provider.BeginChangeScope())
            {
                EntityEntry<TDbEntity> dbEntry = await CreateCoreAsync(entity, dbEntity, cancellationToken);
                dbEntity = dbEntry.Entity;
                UpdateChangeToken(dbEntry, entity);
                await Provider.SaveChangesAsync(cancellationToken);
            }

            entity = _mapper.Map<TEntity>(dbEntity);
            _logger.EntitySaved(entity);

            return entity;
        }

        protected virtual ValueTask<EntityEntry<TDbEntity>> UpdateCoreAsync(
            TEntity entity,
            TDbEntity dbEntity,
            CancellationToken cancellationToken)
        {
            return new ValueTask<EntityEntry<TDbEntity>>(Set.Update(dbEntity));
        }

        /// <inheritdoc />
        public virtual async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken)
        {
            Ensure.Arg.NotNull(entity, nameof(entity));

            if (entity.IsTransient())
                throw new InvalidOperationException("The entity cannot be updated because the id property has the default value.");

            _logger.EntitySaving(entity);

            TDbEntity dbEntity = await GetQueryable(entity.Id)
                                       .FirstOrDefaultAsync(cancellationToken);

            if (dbEntity == null)
                throw new EntityNotFoundException(typeof(TEntity), entity.Id);

            _mapper.Map(entity, dbEntity);

            using (Provider.BeginChangeScope())
            {
                EntityEntry<TDbEntity> dbEntry = await UpdateCoreAsync(entity, dbEntity, cancellationToken);
                dbEntity = dbEntry.Entity;
                UpdateChangeToken(dbEntry, entity);
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
        public virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken)
        {
            Ensure.Arg.NotNull(entity, nameof(entity));

            _logger.EntityDeleting(entity);

            var dbEntity = Activator.CreateInstance<TDbEntity>();
            _mapper.Map(entity, dbEntity);

            using (Provider.BeginChangeScope())
            {
                EntityEntry<TDbEntity> dbEntry = await DeleteCoreAsync(entity, dbEntity, cancellationToken);
                UpdateChangeToken(dbEntry, entity);
                await Provider.SaveChangesAsync(cancellationToken);
            }

            _logger.EntityDeleted(entity);
        }
    }
}
