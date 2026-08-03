# Phase 2 Backlog — Multi-Organism Experimental Slice

Status: active from 30 July 2026.

## Phase objective

Expand the completed single-dish vertical slice into a reusable experimental framework with simplified real organisms, configurable media, a 3D dish presentation, and the first systems needed for comparative experiments.

Phase 2 completion follows milestone M6: two organisms, two media, save-compatible selection, the live 3D colony texture, nutrient intervention, content validation, and regression tests. Expansion to four organisms and four media, additional guided experiments, discovery/journal flows, multiple dishes, and colony transfer belongs to M7 and is not required for Phase 2 exit.

## Working order

Complete epics in dependency order. Do not begin later biological systems merely because an earlier task is small.

## Epic A — Data-driven organism and medium framework

Status: complete and merged through PR #7.

- Define validated organism and medium schemas.
- Move organism-specific and medium-specific values out of central simulation logic.
- Preserve current Rapid Bacterium and Nutrient Agar behaviour as migration defaults.
- Persist selected organism and medium IDs in saves.
- Preserve deterministic replay and exact save continuation.
- Add compatibility handling for older saves.
- Add regression tests for different organism and medium definitions.

Exit criteria:

- New simulations receive explicit organism and medium definitions.
- Existing vertical-slice behaviour remains within documented tolerances.
- Invalid definitions and unknown saved IDs fail clearly.
- The full Edit Mode suite passes.

## Epic B — Real-organism content foundation

Status: *Bacillus subtilis* and *Saccharomyces cerevisiae* are approved and implemented
as the two M6 organisms. Both retain explicit source, confidence, simplification, and
save-compatible identity metadata; qualified subject-matter review remains before release.

- Replace the generic Rapid Bacterium content identity with the first approved real organism after scientific review.
- Use named real organisms with deliberately simplified educational behaviour.
- Store scientific name, player-facing name, short description, source notes, confidence, and simplification notes.
- Add the first two organisms individually, with tests and visibly distinct behaviour.
- Select the remaining organisms for the four-organism M7 set before creating a large content catalogue.

Candidate groups include bacteria, yeast, and filamentous mould. Final species selection requires a dedicated content and safety review.

The first-candidate review is recorded in `docs/research/BACILLUS_SUBTILIS_CONTENT_SAFETY_REVIEW.md`. The implemented identity uses *Bacillus subtilis*, defines safe player-facing claims and simplification boundaries, and retains the migration-default stable ID, definition version, simulation values, and visual profile. Qualified subject-matter and manual visual review remain release gates.

## Epic C — Media system

Status: Low-Nutrient Agar is implemented and tested as the second M6 medium in draft PR #12. Nutrient Agar remains the migration default.

- Add one medium beyond Nutrient Agar so Phase 2 has two media with distinct nutrient availability, moisture retention, diffusion, drying, and spread resistance.
- Add the remaining two media during M7, one at a time.
- Keep laboratory media separate from later fungal-growing substrates.
- Ensure each medium changes player decisions rather than only colour or naming.

## Epic D — 3D petri-dish integration

Status: colony-texture bridge, approved-scene hookup, transparent runtime viewport, authored
alignment, and Inspector alignment actions are implemented on the reconciled presentation
branch; visual review, portrait framing, materials, and mobile performance verification remain.

- Preserve the deterministic 2D simulation and generated colony texture.
- Display the live colony texture on `PetriDish_ColonySurface` in the 3D dish asset.
- Retain the existing 2D UI until a later dedicated UI pass.
- Preserve tap inspection and circular simulation alignment.
- Create Unity-native glass, agar, and colony-surface materials.
- Verify portrait framing and mobile performance.

The product owner is currently editing dish placement, camera framing, materials, and scene composition locally. Automated agents must not modify those Unity assets or scene choices unless explicitly assigned.

The code integration uses `ColonySurfacePresenter` and `MaterialPropertyBlock`. The
runtime bootstrap supplies the existing `DishRenderer` texture source after scene load.
The product owner's scene, imported model, material settings, camera, transforms, and lid
rotation remain outside the automated change.

The runtime UI uses `DishViewportPresenter` to replace the former opaque full-screen
background with four non-raycastable regions around the central dish opening. The fallback
panel follows flat-dish visibility, while the transparent `RawImage` preserves the current
tap-inspection path. The scene retains the product owner's renderer, transform, material,
camera, and alignment choices. Manual device and Simulator verification remains required.

## Epic E — Nutrient intervention

Status: Option A and experiment schema v4 were approved by the product owner on
3 August 2026. Implementation and verification are pending on the dedicated nutrient
intervention branch.

- Add a bounded nutrient dose action.
- Define finite supply, cooldown, or experiment-specific limits.
- Apply delayed simulation effects rather than instant recovery.
- Record interventions in experiment history.
- Add tutorial feedback and regression coverage.

## Epic F — Organism and medium selection

Status: implemented and tested in draft PR #13. The runtime panel enumerates validated catalog content and applies stable IDs through the existing schema-v3 controller/save path.

- Add organism and medium selection flow.
- Keep the current Comfortable Range experiment functional.

Additional guided experiments and discovery/journal entries are M7 content work.

## Epic G — Multiple dishes and colony transfer

Status: deferred to M7; not required for Phase 2 exit.

- Allow a colony sample to be cloned into a new dish.
- Preserve source lineage and selected organism identity.
- Allow dishes to run under different environments for comparison.
- Define save structure and limits before implementing the user interface.

This epic follows stable organism/media definitions and the completed M6 selection flow. Do not begin it during Phase 2.

## Explicitly deferred systems

Do not implement these during the current framework pass:

- evolution and mutation;
- antibacterial or antifungal treatments;
- resistance development;
- contamination and multi-species competition;
- genetic traits;
- mushroom fruiting and yield systems;
- monetisation expansion.

These systems must be designed after multi-dish experiments and lineage tracking are stable.

## Automation-safe next-task order

1. Merge or otherwise incorporate the approved first-organism PR after review; do not duplicate it.
2. Reconcile and review the existing 3D viewport and texture-alignment branch without overwriting product-owner assets.
3. Add one additional medium.
4. Complete content and safety review for a second organism, then implement it only after approval.
5. Add save-compatible organism and medium selection.
6. Add nutrient intervention.
7. Complete M6 content validation, regression testing, and manual 3D verification.

## Definition of done for automated work

- One coherent change per run.
- No duplication of completed work.
- Relevant tests pass.
- Documentation and manual Unity verification steps are updated.
- No automatic merge of feature branches.
- No modification of locally owned visual assets unless explicitly authorised.
