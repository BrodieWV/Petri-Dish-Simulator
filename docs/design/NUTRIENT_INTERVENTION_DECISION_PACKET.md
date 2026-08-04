# Nutrient Intervention Decision Packet

Status: Option A and experiment-wrapper schema v4 were approved by the product owner on
3 August 2026 and implemented on the dedicated nutrient-intervention branch.

## Existing authoritative requirements

M6 and the active Phase 2 backlog require one nutrient intervention that:

- has a finite supply, cooldown, or experiment-specific limit;
- applies a delayed simulation effect rather than instant recovery;
- records intervention history;
- remains deterministic and save-compatible;
- gives player-visible feedback.

The repository does not define the amount, limit, cooldown, delay, delivery shape, spatial scope, or persistence version. Those values materially affect balance and save semantics.

## Current simulation constraints

- Nutrients are normalized per active dish cell and clamped to the selected medium's capacity.
- Nutrient Agar starts and caps at `1.0`.
- Low-Nutrient Agar starts at `0.45` and caps at `0.55`.
- The current organism reaches full nutrient suitability at `0.25` and consumes nutrients only as biomass grows.
- Fixed simulation steps are `0.25` simulated seconds.
- Interventions must use deterministic simulation time, not frame time or wall-clock time.
- Existing schema-v3 saves contain no pending nutrient delivery or intervention-history fields.

These facts mean a dose may have no immediate effect when a medium is already at capacity, and any unfinished delayed dose must be saved to preserve exact continuation.

## Option A: bounded global gradual dose

Approved for M6:

- fixed global dose: `0.12` normalized nutrient units;
- maximum supply: 3 doses per experiment;
- cooldown: 20 fixed steps (5 simulated seconds);
- initial delay: 4 fixed steps (1 simulated second);
- release: equal deterministic increments over the next 12 fixed steps (3 simulated seconds);
- clamp each cell to the selected medium's maximum nutrient capacity;
- show remaining doses, cooldown state, pending delivery, and a plain-language note when capacity limits absorption;
- record request tick, delivery-start tick, completion tick, requested amount, and delivered amount.

Advantages: smallest M6 implementation, consistent with the current global moisture action, deterministic, touch-friendly, and testable without new 3D hit mapping.

Tradeoffs: a global dose is less spatially expressive, the proposed numbers are design values rather than scientific measurements, and applying it too early may waste some or all of the dose at medium capacity.

## Option B: challenge-configured global dose

- add an externally configurable intervention definition containing amount, supply, cooldown, delay, and release duration;
- use the Comfortable Range experiment's definition to select initial values;
- retain global delivery for M6.

Advantages: strongest alignment with the architecture's data-driven `InterventionDefinition` direction and avoids burying tunables in logic.

Tradeoffs: requires a new validated content schema and catalog before one action can ship. This is larger than Option A unless the product owner wants the intervention framework established now.

If Option A is approved, its tunables should still be serialized on the controller or a small configuration asset rather than hard-coded inside the simulation step.

## Option C: targeted local dose

- player selects a dish location and the dose diffuses outward over time.

Advantages: stronger spatial decision-making and clearer visual cause/effect.

Tradeoffs: requires an approved 2D/3D targeting contract, local diffusion/radius rules, additional accessibility feedback, and more complex save state. The current 3D dish deliberately retains a separate 2D tap-inspection surface and has no approved 3D raycast mapping.

This option should be deferred unless the product owner explicitly expands M6.

## Save-version decision

Recommended: keep `SimulationSaveData.schemaVersion` at 3 and bump only the experiment-wrapper schema to 4.

Rationale:

- organism/medium identity and grid restoration remain unchanged in the simulation payload;
- pending delivery, supply, cooldown, and history are experiment/application state;
- schema-v3 experiment saves can migrate with zero pending delivery, full approved supply, no cooldown, and empty history;
- schema-v4 validation can reject non-finite amounts, impossible tick ordering, over-capacity supply counts, and malformed pending delivery without weakening schema-v3 compatibility.

Changing the simulation schema too would be justified only if nutrient delivery becomes authoritative grid state inside `PetriSimulation` rather than an application-owned scheduled intervention.

## Required automated evidence after approval

- dose cannot exceed finite supply or bypass cooldown;
- effect begins only after the approved delay and is distributed across the approved release steps;
- capacity clamping reports actual delivered amount;
- same seed and intervention schedule produce identical snapshots and histories;
- save/load during delay and mid-release continues exactly;
- schema-v3 experiment saves migrate to the approved default intervention state;
- malformed schema-v4 pending/history data fails without replacing a running experiment;
- restart behavior for supply/history is explicit and tested;
- both production media show explainable, meaningfully different responses;
- the full Unity Edit Mode suite passes.

## Recorded approval

The product owner approved nutrient Option A and experiment schema v4 on 3 August 2026.
Options B and C remain unapproved future alternatives.
