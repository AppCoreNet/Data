// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using AppCoreNet.Diagnostics;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace AppCoreNet.Data.MongoDB;

/// <summary>
/// Provides a MongoDB based implementation of the <see cref="IRepository{TId,TEntity}"/> interface.
/// </summary>
/// <typeparam name="TId">The type of the entity id.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TDocument">The type of the MongoDB document.</typeparam>
public class MongoRepository<TId, TEntity, TDocument> : IRepository<TId, TEntity>, IMongoRepository
    where TEntity : class, IEntity<TId>
    where TDocument : class
{
    /// <summary>
    /// Provides a base class for scalar query handlers of this repository.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public abstract class ScalarQueryHandler<TQuery, TResult> : MongoScalarQueryHandler<TQuery, TEntity, TResult?, TDocument>
        where TQuery : IQuery<TEntity, TResult?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScalarQueryHandler{TQuery,TResult}"/> class.
        /// </summary>
        /// <param name="provider">The <see cref="MongoDataProvider"/>.</param>
        protected ScalarQueryHandler(MongoDataProvider provider)
            : base(provider)
        {
        }
    }

    /// <summary>
    /// Provides a base class for vector query handlers of this repository.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public abstract class VectorQueryHandler<TQuery, TResult> : MongoVectorQueryHandler<TQuery, TEntity, TResult, TDocument>
        where TQuery : IQuery<TEntity, IReadOnlyCollection<TResult>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VectorQueryHandler{TQuery,TResult}"/> class.
        /// </summary>
        /// <param name="provider">The <see cref="MongoDataProvider"/>.</param>
        protected VectorQueryHandler(MongoDataProvider provider)
            : base(provider)
        {
        }
    }

    /// <summary>
    /// Provides a base class for paged query handlers of this repository.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public abstract class PagedQueryHandler<TQuery, TResult> : MongoPagedQueryHandler<TQuery, TEntity, TResult, TDocument>
        where TQuery : IPagedQuery<TEntity, TResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PagedQueryHandler{TQuery,TResult}"/> class.
        /// </summary>
        /// <param name="provider">The <see cref="MongoDataProvider"/>.</param>
        protected PagedQueryHandler(MongoDataProvider provider)
            : base(provider)
        {
        }
    }

    private const string ObjectIdField = "_id";

    [SuppressMessage(
        "ReSharper",
        "StaticMemberInGenericType",
        Justification = "Depends on generic parameter.")]
    private static readonly string? _changeTokenField;

    /// <summary>
    /// Gets the <see cref="MongoDataProvider"/>.
    /// </summary>
    public MongoDataProvider Provider { get; }

    /// <summary>
    /// Gets the <see cref="IMongoCollection{TDocument}"/> used by the repository.
    /// </summary>
    protected IMongoCollection<BsonDocument> Collection { get; }

    /// <inheritdoc />
    IDataProvider IRepository<TId, TEntity>.Provider => Provider;

    static MongoRepository()
    {
        if (typeof(IHasChangeToken).IsAssignableFrom(typeof(TEntity))
            || typeof(IHasChangeTokenEx).IsAssignableFrom(typeof(TEntity)))
        {
            IBsonSerializer<TDocument> serializer = BsonSerializer.LookupSerializer<TDocument>();
            var documentSerializer = (IBsonDocumentSerializer)serializer;

            if (documentSerializer.TryGetMemberSerializationInfo(
                    nameof(IHasChangeToken.ChangeToken),
                    out BsonSerializationInfo memberInfo))
            {
                _changeTokenField = memberInfo.ElementName;
            }
            else
            {
                throw new InvalidOperationException(
                    $"Entity implements change token but no '{nameof(IHasChangeToken.ChangeToken)}' property has been found in '{typeof(TDocument)}'");
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoRepository{TId,TEntity,TDocument}"/> class.
    /// </summary>
    /// <param name="provider">The <see cref="MongoDataProvider"/>.</param>
    /// <param name="collectionName">The name of the collection to use.</param>
    /// <param name="collectionSettings">The settings used to access to collection.</param>
    public MongoRepository(
        MongoDataProvider provider,
        string? collectionName = null,
        MongoCollectionSettings? collectionSettings = null)
    {
        Ensure.Arg.NotNull(provider);
        Ensure.Arg.NotEmptyButNull(collectionName);

        Provider = provider;
        Collection = provider.Database.GetCollection<BsonDocument>(
            string.IsNullOrEmpty(collectionName)
                ? provider.GetCollectionName<TDocument>()
                : collectionName,
            collectionSettings);
    }

    /// <summary>
    /// Can be overridden to get the primary key from the specified entity id.
    /// </summary>
    /// <remarks>
    /// For complex keys this needs to be overridden to return a <see cref="BsonValue"/> that uniquely
    /// identifies the entity.
    /// </remarks>
    /// <param name="id">The unique entity id.</param>
    /// <returns>The primary key values.</returns>
    protected virtual BsonValue GetPrimaryKey(TId id)
    {
        if (typeof(TId).IsPrimitive || typeof(TId) == typeof(string) || typeof(TId) == typeof(Guid))
        {
            IDiscriminatorConvention objectDiscriminatorConvention =
                BsonSerializer.LookupDiscriminatorConvention(typeof(object));

            var objectSerializer = new ObjectSerializer(objectDiscriminatorConvention, GuidRepresentation.Standard);
            return objectSerializer.ToBsonValue(id);
        }

        return id.ToBsonDocument();
    }

    private FilterDefinition<BsonDocument> GetModificationFilter(TEntity entity)
    {
        FilterDefinitionBuilder<BsonDocument> builder = Builders<BsonDocument>.Filter;
        FilterDefinition<BsonDocument> filter = builder.Eq(ObjectIdField, GetPrimaryKey(entity.Id));

        switch (entity)
        {
            case IHasChangeToken changeToken:
                filter &= builder.Eq(_changeTokenField, changeToken.ChangeToken);
                break;
            case IHasChangeTokenEx changeTokenEx:
                filter &= builder.Eq(_changeTokenField, changeTokenEx.ExpectedChangeToken);
                break;
        }

        return filter;
    }

    private void UpdateChangeToken(TEntity entity, BsonDocument document)
    {
        switch (entity)
        {
            case IHasChangeToken:
                document.Set(_changeTokenField, new BsonString(Provider.TokenGenerator.Generate()));
                break;

            case IHasChangeTokenEx changeTokenEx:
                string? changeToken = changeTokenEx.ChangeToken;
                if (string.IsNullOrWhiteSpace(changeToken)
                    || string.Equals(changeToken, changeTokenEx.ExpectedChangeToken))
                {
                    document.Set(_changeTokenField, new BsonString(Provider.TokenGenerator.Generate()));
                }

                break;
        }
    }

    private async Task<T> DoAsync<T>(Action before, Func<Task<T>> action, Action<T, long> after, Action<Exception> failed)
    {
        before();

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        try
        {
            T result = await action()
                .ConfigureAwait(false);

            after(result, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception error)
        {
            failed(error);
            throw;
        }
    }

    private async Task DoAsync(Action before, Func<Task> action, Action<long> after, Action<Exception> failed)
    {
        await DoAsync(
            before,
            async () =>
            {
                await action()
                    .ConfigureAwait(false);

                return true;
            },
            (_, elapsedTimeMs) => after(elapsedTimeMs),
            failed);
    }

    /// <inheritdoc />
    public async Task<TResult> QueryAsync<TResult>(IQuery<TEntity, TResult> query, CancellationToken cancellationToken = default)
    {
        Ensure.Arg.NotNull(query);

        IMongoQueryHandler<TEntity, TResult> queryHandler =
            Provider.QueryHandlerFactory.CreateHandler(Provider, query);

        TResult result;
        try
        {
            result =
                await DoAsync(
                        () =>
                        {
                            Provider.Logger.QueryExecuting(query);
                        },
                        async () =>
                        {
                            return await queryHandler.ExecuteAsync(query, cancellationToken)
                                                     .ConfigureAwait(false);
                        },
                        (_, elapsedTimeMs) =>
                        {
                            Provider.Logger.QueryExecuted(query, elapsedTimeMs);
                        },
                        error =>
                        {
                            Provider.Logger.QueryExecuteFailed(error, query);
                        })
                    .ConfigureAwait(false);
        }
        finally
        {
            await DisposeQueryHandlerAsync(queryHandler)
                .ConfigureAwait(false);
        }

        [SuppressMessage(
            "IDisposableAnalyzers.Correctness",
            "IDISP007:Don\'t dispose injected",
            Justification = "Ownership is correct.")]
        [SuppressMessage(
            "ReSharper",
            "SuspiciousTypeConversion.Global",
            Justification = "Handler may implement IDisposable.")]
        async ValueTask DisposeQueryHandlerAsync(IMongoQueryHandler<TEntity, TResult> handler)
        {
            switch (handler)
            {
                case IAsyncDisposable disposable:
                    await disposable.DisposeAsync()
                                    .ConfigureAwait(false);
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }

        return result;
    }

    /// <summary>
    /// Provides the core logic to find an entity by it's unique identifier.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns>The entity if found, otherwise <c>null</c>.</returns>
    protected virtual async Task<TDocument?> FindCoreAsync(TId id, CancellationToken cancellationToken)
    {
        IClientSessionHandle? sessionHandle = Provider.TransactionManager.CurrentTransaction?.SessionHandle;

        FilterDefinition<BsonDocument> filter = Builders<BsonDocument>.Filter.Eq(ObjectIdField, GetPrimaryKey(id));

        IFindFluent<BsonDocument, BsonDocument> find = sessionHandle != null
            ? Collection.Find(sessionHandle, filter)
            : Collection.Find(filter);

        return await find.As<TDocument>()
                         .FirstOrDefaultAsync(cancellationToken)
                         .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<TEntity?> FindAsync(TId id, CancellationToken cancellationToken = default)
    {
        Ensure.Arg.NotNull(id);

        return
            await DoAsync(
                    () =>
                    {
                        Provider.Logger.EntityLoading(typeof(TEntity), id);
                    },
                    async () =>
                    {
                        TDocument? document = await FindCoreAsync(id, cancellationToken)
                            .ConfigureAwait(false);

                        return document != null
                            ? Provider.EntityMapper.Map<TEntity>(document)
                            : default;
                    },
                    (result, elapsedTimeMs) =>
                    {
                        if (result != null)
                        {
                            Provider.Logger.EntityLoaded(result, elapsedTimeMs);
                        }
                        else
                        {
                            Provider.Logger.EntityNotFound(typeof(TEntity), id);
                        }
                    },
                    error =>
                    {
                        Provider.Logger.EntityLoadFailed(error, typeof(TEntity), id);
                    })
                .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<TEntity> LoadAsync(TId id, CancellationToken cancellationToken = default)
    {
        TEntity? entity = await FindAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(TEntity), id!);
        }

        return entity;
    }

    /// <summary>
    /// Provides the core logic to update an entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="document">The document.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns>The updated document.</returns>
    protected virtual async Task<TDocument> UpdateCoreAsync(
        TEntity entity,
        TDocument document,
        CancellationToken cancellationToken)
    {
        IClientSessionHandle? sessionHandle = Provider.TransactionManager.CurrentTransaction?.SessionHandle;

        FilterDefinition<BsonDocument> filter = GetModificationFilter(entity);

        var bson = document.ToBsonDocument();
        UpdateChangeToken(entity, bson);

        ReplaceOneResult result;
        try
        {
            if (sessionHandle == null)
            {
                result = await Collection.ReplaceOneAsync(
                                             filter,
                                             bson,
                                             cancellationToken: cancellationToken)
                                         .ConfigureAwait(false);
            }
            else
            {
                result = await Collection.ReplaceOneAsync(
                                             sessionHandle,
                                             filter,
                                             bson,
                                             cancellationToken: cancellationToken)
                                         .ConfigureAwait(false);
            }
        }
        catch (Exception error)
        {
            throw new EntityUpdateException(error);
        }

        if (result.ModifiedCount == 0)
        {
            throw new EntityConcurrencyException();
        }

        return BsonSerializer.Deserialize<TDocument>(bson);
    }

    /// <inheritdoc />
    public async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Ensure.Arg.NotNull(entity);

        if (entity.IsTransient())
        {
            throw new InvalidOperationException(
                $"The entity cannot be updated because the '{nameof(IEntity<TId>.Id)}' property has the default value.");
        }

        return
            await DoAsync(
                    () =>
                    {
                        Provider.Logger.EntityUpdating(entity);
                    },
                    async () =>
                    {
                        var document = Provider.EntityMapper.Map<TDocument>(entity);

                        document = await UpdateCoreAsync(entity, document, cancellationToken)
                            .ConfigureAwait(false);

                        return Provider.EntityMapper.Map<TEntity>(document);
                    },
                    (result, elapsedTimeMs) =>
                    {
                        Provider.Logger.EntityUpdated(result, elapsedTimeMs);
                    },
                    error =>
                    {
                        Provider.Logger.EntityUpdateFailed(error, entity);
                    })
                .ConfigureAwait(false);
    }

    /// <summary>
    /// Provides the core logic to create an entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="document">The document.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns>The created document.</returns>
    protected virtual async Task<TDocument> CreateCoreAsync(
        TEntity entity,
        TDocument document,
        CancellationToken cancellationToken)
    {
        IClientSessionHandle? sessionHandle = Provider.TransactionManager.CurrentTransaction?.SessionHandle;

        var bson = document.ToBsonDocument();
        UpdateChangeToken(entity, bson);

        try
        {
            if (sessionHandle == null)
            {
                await Collection.InsertOneAsync(bson, cancellationToken: cancellationToken)
                                .ConfigureAwait(false);
            }
            else
            {
                await Collection.InsertOneAsync(sessionHandle, bson, cancellationToken: cancellationToken)
                                .ConfigureAwait(false);
            }
        }
        catch (Exception error)
        {
            throw new EntityUpdateException(error);
        }

        return BsonSerializer.Deserialize<TDocument>(bson);
    }

    /// <inheritdoc />
    public async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Ensure.Arg.NotNull(entity);

        return
            await DoAsync(
                    () =>
                    {
                        Provider.Logger.EntityCreating(entity);
                    },
                    async () =>
                    {
                        var document = Provider.EntityMapper.Map<TDocument>(entity);

                        document = await CreateCoreAsync(entity, document, cancellationToken)
                            .ConfigureAwait(false);

                        return Provider.EntityMapper.Map<TEntity>(document);
                    },
                    (result, elapsedTimeMs) =>
                    {
                        Provider.Logger.EntityCreated(result, elapsedTimeMs);
                    },
                    error =>
                    {
                        Provider.Logger.EntityCreateFailed(error, entity);
                    })
                .ConfigureAwait(false);
    }

    /// <summary>
    /// Provides the core logic to delete an entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns>The asynchronous task.</returns>
    protected virtual async Task DeleteCoreAsync(TEntity entity, CancellationToken cancellationToken)
    {
        IClientSessionHandle? sessionHandle = Provider.TransactionManager.CurrentTransaction?.SessionHandle;
        FilterDefinition<BsonDocument> filter = GetModificationFilter(entity);

        DeleteResult result;
        try
        {
            if (sessionHandle == null)
            {
                result = await Collection.DeleteOneAsync(filter, cancellationToken: cancellationToken)
                                         .ConfigureAwait(false);
            }
            else
            {
                result = await Collection.DeleteOneAsync(sessionHandle, filter, cancellationToken: cancellationToken)
                                         .ConfigureAwait(false);
            }
        }
        catch (Exception error)
        {
            throw new EntityUpdateException(error);
        }

        if (result.DeletedCount == 0)
        {
            throw new EntityConcurrencyException();
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Ensure.Arg.NotNull(entity);

        await DoAsync(
                () =>
                {
                    Provider.Logger.EntityDeleting(entity);
                },
                async () =>
                {
                    await DeleteCoreAsync(entity, cancellationToken)
                        .ConfigureAwait(false);
                },
                elapsedTimeMs =>
                {
                    Provider.Logger.EntityDeleted(entity, elapsedTimeMs);
                },
                error =>
                {
                    Provider.Logger.EntityDeleteFailed(error, entity);
                })
            .ConfigureAwait(false);
    }
}