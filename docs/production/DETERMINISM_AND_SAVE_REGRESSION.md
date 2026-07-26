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

`PetriSimulation` now uses a small serializable xorshift random generator instead of `System.Random`. The generator state is stored in save schema version 2, allowing a loaded culture to continue from the exact same random position.

Cell data is deep-copied both when a save is captured and when it is restored. This prevents the live simulation and stored save from silently modifying each other through shared object references.

Legacy schema version 1 saves remain loadable. Their original random position was not stored, so they use a deterministic fallback derived from the seed and tick. They are stable after loading, but cannot reproduce the exact pre-save random stream.

## Automated tests

Edit Mode tests are located at:

`Assets/Tests/Editor/PetriSimulationTests.cs`

The test suite covers:

1. same-seed deterministic replay;
2. exact save/load continuation including later moisture interventions;
3. save snapshot isolation;
4. restore object isolation;
5. save validation;
6. temperature target clamping.

## Running in Unity

1. Open the project using Unity `6000.3.20f1`.
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
