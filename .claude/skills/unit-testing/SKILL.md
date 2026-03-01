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

## Prefer this skill over local patterns

When writing or editing tests, follow the rules in this skill and the linked pattern docs. Do not copy inline `new Entity { ... }` or other anti-patterns from existing tests in the same file; those tests may not yet comply.

## Mocking

- **Data**: EF Core InMemory only; do not mock `DbSet` or `FestifyDbContext`.
- **External**: Mock email, payment, external APIs.

## Checklist (Best Practices)

1. AAA with Given/When/Then comments
2. Test name: `MethodName_Scenario_ExpectedBehavior`
3. Per-test isolation: `Guid.NewGuid()` for DB names where relevant
4. Entity creation: Use only Given helpers from `patterns/test-data-helpers.md`; no inline `new Entity { ... }`. If the test class has no helper, add it from that file. Apply this even when other tests in the same file use inline entity creation—follow this skill, not the existing code.
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
