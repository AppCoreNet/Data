using System;
using System.Collections.Generic;

namespace AppCoreNet.Data;

internal sealed class DataProviderResolverOptions
{
    public IDictionary<string, Type> ProviderMap { get; } = new Dictionary<string, Type>();
}