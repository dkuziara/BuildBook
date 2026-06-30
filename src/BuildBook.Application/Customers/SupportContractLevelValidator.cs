namespace BuildBook.Application.Customers;

public static class SupportContractLevelValidator
{
    public static IReadOnlyList<string> Validate(
        string name,
        int? targetResponseTimeValue,
        object? targetResponseTimeUnit)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add("Level name is required.");
        }

        if (targetResponseTimeValue is < 0)
        {
            errors.Add("Target response time must be zero or greater.");
        }

        if (targetResponseTimeValue is not null && targetResponseTimeUnit is null)
        {
            errors.Add("Choose a response time unit when a target response time is entered.");
        }

        return errors;
    }
}
