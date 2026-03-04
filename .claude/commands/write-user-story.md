# Write User Story

Use this command when the user wants to create a new user story. The user will provide a brief description of the story. Your job is to gather enough detail through clarification before writing the document.

## Workflow

1. **Start from the user's brief description.** Do not write the story yet. Acknowledge the description and begin clarification.

2. **Ask exactly one clarifying question at a time.** Wait for the user's answer before asking the next. Do not batch multiple questions in one message.

3. **Use questions to explore:**
   - **Intent:** Who is the user/persona (role)? What is the concrete behavior and the business or user goal?
   - **Entry point & flow:** How does the user reach this feature (e.g. from which page, link, or action)?
   - **Happy path:** What does success look like step-by-step? What data is shown or changed?
   - **Error paths:** What can go wrong (API failures, validation, auth, network)? What should the user see or be able to do?
   - **Edge cases:** Empty lists, single item, special characters, large data, permissions, or other boundary conditions.

4. **Stop asking when you have enough** to write a complete story: clear "As a / I want / So that", and acceptance criteria covering happy path, at least one error path, and relevant edge cases.

5. **Write the user story** in `docs/user-stories/` using this format:
   - Filename: `USXXX-Short-Title.md` (pick the next available US number by checking existing files in `docs/user-stories/`).
   - **Title:** `# USXXX — Title`
   - **Status:** `New`
   - **Description:** One sentence in the form: "As a **{role}**, I want **{behavior}**, so that **{goal}**." Add a short context paragraph if it helps (e.g. which page or API is involved).
   - **Acceptance criteria:** Gherkin scenarios in fenced code blocks, grouped by theme (e.g. "Navigate to X", "Happy path", "Error paths", "Edge cases"). Use `Feature:` and `Scenario:` with clear `Given` / `When` / `Then` steps.

6. **After writing,** offer to add or adjust scenarios if the user wants changes.

## Rules

- Ask only **one** clarifying question per message until you have enough to write.
- Prefer open-ended questions (e.g. "What should happen when the API returns an error?") over yes/no when exploring scenarios and edge cases.
- If the user's description already answers a topic (e.g. role or entry point), skip that question and ask the next.
- Reference existing code or docs (e.g. controller names, page paths) when the user has pointed to them, so the story stays aligned with the codebase.
