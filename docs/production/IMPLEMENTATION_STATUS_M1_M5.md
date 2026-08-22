# Implementation Status — Phase 1 Complete / Phase 2 Active

Updated: 22 August 2026.

## Current project state

The Phase 1 vertical slice is functionally complete and verified in Unity `6000.5.3f1`. Phase 2 automated implementation is complete, and M6.5 Laboratory Hub functionalisation is implemented for review.

The M6.5 feature branch gives every visible Hub control an intentional action, routes both New Experiment and Compare entry points consistently, preserves the authoritative experiment when opening the selected dish, and exposes honest unavailable states for deferred Phase 3 destinations. The real Hub dish supports constrained mouse/touch orbit and zoom with Reset View and UI-overlay isolation. The single-dish card is derived from the persistent experiment controller while its arrows remain truthfully disabled. Automated coverage includes routing, presentation constraints, compact Notes behavior, and the Hub-to-experiment-to-Hub lifecycle. Full Unity execution and manual supported-landscape inspection remain release gates.

The data-driven organism and medium framework and live colony-texture bridge are merged.
Draft Phase 2 PRs add the product-owner-approved *Bacillus subtilis* identity (#9), reconcile
the 3D viewport and colony alignment (#11), add Low-Nutrient Agar (#12), and add
save-compatible organism/medium selection (#13). The runtime UI exposes a transparent
central viewport for the product owner's 3D dish without changing authoritative simulation
state. Remaining M6 work is the second approved organism, the nutrient intervention, combined
review, and manual 3D/mobile verification.

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

- Third and fourth production organisms and media beyond the two required by M6
- Visual-profile registry and visual/audio asset selection
- Compatibility rules, multiple nutrient pools, waste, and competition
- Content-version migration beyond the safe schema-2 default mapping
- Additional guided experiments, discovery/journal flows, multiple dishes, and colony transfer

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

The product owner approved *Bacillus subtilis* as the first named organism. Its reviewed identity and educational metadata replace the generic display content while the `rapid-bacterium` stable ID, definition version `1`, Phase 1 simulation balance, and visual profile remain unchanged for schema-version-3 identity matching, older-save migration, and deterministic continuity. The implementation remains in draft PR #9 until review and merge. Qualified microbiology and manual visual review remain release gates, and a second species still requires a dedicated content and safety review plus product-owner approval. See `docs/research/BACILLUS_SUBTILIS_CONTENT_SAFETY_REVIEW.md`.

### 3D dish presentation — Scene hookup and runtime viewport implemented; manual verification pending

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

The approved scene already contains `ColonySurfacePresenter` on
`PetriDish_ColonySurface`, references that object's `MeshRenderer`, targets `_MainTex`, hides
the flat image only after successful binding, and retains the product owner's authored
alignment of scale `1.8,1.8`, offset `0.05,0.05`, with flips disabled.

Complete Unity `6000.5.3f1` Edit Mode verification after branch reconciliation on
3 August 2026: 99 passed, 0 failed, 0 skipped. Targeted colony-surface verification passed
15 tests. The full suite includes viewport, binding, alignment, determinism, save/load,
migration, inspection, restart, accessibility, and simulation coverage.

Automated agents must not overwrite the current scene placement, camera framing, model, materials, or lid setup without explicit assignment.

Manual verification still required:

- open the approved scene and confirm the existing presenter, renderer reference, `_MainTex`
  property, flat-image hiding, and authored alignment remain intact;
- enter Play Mode and confirm the circular simulation texture is centred on the agar using
  the authored scale `1.8,1.8` and offset `0.05,0.05` as the baseline;
- if adjustment is still needed, use Auto Fit or Auto Centre and Unity Undo to compare with
  the authored baseline; save scene changes only after product-owner visual approval;
- confirm live growth, restart, new-seed restart, save/load, and scene reload;
- confirm the runtime viewport exposes the 3D dish across representative portrait safe areas;
- review glass, agar, and colony-surface materials and profile mobile performance;
- retain the current 2D tap surface until a later 3D raycast mapping is designed and tested.

## Planned later systems

Remaining M6 work:

1. Approve, review, implement, and test the second named organism.
2. Approve the nutrient intervention contract, then implement and test it.
3. Complete qualified content review and manual 3D/mobile verification.

The reconciled 3D viewport/alignment work, Low-Nutrient Agar, and save-compatible selection
are implemented in draft PRs #11, #12, and #13. Exact unresolved product decisions are
recorded in `docs/production/PHASE_2_DECISIONS_REQUIRED.md`.

Combined integration verification on `codex/phase-2-integration-verification` used Unity
`6000.5.3f1` in a temporary isolated project copy: 103 Edit Mode tests passed, with 0 failed
and 0 skipped. The protected FBX/art and approved scene remain byte-identical to the reviewed
presentation branch.

M7 then expands to four organisms and four media, guided experiments, discoveries and journal, multiple dishes, and colony transfer. Evolution, antibacterial/antifungal treatment, and resistance remain later than M7 and require stable lineage tracking.

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
