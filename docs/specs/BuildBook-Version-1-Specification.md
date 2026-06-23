# BuildBook — Version 1 Specification

## 1. Purpose

**BuildBook** is an internal web application that will replace the existing **Product Serial Numbers** spreadsheet.

The current spreadsheet holds important product, serial number, build, customer, software, firmware, shipping and credential information, but it is wide, awkward to search, difficult to update safely and unsuitable for storing sensitive data such as passwords and BitLocker recovery keys.

BuildBook Version 1 should provide a simple, fast and intuitive internal system for recording, searching and updating this information.

The application should be designed around one clear principle:

> A user should be able to find a device quickly, understand the record easily, and update the correct information without needing to scroll through a huge spreadsheet.

---

## 2. Application Name

The application will be called:

# BuildBook

Each individual product/device entry will be called a:

# Build Record

Example usage:

> “Open the Build Record for serial number 1000000.”

---

## 3. Version 1 Goals

Version 1 should focus on replacing the spreadsheet with something cleaner, safer and easier to use.

The main goals are:

1. Import the existing spreadsheet data.
2. Allow users to search across almost all non-sensitive fields.
3. Allow users to add new Build Records.
4. Allow users to update existing Build Records.
5. Display each Build Record in clear sections.
6. Protect sensitive fields such as passwords and BitLocker recovery keys.
7. Record a basic audit history of changes.
8. Allow useful exports without exposing sensitive information.
9. Provide basic internal reports.
10. Keep the application simple enough that staff will actually use it.

---

## 4. Out of Scope for Version 1

Version 1 should not attempt to become a full asset-management or workflow system.

The following are out of scope for Version 1:

* Customer access.
* Public web access.
* Mobile app.
* Freshdesk integration.
* Azure DevOps integration.
* Automated firmware detection.
* Automated device discovery.
* Complex stock-control features.
* Complex approval workflows.
* Advanced dashboards.
* Offline use.
* Barcode or QR scanning.
* Full document-management features.

These may be considered later, but Version 1 should remain practical and focused.

---

## 5. Users

BuildBook is for internal use only.

Likely user groups:

| User Group            | Typical Use                                                    |
| --------------------- | -------------------------------------------------------------- |
| Production / Assembly | Create and update product build information                    |
| QA / Checking         | Record checking information and QA references                  |
| Support               | Search for customer, device, version and configuration details |
| Admin / Sales Support | Look up order, invoice and shipment information                |
| System Administrator  | Manage users, roles and system settings                        |

---

## 6. Technology Stack

BuildBook Version 1 will use the following technology stack:

| Area                  | Technology                                                                                           |
| --------------------- | ---------------------------------------------------------------------------------------------------- |
| Application Framework | ASP.NET Core Blazor Web App                                                                          |
| Rendering Mode        | Interactive Server rendering                                                                         |
| Language              | C#                                                                                                   |
| UI Components         | Razor components                                                                                     |
| Database              | SQL Server                                                                                           |
| Data Access           | Entity Framework Core                                                                                |
| Authentication        | Windows Authentication, Active Directory, or Microsoft Entra ID depending on internal infrastructure |
| Hosting               | Internal IIS server or internal Windows server                                                       |
| Import/Export         | Excel and CSV support                                                                                |
| Search                | SQL Server-based search for Version 1                                                                |
| Secret Storage        | Encrypted server-side storage                                                                        |
| Audit History         | SQL Server audit tables                                                                              |

The application should not require a separate JavaScript front end such as React, Angular or Vue.

Small amounts of JavaScript interop may be used only where necessary, but Version 1 should aim to keep the application primarily C# and Razor-based.

---

## 7. Recommended Application Type

The recommended project type is:

# ASP.NET Core Blazor Web App

## using Interactive Server rendering

This is suitable because BuildBook is an internal business application based mainly around:

* Data entry forms.
* Search.
* Tables.
* Record viewing.
* Editing.
* Import/export.
* Permissions.
* SQL Server data.
* Internal users.

Interactive Server rendering is a good fit because:

* Most application logic remains on the server.
* Sensitive data is easier to protect.
* SQL Server access remains server-side.
* Development can be mostly C#.
* The application does not need a separate browser-side JavaScript framework.
* It is well-suited to internal network use.

---

## 8. Core Concept

Each row in the current spreadsheet becomes one **Build Record**.

A Build Record represents one physical product/device.

Rather than showing one very wide row, the record should be split into clear sections:

1. Summary
2. Product Details
3. Build Details
4. Customer & Shipping
5. Hardware
6. Software & Firmware
7. Network
8. Credentials & Recovery
9. Notes
10. History

This should make the application much easier to read and update than the existing spreadsheet.

---

## 9. Main Screens

## 9.1 Home Page

The home page should be simple and search-focused.

It should include:

* A large search box.
* A button to add a new Build Record.
* Recently viewed records.
* Recently updated records.
* A small set of useful summary counts.

Example layout:

```text
BuildBook

[ Search serial number, customer, machine name, invoice, firmware version... ]

[ Add New Build Record ]

Recently Updated
- CDM61100 / Serial 1000000 / APVL
- CDM61100 / Serial 1000001 / Dounreay
```

The search box should be the most prominent feature on the page.

---

## 9.2 Build Register

The Build Register replaces the spreadsheet view.

It should show a table of Build Records using sensible default columns.

Default columns:

| Column           |
| ---------------- |
| Product Code     |
| Product Name     |
| Serial Number    |
| Customer         |
| Machine Name     |
| RadSight Version |
| Windows Version  |
| Date Assembled   |
| Date Shipped     |
| Checked By       |
| Last Updated     |

The user should be able to:

* Search within the register.
* Sort by column.
* Filter by customer.
* Filter by product code.
* Filter by date shipped.
* Filter by RadSight version.
* Filter by Windows version.
* Filter by missing information.
* Open a Build Record from the table.

The Build Register should avoid horizontal scrolling where possible.

Version 1 does not need complex user-defined column layouts, but the default layout should be useful for day-to-day work.

---

## 9.3 Build Record Detail Page

The Build Record detail page should show one device clearly.

At the top of the page, show a summary panel:

```text
RadSight Access Terminal

Product Code: CDM61100
Serial Number: 1000000
Customer: APVL
Machine Name: RADSIGHT-11996
RadSight Version: 1.3.6.1946
Windows Version: Windows 10
Date Shipped: 17/10/2019
```

Below the summary, the information should be split into tabs or collapsible sections.

Recommended sections:

| Section                | Purpose                                              |
| ---------------------- | ---------------------------------------------------- |
| Summary                | Important information at a glance                    |
| Product                | Product code, name, classification and serial number |
| Build                  | Assembly, manufacturer and checking details          |
| Customer & Shipping    | Customer, order, invoice and shipment details        |
| Hardware               | Panel, radio, router and machine details             |
| Software & Firmware    | RadSight, Windows, image and firmware versions       |
| Network                | Wi-Fi, router and network details                    |
| Credentials & Recovery | Passwords and BitLocker recovery key                 |
| Notes                  | General notes                                        |
| History                | Audit history                                        |

Each section should have its own **Edit** button.

This is better than having one enormous edit form.

---

## 9.4 Add New Build Record

The user should be able to create a new Build Record from a simple form.

Required fields:

| Field         |
| ------------- |
| Product Code  |
| Product Name  |
| Serial Number |

Optional fields on the initial create screen:

| Field                  |
| ---------------------- |
| Product Classification |
| Customer               |
| Machine Name           |
| Date Assembled         |
| Assembled By           |

After the record is saved, the user should be taken to the full Build Record page where the remaining details can be completed.

The application should warn the user if:

* The serial number already exists.
* The machine name already exists.
* Required fields are missing.

---

## 9.5 Edit Build Record

Editing should happen section-by-section.

Example:

```text
Product Details       [Edit]
Build Details         [Edit]
Customer & Shipping   [Edit]
Software & Firmware   [Edit]
Network               [Edit]
Credentials           [Edit]
```

When a user saves a section, the system should:

1. Validate the entered data.
2. Save the changes.
3. Record the changes in the audit history.
4. Return the user to the Build Record detail page.
5. Clearly show that the save was successful.

The system should warn the user before they leave a page with unsaved changes.

---

## 10. Build Record Fields

The following field groups should be included in Version 1.

---

## 10.1 Product Details

| Field                  | Type             | Required |
| ---------------------- | ---------------- | -------- |
| Product Code           | Text             | Yes      |
| Product Name           | Text             | Yes      |
| Product Classification | Text or dropdown | No       |
| Serial Number          | Text             | Yes      |
| Internal Status        | Dropdown         | No       |

Suggested internal statuses:

| Status         |
| -------------- |
| Draft          |
| In Build       |
| Awaiting Check |
| Checked        |
| Shipped        |
| Returned       |
| Retired        |

---

## 10.2 Build Details

| Field                   | Type                   |
| ----------------------- | ---------------------- |
| Assembled In            | Text                   |
| Assembled By            | Text or user lookup    |
| Date Assembled          | Date                   |
| H/W Manufacturer        | Text                   |
| Manufacturer Part No.   | Text                   |
| Manufacturer Revision   | Text                   |
| Manufacturer Serial No. | Text                   |
| Checked By              | Text or user lookup    |
| Packing List            | Text or file reference |
| QA Number               | Text                   |

---

## 10.3 Customer & Shipping

| Field          | Type                    |
| -------------- | ----------------------- |
| Customer       | Text or customer lookup |
| Customer Order | Text                    |
| OA Number      | Text                    |
| Invoice Number | Text                    |
| Date Shipped   | Date                    |
| Shipping Notes | Long text               |

For Version 1, customers can be created during spreadsheet import.

The customer field should ideally become a controlled lookup list so that the same customer is not entered in several different ways.

---

## 10.4 Hardware Details

| Field                  | Type      |
| ---------------------- | --------- |
| Panel Device Model     | Text      |
| Panel Device Serial    | Text      |
| Panel Firmware Version | Text      |
| Machine Name           | Text      |
| Radio Serial Number    | Text      |
| Router Used            | Text      |
| Hardware Notes         | Long text |

---

## 10.5 Software & Firmware

| Field                            | Type |
| -------------------------------- | ---- |
| Disk Image Version               | Text |
| RadSight Version                 | Text |
| Windows Version                  | Text |
| Windows Latest Patch             | Text |
| Bleuvio Firmware Version         | Text |
| Charthouse IRDA Firmware Version | Text |
| Radio Firmware                   | Text |

Version 1 only needs to store the current value.

Full version history can be added later if needed.

---

## 10.6 User Accounts and Network

| Field                  | Type   | Sensitive |
| ---------------------- | ------ | --------- |
| RadSight User Login    | Text   | No        |
| Kiosk User             | Text   | No        |
| Windows Admin User     | Text   | No        |
| Wi-Fi SSID             | Text   | No        |
| RadSight User Password | Secret | Yes       |
| Windows Admin Password | Secret | Yes       |
| Kiosk Password         | Secret | Yes       |
| Wi-Fi Password         | Secret | Yes       |
| Router Password        | Secret | Yes       |

---

## 10.7 Recovery Information

| Field                  | Type   | Sensitive |
| ---------------------- | ------ | --------- |
| BitLocker Recovery Key | Secret | Yes       |

The BitLocker recovery key must not be treated as a normal text field.

It should be:

* Hidden by default.
* Revealable only to authorised users.
* Audited when viewed.
* Excluded from normal search.
* Excluded from normal exports.
* Stored encrypted.

---

## 10.8 Notes

| Field | Type      |
| ----- | --------- |
| Note  | Long text |

Version 1 can use a single notes field.

Later versions could split this into:

* Build notes.
* Support notes.
* Customer notes.
* Internal notes.

---

## 11. Search Requirements

Search is one of the most important parts of BuildBook.

The user should be able to search from:

* The home page.
* The Build Register.
* The main navigation area.

The search should match against most non-sensitive fields.

Searchable fields should include:

* Product Code
* Product Name
* Product Classification
* Serial Number
* Assembled In
* Assembled By
* H/W Manufacturer
* Manufacturer Part No.
* Manufacturer Revision
* Manufacturer Serial No.
* Customer
* Customer Order
* OA Number
* Invoice Number
* Panel Device Model
* Panel Device Serial
* Panel Firmware Version
* Disk Image Version
* RadSight User Login
* Kiosk User
* Machine Name
* RadSight Version
* Windows Version
* Windows Latest Patch
* Bleuvio Firmware Version
* Charthouse IRDA Firmware Version
* Radio Firmware
* Radio Serial Number
* Wi-Fi SSID
* Router Used
* Packing List
* Checked By
* Notes

Sensitive fields should not be included in global search:

* Passwords
* Wi-Fi password
* Router password
* BitLocker recovery key

Search features required in Version 1:

| Feature                     | Required                  |
| --------------------------- | ------------------------- |
| Partial matching            | Yes                       |
| Case-insensitive matching   | Yes                       |
| Search across normal fields | Yes                       |
| Search results table        | Yes                       |
| Open record from result     | Yes                       |
| Filter results              | Yes                       |
| Sort results                | Yes                       |
| Highlight matching fields   | Nice to have              |
| Fuzzy typo matching         | Not required in Version 1 |

Example searches that should work:

```text
1000000
```

```text
APVL
```

```text
GB1989
```

```text
RADSIGHT-11996
```

```text
1.3.6.1946
```

```text
D10A006300084
```

---

## 12. Sensitive Data Requirements

BuildBook will contain sensitive internal information.

Sensitive fields include:

* RadSight user password.
* Windows admin password.
* Kiosk password.
* Wi-Fi password.
* Router password.
* BitLocker recovery key.

Version 1 requirements:

1. Sensitive values must be encrypted in the database.
2. Sensitive values must be masked by default.
3. Sensitive values must not appear in list views.
4. Sensitive values must not appear in normal exports.
5. Sensitive values must not be included in global search.
6. Only authorised users can reveal sensitive values.
7. Revealing a sensitive value must create an audit record.
8. Changing a sensitive value must create an audit record.
9. Sensitive values must not be written to application logs.
10. Sensitive values should not be sent to the browser unless the user has explicitly requested to reveal them and has permission.

Example display:

```text
Windows Admin Password: ••••••••••••  [Reveal]
BitLocker Recovery Key: ••••••••••••  [Reveal]
```

When the user clicks **Reveal**, the server should check permissions before returning the value.

---

## 13. User Roles and Permissions

Version 1 should use simple role-based permissions.

## 13.1 Roles

| Role                  | Description                              |
| --------------------- | ---------------------------------------- |
| Administrator         | Full system access                       |
| Editor                | Can add and edit normal Build Records    |
| Viewer                | Can search and view normal Build Records |
| Sensitive Data Viewer | Can reveal passwords and recovery keys   |

A user may have more than one role.

Examples:

| User Type            | Roles                          |
| -------------------- | ------------------------------ |
| Production user      | Editor                         |
| Support user         | Viewer                         |
| Senior support user  | Viewer + Sensitive Data Viewer |
| System administrator | Administrator                  |

---

## 13.2 Permission Rules

| Action                    | Required Role                          |
| ------------------------- | -------------------------------------- |
| View Build Register       | Viewer, Editor or Administrator        |
| View Build Record         | Viewer, Editor or Administrator        |
| Add Build Record          | Editor or Administrator                |
| Edit normal fields        | Editor or Administrator                |
| Reveal sensitive data     | Sensitive Data Viewer or Administrator |
| Import spreadsheet        | Administrator                          |
| Export non-sensitive data | Viewer, Editor or Administrator        |
| Manage users              | Administrator                          |
| Delete records            | Administrator only                     |

For Version 1, deletion should be avoided where possible. A safer approach is to mark a record as retired or inactive.

---

## 14. Import Requirements

Version 1 must support importing the existing spreadsheet.

Import process:

1. User uploads the spreadsheet.
2. System reads the column headers.
3. System maps spreadsheet columns to BuildBook fields.
4. User reviews the mapping.
5. System validates the data.
6. System displays warnings and errors.
7. User confirms the import.
8. System creates Build Records.
9. System stores the original spreadsheet row number against each imported record.

The import should detect:

* Missing product codes.
* Missing product names.
* Missing serial numbers.
* Duplicate serial numbers.
* Duplicate machine names.
* Invalid dates.
* Unexpected blank values.
* Sensitive fields being imported.
* Rows that appear incomplete.

The import should produce an import summary:

```text
Rows read: 350
Records created: 342
Records skipped: 3
Warnings: 18
Errors: 5
```

Sensitive fields imported from the spreadsheet should be moved into encrypted secret storage, not stored as normal text.

---

## 15. Export Requirements

Version 1 should allow users to export data to:

* CSV.
* Excel.

Required exports:

| Export                            | Required |
| --------------------------------- | -------- |
| Current search results to CSV     | Yes      |
| Current search results to Excel   | Yes      |
| Basic customer device list        | Yes      |
| Basic product/version report      | Yes      |
| Export including sensitive fields | No       |

Normal exports must not include:

* Passwords.
* Wi-Fi passwords.
* Router passwords.
* BitLocker recovery keys.

If a future version adds sensitive exports, that should require administrator permission and should create an audit record.

---

## 16. Audit History

Version 1 should record a basic audit history.

The audit history should include:

* Record created.
* Record edited.
* Field changed.
* Sensitive value viewed.
* Sensitive value changed.
* Record marked inactive or retired.
* Spreadsheet import performed.

Audit fields:

| Field           |
| --------------- |
| Audit ID        |
| Date/time       |
| User            |
| Build Record ID |
| Action          |
| Field changed   |
| Old value       |
| New value       |

For sensitive fields, the audit history should record that a value was viewed or changed, but it must not store the actual secret value.

Example audit entries:

```text
22/06/2026 14:31 — David changed RadSight Version from 1.3.6.1946 to 1.3.7.2001.
22/06/2026 14:33 — David viewed BitLocker Recovery Key.
```

---

## 17. Basic Reports

Version 1 should include simple reports that help replace spreadsheet filtering.

Required reports:

1. Devices by customer.
2. Devices by product code.
3. Devices by RadSight version.
4. Devices by Windows version.
5. Devices shipped within a date range.
6. Records missing serial number.
7. Records missing customer.
8. Records missing QA number.
9. Records missing BitLocker recovery key.
10. Recently updated records.

Reports should be:

* Searchable.
* Filterable.
* Exportable to CSV or Excel.

---

## 18. Data Validation

Version 1 should enforce sensible validation without making data entry difficult.

Required fields:

| Field         |
| ------------- |
| Product Code  |
| Product Name  |
| Serial Number |

Recommended validation:

| Field                  | Rule                                    |
| ---------------------- | --------------------------------------- |
| Serial Number          | Must be unique                          |
| Machine Name           | Warn if duplicate                       |
| Date Shipped           | Cannot be before Date Assembled         |
| Product Code           | Cannot be blank                         |
| Product Name           | Cannot be blank                         |
| BitLocker Recovery Key | Should match expected format if entered |
| Invoice Number         | Free text allowed                       |
| Firmware Versions      | Free text allowed                       |
| RadSight Version       | Free text allowed                       |
| Windows Version        | Free text allowed                       |

The system should allow unknown values, but should encourage consistent wording.

Recommended standard values:

| Value           |
| --------------- |
| Unknown         |
| Not Applicable  |
| To Be Confirmed |

This is better than mixing blank cells, `N/A`, `n/a`, hyphens and random comments.

---

## 19. Usability Requirements

BuildBook must be easy and intuitive to use.

The application should:

* Load quickly.
* Have a clear search box.
* Avoid huge forms.
* Avoid horizontal scrolling.
* Use tabs or collapsible sections.
* Use plain language labels.
* Make required fields obvious.
* Make saving clear.
* Warn before discarding unsaved changes.
* Show when a record was last updated.
* Show who last updated a record.
* Use dropdowns where values are repeated.
* Allow free text where flexibility is needed.
* Keep common tasks to as few clicks as practical.

Common task targets:

| Task                                    | Target                           |
| --------------------------------------- | -------------------------------- |
| Find a device by serial number          | Under 10 seconds                 |
| Find a device by customer               | Under 10 seconds                 |
| Open a Build Record from search results | One click                        |
| Update RadSight version                 | Edit one section only            |
| Reveal BitLocker key                    | One click after permission check |
| Export current search results           | One or two clicks                |

---

## 20. Blazor UI Requirements

Because the application will be built as an ASP.NET Core Blazor Web App, the UI should be component-based.

Suggested Razor components:

| Component               | Purpose                                    |
| ----------------------- | ------------------------------------------ |
| BuildRecordSummaryCard  | Top summary panel for a Build Record       |
| BuildRegisterTable      | Main searchable table                      |
| BuildRecordSearchBox    | Reusable search box                        |
| ProductDetailsSection   | Product details display/edit section       |
| BuildDetailsSection     | Build details display/edit section         |
| CustomerShippingSection | Customer and shipping display/edit section |
| HardwareSection         | Hardware display/edit section              |
| SoftwareFirmwareSection | Software and firmware display/edit section |
| NetworkSection          | Network display/edit section               |
| CredentialsSection      | Sensitive data display/reveal section      |
| NotesSection            | Notes display/edit section                 |
| AuditHistoryTable       | Change history display                     |
| ImportSpreadsheetWizard | Spreadsheet import workflow                |
| ExportButton            | Export current results                     |
| ValidationSummaryPanel  | User-friendly validation messages          |

The application should avoid large pages with too much logic in one component.

Common UI logic should be placed in reusable components and services.

---

## 21. Suggested Project Structure

Suggested solution structure:

```text
BuildBook.sln

/src
  /BuildBook.Web
    Blazor web application
    Razor components
    Pages
    Layouts
    Authentication setup

  /BuildBook.Application
    Business logic
    Validation
    Search services
    Import/export services
    Audit services

  /BuildBook.Domain
    Core entities
    Enums
    Domain models

  /BuildBook.Infrastructure
    Entity Framework Core
    SQL Server access
    Secret encryption
    File import/export implementation

/tests
  /BuildBook.Tests
    Unit tests

  /BuildBook.IntegrationTests
    Database and service tests
```

This keeps the application maintainable without making Version 1 too complicated.

---

## 22. SQL Server Database Design

Version 1 can use a practical database structure.

It should not be over-complicated, but sensitive data should be separated from ordinary Build Record data.

Recommended tables:

| Table              | Purpose                                                           |
| ------------------ | ----------------------------------------------------------------- |
| BuildRecords       | Main product/device records                                       |
| Customers          | Customer lookup records                                           |
| BuildRecordSecrets | Passwords and recovery keys                                       |
| BuildRecordAudit   | Audit history                                                     |
| Users              | Application users, if not relying fully on Windows/Entra identity |
| Roles              | Application roles                                                 |
| UserRoles          | Links users to roles                                              |
| Imports            | Spreadsheet import history                                        |
| ImportWarnings     | Warnings/errors from imports                                      |

---

## 23. BuildRecords Table

The BuildRecords table should contain ordinary non-sensitive fields.

Example fields:

| Field                         |
| ----------------------------- |
| Id                            |
| ProductCode                   |
| ProductName                   |
| ProductClassification         |
| SerialNumber                  |
| InternalStatus                |
| AssembledIn                   |
| AssembledBy                   |
| DateAssembled                 |
| HardwareManufacturer          |
| ManufacturerPartNumber        |
| ManufacturerRevision          |
| ManufacturerSerialNumber      |
| CustomerId                    |
| CustomerOrder                 |
| OANumber                      |
| InvoiceNumber                 |
| DateShipped                   |
| PanelDeviceModel              |
| PanelDeviceSerial             |
| PanelFirmwareVersion          |
| DiskImageVersion              |
| RadSightUserLogin             |
| KioskUser                     |
| WindowsAdminUser              |
| MachineName                   |
| RadSightVersion               |
| WindowsVersion                |
| WindowsLatestPatch            |
| BleuvioFirmwareVersion        |
| CharthouseIrdaFirmwareVersion |
| RadioFirmware                 |
| RadioSerialNumber             |
| WifiSsid                      |
| RouterUsed                    |
| PackingList                   |
| CheckedBy                     |
| Note                          |
| OriginalSpreadsheetRowNumber  |
| CreatedAt                     |
| CreatedBy                     |
| LastUpdatedAt                 |
| LastUpdatedBy                 |
| IsActive                      |

---

## 24. BuildRecordSecrets Table

Sensitive information should be stored separately.

| Field                | Notes                         |
| -------------------- | ----------------------------- |
| Id                   | Primary key                   |
| BuildRecordId        | Links secret to Build Record  |
| SecretType           | Password or recovery key type |
| SecretValueEncrypted | Encrypted value               |
| CreatedAt            | Date/time                     |
| CreatedBy            | User                          |
| LastUpdatedAt        | Date/time                     |
| LastUpdatedBy        | User                          |

Secret types:

* RadSightUserPassword
* WindowsAdminPassword
* KioskPassword
* WifiPassword
* RouterPassword
* BitLockerRecoveryKey

The encrypted value should never be written to logs.

The decrypted value should only be returned to the browser after a permission check and an explicit reveal action.

---

## 25. Entity Framework Core Requirements

Entity Framework Core should be used for data access.

Requirements:

* Use code-first migrations or a controlled database migration approach.
* Add indexes on commonly searched fields.
* Keep sensitive fields out of the main BuildRecords entity.
* Use service classes for search, import, export and secret handling.
* Avoid placing database logic directly inside Razor components.

Recommended indexes:

| Field           |
| --------------- |
| SerialNumber    |
| ProductCode     |
| ProductName     |
| CustomerId      |
| MachineName     |
| InvoiceNumber   |
| CustomerOrder   |
| OANumber        |
| RadSightVersion |
| WindowsVersion  |
| DateShipped     |
| LastUpdatedAt   |

---

## 26. Search Implementation

For Version 1, search can be implemented using SQL Server.

Recommended approach:

* Start with SQL queries across key fields.
* Use case-insensitive partial matching.
* Add indexes to common fields.
* Consider SQL Server Full-Text Search if performance requires it.

Search should be implemented in a dedicated service, for example:

```text
BuildRecordSearchService
```

The search service should return:

* Build Record ID.
* Product Code.
* Product Name.
* Serial Number.
* Customer.
* Machine Name.
* RadSight Version.
* Windows Version.
* Date Shipped.
* Matching field summary, if practical.

Sensitive fields must not be searched.

---

## 27. Authentication and Authorisation

BuildBook should require login.

Preferred authentication options:

1. Microsoft Entra ID, if the organisation already uses Microsoft 365.
2. Windows Authentication, if hosted internally on a domain.
3. ASP.NET Core Identity, if separate application accounts are preferred.

Authorisation should be role-based.

Sensitive operations, such as revealing passwords or BitLocker recovery keys, must be checked server-side.

The UI may hide buttons from users without permission, but the server must still enforce the permission.

---

## 28. Hosting

BuildBook should be hosted internally.

Recommended hosting options:

| Option                          | Notes                                              |
| ------------------------------- | -------------------------------------------------- |
| Internal IIS server             | Good fit for ASP.NET Core and Windows environments |
| Internal Windows server service | Possible if IIS is not preferred                   |
| Internal Docker host            | Possible, but probably unnecessary for Version 1   |
| Azure App Service               | Possible if cloud hosting is acceptable            |

For Version 1, an internal IIS-hosted ASP.NET Core application backed by SQL Server would be a sensible default.

---

## 29. Backup and Recovery

Because BuildBook will become the replacement for an important spreadsheet, backup must be included from the start.

Requirements:

* SQL Server database backups.
* Backup schedule agreed internally.
* Restore process documented.
* Restore process tested.
* Imported source spreadsheet archived.
* Sensitive data included in backups only in encrypted form.

---

## 30. Logging

Application logging should record useful technical information but must not expose sensitive data.

Logs may include:

* Application errors.
* Failed imports.
* Search failures.
* Authentication failures.
* Export failures.
* Secret reveal attempts, without the secret value.

Logs must not include:

* Passwords.
* Wi-Fi passwords.
* Router passwords.
* BitLocker recovery keys.
* Decrypted secret values.

---

## 31. Non-Functional Requirements

| Requirement        | Target                                       |
| ------------------ | -------------------------------------------- |
| Internal use only  | Yes                                          |
| Browser support    | Current Microsoft Edge and Chrome            |
| Login required     | Yes                                          |
| Search performance | Results should appear quickly for normal use |
| Data protection    | Sensitive fields encrypted                   |
| Auditability       | Important changes recorded                   |
| Usability          | Simple enough for non-developers             |
| Availability       | Available during normal working hours        |
| Backup             | SQL Server backup required                   |
| Maintainability    | Clear C# application structure               |

---

## 32. Acceptance Criteria

BuildBook Version 1 is successful when:

1. Existing spreadsheet data can be imported.
2. Users can search by serial number, customer, invoice number, machine name, version number and notes.
3. Users can open a clear Build Record instead of scrolling across a wide spreadsheet.
4. Users can add a new Build Record.
5. Users can edit an existing Build Record section by section.
6. Duplicate serial numbers are prevented or clearly warned.
7. Passwords and BitLocker keys are hidden by default.
8. Only authorised users can reveal sensitive fields.
9. Revealing sensitive fields is audited.
10. Normal exports do not include sensitive fields.
11. Users can export search results.
12. Basic reports are available.
13. Changes to records are shown in the record history.
14. The old spreadsheet is no longer needed for normal day-to-day lookup.

---

## 33. Version 1 Summary

BuildBook Version 1 should be a simple, internal, searchable replacement for the Product Serial Numbers spreadsheet.

It should be built using:

* ASP.NET Core Blazor Web App.
* Interactive Server rendering.
* C# and Razor components.
* SQL Server.
* Entity Framework Core.

The priority is not to build a complicated enterprise system.

The priority is to build something staff will actually use because it is faster, clearer and safer than the spreadsheet.

The most important features are:

* Fast search.
* Clear Build Record pages.
* Easy editing.
* Spreadsheet import.
* Secure handling of passwords and BitLocker keys.
* Basic audit history.
* Safe export.
* Simple internal reporting.
