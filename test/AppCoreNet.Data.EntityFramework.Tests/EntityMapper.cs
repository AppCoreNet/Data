// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;

namespace AppCoreNet.Data.EntityFramework;

/// <summary>
/// A simple pass-through entity mapper for testing purposes.
/// Assumes domain entity and DB entity are the same type or directly mappable.
/// </summary>
public class TestEntityMapper : IEntityMapper
{
    public TTarget Map<TTarget>(object source)
    {
        if (source == null)
            return default!; // Or throw ArgumentNullException if preferred

        // For basic tests, assume TTarget is the same as source type or directly assignable.
        // More complex mapping would require a real mapping library or manual implementation.
        if (source is TTarget target)
        {
            return target;
        }

        // Attempt a simple property-based mapping if types are different but compatible for basic tests.
        // This is a very naive implementation.
        var targetInstance = Activator.CreateInstance<TTarget>();
        var sourceProperties = source.GetType().GetProperties();
        var targetProperties = typeof(TTarget).GetProperties();

        foreach (var sourceProp in sourceProperties)
        {
            var targetProp = Array.Find(targetProperties, p => p.Name == sourceProp.Name && p.PropertyType == sourceProp.PropertyType);
            if (targetProp != null && targetProp.CanWrite)
            {
                targetProp.SetValue(targetInstance, sourceProp.GetValue(source));
            }
        }
        return targetInstance;
    }

    public void Map(object source, object target)
    {
        if (source == null || target == null)
            return; // Or throw

        // Naive property copy
        var sourceProperties = source.GetType().GetProperties();
        var targetProperties = target.GetType().GetProperties();

        foreach (var sourceProp in sourceProperties)
        {
            var targetProp = Array.Find(targetProperties, p => p.Name == sourceProp.Name && p.PropertyType == sourceProp.PropertyType);
            if (targetProp != null && targetProp.CanWrite)
            {
                targetProp.SetValue(target, sourceProp.GetValue(source));
            }
        }
    }
}
