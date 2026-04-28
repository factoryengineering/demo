---
name: unit-testing
description: Unit testing patterns for this project (xUnit, WebApplicationFactory, EF Core In-Memory, AAA, Given helpers). Use when writing or reviewing controller, service, or domain tests, or when creating or seeding test entity data.
---

# Unit Testing Patterns for Festify

Test structure, in-memory DbContext, WebApplicationFactory integration tests, and Given helper usage.

## Two Kinds of Tests

| Kind | Setup | Use for |
|------|--------|---------|
| **Unit** | Controllers/services + in-memory `FestifyDbContext` | Business logic, validation, data operations |
| **Integration** | `FestifyWebApplicationFactory` + HTTP client | Routing, status codes, response shape, `Location` headers |

## Test Structure

- **AAA**: `// Given`, `// When`, `// Then`
- **Naming**: `MethodName_Scenario_ExpectedBehavior`
- **Isolation**: Each test creates its own data; no shared state

## When to Load Which Pattern

- **Creating or seeding test entity data** → Load [patterns/test-data-helpers.md](patterns/test-data-helpers.md). Use the documented Given helpers for each entity type; add a helper to the test class if missing. Do not inline `new Entity { ... }`.
- **Unit test DbContext setup** → [patterns/ef-core-in-memory-setup.md](patterns/ef-core-in-memory-setup.md)
- **Integration tests (HTTP pipeline)** → [patterns/web-application-factory-setup.md](patterns/web-application-factory-setup.md)

## Given helpers are mandatory (not optional)

- Any test code that **constructs or seeds a domain entity** (`Venue`, `Act`, `Show`, `TicketSale`, …) must use a **`Given*` helper** from [patterns/test-data-helpers.md](patterns/test-data-helpers.md). Copy the helper into the test class if it is missing.
- **Never** inline `new Entity { ... }` in a test method, **even when** neighboring tests in the same file already do that. Existing inline usage is non-compliant legacy; **do not copy it** and **do not leave it behind** when you edit that file—convert those call sites to `Given*` helpers in the **same change** as your test work.
- **Never** skip helpers to keep a diff smaller or to avoid touching unrelated tests. Helper usage is part of the required implementation, not an optional cleanup.

## Prefer this skill over local patterns

When writing or editing tests, follow the rules in this skill and the linked pattern docs. Treat inline entity construction in the codebase as something to **replace**, not as permission to add more of the same.

## Mocking

- **Data**: EF Core InMemory only; do not mock `DbSet` or `FestifyDbContext`.
- **External**: Mock email, payment, external APIs.

## Traceability

Every test method should be traceable to the acceptance criterion it verifies. The tech spec's Test Coverage table (§5 in `.claude/skills/tech-spec/`) maps scenario IDs to test classes and methods. When writing a new test:

1. Identify the acceptance criterion (e.g. `AC01`) or scenario name from the user story or spec.
2. Name the test so the scenario is recognizable: `MethodName_Scenario_ExpectedBehavior`.
3. After writing the test, update the spec's Test Coverage table with the file path and method name.

This keeps the chain intact: **persona → journey → user story → spec → test**.

## Checklist (Best Practices)

1. AAA with Given/When/Then comments
2. Test name: `MethodName_Scenario_ExpectedBehavior`
3. Per-test isolation: `Guid.NewGuid()` for DB names where relevant
4. Entity creation: **Mandatory** `Given*` helpers only; see **Given helpers are mandatory** above. Same change must fix any remaining inline entity creation in the test file you touched.
5. In Given helpers: navigation properties, never raw foreign key IDs
6. Parent entities required in helper params; scalars optional with defaults
7. Unit tests for logic; integration tests for HTTP contract
8. With `IClassFixture`: reset DB in constructor
9. Do not share `DbContext` between arrange and system under test

## Resources

| File | When to load |
|------|----------------|
| [patterns/test-data-helpers.md](patterns/test-data-helpers.md) | Creating or seeding test entity data in tests |
| [patterns/ef-core-in-memory-setup.md](patterns/ef-core-in-memory-setup.md) | In-memory DbContext for unit tests |
| [patterns/web-application-factory-setup.md](patterns/web-application-factory-setup.md) | Integration tests, WebApplicationFactory |
| [examples/controller-unit-tests.md](examples/controller-unit-tests.md) | Controller tests without HTTP |
| [examples/integration-tests.md](examples/integration-tests.md) | HTTP-level integration tests |
