---
name: blazor-ui
description: Blazor UI best practices for this project using Atomic Design. Use when building or refactoring Blazor (.razor) components, pages, or layouts in Festify.Web; when organizing components into atoms, molecules, organisms, templates, or pages; when adding new Razor components, layouts, or routes; or when the user asks about Blazor structure, component hierarchy, or UI patterns in this codebase.
---

# Blazor UI — Atomic Design

This skill defines how to structure and build Blazor user interfaces in this project. We favor **Atomic Design**: components are organized by scope and reusability (atoms → molecules → organisms → templates → pages) so that the UI is predictable, testable, and easy to change.

---

## Atomic Design hierarchy

| Layer | Purpose | Examples | Lives in |
|-------|--------|----------|----------|
| **Atoms** | Smallest UI units; no composition of other components. | Button, Input, Label, Icon, Badge | `Components/UI/Atoms/` |
| **Molecules** | Compositions of atoms; one clear responsibility. | SearchBar (Input + Button), CardHeader (Title + Badge) | `Components/UI/Molecules/` |
| **Organisms** | Sections of a page; molecules + atoms or other organisms. | NavBar, VenueList, FormSection | `Components/UI/Organisms/` |
| **Templates** | Page shells: layout slots, no domain data. | MainTemplate (header slot, main slot, footer slot) | `Components/Layout/` or `Components/Templates/` |
| **Pages** | Route-backed views; wire data and route params into templates/organisms. | Home, VenueListPage, VenueEditPage | `Components/Pages/` |

**Rules of thumb:**

- Atoms and molecules stay presentational: they receive data via `[Parameter]` and emit via `[Parameter] EventCallback`; they do not call HTTP or application services directly.
- Organisms may inject services (e.g. `HttpClient`, `NavigationManager`) when they own a full feature slice (e.g. “venue list” that fetches and displays).
- Pages are thin: they resolve route parameters, load data (or delegate to an organism), and pass data into templates/organisms. Prefer one primary organism per page when possible.
- Build from the bottom up when adding a new flow: define or reuse atoms/molecules first, then organisms, then wire the page.

---

## Folder structure (Festify.Web)

Align with the existing structure and extend it with Atomic Design:

```
Festify.Web/
├── Components/
│   ├── App.razor              # Root component, head, script includes
│   ├── Routes.razor           # Router + RouteView, DefaultLayout, NotFound
│   ├── _Imports.razor         # Global usings for all .razor files
│   ├── Layout/
│   │   └── MainLayout.razor   # @inherits LayoutComponentBase, @Body
│   ├── Pages/                 # Route-backed pages (@page "...")
│   │   ├── Home.razor
│   │   ├── NotFound.razor
│   │   └── Error.razor
│   └── UI/                    # Atomic Design components (add as needed)
│       ├── Atoms/
│       ├── Molecules/
│       └── Organisms/
```

- **Layout**: Use `Components/Layout/` for layout components that wrap `@Body` (e.g. `MainLayout.razor`). These act as templates that define slots (e.g. sidebar, main content).
- **Pages**: Every `@page` directive lives under `Components/Pages/`. Use kebab-case for route paths (e.g. `@page "/venues"`, `@page "/venues/{Id:guid}"`).
- **UI**: When you add reusable building blocks, place them under `Components/UI/` with subfolders `Atoms/`, `Molecules/`, `Organisms/` so the hierarchy is explicit.

---

## Component conventions

### Naming and files

- One component per file. File name = component name (PascalCase), e.g. `PrimaryButton.razor`, `VenueCard.razor`.
- Use PascalCase for component names and parameters. Use camelCase for local variables and private fields.

### Parameters and events

- Use `[Parameter]` for public input; use `[Parameter] public EventCallback<T> OnXxx { get; set; }` for callbacks so parent can react (e.g. `OnSubmit`, `OnCancel`).
- Prefer explicit parameter names in markup: `<PrimaryButton Label="Save" OnClick="HandleSave" />` for clarity.
- For optional parameters, use `[Parameter] public string? Label { get; set; }` and document defaults in a `<summary>` or in the skill.

### Private members (project rule)

- **Do not** use the `_` prefix for private members. Use `private string? requestId;` and `private bool ShowRequestId => ...` (as in `Error.razor`), not `_requestId`.

### Markup and structure

- Prefer semantic HTML: `<main>`, `<nav>`, `<article>`, `<section>`, `<header>`, `<footer>` where appropriate. Use `<div>` only when no semantic element fits.
- Set `<PageTitle>` on pages so the browser tab and accessibility tree get a meaningful title.
- Use `@layout MainLayout` (or another layout) on pages that should use a layout; omit only for full-screen or error pages if intended.

### Routing and layout

- Define routes with `@page "/path"` at the top of the page. Use route constraints (e.g. `{Id:guid}`) when applicable.
- `Routes.razor` should reference `NotFoundPage="typeof(Pages.NotFound)"` and `DefaultLayout="typeof(Layout.MainLayout)"` so 404 and default layout are consistent.
- Keep layout components thin: they compose `@Body` and optional chrome (nav, footer); they do not hold page-specific logic.

### Data and async

- Use `@code { }` for component logic. Prefer `OnInitializedAsync` / `OnParametersSetAsync` for loading data when the component receives route or query parameters.
- When loading data on a page, show a loading state (e.g. “Loading…”) and handle errors (e.g. redirect to `/Error` or show an inline message) so the UI never stays in a blank or broken state.
- Prefer injecting `HttpClient` via `[Inject]` (or a named client like `IHttpClientFactory` with `Festify.Api` base address) in the component that owns the request (usually an organism or page), not in atoms/molecules.

### Accessibility and forms

- Associate labels with inputs (e.g. `<label for="...">` or wrap input in `<EditForm>` with `<InputText id="..." />` and a matching label).
- Use `EditForm`, `DataAnnotationsValidator`, and `ValidationSummary` for form validation so validation is consistent and accessible.

---

## Checklist (when adding or refactoring Blazor UI)

1. **Placement**: New building block → `UI/Atoms`, `UI/Molecules`, or `UI/Organisms`; route-backed screen → `Pages/`; layout wrapper → `Layout/`.
2. **Atomic level**: Component is at the right layer (atom = no composition of other components; molecule = small composition; organism = section of a page).
3. **Parameters**: Input via `[Parameter]`; output via `EventCallback`; no `_` prefix on private members.
4. **Page**: Has `@page` and `<PageTitle>`; uses `@layout` when a layout is desired.
5. **Semantics**: Uses semantic HTML and accessible form/label wiring where relevant.
6. **Data**: Data loading and HTTP live in pages or organisms, not in atoms/molecules.
7. **Imports**: Shared usings live in `_Imports.razor`; component-specific usings at top of file if needed.

---

## References in this project

- **App shell**: `Components/App.razor` — root HTML, head, static assets, Blazor script.
- **Routing**: `Components/Routes.razor` — Router, RouteView, DefaultLayout, NotFound.
- **Layout**: `Components/Layout/MainLayout.razor` — layout base, `@Body`.
- **Pages**: `Components/Pages/Home.razor`, `NotFound.razor`, `Error.razor` — examples of pages with `@page`, `PageTitle`, and minimal `@code`.
- **Global usings**: `Components/_Imports.razor` — so every Razor file gets the same base usings (e.g. `Microsoft.AspNetCore.Components.Web`, `Festify.Web.Components`).

When adding new UI, follow this structure and the Atomic Design hierarchy so the codebase stays consistent and easy to navigate.
