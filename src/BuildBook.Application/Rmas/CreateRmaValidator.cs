namespace BuildBook.Application.Rmas;

public static class CreateRmaValidator
{
    public static IReadOnlyList<string> Validate(CreateRmaRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.CustomerName))
        {
            errors.Add("Customer is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ProductName))
        {
            errors.Add("Product name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.FaultSummary))
        {
            errors.Add("Fault summary is required.");
        }

        if (string.IsNullOrWhiteSpace(request.InitialFaultDescription))
        {
            errors.Add("Fault description is required.");
        }

        return errors;
    }
}
