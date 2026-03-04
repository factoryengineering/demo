# Implement Spec — Orchestrator Workflow

Use this command when the user wants to **implement a technical specification end-to-end**: backend (via TDD), optional data-warehouse work for domain events, and frontend. You are the orchestrator. You delegate to specialized agents and to the TDD cycle; you do not write spec, tests, or implementation yourself. When any agent escalates, **interrupt the normal workflow** and address the issue before continuing.

## Inputs

The user provides **one** of:

- **Spec path** — e.g. `docs/specs/SPEC002-Venue-Management.md`. Implementation proceeds from this spec.
- **User story path** — e.g. `docs/user-stories/US002-Venue-Management.md`. You must obtain or refine a spec first (delegate to tech-lead-architect).

If the input is ambiguous, ask exactly one clarifying question (e.g. "Do you have an existing spec, or should I derive one from the user story?").

---

## Orchestration Workflow (Pseudocode)

```
FUNCTION implement_spec(user_input):

  spec = null
  user_story = null


  // ── STEP 0: RESOLVE SPEC AND USER STORY ───────────────────────────────────

  IF user_input is a spec path:
    spec = load_spec(user_input)
    user_story = load_document(spec.linked_user_story)   // from spec header
    IF user_story is missing:
      ASK user for user story path or content
      UNTIL user_story is available, PAUSE

  ELSE IF user_input is a user story path or content:
    user_story = load_or_use(user_input)
    delegate_to(tech-lead-architect, {
      task: "Produce a complete technical specification for this user story",
      input: user_story,
      output: "docs/specs/SPECnnn-Short-Name.md"
    })
    ON escalation FROM tech-lead-architect:
      INTERRUPT normal workflow
      HANDLE_ESCALATION(escalation_payload)
      DO NOT proceed to Step 1 until escalation is resolved
    spec = load_spec(agent_output_path)

  VALIDATE spec:
    - Spec has Overview, Schema, API Contract, Behaviour Notes
    - Spec links to user story
  IF invalid:
    REPORT to user what is missing
    STOP or ASK user to provide a valid spec


  // ── STEP 1: BACKEND IMPLEMENTATION VIA TDD CYCLE ───────────────────────────

  tdd_result = run_workflow(tdd-cycle, {
    feature_requirements: spec   // full spec as input to TDD
  })

  ON escalation FROM tdd-cycle (or any sub-agent within it):
    INTERRUPT normal workflow
    HANDLE_ESCALATION(escalation_payload)
    RESUME from Step 1 only after resolution (e.g. spec updated, user clarified)

  IF tdd_result has failing tests or incomplete implementation:
    delegate_to(backend-api-engineer, {
      task: "Complete backend implementation so it strictly satisfies the spec; fix any failing spec-related tests",
      input: spec,
      context: "TDD cycle has produced tests and partial implementation; align and complete"
    })
    ON escalation FROM backend-api-engineer:
      INTERRUPT normal workflow
      HANDLE_ESCALATION(escalation_payload)
      RESUME from Step 1 only after resolution

  ASSERT backend tests pass and implementation matches spec
  IF not:
    LOOP: delegate to backend-api-engineer or re-run tdd-cycle until green and spec-compliant


  // ── STEP 2: DATA WAREHOUSE / EVENT CATALOG (conditional) ──────────────────

  IF spec defines or references domain events (e.g. "Events" section, or event catalog entries in docs/events):
    FOR EACH event type referenced in spec:
      delegate_to(data-analytics-agent, {
        task: "Create or update event catalog entry and data warehouse schema/mappings/load logic for this event",
        input: spec, event_catalog_entry_or_name, user_story
      })
      ON escalation FROM data-analytics-agent:
        INTERRUPT normal workflow
        HANDLE_ESCALATION(escalation_payload)
        RESUME from Step 2 only after resolution (e.g. spec or catalog corrected)
  ELSE:
    SKIP Step 2


  // ── STEP 3: FRONTEND IMPLEMENTATION ────────────────────────────────────────

  delegate_to(frontend-blazor, {
    task: "Implement or update Blazor UI so that the user story acceptance criteria are satisfied, using the API contract from the spec",
    input: user_story, spec
  })

  ON escalation FROM frontend-blazor:
    INTERRUPT normal workflow
    HANDLE_ESCALATION(escalation_payload)
    RESUME from Step 3 only after resolution

  ASSERT frontend builds and runs; acceptance scenarios from user story are satisfiable
  IF not:
    RE-DELEGATE to frontend-blazor with clarified scope or fixed spec/API


  // ── STEP 4: COMPLETION ────────────────────────────────────────────────────

  RUN full test suite (backend + any integration tests)
  IF any failures:
    INVESTIGATE and either fix or HANDLE_ESCALATION as appropriate

  REPORT to user:
    - Spec implemented: routes, behaviour, tests, frontend flows
    - Event catalog / DW updates (if any)
    - Any deviations from original spec and why
    - Suggested spec status update (e.g. Draft → Implemented)
```

---

## Escalation Handling (Interrupt and Resolve)

When any agent returns an **escalation** (not a normal completion), **immediately interrupt** the normal workflow. Do not proceed to the next step until the escalation is addressed.

```
FUNCTION HANDLE_ESCALATION(escalation_payload):

  PARSE escalation_payload:
    - agent_name
    - escalation_type (e.g. spec_ambiguity, story_spec_conflict, payload_catalog_mismatch)
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
        ON success: UPDATE spec; RESUME from Step 1
        ON escalation: PRESENT to user; PAUSE until resolved
      IF escalation_type == "contract_conflict" OR "missing_dependency":
        PRESENT observation and recommendation to user
        ASK user for decision (versioning, add dependency, or change scope)
        RESUME from Step 1 only after user decision is applied (e.g. spec updated)

    // Data-analytics-agent escalations: payload-catalog mismatch, key conflict, schema ownership, ambiguous semantics
    IF agent == data-analytics-agent:
      PRESENT observation and recommendation to user
      IF escalation implies spec or backend change:
        CONSIDER delegating to tech-lead-architect or backend-api-engineer to fix source of truth
      RESUME from Step 2 only after catalog/spec/backend are aligned

    // Frontend-blazor escalations: story-spec conflict, missing design rule, API divergent, a11y/compliance
    IF agent == frontend-blazor:
      IF escalation_type == "story_spec_conflict" OR "api_divergent":
        delegate_to(tech-lead-architect, {
          task: "Align spec with story or document API deviation; update spec if needed",
          input: spec, user_story, escalation_payload
        })
        ON success: UPDATE spec; RESUME from Step 3
      IF escalation_type == "missing_design_rule" OR "accessibility_compliance":
        PRESENT to user; ASK for design or product guidance
        RESUME from Step 3 only after guidance is provided or deferred

    // TDD cycle (or its sub-agents) escalations: re-planning, ambiguous requirements, design conflict
    IF escalation originates from tdd-cycle or tdd-* agent:
      IF escalation suggests spec is wrong or incomplete:
        delegate_to(tech-lead-architect, {
          task: "Revise spec to resolve conflict or ambiguity identified during TDD",
          input: spec, escalation_payload
        })
        ON success: UPDATE spec; RESUME from Step 1
      IF escalation suggests user clarification (e.g. ambiguous_requirements):
        ASK user one focused question; UPDATE test plan or spec with answer
        RESUME from Step 1

  AFTER resolving escalation:
    RETRY the step that triggered the escalation (same agent, same task with any updated inputs)
  IF escalation cannot be resolved in this session:
    REPORT clearly: what is blocked, which agent escalated, what the user must decide or provide
    STOP workflow; do not leave implementation in a half-finished state without a clear handoff
```

---

## Agent and Workflow Reference

| Agent / Workflow        | When to invoke                          | Input                          | Output / Next step        |
|-------------------------|-----------------------------------------|--------------------------------|----------------------------|
| **tech-lead-architect** | No spec yet (only user story); or to fix spec after escalation | User story or spec + escalation | Spec document              |
| **tdd-cycle**           | Backend implementation (tests + code)   | Spec as feature requirements   | Tests + backend code       |
| **backend-api-engineer**| After TDD to complete/align with spec   | Spec, current codebase         | Spec-compliant backend     |
| **data-analytics-agent**| Spec defines or references domain events | Spec, event catalog, user story | Catalog + schema + load    |
| **frontend-blazor**     | UI for the user story                   | User story, spec               | Blazor pages/components    |

---

## Rules for the Orchestrator

1. **Never implement yourself.** Delegate all spec writing, tests, backend code, DW work, and frontend code to the appropriate agent or workflow.
2. **Treat every escalation as a hard stop.** Interrupt the workflow, run `HANDLE_ESCALATION`, and resume only after the issue is resolved or explicitly deferred by the user.
3. **Resume from the interrupted step.** After resolving an escalation, retry the same step (same agent, updated inputs if any); do not skip ahead.
4. **Keep spec and story in sync.** If tech-lead-architect or any agent updates the spec, reload it and use the updated spec for all subsequent steps.
5. **Confirm quality before completion.** Run the full test suite and assert backend and frontend are consistent with the spec and user story before reporting done.
