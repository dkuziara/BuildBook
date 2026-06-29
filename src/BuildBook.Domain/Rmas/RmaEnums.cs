namespace BuildBook.Domain.Rmas;

public enum RmaStatus
{
    BookedIn = 1,
    WorkInProgress = 2,
    ReadyToShip = 3,
    Shipped = 4,
    OnHold = 5,
    CancelledNoReply = 6,
    CustomerFixed = 7,
    Closed = 8
}

public enum RmaPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Urgent = 4
}

public enum RmaWarrantyStatus
{
    InWarranty = 1,
    OutOfWarranty = 2,
    ExtendedWarranty = 3,
    WarrantyUnknown = 4,
    NotApplicable = 5
}

public enum RmaFaultCategory
{
    HardwareFailure = 1,
    SoftwareIssue = 2,
    FirmwareIssue = 3,
    DiskStorageIssue = 4,
    PowerIssue = 5,
    NetworkIssue = 6,
    ConfigurationIssue = 7,
    LicensingIssue = 8,
    UserCustomerSetupIssue = 9,
    PhysicalDamage = 10,
    NoFaultFound = 11,
    Unknown = 12
}

public enum RmaYesNoUnknown
{
    Yes = 1,
    No = 2,
    Unknown = 3
}

public enum RmaCustomerImpact
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum RmaRootCauseCategory
{
    ComponentFailure = 1,
    DiskStorageFailure = 2,
    PowerSupplyIssue = 3,
    CorruptSoftwareConfiguration = 4,
    FirmwareIssue = 5,
    LicensingConfigurationMissing = 6,
    CustomerEnvironmentIssue = 7,
    PhysicalDamage = 8,
    WearAndTear = 9,
    NoFaultFound = 10,
    Unknown = 11
}

public enum RmaTestResult
{
    Pass = 1,
    Fail = 2,
    NotTested = 3
}

public enum RmaQaResult
{
    Pass = 1,
    Fail = 2,
    NotRequired = 3
}

public enum RmaOutcome
{
    RepairedAndReturned = 1,
    ReplacedAndReturned = 2,
    NoFaultFound = 3,
    CustomerFixed = 4,
    Cancelled = 5,
    Scrapped = 6,
    ReturnedUnrepaired = 7,
    AwaitingCustomerResponseThenClosed = 8,
    Other = 9
}

public enum RmaNoteType
{
    InternalNote = 1,
    DiagnosisNote = 2,
    RepairNote = 3,
    CustomerNote = 4,
    CommercialNote = 5
}
