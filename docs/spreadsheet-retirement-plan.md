# Spreadsheet Retirement Plan

This plan describes how the old spreadsheet should move to read-only or archived status once BuildBook has been accepted for normal use.

The goal is to avoid two live systems drifting apart after acceptance.

## Retirement principles

- BuildBook becomes the primary system for day-to-day lookup and updates.
- The old spreadsheet is no longer used as the operational source of truth after acceptance.
- Historical access is preserved in a controlled way.

## Preconditions

Before retiring the spreadsheet, confirm:

- user acceptance testing has completed
- BuildBook has been accepted for internal use
- the latest required spreadsheet data has been imported
- any important import warnings or cleanup actions have been reviewed
- backup and restore arrangements are in place

## Recommended transition approach

Use one of these internal approaches:

- mark the spreadsheet read-only for normal users
- move the spreadsheet to an archive location with restricted edit rights
- keep one retained archival copy for audit or historical reference

The important point is that the spreadsheet should no longer be editable as the live operational record after acceptance.

## Transition steps

1. Complete final UAT sign-off.
2. Perform a final verified spreadsheet import if required.
3. Confirm key BuildBook reports, search, and exports work in the accepted environment.
4. Inform internal users that BuildBook is now the primary system.
5. Change the spreadsheet to read-only or move it to an archive location.
6. Keep an archival copy under controlled access.
7. Update any internal team instructions that still point users to the spreadsheet first.

## Archive expectations

When archiving the spreadsheet:

- store it in an internal controlled location
- preserve its original file name and date context where useful
- restrict edit permissions
- keep access limited to staff who still need historical reference access

## Communication points

When announcing retirement of the spreadsheet, tell users:

- BuildBook is now the system to use for new changes
- the spreadsheet is read-only or archived
- where to find BuildBook
- who to contact if imported data needs correction

## Post-retirement checks

After the transition:

- confirm no routine updates are being made to the spreadsheet
- confirm support staff are using BuildBook for lookup
- confirm production or admin users are using BuildBook for edits
- confirm archived spreadsheet access is still available when genuinely needed

## Exception handling

If an issue is found immediately after retirement:

- correct the issue in BuildBook where possible
- use the archived spreadsheet only as a historical reference
- avoid restarting parallel editing in both systems unless a controlled rollback decision is made

## Final outcome

At the end of this plan:

- BuildBook is the live internal record system
- the old spreadsheet remains available only for controlled historical reference
- day-to-day updates no longer depend on the spreadsheet
