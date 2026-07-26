# Repository Readiness Audit

## Audit date

26 July 2026

## Current status

The repository is ready for Unity project initialization and Milestone M1 implementation. Product, simulation, content, UX, safety, testing, and agent boundaries are defined well enough that an implementation agent should not need to invent the core vertical slice.

## Available foundations

### Product definition

- Vision
- Game design document
- Principles
- Decision log
- Roadmap
- Milestones
- Progression and economy
- Analytics

### Simulation definition

- Layered architecture
- Runtime and definition data models
- Deterministic simulation requirement
- Initial environmental model
- Rapid Bacterium and Nutrient Agar starting values
- Fixed first guided experiment
- Golden scenarios and test strategy

### Presentation definition

- Screen hierarchy and behavioural wireframes
- Mobile safe-area requirements
- Accessibility rules
- Scene hierarchy
- Prefab responsibilities
- Asset register
- Art and audio direction
- Complete tutorial and outcome copy

### Production definition

- Agent rules
- Phase 1 backlog
- Unity build brief
- Codex review brief
- M1/M2 Codex implementation prompt
- Acceptance criteria for M0–M7

### Content and safety

- Four initial organism archetypes
- Four initial media
- Twelve guided experiment concepts
- Twenty challenge concepts
- Accuracy labels
- Scientific review workflow
- Prohibited unsafe content

## Readiness checks

| Area | Status | Notes |
|---|---|---|
| Game vision | Ready | Clear audience and product ladder |
| Vertical slice | Ready | One full guided experiment specified |
| Simulation scope | Ready | Simplified deterministic grid selected |
| Starting balance | Ready for prototyping | Values explicitly provisional |
| Architecture | Ready | Simulation separated from Unity presentation |
| UI flow | Ready for functional implementation | Final visual design remains open |
| Unity hierarchy | Ready | Scenes and prefabs specified |
| Art production | Ready for placeholders | Final style exploration still required |
| Audio production | Ready for placeholders | Asset list exists |
| Tutorial copy | Ready | First experiment copy complete |
| Testing | Ready | Determinism and golden scenarios defined |
| Safety | Ready | Strong scope and review controls |
| Monetisation | Deferred correctly | Not part of prototype |
| Online services | Deferred correctly | Not required for core play |

## Remaining decisions before Unity initialization

These are implementation setup choices rather than unresolved game design:

1. Exact Unity LTS editor version
2. Render pipeline: Built-in or URP
3. Portrait-only versus portrait-preferred responsive orientation for the first mobile build
4. Minimum Android API level and target device baseline
5. Input System package versus legacy input for initial prototype
6. Repository strategy for Unity project at root versus a named subdirectory
7. Text rendering package and initial redistributable font selection

Recommended defaults:

- Current stable Unity LTS
- URP only if transparent dish effects and planned visual work justify it; otherwise Built-in keeps the prototype lighter
- Portrait-first mobile layout
- Unity Input System
- Unity project at repository root unless multiple products are expected
- TextMeshPro with a properly licensed font

These defaults should be recorded in `docs/DECISIONS.md` when selected.

## Known design uncertainties

### Colony rendering method

Options include texture painting, mesh generation, cell sprites, shader field rendering, or a hybrid. The simulation architecture does not depend on the choice. M3 should prototype at least two approaches against readability and mobile performance.

### Global versus local temperature

The vertical slice can use global temperature. The grid still allows local temperature values for future gradients. Do not build complex heat diffusion until a challenge requires it.

### Moisture field timing

The vertical slice requires edge drying and moisture diffusion by M4. M2 can begin with uniform moisture if the state model already supports per-cell values.

### Health display

The player-facing health percentage combines multiple internal signals. Usability testing should determine whether `Health`, `Condition`, or growth trend is the clearest primary indicator.

## Risks before coding

- Selecting a sophisticated rendering approach before testing basic growth readability
- Mixing Unity authoring definitions with mutable simulation state
- Treating provisional balance values as scientific constants
- Expanding M1/M2 into complete game systems
- Committing generated Unity folders or large unlicensed assets

## Recommended implementation sequence

1. Record Unity setup decisions.
2. Initialize the Unity project and `.gitignore`.
3. Build responsive static dish scene.
4. Build a separate Simulation Lab.
5. Implement deterministic clock and state.
6. Add environment and population tests.
7. Connect read-only snapshots to placeholder visuals.
8. Tune the six validation scenarios.
9. Begin M3 colony rendering prototypes.

## Audit conclusion

**Phase 0 is complete enough to begin implementation.**

The next repository action should be initialization of the Unity project followed by the M1/M2 Codex prompt. No additional large design phase is required before coding, although final visual design and scientific source notes will continue alongside production.
