using BuildBook.Domain.Customers;
using BuildBook.Domain.Rmas;

namespace BuildBook.Application.Customers;

public sealed record SupportContractLevelModel(
    int Id,
    string Name,
    string? Description,
    int? TargetResponseTimeValue,
    SupportResponseTimeUnit? TargetResponseTimeUnit,
    RmaPriority? DefaultRmaPriority,
    int RmaPriorityWeight,
    int DisplayOrder,
    bool IsActive);
