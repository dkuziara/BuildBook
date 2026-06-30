# Customer Module UAT Checklist

## Customers list and detail

- Open `Customers` and confirm the list loads.
- Search by customer name, contact name and email.
- Filter by support contract level, support contract status and active state.
- Open a customer detail page and confirm address, contacts, linked Build Records and linked RMAs display correctly.

## Customer maintenance

- Create a new customer with no contract level.
- Edit an existing customer to change contract level, contract status and contract end date.
- Confirm duplicate customer names are blocked.

## Shared customer selection

- Open a Build Record and confirm the customer field uses the shared customer list.
- Open an RMA and confirm the customer field uses the same shared customer list.

## Support contract levels and settings

- Open `Admin > Support Contract Levels` and add or edit a level.
- Open `Admin > System Settings` and save a valid `Support Site URL Template`.
- Confirm the template requires `{1}` and blocks unsafe schemes.

## Support Ticket No.

- Add or update `Support Ticket No.` on an RMA.
- Confirm `Open Support Ticket` appears when a valid template is configured.
- Confirm the ticket number shows without a link when the template is blank.

## Priority guidance

- Link an open RMA to a customer with an active contract level that has a default RMA priority.
- Confirm BuildBook does not silently overwrite the selected priority.
- Confirm the customer reports can identify a priority mismatch when the selected priority is lower than suggested.

## Reports and exports

- Open `Customers > Reports`.
- Review customers by contract level.
- Review customers with no contract.
- Review expired contracts and contracts expiring in 30, 60 and 90 days.
- Review open RMAs by contract level and overdue RMAs by contract level.
- Review RMAs with no `Support Ticket No.`.
- Export a selected customer report to CSV.
- Export a selected customer report to Excel.

## Security checks

- Confirm customer screens do not expose Build Record secrets.
- Confirm customer exports do not include passwords, router credentials or BitLocker recovery keys.
- Confirm reports remain operational and compact rather than acting like a full CRM workflow.
