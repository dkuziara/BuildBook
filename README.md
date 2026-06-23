# BuildBook

BuildBook is an internal web application for replacing the existing **Product Serial Numbers** spreadsheet.

The current spreadsheet stores product serial numbers, build information, customer details, software and firmware versions, shipping information, notes, passwords and BitLocker recovery keys. BuildBook will provide a cleaner, safer and easier way to search, view and update this information.

## Purpose

BuildBook is intended to help internal staff:

* Find product/device records quickly.
* Search across most non-sensitive fields.
* Add new Build Records.
* Update existing Build Records.
* View each product/device in clear sections.
* Protect sensitive information such as passwords and BitLocker recovery keys.
* Export non-sensitive reports.
* Replace day-to-day use of the spreadsheet.

## Technology

BuildBook Version 1 will be built using:

* ASP.NET Core Blazor Web App
* Interactive Server rendering
* C#
* Razor components
* SQL Server
* Entity Framework Core

The application should not use React, Angular, Vue, Node.js, or a separate JavaScript front end.

Small amounts of JavaScript interop may be used only where there is a clear technical reason.

## Documentation

Project documentation is stored in the `docs` folder.

* [BuildBook Version 1 Specification](docs/specs/BuildBook-Version-1-Specification.md)
* [BuildBook Version 1 Backlog](docs/backlog/BuildBook-Backlog.md)
* [Codex Instructions](AGENTS.md)

## Version 1 Scope

Version 1 focuses on replacing the spreadsheet with a practical internal web application.

Included in Version 1:

* User login
* Build Register
* Build Record create/view/edit screens
* Search across non-sensitive fields
* Spreadsheet import
* Basic reports
* CSV/Excel export without sensitive fields
* Secure handling of passwords and BitLocker recovery keys
* Basic audit history

Out of scope for Version 1:

* Customer access
* Public access
* Mobile app
* Freshdesk integration
* Azure DevOps integration
* Automated firmware detection
* Automated device discovery
* Complex stock control
* Complex workflow automation

## Build Record

Each row from the existing spreadsheet will become one **Build Record**.

A Build Record represents one physical product/device and includes sections such as:

* Product details
* Build details
* Customer and shipping details
* Hardware details
* Software and firmware details
* Network details
* Credentials and recovery information
* Notes
* History

## Security Notes

BuildBook may contain sensitive internal information, including:

* Windows admin passwords
* Kiosk passwords
* Wi-Fi passwords
* Router passwords
* BitLocker recovery keys

Sensitive values must:

* Be stored encrypted
* Be masked by default
* Be excluded from normal search
* Be excluded from normal exports
* Never be written to logs
* Never be stored as plain text in audit history
* Only be revealed to authorised users
* Be audited when viewed or changed

## Development Status

This project is currently in initial setup.

The first implementation tasks are:

1. Create the solution structure.
2. Create a runnable ASP.NET Core Blazor Web App using Interactive Server rendering.
3. Add base navigation and layout.
4. Add configuration and logging.
5. Add SQL Server and Entity Framework Core setup.

See the backlog document for the full ordered implementation plan.

## Local Development

Local development instructions will be added once the initial solution has been created.

Expected future commands:

```bash
dotnet build
dotnet run --project src/BuildBook.Web/BuildBook.Web.csproj
```

## Repository Guidance

Before implementing work items, read:

* `AGENTS.md`
* `docs/specs/BuildBook-Version-1-Specification.md`
* `docs/backlog/BuildBook-Backlog.md`

Implementation should follow the GitHub Issues in order and should not jump ahead to later backlog items unless a small supporting change is necessary.

