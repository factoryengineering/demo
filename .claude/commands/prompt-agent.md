---
description: Write a structured agent prompt from a brief description, following the Role / Task / Context / Constraints / Escalation pattern.
argument-hint: <brief description of the agent's purpose>
---

# Write an Agent Prompt

Generate a concise, structured agent prompt from the user's brief description. The prompt is a seed — it will later be expanded into a full agent definition with workflow steps, output format, quality checks, and memory instructions.

## User Input

The user provides a **brief description** of the agent they want — a sentence or two covering what the agent does and where it fits. If the description is too vague to identify the agent's single responsibility, ask one clarifying question before proceeding.

## Prompt Structure

Produce a prompt with exactly these sections, in this order:

### 1. Title

A short, bold name. Two to four words. No punctuation.

### 2. Role

One sentence. "You are a senior [specialty] specializing in [specific capability]."

Constraints on Role:
- Must name a concrete specialty, not a generic title like "software engineer."
- Must state what the agent is unusually good at — the reason an orchestrator would choose this agent over a generalist.

### 3. Task

One to two sentences. What the agent receives as input, what it produces as output, and the quality bar — "exactly one," "minimal," "exhaustive," etc.

Constraints on Task:
- Must be specific enough that the agent can tell when it is done.
- Must include a quantity or completeness qualifier.

### 4. Context

Two to three sentences. Where this agent sits in a larger workflow. Who hands it work, who consumes its output, and what happened before it was invoked.

Constraints on Context:
- Must name the upstream and downstream agents or roles, even if they are human.
- Must describe what state the world is in when this agent is invoked (e.g., "a failing test already exists," "the test suite is green," "no code has been written yet").

### 5. Constraints

A paragraph of imperative sentences. What the agent must always do, what it must never do, and the verification steps it must perform before reporting done.

Constraints on Constraints:
- Must include at least one "always" rule and one "never" rule.
- Must include a run/verify step — the agent should confirm its own work before reporting.
- End with: "Append what you learn about this codebase to your memory file."

### 6. Escalation

A paragraph introducing the section, followed by a bulleted list. Each bullet is a bolded condition name, a colon, then a one-sentence description of when it applies.

Constraints on Escalation:
- Must include at least three escalation conditions.
- Conditions must be specific to this agent's domain — not generic problems like "an error occurred."
- Each condition must be something this agent should NOT attempt to fix itself.
- End the section with: "In each case, do not fix the problem yourself. Report what you observed, why you stopped, and what you recommend the orchestrator do next."

## Quality Checks

Before presenting the prompt, verify:

- [ ] Each section is present and in order.
- [ ] Role names a concrete specialty, not a generic title.
- [ ] Task has a clear done-condition and a quantity qualifier.
- [ ] Context names upstream and downstream agents.
- [ ] Constraints include at least one "always," one "never," and a verify step.
- [ ] Escalation has at least three domain-specific conditions.
- [ ] The agent has exactly one responsibility — it does not overlap with adjacent agents.
- [ ] No section contains implementation details, code samples, or tool names.
- [ ] The entire prompt fits in a single screen (~30 lines). It is a seed, not a specification.

## Style

- Use bold for section headers but not for emphasis within sentences.
- Write in imperative voice within Constraints and Escalation.
- Use em dashes for parenthetical clauses, not parentheses.
- Do not use bullet points in Role, Task, Context, or Constraints — only in Escalation.
- Do not explain the rationale for the prompt. Output only the prompt itself.

## Output

Return the prompt as a single markdown block. Nothing before it, nothing after it.
