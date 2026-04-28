# Agent Memory Retrospective

Use this command to review agent memory files for stale information, lessons learned, and actionable improvements. Present findings as recommendations and wait for confirmation before making changes.

## Inputs

No arguments required. The retro covers all agent memory directories in `.claude/agent-memory/`.

---

## Workflow

### 1. Inventory agent memory

Read every file in `.claude/agent-memory/*/` — both `MEMORY.md` indexes and any topic files (e.g. `patterns.md`, `debugging.md`). List the agents that have memory and note which agents have no memory directory yet.

### 2. Check for stale facts

For each factual claim in agent memory — endpoint status, file paths, class names, project structure, implementation status — verify it against the current codebase:

- If memory says a file exists, check that it does.
- If memory says an endpoint is "not yet implemented," check git log and the controller.
- If memory names a function, class, or pattern, grep for it.
- If memory describes a convention, check whether the skill docs agree.

Flag every stale or incorrect entry with what the memory says vs. what is actually true.

### 3. Check for cross-agent inconsistencies

Compare claims across agents. If two agents describe the same thing differently — naming conventions, test patterns, endpoint routes, project structure — flag the inconsistency and identify which version is correct.

### 4. Identify lessons that should be promoted

Look for knowledge that is trapped in one agent's memory but would benefit all agents. Common examples:

- A testing caveat discovered by `tdd-test-writer` that should live in the `unit-testing` skill.
- A frontend pattern discovered by `frontend-blazor` that should live in the `blazor-ui` skill.
- An architecture observation that should be in a shared skill or CLAUDE.md rather than one agent's memory.

### 5. Check alignment with skills and workflows

For each agent that has memory, read the corresponding agent definition (`.claude/agents/<agent-name>.md`) and any skills it references. Flag:

- Memory entries that contradict skill documentation.
- Workflow inputs or outputs that have changed since the memory was written.
- Agent capabilities or escalation types that are documented in the agent definition but not reflected in the agent's memory (or vice versa).

### 6. Present recommendations

Compile findings into a numbered list of recommendations, grouped by category:

```
## Retro Findings

### Stale facts
1. [agent] — memory says X, but current state is Y. Recommend: update memory.

### Cross-agent inconsistencies
2. [agent-a] says X, [agent-b] says Y. The correct answer is Z. Recommend: update [agent].

### Lessons to promote
3. [agent] discovered X. This belongs in [skill] so all agents benefit. Recommend: add to skill, optionally remove from agent memory.

### Skill/workflow misalignment
4. [agent] memory references X, but the [skill/workflow] now says Y. Recommend: update memory.
```

**Present this list to the user. Do not make any changes yet.**

### 7. Confirm and apply

Ask the user which recommendations to apply. They may approve all, select a subset, or modify the recommendations.

For each approved recommendation:
- Make the change (edit agent memory, update skill, etc.).
- Report what was changed.

For each rejected recommendation:
- Note it and move on.

### 8. Summary

After all approved changes are applied, report:
- Number of stale entries fixed.
- Number of lessons promoted to skills.
- Number of inconsistencies resolved.
- Any remaining issues the user deferred.

---

## Rules

- **Never change agent memory, skills, or workflows without presenting recommendations first.** The user must confirm before any edits.
- **Verify before flagging.** Do not report a memory entry as stale based on assumption — check the codebase, git log, or skill docs.
- **Be specific.** Every recommendation must name the file, the line or section, what is wrong, and what the fix is.
- **Keep it concise.** The retro is a health check, not an audit. Focus on entries that are wrong, inconsistent, or trapped in one agent when they should be shared.
