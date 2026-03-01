#!/usr/bin/env python3
"""
Insert or ensure the ERD SVG image reference below "## Table Dependencies" in a catalog entry.

If the heading exists, inserts a line like:
  ![Description](../attachments/NAME.svg)
immediately after it (and any following blank lines). If that exact image line
already exists in that section, leaves the file unchanged.

Usage:
  python .claude/skills/mermaid-erd/scripts/embed_erd_svg.py CATALOG_PATH DIAGRAM_NAME

  # Example
  python .claude/skills/mermaid-erd/scripts/embed_erd_svg.py docs/events/VenueEvent-VenueCreated.md VenueEvent-VenueCreated

  # Preview without writing
  python .claude/skills/mermaid-erd/scripts/embed_erd_svg.py docs/events/VenueEvent-VenueCreated.md VenueEvent-VenueCreated --dry-run
"""

import argparse
import re
from pathlib import Path


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


def embed_svg(catalog_path: Path, diagram_name: str, dry_run: bool = False) -> bool:
    """
    Insert SVG image reference after ## Table Dependencies if not already present.
    Returns True if file was modified (or would be in dry_run).
    """
    catalog_text = catalog_path.read_text(encoding="utf-8")
    image_line = f"![{diagram_name} table dependencies](../attachments/{diagram_name}.svg)"

    if image_line in catalog_text:
        print(f"No change (image already present): {catalog_path}")
        return False

    # Find ## Table Dependencies heading and position after it (and any following blank lines)
    pattern = re.compile(
        r"(^## Table Dependencies\s*\n)((?:\s*\n)*)",
        re.MULTILINE,
    )
    match = pattern.search(catalog_text)
    if not match:
        print(f"No '## Table Dependencies' heading found in {catalog_path}")
        return False

    insert_after = match.end()
    rest = catalog_text[insert_after:]

    # Check if an SVG image ref already exists in the Table Dependencies section
    next_section = rest.split("\n## ", 1)[0]
    if re.search(r"!\[.*?\]\s*\(\s*\.\./attachments/.*?\.svg\s*\)", next_section):
        print(f"No change (SVG image already present): {catalog_path}")
        return False

    new_text = catalog_text[:insert_after] + image_line + "\n\n" + catalog_text[insert_after:]
    if not dry_run:
        catalog_path.write_text(new_text, encoding="utf-8")
    return True


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Embed ERD SVG reference below ## Table Dependencies in a catalog entry."
    )
    parser.add_argument("catalog_path", type=Path, help="Path to the catalog .md file (e.g. docs/events/VenueEvent-VenueCreated.md)")
    parser.add_argument("diagram_name", help="Diagram name matching docs/attachments/{NAME}.svg (e.g. VenueEvent-VenueCreated)")
    parser.add_argument("--dry-run", action="store_true", help="Print what would be done without writing")
    args = parser.parse_args()

    script_dir = Path(__file__).resolve().parent
    repo_root = find_repo_root(script_dir)

    catalog_path = (repo_root / args.catalog_path) if not args.catalog_path.is_absolute() else args.catalog_path
    if not catalog_path.exists():
        raise SystemExit(f"Catalog file not found: {catalog_path}")

    svg_path = repo_root / "docs" / "attachments" / f"{args.diagram_name}.svg"
    if not svg_path.exists():
        raise SystemExit(f"SVG not found: {svg_path}\nRun mmdc first to render the diagram.")

    modified = embed_svg(catalog_path, args.diagram_name, dry_run=args.dry_run)
    if modified:
        if args.dry_run:
            print(f"Would insert image line after ## Table Dependencies in {catalog_path}")
        else:
            print(f"Inserted image line after ## Table Dependencies in {catalog_path}")


if __name__ == "__main__":
    main()
