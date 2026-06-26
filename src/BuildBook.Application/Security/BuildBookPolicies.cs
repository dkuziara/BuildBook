namespace BuildBook.Application.Security;

public static class BuildBookPolicies
{
    public const string ViewBuildRecords = "BuildBook.ViewBuildRecords";
    public const string EditBuildRecords = "BuildBook.EditBuildRecords";
    public const string RevealSensitiveData = "BuildBook.RevealSensitiveData";
    public const string ManageSensitiveData = "BuildBook.ManageSensitiveData";
    public const string ImportSpreadsheet = "BuildBook.ImportSpreadsheet";
    public const string ExportNonSensitiveData = "BuildBook.ExportNonSensitiveData";
    public const string ManageUsers = "BuildBook.ManageUsers";
    public const string DeleteRecords = "BuildBook.DeleteRecords";
}
