# User And Role Management

BuildBook does not have its own login form or password database.

People sign in with Windows Authentication, and BuildBook then checks which application roles have been assigned to that Windows username.

## Authentication

- Windows Authentication identifies the signed-in user.
- Local development can use the development authentication bypass instead of a live Windows sign-in.
- The current signed-in username and authentication mode are shown in the application header.

## BuildBook roles

BuildBook uses these fixed application roles:

- Administrator
- Editor
- Viewer
- Sensitive Data Viewer

These roles control access to BuildBook features. Permissions do not depend on hard-coded usernames.

## Bootstrap administrators

Bootstrap administrators are configured in app settings so an initial administrator can reach the Users & Roles page before database role assignments are complete.

Example development configuration:

```json
{
  "BuildBook": {
    "Authorization": {
      "UseDevelopmentAuthentication": true,
      "DevelopmentUserName": "AzureAD\\DavidKuziara",
      "BootstrapAdministrators": [
        "AzureAD\\DavidKuziara"
      ]
    }
  }
}
```

While a username remains in `BootstrapAdministrators`, BuildBook always treats that user as an Administrator.

## Add a user

1. Open `Admin > Users & Roles`.
2. Enter the Windows username in `DOMAIN\User` or `AzureAD\User` format.
3. Optionally enter a display name and email address.
4. Save the new user.

## Assign roles

1. Find the user in the Managed users section.
2. Tick the BuildBook roles they need.
3. Save the user.

Administrators can also update display name, email address, and activation status from the same page.

## Lockout protection

- The last Administrator cannot be deactivated.
- The last Administrator role assignment cannot be removed.
- Configured bootstrap administrators keep Administrator access while they remain configured.
