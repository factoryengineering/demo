---
name: event-catalog
description: Creates and maintains event catalog entries in docs/events/ that document how domain events map to data warehouse tables. Use when writing a new catalog entry, reviewing an existing one, or understanding how an event is processed.
---

# Event Catalog

Documents domain events published by Festify services and how they populate the data warehouse.

## Catalog Entry Anatomy

| Section | Purpose |
|---|---|
| **Header** | Event type, name, status |
| **User Story** | Traceability link to docs/user-stories/ |
| **Event Structure** | Body and Attributes examples (realistic JSON) |
| **Table Dependencies** | Target DW table(s), columns, alternate key, column mappings |

## Status Values

| Status | When to use |
|---|---|
| **Prospective** | Event is planned; not yet emitted or present in the data warehouse |
| **Published** | Event is emitted by the API but not yet consumed by the load agent |
| **Completed** | Implemented end-to-end in the data warehouse load agent |

## File Location and Naming

- **Path**: `docs/events/<EventType>-<EventName>.md` (e.g. `docs/events/VenueEvent-VenueCreated.md`)
- **Event Type**: domain area (e.g. `VenueEvent`)
- **Event Name**: action in past tense (e.g. `VenueCreated`)

## When to Load Which Pattern

- **Starting a new entry** → Copy the template from [patterns/catalog-entry-template.md](patterns/catalog-entry-template.md)
- **Designing the DW target table** → [patterns/dw-table-design.md](patterns/dw-table-design.md)
- **Seeing a complete worked example** → [examples/VenueEvent-VenueCreated.md](examples/VenueEvent-VenueCreated.md)

## Checklist

1. Status is one of: Prospective / Published / Completed
2. Body example uses realistic domain data, not placeholders
3. Attributes include `correlationId`, `occurredAt`, `source`
4. Every NOT NULL column is marked in the Table Columns table
5. Alternate key column(s) match the business key exposed in the event
6. All Body field paths use dot notation (`Body.venueGuid`)
7. `CreatedAt` maps to `Attributes.occurredAt`
8. Surrogate PK (`*Key` identity column) is present but not mapped from the event
9. Related entries and correlated events sections are present (even if "(none yet)")

## Resources

| File | When to load |
|---|---|
| [patterns/catalog-entry-template.md](patterns/catalog-entry-template.md) | Starting a new catalog entry |
| [patterns/dw-table-design.md](patterns/dw-table-design.md) | Designing the target data warehouse table |
| [examples/VenueEvent-VenueCreated.md](examples/VenueEvent-VenueCreated.md) | Seeing a complete worked example |
