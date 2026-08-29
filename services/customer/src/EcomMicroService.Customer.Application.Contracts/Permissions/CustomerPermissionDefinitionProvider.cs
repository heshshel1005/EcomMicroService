using EcomMicroService.Customer.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace EcomMicroService.Customer.Permissions;

public class CustomerPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        context.AddGroup(CustomerPermissions.GroupName, L("Permission:Customer"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<CustomerResource>(name);
    }
}
