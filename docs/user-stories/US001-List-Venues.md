# US001 — List Venues

- **Status**: New

## Description

As a **user**,
I want to **view a list of all venues from the Venues page, accessible from the landing page via a navigation card**,
so that **I can browse available performance spaces by name and choose one for events or booking**.

The Venues page is reached from the Festify landing page (Home) via a navigation card. The page calls the Festify API `VenuesController` to list all venues and displays them by name in a scrollable container.

---

## Acceptance Criteria

### Navigate to Venues page

```gherkin
Feature: Navigate to Venues page from landing

  Scenario: User reaches Venues page via navigation card on landing page
    Given I am on the Festify landing page
    When I click the Venues navigation card
    Then I am on the Venues page
    And the page title or heading indicates "Venues"
```

### List venues (happy path)

```gherkin
Feature: List all venues on Venues page

  Scenario: Venues are displayed by name in a scrollable container
    Given I am on the Venues page
    And the API returns at least one venue
    When the page has finished loading
    Then I see a list of venues
    And each venue is displayed by name
    And the list is in a scrollable container
    And venue names are visible without horizontal clipping

  Scenario: Multiple venues are ordered by name
    Given I am on the Venues page
    And the API returns venues "The Grand Hall", "Arena East", "Studio B"
    When the page has finished loading
    Then I see the venues in name order
    And "Arena East" appears before "Studio B"
    And "Studio B" appears before "The Grand Hall"

  Scenario: List is scrollable when many venues exist
    Given I am on the Venues page
    And the API returns more venues than fit in the viewport
    When the page has finished loading
    Then I see a scrollable container
    And I can scroll to see all venues
    And all venue names remain visible when scrolled
```

### Error paths

```gherkin
Feature: Venues page error handling

  Scenario: API returns an error when listing venues
    Given I am on the Venues page
    And the list-venues API request fails with 500 Internal Server Error
    When the page has finished loading or the request completes
    Then I see an error message indicating venues could not be loaded
    And I am not shown a partial or empty list as if success

  Scenario: Network failure when loading venues
    Given I am on the Venues page
    And the list-venues API request fails due to network error
    When the page has finished loading or the request completes
    Then I see an error message indicating a connection or loading problem
    And I have a way to retry loading venues

  Scenario: API returns 401 Unauthorized when listing venues
    Given I am on the Venues page
    And the list-venues API request returns 401 Unauthorized
    When the page has finished loading or the request completes
    Then I see an appropriate message or am redirected to sign in
    And I do not see venue data
```

### Edge cases

```gherkin
Feature: Venues page edge cases

  Scenario: No venues exist (empty state)
    Given I am on the Venues page
    And the API returns an empty list of venues
    When the page has finished loading
    Then I see the Venues page
    And I see an empty state message indicating no venues exist
    And the scrollable container is present but empty or shows the message
    And I do not see an error message

  Scenario: Single venue is displayed correctly
    Given I am on the Venues page
    And the API returns exactly one venue with name "Solo Stage"
    When the page has finished loading
    Then I see "Solo Stage" in the list
    And the scrollable container is present
    And the single venue is displayed by name

  Scenario: Venue names with special characters display correctly
    Given I am on the Venues page
    And the API returns a venue with name "Café René & Co."
    When the page has finished loading
    Then I see "Café René & Co." in the list
    And the name is fully visible and not corrupted
```
