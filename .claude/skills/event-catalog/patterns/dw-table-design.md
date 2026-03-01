# Data Warehouse Table Design Patterns

Rules for designing the target tables documented in Festify event catalog entries.

## Primary Keys

Always an identity `int` column. Name it `[EntityName]Key` (e.g. `VenueKey`, `ShowKey`). Never expose it through the event or the API. Do not document it in the column mappings — it is infrastructure, not a mapping.

```sql
"VenueKey" int GENERATED ALWAYS AS IDENTITY PRIMARY KEY
```

## Business Keys (Alternate Keys)

Every DW table has a `uuid` business key, named `[EntityName]Guid` (e.g. `VenueGuid`). It:

- Maps directly from the event Body (`Body.venueGuid`)
- Carries a `UNIQUE` constraint
- Is used for deduplication (NOT EXISTS checks) and foreign key resolution from child tables

Document it in both the **Alternate Key** table and the **Table Columns** table.

## Foreign Keys

Foreign keys **always reference the surrogate PK of the parent table**, resolved via the parent's alternate key (GUID). The resolution path is:

```
Event property (GUID) → parent.AlternateKeyColumn → parent.SurrogateKey → this.ForeignKeyColumn
```

Name FK columns `[ParentEntity]Key` (e.g. `VenueKey` in a `Show` table). Document the resolution path in a `#### Foreign Key Relationships` section:

```markdown
**`ShowKey` → `Show`**

| Foreign alternate key column | Event property   |
| ---------------------------- | ---------------- |
| `ShowGuid`                   | `Body.showGuid`  |
```

## Data Types

| Source type | DW column type | Notes |
|---|---|---|
| `Guid` / `uuid` | `uuid` | Business keys, FK resolution GUIDs |
| `int` (surrogate) | `int` | Surrogate keys only |
| `string` (bounded) | `varchar(n)` | Use the same length limit as the API schema |
| `double` / `float` (lat/long) | `double precision` | WGS 84 coordinates |
| `DateTimeOffset` | `timestamptz` | All timestamps include time zone |
| `bool` | `boolean` | |
| JSON | `jsonb` | For unstructured payload columns |

## Audit Columns

Include `CreatedAt timestamptz NOT NULL` on every table. Map it from `Attributes.occurredAt`, which records when the event was emitted — this gives a stable "first seen" timestamp independent of operational clock skew.

Do **not** include `UpdatedAt` or soft-delete columns unless the event type explicitly models updates or deletions.

## Nullable Columns

Mark columns `NULL` only when the source field is genuinely optional in the API schema. When in doubt, check the OpenAPI spec or the entity definition — if the field has `nullable: true`, the column is `NULL`.

## Naming Conventions

| Object | Convention | Example |
|---|---|---|
| Table | Singular, PascalCase | `Venue`, `Show`, `TicketSale` |
| Surrogate PK | `[Entity]Key` | `VenueKey` |
| Business key | `[Entity]Guid` | `VenueGuid` |
| FK column | `[ParentEntity]Key` | `VenueKey` in `Show` |
| Audit timestamp | `CreatedAt` | — |

## Column Order

List columns in this order in the Table Columns table:

1. Surrogate PK (`[Entity]Key`)
2. Business key (`[Entity]Guid`)
3. Foreign keys (`[Parent]Key`) — in dependency order
4. Payload columns (alphabetical within groups)
5. Audit columns (`CreatedAt` last)

## Anti-Patterns

### ❌ Integer primary key as business key

```markdown
-- Wrong: using int PK as the lookup key
| `VenueId` | int | NOT NULL | Business identifier |
```

GUIDs are used as business keys. Integer PKs are internal infrastructure.

### ❌ Nullable NOT NULL columns

Only mark columns nullable when the source genuinely allows null. Do not add `NULL` to columns that are required in the API schema.

### ❌ Timestamp without time zone

Use `timestamptz` (timestamp with time zone) for all timestamps. The Festify API surfaces `DateTimeOffset` values, which carry UTC offsets.

### ❌ varchar without length limit

Always specify the length. Match the API schema's `maxLength` constraint. If the API has no explicit limit, use `text`.
