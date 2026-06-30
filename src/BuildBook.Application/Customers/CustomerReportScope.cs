namespace BuildBook.Application.Customers;

public enum CustomerReportScope
{
    AllCustomers = 0,
    CustomersByContractLevel = 1,
    CustomersWithNoContract = 2,
    ExpiredContracts = 3,
    ContractsExpiringWithinDays = 4,
    OpenRmasByContractLevel = 5,
    OverdueRmasByContractLevel = 6,
    PriorityMismatch = 7,
    MissingSupportTicketNumber = 8
}
