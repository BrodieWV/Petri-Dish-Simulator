# Determinism and Save Regression Coverage

## Purpose

The first automated regression pass protects the simulation properties that later balancing, content, and progression systems depend on:

- identical seeds and player inputs produce identical outcomes;
- save and load resumes the exact random sequence;
- captured saves do not change when the live simulation continues;
- restored simulations do not share mutable cell objects with save data;
- invalid schema versions and mismatched seeds are rejected;
- public environmental controls remain inside supported bounds.

## Implementation

`PetriSimulation` uses a small serializable xorshift random generator instead of
`System.Random`. The generator state introduced in save schema version 2 remains in
schema version 3, allowing a loaded culture to continue from the exact same random
position.

Cell data is deep-copied both when a save is captured and when it is restored. This prevents the live simulation and stored save from silently modifying each other through shared object references.

Legacy simulation schema version 1 saves remain loadable when restored directly to the
default content pair. Their original random position was not stored, so they use a
deterministic fallback derived from the seed and tick. They are stable after loading, but
cannot reproduce the exact pre-save random stream.

Simulation and experiment save schema version 3 store the selected organism and medium
IDs and definition versions. Existing schema-version-2 experiment saves migrate to Rapid
Bacterium on Nutrient Agar, the only pair available when they were written. Schema 3
resolves and validates exact definitions before replacing a running experiment; missing
or incompatible content produces a controlled load error.

The application save wrapper now preserves the fractional fixed-step accumulator, pause state, simulation speed, guided stage, and active simulation state. Loading validates the complete candidate before replacing the running experiment, so malformed or unsupported data cannot partially mutate live state. Writes use a temporary file and retain the previous save as a recovery backup.

Same-seed restart uses the active experiment seed rather than reverting to the tutorial seed. Snapshot publication occurs only when simulation state changes, while fixed-step simulation and dish-rendering work buffers are reused to reduce managed allocations.

## Automated tests

Edit Mode tests are located at:

`Assets/Tests/Editor/PetriSimulationTests.cs` and
`Assets/Tests/Editor/SimulationDefinitionTests.cs`

The test suite covers:

1. same-seed deterministic replay;
2. exact save/load continuation including later moisture interventions;
3. save snapshot isolation;
4. restore object isolation;
5. save validation;
6. temperature target clamping and non-finite input rejection;
7. application-level fractional-clock continuation;
8. active-seed restart;
9. pause and speed restoration;
10. malformed-save isolation and backup recovery;
11. initialization event timing and snapshot publication cadence;
12. zero managed allocations during warmed-up fixed simulation steps;
13. default-definition parity with the original vertical-slice values;
14. distinct organism and medium outcomes;
15. deterministic replay with custom definitions;
16. selected-definition ID preservation and exact continuation;
17. schema-version-2 content migration;
18. malformed, duplicate, missing, and unsupported definition rejection;
19. same-seed and new-seed restarts preserve the selected definitions.

Application-level persistence and lifecycle tests are located at:

`Assets/Tests/Editor/ExperimentControllerTests.cs`

## Latest automated verification

On 30 July 2026, the complete Edit Mode suite compiled and passed all 80 test cases using
the production-baseline Unity `6000.5.3f1` editor.

## Running in Unity

1. Open the project using Unity `6000.5.3f1`.
2. Allow Package Manager to install `com.unity.test-framework`.
3. Open `Window > General > Test Runner`.
4. Select the **EditMode** tab.
5. Run all tests.

For command-line verification on a machine with Unity installed:

```text
Unity -batchmode -nographics -projectPath <repo-path> -runTests -testPlatform EditMode -testResults TestResults.xml -quit
```

Use the platform-specific Unity executable path rather than the literal `Unity` command when required.

## Acceptance criteria

This regression step is complete when:

- the project imports without compiler errors;
- all Edit Mode tests pass;
- a manual save/load during **The Comfortable Range** resumes without a visible state jump;
- restarting with the same seed and repeating the same interventions produces the same dish pattern.

## Next recommended test layer

Add scenario-level tests for three named outcomes:

- comfortable growth;
- heat-stress decline;
- dry-out followed by moisture recovery.

Those tests should assert broad outcome ranges rather than exact balance values so normal tuning does not make the suite brittle.
