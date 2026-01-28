using MoYu.Xunit;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;
using Xunit.Abstractions;

[assembly: TestFramework("MoYu.UnitTests.TestProgram", "MoYu.UnitTests")]

namespace MoYu.UnitTests;

public class TestProgram : TestStartup
{
    public TestProgram(IMessageSink messageSink) : base(messageSink)
    {
        Serve.RunNative(services =>
        {
            services.AddHttpRemote();
        });
    }
}