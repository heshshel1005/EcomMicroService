using Microsoft.EntityFrameworkCore;
using Volo.Abp;

namespace EcomMicroService.Payment.EntityFrameworkCore;

public static class PaymentDbContextModelCreatingExtensions
{
    public static void ConfigurePayment(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));
    }
}
