# BuildBook — Codex Instructions

BuildBook is an internal web application replacing the Product Serial Numbers spreadsheet.

Before implementing any issue, read:

- docs/specs/BuildBook-Version-1-Specification.md
- docs/backlog/BuildBook-Backlog.md
- The current GitHub issue being worked on

For RMA module work, also read:

- docs/specs/BuildBook-RMA-Module-Specification.md

## Technology

Use:

- ASP.NET Core Blazor Web App
- Interactive Server rendering
- C#
- Razor components
- SQL Server
- Entity Framework Core

Do not use:

- React
- Angular
- Vue
- Node.js backend
- A separate JavaScript front end

Small JavaScript interop is allowed only if there is a clear reason.

## Working rules

- Implement only the GitHub issue being worked on.
- Do not jump ahead to later backlog items.
- Keep changes small and reviewable.
- Prefer simple, maintainable code over clever abstractions.
- Keep UI clear and suitable for internal business users.
- Do not introduce public/customer-facing features.
- Do not implement out-of-scope features unless explicitly requested.

## Security rules

BuildBook may contain sensitive internal data including:

- Windows admin passwords
- Kiosk passwords
- Wi-Fi passwords
- Router passwords
- BitLocker recovery keys

Sensitive values must:

- Be stored separately from normal Build Record fields
- Be encrypted at rest
- Be masked by default
- Never be included in normal search
- Never be included in normal exports
- Never be written to logs
- Never be stored in audit history as plain text

Viewing or changing sensitive values must be audited.

## Build and test expectations

Before finishing a task, run where applicable:

```bash
dotnet build
dotnet test
```

For UI work, confirm the Blazor app runs locally.

# Completion summary

When finishing a task, summarise:

- What changed
- Files changed
- Commands run
- Tests added or updated
- Any follow-up issues needed




