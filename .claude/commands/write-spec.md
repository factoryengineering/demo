# Write Specification

Use this command when the user wants to create a technical specification from a user story. The user will provide a user story (by reference, path, or pasted content). Your job is to clarify scope and design choices through dialogue, propose alternatives where it helps, then write the spec document.

## Workflow

### 1. Load the user story

- If the user gave a path or reference (e.g. `US001-List-Venues` or `docs/user-stories/US002-Venue-Management.md`), read that file.
- If they pasted the story, use it as given.
- Briefly confirm which story you're spec'ing and what it covers (one sentence).

### 2. Clarify scope and details — one question at a time

Ask **exactly one clarifying question per message**. Wait for the user's answer before asking the next. Do not batch multiple questions.

Use questions to establish:

- **Scope:** Which operations or flows does this spec cover? (e.g. list only vs full CRUD; which endpoints or screens?)
- **Data model:** What entities and attributes? New tables vs existing? Public identifier (e.g. GUID) vs internal key?
- **API shape:** Route prefix, path parameters (e.g. resource GUID in URL), request/response fields, and any constraints (ordering, pagination, filtering).
- **Behaviour:** Ordering, empty results, validation rules, error responses (4xx/5xx), and what must not be exposed (e.g. internal IDs).
- **Consistency:** Alignment with existing specs (e.g. shared schemas, naming, patterns from `docs/specs/`).

Skip questions the user story or existing specs already answer; move to the next open point.

### 3. Propose alternative designs

When a decision affects the contract or structure in a non-obvious way, **propose 2–3 concrete alternatives** and briefly compare them. Examples:

- **API design:** e.g. "Option A: Single GET with query params for filter/sort. Option B: Separate list and search endpoints. Option C: List only, no filter in v1."
- **Identifiers:** e.g. "Option A: GUID in URL and response. Option B: Slug. Option C: Integer with GUID only in response."
- **Schema scope:** e.g. "Option A: One spec for list + detail. Option B: Separate spec for list, another for CRUD."

Present options in a short list with one-line trade-offs. Ask the user which they prefer (or if they want a different variant) before locking it in. Do this for at most a few major choices so the conversation stays focused.

### 4. Stop clarifying when you can write the spec

You have enough when you know:

- Which user story file to link and the spec’s scope (operations/flows).
- Tables and columns (or “no new tables”) and any migration/index notes.
- Paths, verbs, request/response shapes, and key behaviour (ordering, empty list, errors, identifier exposure).

Then tell the user you’re ready to draft the spec and do so.

### 5. Write the specification

- **Path:** `docs/specs/SPECnnn-Short-Name.md`. Use the next free SPEC number (align with user story number when it makes sense, e.g. SPEC001 ↔ US001).
- **Structure:** Follow the tech-spec format:
  - **Header:** Title `# SPECnnn — Name`, **User Story** link to `../user-stories/USnnn-Name.md`, **Status:** `Draft`, then `---`.
  - **1. Overview:** What the spec defines, key concepts, identifiers, and scope (routes/consumers). No implementation detail.
  - **2. Database schema:** ERD (Mermaid), table(s) with Column/Type/Constraints (or Use), and optional Migration notes.
  - **3. API contract:** OpenAPI 3.0.x YAML for the paths and schemas this spec owns. Shared schemas can be `$ref`’d if they exist elsewhere.
  - **4. Behaviour notes:** Ordering, empty results, validation, errors, what is exposed/not exposed, stability.

Use existing specs in `docs/specs/` (e.g. SPEC001, SPEC002) as templates. Keep request/response schemas consistent with other specs where they share domains.

### 6. After writing

- Offer to adjust scope, API shape, or behaviour if the user wants changes.
- Offer to add or refine sections (e.g. migration notes, more error cases).

## Rules

- **One clarifying question per message** until you have enough to write. No lists of questions.
- Prefer open-ended questions for scope and behaviour; use short multiple-choice only when comparing concrete alternatives.
- When proposing alternatives, keep the list short (2–3 options) and state the main trade-off for each.
- If the user story or an existing spec clearly fixes a choice (e.g. “return list ordered by name”), do not ask again; incorporate it and ask the next open question.
- Reference the tech-spec skill and existing specs in `docs/specs/` so the output matches project conventions.
