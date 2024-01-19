using System;
using System.Collections.Generic;

namespace AppCore.Data.EntityFrameworkCore;

internal sealed class DbContextDataProviderOptions
{
    public Type? EntityMapperType { get; set; }

    public Type? TokenGeneratorType { get; set; }

    public ISet<Type> QueryHandlerTypes { get; } = new HashSet<Type>();
}