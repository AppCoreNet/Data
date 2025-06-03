// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace AppCoreNet.Data.EntityFramework;

internal sealed class DbModelProperties
{
    private static readonly
        ConcurrentDictionary<(Type DbContextType, Type DbEntityType, Type EntityType), DbModelProperties> _cache =
            new();

    public IReadOnlyList<string> PrimaryKeyPropertyNames { get; private set; } = new List<string>();

    public bool HasConcurrencyToken { get; private set; }

    public string? ConcurrencyTokenPropertyName { get; private set; }

    private readonly Type _dbContextType;
    private readonly Type _dbEntityType; // This is the type used in DbSet<T>, potentially a proxy
    private readonly Type _entityType;   // This is the domain entity type IEntity<TId>

    private DbModelProperties(DbContext dbContext, Type dbEntityType, Type entityType)
    {
        _dbContextType = dbContext.GetType();
        _dbEntityType = dbEntityType;
        _entityType = entityType;
        Initialize(dbContext);
    }

    private void Initialize(DbContext dbContext)
    {
        ObjectContext objectContext = ((IObjectContextAdapter)dbContext).ObjectContext;
        MetadataWorkspace workspace = objectContext.MetadataWorkspace;

        // Determine the non-proxy type for dbEntityType to ensure reliable metadata lookup
        Type typeForMetadataLookup = _dbEntityType;
        if (_dbEntityType.Namespace == "System.Data.Entity.DynamicProxies" && _dbEntityType.BaseType != null)
        {
            typeForMetadataLookup = _dbEntityType.BaseType;
        }

        // Get OSpace EntityType first, as dbEntityType is a CLR type
        EntityType? ospaceEntityType = workspace
                                      .GetItems<EntityType>(DataSpace.OSpace)
                                      .FirstOrDefault(et => et.FullName == typeForMetadataLookup.FullName);

        if (ospaceEntityType == null)
        {
            throw new InvalidOperationException($"Could not find OSpace EntityType for '{typeForMetadataLookup.FullName}' in MetadataWorkspace of DbContext '{_dbContextType.FullName}'.");
        }

        // Get corresponding CSpace EntityType
        var cspaceEntityType = workspace.GetItem<EntityType>(ospaceEntityType.FullName, DataSpace.CSpace);
        if (cspaceEntityType == null)
        {
             // This case should ideally not happen if OSpace type was found and metadata is consistent.
            throw new InvalidOperationException($"Could not find CSpace EntityType corresponding to OSpace '{ospaceEntityType.FullName}' for DbContext '{_dbContextType.FullName}'.");
        }

        PrimaryKeyPropertyNames = cspaceEntityType.KeyProperties.Select(p => p.Name).ToList().AsReadOnly();

        foreach (EdmProperty property in cspaceEntityType.Properties)
        {
            if (property.ConcurrencyMode == ConcurrencyMode.Fixed)
            {
                ConcurrencyTokenPropertyName = property.Name;
                HasConcurrencyToken = true;
                break;
            }
        }

        // Validation logic
        if (typeof(IHasChangeToken).IsAssignableFrom(_entityType)
            || typeof(IHasChangeTokenEx).IsAssignableFrom(_entityType))
        {
            if (!HasConcurrencyToken)
            {
                throw new ArgumentException(
                    $"The entity type '{_entityType.FullName}' implements 'IHasChangeToken' or 'IHasChangeTokenEx' but no matching concurrency token property (ConcurrencyMode.Fixed) was found in the database model for DbEntityType '{_dbEntityType.FullName}' (resolved as '{typeForMetadataLookup.FullName}').");
            }
        }
        else if (HasConcurrencyToken)
        {
            throw new ArgumentException(
                $"The database model for DbEntityType '{_dbEntityType.FullName}' (resolved as '{typeForMetadataLookup.FullName}') contains concurrency token property '{ConcurrencyTokenPropertyName}' but the entity type '{_entityType.FullName}' does not implement 'IHasChangeToken' or 'IHasChangeTokenEx'.");
        }
    }

    internal static DbModelProperties Get(DbContext dbContext, Type dbEntityType, Type entityType)
    {
        return _cache.GetOrAdd(
            (dbContext.GetType(), DbEntityType: dbEntityType, EntityType: entityType),
            key => new DbModelProperties(dbContext, key.DbEntityType, key.EntityType));
    }
}