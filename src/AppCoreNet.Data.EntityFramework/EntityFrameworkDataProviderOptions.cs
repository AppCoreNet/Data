// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Collections.Generic;

namespace AppCoreNet.Data.EntityFramework;

internal sealed class EntityFrameworkDataProviderOptions
{
    public Type? EntityMapperType { get; set; }

    public Type? TokenGeneratorType { get; set; }

    public ISet<Type> QueryHandlerTypes { get; } = new HashSet<Type>();
}