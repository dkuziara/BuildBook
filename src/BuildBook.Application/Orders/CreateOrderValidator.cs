namespace BuildBook.Application.Orders;

public static class CreateOrderValidator
{
    public static IReadOnlyList<string> Validate(CreateOrderRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.OrderTitle))
        {
            errors.Add("Order title is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Status))
        {
            errors.Add("Status is required.");
        }

        if (request.Priority is null)
        {
            errors.Add("Priority is required.");
        }

        if (request.StartDate is not null
            && request.DueDate is not null
            && request.DueDate.Value < request.StartDate.Value)
        {
            errors.Add("Due date cannot be before the start date.");
        }

        return errors;
    }
}
