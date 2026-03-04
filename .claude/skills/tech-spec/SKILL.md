---
name: tech-spec
description: Defines the structure and format of technical specifications in this project. Use when writing a new spec in docs/specs/, reviewing or refactoring an existing spec, or when the user asks how specs are structured or what format to follow.
---

# Technical Specification (Tech Spec)

Technical specifications in this project define API contracts, database schema, and behaviour for a feature or user story. They live in `docs/specs/` and are the single source of truth for implementation.

---

## Spec anatomy

Every spec follows this structure. Adapt section depth and content to scope (e.g. a list-only spec may have a smaller API section than a full CRUD spec).

### Header

- **Title**: `# SPECnnn — [Short Name]` (e.g. `# SPEC001 — List Venues`). Use the next available SPEC number; numbers align with user stories where applicable (e.g. SPEC001 ↔ US001).
- **Metadata block** (immediately under the title):
  - `**User Story**:` Link to the user story in `docs/user-stories/` using relative path `../user-stories/USnnn-Name.md`.
  - `**Status**:` One of `Draft` | `Approved` | `Implemented` (or project convention).
- A horizontal rule `---` separates the header from the body.

### 1. Overview

- One or two short paragraphs.
- State what the spec defines (e.g. “API contract and behaviour for listing all venues”).
- Introduce key concepts, identifiers (e.g. `venueGuid` vs internal `venueId`), and scope (routes, consumers). No implementation detail here—focus on contract and intent.

### 2. Database Schema

- **2.1 ERD**: A Mermaid `erDiagram` block showing the table(s) and columns relevant to this spec. Use consistent types (e.g. `int`, `uuid`, `nvarchar`) and annotate PK and “public identifier” where it matters.
- **2.2 [Table name] Table**: A markdown table. For full table definitions use columns like `Column`, `Type`, `Constraints`. For read-only or scoped specs you may use `Column`, `Type`, `Use` to describe how each column is used (e.g. “Returned as `venueGuid` in response”, “Sort key”).
- **2.3 Migration Notes** (optional): Indexes, collation, or future changes (e.g. “Add unique index on `VenueGuid`”, “Planned migration to geography”).

### 3. API Contract

- **3.1 OpenAPI Specification**: A single YAML code block (OpenAPI 3.0.x). Include only the paths and components this spec owns. Use `operationId`, `summary`, and `description` where they help. Reference shared schemas (e.g. `VenueResponse`, `ProblemDetails`) so the spec stays consistent with other specs.
- For list endpoints, document ordering and empty-list behaviour in the operation `description` or in Behaviour Notes.

### 4. Behaviour Notes

- Bullet list of rules that are not fully expressed in the schema or OpenAPI: ordering, empty results, error handling, validation rules, identifier exposure (e.g. “only `venueGuid` is exposed”), stability guarantees, and any client expectations (e.g. 401 handling).

---

## File location and naming

- **Path**: `docs/specs/SPECnnn-Short-Name.md`
- **Naming**: `SPEC` + zero-padded number + `-` + short kebab-case name (e.g. `SPEC001-List-Venues.md`, `SPEC002-Venue-Management.md`).

---

## Conventions

- **Identifiers**: Prefer a public, stable identifier (e.g. GUID) in URLs and responses; do not expose internal surrogate keys (e.g. integer `VenueId`) in the API.
- **OpenAPI**: Keep request/response schemas in sync with other specs (e.g. shared `VenueResponse`). Use `$ref` to shared components where possible.
- **ERD**: Show only tables and columns relevant to this spec; full schema may live in a domain or database doc.
- **Behaviour**: Put anything that constrains implementation or client behaviour in §4 Behaviour Notes so implementers and reviewers have one place to look.

---

## Checklist (when writing or reviewing a spec)

1. Title and filename use the same SPEC number and a consistent short name.
2. User Story link exists and points to the correct file in `docs/user-stories/`.
3. Overview clearly states scope and key concepts without diving into implementation.
4. Database section includes an ERD and a table section; migration notes are present if there are indexes or future changes.
5. API section includes an OpenAPI block; only paths/schemas owned by this spec are included.
6. Ordering, empty list, errors, and identifier exposure are specified (in OpenAPI and/or Behaviour Notes).
7. Status is set and matches project workflow (e.g. Draft until review).

---

## Reference specs

- **Narrow scope (single operation)**: `docs/specs/SPEC001-List-Venues.md` — list endpoint, ordering, empty list, minimal schema section.
- **Broad scope (CRUD)**: `docs/specs/SPEC002-Venue-Management.md` — full table definition, multiple operations, shared request/response schemas.

Use these as templates when creating a new spec; copy the section headings and adapt content to the feature.
