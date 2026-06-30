using BuildBook.Application.Rmas;

namespace BuildBook.Tests;

public class CreateRmaValidatorTests
{
    [Fact]
    public void Validate_ReturnsExpectedErrorsForMissingRequiredFields()
    {
        var errors = CreateRmaValidator.Validate(new CreateRmaRequest());

        Assert.Equal(4, errors.Count);
        Assert.Contains("Customer is required.", errors);
        Assert.Contains("Product name is required.", errors);
        Assert.Contains("Fault summary is required.", errors);
        Assert.Contains("Fault description is required.", errors);
    }

    [Fact]
    public void Validate_AllowsOptionalPlannerMigrationFields()
    {
        var errors = CreateRmaValidator.Validate(new CreateRmaRequest
        {
            CustomerName = "Acme Medical",
            ProductName = "RadSight Access Terminal",
            FaultSummary = "No power",
            InitialFaultDescription = "Unit does not power on.",
            MigrationSource = "Planner manual recreation",
            OriginalPlannerTaskTitle = "Acme terminal return",
            OriginalPlannerNotes = "Customer advised that the unit failed after transport."
        });

        Assert.Empty(errors);
    }
}
