# Customer Module Guide

## Purpose

The `Customers` module keeps shared customer records, support contract details and support-ticket references consistent across Build Records and RMAs.

It is a BuildBook operational module, not a separate CRM system.

## What the module covers

- Customer list, detail, create and edit screens
- Editable support contract levels
- Shared customer lookup in Build Records and RMAs
- `Support Ticket No.` storage on RMAs
- Support ticket links generated from the Admin URL template
- Customer reports and safe exports
- RMA priority guidance based on support contract level

## Core screens

### Customers

Use `/customers` to:

- Search customer names, contacts and contract details
- Filter by contract level, contract status and active state
- Open a customer record
- Add a customer if you have permission
- Open customer reports

### Customer detail

Use a customer detail page to review:

- Summary and contract status
- Address and contact information
- Linked Build Records
- Linked RMAs
- Support Ticket No. references via linked RMAs

### Support contract levels

Use `Admin > Support Contract Levels` to manage editable levels such as Bronze, Silver and Gold.

These levels are not hard-coded. BuildBook uses the configured default RMA priority to suggest urgency on customer-linked RMAs.

### System settings

Use `Admin > System Settings` to manage:

- `Support ticket label`
- `Support Site URL Template`

The URL template must start with `http://` or `https://` and include `{1}` as the placeholder for the ticket number.

Example:

`https://charthousedatamanagement.freshdesk.com/a/tickets/{1}`

If the template is blank, BuildBook still shows `Support Ticket No.` but does not offer an external link.

## Reports

Use `/customers/reports` to review:

- Customers by contract level
- Customers with no contract
- Expired contracts
- Contracts expiring within 30, 60 or 90 days
- Open RMAs by contract level
- Overdue RMAs by contract level
- RMAs with no `Support Ticket No.`
- RMAs where selected priority is below the contract-suggested priority

Exports are available to CSV and Excel from the current report selection.

## Priority guidance

Customer contract levels can influence RMA priority guidance, but BuildBook does not silently overwrite the selected priority.

Use the priority-mismatch report to find open RMAs where:

- The customer has an active contract level with a default RMA priority
- The selected RMA priority is blank or lower than that suggested priority

## Security notes

- Customer reports and exports do not include Build Record secrets
- Passwords, router credentials and BitLocker recovery keys are never exposed through customer screens
- Support ticket links use the configured template and the stored ticket identifier only

## Suggested acceptance path

1. Create or edit a customer with a contract level and end date.
2. Confirm the customer appears correctly in the Customers list.
3. Open a linked RMA and check the customer, contract and `Support Ticket No.` details.
4. Open Customer Reports and review contract, workload and data-quality results.
5. Export a selected report to CSV or Excel and confirm no secret fields are present.
