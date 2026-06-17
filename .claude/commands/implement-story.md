---
description: Implements a user story end-to-end — backend via TDD, frontend via Blazor — by locating the linked spec and wireframe.
argument-hint: <user story path>
---

# Implement Story — Orchestrator Workflow

Use this command when the user wants to **implement a user story end-to-end**: backend (via the TDD cycle), optional data-warehouse work for domain events, and frontend (via the Blazor agent). You are the orchestrator. You delegate to specialized agents and workflows; you do not write specs, tests, or implementation yourself. When any agent escalates, **interrupt the normal workflow** and address the issue before continuing.

## Inputs

The user provides a **user story path** — e.g. `docs/user-stories/US002-Venue-Management.md`.

If the input is ambiguous (story ID only, title without path, or pasted content without a file), ask exactly one clarifying question to resolve the path.

---

## Orchestration Workflow (Pseudocode)

```
FUNCTION implement_story(user_story_input):

  user_story = load_or_use(user_story_input)
  spec = null
  wireframe = null
  persona = null
  journey = null
  has_ui = false


  // ── STEP 0: RESOLVE LINKED ARTIFACTS ──────────────────────────────────────

  PARSE user_story.Traceability section:
    spec_path       = row "Spec" link (relative path to docs/specs/SPEC*.md)
    wireframe_path  = row "Wireframe" link (relative path to docs/wireframes/WF*.md)
    persona_path    = row "Persona" link (if present)
    journey_path    = row "Journey" link (if present)

  IF spec_path is missing OR link is "—":
    // Fallback: search docs/specs/ for a spec whose header links back to this story
    spec_path = find_spec_linked_to(user_story)
  IF wireframe_path is missing OR link is "—":
    // Fallback: search docs/wireframes/ for a wireframe whose header links back to this story
    wireframe_path = find_wireframe_linked_to(user_story)

  has_ui = user_story has a UI Description section OR wireframe_path is not null

  IF spec_path is still missing:
    delegate_to(tech-lead-architect, {
      task: "Produce a complete technical specification for this user story",
      input: user_story,
      output: "docs/specs/SPECnnn-Short-Name.md"
    })
    ON escalation FROM tech-lead-architect:
      INTERRUPT normal workflow
      HANDLE_ESCALATION(escalation_payload)
      DO NOT proceed until escalation is resolved
    spec_path = agent_output_path
    UPDATE user_story.Traceability with new spec link

  spec = load_spec(spec_path)

  IF has_ui AND wireframe_path is still missing:
    PRESENT to user: "This UI story has no wireframe in Traceability and none was found in docs/wireframes/."
    ASK user to provide a wireframe path or confirm proceeding from UI Description only
    IF user provides wireframe_path:
      wireframe = load_wireframe(wireframe_path)
    ELSE IF user defers:
      wireframe = null
    ELSE:
      PAUSE workflow

  ELSE IF wireframe_path is not null:
    wireframe = load_wireframe(wireframe_path)

  IF persona_path is null AND user_story.UI_Description references a persona:
    persona_path = extract_persona_path(user_story.UI_Description)
  IF journey_path is null AND user_story.UI_Description references a journey:
    journey_path = extract_journey_path(user_story.UI_Description)
  IF persona_path: persona = load_document(persona_path)
  IF journey_path: journey = load_document(journey_path)

  VALIDATE spec:
    - Spec has Overview, Schema, API Contract, Behaviour Notes
    - Spec links to this user story (or was just created from it)
  IF invalid:
    REPORT to user what is missing
    STOP or delegate to tech-lead-architect to fix the spec


  // ── PHASE A: BACKEND IMPLEMENTATION ───────────────────────────────────────

  // Run the TDD cycle workflow — see .cursor/commands/tdd-cycle.md
  tdd_result = run_workflow(tdd-cycle, {
    feature_requirements: spec
  })

  ON escalation FROM tdd-cycle (or any sub-agent within it):
    INTERRUPT normal workflow
    HANDLE_ESCALATION(escalation_payload)
    RESUME from PHASE A only after resolution (e.g. spec updated, user clarified)

  IF tdd_result has failing tests or incomplete implementation:
    delegate_to(backend-api-engineer, {
      task: "Complete backend implementation so it strictly satisfies the spec; fix any failing spec-related tests",
      input: spec,
      context: "TDD cycle has produced tests and partial implementation; align and complete"
    })
    ON escalation FROM backend-api-engineer:
      INTERRUPT normal workflow
      HANDLE_ESCALATION(escalation_payload)
      RESUME from PHASE A only after resolution

  ASSERT backend tests pass and implementation matches spec
  IF not:
    LOOP: delegate to backend-api-engineer or re-run tdd-cycle until green and spec-compliant


  // ── STEP A2: UPDATE SPEC TEST COVERAGE TABLE ─────────────────────────────

  COLLECT test_files and test_methods created or modified during PHASE A
  FOR EACH acceptance criterion in user_story (AC01, AC02, …) or spec behaviour notes:
    FIND the test method that verifies this criterion
    UPDATE the spec's Test Coverage table with: scenario name/ID, test file path, test method name
  IF a criterion has no corresponding test:
    MARK it with "—" in the test columns so the gap is visible
  WRITE updated spec to disk


  // ── STEP A3: DATA WAREHOUSE / EVENT CATALOG (conditional) ───────────────

  IF spec defines or references domain events (e.g. "Events" section, or event catalog entries in docs/events):
    FOR EACH event type referenced in spec:
      delegate_to(data-analytics-agent, {
        task: "Create or update event catalog entry and data warehouse schema/mappings/load logic for this event",
        input: spec, event_catalog_entry_or_name, user_story
      })
      ON escalation FROM data-analytics-agent:
        INTERRUPT normal workflow
        HANDLE_ESCALATION(escalation_payload)
        RESUME from Step A3 only after resolution
  ELSE:
    SKIP Step A3


  // ── PHASE B: FRONTEND IMPLEMENTATION (conditional) ────────────────────────

  IF NOT has_ui:
    SKIP PHASE B — API-only story; no Blazor work in scope
  ELSE:

    delegate_to(frontend-blazor, {
      task: "Implement or update Blazor UI so that the user story acceptance criteria are satisfied, using the API contract from the spec. Use the wireframe for page layout, routes, component inventory, interactions, and API mappings. Use the UI Description for behavior and states. Use the persona for context on who the user is and what they expect. Use the journey to understand where this page sits in the flow and what pages link to and from it.",
      input: user_story, spec, wireframe, persona, journey
    })

    ON escalation FROM frontend-blazor:
      INTERRUPT normal workflow
      HANDLE_ESCALATION(escalation_payload)
      RESUME from PHASE B only after resolution

    ASSERT frontend builds and runs; acceptance scenarios from user story are satisfiable
    IF not:
      RE-DELEGATE to frontend-blazor with clarified scope or fixed spec/API


  // ── STEP C: COMPLETION ────────────────────────────────────────────────────

  RUN full test suite (backend + any integration tests)
  IF any failures:
    INVESTIGATE and either fix or HANDLE_ESCALATION as appropriate

  REPORT to user:
    - User story implemented: backend routes/behaviour/tests; frontend flows (if UI story)
    - Artifacts used: spec path, wireframe path (if any)
    - Event catalog / DW updates (if any)
    - Any deviations from spec or story and why
    - Suggested status updates (e.g. story New → Done; spec Draft → Implemented)
```

---

## Escalation Handling (Interrupt and Resolve)

When any agent returns an **escalation** (not a normal completion), **immediately interrupt** the normal workflow. Do not proceed to the next step until the escalation is addressed.

```
FUNCTION HANDLE_ESCALATION(escalation_payload):

  PARSE escalation_payload:
    - agent_name
    - escalation_type (e.g. spec_ambiguity, story_spec_conflict, missing_wireframe, payload_catalog_mismatch)
    - observation (what was found)
    - why_stopped (risk or blocker)
    - recommendation (what to do next)

  ROUTE by escalation_type and agent:

    // Tech-lead-architect escalations: story incomplete, cross-cutting change, trade-off, conflict
    IF agent == tech-lead-architect:
      PRESENT observation and recommendation to user
      ASK user to clarify story, make product decision, or resolve conflict
      IF user provides clarification or updated story/spec:
        RE-RUN tech-lead-architect with updated input
        RESUME workflow from Step 0 with new spec
      ELSE:
        PAUSE workflow; REPORT "Blocked on spec/story decision"

    // Backend-api-engineer escalations: spec ambiguity, contract conflict, missing dependency, test-spec mismatch
    IF agent == backend-api-engineer:
      IF escalation_type == "spec_ambiguity" OR "test_spec_mismatch":
        delegate_to(tech-lead-architect, {
          task: "Resolve ambiguity or align spec with tests; update spec document",
          input: spec, escalation_payload
        })
        ON success: UPDATE spec; RESUME from PHASE A
        ON escalation: PRESENT to user; PAUSE until resolved
      IF escalation_type == "contract_conflict" OR "missing_dependency":
        PRESENT observation and recommendation to user
        ASK user for decision (versioning, add dependency, or change scope)
        RESUME from PHASE A only after user decision is applied (e.g. spec updated)

    // Data-analytics-agent escalations: payload-catalog mismatch, key conflict, schema ownership, ambiguous semantics
    IF agent == data-analytics-agent:
      PRESENT observation and recommendation to user
      IF escalation implies spec or backend change:
        CONSIDER delegating to tech-lead-architect or backend-api-engineer to fix source of truth
      RESUME from Step A3 only after catalog/spec/backend are aligned

    // Frontend-blazor escalations: story-spec conflict, missing design rule, API divergent, a11y/compliance, UX doc issues
    IF agent == frontend-blazor:
      IF escalation_type == "story_spec_conflict" OR "api_divergent":
        delegate_to(tech-lead-architect, {
          task: "Align spec with story or document API deviation; update spec if needed",
          input: spec, user_story, escalation_payload
        })
        ON success: UPDATE spec; RESUME from PHASE B
      IF escalation_type == "missing_wireframe" OR "wireframe_story_conflict":
        PRESENT observation to user: which wireframe field conflicts with story or spec
        ASK user to update the wireframe, story Traceability link, or confirm wireframe as source of truth
        RESUME from PHASE B only after artifacts are aligned
      IF escalation_type == "missing_design_rule" OR "accessibility_compliance":
        PRESENT to user; ASK for design or product guidance
        RESUME from PHASE B only after guidance is provided or deferred
      IF escalation_type == "missing_ui_description":
        PRESENT to user: "This story needs a UI Description section (layout, components, interactions, states) before the frontend can be built."
        ASK user to add the section or delegate:
          delegate_to(tech-lead-architect, {
            task: "Add a UI Description section to this user story following the ui-description skill format. Reference the persona and journey.",
            input: user_story, persona, journey
          })
        ON success: RELOAD user_story; RESUME from PHASE B
      IF escalation_type == "persona_mismatch":
        PRESENT observation to user: which persona field conflicts and what the story expects
        ASK user to update the persona file or the story's persona reference
        RESUME from PHASE B only after persona and story are aligned
      IF escalation_type == "journey_gap":
        PRESENT observation to user: which journey steps are missing or inconsistent
        ASK user to update the journey or confirm the intended flow
        RESUME from PHASE B only after journey matches the page flow

    // TDD cycle (or its sub-agents) escalations: re-planning, ambiguous requirements, design conflict
    IF escalation originates from tdd-cycle or tdd-* agent:
      IF escalation suggests spec is wrong or incomplete:
        delegate_to(tech-lead-architect, {
          task: "Revise spec to resolve conflict or ambiguity identified during TDD",
          input: spec, escalation_payload
        })
        ON success: UPDATE spec; RESUME from PHASE A
      IF escalation suggests user clarification (e.g. ambiguous_requirements):
        ASK user one focused question; UPDATE test plan or spec with answer
        RESUME from PHASE A

  AFTER resolving escalation:
    RETRY the step that triggered the escalation (same agent, same task with any updated inputs)
  IF escalation cannot be resolved in this session:
    REPORT clearly: what is blocked, which agent escalated, what the user must decide or provide
    STOP workflow; do not leave implementation in a half-finished state without a clear handoff
```

---

## Agent and Workflow Reference

| Agent / Workflow        | Phase | When to invoke                          | Input                          | Output / Next step        |
|-------------------------|-------|-----------------------------------------|--------------------------------|----------------------------|
| **tech-lead-architect** | 0     | No spec linked to story; or to fix spec after escalation | User story or spec + escalation | Spec document              |
| **tdd-cycle**           | A     | Backend implementation (tests + code)   | Spec as feature requirements   | Tests + backend code       |
| **backend-api-engineer**| A     | After TDD to complete/align with spec   | Spec, current codebase         | Spec-compliant backend     |
| **data-analytics-agent**| A3    | Spec defines or references domain events | Spec, event catalog, user story | Catalog + schema + load    |
| **frontend-blazor**     | B     | UI story (UI Description or wireframe)  | User story, spec, wireframe, persona, journey | Blazor pages/components    |

---

## Rules for the Orchestrator

1. **Never implement yourself.** Delegate all spec writing, tests, backend code, DW work, and frontend code to the appropriate agent or workflow.
2. **Start from the user story.** Resolve the linked spec and wireframe from Traceability (with fallback search) before any implementation work.
3. **Backend before frontend.** Complete PHASE A (backend + test coverage + optional DW) before starting PHASE B. The frontend agent depends on a stable API contract from the spec and working backend endpoints.
4. **Skip frontend for API-only stories.** If the story has no UI Description and no wireframe, PHASE B is out of scope.
5. **Treat every escalation as a hard stop.** Interrupt the workflow, run `HANDLE_ESCALATION`, and resume only after the issue is resolved or explicitly deferred by the user.
6. **Resume from the interrupted step.** After resolving an escalation, retry the same step (same agent, updated inputs if any); do not skip ahead.
7. **Keep artifacts in sync.** If any agent updates the spec, story, or wireframe, reload them and use the updated versions for all subsequent steps.
8. **Confirm quality before completion.** Run the full test suite and assert backend (and frontend, if applicable) are consistent with the spec and user story before reporting done.
