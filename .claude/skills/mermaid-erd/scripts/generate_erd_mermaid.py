#!/usr/bin/env python3
"""
Generate a Mermaid erDiagram .mmd file for a subset of data warehouse tables.

Computes the transitive closure of seed tables under foreign-key (parent) relationships,
topological order, and writes docs/mermaid/{name}.mmd.

Requires: psycopg2 (pip install psycopg2-binary).

Usage:
  # From repo root; default DB localhost:5432, user root, password ChangeMe1, db datawarehouse
  python .claude/skills/mermaid-erd/scripts/generate_erd_mermaid.py NAME TABLE1 [TABLE2 ...]

  # With connection string
  export DATAWAREHOUSE_CONNECTION_STRING="host=localhost dbname=datawarehouse user=root password=ChangeMe1"
  python .claude/skills/mermaid-erd/scripts/generate_erd_mermaid.py NAME TABLE1 TABLE2

  # Example
  python .claude/skills/mermaid-erd/scripts/generate_erd_mermaid.py VenueEvent-VenueCreated Venue
"""

from pathlib import Path
import argparse
import os
import sys


def find_repo_root(start: Path) -> Path:
    current = start.resolve()
    for _ in range(10):
        if (current / "docs" / "mermaid").is_dir():
            return current
        if (current / "docs" / "events").is_dir():
            return current
        parent = current.parent
        if parent == current:
            break
        current = parent
    raise FileNotFoundError("Repo root not found (expected docs/mermaid or docs/events)")


def strip_quotes(s: str) -> str:
    s = s.strip()
    if len(s) >= 2 and s.startswith('"') and s.endswith('"'):
        return s[1:-1]
    return s


def get_connection():
    try:
        import psycopg2
    except ImportError:
        raise SystemExit("psycopg2 is required. Install with: pip install psycopg2-binary")
    conn_str = os.environ.get("DATAWAREHOUSE_CONNECTION_STRING")
    if conn_str:
        return psycopg2.connect(conn_str)
    return psycopg2.connect(
        host=os.environ.get("PGHOST", "localhost"),
        port=int(os.environ.get("PGPORT", "5432")),
        dbname=os.environ.get("PGDATABASE", "datawarehouse"),
        user=os.environ.get("PGUSER", "root"),
        password=os.environ.get("PGPASSWORD", "ChangeMe1"),
    )


COLUMNS_QUERY = """
SELECT table_name, column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_schema = 'public' AND table_name = ANY(%s)
ORDER BY table_name, ordinal_position
"""

FKS_QUERY = """
SELECT c.conrelid::regclass AS child_tbl, a.attname AS child_col,
       c.confrelid::regclass AS parent_tbl, af.attname AS parent_col
FROM pg_constraint c
JOIN pg_attribute a ON a.attnum = ANY(c.conkey) AND a.attrelid = c.conrelid
  AND a.attnum > 0 AND NOT a.attisdropped
JOIN pg_attribute af ON af.attnum = ANY(c.confkey) AND af.attrelid = c.confrelid
  AND af.attnum > 0 AND NOT af.attisdropped
WHERE c.contype = 'f'
  AND c.connamespace = (SELECT oid FROM pg_namespace WHERE nspname = 'public')
"""


def fetch_columns(conn, table_names: list[str]) -> dict[str, list[tuple[str, str, str]]]:
    """Return {table_name: [(column_name, data_type, is_nullable), ...]}."""
    if not table_names:
        return {}
    with conn.cursor() as cur:
        cur.execute(COLUMNS_QUERY, (list(table_names),))
        rows = cur.fetchall()
    by_table: dict[str, list[tuple[str, str, str]]] = {}
    for table_name, column_name, data_type, is_nullable in rows:
        table_name = strip_quotes(table_name) if isinstance(table_name, str) else table_name
        by_table.setdefault(table_name, []).append((column_name, data_type, is_nullable or "NO"))
    return by_table


def fetch_fks(conn) -> list[tuple[str, str, str, str]]:
    """Return [(child_table, child_column, parent_table, parent_column), ...]."""
    with conn.cursor() as cur:
        cur.execute(FKS_QUERY)
        rows = cur.fetchall()
    return [
        (strip_quotes(str(c_t)), c_col, strip_quotes(str(p_t)), p_col)
        for c_t, c_col, p_t, p_col in rows
    ]


def compute_closure(seed_tables: list[str], fks: list[tuple[str, str, str, str]]) -> set[str]:
    """Set of all tables in the transitive closure (seed + all referenced parents)."""
    child_to_parents: dict[str, set[str]] = {}
    for child_t, _c_col, parent_t, _p_col in fks:
        child_to_parents.setdefault(child_t, set()).add(parent_t)
    closure = set(seed_tables)
    changed = True
    while changed:
        changed = False
        for table in list(closure):
            for parent in child_to_parents.get(table, set()):
                if parent not in closure:
                    closure.add(parent)
                    changed = True
    return closure


def topological_order(tables: set[str], fks: list[tuple[str, str, str, str]]) -> list[str]:
    """Order tables so every parent appears before its children (roots first)."""
    parents_to_children: dict[str, set[str]] = {}
    for child_t, _c_col, parent_t, _p_col in fks:
        if child_t in tables and parent_t in tables:
            parents_to_children.setdefault(parent_t, set()).add(child_t)
    in_degree = {t: 0 for t in tables}
    for parent, children in parents_to_children.items():
        for c in children:
            in_degree[c] += 1
    queue = sorted(t for t in tables if in_degree[t] == 0)
    order = []
    while queue:
        t = queue.pop(0)
        order.append(t)
        for c in sorted(parents_to_children.get(t, set())):
            in_degree[c] -= 1
            if in_degree[c] == 0:
                queue.append(c)
    # Any remaining (cycle) append at end
    for t in sorted(tables):
        if t not in order:
            order.append(t)
    return order


def mermaid_type(data_type: str) -> str:
    if data_type in ("integer", "bigint", "smallint"):
        return "int"
    if data_type == "uuid":
        return "uuid"
    if data_type in ("character varying", "varchar", "text"):
        return "string"
    if data_type == "boolean":
        return "boolean"
    if "timestamp" in data_type or "time " in data_type:
        return "timestamp"
    return "string"


def primary_key_column(columns: list[tuple[str, str, str]]) -> str | None:
    """Heuristic: first NOT NULL column ending with Key or Id."""
    for col_name, _dt, nullable in columns:
        if (col_name.endswith("Key") or col_name.endswith("Id")) and nullable == "NO":
            return col_name
    return columns[0][0] if columns else None


def build_mermaid(
    name: str,
    seed_tables: list[str],
    order: list[str],
    columns_by_table: dict[str, list[tuple[str, str, str]]],
    fks: list[tuple[str, str, str, str]],
) -> str:
    table_set = set(order)
    lines = [
        f"%% {name}: seed tables and all referenced tables (recursively)",
        f"%% Seed: {', '.join(seed_tables)}",
        "",
        "erDiagram",
    ]
    for table in order:
        cols = columns_by_table.get(table, [])
        pk = primary_key_column(cols)
        block = []
        for col_name, data_type, nullable in cols:
            m_type = mermaid_type(data_type)
            suffix = " PK" if col_name == pk else ""
            is_fk = any(
                c_t == table and c_col == col_name and p_t in table_set
                for c_t, c_col, p_t, _ in fks
            )
            if is_fk and suffix != " PK":
                suffix = " FK"
            block.append(f"        {m_type} {col_name}{suffix}")
        lines.append(f"    {table} {{")
        lines.extend(block)
        lines.append("    }")
        lines.append("")
    for child_t, child_col, parent_t, _ in fks:
        if child_t in table_set and parent_t in table_set:
            lines.append(f'    {parent_t} ||--o{{ {child_t} : "{child_col}"')
    return "\n".join(lines) + "\n"


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Generate Mermaid erDiagram .md for a subset of DW tables (closure under FKs)."
    )
    parser.add_argument("name", help="Diagram name matching catalog entry (e.g. VenueEvent-VenueCreated)")
    parser.add_argument("tables", nargs="+", help="Seed table names (e.g. Venue)")
    args = parser.parse_args()

    script_dir = Path(__file__).resolve().parent
    repo_root = find_repo_root(script_dir)
    mermaid_dir = repo_root / "docs" / "mermaid"
    mermaid_dir.mkdir(parents=True, exist_ok=True)
    out_path = mermaid_dir / f"{args.name}.mmd"

    conn = get_connection()
    try:
        fks = fetch_fks(conn)
        closure = compute_closure(args.tables, fks)
        order = topological_order(closure, fks)
        columns_by_table = fetch_columns(conn, list(closure))
        content = build_mermaid(args.name, args.tables, order, columns_by_table, fks)
        out_path.write_text(content, encoding="utf-8")
        print(f"Wrote {len(closure)} table(s) to {out_path}")
    finally:
        conn.close()


if __name__ == "__main__":
    main()
