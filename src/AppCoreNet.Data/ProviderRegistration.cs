// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;

namespace AppCoreNet.Data;

internal sealed class ProviderRegistration
{
    public string Name { get; }

    public Type ProviderType { get; }

    public ProviderRegistration(string name, Type providerType)
    {
        Name = name;
        ProviderType = providerType;
    }
}