# Example: VenueCreated

The canonical catalog entry for this project is `docs/events/VenueEvent-VenueCreated.md`. Read that file first, then use the annotations below to understand the decisions made.

---

## Annotated Decisions

### Status: Prospective

The event is documented ahead of implementation. It will advance to **Published** once `POST /api/venues` emits the event, and to **Completed** once the data warehouse load agent processes it.

### Body shape matches the API response, not the request

The Body mirrors `VenueResponse` (the 201 response body), not `CreateVenueRequest`. This is deliberate — the response includes the server-assigned `venueGuid`, which is the field the data warehouse uses as its business key. The request body omits it.

### Attributes envelope

```json
{
  "correlationId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "occurredAt": "2026-03-01T14:22:05.123Z",
  "source": "Festify.Api"
}
```

`occurredAt` is ISO 8601 with a `Z` suffix (UTC). This maps to `CreatedAt` in the DW table.

### VenueKey vs VenueGuid

| Column | Role | Populated from |
|---|---|---|
| `VenueKey` | Surrogate PK (identity int) | Database auto-increment; not in event |
| `VenueGuid` | Business key; alternate key | `Body.venueGuid` |

Child tables (e.g. a future `Show` table) resolve the `VenueKey` FK by looking up `VenueGuid` in the `Venue` table — never by storing the int directly from the event.

### double precision for coordinates

Latitude and Longitude use `double precision`, matching the Festify API's `float` (64-bit). The spec notes these will migrate to a PostGIS `geography` point column when geospatial search is implemented; the DW table will need the same migration at that time.

### Description is NULL

`Description` is `nullable: true` in the API schema (`VenueResponse`). The DW column is therefore `NULL`. All other Venue fields are required and map to `NOT NULL` columns.

### Body example uses real data

```json
{
  "venueGuid": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "name": "Metro Chicago",
  "address": "3730 N Clark St, Chicago, IL 60613",
  "latitude": 41.9497,
  "longitude": -87.6631,
  "seatingCapacity": 1100,
  "description": "Historic Chicago music venue since 1982."
}
```

Real venue name, real coordinates, real address. Placeholders like `"string"` or `0` are not acceptable — they obscure whether the field mapping makes sense.

---

## What a Future Child Entry Looks Like

When `ShowCreated` is documented, its `Venue` FK section will reference this entry:

```markdown
#### Foreign Key Relationships

**`VenueKey` → `Venue`**

| Foreign alternate key column | Event property   |
| ---------------------------- | ---------------- |
| `VenueGuid`                  | `Body.venueGuid` |
```

And the **Related event catalog entries** in both files will link to each other.
