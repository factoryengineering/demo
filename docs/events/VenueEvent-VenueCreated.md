# VenueEvent - VenueCreated

- **Status** – **Prospective** (planned; event not yet emitted)
- **Event Type** - `VenueEvent`
- **Event Name** - `VenueCreated`

Published by the Festify API when a new venue is successfully created via `POST /api/venues`. The payload reflects the full venue record as returned in the 201 Created response.

## User Story

This event is referenced by the following user story:

- **[US001](../user-stories/US001-Venue-Management.md)** – Venue Management

## Event Structure

### Body

Example body:

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

### Attributes

Example attributes:

```json
{
  "correlationId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "occurredAt": "2026-03-01T14:22:05.123Z",
  "source": "Festify.Api"
}
```

**Related event catalog entries:**

(none yet)

**Correlated events:**

(none found)

## Table Dependencies

### INSERT `Venue`

One row per event.

#### Table Columns

| Column Name       | Data Type         | Nullable | Description                                                  |
| ----------------- | ----------------- | -------- | ------------------------------------------------------------ |
| `VenueKey`        | int               | NOT NULL | Surrogate primary key (identity); not exposed in events      |
| `VenueGuid`       | uuid              | NOT NULL | Business key; stable public identifier; alternate key        |
| `Name`            | varchar(200)      | NOT NULL | Venue name                                                   |
| `Address`         | varchar(500)      | NOT NULL | Full street address                                          |
| `Latitude`        | double precision  | NOT NULL | WGS 84 latitude (−90 to +90)                                 |
| `Longitude`       | double precision  | NOT NULL | WGS 84 longitude (−180 to +180)                              |
| `SeatingCapacity` | int               | NOT NULL | Maximum audience size; positive integer                      |
| `Description`     | varchar(2000)     | NULL     | Optional free-text description of the venue                  |
| `CreatedAt`       | timestamptz       | NOT NULL | When the venue was ingested into the data warehouse          |

#### Alternate Key

| Alternate key column | Event property   |
| -------------------- | ---------------- |
| `VenueGuid`          | `Body.venueGuid` |

#### Additional Column Mappings

| Target Column     | Event property          |
| ----------------- | ----------------------- |
| `Name`            | `Body.name`             |
| `Address`         | `Body.address`          |
| `Latitude`        | `Body.latitude`         |
| `Longitude`       | `Body.longitude`        |
| `SeatingCapacity` | `Body.seatingCapacity`  |
| `Description`     | `Body.description`      |
| `CreatedAt`       | `Attributes.occurredAt` |
