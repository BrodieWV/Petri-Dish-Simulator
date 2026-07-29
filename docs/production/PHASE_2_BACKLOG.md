# Phase 2 Backlog — Multi-Organism Experimental Slice

Status: active from 30 July 2026.

## Phase objective

Expand the completed single-dish vertical slice into a reusable experimental framework with simplified real organisms, configurable media, a 3D dish presentation, and the first systems needed for comparative experiments.

## Working order

Complete epics in dependency order. Do not begin later biological systems merely because an earlier task is small.

## Epic A — Data-driven organism and medium framework

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

- Replace the generic Rapid Bacterium content identity with the first approved real organism after scientific review.
- Use named real organisms with deliberately simplified educational behaviour.
- Store scientific name, player-facing name, short description, source notes, confidence, and simplification notes.
- Add organisms individually, with tests and visibly distinct behaviour.
- Select the first four organisms before creating a large content catalogue.

Candidate groups include bacteria, yeast, and filamentous mould. Final species selection requires a dedicated content and safety review.

## Epic C — Media system

- Add four media with distinct nutrient availability, moisture retention, diffusion, drying, and spread resistance.
- Begin with nutrient agar and add media one at a time.
- Keep laboratory media separate from later fungal-growing substrates.
- Ensure each medium changes player decisions rather than only colour or naming.

## Epic D — 3D petri-dish integration

- Preserve the deterministic 2D simulation and generated colony texture.
- Display the live colony texture on `PetriDish_ColonySurface` in the 3D dish asset.
- Retain the existing 2D UI until a later dedicated UI pass.
- Preserve tap inspection and circular simulation alignment.
- Create Unity-native glass, agar, and colony-surface materials.
- Verify portrait framing and mobile performance.

The product owner is currently editing dish placement, camera framing, materials, and scene composition locally. Automated agents must not modify those Unity assets or scene choices unless explicitly assigned.

## Epic E — Nutrient intervention

- Add a bounded nutrient dose action.
- Define finite supply, cooldown, or experiment-specific limits.
- Apply delayed simulation effects rather than instant recovery.
- Record interventions in experiment history.
- Add tutorial feedback and regression coverage.

## Epic F — Experiment selection and discoveries

- Add organism and medium selection flow.
- Add at least three guided experiments.
- Add discovery results and journal entries.
- Keep the current Comfortable Range experiment functional.

## Epic G — Multiple dishes and colony transfer

- Allow a colony sample to be cloned into a new dish.
- Preserve source lineage and selected organism identity.
- Allow dishes to run under different environments for comparison.
- Define save structure and limits before implementing the user interface.

This epic follows stable organism/media definitions and is not the immediate next task.

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

1. Complete and verify the organism/media framework.
2. Update documentation and save compatibility notes.
3. Connect the live colony texture to the approved 3D dish only after the framework branch is merged and local Unity work is committed.
4. Add one reviewed real organism.
5. Add one additional medium.
6. Add nutrient intervention.
7. Expand toward four organisms and four media.

## Definition of done for automated work

- One coherent change per run.
- No duplication of completed work.
- Relevant tests pass.
- Documentation and manual Unity verification steps are updated.
- No automatic merge of feature branches.
- No modification of locally owned visual assets unless explicitly authorised.
