---
name: user-story
description: Documents the structure and format of user stories in this project. Use when writing a new user story in docs/user-stories/, reviewing or refactoring an existing one, or when the user asks how user stories are structured or what format to follow.
---

# User Story Structure

User stories in this project live in `docs/user-stories/` and follow a consistent format so they are traceable, testable, and easy to compare.

## File Location and Naming

| Item | Convention |
|------|------------|
| **Directory** | `docs/user-stories/` |
| **Filename** | `USXXX-Short-Title.md` (e.g. `US001-List-Venues.md`, `US002-Venue-Management.md`) |
| **Story ID** | `US` + 3-digit number. Use the next available number by checking existing files in the directory. |

## Document Anatomy

| Section | Purpose |
|--------|---------|
| **Title** | `# USXXX — Title` (em dash between ID and title) |
| **Status** | `- **Status**: New` (or Draft, Done, etc., as the project uses) |
| **Description** | One-sentence user story plus optional context paragraph |
| **Acceptance Criteria** | Gherkin scenarios grouped by theme |

## Description Format

Use the standard user-story sentence:

- **As a** {role}, **I want** {behavior}, **so that** {goal}.

Format the three parts in bold: **role**, **behavior**, **goal**.

Optionally add a short context paragraph below (e.g. which page or API is involved, entry point, or key constraints). Keep it to one or two sentences.

**Example:**

```markdown
As a **user**,
I want to **view a list of all venues from the Venues page, accessible from the landing page via a navigation card**,
so that **I can browse available performance spaces by name and choose one for events or booking**.

The Venues page is reached from the Festify landing page (Home) via a navigation card. The page calls the Festify API `VenuesController` to list all venues and displays them by name in a scrollable container.
```

## Acceptance Criteria (Gherkin)

Acceptance criteria are written in Gherkin and placed in fenced code blocks with the `gherkin` language tag.

### Structure

- **Feature:** One per logical area (e.g. "Navigate to Venues page", "List all venues", "Error handling").
- **Scenario:** One per concrete case. Use a clear, outcome-focused title.
- **Steps:** `Given` (precondition), `When` (action), `Then` (outcome). Use `And` to extend any of the three.
- **Optional:** Number scenarios with `# AC01`, `# AC02`, etc. when traceability to tests or specs is needed.

### Grouping by Theme

Group scenarios under level-3 headings so readers can quickly find happy path, errors, and edge cases:

| Theme | Use for |
|-------|--------|
| **Happy path** | Normal success flows: user does X and sees/gets Y. |
| **Error paths** | API failures, validation errors, auth (401/403), network errors. What the user sees and can do (e.g. retry, sign-in). |
| **Edge cases** | Empty list, single item, special characters, large data, permissions, or other boundary conditions. |

### Example Snippet

```gherkin
Feature: List all venues on Venues page

  Scenario: Venues are displayed by name in a scrollable container
    Given I am on the Venues page
    And the API returns at least one venue
    When the page has finished loading
    Then I see a list of venues
    And each venue is displayed by name
    And the list is in a scrollable container
```

## Section Separator

Use a horizontal rule (`---`) between the Description and the Acceptance Criteria.

## Checklist (when writing or reviewing)

1. Filename is `USXXX-Short-Title.md` and ID matches the next available number.
2. Title uses em dash: `# USXXX — Title`.
3. Description is one sentence in "As a **role**, I want **behavior**, so that **goal**."
4. Acceptance criteria are in ```gherkin code blocks.
5. Scenarios have clear Given/When/Then (and And where needed).
6. Scenarios are grouped under headings (e.g. happy path, error paths, edge cases).
7. At least one scenario each for success, failure, and edge cases when relevant.

## References

- Existing examples: `docs/user-stories/US001-List-Venues.md`, `docs/user-stories/US002-Venue-Management.md`
- Command for drafting stories via clarification: `.cursor/commands/write-user-story.md`
