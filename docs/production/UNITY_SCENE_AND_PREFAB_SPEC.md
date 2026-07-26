# Unity Scene and Prefab Specification

## Purpose

Define the Unity objects required for the vertical slice without prescribing implementation details that belong in code.

## Scene strategy

### `Bootstrap`

Persistent startup scene responsible for loading services and routing to the correct screen.

Required root objects:

- `AppRoot`
- `ServiceContainer`
- `AudioRoot`
- `SceneTransitionRoot`
- `DevelopmentDiagnostics` — development builds only

No game-specific dish visuals belong here.

### `MainMenu`

Required objects:

- `MainMenuCanvas`
- `SafeAreaRoot`
- `DishPreviewPanel`
- `PrimaryNavigationPanel`
- `SettingsButton`
- `VersionLabel`

The dish preview consumes a saved snapshot or authored preview image. It does not run the full simulation in M1–M5.

### `GuidedExperimentSelect`

Required objects:

- `GuidedExperimentCanvas`
- `ExperimentList`
- `ExperimentCardContainer`
- `ExperimentDetailsPanel`
- `StartButton`
- `BackButton`

### `ExperimentSetup`

Required objects:

- `ExperimentSetupCanvas`
- `OrganismSelectionField`
- `MediumSelectionField`
- `DishSelectionField`
- `StartingConditionsPanel`
- `LessonPanel`
- `StartDishButton`
- `BackButton`

### `Experiment`

Required hierarchy:

```text
ExperimentRoot
├── SimulationHost
├── ExperimentApplication
├── DishPresentationRoot
│   ├── DishBase
│   ├── MediumSurface
│   ├── EnvironmentVisualLayer
│   ├── ColonyVisualLayer
│   ├── EffectsLayer
│   └── InspectionLayer
├── ExperimentCamera
├── ExperimentCanvas
│   └── SafeAreaRoot
│       ├── TopBar
│       ├── StatusStrip
│       ├── ObjectiveChip
│       ├── InterventionTray
│       ├── ActiveInterventionPanel
│       ├── InspectionPanel
│       ├── TutorialPromptLayer
│       ├── DiscoveryLayer
│       └── PauseLayer
├── ExperimentAudio
└── DevelopmentTools
```

`SimulationHost` owns the simulation instance but not the UI or visuals. `DishPresentationRoot` reads snapshots and renders them. UI sends commands through the application layer.

### `Outcome`

Required objects:

- `OutcomeCanvas`
- `OutcomeHeader`
- `ResultMetricsPanel`
- `CauseSummaryPanel`
- `DiscoverySummaryPanel`
- `RewardsPanel`
- `TimelineButton`
- `RetrySameSeedButton`
- `ContinueButton`

### `Journal`

M5 requires only enough structure to display discoveries earned in the vertical slice.

Required objects:

- `JournalCanvas`
- `CategoryTabs`
- `EntryList`
- `EntryDetailsPanel`
- `BackButton`

## Prefabs

### `prefab_dish_standard`

Contains:

- Base sprite or mesh
- Glass overlay
- Medium mask
- Colony render target or visual container
- Environmental overlay container
- Edge mask
- Inspection coordinate surface

Must not contain simulation rules.

### `prefab_colony_renderer`

Responsibilities:

- Consume population and condition snapshots
- Render density
- Render active edge
- Blend healthy, stressed, recovering, and dying presentation states
- Interpolate between simulation ticks

It does not calculate growth or health.

### `prefab_environment_overlay`

Modes:

- Temperature
- Moisture
- Nutrients
- Population density
- Stress

Only one overlay needs to be visible at once in the development build.

### `prefab_intervention_button`

Fields:

- Icon
- Label
- Locked state
- Selected state
- Alert badge
- Accessibility description

Variants are data-driven rather than separate logic prefabs.

### `prefab_temperature_control`

Contains:

- Current temperature label
- Target temperature label if lag is retained
- Slider
- Increment and decrement buttons
- Preferred-range indicator when discovered
- Help text

### `prefab_moisture_control`

Contains:

- Current moisture label
- Dose button or bounded control
- Cooldown indicator
- Preferred-range indicator when discovered
- Help text

### `prefab_status_strip`

Contains:

- Condition label and icon
- Limiting-factor label and icon
- Coverage
- Health or trend

### `prefab_tutorial_prompt`

Contains:

- Prompt text
- `Show me` action
- Continue action
- Collapse action when allowed

### `prefab_discovery_overlay`

Contains:

- Title
- Observation
- Accuracy label
- Illustration slot
- Journal and continue actions

### `prefab_outcome_cause_item`

Displays one causal statement with:

- Event icon
- Cause
- Effect
- Simulation timestamp

### `prefab_experiment_card`

Displays:

- Thumbnail
- Title
- Lesson
- Organism
- Duration
- Difficulty
- Lock or completion state

## ScriptableObject definition assets

Recommended authored definitions:

- `OrganismDefinition`
- `MediumDefinition`
- `DishDefinition`
- `InterventionDefinition`
- `GuidedExperimentDefinition`
- `DiscoveryDefinition`
- `VisualProfile`
- `AudioProfile`
- `SimulationTuningProfile`

Definitions may use ScriptableObjects, but runtime simulation state must be serialisable without Unity object references.

## Development tools scene

A separate `SimulationLab` scene is recommended.

It should provide:

- Seed entry
- Start/reset controls
- Tick stepping
- Speed controls
- Temperature and moisture controls
- Environment presets
- Grid-cell inspector
- Overlay selector
- Snapshot export summary
- Determinism comparison
- Live tuning values

The laboratory scene is not player-facing and may use utilitarian UI.

## Canvas and scaling

- Use one consistent canvas scaling policy across scenes.
- Apply safe area once at the appropriate root.
- Avoid multiple nested Canvas components unless profiling shows a clear benefit.
- Keep frequently changing status UI separate from large static panels where practical.

## Scene transition rules

- Transitions must not destroy unsaved experiment state accidentally.
- Starting an experiment creates authoritative state before opening the experiment scene.
- Outcome data is captured before leaving the experiment scene.
- Restart same seed creates a new experiment instance from the original setup, not from mutated live state.

## Prefab completion standard

A prefab is complete when:

- Purpose and ownership are clear.
- It has no hidden authoritative simulation state.
- Required fields validate.
- Placeholder art can be replaced without hierarchy redesign.
- It behaves correctly at target aspect ratios.
- Accessibility labels are available for interactive elements.
