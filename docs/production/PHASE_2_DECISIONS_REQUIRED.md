# Phase 2 Decisions Required

Updated: 3 August 2026.

This document records the product decisions that unblocked the remaining M6 work. Both
decisions were approved by the product owner on 3 August 2026.

## Decision 1: second named organism — approved

The product owner approved *Saccharomyces cerevisiae* as the second M6 organism, using a
non-strain-specific educational identity and the stable ID
`saccharomyces-cerevisiae`. Its implementation is isolated on the organism feature branch.

The approved scope defines:

- the species and player-facing name;
- the stable content ID and whether it represents a species, strain, or educational archetype;
- the traits the game may teach and the claims it must avoid;
- the intended gameplay contrast with *Bacillus subtilis*;
- the visual-profile direction.

The content implementation needs primary-source review, explicit confidence and
simplification metadata, externally tunable values, a distinct-outcome regression test,
and qualified subject-matter review before release.

The approved organism is *Saccharomyces cerevisiae*, represented as a non-strain-specific
educational culture with the stable ID `saccharomyces-cerevisiae`. The approved contrast,
visual boundaries, exclusions, and sources are recorded in
`docs/research/SACCHAROMYCES_CEREVISIAE_CANDIDATE_REVIEW.md` and D-017.

## Decision 2: bounded delayed nutrient intervention — approved

M6 uses a bounded global nutrient dose that is delayed rather than an instant recovery
and recorded in experiment history.

The approved contract defines:

- dose amount or adjustable range in normalized simulation units;
- per-experiment supply, maximum use count, or another finite limit;
- cooldown in simulation ticks or seconds;
- delay and delivery shape, such as a fixed delay followed by gradual release;
- whether the dose is global or spatially targeted for M6;
- player-facing feedback and the point at which the action is considered complete;
- history fields that must survive save/load;
- whether adding intervention history requires a schema-v3 additive migration or a new schema version.

Option A in `docs/design/NUTRIENT_INTERVENTION_DECISION_PACKET.md` is approved: a global
fixed dose of 0.12 normalized nutrient units, three doses per experiment, a 20-step
cooldown, four-step initial delay, equal delivery over 12 steps, per-cell medium-capacity
clamping, recorded request/start/completion ticks and requested/delivered amounts, and
experiment wrapper schema v4. Simulation saves remain schema v3.

The implementation remains deterministic, validates malformed saved history, avoids
instant health recovery, exposes balance values on the controller, and covers exact
continuation across pending interventions.

## Release verification still owned by the product team

These checks do not block code integration but remain release gates:

- qualified microbiology review of named-organism claims;
- visual confirmation that the colony texture is not presented as strain-specific or diagnostic;
- product-owner approval of the 3D colony alignment and materials;
- Play Mode verification of growth, restart, save/load, and scene reload on the approved scene;
- portrait safe-area, Large text, touch-target, and representative mobile performance checks.
