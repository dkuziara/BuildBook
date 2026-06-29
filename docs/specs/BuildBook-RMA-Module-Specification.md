# BuildBook RMA Module — Specification

## 1. Purpose

The **BuildBook RMA Module** will replace the current Microsoft Planner-based Returns/Repairs workflow with a structured, searchable and professional internal RMA management system.

The module will be part of the existing BuildBook application and will use the same SQL Server database.

The purpose is to record, track, repair, test, return and analyse returned products more effectively than is possible in Microsoft Planner.

The RMA module should provide:

* A clearer and more professional user experience.
* Structured RMA records instead of free-text task notes.
* Linkage between RMAs and existing Build Records where possible.
* Better search and filtering.
* Repair checklists.
* Fault diagnosis records.
* Testing and QA sign-off.
* Shipping and return tracking.
* Customer/contact details.
* Attachments and evidence.
* Audit history.
* Reporting and failure trend analysis.

---

## 2. Application Area

The RMA functionality should be added as a new top-level BuildBook area.

Suggested navigation:

```text
Home
Build Register
RMAs
Reports
Admin
```

The module should be called:

# RMAs

Each returned item should be called an:

# RMA Record

Example:

```text
RMA-0037 — Radsat2 — Possible hard drive failure — Sellafield
```

---

## 3. Design Goals

The RMA module should improve on the current Planner workflow.

The experience should be:

* Professional.
* Fast to use.
* More structured than Planner.
* Easier to search.
* Easier to report on.
* Better suited to technical support and repair work.
* Linked to BuildBook device/build information.
* Compact and operational rather than oversized or “landing page” styled.
* Suitable for daily internal use by Support, Production, QA and Management.

The UI should avoid the weaknesses of the current Planner workflow:

* Important information buried in long notes.
* No direct link to Build Records.
* No structured serial number/product/customer fields.
* Limited reporting.
* Limited workflow validation.
* Weak repair history.
* Weak failure trend analysis.
* Difficult to enforce required checks before shipping.
* Difficult to distinguish customer notes, internal notes, diagnosis and repair actions.

---

## 4. Technology

The RMA module should use the existing BuildBook technology stack:

| Area           | Technology                                       |
| -------------- | ------------------------------------------------ |
| Application    | ASP.NET Core Blazor Web App                      |
| Rendering      | Interactive Server rendering                     |
| Language       | C#                                               |
| UI             | Razor components                                 |
| Database       | Existing BuildBook SQL Server database           |
| Data access    | Entity Framework Core                            |
| Authentication | Existing BuildBook authentication                |
| Authorisation  | Existing BuildBook roles/permissions             |
| Audit          | Existing BuildBook audit approach where possible |

The RMA module must not be a separate application.

It must share the existing BuildBook database and should reuse existing BuildBook services, styling, layout, role handling and audit patterns.

---

## 5. Relationship to Build Records

Where possible, each RMA should link back to an existing **Build Record**.

An RMA may be linked by:

* Serial number.
* Product code.
* Machine name.
* Customer.
* Original order number.
* Invoice number.
* Manual selection.

The system should attempt to find matching Build Records when an RMA is created.

Example:

```text
Serial number entered: 1010002

Matching Build Record found:
Product: RADSAT2
Customer: Sellafield
Original Order: MS4259/OC-0376
Build Record ID: 123
```

The user should be able to confirm or reject the match.

If no Build Record exists, the RMA should still be allowed, but marked as:

```text
No linked Build Record
```

This is important because some returns may involve older products, third-party equipment, legacy devices, incomplete records or items not originally entered into BuildBook.

---

## 6. Core RMA Workflow

The workflow should support the following statuses.

| Status               | Meaning                                                             |
| -------------------- | ------------------------------------------------------------------- |
| Booked In            | Item has been received or return is expected                        |
| Work In Progress     | Item is being diagnosed or repaired                                 |
| Ready To Ship        | Repair/test work is complete and item is ready to return            |
| Shipped              | Item has been shipped back to the customer                          |
| On Hold              | Waiting for customer, parts, payment, approval or other blocker     |
| Cancelled / No Reply | RMA closed without repair or return due to cancellation/no response |
| Customer Fixed       | Customer resolved the issue without further repair work             |
| Closed               | Fully completed and archived                                        |

The existing Planner buckets are a good starting point, but BuildBook should make them more structured and enforce useful rules.

---

## 7. RMA Record Structure

Each RMA Record should have the following sections.

## 7.1 Summary

The top of the RMA detail page should show the key information:

| Field               |
| ------------------- |
| RMA Number          |
| Status              |
| Product Name        |
| Serial Number       |
| Customer            |
| Fault Summary       |
| Priority            |
| Assigned To         |
| Due Date            |
| Linked Build Record |
| Warranty Status     |
| Last Updated        |

Example:

```text
RMA-0037 — Radsat2 — Possible hard drive failure

Customer: Sellafield
Serial Number: 1010002
Status: Work In Progress
Priority: Medium
Assigned To: Giles
Linked Build Record: Yes
Warranty: Out of warranty
Due Date: 09/03/2026
```

---

## 7.2 Intake / Booking In

This section records the initial returned item information.

Fields:

| Field                     | Type                 | Notes                |
| ------------------------- | -------------------- | -------------------- |
| RMA Number                | Generated            | Required             |
| Date Created              | Date/time            | Required             |
| Created By                | User                 | Required             |
| Date Item Received        | Date                 | Optional initially   |
| Received By               | User                 | Optional             |
| Product Name              | Text / lookup        | Required             |
| Product Code              | Text / lookup        | Optional             |
| Serial Number             | Text                 | Required where known |
| Customer                  | Customer lookup/text | Required             |
| Contact Name              | Text                 | Optional             |
| Contact Email             | Text                 | Optional             |
| Contact Phone             | Text                 | Optional             |
| Customer Address          | Long text            | Optional             |
| Customer Reference        | Text                 | Optional             |
| Support Ticket Number     | Text                 | Optional             |
| Support Ticket URL        | URL                  | Optional             |
| Original Order Number     | Text                 | Optional             |
| Original Order Date       | Date                 | Optional             |
| Original Invoice Number   | Text                 | Optional             |
| Linked Build Record ID    | Lookup               | Optional             |
| Initial Fault Description | Long text            | Required             |

The system should support customer/contact details but avoid requiring excessive personal data where it is not needed.

---

## 7.3 Fault Details

This section records what has gone wrong.

Fields:

| Field              | Type           |
| ------------------ | -------------- |
| Fault Summary      | Short text     |
| Fault Description  | Long text      |
| Reported Symptoms  | Long text      |
| Fault Category     | Dropdown       |
| Fault Subcategory  | Dropdown       |
| Intermittent Fault | Yes/No         |
| Safety Concern     | Yes/No         |
| Data Loss Concern  | Yes/No         |
| Customer Impact    | Dropdown       |
| Reproducible       | Yes/No/Unknown |
| Initial Diagnosis  | Long text      |

Suggested fault categories:

| Category                  |
| ------------------------- |
| Hardware failure          |
| Software issue            |
| Firmware issue            |
| Disk/storage issue        |
| Power issue               |
| Network issue             |
| Configuration issue       |
| Licensing issue           |
| User/customer setup issue |
| Physical damage           |
| No fault found            |
| Unknown                   |

Useful improvement over Planner:

> Fault categories allow reporting such as “How many RMA cases are hard-drive failures?” or “Which product has the highest repeat return rate?”

---

## 7.4 Warranty and Commercial Details

This section helps decide whether the repair is chargeable.

Fields:

| Field                      | Type           |
| -------------------------- | -------------- |
| Warranty Status            | Dropdown       |
| Warranty Expiry Date       | Date           |
| Chargeable Repair          | Yes/No/Unknown |
| Customer Approval Required | Yes/No         |
| Customer Approval Received | Yes/No         |
| Approval Date              | Date           |
| Quote Number               | Text           |
| Purchase Order Number      | Text           |
| Invoice Number             | Text           |
| Estimated Repair Cost      | Currency       |
| Actual Repair Cost         | Currency       |
| Commercial Notes           | Long text      |

Suggested warranty statuses:

| Status            |
| ----------------- |
| In warranty       |
| Out of warranty   |
| Extended warranty |
| Warranty unknown  |
| Not applicable    |

Planner does not handle this well. BuildBook should make it clear whether work can proceed, whether customer approval is needed, and whether the repair is chargeable.

---

## 7.5 Assignment and Priority

Fields:

| Field                  | Type          |
| ---------------------- | ------------- |
| Assigned To            | User lookup   |
| Priority               | Dropdown      |
| Due Date               | Date          |
| Target Completion Date | Date          |
| On Hold Reason         | Dropdown/text |
| Escalation Required    | Yes/No        |
| Escalated To           | User lookup   |
| Escalation Notes       | Long text     |

Suggested priorities:

| Priority |
| -------- |
| Low      |
| Medium   |
| High     |
| Urgent   |

Suggested on-hold reasons:

| Reason                        |
| ----------------------------- |
| Waiting for customer          |
| Waiting for parts             |
| Waiting for payment           |
| Waiting for approval          |
| Waiting for test equipment    |
| Waiting for internal decision |
| Waiting for courier           |
| Other                         |

---

## 7.6 Diagnosis and Repair Work

This section records the technical work carried out.

Fields:

| Field                 | Type                  |
| --------------------- | --------------------- |
| Diagnosis Notes       | Long text             |
| Root Cause            | Long text             |
| Root Cause Category   | Dropdown              |
| Repair Action Taken   | Long text             |
| Parts Replaced        | Structured list       |
| Software Updated      | Yes/No                |
| Firmware Updated      | Yes/No                |
| Configuration Changed | Yes/No                |
| Licence Key Added     | Yes/No                |
| Data Recovered        | Yes/No/Not applicable |
| Data Destroyed        | Yes/No/Not applicable |
| Repair Completed Date | Date                  |
| Repair Completed By   | User lookup           |

Suggested root cause categories:

| Category                        |
| ------------------------------- |
| Component failure               |
| Disk/storage failure            |
| Power supply issue              |
| Corrupt software/configuration  |
| Firmware issue                  |
| Licensing/configuration missing |
| Customer environment issue      |
| Physical damage                 |
| Wear and tear                   |
| No fault found                  |
| Unknown                         |

Planner can store free-text repair notes, but BuildBook should separate diagnosis, root cause and repair action so that these can be searched and reported later.

---

## 7.7 Repair Checklist

Each RMA should support a checklist.

Default checklist template:

| Checklist Item                                    |
| ------------------------------------------------- |
| Diagnose fault                                    |
| Confirm serial number                             |
| Check linked Build Record                         |
| Confirm warranty status                           |
| Fix issue                                         |
| Run functional test                               |
| Run product-specific test                         |
| Check licence key                                 |
| Check antivirus/security where applicable         |
| Confirm BitLocker/recovery state where applicable |
| Clean/prepare device                              |
| Confirm return address                            |
| Confirm shipment approved                         |
| Arrange courier/collection                        |
| Mark shipped                                      |
| Close RMA                                         |

The checklist should be customisable per RMA.

Each item should record:

| Field          |
| -------------- |
| Checklist Item |
| Completed      |
| Completed By   |
| Completed At   |

Useful improvement over Planner:

> Checklist completion can be used to prevent moving an RMA to “Ready To Ship” before required repair/test items are complete.

---

## 7.8 Testing and QA Sign-Off

This section should record whether the item is safe and ready to return.

Fields:

| Field               | Type                   |
| ------------------- | ---------------------- |
| Test Required       | Yes/No                 |
| Test Plan Used      | Text                   |
| Test Result         | Pass/Fail/Not tested   |
| Tested By           | User lookup            |
| Test Date           | Date                   |
| QA Required         | Yes/No                 |
| QA Result           | Pass/Fail/Not required |
| QA Checked By       | User lookup            |
| QA Date             | Date                   |
| Test Notes          | Long text              |
| Release Approved    | Yes/No                 |
| Release Approved By | User lookup            |
| Release Approved At | Date/time              |

The system should optionally warn if an RMA is moved to **Ready To Ship** without a passed test or QA sign-off.

---

## 7.9 Shipping / Return

Fields:

| Field                      | Type        |
| -------------------------- | ----------- |
| Return Method              | Dropdown    |
| Courier                    | Text        |
| Tracking Number            | Text        |
| Collection Arranged        | Yes/No      |
| Collection Date            | Date        |
| Shipped Date               | Date        |
| Shipped By                 | User lookup |
| Return Address             | Long text   |
| Shipping Notes             | Long text   |
| Proof of Delivery Received | Yes/No      |
| Proof of Delivery Date     | Date        |

Suggested return methods:

| Method              |
| ------------------- |
| Customer collection |
| Courier             |
| Hand delivered      |
| Internal transport  |
| Not returned        |
| Other               |

Useful improvement over Planner:

> The system can show “Ready To Ship but not shipped”, “Shipped without tracking number”, or “On hold waiting for collection”.

---

## 7.10 Customer Communication

The system should allow internal tracking of customer communication.

Fields / records:

| Field              |
| ------------------ |
| Communication Date |
| Contact Method     |
| Contact Person     |
| Summary            |
| Follow-up Required |
| Follow-up Date     |
| Recorded By        |

Suggested contact methods:

| Method    |
| --------- |
| Email     |
| Phone     |
| Freshdesk |
| Teams     |
| In person |
| Other     |

Useful improvement over Planner:

> Communication history should not be mixed into repair notes. It should be searchable and clearly separated.

---

## 7.11 Attachments and Evidence

Each RMA should support attachments.

Attachment types:

| Type             |
| ---------------- |
| Photo            |
| Screenshot       |
| Test result      |
| Courier document |
| Customer email   |
| Quote            |
| Purchase order   |
| Invoice          |
| Diagnostic log   |
| Other            |

Attachment metadata:

| Field           |
| --------------- |
| File Name       |
| Uploaded By     |
| Uploaded At     |
| Attachment Type |
| Description     |

Possible examples:

* Photo of damaged unit.
* BIOS screen showing missing disk.
* Courier label.
* Customer email approval.
* Test output.
* Repair evidence.

---

## 7.12 Notes

Notes should be split rather than one giant free-text field.

Recommended note types:

| Note Type       | Purpose                         |
| --------------- | ------------------------------- |
| Internal Note   | General internal notes          |
| Diagnosis Note  | Technical diagnosis             |
| Repair Note     | What was repaired               |
| Customer Note   | Customer-facing summary         |
| Commercial Note | Payment/approval/warranty notes |

This avoids the Planner problem where all information ends up in one large unstructured notes block.

---

## 7.13 History

Each RMA should have a clear history/audit trail.

History should record:

| Event                     |
| ------------------------- |
| RMA created               |
| Status changed            |
| Assigned user changed     |
| Priority changed          |
| Due date changed          |
| Checklist item completed  |
| Fault category changed    |
| Build Record link changed |
| Warranty status changed   |
| Test result changed       |
| Shipping details changed  |
| Attachment uploaded       |
| Note added                |
| RMA closed                |

History entries should include:

| Field                         |
| ----------------------------- |
| Date/time                     |
| User                          |
| Action                        |
| Old value                     |
| New value                     |
| Comment/reason where relevant |

---

## 8. RMA List Page

The RMA list page should be compact and operational.

It should not look like a large marketing page.

Default columns:

| Column        |
| ------------- |
| RMA Number    |
| Status        |
| Product       |
| Serial Number |
| Customer      |
| Fault Summary |
| Priority      |
| Assigned To   |
| Due Date      |
| Warranty      |
| Last Updated  |

Required features:

* Search by RMA number.
* Search by serial number.
* Search by customer.
* Search by product.
* Search by fault text.
* Search by support ticket number.
* Filter by status.
* Filter by assigned user.
* Filter by priority.
* Filter by customer.
* Filter by product.
* Filter by warranty status.
* Filter by overdue.
* Filter by linked/unlinked Build Record.
* Sort by due date, status, priority and last updated.
* Export non-sensitive RMA list to CSV/Excel.

The RMA list should show important work quickly without excessive vertical space.

---

## 9. RMA Board View

The module should include a board-style view similar to Planner, but more useful.

Columns:

| Column               |
| -------------------- |
| Booked In            |
| Work In Progress     |
| Ready To Ship        |
| Shipped              |
| On Hold              |
| Cancelled / No Reply |
| Customer Fixed       |
| Closed               |

Each card should show:

| Field                         |
| ----------------------------- |
| RMA Number                    |
| Product                       |
| Customer                      |
| Fault Summary                 |
| Priority                      |
| Assigned To                   |
| Due Date                      |
| Linked Build Record indicator |
| Checklist progress            |
| Overdue indicator             |

Example card:

```text
RMA-0037
Radsat2 — Sellafield
Possible hard drive failure

Status: Work In Progress
Priority: Medium
Assigned: Giles
Checklist: 4/5
Due: 09/03/2026
Linked Build: Yes
```

Useful improvements over Planner:

* Cards should be generated from structured data.
* Moving between statuses can enforce rules.
* The board can show warnings such as overdue, missing serial number or not linked to a Build Record.
* The board should support filters.

---

## 10. RMA Detail Page

The RMA detail page should feel better than a Planner task popup.

Recommended layout:

### Top Summary Bar

* RMA number.
* Status.
* Product.
* Customer.
* Serial number.
* Priority.
* Assigned user.
* Due date.
* Linked Build Record button.

### Main Content

Use sections or tabs:

| Tab / Section      |
| ------------------ |
| Overview           |
| Intake             |
| Fault              |
| Diagnosis & Repair |
| Checklist          |
| Testing / QA       |
| Shipping           |
| Communication      |
| Attachments        |
| Notes              |
| History            |

### Side Panel

A right-hand side panel could show:

* Current status.
* Assigned user.
* Priority.
* Due date.
* Checklist progress.
* Linked Build Record.
* Support ticket link.
* Warranty status.
* Quick actions.

This would be more useful than Planner’s chat-focused layout because the structured RMA data remains the centre of the page.

---

## 11. Link from Build Record to RMAs

The Build Record detail page should include an **RMAs** section.

This section should show all RMAs linked to the Build Record.

Columns:

| Column        |
| ------------- |
| RMA Number    |
| Status        |
| Fault Summary |
| Date Created  |
| Date Closed   |
| Root Cause    |
| Outcome       |

This is one of the strongest reasons to include RMAs in BuildBook.

From a Build Record, a user should be able to answer:

* Has this device been returned before?
* How many times?
* What failed?
* What was repaired?
* Was it in warranty?
* Was it returned to the customer?
* Was the same fault seen before?

---

## 12. RMA Creation Workflow

The user should be able to create a new RMA from:

1. The RMA module.
2. A Build Record.
3. A search result.
4. A future Freshdesk link, if integration is added later.

Minimum required fields to create an RMA:

| Field                     |
| ------------------------- |
| Customer                  |
| Product Name              |
| Fault Summary             |
| Initial Fault Description |

Strongly recommended fields:

| Field                 |
| --------------------- |
| Serial Number         |
| Support Ticket Number |
| Contact Name          |
| Due Date              |
| Priority              |

Creation behaviour:

1. User enters customer/product/serial/fault information.
2. System searches for matching Build Records.
3. User links to a matching Build Record or continues without a link.
4. System assigns the next RMA number.
5. Status defaults to **Booked In**.
6. Default repair checklist is created.
7. Audit history records RMA creation.

---

## 13. RMA Numbering

RMA numbers should be generated automatically.

Suggested format:

```text
RMA-0001
RMA-0002
RMA-0003
```

The numbering should be unique and sequential.

Optional future format:

```text
RMA-2026-0037
```

For Version 1, simple sequential numbering is acceptable.

---

## 14. Status Transition Rules

The system should support status transitions.

Initial rules:

| From                 | To                   | Rule                                 |
| -------------------- | -------------------- | ------------------------------------ |
| Booked In            | Work In Progress     | Allowed                              |
| Work In Progress     | On Hold              | Allowed                              |
| On Hold              | Work In Progress     | Allowed                              |
| Work In Progress     | Ready To Ship        | Warn if checklist/test incomplete    |
| Ready To Ship        | Shipped              | Require shipped date or confirmation |
| Any open status      | Cancelled / No Reply | Require reason                       |
| Any open status      | Customer Fixed       | Require note                         |
| Shipped              | Closed               | Allowed                              |
| Customer Fixed       | Closed               | Allowed                              |
| Cancelled / No Reply | Closed               | Allowed                              |

The system does not need to be overly restrictive at first, but it should warn users when important information is missing.

Examples:

* Moving to Ready To Ship without test result.
* Moving to Shipped without tracking number or shipped date.
* Closing without outcome/root cause.
* No linked Build Record when serial number appears to match one.

---

## 15. RMA Outcomes

When closing an RMA, the system should require an outcome.

Suggested outcomes:

| Outcome                                |
| -------------------------------------- |
| Repaired and returned                  |
| Replaced and returned                  |
| No fault found                         |
| Customer fixed                         |
| Cancelled                              |
| Scrapped                               |
| Returned unrepaired                    |
| Awaiting customer response then closed |
| Other                                  |

Closing fields:

| Field                   |
| ----------------------- |
| Closed Date             |
| Closed By               |
| Outcome                 |
| Closure Notes           |
| Root Cause              |
| Customer-facing Summary |

This will make reporting much more useful than Planner.

---

## 16. Search Requirements

RMA search should search across:

* RMA number.
* Product name.
* Product code.
* Serial number.
* Customer.
* Contact name.
* Fault summary.
* Fault description.
* Diagnosis notes.
* Repair action.
* Root cause.
* Support ticket number.
* Original order number.
* Linked Build Record serial number.
* Assigned user.
* Shipping tracking number.

Search should not expose secrets from the linked Build Record.

---

## 17. Reporting Requirements

The RMA module should include reports that Planner cannot easily provide.

## 17.1 Operational Reports

* Open RMAs.
* Overdue RMAs.
* RMAs by status.
* RMAs by assigned user.
* RMAs due this week.
* RMAs waiting for customer.
* RMAs waiting for parts.
* Ready to ship but not shipped.
* Shipped but not closed.

## 17.2 Customer Reports

* RMAs by customer.
* RMAs for a customer within date range.
* Repeat returns by customer.
* Customer devices with multiple RMAs.

## 17.3 Product / Failure Reports

* RMAs by product.
* RMAs by fault category.
* RMAs by root cause category.
* Repeat failures by serial number.
* RMAs by firmware version, where linked to Build Record.
* RMAs by RadSight version, where linked to Build Record.
* Average repair time by product.
* Average repair time by fault category.
* Products with highest RMA count.

## 17.4 Commercial Reports

* Chargeable repairs.
* Out-of-warranty repairs.
* Repairs awaiting approval.
* Repairs awaiting payment.
* Repair costs by customer.
* Repair costs by product.

---

## 18. Useful Features Not Practical in Planner

The following should be considered important improvements over Planner.

## 18.1 Automatic Build Record Matching

When a serial number is entered, the system should suggest matching Build Records.

This allows automatic display of:

* Product code.
* Product name.
* Customer.
* Original order.
* Shipping date.
* Software version.
* Firmware version.
* Machine name.
* Previous RMAs.

---

## 18.2 Repeat Failure Detection

If the same serial number has previous RMAs, show a warning:

```text
This device has 2 previous RMAs.
```

If the same product/fault category appears frequently, the system should support future trend analysis.

---

## 18.3 Warranty Awareness

If the linked Build Record has a date shipped, the system can infer an approximate warranty status if warranty rules are added later.

Example:

```text
Date shipped: 17/10/2019
Likely warranty status: Out of warranty
```

For Version 1, this can be manual, but the design should allow automatic rules later.

---

## 18.4 Required Checks Before Shipping

The system can warn if:

* Checklist incomplete.
* Test not passed.
* QA not signed off.
* Return address missing.
* Shipping method missing.
* Customer approval missing.
* Chargeable repair not approved.

---

## 18.5 Structured Root Cause Data

Instead of relying only on notes, the system should record structured root cause categories.

This allows later reports such as:

```text
Top 10 failure causes this year
```

---

## 18.6 Linked Freshdesk Ticket

The system should allow the user to store:

* Freshdesk ticket number.
* Freshdesk ticket URL.

Full Freshdesk integration is out of scope initially, but storing the link is useful immediately.

---

## 18.7 Parts Replaced

The system should allow recording parts replaced.

Fields:

| Field         |
| ------------- |
| Part Name     |
| Part Number   |
| Quantity      |
| Serial Number |
| Cost          |
| Supplier      |
| Notes         |

This allows tracking repeated part failures and repair costs.

---

## 18.8 Customer-Facing Repair Summary

The system should allow a clean customer-facing summary to be written separately from internal notes.

This could later be exported into an email or PDF.

---

## 18.9 RMA Timeline

Each RMA should have a timeline showing:

* Created.
* Booked in.
* Work started.
* On hold.
* Repair completed.
* Tested.
* Ready to ship.
* Shipped.
* Closed.

This is easier to understand than Planner comments and task history.

---

## 18.10 RMA Metrics

The system should calculate:

* Days open.
* Days in current status.
* Days on hold.
* Time from received to repaired.
* Time from ready to ship to shipped.
* Total repair time.

These metrics are not readily available in Planner.

---

## 19. Permissions

The RMA module should reuse BuildBook roles where possible.

Suggested permissions:

| Action                      | Role                                   |
| --------------------------- | -------------------------------------- |
| View RMAs                   | Viewer, Editor, Administrator          |
| Create RMA                  | Editor, Administrator                  |
| Edit RMA                    | Editor, Administrator                  |
| Change RMA status           | Editor, Administrator                  |
| Close RMA                   | Editor, Administrator                  |
| Delete RMA                  | Administrator only, preferably avoided |
| View commercial/cost fields | Editor, Administrator                  |
| Manage RMA settings         | Administrator                          |
| Export RMA reports          | Viewer, Editor, Administrator          |

Sensitive Build Record secrets must not be shown through the RMA module.

---

## 20. Data Model

The RMA module should use the existing BuildBook SQL Server database.

Suggested tables:

## 20.1 RmaRecords

| Field                    |
| ------------------------ |
| Id                       |
| RmaNumber                |
| BuildRecordId            |
| Status                   |
| Priority                 |
| AssignedToUserId         |
| CreatedByUserId          |
| CreatedAt                |
| LastUpdatedByUserId      |
| LastUpdatedAt            |
| ClosedByUserId           |
| ClosedAt                 |
| ProductCode              |
| ProductName              |
| SerialNumber             |
| CustomerId               |
| ContactName              |
| ContactEmail             |
| ContactPhone             |
| CustomerAddress          |
| SupportTicketNumber      |
| SupportTicketUrl         |
| OriginalOrderNumber      |
| OriginalOrderDate        |
| OriginalInvoiceNumber    |
| FaultSummary             |
| FaultDescription         |
| FaultCategory            |
| FaultSubcategory         |
| ReportedSymptoms         |
| InitialDiagnosis         |
| RootCause                |
| RootCauseCategory        |
| RepairActionTaken        |
| WarrantyStatus           |
| WarrantyExpiryDate       |
| ChargeableRepair         |
| CustomerApprovalRequired |
| CustomerApprovalReceived |
| CustomerApprovalDate     |
| QuoteNumber              |
| PurchaseOrderNumber      |
| RepairInvoiceNumber      |
| EstimatedRepairCost      |
| ActualRepairCost         |
| DateItemReceived         |
| ReceivedByUserId         |
| RepairCompletedDate      |
| RepairCompletedByUserId  |
| TestRequired             |
| TestPlanUsed             |
| TestResult               |
| TestedByUserId           |
| TestDate                 |
| QaRequired               |
| QaResult                 |
| QaCheckedByUserId        |
| QaDate                   |
| ReleaseApproved          |
| ReleaseApprovedByUserId  |
| ReleaseApprovedAt        |
| ReturnMethod             |
| Courier                  |
| TrackingNumber           |
| CollectionArranged       |
| CollectionDate           |
| ShippedDate              |
| ShippedByUserId          |
| ReturnAddress            |
| ShippingNotes            |
| Outcome                  |
| ClosureNotes             |
| CustomerFacingSummary    |
| IsActive                 |

---

## 20.2 RmaChecklistItems

| Field             |
| ----------------- |
| Id                |
| RmaRecordId       |
| DisplayOrder      |
| Text              |
| IsCompleted       |
| CompletedByUserId |
| CompletedAt       |
| ShowInBoardView   |

---

## 20.3 RmaNotes

| Field               |
| ------------------- |
| Id                  |
| RmaRecordId         |
| NoteType            |
| NoteText            |
| CreatedByUserId     |
| CreatedAt           |
| LastUpdatedByUserId |
| LastUpdatedAt       |

---

## 20.4 RmaCommunications

| Field             |
| ----------------- |
| Id                |
| RmaRecordId       |
| CommunicationDate |
| ContactMethod     |
| ContactPerson     |
| Summary           |
| FollowUpRequired  |
| FollowUpDate      |
| CreatedByUserId   |
| CreatedAt         |

---

## 20.5 RmaAttachments

| Field            |
| ---------------- |
| Id               |
| RmaRecordId      |
| FileName         |
| StoredFilePath   |
| ContentType      |
| AttachmentType   |
| Description      |
| UploadedByUserId |
| UploadedAt       |

---

## 20.6 RmaParts

| Field        |
| ------------ |
| Id           |
| RmaRecordId  |
| PartName     |
| PartNumber   |
| Quantity     |
| SerialNumber |
| Supplier     |
| UnitCost     |
| Notes        |

---

## 20.7 RmaStatusHistory

| Field           |
| --------------- |
| Id              |
| RmaRecordId     |
| OldStatus       |
| NewStatus       |
| ChangedByUserId |
| ChangedAt       |
| Reason          |

---

## 20.8 RmaAudit

The RMA module may use the existing BuildBook audit mechanism if suitable. If not, add an RMA-specific audit table.

| Field        |
| ------------ |
| Id           |
| RmaRecordId  |
| UserId       |
| Action       |
| FieldChanged |
| OldValue     |
| NewValue     |
| CreatedAt    |

---

## 21. UI Requirements

The RMA module should follow the professional compact BuildBook styling.

Specific UI requirements:

* Use compact page headers.
* Avoid oversized hero titles.
* Make lists and key operational data visible high on the page.
* Use consistent buttons.
* Use consistent cards.
* Use clear status badges.
* Use clear priority indicators.
* Avoid full-page horizontal scrolling.
* Use structured sections rather than a single large notes field.
* Make the RMA detail page feel calmer and clearer than Planner.
* Provide a better experience than Planner’s dark modal/task-panel layout.
* Keep the design consistent with the rest of BuildBook.

---

## 22. RMA Dashboard

The RMA module should have a dashboard or summary area.

Useful summary cards:

| Card                    |
| ----------------------- |
| Open RMAs               |
| Overdue RMAs            |
| Waiting for customer    |
| Waiting for parts       |
| Ready to ship           |
| Shipped not closed      |
| RMAs created this month |
| Average days open       |

This should be compact and operational.

---

## 23. Validation Rules

Required at creation:

| Field             |
| ----------------- |
| Customer          |
| Product Name      |
| Fault Summary     |
| Fault Description |

Recommended warnings:

| Condition               |
| ----------------------- |
| Serial number missing   |
| No linked Build Record  |
| Due date missing        |
| Contact email missing   |
| Warranty status unknown |
| Support ticket missing  |
| Fault category missing  |

Required before Ready To Ship:

| Field / Condition                         |
| ----------------------------------------- |
| Repair action recorded                    |
| Checklist substantially complete          |
| Test result recorded, if required         |
| QA result recorded, if required           |
| Customer approval received, if chargeable |
| Return address recorded                   |

Required before Shipped:

| Field / Condition                        |
| ---------------------------------------- |
| Shipped date                             |
| Return method                            |
| Courier/tracking number where applicable |

Required before Closed:

| Field / Condition                  |
| ---------------------------------- |
| Outcome                            |
| Closure notes                      |
| Root cause or reason unknown       |
| Customer-facing summary, if needed |

---

## 24. Notifications and Reminders

Version 1 does not need full email automation, but the design should allow it later.

Possible future notifications:

* RMA assigned to user.
* RMA overdue.
* RMA waiting for customer too long.
* RMA ready to ship.
* RMA shipped.
* RMA closed.
* Customer approval required.
* On-hold item needs review.

For the initial implementation, dashboard/report visibility may be enough.

---

## 25. Import from Planner

A one-off manual migration may be needed.

Minimum import approach:

* Manually recreate active RMAs in BuildBook.
* Store original Planner task title.
* Store original Planner notes in an internal note.
* Store checklist items where practical.
* Store original created date if known.
* Store original assigned user if known.

A fully automated Planner import is out of scope unless needed later.

---

## 26. Out of Scope for Initial RMA Module

The following should not be implemented in the first RMA module unless specifically approved:

* Customer portal.
* Customer self-service RMA creation.
* Automatic Freshdesk API integration.
* Automatic courier integration.
* Automatic email sending.
* Automatic warranty calculation rules.
* Inventory/stock control for spare parts.
* Full finance system integration.
* Mobile app.
* External access.
* Digital signature capture.

---

## 27. Suggested Implementation Phases

## Phase RMA-1 — Core RMA Register

* Database entities.
* RMA number generation.
* Create RMA.
* View RMA.
* Edit basic RMA fields.
* RMA list with search/filter.
* Link RMA to Build Record.
* Basic status workflow.

## Phase RMA-2 — Repair Workflow

* Diagnosis and repair sections.
* Checklist.
* Testing/QA sign-off.
* Shipping/return fields.
* Status transition warnings.
* RMA history.

## Phase RMA-3 — Board and Reporting

* Board view.
* Operational reports.
* Customer/product/failure reports.
* Repeat failure detection.
* Build Record RMA history section.

## Phase RMA-4 — Attachments and Communication

* Attachments.
* Customer communication log.
* Parts replaced.
* Customer-facing summary.

## Phase RMA-5 — Optional Integrations

* Freshdesk link improvements.
* Planner import helper.
* Notifications.
* Email templates.
* Courier tracking integration.

---

## 28. Acceptance Criteria

The RMA module is successful when:

1. Users can create a structured RMA record.
2. Users can search and filter RMAs.
3. Users can link an RMA to an existing Build Record.
4. Users can open a Build Record and see linked RMAs.
5. Users can track status from Booked In through Closed.
6. Users can record diagnosis, repair action, test result and shipping details.
7. Users can use a repair checklist.
8. Users can record customer communication separately from technical notes.
9. Users can see RMA history.
10. Users can identify overdue RMAs.
11. Users can report on RMAs by customer, product, status and fault category.
12. Users can identify repeat RMAs for the same serial number.
13. Sensitive Build Record secrets are not exposed through the RMA module.
14. The UI looks professional and is more pleasant to use than Planner.
15. The module uses the same SQL Server database as BuildBook.
16. Existing BuildBook functionality continues to work.

---

## 29. Summary

The BuildBook RMA Module should replace the current Microsoft Planner workflow with a structured internal RMA system.

The most important improvements are:

* Direct linkage to Build Records.
* Structured fault, repair, test and shipping data.
* Better search and reporting.
* Better visibility of status and ownership.
* Better repeat-failure analysis.
* Better warranty/commercial tracking.
* Better audit history.
* More professional and focused user experience.

Planner is useful for simple task tracking, but BuildBook can become the authoritative record of the returned device, what failed, what was done, and what happened afterwards.
