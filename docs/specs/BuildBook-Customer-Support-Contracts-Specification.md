# BuildBook Customer & Support Contracts Module — Specification

## 1. Purpose

Add a new **Customers** module to BuildBook for storing customer/account details, contact details, support contract information and support ticket link configuration.

The module should prevent duplicated customer names across Build Records and RMAs by making customer selection use a controlled dropdown/list.

The module should also allow RMA priority and urgency to be influenced by a customer’s support contract level and contract status.

---

## 2. Navigation

Add a new top-level navigation item:

```text
Home
Build Register
RMAs
Customers
Reports
Admin
```

The new module should be called:

```text
Customers
```

---

## 3. Customer Records

Each customer should have one master customer record.

### 3.1 Customer Fields

| Field | Type | Notes |
|---|---|---|
| Customer Name | Text | Required, unique where practical |
| Account Code | Text | Optional internal reference |
| Address Line 1 | Text | Optional |
| Address Line 2 | Text | Optional |
| Town / City | Text | Optional |
| County / Region | Text | Optional |
| Postcode | Text | Optional |
| Country | Text | Optional |
| Main Phone | Text | Optional |
| Main Email | Text | Optional |
| Website | Text | Optional |
| Primary Contact Name | Text | Optional |
| Primary Contact Email | Text | Optional |
| Primary Contact Phone | Text | Optional |
| Support Contract Level | Dropdown | Bronze/Silver/Gold/etc., or none |
| Support Contract Status | Dropdown | Active/Expired/None/etc. |
| Support Contract Start Date | Date | Optional |
| Support Contract End Date | Date | Optional |
| Support Notes | Long text | Optional |
| Is Active | Boolean | Allows hiding old customers without deleting them |

### 3.2 Customer Status

Suggested customer statuses:

```text
Active
Inactive
Archived
```

This is separate from support contract status.

---

## 4. Support Contract Levels

Support contract levels should be editable in Admin or Customers settings.

Default examples:

| Level | Example Response Time | Notes |
|---|---|---|
| Bronze | 2 working days | Basic support |
| Silver | 1 working day | Standard support |
| Gold | 4 working hours | Priority support |

These must be editable, not hard-coded.

### 4.1 Support Contract Level Fields

| Field | Type | Notes |
|---|---|---|
| Name | Text | Bronze, Silver, Gold, etc. |
| Description | Text | Optional |
| Target Response Time Value | Number | Example: 4 |
| Target Response Time Unit | Dropdown | Hours / Working Hours / Days / Working Days |
| RMA Priority Weight | Number | Used to influence suggested RMA priority |
| Default RMA Priority | Dropdown | Optional |
| Is Active | Boolean | Allows old levels to be retired |
| Display Order | Number | Controls dropdown order |

This allows levels to be renamed later, for example:

```text
Bronze -> Standard
Silver -> Enhanced
Gold -> Premium
```

without changing the application logic.

---

## 5. Support Contract Statuses

Support contract status should also be controlled.

Suggested statuses:

```text
No Contract
Active
Expired
Pending Renewal
Suspended
Unknown
```

Some customers will have no contract, so the system must support:

```text
Support Contract Level: None
Support Contract Status: No Contract
```

or:

```text
Support Contract Level: blank
Support Contract Status: No Contract
```

Use **No Contract** explicitly rather than relying on a blank field.

---

## 6. Impact on RMA Priority

Customer contract level should influence RMA priority, but it should not blindly override the user’s chosen priority.

Recommended design:

### 6.1 Store Two Priority Values on RMA

| Field | Purpose |
|---|---|
| Selected Priority | The actual priority chosen by the user |
| Suggested Priority | Calculated from contract level, warranty/commercial rules, fault impact and urgency |

Example:

```text
Customer: Sellafield
Contract Level: Gold
Fault Impact: Device unusable
Suggested Priority: High
Selected Priority: Medium
Warning: Customer contract suggests High priority.
```

### 6.2 Suggested Priority Rules

Initial simple rule:

| Contract Level | Suggested Minimum RMA Priority |
|---|---|
| Gold | High |
| Silver | Medium |
| Bronze | Medium or Low |
| No Contract | Low or Medium |
| Expired | Low or Medium |
| Unknown | Medium |

A more advanced rule could be added later:

```text
Suggested Priority = Contract Level + Fault Impact + Safety Concern + Due Date + Customer Impact
```

For Version 1 of this module, keep it simple:

- If customer has **Gold** and RMA priority is below **High**, show a warning.
- If customer has **No Contract**, do not auto-promote priority.
- If support contract is **Expired**, show a warning.
- Allow authorised users to override the suggestion.

Do not silently change RMA priority behind the user’s back.

---

## 7. Customer Dropdowns in Build and RMA Modules

Any customer field in BuildBook should use the central Customers table.

### 7.1 Build Records

Current Build Record customer fields should change from free text to a customer lookup/dropdown.

| Current | New |
|---|---|
| Customer text field | CustomerId linked to Customers table |

The Build Record should display the customer name, but store the Customer ID.

### 7.2 RMAs

RMA customer fields should also use the same customer table.

| Current | New |
|---|---|
| Customer text field | CustomerId linked to Customers table |

The RMA creation screen should allow:

- Select existing customer from dropdown/search.
- Create new customer if authorised.
- Warn if the typed name resembles an existing customer.

The dropdown should ideally be searchable, not a tiny fixed select box, because customer lists grow.

---

## 8. Customer Deduplication and Migration

Existing Build Records and RMAs may already contain customer names as text.

A migration/import process should:

1. Read distinct customer names from Build Records.
2. Read distinct customer names from RMAs.
3. Create Customer records.
4. Link Build Records to matching Customer records.
5. Link RMAs to matching Customer records.
6. Preserve original customer text in a legacy field if needed.
7. Report possible duplicates.

Example duplicate warning:

```text
Possible duplicate customers:
- Sellafield
- Sellafield Ltd
- Sellafield Sites
```

Do not automatically merge uncertain matches. Show them for manual review.

---

## 9. Support Ticket No.

There should be **one support ticket field** called:

```text
Support Ticket No.
```

It should store only the identifier, for example:

```text
5678
```

Do not store the whole Freshdesk URL in each RMA or Build Record.

### 9.1 Rename Existing Freshdesk Fields

Any existing fields such as:

```text
Freshdesk Ticket
Freshdesk URL
Support Ticket URL
Freshdesk Ticket Number
FreshdeskUrl
```

should be consolidated into:

```text
SupportTicketNo
```

Displayed label:

```text
Support Ticket No.
```

### 9.2 Where It Should Appear

The field should appear where useful, especially:

- RMA intake/details.
- RMA list/register.
- RMA search.
- RMA reports.
- Possibly Build Record notes/support section, if Build Records need to link directly to support tickets.

For RMAs, this field is definitely useful.

For Build Records, only include it if there is a genuine need to link a Build Record directly to a support ticket. Otherwise, keep support tickets on the RMA records.

---

## 10. Support Site URL Template

Add an Admin setting:

```text
Support Site URL Template
```

Example:

```text
https://charthousedatamanagement.freshdesk.com/a/tickets/{1}
```

When an RMA has:

```text
Support Ticket No. = 5678
```

BuildBook should generate:

```text
https://charthousedatamanagement.freshdesk.com/a/tickets/5678
```

and display a button or link:

```text
Open Support Ticket
```

### 10.1 Admin Settings

Add an Admin area:

```text
Admin > System Settings
```

or extend the existing Admin page.

Fields:

| Field | Type | Notes |
|---|---|---|
| Support Site URL Template | Text | Example contains `{1}` |
| Support Ticket Label | Text | Optional; default “Support Ticket No.” |

### 10.2 Validation

The URL template should be validated.

Rules:

- Must start with `http://` or `https://`.
- Must contain `{1}`.
- Must not allow `javascript:` or unsafe schemes.
- Ticket number should be URL-encoded before insertion.
- If no template is configured, show the ticket number but not the link/button.
- If no ticket number is present, hide or disable the button.

### 10.3 Display Behaviour

In RMA detail:

```text
Support Ticket No.: 5678    [Open Support Ticket]
```

In RMA list:

```text
5678
```

or:

```text
5678 ↗
```

In customer support history:

```text
RMA-0037 | Support Ticket No. 5678 | Open Support Ticket
```

---

## 11. Customer Page Layout

### 11.1 Customer List Page

Columns:

| Column |
|---|
| Customer Name |
| Primary Contact |
| Email |
| Phone |
| Support Contract Level |
| Support Contract Status |
| Contract End Date |
| Active |
| Last Updated |

Features:

- Search by customer name.
- Search by contact name.
- Search by email.
- Filter by contract level.
- Filter by contract status.
- Filter by active/inactive.
- Sort by name, contract level, status and end date.

### 11.2 Customer Detail Page

Sections:

```text
Summary
Address
Contacts
Support Contract
Linked Build Records
Linked RMAs
Notes
History
```

The customer detail page should show:

- All Build Records for that customer.
- All RMAs for that customer.
- Open RMAs.
- Overdue RMAs.
- Support contract level/status.
- Support ticket references through related RMAs.

This is where the module becomes more useful than simple dropdown management.

---

## 12. Reports

Add customer/support reports:

| Report | Purpose |
|---|---|
| Customers by contract level | Shows Bronze/Silver/Gold/etc. |
| Customers with no contract | Useful commercial/support view |
| Expired contracts | Customers needing renewal |
| Contracts expiring soon | Next 30/60/90 days |
| Open RMAs by contract level | Shows service demand by contract |
| Overdue RMAs by contract level | Helps check SLA risk |
| RMA priority mismatch | Gold customer with low/medium RMA priority |
| RMAs with no support ticket number | Data quality |

---

## 13. Data Model

### 13.1 Customers Table

Suggested table:

```text
Customers
```

Fields:

```text
Id
Name
AccountCode
AddressLine1
AddressLine2
TownCity
CountyRegion
Postcode
Country
MainPhone
MainEmail
Website
PrimaryContactName
PrimaryContactEmail
PrimaryContactPhone
SupportContractLevelId
SupportContractStatus
SupportContractStartDate
SupportContractEndDate
SupportNotes
IsActive
CreatedAt
CreatedBy
LastUpdatedAt
LastUpdatedBy
```

### 13.2 SupportContractLevels Table

Suggested table:

```text
SupportContractLevels
```

Fields:

```text
Id
Name
Description
TargetResponseTimeValue
TargetResponseTimeUnit
DefaultRmaPriority
RmaPriorityWeight
DisplayOrder
IsActive
CreatedAt
CreatedBy
LastUpdatedAt
LastUpdatedBy
```

### 13.3 SystemSettings Table

If not already present:

```text
SystemSettings
```

Fields:

```text
Id
Key
Value
Description
LastUpdatedAt
LastUpdatedBy
```

Example setting:

```text
Key: SupportTicketUrlTemplate
Value: https://charthousedatamanagement.freshdesk.com/a/tickets/{1}
```

### 13.4 BuildRecords Changes

Add or confirm:

```text
CustomerId
```

If existing customer text remains temporarily:

```text
LegacyCustomerName
```

### 13.5 RmaRecords Changes

Add or confirm:

```text
CustomerId
SupportTicketNo
SuggestedPriority
PriorityOverrideReason
```

Remove or deprecate:

```text
FreshdeskTicketUrl
SupportTicketUrl
FreshdeskTicketNumber
FreshdeskUrl
```

The exact existing field names may differ, so Codex should inspect the current model first.

---

## 14. Permissions

Suggested permissions:

| Action | Role |
|---|---|
| View customers | Viewer, Editor, Administrator |
| Add customer | Editor, Administrator |
| Edit customer | Editor, Administrator |
| Manage support contract levels | Administrator |
| Manage support site URL template | Administrator |
| View customer contract status | Viewer, Editor, Administrator |
| Export customer list | Viewer, Editor, Administrator |

---

## 15. Module Naming

This module is **CRM-adjacent**, but it should not be called a CRM inside BuildBook.

Recommended module name:

```text
Customers
```

Recommended feature name:

```text
Customer & Support Contracts
```

Reason: it is clear, practical, and does not imply a full sales CRM.

---

## 16. Suggested Implementation Order

1. Add Customer and Support Contract Level entities.
2. Add support contract level admin management.
3. Add customer list/detail/create/edit pages.
4. Migrate existing Build/RMA customer text into Customer records.
5. Change Build Record customer fields to dropdown/search customer selector.
6. Change RMA customer fields to dropdown/search customer selector.
7. Add Support Ticket No. field and remove/rename Freshdesk-specific fields.
8. Add Support Site URL Template setting in Admin.
9. Add Open Support Ticket link/button.
10. Add RMA suggested-priority behaviour based on support contract level.
11. Add customer reports.

---

## 17. Important Design Recommendation

Do not make this a big CRM system.

Make it a **Customer Master Data** module for BuildBook.

Its job is to answer:

- Who is this customer?
- What contact details do we have?
- What support contract do they have?
- Are they in contract?
- What Build Records belong to them?
- What RMAs belong to them?
- What support tickets are linked?
- Should their contract level influence RMA urgency?

That is exactly the right scope for BuildBook.
