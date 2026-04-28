---
name: ui-description
description: Defines how to write the UI Description section within a user story. Use when a user story describes a user interface and needs a UI Description section between the Description and Acceptance Criteria, when reviewing a UI story for completeness, or when the user asks how to document the visual layout, interaction behavior, or component structure of a page in a user story.
---

# UI Description

User stories that describe a user interface include a **UI Description** section between the Description and the Acceptance Criteria. This section tells developers what the user sees and how the page behaves — enough to build the right UI without guessing, but without dictating implementation details.

API-only stories omit this section entirely.

---

## When to Include

Include a UI Description when the acceptance criteria reference visual elements — pages, lists, containers, cards, buttons, error messages, loading states, or navigation actions. If every acceptance criterion is an HTTP request/response, the story is API-only and does not need this section.

---

## Section Placement

```
# USXXX — Title
- **Status**: ...

## Description
As a **role**, I want ... so that ...

---

## UI Description          <-- here, between Description and Acceptance Criteria

---

## Acceptance Criteria
```

---

## Section Anatomy

The UI Description has five subsections in this order. Each is a level-3 heading.

### 1. Persona and Journey

One to two sentences identifying which persona this page serves and where it sits in their journey. Reference the persona file and journey file by name.

```markdown
### Persona and Journey

This page serves **Jordan the Venue Manager** (`docs/personas/venue-manager.md`) at step 2 of **JNY-001 — Browse and Select a Venue** (`docs/journeys/JNY-001-Browse-And-Select-Venue.md`). Jordan arrives here from the landing page and expects to scan a list of venues quickly.
```

### 2. Page Layout

A short description of what the user sees when the page loads — the major regions and their spatial relationship. Describe layout in terms of behavior, not CSS or implementation. Use terms like "heading at the top," "scrollable list below the heading," "full-width card."

```markdown
### Page Layout

The page has a heading ("Venues") at the top. Below the heading is a scrollable container that fills the remaining viewport height. Inside the container, each venue appears as a row displaying the venue name.
```

### 3. Component Inventory

A table listing the visible UI components, their Atomic Design level (from the `blazor-ui` skill), and a brief description. This gives the frontend developer a checklist of what to build or reuse.

```markdown
### Component Inventory

| Component | Level | Description |
|-----------|-------|-------------|
| Page heading | Atom | Displays "Venues" |
| Venue list container | Organism | Scrollable container holding the list of venues |
| Venue list item | Molecule | Displays a single venue name as a row |
| Empty state message | Atom | Shown when the API returns no venues |
| Error message | Molecule | Shown when the API call fails; includes a retry action |
| Loading indicator | Atom | Shown while the API call is in progress |
```

### 4. Interaction Behavior

A bulleted list of behaviors the user can trigger or observe. Cover loading, scrolling, navigation, error recovery, and any transitions.

```markdown
### Interaction Behavior

- On page load, a loading indicator is shown while the API call is in progress.
- When data arrives, the loading indicator is replaced by the venue list, sorted alphabetically by name.
- If the API returns an empty list, the venue list container shows an empty-state message instead of rows.
- If the API call fails, an error message replaces the list with a retry action. Clicking retry re-issues the API call.
- The venue list is scrollable when the number of venues exceeds the viewport height.
```

### 5. States

A table summarizing the distinct visual states of the page. Each state names what the user sees and which components are visible.

```markdown
### States

| State | What the user sees | Visible components |
|-------|--------------------|--------------------|
| Loading | Loading indicator centered in the container | Loading indicator |
| Loaded | Alphabetically sorted list of venue names | Venue list container, venue list items |
| Empty | Message indicating no venues exist | Empty state message |
| Error | Error message with retry action | Error message |
```

---

## Style Rules

- Describe what the user sees and does, not how it is implemented. Say "scrollable container" not "`overflow-y: auto`". Say "retry action" not "`<button @onclick="Retry">`".
- Reference Atomic Design levels (atom, molecule, organism, page) from the `blazor-ui` skill so the frontend developer knows where each component lives.
- Reference the persona and journey by name and file path so the connection is traceable.
- Keep the section concise. The entire UI Description should fit in roughly 40–60 lines of markdown. If it's longer, the page may be too complex for a single story.

---

## Checklist

1. The story describes a UI (not API-only).
2. UI Description sits between Description and Acceptance Criteria, separated by horizontal rules.
3. Persona and Journey subsection names the persona and journey file.
4. Page Layout subsection describes spatial arrangement without implementation details.
5. Component Inventory table lists every visible component with its Atomic Design level.
6. Interaction Behavior covers loading, success, empty, and error states at minimum.
7. States table lists every distinct visual state of the page.
8. No CSS, HTML tags, or Blazor syntax appears in the UI Description.
9. The entire section is under 60 lines.
