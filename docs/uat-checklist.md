# User Acceptance Testing Checklist

This checklist is for internal acceptance testing before BuildBook replaces the existing spreadsheet for normal day-to-day use.

The checklist focuses on the core Version 1 workflows:

- import
- search
- edit
- secrets
- reports
- export

## Test setup

Before starting UAT, confirm:

- the latest BuildBook build is deployed to the test environment
- a representative import file is available
- at least one Administrator, Editor, Viewer, and Sensitive Data Viewer account is available
- the SQL Server database is reset or in a known test state

## Import

- Upload a representative spreadsheet file.
- Confirm column mapping can be reviewed.
- Confirm preview and validation steps load correctly.
- Confirm warnings and errors are shown clearly.
- Confirm a successful import creates Build Records.
- Confirm sensitive spreadsheet fields are stored separately from normal Build Record fields.
- Confirm import history records the batch summary.

## Search

- Search by serial number.
- Search by customer.
- Search by invoice number.
- Search by machine name.
- Search by RadSight version.
- Confirm partial matching works where expected.
- Confirm sensitive values are not searchable.
- Confirm a search result opens the correct Build Record.

## Edit

- Create a new Build Record with required fields only.
- Edit product details and confirm the changes save.
- Edit customer and shipping details and confirm the changes save.
- Edit software and firmware details and confirm the changes save.
- Confirm validation errors appear when invalid values are entered.
- Confirm saved changes appear in the audit history.

## Secrets

- Confirm secret values are masked by default.
- Confirm a Viewer cannot reveal secret values.
- Confirm a Sensitive Data Viewer or Administrator can reveal secret values.
- Confirm revealing a secret creates an audit entry without storing the secret value in plain text.
- Confirm secret values do not appear in normal list views.

## Reports

- Open the Reports page.
- Confirm customer reports load without showing sensitive data.
- Confirm missing-data reports load.
- Confirm version reports load.
- Confirm report links open matching Build Register results.

## Export

- Export Build Register results to CSV.
- Export Build Register results to Excel.
- Confirm exported files contain the expected non-sensitive fields.
- Confirm exported files do not contain passwords or BitLocker recovery keys.

## Authorization

- Confirm an Administrator can open `Admin > Users & Roles`.
- Confirm a non-Administrator cannot open the admin page.
- Confirm the signed-in username is shown in the header.
- Confirm the authentication mode is shown in the header.

## Acceptance decision

BuildBook is ready for acceptance when:

- critical workflows complete successfully
- sensitive values remain protected
- role-based access works as expected
- exports and reports remain non-sensitive
- users agree the system is suitable for internal daily use

## Sign-off record

Suggested sign-off details:

- test environment
- test date
- testers
- issues found
- decision: accepted, accepted with follow-up actions, or not accepted
