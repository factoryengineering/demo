# ERD Procedure (full reference)

## Contents

- Inputs
- Steps (overview)
- 1. Export schema
- 2. Transitive closure under FKs
- 3. Mermaid diagram
- 4. Render Mermaid to SVG
- 5. Embed SVG in catalog entry
- Tool summary
- Scripts
- Checklist

---

Procedure to produce an Entity-Relationship diagram for a **subset of tables** (seed list), render it to SVG, and embed it in a catalog entry using the live PostgreSQL schema and deterministic tooling.

## Inputs

- **Seed tables**: List of table names (e.g. the table(s) in a catalog entry's `### INSERT/UPDATE \`TableName\``).
- **Diagram name**: Matches the event catalog filename without extension (e.g. `VenueEvent-VenueCreated`).
- **Catalog file**: Path to the catalog entry that will embed the SVG (e.g. `docs/events/VenueEvent-VenueCreated.md`).

## Steps (overview)

1. Export the current PostgreSQL schema (tables, columns, foreign keys).
2. Compute the **transitive closure** of the seed tables under **foreign-key "referenced"** (parent) relationships.
3. Produce a **Mermaid** `erDiagram` in `docs/mermaid/{Name}.mmd`.
4. Render that Mermaid to **SVG** in `docs/attachments/{Name}.svg` using `mmdc`.
5. **Embed** the SVG in the catalog entry (top of `## Table Dependencies`).

## 1. Export schema

**Option A — docker exec pg_dump** (from host):
```bash
docker exec datawarehouse pg_dump -U root -d datawarehouse --schema-only --no-owner --no-privileges
```

**Option B — query information_schema** (used by the generator script):
```sql
-- Columns
SELECT table_name, column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_schema = 'public' AND table_name = ANY('{Venue,OtherTable}')
ORDER BY table_name, ordinal_position;

-- Foreign keys
SELECT c.conrelid::regclass AS child_tbl, a.attname AS child_col,
       c.confrelid::regclass AS parent_tbl, af.attname AS parent_col
FROM pg_constraint c
JOIN pg_attribute a ON a.attnum = ANY(c.conkey) AND a.attrelid = c.conrelid
  AND a.attnum > 0 AND NOT a.attisdropped
JOIN pg_attribute af ON af.attnum = ANY(c.confkey) AND af.attrelid = c.confrelid
  AND af.attnum > 0 AND NOT af.attisdropped
WHERE c.contype = 'f'
  AND c.connamespace = (SELECT oid FROM pg_namespace WHERE nspname = 'public');
```

## 2. Transitive closure under FKs

- **Direction**: "Referenced" only. From each table, add every **parent** table (table referenced by a foreign key). Repeat until no new parents.
- **Algorithm**: Start with seed set S. For each table in S, add all parent tables. Repeat until stable. Result = closure (all referenced ancestors).
- **Scope**: Restrict to `public` schema.

## 3. Mermaid diagram

- **Path**: `docs/mermaid/{Name}.mmd`
- **Content**: Plain Mermaid `erDiagram` file — no markdown fencing, no front matter.
- **Order**: Entities in **topological order** (parents before children; roots first).
- **Entities**: One per table in the closure. Columns with PK/FK suffixes.
- **Relationships**: One per FK — `ParentTable ||--o{ ChildTable : "FKColumnName"`.
- **Comments**: At top of the file, state seed table(s) and that the diagram shows "seed tables and all referenced tables (recursively)".
- **Format**: Match `docs/mermaid/VenueEvent-VenueCreated.mmd`.

**Determinism**: Same schema + same seed list + same FK queries → same closure and same topological order → same output.

## 4. Render Mermaid to SVG

- **Tool**: Use **exactly** the `mmdc` command-line tool. Do **not** use `npx`, npm scripts, or any other wrapper — invoke `mmdc` directly.
- **Command** (run from repository root):
  ```bash
  mmdc --puppeteerConfigFile .claude/skills/mermaid-erd/puppeteer-config.json \
       -i docs/mermaid/{Name}.mmd -o docs/attachments/{Name}.svg
  ```
- **Config**: The committed `puppeteer-config.json` only sets `--no-sandbox` / `--disable-setuid-sandbox` args — these are harmless on all platforms and required in Linux containers. `mmdc` selects its browser as follows:
  1. `PUPPETEER_EXECUTABLE_PATH` env var (if set) — use this on Linux if the bundled Chrome is the wrong architecture.
  2. Puppeteer's own bundled Chromium download — works out of the box on macOS and Windows.
  - **Linux ARM64 setup**: `export PUPPETEER_EXECUTABLE_PATH=<path-to-arm64-chrome>` (e.g. from `npx playwright install chromium`).
- **Timeout**: `mmdc` can take several seconds. In automated environments, use a **60-second timeout** so the process completes and the SVG is written.

## 5. Embed SVG in catalog entry

- **Placement**: At the **top of the `## Table Dependencies` section** in the catalog entry (before the first `### INSERT` / `### UPDATE`).
- **Syntax**:
  ```markdown
  ## Table Dependencies

  ![{Name} table dependencies](../attachments/{Name}.svg)

  ### INSERT `TableName`
  ```
- **Implementation**: Use `embed_erd_svg.py` or insert manually.

## Tool summary

| Step | Tool |
|------|------|
| Closure + Mermaid | `scripts/generate_erd_mermaid.py` (DB → closure → `docs/mermaid/{Name}.mmd`) |
| Mermaid → SVG | **mmdc** only (no npx); 60-second timeout |
| Embed in catalog | `scripts/embed_erd_svg.py` or manual |
| Export schema | Optional: `docker exec datawarehouse pg_dump ... --schema-only` or DB queries (script uses DB directly) |

## Scripts

### generate_erd_mermaid.py

- **Inputs**: Diagram name, seed table names (positional). DB via `DATAWAREHOUSE_CONNECTION_STRING` or defaults (localhost:5432, user root, password ChangeMe1, db datawarehouse).
- **Requires**: `pip install psycopg2-binary`
- **Logic**: Connects to PostgreSQL, queries `information_schema.columns` and `pg_constraint`+`pg_attribute` for FKs; computes closure (parents only), topological sort; writes `docs/mermaid/{Name}.mmd`.
- **Run**: `python3 .claude/skills/mermaid-erd/scripts/generate_erd_mermaid.py NAME TABLE1 [TABLE2 ...]`

### embed_erd_svg.py

- **Inputs**: Catalog path (e.g. `docs/events/VenueEvent-VenueCreated.md`), diagram name.
- **Action**: Inserts `![...](../attachments/{Name}.svg)` after `## Table Dependencies` if not already present. Requires the SVG to exist. Use `--dry-run` to preview.
- **Run**: `python3 .claude/skills/mermaid-erd/scripts/embed_erd_svg.py CATALOG_PATH NAME`

## Checklist

- [ ] Run `python3 .claude/skills/mermaid-erd/scripts/generate_erd_mermaid.py NAME TABLE1 [TABLE2 ...]`
- [ ] Verify `docs/mermaid/{Name}.mmd` looks correct (check FK relationships and column types)
- [ ] Render: use **exactly** `mmdc` (no npx); run `mmdc --puppeteerConfigFile .claude/skills/mermaid-erd/puppeteer-config.json -i docs/mermaid/{Name}.mmd -o docs/attachments/{Name}.svg` with **60-second timeout**
- [ ] Run `python3 .claude/skills/mermaid-erd/scripts/embed_erd_svg.py docs/events/{Name}.md NAME` (or insert image line manually at top of `## Table Dependencies`)
- [ ] Commit `docs/mermaid/{Name}.mmd`, `docs/attachments/{Name}.svg`, and the updated catalog entry
