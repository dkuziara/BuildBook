using BuildBook.Domain.Customers;

namespace BuildBook.Application.Customers;

public static class CustomerFormValidator
{
    public static IReadOnlyList<string> Validate(
        string name,
        string supportContractStatus,
        DateOnly? supportContractStartDate,
        DateOnly? supportContractEndDate)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add("Customer name is required.");
        }

        if (string.IsNullOrWhiteSpace(supportContractStatus))
        {
            errors.Add("Support contract status is required.");
        }
        else if (!CustomerSupportContractStatuses.All.Contains(supportContractStatus, StringComparer.Ordinal))
        {
            errors.Add("Support contract status is not recognised.");
        }

        if (supportContractStartDate is not null
            && supportContractEndDate is not null
            && supportContractEndDate < supportContractStartDate)
        {
            errors.Add("Support contract end date cannot be earlier than the start date.");
        }

        return errors;
    }
}
