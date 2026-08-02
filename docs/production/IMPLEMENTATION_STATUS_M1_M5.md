# Implementation Status — Phase 1 Complete / Phase 2 Active

Updated: 2 August 2026.

## Current project state

The Phase 1 vertical slice is functionally complete and verified in Unity `6000.5.3f1`. Phase 2 is active.

The data-driven organism and medium framework and the live colony-texture bridge are merged.
The runtime UI now exposes a transparent central viewport for the product owner's 3D dish
without changing authoritative simulation state or locally owned scene composition.

## Engine baseline

- Unity 6.5
- Editor version `6000.5.3f1`
- Portrait-first mobile layout
- GameObject-based Unity UI through `com.unity.ugui`
- Unity project stored at the repository root

Do not downgrade the production editor baseline.

## Phase 2 — Data-driven organism and medium foundation

Implemented:

- Unity-authored `OrganismDefinition` and `MediumDefinition` ScriptableObjects
- Explicit organism preferred/growth/survival ranges, source/confidence/simplification metadata, carrying capacity, resource demand, and visual-profile ID
- Explicit medium source/confidence/simplification metadata, capacities, optional deterministic diffusion, edge-drying multiplier, spread resistance, and visual-profile ID
- Validated `SimulationDefinitionCatalog` with stable unique ID resolution
- Default Rapid Bacterium and Nutrient Agar assets containing the previous vertical-slice values
- Immutable runtime copies of definition values for deterministic simulations
- Selected organism and medium passed into new, restarted, and restored simulations
- Simulation and experiment save schema version 3 with definition IDs and versions
- Controlled migration of schema-version-2 saves to the original default pair
- Controlled rejection of missing, malformed, duplicate, unsupported, and version-mismatched definitions
- Edit Mode coverage for baseline parity, distinct organism growth, distinct medium drying, deterministic replay, selected-definition save/load and restarts, schema-2 migration, and invalid content
- Complete Unity `6000.5.3f1` Edit Mode verification on 30 July 2026: 81 passed, 0 failed

Deliberately deferred:

- Runtime organism or medium selection UI
- Additional production organisms or media
- Visual-profile registry and visual/audio asset selection
- Compatibility rules, multiple nutrient pools, waste, and competition
- Content-version migration beyond the safe schema-2 default mapping

## M1 — Unity project and responsive dish scene — Complete

Implemented:

- Complete Unity project and package baseline
- Tracked `.meta` files and project settings
- Portrait-first responsive runtime UI
- Safe-area handling and phone simulator verification
- One-click project setup command
- Dish, status, instructions, controls, outcomes, inspection, and save controls

Remaining production review:

- Android SDK and build-profile configuration
- Final production assets and prefab decisions

## M2 — Deterministic simulation core — Complete

Implemented:

- Deterministic 48 × 48 circularly masked dish simulation
- Fixed 0.25-second steps
- Serializable random state and exact save continuation
- Deep-copy save isolation and schema validation
- Temperature, moisture, nutrients, biomass, health, stress, growth, decline, death, and spread
- Circular edge drying and player-visible aggregate metrics
- Input validation and allocation-conscious stepping
- Unity Edit Mode verification with 70 passing tests and no failures on 28 July 2026

Remaining review:

- Codex determinism review
- Random-stream separation when independent organism or event channels are introduced
- Device-based balance validation

## M3 — First living colony — Complete

Implemented:

- Procedural colony texture renderer
- Glass rim, agar depth, highlights, colony mottling, smooth presentation, and visible growth
- Healthy, stressed, dry, and declining feedback
- Non-colour stress patterns

Remaining presentation work:

- More expressive colony boundaries
- Final texture and art pass
- Mobile profiling and renderer optimisation

## M4 — Player intervention loop — Complete

Implemented:

- Temperature and moisture interventions
- Pause and speed controls
- Save, load, restart, and new-seed flows
- Plain-language limiting-factor feedback
- Tap-driven cell inspection
- Standard and Large text modes
- Accessible pause and playback states

Remaining review:

- Audio and haptic feedback
- Assistive-technology strategy
- Narrow-phone Large-mode usability review

## M5 — Comfortable Range vertical slice — Functionally complete

Implemented:

- Guided observation, warming, ideal hold, heater fault, correction, moisture rescue, recovery, completion, and failure
- Discovery outcome text
- Seeded restart and save/resume of tutorial stage

Remaining presentation work:

- Final audio and art
- Menu and experiment selection
- Full journal/timeline presentation
- Tutorial usability testing

## Active Phase 2 engineering work

### Data-driven organism and medium framework — Complete

Implemented and merged through PR #7:

- Move organism and medium parameters out of central simulation logic
- Add validated serialisable definitions
- Persist selected definition IDs in saves
- Preserve deterministic replay and current vertical-slice behaviour
- Add regression tests for distinct organisms and media
- Begin with the current Rapid Bacterium and Nutrient Agar only, then replace the generic identity with reviewed real-organism content

Recommended branch: `feature/data-driven-organisms-media`.

### Real-organism direction

The project will use named real organisms with simplified educational behaviour. It will not attempt laboratory-grade prediction. Organism content requires scientific names, source-backed traits, confidence, and simplification notes.

### 3D dish presentation — Code and runtime viewport implemented; manual verification pending

The product owner has created and imported a reusable 3D petri dish with separate:

- glass base;
- glass wall and rim;
- agar;
- colony surface;
- removable lid.

The preferred portrait camera angle has been selected. The 2D UI remains in place.
`DishRenderer` now exposes its existing generated texture, `ColonySurfacePresenter`
applies it to a validated `MeshRenderer` property using a cached
`MaterialPropertyBlock`, and `RuntimeBootstrap` binds presenters after scene loads.

The bridge reuses the same live texture for restarts and save/load. If the texture object
is recreated, the presenter receives the replacement without creating another simulation
or copying texture pixels. Shared imported materials are never modified.

The runtime `ScreenSpaceOverlay` UI previously placed an opaque full-screen `Background`
behind an opaque `DishPanel`, so hiding only the old `RawImage` could not reveal the world
camera. `DishViewportPresenter` now builds four non-raycastable background regions around
the dish rectangle, leaving its centre transparent. The fallback `DishPanel` remains opaque
while the 2D dish is visible and becomes transparent only when a successful 3D binding hides
the flat image. The transparent `RawImage` stays active as the existing tap-inspection surface.

The presenter now exposes texture scale X/Y, offset X/Y, Flip X, and Flip Y controls for
aligning the generated colony texture to the approved colony-surface UVs. Defaults preserve
the previous 1,1 tiling and 0,0 offset. Inspector and runtime changes update `_MainTex_ST`
through the presenter's cached `MaterialPropertyBlock` while the live texture remains bound
through `_MainTex`. Quarter-turn rotation is not included because the built-in Standard
shader's scale/offset transform cannot swap UV axes without changing the shader/material or
copying the live texture.

The presenter Inspector also provides Auto Centre, Auto Fit, and Reset Alignment actions.
Auto Centre preserves the selected scale and flips while centring against the colony
surface's UV0 bounds. Auto Fit applies a uniform, aspect-preserving scale from those bounds
and then centres the texture. Reset Alignment restores the default scale, offset, and flip
values. These actions read mesh data without editing the FBX, UVs, materials, transforms, or
scene hierarchy, and a failed action leaves the existing live binding unchanged.

The combined viewport and alignment branch requires refreshed complete Edit Mode verification
after reconciliation.

Automated agents must not overwrite the current scene placement, camera framing, model, materials, or lid setup without explicit assignment.

Manual verification still required:

- add `ColonySurfacePresenter` to `PetriDish_ColonySurface`;
- assign that object's `MeshRenderer`;
- verify the material's texture property (`_MainTex` for the built-in Standard shader);
- try Auto Fit, then Auto Centre, and use Reset Alignment to return to the known default;
- enter Play Mode and adjust Texture Scale, Texture Offset, Flip X, and Flip Y on the
  presenter until the circular simulation mask is centred on the agar;
- start with scale 1,1 and offset 0,0, adjust scale first, then offset in small increments,
  and use flips only when the model UV direction is reversed;
- confirm live growth, restart, new-seed restart, save/load, and scene reload;
- enable flat-image hiding only after the 3D output is confirmed;
- confirm the runtime viewport exposes the 3D dish across representative portrait safe areas;
- retain the current 2D tap surface until a later 3D raycast mapping is designed and tested.

## Planned later systems

After stable organism/media definitions:

1. Add one reviewed real organism and one additional medium.
2. Add nutrient intervention.
3. Expand to four organisms and four media.
4. Add experiment selection and discoveries.
5. Add multiple dishes and colony transfer.
6. Add evolution, antibacterial/antifungal treatment, and resistance only after lineage tracking is stable.

## Running and verification

1. Install Unity `6000.5.3f1` through Unity Hub.
2. Open the repository root as the Unity project.
3. Allow Package Manager to resolve dependencies.
4. Select `Petri Dish > Setup Vertical Slice Project` when required.
5. Open `Assets/PetriDish/Scenes/PetriDishVerticalSlice.unity`.
6. Enter Play Mode.
7. Run the complete Edit Mode suite through Test Runner.
8. Verify Standard and Large text modes in narrow portrait profiles.
9. Preserve and manually verify the locally configured 3D dish scene before merging code branches.

## Active source of next work

Use `docs/production/PHASE_2_BACKLOG.md`. The old Phase 1 backlog is archived under `docs/archive/PHASE_1_BACKLOG.md`.
