using System;

using Xunit.Abstractions;

namespace UnsafeCollections.Tests;

public class BaseTests : IDisposable
{
    protected ITestOutputHelper Output { get; }

    protected BaseTests(ITestOutputHelper output) => Output = output;

    public virtual void Dispose() { }
}