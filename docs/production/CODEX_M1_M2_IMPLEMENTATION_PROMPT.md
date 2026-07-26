# Codex Implementation Prompt — Milestones M1 and M2

Use this document as the initial repository task for Codex after the Unity project has been added.

## Mission

Implement Milestone M1, Static Dish Scene, and Milestone M2, Deterministic Simulation Core, for Petri Dish Simulator.

Do not implement the complete colony renderer, progression, monetisation, online services, advanced chemistry, multiple organisms, or production art.

## Read first

Read these files before changing the repository:

1. `AGENTS.md`
2. `docs/DECISIONS.md`
3. `docs/ARCHITECTURE.md`
4. `docs/MILESTONES.md`
5. `docs/design/VERTICAL_SLICE_SPECIFICATION.md`
6. `docs/design/STARTING_BALANCE_VALUES.md`
7. `docs/design/SCREEN_WIREFRAMES_AND_BEHAVIOUR.md`
8. `docs/production/UNITY_SCENE_AND_PREFAB_SPEC.md`
9. `docs/production/PHASE_1_BACKLOG.md`
10. `docs/TEST_STRATEGY.md`

If these documents conflict, follow the source-of-truth order in `AGENTS.md` and report the conflict rather than silently choosing a new design.

## Required deliverables

### M1 — Static dish scene

Create:

- Unity project structure aligned with `docs/production/PROJECT_STRUCTURE.md`
- `Bootstrap` scene
- `Experiment` scene
- Responsive safe-area UI structure
- Placeholder standard dish
- Placeholder nutrient agar surface
- Top bar, status strip, intervention tray, and temperature panel placeholders
- Development-only live-tuning panel for dish scale, UI margins, panel height, spacing, and text scale
- Representative phone and tablet layout checks

M1 must contain no fake growth logic embedded in presentation objects.

### M2 — Deterministic simulation core

Create an engine-independent simulation assembly with:

- Fixed-step simulation clock
- Experiment seed
- Separate deterministic random streams for world, simulation events, and visual-only randomness
- Serializable experiment state
- Configurable two-dimensional grid
- Cell state containing temperature, moisture, nutrients, and population density
- Rapid Bacterium definition data
- Nutrient Agar definition data
- Smooth temperature and moisture suitability curves
- Combined minimum-factor suitability
- Nutrient consumption and biomass growth
- Read-only simulation snapshot output
- Pause, resume, speed, reset-same-seed, and reset-new-seed commands
- Development grid inspection and environment overlays

Do not reference GameObjects, MonoBehaviours, prefabs, materials, UI, audio, ads, or platform SDKs from simulation logic.

## Tests

Add automated tests for:

- Same seed and same commands produce identical snapshots.
- Frame render rate does not affect simulation output.
- Ideal conditions produce positive growth.
- 18 °C produces slower growth than 28 °C.
- 36 °C stops positive growth and increases stress once stress exists in scope.
- Moisture below the growth minimum stops growth.
- Nutrient depletion limits growth.
- Population never exceeds local carrying capacity.
- State can serialize and deserialize without Unity scene-object references.
- Reset with the same seed reproduces initial state.

If stress is deferred until M3, explicitly mark the heat-stress test as pending and still verify that heat stops growth.

## Development tools

Provide a development panel or Simulation Lab supporting:

- Seed entry
- Tick stepping
- Pause and speed
- Temperature
- Moisture
- Presets: Ideal, Cool, Hot, Dry, Starved
- Overlay selection
- Cell inspection
- Same-seed restart
- New-seed restart

Temporary controls must be clearly isolated from production UI.

## Constraints

- Use the repository’s selected Unity LTS version. If none is selected, stop before upgrading packages and report the missing decision.
- Do not add third-party packages unless essential and documented.
- Keep values externally configurable.
- Avoid singletons that hide state ownership.
- Avoid static mutable simulation state.
- Do not use `UnityEngine.Random` for authoritative simulation.
- Do not create save files containing Unity object references.
- Do not invent new organism mechanics.
- Do not change the documented scope without recording a decision proposal.

## Definition of done

M1 and M2 are complete only when:

- The project opens without missing references.
- Required scenes are present and included appropriately in build settings.
- The dish layout works at representative aspect ratios.
- Automated tests pass.
- Same-seed determinism is demonstrated.
- Development presets show inspectable differences.
- Simulation code is isolated from presentation code.
- Manual verification steps are added to the repository.
- `docs/production/IMPLEMENTATION_STATUS.md` records completed work, deferred work, known limitations, test results, and exact Unity verification steps.

## Final response format

Report:

1. Summary of implementation
2. Files and systems added or changed
3. Tests run and results
4. Manual Unity verification steps
5. Known limitations
6. Decisions or ambiguities requiring review
7. Exact next recommended task

Do not claim success for checks that were not run.
