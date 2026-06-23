namespace BuildBook.Domain.BuildRecords;

public enum SecretType
{
    RadSightUserPassword = 0,
    WindowsAdminPassword = 1,
    KioskPassword = 2,
    WifiPassword = 3,
    RouterPassword = 4,
    BitLockerRecoveryKey = 5
}
