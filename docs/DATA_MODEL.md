# Data Model

## Goals

Data-driven content, versioned saves, deterministic experiments, validation, and separation of static definitions from live state.

## Definitions

### OrganismDefinition

Stable ID, display name, scientific label, tolerances, growth, metabolism, nutrients, spread, competition, dormancy, visuals, audio, discoveries, and unlocks.

The implemented Phase 2 subset is a Unity ScriptableObject containing a stable lowercase
slug ID, definition version, identity metadata, starting-colony values, temperature and
moisture suitability, health/stress/growth/death rates, resource consumption, and spread.
Visual and audio fields are deliberately excluded from the simulation definition.

### MediumDefinition

Stable ID, category, water retention, nutrient capacity, diffusion, evaporation, structure, visuals, compatibility, and educational note.

The implemented Phase 2 subset is a Unity ScriptableObject containing a stable lowercase
slug ID, definition version, identity metadata, starting moisture/nutrients, and
edge/interior/heat evaporation values. Visual fields are deliberately excluded.

### SimulationDefinitionCatalog

Unity-authored catalog containing the available organism and medium definitions and the
default pair. Catalog validation rejects missing entries, malformed values, invalid IDs,
and duplicate IDs before a simulation can use the content.

### DishDefinition

Stable ID, geometry, grid profile, boundaries, evaporation, theme, and unlocks.

### InterventionDefinition

Stable ID, environmental effect, delay, duration, cost, cooldown, restrictions, and feedback.

### ChallengeDefinition

Starting setup, allowed actions, objectives, failure, scoring, hints, rewards, and educational objective.

### DiscoveryDefinition

Trigger, observation, explanation, simplification label, reward, and related entries.

## Runtime state

ExperimentState stores schema/content versions, seed, tick, dish/grid state, populations, environment, events, challenge state, intervention history, random state, and outcome.

Simulation save schema version 3 adds `organismId`, `organismDefinitionVersion`,
`mediumId`, and `mediumDefinitionVersion`. Schema-version-2 experiment/simulation saves
migrate to `rapid-bacterium` on `nutrient-agar`; schema 3 requires exact catalog matches.

PlayerProfile stores settings, unlocks, discoveries, challenge results, achievements, statistics, cosmetics, and tutorial progress.

## Rules

IDs never depend on display names, are never reused, and must validate. Save schema changes require migration fixtures and safe failure behaviour.

Definition versions begin at 1 and must increase whenever a change would alter
deterministic continuation. Runtime simulation instances copy validated values and never
retain mutable authoritative state inside a ScriptableObject.
