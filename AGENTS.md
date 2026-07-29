# AGENTS.md

This file defines how AI agents and human contributors should work on Petri Dish Simulator.

## Primary objective

Build the smallest scientifically coherent and visually satisfying petri ecosystem game before adding advanced biology, online services, or large content libraries.

The project is now in Phase 2: expand the completed single-organism vertical slice into a data-driven experimental framework using named real organisms with simplified educational behaviour.

## Source of truth

When documents disagree, use this priority:

1. `docs/DECISIONS.md`
2. `docs/MILESTONES.md`
3. `docs/production/PHASE_2_BACKLOG.md`
4. `docs/GAME_DESIGN_DOCUMENT.md`
5. `docs/ARCHITECTURE.md`
6. `docs/ROADMAP.md`
7. Current implementation and task notes

Agents must record any deliberate change to a major project decision in `docs/DECISIONS.md`.

## Working rules

- Read `docs/production/IMPLEMENTATION_STATUS_M1_M5.md`, `docs/ROADMAP.md`, and the active backlog before selecting work.
- Work only on the currently active milestone or an explicitly assigned task.
- Do not expand scope while completing a milestone.
- Keep simulation logic separate from visuals, UI, saving, monetisation, and platform services.
- Prefer data-driven content over organism-specific hard-coding.
- Every simulation rule must have a player-visible explanation.
- The same seed and inputs should produce the same result unless the design explicitly states otherwise.
- Do not represent invented mechanics as established science.
- Use real organism names only with source-backed traits and explicit simplification notes.
- Mark simplified, uncertain, fictional, or educationalised data clearly.
- Design mobile-first controls and readable feedback.
- Avoid requiring an internet connection for the core game.
- Do not add AI-generated runtime dialogue or cloud inference to the MVP.

## Automated work rules

- Inspect recent commits and active branches before starting to avoid duplication.
- Prefer one coherent, tested change per automation run.
- Continue an approved feature branch where practical instead of creating a new branch every run.
- Never merge a feature branch automatically.
- Do not modify Unity scenes, prefabs, models, materials, cameras, or visual composition while the product owner is actively editing them locally unless explicitly assigned.
- Do not modify files owned by another active feature branch without reviewing that branch's purpose.
- Stop and report rather than guessing when save compatibility, scene ownership, or scientific claims are unclear.
- Simulation, save-schema, or content-schema changes require the complete relevant test suite.
- Record any manual Unity import, scene, material, or device-verification step still required.
- Do not begin a later deferred system merely because the assigned task completed early.

## Task completion standard

A task is complete only when:

- Its acceptance criteria are satisfied.
- Relevant documentation is updated.
- Edge cases and failure states are considered.
- No unrelated feature is broken.
- A manual verification path is recorded.
- Any new tunable value is externally configurable rather than buried in logic.
- The result works with touch input and common mobile aspect ratios where applicable.
- Scientific values include source, confidence, and simplification metadata where applicable.

## Agent roles

### Product and design agent

Maintains scope, player loop, progression, challenge structure, and accessibility. Rejects systems that are technically interesting but do not improve player understanding or enjoyment.

### Simulation agent

Owns environmental state, organism traits, resource flows, population state, events, determinism, and time stepping. Must protect separation between simulation state and presentation.

### Unity implementation agent

Translates approved systems into Unity scenes, prefabs, ScriptableObjects, services, and tests. Must not redesign the game silently.

### Content and science agent

Creates real-organism definitions, media, challenges, discoveries, tooltips, and educational notes from approved templates. Every biological claim requires a source, confidence level, and simplification note. The game must not imply laboratory-grade prediction.

### Art and UX agent

Creates the readable visual language of colonies, stress, nutrients, moisture, contamination, warnings, and player controls. Must preserve simulation truth and accessibility cues.

### QA and review agent

Checks milestone acceptance criteria, determinism, save compatibility, content validation, performance, accessibility, and misleading scientific claims.

## Branch and review approach

Suggested branches:

- `feature/data-driven-organisms-media`
- `feature/real-organism-content`
- `feature/medium-definitions`
- `feature/3d-dish-integration`
- `feature/nutrient-intervention`
- `feature/multi-dish-experiments`
- `feature/colony-transfer`
- `content/organism-pack-01`

Changes that affect architecture, save data, simulation rules, scientific content, Unity scenes, or content schemas require review before merging.

## Current ownership boundary

The product owner is currently working locally on the 3D petri-dish model, camera framing, scene placement, and materials. Automated code work should focus on the data-driven organism/media framework and must not overwrite those visual changes.

## Prohibited shortcuts

- UI directly changing organism visuals without changing simulation state
- Visual effects acting as the only record of colony state
- Randomness without an owned deterministic random stream
- Organism behaviour encoded only in scene objects
- Save files containing direct references to transient Unity objects
- Scientific labels used for fictional or unreviewed values without qualification
- Treating simplified game outputs as laboratory predictions
- Monetisation interrupting an active experiment
- Automatic merging of unreviewed feature work
