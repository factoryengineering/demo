---
name: user-journey
description: Defines how to write journey maps in docs/journeys/. Use when creating a new user journey, reviewing or updating an existing one, when a user story needs to identify where a page sits in a larger flow, or when the user asks about journey map format, structure, or best practices in this project.
---

# User Journey

A journey map traces one persona through a single goal from start to finish. It captures the sequence of pages, actions, and outcomes the persona experiences — the connective tissue that individual user stories often leave implicit.

---

## File Location and Naming

| Item | Convention |
|------|------------|
| **Directory** | `docs/journeys/` |
| **Filename** | `JNY-XXX-Short-Title.md` (e.g. `JNY-001-Browse-And-Select-Venue.md`) |
| **Journey ID** | `JNY-` + 3-digit number. Use the next available number by checking existing files. |

---

## Document Anatomy

Each journey file has five sections in this order.

### 1. Title

`# JNY-XXX — Journey Title`

Use an em dash between the ID and the title.

### 2. Metadata

| Field | Value |
|-------|-------|
| **Persona** | Name of the persona (must match a file in `docs/personas/`) |
| **Goal** | One sentence — what the persona is trying to accomplish |
| **Entry point** | Where the journey begins (e.g. "Landing page", "Email link", "Direct URL") |
| **Exit point** | Where the journey ends when the goal is achieved |

### 3. Steps

A numbered list of steps. Each step has:

| Column | Description |
|--------|-------------|
| **Page** | The page or view the persona is on |
| **Action** | What the persona does |
| **Outcome** | What they see or what happens as a result |
| **Emotion** | One word — confident, uncertain, frustrated, satisfied, neutral |

Present this as a markdown table:

```markdown
| # | Page | Action | Outcome | Emotion |
|---|------|--------|---------|---------|
| 1 | Landing page | Clicks the Venues card | Navigates to the Venues page | Confident |
| 2 | Venues page | Scans the venue list | Sees venues listed by name in alphabetical order | Neutral |
| 3 | Venues page | Scrolls to find "The Grand Hall" | Finds the venue | Satisfied |
```

Keep the step count between 3 and 10. If a journey needs more than 10 steps it should probably be split into two journeys.

### 4. Alternate Paths

A bulleted list of deviations from the happy path — errors, empty states, decision branches — and which step they diverge from. Each item names the condition and describes what the persona sees or does.

```markdown
- **Step 2 — API error**: The venue list fails to load. The persona sees an error message with a retry option. (See US001 error paths.)
- **Step 2 — Empty list**: No venues exist. The persona sees an empty-state message instead of a list.
```

### 5. Related Stories

A bulleted list of user story IDs that implement parts of this journey.

---

## Example

```markdown
# JNY-001 — Browse and Select a Venue

## Metadata

| Field | Value |
|-------|-------|
| **Persona** | Jordan the Venue Manager |
| **Goal** | Find a specific venue in the catalog to review its details |
| **Entry point** | Landing page |
| **Exit point** | Venue detail page |

## Steps

| # | Page | Action | Outcome | Emotion |
|---|------|--------|---------|---------|
| 1 | Landing page | Clicks the Venues navigation card | Navigates to the Venues page | Confident |
| 2 | Venues page | Scans the alphabetically sorted venue list | Sees all venues by name | Neutral |
| 3 | Venues page | Scrolls to find "The Grand Hall" | Locates the venue in the list | Satisfied |
| 4 | Venues page | Clicks "The Grand Hall" | Navigates to the venue detail page | Confident |

## Alternate Paths

- **Step 2 — API error**: Venue list fails to load. Persona sees an error message with a retry button.
- **Step 2 — Empty catalog**: No venues exist. Persona sees an empty-state message.
- **Step 3 — Venue not found**: Persona scrolls through the entire list without finding their venue.

## Related Stories

- US001 — List Venues
```

---

## Checklist

1. File lives in `docs/journeys/` with the `JNY-XXX-Short-Title.md` naming convention.
2. Title uses em dash: `# JNY-XXX — Title`.
3. Metadata table includes Persona, Goal, Entry point, and Exit point.
4. Persona name matches a file in `docs/personas/`.
5. Steps table has columns: #, Page, Action, Outcome, Emotion.
6. Step count is between 3 and 10.
7. Alternate Paths section lists at least one deviation with the step it diverges from.
8. Related Stories section lists user story IDs that implement this journey.
