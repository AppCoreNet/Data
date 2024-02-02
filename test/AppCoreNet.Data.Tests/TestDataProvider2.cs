// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

namespace AppCoreNet.Data;

public class TestDataProvider2 : IDataProvider
{
    public string Name { get; }

    public ITransactionManager? TransactionManager => null;

    public TestDataProvider2(string name)
    {
        Name = name;
    }
}