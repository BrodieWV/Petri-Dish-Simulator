# Implementation Status — Milestones M1 to M5

## Scope of this implementation pass

This pass creates a reviewable Unity vertical slice in code. Formal testing, art replacement, performance tuning, and Codex hardening remain later work.

## Engine baseline

- Unity 6.3 LTS
- Editor version `6000.3.20f1`
- Portrait-first mobile layout
- GameObject-based Unity UI through `com.unity.ugui`
- Unity project stored at the repository root

Unity 6.3 LTS is the locked production baseline. Do not downgrade the project to Unity 2022 or Unity 2023.

## M1 — Unity project and responsive dish scene

Implemented:

- Unity 6.3 LTS project marker
- Minimal Unity 6 package manifest with uGUI and Visual Studio integration
- Unity `.gitignore`
- Portrait-first runtime-generated responsive UI
- Safe proportional anchors for phone and tablet layouts
- Dish, status, instructions, controls, outcomes, and save controls
- One-click editor setup command at `Petri Dish > Setup Vertical Slice Project`

Review pending:

- Confirm Android SDK and minimum API level
- Inspect device safe areas and cut-outs
- Replace generated placeholder layout with production prefabs if preferred
- Review Unity 6 Build Profiles and Android platform configuration

## M2 — Deterministic simulation core

Implemented:

- 48 × 48 two-dimensional dish grid
- Fixed 0.25-second simulation steps
- Seeded colony generation and spread randomness
- Temperature target and delayed temperature movement
- Moisture, edge drying, heat drying, nutrients, biomass, health, and stress
- Suitability curves for temperature, moisture, and nutrients
- Growth, decline, death, resource consumption, and local spread
- Read-only simulation snapshots
- Serializable save state

Review pending:

- Codex determinism review
- Random-stream separation refinement
- Balance validation
- Automated regression scenarios

## M3 — First living colony

Implemented:

- Texture-driven dish renderer
- Healthy, stressed, dry, and declining colour response
- Visible biomass expansion
- Smooth bilinear presentation over the low-resolution grid
- Slower growth near tolerance limits
- Nutrient depletion and environmental decline

Review pending:

- More expressive colony edge animation
- Shape and texture art pass
- Visual feedback beyond colour
- Mobile profiling
- Unity 6 renderer and texture-update optimisation review

## M4 — Player intervention loop

Implemented:

- Bounded temperature slider
- Delayed temperature response
- Moisture intervention
- Context-controlled moisture button
- Pause and speed cycling
- Same-seed and new-seed restarts
- Plain-language condition labels
- Coverage, temperature, moisture, and nutrient metrics
- Save and load controls

Review pending:

- Local tap inspection panel
- Haptic and audio feedback
- More explicit speed label
- Accessibility pass
- Unity 6 Input System migration if production controls require it

## M5 — The Comfortable Range vertical slice

Implemented:

- Cool-start observation stage
- Player warming stage
- Ideal-range hold stage
- Deterministic heater fault
- Temperature correction
- Moisture rescue
- Delayed recovery
- Completion and failure states
- Discovery outcome text
- Seeded restart
- Save and resume of experiment and tutorial stage

Review pending:

- Menu and experiment-selection scenes
- Full outcome timeline and journal screens
- Audio and final art assets
- Checkpoint-specific recovery rather than full restart
- Tutorial usability testing
- Copy verification against the full content deck

## Running the project

1. Install Unity `6000.3.20f1` through Unity Hub with Android Build Support if mobile builds are required.
2. Open the repository root as the Unity project.
3. Allow Unity Package Manager to resolve the Unity 6 packages.
4. Select `Petri Dish > Setup Vertical Slice Project`.
5. Open `Assets/PetriDish/Scenes/PetriDishVerticalSlice.unity` if it is not already open.
6. Enter Play Mode.

The UI and experiment controller are generated at runtime. No manually wired scene hierarchy is required for the first pass.

## Important limitations

- The code has not been compiled inside Unity during this ChatGPT implementation pass.
- Formal testing was intentionally deferred at the user's request.
- Placeholder UI and procedural colours are used instead of final art.
- Unity 6 may expose API or package issues that must be corrected during the first editor import.
- The implementation should be treated as a substantial first draft for Codex review, correction, and refinement.
