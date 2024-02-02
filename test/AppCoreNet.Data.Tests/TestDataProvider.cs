// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

namespace AppCoreNet.Data;

public class TestDataProvider : IDataProvider
{
    public string Name { get; }

    public ITransactionManager? TransactionManager => null;

    public TestDataProvider(string name)
    {
        Name = name;
    }
}