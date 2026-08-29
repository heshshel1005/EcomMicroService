using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace EcomMicroService.Payment.EntityFrameworkCore;

[ConnectionStringName(EcomMicroServiceNames.PaymentDb)]
public interface IPaymentDbContext : IEfCoreDbContext
{
}
