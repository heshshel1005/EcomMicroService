using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.Hosting;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddSharedEndpoints(this IHostApplicationBuilder builder)
    {
        builder.AddRedisDistributedCache(connectionName: EcomMicroServiceNames.Redis);
        builder.AddSeqEndpoint(connectionName: EcomMicroServiceNames.Seq);

        return builder;
    }
}
