# US001 — Venue Management

- **Status**: New

## Description

As a **venue manager**,
I want to **create, view, update, and delete venues**,
so that **the system maintains an accurate catalog of performance spaces available for booking shows**.

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
