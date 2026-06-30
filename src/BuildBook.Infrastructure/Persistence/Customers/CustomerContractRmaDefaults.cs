using BuildBook.Domain.Customers;
using BuildBook.Domain.Rmas;

namespace BuildBook.Infrastructure.Persistence.Customers;

internal static class CustomerContractRmaDefaults
{
    public static RmaPriority? GetPriority(string supportContractStatus, SupportContractLevel? supportContractLevel)
    {
        return string.Equals(supportContractStatus, CustomerSupportContractStatuses.Active, StringComparison.Ordinal)
            ? supportContractLevel?.DefaultRmaPriority
            : null;
    }

    public static RmaWarrantyStatus? GetWarrantyStatus(string supportContractStatus, SupportContractLevel? supportContractLevel)
    {
        return string.Equals(supportContractStatus, CustomerSupportContractStatuses.Active, StringComparison.Ordinal)
            && supportContractLevel is not null
                ? RmaWarrantyStatus.ExtendedWarranty
                : null;
    }

    public static DateOnly? GetWarrantyExpiryDate(string supportContractStatus, DateOnly? supportContractEndDate)
    {
        return string.Equals(supportContractStatus, CustomerSupportContractStatuses.Active, StringComparison.Ordinal)
            ? supportContractEndDate
            : null;
    }
}
