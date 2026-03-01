# Build Your Own Software Factory: Customized AI-Assisted Development
## Talk Script — Version 2

**Duration:** 60 minutes
**Acts 1–2:** Cursor
**Act 3:** Claude Code
**Structure:** Demo-first, three acts, personal story arc in Act 2

---

## DEMO ENVIRONMENT CHECKLIST

Before the talk:

**Repository**
- [ ] Two branches ready: `main` (no skills) and `with-skill` (skills installed)
- [ ] `.claude/skills/` populated on `with-skill` branch with standalone skill for Act 1
- [ ] Symlinks committed: `.cursor/commands/ → ../.claude/commands/`
- [ ] `openskills` available globally: `npm install -g openskills` or npx ready
- [ ] All commands live in `.claude/commands/`: `create-catalog-entry`, `write-migration`, `write-handler`

**Event catalog artifacts (Act 2)**
- [ ] A real event catalog entry document — not a stub
- [ ] A real user story document to run `create-catalog-entry` against
- [ ] Pre-run output for `write-migration` and `write-handler` ready to show
- [ ] ERD generation: `mmdc` installed and on PATH, ERD skill ready, SVG output pre-generated as fallback

**Claude Code (Act 3)**
- [ ] TDD agents defined in `.claude/agents/`: `tdd-planner`, `tdd-test-writer`, `tdd-code-writer`, `tdd-refactor`
- [ ] `tdd-cycle.md` in `.claude/commands/`
- [ ] Specification document ready to run the TDD cycle against
- [ ] Test data helpers skill visible to show in the walk-through

**Logistics**
- [ ] Cursor open, font size readable from back of room
- [ ] Claude Code open, ready to switch to
- [ ] Browser tab: `https://factoryengineering.dev` — minimized, ready
- [ ] Slides on a second display or separate machine

---

## OPENING (4 minutes)

### [SLIDE: Title — "Build Your Own Software Factory"]

---

**SCRIPT:**

Let me start with something you've probably already felt.

You're using an AI coding assistant. You ask it to write something for your project — a class, a handler, a migration. And it produces code that is... fine. It compiles. It's not wrong. But it's not quite how your team writes things. The naming is slightly off. The structure doesn't match your patterns. It made a choice that any developer on your team would immediately know isn't how you do it here.

You fix it. You move on. But the next time you ask, it makes the same kinds of choices again. Because it doesn't remember. And it doesn't know your project.

That's not a bug in the tool. It's a fundamental characteristic of how these tools work. The model was trained on the average of the internet. It knows an enormous amount about software in general. But it knows nothing specific about your domain, your stack, your architecture, your team's conventions — unless you build a system to give it that knowledge.

That system is what I call a Software Factory. And tonight I'm going to show you how to build one.

Not by installing a template. Not by downloading someone else's generic setup. By engineering it yourself, starting from your actual project and your actual team.

I'm going to do this in three acts. Skills. Commands. Agents and workflows. Each act builds on the last. Each one makes the next possible.

Let's go.

---

## ACT 1: SKILLS (15 minutes)

### [SLIDE: "Act 1 — Skills"]
### [SLIDE: "The AI produces the average. Skills shift the average toward your team."]

---

**SCRIPT:**

The first building block is a skill. A skill is how you teach the AI your team's standards for a particular kind of work.

Let me show you what that looks like in practice.

### [DEMO 1A: Generic output — `main` branch, Cursor]

[In Cursor, confirm you're on `main`. Run:]
```
Write a failing unit test for AC05 based on @docs/specs/SPEC001-Venue-Management.md
```

[Let the output appear. Read through it briefly with the audience. Point to two or three specific things that are technically fine but wrong for your project — a pattern it used, a structural choice, a naming convention.]

That's what the AI knows about this problem from the internet. And honestly, it's not bad. If you didn't have a codebase with established patterns, this might be fine.

But we do have established patterns. And this doesn't follow them.

[Switch to `with-skill` branch.]

Same prompt. Different branch.

[Run the same prompt:]
```
Write a failing unit test for AC05 based on @docs/specs/SPEC001-Venue-Management.md
```

[Let the output appear. Point to the same spots you called out before. Show how they're now handled correctly.]

Same model. Same words. The difference is that on this branch, the AI read a skill before responding. That skill encoded how we write this kind of code. And the AI followed it.

Let me show you what that skill looks like.

### [DEMO 1B: Open the skill]

[Navigate to `.claude/skills/[skill-name]/SKILL.md`. Open it. Walk through it.]

This is all it is. A directory with a markdown file. The frontmatter at the top has two fields that matter: the `name` and the `description`.

[Point to the description.]

This is what the AI reads to decide whether to load the skill. It reads the description of every skill in `.claude/skills/` and asks: is this relevant to what the user just asked? If yes, it loads the skill into context before responding. If no, it stays dormant.

So write the description as a condition, not a label. Not "inventory handler skill." Write: "Use when writing or reviewing inventory update handlers in this project." Give it a trigger.

The body of the skill is your standards. Your naming conventions. The patterns you use. The things you don't do. Examples of what right looks like. References to templates.

This is not generic best practice. This is your team's opinion, encoded.

### [SLIDE: "You don't write a skill. You do the task first."]

Now — how do you actually create one of these?

Here's the key move: you don't write a skill from scratch. You do the task first. You work with the AI until the output is exactly what you want. Then you ask the AI to codify what you just established.

Let me show you.

### [DEMO 1C: Install skill-creator, generate output, create a skill]

[Run:]
```bash
npx openskills install anthropics/skills
```

[Explain briefly while it runs:]

`skill-creator` is a published skill from Anthropic. Once it's installed in `.claude/skills/`, the AI knows how to create skills that follow the standard structure. I'm not building the skill directory by hand — I'm using the same iterative process that produced every other skill in the factory.

[Now give the AI a task. Generate output. Deliberately iterate — push back on two or three specific things. Name the conventions you want. Adjust until the output is exactly right. Be specific and don't rush this — the audience needs to see you enforcing quality.]

That's the output I want. Not approximately right. Exactly right.

[Now prompt the AI:]

"Create a skill that captures the standards and opinions I just expressed. Use skill-creator. Store it in `.claude/skills/`."

[Let the AI generate the skill. Open it.]

Look at what it produced. It wrote the frontmatter, captured the standards I enforced during that iteration, organized them into a usable reference. I didn't write this — I did the task and asked the AI to document what it learned.

### [SLIDE: Skill creation loop]

```
Do the task → Get the output right →
Codify as a skill → Run on the next task →
Refine when it misses → Commit
```

Every skill in the factory was created this way. You don't design them in advance. You earn them through iteration.

### [SLIDE: "factoryengineering.dev/skills"]

Full reference for skills — structure, description patterns, how to install across multiple IDEs — at `factoryengineering.dev`.

---

## ACT 2: COMMANDS (18 minutes)

### [SLIDE: "Act 2 — Commands"]
### [SLIDE: "Skills encode knowledge. Commands encode work."]

---

**SCRIPT:**

Skills teach the AI how to do something. Commands tell it what to do with a specific artifact.

The key pattern is three words: **slash-command at-artifact**.

```
/write-handler @events/catalog/order-placed.md
```

The command provides the instructions. The artifact provides the target. The AI does the work.

Let me show you the domain I've been building this factory in.

### [SLIDE: Event-driven architecture — the domain]

The system I was working on is event-driven. When something happens in the domain — an order is placed, inventory is updated, a payment is processed — that event gets published. Other parts of the system subscribe to it and handle it.

For every event, we needed three things: a **catalog entry** — the authoritative description of the event, its schema, its semantics, its contract. A **database migration** — the schema changes needed to handle this event. And a **handler** — the code that processes the event when it arrives.

These three artifacts have a direct structural relationship. The catalog entry drives the migration. The migration and the catalog entry together drive the handler.

When I realized that, I realized I could encode the entire pipeline as commands.

### [DEMO 2A: Show the catalog entry]

[Open a real event catalog entry document.]

This is what a catalog entry looks like in our project. Event name, schema, semantics, validation rules, version history. Specific structure. Specific conventions — which are encoded in a skill that loads whenever you're working with catalog entries.

### [DEMO 2B: Run `/write-migration`]

[In Cursor, run:]
```
/write-migration @events/catalog/order-placed.md
```

[Let it run. Walk through the output.]

The command read the catalog entry, understood the schema, and produced a migration that creates the right tables, columns, and indexes — following our migration conventions, which live in a skill.

### [DEMO 2C: Run `/write-handler`]

[Run:]
```
/write-handler @events/catalog/order-placed.md
```

[Walk through the output.]

Same source artifact. Different command. Now it produced the handler — our patterns, our structure, our conventions.

One catalog entry. Two commands. Two production-ready artifacts.

### [SLIDE: "Then I got the team Cursor licenses."]

Here's where the story gets interesting.

I built this factory alone. I'd been running it for a few weeks. And I started to notice something: the backlog was full of stories that all followed the same shape. New event. Migration. Handler. New event. Migration. Handler.

The factory could handle all of it. Every story that fit that pattern was essentially solved — as long as the catalog entry existed.

I made the case to my manager to get Cursor licenses for the team. Showed the before and after. Showed the output. Made the business case. Got the licenses approved.

Then I sat down with the team and showed them the commands.

### [SLIDE: "Watching them run the commands for the first time."]

That moment — watching a developer on my team run `/write-migration` for the first time and see what came out — that's the moment I keep coming back to.

They didn't have to study our migration conventions. They didn't have to read previous handlers to understand the pattern. They ran the command. The factory already knew. The output was right.

A story that would have taken five days took one day. And it wasn't a fluke. Most of the backlog looked like that. The factory didn't just accelerate one story. It changed the shape of the whole project.

### [SLIDE: "All I had to do was keep the event catalog filled."]

That's when I realized I had created a different problem for myself.

I was producing the catalog entries. The team was consuming them. As long as I kept the catalog filled, the team could keep moving. But I had become the bottleneck. The factory made the team fast, and I was the thing slowing it down.

So I built one more command.

### [DEMO 2D: Run `/create-catalog-entry`]

[Open a user story document. In Cursor, run:]
```
/create-catalog-entry @stories/order-fulfillment.md
```

[Walk through what it does:]

This command reads a user story and produces a catalog entry. It uses two skills: one that knows how to load a user story from Azure DevOps using a personal access token, and one that defines the structure and conventions of a catalog entry.

It does the domain reasoning — what event does this story imply, what's the schema, what are the semantics. I iterate on the mapping a few times to get it right. Then I commit the command.

### [DEMO 2E: ERD generation]

There's one more piece I want to show you.

[Show or describe the ERD command.]

After the catalog entry is generated, there's a command that reads the data model section and generates an entity-relationship diagram. It uses `mmdc` — the Mermaid CLI — to render the diagram as an SVG and inserts it directly into the markdown.

[Show pre-generated output with the SVG embedded in the catalog entry.]

Now the catalog entry isn't just text. It's a living technical document with a rendered data model. Any developer can open it and immediately understand the structure.

### [SLIDE: "The team is now generating their own catalog entries."]

The last part of this story is the best part.

The team is now running `/create-catalog-entry` themselves. They're not waiting on me. They take a user story, generate the catalog entry, iterate on it until it's right, then run the migration and handler commands. End to end. One developer. One day.

I am no longer the bottleneck. The factory eliminated me from the critical path.

That's what commands do at scale. They don't just accelerate individual tasks. They change who can do what — and how a team organizes around the work.

### [SLIDE: "factoryengineering.dev/commands"]

---

## TRANSITION TO ACT 3 (2 minutes)

### [SLIDE: "The same commands. A different IDE."]

---

**SCRIPT:**

Before I move to the third act, I want to show you something quickly.

[Open the terminal. Navigate to the repo root. Run:]
```bash
ls -la .cursor/commands
```

[Show that it's a symlink pointing to `../.claude/commands`.]

The commands I just demonstrated in Cursor — `write-migration`, `write-handler`, `create-catalog-entry` — they live in `.claude/commands/`. The `.cursor/commands/` folder is a symlink that points there.

This means the same command files work in Claude Code. In Kilo Code. In Windsurf. One canonical location. Every IDE reads from it.

[Switch to Claude Code.]

For Act 3, I'm switching to Claude Code. The commands are the same files — nothing changes there. But what I want to show you next requires something Cursor doesn't have: a true orchestration layer. An agent that reads a workflow document, delegates to specialist sub-agents, evaluates results, and decides what to do next based on what it discovers.

For agents and workflows, you need Claude Code or Kilo Code. Everything else I showed you tonight works everywhere.

---

## ACT 3: AGENTS AND WORKFLOWS (18 minutes)

### [SLIDE: "Act 3 — Agents and Workflows"]
### [SLIDE: "Skills encode knowledge. Commands encode work. Agents accumulate memory. Workflows orchestrate agents."]

---

**SCRIPT:**

An agent, in the factory engineering sense, is not the AI assistant itself. It's a specific defined role — a configured AI instance with three properties.

A defined role. A fresh context for each invocation — it starts clean every time, no baggage. And persistent memory: it reads a markdown file at the start of a session, does its work, writes back what it learned at the end. The next time it runs, it reads that accumulated knowledge and builds on it.

That third property is what makes an agent different from a command. A command is stateless. An agent accumulates.

### [SLIDE: "RTCC — Role, Task, Context, Constraints"]

When I build an agent, I follow a pattern I call RTCC:

- **Role** — who is this agent?
- **Task** — what specific thing is it responsible for?
- **Context** — where does it operate in the larger system?
- **Constraints** — what must it always do, and what must it never do?

This forces precision. Vague agents produce vague output. If you can't state the role in one sentence, the agent's scope is too broad. If you can't state the task in one sentence, it will try to do too much.

Let me build one live.

### [DEMO 3A: Build the TDD test-writer agent in Claude Code]

[In Claude Code, run `/agents`. Select "Create new agent" → "Project (.claude/agents/)" → "Generate with Claude (recommended)".]

[Enter this description, reading it aloud as you type:]

```
TDD Test Writer.

Role: You are a senior TDD engineer specializing in
precise, single-responsibility tests.

Task: You will be given exactly one scenario and one set
of acceptance criteria. Write exactly one failing test
for that scenario. Nothing more.

Context: You are one agent in a TDD cycle. A planner
will have already produced a test plan. An orchestrator
will delegate individual scenarios to you. A code writer
will take your failing test and make it pass.

Constraints: Run the test after writing it. Confirm it
fails for the expected reason — an assertion failure or
a not-implemented stub, not a compile error. Never write
more than one test. Never write implementation code.
Append what you learn about this codebase to your memory
file.
```

[Let Claude Code generate the agent. Open the resulting file. Walk through it.]

Notice the memory field. This tells the agent where to read from and write to at the start and end of each session. Right now that file is empty. The first time this agent runs, it discovers your test framework, your file structure, your assertion patterns. It writes that to memory. Every run after builds on it.

### [DEMO 3B: Run the agent on a single scenario]

[Invoke the agent:]
```
@"tdd-test-writer (agent)" write a failing test for:
when an order is placed for an out-of-stock item,
the handler should reject it with an InsufficientInventory error
```

[Let it run. Walk through the output — the test, the confirmation that it ran and failed for the right reason.]

One scenario. One test. Exactly what we asked for.

Now look at the memory file.

[Open the memory file.]

It discovered things about our codebase and recorded them. The next developer who runs this agent starts from that accumulated knowledge — not from zero.

That's the difference between a command and an agent.

### [SLIDE: "Now put four agents together."]

One agent is useful. Four agents coordinated by an orchestrator is a factory.

### [DEMO 3C: Walk through the TDD cycle workflow]

[Open `.claude/commands/tdd-cycle.md`. Walk through the structure, reading key sections aloud.]

This is a workflow document. It lives in `.claude/commands/` — same folder as everything else, available in every IDE via the symlink. But it's not a command. A command tells one agent what to do. This document tells an orchestrator how to coordinate four agents.

[Walk through each phase:]

Phase 1: the orchestrator delegates to `tdd-planner`. It comes back with an ordered test plan — a backlog for this specification.

Phase 2: the TDD loop. For each test in the plan — delegate to `tdd-test-writer`, get back a failing test, confirm it fails for the right reason, delegate to `tdd-code-writer`, make it pass, confirm all previous tests still pass, evaluate whether refactoring is warranted, then: does the plan still make sense given what we just learned? If not, delegate back to `tdd-planner` with current state.

Phase 3: full test suite, type check, summary of deviations from the original plan and why.

[Point to the Orchestration Rules section:]

These rules are what make this orchestration rather than a checklist. The orchestrator never writes a test itself. Never writes implementation. The plan can change mid-loop. The orchestrator decides dynamically — based on what it discovers during execution — whether to re-delegate, loop again, or escalate to the user.

### [DEMO 3D: Show the test data helpers skill]

[Open the test data helpers skill.]

The agents don't operate in a vacuum. They use skills. This skill encodes the patterns for building test fixtures in our project. When `tdd-test-writer` runs, it reads this skill. The tests it produces use our test data patterns.

Skills feed agents. Agents are coordinated by workflows. The whole factory is connected.

### [DEMO 3E: Set the TDD cycle running]

[Run:]
```
/tdd-cycle @docs/specs/order-fulfillment.md
```

[Let the orchestrator read the spec and delegate to `tdd-planner`. Show the plan.]

[Let it proceed to the first test iteration. Show the delegation to `tdd-test-writer`, the failing test, and the delegation to `tdd-code-writer`.]

[Stop it before it runs too long. The point is made.]

What you're watching is the factory running. No human in the loop. The orchestrator is making decisions based on what it discovers — adjusting the plan, routing work, evaluating results before proceeding.

That's not a script. That's not a checklist. That's a system.

### [SLIDE: "factoryengineering.dev/workflows"]

---

## CLOSING (3 minutes)

### [SLIDE: "You don't install a Software Factory. You engineer one."]

---

**SCRIPT:**

Let me bring this back to where we started.

The AI produces the average. Your factory shifts that average toward your team — your domain, your architecture, your standards, your accumulated knowledge.

Skills encode that knowledge. Commands put it to work on specific artifacts. Agents accumulate memory over time. Workflows coordinate those agents through work that's too complex for a single session.

And I want to be honest about how the factory I showed you tonight actually got built. I didn't design it in advance. I built one skill. Then iterated on it. Then built one command. Then made the case to my manager for the licenses. Then built two more commands. Then watched the team run them for the first time and felt something shift.

Then I realized I was the bottleneck and built one more command to fix that.

The factory grew with every task. Every problem became a new rule. Every mistake became a ratchet that couldn't slip back.

You don't need to build the whole thing before you get value. You need to build the next piece.

Start with your next task. Do it once. Get the output right. Ask the AI to codify it. Commit it. Then when something goes wrong, ask the crucial question: which factory rule should have caught this? Update the factory. Move on.

That's how you build a software factory.

### [SLIDE: factoryengineering.dev — full resource slide]

Everything I showed tonight — full reference for skills, commands, agents, and workflows, IDE-by-IDE guidance, the symlink setup, the openskills registry — it's all at `factoryengineering.dev`.

Thank you.

---

## Q&A PREPARATION

**Q: Do I need Claude Code specifically, or does this work in my IDE?**
Skills and commands work in Cursor, Windsurf, Kilo Code, Copilot, and Claude Code — that's what the symlink setup is for. Agents and workflows as I demonstrated them require Claude Code or Kilo Code. If you're on Cursor, you can approximate agent memory by instructing the assistant to read from and write to a markdown file, but you won't get true sub-agent orchestration. `factoryengineering.dev` has the full IDE comparison.

**Q: How do you keep the factory from going stale?**
The mindset is: when something goes wrong, update the factory. Not just fix the output — update the rule that allowed the mistake. If you treat factory artifacts like production code — reviewed in PRs, improved over time, owned by the team — they stay current. If you treat them as set-and-forget, they drift.

**Q: What if my team resists learning new tools?**
The commands demo is the answer. Your team doesn't need to understand how the factory works — they need to know `/write-migration @catalog/[event].md`. That's a one-line instruction. The hard part is already done for them. Lower the floor and adoption follows.

**Q: Can I share factory artifacts across projects?**
Some skills are project-specific — your domain rules, your architectural patterns. Those stay bespoke. Others are general enough to share. The `openskills` registry at `factoryengineering.dev` is built for this: you can publish skills your whole organization can pull in.

**Q: How did you handle the Azure DevOps personal access token in the skill?**
The skill uses an environment variable — it instructs the AI on how to use the token that's available in the environment. The skill itself doesn't store credentials. It documents the required variable; the developer sets it up once. Same pattern as any tool that needs API access.

**Q: How long did it take to build what you showed?**
The first skill took about an hour — most of that was doing the task and getting the output right. The commands each took a day or two of iteration. The agents and workflow took longer because I was working out the pattern as I went. Now that the pattern is established, a new command takes an afternoon and a new skill takes an hour. The factory pays back its construction cost quickly.

---

## SLIDE DECK OUTLINE

1. Title card
2. "The AI produces the average" — setup
3. **ACT 1: SKILLS** (section card)
4. Before/after demo — transition slide
5. Skill directory structure
6. Skill creation loop diagram
7. `factoryengineering.dev/skills`
8. **ACT 2: COMMANDS** (section card)
9. Slash-command at-artifact — pattern
10. Event-driven domain — three artifacts per event (catalog entry → migration → handler)
11. The full pipeline (catalog entry → migration → handler → ERD)
12. "Then I got the team Cursor licenses" — story beat
13. "Watching them run the commands for the first time" — story beat
14. "All I had to do was keep the event catalog filled"
15. "The team is now generating their own catalog entries"
16. `factoryengineering.dev/commands`
17. **TRANSITION:** "Same commands, different IDE" — symlink reveal
18. **ACT 3: AGENTS AND WORKFLOWS** (section card)
19. RTCC — Role, Task, Context, Constraints
20. Agent anatomy — definition + memory file side by side
21. "Four agents, one orchestrator" — TDD cycle diagram
22. What makes it orchestration, not a checklist
23. `factoryengineering.dev/workflows`
24. "You don't install a Software Factory. You engineer one."
25. `factoryengineering.dev` — full resource slide
