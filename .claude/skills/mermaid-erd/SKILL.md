---
name: mermaid-erd
description: Produces Mermaid entity-relationship diagrams for data warehouse tables and embeds the rendered SVG in event catalog entries. Use when creating or updating table dependency diagrams, ERDs for catalog entries, or embedding diagram SVGs in docs/events/ under "Table Dependencies".
---

# Mermaid ERD

Produce an Entity-Relationship diagram for a **subset of tables** (seed list), render it to SVG, and embed the image in a catalog entry. Follow this workflow and use **exactly** the tools specified.

## Inputs

- **Seed tables**: Table names to start from (e.g. from a catalog entry's `### INSERT/UPDATE \`TableName\``).
- **Diagram name**: Matches the catalog entry filename (e.g. `VenueEvent-VenueCreated`).
- **Catalog file**: Doc that will embed the SVG (e.g. `docs/events/VenueEvent-VenueCreated.md`).

## File Locations

| Item | Path |
|------|------|
| Mermaid source | `docs/mermaid/{Name}.mmd` |
| SVG output | `docs/attachments/{Name}.svg` |
| Catalog entry | `docs/events/{Name}.md` |
| SVG ref (from catalog) | `../attachments/{Name}.svg` |

## Workflow

1. **Get schema and FKs**
   Query the data warehouse via MCP (`user-local-datawarehouse`) or `docker exec`:
   ```bash
   docker exec datawarehouse pg_dump -U root -d datawarehouse --schema-only --no-owner --no-privileges
   ```
   Query `information_schema.columns` for columns and `pg_constraint` + `pg_attribute` for FKs (child_table, child_column, parent_table, parent_column) in the `public` schema.

2. **Compute transitive closure**
   From the seed tables, add every **parent** table (referenced by a FK). Repeat until no new parents. Restrict to `public` schema and tables present in the schema.

3. **Topological order**
   Sort the closure so parents come before children (roots first).

4. **Write Mermaid**
   - **Path**: `docs/mermaid/{Name}.mmd`
   - **Format**: Plain Mermaid `erDiagram` file (no markdown fencing). One entity per table (columns with PK/FK suffixes). One relationship per FK: `ParentTable ||--o{ ChildTable : "FKColumnName"`.
   - **Attribute types**: Use only valid erDiagram types: `int`, `string`, `boolean`, `timestamp`, `uuid`, `text`. See **Valid erDiagram types** below.
   - Match style of `docs/mermaid/VenueEvent-VenueCreated.mmd`.

5. **Render to SVG**
   Use **exactly** the `mmdc` command-line tool. Do **not** use `npx` or any other wrapper — invoke `mmdc` directly. Run from repository root with a **60-second timeout**:
   ```bash
   mmdc --puppeteerConfigFile .claude/skills/mermaid-erd/puppeteer-config.json \
        -i docs/mermaid/{Name}.mmd -o docs/attachments/{Name}.svg
   ```
   The committed `puppeteer-config.json` only adds `--no-sandbox` flags (safe on all platforms). `mmdc` uses its bundled Chromium by default — no extra setup needed on macOS or Windows. On Linux machines where the bundled binary is the wrong architecture, set `PUPPETEER_EXECUTABLE_PATH` to your system or Playwright Chromium path.

6. **Embed in catalog entry**
   Insert the image at the **top of the `## Table Dependencies` section** (before the first `### INSERT` / `### UPDATE`):
   ```markdown
   ## Table Dependencies

   ![{Name} table dependencies](../attachments/{Name}.svg)

   ### INSERT `TableName`
   ```

## Scripts (deterministic, repeatable)

Run from **repository root**:

1. **Generate Mermaid** (requires psycopg2; DB: localhost:5432, user root, password ChangeMe1, db datawarehouse, or set `DATAWAREHOUSE_CONNECTION_STRING`):
   ```bash
   python3 .claude/skills/mermaid-erd/scripts/generate_erd_mermaid.py NAME TABLE1 [TABLE2 ...]
   ```
   Writes `docs/mermaid/{Name}.mmd` from the transitive closure of seed tables under FKs.

2. **Render SVG** — use **exactly** `mmdc` (no npx); use a **60-second timeout**:
   ```bash
   mmdc --puppeteerConfigFile .claude/skills/mermaid-erd/puppeteer-config.json \
        -i docs/mermaid/{Name}.mmd -o docs/attachments/{Name}.svg
   ```

3. **Embed in catalog entry**:
   ```bash
   python3 .claude/skills/mermaid-erd/scripts/embed_erd_svg.py docs/events/FILE.md NAME
   ```
   Inserts `![...](../attachments/{Name}.svg)` after `## Table Dependencies` if not already present.

**Example** (VenueEvent-VenueCreated):
```bash
python3 .claude/skills/mermaid-erd/scripts/generate_erd_mermaid.py VenueEvent-VenueCreated Venue
mmdc --puppeteerConfigFile .claude/skills/mermaid-erd/puppeteer-config.json \
     -i docs/mermaid/VenueEvent-VenueCreated.mmd -o docs/attachments/VenueEvent-VenueCreated.svg
python3 .claude/skills/mermaid-erd/scripts/embed_erd_svg.py docs/events/VenueEvent-VenueCreated.md VenueEvent-VenueCreated
```

## Valid erDiagram types

| Type | Use for |
|------|---------|
| `int` | Integer columns (PK, FK, counts). |
| `string` | Short text (varchar, char, names, codes). |
| `boolean` | Boolean columns. |
| `timestamp` | Date/time columns. |
| `uuid` | GUID/UUID columns. |
| `text` | Long text. |

Avoid types Mermaid may not recognize (e.g. `numeric`, `decimal`); use `string` if necessary.

## Full procedure

For schema export options, FK query SQL, closure algorithm, and checklist, see [references/procedure.md](references/procedure.md).
