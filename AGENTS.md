# AGENTS.md

This file defines how AI agents and human contributors should work on Petri Dish Simulator.

## Primary objective

Build the smallest scientifically coherent and visually satisfying petri ecosystem game before adding advanced biology, online services, or large content libraries.

## Source of truth

When documents disagree, use this priority:

1. `docs/DECISIONS.md`
2. `docs/MILESTONES.md`
3. `docs/GAME_DESIGN_DOCUMENT.md`
4. `docs/ARCHITECTURE.md`
5. `docs/ROADMAP.md`
6. Current implementation and task notes

Agents must record any deliberate change to a major project decision in `docs/DECISIONS.md`.

## Working rules

- Do not expand scope while completing a milestone.
- Keep simulation logic separate from visuals, UI, saving, monetisation, and platform services.
- Prefer data-driven content over organism-specific hard-coding.
- Every simulation rule must have a player-visible explanation.
- The same seed and inputs should produce the same result unless the design explicitly states otherwise.
- Do not represent invented mechanics as established science.
- Mark simplified, uncertain, fictional, or educationalised data clearly.
- Design mobile-first controls and readable feedback.
- Avoid requiring an internet connection for the core game.
- Do not add AI-generated runtime dialogue or cloud inference to Phase 1.

## Task completion standard

A task is complete only when:

- Its acceptance criteria are satisfied.
- Relevant documentation is updated.
- Edge cases and failure states are considered.
- No unrelated feature is broken.
- A manual verification path is recorded.
- Any new tunable value is externally configurable rather than buried in logic.
- The result works with touch input and common mobile aspect ratios where applicable.

## Agent roles

### Product and design agent

Maintains scope, player loop, progression, challenge structure, and accessibility. Rejects systems that are technically interesting but do not improve player understanding or enjoyment.

### Simulation agent

Owns environmental state, organism traits, resource flows, population state, events, determinism, and time stepping. Must protect separation between simulation state and presentation.

### Unity implementation agent

Translates approved systems into Unity scenes, prefabs, ScriptableObjects, services, and tests. Must not redesign the game silently.

### Content agent

Creates organisms, media, challenges, discoveries, tooltips, and educational notes from approved templates. Every claim requires a confidence or simplification label.

### Art and UX agent

Creates the readable visual language of colonies, stress, nutrients, moisture, contamination, warnings, and player controls.

### QA and review agent

Checks milestone acceptance criteria, determinism, save compatibility, content validation, performance, accessibility, and misleading scientific claims.

## Branch and review approach

Suggested branches:

- `docs/phase-0-foundation`
- `feature/simulation-core`
- `feature/dish-visualisation`
- `feature/environment-controls`
- `feature/save-and-load`
- `content/organism-pack-01`

Changes that affect architecture, save data, simulation rules, or content schemas require review before merging.

## Prohibited shortcuts

- UI directly changing organism visuals without changing simulation state
- Visual effects acting as the only record of colony state
- Randomness without an owned deterministic random stream
- Organism behaviour encoded only in scene objects
- Save files containing direct references to transient Unity objects
- Scientific labels used for fictional values without qualification
- Monetisation interrupting an active experiment
