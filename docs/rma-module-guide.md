# RMA Module Guide

The BuildBook RMA module is the working area for booked-in returns, diagnosis, repair, test, return shipping, and closure.

## Daily use

Use the **RMA Register** to find active work quickly.

Use the **Board view** to review workload by status and spot blockers.

Use the **RMA detail page** as the main record for:

- intake and customer details
- structured fault details
- workflow ownership and dates
- checklist progress
- notes, attachments, and linked Build Records

## Register, board, and reports

The register is best for searching and opening records.

The board is best for checking where work is stuck.

Reports are best for:

- status-based handover checks
- repeat return reviews
- customer and product trend reviews
- turnaround and open-age monitoring

## Sensitive data handling

RMA pages can link to Build Records, but normal RMA views must not expose Build Record secrets.

Secrets remain masked by default and follow the existing BuildBook reveal permissions and audit rules.

## Planner migration traceability

When manually recreating an active Planner item in BuildBook, capture:

- migration source
- original Planner task title
- original Planner notes

This keeps the recreated record traceable without relying on Planner after cutover.
