# RMA Planner Migration Guide

BuildBook replaces the previous Microsoft Planner workflow for RMAs.

The initial migration approach is manual recreation of active work rather than automated import.

## Recommended process

1. Review the active Planner card.
2. Create a matching RMA in BuildBook.
3. Copy the current customer, product, serial, fault summary, and fault description into the new record.
4. Record the **migration source**, **original Planner task title**, and **original Planner notes** in the intake section.
5. Recreate any still-relevant checklist items.
6. Link the Build Record when a matching unit already exists in BuildBook.
7. Continue all future work in BuildBook only.

## What to preserve

Preserve the context needed to understand the old task:

- original Planner task title
- original notes
- current assignee if known
- current stage or blocker

## Cutover note

After migration, Planner should be treated as historical reference only and BuildBook should become the authoritative working record.
