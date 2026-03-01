# Catalog Entry Template

Copy this template verbatim for every new catalog entry. Replace all `[placeholders]`.

---

```markdown
# [EventType] - [EventName]

- **Status** – **[Prospective | Published | Completed]** ([one-line status description])
- **Event Type** - `[EventType]`
- **Event Name** - `[EventName]`

[One or two sentences: when is this event published, and by which service/endpoint?]

## User Story

This event is referenced by the following user story:

- **[[US###]](../user-stories/[US###]-[slug].md)** – [Story title]

## Event Structure

### Body

Example body:

```json
{
  [Complete, realistic JSON matching the API response shape for this event]
}
```

[Optional: note any important difference from the API request vs response body.]

### Attributes

Example attributes:

```json
{
  "correlationId": "[uuid]",
  "occurredAt": "[ISO 8601 timestamp]",
  "source": "Festify.Api"
}
```

**Related event catalog entries:**

[Bulleted links to entries that write to the same table, a parent table, or a child table.
Use "(none yet)" if no related entries exist.]

**Correlated events:**

[Bulleted list of event types that share a correlationId with this event.
Use "(none found)" until the data warehouse is populated and queried.]

## Table Dependencies

### INSERT `[TableName]`

[Row count description, e.g. "One row per event."]

#### Table Columns

| Column Name       | Data Type    | Nullable | Description                          |
| ----------------- | ------------ | -------- | ------------------------------------ |
| `[SurrogateKey]`  | int          | NOT NULL | Surrogate primary key (identity)     |
| `[BusinessKey]`   | uuid         | NOT NULL | Business key; alternate key          |
| `[OtherColumn]`   | [DataType]   | [NULL/NOT NULL] | [Description]               |

#### Alternate Key

| Alternate key column | Event property          |
| -------------------- | ----------------------- |
| `[ColumnName]`       | `Body.[fieldName]`      |

#### Additional Column Mappings

| Target Column    | Event property             |
| ---------------- | -------------------------- |
| `[ColumnName]`   | `Body.[fieldName]`         |
| `CreatedAt`      | `Attributes.occurredAt`    |
```

---

## Rules

### Surrogate key

Every DW table has an identity `int` surrogate primary key named `[EntityName]Key` (e.g. `VenueKey`). It is **never** populated from the event; omit it from the column mappings.

### Business key

Every DW table has a `uuid` business key named `[EntityName]Guid` (e.g. `VenueGuid`). It is the alternate key — used for deduplication and foreign key lookups. Map it from `Body.[entityGuid]`.

### Foreign keys

When a table references another DW table, document the resolution path: event GUID property → foreign table's alternate key column → foreign table's surrogate PK → this table's FK column.

Use this format under a separate `#### Foreign Key Relationships` heading:

```markdown
#### Foreign Key Relationships

**`[FkColumn]` → `[ForeignTable]`**

| Foreign alternate key column | Event property       |
| ---------------------------- | -------------------- |
| `[ForeignGuidColumn]`        | `Body.[fieldName]`   |
```

### Nullability

Mark every NOT NULL column clearly. Nullable columns must also be shown; use `NULL` in the Nullable column.

### Body example realism

Use realistic domain values in the Body example — real venue names, addresses, coordinates, etc. Do not use placeholder strings like `"string"` or `0` for numeric fields.

### Status progression

Start new entries at **Prospective**. Advance to **Published** once the API emits the event (even if the load agent doesn't consume it). Advance to **Completed** once the load agent processes it end-to-end.
