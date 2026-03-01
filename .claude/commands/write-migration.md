---
description: Create an EF Core migration for the tables documented in a Festify event catalog entry.
argument-hint: <EventType-EventName or catalog file path>
---

# Create EF Core Migration from Event Catalog Entry

Create an EF Core migration for the data warehouse tables documented in a catalog entry. The catalog entry is the source of truth for table names, columns, alternate keys, and foreign key relationships.

## User Input

The user specifies **one** event catalog entry, either by:

- **Identifier**: e.g. `VenueEvent-VenueCreated` (resolves to `docs/events/VenueEvent-VenueCreated.md`)
- **Path**: e.g. `docs/events/VenueEvent-VenueCreated.md`

If the user does not specify, ask which catalog entry to use.

## Prerequisites

Read the event-catalog skill before proceeding:

- `.claude/skills/event-catalog/SKILL.md` – catalog structure, Table Dependencies sections, column types, alternate keys, foreign keys

## Step-by-Step Process

### Step 1: Load and Parse the Catalog Entry

1. Resolve the identifier to `docs/events/<EventType>-<EventName>.md` and read the file.
2. From each `### INSERT \`TableName\`` section, extract:
   - **Table name** (e.g. `Venue`)
   - **Table Columns** — name, data type, nullability
   - **Alternate Key** — column(s) forming the unique business key
   - **Foreign Key Relationships** — FK column, referenced table, referenced PK column
   - **Additional Column Mappings** — all non-PK, non-FK columns

### Step 2: Check for Existing Tables

For each table in the catalog entry, check whether it already exists in the data warehouse by querying the database:

```sql
SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'public'
  AND table_name = '[TableName]';
```

Or search the existing migrations:

```bash
grep -r "migrationBuilder.CreateTable" Festify.DataWarehouse/Migrations/ | grep '"[TableName]"'
```

Skip tables that already exist and report them. If a table exists but is missing columns documented in the catalog, create an `AddColumn` migration instead.

### Step 3: Create the Entity Model

For each new table, create or update the corresponding EF Core entity class in `Festify.DataWarehouse/Models/`.

**File**: `Festify.DataWarehouse/Models/<TableName>.cs`

**Naming conventions:**
- Class name matches the table name (e.g. `Venue`)
- Surrogate PK property: `<TableName>Id` (maps to `<TableName>Id` column; EF convention picks this up)
- Business key property: `<TableName>Guid` (type `Guid`)
- Foreign key properties: `<ForeignTable>Id` (type `int`)
- Data properties: PascalCase matching column names

**EF Core property mapping:**
- `int` identity PK → `[DatabaseGenerated(DatabaseGeneratedOption.Identity)]` or configured in `OnModelCreating`
- `uuid` → `Guid`
- `varchar(N)` → `string` with `HasMaxLength(N)`
- `text` → `string`
- `double precision` → `double`
- `timestamptz` → `DateTimeOffset`
- `boolean` → `bool`
- Nullable columns → nullable reference types (`string?`, `double?`, etc.)

**Example** (from `VenueEvent-VenueCreated`):

```csharp
namespace Festify.DataWarehouse.Models;

public class Venue
{
    public int VenueId { get; set; }
    public Guid VenueGuid { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int SeatingCapacity { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
```

### Step 4: Register the Entity in DbContext

Open `Festify.DataWarehouse/Data/DataWarehouseDbContext.cs` and:

1. Add a `DbSet<TableName>` property.
2. In `OnModelCreating`, configure:
   - Primary key (if not following EF convention)
   - Table name (e.g. `entity.ToTable("Venue")`)
   - Unique index for the alternate key: `HasIndex(e => e.VenueGuid).IsUnique()`
   - Index for each FK column: `HasIndex(e => e.ForeignId)` (PostgreSQL does not auto-index FKs)
   - String max lengths: `HasMaxLength(N)` on all `varchar` columns
   - Column type overrides where needed (e.g. `HasColumnType("jsonb")`)
   - FK relationships: `HasOne<ForeignTable>().WithMany().HasForeignKey(e => e.ForeignId)`

### Step 5: Generate the Migration

From the **repository root**, run:

```bash
dotnet ef migrations add Add<TableName>Table --project Festify.DataWarehouse --startup-project Festify.DataWarehouse
```

If adding multiple tables from the same catalog entry, add them all to DbContext first, then run one migration:

```bash
dotnet ef migrations add Add<EventName>Tables --project Festify.DataWarehouse --startup-project Festify.DataWarehouse
```

Verify the generated migration file in `Festify.DataWarehouse/Migrations/` includes:
- `CreateTable` with all columns, correct types, and nullability
- `CreateIndex` for the alternate key (unique)
- `CreateIndex` for each FK column (non-unique)
- `AddForeignKey` for each FK relationship
- Correct `Down()` reversing all operations

### Step 6: Build Verification

```bash
dotnet build Festify.DataWarehouse/Festify.DataWarehouse.csproj
```

Fix any compilation errors before proceeding.

### Step 7: Verification Checklist

- [ ] Entity model file created in `Festify.DataWarehouse/Models/` for each new table.
- [ ] `DbSet<TableName>` added to `DataWarehouseDbContext`.
- [ ] `OnModelCreating` configures table name, alternate key unique index, FK indexes, and string lengths.
- [ ] Migration generated with `dotnet ef migrations add`.
- [ ] Migration `Up()` creates each table with correct columns, types, and constraints.
- [ ] Migration `Down()` drops each table.
- [ ] Project builds without errors.

## Reference

- **Event catalog skill**: `.claude/skills/event-catalog/SKILL.md`
- **Catalog entry example**: `docs/events/VenueEvent-VenueCreated.md`
- **DbContext**: `Festify.DataWarehouse/Data/DataWarehouseDbContext.cs`
- **Existing migration example**: `Festify.DataWarehouse/Migrations/20260301064939_InitialCreate.cs`
- **Models folder**: `Festify.DataWarehouse/Models/`
