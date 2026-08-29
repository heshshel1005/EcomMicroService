using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace EcomMicroService.Payment.EntityFrameworkCore;

[ConnectionStringName(EcomMicroServiceNames.PaymentDb)]
public class PaymentDbContext(DbContextOptions<PaymentDbContext> options)
    : AbpDbContext<PaymentDbContext>(options),
        IPaymentDbContext
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ConfigurePayment();
    }
}
