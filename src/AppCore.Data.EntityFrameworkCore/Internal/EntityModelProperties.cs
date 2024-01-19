// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AppCore.Data.EntityFrameworkCore;

internal class EntityModelProperties<TId, TEntity>
    where TEntity : class, IEntity<TId>
{
    public Func<TId, object?[]> GetIdValues { get; }

    public IReadOnlyList<string> IdPropertyNames { get; }

    public EntityModelProperties()
    {
        Type entityType = typeof(TEntity);

        Type entityIfaceType =
            entityType.GetInterfaces()
                      .First(f => f.GetGenericTypeDefinition() == typeof(IEntity<>));

        Type idType = entityIfaceType.GenericTypeArguments[0];
        switch (Type.GetTypeCode(idType))
        {
            case TypeCode.Object:
            {
                PropertyInfo[] idProperties = idType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

                IdPropertyNames = idProperties.Select(p => p.Name)
                                              .ToList()
                                              .AsReadOnly();

                Func<TId, object?>[] idPropertyGetters =
                    idProperties
                        .Select(p => new Func<TId, object?>(o => p.GetValue(o)))
                        .ToArray();

                GetIdValues = id =>
                {
                    object?[] result = new object[idPropertyGetters.Length];
                    for (int i = 0; i < idPropertyGetters.Length; i++)
                    {
                        result[i] = idPropertyGetters[i](id);
                    }

                    return result;
                };
            }
                break;

            default:
                IdPropertyNames = new List<string>().AsReadOnly();
                GetIdValues = id => new object?[] { id };
                break;
        }
    }
}