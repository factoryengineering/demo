---
description: Create SQL scripts that load data warehouse tables from a Festify domain event, based on its catalog entry.
argument-hint: <EventType-EventName or catalog file path>
---

# Write SQL Script from Event Catalog Entry

Create one or more SQL scripts that populate data warehouse tables when a Festify domain event is received. The catalog entry is the source of truth for event structure, target tables, column mappings, alternate keys, and foreign key resolution.

## User Input

The user specifies **one** event catalog entry, either by:

- **Identifier**: e.g. `VenueEvent-VenueCreated` (resolves to `docs/events/VenueEvent-VenueCreated.md`)
- **Path**: e.g. `docs/events/VenueEvent-VenueCreated.md`

If the user does not specify, ask which catalog entry to use.

## Prerequisites

Read the event-catalog skill before proceeding:

- `.claude/skills/event-catalog/SKILL.md` – catalog structure, Table Dependencies sections, alternate keys, foreign keys, column mappings

## How Scripts Are Executed

`SqlScriptProcessingService` runs on a one-minute schedule. For each unprocessed `Event` row it:

1. Resolves the script directory: `Scripts/<EventType>/` (relative to `AppContext.BaseDirectory`)
2. Runs all `*.sql` files in that directory **in alphabetical order**
3. Marks the event as processed

**Implications:**
- Scripts are named `<nn>-<description>.sql` where `nn` is a zero-padded sequence number (e.g. `01-insert-venue.sql`)
- A lower number runs first — insert parent tables before child tables that FK to them
- Each script receives the full event row implicitly through the database connection; use a CTE or subquery to join to the `Event` table

## Step-by-Step Process

### Step 1: Load and Parse the Catalog Entry

1. Resolve the identifier to `docs/events/<EventType>-<EventName>.md` and read the file.
2. From each `### INSERT \`TableName\`` or `### UPDATE \`TableName\`` section, extract:
   - **Event Type** (from the entry header, e.g. `VenueEvent`)
   - **Event Name** (from the entry header, e.g. `VenueCreated`)
   - **Target table** (e.g. `Venue`)
   - **Alternate key** column(s) — used for deduplication
   - **Foreign key relationships** — FK column, foreign table, foreign alternate key column
   - **Additional column mappings** — all columns mapped from event `Payload` fields

3. **Script order**: Create scripts so that any table referenced by a FK is populated by a lower-numbered script. If another event already writes to a parent table, the parent row will exist before this event fires — no ordering concern for cross-event FKs.

### Step 2: Determine the Script Directory

`Festify.DataWarehouse/Scripts/<EventType>/`

Example for `VenueEvent`: `Festify.DataWarehouse/Scripts/VenueEvent/`

Create the directory if it does not exist. Inspect any existing scripts to determine the next sequence number.

### Step 3: Create Each Script File

For each table dependency, create one `.sql` file.

**File naming**: `<nn>-insert-<tablename-lowercase>.sql`
- Example: `01-insert-venue.sql`
- Use two-digit sequence numbers to preserve sort order beyond 9 scripts.

**Script structure:**

```sql
-- <EventType>/<EventName> → <TableName>
-- Inserts one row per event into <TableName>.

WITH event AS (
    SELECT
        e."EventId",
        e."Payload"->>'correlationId' AS "CorrelationId",  -- Attributes fields
        (e."Payload"->'body'->>'venueGuid')::uuid AS "VenueGuid",
        e."Payload"->'body'->>'name' AS "Name",
        -- ... all mapped columns
        (e."Payload"->>'occurredAt')::timestamptz AS "OccurredAt"
    FROM "Event" e
    WHERE e."EventType" = '<EventType>'
      AND NOT e."Processed"
      AND e."Payload"->'body' ? '<requiredField>'   -- attribute existence guard if needed
)
INSERT INTO "<TableName>" (
    "<AltKeyColumn>",
    "<DataColumn1>",
    -- ...
    "CreatedAt"
)
SELECT
    e."VenueGuid",
    e."Name",
    -- ...
    e."OccurredAt"
FROM event e
WHERE NOT EXISTS (
    SELECT 1
    FROM "<TableName>" t
    WHERE t."<AltKeyColumn>" = e."<AltKeyColumn>"
);
```

**Key rules:**

- **Event CTE**: Filter `"EventType" = '<EventType>'` and `NOT e."Processed"` to target only pending events of the correct type. Do not filter on `EventName` — the `EventType` column on `Event` carries the full type string (e.g. `VenueEvent`).
- **Payload JSON path**: The `Payload` column is `jsonb`. Use `->>` for text, `->` for objects, `::uuid` / `::int` / `::timestamptz` to cast. Body fields live under `Payload->'body'`; attributes under `Payload` directly (e.g. `Payload->>'occurredAt'`).
- **Deduplication**: Always guard with `WHERE NOT EXISTS (SELECT 1 FROM "<TableName>" t WHERE t."<AltKeyCol>" = e."<AltKeyVal>")` using the catalog's alternate key.
- **Foreign keys**: Join to the parent table on its alternate key to resolve the surrogate PK:
  ```sql
  JOIN "ParentTable" pt ON pt."ParentTableGuid" = (e."Payload"->'body'->>'parentGuid')::uuid
  ```
  Then insert `pt."ParentTableId"` into the FK column.
- **Single DML per file**: Each script file contains exactly one `INSERT` or `UPDATE` statement.
- **One row per event**: Standard case. For array-per-row (e.g. `Payload->'body'->'items'`), use `jsonb_array_elements`:
  ```sql
  FROM event e, jsonb_array_elements(e."Payload"->'body'->'items') AS item
  ```

### Step 4: Verify the Payload Shape

Before writing the script, confirm the actual `Payload` JSON structure by querying a recent event:

```sql
SELECT e."Payload"
FROM "Event" e
WHERE e."EventType" = '<EventType>'
ORDER BY e."EventId" DESC
LIMIT 1;
```

Use the MCP tool `user-local-datawarehouse` or:

```bash
docker exec datawarehouse psql -U root -d datawarehouse -c \
  "SELECT \"Payload\" FROM \"Event\" WHERE \"EventType\" = '<EventType>' ORDER BY \"EventId\" DESC LIMIT 1;"
```

If no events exist yet (prospective), use the payload shape from the catalog entry's Event Structure section.

### Step 5: Verification Checklist

- [ ] Script directory is `Festify.DataWarehouse/Scripts/<EventType>/`.
- [ ] One `.sql` file per table dependency, named `<nn>-<description>.sql` with a two-digit sequence.
- [ ] Scripts are ordered so parent tables (no FK dependency on other new scripts) have lower numbers.
- [ ] Each script has exactly one `INSERT` or `UPDATE`.
- [ ] Event CTE filters `"EventType" = '<EventType>'` and `NOT e."Processed"`.
- [ ] Payload JSON paths match the actual event structure (verified against real data or catalog entry).
- [ ] Alternate key deduplication (`NOT EXISTS`) uses the catalog's alternate key column(s).
- [ ] Foreign keys are resolved via `JOIN` on the parent table's alternate key column.
- [ ] `CreatedAt` is mapped from `Payload->>'occurredAt'` cast to `timestamptz`.
- [ ] Catalog entry status updated to **Completed** if all table dependencies are now implemented.

## Reference

- **Event catalog skill**: `.claude/skills/event-catalog/SKILL.md`
- **Catalog entry example**: `docs/events/VenueEvent-VenueCreated.md`
- **Script processor**: `Festify.DataWarehouse/Services/SqlScriptProcessingService.cs`
- **Scripts folder**: `Festify.DataWarehouse/Scripts/`
- **Event model**: `Festify.DataWarehouse/Models/Event.cs`
- **Local DB**: MCP `user-local-datawarehouse` or `docker exec datawarehouse psql -U root -d datawarehouse`
