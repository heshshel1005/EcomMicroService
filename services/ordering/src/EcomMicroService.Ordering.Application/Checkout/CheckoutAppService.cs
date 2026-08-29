using System;
using System.Linq;
using System.Threading.Tasks;
using EcomMicroService.Ordering.Checkout;
using EcomMicroService.Ordering.Orders;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace EcomMicroService.Ordering;

[Volo.Abp.RemoteService(IsEnabled = false)]
public class CheckoutAppService : ApplicationService, ICheckoutAppService
{
    private readonly IShippingTaxCalculationService _shippingTax;
    private readonly IRepository<Order, Guid> _orderRepository;
    private readonly IRepository<OrderStatusHistory, Guid> _historyRepository;
    private readonly ShopIntegrationClients _shop;

    public CheckoutAppService(
        IShippingTaxCalculationService shippingTax,
        IRepository<Order, Guid> orderRepository,
        IRepository<OrderStatusHistory, Guid> historyRepository,
        ShopIntegrationClients shop)
    {
        _shippingTax = shippingTax;
        _orderRepository = orderRepository;
        _historyRepository = historyRepository;
        _shop = shop;
    }

    [AllowAnonymous]
    public async Task<CheckoutSummaryDto> GetSummaryAsync(Guid? guestCartId = null, string? couponCode = null)
    {
        var cart = await _shop.GetCartAsync(guestCartId);
        var subtotal = cart.Items == null || cart.Items.Count == 0
            ? 0
            : cart.Items.Sum(i => (i.UnitPrice ?? 0) * i.Quantity);
        var (coupon, discount) = await ResolveCouponAsync(couponCode, subtotal);
        var shippingOptions = await _shippingTax.GetShippingOptionsAsync(subtotal);
        var taxAmount = await _shippingTax.CalculateTaxAsync(subtotal);

        return new CheckoutSummaryDto
        {
            Cart = MapCart(cart),
            SubTotal = subtotal,
            DiscountAmount = discount,
            AppliedCouponCode = coupon?.Code,
            ShippingOptions = shippingOptions,
            TaxAmount = taxAmount,
            DefaultShippingMethodCode = shippingOptions.FirstOrDefault()?.Code,
        };
    }

    [AllowAnonymous]
    public async Task<SubmitCheckoutResultDto> SubmitOrderAsync(SubmitCheckoutDto input, Guid? guestCartId = null)
    {
        var cart = await _shop.GetCartAsync(guestCartId);
        if (cart.Items == null || cart.Items.Count == 0)
            throw new Volo.Abp.BusinessException("ECommerce:CartEmpty").WithData("Message", "Cart is empty.");

        foreach (var item in cart.Items)
            await _shop.ValidateVariantAsync(item.ProductVariantId, item.Quantity);

        var subtotal = cart.Items.Sum(i => (i.UnitPrice ?? 0) * i.Quantity);
        var shippingOptions = await _shippingTax.GetShippingOptionsAsync(subtotal);
        var selected = shippingOptions.FirstOrDefault(x => x.Code == input.ShippingMethodCode)
            ?? shippingOptions.FirstOrDefault()
            ?? throw new Volo.Abp.BusinessException("ECommerce:InvalidShippingMethod");
        var taxAmount = await _shippingTax.CalculateTaxAsync(subtotal);
        var (coupon, discount) = await ResolveCouponAsync(input.CouponCode, subtotal);
        var total = Math.Max(0, subtotal - discount) + selected.Amount + taxAmount;
        var shipping = input.ShippingAddress ?? throw new Volo.Abp.BusinessException("ECommerce:ShippingAddressRequired");

        var order = new Order(
            GuidGenerator.Create(),
            CurrentUser.Id,
            input.ContactEmail,
            input.ContactPhone,
            input.ContactName,
            shipping.Street,
            shipping.Street2,
            shipping.City,
            shipping.Region,
            shipping.PostalCode,
            shipping.Country,
            shipping.DeliveryInstructions,
            input.BillingSameAsShipping,
            selected.Code,
            selected.Name,
            selected.Amount,
            taxAmount,
            subtotal,
            total,
            coupon?.Id,
            discount,
            input.BillingSameAsShipping ? null : input.BillingAddress?.Street,
            input.BillingSameAsShipping ? null : input.BillingAddress?.Street2,
            input.BillingSameAsShipping ? null : input.BillingAddress?.City,
            input.BillingSameAsShipping ? null : input.BillingAddress?.Region,
            input.BillingSameAsShipping ? null : input.BillingAddress?.PostalCode,
            input.BillingSameAsShipping ? null : input.BillingAddress?.Country);

        foreach (var item in cart.Items)
        {
            order.AddLine(new OrderLine(
                GuidGenerator.Create(),
                order.Id,
                item.ProductVariantId,
                item.ProductId,
                item.ProductName,
                item.Sku,
                item.UnitPrice ?? 0,
                item.Quantity));
        }

        await _orderRepository.InsertAsync(order);
        await _historyRepository.InsertAsync(new OrderStatusHistory(GuidGenerator.Create(), order.Id, OrderStatus.Pending));

        if (coupon != null)
        {
            try { await _shop.RecordCouponUsageAsync(coupon.Id, CurrentUser.Id, order.Id); } catch { /* optional */ }
        }

        var lines = cart.Items.Select(i => new ShopIntegrationClients.InventoryLineDto
        {
            ProductVariantId = i.ProductVariantId,
            Quantity = i.Quantity
        }).ToList();
        try { await _shop.ReleaseReservationsAsync(lines); } catch { /* still clear cart */ }
        await _shop.ClearCartAsync(guestCartId);

        return new SubmitCheckoutResultDto { OrderId = order.Id, Status = order.Status.ToString() };
    }

    private static CheckoutCartDto MapCart(ShopIntegrationClients.RemoteCartDto cart) => new()
    {
        Id = cart.Id,
        ItemCount = cart.ItemCount,
        Items = (cart.Items ?? []).Select(i => new CheckoutCartItemDto
        {
            Id = i.Id,
            ProductVariantId = i.ProductVariantId,
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            Sku = i.Sku,
            UnitPrice = i.UnitPrice,
            Quantity = i.Quantity
        }).ToList()
    };

    private async Task<(ShopIntegrationClients.CouponPreviewDto? coupon, decimal discount)> ResolveCouponAsync(string? code, decimal subtotal)
    {
        if (string.IsNullOrWhiteSpace(code)) return (null, 0);
        try
        {
            var preview = await _shop.PreviewCouponAsync(code, subtotal);
            if (preview == null || !preview.IsValid) return (null, 0);
            return (preview, preview.DiscountAmount);
        }
        catch
        {
            return (null, 0);
        }
    }
}
