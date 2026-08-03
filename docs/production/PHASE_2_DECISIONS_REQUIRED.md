# Phase 2 Decisions Required

Updated: 3 August 2026.

This document records the M6 work that automated implementation cannot complete without changing unapproved scientific content, player-facing balance, or save semantics. No placeholder choice should be treated as approved.

## Decision 1: second named organism

M6 requires two named real organisms with meaningfully different behaviour. The product owner approved *Bacillus subtilis* as organism one, but the repository does not select organism two.

Approval is required for:

- the species and player-facing name;
- the stable content ID and whether it represents a species, strain, or educational archetype;
- the traits the game may teach and the claims it must avoid;
- the intended gameplay contrast with *Bacillus subtilis*;
- the visual-profile direction.

After approval, the content needs primary-source review, explicit confidence and simplification metadata, externally tunable values, a distinct-outcome regression test, and qualified subject-matter review before release.

The Content Bible's existing Yeast-like Culture archetype makes *Saccharomyces cerevisiae* a plausible candidate for review, not an approved selection. No species asset or simulation values have been created for it.

## Decision 2: bounded delayed nutrient intervention

M6 requires a nutrient dose that is bounded, delayed rather than an instant recovery, and recorded in experiment history. The repository does not approve the concrete interaction contract.

Approval is required for:

- dose amount or adjustable range in normalized simulation units;
- per-experiment supply, maximum use count, or another finite limit;
- cooldown in simulation ticks or seconds;
- delay and delivery shape, such as a fixed delay followed by gradual release;
- whether the dose is global or spatially targeted for M6;
- player-facing feedback and the point at which the action is considered complete;
- history fields that must survive save/load;
- whether adding intervention history requires a schema-v3 additive migration or a new schema version.

The implementation must remain deterministic, validate malformed saved history, avoid instant health recovery, expose all balance values outside core simulation logic, and add exact-continuation tests across pending interventions. Choosing these values in code without approval would change game balance and save behaviour.

## Release verification still owned by the product team

These checks do not block code integration but remain release gates:

- qualified microbiology review of named-organism claims;
- visual confirmation that the colony texture is not presented as strain-specific or diagnostic;
- product-owner approval of the 3D colony alignment and materials;
- Play Mode verification of growth, restart, save/load, and scene reload on the approved scene;
- portrait safe-area, Large text, touch-target, and representative mobile performance checks.
