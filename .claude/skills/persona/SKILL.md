---
name: persona
description: Defines how to write and maintain persona files in docs/personas/. Use when creating a new persona, reviewing or updating an existing one, when a user story needs a named persona instead of a generic role, or when the user asks about persona format, structure, or best practices in this project.
---

# Persona

Personas are fictional but realistic archetypes of the people who use Festify. Every user story that describes a UI references a persona by name so the team builds for a real human rather than a generic "user."

---

## File Location and Naming

| Item | Convention |
|------|------------|
| **Directory** | `docs/personas/` |
| **Filename** | `kebab-case-name.md` (e.g. `casual-concertgoer.md`, `venue-manager.md`) |

---

## Document Anatomy

Each persona file has five sections in this order.

### 1. Title

`# Persona — Display Name`

A short, memorable name that the team uses in conversation and in user stories.

### 2. Summary

One or two sentences: who this person is, what they care about, and why they use Festify.

### 3. Profile

A table of key attributes:

| Attribute | Description |
|-----------|-------------|
| **Role** | Their relationship to the product (e.g. event organizer, ticket buyer, venue staff) |
| **Goal** | The primary outcome they want from Festify |
| **Tech comfort** | Low, moderate, or high — how confident they are navigating web apps |
| **Frequency** | How often they use the product (daily, weekly, occasionally) |
| **Frustrations** | One or two pain points that affect their experience |

### 4. Context

A short paragraph — two to four sentences — describing a realistic scenario in which this persona interacts with the product. Include enough detail that a developer reading it can picture the moment: where they are, what device they're using, what just happened before they opened the app.

### 5. Referenced By

A bulleted list of user story IDs that reference this persona. Keep this list current as stories are added or retired.

---

## Example

```markdown
# Persona — Jordan the Venue Manager

Jordan runs operations for a mid-size concert hall and uses Festify to keep the venue catalog accurate and up to date.

## Profile

| Attribute | Description |
|-----------|-------------|
| **Role** | Venue operations manager |
| **Goal** | Maintain an accurate, complete venue listing so promoters can find and book the space |
| **Tech comfort** | Moderate — comfortable with web forms but impatient with slow or confusing interfaces |
| **Frequency** | Weekly |
| **Frustrations** | Having to re-enter data that the system should remember; unclear error messages when something fails |

## Context

Jordan is at their desk between meetings. They open Festify on a laptop to update the seating capacity after a renovation. They want to find the venue quickly, make the edit, and move on — anything that takes more than a couple of minutes feels like wasted time.

## Referenced By

- US001 — List Venues
- US002 — Venue Management
```

---

## Checklist

1. File lives in `docs/personas/` with a kebab-case filename.
2. Title uses the format `# Persona — Display Name`.
3. Summary is one to two sentences.
4. Profile table includes all five attributes: Role, Goal, Tech comfort, Frequency, Frustrations.
5. Context paragraph is two to four sentences and paints a concrete scenario.
6. Referenced By list is present and current.
7. The persona is distinct from every other persona in the directory — no two personas serve the same purpose.
