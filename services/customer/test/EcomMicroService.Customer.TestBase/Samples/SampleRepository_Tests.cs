using Volo.Abp.Modularity;
using Xunit;

namespace EcomMicroService.Customer.Samples;

/* Write your custom repository tests like that, in this project, as abstract classes.
 * Then inherit these abstract classes from EF Core & MongoDB test basket.
 * In this way, both database providers are tests with the same set tests.
 */
public abstract class SampleRepository_Tests<TStartupModule> : BasketTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    //private readonly ISampleRepository _sampleRepository;

    protected SampleRepository_Tests()
    {
        //_sampleRepository = GetRequiredService<ISampleRepository>();
    }

    [Fact]
    public void Method1()
    {
        Assert.Fail("Not implemented!");
    }
}
