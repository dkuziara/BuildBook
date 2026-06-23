namespace BuildBook.Application.Security;

public static class BuildBookRoles
{
    public const string Administrator = "Administrator";
    public const string Editor = "Editor";
    public const string Viewer = "Viewer";
    public const string SensitiveDataViewer = "Sensitive Data Viewer";

    public static readonly string[] All =
    [
        Administrator,
        Editor,
        Viewer,
        SensitiveDataViewer
    ];
}
