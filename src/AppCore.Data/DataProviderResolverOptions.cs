using System;
using System.Collections.Generic;

namespace AppCore.Data;

internal sealed class DataProviderResolverOptions
{
    public IDictionary<string, Type> ProviderMap { get; } = new Dictionary<string, Type>();
}