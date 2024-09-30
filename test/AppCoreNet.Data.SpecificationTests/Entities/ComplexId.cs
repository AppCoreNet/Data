// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;

namespace AppCoreNet.Data.Entities;

public class ComplexId
{
    public Guid Id { get; set; }

    public int Version { get; set; }
}