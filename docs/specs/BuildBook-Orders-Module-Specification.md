# BuildBook Orders Module — Specification

## 1. Purpose

Add a new **Orders** module to BuildBook for managing internal production/order workflow currently tracked in Microsoft Planner.

The Orders module should replace the Planner board/spreadsheet workflow with a structured, searchable and professional internal system for tracking customer orders, production preparation, build progress, shipping readiness, shipping completion and invoicing readiness.

The module must use the existing BuildBook application and SQL Server database.

The module should integrate with existing BuildBook areas where appropriate:

- **Customers** — customer selection should come from the central Customers table.
- **Build Records** — orders should be linkable to one or more Build Records where relevant.
- **RMAs** — not directly required for order creation, but order history should be useful context if an RMA is later raised.
- **Admin/System Settings** — reusable configuration should live in Admin where appropriate.

The module should be more structured and more pleasant to use than Microsoft Planner, while preserving the useful board-style workflow view.

---

## 2. Important Privacy / Data Protection Requirement

The specification, test data, seed data, GitHub issues, screenshots, examples and developer documentation must not include real customer data from the uploaded Planner export.

The uploaded spreadsheet may contain real names, organisations, references, notes, addresses or order details. These must be treated as operational data for import only.

For specification and development examples, use only synthetic examples such as:

```text
Example Customer
Demo Site
Internal Test Customer
ORDER-DEMO-001
```

Do not copy real customer names, addresses, emails, order numbers, notes or task text from the Planner export into the specification, seed data or GitHub tickets.

---

## 3. Source Data Observed from Planner Export

The uploaded Microsoft Planner export contains the following worksheets:

| Worksheet | Purpose |
|---|---|
| Plan | Plan metadata, including plan ID, plan name and export date |
| Consolidated Data | Task data with bucket/user names resolved |
| Tasks | Raw task data using IDs for buckets/users |
| Goals | Planner goals, currently minimal/empty |
| Buckets | Bucket ID to bucket name mapping |
| Users | User ID to user name/email mapping |

The exported task columns include:

| Column |
|---|
| Task ID |
| Task Name |
| Bucket |
| Goal |
| Status |
| Priority |
| Assigned To |
| Created By |
| Created Date |
| Due date |
| Start date |
| Is Recurring |
| Late |
| Completed Date |
| Completed By |
| Completed Checklist Items |
| Checklist Items |
| Labels |
| Notes |

The **Checklist Items** field appears to contain multiple checklist items separated by semicolons.

The **Assigned To** field may contain multiple users separated by semicolons in the consolidated data or multiple IDs separated by semicolons in the raw Tasks tab.

The import process must handle both the raw `Tasks` sheet and the resolved `Consolidated Data` sheet, with a preference for the sheet that provides the most usable human-readable values.

---

## 4. Module Name and Navigation

The module should be called:

```text
Orders
```

Add a top-level navigation item:

```text
Home
Build Register
Orders
RMAs
Customers
Reports
Admin
```

Each record should be called an:

```text
Order
```

or, where more explicit wording is needed:

```text
Production Order
```

Use **Orders** in navigation and page titles unless a screen specifically needs to distinguish production orders from customer purchase orders.

---

## 5. Design Goals

The Orders module should feel like a compact internal operations tool, not a marketing page.

The UI should be:

- Professional.
- Compact.
- Fast to scan.
- Better structured than Planner.
- Easier to report on than Planner.
- Searchable by almost every useful non-sensitive field.
- Clearly linked to Customers and Build Records.
- Suitable for Support, Production, QA, Admin and Management users.

The module should improve on Planner by adding:

- Structured fields instead of relying only on task names/notes.
- Customer dropdowns from the Customers module.
- Linkage to Build Records.
- Import from Planner export.
- Better reporting.
- Better order history.
- Better workflow validation.
- Better search.
- Better invoicing readiness visibility.
- Better overdue/ageing information.
- Better checklists with completion metadata.
- Audit history.

---

## 6. Core Order Workflow

The default workflow should be based on the Planner buckets.

Initial order statuses:

| Status | Meaning |
|---|---|
| Order Received | Order has been received and needs review/action |
| Parts Ordered / Stock Allocated | Parts are ordered or existing stock has been allocated |
| Built | Product/device/build work has been completed |
| Prepared for Shipping | Item is packed/prepared but not yet ready for collection/shipping completion |
| Ready for Collection | Item is ready for customer/courier/internal collection |
| Shipped | Item has been shipped/despatched |
| Contract Ready for Invoicing | Contract/order is ready to be invoiced |
| Invoiced | Order has been invoiced/completed |

These statuses should be configurable later, but for the first implementation they can be seeded defaults.

The system should keep a status history whenever the order status changes.

---

## 7. Order Record Structure

Each Order should have the following sections.

### 7.1 Summary

Top-level summary fields:

| Field | Notes |
|---|---|
| Order Number / Internal Reference | Generated or manually entered |
| Order Title | From Planner task name or entered manually |
| Customer | Lookup to Customers table where possible |
| Status | Workflow status |
| Priority | Low/Medium/High/Urgent or imported Planner value |
| Assigned To | One or more BuildBook users |
| Due Date | Date |
| Start Date | Date |
| Created Date | Date/time |
| Created By | User |
| Completed Date | Date/time |
| Completed By | User |
| Linked Build Records | One or more Build Records |
| Support Ticket No. | Optional, if relevant |
| Last Updated | Date/time |

### 7.2 Order Details

Fields:

| Field | Type |
|---|---|
| Order Title | Text |
| Order Description / Notes | Long text |
| CustomerId | Customer lookup |
| Customer Reference | Text |
| Customer Purchase Order No. | Text |
| Internal Order Reference | Text |
| Quote Number | Text |
| Sales / Admin Owner | User lookup |
| Production Owner | User lookup |
| Priority | Dropdown |
| Status | Dropdown |
| Labels | Structured labels |
| Is Recurring | Boolean |
| Planner Task ID | Text, import traceability |
| Planner Plan ID | Text, import traceability |
| Planner Bucket ID | Text, import traceability |
| Planner Source | Text, import traceability |

### 7.3 Dates

Fields:

| Field | Type |
|---|---|
| Created Date | Date/time |
| Start Date | Date |
| Due Date | Date |
| Completed Date | Date/time |
| Shipped Date | Date |
| Ready for Invoicing Date | Date |
| Invoiced Date | Date |
| Last Updated At | Date/time |

### 7.4 Assignment

Orders may have multiple assigned users.

Fields for each assignment:

| Field |
|---|
| OrderId |
| ApplicationUserId |
| Role / Assignment Type |
| Assigned At |
| Assigned By |

Assignment types could include:

| Assignment Type |
|---|
| Owner |
| Production |
| Support |
| Sales/Admin |
| QA |
| Other |

### 7.5 Checklist

Each imported Planner checklist item should become a structured checklist item.

Fields:

| Field |
|---|
| Checklist Item |
| Display Order |
| Is Completed |
| Completed By |
| Completed At |
| Source |
| Show In Board View |

Import rules:

- Split `Checklist Items` by semicolon.
- Split `Completed Checklist Items` by semicolon where present.
- Trim whitespace.
- Ignore blank items.
- Preserve original checklist text.
- Mark items completed if they appear in the completed checklist list.
- If completion metadata is not available per checklist item, use the order-level completed user/date only where appropriate and clearly mark it as imported/inferred.

### 7.6 Notes

Planner task notes should be imported into a structured note record rather than being mixed with the main order fields.

Suggested note types:

| Note Type |
|---|
| Internal Note |
| Production Note |
| Shipping Note |
| Invoicing Note |
| Planner Imported Note |

Fields:

| Field |
|---|
| OrderId |
| NoteType |
| NoteText |
| CreatedBy |
| CreatedAt |
| LastUpdatedBy |
| LastUpdatedAt |

### 7.7 Labels

Planner labels should be imported as structured labels where possible.

Fields:

| Field |
|---|
| OrderId |
| LabelText |
| Source |

Labels should be searchable and filterable.

### 7.8 History / Audit

Order history should record:

| Event |
|---|
| Order created |
| Order imported |
| Status changed |
| Priority changed |
| Due date changed |
| Assignment changed |
| Checklist item completed |
| Checklist item added |
| Note added/edited |
| Customer changed |
| Build Record link changed |
| Shipped date changed |
| Invoiced date changed |
| Order closed/completed |

Each history record should include:

| Field |
|---|
| Date/time |
| User |
| Action |
| Field changed |
| Old value |
| New value |
| Comment/reason where useful |

---

## 8. Order List / Register Page

The Orders list page should be compact and operational.

Default columns:

| Column |
|---|
| Order |
| Status |
| Customer |
| Priority |
| Assigned To |
| Start Date |
| Due Date |
| Checklist Progress |
| Linked Builds |
| Last Updated |

Required features:

- Search by order title.
- Search by order/reference number.
- Search by customer.
- Search by assigned user.
- Search by note text.
- Search by checklist item text.
- Search by support ticket number.
- Filter by status.
- Filter by priority.
- Filter by assigned user.
- Filter by customer.
- Filter by overdue.
- Filter by completed/incomplete.
- Filter by linked/unlinked Build Record.
- Sort by due date, status, priority and last updated.
- Export non-sensitive order list to CSV/Excel.

The list should make important records visible without excessive scrolling.

---

## 9. Order Board View

The Orders module should include a board-style view similar to Planner but based on structured BuildBook data.

Board columns should initially match the default order statuses:

| Column |
|---|
| Order Received |
| Parts Ordered / Stock Allocated |
| Built |
| Prepared for Shipping |
| Ready for Collection |
| Shipped |
| Contract Ready for Invoicing |
| Invoiced |

Each board card should show:

| Field |
|---|
| Order title |
| Customer |
| Priority |
| Assigned users |
| Due date |
| Checklist progress |
| Linked Build Record indicator |
| Overdue indicator |
| Invoicing/shipping warning where relevant |

The board should improve on Planner by showing structured warnings, linked Build Record status and operational ageing.

Drag-and-drop status movement is optional. A clear status change action is acceptable for the first version.

---

## 10. Order Detail Page

The Order detail page should be clearer and more structured than the Planner task popup.

Recommended layout:

### Top Summary Area

- Order title/reference.
- Status.
- Customer.
- Priority.
- Assigned users.
- Due date.
- Checklist progress.
- Linked Build Records.
- Key warnings.

### Main Sections

| Section |
|---|
| Overview |
| Order Details |
| Customer |
| Dates |
| Assignments |
| Checklist |
| Linked Build Records |
| Shipping |
| Invoicing |
| Notes |
| History |

### Side Panel

Optional but useful:

- Status.
- Priority.
- Owner.
- Due date.
- Days open.
- Checklist progress.
- Open warnings.
- Quick actions.

---

## 11. Link to Customers

Orders should use the central Customers table from the Customer & Support Contracts module.

Requirements:

- Customer field must be a dropdown/search selector.
- Do not create duplicate free-text customer names.
- If imported task data contains a customer name in the title or notes, the import should not blindly create a customer unless a clear customer field can be identified or the user confirms it.
- Existing customer matching should be cautious and reviewable.
- Imported records may remain unlinked to a Customer if uncertain.

The Orders module should not duplicate customer address/contact fields. Those belong in the Customers module.

---

## 12. Link to Build Records

Orders should be linkable to one or more Build Records.

Use cases:

- One order results in one product/device Build Record.
- One order results in multiple product/device Build Records.
- An order is for licence/configuration work and may not require a Build Record.
- An order is legacy/imported and cannot be matched.

From an Order, users should be able to:

- Link an existing Build Record.
- Create a new Build Record from the Order where authorised.
- See linked Build Records.
- Open linked Build Records.
- See whether linked Build Records are shipped or still in build.

From a Build Record, users should be able to see related Orders.

---

## 13. Shipping and Invoicing

The Planner workflow includes shipping and invoicing-related statuses. BuildBook should make these structured.

### 13.1 Shipping Fields

| Field |
|---|
| Shipping Required |
| Shipping Method |
| Courier |
| Tracking Number |
| Collection Required |
| Collection Date |
| Shipped Date |
| Shipped By |
| Shipping Notes |

### 13.2 Invoicing Fields

| Field |
|---|
| Contract Ready for Invoicing |
| Ready for Invoicing Date |
| Invoice Number |
| Invoiced Date |
| Invoiced By |
| Invoicing Notes |

The system should warn when:

- Status is Shipped but shipped date is missing.
- Status is Contract Ready for Invoicing but invoicing fields are incomplete.
- Status is Invoiced but invoice number/date is missing.

---

## 14. Planner Export Import

The Orders module must support importing from the Planner-exported spreadsheet.

### 14.1 Import Source

Expected worksheets:

| Worksheet |
|---|
| Plan |
| Tasks |
| Consolidated Data |
| Buckets |
| Users |

The import should use `Consolidated Data` when human-readable names are available. It may use `Tasks`, `Buckets` and `Users` to resolve IDs when needed.

### 14.2 Import Mapping

| Planner Column | BuildBook Field |
|---|---|
| Task ID | PlannerTaskId |
| Task Name | OrderTitle |
| Bucket | OrderStatus |
| Goal | PlannerGoal |
| Status | PlannerStatus |
| Priority | Priority |
| Assigned To | OrderAssignments |
| Created By | CreatedBy / ImportedCreatedBy |
| Created Date | CreatedAt / PlannerCreatedDate |
| Due date | DueDate |
| Start date | StartDate |
| Is Recurring | IsRecurring |
| Late | ImportedLateFlag / computed overdue |
| Completed Date | CompletedAt |
| Completed By | CompletedBy |
| Completed Checklist Items | Completed checklist parsing |
| Checklist Items | Checklist item parsing |
| Labels | Labels |
| Notes | Imported note |

### 14.3 Checklist Import

The import must parse checklist fields carefully.

Rules:

- Split checklist item fields on semicolons.
- Trim each item.
- Remove empty values.
- Preserve original text.
- Preserve display order.
- Avoid duplicating the same item within the same order where practical.
- Mark completed checklist items based on the completed checklist field.
- Do not assume every semicolon in notes is a checklist separator.
- Do not parse normal notes into checklist items.

### 14.4 Assigned User Import

Rules:

- Split assigned users on semicolons.
- Map names or IDs to BuildBook users where possible.
- If a user cannot be matched, preserve the imported name as text and flag the assignment for review.
- Do not automatically create BuildBook application users unless approved by an Administrator.

### 14.5 Bucket / Status Import

Rules:

- Map Planner bucket names to Order statuses.
- If an unknown bucket is found, create an import warning and use an `Unknown` or `Imported - Unmapped` status.
- Preserve original bucket name and ID for traceability.

### 14.6 Notes Import

Rules:

- Import Planner notes as `Planner Imported Note`.
- Preserve line breaks.
- Do not extract real customer information into examples or seed data.
- Do not treat notes as structured customer master data without user confirmation.

### 14.7 Import Preview

Before importing, show:

- Rows read.
- Orders to create.
- Orders skipped.
- Duplicate Planner task IDs.
- Unknown buckets.
- Unknown users.
- Checklist parsing warnings.
- Rows with missing task names.
- Rows with invalid dates.

### 14.8 Import Idempotency

The import must avoid duplicate imports.

Rules:

- `PlannerTaskId` should be unique where present.
- If a task with the same `PlannerTaskId` already exists, offer skip/update behaviour.
- Default should be safe: skip existing unless the user explicitly chooses update.
- Keep an import batch/history record.

---

## 15. Reports

Add order reports that Planner cannot easily provide.

### 15.1 Operational Reports

- Open Orders.
- Overdue Orders.
- Orders due this week.
- Orders by status.
- Orders by assigned user.
- Orders waiting for parts/stock.
- Built but not prepared for shipping.
- Ready for collection but not shipped.
- Shipped but not ready for invoicing.
- Ready for invoicing but not invoiced.

### 15.2 Customer Reports

- Orders by customer.
- Open orders by customer.
- Orders without linked customer.
- Orders for customer within date range.

### 15.3 Build Linkage Reports

- Orders with no linked Build Record.
- Orders with multiple linked Build Records.
- Build Records with no linked Order.
- Orders where linked Build Record status appears inconsistent with Order status.

### 15.4 Checklist Reports

- Orders with incomplete checklist.
- Checklist completion by order.
- Common checklist items.
- Orders ready to move status based on checklist completion.

### 15.5 Invoicing Reports

- Orders ready for invoicing.
- Orders invoiced this month.
- Orders shipped but not invoiced.
- Orders missing invoice number.

---

## 16. Permissions

Suggested permissions:

| Action | Role |
|---|---|
| View orders | Viewer, Editor, Administrator |
| Create order | Editor, Administrator |
| Edit order | Editor, Administrator |
| Change order status | Editor, Administrator |
| Complete checklist item | Editor, Administrator |
| Link Build Record | Editor, Administrator |
| Import Planner spreadsheet | Administrator |
| Export orders | Viewer, Editor, Administrator |
| Delete order | Administrator only, preferably avoided |

Deletion should be avoided. Use inactive/archived where possible.

---

## 17. Data Model

Suggested tables:

### 17.1 OrderRecords

| Field |
|---|
| Id |
| OrderNumber |
| OrderTitle |
| CustomerId |
| Status |
| Priority |
| StartDate |
| DueDate |
| CompletedAt |
| CompletedByUserId |
| CreatedAt |
| CreatedByUserId |
| LastUpdatedAt |
| LastUpdatedByUserId |
| IsRecurring |
| PlannerTaskId |
| PlannerPlanId |
| PlannerBucketId |
| PlannerBucketName |
| PlannerStatus |
| PlannerGoal |
| ImportedLateFlag |
| NotesSummary |
| SupportTicketNo |
| ShippingRequired |
| ShippingMethod |
| Courier |
| TrackingNumber |
| CollectionRequired |
| CollectionDate |
| ShippedDate |
| ShippedByUserId |
| ShippingNotes |
| ContractReadyForInvoicing |
| ReadyForInvoicingDate |
| InvoiceNumber |
| InvoicedDate |
| InvoicedByUserId |
| InvoicingNotes |
| IsActive |

### 17.2 OrderAssignments

| Field |
|---|
| Id |
| OrderRecordId |
| ApplicationUserId |
| ImportedUserText |
| AssignmentType |
| AssignedAt |
| AssignedByUserId |

### 17.3 OrderChecklistItems

| Field |
|---|
| Id |
| OrderRecordId |
| DisplayOrder |
| Text |
| IsCompleted |
| CompletedByUserId |
| CompletedAt |
| ImportedCompletedText |
| ShowInBoardView |

### 17.4 OrderNotes

| Field |
|---|
| Id |
| OrderRecordId |
| NoteType |
| NoteText |
| CreatedByUserId |
| CreatedAt |
| LastUpdatedByUserId |
| LastUpdatedAt |

### 17.5 OrderLabels

| Field |
|---|
| Id |
| OrderRecordId |
| LabelText |
| Source |

### 17.6 OrderBuildRecordLinks

| Field |
|---|
| Id |
| OrderRecordId |
| BuildRecordId |
| LinkType |
| LinkedAt |
| LinkedByUserId |

### 17.7 OrderStatusHistory

| Field |
|---|
| Id |
| OrderRecordId |
| OldStatus |
| NewStatus |
| ChangedByUserId |
| ChangedAt |
| Reason |

### 17.8 OrderImportBatches

| Field |
|---|
| Id |
| FileName |
| PlanId |
| PlanName |
| ExportDate |
| ImportedAt |
| ImportedByUserId |
| RowsRead |
| OrdersCreated |
| OrdersUpdated |
| OrdersSkipped |
| Warnings |
| Errors |

### 17.9 OrderImportWarnings

| Field |
|---|
| Id |
| OrderImportBatchId |
| RowNumber |
| PlannerTaskId |
| WarningType |
| Message |
| Severity |

---

## 18. Validation Rules

Required to create an Order manually:

| Field |
|---|
| Order Title |
| Status |
| Priority |

Recommended warnings:

| Condition |
|---|
| Customer not selected |
| Due date missing |
| Assigned user missing |
| Status is Shipped but shipped date missing |
| Status is Invoiced but invoice number missing |
| Status is Contract Ready for Invoicing but invoice readiness date missing |
| Order appears to require Build Record but none is linked |
| Checklist incomplete when moving to later statuses |

---

## 19. Better than Planner Features

The Orders module should provide improvements not practical in Planner:

- Structured status history.
- Structured checklist import and reporting.
- Controlled customer selection.
- Linkage to Build Records.
- Order-to-build traceability.
- Import preview and warnings.
- Idempotent imports.
- Reports for shipping/invoicing bottlenecks.
- Reports for unlinked orders.
- Audit history.
- Safer data model.
- Better search.
- Better export.
- More compact professional UI.
- No dependency on long unstructured notes.

---

## 20. Suggested Implementation Phases

### Phase Orders-1 — Foundation and Import

- Add specification.
- Add navigation.
- Add domain model.
- Add database migration.
- Add import batch/warning structure.
- Add Planner export parser.

### Phase Orders-2 — Core Order UI

- Add Orders register.
- Add Order detail.
- Add manual create/edit.
- Add status and assignment management.
- Add checklist UI.

### Phase Orders-3 — Integration

- Customer dropdown integration.
- Link Orders to Build Records.
- Add linked Orders to Build Record page.
- Add Support Ticket No. where useful.

### Phase Orders-4 — Operational Workflow

- Shipping fields.
- Invoicing fields.
- Workflow warnings.
- Board view.
- Status history.

### Phase Orders-5 — Reporting and Polish

- Reports.
- Exports.
- UI polish.
- Tests.
- UAT documentation.
- Planner migration guide.

---

## 21. Acceptance Criteria

The Orders module is successful when:

1. Users can import the Planner-exported spreadsheet.
2. Checklist items are parsed from semicolon-separated values.
3. Assigned users are imported or flagged for review.
4. Orders can be searched and filtered.
5. Orders can be viewed in a register.
6. Orders can be viewed in a board by status.
7. Orders can be created manually.
8. Orders can be edited.
9. Orders can be linked to Customers.
10. Orders can be linked to one or more Build Records.
11. Build Records show linked Orders.
12. Order status history is recorded.
13. Checklist progress is visible.
14. Shipping and invoicing readiness are visible.
15. Useful reports are available.
16. Sensitive Build Record secrets are not exposed.
17. No real customer data is copied into specification, seed data or GitHub work items.
18. The UI is compact, professional and more useful than Planner for BuildBook’s production workflow.

---

## 22. Summary

The Orders module should replace the Microsoft Planner production order workflow with a structured BuildBook module.

The key value is not just recreating a Planner board. The key value is linking order workflow to customers, Build Records, checklists, shipping, invoicing and reports.

The module should preserve the simplicity of a board/list workflow while adding the structure, search, traceability and reporting that Planner cannot provide.
