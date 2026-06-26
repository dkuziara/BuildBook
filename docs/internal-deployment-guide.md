# Internal Deployment Guide

This guide covers a practical Version 1 deployment of BuildBook to IIS or another internal Windows Server host.

BuildBook is an internal ASP.NET Core Blazor Web App that uses:

- Windows Authentication
- SQL Server
- Server-side secret storage and encryption
- Automatic EF Core database migration on startup

## Recommended hosting model

Use one of these internal hosting options:

- IIS on an internal Windows Server
- An internal Windows Server running the published ASP.NET Core app behind IIS

For Version 1, IIS with Windows Authentication enabled is the simplest supported deployment path.

## Before you deploy

Make sure the target environment has:

- .NET 10 hosting support installed
- Access to the target SQL Server instance
- Windows Authentication available for the application
- A folder for ASP.NET Core Data Protection keys
- A service account or application pool identity with access to:
  - the published application folder
  - the Data Protection key folder
  - the SQL Server database

## Publish the application

From the repository root:

```powershell
dotnet publish .\src\BuildBook.Web\BuildBook.Web.csproj -c Release -o .\publish\BuildBook.Web
```

Copy the published output to the deployment server.

## IIS setup

Create or update an IIS site or application for BuildBook.

Recommended IIS settings:

- Application pool: `No Managed Code`
- Authentication:
  - `Windows Authentication`: enabled
  - `Anonymous Authentication`: disabled unless there is a specific internal reverse-proxy requirement
- HTTPS binding enabled

BuildBook requires authenticated internal users. Do not add a separate BuildBook login form.

## Production configuration

Create an environment-specific `appsettings.Production.json` or use environment variables.

Example:

```json
{
  "BuildBook": {
    "ApplicationName": "BuildBook",
    "SupportContact": "Internal IT",
    "EnableDetailedErrors": false,
    "SeedDevelopmentData": false,
    "DefaultPageSize": 25,
    "DataProtectionKeyDirectory": "C:\\BuildBook\\Keys",
    "Authorization": {
      "BootstrapAdministrators": [
        "DOMAIN\\BuildBookAdmin"
      ]
    }
  },
  "ConnectionStrings": {
    "BuildBookDatabase": "Server=SQLSERVER01;Database=BuildBook;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

## Required configuration notes

- `BuildBook:EnableDetailedErrors` should stay `false` in deployed environments.
- `BuildBook:SeedDevelopmentData` should stay `false` in deployed environments.
- `BuildBook:Authorization:UseDevelopmentAuthentication` should not be enabled in deployed environments.
- `BuildBook:Authorization:BootstrapAdministrators` should include at least one trusted Windows username so admin access can be initialized safely.
- `ConnectionStrings:BuildBookDatabase` should point to the shared SQL Server database used by the deployed app.
- `BuildBook:DataProtectionKeyDirectory` should point to a durable server folder, not a temporary location.

## Database behavior

BuildBook applies pending EF Core migrations automatically during startup.

Deployment implications:

- The application pool identity must be able to connect to SQL Server.
- The application pool identity must be able to create or update schema objects during deployment startup.
- If your environment requires stricter change control, run migrations in a controlled release step before switching traffic to the new build.

## Data Protection keys

Sensitive features rely on ASP.NET Core Data Protection.

For a stable deployment:

- persist keys to a server folder such as `C:\BuildBook\Keys`
- grant the IIS application pool identity access to that folder
- keep the key directory backed up with normal server backups

Do not leave keys in an ephemeral location on a server that may be rebuilt or rotated.

## Windows Authentication and roles

BuildBook uses Windows Authentication to identify users.

Access inside the app is then controlled by BuildBook roles:

- Administrator
- Editor
- Viewer
- Sensitive Data Viewer

There is no BuildBook username/password store.

Bootstrap administrators configured in `BuildBook:Authorization:BootstrapAdministrators` are treated as Administrators even before database role assignments are complete.

After deployment:

1. Sign in as a bootstrap administrator.
2. Open `Admin > Users & Roles`.
3. Add Windows users in `DOMAIN\User` or `AzureAD\User` format.
4. Assign the required BuildBook roles.

## Logging and secrets

BuildBook is designed so sensitive values are not logged in plain text.

For deployment:

- keep standard application logging enabled
- do not enable development error pages
- do not add custom logging that records decrypted secrets
- do not expose sensitive values in exports or diagnostic tooling

## Post-deployment checks

After deployment, confirm:

1. The site opens over HTTPS.
2. Windows Authentication signs the user in automatically.
3. A bootstrap administrator can access `Admin > Users & Roles`.
4. Non-admin users cannot access the admin page.
5. The Build Register loads.
6. Reports export without including sensitive fields.
7. Secret values remain masked until explicitly revealed by an authorized user.
8. The header shows the signed-in username and authentication mode.

## Operational notes

- Keep `ASPNETCORE_ENVIRONMENT` set to a non-development value such as `Production`.
- The Development environment should not be enabled on deployed servers.
- Back up both the SQL Server database and the Data Protection key directory.
- Use normal internal patching, certificate, and IIS maintenance processes for the host server.
