# Petri Dish Simulator

**Working title:** Petri Dish Simulator  
**Project type:** Casual educational ecosystem simulation game  
**Initial platform:** Mobile, with a PC development build  
**Engine target:** Unity 6.5 (`6000.5.3f1`)
**Project phase:** First playable vertical-slice implementation

## Concept

Petri Dish Simulator lets players create a small living ecosystem by selecting an organism, growth medium, food source, and environmental conditions. The player then observes growth, competition, stress, adaptation, reproduction, dormancy, and collapse.

The first release is a game, not a laboratory simulator. It should teach real biological relationships through understandable cause and effect while remaining visually active, forgiving, and enjoyable for younger or casual players.

## Player fantasy

> “I created this tiny world, changed its environment, and discovered why it survived, changed, or failed.”

## Core loop

1. Choose a dish, medium, organism, and optional food source.
2. Set temperature, moisture, light, airflow, and nutrient levels.
3. Start the culture.
4. Observe visible changes and organism feedback.
5. Intervene by adjusting the environment or adding resources.
6. Complete discoveries, challenges, and collection goals.
7. Save successful cultures, unlock new organisms, and try more complex ecosystems.

## Current implementation

The repository contains a first-pass implementation of Milestones M1–M5:

- Deterministic 48 × 48 dish simulation
- Rapid Bacterium and Nutrient Agar vertical slice
- Temperature and moisture controls
- Colony growth, stress, resource use, decline, and recovery
- Runtime-generated portrait mobile UI
- Save and load
- Seeded restart
- Guided experiment: **The Comfortable Range**

See `docs/production/IMPLEMENTATION_STATUS_M1_M5.md` for the detailed handoff and known limitations.

## Opening the project

1. Install Unity `6000.5.3f1` through Unity Hub.
2. Include Android Build Support for mobile builds.
3. Open the repository root as the Unity project.
4. Allow Package Manager to resolve dependencies.
5. Run `Petri Dish > Setup Vertical Slice Project`.
6. Enter Play Mode in `Assets/PetriDish/Scenes/PetriDishVerticalSlice.unity`.

Do not open or resave the project using Unity 2022 or Unity 2023.

## Initial scope

The first playable version focuses on a single dish containing one primary organism type. The simulation uses simplified traits and environmental tolerances rather than species-level laboratory accuracy.

Initial organisms:

- Fast-growing bacterium
- Filamentous fungus
- Slime mould
- Yeast-like colony

Initial media:

- Nutrient agar
- Wood-chip substrate
- Moist soil gel
- Low-nutrient agar

Initial environmental controls:

- Temperature
- Moisture
- Light cycle and intensity
- Airflow
- Food or nutrient input

## Design pillars

- Visible cause and effect
- A living dish at all times
- Science-inspired, not science-faking
- Easy to start, difficult to master
- Failure produces information
- Expandable from children’s game to advanced simulation

## Key documents

- `AGENTS.md`
- `docs/VISION.md`
- `docs/GAME_DESIGN_DOCUMENT.md`
- `docs/ARCHITECTURE.md`
- `docs/DATA_MODEL.md`
- `docs/ROADMAP.md`
- `docs/MILESTONES.md`
- `docs/SIMULATION_MODEL.md`
- `docs/TEST_STRATEGY.md`
- `docs/design/VERTICAL_SLICE_SPECIFICATION.md`
- `docs/design/STARTING_BALANCE_VALUES.md`
- `docs/production/UNITY_SCENE_AND_PREFAB_SPEC.md`
- `docs/production/IMPLEMENTATION_STATUS_M1_M5.md`
- `docs/production/CODEX_REVIEW_BRIEF.md`

## Current decision

Do not begin with a chemically exact simulation. Build a deterministic, data-driven ecological model that produces understandable outcomes and can later accept more detailed biology modules.
