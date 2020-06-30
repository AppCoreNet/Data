// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AppCore.Data.EntityFrameworkCore
{
    internal class DbModelProperties
    {
        private static readonly ConcurrentDictionary<ValueTuple<Type, Type>, DbModelProperties> Properties =
            new ConcurrentDictionary<(Type, Type), DbModelProperties>();

        public IReadOnlyList<string> PrimaryKeyPropertyNames { get; }

        public bool HasConcurrencyToken { get; }

        public string ConcurrencyTokenPropertyName { get; }

        private DbModelProperties(IModel model, Type dbEntityType)
        {
            IEntityType modelEntityType = model.FindEntityType(dbEntityType);

            PrimaryKeyPropertyNames = modelEntityType
                                      .FindPrimaryKey()
                                      .Properties.Select(p => p.Name)
                                      .ToList();

            IProperty concurrencyToken = modelEntityType.FindProperty(nameof(IHasConcurrencyToken.ConcurrencyToken));
            if ((concurrencyToken?.IsConcurrencyToken).GetValueOrDefault())
            {
                HasConcurrencyToken = true;
                ConcurrencyTokenPropertyName = concurrencyToken.Name;
            }
        }

        public static DbModelProperties Get(Type dbContextType, Type dbEntityType, IModel model, Type entityType)
        {
            return Properties.GetOrAdd((dbContextType, dbEntityType), t =>
            {
                var properties = new DbModelProperties(model, t.Item2);

                if (typeof(IHasConcurrencyToken).IsAssignableFrom(entityType))
                {
                    if (!properties.HasConcurrencyToken)
                    {
                        throw new ArgumentException(
                            "The entity implements 'IHasConcurrencyToken' but no matching property was found in the database model.");
                    }
                }
                else if (properties.HasConcurrencyToken)
                {
                    throw new ArgumentException(
                        "The database model contains concurrency token but the entity does not implement 'IHasConcurrencyToken'.");
                }

                return properties;

            });
        }
    }
}