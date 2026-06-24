namespace BuildBook.Application.BuildRecords;

public static class UpdateProductDetailsValidator
{
    public static IReadOnlyList<string> Validate(UpdateProductDetailsRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.ProductCode))
        {
            errors.Add("Product code is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ProductName))
        {
            errors.Add("Product name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.SerialNumber))
        {
            errors.Add("Serial number is required.");
        }

        return errors;
    }
}
