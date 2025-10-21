using InjectionExpert.Utilities;
using Microsoft.Extensions.Logging;

namespace InjectionExpert.Tests.Utilities;

[TestFixture, TestOf(typeof(InjectionContainerLoggingExtensions))]
public class TestInjectionContainerLoggingExtensions
{
    [Test]
    public void GetLogger_Generic()
    {
        var container = new InjectionContainer()
            .AddLogging(new LoggerFactory());
        var logger = container.RequireInjection<ILogger<TestInjectionContainerLoggingExtensions>>();
        Assert.That(logger, Is.Not.Null);
        Assert.That(logger, Is.TypeOf<Logger<TestInjectionContainerLoggingExtensions>>());
    }
    
    [Test]
    public void GetLogger_NonGeneric()
    {
        var container = new InjectionContainer()
            .AddLogging(new LoggerFactory());
        var logger = container.RequireInjection<ILogger>();
        Assert.That(logger, Is.Not.Null);
    }
}