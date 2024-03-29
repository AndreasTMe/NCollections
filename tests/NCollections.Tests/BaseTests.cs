using System;
using System.Diagnostics.CodeAnalysis;

using Xunit.Abstractions;

namespace NCollections.Tests;

public class BaseTests : IDisposable
{
    [ExcludeFromCodeCoverage]
    protected ITestOutputHelper Output { get; }

    protected BaseTests(ITestOutputHelper output) => Output = output;

    public virtual void Dispose() { }
}