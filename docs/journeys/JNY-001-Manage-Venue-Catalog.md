# JNY-001 — Manage Venue Catalog

## Metadata

| Field | Value |
|-------|-------|
| **Persona** | Jordan the Venue Manager (`docs/personas/venue-manager.md`) |
| **Goal** | Review the venue list, verify details, and update a venue's information |
| **Entry point** | Landing page |
| **Exit point** | Venues page, after confirming the update |

## Steps

| # | Page | Action | Outcome | Emotion |
|---|------|--------|---------|---------|
| 1 | Landing page | Clicks the Venues navigation card | Navigates to the Venues page | Confident |
| 2 | Venues page | Scans the alphabetically sorted venue list | Sees all venues by name | Neutral |
| 3 | Venues page | Finds "The Grand Hall" in the list | Locates the venue they need to update | Satisfied |
| 4 | Venues page | Clicks "The Grand Hall" | Navigates to the venue detail view | Confident |
| 5 | Venue detail | Reviews current name, address, and seating capacity | Confirms which field needs updating | Neutral |
| 6 | Venue detail | Edits seating capacity and saves | Sees confirmation that the venue was updated | Satisfied |
| 7 | Venues page | Returns to the venue list | Sees the updated venue in the list | Confident |

## Alternate Paths

- **Step 2 — API error**: The venue list fails to load. Jordan sees an error message with a retry option. (US001 error paths.)
- **Step 2 — Empty list**: No venues exist yet. Jordan sees an empty-state message and a prompt to create the first venue.
- **Step 3 — Venue not listed**: The venue Jordan needs is not in the catalog. Jordan creates a new venue instead of editing an existing one.
- **Step 6 — Validation error**: Jordan submits invalid data (e.g. negative seating capacity). The form shows a clear error and Jordan corrects the field.
- **Step 6 — Save fails**: The API returns an error. Jordan sees an error message and can retry without losing their edits.

## Related Stories

- US001 — List Venues (steps 1–3, 7)
- US002 — Venue Management (steps 4–6, plus alternate path for creating a new venue)
