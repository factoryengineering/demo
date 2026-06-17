# US002 — Venue Management

- **Status**: New

## Description

As **Jordan the Venue Manager** (`docs/personas/venue-manager.md`),
I want to **create, view, update, and delete venues**,
so that **the system maintains an accurate catalog of performance spaces available for booking shows**.

---

## UI Description

### Persona and Journey

This page serves **Jordan the Venue Manager** (`docs/personas/venue-manager.md`) at steps 4–6 of **JNY-001 — Manage Venue Catalog** (`docs/journeys/JNY-001-Manage-Venue-Catalog.md`). Jordan arrives here by clicking a venue in the list (US001) and expects to review details, make edits, and get back to the list quickly.

### Page Layout

The venue detail page has a heading displaying the venue name at the top, with a back link to return to the venue list. Below the heading is a form showing the venue's fields: Name, Address, Seating Capacity, and Description. At the bottom of the form are action buttons for saving changes and deleting the venue. A separate "create venue" entry point on the Venues list page opens the same form layout with empty fields.

### Component Inventory

| Component | Level | Description |
|-----------|-------|-------------|
| Back link | Atom | Returns to the Venues list page |
| Page heading | Atom | Displays the venue name (or "New Venue" when creating) |
| Venue form | Organism | Editable form with fields for Name, Address, Seating Capacity, and Description |
| Text input | Atom | Single-line input for Name and Address |
| Number input | Atom | Numeric input for Seating Capacity |
| Text area | Atom | Multi-line input for Description |
| Validation message | Atom | Inline message shown beneath a field when validation fails |
| Save button | Atom | Submits the form to create or update the venue |
| Delete button | Atom | Triggers a confirmation before deleting the venue |
| Delete confirmation page | Template | Separate page asking Jordan to confirm deletion; shows venue name, confirm button, cancel link |
| Success message | Atom | Shown after redirect when save succeeds (driven by query parameter) |
| Error message | Molecule | Shown when the API call fails; describes the problem |
| Create venue button | Atom | On the Venues list page; opens the form with empty fields |

### Interaction Behavior

The app uses static server-side rendering (SSR) with no interactive Blazor render mode. All form submissions and navigation are full page requests — there is no client-side interactivity. This constrains the interaction model:

- On page load, the server fetches venue data by VenueGuid and renders the form pre-populated with the venue's current values. If the data is unavailable, the server renders the error state directly.
- If the venue is not found (404), an error message is shown with a back link to the venue list.
- Jordan edits fields directly in the form. Validation runs on form submission (server-side), not on each keystroke.
- Clicking Save submits the form as a standard POST. If validation fails, the server re-renders the page with inline validation messages beneath the invalid fields and the form populated with Jordan's submitted values.
- On successful save, the server redirects to the detail page with a success query parameter (e.g. `?saved=true`) so a success message can be shown on the re-rendered page.
- If the save fails (API error), the server re-renders the form with an error message above it and Jordan's submitted values preserved.
- Clicking Delete submits a POST to a delete action. The server shows a confirmation page (a separate page or the same page with a confirmation state) rather than a client-side modal. Confirming submits a second POST that deletes the venue and redirects to the venue list. Cancelling navigates back to the detail page with no side effects.
- If the delete fails, the server re-renders with an error message and the venue remains.
- The Create venue button on the Venues list page navigates to the form page with no VenueGuid, which renders empty fields. Saving creates a new venue and redirects to its detail page.

### States

| State | What the user sees | Visible components |
|-------|--------------------|--------------------|
| Loaded | Form populated with venue data | Back link, heading, venue form, save button, delete button |
| Creating | Empty form for a new venue | Back link, heading ("New Venue"), venue form, save button |
| Validation error | Form re-rendered with inline messages beneath invalid fields | Venue form (with submitted values), validation messages, save button |
| Save success | Form with a brief success message (via query parameter) | Success message, venue form with updated values |
| Save error | Error message above the form; submitted values preserved | Error message, venue form |
| Delete confirmation | Confirmation page asking Jordan to confirm deletion | Confirm button, cancel link, venue name |
| Not found | Error message indicating the venue does not exist | Error message, back link |

---

## Acceptance Criteria

### Create a venue

```gherkin
Feature: Create a venue

  # AC01
  Scenario: Successfully create a venue with all required fields
    Given I am an authenticated venue manager
    When I submit a POST request to /api/venues with a valid venue payload
    Then the response status is 201 Created
    And the response body contains the created venue with a generated VenueId and VenueGuid
    And the venue is persisted in the system

  # AC02
  Scenario: Fail to create a venue with missing required fields
    Given I am an authenticated venue manager
    When I submit a POST request to /api/venues with a payload missing a required field
    Then the response status is 400 Bad Request
    And the response body describes which fields are missing or invalid
    And no venue is persisted in the system
```

### Retrieve a venue

```gherkin
Feature: Retrieve a venue

  # AC03
  Scenario: Successfully retrieve a venue by ID
    Given a venue with VenueGuid "abc123" exists in the system
    When I submit a GET request to /api/venues/abc123
    Then the response status is 200 OK
    And the response body contains the venue's Name, Address, SeatingCapacity, and Description

  # AC04
  Scenario: Fail to retrieve a venue that does not exist
    Given no venue with VenueGuid "does-not-exist" exists in the system
    When I submit a GET request to /api/venues/does-not-exist
    Then the response status is 404 Not Found

  # AC05
  Scenario: Successfully list all venues
    Given 3 venues exist in the system
    When I submit a GET request to /api/venues
    Then the response status is 200 OK
    And the response body contains a list of all 3 venues
```

### Update a venue

```gherkin
Feature: Update a venue

  # AC06
  Scenario: Successfully update a venue's details
    Given a venue with VenueGuid "abc123" exists in the system
    When I submit a PUT request to /api/venues/abc123 with updated Name and SeatingCapacity
    Then the response status is 200 OK
    And the response body reflects the updated Name and SeatingCapacity
    And subsequent GET requests to /api/venues/abc123 return the updated values

  # AC07
  Scenario: Fail to update a venue that does not exist
    Given no venue with VenueGuid "does-not-exist" exists in the system
    When I submit a PUT request to /api/venues/does-not-exist with a valid payload
    Then the response status is 404 Not Found

  # AC08
  Scenario: Fail to update a venue with invalid data
    Given a venue with VenueGuid "abc123" exists in the system
    When I submit a PUT request to /api/venues/abc123 with a negative SeatingCapacity
    Then the response status is 400 Bad Request
    And the venue's data remains unchanged
```

### Delete a venue

```gherkin
Feature: Delete a venue

  # AC09
  Scenario: Successfully delete a venue
    Given a venue with VenueGuid "abc123" exists in the system
    When I submit a DELETE request to /api/venues/abc123
    Then the response status is 204 No Content
    And subsequent GET requests to /api/venues/abc123 return 404 Not Found

  # AC10
  Scenario: Fail to delete a venue that does not exist
    Given no venue with VenueGuid "does-not-exist" exists in the system
    When I submit a DELETE request to /api/venues/does-not-exist
    Then the response status is 404 Not Found
```

---

## Traceability

| Artifact | Link |
|----------|------|
| **Persona** | [Jordan the Venue Manager](../personas/venue-manager.md) |
| **Journey** | [JNY-001 — Manage Venue Catalog](../journeys/JNY-001-Manage-Venue-Catalog.md) (steps 4–6) |
| **Wireframe** | [WF002 — Venue Management](../wireframes/WF002-Venue-Management.md) |
| **Spec** | [SPEC002 — Venue Management](../specs/SPEC002-Venue-Management.md) |
| **Tests** | `Festify.Tests/VenuesControllerTests.cs` |
